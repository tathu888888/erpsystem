Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports MySql.Data.MySqlClient
Imports System.Data

Partial Public Class menu

    ' ========= Theme colors =========
    Private ReadOnly cBg As Color = Color.FromArgb(245, 246, 250)
    Private ReadOnly cCard As Color = Color.White
    Private ReadOnly cPrimary As Color = Color.FromArgb(46, 98, 238)
    Private ReadOnly cPrimaryDark As Color = Color.FromArgb(36, 78, 200)
    Private ReadOnly cText As Color = Color.FromArgb(25, 28, 36)
    Private ReadOnly cSubText As Color = Color.FromArgb(120, 125, 140)
    Private ReadOnly cBorder As Color = Color.FromArgb(225, 228, 236)

    ' ========= layout-managed controls =========
    Private menuCard As RoundedPanel
    Private subtitleLabel As Label
    Private themeApplied As Boolean = False

    ' ========= Added buttons =========
    Private lotStockBtn As Button
    Private itemCreateBtn As Button
    Private supplierBtn As Button
    Private poBtn As Button
    Private poListBtn As Button   ' ★追加：発注書一覧

    Private invoiceBtn As Button
    Private receiptBtn As Button
    Private paymentBtn As Button



    ' ========= DB Connection =========
    Private ReadOnly connectionString As String =
        "Server=127.0.0.1;Port=3306;Database=sunstar;Uid=root;Pwd=1234;SslMode=Disabled;"

    ' ★重要：Designerより後で AutoScale を固定して上書きする
    Public Sub New()
        InitializeComponent()

        ' 1) DPIに追従してきれいに拡大縮小したい → Dpi
        Me.AutoScaleMode = AutoScaleMode.Dpi

        ' 2) 何があっても拡大縮小させたくない → None
        ' Me.AutoScaleMode = AutoScaleMode.None
    End Sub

    ' Loadでやると後から上書きされることがあるので、Shownでやる
    Private Sub menu_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        ApplyTheme()

        ' ===== ログイン必須チェック =====
        If Not AppSession2.IsLoggedIn Then
            MessageBox.Show("未ログインです。ログイン画面から起動してください。")
            Me.Close()
            Return
        End If


        If AppSession2.IsAdmin Then
            If invoiceBtn IsNot Nothing Then invoiceBtn.Visible = True
            If receiptBtn IsNot Nothing Then receiptBtn.Visible = True
            If paymentBtn IsNot Nothing Then paymentBtn.Visible = True
        Else
            If invoiceBtn IsNot Nothing Then invoiceBtn.Visible = True     ' ←閲覧OKなら True
            If receiptBtn IsNot Nothing Then receiptBtn.Visible = True
            If paymentBtn IsNot Nothing Then paymentBtn.Visible = True
        End If


        ' ===== 管理者/一般の出し分け =====
        ApplyRoleVisibility()

        LayoutMenu()
    End Sub

    Private Sub menu_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        LayoutMenu()
    End Sub

    ' =========================
    ' 権限で表示切り替え（生成はしない。Visibleだけ）
    ' =========================
    Private Sub ApplyRoleVisibility()
        If AppSession2.IsAdmin Then
            ' 管理者：全部表示
            shipBtn.Visible = True
            CustomerBtn.Visible = True
            CustomerlistBtn.Visible = True

            If lotStockBtn IsNot Nothing Then lotStockBtn.Visible = True
            If itemCreateBtn IsNot Nothing Then itemCreateBtn.Visible = True
            If supplierBtn IsNot Nothing Then supplierBtn.Visible = True
            If poBtn IsNot Nothing Then poBtn.Visible = True
            If poListBtn IsNot Nothing Then poListBtn.Visible = True

        Else
            ' 一般：閲覧OK / 作成NG（好みで変更）
            shipBtn.Visible = True
            CustomerBtn.Visible = True
            CustomerlistBtn.Visible = True

            If lotStockBtn IsNot Nothing Then lotStockBtn.Visible = True
            If itemCreateBtn IsNot Nothing Then itemCreateBtn.Visible = False

            ' ★一般ユーザーも閲覧OKにする例（不要なら False に）
            If supplierBtn IsNot Nothing Then supplierBtn.Visible = True
            If poBtn IsNot Nothing Then poBtn.Visible = True
        End If
    End Sub

    ' =========================
    ' Rounded path helper
    ' =========================
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
    ' RoundedPanel (card)
    ' =========================
    Private Class RoundedPanel
        Inherits Panel
        Public Property Radius As Integer = 22
        Public Property BorderColor As Color = Color.Gainsboro
        Public Property BorderWidth As Integer = 1
        Public Property FillColor As Color = Color.White
        Public Property Shadow As Boolean = True

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            MyBase.OnPaint(e)
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias

            Dim rect As Rectangle = Me.ClientRectangle
            rect.Inflate(-1, -1)

            ' shadow
            If Shadow Then
                Using shadowBrush As New SolidBrush(Color.FromArgb(22, 0, 0, 0))
                    Dim sRect As Rectangle = rect
                    sRect.Offset(0, 5)
                    sRect.Inflate(2, 2)
                    Using sp As GraphicsPath = CreateRoundPath(sRect, Radius)
                        e.Graphics.FillPath(shadowBrush, sp)
                    End Using
                End Using
            End If

            Using path As GraphicsPath = CreateRoundPath(rect, Radius)
                Using fill As New SolidBrush(FillColor)
                    e.Graphics.FillPath(fill, path)
                End Using
                Using pen As New Pen(BorderColor, BorderWidth)
                    e.Graphics.DrawPath(pen, path)
                End Using
            End Using
        End Sub

        Private Function CreateRoundPath(r As Rectangle, radius As Integer) As GraphicsPath
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

    ' =========================
    ' Button styles
    ' =========================
    Private Sub StylePrimary(btn As Button)
        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderSize = 0
        btn.BackColor = cPrimary
        btn.ForeColor = Color.White
        btn.Font = New Font("Yu Gothic UI", 12.5F, FontStyle.Bold)
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
        btn.Font = New Font("Yu Gothic UI", 12.0F, FontStyle.Bold)
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

    ' =========================
    ' ApplyTheme（見た目だけ。配置はしない）
    ' =========================
    Private Sub ApplyTheme()
        If themeApplied Then Return
        themeApplied = True

        Me.BackColor = cBg
        Me.FormBorderStyle = FormBorderStyle.None
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.DoubleBuffered = True
        Me.WindowState = FormWindowState.Maximized

        Label1.Text = "顧客管理システム"
        Label1.ForeColor = cText
        Label1.Font = New Font("Yu Gothic UI", 24.0F, FontStyle.Bold)
        Label1.AutoSize = True

        subtitleLabel = New Label() With {
            .Text = "操作を選択してください",
            .ForeColor = cSubText,
            .Font = New Font("Yu Gothic UI", 12.0F, FontStyle.Regular),
            .AutoSize = True
        }
        Me.Controls.Add(subtitleLabel)

        shutdownBtn.Text = "✕"
        shutdownBtn.FlatStyle = FlatStyle.Flat
        shutdownBtn.FlatAppearance.BorderSize = 0
        shutdownBtn.BackColor = Color.White
        shutdownBtn.ForeColor = cSubText
        shutdownBtn.Cursor = Cursors.Hand
        shutdownBtn.Size = New Size(44, 44)
        shutdownBtn.Anchor = AnchorStyles.Top Or AnchorStyles.Right

        Using gp = RoundRectPath(New Rectangle(0, 0, shutdownBtn.Width, shutdownBtn.Height), 22)
            shutdownBtn.Region = New Region(gp)
        End Using

        AddHandler shutdownBtn.MouseEnter, Sub()
                                               shutdownBtn.BackColor = Color.FromArgb(255, 235, 238)
                                               shutdownBtn.ForeColor = Color.FromArgb(198, 40, 40)
                                           End Sub
        AddHandler shutdownBtn.MouseLeave, Sub()
                                               shutdownBtn.BackColor = Color.White
                                               shutdownBtn.ForeColor = cSubText
                                           End Sub

        menuCard = New RoundedPanel() With {
            .Radius = 24,
            .BorderColor = cBorder,
            .BorderWidth = 1,
            .FillColor = cCard,
            .Shadow = True
        }
        Me.Controls.Add(menuCard)

        ' ボタンをカードへ移動（1回だけ）
        If shipBtn.Parent IsNot menuCard Then
            Me.Controls.Remove(shipBtn) : menuCard.Controls.Add(shipBtn)
        End If
        If CustomerBtn.Parent IsNot menuCard Then
            Me.Controls.Remove(CustomerBtn) : menuCard.Controls.Add(CustomerBtn)
        End If
        If CustomerlistBtn.Parent IsNot menuCard Then
            Me.Controls.Remove(CustomerlistBtn) : menuCard.Controls.Add(CustomerlistBtn)
        End If

        ' ★追加ボタン：ロット在庫一覧 / アイテムマスタ作成（Designer不要）
        If lotStockBtn Is Nothing Then
            lotStockBtn = New Button() With {.Name = "lotStockBtn", .Text = "ロット在庫一覧"}
            AddHandler lotStockBtn.Click, AddressOf lotStockBtn_Click
            menuCard.Controls.Add(lotStockBtn)
        End If

        If poListBtn Is Nothing Then
            poListBtn = New Button() With {.Name = "poListBtn", .Text = "発注書一覧"}
            AddHandler poListBtn.Click, AddressOf poListBtn_Click
            menuCard.Controls.Add(poListBtn)
        End If

        If itemCreateBtn Is Nothing Then
            itemCreateBtn = New Button() With {.Name = "itemCreateBtn", .Text = "アイテムマスタ作成"}
            AddHandler itemCreateBtn.Click, AddressOf itemCreateBtn_Click
            menuCard.Controls.Add(itemCreateBtn)
        End If

        ' ★追加：仕入先 / 発注書（Designer不要）
        If supplierBtn Is Nothing Then
            supplierBtn = New Button() With {.Name = "supplierBtn", .Text = "仕入先マスタ"}
            AddHandler supplierBtn.Click, AddressOf supplierBtn_Click
            menuCard.Controls.Add(supplierBtn)
        End If

        If poBtn Is Nothing Then
            poBtn = New Button() With {.Name = "poBtn", .Text = "発注書"}
            AddHandler poBtn.Click, AddressOf poBtn_Click
            menuCard.Controls.Add(poBtn)
        End If

        If invoiceBtn Is Nothing Then
            invoiceBtn = New Button() With {.Name = "invoiceBtn", .Text = "請求書（売掛/売上）"}
            AddHandler invoiceBtn.Click, AddressOf invoiceBtn_Click
            menuCard.Controls.Add(invoiceBtn)
        End If

        If receiptBtn Is Nothing Then
            receiptBtn = New Button() With {.Name = "receiptBtn", .Text = "入金（現金/売掛）"}
            AddHandler receiptBtn.Click, AddressOf receiptBtn_Click
            menuCard.Controls.Add(receiptBtn)
        End If

        If paymentBtn Is Nothing Then
            paymentBtn = New Button() With {.Name = "paymentBtn", .Text = "支払（買掛/現金）"}
            AddHandler paymentBtn.Click, AddressOf paymentBtn_Click
            menuCard.Controls.Add(paymentBtn)
        End If

        StyleGhost(invoiceBtn)
        StyleGhost(receiptBtn)
        StyleGhost(paymentBtn)


        shipBtn.Text = "日次処理（発送）"
        CustomerBtn.Text = "顧客登録"
        CustomerlistBtn.Text = "顧客マスタ一覧"

        StylePrimary(shipBtn)
        StyleGhost(CustomerBtn)
        StyleGhost(CustomerlistBtn)

        StyleGhost(lotStockBtn)
        StyleGhost(itemCreateBtn)
        StyleGhost(supplierBtn)
        StyleGhost(poBtn)
        StyleGhost(poListBtn) ' ★追加

    End Sub

    ' =========================
    ' LayoutMenu（ここで毎回配置）
    ' =========================
    ' =========================
    ' LayoutMenu（ここで毎回配置）
    ' =========================
    ' =========================
    ' LayoutMenu（ここで毎回配置）
    ' =========================
    Private Sub LayoutMenu()
        If Not themeApplied OrElse menuCard Is Nothing Then Return
        If Me.ClientSize.Width <= 0 OrElse Me.ClientSize.Height <= 0 Then Return

        ' 全体を少し小さく
        Dim scale As Single = (Me.DeviceDpi / 96.0F) * 0.8F

        Dim margin As Integer = CInt(40 * scale)
        Dim topY As Integer = CInt(30 * scale)

        Label1.Location = New Point(margin, topY)
        subtitleLabel.Location = New Point(margin, Label1.Bottom + CInt(8 * scale))
        shutdownBtn.Location = New Point(Me.ClientSize.Width - shutdownBtn.Width - margin, topY)

        Dim cardW As Integer = Math.Min(Me.ClientSize.Width - margin * 2, CInt(680 * scale))
        Dim padLR As Integer = CInt(40 * scale)
        Dim padTop As Integer = CInt(34 * scale)
        Dim padBottom As Integer = CInt(34 * scale)

        ' ★ボタンを少し小さく
        Dim btnH As Integer = CInt(48 * scale)   ' 58→48
        Dim gap As Integer = CInt(10 * scale)    ' 16→10
        Dim btnW As Integer = cardW - padLR * 2

        ' ===== 表示されるボタンだけを並べる =====
        Dim buttons As New List(Of Button)()
        If shipBtn.Visible Then buttons.Add(shipBtn)
        If CustomerBtn.Visible Then buttons.Add(CustomerBtn)
        If CustomerlistBtn.Visible Then buttons.Add(CustomerlistBtn)
        If lotStockBtn IsNot Nothing AndAlso lotStockBtn.Visible Then buttons.Add(lotStockBtn)
        If itemCreateBtn IsNot Nothing AndAlso itemCreateBtn.Visible Then buttons.Add(itemCreateBtn)
        If supplierBtn IsNot Nothing AndAlso supplierBtn.Visible Then buttons.Add(supplierBtn)
        If poBtn IsNot Nothing AndAlso poBtn.Visible Then buttons.Add(poBtn)
        If poListBtn IsNot Nothing AndAlso poListBtn.Visible Then buttons.Add(poListBtn)
        If invoiceBtn IsNot Nothing AndAlso invoiceBtn.Visible Then buttons.Add(invoiceBtn)
        If receiptBtn IsNot Nothing AndAlso receiptBtn.Visible Then buttons.Add(receiptBtn)
        If paymentBtn IsNot Nothing AndAlso paymentBtn.Visible Then buttons.Add(paymentBtn)

        If buttons.Count = 0 Then buttons.Add(shipBtn)

        ' ===== 高さ計算（画面からはみ出さないよう制限）=====
        Dim calculatedH As Integer =
        padTop + (btnH * buttons.Count) + (gap * Math.Max(0, buttons.Count - 1)) + padBottom

        Dim maxH As Integer =
        Me.ClientSize.Height - (subtitleLabel.Bottom + CInt(60 * scale))

        Dim cardH As Integer = Math.Min(calculatedH, maxH)

        ' ★はみ出す場合はスクロール
        menuCard.AutoScroll = (calculatedH > maxH)

        menuCard.Size = New Size(cardW, cardH)
        menuCard.Location = New Point((Me.ClientSize.Width - menuCard.Width) \ 2,
                                  subtitleLabel.Bottom + CInt(26 * scale))

        Dim bx As Integer = padLR
        Dim y As Integer = padTop
        For Each b In buttons
            b.Size = New Size(btnW, btnH)
            b.Location = New Point(bx, y)
            b.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
            y += btnH + gap
        Next

        menuCard.BringToFront()
        shutdownBtn.BringToFront()
        Label1.BringToFront()
        subtitleLabel.BringToFront()
    End Sub
    ' =========================
    ' Shutdown click
    ' =========================
    Private Sub shutdownBtn_Click(sender As Object, e As EventArgs) Handles shutdownBtn.Click
        Dim result = MessageBox.Show("終了しますか？", "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Question)
        If result = DialogResult.OK Then
            Application.Exit()
        End If
    End Sub

    ' =========================
    ' Menu Buttons（既存）
    ' =========================
    Private Sub shipBtn_Click(sender As Object, e As EventArgs) Handles shipBtn.Click
        Using f As New ship
            Me.Hide()
            f.ShowDialog()
            Me.Show()
        End Using
    End Sub

    Private Sub CustomerBtn_Click(sender As Object, e As EventArgs) Handles CustomerBtn.Click
        If Not AppSession2.IsAdmin Then
            MessageBox.Show("権限がありません。")
            Return
        End If

        Using f As New Customer
            Me.Hide()
            f.ShowDialog()
            Me.Show()
        End Using
    End Sub

    Private Sub CustomerlistBtn_Click(sender As Object, e As EventArgs) Handles CustomerlistBtn.Click
        If Not AppSession2.IsAdmin Then
            MessageBox.Show("権限がありません。")
            Return
        End If

        Using f As New Customerlist
            Me.Hide()
            f.ShowDialog()
            Me.Show()
        End Using
    End Sub

    ' =========================
    ' ★追加：仕入先 / 発注書
    ' =========================
    Private Sub supplierBtn_Click(sender As Object, e As EventArgs)
        Try
            Using f As New SupplierCreateForm(connectionString) ' ←あなたのフォーム名に変更
                Me.Hide()
                f.ShowDialog(Me)
                Me.Show()
            End Using
        Catch ex As Exception
            MessageBox.Show($"仕入先画面エラー: {ex.Message}")
        End Try
    End Sub

    Private Sub poBtn_Click(sender As Object, e As EventArgs)
        Try
            Using f As New PurchaseOrderForm(connectionString) ' ←あなたのフォーム名に変更
                Me.Hide()
                f.ShowDialog(Me)
                Me.Show()
            End Using
        Catch ex As Exception
            MessageBox.Show($"発注書画面エラー: {ex.Message}")
        End Try
    End Sub

    ' =========================
    ' ★追加：ロット在庫一覧 / アイテムマスタ作成
    ' =========================
    Private Sub lotStockBtn_Click(sender As Object, e As EventArgs)
        Try
            Dim itemId As Integer = 0
            Dim itemName As String = ""
            Dim conv As Integer = 1

            If Not PickItemFromMaster(connectionString, itemId, itemName, conv) Then Return

            Using f As New LotStockListForm(connectionString, itemId, itemName, conv)
                Me.Hide()
                f.ShowDialog(Me)
                Me.Show()
            End Using

        Catch ex As Exception
            MessageBox.Show($"ロット在庫一覧エラー: {ex.Message}")
        End Try
    End Sub

    Private Sub poListBtn_Click(sender As Object, e As EventArgs)
        Try
            Using f As New PurchaseOrderListForm(connectionString) ' ★一覧フォーム
                Me.Hide()
                f.ShowDialog(Me)
                Me.Show()
            End Using
        Catch ex As Exception
            MessageBox.Show($"発注書一覧エラー: {ex.Message}")
        End Try
    End Sub


    Private Sub itemCreateBtn_Click(sender As Object, e As EventArgs)
        If Not AppSession2.IsAdmin Then
            MessageBox.Show("権限がありません。")
            Return
        End If

        Using f As New ItemMasterCreateForm(connectionString)
            Me.Hide()
            f.ShowDialog(Me)
            Me.Show()
        End Using
    End Sub


    Private Sub invoiceBtn_Click(sender As Object, e As EventArgs)
        Try
            Using f As New InvoiceForm(connectionString)   ' ←あなたの InvoiceForm のコンストラクタに合わせる
                Me.Hide()
                f.ShowDialog(Me)
                Me.Show()
            End Using
        Catch ex As Exception
            MessageBox.Show($"請求書画面エラー: {ex.Message}")
        End Try
    End Sub

    Private Sub receiptBtn_Click(sender As Object, e As EventArgs)
        Try
            Using f As New ReceiptForm(connectionString)   ' ←ReceiptForm のコンストラクタに合わせる
                Me.Hide()
                f.ShowDialog(Me)
                Me.Show()
            End Using
        Catch ex As Exception
            MessageBox.Show($"入金画面エラー: {ex.Message}")
        End Try
    End Sub

    Private Sub paymentBtn_Click(sender As Object, e As EventArgs)
        Try
            Using f As New PaymentForm(connectionString)   ' ←PaymentForm のコンストラクタに合わせる
                Me.Hide()
                f.ShowDialog(Me)
                Me.Show()
            End Using
        Catch ex As Exception
            MessageBox.Show($"支払画面エラー: {ex.Message}")
        End Try
    End Sub


    ' =========================
    ' item_master から選択（簡易ComboBoxダイアログ）
    ' =========================
    Private Function PickItemFromMaster(cs As String, ByRef itemId As Integer, ByRef itemName As String, ByRef conv As Integer) As Boolean
        Dim dt As New DataTable()

        Dim sql As String =
"SELECT item_id, item_name, COALESCE(conversion_qty,1) AS conversion_qty
 FROM item_master
 WHERE is_active=1
 ORDER BY item_name;"

        Using conn As New MySqlConnection(cs)
            conn.Open()
            Using da As New MySqlDataAdapter(sql, conn)
                da.Fill(dt)
            End Using
        End Using

        If dt.Rows.Count = 0 Then
            MessageBox.Show("アイテムマスタが空です。先にアイテムを登録してください。")
            Return False
        End If

        ' ★ByRefに直接触らないための一時変数
        Dim pickedId As Integer = 0
        Dim pickedName As String = ""
        Dim pickedConv As Integer = 1

        Using dlg As New Form()
            dlg.Text = "アイテム選択"
            dlg.StartPosition = FormStartPosition.CenterParent
            dlg.Size = New Size(520, 180)
            dlg.MinimizeBox = False
            dlg.MaximizeBox = False
            dlg.FormBorderStyle = FormBorderStyle.FixedDialog

            Dim cb As New ComboBox() With {
                .Left = 16, .Top = 18, .Width = 470,
                .DropDownStyle = ComboBoxStyle.DropDownList
            }
            cb.DataSource = dt
            cb.ValueMember = "item_id"
            cb.DisplayMember = "item_name"

            Dim btnOk As New Button() With {.Text = "OK", .Left = 286, .Top = 70, .Width = 95, .Height = 34}
            Dim btnCancel As New Button() With {.Text = "キャンセル", .Left = 391, .Top = 70, .Width = 95, .Height = 34}

            AddHandler btnOk.Click,
                Sub()
                    Dim row As DataRowView = TryCast(cb.SelectedItem, DataRowView)
                    If row Is Nothing Then Return

                    ' ★ラムダ内は ByRef じゃなくローカルへ
                    pickedId = Convert.ToInt32(row("item_id"))
                    pickedName = row("item_name").ToString()
                    pickedConv = Math.Max(1, Convert.ToInt32(row("conversion_qty")))

                    dlg.DialogResult = DialogResult.OK
                    dlg.Close()
                End Sub

            AddHandler btnCancel.Click,
                Sub()
                    dlg.DialogResult = DialogResult.Cancel
                    dlg.Close()
                End Sub

            dlg.Controls.Add(cb)
            dlg.Controls.Add(btnOk)
            dlg.Controls.Add(btnCancel)

            dlg.AcceptButton = btnOk
            dlg.CancelButton = btnCancel

            Dim res As DialogResult = dlg.ShowDialog(Me)
            If res <> DialogResult.OK Then
                Return False
            End If
        End Using

        ' ★ここで初めて ByRef に反映（ラムダ外）
        itemId = pickedId
        itemName = pickedName
        conv = pickedConv
        Return True
    End Function

End Class
