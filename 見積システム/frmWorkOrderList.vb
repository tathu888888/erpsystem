Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Drawing
Imports System.Windows.Forms

Public Class frmWorkOrderList

    ' UI
    Private dgv As DataGridView
    Private cboStatus As ComboBox
    Private dtFrom As DateTimePicker
    Private dtTo As DateTimePicker
    Private txtKeyword As TextBox
    Private btnSearch As Button
    Private btnClear As Button
    Private btnNew As Button
    Private btnRefresh As Button
    Private lblCount As Label

    ' ====== フォーム起動 ======
    Private Sub frmWorkOrderList_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Work Order List"
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.Width = 1200
        Me.Height = 750

        BuildUi()
        InitStatus()
        InitDates()

        LoadList()
    End Sub

    ' ====== UI構築 ======
    Private Sub BuildUi()
        Dim panelTop As New Panel With {.Dock = DockStyle.Top, .Height = 80}
        Dim panelBottom As New Panel With {.Dock = DockStyle.Bottom, .Height = 36}
        dgv = New DataGridView With {.Dock = DockStyle.Fill}

        Me.Controls.Add(dgv)
        Me.Controls.Add(panelBottom)
        Me.Controls.Add(panelTop)

        ' --- Top controls ---
        Dim lblStatus As New Label With {.Text = "Status:", .AutoSize = True, .Left = 12, .Top = 14}
        cboStatus = New ComboBox With {.Left = 70, .Top = 10, .Width = 160, .DropDownStyle = ComboBoxStyle.DropDownList}

        Dim lblFrom As New Label With {.Text = "From:", .AutoSize = True, .Left = 250, .Top = 14}
        dtFrom = New DateTimePicker With {.Left = 300, .Top = 10, .Width = 130, .Format = DateTimePickerFormat.Custom, .CustomFormat = "yyyy-MM-dd"}

        Dim lblTo As New Label With {.Text = "To:", .AutoSize = True, .Left = 450, .Top = 14}
        dtTo = New DateTimePicker With {.Left = 485, .Top = 10, .Width = 130, .Format = DateTimePickerFormat.Custom, .CustomFormat = "yyyy-MM-dd"}

        Dim lblKey As New Label With {.Text = "Keyword:", .AutoSize = True, .Left = 635, .Top = 14}
        txtKeyword = New TextBox With {.Left = 705, .Top = 10, .Width = 220}

        btnSearch = New Button With {.Text = "Search", .Left = 940, .Top = 8, .Width = 90}
        btnClear = New Button With {.Text = "Clear", .Left = 1040, .Top = 8, .Width = 90}

        btnNew = New Button With {.Text = "New", .Left = 940, .Top = 42, .Width = 90}
        btnRefresh = New Button With {.Text = "Refresh", .Left = 1040, .Top = 42, .Width = 90}

        AddHandler btnSearch.Click, AddressOf btnSearch_Click
        AddHandler btnClear.Click, AddressOf btnClear_Click
        AddHandler btnNew.Click, AddressOf btnNew_Click
        AddHandler btnRefresh.Click, AddressOf btnRefresh_Click
        AddHandler cboStatus.SelectedIndexChanged, AddressOf AnyFilter_Changed
        AddHandler dtFrom.ValueChanged, AddressOf AnyFilter_Changed
        AddHandler dtTo.ValueChanged, AddressOf AnyFilter_Changed
        AddHandler txtKeyword.KeyDown, AddressOf txtKeyword_KeyDown

        panelTop.Controls.AddRange(New Control() {
            lblStatus, cboStatus, lblFrom, dtFrom, lblTo, dtTo, lblKey, txtKeyword,
            btnSearch, btnClear, btnNew, btnRefresh
        })

        ' --- Bottom controls ---
        lblCount = New Label With {.AutoSize = True, .Left = 12, .Top = 10, .Text = "0 rows"}
        panelBottom.Controls.Add(lblCount)

        ' --- Grid setup ---
        SetupGrid()
    End Sub

    Private Sub SetupGrid()
        dgv.AllowUserToAddRows = False
        dgv.AllowUserToDeleteRows = False
        dgv.ReadOnly = True
        dgv.MultiSelect = False
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgv.AutoGenerateColumns = False
        dgv.RowHeadersVisible = False
        dgv.EnableHeadersVisualStyles = False
        dgv.ColumnHeadersHeight = 34
        dgv.Font = New Font("Yu Gothic UI", 10)

        dgv.Columns.Clear()

        dgv.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "work_order_id", .DataPropertyName = "work_order_id", .Visible = False})
        dgv.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "work_order_no", .HeaderText = "WO No", .DataPropertyName = "work_order_no", .Width = 140})
        dgv.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "item_name", .HeaderText = "Assembly Item", .DataPropertyName = "item_name", .Width = 320})
        dgv.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "qty_to_produce", .HeaderText = "Qty", .DataPropertyName = "qty_to_produce", .Width = 90, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight}})
        dgv.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "status", .HeaderText = "Status", .DataPropertyName = "status", .Width = 140})
        dgv.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "start_date", .HeaderText = "Start", .DataPropertyName = "start_date", .Width = 110})
        dgv.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "due_date", .HeaderText = "Due", .DataPropertyName = "due_date", .Width = 110})
        dgv.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "posted", .HeaderText = "Posted", .DataPropertyName = "posted", .Width = 80})
        dgv.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "created_at", .HeaderText = "Created", .DataPropertyName = "created_at", .Width = 160})

        AddHandler dgv.CellDoubleClick, AddressOf dgv_CellDoubleClick
        AddHandler dgv.CellFormatting, AddressOf dgv_CellFormatting
    End Sub

    ' ====== フィルター初期化 ======
    Private Sub InitStatus()
        cboStatus.Items.Clear()
        cboStatus.Items.Add("(All)")
        cboStatus.Items.Add("PLANNED")
        cboStatus.Items.Add("RELEASED")
        cboStatus.Items.Add("IN_PROGRESS")
        cboStatus.Items.Add("COMPLETED")
        cboStatus.Items.Add("CLOSED")
        cboStatus.SelectedIndex = 0
    End Sub

    Private Sub InitDates()
        ' 直近30日を初期表示
        dtTo.Value = Date.Today
        dtFrom.Value = Date.Today.AddDays(-30)
    End Sub

    ' ====== 検索実行 ======
    Private Sub LoadList()
        Dim sql As String =
"SELECT
    w.work_order_id,
    w.work_order_no,
    i.item_name,
    w.qty_to_produce,
    w.status,
    DATE_FORMAT(w.start_date, '%Y-%m-%d') AS start_date,
    DATE_FORMAT(w.due_date, '%Y-%m-%d') AS due_date,
    CASE WHEN IFNULL(w.posted, 0) = 1 THEN 'YES' ELSE 'NO' END AS posted,
    DATE_FORMAT(w.created_at, '%Y-%m-%d %H:%i') AS created_at
FROM work_order_header w
JOIN item_master i ON i.item_id = w.assembly_item_id
WHERE 1=1
  AND (@p_from IS NULL OR w.start_date >= @p_from)
  AND (@p_to   IS NULL OR w.start_date <= @p_to)
  AND (@p_status IS NULL OR w.status = @p_status)
  AND (
       @p_kw IS NULL
       OR w.work_order_no LIKE CONCAT('%', @p_kw, '%')
       OR i.item_name     LIKE CONCAT('%', @p_kw, '%')
      )
ORDER BY w.work_order_id DESC;"

        Dim p As New List(Of MySqlParameter)

        ' date filters（NULL許容にしているけど、基本は入れる）
        p.Add(New MySqlParameter("@p_from", MySqlDbType.Date) With {.Value = dtFrom.Value.Date})
        p.Add(New MySqlParameter("@p_to", MySqlDbType.Date) With {.Value = dtTo.Value.Date})

        ' status
        Dim status As String = If(cboStatus.SelectedIndex <= 0, Nothing, CStr(cboStatus.SelectedItem))
        If String.IsNullOrWhiteSpace(status) Then
            p.Add(New MySqlParameter("@p_status", MySqlDbType.VarChar) With {.Value = DBNull.Value})
        Else
            p.Add(New MySqlParameter("@p_status", MySqlDbType.VarChar) With {.Value = status})
        End If

        ' keyword
        Dim kw As String = txtKeyword.Text.Trim()
        If String.IsNullOrWhiteSpace(kw) Then
            p.Add(New MySqlParameter("@p_kw", MySqlDbType.VarChar) With {.Value = DBNull.Value})
        Else
            p.Add(New MySqlParameter("@p_kw", MySqlDbType.VarChar) With {.Value = kw})
        End If

        ' NOTE: MySQLで @p_from/@p_to をNULL扱いしたいなら、ここでDBNullもOK
        ' 今回は常に日付を入れているので、@p_from/@p_to はNULLにならない想定

        Dim dt As DataTable = Db.GetDataTable(sql, p)
        dgv.DataSource = dt
        lblCount.Text = $"{dt.Rows.Count} rows"
    End Sub

    ' ====== 色付け（Sunstarっぽく視認性UP） ======
    Private Sub dgv_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs)
        If dgv.Columns(e.ColumnIndex).Name <> "status" Then Return
        If e.Value Is Nothing Then Return

        Dim s As String = e.Value.ToString()

        Dim row = dgv.Rows(e.RowIndex)
        row.DefaultCellStyle.BackColor = Color.White

        Select Case s
            Case "PLANNED"
                row.DefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245)
            Case "RELEASED"
                row.DefaultCellStyle.BackColor = Color.FromArgb(235, 248, 255)
            Case "IN_PROGRESS"
                row.DefaultCellStyle.BackColor = Color.FromArgb(255, 250, 230)
            Case "COMPLETED"
                row.DefaultCellStyle.BackColor = Color.FromArgb(235, 255, 235)
            Case "CLOSED"
                row.DefaultCellStyle.BackColor = Color.FromArgb(240, 240, 255)
        End Select
    End Sub

    ' ====== イベント ======
    Private Sub btnSearch_Click(sender As Object, e As EventArgs)
        LoadList()
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs)
        LoadList()
    End Sub

    Private Sub btnClear_Click(sender As Object, e As EventArgs)
        cboStatus.SelectedIndex = 0
        txtKeyword.Text = ""
        InitDates()
        LoadList()
    End Sub

    Private Sub btnNew_Click(sender As Object, e As EventArgs)
        ' ★ここはあなたの新規画面に接続
        ' 例：Dim f As New frmWorkOrderEntry() : f.ShowDialog() : LoadList()

        MessageBox.Show("New WorkOrder: ここに frmWorkOrderEntry を接続してください。", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub AnyFilter_Changed(sender As Object, e As EventArgs)
        ' 変更するたびに即検索したいならON（重いならコメントアウト）
        'LoadList()
    End Sub

    Private Sub txtKeyword_KeyDown(sender As Object, e As KeyEventArgs)
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            LoadList()
        End If
    End Sub

    Private Sub dgv_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex < 0 Then Return
        Dim idObj = dgv.Rows(e.RowIndex).Cells("work_order_id").Value
        If idObj Is Nothing OrElse idObj Is DBNull.Value Then Return

        Dim workOrderId As Long = CLng(idObj)

        ' ★ここはあなたの編集画面に接続
        ' 例：Dim f As New frmWorkOrderEntry(workOrderId) : f.ShowDialog() : LoadList()

        MessageBox.Show($"Open WorkOrder ID={workOrderId} : ここに frmWorkOrderEntry を接続してください。", "Open", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

End Class