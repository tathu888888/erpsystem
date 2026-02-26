Option Strict On
Option Infer On

Imports System.Data
Imports System.Drawing
Imports System.Threading
Imports System.Threading.Tasks
Imports MySql.Data.MySqlClient

Public Class FrmAssemblyOrderList

    '    ' ====== Controls ======
    '    Private pnlTop As Panel
    '    Private dtFrom As DateTimePicker
    '    Private dtTo As DateTimePicker
    '    Private cboStatus As ComboBox
    '    Private cboItem As ComboBox
    '    Private cboUser As ComboBox
    '    Private btnSearch As Button
    '    Private btnToday As Button
    '    Private lblCount As Label

    '    Private grid As DataGridView

    '    ' ====== State ======
    '    Private _cts As CancellationTokenSource

    '    ' ====== Form Load ======
    '    Private Async Sub FrmAssemblyOrderList_Load(sender As Object, e As EventArgs) Handles MyBase.Load
    '        Me.Text = "組立オーダー一覧（検索・進捗）"
    '        Me.StartPosition = FormStartPosition.CenterScreen
    '        Me.WindowState = FormWindowState.Maximized

    '        BuildUi()

    '        ' 初期値（現場ホーム画面：今日～今日+14日 などもアリ。ここは当月表示）
    '        dtFrom.Value = New Date(Date.Today.Year, Date.Today.Month, 1)
    '        dtTo.Value = dtFrom.Value.AddMonths(1).AddDays(-1)

    '        Await LoadMasterCombosAsync()

    '        ' 初回表示
    '        Await SearchAsync()
    '    End Sub

    '    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
    '        MyBase.OnFormClosing(e)
    '        CancelRunning()
    '    End Sub

    '    ' ====== UI Construction ======
    '    Private Sub BuildUi()
    '        pnlTop = New Panel With {.Dock = DockStyle.Top, .Height = 64, .Padding = New Padding(12), .BackColor = Color.WhiteSmoke}
    '        Me.Controls.Add(pnlTop)

    '        Dim x As Integer = 12
    '        Dim y As Integer = 18

    '        ' 日付From
    '        pnlTop.Controls.Add(New Label With {.Text = "日付", .AutoSize = True, .Location = New Point(x, y + 4)})
    '        x += 42

    '        dtFrom = New DateTimePicker With {.Format = DateTimePickerFormat.Custom, .CustomFormat = "yyyy-MM-dd", .Width = 120, .Location = New Point(x, y)}
    '        pnlTop.Controls.Add(dtFrom)
    '        x += dtFrom.Width + 8

    '        pnlTop.Controls.Add(New Label With {.Text = "～", .AutoSize = True, .Location = New Point(x, y + 4)})
    '        x += 24

    '        dtTo = New DateTimePicker With {.Format = DateTimePickerFormat.Custom, .CustomFormat = "yyyy-MM-dd", .Width = 120, .Location = New Point(x, y)}
    '        pnlTop.Controls.Add(dtTo)
    '        x += dtTo.Width + 16

    '        ' ステータス
    '        pnlTop.Controls.Add(New Label With {.Text = "ステータス", .AutoSize = True, .Location = New Point(x, y + 4)})
    '        x += 80

    '        cboStatus = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Width = 140, .Location = New Point(x, y)}
    '        pnlTop.Controls.Add(cboStatus)
    '        x += cboStatus.Width + 16

    '        ' 完成品
    '        pnlTop.Controls.Add(New Label With {.Text = "完成品", .AutoSize = True, .Location = New Point(x, y + 4)})
    '        x += 52

    '        cboItem = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Width = 220, .Location = New Point(x, y)}
    '        pnlTop.Controls.Add(cboItem)
    '        x += cboItem.Width + 16

    '        ' 担当者
    '        pnlTop.Controls.Add(New Label With {.Text = "担当者", .AutoSize = True, .Location = New Point(x, y + 4)})
    '        x += 52

    '        cboUser = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Width = 180, .Location = New Point(x, y)}
    '        pnlTop.Controls.Add(cboUser)
    '        x += cboUser.Width + 16

    '        btnSearch = New Button With {.Text = "検索", .Width = 90, .Height = 28, .Location = New Point(x, y - 1)}
    '        AddHandler btnSearch.Click, Async Sub() Await SearchAsync()
    '        pnlTop.Controls.Add(btnSearch)
    '        x += btnSearch.Width + 8

    '        btnToday = New Button With {.Text = "今日の作業", .Width = 110, .Height = 28, .Location = New Point(x, y - 1)}
    '        AddHandler btnToday.Click, Async Sub() Await FilterTodayAsync()
    '        pnlTop.Controls.Add(btnToday)
    '        x += btnToday.Width + 12

    '        lblCount = New Label With {.Text = "件数: 0", .AutoSize = True, .Location = New Point(x, y + 4)}
    '        pnlTop.Controls.Add(lblCount)

    '        ' Grid
    '        grid = New DataGridView With {
    '            .Dock = DockStyle.Fill,
    '            .ReadOnly = True,
    '            .AllowUserToAddRows = False,
    '            .AllowUserToDeleteRows = False,
    '            .MultiSelect = False,
    '            .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
    '            .AutoGenerateColumns = False,
    '            .RowHeadersVisible = False,
    '            .BackgroundColor = Color.White
    '        }
    '        AddHandler grid.CellDoubleClick, AddressOf Grid_CellDoubleClick
    '        AddHandler grid.CellFormatting, AddressOf Grid_CellFormatting

    '        Me.Controls.Add(grid)

    '        BuildGridColumns()
    '        BuildStatusCombo()
    '    End Sub

    '    Private Sub BuildGridColumns()
    '        grid.Columns.Clear()

    '        ' 状態
    '        grid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "status", .HeaderText = "状態", .DataPropertyName = "status", .Width = 90})

    '        grid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "assembly_no", .HeaderText = "オーダーNo", .DataPropertyName = "assembly_no", .Width = 140})
    '        grid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "assembly_date", .HeaderText = "日付", .DataPropertyName = "assembly_date", .Width = 110, .DefaultCellStyle = New DataGridViewCellStyle With {.Format = "yyyy-MM-dd"}})
    '        grid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "item_name", .HeaderText = "完成品", .DataPropertyName = "item_name", .Width = 260})

    '        grid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "planned_qty", .HeaderText = "計画数量", .DataPropertyName = "planned_qty", .Width = 90, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight}})
    '        grid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "completed_qty", .HeaderText = "完了数量", .DataPropertyName = "completed_qty", .Width = 90, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight}})

    '        grid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "progress_pct", .HeaderText = "進捗%", .DataPropertyName = "progress_pct", .Width = 80, .DefaultCellStyle = New DataGridViewCellStyle With {.Alignment = DataGridViewContentAlignment.MiddleRight, .Format = "0.0"}})

    '        grid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "due_date", .HeaderText = "納期", .DataPropertyName = "due_date", .Width = 110, .DefaultCellStyle = New DataGridViewCellStyle With {.Format = "yyyy-MM-dd"}})
    '        grid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "user_name", .HeaderText = "担当", .DataPropertyName = "user_name", .Width = 140})

    '        ' hidden keys
    '        grid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "assembly_id", .HeaderText = "assembly_id", .DataPropertyName = "assembly_id", .Visible = False})
    '        grid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "item_id", .HeaderText = "item_id", .DataPropertyName = "item_id", .Visible = False})
    '        grid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "assigned_user_id", .HeaderText = "assigned_user_id", .DataPropertyName = "assigned_user_id", .Visible = False})
    '    End Sub

    '    Private Sub BuildStatusCombo()
    '        ' 表示名 / DB値
    '        Dim dt As New DataTable()
    '        dt.Columns.Add("text", GetType(String))
    '        dt.Columns.Add("value", GetType(String))

    '        dt.Rows.Add("すべて", "")
    '        dt.Rows.Add("計画", "PLANNED")
    '        dt.Rows.Add("作業中", "IN_PROGRESS")
    '        dt.Rows.Add("完了", "COMPLETED")
    '        dt.Rows.Add("キャンセル", "CANCELLED")

    '        cboStatus.DataSource = dt
    '        cboStatus.DisplayMember = "text"
    '        cboStatus.ValueMember = "value"
    '        cboStatus.SelectedValue = ""
    '    End Sub

    '    ' ====== Master Combos ======
    '    Private Async Function LoadMasterCombosAsync() As Task
    '        Await Task.Run(Sub()
    '                           ' 完成品マスタ（例：is_finished_good=1 などがあればWHERE追加）
    '                           Dim itemSql =
    '"SELECT 0 AS item_id, 'すべて' AS item_name
    ' UNION ALL
    'SELECT item_id, item_name
    '  FROM item_master
    ' ORDER BY item_id;"

    '                           Dim userSql =
    '"SELECT 0 AS user_id, 'すべて' AS user_name
    ' UNION ALL
    'SELECT user_id, user_name
    '  FROM user_master
    ' ORDER BY user_id;"

    '                           Dim dtItem = Db.FillTable(itemSql, Nothing)
    '                           Dim dtUser = Db.FillTable(userSql, Nothing)

    '                           Me.Invoke(Sub()
    '                                         cboItem.DataSource = dtItem
    '                                         cboItem.DisplayMember = "item_name"
    '                                         cboItem.ValueMember = "item_id"
    '                                         cboItem.SelectedValue = 0

    '                                         cboUser.DataSource = dtUser
    '                                         cboUser.DisplayMember = "user_name"
    '                                         cboUser.ValueMember = "user_id"
    '                                         cboUser.SelectedValue = 0
    '                                     End Sub)
    '                       End Sub)
    '    End Function

    '    ' ====== Search ======
    '    Private Async Function SearchAsync() As Task
    '        CancelRunning()
    '        _cts = New CancellationTokenSource()
    '        Dim token = _cts.Token

    '        btnSearch.Enabled = False
    '        btnToday.Enabled = False
    '        Me.Cursor = Cursors.WaitCursor

    '        Try
    '            Dim fromDate = dtFrom.Value.Date
    '            Dim toDate = dtTo.Value.Date
    '            Dim statusVal = CStr(cboStatus.SelectedValue)
    '            Dim itemId = Convert.ToInt64(cboItem.SelectedValue)
    '            Dim userId = Convert.ToInt64(cboUser.SelectedValue)

    '            Dim dt As DataTable = Await Task.Run(Function()
    '                                                     token.ThrowIfCancellationRequested()
    '                                                     Return QueryAssemblyOrders(fromDate, toDate, statusVal, itemId, userId)
    '                                                 End Function, token)

    '            ' 進捗列を追加/更新
    '            If Not dt.Columns.Contains("progress_pct") Then
    '                dt.Columns.Add("progress_pct", GetType(Decimal))
    '            End If

    '            For Each r As DataRow In dt.Rows
    '                Dim planned = SafeDec(r("planned_qty"))
    '                Dim completed = SafeDec(r("completed_qty"))
    '                Dim pct As Decimal = 0D
    '                If planned > 0D Then pct = (completed / planned) * 100D
    '                r("progress_pct") = pct
    '            Next

    '            grid.DataSource = dt
    '            lblCount.Text = $"件数: {dt.Rows.Count}"
    '        Catch ex As OperationCanceledException
    '            ' キャンセルは無視
    '        Catch ex As Exception
    '            MessageBox.Show(ex.Message, "検索エラー", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '        Finally
    '            Me.Cursor = Cursors.Default
    '            btnSearch.Enabled = True
    '            btnToday.Enabled = True
    '        End Try
    '    End Function

    '    Private Async Function FilterTodayAsync() As Task
    '        dtFrom.Value = Date.Today
    '        dtTo.Value = Date.Today
    '        cboStatus.SelectedValue = "IN_PROGRESS" ' 現場的に「作業中」優先。不要なら消してOK
    '        Await SearchAsync()
    '    End Function

    '    Private Sub CancelRunning()
    '        Try
    '            If _cts IsNot Nothing Then
    '                _cts.Cancel()
    '                _cts.Dispose()
    '                _cts = Nothing
    '            End If
    '        Catch
    '        End Try
    '    End Sub

    '    ' ====== DB Query ======
    '    Private Function QueryAssemblyOrders(fromDate As Date, toDate As Date, statusVal As String, itemId As Long, userId As Long) As DataTable
    '        Dim sql As String =
    '"
    'SELECT
    '    a.assembly_id,
    '    a.assembly_no,
    '    a.assembly_date,
    '    a.item_id,
    '    i.item_name,
    '    a.planned_qty,
    '    a.completed_qty,
    '    a.status,
    '    a.due_date,
    '    a.assigned_user_id,
    '    u.user_name
    'FROM assembly_order_header a
    'LEFT JOIN item_master i ON a.item_id = i.item_id
    'LEFT JOIN user_master u ON a.assigned_user_id = u.user_id
    'WHERE a.assembly_date BETWEEN @from AND @to
    '  AND (@status = '' OR a.status = @status)
    '  AND (@item_id = 0 OR a.item_id = @item_id)
    '  AND (@user_id = 0 OR a.assigned_user_id = @user_id)
    'ORDER BY
    '    CASE WHEN a.due_date IS NULL THEN 1 ELSE 0 END,
    '    a.due_date ASC,
    '    a.assembly_date DESC;
    '"

    '        Dim ps As New List(Of MySqlParameter) From {
    '            New MySqlParameter("@from", MySqlDbType.Date) With {.Value = fromDate},
    '            New MySqlParameter("@to", MySqlDbType.Date) With {.Value = toDate},
    '            New MySqlParameter("@status", MySqlDbType.VarChar) With {.Value = If(statusVal, "")},
    '            New MySqlParameter("@item_id", MySqlDbType.Int64) With {.Value = itemId},
    '            New MySqlParameter("@user_id", MySqlDbType.Int64) With {.Value = userId}
    '        }

    '        Return Db.FillTable(sql, ps)
    '    End Function

    '    ' ====== Visuals / Formatting ======
    '    Private Sub Grid_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs)
    '        If e.RowIndex < 0 Then Return

    '        Dim row = grid.Rows(e.RowIndex)
    '        Dim status = SafeStr(row.Cells("status").Value)
    '        Dim dueDateObj = row.Cells("due_date").Value
    '        Dim dueDate As Date? = Nothing
    '        If dueDateObj IsNot Nothing AndAlso dueDateObj IsNot DBNull.Value Then
    '            dueDate = Convert.ToDateTime(dueDateObj).Date
    '        End If

    '        ' 行の色（現場ホーム向け）
    '        ' 完了：緑 / 期限切れ未完：薄赤 / 作業中：薄黄
    '        If status = "COMPLETED" Then
    '            row.DefaultCellStyle.BackColor = Color.Honeydew
    '        ElseIf dueDate.HasValue AndAlso dueDate.Value < Date.Today AndAlso status <> "COMPLETED" Then
    '            row.DefaultCellStyle.BackColor = Color.MistyRose
    '        ElseIf status = "IN_PROGRESS" Then
    '            row.DefaultCellStyle.BackColor = Color.LemonChiffon
    '        Else
    '            row.DefaultCellStyle.BackColor = Color.White
    '        End If

    '        ' ステータスを日本語表示にしたい場合（見た目だけ変える）
    '        If grid.Columns(e.ColumnIndex).Name = "status" AndAlso e.Value IsNot Nothing Then
    '            Dim s = SafeStr(e.Value)
    '            Select Case s
    '                Case "PLANNED" : e.Value = "計画"
    '                Case "IN_PROGRESS" : e.Value = "作業中"
    '                Case "COMPLETED" : e.Value = "完了"
    '                Case "CANCELLED" : e.Value = "キャンセル"
    '            End Select
    '            e.FormattingApplied = True
    '        End If
    '    End Sub

    '    ' ====== Double click => open detail ======
    '    Private Sub Grid_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs)
    '        If e.RowIndex < 0 Then Return
    '        Dim row = grid.Rows(e.RowIndex)

    '        Dim assemblyId = Convert.ToInt64(row.Cells("assembly_id").Value)
    '        Dim assemblyNo = SafeStr(row.Cells("assembly_no").Value)

    '        ' TODO: 詳細フォームへ
    '        MessageBox.Show($"詳細を開く: {assemblyNo} (ID={assemblyId})", "Open Detail", MessageBoxButtons.OK, MessageBoxIcon.Information)

    '        ' 例：
    '        ' Using f As New FrmAssemblyOrderDetail(assemblyId)
    '        '     f.ShowDialog(Me)
    '        ' End Using
    '    End Sub

    '    ' ====== Helpers ======
    '    Private Function SafeStr(v As Object) As String
    '        If v Is Nothing OrElse v Is DBNull.Value Then Return ""
    '        Return v.ToString()
    '    End Function

    '    Private Function SafeDec(v As Object) As Decimal
    '        If v Is Nothing OrElse v Is DBNull.Value Then Return 0D
    '        Dim d As Decimal
    '        If Decimal.TryParse(v.ToString(), d) Then Return d
    '        Return 0D
    '    End Function

End Class