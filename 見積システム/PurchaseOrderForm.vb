Option Strict On
Option Explicit On

Imports MySql.Data.MySqlClient
Imports System.Data
Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms

Public Class PurchaseOrderForm
    Inherits Form

    Private ReadOnly _cs As String

    ' controls
    Private lblTitle As Label
    Private lblSupplier As Label
    Private cboSupplier As ComboBox
    Private btnSupplierNew As Button
    Private btnReloadSupplier As Button

    Private lblDate As Label
    Private dtpOrderDate As DateTimePicker

    Private dgv As DataGridView
    Private lblTotal As Label
    Private btnReceive As Button
    Private btnSave As Button
    Private btnClose As Button

    Private _poId As Long = 0
    Private _mode As String = "NEW" ' NEW / OPEN_SAVED

    ' supplier cache
    Private supplierDt As DataTable

    Public Sub New(cs As String)
        _cs = cs
        BuildUi()
    End Sub

    Private Sub BuildUi()
        Me.Text = "発注書（仕入）"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ClientSize = New Size(980, 620)

        Dim fTitle As New Font("Yu Gothic UI", 16.0F, FontStyle.Bold)
        Dim fLbl As New Font("Yu Gothic UI", 11.0F, FontStyle.Bold)
        Dim fCtl As New Font("Yu Gothic UI", 11.0F, FontStyle.Regular)

        lblTitle = New Label() With {.Left = 16, .Top = 14, .Width = 600, .Height = 34, .Font = fTitle, .Text = "発注書 作成"}
        Me.Controls.Add(lblTitle)

        lblSupplier = New Label() With {.Left = 16, .Top = 66, .Width = 90, .Height = 26, .Font = fLbl, .Text = "仕入先"}
        Me.Controls.Add(lblSupplier)

        cboSupplier = New ComboBox() With {
            .Left = 110, .Top = 62, .Width = 520, .Height = 30,
            .Font = fCtl,
            .DropDownStyle = ComboBoxStyle.DropDownList
        }
        Me.Controls.Add(cboSupplier)

        btnReloadSupplier = New Button() With {.Left = 640, .Top = 61, .Width = 90, .Height = 32, .Text = "再読込"}
        Me.Controls.Add(btnReloadSupplier)

        btnSupplierNew = New Button() With {.Left = 736, .Top = 61, .Width = 110, .Height = 32, .Text = "仕入先登録"}
        Me.Controls.Add(btnSupplierNew)

        lblDate = New Label() With {.Left = 16, .Top = 110, .Width = 90, .Height = 26, .Font = fLbl, .Text = "発注日"}
        Me.Controls.Add(lblDate)

        dtpOrderDate = New DateTimePicker() With {.Left = 110, .Top = 106, .Width = 200, .Font = fCtl, .Format = DateTimePickerFormat.Short}
        Me.Controls.Add(dtpOrderDate)

        dgv = New DataGridView() With {
            .Left = 16, .Top = 152,
            .Width = Me.ClientSize.Width - 32,
            .Height = 360,
            .AllowUserToAddRows = True,
            .AllowUserToDeleteRows = True,
            .RowHeadersVisible = False,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
            .SelectionMode = DataGridViewSelectionMode.CellSelect,
            .MultiSelect = False
        }
        Me.Controls.Add(dgv)

        lblTotal = New Label() With {.Left = 16, .Top = dgv.Bottom + 10, .Width = 600, .Height = 26, .Font = fLbl, .Text = "合計: 0 円"}
        Me.Controls.Add(lblTotal)

        btnReceive = New Button() With {.Left = Me.ClientSize.Width - 310, .Top = dgv.Bottom + 6, .Width = 90, .Height = 34, .Text = "入荷"}
        btnSave = New Button() With {.Left = Me.ClientSize.Width - 210, .Top = dgv.Bottom + 6, .Width = 90, .Height = 34, .Text = "保存"}
        btnClose = New Button() With {.Left = Me.ClientSize.Width - 110, .Top = dgv.Bottom + 6, .Width = 90, .Height = 34, .Text = "閉じる"}

        Me.Controls.Add(btnReceive)
        Me.Controls.Add(btnSave)
        Me.Controls.Add(btnClose)

        AddHandler Me.Load, AddressOf PurchaseOrderForm_Load
        AddHandler btnClose.Click, Sub() Me.Close()
        AddHandler btnSave.Click, AddressOf btnSave_Click
        AddHandler btnReloadSupplier.Click, Sub() LoadSuppliers()
        AddHandler btnSupplierNew.Click, AddressOf btnSupplierNew_Click
        AddHandler btnReceive.Click, AddressOf btnReceive_Click

        AddHandler dgv.CellEndEdit, AddressOf dgv_CellEndEdit
        AddHandler dgv.CellValueChanged, AddressOf dgv_CellValueChanged
        AddHandler dgv.RowsRemoved, Sub() RecalcTotal()
        AddHandler dgv.UserDeletedRow, Sub() RecalcTotal()

        Me.AcceptButton = btnSave
        Me.CancelButton = btnClose
    End Sub

    Private Sub PurchaseOrderForm_Load(sender As Object, e As EventArgs)
        SetupGrid()
        LoadSuppliers()

        If _mode = "OPEN_SAVED" AndAlso _poId > 0 Then
            LoadPurchaseOrder(_poId)
            ApplyModeUi()
        Else
            _mode = "NEW"
            _poId = 0
            ApplyModeUi()
        End If
    End Sub

    Private Sub ApplyModeUi()
        If _mode = "OPEN_SAVED" AndAlso _poId > 0 Then
            lblTitle.Text = $"発注書 詳細 PO#{_poId}"
            btnReceive.Enabled = True
        Else
            lblTitle.Text = "発注書 作成"
            dtpOrderDate.Value = Date.Today
            btnReceive.Enabled = False ' 新規は保存してから入荷
        End If
    End Sub

    Private Sub SetupGrid()
        dgv.Columns.Clear()

        Dim colItemId As New DataGridViewTextBoxColumn() With {.Name = "item_id", .HeaderText = "item_id", .Width = 60, .Visible = False}
        Dim colItemName As New DataGridViewTextBoxColumn() With {.Name = "item_name", .HeaderText = "商品/原材料名", .Width = 260}
        Dim colLotNo As New DataGridViewTextBoxColumn() With {.Name = "lot_no", .HeaderText = "ロット番号", .Width = 160}
        Dim colQty As New DataGridViewTextBoxColumn() With {.Name = "qty", .HeaderText = "数量", .Width = 90}
        Dim colUnit As New DataGridViewTextBoxColumn() With {.Name = "unit", .HeaderText = "単位(例: 箱/個)", .Width = 110}
        Dim colPrice As New DataGridViewTextBoxColumn() With {.Name = "unit_price", .HeaderText = "単価", .Width = 110}
        Dim colAmount As New DataGridViewTextBoxColumn() With {.Name = "amount", .HeaderText = "金額", .Width = 120, .ReadOnly = True}
        Dim colRemark As New DataGridViewTextBoxColumn() With {.Name = "remark", .HeaderText = "備考", .Width = 180}

        dgv.Columns.Add(colItemId)
        dgv.Columns.Add(colItemName)
        dgv.Columns.Add(colLotNo)
        dgv.Columns.Add(colQty)
        dgv.Columns.Add(colUnit)
        dgv.Columns.Add(colPrice)
        dgv.Columns.Add(colAmount)
        dgv.Columns.Add(colRemark)

        dgv.Columns("amount").DefaultCellStyle.BackColor = Color.FromArgb(248, 249, 252)
    End Sub

    ' =========================
    ' 入荷確定：在庫台帳 + 在庫再計算 + PO明細受領 + POステータス + GL仕訳
    ' =========================
    Private Sub btnReceive_Click(sender As Object, e As EventArgs)
        If _poId <= 0 Then
            MessageBox.Show("先に発注書を保存してください。")
            Return
        End If

        If cboSupplier.SelectedValue Is Nothing OrElse cboSupplier.SelectedValue Is DBNull.Value Then
            MessageBox.Show("仕入先を選択してください。")
            Return
        End If

        Dim supplierId As Long = Convert.ToInt64(cboSupplier.SelectedValue)

        ' 画面行を収集（qty/lot/item/price）
        Dim lines As New List(Of (lineNo As Integer, itemName As String, qty As Decimal, lotNo As String, unitPrice As Decimal))()
        Dim ln As Integer = 0

        For Each r As DataGridViewRow In dgv.Rows
            If r Is Nothing OrElse r.IsNewRow Then Continue For

            Dim name = SafeCellStr(r, "item_name")
            Dim qtyStr = SafeCellStr(r, "qty")
            Dim lotNo = SafeCellStr(r, "lot_no")
            Dim priceStr = SafeCellStr(r, "unit_price")

            If name = "" AndAlso qtyStr = "" AndAlso priceStr = "" Then Continue For

            Dim q As Decimal
            Dim p As Decimal
            If name = "" OrElse Not Decimal.TryParse(qtyStr, q) OrElse q <= 0D Then
                MessageBox.Show($"入荷できない行があります（行 {r.Index + 1}）")
                Return
            End If
            If Not Decimal.TryParse(priceStr, p) OrElse p < 0D Then
                MessageBox.Show($"単価が不正です（行 {r.Index + 1}）")
                Return
            End If

            If lotNo = "" Then
                lotNo = $"PO-{_poId}-{r.Index + 1}-{Date.Now:yyyyMMdd}"
                r.Cells("lot_no").Value = lotNo
            End If

            ln += 1
            lines.Add((ln, name, q, lotNo, p))
        Next

        If lines.Count = 0 Then
            MessageBox.Show("入荷する明細がありません。")
            Return
        End If

        Try
            Using conn As New MySqlConnection(_cs)
                conn.Open()
                Using tx = conn.BeginTransaction()

                    Dim touchedItems As New HashSet(Of Integer)()
                    Dim touchedLots As New List(Of (lotId As Long, itemId As Integer))()

                    ' GL用（後で CreateGlForPoReceive に渡す）
                    Dim glLines As New List(Of (lineNo As Integer, itemId As Integer, qty As Decimal, unitPrice As Decimal))()

                    Dim saveSeq As Integer = CInt(DateTimeOffset.UtcNow.ToUnixTimeSeconds())

                    For Each po In lines
                        ' 1) item_id
                        Dim itemId As Integer = GetItemIdByName(conn, tx, po.itemName)
                        If itemId <= 0 Then Throw New Exception($"item_master に存在しません: {po.itemName}")

                        ' 2) lot 取得/作成
                        Dim lotId As Long = GetOrCreateLot(conn, tx, itemId, po.lotNo, dtpOrderDate.Value.Date)

                        ' 3) ledger insert（入荷なので +）
                        InsertLedger(conn, tx, itemId, lotId, po.qty, _poId, po.lineNo, saveSeq)

                        ' 4) 明細 received_qty を増やす & lot_no 保持
                        AddReceivedQty(conn, tx, _poId, po.lineNo, po.qty, po.lotNo)

                        touchedItems.Add(itemId)
                        touchedLots.Add((lotId, itemId))

                        glLines.Add((po.lineNo, itemId, po.qty, po.unitPrice))
                    Next

                    ' 5) lot / item の再計算更新
                    For Each t In touchedLots
                        UpdateLotOnHandFromLedger(conn, tx, t.lotId, t.itemId)
                    Next
                    For Each itemId In touchedItems
                        UpdateItemOnHandFromLedger(conn, tx, itemId)
                    Next

                    ' 6) header status 更新（PARTIAL/RECEIVED）
                    SetPOStatus(conn, tx, _poId)

                    ' 7) GL仕訳（借方：在庫 / 貸方：買掛）
                    Dim supplierCode As String = GetSupplierCode(conn, tx, supplierId)
                    CreateGlForPoReceive(
                        poId:=_poId,
                        receiveDate:=dtpOrderDate.Value.Date,
                        supplierId:=supplierId,
                        supplierCode:=supplierCode,
                        lines:=glLines,
                        saveSeq:=saveSeq,
                        conn:=conn,
                        tx:=tx
                    )

                    tx.Commit()
                End Using
            End Using

            MessageBox.Show("入荷を確定しました。在庫と仕訳を更新しました。")
            btnReceive.Enabled = False
            lblTitle.Text = $"発注書（RECEIVED） PO#{_poId}"

        Catch ex As Exception
            MessageBox.Show("入荷エラー: " & ex.Message)
        End Try
    End Sub

    ' =========================
    ' 仕訳作成（PO_RECEIVE）
    ' 借方：在庫（itemごとの在庫勘定）
    ' 貸方：買掛金（仕入先AP勘定）
    ' =========================
    Private Sub CreateGlForPoReceive(
    poId As Long,
    receiveDate As Date,
    supplierId As Long,
    supplierCode As String,
    lines As List(Of (lineNo As Integer, itemId As Integer, qty As Decimal, unitPrice As Decimal)),
    saveSeq As Integer,
    conn As MySqlConnection,
    tx As MySqlTransaction
)
        Dim debitMap As New Dictionary(Of String, Decimal)()
        Dim total As Decimal = 0D

        For Each ln In lines
            If ln.qty <= 0D Then Continue For
            Dim amt As Decimal = ln.qty * ln.unitPrice
            If amt <= 0D Then Continue For

            Dim invCode As String = GetInventoryAccountCodeFromItem(ln.itemId, conn, tx)
            If String.IsNullOrWhiteSpace(invCode) Then
                Throw New Exception($"在庫勘定が取得できません item_id={ln.itemId}")
            End If

            If Not debitMap.ContainsKey(invCode) Then debitMap(invCode) = 0D
            debitMap(invCode) += amt
            total += amt
        Next

        If total <= 0D Then Throw New Exception("入荷仕訳の金額が0です（qty/unit_price を確認）")

        Dim apCode As String = GetApAccountCodeFromSupplier(conn, tx, supplierId)

        If ExistsGlForRef(conn, tx, "PO_RECEIVE", poId) Then
            Throw New Exception("このPOの入荷仕訳(PO_RECEIVE)は既に作成済みです（重複防止）")
        End If

        Dim memo As String = $"PO RECEIVE / supplier={supplierCode}"

        ' ★DB仕様に合わせて tran_type/tran_id を入れる（PO入荷なので tran_type=PO_RECEIVE / tran_id=poId）
        Dim journalId As Long = InsertGlHeader(
        tranType:="PO_RECEIVE",
        tranId:=poId,
        refType:="PO_RECEIVE",
        refId:=poId,
        journalDate:=receiveDate,
        memo:=memo,
        saveSeq:=saveSeq,
        conn:=conn,
        tx:=tx
    )

        Dim lineNo As Integer = 0

        Using cmd As New MySqlCommand(
        "INSERT INTO gl_journal_line " &
        "(journal_id, line_no, account_code, debit, credit, item_id, customer_code, dept_code) " &
        "VALUES (@jid, @ln, @ac, @d, @c, NULL, @code, NULL);", conn, tx)

            cmd.Parameters.Add("@jid", MySqlDbType.Int64).Value = journalId
            cmd.Parameters.Add("@ln", MySqlDbType.Int32)
            cmd.Parameters.Add("@ac", MySqlDbType.VarChar)
            cmd.Parameters.Add("@d", MySqlDbType.Decimal)
            cmd.Parameters.Add("@c", MySqlDbType.Decimal)
            cmd.Parameters.Add("@code", MySqlDbType.VarChar).Value = If(supplierCode, "")

            ' 借方：在庫（勘定別に集計して複数行）
            For Each kv In debitMap
                If kv.Value = 0D Then Continue For
                lineNo += 1
                cmd.Parameters("@ln").Value = lineNo
                cmd.Parameters("@ac").Value = kv.Key
                cmd.Parameters("@d").Value = kv.Value
                cmd.Parameters("@c").Value = 0D
                cmd.ExecuteNonQuery()
            Next

            ' 貸方：買掛（合計1行）
            lineNo += 1
            cmd.Parameters("@ln").Value = lineNo
            cmd.Parameters("@ac").Value = apCode
            cmd.Parameters("@d").Value = 0D
            cmd.Parameters("@c").Value = total
            cmd.ExecuteNonQuery()
        End Using
    End Sub


    Private Function ExistsGlForRef(conn As MySqlConnection, tx As MySqlTransaction, refType As String, refId As Long) As Boolean
        Using cmd As New MySqlCommand(
        "SELECT 1 FROM gl_journal_header WHERE ref_type=@t AND ref_id=@id LIMIT 1;", conn, tx)
            cmd.Parameters.AddWithValue("@t", refType)
            cmd.Parameters.AddWithValue("@id", refId)
            Dim o = cmd.ExecuteScalar()
            Return (o IsNot Nothing AndAlso Not IsDBNull(o))
        End Using
    End Function


    ' -------------------------
    ' GL Header Insert
    '  想定：gl_journal_header(journal_date, ref_type, ref_id, memo, save_seq, is_void)
    ' -------------------------
    Private Function InsertGlHeader(
    tranType As String,
    tranId As Long,
    refType As String,
    refId As Long,
    journalDate As Date,
    memo As String,
    saveSeq As Integer,
    conn As MySqlConnection,
    tx As MySqlTransaction
) As Long

        Dim sql As String =
"INSERT INTO gl_journal_header
 (tran_type, tran_id, save_seq, journal_date, ref_type, ref_id, memo, posted, is_reversal, reversed_journal_id)
 VALUES
 (@tran_type, @tran_id, @save_seq, @journal_date, @ref_type, @ref_id, @memo, 1, 0, NULL);"

        Using cmd As New MySqlCommand(sql, conn, tx)
            cmd.Parameters.AddWithValue("@tran_type", tranType)
            cmd.Parameters.AddWithValue("@tran_id", tranId)
            cmd.Parameters.AddWithValue("@save_seq", saveSeq)
            cmd.Parameters.AddWithValue("@journal_date", journalDate)
            cmd.Parameters.AddWithValue("@ref_type", refType)
            cmd.Parameters.AddWithValue("@ref_id", refId)
            cmd.Parameters.AddWithValue("@memo", memo)
            cmd.ExecuteNonQuery()
        End Using

        Using cmd As New MySqlCommand("SELECT LAST_INSERT_ID();", conn, tx)
            Return Convert.ToInt64(cmd.ExecuteScalar())
        End Using
    End Function


    ' -------------------------
    ' item -> inventory account_code
    ' item_master.inventory_account_id -> account_master.account_code
    ' -------------------------
    Private Function GetInventoryAccountCodeFromItem(itemId As Integer, conn As MySqlConnection, tx As MySqlTransaction) As String
        Dim sql As String =
"SELECT am.account_code
 FROM item_master im
 JOIN account_master am ON am.account_id = im.inventory_account_id
 WHERE im.item_id=@id
 LIMIT 1;"

        Using cmd As New MySqlCommand(sql, conn, tx)
            cmd.Parameters.AddWithValue("@id", itemId)
            Dim o = cmd.ExecuteScalar()
            If o Is Nothing OrElse IsDBNull(o) Then Return ""
            Return o.ToString()
        End Using
    End Function

    Private Function GetApAccountCodeFromSupplier(conn As MySqlConnection, tx As MySqlTransaction, supplierId As Long) As String
        Using cmd As New MySqlCommand("SELECT ap_account_code FROM supplier_master WHERE supplier_id=@id LIMIT 1", conn, tx)
            cmd.Parameters.AddWithValue("@id", supplierId)
            Dim r = cmd.ExecuteScalar()
            If r Is Nothing OrElse IsDBNull(r) OrElse r.ToString().Trim() = "" Then
                Throw New Exception($"仕入先の買掛勘定(ap_account_code)が未設定です（supplier_id={supplierId}）")
            End If
            Return r.ToString()
        End Using
    End Function

    Private Function GetSupplierCode(conn As MySqlConnection, tx As MySqlTransaction, supplierId As Long) As String
        Using cmd As New MySqlCommand("SELECT supplier_code FROM supplier_master WHERE supplier_id=@id LIMIT 1;", conn, tx)
            cmd.Parameters.AddWithValue("@id", supplierId)
            Dim o = cmd.ExecuteScalar()
            If o Is Nothing OrElse IsDBNull(o) Then Return ""
            Return o.ToString()
        End Using
    End Function

    ' =========================
    ' Inventory Ledger / Lot / Item update
    ' =========================
    Private Function GetItemIdByName(conn As MySqlConnection, tx As MySqlTransaction, itemName As String) As Integer
        Dim sql =
"SELECT item_id
 FROM item_master
 WHERE is_active=1 AND item_name=@name
 LIMIT 1;"
        Using cmd As New MySqlCommand(sql, conn, tx)
            cmd.Parameters.AddWithValue("@name", itemName)
            Dim o = cmd.ExecuteScalar()
            If o Is Nothing OrElse o Is DBNull.Value Then Return 0
            Return Convert.ToInt32(o)
        End Using
    End Function

    Private Function GetOrCreateLot(conn As MySqlConnection, tx As MySqlTransaction, itemId As Integer, lotNo As String, receivedDate As Date) As Long
        Dim sqlSel =
"SELECT lot_id
 FROM lot
 WHERE item_id=@item_id AND lot_no=@lot_no
 LIMIT 1;"
        Using cmd As New MySqlCommand(sqlSel, conn, tx)
            cmd.Parameters.AddWithValue("@item_id", itemId)
            cmd.Parameters.AddWithValue("@lot_no", lotNo)
            Dim o = cmd.ExecuteScalar()
            If o IsNot Nothing AndAlso o IsNot DBNull.Value Then
                Return Convert.ToInt64(o)
            End If
        End Using

        Dim sqlIns =
"INSERT INTO lot(item_id, lot_no, received_date, qty_on_hand_pieces, is_active)
 VALUES(@item_id, @lot_no, @d, 0, 1);"
        Using cmd As New MySqlCommand(sqlIns, conn, tx)
            cmd.Parameters.AddWithValue("@item_id", itemId)
            cmd.Parameters.AddWithValue("@lot_no", lotNo)
            cmd.Parameters.AddWithValue("@d", receivedDate)
            cmd.ExecuteNonQuery()
        End Using

        Using cmd As New MySqlCommand("SELECT LAST_INSERT_ID();", conn, tx)
            Return Convert.ToInt64(cmd.ExecuteScalar())
        End Using
    End Function

    Private Sub InsertLedger(conn As MySqlConnection, tx As MySqlTransaction,
                            itemId As Integer, lotId As Long, qtyDelta As Decimal,
                            poId As Long, lineNo As Integer, saveSeq As Integer)

        Dim sql =
"INSERT INTO inventory_ledger
(item_id, lot_id, qty_delta, ref_type, ref_id, ref_line_no, entry_type, save_seq, is_void)
VALUES
(@item_id, @lot_id, @qty, 'PO_RECEIVE', @po_id, @line_no, 'RECEIVE', @save_seq, 0);"

        Using cmd As New MySqlCommand(sql, conn, tx)
            cmd.Parameters.AddWithValue("@item_id", itemId)
            cmd.Parameters.AddWithValue("@lot_id", lotId)
            cmd.Parameters.AddWithValue("@qty", qtyDelta)
            cmd.Parameters.AddWithValue("@po_id", poId)
            cmd.Parameters.AddWithValue("@line_no", lineNo)
            cmd.Parameters.AddWithValue("@save_seq", saveSeq)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Private Sub AddReceivedQty(conn As MySqlConnection, tx As MySqlTransaction,
                               poId As Long, lineNo As Integer, recvQty As Decimal, lotNo As String)

        Dim sql =
"UPDATE purchase_order_detail
 SET received_qty = received_qty + @recv,
     lot_no = COALESCE(lot_no, @lot_no)
 WHERE po_id=@po_id AND line_no=@line_no;"

        Using cmd As New MySqlCommand(sql, conn, tx)
            cmd.Parameters.AddWithValue("@recv", recvQty)
            cmd.Parameters.AddWithValue("@lot_no", lotNo)
            cmd.Parameters.AddWithValue("@po_id", poId)
            cmd.Parameters.AddWithValue("@line_no", lineNo)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Private Sub UpdateLotOnHandFromLedger(conn As MySqlConnection, tx As MySqlTransaction, lotId As Long, itemId As Integer)
        Dim sql =
"UPDATE lot l
LEFT JOIN (
  SELECT lot_id, item_id, SUM(qty_delta) AS balance
  FROM inventory_ledger
  WHERE is_void=0 AND lot_id=@lot_id AND item_id=@item_id
  GROUP BY lot_id, item_id
) x ON x.lot_id=l.lot_id AND x.item_id=l.item_id
SET l.qty_on_hand_pieces = COALESCE(x.balance, 0),
    l.updated_at = CURRENT_TIMESTAMP
WHERE l.lot_id=@lot_id;"
        Using cmd As New MySqlCommand(sql, conn, tx)
            cmd.Parameters.AddWithValue("@lot_id", lotId)
            cmd.Parameters.AddWithValue("@item_id", itemId)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Private Sub UpdateItemOnHandFromLedger(conn As MySqlConnection, tx As MySqlTransaction, itemId As Integer)
        Dim sql =
"UPDATE item_master im
LEFT JOIN (
  SELECT item_id, SUM(qty_delta) AS bal
  FROM inventory_ledger
  WHERE is_void=0 AND item_id=@item_id
  GROUP BY item_id
) x ON x.item_id=im.item_id
SET im.quantity1 = COALESCE(x.bal, 0)
WHERE im.item_id=@item_id;"
        Using cmd As New MySqlCommand(sql, conn, tx)
            cmd.Parameters.AddWithValue("@item_id", itemId)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Private Sub SetPOStatus(conn As MySqlConnection, tx As MySqlTransaction, poId As Long)
        Dim sqlChk =
"SELECT
  SUM(CASE WHEN received_qty >= qty THEN 1 ELSE 0 END) AS done_cnt,
  COUNT(*) AS all_cnt
FROM purchase_order_detail
WHERE po_id=@po_id;"

        Dim doneCnt As Integer = 0
        Dim allCnt As Integer = 0

        Using cmd As New MySqlCommand(sqlChk, conn, tx)
            cmd.Parameters.AddWithValue("@po_id", poId)
            Using rd = cmd.ExecuteReader()
                If rd.Read() Then
                    doneCnt = Convert.ToInt32(rd("done_cnt"))
                    allCnt = Convert.ToInt32(rd("all_cnt"))
                End If
            End Using
        End Using

        Dim newStatus As String = If(allCnt > 0 AndAlso doneCnt = allCnt, "RECEIVED", "PARTIAL")

        Dim sqlUpd =
"UPDATE purchase_order_header
SET status=@st
WHERE po_id=@po_id;"

        Using cmd As New MySqlCommand(sqlUpd, conn, tx)
            cmd.Parameters.AddWithValue("@st", newStatus)
            cmd.Parameters.AddWithValue("@po_id", poId)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    ' =========================
    ' Save PO (header+detail)
    ' =========================
    Private Sub btnSave_Click(sender As Object, e As EventArgs)
        If cboSupplier.SelectedValue Is Nothing OrElse cboSupplier.SelectedValue Is DBNull.Value Then
            MessageBox.Show("仕入先を選択してください。")
            Return
        End If

        Dim supplierId As Long = Convert.ToInt64(cboSupplier.SelectedValue)
        Dim poDate As Date = dtpOrderDate.Value.Date

        Dim supplierName As String = ""
        If TypeOf cboSupplier.SelectedItem Is DataRowView Then
            supplierName = DirectCast(cboSupplier.SelectedItem, DataRowView)("supplier_name").ToString()
        End If

        Dim lines As New List(Of PO_Line)()

        For Each r As DataGridViewRow In dgv.Rows
            If r Is Nothing OrElse r.IsNewRow Then Continue For

            Dim name = SafeCellStr(r, "item_name")
            Dim qtyStr = SafeCellStr(r, "qty")
            Dim unit = SafeCellStr(r, "unit")
            Dim priceStr = SafeCellStr(r, "unit_price")
            Dim remark = SafeCellStr(r, "remark")
            Dim lotNo = SafeCellStr(r, "lot_no")

            If name = "" AndAlso qtyStr = "" AndAlso priceStr = "" AndAlso unit = "" AndAlso remark = "" AndAlso lotNo = "" Then
                Continue For
            End If

            If name = "" Then
                MessageBox.Show($"商品/原材料名が未入力の行があります（行 {r.Index + 1}）")
                Return
            End If

            Dim qty As Decimal
            If Not Decimal.TryParse(qtyStr, qty) OrElse qty <= 0D Then
                MessageBox.Show($"数量が不正です（行 {r.Index + 1}）")
                Return
            End If

            Dim price As Decimal
            If Not Decimal.TryParse(priceStr, price) OrElse price < 0D Then
                MessageBox.Show($"単価が不正です（行 {r.Index + 1}）")
                Return
            End If

            Dim amount As Decimal = qty * price

            lines.Add(New PO_Line With {
                .ItemName = name,
                .Qty = qty,
                .Unit = unit,
                .UnitPrice = price,
                .Amount = amount,
                .Remark = remark
            })
        Next

        If lines.Count = 0 Then
            MessageBox.Show("明細がありません。")
            Return
        End If

        Dim total As Decimal = lines.Sum(Function(po) po.Amount)

        Dim sqlInsHeader As String =
"INSERT INTO purchase_order_header
 (po_no, order_date, po_date, supplier_id, supplier_name, status, subtotal, tax, total, remark)
 VALUES
 (@po_no, @order_date, @po_date, @sid, @sname, 'OPEN', @subtotal, @tax, @total, @remark);"

        Dim sqlUpdHeader As String =
"UPDATE purchase_order_header
 SET order_date=@order_date,
     po_date=@po_date,
     supplier_id=@sid,
     supplier_name=@sname,
     subtotal=@subtotal,
     tax=@tax,
     total=@total,
     remark=@remark
 WHERE po_id=@po_id;"

        Dim sqlSelRecvMap As String =
"SELECT line_no, received_qty, lot_no
 FROM purchase_order_detail
 WHERE po_id=@po_id;"

        Dim sqlDelDetail As String =
"DELETE FROM purchase_order_detail WHERE po_id=@po_id;"

        Dim sqlInsDetail As String =
"INSERT INTO purchase_order_detail
 (po_id, line_no, item_id, item_name, lot_no, qty, received_qty, unit, unit_price, amount, remark)
 VALUES
 (@po_id, @line_no, @item_id, @name, @lot_no, @qty, @received_qty, @unit, @price, @amount, @remark);"

        Try
            Using conn As New MySqlConnection(_cs)
                conn.Open()
                Using tx = conn.BeginTransaction()

                    Dim headerId As Long = _poId

                    If _poId = 0 Then
                        Dim poNo As String = "PO-" & Date.Now.ToString("yyyyMMdd-HHmmss")

                        Using cmdH As New MySqlCommand(sqlInsHeader, conn, tx)
                            cmdH.Parameters.AddWithValue("@po_no", poNo)
                            cmdH.Parameters.AddWithValue("@order_date", poDate)
                            cmdH.Parameters.AddWithValue("@po_date", poDate)
                            cmdH.Parameters.AddWithValue("@sid", supplierId)
                            cmdH.Parameters.AddWithValue("@sname", supplierName)
                            cmdH.Parameters.AddWithValue("@subtotal", total)
                            cmdH.Parameters.AddWithValue("@tax", 0D)
                            cmdH.Parameters.AddWithValue("@total", total)
                            cmdH.Parameters.AddWithValue("@remark", "")
                            cmdH.ExecuteNonQuery()
                        End Using

                        Using cmdId As New MySqlCommand("SELECT LAST_INSERT_ID();", conn, tx)
                            headerId = Convert.ToInt64(cmdId.ExecuteScalar())
                        End Using

                        _poId = headerId
                        _mode = "OPEN_SAVED"
                    Else
                        Using cmdH As New MySqlCommand(sqlUpdHeader, conn, tx)
                            cmdH.Parameters.AddWithValue("@po_id", _poId)
                            cmdH.Parameters.AddWithValue("@order_date", poDate)
                            cmdH.Parameters.AddWithValue("@po_date", poDate)
                            cmdH.Parameters.AddWithValue("@sid", supplierId)
                            cmdH.Parameters.AddWithValue("@sname", supplierName)
                            cmdH.Parameters.AddWithValue("@subtotal", total)
                            cmdH.Parameters.AddWithValue("@tax", 0D)
                            cmdH.Parameters.AddWithValue("@total", total)
                            cmdH.Parameters.AddWithValue("@remark", "")
                            cmdH.ExecuteNonQuery()
                        End Using
                    End If

                    ' 既存明細の received_qty / lot_no を退避（保存時に行番号維持したい場合）
                    Dim recvMap As New Dictionary(Of Integer, (recv As Decimal, lotNo As String))()
                    If headerId > 0 Then
                        Using cmd As New MySqlCommand(sqlSelRecvMap, conn, tx)
                            cmd.Parameters.AddWithValue("@po_id", headerId)
                            Using rd = cmd.ExecuteReader()
                                While rd.Read()
                                    Dim ln As Integer = Convert.ToInt32(rd("line_no"))
                                    Dim rq As Decimal = If(IsDBNull(rd("received_qty")), 0D, Convert.ToDecimal(rd("received_qty")))
                                    Dim lot As String = If(IsDBNull(rd("lot_no")), "", rd("lot_no").ToString())
                                    recvMap(ln) = (rq, lot)
                                End While
                            End Using
                        End Using
                    End If

                    Using cmdDel As New MySqlCommand(sqlDelDetail, conn, tx)
                        cmdDel.Parameters.AddWithValue("@po_id", headerId)
                        cmdDel.ExecuteNonQuery()
                    End Using

                    Using cmdD As New MySqlCommand(sqlInsDetail, conn, tx)
                        cmdD.Parameters.Add("@po_id", MySqlDbType.Int64)
                        cmdD.Parameters.Add("@line_no", MySqlDbType.Int32)
                        cmdD.Parameters.Add("@item_id", MySqlDbType.Int32)
                        cmdD.Parameters.Add("@name", MySqlDbType.VarChar)
                        cmdD.Parameters.Add("@lot_no", MySqlDbType.VarChar)
                        cmdD.Parameters.Add("@qty", MySqlDbType.Decimal)
                        cmdD.Parameters.Add("@received_qty", MySqlDbType.Decimal)
                        cmdD.Parameters.Add("@unit", MySqlDbType.VarChar)
                        cmdD.Parameters.Add("@price", MySqlDbType.Decimal)
                        cmdD.Parameters.Add("@amount", MySqlDbType.Decimal)
                        cmdD.Parameters.Add("@remark", MySqlDbType.VarChar)

                        Dim ln As Integer = 0
                        For Each po In lines
                            ln += 1

                            Dim prevRecv As Decimal = 0D
                            Dim prevLot As String = ""
                            If recvMap.ContainsKey(ln) Then
                                prevRecv = recvMap(ln).recv
                                prevLot = recvMap(ln).lotNo
                            End If

                            cmdD.Parameters("@po_id").Value = headerId
                            cmdD.Parameters("@line_no").Value = ln
                            cmdD.Parameters("@item_id").Value = 0 ' ここは「名前からitem_id採番」するなら差し替え
                            cmdD.Parameters("@name").Value = po.ItemName
                            cmdD.Parameters("@lot_no").Value = If(String.IsNullOrWhiteSpace(prevLot), CType(DBNull.Value, Object), prevLot)
                            cmdD.Parameters("@qty").Value = po.Qty
                            cmdD.Parameters("@received_qty").Value = prevRecv
                            cmdD.Parameters("@unit").Value = po.Unit
                            cmdD.Parameters("@price").Value = po.UnitPrice
                            cmdD.Parameters("@amount").Value = po.Amount
                            cmdD.Parameters("@remark").Value = po.Remark
                            cmdD.ExecuteNonQuery()
                        Next
                    End Using

                    tx.Commit()
                End Using
            End Using

            MessageBox.Show("発注書を保存しました。")
            Me.DialogResult = DialogResult.OK
            Me.Close()

        Catch ex As Exception
            MessageBox.Show("保存エラー: " & ex.Message)
        End Try
    End Sub

    ' =========================
    ' Load suppliers / OpenSaved / Load PO
    ' =========================
    Private Sub LoadSuppliers()
        Dim sql As String =
"SELECT supplier_id, supplier_code, supplier_name
 FROM supplier_master
 WHERE is_active=1
 ORDER BY supplier_name, supplier_code;"

        supplierDt = New DataTable()

        Using conn As New MySqlConnection(_cs)
            conn.Open()
            Using da As New MySqlDataAdapter(sql, conn)
                da.Fill(supplierDt)
            End Using
        End Using

        cboSupplier.DataSource = supplierDt
        cboSupplier.ValueMember = "supplier_id"
        cboSupplier.DisplayMember = "supplier_name"

        If supplierDt.Rows.Count = 0 Then
            MessageBox.Show("仕入先マスタが空です。先に仕入先を登録してください。")
        End If
    End Sub

    Private Sub btnSupplierNew_Click(sender As Object, e As EventArgs)
        Using f As New SupplierCreateForm(_cs)
            Dim res = f.ShowDialog(Me)
            If res = DialogResult.OK Then
                LoadSuppliers()
            End If
        End Using
    End Sub

    Public Sub OpenSaved(poId As Long)
        _poId = poId
        _mode = "OPEN_SAVED"
    End Sub

    Private Sub LoadPurchaseOrder(poId As Long)
        Dim sqlH As String =
"SELECT po_id, po_no, order_date, supplier_id, supplier_name, status
 FROM purchase_order_header
 WHERE po_id=@id
 LIMIT 1;"

        Dim sqlD As String =
"SELECT line_no, item_id, item_name, lot_no, qty, unit, unit_price, amount, remark
 FROM purchase_order_detail
 WHERE po_id=@id
 ORDER BY line_no;"

        Using conn As New MySqlConnection(_cs)
            conn.Open()

            Using cmd As New MySqlCommand(sqlH, conn)
                cmd.Parameters.AddWithValue("@id", poId)
                Using rd = cmd.ExecuteReader()
                    If Not rd.Read() Then
                        MessageBox.Show("発注書が見つかりません。")
                        Return
                    End If

                    lblTitle.Text = $"発注書 詳細 PO#{poId} ({rd("status").ToString()})"
                    dtpOrderDate.Value = Convert.ToDateTime(rd("order_date"))
                    cboSupplier.SelectedValue = Convert.ToInt64(rd("supplier_id"))
                End Using
            End Using

            dgv.Rows.Clear()
            Using cmd As New MySqlCommand(sqlD, conn)
                cmd.Parameters.AddWithValue("@id", poId)
                Using rd = cmd.ExecuteReader()
                    While rd.Read()
                        Dim idx = dgv.Rows.Add()
                        Dim r = dgv.Rows(idx)
                        r.Cells("item_id").Value = Convert.ToInt32(rd("item_id"))
                        r.Cells("item_name").Value = rd("item_name").ToString()
                        r.Cells("lot_no").Value = If(IsDBNull(rd("lot_no")), "", rd("lot_no").ToString())
                        r.Cells("qty").Value = rd("qty").ToString()
                        r.Cells("unit").Value = rd("unit").ToString()
                        r.Cells("unit_price").Value = rd("unit_price").ToString()
                        r.Cells("amount").Value = rd("amount").ToString()
                        r.Cells("remark").Value = rd("remark").ToString()
                    End While
                End Using
            End Using
        End Using

        RecalcTotal()
        btnReceive.Enabled = True
    End Sub

    ' =========================
    ' Grid calc
    ' =========================
    Private Sub dgv_CellValueChanged(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Return
        Dim col = dgv.Columns(e.ColumnIndex).Name
        If col = "qty" OrElse col = "unit_price" Then
            RecalcRow(e.RowIndex)
            RecalcTotal()
        End If
    End Sub

    Private Sub dgv_CellEndEdit(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Return
        Dim col = dgv.Columns(e.ColumnIndex).Name
        If col = "qty" OrElse col = "unit_price" Then
            RecalcRow(e.RowIndex)
            RecalcTotal()
        End If
    End Sub

    Private Sub RecalcRow(rowIndex As Integer)
        If rowIndex < 0 OrElse rowIndex >= dgv.Rows.Count Then Return
        Dim r = dgv.Rows(rowIndex)
        If r Is Nothing OrElse r.IsNewRow Then Return

        Dim qtyStr = SafeCellStr(r, "qty")
        Dim priceStr = SafeCellStr(r, "unit_price")

        Dim q As Decimal
        Dim p As Decimal
        If qtyStr = "" OrElse priceStr = "" Then
            r.Cells("amount").Value = ""
            Return
        End If
        If Not Decimal.TryParse(qtyStr, q) Then
            r.Cells("amount").Value = ""
            Return
        End If
        If Not Decimal.TryParse(priceStr, p) Then
            r.Cells("amount").Value = ""
            Return
        End If
        If q <= 0D OrElse p < 0D Then
            r.Cells("amount").Value = ""
            Return
        End If

        Dim a As Decimal = q * p
        r.Cells("amount").Value = a.ToString("0")
    End Sub

    Private Sub RecalcTotal()
        Dim sum As Decimal = 0D
        For Each r As DataGridViewRow In dgv.Rows
            If r Is Nothing OrElse r.IsNewRow Then Continue For
            Dim aStr = SafeCellStr(r, "amount")
            Dim a As Decimal
            If aStr <> "" AndAlso Decimal.TryParse(aStr, a) Then
                sum += a
            End If
        Next
        lblTotal.Text = $"合計: {sum.ToString("0")} 円"
    End Sub

    Private Function SafeCellStr(row As DataGridViewRow, colName As String) As String
        Try
            Dim o = row.Cells(colName).Value
            If o Is Nothing OrElse o Is DBNull.Value Then Return ""
            Return o.ToString().Trim()
        Catch
            Return ""
        End Try
    End Function

    Private Class PO_Line
        Public Property ItemName As String
        Public Property Qty As Decimal
        Public Property Unit As String
        Public Property UnitPrice As Decimal
        Public Property Amount As Decimal
        Public Property Remark As String
    End Class

End Class
