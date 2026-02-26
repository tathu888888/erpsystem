Imports MySqlConnector
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

Public Class frmWorkOrder
    Inherits Form

    '========================================================
    ' DB設定/フィールド
    '========================================================
    Private ReadOnly _connStr As String =
        "Server=localhost;Database=sunstar;Uid=root;Pwd=****;Charset=utf8mb4;Allow User Variables=True;"

    Private _woId As Integer = 0
    Private _assemblyItemId As Integer = 0
    Private _bomId As Integer = 0
    Private _dtLines As DataTable

    '========================================================
    ' 画面コントロール（デザイナ無しなので全部ここで保持）
    '========================================================
    Private txtWoId As TextBox
    Private txtWoNo As TextBox
    Private dtpWoDate As DateTimePicker

    Private txtAssemblyItemCode As TextBox
    Private txtAssemblyItemName As TextBox
    Private btnPickAssemblyItem As Button

    Private txtBomId As TextBox
    Private txtBomDisplay As TextBox
    Private btnAutoPickBom As Button

    Private numQtyPlanned As NumericUpDown
    Private cmbStatus As ComboBox
    Private txtNotes As TextBox

    Private dgvLines As DataGridView

    Private btnSaveHeader As Button
    Private btnExplodeBom As Button
    Private btnReloadLines As Button

    '========================================================
    ' コンストラクタ
    '========================================================
    Public Sub New(Optional woId As Integer = 0)
        _woId = woId
        InitializeComponent()
    End Sub

    '========================================================
    ' 画面生成（デザイナ無し：全部ここで作る）
    '========================================================
    Private Sub InitializeComponent()
        Me.Text = "Work Order"
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.Size = New Size(1180, 760)
        Me.MinimumSize = New Size(1000, 650)

        Dim root As New TableLayoutPanel() With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 3,
            .Padding = New Padding(12)
        }
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 180))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 64))
        Me.Controls.Add(root)

        '---------------------------
        ' 上部：ヘッダ
        '---------------------------
        Dim header As New TableLayoutPanel() With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 8,
            .RowCount = 6
        }
        For i As Integer = 0 To 7
            header.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, If(i Mod 2 = 0, 10, 15)))
        Next
        For r As Integer = 0 To 5
            header.RowStyles.Add(New RowStyle(SizeType.Absolute, 28))
        Next
        root.Controls.Add(header, 0, 0)

        ' Labels helper
        Dim fLbl As New Font("Yu Gothic UI", 9.0!, FontStyle.Regular)

        ' wo_id / wo_no / wo_date
        header.Controls.Add(MkLabel("WO ID", fLbl), 0, 0)
        txtWoId = MkText(readOnly:=True)
        header.Controls.Add(txtWoId, 1, 0)

        header.Controls.Add(MkLabel("WO No", fLbl), 2, 0)
        txtWoNo = MkText(readOnly:=True)
        header.Controls.Add(txtWoNo, 3, 0)

        header.Controls.Add(MkLabel("WO Date", fLbl), 4, 0)
        dtpWoDate = New DateTimePicker() With {.Dock = DockStyle.Fill, .Format = DateTimePickerFormat.Short}
        header.Controls.Add(dtpWoDate, 5, 0)

        header.Controls.Add(MkLabel("Status", fLbl), 6, 0)
        cmbStatus = New ComboBox() With {.Dock = DockStyle.Fill, .DropDownStyle = ComboBoxStyle.DropDownList}
        header.Controls.Add(cmbStatus, 7, 0)

        ' assembly item
        header.Controls.Add(MkLabel("Assembly Code", fLbl), 0, 1)
        txtAssemblyItemCode = MkText(readOnly:=True)
        header.Controls.Add(txtAssemblyItemCode, 1, 1)

        header.Controls.Add(MkLabel("Assembly Name", fLbl), 2, 1)
        txtAssemblyItemName = MkText(readOnly:=True)
        header.SetColumnSpan(txtAssemblyItemName, 3)
        header.Controls.Add(txtAssemblyItemName, 3, 1)

        btnPickAssemblyItem = New Button() With {.Text = "完成品選択", .Dock = DockStyle.Fill}
        AddHandler btnPickAssemblyItem.Click, AddressOf btnPickAssemblyItem_Click
        header.Controls.Add(btnPickAssemblyItem, 6, 1)
        header.SetColumnSpan(btnPickAssemblyItem, 2)

        ' BOM
        header.Controls.Add(MkLabel("BOM ID", fLbl), 0, 2)
        txtBomId = MkText(readOnly:=True)
        header.Controls.Add(txtBomId, 1, 2)

        header.Controls.Add(MkLabel("BOM", fLbl), 2, 2)
        txtBomDisplay = MkText(readOnly:=True)
        header.SetColumnSpan(txtBomDisplay, 3)
        header.Controls.Add(txtBomDisplay, 3, 2)

        btnAutoPickBom = New Button() With {.Text = "BOM自動選択", .Dock = DockStyle.Fill}
        AddHandler btnAutoPickBom.Click, AddressOf btnAutoPickBom_Click
        header.Controls.Add(btnAutoPickBom, 6, 2)
        header.SetColumnSpan(btnAutoPickBom, 2)

        ' Qty planned
        header.Controls.Add(MkLabel("Qty Planned", fLbl), 0, 3)
        numQtyPlanned = New NumericUpDown() With {
            .Dock = DockStyle.Fill,
            .DecimalPlaces = 4,
            .Minimum = 0D,
            .Maximum = 999999999D,
            .Increment = 1D,
            .Value = 1D
        }
        header.Controls.Add(numQtyPlanned, 1, 3)

        ' Notes
        header.Controls.Add(MkLabel("Notes", fLbl), 0, 4)
        txtNotes = New TextBox() With {.Dock = DockStyle.Fill, .Multiline = True, .ScrollBars = ScrollBars.Vertical}
        header.SetColumnSpan(txtNotes, 7)
        header.Controls.Add(txtNotes, 1, 4)
        header.SetRowSpan(txtNotes, 2) ' 2行分使う

        '---------------------------
        ' 中央：明細グリッド
        '---------------------------
        dgvLines = New DataGridView() With {
            .Dock = DockStyle.Fill,
            .AutoGenerateColumns = False,
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .ReadOnly = True,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            .MultiSelect = False
        }
        root.Controls.Add(dgvLines, 0, 1)

        InitStatus()
        InitLinesGridColumns()

        '---------------------------
        ' 下部：ボタン
        '---------------------------
        Dim footer As New FlowLayoutPanel() With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.LeftToRight,
            .Padding = New Padding(0),
            .WrapContents = False
        }
        root.Controls.Add(footer, 0, 2)

        btnSaveHeader = New Button() With {.Text = "ヘッダ保存", .Width = 140, .Height = 40}
        AddHandler btnSaveHeader.Click, AddressOf btnSaveHeader_Click
        footer.Controls.Add(btnSaveHeader)

        btnExplodeBom = New Button() With {.Text = "BOM展開", .Width = 140, .Height = 40}
        AddHandler btnExplodeBom.Click, AddressOf btnExplodeBom_Click
        footer.Controls.Add(btnExplodeBom)

        btnReloadLines = New Button() With {.Text = "明細再読込", .Width = 140, .Height = 40}
        AddHandler btnReloadLines.Click, AddressOf btnReloadLines_Click
        footer.Controls.Add(btnReloadLines)

        Dim spacer As New Panel() With {.Width = 24, .Height = 40}
        footer.Controls.Add(spacer)

        Dim btnClose As New Button() With {.Text = "閉じる", .Width = 120, .Height = 40}
        AddHandler btnClose.Click, Sub() Me.Close()
        footer.Controls.Add(btnClose)

        ' Loadイベント（デザイナ無しなので自前で登録）
        AddHandler Me.Load, AddressOf frmWorkOrder_Load
    End Sub

    Private Function MkLabel(text As String, f As Font) As Label
        Return New Label() With {
            .Text = text,
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Font = f
        }
    End Function

    Private Function MkText(Optional ReadOnly As Boolean = False) As TextBox
        Return New TextBox() With {
            .Dock = DockStyle.Fill,
            .ReadOnly = readOnly
        }
    End Function

    '========================================================
    ' Load
    '========================================================
    Private Sub frmWorkOrder_Load(sender As Object, e As EventArgs)
        Try
            If _woId > 0 Then
                LoadHeader(_woId)
                LoadLines(_woId)
            Else
                InitNew()
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message, "エラー(frmWorkOrder_Load)", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    '========================================================
    ' 初期化系
    '========================================================
    Private Sub InitStatus()
        cmbStatus.Items.Clear()
        cmbStatus.Items.Add("PLANNED")
        cmbStatus.Items.Add("RELEASED")
        cmbStatus.Items.Add("IN_PROGRESS")
        cmbStatus.Items.Add("COMPLETED")
        cmbStatus.Items.Add("CLOSED")
        cmbStatus.Items.Add("CANCELLED")
        cmbStatus.SelectedItem = "PLANNED"
    End Sub

    Private Sub InitLinesGridColumns()
        dgvLines.Columns.Clear()
        dgvLines.Columns.Add(MakeTextCol("wo_line_id", "wo_line_id", 80, True))
        dgvLines.Columns.Add(MakeNumCol("sort_no", "順", 50))
        dgvLines.Columns.Add(MakeTextCol("component_item_code", "部材コード", 120))
        dgvLines.Columns.Add(MakeTextCol("component_item_name", "部材名", 260))
        dgvLines.Columns.Add(MakeNumCol("qty_per", "員数", 90))
        dgvLines.Columns.Add(MakeNumCol("scrap_rate", "スクラップ率", 95))
        dgvLines.Columns.Add(MakeNumCol("qty_required", "必要数", 90))
        dgvLines.Columns.Add(MakeTextCol("issue_method", "払出方法", 90))
        dgvLines.Columns.Add(MakeNumCol("qty_issued", "払出済", 90))
        dgvLines.Columns.Add(MakeTextCol("is_active", "有効", 60))
    End Sub

    Private Function MakeTextCol(dataProp As String, header As String, width As Integer, Optional hidden As Boolean = False) As DataGridViewTextBoxColumn
        Dim c As New DataGridViewTextBoxColumn()
        c.DataPropertyName = dataProp
        c.HeaderText = header
        c.Width = width
        c.Visible = Not hidden
        c.SortMode = DataGridViewColumnSortMode.NotSortable
        Return c
    End Function

    Private Function MakeNumCol(dataProp As String, header As String, width As Integer) As DataGridViewTextBoxColumn
        Dim c = MakeTextCol(dataProp, header, width, False)
        c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
        c.DefaultCellStyle.Format = "N4"
        Return c
    End Function

    Private Sub InitNew()
        txtWoId.Text = ""
        txtWoNo.Text = ""
        dtpWoDate.Value = Date.Today

        _assemblyItemId = 0
        txtAssemblyItemCode.Text = ""
        txtAssemblyItemName.Text = ""

        _bomId = 0
        txtBomId.Text = ""
        txtBomDisplay.Text = ""

        numQtyPlanned.Value = 1D
        cmbStatus.SelectedItem = "PLANNED"
        txtNotes.Text = ""

        _dtLines = Nothing
        dgvLines.DataSource = Nothing
    End Sub

    '========================================================
    ' ヘッダ読み込み
    '========================================================
    Private Sub LoadHeader(woId As Integer)
        Using conn As New MySqlConnection(_connStr)
            conn.Open()
            Using cmd As New MySqlCommand("
                SELECT
                  wo_id, wo_no, wo_date, assembly_item_id, bom_id,
                  qty_planned, qty_completed, status, notes
                FROM work_order_header
                WHERE wo_id = @wo_id
            ", conn)
                cmd.Parameters.AddWithValue("@wo_id", woId)

                Using r = cmd.ExecuteReader()
                    If Not r.Read() Then Return

                    _woId = Convert.ToInt32(r("wo_id"))
                    txtWoId.Text = _woId.ToString()
                    txtWoNo.Text = r("wo_no").ToString()
                    dtpWoDate.Value = Convert.ToDateTime(r("wo_date"))

                    _assemblyItemId = Convert.ToInt32(r("assembly_item_id"))
                    _bomId = If(IsDBNull(r("bom_id")), 0, Convert.ToInt32(r("bom_id")))

                    numQtyPlanned.Value = Convert.ToDecimal(r("qty_planned"))
                    cmbStatus.SelectedItem = r("status").ToString()
                    txtNotes.Text = If(IsDBNull(r("notes")), "", r("notes").ToString())
                End Using
            End Using
        End Using

        LoadAssemblyItemInfo(_assemblyItemId)

        If _bomId > 0 Then
            LoadBomDisplay(_bomId)
        Else
            txtBomId.Text = ""
            txtBomDisplay.Text = ""
        End If
    End Sub

    Private Sub LoadAssemblyItemInfo(itemId As Integer)
        If itemId <= 0 Then Return

        Using conn As New MySqlConnection(_connStr)
            conn.Open()
            Using cmd As New MySqlCommand("
                SELECT item_code, item_name
                FROM item_master
                WHERE item_id = @id
            ", conn)
                cmd.Parameters.AddWithValue("@id", itemId)
                Using r = cmd.ExecuteReader()
                    If r.Read() Then
                        txtAssemblyItemCode.Text = r("item_code").ToString()
                        txtAssemblyItemName.Text = r("item_name").ToString()
                    End If
                End Using
            End Using
        End Using
    End Sub

    Private Sub LoadBomDisplay(bomId As Integer)
        Using conn As New MySqlConnection(_connStr)
            conn.Open()
            Using cmd As New MySqlCommand("
                SELECT bom_id, bom_code, revision
                FROM bom_header
                WHERE bom_id = @bom_id
            ", conn)
                cmd.Parameters.AddWithValue("@bom_id", bomId)
                Using r = cmd.ExecuteReader()
                    If r.Read() Then
                        txtBomId.Text = r("bom_id").ToString()
                        Dim code As String = If(IsDBNull(r("bom_code")), "", r("bom_code").ToString())
                        Dim rev As String = r("revision").ToString()
                        txtBomDisplay.Text = $"{code} rev:{rev}"
                    End If
                End Using
            End Using
        End Using
    End Sub

    '========================================================
    ' 明細読み込み
    '========================================================
    Private Sub LoadLines(woId As Integer)
        Using conn As New MySqlConnection(_connStr)
            conn.Open()
            Using cmd As New MySqlCommand("
                SELECT
                  wol.wo_line_id, wol.wo_id, wol.bom_line_id, wol.component_item_id,
                  im.item_code AS component_item_code,
                  im.item_name AS component_item_name,
                  wol.qty_per, wol.scrap_rate, wol.qty_required,
                  wol.issue_method, wol.qty_issued, wol.sort_no, wol.is_active
                FROM work_order_line wol
                JOIN item_master im ON im.item_id = wol.component_item_id
                WHERE wol.wo_id = @wo_id
                ORDER BY wol.sort_no, wol.wo_line_id
            ", conn)
                cmd.Parameters.AddWithValue("@wo_id", woId)

                Using da As New MySqlDataAdapter(cmd)
                    _dtLines = New DataTable()
                    da.Fill(_dtLines)
                    dgvLines.DataSource = _dtLines
                End Using
            End Using
        End Using
    End Sub

    Private Sub btnReloadLines_Click(sender As Object, e As EventArgs)
        Try
            If _woId <= 0 Then Return
            LoadLines(_woId)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "エラー(明細再読込)", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    '========================================================
    ' 完成品選択（簡易：InputBox）
    '========================================================
    Private Sub btnPickAssemblyItem_Click(sender As Object, e As EventArgs)
        Try
            Dim code As String = InputBox("完成品の item_code を入力してください。", "完成品選択")
            If String.IsNullOrWhiteSpace(code) Then Return

            Dim id As Integer = FindItemIdByCode(code.Trim())
            If id <= 0 Then
                MessageBox.Show("品目が見つかりません。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            _assemblyItemId = id
            LoadAssemblyItemInfo(_assemblyItemId)

            ' 完成品変更でBOM/明細クリア
            _bomId = 0
            txtBomId.Text = ""
            txtBomDisplay.Text = ""
            dgvLines.DataSource = Nothing
            _dtLines = Nothing
        Catch ex As Exception
            MessageBox.Show(ex.Message, "エラー(完成品選択)", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function FindItemIdByCode(itemCode As String) As Integer
        Using conn As New MySqlConnection(_connStr)
            conn.Open()
            Using cmd As New MySqlCommand("
                SELECT item_id
                FROM item_master
                WHERE item_code = @code
                LIMIT 1
            ", conn)
                cmd.Parameters.AddWithValue("@code", itemCode)
                Dim obj = cmd.ExecuteScalar()
                If obj Is Nothing OrElse obj Is DBNull.Value Then Return 0
                Return Convert.ToInt32(obj)
            End Using
        End Using
    End Function

    '========================================================
    ' 有効BOM自動選択
    '========================================================
    Private Sub btnAutoPickBom_Click(sender As Object, e As EventArgs)
        Try
            If _assemblyItemId <= 0 Then
                MessageBox.Show("先に完成品を選択してください。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Dim bomId As Integer = GetEffectiveBomId(_assemblyItemId)
            If bomId <= 0 Then
                MessageBox.Show("有効なBOMが見つかりません。BOMを作成してください。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            _bomId = bomId
            LoadBomDisplay(_bomId)

            ' bomが変わったら明細はクリア（再展開前提）
            dgvLines.DataSource = Nothing
            _dtLines = Nothing
        Catch ex As Exception
            MessageBox.Show(ex.Message, "エラー(BOM自動選択)", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function GetEffectiveBomId(assemblyItemId As Integer) As Integer
        Using conn As New MySqlConnection(_connStr)
            conn.Open()
            Using cmd As New MySqlCommand("
                SELECT bom_id
                FROM bom_header
                WHERE assembly_item_id = @assembly_item_id
                  AND is_active = 1
                  AND (effective_from IS NULL OR effective_from <= CURDATE())
                  AND (effective_to   IS NULL OR effective_to   >= CURDATE())
                ORDER BY updated_at DESC, revision DESC, bom_id DESC
                LIMIT 1
            ", conn)
                cmd.Parameters.AddWithValue("@assembly_item_id", assemblyItemId)
                Dim obj = cmd.ExecuteScalar()
                If obj Is Nothing OrElse obj Is DBNull.Value Then Return 0
                Return Convert.ToInt32(obj)
            End Using
        End Using
    End Function

    '========================================================
    ' ヘッダ保存（INSERT/UPDATE）
    '========================================================
    Private Sub btnSaveHeader_Click(sender As Object, e As EventArgs)
        Try
            If _assemblyItemId <= 0 Then
                MessageBox.Show("完成品を選択してください。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            If numQtyPlanned.Value <= 0D Then
                MessageBox.Show("予定数量は 0 より大きくしてください。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim woDate As Date = dtpWoDate.Value.Date
            Dim status As String = If(cmbStatus.SelectedItem, "PLANNED").ToString()
            Dim qtyPlanned As Decimal = Convert.ToDecimal(numQtyPlanned.Value)
            Dim notes As String = If(String.IsNullOrWhiteSpace(txtNotes.Text), Nothing, txtNotes.Text.Trim())

            Using conn As New MySqlConnection(_connStr)
                conn.Open()
                Using tx = conn.BeginTransaction()

                    If _woId <= 0 Then
                        Dim woNo As String = GenerateWoNo(conn, tx, woDate)

                        Using cmd As New MySqlCommand("
                            INSERT INTO work_order_header
                              (wo_no, wo_date, assembly_item_id, bom_id, qty_planned, qty_completed, status, notes, created_by)
                            VALUES
                              (@wo_no, @wo_date, @assembly_item_id, @bom_id, @qty_planned, 0.0000, @status, @notes, @created_by);
                            SELECT LAST_INSERT_ID();
                        ", conn, tx)

                            cmd.Parameters.AddWithValue("@wo_no", woNo)
                            cmd.Parameters.AddWithValue("@wo_date", woDate)
                            cmd.Parameters.AddWithValue("@assembly_item_id", _assemblyItemId)
                            cmd.Parameters.AddWithValue("@bom_id", If(_bomId > 0, CType(_bomId, Object), DBNull.Value))
                            cmd.Parameters.AddWithValue("@qty_planned", qtyPlanned)
                            cmd.Parameters.AddWithValue("@status", status)
                            cmd.Parameters.AddWithValue("@notes", If(notes Is Nothing, CType(DBNull.Value, Object), notes))
                            cmd.Parameters.AddWithValue("@created_by", DBNull.Value)

                            Dim newIdObj = cmd.ExecuteScalar()
                            _woId = Convert.ToInt32(newIdObj)

                            txtWoId.Text = _woId.ToString()
                            txtWoNo.Text = woNo
                        End Using
                    Else
                        Using cmd As New MySqlCommand("
                            UPDATE work_order_header SET
                              wo_date = @wo_date,
                              assembly_item_id = @assembly_item_id,
                              bom_id = @bom_id,
                              qty_planned = @qty_planned,
                              status = @status,
                              notes = @notes
                            WHERE wo_id = @wo_id
                        ", conn, tx)

                            cmd.Parameters.AddWithValue("@wo_id", _woId)
                            cmd.Parameters.AddWithValue("@wo_date", woDate)
                            cmd.Parameters.AddWithValue("@assembly_item_id", _assemblyItemId)
                            cmd.Parameters.AddWithValue("@bom_id", If(_bomId > 0, CType(_bomId, Object), DBNull.Value))
                            cmd.Parameters.AddWithValue("@qty_planned", qtyPlanned)
                            cmd.Parameters.AddWithValue("@status", status)
                            cmd.Parameters.AddWithValue("@notes", If(notes Is Nothing, CType(DBNull.Value, Object), notes))

                            cmd.ExecuteNonQuery()
                        End Using
                    End If

                    tx.Commit()
                End Using
            End Using

            MessageBox.Show("保存しました。", "完了", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "エラー(ヘッダ保存)", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function GenerateWoNo(conn As MySqlConnection, tx As MySqlTransaction, woDate As Date) As String
        Dim prefix As String = "WO" & woDate.ToString("yyyyMMdd") & "-"

        Using cmd As New MySqlCommand("
            SELECT IFNULL(MAX(wo_no), '')
            FROM work_order_header
            WHERE wo_no LIKE CONCAT(@prefix, '%')
        ", conn, tx)

            cmd.Parameters.AddWithValue("@prefix", prefix)
            Dim maxNo As String = Convert.ToString(cmd.ExecuteScalar())

            Dim nextSeq As Integer = 1
            If Not String.IsNullOrWhiteSpace(maxNo) AndAlso maxNo.StartsWith(prefix) Then
                Dim tail As String = maxNo.Substring(prefix.Length)
                Dim n As Integer
                If Integer.TryParse(tail, n) Then nextSeq = n + 1
            End If

            Return prefix & nextSeq.ToString("0000")
        End Using
    End Function

    '========================================================
    ' BOM展開（DELETE→INSERT）
    '========================================================
    Private Sub btnExplodeBom_Click(sender As Object, e As EventArgs)
        Try
            If _woId <= 0 Then
                MessageBox.Show("先にヘッダを保存してください。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            If _bomId <= 0 Then
                MessageBox.Show("BOMを選択してください（自動選択ボタン推奨）。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim qtyPlanned As Decimal = Convert.ToDecimal(numQtyPlanned.Value)

            Using conn As New MySqlConnection(_connStr)
                conn.Open()
                Using tx = conn.BeginTransaction()

                    Using cmdDel As New MySqlCommand(
                        "DELETE FROM work_order_line WHERE wo_id = @wo_id", conn, tx)
                        cmdDel.Parameters.AddWithValue("@wo_id", _woId)
                        cmdDel.ExecuteNonQuery()
                    End Using

                    Using cmdIns As New MySqlCommand("
                        INSERT INTO work_order_line
                          (wo_id, bom_line_id, component_item_id, qty_per, scrap_rate, qty_required, issue_method, qty_issued, sort_no, is_active)
                        SELECT
                          @wo_id,
                          bl.bom_line_id,
                          bl.component_item_id,
                          bl.qty_per,
                          bl.scrap_rate,
                          ROUND(@qty_planned * bl.qty_per * (1 + bl.scrap_rate), 6) AS qty_required,
                          bl.issue_method,
                          0.0000 AS qty_issued,
                          bl.sort_no,
                          bl.is_active
                        FROM bom_line bl
                        WHERE bl.bom_id = @bom_id
                          AND bl.is_active = 1
                        ORDER BY bl.sort_no, bl.bom_line_id
                    ", conn, tx)

                        cmdIns.Parameters.AddWithValue("@wo_id", _woId)
                        cmdIns.Parameters.AddWithValue("@bom_id", _bomId)
                        cmdIns.Parameters.AddWithValue("@qty_planned", qtyPlanned)
                        cmdIns.ExecuteNonQuery()
                    End Using

                    tx.Commit()
                End Using
            End Using

            LoadLines(_woId)
            MessageBox.Show("BOM展開しました。", "完了", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "エラー(BOM展開)", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class