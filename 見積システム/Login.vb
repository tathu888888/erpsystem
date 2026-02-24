Imports MySql.Data.MySqlClient
Imports System.Security.Cryptography
Imports System.Text
Imports System.Drawing
Imports System.Windows.Forms
Imports System.Linq

Public Class Login

    Private ReadOnly connectionString As String =
        "Server=127.0.0.1;Port=3306;Database=sunstar;Uid=root;Pwd=1234;"

    ' ★ここは実ファイルに合わせて修正（例）
    Private Const BG_IMAGE_PATH As String =
        "C:\Users\kensyu\OneDrive\Desktop\通信販売業者の顧客管理システム\通信販売業者の顧客管理システム\見積システム\image\login_bg.png.png"

    Private pCard As Panel
    Private tlp As TableLayoutPanel

    ' 位置の強制センタリングを「初回だけ」行うため
    Private _didInitialCenter As Boolean = False

    Private Sub Login_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' ===== フォーム基本 =====
        Me.Text = "Login"
        Me.ClientSize = New Size(980, 640)
        Me.DoubleBuffered = True

        ' DPIずれ対策（環境によっては右下寄りになるのを抑える）
        Me.AutoScaleMode = AutoScaleMode.Dpi

        ' ★ CenterScreenは信用しない（Manualで固定）
        Me.StartPosition = FormStartPosition.Manual
        Me.Location = New Point(0, 0) ' 変な保存位置を無効化

        ApplyBackground()
        BuildCenterCard()

        ' Load時点でも中央にする（ただしShownで再度確定）
        ForceCenterForm()
        CenterCard()
    End Sub

    ' ★ここが最重要：表示された後にもう一度中央へ寄せる
    Private Sub Login_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        If _didInitialCenter Then Return
        _didInitialCenter = True

        ForceCenterForm()
        CenterCard()
    End Sub

    Private Sub Login_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        CenterCard()
    End Sub

    ' =========================
    ' 右下に開く問題の最終対策
    ' =========================
    Private Sub ForceCenterForm()
        ' どのモニタに出すか：基本はPrimary
        Dim wa As Rectangle = Screen.PrimaryScreen.WorkingArea

        ' フォームがすでに別モニタに出ている/出そうな場合はそのモニタ基準にする
        Dim targetScreen = Screen.FromPoint(New Point(Math.Max(Me.Left, 0), Math.Max(Me.Top, 0)))
        If targetScreen IsNot Nothing Then
            wa = targetScreen.WorkingArea
        End If

        Dim x As Integer = wa.Left + (wa.Width - Me.Width) \ 2
        Dim y As Integer = wa.Top + (wa.Height - Me.Height) \ 2

        ' 念のため画面外に行かないようクランプ
        If x < wa.Left Then x = wa.Left
        If y < wa.Top Then y = wa.Top

        Me.Location = New Point(x, y)
    End Sub

    ' =========================
    ' 背景画像
    ' =========================
    Private Sub ApplyBackground()
        Dim candidates As New List(Of String) From {
            BG_IMAGE_PATH,
            IO.Path.Combine(Application.StartupPath, "image", "login_bg.png"),
            IO.Path.Combine(Application.StartupPath, "images", "login_bg.png"),
            IO.Path.Combine(Application.StartupPath, "login_bg.png"),
            IO.Path.Combine(Environment.CurrentDirectory, "image", "login_bg.png"),
            IO.Path.Combine(Environment.CurrentDirectory, "images", "login_bg.png"),
            IO.Path.Combine(Environment.CurrentDirectory, "login_bg.png")
        }

        Dim found As String = candidates.FirstOrDefault(
            Function(p) Not String.IsNullOrWhiteSpace(p) AndAlso IO.File.Exists(p)
        )

        If String.IsNullOrEmpty(found) Then
            Me.BackgroundImage = Nothing
            Me.BackColor = Color.FromArgb(230, 235, 245)
            Return
        End If

        Try
            ' ファイルロック回避
            Dim bytes = IO.File.ReadAllBytes(found)
            Using ms As New IO.MemoryStream(bytes)
                Me.BackgroundImage = Image.FromStream(ms)
            End Using
            Me.BackgroundImageLayout = ImageLayout.Stretch
        Catch
            Me.BackgroundImage = Nothing
            Me.BackColor = Color.FromArgb(230, 235, 245)
        End Try
    End Sub

    ' =========================
    ' 中央カードUI
    ' =========================
    Private Sub BuildCenterCard()
        If pCard IsNot Nothing Then Return

        pCard = New Panel() With {
            .Name = "pCard",
            .Size = New Size(560, 320),
            .BackColor = Color.FromArgb(210, 225, 230, 236),
            .Anchor = AnchorStyles.None
        }
        Me.Controls.Add(pCard)
        pCard.BringToFront()

        tlp = New TableLayoutPanel() With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 8,
            .Padding = New Padding(44, 34, 44, 34),
            .BackColor = Color.Transparent
        }

        tlp.RowStyles.Add(New RowStyle(SizeType.Absolute, 46))
        tlp.RowStyles.Add(New RowStyle(SizeType.Absolute, 10))
        tlp.RowStyles.Add(New RowStyle(SizeType.Absolute, 18))
        tlp.RowStyles.Add(New RowStyle(SizeType.Absolute, 46))
        tlp.RowStyles.Add(New RowStyle(SizeType.Absolute, 18))
        tlp.RowStyles.Add(New RowStyle(SizeType.Absolute, 46))
        tlp.RowStyles.Add(New RowStyle(SizeType.Absolute, 18))
        tlp.RowStyles.Add(New RowStyle(SizeType.Absolute, 52))

        pCard.Controls.Add(tlp)

        Dim lblTitle As New Label() With {
            .Text = "LOGIN",
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleCenter,
            .Font = New Font("Yu Gothic UI", 18.0F, FontStyle.Bold),
            .ForeColor = Color.FromArgb(35, 35, 35)
        }
        tlp.Controls.Add(lblTitle, 0, 0)

        Dim lblUser As New Label() With {
            .Text = "ユーザー名",
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.BottomLeft,
            .Font = New Font("Yu Gothic UI", 10.0F, FontStyle.Bold),
            .ForeColor = Color.FromArgb(60, 60, 60)
        }
        tlp.Controls.Add(lblUser, 0, 2)

        ' ★ DesignerにあるtxtUserをカードへ移動（前提）
        PrepareTextBox(txtUser)
        txtUser.Parent = Nothing
        tlp.Controls.Add(txtUser, 0, 3)

        Dim lblPass As New Label() With {
            .Text = "パスワード",
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.BottomLeft,
            .Font = New Font("Yu Gothic UI", 10.0F, FontStyle.Bold),
            .ForeColor = Color.FromArgb(60, 60, 60)
        }
        tlp.Controls.Add(lblPass, 0, 4)

        PrepareTextBox(txtPass)
        txtPass.UseSystemPasswordChar = True
        txtPass.Parent = Nothing
        tlp.Controls.Add(txtPass, 0, 5)

        Dim btnRow As New FlowLayoutPanel() With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.RightToLeft,
            .WrapContents = False,
            .BackColor = Color.Transparent,
            .Padding = New Padding(0, 6, 0, 0)
        }

        PrepareButton(btnLogin, "ログイン")
        PrepareButton(btnCancel, "キャンセル")

        btnLogin.Parent = Nothing
        btnCancel.Parent = Nothing

        btnRow.Controls.Add(btnLogin)
        btnRow.Controls.Add(btnCancel)
        tlp.Controls.Add(btnRow, 0, 7)

        pCard.BringToFront()
    End Sub

    Private Sub PrepareTextBox(tb As TextBox)
        tb.BorderStyle = BorderStyle.FixedSingle
        tb.Font = New Font("Yu Gothic UI", 12.0F, FontStyle.Regular)
        tb.Dock = DockStyle.Fill
        tb.Margin = New Padding(0)
        tb.BackColor = Color.White
        tb.ForeColor = Color.FromArgb(30, 30, 30)
    End Sub

    Private Sub PrepareButton(btn As Button, text As String)
        btn.Text = text
        btn.Font = New Font("Yu Gothic UI", 11.0F, FontStyle.Bold)
        btn.Size = New Size(140, 40)
        btn.Margin = New Padding(10, 0, 0, 0)
        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderSize = 0
        btn.BackColor = Color.FromArgb(90, 110, 140)
        btn.ForeColor = Color.White
    End Sub

    Private Sub CenterCard()
        If pCard Is Nothing Then Return
        pCard.Left = (Me.ClientSize.Width - pCard.Width) \ 2
        pCard.Top = (Me.ClientSize.Height - pCard.Height) \ 2
        pCard.BringToFront()
    End Sub

    ' =========================
    ' ログイン処理
    ' =========================
    Private Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
        Dim u = txtUser.Text.Trim()
        Dim p = txtPass.Text

        If u = "" OrElse p = "" Then
            MessageBox.Show("ユーザー名とパスワードを入力してください。")
            Return
        End If

        Dim pHash As String = Sha256Hex(p)

        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()

                Dim sql As String =
                    "SELECT user_id, username, role " &
                    "FROM users " &
                    "WHERE username=@u AND password_hash=@p AND is_active=1 " &
                    "LIMIT 1"

                Using cmd As New MySqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@u", u)
                    cmd.Parameters.AddWithValue("@p", pHash)

                    Using r = cmd.ExecuteReader()
                        If r.Read() Then
                            AppSession2.IsLoggedIn = True
                            AppSession2.UserId = Convert.ToInt32(r("user_id"))
                            AppSession2.Username = r("username").ToString()
                            AppSession2.Role = r("role").ToString()

                            Me.Hide()
                            Using m As New menu()
                                m.ShowDialog()
                            End Using
                            Me.Close()
                            Return
                        End If
                    End Using
                End Using
            End Using

            MessageBox.Show("ログインに失敗しました。")

        Catch ex As Exception
            MessageBox.Show("ログイン処理でエラー: " & ex.ToString())
        End Try
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        AppSession.Logout()
        Me.Close()
    End Sub

    Private Function Sha256Hex(input As String) As String
        Using sha = SHA256.Create()
            Dim bytes = Encoding.UTF8.GetBytes(input)
            Dim hash = sha.ComputeHash(bytes)
            Dim sb As New StringBuilder()
            For Each b In hash
                sb.Append(b.ToString("x2"))
            Next
            Return sb.ToString()
        End Using
    End Function

End Class
