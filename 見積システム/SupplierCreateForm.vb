Imports MySql.Data.MySqlClient
Imports System.Windows.Forms
Imports System.Drawing

Public Class SupplierCreateForm
    Inherits Form

    Private ReadOnly _cs As String

    ' controls（Designer不要）
    Private txtCode As TextBox
    Private txtName As TextBox
    Private txtTel As TextBox
    Private txtEmail As TextBox
    Private txtAddr1 As TextBox
    Private txtAddr2 As TextBox
    Private txtRemark As TextBox
    Private btnSave As Button
    Private btnClose As Button

    ' layout
    Private xL As Integer = 18
    Private wL As Integer = 120
    Private xT As Integer
    Private wT As Integer = 540

    Private fLbl As Font = New Font("Yu Gothic UI", 10.5F, FontStyle.Bold)
    Private fTxt As Font = New Font("Yu Gothic UI", 10.5F, FontStyle.Regular)

    Public Sub New(cs As String)
        _cs = cs
        xT = xL + wL + 10
        BuildUi()
    End Sub

    Private Function MakeLabel(caption As String, top As Integer) As Label
        Return New Label() With {
            .Text = caption,
            .Left = xL,
            .Top = top + 4,
            .Width = wL,
            .Height = 26,
            .Font = fLbl
        }
    End Function

    Private Function MakeText(top As Integer, multiline As Boolean, height As Integer) As TextBox
        Dim tb As New TextBox() With {
            .Left = xT,
            .Top = top,
            .Width = wT,
            .Font = fTxt
        }

        If multiline Then
            tb.Multiline = True
            tb.ScrollBars = ScrollBars.Vertical
            tb.Height = If(height > 0, height, 70)
        Else
            tb.Multiline = False
            tb.Height = 28
        End If

        Return tb
    End Function

    Private Sub BuildUi()
        Me.Text = "仕入先マスタ作成"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ClientSize = New Size(720, 420)

        Dim y As Integer = 18
        Dim th As Integer = 28
        Dim gap As Integer = 10

        Me.Controls.Add(MakeLabel("仕入先コード", y))
        txtCode = MakeText(y, False, 0) : Me.Controls.Add(txtCode)
        y += th + gap

        Me.Controls.Add(MakeLabel("仕入先名", y))
        txtName = MakeText(y, False, 0) : Me.Controls.Add(txtName)
        y += th + gap

        Me.Controls.Add(MakeLabel("TEL", y))
        txtTel = MakeText(y, False, 0) : Me.Controls.Add(txtTel)
        y += th + gap

        Me.Controls.Add(MakeLabel("Email", y))
        txtEmail = MakeText(y, False, 0) : Me.Controls.Add(txtEmail)
        y += th + gap

        Me.Controls.Add(MakeLabel("住所1", y))
        txtAddr1 = MakeText(y, False, 0) : Me.Controls.Add(txtAddr1)
        y += th + gap

        Me.Controls.Add(MakeLabel("住所2", y))
        txtAddr2 = MakeText(y, False, 0) : Me.Controls.Add(txtAddr2)
        y += th + gap

        Me.Controls.Add(MakeLabel("備考", y))
        txtRemark = MakeText(y, True, 90) : Me.Controls.Add(txtRemark)
        y += txtRemark.Height + 16

        btnSave = New Button() With {.Text = "保存", .Left = Me.ClientSize.Width - 210, .Top = y, .Width = 90, .Height = 34}
        btnClose = New Button() With {.Text = "閉じる", .Left = Me.ClientSize.Width - 110, .Top = y, .Width = 90, .Height = 34}
        Me.Controls.Add(btnSave)
        Me.Controls.Add(btnClose)

        AddHandler btnSave.Click, AddressOf btnSave_Click
        AddHandler btnClose.Click, Sub() Me.Close()

        Me.AcceptButton = btnSave
        Me.CancelButton = btnClose
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs)
        Dim code = (If(txtCode.Text, "")).Trim()
        Dim name = (If(txtName.Text, "")).Trim()

        If code = "" Then
            MessageBox.Show("仕入先コードを入力してください。")
            txtCode.Focus()
            Return
        End If
        If name = "" Then
            MessageBox.Show("仕入先名を入力してください。")
            txtName.Focus()
            Return
        End If

        Dim sql As String =
"INSERT INTO supplier_master
(supplier_code, supplier_name, tel, email, address1, address2, remark, is_active)
VALUES
(@code, @name, @tel, @email, @a1, @a2, @remark, 1);"

        Try
            Using conn As New MySqlConnection(_cs)
                conn.Open()
                Using cmd As New MySqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@code", code)
                    cmd.Parameters.AddWithValue("@name", name)
                    cmd.Parameters.AddWithValue("@tel", (If(txtTel.Text, "")).Trim())
                    cmd.Parameters.AddWithValue("@email", (If(txtEmail.Text, "")).Trim())
                    cmd.Parameters.AddWithValue("@a1", (If(txtAddr1.Text, "")).Trim())
                    cmd.Parameters.AddWithValue("@a2", (If(txtAddr2.Text, "")).Trim())
                    cmd.Parameters.AddWithValue("@remark", (If(txtRemark.Text, "")).Trim())
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            MessageBox.Show("登録しました。")
            Me.DialogResult = DialogResult.OK
            Me.Close()

        Catch ex As MySqlException
            If ex.Message IsNot Nothing AndAlso ex.Message.IndexOf("Duplicate", StringComparison.OrdinalIgnoreCase) >= 0 Then
                MessageBox.Show("仕入先コードが重複しています。別のコードにしてください。")
            Else
                MessageBox.Show("登録エラー: " & ex.Message)
            End If
        Catch ex As Exception
            MessageBox.Show("登録エラー: " & ex.Message)
        End Try
    End Sub

End Class