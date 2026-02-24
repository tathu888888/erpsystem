Imports MySql.Data.MySqlClient
Imports System.Data
Imports System.Windows.Forms

Public Class PaymentForm
    Inherits Form

    Private ReadOnly _cs As String

    ' controls
    Private ReadOnly txtSupplierCode As New TextBox()
    Private ReadOnly txtSupplierName As New TextBox()
    Private ReadOnly dtpPaymentDate As New DateTimePicker()
    Private ReadOnly txtAmount As New TextBox()
    Private ReadOnly cboCashAccount As New ComboBox()
    Private ReadOnly btnLoad As New Button()
    Private ReadOnly btnAutoApply As New Button()
    Private ReadOnly btnSave As New Button()
    Private ReadOnly dgv As New DataGridView()

    ' ======= settings =======
    Private Const AP_ACCOUNT_CODE As String = "2100" ' 買掛金
    ' supplier_master の列名が違う場合はここだけ直す
    Private Const SUPPLIER_NAME_COL As String = "supplier_name"

    Public Sub New(cs As String)
        _cs = cs
        BuildUi()
    End Sub

    Private Sub BuildUi()
        Me.Text = "支払（AP Payment：買掛/現金）"
        Me.Width = 980
        Me.Height = 720
        Me.StartPosition = FormStartPosition.CenterParent

        Dim y As Integer = 16

        Dim lblSup As New Label() With {.Text = "仕入先コード", .Left = 16, .Top = y + 6, .Width = 90}
        txtSupplierCode.Left = 120 : txtSupplierCode.Top = y : txtSupplierCode.Width = 140

        btnLoad.Text = "読込"
        btnLoad.Left = 270 : btnLoad.Top = y : btnLoad.Width = 70

        Dim lblName As New Label() With {.Text = "仕入先名", .Left = 360, .Top = y + 6, .Width = 70}
        txtSupplierName.Left = 430 : txtSupplierName.Top = y : txtSupplierName.Width = 260
        txtSupplierName.ReadOnly = True

        y += 44

        Dim lblDate As New Label() With {.Text = "支払日", .Left = 16, .Top = y + 6, .Width = 90}
        dtpPaymentDate.Left = 120 : dtpPaymentDate.Top = y : dtpPaymentDate.Width = 160

        Dim lblAmt As New Label() With {.Text = "支払額", .Left = 300, .Top = y + 6, .Width = 60}
        txtAmount.Left = 360 : txtAmount.Top = y : txtAmount.Width = 140

        Dim lblCash As New Label() With {.Text = "支払科目", .Left = 520, .Top = y + 6, .Width = 70}
        cboCashAccount.Left = 590 : cboCashAccount.Top = y : cboCashAccount.Width = 150
        cboCashAccount.DropDownStyle = ComboBoxStyle.DropDownList
        cboCashAccount.Items.AddRange(New Object() {"1000:現金", "1010:普通預金"})
        cboCashAccount.SelectedIndex = 0

        btnAutoApply.Text = "自動配賦"
        btnAutoApply.Left = 750 : btnAutoApply.Top = y : btnAutoApply.Width = 90

        btnSave.Text = "保存"
        btnSave.Left = 845 : btnSave.Top = y : btnSave.Width = 90

        y += 48

        dgv.Left = 16 : dgv.Top = y
        dgv.Width = Me.ClientSize.Width - 32
        dgv.Height = Me.ClientSize.Height - y - 16
        dgv.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Bottom

        dgv.AllowUserToAddRows = False
        dgv.RowHeadersVisible = False
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill

        dgv.Columns.Clear()
        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "po_id", .HeaderText = "PO ID(ref_id)", .FillWeight = 18, .ReadOnly = True})
        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "po_date", .HeaderText = "受入日", .FillWeight = 22, .ReadOnly = True})
        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "remaining", .HeaderText = "未払(買掛)", .FillWeight = 22, .ReadOnly = True})
        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "apply_amount", .HeaderText = "今回支払", .FillWeight = 22})

        Me.Controls.AddRange(New Control() {
            lblSup, txtSupplierCode, btnLoad, lblName, txtSupplierName,
            lblDate, dtpPaymentDate, lblAmt, txtAmount, lblCash, cboCashAccount,
            btnAutoApply, btnSave, dgv
        })

        AddHandler btnLoad.Click, AddressOf BtnLoad_Click
        AddHandler btnAutoApply.Click, AddressOf BtnAutoApply_Click
        AddHandler btnSave.Click, AddressOf BtnSave_Click
        AddHandler txtSupplierCode.KeyDown, AddressOf TxtSupplierCode_KeyDown
    End Sub

    Private Sub TxtSupplierCode_KeyDown(sender As Object, e As KeyEventArgs)
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            LoadSupplierAndOpenPos()
        End If
    End Sub

    Private Sub BtnLoad_Click(sender As Object, e As EventArgs)
        LoadSupplierAndOpenPos()
    End Sub

    Private Sub LoadSupplierAndOpenPos()
        Dim sup As String = txtSupplierCode.Text.Trim()
        If sup = "" Then
            MessageBox.Show("仕入先コードを入力してください。")
            Return
        End If

        Using conn As New MySqlConnection(_cs)
            conn.Open()

            ' 仕入先名（列名が違えば SUPPLIER_NAME_COL を修正）
            Using cmd As New MySqlCommand($"SELECT {SUPPLIER_NAME_COL} FROM supplier_master WHERE supplier_code=@s LIMIT 1;", conn)
                cmd.Parameters.AddWithValue("@s", sup)
                Dim r = cmd.ExecuteScalar()
                If r Is Nothing OrElse IsDBNull(r) Then
                    txtSupplierName.Text = ""
                    MessageBox.Show("仕入先コードが存在しません。")
                    Return
                End If
                txtSupplierName.Text = r.ToString()
            End Using

            ' PO未払い（GLのPO_RECEIVEから作る：POヘッダ無しでもOK）
            Dim sql As String =
"SELECT
   gh.ref_id AS po_id,
   gh.journal_date AS po_date,
   (IFNULL(ap.ap_credit,0) - IFNULL(pa.paid,0)) AS remaining
FROM gl_journal_header gh
JOIN (
    SELECT gh2.ref_id AS po_id, SUM(gl2.credit - gl2.debit) AS ap_credit
      FROM gl_journal_header gh2
      JOIN gl_journal_line gl2 ON gl2.journal_id=gh2.journal_id
     WHERE gh2.ref_type='PO_RECEIVE'
       AND gl2.account_code=@ap
     GROUP BY gh2.ref_id
) ap ON ap.po_id = gh.ref_id
LEFT JOIN (
    SELECT po_id, SUM(applied_amount) AS paid
      FROM ap_payment_apply
     GROUP BY po_id
) pa ON pa.po_id = gh.ref_id
WHERE gh.ref_type='PO_RECEIVE'
  AND EXISTS (
      SELECT 1
        FROM gl_journal_line gls
       WHERE gls.journal_id=gh.journal_id
         AND gls.account_code=@ap
         AND gls.supplier_code=@s
  )
GROUP BY gh.ref_id, gh.journal_date, ap.ap_credit, pa.paid
HAVING remaining > 0
ORDER BY gh.journal_date, gh.ref_id;"

            Dim dt As New DataTable()
            Using da As New MySqlDataAdapter(sql, conn)
                da.SelectCommand.Parameters.AddWithValue("@s", sup)
                da.SelectCommand.Parameters.AddWithValue("@ap", AP_ACCOUNT_CODE)
                da.Fill(dt)
            End Using

            dgv.Rows.Clear()
            For Each row As DataRow In dt.Rows
                dgv.Rows.Add(
                    row("po_id").ToString(),
                    CDate(row("po_date")).ToString("yyyy-MM-dd"),
                    Convert.ToDecimal(row("remaining")).ToString("0.00"),
                    ""
                )
            Next
        End Using
    End Sub

    Private Sub BtnAutoApply_Click(sender As Object, e As EventArgs)
        Dim amt As Decimal
        If Not Decimal.TryParse(txtAmount.Text.Trim(), amt) OrElse amt <= 0D Then
            MessageBox.Show("支払額を正しく入力してください。")
            Return
        End If

        Dim remainPay As Decimal = amt

        For Each r As DataGridViewRow In dgv.Rows
            Dim remainRow As Decimal = 0D
            Decimal.TryParse(Convert.ToString(r.Cells("remaining").Value), remainRow)

            If remainRow <= 0D OrElse remainPay <= 0D Then
                r.Cells("apply_amount").Value = ""
                Continue For
            End If

            Dim ap As Decimal = Math.Min(remainRow, remainPay)
            r.Cells("apply_amount").Value = ap.ToString("0.00")
            remainPay -= ap
        Next
    End Sub

    Private Sub BtnSave_Click(sender As Object, e As EventArgs)
        Dim sup As String = txtSupplierCode.Text.Trim()
        If sup = "" Then
            MessageBox.Show("仕入先コードを入力してください。")
            Return
        End If

        Dim payAmt As Decimal
        If Not Decimal.TryParse(txtAmount.Text.Trim(), payAmt) OrElse payAmt <= 0D Then
            MessageBox.Show("支払額を正しく入力してください。")
            Return
        End If

        Dim cashCode As String = cboCashAccount.SelectedItem.ToString().Split(":"c)(0)

        Dim applyList As New List(Of Tuple(Of Long, Decimal))()
        Dim applyTotal As Decimal = 0D

        For Each r As DataGridViewRow In dgv.Rows
            Dim poIdStr As String = Convert.ToString(r.Cells("po_id").Value)
            If String.IsNullOrWhiteSpace(poIdStr) Then Continue For

            Dim poId As Long
            If Not Long.TryParse(poIdStr, poId) Then Continue For

            Dim apStr As String = Convert.ToString(r.Cells("apply_amount").Value)
            If String.IsNullOrWhiteSpace(apStr) Then Continue For

            Dim ap As Decimal
            If Not Decimal.TryParse(apStr, ap) OrElse ap <= 0D Then Continue For

            Dim remainRow As Decimal = 0D
            Decimal.TryParse(Convert.ToString(r.Cells("remaining").Value), remainRow)

            If ap > remainRow Then
                MessageBox.Show($"PO ID={poId} の支払が未払残を超えています。")
                Return
            End If

            applyList.Add(Tuple.Create(poId, ap))
            applyTotal += ap
        Next

        If applyList.Count = 0 Then
            MessageBox.Show("支払配賦がありません。自動配賦または手入力してください。")
            Return
        End If

        If applyTotal <> payAmt Then
            MessageBox.Show($"支払額({payAmt:0.00}) と 配賦合計({applyTotal:0.00}) が一致しません。")
            Return
        End If

        Using conn As New MySqlConnection(_cs)
            conn.Open()
            Using tx = conn.BeginTransaction()
                Try
                    ' 1) payment header
                    Dim paymentId As Long
                    Using cmd As New MySqlCommand(
                        "INSERT INTO ap_payment_header(payment_date, supplier_code, amount, cash_account_code, memo) " &
                        "VALUES(@d,@s,@a,@cash,@m);", conn, tx)

                        cmd.Parameters.AddWithValue("@d", dtpPaymentDate.Value.Date)
                        cmd.Parameters.AddWithValue("@s", sup)
                        cmd.Parameters.AddWithValue("@a", payAmt)
                        cmd.Parameters.AddWithValue("@cash", cashCode)
                        cmd.Parameters.AddWithValue("@m", DBNull.Value)
                        cmd.ExecuteNonQuery()
                    End Using

                    Using cmd As New MySqlCommand("SELECT LAST_INSERT_ID();", conn, tx)
                        paymentId = Convert.ToInt64(cmd.ExecuteScalar())
                    End Using

                    ' 2) apply rows
                    Using cmd As New MySqlCommand(
                        "INSERT INTO ap_payment_apply(payment_id, po_id, applied_amount) " &
                        "VALUES(@pid,@po,@amt);", conn, tx)

                        cmd.Parameters.Add("@pid", MySqlDbType.UInt64).Value = paymentId
                        cmd.Parameters.Add("@po", MySqlDbType.UInt64)
                        cmd.Parameters.Add("@amt", MySqlDbType.Decimal)

                        For Each t In applyList
                            cmd.Parameters("@po").Value = t.Item1
                            cmd.Parameters("@amt").Value = t.Item2
                            cmd.ExecuteNonQuery()
                        Next
                    End Using

                    ' 3) GL header（AP_PAYMENT）
                    Dim journalId As Long
                    Using cmd As New MySqlCommand(
                        "INSERT INTO gl_journal_header " &
                        "(tran_type, tran_id, save_seq, journal_date, ref_type, ref_id, memo, posted, is_reversal) " &
                        "VALUES('AP_PAYMENT', @tid, 1, @d, 'AP_PAYMENT', @rid, @memo, 1, 0);", conn, tx)

                        cmd.Parameters.AddWithValue("@tid", paymentId)
                        cmd.Parameters.AddWithValue("@d", dtpPaymentDate.Value.Date)
                        cmd.Parameters.AddWithValue("@rid", paymentId)
                        cmd.Parameters.AddWithValue("@memo", $"AP PAYMENT / supplier={sup}")
                        cmd.ExecuteNonQuery()
                    End Using

                    Using cmd As New MySqlCommand("SELECT LAST_INSERT_ID();", conn, tx)
                        journalId = Convert.ToInt64(cmd.ExecuteScalar())
                    End Using

                    ' 4) GL lines（借：買掛2100 / 貸：現金・預金）
                    Using cmd As New MySqlCommand(
                        "INSERT INTO gl_journal_line " &
                        "(journal_id, line_no, account_code, debit, credit, customer_code, supplier_code, item_id, dept_code) " &
                        "VALUES(@jid,@ln,@ac,@d,@c,NULL,@sup,NULL,NULL);", conn, tx)

                        cmd.Parameters.AddWithValue("@jid", journalId)
                        cmd.Parameters.Add("@ln", MySqlDbType.Int32)
                        cmd.Parameters.Add("@ac", MySqlDbType.VarChar)
                        cmd.Parameters.Add("@d", MySqlDbType.Decimal)
                        cmd.Parameters.Add("@c", MySqlDbType.Decimal)
                        cmd.Parameters.AddWithValue("@sup", sup)

                        ' 借方：買掛金
                        cmd.Parameters("@ln").Value = 1
                        cmd.Parameters("@ac").Value = AP_ACCOUNT_CODE
                        cmd.Parameters("@d").Value = payAmt
                        cmd.Parameters("@c").Value = 0D
                        cmd.ExecuteNonQuery()

                        ' 貸方：現金/預金
                        cmd.Parameters("@ln").Value = 2
                        cmd.Parameters("@ac").Value = cashCode
                        cmd.Parameters("@d").Value = 0D
                        cmd.Parameters("@c").Value = payAmt
                        cmd.ExecuteNonQuery()
                    End Using

                    tx.Commit()
                    MessageBox.Show($"支払を保存しました。payment_id={paymentId}", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information)

                    LoadSupplierAndOpenPos()

                Catch ex As Exception
                    Try : tx.Rollback() : Catch : End Try
                    MessageBox.Show("保存エラー: " & ex.Message)
                End Try
            End Using
        End Using
    End Sub
End Class
