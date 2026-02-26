Imports System.ComponentModel
Imports System.Globalization

Public Class IssueConsumeForm

    '    Private ReadOnly _svc As New IssueService()
    '    Private _issueId As Long = 0

    '    ' UI controls (Designerを使わない最小実装)
    '    Private txtIssueId As New TextBox()
    '    Private dtIssueDate As New DateTimePicker()
    '    Private txtRefType As New TextBox()
    '    Private txtRefId As New TextBox()
    '    Private txtWarehouseId As New TextBox()
    '    Private txtMemo As New TextBox()
    '    Private txtUser As New TextBox()

    '    Private dgv As New DataGridView()
    '    Private btnNew As New Button()
    '    Private btnLoad As New Button()
    '    Private btnSave As New Button()
    '    Private btnPost As New Button()
    '    Private btnReverse As New Button()

    '    Public Sub New()
    '        InitializeComponent()
    '        BuildUi()
    '        InitGrid()
    '        ResetForm()
    '    End Sub

    '    Private Sub BuildUi()
    '        Me.Text = "払出（消費）実績（Issue / Consume）"
    '        Me.Width = 1200
    '        Me.Height = 750

    '        Dim y = 10

    '        Dim lbl1 As New Label() With {.Text = "Issue ID", .Left = 10, .Top = y + 6, .Width = 80}
    '        txtIssueId.Left = 95 : txtIssueId.Top = y : txtIssueId.Width = 120 : txtIssueId.ReadOnly = True

    '        Dim lbl2 As New Label() With {.Text = "Issue Date", .Left = 230, .Top = y + 6, .Width = 80}
    '        dtIssueDate.Left = 320 : dtIssueDate.Top = y : dtIssueDate.Width = 140

    '        Dim lblUser As New Label() With {.Text = "User", .Left = 480, .Top = y + 6, .Width = 50}
    '        txtUser.Left = 535 : txtUser.Top = y : txtUser.Width = 140 : txtUser.Text = Environment.UserName

    '        y += 35

    '        Dim lbl3 As New Label() With {.Text = "Ref Type", .Left = 10, .Top = y + 6, .Width = 80}
    '        txtRefType.Left = 95 : txtRefType.Top = y : txtRefType.Width = 120 : txtRefType.Text = "PROD_ORDER"

    '        Dim lbl4 As New Label() With {.Text = "Ref ID", .Left = 230, .Top = y + 6, .Width = 80}
    '        txtRefId.Left = 320 : txtRefId.Top = y : txtRefId.Width = 140

    '        Dim lbl5 As New Label() With {.Text = "Warehouse", .Left = 480, .Top = y + 6, .Width = 80}
    '        txtWarehouseId.Left = 565 : txtWarehouseId.Top = y : txtWarehouseId.Width = 110

    '        y += 35

    '        Dim lbl6 As New Label() With {.Text = "Memo", .Left = 10, .Top = y + 6, .Width = 80}
    '        txtMemo.Left = 95 : txtMemo.Top = y : txtMemo.Width = 580

    '        y += 40

    '        btnNew.Text = "新規"
    '        btnLoad.Text = "読込"
    '        btnSave.Text = "保存"
    '        btnPost.Text = "確定(POST)"
    '        btnReverse.Text = "取消(REV)"

    '        btnNew.Left = 10 : btnNew.Top = y : btnNew.Width = 90
    '        btnLoad.Left = 110 : btnLoad.Top = y : btnLoad.Width = 90
    '        btnSave.Left = 210 : btnSave.Top = y : btnSave.Width = 90
    '        btnPost.Left = 310 : btnPost.Top = y : btnPost.Width = 120
    '        btnReverse.Left = 440 : btnReverse.Top = y : btnReverse.Width = 120

    '        AddHandler btnNew.Click, AddressOf OnNew
    '        AddHandler btnLoad.Click, AddressOf OnLoad
    '        AddHandler btnSave.Click, AddressOf OnSave
    '        AddHandler btnPost.Click, AddressOf OnPost
    '        AddHandler btnReverse.Click, AddressOf OnReverse

    '        y += 45

    '        dgv.Left = 10 : dgv.Top = y : dgv.Width = Me.ClientSize.Width - 20 : dgv.Height = Me.ClientSize.Height - y - 15
    '        dgv.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Bottom

    '        Me.Controls.AddRange(New Control() {
    '            lbl1, txtIssueId, lbl2, dtIssueDate, lblUser, txtUser,
    '            lbl3, txtRefType, lbl4, txtRefId, lbl5, txtWarehouseId,
    '            lbl6, txtMemo,
    '            btnNew, btnLoad, btnSave, btnPost, btnReverse,
    '            dgv
    '        })
    '    End Sub

    '    Private Sub InitGrid()
    '        dgv.AllowUserToAddRows = True
    '        dgv.AllowUserToDeleteRows = True
    '        dgv.AutoGenerateColumns = False
    '        dgv.RowHeadersWidth = 30

    '        dgv.Columns.Clear()
    '        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "item_id", .HeaderText = "Item ID", .DataPropertyName = "item_id", .Width = 90})
    '        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "qty_pieces", .HeaderText = "Qty (pieces)", .DataPropertyName = "qty_pieces", .Width = 110})
    '        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "lot_id", .HeaderText = "Lot ID", .DataPropertyName = "lot_id", .Width = 90})
    '        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "lot_unit_id", .HeaderText = "Lot Unit ID", .DataPropertyName = "lot_unit_id", .Width = 110})
    '        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "reason_code", .HeaderText = "Reason", .DataPropertyName = "reason_code", .Width = 120})
    '        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "note", .HeaderText = "Note", .DataPropertyName = "note", .Width = 250})
    '        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "alloc_key", .HeaderText = "Alloc Key", .DataPropertyName = "alloc_key", .Width = 200})

    '        AddHandler dgv.DataError, Sub(s, e) e.ThrowException = False
    '    End Sub

    '    Private Sub ResetForm()
    '        _issueId = 0
    '        txtIssueId.Text = ""
    '        dtIssueDate.Value = Date.Today
    '        txtRefType.Text = "PROD_ORDER"
    '        txtRefId.Text = ""
    '        txtWarehouseId.Text = ""
    '        txtMemo.Text = ""
    '        dgv.Rows.Clear()
    '    End Sub

    '    ' ==========
    '    ' events
    '    ' ==========

    '    Private Sub OnNew(sender As Object, e As EventArgs)
    '        ResetForm()
    '        dgv.Rows.Add() ' 1行追加
    '    End Sub

    '    Private Sub OnLoad(sender As Object, e As EventArgs)
    '        Dim input = InputBox("Issue ID を入力", "読込", If(_issueId > 0, _issueId.ToString(), ""))
    '        If String.IsNullOrWhiteSpace(input) Then Return
    '        Dim id As Long
    '        If Not Long.TryParse(input, id) Then
    '            MessageBox.Show("数値を入力してください。")
    '            Return
    '        End If

    '        LoadIssue(id)
    '    End Sub

    '    Private Sub OnSave(sender As Object, e As EventArgs)
    '        Try
    '            Dim header = ReadHeaderFromUi()
    '            header.IssueId = _issueId
    '            Dim lines = ReadLinesFromGrid()

    '            Dim newId = _svc.SaveDraft(header, lines)
    '            _issueId = newId
    '            txtIssueId.Text = newId.ToString()

    '            MessageBox.Show($"保存しました。Issue ID={newId}")
    '        Catch ex As Exception
    '            MessageBox.Show(ex.Message, "保存エラー")
    '        End Try
    '    End Sub

    '    Private Sub OnPost(sender As Object, e As EventArgs)
    '        Try
    '            If _issueId <= 0 Then Throw New Exception("先に保存してください。")
    '            _svc.Post(_issueId, txtUser.Text.Trim())
    '            MessageBox.Show("確定しました（在庫台帳/在庫/個体を更新）。")
    '        Catch ex As Exception
    '            MessageBox.Show(ex.Message, "確定エラー")
    '        End Try
    '    End Sub

    '    Private Sub OnReverse(sender As Object, e As EventArgs)
    '        Try
    '            If _issueId <= 0 Then Throw New Exception("Issue ID がありません。")
    '            _svc.Reverse(_issueId, txtUser.Text.Trim())
    '            MessageBox.Show("取消しました（逆仕訳）。")
    '        Catch ex As Exception
    '            MessageBox.Show(ex.Message, "取消エラー")
    '        End Try
    '    End Sub

    '    ' ==========
    '    ' UI → Model
    '    ' ==========

    '    Private Function ReadHeaderFromUi() As IssueHeader
    '        Dim h As New IssueHeader With {
    '            .IssueDate = dtIssueDate.Value.Date,
    '            .RefType = txtRefType.Text.Trim(),
    '            .RefId = ParseNullableLong(txtRefId.Text),
    '            .WarehouseId = ParseNullableLong(txtWarehouseId.Text),
    '            .Memo = txtMemo.Text.Trim(),
    '            .CreatedBy = txtUser.Text.Trim()
    '        }
    '        If String.IsNullOrWhiteSpace(h.RefType) Then Throw New Exception("RefType は必須です。")
    '        Return h
    '    End Function

    '    Private Function ReadLinesFromGrid() As List(Of IssueLine)
    '        Dim list As New List(Of IssueLine)

    '        For Each row As DataGridViewRow In dgv.Rows
    '            If row.IsNewRow Then Continue For

    '            Dim itemIdStr = ToStr(row.Cells("item_id").Value)
    '            Dim qtyStr = ToStr(row.Cells("qty_pieces").Value)
    '            Dim lotIdStr = ToStr(row.Cells("lot_id").Value)

    '            If String.IsNullOrWhiteSpace(itemIdStr) AndAlso String.IsNullOrWhiteSpace(qtyStr) AndAlso String.IsNullOrWhiteSpace(lotIdStr) Then
    '                Continue For
    '            End If

    '            Dim itemId As Long
    '            If Not Long.TryParse(itemIdStr, itemId) OrElse itemId <= 0 Then Throw New Exception("明細: Item ID が不正です。")

    '            Dim qty As Decimal
    '            If Not Decimal.TryParse(qtyStr, NumberStyles.Any, CultureInfo.InvariantCulture, qty) Then
    '                ' 日本環境向け
    '                If Not Decimal.TryParse(qtyStr, NumberStyles.Any, CultureInfo.CurrentCulture, qty) Then
    '                    Throw New Exception("明細: Qty が不正です。")
    '                End If
    '            End If
    '            If qty <= 0D Then Throw New Exception("明細: Qty は 0 より大きい必要があります。")

    '            Dim lotId = ParseNullableLong(lotIdStr)
    '            Dim lotUnitId = ParseNullableLong(ToStr(row.Cells("lot_unit_id").Value))

    '            Dim ln As New IssueLine With {
    '                .ItemId = itemId,
    '                .QtyPieces = qty,
    '                .LotId = lotId,
    '                .LotUnitId = lotUnitId,
    '                .ReasonCode = ToStr(row.Cells("reason_code").Value),
    '                .Note = ToStr(row.Cells("note").Value),
    '                .AllocKey = ToStr(row.Cells("alloc_key").Value)
    '            }
    '            list.Add(ln)
    '        Next

    '        If list.Count = 0 Then Throw New Exception("明細がありません。")
    '        Return list
    '    End Function

    '    ' ==========
    '    ' Load (簡易：直接DBから読み直し)
    '    ' ※本格運用は IssueService に Get を追加してOK
    '    ' ==========
    '    Private Sub LoadIssue(issueId As Long)
    '        Using cn = Db.OpenConnection()
    '            ' header
    '            Using cmd = Db.CreateCmd(
    '"SELECT issue_id, issue_date, ref_type, ref_id, warehouse_id, memo, posted
    ' FROM issue_header WHERE issue_id=@id;", cn)
    '                Db.Param(cmd, "@id", issueId)
    '                Using r = cmd.ExecuteReader()
    '                    If Not r.Read() Then
    '                        MessageBox.Show("見つかりません。")
    '                        Return
    '                    End If
    '                    _issueId = r.GetInt64("issue_id")
    '                    txtIssueId.Text = _issueId.ToString()
    '                    dtIssueDate.Value = r.GetDateTime("issue_date")
    '                    txtRefType.Text = r.GetString("ref_type")
    '                    txtRefId.Text = If(r.IsDBNull(r.GetOrdinal("ref_id")), "", r.GetInt64("ref_id").ToString())
    '                    txtWarehouseId.Text = If(r.IsDBNull(r.GetOrdinal("warehouse_id")), "", r.GetInt64("warehouse_id").ToString())
    '                    txtMemo.Text = If(r.IsDBNull(r.GetOrdinal("memo")), "", r.GetString("memo"))
    '                End Using
    '            End Using

    '            ' lines
    '            dgv.Rows.Clear()
    '            Using cmd = Db.CreateCmd(
    '"SELECT item_id, qty_pieces, lot_id, lot_unit_id, reason_code, note, alloc_key
    ' FROM issue_line WHERE issue_id=@id ORDER BY issue_line_id;", cn)
    '                Db.Param(cmd, "@id", issueId)
    '                Using r = cmd.ExecuteReader()
    '                    While r.Read()
    '                        dgv.Rows.Add(
    '                            r.GetInt64("item_id").ToString(),
    '                            r.GetDecimal("qty_pieces").ToString(),
    '                            If(r.IsDBNull(r.GetOrdinal("lot_id")), "", r.GetInt64("lot_id").ToString()),
    '                            If(r.IsDBNull(r.GetOrdinal("lot_unit_id")), "", r.GetInt64("lot_unit_id").ToString()),
    '                            If(r.IsDBNull(r.GetOrdinal("reason_code")), "", r.GetString("reason_code")),
    '                            If(r.IsDBNull(r.GetOrdinal("note")), "", r.GetString("note")),
    '                            If(r.IsDBNull(r.GetOrdinal("alloc_key")), "", r.GetString("alloc_key"))
    '                        )
    '                    End While
    '                End Using
    '            End Using
    '        End Using
    '    End Sub

    '    ' ==========
    '    ' helpers
    '    ' ==========
    '    Private Function ParseNullableLong(s As String) As Long?
    '        s = If(s, "").Trim()
    '        If s = "" Then Return Nothing
    '        Dim v As Long
    '        If Long.TryParse(s, v) Then Return v
    '        Throw New Exception($"数値として解釈できません: {s}")
    '    End Function

    '    Private Function ToStr(v As Object) As String
    '        If v Is Nothing OrElse v Is DBNull.Value Then Return ""
    '        Return Convert.ToString(v)
    '    End Function

End Class