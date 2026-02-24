<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Login
    Inherits System.Windows.Forms.Form

    'フォームがコンポーネントの一覧をクリーンアップするために dispose をオーバーライドします。
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows フォーム デザイナーで必要です。
    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.LabelUser = New System.Windows.Forms.Label()
        Me.LabelPass = New System.Windows.Forms.Label()
        Me.txtUser = New System.Windows.Forms.TextBox()
        Me.txtPass = New System.Windows.Forms.TextBox()
        Me.btnLogin = New System.Windows.Forms.Button()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'LabelUser
        '
        Me.LabelUser.AutoSize = True
        Me.LabelUser.Font = New System.Drawing.Font("Yu Gothic UI", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.LabelUser.Location = New System.Drawing.Point(36, 34)
        Me.LabelUser.Name = "LabelUser"
        Me.LabelUser.Size = New System.Drawing.Size(96, 32)
        Me.LabelUser.TabIndex = 0
        Me.LabelUser.Text = "ユーザー"
        '
        'LabelPass
        '
        Me.LabelPass.AutoSize = True
        Me.LabelPass.Font = New System.Drawing.Font("Yu Gothic UI", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.LabelPass.Location = New System.Drawing.Point(36, 112)
        Me.LabelPass.Name = "LabelPass"
        Me.LabelPass.Size = New System.Drawing.Size(115, 32)
        Me.LabelPass.TabIndex = 1
        Me.LabelPass.Text = "パスワード"
        '
        'txtUser
        '
        Me.txtUser.Font = New System.Drawing.Font("Yu Gothic UI", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.txtUser.Location = New System.Drawing.Point(170, 30)
        Me.txtUser.Name = "txtUser"
        Me.txtUser.Size = New System.Drawing.Size(260, 39)
        Me.txtUser.TabIndex = 2
        '
        'txtPass
        '
        Me.txtPass.Font = New System.Drawing.Font("Yu Gothic UI", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.txtPass.Location = New System.Drawing.Point(170, 108)
        Me.txtPass.Name = "txtPass"
        Me.txtPass.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42) '*
        Me.txtPass.Size = New System.Drawing.Size(260, 39)
        Me.txtPass.TabIndex = 3
        '
        'btnLogin
        '
        Me.btnLogin.Font = New System.Drawing.Font("Yu Gothic UI", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.btnLogin.Location = New System.Drawing.Point(170, 178)
        Me.btnLogin.Name = "btnLogin"
        Me.btnLogin.Size = New System.Drawing.Size(126, 46)
        Me.btnLogin.TabIndex = 4
        Me.btnLogin.Text = "ログイン"
        Me.btnLogin.UseVisualStyleBackColor = True
        '
        'btnCancel
        '
        Me.btnCancel.Font = New System.Drawing.Font("Yu Gothic UI", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.btnCancel.Location = New System.Drawing.Point(304, 178)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New System.Drawing.Size(126, 46)
        Me.btnCancel.TabIndex = 5
        Me.btnCancel.Text = "キャンセル"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'Login
        '
        Me.AcceptButton = Me.btnLogin
        Me.CancelButton = Me.btnCancel
        Me.AutoScaleDimensions = New System.Drawing.SizeF(10.0!, 25.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(480, 260)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnLogin)
        Me.Controls.Add(Me.txtPass)
        Me.Controls.Add(Me.txtUser)
        Me.Controls.Add(Me.LabelPass)
        Me.Controls.Add(Me.LabelUser)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "Login"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Login"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents LabelUser As Label
    Friend WithEvents LabelPass As Label
    Friend WithEvents txtUser As TextBox
    Friend WithEvents txtPass As TextBox
    Friend WithEvents btnLogin As Button
    Friend WithEvents btnCancel As Button
End Class
