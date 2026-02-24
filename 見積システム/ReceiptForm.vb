Imports MySql.Data.MySqlClient
Imports System.Data
Imports System.Windows.Forms

Public Class ReceiptForm
    Inherits Form

    Private ReadOnly _cs As String

    ' --- controls ---
    Private ReadOnly txtCustomerCode As New TextBox()
    Private ReadOnly txtCustomerName As New TextBox()
    Private ReadOnly dtpReceiptDate As New DateTimePicker()
    Private ReadOnly txtAmount As New TextBox()
    Private ReadOnly cboCashAccount As New ComboBox()
    Private ReadOnly btnLoad As New Button()
    Private ReadOnly btnAutoApply As New Button()
    Private ReadOnly btnSave As New Button()
    Private ReadOnly dgv As New DataGridView()

    Public Sub New(cs As String)
        _cs = cs
        BuildUi()
    End Sub

    Private Sub BuildUi()
        Me.Text = "入金（Receipt）"
        Me.Width = 980
        Me.Height = 720
        Me.StartPosition = FormStartPosition.CenterParent

        Dim y As Integer = 16

        Dim lblCust As New Label() With {.Text = "顧客コード", .Left = 16, .Top = y + 6, .Width = 80}
        txtCustomerCode.Left = 110 : txtCustomerCode.Top = y : txtCustomerCode.Width = 140

        btnLoad.Text = "読込"
        btnLoad.Left = 260 : btnLoad.Top = y : btnLoad.Width = 70

        Dim lblName As New Label() With {.Text = "顧客名", .Left = 350, .Top = y + 6, .Width = 60}
        txtCustomerName.Left = 410 : txtCustomerName.Top = y : txtCustomerName.Width = 240
        txtCustomerName.ReadOnly = True

        y += 44

        Dim lblDate As New Label() With {.Text = "入金日", .Left = 16, .Top = y + 6, .Width = 80}
        dtpReceiptDate.Left = 110 : dtpReceiptDate.Top = y : dtpReceiptDate.Width = 160

        Dim lblAmt As New Label() With {.Text = "入金額", .Left = 290, .Top = y + 6, .Width = 60}
        txtAmount.Left = 350 : txtAmount.Top = y : txtAmount.Width = 120

        Dim lblCash As New Label() With {.Text = "入金科目", .Left = 490, .Top = y + 6, .Width = 70}
        cboCashAccount.Left = 560 : cboCashAccount.Top = y : cboCashAccount.Width = 140
        cboCashAccount.DropDownStyle = ComboBoxStyle.DropDownList
        cboCashAccount.Items.AddRange(New Object() {"1000:現金", "1010:普通預金"})
        cboCashAccount.SelectedIndex = 0

        btnAutoApply.Text = "自動配賦"
        btnAutoApply.Left = 720 : btnAutoApply.Top = y : btnAutoApply.Width = 90

        btnSave.Text = "保存"
        btnSave.Left = 820 : btnSave.Top = y : btnSave.Width = 90

        y += 48

        dgv.Left = 16 : dgv.Top = y
        dgv.Width = Me.ClientSize.Width - 32
        dgv.Height = Me.ClientSize.Height - y - 16
        dgv.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Bottom

        dgv.AllowUserToAddRows = False
        dgv.RowHeadersVisible = False
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill

        dgv.Columns.Clear()
        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "shipment_batch_id", .HeaderText = "出荷ID", .FillWeight = 15, .ReadOnly = True})
        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "shipment_date", .HeaderText = "出荷日", .FillWeight = 20, .ReadOnly = True})
        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "remaining", .HeaderText = "残高(売掛)", .FillWeight = 20, .ReadOnly = True})
        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "apply_amount", .HeaderText = "今回消込", .FillWeight = 20})

        Me.Controls.AddRange(New Control() {
            lblCust, txtCustomerCode, btnLoad, lblName, txtCustomerName,
            lblDate, dtpReceiptDate, lblAmt, txtAmount, lblCash, cboCashAccount,
            btnAutoApply, btnSave, dgv
        })

        AddHandler btnLoad.Click, AddressOf BtnLoad_Click
        AddHandler btnAutoApply.Click, AddressOf BtnAutoApply_Click
        AddHandler btnSave.Click, AddressOf BtnSave_Click
        AddHandler txtCustomerCode.KeyDown, AddressOf TxtCustomerCode_KeyDown
    End Sub

    Private Sub TxtCustomerCode_KeyDown(sender As Object, e As KeyEventArgs)
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            LoadCustomerAndOpenShipments()
        End If
    End Sub

    Private Sub BtnLoad_Click(sender As Object, e As EventArgs)
        LoadCustomerAndOpenShipments()
    End Sub

    Private Sub LoadCustomerAndOpenShipments()
        Dim cust As String = txtCustomerCode.Text.Trim()
        If cust = "" Then
            MessageBox.Show("顧客コードを入力してください。")
            Return
        End If

        Using conn As New MySqlConnection(_cs)
            conn.Open()

            ' 顧客名
            Using cmd As New MySqlCommand("SELECT customer_name FROM customer_master WHERE customer_code=@c LIMIT 1;", conn)
                cmd.Parameters.AddWithValue("@c", cust)
                Dim r = cmd.ExecuteScalar()
                If r Is Nothing OrElse IsDBNull(r) Then
                    txtCustomerName.Text = ""
                    MessageBox.Show("顧客コードが存在しません。")
                    Return
                End If
                txtCustomerName.Text = r.ToString()
            End Using

            ' 未回収出荷一覧（売掛1100 - 入金消込）
            Dim sql As String =
"SELECT sh.shipment_batch_id, sh.shipment_date,
        (IFNULL(ar.ar_debit,0) - IFNULL(ap.applied,0)) AS remaining
   FROM shipments_header sh
   LEFT JOIN (
      SELECT gh.ref_id AS shipment_batch_id, SUM(gl.debit - gl.credit) AS ar_debit
        FROM gl_journal_header gh
        JOIN gl_journal_line gl ON gl.journal_id=gh.journal_id
       WHERE gh.ref_type='SHIPMENT_SALES' AND gl.account_code='1100'
       GROUP BY gh.ref_id
   ) ar ON ar.shipment_batch_id=sh.shipment_batch_id
   LEFT JOIN (
      SELECT shipment_batch_id, SUM(applied_amount) AS applied
        FROM ar_receipt_apply_shipment
       GROUP BY shipment_batch_id
   ) ap ON ap.shipment_batch_id=sh.shipment_batch_id
  WHERE sh.customer_code=@c
 HAVING remaining > 0
  ORDER BY sh.shipment_date, sh.shipment_batch_id;"

            Dim dt As New DataTable()
            Using da As New MySqlDataAdapter(sql, conn)
                da.SelectCommand.Parameters.AddWithValue("@c", cust)
                da.Fill(dt)
            End Using

            dgv.Rows.Clear()
            For Each row As DataRow In dt.Rows
                dgv.Rows.Add(
                    row("shipment_batch_id").ToString(),
                    CDate(row("shipment_date")).ToString("yyyy-MM-dd"),
                    Convert.ToDecimal(row("remaining")).ToString("0.00"),
                    ""
                )
            Next
        End Using
    End Sub

    Private Sub BtnAutoApply_Click(sender As Object, e As EventArgs)
        Dim amt As Decimal
        If Not Decimal.TryParse(txtAmount.Text.Trim(), amt) OrElse amt <= 0D Then
            MessageBox.Show("入金額を正しく入力してください。")
            Return
        End If

        Dim remain As Decimal = amt

        For Each r As DataGridViewRow In dgv.Rows

            Dim remainRow As Decimal = 0D
            Dim remainStr As String = Convert.ToString(r.Cells("remaining").Value)
            Decimal.TryParse(remainStr, remainRow)

            If remainRow <= 0D OrElse remain <= 0D Then
                r.Cells("apply_amount").Value = ""
                Continue For
            End If

            Dim ap As Decimal = Math.Min(remainRow, remain)
            r.Cells("apply_amount").Value = ap.ToString("0.00")
            remain -= ap

        Next

    End Sub


    Private Sub BtnSave_Click(sender As Object, e As EventArgs)
        Dim cust As String = txtCustomerCode.Text.Trim()
        If cust = "" Then
            MessageBox.Show("顧客コードを入力してください。")
            Return
        End If

        Dim receiptAmt As Decimal
        If Not Decimal.TryParse(txtAmount.Text.Trim(), receiptAmt) OrElse receiptAmt <= 0D Then
            MessageBox.Show("入金額を正しく入力してください。")
            Return
        End If

        Dim cashCode As String = cboCashAccount.SelectedItem.ToString().Split(":"c)(0)

        Dim applyList As New List(Of Tuple(Of Long, Decimal))()
        Dim applyTotal As Decimal = 0D

        For Each r As DataGridViewRow In dgv.Rows
            Dim shipIdStr As String = Convert.ToString(r.Cells("shipment_batch_id").Value)
            If String.IsNullOrWhiteSpace(shipIdStr) Then Continue For

            Dim shipId As Long
            If Not Long.TryParse(shipIdStr, shipId) Then Continue For

            Dim apStr As String = Convert.ToString(r.Cells("apply_amount").Value)
            If String.IsNullOrWhiteSpace(apStr) Then Continue For

            Dim ap As Decimal
            If Not Decimal.TryParse(apStr, ap) OrElse ap <= 0D Then Continue For

            ' ★ rem は予約語なので使わない
            Dim remainRow As Decimal = 0D
            Dim remainRowStr As String = Convert.ToString(r.Cells("remaining").Value)
            Decimal.TryParse(remainRowStr, remainRow)

            If ap > remainRow Then
                MessageBox.Show($"出荷ID={shipId} の消込が残高を超えています。")
                Return
            End If

            applyList.Add(Tuple.Create(shipId, ap))
            applyTotal += ap
        Next

        If applyList.Count = 0 Then
            MessageBox.Show("消込がありません。自動配賦または手入力してください。")
            Return
        End If

        If applyTotal <> receiptAmt Then
            MessageBox.Show($"入金額({receiptAmt:0.00}) と 消込合計({applyTotal:0.00}) が一致しません。")
            Return
        End If

        Using conn As New MySqlConnection(_cs)
            conn.Open()
            Using tx = conn.BeginTransaction()
                Try
                    ' 1) receipt header
                    Dim receiptId As Long
                    Using cmd As New MySqlCommand(
                        "INSERT INTO ar_receipt_header(receipt_date, customer_code, amount, cash_account_code, memo) " &
                        "VALUES(@d,@c,@a,@cash,@m);", conn, tx)

                        cmd.Parameters.AddWithValue("@d", dtpReceiptDate.Value.Date)
                        cmd.Parameters.AddWithValue("@c", cust)
                        cmd.Parameters.AddWithValue("@a", receiptAmt)
                        cmd.Parameters.AddWithValue("@cash", cashCode)
                        cmd.Parameters.AddWithValue("@m", DBNull.Value)
                        cmd.ExecuteNonQuery()
                    End Using

                    Using cmd As New MySqlCommand("SELECT LAST_INSERT_ID();", conn, tx)
                        receiptId = Convert.ToInt64(cmd.ExecuteScalar())
                    End Using

                    ' 2) apply rows
                    Using cmd As New MySqlCommand(
                        "INSERT INTO ar_receipt_apply_shipment(receipt_id, shipment_batch_id, applied_amount) " &
                        "VALUES(@rid,@sid,@amt);", conn, tx)

                        cmd.Parameters.Add("@rid", MySqlDbType.UInt64).Value = receiptId
                        cmd.Parameters.Add("@sid", MySqlDbType.Int64)
                        cmd.Parameters.Add("@amt", MySqlDbType.Decimal)

                        For Each t In applyList
                            cmd.Parameters("@sid").Value = t.Item1
                            cmd.Parameters("@amt").Value = t.Item2
                            cmd.ExecuteNonQuery()
                        Next
                    End Using

                    ' 3) GL header
                    Dim journalId As Long
                    Using cmd As New MySqlCommand(
                        "INSERT INTO gl_journal_header " &
                        "(tran_type, tran_id, save_seq, journal_date, ref_type, ref_id, memo, posted, is_reversal) " &
                        "VALUES('AR_RECEIPT', @tid, 1, @d, 'AR_RECEIPT', @rid, @memo, 1, 0);", conn, tx)

                        cmd.Parameters.AddWithValue("@tid", receiptId)
                        cmd.Parameters.AddWithValue("@d", dtpReceiptDate.Value.Date)
                        cmd.Parameters.AddWithValue("@rid", receiptId)
                        cmd.Parameters.AddWithValue("@memo", $"AR RECEIPT / customer={cust}")
                        cmd.ExecuteNonQuery()
                    End Using

                    Using cmd As New MySqlCommand("SELECT LAST_INSERT_ID();", conn, tx)
                        journalId = Convert.ToInt64(cmd.ExecuteScalar())
                    End Using

                    ' 4) GL lines（借：現金/預金、貸：売掛）
                    Using cmd As New MySqlCommand(
                        "INSERT INTO gl_journal_line " &
                        "(journal_id, line_no, account_code, debit, credit, customer_code, item_id, supplier_code, dept_code) " &
                        "VALUES(@jid,@ln,@ac,@d,@c,@cust,NULL,NULL,NULL);", conn, tx)

                        cmd.Parameters.AddWithValue("@jid", journalId)
                        cmd.Parameters.Add("@ln", MySqlDbType.Int32)
                        cmd.Parameters.Add("@ac", MySqlDbType.VarChar)
                        cmd.Parameters.Add("@d", MySqlDbType.Decimal)
                        cmd.Parameters.Add("@c", MySqlDbType.Decimal)
                        cmd.Parameters.AddWithValue("@cust", cust)

                        ' 借方：現金/預金
                        cmd.Parameters("@ln").Value = 1
                        cmd.Parameters("@ac").Value = cashCode
                        cmd.Parameters("@d").Value = receiptAmt
                        cmd.Parameters("@c").Value = 0D
                        cmd.ExecuteNonQuery()

                        ' 貸方：売掛金
                        cmd.Parameters("@ln").Value = 2
                        cmd.Parameters("@ac").Value = "1100"
                        cmd.Parameters("@d").Value = 0D
                        cmd.Parameters("@c").Value = receiptAmt
                        cmd.ExecuteNonQuery()
                    End Using

                    tx.Commit()
                    MessageBox.Show($"入金を保存しました。receipt_id={receiptId}", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information)

                    LoadCustomerAndOpenShipments()

                Catch ex As Exception
                    Try : tx.Rollback() : Catch : End Try
                    MessageBox.Show("保存エラー: " & ex.Message)
                End Try
            End Using
        End Using
    End Sub
End Class
