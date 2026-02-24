' ==========================================================
' 【一括コピペ用】ロット別 在庫一覧 + ship導線 + アイテムマスタ作成（最短）
'
' ✅ shipmentは一切触りません（在庫閲覧/マスタ登録のみ）
'
' 1) LotStockListForm：ロット別在庫一覧
' 2) ItemMasterCreateForm：アイテムマスタ作成（INSERT）
' 3) ship.vb へ差し込むコード：DGVに「在庫一覧」ボタンを追加し、クリックで一覧を開く
'
' ==========================================================

Imports MySql.Data.MySqlClient
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

' ==========================================================
' 1) ロット別 在庫一覧フォーム（LotStockListForm）
'   - 上：アイテム名 + item_id
'   - 中：DataGridView（ロット一覧）
'   - 下：合計（個数・箱）/ 差分（必要なら）
' ==========================================================
Public Class LotStockListForm
    Inherits Form

    Private ReadOnly _cs As String
    Private ReadOnly _itemId As Integer
    Private ReadOnly _itemName As String
    Private ReadOnly _conv As Integer

    ' 必要なコントロール（Designer不要：コードで生成）
    Private lblHeader As Label
    Private dgvLots As DataGridView
    Private lblTotal As Label
    Private btnReload As Button
    Private btnClose As Button

    Public Sub New(cs As String, itemId As Integer, itemName As String, conversionQty As Integer)
        _cs = cs
        _itemId = itemId
        _itemName = itemName
        _conv = Math.Max(1, conversionQty)

        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        Me.Text = $"ロット在庫一覧 - {_itemName}"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.Size = New Size(980, 650)
        Me.MinimizeBox = False
        Me.MaximizeBox = False

        lblHeader = New Label() With {
            .Name = "lblHeader",
            .Left = 16, .Top = 12, .Width = 930, .Height = 28,
            .Font = New Font("Yu Gothic UI", 12.0F, FontStyle.Bold)
        }

        dgvLots = New DataGridView() With {
            .Name = "dgvLots",
            .Left = 16, .Top = 48, .Width = 930, .Height = 500,
            .ReadOnly = True,
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .RowHeadersVisible = False,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        }

        lblTotal = New Label() With {
            .Name = "lblTotal",
            .Left = 16, .Top = 555, .Width = 930, .Height = 26,
            .Font = New Font("Yu Gothic UI", 11.5F, FontStyle.Bold)
        }

        btnReload = New Button() With {
            .Name = "btnReload",
            .Text = "再読み込み",
            .Left = 16, .Top = 585, .Width = 120, .Height = 34
        }

        btnClose = New Button() With {
            .Name = "btnClose",
            .Text = "閉じる",
            .Left = 826, .Top = 585, .Width = 120, .Height = 34
        }

        AddHandler Me.Load, AddressOf LotStockListForm_Load
        AddHandler btnReload.Click, AddressOf btnReload_Click
        AddHandler btnClose.Click, Sub() Me.Close()

        Me.Controls.Add(lblHeader)
        Me.Controls.Add(dgvLots)
        Me.Controls.Add(lblTotal)
        Me.Controls.Add(btnReload)
        Me.Controls.Add(btnClose)
    End Sub

    Private Sub LotStockListForm_Load(sender As Object, e As EventArgs)
        lblHeader.Text = $"item_id={_itemId} / {_itemName}   (conversion_qty={_conv})"
        LoadLots()
    End Sub

    Private Sub btnReload_Click(sender As Object, e As EventArgs)
        LoadLots()
    End Sub

    Private Sub LoadLots()
        Dim sql As String =
"SELECT
  l.lot_id,
  l.lot_no,
  l.received_date,
  l.qty_on_hand_pieces AS lot_qty_pieces,
  (SELECT COUNT(*) FROM lot_unit u WHERE u.lot_id=l.lot_id AND u.status='ON_HAND') AS unit_on_hand,
  (l.qty_on_hand_pieces - (SELECT COUNT(*) FROM lot_unit u WHERE u.lot_id=l.lot_id AND u.status='ON_HAND')) AS diff_pieces
FROM lot l
WHERE l.item_id=@item_id AND l.is_active=1
ORDER BY l.received_date, l.lot_no;"

        Dim dt As New DataTable()

        Using conn As New MySqlConnection(_cs)
            conn.Open()
            Using da As New MySqlDataAdapter(sql, conn)
                da.SelectCommand.Parameters.AddWithValue("@item_id", _itemId)
                da.Fill(dt)
            End Using
        End Using

        ' 箱数列を計算で追加
        If Not dt.Columns.Contains("lot_qty_boxes") Then
            dt.Columns.Add("lot_qty_boxes", GetType(Integer))
        End If

        For Each r As DataRow In dt.Rows
            Dim pcs As Integer = 0
            Integer.TryParse(r("lot_qty_pieces").ToString(), pcs)
            r("lot_qty_boxes") = pcs \ _conv
        Next

        dgvLots.AutoGenerateColumns = True
        dgvLots.DataSource = dt

        ' 見栄え
        If dgvLots.Columns.Contains("lot_id") Then dgvLots.Columns("lot_id").HeaderText = "lot_id"
        If dgvLots.Columns.Contains("lot_no") Then dgvLots.Columns("lot_no").HeaderText = "ロット番号"
        If dgvLots.Columns.Contains("received_date") Then dgvLots.Columns("received_date").HeaderText = "入庫日"
        If dgvLots.Columns.Contains("lot_qty_pieces") Then dgvLots.Columns("lot_qty_pieces").HeaderText = "残(個)"
        If dgvLots.Columns.Contains("lot_qty_boxes") Then dgvLots.Columns("lot_qty_boxes").HeaderText = "残(箱)"
        If dgvLots.Columns.Contains("unit_on_hand") Then dgvLots.Columns("unit_on_hand").HeaderText = "unit ON_HAND"
        If dgvLots.Columns.Contains("diff_pieces") Then dgvLots.Columns("diff_pieces").HeaderText = "差分(個)"

        ' diff≠0 を赤く（ズレ検知）
        If dgvLots.Columns.Contains("diff_pieces") Then
            For Each row As DataGridViewRow In dgvLots.Rows
                Dim diff As Integer = 0
                Integer.TryParse(If(row.Cells("diff_pieces").Value, "0").ToString(), diff)
                If diff <> 0 Then
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 238)
                End If
            Next
        End If

        ' 合計
        Dim sumPcs As Long = 0
        Dim sumBoxes As Long = 0
        For Each r As DataRow In dt.Rows
            sumPcs += CLng(r("lot_qty_pieces"))
            sumBoxes += CLng(r("lot_qty_boxes"))
        Next
        lblTotal.Text = $"合計: {sumPcs} 個 / {sumBoxes} 箱   （diff≠0が赤）"
    End Sub
End Class


' ==========================================================
' 2) アイテムマスタ作成フォーム（最短で動く版）
'    - item_code が必須でも任意でも動く（空ならitem_codeをINSERTしない）
' ==========================================================
Public Class ItemMasterCreateForm
    Inherits Form

    Private ReadOnly _cs As String

    Private txtItemCode As TextBox
    Private txtItemName As TextBox
    Private txtUnit1 As TextBox
    Private txtUnit1Price As TextBox
    Private txtUnit2 As TextBox
    Private txtUnit2Price As TextBox
    Private txtConv As TextBox
    Private txtDefaultPrice As TextBox
    Private chkLot As CheckBox
    Private lblMsg As Label
    Private btnSave As Button
    Private btnCancel As Button

    Public Sub New(cs As String)
        _cs = cs
        InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        Me.Text = "アイテムマスタ作成"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.Size = New Size(680, 520)
        Me.MinimizeBox = False
        Me.MaximizeBox = False

        Dim f = New Font("Yu Gothic UI", 11.0F, FontStyle.Regular)

        Dim y As Integer = 18
        Dim gap As Integer = 42

        Dim mkLbl = Function(text As String, top As Integer) As Label
                        Return New Label() With {.Text = text, .Left = 18, .Top = top, .Width = 260, .Font = f}
                    End Function

        txtItemCode = New TextBox() With {.Left = 290, .Top = y - 4, .Width = 340, .Font = f}
        Me.Controls.Add(mkLbl("item_code（必須なら入力）", y))
        Me.Controls.Add(txtItemCode)
        y += gap

        txtItemName = New TextBox() With {.Left = 290, .Top = y - 4, .Width = 340, .Font = f}
        Me.Controls.Add(mkLbl("item_name（必須）", y))
        Me.Controls.Add(txtItemName)
        y += gap

        txtUnit1 = New TextBox() With {.Left = 290, .Top = y - 4, .Width = 140, .Font = f}
        txtUnit1Price = New TextBox() With {.Left = 540, .Top = y - 4, .Width = 90, .Font = f, .Text = "0"}
        Me.Controls.Add(mkLbl("unit1（例：個）", y))
        Me.Controls.Add(txtUnit1)
        Me.Controls.Add(New Label() With {.Text = "unit1_price", .Left = 440, .Top = y, .Width = 90, .Font = f})
        Me.Controls.Add(txtUnit1Price)
        y += gap

        txtUnit2 = New TextBox() With {.Left = 290, .Top = y - 4, .Width = 140, .Font = f}
        txtUnit2Price = New TextBox() With {.Left = 540, .Top = y - 4, .Width = 90, .Font = f, .Text = "0"}
        Me.Controls.Add(mkLbl("unit2（例：箱）", y))
        Me.Controls.Add(txtUnit2)
        Me.Controls.Add(New Label() With {.Text = "unit2_price", .Left = 440, .Top = y, .Width = 90, .Font = f})
        Me.Controls.Add(txtUnit2Price)
        y += gap

        txtConv = New TextBox() With {.Left = 290, .Top = y - 4, .Width = 140, .Font = f, .Text = "1"}
        Me.Controls.Add(mkLbl("conversion_qty（1箱あたり個数）", y))
        Me.Controls.Add(txtConv)
        y += gap

        txtDefaultPrice = New TextBox() With {.Left = 290, .Top = y - 4, .Width = 140, .Font = f, .Text = "0"}
        Me.Controls.Add(mkLbl("default_price（保険）", y))
        Me.Controls.Add(txtDefaultPrice)
        y += gap

        chkLot = New CheckBox() With {.Text = "ロット管理（is_lot_item='T'）", .Left = 18, .Top = y, .Width = 360, .Font = f}
        Me.Controls.Add(chkLot)
        y += gap

        lblMsg = New Label() With {.Left = 18, .Top = y, .Width = 612, .Height = 60, .Font = f, .ForeColor = Color.FromArgb(170, 0, 0)}
        Me.Controls.Add(lblMsg)
        y += 72

        btnSave = New Button() With {.Text = "保存（INSERT）", .Left = 18, .Top = y, .Width = 160, .Height = 36, .Font = f}
        btnCancel = New Button() With {.Text = "閉じる", .Left = 470, .Top = y, .Width = 160, .Height = 36, .Font = f}
        Me.Controls.Add(btnSave)
        Me.Controls.Add(btnCancel)

        AddHandler btnSave.Click, AddressOf btnSave_Click
        AddHandler btnCancel.Click, Sub() Me.Close()
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs)
        lblMsg.Text = ""

        Dim itemCode As String = If(txtItemCode.Text, "").Trim()
        Dim itemName As String = If(txtItemName.Text, "").Trim()

        If itemName = "" Then
            lblMsg.Text = "item_name は必須です。"
            Return
        End If

        Dim unit1 As String = If(txtUnit1.Text, "").Trim()
        Dim unit2 As String = If(txtUnit2.Text, "").Trim()

        Dim p1 As Integer = ParseInt(txtUnit1Price.Text, 0)
        Dim p2 As Integer = ParseInt(txtUnit2Price.Text, 0)
        Dim conv As Integer = ParseInt(txtConv.Text, 1)
        Dim defP As Integer = ParseInt(txtDefaultPrice.Text, 0)
        If conv <= 0 Then conv = 1

        Dim isLot As String = If(chkLot.Checked, "T", "F")

        Dim sqlWithCode As String =
"INSERT INTO item_master
(item_code, item_name, unit1, unit1_price, unit2, unit2_price, conversion_qty,
 quantity1, quantity2, default_price, is_active, is_lot_item)
VALUES
(@code, @name, @u1, @p1, @u2, @p2, @conv,
 0, 0, @def, 1, @isLot);"

        Dim sqlNoCode As String =
"INSERT INTO item_master
(item_name, unit1, unit1_price, unit2, unit2_price, conversion_qty,
 quantity1, quantity2, default_price, is_active, is_lot_item)
VALUES
(@name, @u1, @p1, @u2, @p2, @conv,
 0, 0, @def, 1, @isLot);"

        Dim useCode As Boolean = (itemCode <> "")

        Try
            Using conn As New MySqlConnection(_cs)
                conn.Open()
                Using cmd As New MySqlCommand(If(useCode, sqlWithCode, sqlNoCode), conn)
                    If useCode Then cmd.Parameters.AddWithValue("@code", itemCode)
                    cmd.Parameters.AddWithValue("@name", itemName)
                    cmd.Parameters.AddWithValue("@u1", unit1)
                    cmd.Parameters.AddWithValue("@p1", p1)
                    cmd.Parameters.AddWithValue("@u2", unit2)
                    cmd.Parameters.AddWithValue("@p2", p2)
                    cmd.Parameters.AddWithValue("@conv", conv)
                    cmd.Parameters.AddWithValue("@def", defP)
                    cmd.Parameters.AddWithValue("@isLot", isLot)
                    cmd.ExecuteNonQuery()
                End Using

                Dim newId As Long
                Using cmdId As New MySqlCommand("SELECT LAST_INSERT_ID();", conn)
                    newId = Convert.ToInt64(cmdId.ExecuteScalar())
                End Using

                MessageBox.Show($"登録しました。item_id={newId}", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Me.DialogResult = DialogResult.OK
                Me.Close()
            End Using
        Catch ex As Exception
            lblMsg.Text = ex.Message
        End Try
    End Sub

    Private Function ParseInt(s As String, defVal As Integer) As Integer
        Dim v As Integer
        If Integer.TryParse(If(s, "").Trim(), v) Then Return v
        Return defVal
    End Function
End Class


' ==========================================================
' 3) ship.vb に貼るコード（差し込み用：shipmentなし）
' ==========================================================
Public Module ShipLotListPatch

    ' ----------------------------------------------------------
    ' (A) SetupItemColumns() の末尾あたりに追記（DGVにボタン列追加）
    ' ----------------------------------------------------------
    Public Sub EnsureLotListButtonColumn(dgv As DataGridView)
        If dgv Is Nothing Then Return

        If Not dgv.Columns.Contains("btn_lot_list") Then
            Dim b As New DataGridViewButtonColumn() With {
                .Name = "btn_lot_list",
                .HeaderText = "在庫",
                .Text = "一覧",
                .UseColumnTextForButtonValue = True,
                .Width = 70
            }
            dgv.Columns.Add(b)
        End If
    End Sub

    ' ----------------------------------------------------------
    ' (B) CellContentClick で呼ぶ（btn_lot_list クリック分岐）
    '
    ' ship.vb 側の既存イベント（Dgv_CellContentClick_LotAlloc 等）の先頭で：
    '   If ShipLotListPatch.HandleLotListButtonClick(Me, estimateDataGridView, e, connectionString,
    '         Function(itemId) GetItemNameFromMaster(itemId),
    '         Function(itemId) GetConversionQtyFromMaster(itemId)) Then Return
    ' の1行を入れるだけでOK
    ' ----------------------------------------------------------
    Public Function HandleLotListButtonClick(
        owner As Form,
        dgv As DataGridView,
        e As DataGridViewCellEventArgs,
        connectionString As String,
        getItemName As Func(Of Integer, String),
        getConv As Func(Of Integer, Integer)
    ) As Boolean

        If dgv Is Nothing Then Return False
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Return False
        If dgv.Columns(e.ColumnIndex).Name <> "btn_lot_list" Then Return False

        Dim row = dgv.Rows(e.RowIndex)
        If row Is Nothing OrElse row.IsNewRow Then Return True

        If Not dgv.Columns.Contains("item_id") Then
            MessageBox.Show("item_id 列が見つかりません。")
            Return True
        End If

        Dim itemObj = row.Cells("item_id").Value
        If itemObj Is Nothing OrElse itemObj Is DBNull.Value OrElse itemObj.ToString().Trim() = "" Then
            MessageBox.Show("先にアイテムを選択してください。")
            Return True
        End If

        Dim itemId As Integer
        If Not Integer.TryParse(itemObj.ToString(), itemId) Then
            MessageBox.Show("アイテムIDが不正です。")
            Return True
        End If

        Dim itemName As String = If(getItemName Is Nothing, "", getItemName(itemId))
        Dim conv As Integer = If(getConv Is Nothing, 1, getConv(itemId))

        Using f As New LotStockListForm(connectionString, itemId, itemName, conv)
            f.ShowDialog(owner)
        End Using

        Return True
    End Function

    ' ----------------------------------------------------------
    ' (C) （任意）アイテムマスタ作成フォームを開く
    ' 例：ボタンから呼ぶ
    '   If ShipLotListPatch.OpenItemMasterCreate(Me, connectionString) Then
    '       LoadItemMaster() : BindItemComboColumn()
    '   End If
    ' ----------------------------------------------------------
    Public Function OpenItemMasterCreate(owner As Form, connectionString As String) As Boolean
        Using f As New ItemMasterCreateForm(connectionString)
            Return (f.ShowDialog(owner) = DialogResult.OK)
        End Using
    End Function

End Module