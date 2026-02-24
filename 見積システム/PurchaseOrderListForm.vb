Imports MySql.Data.MySqlClient
Imports System.Data
Imports System.Windows.Forms

Public Class PurchaseOrderListForm
    Inherits Form

    Private ReadOnly _cs As String
    Private dgv As DataGridView
    Private dtpFrom As DateTimePicker
    Private dtpTo As DateTimePicker
    Private cboStatus As ComboBox
    Private btnSearch As Button
    Private btnNew As Button
    Private btnOpen As Button

    Private dt As DataTable

    Public Sub New(cs As String)
        _cs = cs
        BuildUi()
    End Sub

    Private Sub BuildUi()
        Me.Text = "発注書 一覧"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.Width = 980
        Me.Height = 620

        dtpFrom = New DateTimePicker() With {.Left = 16, .Top = 16, .Width = 140, .Format = DateTimePickerFormat.Short}
        dtpTo = New DateTimePicker() With {.Left = 170, .Top = 16, .Width = 140, .Format = DateTimePickerFormat.Short}
        cboStatus = New ComboBox() With {.Left = 330, .Top = 16, .Width = 160, .DropDownStyle = ComboBoxStyle.DropDownList}
        cboStatus.Items.AddRange(New Object() {"(全て)", "OPEN", "PARTIAL", "RECEIVED"})
        cboStatus.SelectedIndex = 0

        btnSearch = New Button() With {.Left = 510, .Top = 14, .Width = 90, .Height = 30, .Text = "検索"}
        btnNew = New Button() With {.Left = 610, .Top = 14, .Width = 110, .Height = 30, .Text = "新規作成"}
        btnOpen = New Button() With {.Left = 730, .Top = 14, .Width = 110, .Height = 30, .Text = "開く"}

        dgv = New DataGridView() With {
            .Left = 16, .Top = 56, .Width = Me.ClientSize.Width - 32, .Height = Me.ClientSize.Height - 80,
            .ReadOnly = True, .AllowUserToAddRows = False, .AllowUserToDeleteRows = False,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect, .MultiSelect = False,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
            .RowHeadersVisible = False
        }

        Me.Controls.AddRange(New Control() {dtpFrom, dtpTo, cboStatus, btnSearch, btnNew, btnOpen, dgv})

        AddHandler Me.Load, Sub()
                                dtpFrom.Value = Date.Today.AddMonths(-1)
                                dtpTo.Value = Date.Today
                                Search()
                            End Sub
        AddHandler btnSearch.Click, Sub() Search()
        AddHandler btnNew.Click, Sub()
                                     Using f As New PurchaseOrderForm(_cs)
                                         If f.ShowDialog(Me) = DialogResult.OK Then Search()
                                     End Using
                                 End Sub
        AddHandler btnOpen.Click, Sub() OpenSelected()
        AddHandler dgv.CellDoubleClick, Sub(s, e)
                                            If e.RowIndex >= 0 Then OpenSelected()
                                        End Sub
    End Sub

    Private Sub Search()
        Dim sql As String =
    "SELECT po_id, po_no, po_date, supplier_name, status, total
 FROM purchase_order_header
 WHERE po_date BETWEEN @d1 AND @d2
   AND (@st = '' OR status = @st)
 ORDER BY po_date DESC, po_id DESC;"

        dt = New DataTable()

        Using conn As New MySqlConnection(_cs)
            conn.Open()
            Using cmd As New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@d1", dtpFrom.Value.Date)
                cmd.Parameters.AddWithValue("@d2", dtpTo.Value.Date)
                Dim st As String = If(cboStatus.SelectedIndex <= 0, "", cboStatus.SelectedItem.ToString())
                cmd.Parameters.AddWithValue("@st", st)

                Using da As New MySqlDataAdapter(cmd)
                    da.Fill(dt)
                End Using
            End Using
        End Using

        dgv.DataSource = dt

        ' ---- 安全に列を触る ----
        SetHeaderSafe("po_id", Nothing, False)
        SetHeaderSafe("po_no", "発注番号")
        SetHeaderSafe("po_date", "発注日")
        SetHeaderSafe("supplier_name", "仕入先")
        SetHeaderSafe("status", "状態")
        SetHeaderSafe("total", "合計")
    End Sub


    Private Sub OpenSelected()
        If dgv Is Nothing OrElse dgv.CurrentRow Is Nothing Then
            MessageBox.Show("行を選択してください。")
            Return
        End If

        Dim row = dgv.CurrentRow
        If row Is Nothing Then
            MessageBox.Show("行を選択してください。")
            Return
        End If

        Dim poIdObj = row.Cells("po_id").Value
        If poIdObj Is Nothing OrElse poIdObj Is DBNull.Value Then
            MessageBox.Show("po_id が取得できません。")
            Return
        End If

        Dim poId As Long
        If Not Long.TryParse(poIdObj.ToString(), poId) OrElse poId <= 0 Then
            MessageBox.Show("po_id が不正です。")
            Return
        End If

        Using f As New PurchaseOrderForm(_cs)
            f.OpenSaved(poId) ' PurchaseOrderForm側のOpenSavedを使う
            If f.ShowDialog(Me) = DialogResult.OK Then
                Search()
            End If
        End Using
    End Sub


    Private Sub SetHeaderSafe(colName As String, headerText As String, Optional visible As Boolean = True)
        Dim c = dgv.Columns(colName)
        If c Is Nothing Then Return
        c.Visible = visible
        If headerText IsNot Nothing Then c.HeaderText = headerText
    End Sub

End Class
