Imports MySql.Data.MySqlClient
Imports System.Windows.Forms
Imports System.Drawing
Imports System.Collections.Generic
Imports System.Linq

Public Class LotAllocForm
    Inherits Form

    Private ReadOnly _connStr As String
    Private ReadOnly _itemId As Integer
    Private ReadOnly _itemName As String
    Private ReadOnly _requiredPieces As Integer
    Private ReadOnly _existing As List(Of LotAlloc)

    Private dgv As DataGridView
    Private lblTop As Label
    Private lblSum As Label
    Private btnAuto As Button
    Private btnOk As Button
    Private btnCancel As Button

    ' ★ReadOnlyプロパティに代入できないのでバックフィールドを用意
    Private _resultAllocs As List(Of LotAlloc) = New List(Of LotAlloc)()
    Public ReadOnly Property ResultAllocs As List(Of LotAlloc)
        Get
            Return _resultAllocs
        End Get
    End Property

    Public ReadOnly Property Allocations As List(Of LotAlloc)
        Get
            Return ResultAllocs
        End Get
    End Property


    ' LotAllocForm.vb（クラス内）


    Public Sub New(connStr As String, itemId As Integer, itemName As String, requiredPieces As Integer, existing As List(Of LotAlloc))
        _connStr = connStr
        _itemId = itemId
        _itemName = itemName
        _requiredPieces = requiredPieces
        _existing = If(existing, New List(Of LotAlloc)())

        Me.Text = "ロット割当"
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.StartPosition = FormStartPosition.CenterParent
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ClientSize = New Size(720, 430)

        lblTop = New Label() With {
            .Left = 12, .Top = 10, .Width = 690, .Height = 40,
            .Text = $"[{_itemName}] 必要個数：{_requiredPieces} 個（合計が一致するようにロットに割当て）"
        }

        dgv = New DataGridView() With {
            .Left = 12, .Top = 56, .Width = 690, .Height = 290,
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .RowHeadersVisible = False,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
            .SelectionMode = DataGridViewSelectionMode.CellSelect,
            .MultiSelect = False
        }

        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "lot_id", .HeaderText = "lot_id", .Width = 70, .ReadOnly = True})
        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "lot_no", .HeaderText = "ロット番号", .Width = 180, .ReadOnly = True})
        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "available", .HeaderText = "在庫(個)", .Width = 90, .ReadOnly = True})
        dgv.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "alloc", .HeaderText = "引当(個)", .Width = 90})

        ' 見やすさ：数値は右寄せ
        dgv.Columns("available").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
        dgv.Columns("alloc").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight

        ' 入力系イベント（数字のみ/検証/エラー掃除）
        AddHandler dgv.EditingControlShowing, AddressOf Dgv_EditingControlShowing
        AddHandler dgv.CellValidating, AddressOf Dgv_CellValidating
        AddHandler dgv.CellEndEdit, AddressOf Dgv_CellEndEdit

        lblSum = New Label() With {.Left = 12, .Top = 354, .Width = 520, .Height = 24}

        btnAuto = New Button() With {.Text = "自動割当", .Left = 400, .Top = 382, .Width = 100, .Height = 32}
        btnOk = New Button() With {.Text = "OK", .Left = 510, .Top = 382, .Width = 90, .Height = 32}
        btnCancel = New Button() With {.Text = "キャンセル", .Left = 612, .Top = 382, .Width = 90, .Height = 32}

        AddHandler btnAuto.Click, AddressOf AutoAlloc_Click
        AddHandler btnOk.Click, AddressOf Ok_Click
        AddHandler btnCancel.Click, Sub()
                                        Me.DialogResult = DialogResult.Cancel
                                        Me.Close()
                                    End Sub

        Me.Controls.Add(lblTop)
        Me.Controls.Add(dgv)
        Me.Controls.Add(lblSum)
        Me.Controls.Add(btnAuto)
        Me.Controls.Add(btnOk)
        Me.Controls.Add(btnCancel)

        LoadLots()
        ApplyExisting()
        UpdateSumLabel()
    End Sub

    Private Sub LoadLots()
        dgv.Rows.Clear()

        ' 既存割当の lot_id は「inactiveでも」必ず拾う
        Dim existingIds As New List(Of Long)()
        If _existing IsNot Nothing AndAlso _existing.Count > 0 Then
            existingIds = _existing.Select(Function(x) x.LotId).Distinct().ToList()
        End If

        Using conn As New MySqlConnection(_connStr)
            conn.Open()

            Dim sql As String =
"SELECT l.lot_id, l.lot_no, l.qty_on_hand_pieces
   FROM lot l
  WHERE l.item_id=@item_id
    AND (l.is_active=1 " & If(existingIds.Count > 0, " OR l.lot_id IN ({0})", "") & ")
  ORDER BY l.received_date DESC, l.lot_id DESC"

            Using cmd As New MySqlCommand("", conn)

                cmd.Parameters.AddWithValue("@item_id", _itemId)

                If existingIds.Count > 0 Then
                    Dim ps As New List(Of String)()
                    For i = 0 To existingIds.Count - 1
                        Dim p = "@ex" & i
                        ps.Add(p)
                        cmd.Parameters.AddWithValue(p, existingIds(i))
                    Next
                    cmd.CommandText = String.Format(sql, String.Join(",", ps))
                Else
                    cmd.CommandText = sql
                End If

                Using rdr = cmd.ExecuteReader()
                    While rdr.Read()
                        Dim lotId As Long = Convert.ToInt64(rdr("lot_id"))
                        Dim lotNo As String = Convert.ToString(rdr("lot_no"))
                        Dim avail As Integer = Convert.ToInt32(rdr("qty_on_hand_pieces"))
                        dgv.Rows.Add(lotId.ToString(), lotNo, avail.ToString(), "0")
                    End While
                End Using
            End Using
        End Using

        ' ここで0行なら “呼び出し元の item_id が違う/lotが無い” が確定
        If dgv.Rows.Count = 0 Then
            MessageBox.Show(
            $"ロットが見つかりませんでした。" & vbCrLf &
            $"item_id={_itemId} / item={_itemName}" & vbCrLf &
            $"※ lotテーブルにこのitem_idのデータが無いか、item_idが未確定です。",
            "ロットなし", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Sub


    Private Sub ApplyExisting()
        If _existing Is Nothing OrElse _existing.Count = 0 Then Return

        Dim map = _existing.ToDictionary(Function(x) x.LotId, Function(x) x)

        For Each r As DataGridViewRow In dgv.Rows
            Dim lotId As Long = Convert.ToInt64(r.Cells("lot_id").Value)
            If map.ContainsKey(lotId) Then
                r.Cells("alloc").Value = map(lotId).QtyPieces.ToString()
            End If
        Next
    End Sub

    Private Function SumAlloc() As Integer
        Dim sum As Integer = 0
        For Each r As DataGridViewRow In dgv.Rows
            Dim s = If(r.Cells("alloc").Value, "").ToString().Trim()
            Dim v As Integer
            If s <> "" AndAlso Integer.TryParse(s, v) AndAlso v > 0 Then sum += v
        Next
        Return sum
    End Function

    Private Sub UpdateSumLabel()
        Dim sum = SumAlloc()
        lblSum.Text = $"合計引当：{sum} 個 / 必要：{_requiredPieces} 個"
        lblSum.ForeColor = If(sum = _requiredPieces, Color.DarkGreen, Color.DarkRed)
    End Sub

    ' ====== セル編集完了：エラー掃除＋合計更新 ======
    Private Sub Dgv_CellEndEdit(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex >= 0 Then dgv.Rows(e.RowIndex).ErrorText = ""
        UpdateSumLabel()
    End Sub

    ' ====== 入力を数字だけに制限（alloc列のみ） ======
    Private Sub Dgv_EditingControlShowing(sender As Object, e As DataGridViewEditingControlShowingEventArgs)
        If dgv.CurrentCell Is Nothing Then Return
        If dgv.Columns(dgv.CurrentCell.ColumnIndex).Name <> "alloc" Then Return

        Dim tb = TryCast(e.Control, TextBox)
        If tb Is Nothing Then Return

        RemoveHandler tb.KeyPress, AddressOf AllocTextBox_KeyPress
        AddHandler tb.KeyPress, AddressOf AllocTextBox_KeyPress
    End Sub

    Private Sub AllocTextBox_KeyPress(sender As Object, e As KeyPressEventArgs)
        ' 数字 / バックスペースのみ許可（マイナス/小数は禁止）
        If Char.IsControl(e.KeyChar) Then Return
        If Not Char.IsDigit(e.KeyChar) Then e.Handled = True
    End Sub

    ' ====== セル確定前に検証（在庫超え、整数、0以上） ======
    Private Sub Dgv_CellValidating(sender As Object, e As DataGridViewCellValidatingEventArgs)
        If dgv.Columns(e.ColumnIndex).Name <> "alloc" Then Return

        Dim input As String = If(e.FormattedValue, "").ToString().Trim()

        ' 空欄はOK（=0扱い）
        If input = "" Then
            dgv.Rows(e.RowIndex).ErrorText = ""
            Return
        End If

        Dim v As Integer
        If Not Integer.TryParse(input, v) Then
            dgv.Rows(e.RowIndex).ErrorText = "引当(個)は整数で入力してください。"
            e.Cancel = True
            Return
        End If

        If v < 0 Then
            dgv.Rows(e.RowIndex).ErrorText = "引当(個)は0以上で入力してください。"
            e.Cancel = True
            Return
        End If

        Dim avail As Integer = Convert.ToInt32(dgv.Rows(e.RowIndex).Cells("available").Value)
        If v > avail Then
            dgv.Rows(e.RowIndex).ErrorText = $"在庫超過です（在庫={avail} 個）"
            e.Cancel = True
            Return
        End If

        dgv.Rows(e.RowIndex).ErrorText = ""
    End Sub

    ' ====== 自動割当（新しいロットから順に埋める） ======
    Private Sub AutoAlloc_Click(sender As Object, e As EventArgs)
        ' いったん全部クリア
        For Each r As DataGridViewRow In dgv.Rows
            r.Cells("alloc").Value = "0"
            r.ErrorText = ""
        Next

        Dim remain As Integer = _requiredPieces

        For Each r As DataGridViewRow In dgv.Rows
            If remain <= 0 Then Exit For

            Dim avail As Integer = Convert.ToInt32(r.Cells("available").Value)
            If avail <= 0 Then
                r.Cells("alloc").Value = "0"
                Continue For
            End If

            Dim take As Integer = Math.Min(avail, remain)
            r.Cells("alloc").Value = take.ToString()
            remain -= take
        Next

        UpdateSumLabel()

        If remain > 0 Then
            MessageBox.Show($"在庫が不足しています。必要={_requiredPieces} 個に対し、不足={remain} 個です。")
        End If
    End Sub

    Private Sub Ok_Click(sender As Object, e As EventArgs)
        ' ★超重要：編集中セルの値を確定してから集計する
        dgv.EndEdit()
        Me.Validate()

        Dim allocs As New List(Of LotAlloc)()
        Dim sum As Integer = 0

        For Each r As DataGridViewRow In dgv.Rows
            Dim lotId As Long = Convert.ToInt64(r.Cells("lot_id").Value)
            Dim lotNo As String = Convert.ToString(r.Cells("lot_no").Value)
            Dim avail As Integer = Convert.ToInt32(r.Cells("available").Value)

            Dim s = If(r.Cells("alloc").Value, "").ToString().Trim()
            If s = "" Then Continue For

            Dim v As Integer
            If Not Integer.TryParse(s, v) Then
                MessageBox.Show("引当(個)は整数で入力してください。")
                Return
            End If
            If v < 0 Then
                MessageBox.Show("引当(個)は0以上で入力してください。")
                Return
            End If
            If v = 0 Then Continue For
            If v > avail Then
                MessageBox.Show($"在庫超過です：{lotNo} 在庫={avail} 個 / 引当={v} 個")
                Return
            End If

            sum += v
            allocs.Add(New LotAlloc With {.LotId = lotId, .LotNo = lotNo, .Available = avail, .QtyPieces = v})
        Next

        If allocs.Count = 0 Then
            MessageBox.Show("ロット割当がありません。少なくとも1つのロットに引当してください。")
            Return
        End If
        If sum <> _requiredPieces Then
            MessageBox.Show($"合計引当が一致していません。必要={_requiredPieces} 個 / 合計={sum} 個")
            Return
        End If

        _resultAllocs = allocs
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub



    ' 外側でも使えるDTO
    Public Class LotAlloc
        Public Property LotId As Long
        Public Property LotNo As String
        Public Property Available As Integer
        Public Property QtyPieces As Integer
    End Class
End Class
