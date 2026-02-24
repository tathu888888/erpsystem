Imports System.Windows.Forms
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports MySql.Data.MySqlClient
Imports System.Globalization
Imports System.Collections.Generic
Imports System.Text.RegularExpressions

Public Class Customerlistdetail
    Inherits Form

    ' =========================
    ' DB接続
    ' =========================
    Private ReadOnly connectionString As String =
        "Server=127.0.0.1;Port=3306;Database=sunstar;Uid=root;Pwd=1234;SslMode=Disabled;"

    ' =========================
    ' Controls
    ' =========================
    Private root As TableLayoutPanel ' 3行：Header / Scroll / Footer

    Private headerPanel As Panel
    Private titleLabel As Label
    Private subtitleLabel As Label
    Private closeTopBtn As Button

    Private scrollPanel As Panel
    Private footerHost As Panel

    Private cardPanel As RoundedPanel2
    Private fieldsTable As TableLayoutPanel
    Private statusLabel As Label

    Private footerPanel As RoundedPanel2
    Private btnSave As Button
    Private btnClose As Button
    Private btnCancelSub As Button
    Private btnResumeSub As Button

    Private txtCustomerCode As TextBox
    Private txtCustomerName As TextBox
    Private txtPhoneNumber As TextBox
    Private txtAddress As TextBox
    Private txtApartmentName As TextBox
    Private txtBirthDate As TextBox
    Private txtPaymentMethod As TextBox
    Private txtPurchaseStart As TextBox
    Private txtPurchaseReason As TextBox
    Private txtSubscriptionStatus As TextBox

    Private err As ErrorProvider

    ' =========================
    ' Fonts / Colors
    ' =========================
    Private ReadOnly fontTitle As New Font("Yu Gothic UI", 16.0F, FontStyle.Bold)
    Private ReadOnly fontSub As New Font("Yu Gothic UI", 9.5F, FontStyle.Regular)
    Private ReadOnly fontLabel As New Font("Yu Gothic UI", 10.0F, FontStyle.Regular)
    Private ReadOnly fontInput As New Font("Yu Gothic UI", 11.0F, FontStyle.Regular)

    Private ReadOnly cBg As Color = Color.FromArgb(245, 247, 250)
    Private ReadOnly cHeader As Color = Color.FromArgb(17, 24, 39)
    Private ReadOnly cCard As Color = Color.White
    Private ReadOnly cBorder As Color = Color.FromArgb(225, 231, 239)
    Private ReadOnly cText As Color = Color.FromArgb(17, 24, 39)
    Private ReadOnly cSubText As Color = Color.FromArgb(107, 114, 128)
    Private ReadOnly cPrimary As Color = Color.FromArgb(59, 130, 246)
    Private ReadOnly cDanger As Color = Color.FromArgb(239, 68, 68)
    Private ReadOnly cSuccess As Color = Color.FromArgb(16, 185, 129)

    ' =========================
    ' Public Properties（受け取り口）
    ' =========================
    Public Property CustomerCode As String
        Get
            Return If(txtCustomerCode Is Nothing, "", txtCustomerCode.Text)
        End Get
        Set(value As String)
            If txtCustomerCode IsNot Nothing Then txtCustomerCode.Text = If(value, "")
        End Set
    End Property

    ' =========================
    ' Constructor
    ' =========================
    Public Sub New()
        BuildUI()
        AddHandler Me.Shown, AddressOf OnShownLoad
    End Sub

    ' Ctrl+Enter で保存 / Esc で閉じる
    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If keyData = (Keys.Control Or Keys.Enter) Then
            SaveCustomer(Me, EventArgs.Empty)
            Return True
        End If
        If keyData = Keys.Escape Then
            Me.Close()
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

    Private Sub OnShownLoad(sender As Object, e As EventArgs)
        Dim code As String = Nz(If(txtCustomerCode?.Text, ""))
        If code <> "" Then LoadCustomerByCode(code)
        AdjustCardWidth(Nothing, EventArgs.Empty)
    End Sub

    ' =========================
    ' UI（スクロール＋フッター固定）
    ' =========================
    Private Sub BuildUI()
        ' Form
        Me.Text = "顧客詳細"
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.WindowState = FormWindowState.Maximized

        Me.BackColor = cBg
        Me.ClientSize = New Size(980, 640)
        Me.MinimumSize = New Size(900, 600)
        Me.DoubleBuffered = True
        Me.Font = fontInput
        Me.AutoScaleMode = AutoScaleMode.Dpi

        ' ErrorProvider
        err = New ErrorProvider() With {
            .BlinkStyle = ErrorBlinkStyle.NeverBlink,
            .ContainerControl = Me
        }

        ' ===== root（Header / Scroll / Footer）=====
        root = New TableLayoutPanel() With {
            .Dock = DockStyle.Fill,
            .BackColor = cBg,
            .ColumnCount = 1,
            .RowCount = 3
        }
        root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 84.0F))    ' Header固定
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))   ' Scroll伸びる
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 140.0F))  ' Footer固定
        Me.Controls.Add(root)

        ' ===== Header（row 0）=====
        headerPanel = New Panel() With {.Dock = DockStyle.Fill, .BackColor = cHeader}
        root.Controls.Add(headerPanel, 0, 0)

        titleLabel = New Label() With {
            .AutoSize = True,
            .Text = "顧客 詳細",
            .ForeColor = Color.White,
            .Font = fontTitle,
            .Location = New Point(24, 18)
        }
        headerPanel.Controls.Add(titleLabel)

        subtitleLabel = New Label() With {
            .AutoSize = True,
            .Text = "customer_master を編集します（顧客コードは編集不可） / Ctrl+Enterで保存",
            .ForeColor = Color.FromArgb(203, 213, 225),
            .Font = fontSub,
            .Location = New Point(26, 52)
        }
        headerPanel.Controls.Add(subtitleLabel)

        closeTopBtn = New Button() With {
            .Text = "×",
            .Font = New Font("Yu Gothic UI", 14.0F, FontStyle.Bold),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(31, 41, 55),
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(42, 38),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Right
        }
        closeTopBtn.FlatAppearance.BorderSize = 0
        AddHandler closeTopBtn.Click, Sub() Me.Close()
        headerPanel.Controls.Add(closeTopBtn)

        AddHandler headerPanel.Resize,
            Sub()
                closeTopBtn.Location = New Point(headerPanel.ClientSize.Width - closeTopBtn.Width - 18, 22)
            End Sub

        ' ===== Scroll（row 1）=====
        scrollPanel = New Panel() With {
            .Dock = DockStyle.Fill,
            .AutoScroll = True,
            .BackColor = cBg,
            .Padding = New Padding(22, 18, 22, 18)
        }
        root.Controls.Add(scrollPanel, 0, 1)

        ' ===== Card（スクロール内）=====
        cardPanel = New RoundedPanel2() With {
            .BackColor = cCard,
            .BorderColor = cBorder,
            .BorderWidth = 1,
            .CornerRadius = 18,
            .ShadowColor = Color.FromArgb(30, 0, 0, 0),
            .ShadowOffset = New Point(0, 6),
            .ShadowBlur = 18,
            .Padding = New Padding(22),
            .Dock = DockStyle.Top,
            .AutoSize = True,
            .AutoSizeMode = AutoSizeMode.GrowAndShrink
        }
        scrollPanel.Controls.Add(cardPanel)

        Dim sectionPanel As New Panel() With {.Dock = DockStyle.Top, .Height = 30}
        Dim sectionLabel As New Label() With {
            .AutoSize = True,
            .Text = "基本情報",
            .Font = New Font("Yu Gothic UI", 12.0F, FontStyle.Bold),
            .ForeColor = cText,
            .Location = New Point(0, 2)
        }
        sectionPanel.Controls.Add(sectionLabel)

        Dim hintLabel As New Label() With {
            .AutoSize = True,
            .Text = "日付は yyyy-MM-dd（空欄可）",
            .Font = fontSub,
            .ForeColor = cSubText,
            .Location = New Point(92, 6)
        }
        sectionPanel.Controls.Add(hintLabel)
        cardPanel.Controls.Add(sectionPanel)

        fieldsTable = New TableLayoutPanel() With {
            .Dock = DockStyle.Top,
            .AutoSize = True,
            .AutoSizeMode = AutoSizeMode.GrowAndShrink,
            .ColumnCount = 2,
            .RowCount = 0,
            .Margin = New Padding(0, 8, 0, 0)
        }
        fieldsTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        fieldsTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        cardPanel.Controls.Add(fieldsTable)

        statusLabel = New Label() With {
            .AutoSize = True,
            .Text = "",
            .ForeColor = cSubText,
            .Font = fontSub,
            .Dock = DockStyle.Top,
            .Margin = New Padding(0, 10, 0, 0)
        }
        cardPanel.Controls.Add(statusLabel)

        ' TextBox
        txtCustomerCode = CreateTextBox(readOnlyBox:=True)
        txtCustomerName = CreateTextBox()
        txtPhoneNumber = CreateTextBox()
        txtAddress = CreateTextBox()
        txtApartmentName = CreateTextBox()
        txtBirthDate = CreateTextBox(placeholder:="例: 1990-01-31")
        txtPaymentMethod = CreateTextBox(placeholder:="例: VISA / MASTER / JCB / AMEX")
        txtPurchaseStart = CreateTextBox(placeholder:="例: 2024-04-01")
        txtPurchaseReason = CreateTextBox(multiline:=True, height:=86, placeholder:="購入目的メモ（自由入力）")
        txtPurchaseReason.ScrollBars = ScrollBars.Vertical
        txtSubscriptionStatus = CreateTextBox(readOnlyBox:=True)

        AddRow2("顧客コード", txtCustomerCode, "顧客名", txtCustomerName)
        AddRow2("電話番号", txtPhoneNumber, "住所", txtAddress)
        AddRow2("アパート名など", txtApartmentName, "生年月日", txtBirthDate)
        AddRow2("支払方法", txtPaymentMethod, "購入開始日", txtPurchaseStart)
        AddWide("購入目的", txtPurchaseReason)
        AddWide("定期状況", txtSubscriptionStatus)

        ' 幅追従
        AddHandler scrollPanel.Resize, AddressOf AdjustCardWidth

        ' ===== FooterHost（row 2）※ここが重要：row=2にだけ入れる =====
        footerHost = New Panel() With {
            .Dock = DockStyle.Fill,
            .BackColor = cBg,
            .Padding = New Padding(22, 0, 22, 18)
        }
        root.Controls.Add(footerHost, 0, 2)

        footerPanel = New RoundedPanel2() With {
            .BackColor = cCard,
            .BorderColor = cBorder,
            .BorderWidth = 1,
            .CornerRadius = 18,
            .ShadowColor = Color.FromArgb(20, 0, 0, 0),
            .ShadowOffset = New Point(0, 4),
            .ShadowBlur = 14,
            .Padding = New Padding(16),
            .Dock = DockStyle.Fill
        }
        footerHost.Controls.Add(footerPanel)

        Dim footerTable As New TableLayoutPanel() With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 2,
            .RowCount = 1
        }
        footerTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 55.0F))
        footerTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 45.0F))
        footerPanel.Controls.Add(footerTable)

        Dim leftBox As New FlowLayoutPanel() With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.TopDown,
            .WrapContents = False,
            .AutoSize = True
        }

        btnCancelSub = New Button() With {
            .Text = "定期購読を解約（キャンセル）",
            .Font = New Font("Yu Gothic UI", 11.0F, FontStyle.Bold),
            .ForeColor = Color.White,
            .BackColor = cDanger,
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(260, 45),
            .Margin = New Padding(0, 10, 0, 0)
        }
        btnCancelSub.FlatAppearance.BorderSize = 0
        AddHandler btnCancelSub.Click, AddressOf btnCancelSub_Click
        leftBox.Controls.Add(btnCancelSub)

        btnResumeSub = New Button() With {
            .Text = "定期購読を再開（ACTIVE）",
            .Font = New Font("Yu Gothic UI", 11.0F, FontStyle.Bold),
            .ForeColor = Color.White,
            .BackColor = cSuccess,
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(260, 45),
            .Margin = New Padding(0, 10, 0, 0)
        }
        btnResumeSub.FlatAppearance.BorderSize = 0
        AddHandler btnResumeSub.Click, AddressOf btnResumeSub_Click
        leftBox.Controls.Add(btnResumeSub)

        Dim rightBox As New FlowLayoutPanel() With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.RightToLeft,
            .WrapContents = False,
            .AutoSize = True,
            .Padding = New Padding(0, 18, 0, 0)
        }

        btnClose = New Button() With {
            .Text = "閉じる",
            .Font = New Font("Yu Gothic UI", 11.0F, FontStyle.Bold),
            .ForeColor = cText,
            .BackColor = Color.FromArgb(243, 244, 246),
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(140, 44),
            .Margin = New Padding(10, 0, 0, 0)
        }
        btnClose.FlatAppearance.BorderSize = 0
        AddHandler btnClose.Click, Sub() Me.Close()
        rightBox.Controls.Add(btnClose)

        btnSave = New Button() With {
            .Text = "保存",
            .Font = New Font("Yu Gothic UI", 11.0F, FontStyle.Bold),
            .ForeColor = Color.White,
            .BackColor = cPrimary,
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(140, 44),
            .Margin = New Padding(0, 0, 0, 0)
        }
        btnSave.FlatAppearance.BorderSize = 0
        AddHandler btnSave.Click, AddressOf SaveCustomer
        rightBox.Controls.Add(btnSave)

        footerTable.Controls.Add(leftBox, 0, 0)
        footerTable.Controls.Add(rightBox, 1, 0)

        Me.AcceptButton = btnSave
        Me.CancelButton = btnClose

        ' フォーカススタイル（任意）
        WireFocusStyle(txtCustomerCode)
        WireFocusStyle(txtCustomerName)
        WireFocusStyle(txtPhoneNumber)
        WireFocusStyle(txtAddress)
        WireFocusStyle(txtApartmentName)
        WireFocusStyle(txtBirthDate)
        WireFocusStyle(txtPaymentMethod)
        WireFocusStyle(txtPurchaseStart)
        WireFocusStyle(txtPurchaseReason)
        WireFocusStyle(txtSubscriptionStatus)
    End Sub

    Private Sub AdjustCardWidth(sender As Object, e As EventArgs)
        If scrollPanel Is Nothing OrElse cardPanel Is Nothing Then Return

        Dim padL As Integer = scrollPanel.Padding.Left
        Dim padR As Integer = scrollPanel.Padding.Right
        Dim sb As Integer = If(scrollPanel.VerticalScroll.Visible, SystemInformation.VerticalScrollBarWidth, 0)

        Dim w As Integer = scrollPanel.ClientSize.Width - padL - padR - sb
        If w < 0 Then w = 0

        cardPanel.Width = w
        cardPanel.Location = New Point(padL, scrollPanel.Padding.Top)
    End Sub

    ' =========================
    ' TableLayout helpers（2列/ワイド）
    ' =========================
    Private Function FieldBlock(caption As String, ctrl As Control) As Panel
        Dim p As New Panel() With {.Dock = DockStyle.Fill, .AutoSize = True, .AutoSizeMode = AutoSizeMode.GrowAndShrink}

        Dim lbl As New Label() With {
            .Text = caption,
            .Font = fontLabel,
            .ForeColor = cSubText,
            .Dock = DockStyle.Top,
            .Height = 18
        }

        ctrl.Dock = DockStyle.Top
        ctrl.Margin = New Padding(0, 4, 0, 0)

        p.Controls.Add(ctrl)
        p.Controls.Add(lbl)

        p.Padding = New Padding(0, 6, 10, 6)
        Return p
    End Function

    Private Sub AddRow2(leftCaption As String, leftCtrl As Control,
                       rightCaption As String, rightCtrl As Control)
        fieldsTable.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        fieldsTable.Controls.Add(FieldBlock(leftCaption, leftCtrl), 0, fieldsTable.RowCount)
        fieldsTable.Controls.Add(FieldBlock(rightCaption, rightCtrl), 1, fieldsTable.RowCount)
        fieldsTable.RowCount += 1
    End Sub

    Private Sub AddWide(caption As String, ctrl As Control)
        fieldsTable.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        Dim block = FieldBlock(caption, ctrl)
        fieldsTable.Controls.Add(block, 0, fieldsTable.RowCount)
        fieldsTable.SetColumnSpan(block, 2)
        fieldsTable.RowCount += 1
    End Sub

    ' =========================
    ' 定期：再開
    ' =========================
    Private Sub btnResumeSub_Click(sender As Object, e As EventArgs)
        Dim code As String = Me.CustomerCode
        If String.IsNullOrWhiteSpace(code) Then
            MessageBox.Show("顧客コードが取得できません。")
            Return
        End If

        Dim ok = MessageBox.Show($"顧客コード {code} の定期購読を再開（ACTIVE）しますか？",
                                 "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        If ok <> DialogResult.Yes Then Return

        Dim reason As String = Microsoft.VisualBasic.Interaction.InputBox("再開理由（任意）を入力してください。", "再開理由", "")

        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()
                Using tx = conn.BeginTransaction()
                    Try
                        Dim sqlCloseActive As String =
                            "UPDATE subscriptions " &
                            "SET status='PAUSED', end_date=CURDATE(), cancel_reason=NULL " &
                            "WHERE customer_code=@code AND status='ACTIVE'"

                        Using cmd1 As New MySqlCommand(sqlCloseActive, conn, tx)
                            cmd1.Parameters.AddWithValue("@code", code)
                            cmd1.ExecuteNonQuery()
                        End Using

                        Dim sqlIns As String =
                            "INSERT INTO subscriptions (customer_code, status, start_date, end_date, cancel_reason) " &
                            "VALUES (@code, 'ACTIVE', CURDATE(), NULL, @reason)"

                        Using cmd2 As New MySqlCommand(sqlIns, conn, tx)
                            cmd2.Parameters.AddWithValue("@code", code)
                            cmd2.Parameters.AddWithValue("@reason",
                                If(String.IsNullOrWhiteSpace(reason), CType(DBNull.Value, Object), reason.Trim()))
                            cmd2.ExecuteNonQuery()
                        End Using

                        tx.Commit()
                    Catch
                        Try : tx.Rollback() : Catch : End Try
                        Throw
                    End Try
                End Using
            End Using

            MessageBox.Show("定期購読を再開しました。", "完了", MessageBoxButtons.OK, MessageBoxIcon.Information)
            LoadCustomerByCode(code)

        Catch ex As Exception
            MessageBox.Show("エラー: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' =========================
    ' 定期：解約
    ' =========================
    Private Sub btnCancelSub_Click(sender As Object, e As EventArgs)
        If String.IsNullOrWhiteSpace(Me.CustomerCode) Then
            MessageBox.Show("顧客コードが取得できません。")
            Return
        End If

        Dim ok = MessageBox.Show($"顧客コード {Me.CustomerCode} の定期購読を解約しますか？",
                                 "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
        If ok <> DialogResult.Yes Then Return

        Dim reason As String = Microsoft.VisualBasic.Interaction.InputBox("解約理由（任意）を入力してください。", "解約理由", "")

        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()

                Dim sql As String =
                    "INSERT INTO subscriptions (customer_code, status, start_date, cancel_reason) " &
                    "VALUES (@code, 'CANCELED', CURDATE(), @reason)"

                Using cmd As New MySqlCommand(sql, conn)
                    cmd.Parameters.Add("@code", MySqlDbType.VarChar).Value = Me.CustomerCode
                    cmd.Parameters.Add("@reason", MySqlDbType.VarChar).Value =
                        If(String.IsNullOrWhiteSpace(reason), DBNull.Value, reason.Trim())
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            MessageBox.Show("定期購読を解約しました。", "完了", MessageBoxButtons.OK, MessageBoxIcon.Information)
            LoadCustomerByCode(Me.CustomerCode)

        Catch ex As Exception
            MessageBox.Show("エラー: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' =========================
    ' DBから読み込み（定期状況も取得）
    ' =========================
    Public Sub LoadCustomerByCode(code As String)
        Try
            SetStatus("読み込み中...")

            Using conn As New MySqlConnection(connectionString)
                conn.Open()

                Dim sql As String =
                    "SELECT " &
                    " c.customer_code, c.customer_name, c.phone_number, c.address, c.apartment_name, " &
                    " c.birth_date, c.payment_method, c.purchase_start, c.purchase_reason, " &
                    " COALESCE( " &
                    "   CASE " &
                    "     WHEN s.status='ACTIVE' THEN '定期購読中' " &
                    "     WHEN s.status='PAUSED' THEN '一時停止' " &
                    "     WHEN s.status='CANCELED' THEN '解約' " &
                    "     ELSE NULL " &
                    "   END, '未契約' " &
                    " ) AS subscription_status_jp " &
                    "FROM customer_master c " &
                    "LEFT JOIN subscriptions s " &
                    "  ON s.subscription_id = ( " &
                    "     SELECT s2.subscription_id " &
                    "     FROM subscriptions s2 " &
                    "     WHERE s2.customer_code = c.customer_code " &
                    "     ORDER BY s2.start_date DESC, s2.subscription_id DESC " &
                    "     LIMIT 1 " &
                    "  ) " &
                    "WHERE c.customer_code=@code"

                Using cmd As New MySqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@code", code)

                    Using rdr As MySqlDataReader = cmd.ExecuteReader()
                        If Not rdr.Read() Then
                            MessageBox.Show("この顧客コードはDBに存在しません。")
                            SetStatus("顧客が見つかりません。", True)
                            Return
                        End If

                        txtCustomerCode.Text = SafeStr(rdr("customer_code"))
                        txtCustomerName.Text = SafeStr(rdr("customer_name"))
                        txtPhoneNumber.Text = SafeStr(rdr("phone_number"))
                        txtAddress.Text = SafeStr(rdr("address"))
                        txtApartmentName.Text = SafeStr(rdr("apartment_name"))
                        txtPaymentMethod.Text = SafeStr(rdr("payment_method"))

                        txtPurchaseReason.ForeColor = cText
                        txtPurchaseReason.Text = SafeStr(rdr("purchase_reason"))

                        txtBirthDate.Text = DbDateToText(rdr("birth_date"))
                        txtPurchaseStart.Text = DbDateToText(rdr("purchase_start"))

                        txtSubscriptionStatus.Text = SafeStr(rdr("subscription_status_jp"))
                        UpdateSubscriptionButtons(txtSubscriptionStatus.Text)

                    End Using
                End Using
            End Using

            SetStatus("読み込み完了。")
            AdjustCardWidth(Nothing, EventArgs.Empty)

        Catch ex As Exception
            MessageBox.Show("読み込みに失敗しました: " & ex.Message)
            SetStatus("読み込み失敗: " & ex.Message, True)
        End Try
    End Sub

    ' =========================
    ' 保存（customer_masterのみ更新）
    ' =========================
    Private Sub SaveCustomer(sender As Object, e As EventArgs)
        If Not ValidateAllInputs(showMessage:=True) Then Return
        If err IsNot Nothing Then err.Clear()

        Try
            Dim code As String = Nz(txtCustomerCode.Text)
            If code = "" Then
                MessageBox.Show("顧客コードが空です。")
                SetStatus("顧客コードが空です。", True)
                Return
            End If

            NormalizePlaceholderText(txtBirthDate)
            NormalizePlaceholderText(txtPurchaseStart)
            NormalizePlaceholderText(txtPaymentMethod)
            NormalizePlaceholderText(txtPurchaseReason)

            Dim newName As String = Nz(txtCustomerName.Text)
            Dim newPhone As String = Nz(txtPhoneNumber.Text)
            Dim newAddress As String = Nz(txtAddress.Text)
            Dim newApt As String = Nz(txtApartmentName.Text)
            Dim newPay As String = Nz(txtPaymentMethod.Text)
            Dim newReason As String = Nz(txtPurchaseReason.Text)
            Dim newBirth As Object = ParseDateOrNull(txtBirthDate.Text)
            Dim newStart As Object = ParseDateOrNull(txtPurchaseStart.Text)

            Using conn As New MySqlConnection(connectionString)
                conn.Open()

                Dim selectSql As String =
                    "SELECT customer_name, phone_number, address, apartment_name, birth_date, payment_method, purchase_start, purchase_reason " &
                    "FROM customer_master WHERE customer_code=@code"

                Dim oldName As String = ""
                Dim oldPhone As String = ""
                Dim oldAddress As String = ""
                Dim oldApt As String = ""
                Dim oldPay As String = ""
                Dim oldReason As String = ""
                Dim oldBirth As Object = DBNull.Value
                Dim oldStart As Object = DBNull.Value

                Using cmdSel As New MySqlCommand(selectSql, conn)
                    cmdSel.Parameters.AddWithValue("@code", code)

                    Using rdr As MySqlDataReader = cmdSel.ExecuteReader()
                        If Not rdr.Read() Then
                            MessageBox.Show("この顧客コードはDBに存在しません。")
                            SetStatus("保存失敗（顧客が存在しません）", True)
                            Return
                        End If

                        oldName = SafeStr(rdr("customer_name"))
                        oldPhone = SafeStr(rdr("phone_number"))
                        oldAddress = SafeStr(rdr("address"))
                        oldApt = SafeStr(rdr("apartment_name"))
                        oldPay = SafeStr(rdr("payment_method"))
                        oldReason = SafeStr(rdr("purchase_reason"))
                        oldBirth = If(IsDBNull(rdr("birth_date")), DBNull.Value, rdr("birth_date"))
                        oldStart = If(IsDBNull(rdr("purchase_start")), DBNull.Value, rdr("purchase_start"))
                    End Using
                End Using

                Dim sets As New List(Of String)()
                Dim cmdUpd As New MySqlCommand() With {.Connection = conn}

                If oldName <> newName Then
                    sets.Add("customer_name=@name")
                    cmdUpd.Parameters.AddWithValue("@name", newName)
                End If
                If oldPhone <> newPhone Then
                    sets.Add("phone_number=@phone")
                    cmdUpd.Parameters.AddWithValue("@phone", newPhone)
                End If
                If oldAddress <> newAddress Then
                    sets.Add("address=@addr")
                    cmdUpd.Parameters.AddWithValue("@addr", newAddress)
                End If
                If oldApt <> newApt Then
                    sets.Add("apartment_name=@apt")
                    cmdUpd.Parameters.AddWithValue("@apt", newApt)
                End If
                If Not DbDateEquals(oldBirth, newBirth) Then
                    sets.Add("birth_date=@birth")
                    cmdUpd.Parameters.AddWithValue("@birth", newBirth)
                End If
                If oldPay <> newPay Then
                    sets.Add("payment_method=@pay")
                    cmdUpd.Parameters.AddWithValue("@pay", newPay)
                End If
                If Not DbDateEquals(oldStart, newStart) Then
                    sets.Add("purchase_start=@pstart")
                    cmdUpd.Parameters.AddWithValue("@pstart", newStart)
                End If
                If oldReason <> newReason Then
                    sets.Add("purchase_reason=@reason")
                    cmdUpd.Parameters.AddWithValue("@reason", newReason)
                End If

                If sets.Count = 0 Then
                    MessageBox.Show("変更はありません（更新なし）。")
                    SetStatus("変更なし（更新なし）")
                    Return
                End If

                cmdUpd.CommandText =
                    "UPDATE customer_master SET " & String.Join(", ", sets) & " WHERE customer_code=@code"
                cmdUpd.Parameters.AddWithValue("@code", code)

                Dim affected As Integer = cmdUpd.ExecuteNonQuery()
                MessageBox.Show("保存しました。（更新 " & affected & " 件）")
                SetStatus("保存しました（更新 " & affected & " 件）")
            End Using

        Catch ex As Exception
            MessageBox.Show("保存に失敗しました: " & ex.Message)
            SetStatus("保存失敗: " & ex.Message, True)
        End Try
    End Sub

    ' =========================
    ' TextBox / Placeholder / Status
    ' =========================
    Private Function CreateTextBox(Optional readOnlyBox As Boolean = False,
                                   Optional multiline As Boolean = False,
                                   Optional height As Integer = 34,
                                   Optional placeholder As String = "") As TextBox
        Dim tb As New TextBox() With {
            .Font = fontInput,
            .BorderStyle = BorderStyle.FixedSingle,
            .BackColor = If(readOnlyBox, Color.FromArgb(249, 250, 251), Color.White),
            .ForeColor = cText,
            .ReadOnly = readOnlyBox,
            .Multiline = multiline,
            .Height = height,
            .Tag = placeholder
        }

        If placeholder <> "" Then
            SetPlaceholder(tb)
            AddHandler tb.GotFocus, AddressOf RemovePlaceholder
            AddHandler tb.LostFocus, AddressOf AddPlaceholder
        End If
        Return tb
    End Function

    Private Sub WireFocusStyle(tb As TextBox)
        AddHandler tb.Enter, Sub() tb.BackColor = If(tb.ReadOnly, Color.FromArgb(249, 250, 251), Color.White)
        AddHandler tb.GotFocus, Sub() tb.Refresh()
        AddHandler tb.LostFocus, Sub() tb.Refresh()
    End Sub

    Private Sub SetPlaceholder(tb As TextBox)
        Dim ph As String = TryCast(tb.Tag, String)
        If ph = "" Then Return
        If Nz(tb.Text) = "" Then
            tb.ForeColor = Color.FromArgb(156, 163, 175)
            tb.Text = ph
        End If
    End Sub

    Private Sub RemovePlaceholder(sender As Object, e As EventArgs)
        Dim tb = CType(sender, TextBox)
        Dim ph As String = TryCast(tb.Tag, String)
        If ph <> "" AndAlso tb.Text = ph Then
            tb.Text = ""
            tb.ForeColor = cText
        End If
    End Sub

    Private Sub AddPlaceholder(sender As Object, e As EventArgs)
        Dim tb = CType(sender, TextBox)
        Dim ph As String = TryCast(tb.Tag, String)
        If ph <> "" AndAlso Nz(tb.Text) = "" Then
            tb.ForeColor = Color.FromArgb(156, 163, 175)
            tb.Text = ph
        End If
    End Sub

    Private Sub NormalizePlaceholderText(tb As TextBox)
        If tb Is Nothing Then Return
        Dim ph As String = TryCast(tb.Tag, String)
        If ph <> "" AndAlso tb.Text = ph Then tb.Text = ""
    End Sub

    Private Sub SetStatus(msg As String, Optional isError As Boolean = False)
        If statusLabel Is Nothing Then Return
        statusLabel.Text = msg
        statusLabel.ForeColor = If(isError, cDanger, cSubText)
    End Sub

    ' =========================
    ' Helper（日付）
    ' =========================
    Private Function DbDateToText(v As Object) As String
        If v Is Nothing OrElse IsDBNull(v) Then Return ""
        If TypeOf v Is DateTime Then Return CType(v, DateTime).ToString("yyyy-MM-dd")

        Dim s As String = v.ToString().Trim()
        If s = "" Then Return ""
        Dim d As DateTime
        If DateTime.TryParse(s, d) Then Return d.ToString("yyyy-MM-dd")
        Return s
    End Function

    Private Function ParseDateOrNull(s As String) As Object
        Dim t As String = Nz(s)
        If t = "" Then Return DBNull.Value

        Dim d As DateTime
        If DateTime.TryParseExact(t, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, d) Then
            Return d.Date
        End If
        If DateTime.TryParse(t, d) Then
            Return d.Date
        End If

        Throw New FormatException("日付形式が不正です: " & t & "（yyyy-MM-dd）")
    End Function

    Private Function DbDateEquals(a As Object, b As Object) As Boolean
        Dim aNull As Boolean = (a Is Nothing OrElse IsDBNull(a))
        Dim bNull As Boolean = (b Is Nothing OrElse IsDBNull(b))
        If aNull AndAlso bNull Then Return True
        If aNull Xor bNull Then Return False
        Return CType(a, DateTime).Date = CType(b, DateTime).Date
    End Function

    ' =========================
    ' Helper（文字）
    ' =========================
    Private Function Nz(s As String) As String
        If s Is Nothing Then Return ""
        Return s.Trim()
    End Function

    Private Function SafeStr(v As Object) As String
        If v Is Nothing OrElse IsDBNull(v) Then Return ""
        Return v.ToString().Trim()
    End Function

    ' =========================
    ' Validation（あなたのルールを維持）
    ' =========================
    Private ReadOnly ForbiddenWords As String() = {
        "admin", "root", "system", "null",
        "select", "insert", "update", "delete", "drop",
        "test", "dummy"
    }

    Private Const MAX_NAME As Integer = 60
    Private Const MAX_ADDRESS As Integer = 120
    Private Const MAX_APT As Integer = 60

    Private Shared ReadOnly RX_CTRL As New Regex("[\x00-\x08\x0B\x0C\x0E-\x1F]", RegexOptions.Compiled)

    Private Shared ReadOnly RX_ADDR_ALLOWED As New Regex(
        "^[\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}々A-Za-z0-9\s\-ー－丁目番地号都道府県市区町村郡県府道市町村区、，\.・（）\(\)＃#／/]+$",
        RegexOptions.Compiled)

    Private Shared ReadOnly RX_NAME_ALLOWED As New Regex(
        "^[\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}A-Za-z0-9\s・．\.㈱（）\(\)ー\-株式会社]+$",
        RegexOptions.Compiled)

    Private Shared ReadOnly RX_NAME_ONLY_NUM As New Regex("^[0-9\s]+$", RegexOptions.Compiled)

    Private Shared ReadOnly PREFS As String() = {
        "北海道",
        "青森県", "岩手県", "宮城県", "秋田県", "山形県", "福島県",
        "茨城県", "栃木県", "群馬県", "埼玉県", "千葉県", "東京都", "神奈川県",
        "新潟県", "富山県", "石川県", "福井県", "山梨県", "長野県",
        "岐阜県", "静岡県", "愛知県", "三重県",
        "滋賀県", "京都府", "大阪府", "兵庫県", "奈良県", "和歌山県",
        "鳥取県", "島根県", "岡山県", "広島県", "山口県",
        "徳島県", "香川県", "愛媛県", "高知県",
        "福岡県", "佐賀県", "長崎県", "熊本県", "大分県", "宮崎県", "鹿児島県",
        "沖縄県"
    }

    Private Shared ReadOnly PAYMENT_SET As HashSet(Of String) =
        New HashSet(Of String)(New String() {"VISA", "MASTER", "JCB", "AMEX"}, StringComparer.OrdinalIgnoreCase)

    Private Function NormalizeSpaces(s As String) As String
        If s Is Nothing Then Return ""
        Dim t = s.Replace("　", " ")
        t = Regex.Replace(t, "\s{2,}", " ")
        Return t.Trim()
    End Function

    Private Function RemoveControlChars(s As String) As String
        If s Is Nothing Then Return ""
        Return RX_CTRL.Replace(s, "")
    End Function

    Private Function ContainsForbiddenWords(s As String) As Boolean
        If String.IsNullOrWhiteSpace(s) Then Return False
        For Each w In ForbiddenWords
            If s.IndexOf(w, StringComparison.OrdinalIgnoreCase) >= 0 Then Return True
        Next
        Return False
    End Function

    Private Sub SetErr(c As Control, msg As String)
        If err Is Nothing Then Return
        err.SetError(c, msg)
    End Sub

    Private Function ExtractPrefecture(address As String) As String
        Dim a = If(address, "").Trim()
        If a = "" Then Return ""
        For Each p In PREFS
            If a.StartsWith(p, StringComparison.Ordinal) Then Return p
        Next
        For Each p In PREFS
            If a.Contains(p) Then Return p
        Next
        Return ""
    End Function

    Private Function TryNormalizeAndValidatePhone(input As String, ByRef normalizedPhone As String) As Boolean
        normalizedPhone = ""
        Dim s As String = If(input, "").Trim()
        If s = "" Then Return False

        s = s.Replace("－", "-").Replace("ー", "-").Replace("―", "-").Replace("−", "-")
        Dim digitsOnly As String = Regex.Replace(s, "[^\d]", "")

        If Not digitsOnly.StartsWith("0") Then Return False
        If Not (digitsOnly.Length = 10 OrElse digitsOnly.Length = 11) Then Return False

        normalizedPhone = digitsOnly
        Return True
    End Function

    Private Function ValidateAllInputs(Optional showMessage As Boolean = True) As Boolean
        If err IsNot Nothing Then err.Clear()

        NormalizePlaceholderText(txtBirthDate)
        NormalizePlaceholderText(txtPurchaseStart)
        NormalizePlaceholderText(txtPaymentMethod)
        NormalizePlaceholderText(txtPurchaseReason)

        ' 顧客名（必須）
        Dim name = NormalizeSpaces(RemoveControlChars(Nz(txtCustomerName.Text)))
        If name = "" Then
            SetErr(txtCustomerName, "顧客名を入力してください")
            If showMessage Then MessageBox.Show("顧客名を入力してください。")
            txtCustomerName.Focus()
            Return False
        End If
        If name.Length > MAX_NAME OrElse
           Not RX_NAME_ALLOWED.IsMatch(name) OrElse
           RX_NAME_ONLY_NUM.IsMatch(name) OrElse
           ContainsForbiddenWords(name) Then

            SetErr(txtCustomerName, "顧客名が不正です")
            If showMessage Then MessageBox.Show("顧客名が不正です。")
            txtCustomerName.Focus()
            txtCustomerName.SelectAll()
            Return False
        End If

        ' 電話（必須）
        Dim phoneRaw = Nz(txtPhoneNumber.Text)
        Dim normalized As String = ""
        If phoneRaw = "" OrElse Not TryNormalizeAndValidatePhone(phoneRaw, normalized) Then
            SetErr(txtPhoneNumber, "電話番号が不正です")
            If showMessage Then MessageBox.Show("電話番号の形式が正しくありません。（例：03-1234-5678 / 09012345678）")
            txtPhoneNumber.Focus()
            txtPhoneNumber.SelectAll()
            Return False
        End If

        ' 住所（入力されたらチェック）
        Dim addr = NormalizeSpaces(RemoveControlChars(Nz(txtAddress.Text)))
        If addr <> "" Then
            If addr.Length > MAX_ADDRESS OrElse RX_CTRL.IsMatch(addr) OrElse Not RX_ADDR_ALLOWED.IsMatch(addr) Then
                SetErr(txtAddress, "住所に使用できない文字が含まれています")
                If showMessage Then MessageBox.Show("住所に使用できない文字が含まれています。")
                txtAddress.Focus()
                txtAddress.SelectAll()
                Return False
            End If

            Dim pref = ExtractPrefecture(addr)
            If pref = "" Then
                SetErr(txtAddress, "住所に都道府県名（○○県/東京都/大阪府/京都府 等）を含めてください")
                If showMessage Then MessageBox.Show("住所に都道府県名（○○県/東京都/大阪府/京都府 等）を含めてください。")
                txtAddress.Focus()
                txtAddress.SelectAll()
                Return False
            End If
        End If

        ' アパート（任意）
        Dim apt = NormalizeSpaces(RemoveControlChars(Nz(txtApartmentName.Text)))
        If apt <> "" Then
            If apt.Length > MAX_APT OrElse RX_CTRL.IsMatch(apt) OrElse Not RX_ADDR_ALLOWED.IsMatch(apt) Then
                SetErr(txtApartmentName, "アパート名に使用できない文字が含まれています")
                If showMessage Then MessageBox.Show("アパート名に使用できない文字が含まれています。")
                txtApartmentName.Focus()
                txtApartmentName.SelectAll()
                Return False
            End If
        End If

        ' 日付（空欄可）
        If Nz(txtBirthDate.Text) <> "" Then
            Dim d As DateTime
            If Not DateTime.TryParseExact(Nz(txtBirthDate.Text), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, d) Then
                SetErr(txtBirthDate, "生年月日は yyyy-MM-dd 形式です")
                If showMessage Then MessageBox.Show("生年月日は yyyy-MM-dd 形式で入力してください。")
                txtBirthDate.Focus()
                txtBirthDate.SelectAll()
                Return False
            End If
        End If

        If Nz(txtPurchaseStart.Text) <> "" Then
            Dim d2 As DateTime
            If Not DateTime.TryParseExact(Nz(txtPurchaseStart.Text), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, d2) Then
                SetErr(txtPurchaseStart, "購入開始日は yyyy-MM-dd 形式です")
                If showMessage Then MessageBox.Show("購入開始日は yyyy-MM-dd 形式で入力してください。")
                txtPurchaseStart.Focus()
                txtPurchaseStart.SelectAll()
                Return False
            End If
        End If

        ' 支払方法（空欄可）
        Dim pay = Nz(txtPaymentMethod.Text)
        If pay <> "" AndAlso Not PAYMENT_SET.Contains(pay) Then
            SetErr(txtPaymentMethod, "支払方法は VISA / MASTER / JCB / AMEX のみです")
            If showMessage Then MessageBox.Show("支払方法は VISA / MASTER / JCB / AMEX のみです。")
            txtPaymentMethod.Focus()
            txtPaymentMethod.SelectAll()
            Return False
        End If

        ' 正規化反映
        txtCustomerName.Text = name
        txtPhoneNumber.Text = normalized
        txtAddress.Text = addr
        txtApartmentName.Text = apt
        If pay <> "" Then txtPaymentMethod.Text = pay.Trim().ToUpperInvariant()

        Return True
    End Function


    ' =========================
    ' 定期ボタン表示制御
    ' =========================
    Private Sub UpdateSubscriptionButtons(statusJp As String)
        Dim s As String = Nz(statusJp)

        ' 「定期購読中」だけをACTIVE扱いにする（必要なら条件を増やせる）
        Dim isActive As Boolean = (s = "定期購読中")

        ' ACTIVE → 解約ボタンのみ
        btnCancelSub.Visible = isActive
        btnResumeSub.Visible = Not isActive

        ' ついでに文言も状況に合わせて変える（任意）
        If Not isActive Then
            ' 未契約/一時停止/解約 → 「定期購読する」寄りの表現に
            btnResumeSub.Text = "定期購読を開始（ACTIVE）"
        Else
            btnCancelSub.Text = "定期購読を解約（キャンセル）"
        End If
    End Sub


End Class

' =========================
' RoundedPanel（角丸・影・枠）
' =========================
Public Class RoundedPanel2
    Inherits Panel

    Public Property CornerRadius As Integer = 16
    Public Property BorderColor As Color = Color.Gainsboro
    Public Property BorderWidth As Integer = 1

    Public Property ShadowColor As Color = Color.FromArgb(25, 0, 0, 0)
    Public Property ShadowOffset As Point = New Point(0, 6)
    Public Property ShadowBlur As Integer = 16

    Public Sub New()
        Me.SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer, True)
        Me.DoubleBuffered = True
        Me.BackColor = Color.White
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias

        Dim rect As Rectangle = Me.ClientRectangle
        rect.Width -= 1
        rect.Height -= 1

        ' Shadow
        Using gpShadow As GraphicsPath =
            RoundedRectPath(New Rectangle(rect.X + ShadowOffset.X, rect.Y + ShadowOffset.Y, rect.Width, rect.Height), CornerRadius)
            Using sb As New SolidBrush(ShadowColor)
                e.Graphics.FillPath(sb, gpShadow)
            End Using
        End Using

        ' Card
        Using gp As GraphicsPath = RoundedRectPath(rect, CornerRadius)
            Using b As New SolidBrush(Me.BackColor)
                e.Graphics.FillPath(b, gp)
            End Using

            If BorderWidth > 0 Then
                Using p As New Pen(BorderColor, BorderWidth)
                    e.Graphics.DrawPath(p, gp)
                End Using
            End If
        End Using

        MyBase.OnPaint(e)
    End Sub

    Private Function RoundedRectPath(r As Rectangle, radius As Integer) As GraphicsPath
        Dim path As New GraphicsPath()
        Dim d As Integer = radius * 2

        If radius <= 0 Then
            path.AddRectangle(r)
            path.CloseFigure()
            Return path
        End If

        path.AddArc(r.X, r.Y, d, d, 180, 90)
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90)
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90)
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90)
        path.CloseFigure()

        Return path
    End Function
End Class
