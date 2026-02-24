Imports System.Windows.Forms
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports MySql.Data.MySqlClient
Imports System.Collections.Generic
Imports System.Linq

Imports System.ComponentModel

' ▼ shiplistdetail クラス内（フィールド）に追加

Public Class shiplistdetail
    Inherits Form

    ' =========================
    ' DB接続
    ' =========================
    Private ReadOnly connectionString As String =
        "Server=127.0.0.1;Port=3306;Database=sunstar;Uid=root;Pwd=1234;SslMode=Disabled;"

    ' =========================
    ' 受け取り：一覧から渡す shipment_batch_id（B案）
    ' =========================
    Public Property ShipmentBatchId As Long = 0

    ' =========================
    ' Controls
    ' =========================
    Private titleLabel As Label
    Private shutdownBtn As Button

    Private cardHeader As RoundedPanel
    Private cardGrid As RoundedPanel
    Private footer As RoundedPanel

    Private lblBatchId As Label
    Private txtBatchId As TextBox

    Private lblShipmentDate As Label
    Private dtpShipmentDate As DateTimePicker

    Private lblCustomerCode As Label
    Private txtCustomerCode As TextBox

    Private lblCustomerName As Label
    Private txtCustomerName As TextBox

    Private lblGridTitle As Label
    Private estimateDataGridView As DataGridView

    Private btnSave As Button
    Private btnClose As Button
    Private btnAddRow As Button
    Private btnDeleteRow As Button

    Private itemNameCache As New Dictionary(Of Long, String)()

    Private ReadOnly ep As New ErrorProvider()
    Private isProgrammaticEdit As Boolean = False


    Private itemTable As DataTable

    ' ===== 帳票（印刷プレビュー）=====
    Private btnReport As Button
    Private WithEvents printDoc As Printing.PrintDocument
    Private previewDlg As PrintPreviewDialog

    ' 帳票用：明細スナップショット
    Private reportRows As New List(Of ReportRow)()
    Private reportRowIndex As Integer = 0

    Private Class ReportRow
        Public Property ItemName As String
        Public Property Unit As String
        Public Property Qty As String
        Public Property Qty2 As String
        Public Property UnitPrice As String
        Public Property Amount As String
        Public Property Remark As String
    End Class




    ' =========================
    ' Theme colors
    ' =========================
    Private ReadOnly cBg As Color = Color.FromArgb(245, 246, 250)
    Private ReadOnly cCard As Color = Color.White
    Private ReadOnly cPrimary As Color = Color.FromArgb(46, 98, 238)
    Private ReadOnly cPrimaryDark As Color = Color.FromArgb(36, 78, 200)
    Private ReadOnly cText As Color = Color.FromArgb(25, 28, 36)
    Private ReadOnly cSubText As Color = Color.FromArgb(120, 125, 140)
    Private ReadOnly cBorder As Color = Color.FromArgb(225, 228, 236)

    ' =========================
    ' Fonts
    ' =========================
    Private ReadOnly fontTitle As New Font("Yu Gothic UI", 20.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(128))
    Private ReadOnly fontLabel As New Font("Yu Gothic UI", 11.5F, FontStyle.Bold, GraphicsUnit.Point, CByte(128))
    Private ReadOnly fontInput As New Font("Yu Gothic UI", 12.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(128))

    ' =========================
    ' NEW
    ' =========================
    Public Sub New()
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.Text = "発送詳細"
        Me.ClientSize = New Size(1100, 760)
        Me.MinimumSize = New Size(980, 700)
        Me.FormBorderStyle = FormBorderStyle.None
        Me.BackColor = cBg
        Me.DoubleBuffered = True

        InitControls()

        AddHandler Me.Shown, AddressOf shiplistdetail_Shown
        AddHandler Me.Resize, AddressOf shiplistdetail_Resize
        AddHandler Me.Load, AddressOf shiplistdetail_Load
    End Sub

    ' =========================
    ' Shown / Resize
    ' =========================
    Private Sub shiplistdetail_Shown(sender As Object, e As EventArgs)
        LayoutAll()
    End Sub

    Private Sub shiplistdetail_Resize(sender As Object, e As EventArgs)
        LayoutAll()
    End Sub

    ' =========================
    ' Load
    ' =========================
    Private Sub shiplistdetail_Load(sender As Object, e As EventArgs)
        If ShipmentBatchId > 0 Then
            LoadShipmentBatch(ShipmentBatchId)
        Else
            txtBatchId.Text = ""
            dtpShipmentDate.Checked = False
        End If
    End Sub

    Private Sub ForceCommitLotDisplay()
        If estimateDataGridView Is Nothing Then Return

        ' ★追加：lot_id列が無い画面（lot_alloc_json方式）では何もしない
        If Not estimateDataGridView.Columns.Contains("lot_id") Then Return

        estimateDataGridView.EndEdit()
        estimateDataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit)

        For Each r As DataGridViewRow In estimateDataGridView.Rows
            If r.IsNewRow Then Continue For

            Dim cell = TryCast(r.Cells("lot_id"), DataGridViewComboBoxCell)
            If cell Is Nothing Then Continue For

            cell.ValueMember = "lot_id"
            cell.DisplayMember = "lot_no"
            cell.ValueType = GetType(Long)

            Dim v As Object = r.Cells("lot_id").Value
            If v Is Nothing OrElse v Is DBNull.Value Then Continue For

            Dim id64 As Long
            Try
                id64 = Convert.ToInt64(v)
            Catch
                r.Cells("lot_id").Value = Nothing
                Continue For
            End Try

            r.Cells("lot_id").Value = id64
        Next

        estimateDataGridView.Invalidate()
        estimateDataGridView.Refresh()
    End Sub


    ' =========================
    ' UI生成
    ' =========================
    Private Sub InitControls()
        ' --- title ---
        titleLabel = New Label() With {
            .Text = "日次処理詳細ページ）",
            .Font = fontTitle,
            .ForeColor = cText,
            .AutoSize = True
        }
        Me.Controls.Add(titleLabel)

        ' --- shutdown ---
        shutdownBtn = New Button() With {
            .Text = "✕",
            .Font = New Font("Yu Gothic UI", 16.0F, FontStyle.Regular),
            .Size = New Size(44, 44),
            .BackColor = Color.White,
            .ForeColor = cSubText,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }
        shutdownBtn.FlatAppearance.BorderSize = 0
        AddHandler shutdownBtn.Click, Sub() Me.Close()

        AddHandler shutdownBtn.MouseEnter, Sub()
                                               shutdownBtn.BackColor = Color.FromArgb(255, 235, 238)
                                               shutdownBtn.ForeColor = Color.FromArgb(198, 40, 40)
                                           End Sub
        AddHandler shutdownBtn.MouseLeave, Sub()
                                               shutdownBtn.BackColor = Color.White
                                               shutdownBtn.ForeColor = cSubText
                                           End Sub

        Me.Controls.Add(shutdownBtn)

        ' --- cards ---
        cardHeader = New RoundedPanel() With {.Radius = 22, .BorderColor = cBorder, .BorderWidth = 1, .FillColor = cCard, .Shadow = True}
        cardGrid = New RoundedPanel() With {.Radius = 22, .BorderColor = cBorder, .BorderWidth = 1, .FillColor = cCard, .Shadow = True}
        footer = New RoundedPanel() With {.Radius = 22, .BorderColor = cBorder, .BorderWidth = 1, .FillColor = cCard, .Shadow = True}

        Me.Controls.Add(cardHeader)
        Me.Controls.Add(cardGrid)
        Me.Controls.Add(footer)

        ' --- header controls ---
        lblBatchId = MakeLabel("トランザクションID")
        txtBatchId = MakeTextBox(readOnlyStyle:=True)

        lblShipmentDate = MakeLabel("発送年月日")
        dtpShipmentDate = New DateTimePicker() With {
            .Font = fontInput,
            .Format = DateTimePickerFormat.Custom,
            .CustomFormat = "yyyy年MM月dd日",
            .ShowCheckBox = True,
            .Checked = True
        }

        lblCustomerCode = MakeLabel("顧客コード")
        txtCustomerCode = MakeTextBox(readOnlyStyle:=False)

        lblCustomerName = MakeLabel("顧客名")
        txtCustomerName = MakeTextBox(readOnlyStyle:=True)

        AddHandler txtCustomerCode.Leave, AddressOf txtCustomerCode_Leave

        cardHeader.Controls.Add(lblBatchId)
        cardHeader.Controls.Add(txtBatchId)
        cardHeader.Controls.Add(lblShipmentDate)
        cardHeader.Controls.Add(dtpShipmentDate)
        cardHeader.Controls.Add(lblCustomerCode)
        cardHeader.Controls.Add(txtCustomerCode)
        cardHeader.Controls.Add(lblCustomerName)
        cardHeader.Controls.Add(txtCustomerName)

        ' --- grid title ---
        lblGridTitle = New Label() With {
            .Text = "明細（単位・数量・数量2・単価・金額・備考）",
            .Font = New Font("Yu Gothic UI", 12.5F, FontStyle.Bold, GraphicsUnit.Point, CByte(128)),
            .ForeColor = cText,
            .AutoSize = True
        }
        cardGrid.Controls.Add(lblGridTitle)

        ' --- grid ---
        InitGrid()
        cardGrid.Controls.Add(estimateDataGridView)

        ' --- footer buttons ---
        ' --- footer buttons ---
        btnAddRow = New Button() With {.Text = "行追加"}
        btnDeleteRow = New Button() With {.Text = "行削除"}
        btnReport = New Button() With {.Text = "帳票作成"}   ' ★追加
        btnSave = New Button() With {.Text = "保存"}
        btnClose = New Button() With {.Text = "閉じる"}



        StyleGhost(btnAddRow)
        StyleGhost(btnDeleteRow)
        StyleGhost(btnReport) ' ★好みでStylePrimaryでもOK
        StylePrimary(btnSave)
        StyleGhost(btnClose)

        AddHandler btnReport.Click, AddressOf btnReport_Click  ' ★追加



        ' ★列数に合わせる（unit, quantity, quantity2, unit_price, amount, remark）
        ' item_name, item_id, unit, quantity, quantity2, unit_price, amount, remark
        ' item_name, item_id, lot_id, quantity, quantity2, unit, unit_price, amount, remark

        AddHandler btnAddRow.Click, Sub()
                                        ' item_name, item_id, lot_info, btn_stock, quantity, quantity2, unit, unit_price, amount, remark, lot_alloc_json
                                        estimateDataGridView.Rows.Add("", "", "未割当", "在庫詳細", "", "", "", "0", "", "", "")
                                    End Sub
        AddHandler btnDeleteRow.Click, AddressOf DeleteSelectedRow
        AddHandler btnSave.Click, AddressOf btnSave_Click
        AddHandler btnClose.Click, Sub() Me.Close()

        footer.Controls.Add(btnAddRow)
        footer.Controls.Add(btnDeleteRow)
        footer.Controls.Add(btnReport) ' ★これを追加

        footer.Controls.Add(btnSave)
        footer.Controls.Add(btnClose)
    End Sub

    Private Sub InitGrid()
        estimateDataGridView = New DataGridView()
        estimateDataGridView.AllowUserToAddRows = True
        estimateDataGridView.AllowUserToDeleteRows = True
        estimateDataGridView.RowHeadersVisible = False
        estimateDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        estimateDataGridView.MultiSelect = False
        estimateDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill

        estimateDataGridView.BackgroundColor = Color.White
        estimateDataGridView.BorderStyle = BorderStyle.None
        estimateDataGridView.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
        estimateDataGridView.GridColor = cBorder
        estimateDataGridView.EnableHeadersVisualStyles = False

        estimateDataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 246, 250)
        estimateDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = cText
        estimateDataGridView.ColumnHeadersDefaultCellStyle.Font = New Font("Yu Gothic UI", 11.5F, FontStyle.Bold, GraphicsUnit.Point, CByte(128))
        estimateDataGridView.ColumnHeadersHeight = 44

        estimateDataGridView.DefaultCellStyle.Font = New Font("Yu Gothic UI", 11.5F, FontStyle.Regular, GraphicsUnit.Point, CByte(128))
        estimateDataGridView.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 235, 255)
        estimateDataGridView.DefaultCellStyle.SelectionForeColor = cText
        estimateDataGridView.RowTemplate.Height = 40
        estimateDataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 253)

        estimateDataGridView.Columns.Clear()

        ' 表示用：商品名（手入力）
        ' --- 既存の colLot (ComboBox) は削除する ---

        ' 表示用：商品名（手入力）
        Dim colItemName As New DataGridViewTextBoxColumn() With {
            .Name = "item_name", .HeaderText = "アイテム", .ReadOnly = False
        }

        ' 裏：item_id（非表示）
        Dim colItemId As New DataGridViewTextBoxColumn() With {
            .Name = "item_id", .HeaderText = "item_id", .Visible = False
        }

        ' ★ロット割当の要約表示（例： "3ロット割当" / "未割当"）
        Dim colLotInfo As New DataGridViewTextBoxColumn() With {
            .Name = "lot_info", .HeaderText = "ロット割当", .ReadOnly = True, .Width = 160
        }

        ' ★在庫詳細（ロット割当）ボタン列
        Dim colStockBtn As New DataGridViewButtonColumn() With {
            .Name = "btn_stock", .HeaderText = "在庫詳細", .Text = "在庫詳細", .UseColumnTextForButtonValue = True,
            .Width = 90
        }

        ' ★割当の中身（JSON）を隠し列で保持
        Dim colAllocJson As New DataGridViewTextBoxColumn() With {
            .Name = "lot_alloc_json", .HeaderText = "lot_alloc_json", .Visible = False
        }

        ' 順番：アイテム → ロット割当 → 在庫詳細ボタン → 数量(個) → 数量2(箱) → 単位 → 単価 → 金額 → 備考 → alloc_json(隠し)
        Dim colQty As New DataGridViewTextBoxColumn() With {.Name = "quantity", .HeaderText = "数量(個)"}
        Dim colQty2 As New DataGridViewTextBoxColumn() With {.Name = "quantity2", .HeaderText = "数量2(箱)"}
        Dim colUnit As New DataGridViewTextBoxColumn() With {.Name = "unit", .HeaderText = "単位"}
        Dim colUnitPrice As New DataGridViewTextBoxColumn() With {.Name = "unit_price", .HeaderText = "単価(箱)"}
        Dim colAmount As New DataGridViewTextBoxColumn() With {.Name = "amount", .HeaderText = "金額", .ReadOnly = True}
        Dim colRemark As New DataGridViewTextBoxColumn() With {.Name = "remark", .HeaderText = "備考"}

        estimateDataGridView.Columns.Clear()
        estimateDataGridView.Columns.AddRange(New DataGridViewColumn() {
            colItemName, colItemId, colLotInfo, colStockBtn, colQty, colQty2, colUnit, colUnitPrice, colAmount, colRemark, colAllocJson
        })

        ' ★ボタン押下用
        RemoveHandler estimateDataGridView.CellContentClick, AddressOf Dgv_CellContentClick_Stock
        AddHandler estimateDataGridView.CellContentClick, AddressOf Dgv_CellContentClick_Stock

        ' 既存の編集イベントなど
        RemoveHandler estimateDataGridView.CellEndEdit, AddressOf Grid_CellEndEdit
        AddHandler estimateDataGridView.CellEndEdit, AddressOf Grid_CellEndEdit

        RemoveHandler estimateDataGridView.DataBindingComplete, AddressOf Dgv_DataBindingComplete
        AddHandler estimateDataGridView.DataBindingComplete, AddressOf Dgv_DataBindingComplete

        SetupGridValidation()

    End Sub

    'ApplyInventoryLedger_ForShipmentUpdate(ShipmentBatchId, details, conn, tx)


    Private Sub VoidShipmentLedger(shipmentBatchId As Long, conn As MySqlConnection, tx As MySqlTransaction)
        Dim sql As String =
        "UPDATE inventory_ledger " &
        "SET is_void=1, voided_at=NOW(), void_reason='shipment re-save (void all before rebuild)' " &
        "WHERE ref_type='SHIPMENT' AND ref_id=@id AND is_void=0;"

        Using cmd As New MySqlCommand(sql, conn, tx)
            cmd.Parameters.AddWithValue("@id", shipmentBatchId)
            cmd.ExecuteNonQuery()
        End Using
    End Sub


    Private Sub Dgv_DataError_All(sender As Object, e As DataGridViewDataErrorEventArgs)
        e.ThrowException = False
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Return

        ' ★ロード/内部更新中は黙って通す（消さない）
        If isProgrammaticEdit Then
            e.Cancel = False
            Return
        End If

        Dim dgv = estimateDataGridView
        Dim colName = dgv.Columns(e.ColumnIndex).Name

        If colName = "lot_id" Then
            e.Cancel = False
            ' ※ここで Value を消すと「開いた瞬間に空欄」が起きる
            ' dgv.Rows(e.RowIndex).Cells(e.ColumnIndex).Value = Nothing  ←消す
            dgv.Rows(e.RowIndex).Cells(e.ColumnIndex).ErrorText = ""
            dgv.Rows(e.RowIndex).ErrorText = ""
            Return
        End If

        dgv.Rows(e.RowIndex).Cells(e.ColumnIndex).ErrorText = "入力値が不正です。"
        dgv.Rows(e.RowIndex).ErrorText = "入力エラーがあります"
        MessageBox.Show("入力値が不正です。入力し直してください。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        e.Cancel = True
    End Sub

    Private Sub Dgv_DataBindingComplete(sender As Object, e As DataGridViewBindingCompleteEventArgs)
        ' ★読み込み直後に ComboBox 表示を確定させる（UInt32/Int32 → Int64）
        For Each r As DataGridViewRow In estimateDataGridView.Rows
            If r.IsNewRow Then Continue For

            Dim v As Object = r.Cells("lot_id").Value
            If v Is Nothing OrElse v Is DBNull.Value Then Continue For

            Dim id64 As Long
            Try
                id64 = Convert.ToInt64(v)     ' UInt32 / Int32 / String 全部吸収
            Catch
                r.Cells("lot_id").Value = Nothing
                Continue For
            End Try

            ' ★同じ値の再代入だけど「Long型」で入れ直すのがポイント
            r.Cells("lot_id").Value = id64
        Next

        estimateDataGridView.EndEdit()
        estimateDataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit)
        estimateDataGridView.Invalidate()
    End Sub




    Private stockColName As String = Nothing

    Private Function DetectStockColumnName() As String
        If stockColName IsNot Nothing Then Return stockColName

        Dim candidates As String() = {
        "stock_qty",
        "stock_quantity",
        "inventory_qty",
        "qty_onhand",
        "available_qty",
        "onhand_qty",
        "quantity",
        "quantity_onhand"
    }

        Using conn As New MySqlConnection(connectionString)
            conn.Open()
            Dim sql As String =
            "SELECT COLUMN_NAME " &
            "FROM INFORMATION_SCHEMA.COLUMNS " &
            "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'item_master' AND COLUMN_NAME = @col " &
            "LIMIT 1"

            For Each c In candidates
                Using cmd As New MySqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@col", c)
                    Dim r = cmd.ExecuteScalar()
                    If r IsNot Nothing AndAlso r IsNot DBNull.Value Then
                        stockColName = r.ToString()
                        Return stockColName
                    End If
                End Using
            Next
        End Using

        ' 見つからない場合：在庫チェック不能（=チェックしない）
        stockColName = ""
        Return stockColName
    End Function

    Private Function GetAvailableQtyPieces(itemId As Integer) As Integer
        Dim sql As String =
        "SELECT quantity1 " &
        "FROM item_master " &
        "WHERE item_id = @item_id;"

        Using conn As New MySqlConnection(connectionString),
          cmd As New MySqlCommand(sql, conn)

            cmd.Parameters.Add("@item_id", MySqlDbType.Int32).Value = itemId

            conn.Open()
            Dim result = cmd.ExecuteScalar()

            If result Is Nothing OrElse IsDBNull(result) Then
                Throw New Exception($"在庫が取得できません（item_id={itemId}）")
            End If

            Dim v As Integer = 0
            Integer.TryParse(result.ToString(), v)
            If v < 0 Then v = 0
            Return v
        End Using
    End Function

    Private Sub Dgv_CellValidating_All(sender As Object, e As DataGridViewCellValidatingEventArgs)
        If isProgrammaticEdit Then Return ' ポップアップ反映などの内部更新は弾かない

        Dim dgv = estimateDataGridView
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Return

        Dim row = dgv.Rows(e.RowIndex)
        If row Is Nothing OrElse row.IsNewRow Then Return

        Dim colName As String = dgv.Columns(e.ColumnIndex).Name
        Dim input As String = If(e.FormattedValue, "").ToString().Trim()

        ' いったんクリア
        row.ErrorText = ""
        dgv.Rows(e.RowIndex).Cells(e.ColumnIndex).ErrorText = ""

        ' 「数量2が入った行だけ」厳格に必須チェックする（あなたの仕様と整合）
        Dim q2Now As String = SafeCellStr(row, "quantity2")
        Dim hasQ2 As Boolean = (q2Now <> "")

        ' --------------------------
        ' アイテム（item_name）：数量2が入ってる行は必須
        ' --------------------------
        If colName = "item_name" Then
            If Not hasQ2 AndAlso input = "" Then Return ' 数量2空の行は未入力扱いでOK

            If input = "" Then
                RejectCell(dgv, e, "アイテムを入力してください。")
                Return
            End If
        End If

        ' --------------------------
        ' item_id（裏列）：数量2が入ってる行は必須＆数値
        ' --------------------------
        If colName = "item_id" Then
            If Not hasQ2 Then Return

            If input = "" Then
                RejectCell(dgv, e, "アイテムが未確定です（選択/入力し直してください）。")
                Return
            End If

            Dim tmp As Long
            If Not Long.TryParse(input, tmp) OrElse tmp <= 0 Then
                RejectCell(dgv, e, "アイテムIDが不正です（選択し直してください）。")
                Return
            End If
        End If

        ' --------------------------
        ' 数量2（箱数）：空OK / 入れるなら 1以上の整数
        ' --------------------------
        ' --------------------------
        ' 数量2（箱数）：空OK / 入れるなら 1以上の整数 + 在庫チェック（個数）
        ' --------------------------
        ' --------------------------
        ' 数量2（箱数）：空OK / 入れるなら 1以上の整数 + 在庫チェック（個数）
        ' --------------------------
        If colName = "quantity2" Then
            If input = "" Then Return

            Dim boxCnt As Integer
            If Not Integer.TryParse(input, boxCnt) Then
                RejectCell(dgv, e, "数量2（箱数）は数値を入力してください。")
                Return
            End If
            If boxCnt <= 0 Then
                RejectCell(dgv, e, "数量2（箱数）は1以上を入力してください。")
                Return
            End If

            ' ★ item_id を確定（手入力の item_name も救済）
            Dim itemIdVal As Object = DBNull.Value
            Dim tmpId As Long

            Dim idStr As String = SafeCellStr(row, "item_id")
            If Long.TryParse(idStr, tmpId) AndAlso tmpId > 0 Then
                itemIdVal = tmpId
            Else
                Dim nameStr As String = SafeCellStr(row, "item_name")
                itemIdVal = ResolveItemIdByName(nameStr)
                If Not (itemIdVal Is Nothing OrElse itemIdVal Is DBNull.Value) Then
                    row.Cells("item_id").Value = itemIdVal.ToString()
                End If
            End If

            If itemIdVal Is Nothing OrElse itemIdVal Is DBNull.Value Then
                RejectCell(dgv, e, "先にアイテムを確定してください。")
                Return
            End If

            Dim itemId As Integer = CInt(Convert.ToInt64(itemIdVal))

            ' ★ 箱→個数へ換算して在庫（個数）と比較
            Dim conv As Integer = GetConversionQtyFromMaster(itemId) ' 1箱あたり個数
            Dim reqPieces As Long = CLng(boxCnt) * CLng(conv)
            If reqPieces > Integer.MaxValue Then
                RejectCell(dgv, e, "数量が大きすぎます。")
                Return
            End If

            Dim availablePieces As Integer = GetAvailableQtyPieces(itemId) ' 在庫（個数）

            If CInt(reqPieces) > availablePieces Then
                RejectCell(dgv, e, $"在庫がありません。要求={reqPieces}個 / 在庫={availablePieces}個")
                Return
            End If
        End If

        ' --------------------------
        ' 単価：数量2が入ってる行だけ厳格（空OK：あなたの仕様なら後で補完可能）
        ' ただし shiplistdetail は補完してないので、空禁止にしたいなら下のコメント外す
        ' --------------------------
        If colName = "unit_price" Then
            If Not hasQ2 Then Return

            If input = "" Then
                ' RejectCell(dgv, e, "単価を入力してください。") : Return   ' ←空禁止にしたい場合
                Return
            End If

            Dim v As Decimal
            If Not Decimal.TryParse(input, v) Then
                RejectCell(dgv, e, "単価は数値を入力してください。")
                Return
            End If
            If v < 0D Then
                RejectCell(dgv, e, "単価は0以上を入力してください。")
                Return
            End If
        End If

        ' --------------------------
        ' 数量（個数）：数量2が入ってる行だけ厳格（空OK＝換算で入る/保険）
        ' --------------------------
        If colName = "quantity" Then
            If Not hasQ2 Then Return

            If input = "" Then Return

            Dim v As Integer
            If Not Integer.TryParse(input, v) Then
                RejectCell(dgv, e, "数量（個数）は数値を入力してください。")
                Return
            End If
            If v <= 0 Then
                RejectCell(dgv, e, "数量（個数）は1以上を入力してください。")
                Return
            End If
        End If

        ' --------------------------
        ' 単位：数量2が入ってる行だけ必須
        ' --------------------------
        If colName = "unit" Then
            If Not hasQ2 Then Return
            If input = "" Then
                RejectCell(dgv, e, "単位を入力してください。")
                Return
            End If
        End If

        ' --------------------------
        ' 備考：長すぎるのは弾く（例：200）
        ' --------------------------
        If colName = "remark" Then
            If input.Length > 200 Then
                RejectCell(dgv, e, "備考は200文字以内で入力してください。")
                Return
            End If
        End If
    End Sub

    Private Sub RejectCell(dgv As DataGridView, e As DataGridViewCellValidatingEventArgs, msg As String)
        dgv.Rows(e.RowIndex).Cells(e.ColumnIndex).ErrorText = msg
        dgv.Rows(e.RowIndex).ErrorText = "入力エラーがあります"
        MessageBox.Show(msg, "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        e.Cancel = True
    End Sub

    ' 数字カラムに変な文字が混ざるのを「完全禁止」したくない場合は外してOK。
    '（禁止すると“打てない”挙動になるので、あなたの希望なら“弾く”だけの方が合う）
    Private Sub Dgv_EditingControlShowing(sender As Object, e As DataGridViewEditingControlShowingEventArgs)
        Dim dgv = estimateDataGridView
        If dgv.CurrentCell Is Nothing Then Return
        Dim colName = dgv.CurrentCell.OwningColumn.Name

        RemoveHandler e.Control.KeyPress, AddressOf NumericEditing_KeyPress

        ' ★ここは任意：禁止より“弾く”が希望なら付けなくてOK
        If colName = "quantity2" OrElse colName = "unit_price" OrElse colName = "quantity" Then
            AddHandler e.Control.KeyPress, AddressOf NumericEditing_KeyPress
        End If
    End Sub

    Private Sub NumericEditing_KeyPress(sender As Object, e As KeyPressEventArgs)
        If Char.IsControl(e.KeyChar) Then Return
        If Not Char.IsDigit(e.KeyChar) Then e.Handled = True
    End Sub


    Private Sub SetupGridValidation()
        ep.BlinkStyle = ErrorBlinkStyle.NeverBlink
        ep.ContainerControl = Me

        RemoveHandler estimateDataGridView.CellValidating, AddressOf Dgv_CellValidating_All
        AddHandler estimateDataGridView.CellValidating, AddressOf Dgv_CellValidating_All

        RemoveHandler estimateDataGridView.DataError, AddressOf Dgv_DataError_All
        AddHandler estimateDataGridView.DataError, AddressOf Dgv_DataError_All

        ' ★ここを消す（= “打てない”の原因）
        ' RemoveHandler estimateDataGridView.EditingControlShowing, AddressOf Dgv_EditingControlShowing
        AddHandler estimateDataGridView.EditingControlShowing, AddressOf Dgv_EditingControlShowing

        estimateDataGridView.ShowCellErrors = True
        estimateDataGridView.ShowRowErrors = True
    End Sub


    Private Sub Dgv_CellEndEdit_AutoAmount(sender As Object, e As DataGridViewCellEventArgs)
        Dim dgv = estimateDataGridView
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Return

        Dim colName = dgv.Columns(e.ColumnIndex).Name
        Dim row = dgv.Rows(e.RowIndex)
        If row Is Nothing OrElse row.IsNewRow Then Return

        ' ★仕様：quantity2 入力後に conversion_qty ポップアップ → quantity(個数)へ反映
        If colName = "quantity2" Then
            ShowConversionPopupAndReflect(row)
            ' 反映後に金額も再計算したいので続行
        End If
        RecalcAmountRow(row)

        ' ★仕様：amount = 数量2（箱）× unit_price（箱単価）
        If colName <> "quantity2" AndAlso colName <> "unit_price" Then Return

    End Sub


    ' =========================
    ' 行の再計算（数量2×単価）
    ' =========================
    Private Sub RecalcAmountRow(row As DataGridViewRow)
        If row Is Nothing OrElse row.IsNewRow Then Return
        If Not estimateDataGridView.Columns.Contains("quantity2") Then Return
        If Not estimateDataGridView.Columns.Contains("unit_price") Then Return
        If Not estimateDataGridView.Columns.Contains("amount") Then Return

        Dim q2Str As String = SafeCellStr(row, "quantity2")
        Dim pStr As String = SafeCellStr(row, "unit_price")

        If q2Str = "" OrElse pStr = "" Then
            row.Cells("amount").Value = ""
            Return
        End If

        Dim q2 As Decimal = 0D
        Dim p As Decimal = 0D
        If Not Decimal.TryParse(q2Str, q2) Then
            row.Cells("amount").Value = ""
            Return
        End If
        If Not Decimal.TryParse(pStr, p) Then
            row.Cells("amount").Value = ""
            Return
        End If

        If q2 <= 0 OrElse p < 0 Then
            row.Cells("amount").Value = ""
            Return
        End If

        row.Cells("amount").Value = (q2 * p).ToString("0")
    End Sub


    ' =========================
    ' Layout
    ' =========================
    Private Sub LayoutAll()
        If Me.ClientSize.Width <= 0 OrElse Me.ClientSize.Height <= 0 Then Return

        Dim scale As Single = Me.DeviceDpi / 96.0F
        Dim margin As Integer = CInt(32 * scale)
        Dim topBarH As Integer = CInt(80 * scale)

        titleLabel.Location = New Point(margin, CInt(22 * scale))
        shutdownBtn.Location = New Point(Me.ClientSize.Width - margin - shutdownBtn.Width, CInt(18 * scale))
        Using gp = RoundRectPath(New Rectangle(0, 0, shutdownBtn.Width, shutdownBtn.Height), 22)
            shutdownBtn.Region = New Region(gp)
        End Using

        Dim headerH As Integer = CInt(190 * scale)
        Dim footerH As Integer = CInt(84 * scale)
        Dim gap As Integer = CInt(18 * scale)

        Dim contentW As Integer = Me.ClientSize.Width - (margin * 2)
        Dim contentH As Integer = Me.ClientSize.Height - topBarH - footerH - (gap * 2) - margin
        If contentH < CInt(360 * scale) Then contentH = CInt(360 * scale)

        cardHeader.Location = New Point(margin, topBarH)
        cardHeader.Size = New Size(contentW, headerH)

        cardGrid.Location = New Point(margin, cardHeader.Bottom + gap)
        cardGrid.Size = New Size(contentW, Math.Max(CInt(260 * scale), contentH - headerH))

        footer.Location = New Point(margin, Me.ClientSize.Height - footerH - margin)
        footer.Size = New Size(contentW, footerH)

        Dim xL As Integer = CInt(24 * scale)
        Dim xI As Integer = CInt(170 * scale)
        Dim y As Integer = CInt(22 * scale)
        Dim rowGap As Integer = CInt(50 * scale)

        Dim inputW As Integer = CInt(260 * scale)
        Dim inputH As Integer = CInt(36 * scale)

        lblBatchId.Location = New Point(xL, y)
        txtBatchId.Location = New Point(xI, y - 2)
        txtBatchId.Size = New Size(inputW, inputH)

        y += rowGap
        lblShipmentDate.Location = New Point(xL, y)
        dtpShipmentDate.Location = New Point(xI, y - 3)
        dtpShipmentDate.Size = New Size(CInt(240 * scale), inputH)

        y += rowGap
        lblCustomerCode.Location = New Point(xL, y)
        txtCustomerCode.Location = New Point(xI, y - 2)
        txtCustomerCode.Size = New Size(inputW, inputH)

        Dim rightXLabel As Integer = xI + inputW + CInt(40 * scale)
        Dim rightXInput As Integer = rightXLabel + CInt(120 * scale)

        lblCustomerName.Location = New Point(rightXLabel, y)
        txtCustomerName.Location = New Point(rightXInput, y - 2)
        txtCustomerName.Size = New Size(cardHeader.Width - rightXInput - CInt(24 * scale), inputH)

        lblGridTitle.Location = New Point(CInt(24 * scale), CInt(18 * scale))
        estimateDataGridView.Location = New Point(CInt(24 * scale), lblGridTitle.Bottom + CInt(12 * scale))
        estimateDataGridView.Size = New Size(cardGrid.Width - CInt(48 * scale), cardGrid.Height - estimateDataGridView.Top - CInt(18 * scale))
        estimateDataGridView.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Bottom

        Dim bx As Integer = CInt(18 * scale)
        Dim by As Integer = CInt(20 * scale)
        Dim bw As Integer = CInt(140 * scale)
        Dim bh As Integer = CInt(44 * scale)
        Dim bgap As Integer = CInt(14 * scale)

        btnAddRow.Size = New Size(bw, bh)
        btnDeleteRow.Size = New Size(bw, bh)
        btnSave.Size = New Size(bw, bh)
        btnClose.Size = New Size(bw, bh)

        btnAddRow.Location = New Point(bx, by)
        btnDeleteRow.Location = New Point(btnAddRow.Right + bgap, by)

        btnClose.Location = New Point(footer.Width - bx - bw, by)
        btnSave.Location = New Point(btnClose.Left - bgap - bw, by)

        btnReport.Size = New Size(bw, bh)
        btnReport.Location = New Point(btnSave.Left - bgap - bw, by)
    End Sub

    ' =========================
    ' UI helpers
    ' =========================
    Private Function MakeLabel(text As String) As Label
        Return New Label() With {.Text = text, .Font = fontLabel, .ForeColor = cSubText, .AutoSize = True}
    End Function

    Private Function MakeTextBox(readOnlyStyle As Boolean) As TextBox
        Dim tb As New TextBox()
        tb.Font = fontInput
        tb.BorderStyle = BorderStyle.FixedSingle
        tb.ReadOnly = readOnlyStyle
        tb.BackColor = If(readOnlyStyle, Color.FromArgb(248, 249, 252), Color.White)
        tb.ForeColor = cText
        Return tb
    End Function

    ' =========================
    ' RoundedPanel (card)
    ' =========================
    Private Class RoundedPanel
        Inherits Panel
        Public Property Radius As Integer = 18
        Public Property BorderColor As Color = Color.Gainsboro
        Public Property BorderWidth As Integer = 1
        Public Property FillColor As Color = Color.White
        Public Property Shadow As Boolean = True

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            MyBase.OnPaint(e)
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias

            Dim rect As Rectangle = Me.ClientRectangle
            rect.Inflate(-1, -1)

            If Shadow Then
                Using shadowBrush As New SolidBrush(Color.FromArgb(22, 0, 0, 0))
                    Dim sRect As Rectangle = rect
                    sRect.Offset(0, 5)
                    sRect.Inflate(2, 2)
                    Using sp As GraphicsPath = CreateRoundPathLocal(sRect, Radius)
                        e.Graphics.FillPath(shadowBrush, sp)
                    End Using
                End Using
            End If

            Using path As GraphicsPath = CreateRoundPathLocal(rect, Radius)
                Using fill As New SolidBrush(FillColor)
                    e.Graphics.FillPath(fill, path)
                End Using
                Using pen As New Pen(BorderColor, BorderWidth)
                    e.Graphics.DrawPath(pen, path)
                End Using
            End Using
        End Sub

        Private Function CreateRoundPathLocal(r As Rectangle, radius As Integer) As GraphicsPath
            Dim p As New GraphicsPath()
            Dim d As Integer = radius * 2
            p.StartFigure()
            p.AddArc(r.X, r.Y, d, d, 180, 90)
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90)
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90)
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90)
            p.CloseFigure()
            Return p
        End Function
    End Class

    Private Sub StylePrimary(btn As Button)
        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderSize = 0
        btn.BackColor = cPrimary
        btn.ForeColor = Color.White
        btn.Font = New Font("Yu Gothic UI", 12.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(128))
        btn.Cursor = Cursors.Hand

        AddHandler btn.MouseEnter, Sub() btn.BackColor = cPrimaryDark
        AddHandler btn.MouseLeave, Sub() btn.BackColor = cPrimary

        AddHandler btn.Resize, Sub()
                                   If btn.Width <= 0 OrElse btn.Height <= 0 Then Return
                                   Using gp = RoundRectPath(New Rectangle(0, 0, btn.Width, btn.Height), 16)
                                       btn.Region = New Region(gp)
                                   End Using
                               End Sub
    End Sub

    Private Sub StyleGhost(btn As Button)
        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderSize = 1
        btn.FlatAppearance.BorderColor = cBorder
        btn.BackColor = Color.White
        btn.ForeColor = cText
        btn.Font = New Font("Yu Gothic UI", 12.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(128))
        btn.Cursor = Cursors.Hand

        AddHandler btn.MouseEnter, Sub() btn.BackColor = Color.FromArgb(245, 246, 250)
        AddHandler btn.MouseLeave, Sub() btn.BackColor = Color.White

        AddHandler btn.Resize, Sub()
                                   If btn.Width <= 0 OrElse btn.Height <= 0 Then Return
                                   Using gp = RoundRectPath(New Rectangle(0, 0, btn.Width, btn.Height), 16)
                                       btn.Region = New Region(gp)
                                   End Using
                               End Sub
    End Sub

    Private Function RoundRectPath(r As Rectangle, radius As Integer) As GraphicsPath
        Dim p As New GraphicsPath()
        Dim d As Integer = radius * 2
        p.StartFigure()
        p.AddArc(r.X, r.Y, d, d, 180, 90)
        p.AddArc(r.Right - d, r.Y, d, d, 270, 90)
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90)
        p.AddArc(r.X, r.Bottom - d, d, d, 90, 90)
        p.CloseFigure()
        Return p
    End Function

    ' =========================
    ' Delete row
    ' =========================
    Private Sub DeleteSelectedRow(sender As Object, e As EventArgs)
        If estimateDataGridView Is Nothing Then Return
        If estimateDataGridView.SelectedRows Is Nothing OrElse estimateDataGridView.SelectedRows.Count = 0 Then Return

        Dim r = estimateDataGridView.SelectedRows(0)
        If r IsNot Nothing AndAlso Not r.IsNewRow Then
            estimateDataGridView.Rows.Remove(r)
        End If
    End Sub

    ' =========================
    ' 読み込み（header + detail + customer）
    ' =========================
    Private Sub LoadShipmentBatch(batchId As Long)

        Dim sqlH As String =
        "SELECT h.shipment_batch_id, h.shipment_date, h.customer_code, COALESCE(c.customer_name,'') AS customer_name " &
        "FROM shipments_header h " &
        "LEFT JOIN customer_master c ON c.customer_code = h.customer_code " &
        "WHERE h.shipment_batch_id = @id"

        Using conn As New MySqlConnection(connectionString)
            conn.Open()

            ' --- header ---
            Using cmdH As New MySqlCommand(sqlH, conn)
                cmdH.Parameters.AddWithValue("@id", batchId)
                Using rdr = cmdH.ExecuteReader()
                    If rdr.Read() Then
                        txtBatchId.Text = SafeStr(rdr("shipment_batch_id"))

                        Dim dtObj = rdr("shipment_date")
                        If dtObj Is DBNull.Value Then
                            dtpShipmentDate.Checked = False
                        Else
                            dtpShipmentDate.Value = Convert.ToDateTime(dtObj)
                            dtpShipmentDate.Checked = True
                        End If

                        txtCustomerCode.Text = SafeStr(rdr("customer_code"))
                        txtCustomerName.Text = SafeStr(rdr("customer_name"))
                    End If
                End Using
            End Using

            ' --- detail ---
            ' ※JSON方式の画面は lot_id Combo を持たないので、ここでは “列名で” 埋めるだけ
            Dim sqlD As String =
"SELECT
  il.ref_line_no AS line_no,
  il.item_id,
  il.lot_id,
  COALESCE(m.item_name,'') AS item_name,
  '' AS unit,
  (-il.qty_delta) AS quantity,
  0 AS quantity2,
  0 AS unit_price,
  0 AS amount,
  '' AS remark,
  COALESCE(l.lot_no,'') AS lot_no,
  COALESCE(l.qty_on_hand_pieces,0) AS lot_available   -- ★必須
FROM inventory_ledger il
LEFT JOIN lot l ON l.lot_id = il.lot_id
LEFT JOIN item_master m ON m.item_id = il.item_id
WHERE il.ref_type='SHIPMENT' AND il.ref_id=@id AND il.entry_type='ISSUE' AND il.is_void=0
ORDER BY il.ref_line_no;
;
"

            isProgrammaticEdit = True
            Try
                estimateDataGridView.Rows.Clear()

                Using cmdD As New MySqlCommand(sqlD, conn)
                    cmdD.Parameters.AddWithValue("@id", batchId)

                    Using rdr = cmdD.ExecuteReader()

                        ' ---- グルーピング用 ----
                        Dim curRow As DataGridViewRow = Nothing
                        Dim curItemId As Long = -1
                        Dim curUnit As String = ""
                        Dim curRemark As String = ""
                        Dim curPrice As Decimal = -1D

                        Dim groupQtySum As Integer = 0
                        Dim groupQty2 As Decimal = 0D
                        Dim groupAmount As Decimal = 0D

                        ' lot_id -> LotAlloc（同一ロットが複数行でも合算）
                        Dim allocMap As Dictionary(Of Long, LotAlloc) = Nothing

                        Dim hasGroup As Boolean = False

                        Dim finalizeGroup As Action =
                Sub()
                    If Not hasGroup OrElse curRow Is Nothing Then Return

                    ' グループ合計を画面へ
                    curRow.Cells("quantity").Value = groupQtySum.ToString()
                    curRow.Cells("quantity2").Value = If(groupQty2 > 0D, groupQty2.ToString("0"), "")
                    curRow.Cells("amount").Value = If(groupAmount > 0D, groupAmount.ToString("0"), "")
                    ' lot_info / json
                    Dim allocs As List(Of LotAlloc) =
                        If(allocMap Is Nothing, New List(Of LotAlloc)(), allocMap.Values.ToList())
                    SetLotInfo(curRow, allocs)
                End Sub

                        Dim prevItemId As Long = -1
                        Dim prevUnit As String = ""
                        Dim prevRemark As String = ""
                        Dim prevPrice As Decimal = -1D
                        Dim prevWasMarker As Boolean = False

                        While rdr.Read()

                            Dim itemIdObj = rdr("item_id")
                            Dim itemIdNow As Long = If(itemIdObj Is DBNull.Value, -1L, Convert.ToInt64(itemIdObj))

                            Dim unitNow As String = SafeStr(rdr("unit"))
                            Dim remarkNow As String = SafeStr(rdr("remark"))

                            Dim priceNow As Decimal = 0D
                            Decimal.TryParse(SafeStr(rdr("unit_price")), priceNow)

                            Dim qtyNow As Integer = 0
                            Integer.TryParse(SafeStr(rdr("quantity")), qtyNow)

                            Dim qty2Now As Decimal = 0D
                            Decimal.TryParse(SafeStr(rdr("quantity2")), qty2Now)

                            Dim amtNow As Decimal = 0D
                            Decimal.TryParse(SafeStr(rdr("amount")), amtNow)

                            ' ★グループ開始判定
                            '  - 展開保存では「先頭行だけ quantity2/amount が入る」前提
                            '  - ただし、念のため item/unit/price/remark の変化でも区切る
                            Dim isMarker As Boolean = (qty2Now > 0D) OrElse (amtNow > 0D)

                            Dim startNew As Boolean =
                    (Not hasGroup) OrElse
                    isMarker OrElse
                    (itemIdNow <> prevItemId) OrElse
                    (unitNow <> prevUnit) OrElse
                    (remarkNow <> prevRemark) OrElse
                    (priceNow <> prevPrice) OrElse
                    (prevWasMarker AndAlso isMarker) ' 念のため

                            If startNew Then
                                ' 直前グループ確定
                                finalizeGroup()

                                ' 新グループ初期化
                                hasGroup = True
                                groupQtySum = 0
                                groupQty2 = 0D
                                groupAmount = 0D
                                allocMap = New Dictionary(Of Long, LotAlloc)()

                                ' 行を作成
                                Dim rowIdx = estimateDataGridView.Rows.Add()
                                curRow = estimateDataGridView.Rows(rowIdx)

                                curRow.Cells("item_name").Value = SafeStr(rdr("item_name"))
                                curRow.Cells("item_id").Value = If(itemIdNow > 0, itemIdNow.ToString(), "")

                                curRow.Cells("unit").Value = unitNow
                                curRow.Cells("unit_price").Value = priceNow.ToString("0")

                                ' 先頭行の qty2/amount を採用（無い場合もある）
                                If qty2Now > 0D Then groupQty2 = qty2Now
                                If amtNow > 0D Then groupAmount = amtNow

                                ' いったん初期化（後で finalizeGroup で上書き）
                                curRow.Cells("lot_info").Value = "未割当"
                                curRow.Cells("lot_alloc_json").Value = ""
                                curRow.Cells("remark").Value = remarkNow

                                curItemId = itemIdNow
                                curUnit = unitNow
                                curRemark = remarkNow
                                curPrice = priceNow
                            Else
                                ' 同一グループ継続：先頭以外の qty2/amount は基本0の想定
                                '（もし入って来ても「先頭を優先」するなら無視でOK）
                            End If

                            ' グループ合計 quantity
                            If qtyNow > 0 Then groupQtySum += qtyNow

                            ' ロット割当追加
                            Dim lotIdObj = rdr("lot_id")
                            If Not (lotIdObj Is Nothing OrElse lotIdObj Is DBNull.Value) Then
                                Dim lotId As Long = Convert.ToInt64(lotIdObj)
                                If lotId > 0 AndAlso qtyNow > 0 Then
                                    If Not allocMap.ContainsKey(lotId) Then
                                        allocMap(lotId) = New LotAlloc With {
                                .LotId = lotId,
                                .LotNo = SafeStr(rdr("lot_no")),
                                .QtyPieces = 0,
                                .Available = CInt(Val(SafeStr(rdr("lot_available"))))
                            }
                                    End If
                                    allocMap(lotId).QtyPieces += qtyNow
                                End If
                            End If

                            ' 次比較用
                            prevItemId = itemIdNow
                            prevUnit = unitNow
                            prevRemark = remarkNow
                            prevPrice = priceNow
                            prevWasMarker = isMarker
                        End While

                        ' 最終グループ確定
                        finalizeGroup()
                    End Using
                End Using

            Finally
                isProgrammaticEdit = False
                estimateDataGridView.Refresh()
                estimateDataGridView.Invalidate()
                estimateDataGridView.AutoResizeColumns()
            End Try
        End Using
    End Sub

    ' =========================
    ' 顧客コード → 顧客名
    ' =========================
    Private Sub txtCustomerCode_Leave(sender As Object, e As EventArgs)
        Dim code As String = txtCustomerCode.Text.Trim()
        If code = "" Then
            txtCustomerName.Text = ""
            Return
        End If

        Dim sql As String = "SELECT customer_name FROM customer_master WHERE customer_code = @code LIMIT 1"

        Using conn As New MySqlConnection(connectionString)
            conn.Open()
            Using cmd As New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@code", code)
                Dim result = cmd.ExecuteScalar()
                txtCustomerName.Text = If(result Is Nothing OrElse result Is DBNull.Value, "", result.ToString())
            End Using
        End Using
    End Sub

    ' =========================
    ' Grid編集：金額再計算（数量×単価）
    ' =========================
    ' =========================
    ' Grid編集：金額再計算（数量2×単価） + quantity2編集時に換算ポップアップ
    ' =========================
    Private Sub Grid_CellEndEdit(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex < 0 Then Return

        Dim row = estimateDataGridView.Rows(e.RowIndex)
        If row Is Nothing OrElse row.IsNewRow Then Return

        Dim colName As String = estimateDataGridView.Columns(e.ColumnIndex).Name

        ' ★ item_name 編集終了：item_id確定→ロット再読込
        If colName = "item_name" Then
            Dim nameStr As String = SafeCellStr(row, "item_name")
            Dim idVal As Object = ResolveItemIdByName(nameStr)

            If Not (idVal Is Nothing OrElse idVal Is DBNull.Value) Then
                row.Cells("item_id").Value = idVal.ToString()

                ' ロット候補を再ロード（itemId確定後）
                FillLotComboForRow(row, CInt(Convert.ToInt64(idVal)))

                ' 表示確定（UInt32等を排除）
                Dim v = row.Cells("lot_id").Value
                If Not (v Is Nothing OrElse v Is DBNull.Value) Then
                    row.Cells("lot_id").Value = Convert.ToInt64(v)
                End If
            Else
                row.Cells("item_id").Value = ""
                row.Cells("lot_id").Value = Nothing     ' ★ keepLotId は使わない
                Dim cell = TryCast(row.Cells("lot_id"), DataGridViewComboBoxCell)
                If cell IsNot Nothing Then cell.DataSource = Nothing
            End If
        End If


        ' ★ quantity2 編集終了：換算ポップアップ → quantityへ反映
        If colName = "quantity2" Then
            ' 1) conversion_qty ポップアップ → quantity(個)へ反映
            ShowConversionPopupAndReflect(row)

            ' 2) そのまま在庫詳細を開く（自動）
            '    ※勝手に開きたくないなら、ここはコメントアウトしてOK
            Dim r As Integer = row.Index
            Dim c As Integer = estimateDataGridView.Columns("btn_stock").Index
            Dgv_CellContentClick_Stock(estimateDataGridView, New DataGridViewCellEventArgs(c, r))
        End If


        ' ★ 金額は「数量2 × 単価（箱単価）」で計算
        If colName <> "quantity2" AndAlso colName <> "unit_price" Then Return

        Dim q2 As Decimal = 0D
        Dim p As Decimal = 0D
        Decimal.TryParse(SafeCellStr(row, "quantity2"), q2)
        Decimal.TryParse(SafeCellStr(row, "unit_price"), p)

        If q2 <= 0 OrElse p < 0 Then
            row.Cells("amount").Value = ""
            Return
        End If

        row.Cells("amount").Value = (q2 * p).ToString("0")
    End Sub


    ' DataGridView の行ごとに lot候補を入れて、keepLotId があればそれを選択状態にする
    Private Sub FillLotComboForRow(row As DataGridViewRow, itemId As Integer, Optional keepLotId As Long? = Nothing)

        If estimateDataGridView Is Nothing Then Exit Sub
        If Not estimateDataGridView.Columns.Contains("lot_id") Then Exit Sub


        Dim cell = TryCast(row.Cells("lot_id"), DataGridViewComboBoxCell)
        If cell Is Nothing Then Exit Sub

        ' ★ValueType を Long に固定（ここが超重要）
        cell.ValueType = GetType(Long)

        Dim dt As New DataTable()
        dt.Columns.Add("lot_id", GetType(Long))
        dt.Columns.Add("lot_no", GetType(String))

        ' 先頭に「未選択」
        dt.Rows.Add(0L, "")

        Using conn As New MySqlConnection(connectionString)
            conn.Open()

            ' ★keepLotId があるなら「候補に無くても」拾えるようにする
            '   (例：is_active=0 を通常候補から除外してる場合でも、既存明細のlotは表示したい)
            Dim sql As String =
"SELECT l.lot_id, l.lot_no
   FROM lot l
  WHERE l.item_id = @item_id
    AND (l.is_active = 1 OR l.lot_id = @keep_id)
  ORDER BY l.received_date DESC, l.lot_id DESC"

            Using cmd As New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@item_id", itemId)
                cmd.Parameters.AddWithValue("@keep_id", If(keepLotId.HasValue, keepLotId.Value, -1L))

                Using rdr = cmd.ExecuteReader()
                    While rdr.Read()
                        Dim lid As Long = Convert.ToInt64(rdr("lot_id"))
                        Dim lno As String = Convert.ToString(rdr("lot_no"))
                        dt.Rows.Add(lid, lno)
                    End While
                End Using
            End Using
        End Using

        ' ★DataSource を先に確定
        cell.DataSource = dt
        cell.DisplayMember = "lot_no"
        cell.ValueMember = "lot_id"

        ' ★表示スタイル（任意）
        cell.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton
        Debug.WriteLine(keepLotId, "tes2222t")

        ' ★既存 lot を必ず表示させる：ここで Value を入れる
        If keepLotId.HasValue Then
            Dim found As Boolean = False
            For Each r As DataRow In dt.Rows
                If Convert.ToInt64(r("lot_id")) = keepLotId.Value Then

                    found = True
                    Exit For
                End If
            Next


            If found Then
                Debug.WriteLine(keepLotId, "tes2222t")

                cell.Value = CLng(keepLotId.Value)

                Debug.WriteLine($"keepLotId={keepLotId} / type={(If(keepLotId.HasValue, keepLotId.Value.GetType().FullName, "Nothing"))}")

                If dt.Rows.Count > 1 Then
                    Debug.WriteLine($"dt lot_id type={dt.Rows(1)("lot_id").GetType().FullName}")
                End If
                Debug.WriteLine($"col.ValueType={estimateDataGridView.Columns("lot_id").ValueType?.FullName}")
                Debug.WriteLine($"cell.ValueType={cell.ValueType?.FullName}")

            Else
                ' 候補に存在しないなら空欄扱い（または 0）
                cell.Value = 0L
            End If
        Else
            cell.Value = 0L
        End If
    End Sub





    Private Sub ApplyInventoryLedger_ForShipmentUpdate(
    batchId As Long,
    details As List(Of DetailRow),
    conn As MySqlConnection,
    tx As MySqlTransaction
)
        '=========================
        ' 0) oldSeq は「台帳の MAX(save_seq)」を正とする（ヘッダ不整合でも重複しない）
        '=========================
        Dim oldSeq As Integer = 0
        Using cmdMax As New MySqlCommand(
        "SELECT COALESCE(MAX(save_seq),0) " &
        "FROM inventory_ledger " &
        "WHERE ref_type='SHIPMENT' AND ref_id=@id", conn, tx)
            cmdMax.Parameters.AddWithValue("@id", batchId)
            Dim r = cmdMax.ExecuteScalar()
            oldSeq = If(r Is Nothing OrElse IsDBNull(r), 0, Convert.ToInt32(r))
        End Using

        Dim newSeq As Integer = oldSeq + 1

        ' shipments_header.save_seq は “表示用” として同期（任意だがやるのが安全）
        Using cmdUpd As New MySqlCommand(
        "UPDATE shipments_header SET save_seq=@seq WHERE shipment_batch_id=@id", conn, tx)
            cmdUpd.Parameters.AddWithValue("@seq", newSeq)
            cmdUpd.Parameters.AddWithValue("@id", batchId)
            cmdUpd.ExecuteNonQuery()
        End Using

        '=========================
        ' 1) 旧台帳(ISSUE)を集計し item_master.quantity1 を戻す（＋）
        '=========================
        Dim oldAgg As New Dictionary(Of Integer, Integer)() ' itemId -> sum(issue_qty_abs)

        If oldSeq > 0 Then
            Using cmdOld As New MySqlCommand(
            "SELECT item_id, SUM(-qty_delta) AS qty_out " &
            "FROM inventory_ledger " &
            "WHERE ref_type='SHIPMENT' AND ref_id=@id AND save_seq=@seq " &
            "  AND entry_type='ISSUE' AND is_void=0 " &
            "GROUP BY item_id", conn, tx)

                cmdOld.Parameters.AddWithValue("@id", batchId)
                cmdOld.Parameters.AddWithValue("@seq", oldSeq)

                Using rdr = cmdOld.ExecuteReader()
                    While rdr.Read()
                        Dim itemId = Convert.ToInt32(rdr("item_id"))
                        Dim qtyOut = Convert.ToInt32(rdr("qty_out")) ' 出庫個数（正の値）
                        If qtyOut > 0 Then oldAgg(itemId) = qtyOut
                    End While
                End Using
            End Using
        End If

        '=========================
        ' 1-1) 対象itemをロック（同時更新対策）
        '=========================
        If oldAgg.Count > 0 OrElse details.Count > 0 Then
            Dim lockIds As New HashSet(Of Integer)()

            For Each kv In oldAgg
                lockIds.Add(kv.Key)
            Next
            For Each d In details
                If d.ItemId IsNot Nothing AndAlso Not IsDBNull(d.ItemId) Then
                    lockIds.Add(Convert.ToInt32(Convert.ToInt64(d.ItemId)))
                End If
            Next

            Dim idList = lockIds.ToList()
            If idList.Count > 0 Then
                Using cmdLockItems As New MySqlCommand("", conn, tx)
                    Dim ps As New List(Of String)()
                    For i = 0 To idList.Count - 1
                        Dim pName = "@p" & i
                        ps.Add(pName)
                        cmdLockItems.Parameters.AddWithValue(pName, idList(i))
                    Next
                    cmdLockItems.CommandText =
                    "SELECT item_id FROM item_master WHERE item_id IN (" & String.Join(",", ps) & ") FOR UPDATE"
                    cmdLockItems.ExecuteNonQuery()
                End Using
            End If
        End If

        '=========================
        ' 1-2) quantity1 を戻す（＋）
        '=========================
        Using cmdAdd As New MySqlCommand(
        "UPDATE item_master SET quantity1 = quantity1 + @delta WHERE item_id=@item_id", conn, tx)
            cmdAdd.Parameters.Add("@delta", MySqlDbType.Int32)
            cmdAdd.Parameters.Add("@item_id", MySqlDbType.Int32)

            For Each kv In oldAgg
                cmdAdd.Parameters("@delta").Value = kv.Value
                cmdAdd.Parameters("@item_id").Value = kv.Key
                cmdAdd.ExecuteNonQuery()
            Next
        End Using

        '=========================
        ' 1-3) 旧台帳を void（消さない）
        '=========================
        If oldSeq > 0 Then
            Using cmdVoid As New MySqlCommand(
            "UPDATE inventory_ledger " &
            "SET is_void=1, voided_at=NOW(), void_reason='shipment re-save' " &
            "WHERE ref_type='SHIPMENT' AND ref_id=@id AND save_seq=@seq AND is_void=0", conn, tx)
                cmdVoid.Parameters.AddWithValue("@id", batchId)
                cmdVoid.Parameters.AddWithValue("@seq", oldSeq)
                cmdVoid.ExecuteNonQuery()
            End Using
        End If

        '=========================
        ' 2) 新明細を ISSUE として台帳へ（qty_deltaはマイナス）
        '   ★ref_line_no は d.LineNo を使う（再採番しない）
        '   ★lot_id も入れる（推奨）
        '=========================
        Using cmdIns As New MySqlCommand(
        "INSERT INTO inventory_ledger " &
        "(item_id, lot_id, qty_delta, ref_type, ref_id, ref_line_no, entry_type, save_seq) " &
        "VALUES (@item_id, @lot_id, @qty_delta, 'SHIPMENT', @ref_id, @line_no, 'ISSUE', @save_seq)", conn, tx)

            cmdIns.Parameters.Add("@item_id", MySqlDbType.Int32)
            cmdIns.Parameters.Add("@lot_id", MySqlDbType.Int64)
            cmdIns.Parameters.Add("@qty_delta", MySqlDbType.Int32)
            cmdIns.Parameters.Add("@ref_id", MySqlDbType.Int64).Value = batchId
            cmdIns.Parameters.Add("@line_no", MySqlDbType.Int32)
            cmdIns.Parameters.Add("@save_seq", MySqlDbType.Int32).Value = newSeq

            For Each d In details
                If d.ItemId Is Nothing OrElse IsDBNull(d.ItemId) Then Continue For

                Dim itemId As Integer = Convert.ToInt32(Convert.ToInt64(d.ItemId))
                Dim pieces As Integer = CInt(Math.Truncate(d.Quantity))
                If pieces <= 0 Then Continue For

                cmdIns.Parameters("@item_id").Value = itemId
                cmdIns.Parameters("@lot_id").Value =
                If(d.LotId Is Nothing OrElse IsDBNull(d.LotId), DBNull.Value, Convert.ToInt64(d.LotId))
                cmdIns.Parameters("@qty_delta").Value = -pieces

                ' ★ここが重要：d.LineNo を使う
                cmdIns.Parameters("@line_no").Value = d.LineNo

                cmdIns.ExecuteNonQuery()
            Next
        End Using

        '=========================
        ' 3) item_master.quantity1 を出庫分だけ減らす（－）
        '=========================
        Using cmdSub As New MySqlCommand(
        "UPDATE item_master SET quantity1 = quantity1 - @delta WHERE item_id=@item_id", conn, tx)
            cmdSub.Parameters.Add("@delta", MySqlDbType.Int32)
            cmdSub.Parameters.Add("@item_id", MySqlDbType.Int32)

            For Each d In details
                If d.ItemId Is Nothing OrElse IsDBNull(d.ItemId) Then Continue For
                Dim itemId As Integer = Convert.ToInt32(Convert.ToInt64(d.ItemId))
                Dim pieces As Integer = CInt(Math.Truncate(d.Quantity))
                If pieces <= 0 Then Continue For

                cmdSub.Parameters("@delta").Value = pieces
                cmdSub.Parameters("@item_id").Value = itemId
                cmdSub.ExecuteNonQuery()
            Next
        End Using
    End Sub


    ' =========================
    ' ★個体（lot_unit）払い出し＆shipment_unit_alloc紐付け
    ' =========================

    ' lot_unit から「ON_HAND」を必要数だけロックして取得
    Private Function PickUnitIdsForIssue(lotId As Long, needQty As Integer,
                                    conn As MySqlConnection, tx As MySqlTransaction) As List(Of Long)

        Dim ids As New List(Of Long)()

        Dim sql As String =
"SELECT unit_id
  FROM lot_unit
 WHERE lot_id = @lot_id
   AND status = 'ON_HAND'
 ORDER BY unit_id
 LIMIT @n
 FOR UPDATE;"

        Using cmd As New MySqlCommand(sql, conn, tx)
            cmd.Parameters.AddWithValue("@lot_id", lotId)
            cmd.Parameters.AddWithValue("@n", needQty)
            Using rdr = cmd.ExecuteReader()
                While rdr.Read()
                    ids.Add(Convert.ToInt64(rdr("unit_id")))
                End While
            End Using
        End Using

        If ids.Count <> needQty Then
            Throw New Exception($"lot_unit不足: lot_id={lotId}, 要求={needQty}, 取得={ids.Count}")
        End If

        Return ids
    End Function

    ' shipment_unit_alloc に紐付けて、lot_unit を ISSUED に更新
    Private Sub ApplyUnitAlloc_Issue(shipmentDetailId As Long, lotId As Long, qtyPieces As Integer,
                                conn As MySqlConnection, tx As MySqlTransaction)

        If qtyPieces <= 0 Then Exit Sub

        ' 1) unit_id を確保（FOR UPDATE）
        Dim unitIds = PickUnitIdsForIssue(lotId, qtyPieces, conn, tx)

        ' 2) 紐付け INSERT
        Using cmdIns As New MySqlCommand(
        "INSERT INTO shipment_unit_alloc (shipment_detail_id, unit_id) VALUES (@d,@u)", conn, tx)

            cmdIns.Parameters.Add("@d", MySqlDbType.Int64).Value = shipmentDetailId
            cmdIns.Parameters.Add("@u", MySqlDbType.Int64)

            For Each uid In unitIds
                cmdIns.Parameters("@u").Value = uid
                cmdIns.ExecuteNonQuery()
            Next
        End Using

        ' 3) lot_unit を出庫済みに更新
        Dim ps As New List(Of String)()
        Using cmdUp As New MySqlCommand("", conn, tx)
            For i = 0 To unitIds.Count - 1
                Dim p = "@p" & i
                ps.Add(p)
                cmdUp.Parameters.AddWithValue(p, unitIds(i))
            Next

            cmdUp.CommandText =
            "UPDATE lot_unit " &
            "SET status='ISSUED' " &
            "WHERE unit_id IN (" & String.Join(",", ps) & ")"

            cmdUp.ExecuteNonQuery()
        End Using
    End Sub

    ' 旧 shipment_unit_alloc を batch で削除（CASCADEが無い環境の保険）
    Private Sub DeleteUnitAllocByBatch(batchId As Long, conn As MySqlConnection, tx As MySqlTransaction)
        Dim sql As String =
"DELETE sua
   FROM shipment_unit_alloc sua
   JOIN shipments_detail sd ON sd.shipment_detail_id = sua.shipment_detail_id
  WHERE sd.shipment_batch_id=@id;"

        Using cmd As New MySqlCommand(sql, conn, tx)
            cmd.Parameters.AddWithValue("@id", batchId)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Private Sub ShowConversionPopupAndReflect(row As DataGridViewRow)
        If row Is Nothing OrElse row.IsNewRow Then Return

        ' 箱数
        Dim boxStr As String = SafeCellStr(row, "quantity2")
        Dim boxCnt As Integer
        If boxStr = "" OrElse Not Integer.TryParse(boxStr, boxCnt) OrElse boxCnt <= 0 Then
            row.Cells("quantity").Value = ""
            Return
        End If

        ' item_id を確定（手入力行も救済）
        Dim itemIdVal As Object = DBNull.Value
        Dim tmp As Long

        Dim itemIdStr As String = SafeCellStr(row, "item_id")
        If Long.TryParse(itemIdStr, tmp) AndAlso tmp > 0 Then
            itemIdVal = tmp
        Else
            Dim itemNameStr As String = SafeCellStr(row, "item_name")
            itemIdVal = ResolveItemIdByName(itemNameStr)
            If Not (itemIdVal Is Nothing OrElse itemIdVal Is DBNull.Value) Then
                row.Cells("item_id").Value = itemIdVal.ToString()
            End If
        End If

        If itemIdVal Is Nothing OrElse itemIdVal Is DBNull.Value Then
            ' item が確定できないならポップアップ出せない
            Return
        End If

        Dim itemId As Integer = CInt(Convert.ToInt64(itemIdVal))
        Dim defaultConv As Integer = GetConversionQtyFromMaster(itemId)
        Dim itemName As String = SafeCellStr(row, "item_name")
        If itemName = "" Then itemName = GetItemNameFromMaster(itemId)

        ' 親フォームを隠す → ダイアログ → 戻す（フォーカス崩れ対策）
        Me.Hide()
        Try
            Using dlg As New ConversionQtyDialog(itemName, defaultConv)
                Dim res = dlg.ShowDialog()
                If res <> DialogResult.OK Then Return

                Dim conv As Integer = dlg.ConversionQty
                Dim pieces As Long = CLng(boxCnt) * CLng(conv)
                If pieces > Integer.MaxValue Then
                    MessageBox.Show("数量が大きすぎます。")
                    row.Cells("quantity").Value = ""
                    Return
                End If

                ' ★数量1（個数）へ反映
                row.Cells("quantity").Value = CInt(pieces).ToString()
            End Using
        Finally
            Me.Show()
            Me.Activate()
        End Try
    End Sub


    ' =========================
    ' conversion_qty 編集ダイアログ
    ' =========================
    Private Class ConversionQtyDialog
        Inherits Form

        Public ReadOnly Property ConversionQty As Integer
            Get
                Dim v As Integer = 1
                Integer.TryParse(txtConv.Text.Trim(), v)
                If v <= 0 Then v = 1
                Return v
            End Get
        End Property

        Private lbl As Label
        Private txtConv As TextBox
        Private btnOk As Button
        Private btnCancel As Button

        Public Sub New(itemName As String, defaultConv As Integer)
            Me.Text = "単位換算（1箱あたり個数）"
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.StartPosition = FormStartPosition.CenterScreen
            Me.ClientSize = New Size(420, 190)

            lbl = New Label() With {
            .AutoSize = False,
            .Text = $"[{itemName}]" & vbCrLf &
                    "1箱あたりの個数（conversion_qty）を確認／修正してください。",
            .Left = 16,
            .Top = 14,
            .Width = 388,
            .Height = 60
        }

            txtConv = New TextBox() With {
            .Left = 16,
            .Top = 84,
            .Width = 160,
            .Text = If(defaultConv <= 0, "1", defaultConv.ToString())
        }

            btnOk = New Button() With {
            .Text = "OK",
            .Left = 220,
            .Top = 130,
            .Width = 90,
            .Height = 34
        }

            btnCancel = New Button() With {
            .Text = "キャンセル",
            .Left = 314,
            .Top = 130,
            .Width = 90,
            .Height = 34
        }

            AddHandler btnOk.Click,
            Sub()
                Dim v As Integer
                If Not Integer.TryParse(txtConv.Text.Trim(), v) OrElse v <= 0 Then
                    MessageBox.Show("1以上の整数を入力してください。")
                    txtConv.Focus()
                    Return
                End If
                Me.DialogResult = DialogResult.OK
                Me.Close()
            End Sub

            AddHandler btnCancel.Click,
            Sub()
                Me.DialogResult = DialogResult.Cancel
                Me.Close()
            End Sub

            Me.Controls.Add(lbl)
            Me.Controls.Add(txtConv)
            Me.Controls.Add(btnOk)
            Me.Controls.Add(btnCancel)

            Me.AcceptButton = btnOk
            Me.CancelButton = btnCancel
        End Sub
    End Class

    Private Function GetConversionQtyFromMaster(itemId As Integer) As Integer
        Using conn As New MySqlConnection(connectionString)
            conn.Open()
            Dim sql As String = "SELECT COALESCE(conversion_qty,1) FROM item_master WHERE item_id=@id LIMIT 1"
            Using cmd As New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@id", itemId)
                Dim r = cmd.ExecuteScalar()
                Dim v As Integer = 1
                If r IsNot Nothing AndAlso r IsNot DBNull.Value Then Integer.TryParse(r.ToString(), v)
                If v <= 0 Then v = 1
                Return v
            End Using
        End Using
    End Function

    Private Function GetItemNameFromMaster(itemId As Integer) As String
        Using conn As New MySqlConnection(connectionString)
            conn.Open()
            Dim sql As String = "SELECT COALESCE(item_name,'') FROM item_master WHERE item_id=@id LIMIT 1"
            Using cmd As New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@id", itemId)
                Dim r = cmd.ExecuteScalar()
                If r Is Nothing OrElse r Is DBNull.Value Then Return ""
                Return r.ToString()
            End Using
        End Using
    End Function

    Private Function SafeCellStr(row As DataGridViewRow, colName As String) As String
        Dim v = row.Cells(colName).Value
        If v Is Nothing Then Return ""
        Return v.ToString().Trim()
    End Function

    ' =========================
    ' 保存（header更新 + detail全入替）
    ' =========================


    Private Sub btnReport_Click(sender As Object, e As EventArgs)
        ' 現在画面の内容を“帳票用”に確定させる（未確定セル対策）
        If estimateDataGridView IsNot Nothing Then
            estimateDataGridView.EndEdit()
            estimateDataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit)
        End If

        BuildReportSnapshot()

        printDoc = New Printing.PrintDocument()
        AddHandler printDoc.PrintPage, AddressOf printDoc_PrintPage

        previewDlg = New PrintPreviewDialog() With {
        .Document = printDoc,
        .Width = 1100,
        .Height = 800
    }

        reportRowIndex = 0
        previewDlg.ShowDialog()
    End Sub
    Private Sub printDoc_PrintPage(sender As Object, e As Printing.PrintPageEventArgs)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias

        Dim marginL As Integer = 40
        Dim marginT As Integer = 40
        Dim marginR As Integer = 40
        Dim marginB As Integer = 60

        Dim pageW As Integer = e.MarginBounds.Width
        Dim pageH As Integer = e.MarginBounds.Height

        Dim x As Integer = e.MarginBounds.Left
        Dim y As Integer = e.MarginBounds.Top

        Using fTitle As New Font("Yu Gothic UI", 16, FontStyle.Bold)
            g.DrawString("発送明細帳票", fTitle, Brushes.Black, x, y)
        End Using
        y += 34

        Using fMeta As New Font("Yu Gothic UI", 10, FontStyle.Regular)
            Dim batchId As String = txtBatchId.Text.Trim()
            Dim shipDate As String = If(dtpShipmentDate.Checked, dtpShipmentDate.Value.ToString("yyyy/MM/dd"), "未設定")
            Dim cCode As String = txtCustomerCode.Text.Trim()
            Dim cName As String = txtCustomerName.Text.Trim()

            g.DrawString($"トランザクションID: {batchId}", fMeta, Brushes.Black, x, y) : y += 18
            g.DrawString($"発送日: {shipDate}", fMeta, Brushes.Black, x, y) : y += 18
            g.DrawString($"顧客: {cCode}  {cName}", fMeta, Brushes.Black, x, y) : y += 22
        End Using

        ' 罫線・表設定
        Dim tableTop As Integer = y + 6
        Dim rowH As Integer = 22

        ' 列幅（比率）
        Dim wItem As Integer = CInt(pageW * 0.23)
        Dim wUnit As Integer = CInt(pageW * 0.08)
        Dim wQty As Integer = CInt(pageW * 0.08)
        Dim wQty2 As Integer = CInt(pageW * 0.08)
        Dim wPrice As Integer = CInt(pageW * 0.1)
        Dim wAmt As Integer = CInt(pageW * 0.1)
        Dim wRemark As Integer = pageW - (wItem + wUnit + wQty + wQty2 + wPrice + wAmt) - 2

        Dim colX As Integer() = {
        x,
        x + wItem,
        x + wItem + wUnit,
        x + wItem + wUnit + wQty,
        x + wItem + wUnit + wQty + wQty2,
        x + wItem + wUnit + wQty + wQty2 + wPrice,
        x + wItem + wUnit + wQty + wQty2 + wPrice + wAmt
    }

        Using fHead As New Font("Yu Gothic UI", 9.5F, FontStyle.Bold)
            Using pen As New Pen(Color.Black, 1)
                ' ヘッダー背景
                g.FillRectangle(Brushes.Gainsboro, x, tableTop, pageW, rowH)

                ' ヘッダー文字
                g.DrawString("商品名", fHead, Brushes.Black, colX(0) + 4, tableTop + 3)
                g.DrawString("単位", fHead, Brushes.Black, colX(1) + 4, tableTop + 3)
                g.DrawString("数量", fHead, Brushes.Black, colX(2) + 4, tableTop + 3)
                g.DrawString("数量2", fHead, Brushes.Black, colX(3) + 4, tableTop + 3)
                g.DrawString("単価", fHead, Brushes.Black, colX(4) + 4, tableTop + 3)
                g.DrawString("金額", fHead, Brushes.Black, colX(5) + 4, tableTop + 3)
                g.DrawString("備考", fHead, Brushes.Black, colX(6) + 4, tableTop + 3)

                ' 罫線（ヘッダー）
                g.DrawRectangle(pen, x, tableTop, pageW, rowH)
            End Using
        End Using

        y = tableTop + rowH

        Using fCell As New Font("Yu Gothic UI", 9.2F, FontStyle.Regular)
            Using pen As New Pen(Color.Black, 1)
                Dim maxY As Integer = e.MarginBounds.Bottom - marginB

                While reportRowIndex < reportRows.Count
                    If y + rowH > maxY Then
                        e.HasMorePages = True
                        Return
                    End If

                    Dim rr = reportRows(reportRowIndex)

                    ' 行枠
                    g.DrawRectangle(pen, x, y, pageW, rowH)

                    ' 文字
                    g.DrawString(rr.ItemName, fCell, Brushes.Black, colX(0) + 4, y + 3)
                    g.DrawString(rr.Unit, fCell, Brushes.Black, colX(1) + 4, y + 3)
                    g.DrawString(rr.Qty, fCell, Brushes.Black, colX(2) + 4, y + 3)
                    g.DrawString(rr.Qty2, fCell, Brushes.Black, colX(3) + 4, y + 3)

                    ' 数値は右寄せ
                    DrawRight(g, rr.UnitPrice, fCell, Brushes.Black, colX(4), wPrice, y, rowH)
                    DrawRight(g, rr.Amount, fCell, Brushes.Black, colX(5), wAmt, y, rowH)

                    g.DrawString(rr.Remark, fCell, Brushes.Black, colX(6) + 4, y + 3)

                    ' 縦罫線
                    g.DrawLine(pen, colX(1), y, colX(1), y + rowH)
                    g.DrawLine(pen, colX(2), y, colX(2), y + rowH)
                    g.DrawLine(pen, colX(3), y, colX(3), y + rowH)
                    g.DrawLine(pen, colX(4), y, colX(4), y + rowH)
                    g.DrawLine(pen, colX(5), y, colX(5), y + rowH)
                    g.DrawLine(pen, colX(6), y, colX(6), y + rowH)

                    y += rowH
                    reportRowIndex += 1
                End While

                ' 合計金額
                Dim total As Decimal = 0D
                For Each rr In reportRows
                    Dim a As Decimal
                    Decimal.TryParse(rr.Amount, a)
                    total += a
                Next

                y += 10
                Using fSum As New Font("Yu Gothic UI", 11, FontStyle.Bold)
                    g.DrawString($"合計金額: {total:#,0}", fSum, Brushes.Black, x, y)
                End Using
            End Using
        End Using

        e.HasMorePages = False
    End Sub

    Private Sub DrawRight(g As Graphics, text As String, f As Font, br As Brush, x As Integer, w As Integer, y As Integer, h As Integer)
        Dim sz = g.MeasureString(text, f)
        g.DrawString(text, f, br, x + w - sz.Width - 6, y + 3)
    End Sub


    Private Sub BuildReportSnapshot()
        reportRows.Clear()

        For Each r As DataGridViewRow In estimateDataGridView.Rows
            If r.IsNewRow Then Continue For

            Dim itemName As String = SafeCellStr(r, "item_name")
            Dim unit As String = SafeCellStr(r, "unit")
            Dim qty As String = SafeCellStr(r, "quantity")
            Dim qty2 As String = SafeCellStr(r, "quantity2")
            Dim price As String = SafeCellStr(r, "unit_price")
            Dim amount As String = SafeCellStr(r, "amount")
            Dim remark As String = SafeCellStr(r, "remark")

            ' 完全空行はスキップ
            If itemName = "" AndAlso unit = "" AndAlso qty = "" AndAlso qty2 = "" AndAlso price = "" AndAlso amount = "" AndAlso remark = "" Then
                Continue For
            End If

            reportRows.Add(New ReportRow With {
            .ItemName = itemName,
            .Unit = unit,
            .Qty = qty,
            .Qty2 = qty2,
            .UnitPrice = price,
            .Amount = amount,
            .Remark = remark
        })
        Next
    End Sub




    Private Sub btnSave_Click(sender As Object, e As EventArgs)

        If ShipmentBatchId <= 0 Then
            MessageBox.Show("ShipmentBatchId が不正です（一覧から開いてください）。")
            Return
        End If

        Dim customerCode As String = txtCustomerCode.Text.Trim()
        If customerCode = "" Then
            MessageBox.Show("顧客コードを入力してください。")
            txtCustomerCode.Focus()
            Return
        End If

        ' 編集中セルを確定
        If estimateDataGridView IsNot Nothing Then
            estimateDataGridView.EndEdit()
            estimateDataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit)
        End If

        Dim details As New List(Of DetailRow)()
        Dim lineNo As Integer = 1

        ' =========================
        ' 画面→details（ロット内訳で展開）
        ' =========================
        For Each row As DataGridViewRow In estimateDataGridView.Rows
            If row.IsNewRow Then Continue For

            ' item_id確定（手入力救済）
            Dim itemIdStr As String = SafeCellStr(row, "item_id")
            Dim itemIdVal As Object = DBNull.Value

            Dim tmpItemId As Long
            If Long.TryParse(itemIdStr, tmpItemId) AndAlso tmpItemId > 0 Then
                itemIdVal = tmpItemId
            Else
                Dim itemNameStr As String = SafeCellStr(row, "item_name")
                itemIdVal = ResolveItemIdByName(itemNameStr)
                If Not (itemIdVal Is Nothing OrElse itemIdVal Is DBNull.Value) Then
                    row.Cells("item_id").Value = itemIdVal.ToString()
                End If
            End If

            Dim unitStr As String = SafeCellStr(row, "unit")
            Dim qStr As String = SafeCellStr(row, "quantity")
            Dim q2Str As String = SafeCellStr(row, "quantity2")
            Dim pStr As String = SafeCellStr(row, "unit_price")
            Dim rStr As String = SafeCellStr(row, "remark")

            ' 空行スキップ
            If (itemIdVal Is DBNull.Value) AndAlso unitStr = "" AndAlso qStr = "" AndAlso q2Str = "" AndAlso pStr = "" AndAlso rStr = "" Then
                Continue For
            End If

            ' ★ロット割当（JSON）
            Dim allocJson As String = SafeCellStr(row, "lot_alloc_json")
            Dim allocs As List(Of LotAlloc) = JsonToAllocList(allocJson)



            Dim q As Decimal = 0D
            Dim q2 As Decimal = 0D
            Dim p As Decimal = 0D

            Decimal.TryParse(q2Str, q2)
            Decimal.TryParse(qStr, q)

            ' quantity が空/0で quantity2 があるなら換算（保険）
            If (q <= 0D) AndAlso (q2 > 0D) Then
                If Not (itemIdVal Is Nothing OrElse itemIdVal Is DBNull.Value) Then
                    Dim itemId As Integer = CInt(Convert.ToInt64(itemIdVal))
                    Dim conv As Integer = GetConversionQtyFromMaster(itemId)
                    q = q2 * conv
                End If
            End If

            Decimal.TryParse(pStr, p)

            unitStr = If(unitStr, "").Replace(",", "，")
            rStr = If(rStr, "").Replace(",", "，")

            Dim totalPieces As Integer = CInt(Math.Truncate(q))
            If totalPieces <= 0 Then
                ' 0数量は保存しない（監査ノイズ・台帳ノイズを根絶）
                ' ※必要なら MessageBox で警告する運用も可
                Continue For
            End If


            ' 割当合計一致チェック
            Dim sumAlloc As Integer = 0
            If allocs IsNot Nothing Then
                sumAlloc = allocs.Sum(Function(a) a.QtyPieces)
            End If

            If sumAlloc <> totalPieces Then
                MessageBox.Show($"ロット割当の合計が一致しません。要求={totalPieces} / 割当={sumAlloc}")
                Return
            End If

            ' amount は 箱単価×箱数（先頭明細だけ）
            Dim totalAmount As Decimal = q2 * p

            ' ロット数だけ details.Add（展開）
            For i = 0 To allocs.Count - 1
                Dim a = allocs(i)

                details.Add(New DetailRow With {
                .LineNo = lineNo,
                .ItemId = itemIdVal,
                .LotId = a.LotId,
                .Unit = unitStr,
                .Quantity = a.QtyPieces,
                .Quantity2 = If(i = 0, q2, 0D),
                .UnitPrice = p,
                .Amount = If(i = 0, totalAmount, 0D),
                .Remark = rStr
            })

                lineNo += 1
            Next
        Next

        If details.Count = 0 Then
            MessageBox.Show("明細がありません。")
            Return
        End If

        ' =========================
        ' DB保存（TX）
        ' =========================
        Using conn As New MySqlConnection(connectionString)
            conn.Open()
            Using tx = conn.BeginTransaction()
                Try
                    ' 旧ロット集計（削除前に取る）
                    Dim oldLotAgg = GetOldLotAgg(ShipmentBatchId, conn, tx)

                    ' header update
                    Dim sqlH As String =
                    "UPDATE shipments_header SET shipment_date=@shipment_date, customer_code=@customer_code " &
                    "WHERE shipment_batch_id=@id"

                    Using cmdH As New MySqlCommand(sqlH, conn, tx)
                        If dtpShipmentDate.Checked Then
                            cmdH.Parameters.AddWithValue("@shipment_date", dtpShipmentDate.Value.Date)
                        Else
                            cmdH.Parameters.AddWithValue("@shipment_date", DBNull.Value)
                        End If
                        cmdH.Parameters.AddWithValue("@customer_code", customerCode)
                        cmdH.Parameters.AddWithValue("@id", ShipmentBatchId)
                        cmdH.ExecuteNonQuery()
                    End Using

                    ' ★子テーブル（shipment_unit_alloc）を先に消す（CASCADE無い環境の保険）
                    DeleteUnitAllocByBatch(ShipmentBatchId, conn, tx)

                    ' detail: 全削除 → 再INSERT
                    Using cmdDel As New MySqlCommand("DELETE FROM shipments_detail WHERE shipment_batch_id=@id", conn, tx)
                        cmdDel.Parameters.AddWithValue("@id", ShipmentBatchId)
                        cmdDel.ExecuteNonQuery()
                    End Using

                    ' INSERT（IDを取得する）
                    Dim sqlIns As String =
                    "INSERT INTO shipments_detail " &
                    "(shipment_batch_id, line_no, item_id, lot_id, unit, quantity, quantity2, unit_price, remark) " &
                    "VALUES (@id, @line_no, @item_id, @lot_id, @unit, @qty, @qty2, @price, @remark);" &
                    "SELECT LAST_INSERT_ID();"

                    Using cmdIns As New MySqlCommand(sqlIns, conn, tx)
                        cmdIns.Parameters.Add("@id", MySqlDbType.Int64)
                        cmdIns.Parameters.Add("@line_no", MySqlDbType.Int32)
                        cmdIns.Parameters.Add("@item_id", MySqlDbType.Int64)
                        cmdIns.Parameters.Add("@lot_id", MySqlDbType.Int64)
                        cmdIns.Parameters.Add("@unit", MySqlDbType.VarChar)
                        cmdIns.Parameters.Add("@qty", MySqlDbType.Decimal)
                        cmdIns.Parameters.Add("@qty2", MySqlDbType.Decimal)
                        cmdIns.Parameters.Add("@price", MySqlDbType.Decimal)
                        cmdIns.Parameters.Add("@remark", MySqlDbType.VarChar)

                        For Each d In details
                            If CDec(d.Quantity) <= 0D Then
                                Continue For
                            End If

                            cmdIns.Parameters("@id").Value = ShipmentBatchId
                            cmdIns.Parameters("@line_no").Value = d.LineNo
                            cmdIns.Parameters("@item_id").Value = If(d.ItemId Is Nothing OrElse d.ItemId Is DBNull.Value, DBNull.Value, d.ItemId)
                            cmdIns.Parameters("@lot_id").Value = If(d.LotId Is Nothing OrElse d.LotId Is DBNull.Value, DBNull.Value, d.LotId)
                            cmdIns.Parameters("@unit").Value = d.Unit
                            cmdIns.Parameters("@qty").Value = d.Quantity
                            cmdIns.Parameters("@qty2").Value = d.Quantity2
                            cmdIns.Parameters("@price").Value = d.UnitPrice
                            cmdIns.Parameters("@remark").Value = d.Remark

                            Dim newDetailId As Long = Convert.ToInt64(cmdIns.ExecuteScalar())

                            ' ★ここが追加：個体（lot_unit）を qtyPieces 分払い出して紐付ける
If Not (d.LotId Is Nothing OrElse d.LotId Is DBNull.Value) Then
    Dim lotId As Long = Convert.ToInt64(d.LotId)
    Dim pieces As Integer = CInt(Math.Truncate(Convert.ToDecimal(d.Quantity)))

    If pieces > 0 Then
        ApplyUnitAlloc_Issue(newDetailId, lotId, pieces, conn, tx)
    End If
End If

                        Next
                    End Using

                    ' --- ロット在庫（旧を戻す→新を引く） ---
                    Dim newLotAgg = BuildNewLotAgg(details)
                    ApplyLotStockForResave(oldLotAgg, newLotAgg, conn, tx)

                    VoidShipmentLedger(ShipmentBatchId, conn, tx)

' ★0数量を含めない details にしている前提だが、念のためここでも除外したリストを渡す
Dim detailsNonZero = details.Where(Function(x) Convert.ToDecimal(x.Quantity) > 0D).ToList()

                    ' --- 全体在庫台帳（既存ロジック） ---
                    ApplyInventoryLedger_ForShipmentUpdate(ShipmentBatchId, detailsNonZero, conn, tx)

                    tx.Commit()
                    MessageBox.Show("保存しました。")

                Catch ex As Exception
                    Try : tx.Rollback() : Catch : End Try
                    MessageBox.Show("保存エラー: " & ex.Message)
                    Return
                End Try
            End Using
        End Using

        LoadShipmentBatch(ShipmentBatchId)
    End Sub



    ' ★Shipと同じ：1行（1アイテム）に対するロット割当
    Private Class LotAlloc
        Public Property LotId As Long
        Public Property LotNo As String
        Public Property QtyPieces As Integer
        Public Property Available As Integer ' ←追加
    End Class


    Private Function AllocListToJson(list As List(Of LotAlloc)) As String
        If list Is Nothing OrElse list.Count = 0 Then Return ""
        Return System.Text.Json.JsonSerializer.Serialize(list)
    End Function

    Private Function JsonToAllocList(json As String) As List(Of LotAlloc)
        json = If(json, "").Trim()
        If json = "" Then Return New List(Of LotAlloc)()
        Try
            Return System.Text.Json.JsonSerializer.Deserialize(Of List(Of LotAlloc))(json)
        Catch
            Return New List(Of LotAlloc)()
        End Try
    End Function

    Private Sub SetLotInfo(row As DataGridViewRow, allocs As List(Of LotAlloc))
        If row Is Nothing OrElse row.IsNewRow Then Return

        If allocs Is Nothing OrElse allocs.Count = 0 Then
            row.Cells("lot_info").Value = "未割当"
            row.Cells("lot_alloc_json").Value = ""
            Return
        End If

        Dim lotCnt = allocs.Count
        Dim sumPieces = allocs.Sum(Function(a) a.QtyPieces)
        row.Cells("lot_info").Value = $"{lotCnt}ロット割当（{sumPieces}個）"
        row.Cells("lot_alloc_json").Value = AllocListToJson(allocs)
    End Sub




    Private Sub Dgv_CellContentClick_Stock(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Return
        If estimateDataGridView.Columns(e.ColumnIndex).Name <> "btn_stock" Then Return

        Dim row = estimateDataGridView.Rows(e.RowIndex)
        If row Is Nothing OrElse row.IsNewRow Then Return

        ' item_id 確定（手入力救済）
        Dim itemIdVal As Object = DBNull.Value
        Dim tmp As Long
        Dim itemIdStr As String = SafeCellStr(row, "item_id")

        If Long.TryParse(itemIdStr, tmp) AndAlso tmp > 0 Then
            itemIdVal = tmp
        Else
            Dim itemNameStr As String = SafeCellStr(row, "item_name")
            itemIdVal = ResolveItemIdByName(itemNameStr)
            If Not (itemIdVal Is Nothing OrElse itemIdVal Is DBNull.Value) Then
                row.Cells("item_id").Value = itemIdVal.ToString()
            End If
        End If

        If itemIdVal Is Nothing OrElse itemIdVal Is DBNull.Value Then
            MessageBox.Show("先にアイテムを確定してください。")
            Return
        End If

        Dim itemId As Integer = CInt(Convert.ToInt64(itemIdVal))
        Dim itemName As String = SafeCellStr(row, "item_name")
        If itemName = "" Then itemName = GetItemNameFromMaster(itemId)

        ' 必要個数 requiredPieces（quantity が入ってればそれ優先、なければ quantity2×conv）
        Dim requiredPieces As Integer = 0
        Dim qStr = SafeCellStr(row, "quantity")
        Dim q2Str = SafeCellStr(row, "quantity2")

        Dim q As Integer = 0
        If Integer.TryParse(qStr, q) AndAlso q > 0 Then
            requiredPieces = q
        Else
            Dim box As Integer = 0
            If Integer.TryParse(q2Str, box) AndAlso box > 0 Then
                Dim conv As Integer = GetConversionQtyFromMaster(itemId)
                Dim tmpPieces As Long = CLng(box) * CLng(conv)
                If tmpPieces > Integer.MaxValue Then
                    MessageBox.Show("数量が大きすぎます。")
                    Return
                End If
                requiredPieces = CInt(tmpPieces)
            End If
        End If

        If requiredPieces <= 0 Then
            MessageBox.Show("数量（個）または数量2（箱）を入力してください。")
            Return
        End If

        ' 既存割当（JSON）
        ' JSON（shiplistdetail側のLotAlloc）を読む
        Dim existingLocal As List(Of LotAlloc) = JsonToAllocList(SafeCellStr(row, "lot_alloc_json"))

        ' LotAllocForm が要求する型に変換する
        Dim existing As List(Of LotAllocForm.LotAlloc) =
            If(existingLocal Is Nothing, New List(Of LotAllocForm.LotAlloc)(),
               existingLocal.Select(Function(x) New LotAllocForm.LotAlloc With {
                   .LotId = x.LotId,
                   .LotNo = x.LotNo,
                   .QtyPieces = x.QtyPieces
               }).ToList())

        ' ★在庫詳細ポップアップ
        Me.Hide()
        Try
            Using dlg As New LotAllocForm(connectionString, itemId, itemName, requiredPieces, existing)
                If dlg.ShowDialog() <> DialogResult.OK Then Return
                Dim allocs As List(Of LotAlloc) =
                        dlg.ResultAllocs.Select(Function(a) New LotAlloc With {
                            .LotId = a.LotId,
                            .LotNo = a.LotNo,
                            .QtyPieces = a.QtyPieces,
                            .Available = a.Available
                        }).ToList()

                SetLotInfo(row, allocs)  ' ←これが無いと反映されない            End Using

            End Using

        Finally
            Me.Show()
            Me.Activate()
        End Try

    End Sub


    Private Function AutoAllocateLots(itemId As Long, needPieces As Integer,
                                  conn As MySqlConnection, tx As MySqlTransaction) As List(Of LotAlloc)

        Dim result As New List(Of LotAlloc)()
        If needPieces <= 0 Then Return result

        ' ロット管理か？
        Dim isLot As Boolean = False
        Using cmd As New MySqlCommand("SELECT is_lot_item FROM item_master WHERE item_id=@id", conn, tx)
            cmd.Parameters.AddWithValue("@id", itemId)
            Dim r = cmd.ExecuteScalar()
            isLot = (r IsNot Nothing AndAlso r IsNot DBNull.Value AndAlso r.ToString() = "T")
        End Using
        If Not isLot Then Return result

        ' 候補ロットをロックして取得（FIFO）
        Dim sql As String =
"SELECT lot_id, lot_no, qty_on_hand_pieces
  FROM lot
 WHERE item_id=@item_id AND is_active=1 AND qty_on_hand_pieces > 0
 ORDER BY received_date ASC, lot_id ASC
 FOR UPDATE;"

        Dim remain As Integer = needPieces

        Using cmd As New MySqlCommand(sql, conn, tx)
            cmd.Parameters.AddWithValue("@item_id", itemId)
            Using rdr = cmd.ExecuteReader()
                While rdr.Read() AndAlso remain > 0
                    Dim lotId As Long = Convert.ToInt64(rdr("lot_id"))
                    Dim lotNo As String = Convert.ToString(rdr("lot_no"))
                    Dim avail As Integer = Convert.ToInt32(rdr("qty_on_hand_pieces"))

                    Dim take As Integer = Math.Min(avail, remain)
                    If take > 0 Then
                        result.Add(New LotAlloc With {
                        .LotId = lotId,
                        .LotNo = lotNo,
                        .QtyPieces = take,
                        .Available = avail
                    })
                        remain -= take
                    End If
                End While
            End Using
        End Using

        If remain > 0 Then
            Throw New Exception($"ロット在庫不足（item_id={itemId}, 要求={needPieces}, 不足={remain}）")
        End If

        Return result
    End Function


    Private Function GetOldLotAgg(batchId As Long, conn As MySqlConnection, tx As MySqlTransaction) As Dictionary(Of Long, Integer)
        Dim agg As New Dictionary(Of Long, Integer)()

        Using cmd As New MySqlCommand(
        "SELECT lot_id, SUM(quantity) AS qty " &
        "FROM shipments_detail " &
        "WHERE shipment_batch_id=@id AND lot_id IS NOT NULL " &
        "GROUP BY lot_id", conn, tx)

            cmd.Parameters.AddWithValue("@id", batchId)
            Using rdr = cmd.ExecuteReader()
                While rdr.Read()
                    Dim lotId As Long = Convert.ToInt64(rdr("lot_id"))
                    Dim qty As Integer = Convert.ToInt32(rdr("qty"))
                    If qty > 0 Then agg(lotId) = qty
                End While
            End Using
        End Using

        Return agg
    End Function

    Private Function BuildNewLotAgg(details As List(Of DetailRow)) As Dictionary(Of Long, Integer)
        Dim agg As New Dictionary(Of Long, Integer)()
        For Each d In details
            If d.LotId Is Nothing OrElse d.LotId Is DBNull.Value Then Continue For
            Dim lotId As Long = Convert.ToInt64(d.LotId)
            Dim qty As Integer = CInt(Math.Truncate(d.Quantity)) ' 個数
            If qty <= 0 Then Continue For
            If Not agg.ContainsKey(lotId) Then agg(lotId) = 0
            agg(lotId) += qty
        Next
        Return agg
    End Function


    Private Sub ApplyLotStockForResave(oldAgg As Dictionary(Of Long, Integer),
                                  newAgg As Dictionary(Of Long, Integer),
                                  conn As MySqlConnection,
                                  tx As MySqlTransaction)

        Dim lockIds As New HashSet(Of Long)()
        For Each k In oldAgg.Keys : lockIds.Add(k) : Next
        For Each k In newAgg.Keys : lockIds.Add(k) : Next
        If lockIds.Count = 0 Then Return

        ' 1) ロット行をロック
        Dim idList = lockIds.ToList()
        Using cmdLock As New MySqlCommand("", conn, tx)
            Dim ps As New List(Of String)()
            For i = 0 To idList.Count - 1
                Dim p = "@p" & i
                ps.Add(p)
                cmdLock.Parameters.AddWithValue(p, idList(i))
            Next
            cmdLock.CommandText = "SELECT lot_id FROM lot WHERE lot_id IN (" & String.Join(",", ps) & ") FOR UPDATE"
            cmdLock.ExecuteNonQuery()
        End Using

        ' 2) 旧分を戻す（+）
        Using cmdAdd As New MySqlCommand(
        "UPDATE lot SET qty_on_hand_pieces = qty_on_hand_pieces + @delta WHERE lot_id=@lot_id", conn, tx)
            cmdAdd.Parameters.Add("@delta", MySqlDbType.Int32)
            cmdAdd.Parameters.Add("@lot_id", MySqlDbType.Int64)

            For Each kv In oldAgg
                cmdAdd.Parameters("@delta").Value = kv.Value
                cmdAdd.Parameters("@lot_id").Value = kv.Key
                cmdAdd.ExecuteNonQuery()
            Next
        End Using

        ' 3) 新分を引く（-）※不足ならエラー
        Using cmdSub As New MySqlCommand(
        "UPDATE lot SET qty_on_hand_pieces = qty_on_hand_pieces - @delta " &
        "WHERE lot_id=@lot_id AND qty_on_hand_pieces >= @delta", conn, tx)

            cmdSub.Parameters.Add("@delta", MySqlDbType.Int32)
            cmdSub.Parameters.Add("@lot_id", MySqlDbType.Int64)

            For Each kv In newAgg
                cmdSub.Parameters("@delta").Value = kv.Value
                cmdSub.Parameters("@lot_id").Value = kv.Key
                Dim aff = cmdSub.ExecuteNonQuery()
                If aff = 0 Then
                    Throw New Exception($"ロット在庫が不足しています（lot_id={kv.Key}, 要求={kv.Value}）")
                End If
            Next
        End Using
    End Sub




    Private Function ResolveItemIdByName(itemName As String) As Object
        itemName = If(itemName, "").Trim()
        If itemName = "" Then Return DBNull.Value

        Using conn As New MySqlConnection(connectionString)
            conn.Open()
            Dim sql As String = "SELECT item_id FROM item_master WHERE item_name = @name LIMIT 1"
            Using cmd As New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@name", itemName)
                Dim r = cmd.ExecuteScalar()
                If r Is Nothing OrElse r Is DBNull.Value Then Return DBNull.Value
                Return Convert.ToInt64(r)
            End Using
        End Using
    End Function


    Private Class DetailRow
        Public Property LineNo As Integer
        Public Property ItemId As Object   ' DBNull.Value or Long
        Public Property LotId As Object    ' ★追加：DBNull.Value or Long
        Public Property Unit As String
        Public Property Quantity As Decimal   ' 個数
        Public Property Quantity2 As Decimal  ' 箱数
        Public Property UnitPrice As Decimal
        Public Property Amount As Decimal
        Public Property Remark As String
    End Class



    Private Function SafeStr(v As Object) As String
        If v Is Nothing OrElse v Is DBNull.Value Then Return ""
        Return v.ToString()
    End Function

End Class
