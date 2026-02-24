Imports MySql.Data.MySqlClient
Imports System.Data
Imports System.Windows.Forms

Public Class InvoiceForm
    Inherits Form

    Private ReadOnly _cs As String

    ' ===== controls =====
    Private ReadOnly txtCustomerCode As New TextBox()
    Private ReadOnly txtCustomerName As New TextBox()
    Private ReadOnly dtpInvoiceDate As New DateTimePicker()
    Private ReadOnly txtTaxRate As New TextBox()
    Private ReadOnly btnLoad As New Button()
    Private ReadOnly btnCreate As New Button()
    Private ReadOnly dgv As New DataGridView()

    ' ===== settings =====
    Private Const AR_CODE As String = "1100"      ' 売掛金
    Private Const TAX_CODE As String = "2200"    ' 仮受消費税（必要なら変更）
    Private Const USE_TAX As Boolean = True      ' 消費税を使うなら True

    Public Sub New(cs As String)
        _cs = cs
        BuildUi()
    End Sub

    Private Sub BuildUi()
        Me.Text = "請求書（Invoice：売掛/売上）"
        Me.Width = 1050
        Me.Height = 720
        Me.StartPosition = FormStartPosition.CenterParent

        Dim y As Integer = 16

        Dim lblCust As New Label() With {.Text = "顧客コード", .Left = 16, .Top = y + 6, .Width = 80}
        txtCustomerCode.Left = 110 : txtCustomerCode.Top = y : txtCustomerCode.Width = 140

        btnLoad.Text = "読込"
        btnLoad.Left = 260 : btnLoad.Top = y : btnLoad.Width = 70

        Dim lblName As New Label() With {.Text = "顧客名", .Left = 350, .Top = y + 6, .Width = 50}
        txtCustomerName.Left = 410 : txtCustomerName.Top = y : txtCustomerName.Width = 260
        txtCustomerName.ReadOnly = True

        Dim lblDate As New Label() With {.Text = "請求日", .Left = 690, .Top = y + 6, .Width = 50}
        dtpInvoiceDate.Left = 740 : dtpInvoiceDate.Top = y : dtpInvoiceDate.Width = 140

        Dim lblTax As New Label() With {.Text = "税率", .Left = 890, .Top = y + 6, .Width = 40}
        txtTaxRate.Left = 930 : txtTaxRate.Top = y : txtTaxRate.Width = 60
        txtTaxRate.Text = "0.10" ' 10%

        y += 44

        btnCreate.Text = "請求書発行（売掛/売上 仕訳）"
        btnCreate.Left = 16 : btnCreate.Top = y : btnCreate.Width = 320

        y += 44

        dgv.Left = 16 : dgv.Top = y
        dgv.Width = Me.ClientSize.Width - 32
        dgv.Height = Me.ClientSize.Height - y - 16
        dgv.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Bottom

        dgv.AllowUserToAddRows = False
        dgv.RowHeadersVisible = False
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill

        dgv.Columns.Clear()
        dgv.Columns.Add(New DataGridViewCheckBoxColumn() With {.Name = "sel", .HeaderText = "選択", .FillWeight = 8})
        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "shipment_batch_id", .HeaderText = "出荷ID", .ReadOnly = True, .FillWeight = 15})
        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "shipment_date", .HeaderText = "出荷日", .ReadOnly = True, .FillWeight = 20})
        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "amount", .HeaderText = "金額", .ReadOnly = True, .FillWeight = 20})
        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "memo", .HeaderText = "備考", .ReadOnly = True, .FillWeight = 37})

        Me.Controls.AddRange(New Control() {
            lblCust, txtCustomerCode, btnLoad, lblName, txtCustomerName,
            lblDate, dtpInvoiceDate, lblTax, txtTaxRate,
            btnCreate, dgv
        })

        AddHandler btnLoad.Click, AddressOf BtnLoad_Click
        AddHandler btnCreate.Click, AddressOf BtnCreate_Click
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

            ' 未請求出荷一覧（ar_invoice_apply_shipment に無いもの）
            Dim sql As String =
"SELECT
   sh.shipment_batch_id,
   sh.shipment_date,
   COALESCE(SUM(sd.quantity2 * sd.unit_price), 0) AS amount,
   CONCAT('SHIPMENT ', sh.shipment_batch_id, ' / customer=', sh.customer_code) AS memo
FROM shipments_header sh
JOIN shipments_detail sd ON sd.shipment_batch_id = sh.shipment_batch_id
LEFT JOIN ar_invoice_apply_shipment ais ON ais.shipment_batch_id = sh.shipment_batch_id
WHERE sh.customer_code = @c
  AND ais.shipment_batch_id IS NULL
GROUP BY sh.shipment_batch_id, sh.shipment_date, sh.customer_code
HAVING amount > 0
ORDER BY sh.shipment_date, sh.shipment_batch_id;"

            Dim dt As New DataTable()
            Using da As New MySqlDataAdapter(sql, conn)
                da.SelectCommand.Parameters.AddWithValue("@c", cust)
                da.Fill(dt)
            End Using

            dgv.Rows.Clear()
            For Each row As DataRow In dt.Rows
                dgv.Rows.Add(False,
                             row("shipment_batch_id").ToString(),
                             CDate(row("shipment_date")).ToString("yyyy-MM-dd"),
                             Convert.ToDecimal(row("amount")).ToString("0.00"),
                             row("memo").ToString())
            Next
        End Using
    End Sub

    Private Sub BtnCreate_Click(sender As Object, e As EventArgs)
        Dim cust As String = txtCustomerCode.Text.Trim()
        If cust = "" Then
            MessageBox.Show("顧客コードを入力してください。")
            Return
        End If

        Dim taxRate As Decimal = 0D
        Decimal.TryParse(txtTaxRate.Text.Trim(), taxRate)
        If taxRate < 0D Then taxRate = 0D

        ' 選択された出荷IDを集める
        Dim shipIds As New List(Of Long)()
        Dim totalAmount As Decimal = 0D

        For Each r As DataGridViewRow In dgv.Rows
            Dim selObj = r.Cells("sel").Value
            Dim sel As Boolean = False
            If selObj IsNot Nothing AndAlso Not IsDBNull(selObj) Then Boolean.TryParse(selObj.ToString(), sel)
            If Not sel Then Continue For

            Dim idStr As String = Convert.ToString(r.Cells("shipment_batch_id").Value)
            Dim sid As Long
            If Not Long.TryParse(idStr, sid) Then Continue For

            Dim amt As Decimal = 0D
            Decimal.TryParse(Convert.ToString(r.Cells("amount").Value), amt)
            If amt <= 0D Then Continue For

            shipIds.Add(sid)
            totalAmount += amt
        Next

        If shipIds.Count = 0 Then
            MessageBox.Show("出荷が選択されていません。")
            Return
        End If

        If totalAmount <= 0D Then
            MessageBox.Show("請求金額が0です。")
            Return
        End If

        Dim taxAmount As Decimal = 0D
        If USE_TAX Then
            taxAmount = Decimal.Round(totalAmount * taxRate, 2, MidpointRounding.AwayFromZero)
        End If

        Dim grandTotal As Decimal = totalAmount + taxAmount

        Using conn As New MySqlConnection(_cs)
            conn.Open()
            Using tx = conn.BeginTransaction()
                Try
                    ' 0) 二重請求防止：対象出荷をロックして、未請求か確認
                    For Each sid In shipIds
                        Using cmdLock As New MySqlCommand(
                            "SELECT shipment_batch_id FROM shipments_header WHERE shipment_batch_id=@id FOR UPDATE;", conn, tx)
                            cmdLock.Parameters.AddWithValue("@id", sid)
                            cmdLock.ExecuteScalar()
                        End Using

                        Using cmdChk As New MySqlCommand(
                            "SELECT 1 FROM ar_invoice_apply_shipment WHERE shipment_batch_id=@id LIMIT 1;", conn, tx)
                            cmdChk.Parameters.AddWithValue("@id", sid)
                            Dim ex = cmdChk.ExecuteScalar()
                            If ex IsNot Nothing Then
                                Throw New Exception($"出荷ID={sid} は既に請求済みです。")
                            End If
                        End Using
                    Next

                    ' 1) invoice header
                    Dim invoiceId As Long
                    Using cmd As New MySqlCommand(
                        "INSERT INTO ar_invoice_header(invoice_date, customer_code, amount, tax_amount, total_amount, memo) " &
                        "VALUES(@d,@c,@a,@t,@g,@m);", conn, tx)

                        cmd.Parameters.AddWithValue("@d", dtpInvoiceDate.Value.Date)
                        cmd.Parameters.AddWithValue("@c", cust)
                        cmd.Parameters.AddWithValue("@a", totalAmount)
                        cmd.Parameters.AddWithValue("@t", taxAmount)
                        cmd.Parameters.AddWithValue("@g", grandTotal)
                        cmd.Parameters.AddWithValue("@m", $"INVOICE / customer={cust}")
                        cmd.ExecuteNonQuery()
                    End Using

                    Using cmd As New MySqlCommand("SELECT LAST_INSERT_ID();", conn, tx)
                        invoiceId = Convert.ToInt64(cmd.ExecuteScalar())
                    End Using

                    ' 2) invoice apply shipment
                    Using cmd As New MySqlCommand(
                        "INSERT INTO ar_invoice_apply_shipment(invoice_id, shipment_batch_id) VALUES(@iid,@sid);", conn, tx)

                        cmd.Parameters.Add("@iid", MySqlDbType.UInt64).Value = invoiceId
                        cmd.Parameters.Add("@sid", MySqlDbType.Int64)

                        For Each sid In shipIds
                            cmd.Parameters("@sid").Value = sid
                            cmd.ExecuteNonQuery()
                        Next
                    End Using

                    ' 3) 売上を科目別に集計（item_master.revenue_account_id → account_master.account_code）
                    Dim revenueMap As New Dictionary(Of String, Decimal)()

                    Dim sqlRev As String =
"SELECT
   am.account_code AS revenue_code,
   SUM(sd.quantity2 * sd.unit_price) AS amt
FROM shipments_detail sd
JOIN item_master im ON im.item_id = sd.item_id
JOIN account_master am ON am.account_id = im.revenue_account_id
WHERE sd.shipment_batch_id IN (" & String.Join(",", shipIds.Select(Function(x) x.ToString())) & ")
GROUP BY am.account_code;"

                    Using cmd As New MySqlCommand(sqlRev, conn, tx)
                        Using rdr = cmd.ExecuteReader()
                            While rdr.Read()
                                Dim code As String = Convert.ToString(rdr("revenue_code"))
                                Dim a As Decimal = 0D
                                Decimal.TryParse(Convert.ToString(rdr("amt")), a)
                                If code <> "" AndAlso a <> 0D Then revenueMap(code) = a
                            End While
                        End Using
                    End Using

                    If revenueMap.Count = 0 Then
                        Throw New Exception("売上科目が取得できません（item_master.revenue_account_id 未設定の可能性）。")
                    End If

                    ' 4) invoice lines（売上科目別）
                    Dim ln As Integer = 0
                    Using cmd As New MySqlCommand(
                        "INSERT INTO ar_invoice_line(invoice_id, line_no, revenue_account_code, amount, item_id, memo) " &
                        "VALUES(@iid,@ln,@ac,@amt,NULL,@m);", conn, tx)

                        cmd.Parameters.Add("@iid", MySqlDbType.UInt64).Value = invoiceId
                        cmd.Parameters.Add("@ln", MySqlDbType.Int32)
                        cmd.Parameters.Add("@ac", MySqlDbType.VarChar)
                        cmd.Parameters.Add("@amt", MySqlDbType.Decimal)
                        cmd.Parameters.Add("@m", MySqlDbType.VarChar)

                        For Each kv In revenueMap
                            ln += 1
                            cmd.Parameters("@ln").Value = ln
                            cmd.Parameters("@ac").Value = kv.Key
                            cmd.Parameters("@amt").Value = kv.Value
                            cmd.Parameters("@m").Value = $"Revenue {kv.Key}"
                            cmd.ExecuteNonQuery()
                        Next
                    End Using

                    ' 5) GL header
                    Dim journalId As Long
                    Using cmd As New MySqlCommand(
                        "INSERT INTO gl_journal_header " &
                        "(tran_type, tran_id, save_seq, journal_date, ref_type, ref_id, memo, posted, is_reversal) " &
                        "VALUES('AR_INVOICE', @tid, 1, @d, 'AR_INVOICE', @rid, @memo, 1, 0);", conn, tx)

                        cmd.Parameters.AddWithValue("@tid", invoiceId)
                        cmd.Parameters.AddWithValue("@d", dtpInvoiceDate.Value.Date)
                        cmd.Parameters.AddWithValue("@rid", invoiceId)
                        cmd.Parameters.AddWithValue("@memo", $"AR INVOICE / customer={cust}")
                        cmd.ExecuteNonQuery()
                    End Using

                    Using cmd As New MySqlCommand("SELECT LAST_INSERT_ID();", conn, tx)
                        journalId = Convert.ToInt64(cmd.ExecuteScalar())
                    End Using

                    ' 6) GL lines：借 売掛 / 貸 売上（＋税）
                    Dim lineNo As Integer = 0
                    Using cmd As New MySqlCommand(
                        "INSERT INTO gl_journal_line " &
                        "(journal_id, line_no, account_code, debit, credit, customer_code, supplier_code, item_id, dept_code) " &
                        "VALUES(@jid,@ln,@ac,@d,@c,@cust,NULL,NULL,NULL);", conn, tx)

                        cmd.Parameters.AddWithValue("@jid", journalId)
                        cmd.Parameters.Add("@ln", MySqlDbType.Int32)
                        cmd.Parameters.Add("@ac", MySqlDbType.VarChar)
                        cmd.Parameters.Add("@d", MySqlDbType.Decimal)
                        cmd.Parameters.Add("@c", MySqlDbType.Decimal)
                        cmd.Parameters.AddWithValue("@cust", cust)

                        ' 借：売掛金（合計）
                        lineNo += 1
                        cmd.Parameters("@ln").Value = lineNo
                        cmd.Parameters("@ac").Value = AR_CODE
                        cmd.Parameters("@d").Value = grandTotal
                        cmd.Parameters("@c").Value = 0D
                        cmd.ExecuteNonQuery()

                        ' 貸：売上（科目別）
                        For Each kv In revenueMap
                            lineNo += 1
                            cmd.Parameters("@ln").Value = lineNo
                            cmd.Parameters("@ac").Value = kv.Key
                            cmd.Parameters("@d").Value = 0D
                            cmd.Parameters("@c").Value = kv.Value
                            cmd.ExecuteNonQuery()
                        Next

                        ' 貸：仮受消費税
                        If USE_TAX AndAlso taxAmount <> 0D Then
                            lineNo += 1
                            cmd.Parameters("@ln").Value = lineNo
                            cmd.Parameters("@ac").Value = TAX_CODE
                            cmd.Parameters("@d").Value = 0D
                            cmd.Parameters("@c").Value = taxAmount
                            cmd.ExecuteNonQuery()
                        End If
                    End Using

                    tx.Commit()
                    MessageBox.Show($"請求書を発行しました。invoice_id={invoiceId}", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information)

                    LoadCustomerAndOpenShipments()

                Catch ex As Exception
                    Try : tx.Rollback() : Catch : End Try
                    MessageBox.Show("発行エラー: " & ex.Message)
                End Try
            End Using
        End Using
    End Sub
End Class
