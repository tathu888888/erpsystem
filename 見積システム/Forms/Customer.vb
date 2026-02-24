Imports MySql.Data.MySqlClient
Imports System.Windows.Forms
Imports System.Text.RegularExpressions
Imports System.Drawing
Imports System.Drawing.Drawing2D

Public Class Customer

    Private ReadOnly connectionString As String =
        "Server=127.0.0.1;Port=3306;Database=sunstar;Uid=root;Pwd=1234;SslMode=Disabled;"

    '========================
    ' 使用禁止語（必要に応じて追加）
    '========================
    Private ReadOnly ForbiddenWords As String() = {
        "admin", "root", "system", "null",
        "select", "insert", "update", "delete", "drop",
        "test", "dummy"
    }

    '========================
    ' Validation rules / helpers
    '========================

    ' 最大長（DBのvarchar想定で調整）
    Private Const MAX_NAME As Integer = 60
    Private Const MAX_ADDRESS As Integer = 120
    Private Const MAX_APT As Integer = 60

    ' 制御文字（改行/タブ等の混入）を禁止
    Private Shared ReadOnly RX_CTRL As New Regex("[\x00-\x08\x0B\x0C\x0E-\x1F]", RegexOptions.Compiled)

    ' 電話
    Private Shared ReadOnly RX_MULTI_HYPHEN As New Regex("\-{2,}", RegexOptions.Compiled)

    ' 住所/アパート：許可する文字（日本住所想定）
    Private Shared ReadOnly RX_ADDR_ALLOWED As New Regex(
"^[\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}々A-Za-z0-9\s\-ー－丁目番地号都道府県市区町村郡県府道市町村区、，\.・（）\(\)＃#／/]+$",
RegexOptions.Compiled)

    ' 名前：許可
    Private Shared ReadOnly RX_NAME_ALLOWED As New Regex("^[\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}A-Za-z0-9\s・．\.㈱（）\(\)ー\-株式会社]+$", RegexOptions.Compiled)
    Private Shared ReadOnly RX_NAME_ONLY_NUM As New Regex("^[0-9\s]+$", RegexOptions.Compiled)

    ' 顧客コード：6桁
    Private Shared ReadOnly RX_CODE_6 As New Regex("^\d{6}$", RegexOptions.Compiled)

    ' 都道府県（prefecture自動抽出用）
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

    '========================
    ' Theme
    '========================
    Private ReadOnly bgColor As Color = Color.FromArgb(245, 246, 248)
    Private ReadOnly cardColor As Color = Color.White
    Private ReadOnly textColor As Color = Color.FromArgb(35, 38, 47)
    Private ReadOnly mutedText As Color = Color.FromArgb(110, 115, 130)
    Private ReadOnly primary As Color = Color.FromArgb(65, 105, 225)
    Private ReadOnly danger As Color = Color.FromArgb(220, 53, 69)

    Private ReadOnly fontTitle As New Font("Segoe UI", 18, FontStyle.Bold)
    Private ReadOnly fontSub As New Font("Segoe UI", 10, FontStyle.Regular)
    Private ReadOnly fontLabel As New Font("Segoe UI", 10, FontStyle.Regular)
    Private ReadOnly fontInput As New Font("Segoe UI", 11, FontStyle.Regular)

    Private headerSub As Label
    Private card As RoundedPanel
    Private err As ErrorProvider

    ' 住所から抽出した都道府県（保存に使う）
    Private extractedPrefecture As String = ""

    '========================
    ' Utility
    '========================
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

    Private Sub ClearErr(c As Control)
        If err Is Nothing Then Return
        err.SetError(c, "")
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

    '========================
    ' Load
    '========================
    Private Sub Customer_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        InitControls()
        InitRealtimeValidation()

        txtCustomerName.MaxLength = MAX_NAME
        txtAddress.MaxLength = MAX_ADDRESS
        txtApartmentName.MaxLength = MAX_APT

        ' Strict（貼り付け対策 + 即時バリデーション）だけに統一
        AddHandler txtCustomerName.TextChanged, AddressOf TxtCustomerName_TextChanged_Strict
        AddHandler txtAddress.TextChanged, AddressOf TxtAddress_TextChanged_Strict
        AddHandler txtApartmentName.TextChanged, AddressOf TxtApartment_TextChanged_Strict
        AddHandler cmbPaymentMethod.SelectedIndexChanged, AddressOf CmbPaymentMethod_Changed_Instant
        AddHandler dtpBirthDate.ValueChanged, AddressOf BirthDate_Changed_Instant

        ApplyModernDesign()
    End Sub

    '========================
    ' UI見た目
    '========================
    Private Sub ApplyModernDesign()
        Me.BackColor = bgColor
        Me.DoubleBuffered = True

        LabelTitle.Font = fontTitle
        LabelTitle.ForeColor = textColor
        LabelTitle.Text = "Customer Registration"

        headerSub = New Label() With {
            .AutoSize = True,
            .Font = fontSub,
            .ForeColor = mutedText,
            .Text = "顧客情報を入力して登録します（入力チェックはリアルタイムで行われます）"
        }
        headerSub.Location = New Point(LabelTitle.Left + 2, LabelTitle.Bottom + 6)
        Me.Controls.Add(headerSub)
        headerSub.BringToFront()

        card = New RoundedPanel() With {
            .Radius = 18,
            .FillColor = cardColor,
            .BorderColor = Color.FromArgb(235, 238, 244),
            .BorderWidth = 1,
            .ShadowColor = Color.FromArgb(35, 38, 47),
            .ShadowAlpha = 22,
            .ShadowOffset = New Point(0, 6)
        }

        Dim cardLeft As Integer = 20
        Dim cardTop As Integer = headerSub.Bottom + 16
        Dim cardWidth As Integer = Me.ClientSize.Width - 40
        Dim cardHeight As Integer = Me.ClientSize.Height - cardTop - 18

        card.Location = New Point(cardLeft, cardTop)
        card.Size = New Size(cardWidth, cardHeight)
        Me.Controls.Add(card)
        card.SendToBack()

        StyleButtonPrimary(btnSave)
        StyleButtonSecondary(btnClear)
        StyleButtonDanger(btnBack)

        Dim leftInputX As Integer = 170
        Dim btnY As Integer = card.Height - 70
        btnSave.Size = New Size(140, 42)
        btnClear.Size = New Size(140, 42)
        btnBack.Size = New Size(160, 42)

        btnSave.Location = New Point(leftInputX, btnY)
        btnClear.Location = New Point(leftInputX + 150, btnY)
        btnBack.Location = New Point(leftInputX + 300, btnY)

        MoveToCard(lblCustomerCode, txtCustomerCode,
                   lblCustomerName, txtCustomerName,
                   lblPhoneNumber, txtPhoneNumber,
                   lblAddress, txtAddress,
                   lblApartmentName, txtApartmentName,
                   lblPaymentMethod, cmbPaymentMethod,
                   lblBirthDate, dtpBirthDate,
                   btnSave, btnClear, btnBack)

        err = New ErrorProvider() With {.BlinkStyle = ErrorBlinkStyle.NeverBlink}

        HookFocus(txtCustomerCode)
        HookFocus(txtCustomerName)
        HookFocus(txtPhoneNumber)
        HookFocus(txtAddress)
        HookFocus(txtApartmentName)
        HookFocus(cmbPaymentMethod)
        HookFocus(dtpBirthDate)
    End Sub

    Private Sub MoveToCard(ParamArray ctrls As Control())
        For Each c In ctrls
            If c.Parent IsNot card Then
                Dim old = c.Location
                Me.Controls.Remove(c)
                card.Controls.Add(c)
                c.Location = old
            End If
        Next
    End Sub

    Private Sub StyleButtonPrimary(b As Button)
        ApplyButtonBase(b, primary, Color.White)
    End Sub
    Private Sub StyleButtonSecondary(b As Button)
        ApplyButtonBase(b, Color.FromArgb(240, 242, 246), textColor)
    End Sub
    Private Sub StyleButtonDanger(b As Button)
        ApplyButtonBase(b, danger, Color.White)
    End Sub

    Private Sub ApplyButtonBase(b As Button, back As Color, fore As Color)
        b.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        b.FlatStyle = FlatStyle.Flat
        b.FlatAppearance.BorderSize = 0
        b.BackColor = back
        b.ForeColor = fore
        b.Cursor = Cursors.Hand

        Dim normal = back
        AddHandler b.MouseEnter, Sub() b.BackColor = Darken(normal, 0.06F)
        AddHandler b.MouseLeave, Sub() b.BackColor = normal
    End Sub

    Private Function Darken(c As Color, amount As Single) As Color
        Dim r = Math.Max(0, CInt(c.R * (1 - amount)))
        Dim g = Math.Max(0, CInt(c.G * (1 - amount)))
        Dim bb = Math.Max(0, CInt(c.B * (1 - amount)))
        Return Color.FromArgb(c.A, r, g, bb)
    End Function

    Private Sub HookFocus(ctrl As Control)
        AddHandler ctrl.Enter, Sub() ctrl.BackColor = Color.FromArgb(245, 248, 255)
        AddHandler ctrl.Leave, Sub() ctrl.BackColor = Color.White
    End Sub

    '========================
    ' Init
    '========================
    Private Sub InitControls()
        cmbPaymentMethod.DropDownStyle = ComboBoxStyle.DropDownList
        cmbPaymentMethod.Items.Clear()
        cmbPaymentMethod.Items.AddRange(New Object() {"VISA", "MASTER", "JCB", "AMEX"})
        cmbPaymentMethod.SelectedIndex = -1

        dtpBirthDate.ShowCheckBox = True
        dtpBirthDate.Checked = False
    End Sub

    Private Sub InitRealtimeValidation()
        txtCustomerCode.MaxLength = 6
        txtPhoneNumber.MaxLength = 13

        ApplyCustomerCodeVisual(txtCustomerCode.Text)
        ApplyPhoneVisual()
    End Sub

    '========================
    ' Save
    '========================
    '========================
    ' Save (Plan A: customer + subscription)
    '========================
    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click

        If Not ValidateAllInputs() Then Return
        If err IsNot Nothing Then err.Clear()

        Dim customerCode As String = txtCustomerCode.Text.Trim()
        Dim customerName As String = txtCustomerName.Text.Trim()
        Dim phoneNumberRaw As String = txtPhoneNumber.Text.Trim()
        Dim paymentMethod As String = If(cmbPaymentMethod.SelectedItem, "").ToString()

        Dim normalizedPhone As String = ""
        If Not TryNormalizeAndValidatePhone(phoneNumberRaw, normalizedPhone) Then
            SetErr(txtPhoneNumber, "電話番号の形式が正しくありません")
            MessageBox.Show("電話番号の形式が正しくありません。（例：03-1234-5678 / 09012345678）")
            txtPhoneNumber.Focus()
            txtPhoneNumber.SelectAll()
            Return
        End If

        Dim birthDateObj As Object = If(dtpBirthDate.Checked, dtpBirthDate.Value.Date, DBNull.Value)

        ' 正規化済みを使う
        Dim addressText = NormalizeSpaces(RemoveControlChars(txtAddress.Text))
        Dim aptText = NormalizeSpaces(RemoveControlChars(txtApartmentName.Text))

        ' ※顧客マスタが NOT NULL の場合は DBNull.Value にしない方が安全
        Dim address As Object = If(String.IsNullOrWhiteSpace(addressText), "", addressText)
        Dim apt As Object = If(String.IsNullOrWhiteSpace(aptText), "", aptText)

        ' prefecture自動抽出
        Dim extracted = ExtractPrefecture(addressText)
        Dim pref As Object = If(extracted = "", "", extracted)

        ' ========================
        ' Plan A: 顧客登録と同時に定期購読を作る
        ' ========================
        Dim sqlCustomer As String =
        "INSERT INTO customer_master " &
        "(customer_code, customer_name, phone_number, address, prefecture, apartment_name, birth_date, payment_method) " &
        "VALUES (@code,@name,@phone,@address,@pref,@apt,@birth,@pay)"

        Dim sqlSub As String =
        "INSERT INTO subscriptions " &
        "(customer_code, product_code, unit_price, quantity, ship_day_of_month, status, start_date, end_date, cancel_reason, active_guard) " &
        "VALUES (@code, @product, @unit_price, @qty, @ship_day, @status, CURDATE(), NULL, NULL, 1)"

        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()

                Using tx = conn.BeginTransaction()
                    Try
                        ' ① 事前チェック：顧客コード重複（分かりやすく止める）
                        If CustomerExists(conn, tx, customerCode) Then
                            SetErr(txtCustomerCode, "既に登録されています")
                            MessageBox.Show("その顧客コードは既に登録されています。別のコードにしてください。")
                            txtCustomerCode.Focus()
                            txtCustomerCode.SelectAll()
                            tx.Rollback()
                            Return
                        End If

                        ' ② 顧客 INSERT
                        Using cmd As New MySqlCommand(sqlCustomer, conn, tx)
                            cmd.Parameters.Add("@code", MySqlDbType.VarChar).Value = customerCode
                            cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = customerName
                            cmd.Parameters.Add("@phone", MySqlDbType.VarChar).Value = normalizedPhone
                            cmd.Parameters.Add("@address", MySqlDbType.VarChar).Value = address
                            cmd.Parameters.Add("@pref", MySqlDbType.VarChar).Value = pref
                            cmd.Parameters.Add("@apt", MySqlDbType.VarChar).Value = apt
                            cmd.Parameters.Add("@birth", MySqlDbType.Date).Value = birthDateObj
                            cmd.Parameters.Add("@pay", MySqlDbType.VarChar).Value = paymentMethod
                            cmd.ExecuteNonQuery()
                        End Using

                        ' ③ 定期購読 INSERT（初期購読を作成）
                        Using cmd2 As New MySqlCommand("
    INSERT INTO subscriptions (customer_code, start_date)
    VALUES (@code, CURDATE())
", conn, tx)
                            cmd2.Parameters.Add("@code", MySqlDbType.VarChar).Value = customerCode
                            cmd2.ExecuteNonQuery()
                        End Using

                        tx.Commit()
                    Catch ex As MySqlException
                        tx.Rollback()
                        Throw
                    Catch ex As Exception
                        tx.Rollback()
                        Throw
                    End Try
                End Using
            End Using

            MessageBox.Show("顧客＋定期購読を登録しました。")
            ClearInputs()

        Catch ex As MySqlException
            HandleMySqlException(ex, customerCode, normalizedPhone)
        Catch ex As Exception
            MessageBox.Show($"登録エラー: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub

    '========================
    ' CustomerCode
    '========================
    Private Sub txtCustomerCode_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtCustomerCode.KeyPress
        If Char.IsControl(e.KeyChar) Then Return
        If Not Char.IsDigit(e.KeyChar) Then e.Handled = True
    End Sub

    Private Sub txtCustomerCode_TextChanged(sender As Object, e As EventArgs) Handles txtCustomerCode.TextChanged
        ApplyCustomerCodeVisual(txtCustomerCode.Text)
    End Sub


    Private Sub HandleMySqlException(ex As MySqlException, customerCode As String, normalizedPhone As String)

        ' MySQLのエラー番号で分岐してユーザー向けメッセージに変換する
        Select Case ex.Number

            Case 1062 ' Duplicate entry（UNIQUE違反）
                ' どのUNIQUEに引っかかったか判定（キー名で判定するのが一番確実）
                If ex.Message.IndexOf("uq_customer_phone", StringComparison.OrdinalIgnoreCase) >= 0 Then
                    SetErr(txtPhoneNumber, "この電話番号は既に登録されています")
                    MessageBox.Show("この電話番号は既に登録されています。別の電話番号を入力してください。",
                                "重複エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    txtPhoneNumber.Focus()
                    txtPhoneNumber.SelectAll()
                    Return
                End If

                If ex.Message.IndexOf("PRIMARY", StringComparison.OrdinalIgnoreCase) >= 0 _
               OrElse ex.Message.IndexOf("customer_code", StringComparison.OrdinalIgnoreCase) >= 0 Then
                    SetErr(txtCustomerCode, "この顧客コードは既に登録されています")
                    MessageBox.Show("この顧客コードは既に登録されています。別の顧客コードを入力してください。",
                                "重複エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    txtCustomerCode.Focus()
                    txtCustomerCode.SelectAll()
                    Return
                End If

                MessageBox.Show("同じ内容のデータが既に存在します。入力内容を確認してください。",
                            "重複エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return

            Case 1048 ' Column cannot be null（NOT NULL違反）
                MessageBox.Show("必須項目が未入力です。入力内容を確認してください。",
                            "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return

            Case 1406 ' Data too long for column（長さ超過）
                MessageBox.Show("入力が長すぎます。文字数を減らしてください。",
                            "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return

            Case 1452 ' Cannot add or update child row（外部キー違反）
                MessageBox.Show("関連データが不正です。（参照先が存在しません）",
                            "整合性エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return

            Case 3819 ' Check constraint violated（CHECK制約違反：MySQL 8）
                MessageBox.Show("入力内容がルールに合いません。入力を見直してください。",
                            "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return

            Case Else
                ' それ以外は「ユーザー向け」＋必要ならログ用に ex を残す
                MessageBox.Show("データベースエラーが発生しました。入力内容を確認して、再度お試しください。",
                            "DBエラー", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
        End Select

    End Sub

    Private Sub txtCustomerCode_Leave(sender As Object, e As EventArgs) Handles txtCustomerCode.Leave
        Dim s = txtCustomerCode.Text.Trim()
        If s <> "" AndAlso Not RX_CODE_6.IsMatch(s) Then
            SetErr(txtCustomerCode, "半角数字6桁で入力してください")
            MessageBox.Show("顧客コードは半角数字6桁で入力してください。")
            txtCustomerCode.Focus()
            txtCustomerCode.SelectAll()
        Else
            ClearErr(txtCustomerCode)
        End If
    End Sub

    Private Sub ApplyCustomerCodeVisual(text As String)
        Dim s = If(text, "").Trim()

        If s = "" Then
            txtCustomerCode.BackColor = Color.White
            Return
        End If

        If Regex.IsMatch(s, "^\d{1,5}$") Then
            txtCustomerCode.BackColor = Color.FromArgb(255, 249, 230) ' 入力途中：黄
        ElseIf RX_CODE_6.IsMatch(s) Then
            txtCustomerCode.BackColor = Color.FromArgb(235, 255, 235) ' OK：緑
        Else
            txtCustomerCode.BackColor = Color.FromArgb(255, 235, 238) ' NG：赤
        End If
    End Sub

    '========================
    ' Phone (paste-safe)
    '========================
    Private Sub txtPhoneNumber_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtPhoneNumber.KeyPress
        If Char.IsControl(e.KeyChar) Then Return

        If Not (Char.IsDigit(e.KeyChar) OrElse e.KeyChar = "-"c) Then
            e.Handled = True
            Return
        End If

        If e.KeyChar = "-"c AndAlso txtPhoneNumber.SelectionStart = 0 AndAlso txtPhoneNumber.TextLength = 0 Then
            e.Handled = True
            Return
        End If

        Dim pos = txtPhoneNumber.SelectionStart
        If e.KeyChar = "-"c AndAlso pos > 0 AndAlso txtPhoneNumber.Text.Substring(pos - 1, 1) = "-" Then
            e.Handled = True
            Return
        End If
    End Sub

    Private Sub txtPhoneNumber_TextChanged(sender As Object, e As EventArgs) Handles txtPhoneNumber.TextChanged
        ApplyPhoneVisual()
    End Sub

    Private Sub txtPhoneNumber_Leave(sender As Object, e As EventArgs) Handles txtPhoneNumber.Leave
        Dim s = txtPhoneNumber.Text.Trim()
        If s = "" Then Return

        Dim normalized As String = ""
        If Not TryNormalizeAndValidatePhone(s, normalized) Then
            SetErr(txtPhoneNumber, "電話番号の形式が正しくありません")
            MessageBox.Show("電話番号の形式が正しくありません。（例：03-1234-5678 / 09012345678）")
            txtPhoneNumber.Focus()
            txtPhoneNumber.SelectAll()
            Return
        End If

        ClearErr(txtPhoneNumber)
        txtPhoneNumber.Text = normalized
    End Sub

    Private Sub ApplyPhoneVisual()
        Dim raw = If(txtPhoneNumber.Text, "")

        Dim t = raw.Replace("－", "-").Replace("ー", "-").Replace("―", "-").Replace("−", "-")
        Dim cleaned = Regex.Replace(t, "[^0-9\-]", "")
        cleaned = RX_MULTI_HYPHEN.Replace(cleaned, "-")

        If cleaned.Length > 13 Then cleaned = cleaned.Substring(0, 13)

        If cleaned <> raw Then
            Dim pos = txtPhoneNumber.SelectionStart
            txtPhoneNumber.Text = cleaned
            txtPhoneNumber.SelectionStart = Math.Min(pos, txtPhoneNumber.TextLength)
        End If

        If cleaned = "" Then
            ClearErr(txtPhoneNumber)
            txtPhoneNumber.BackColor = Color.White
            Return
        End If

        txtPhoneNumber.BackColor = Color.FromArgb(255, 249, 230)

        Dim normalized As String = ""
        If TryNormalizeAndValidatePhone(cleaned, normalized) Then
            ClearErr(txtPhoneNumber)
            txtPhoneNumber.BackColor = Color.FromArgb(235, 255, 235)
        Else
            SetErr(txtPhoneNumber, "電話番号の形式が正しくありません")
            txtPhoneNumber.BackColor = Color.FromArgb(255, 235, 238)
        End If
    End Sub

    '========================
    ' Instant validation (Strict) - NAME / ADDRESS / APT / Payment / Birth
    '========================
    Private Sub TxtCustomerName_TextChanged_Strict(sender As Object, e As EventArgs)
        Dim raw = txtCustomerName.Text
        Dim cleaned = NormalizeSpaces(RemoveControlChars(raw))

        If cleaned <> raw Then
            Dim pos = txtCustomerName.SelectionStart
            txtCustomerName.Text = cleaned
            txtCustomerName.SelectionStart = Math.Min(pos, txtCustomerName.TextLength)
        End If

        If cleaned = "" Then
            ClearErr(txtCustomerName)
            txtCustomerName.BackColor = Color.White
            Return
        End If

        If cleaned.Length > MAX_NAME Then
            SetErr(txtCustomerName, $"顧客名は最大{MAX_NAME}文字です")
            txtCustomerName.BackColor = Color.FromArgb(255, 235, 238)
            Return
        End If

        If Not RX_NAME_ALLOWED.IsMatch(cleaned) OrElse RX_NAME_ONLY_NUM.IsMatch(cleaned) Then
            SetErr(txtCustomerName, "使用できない文字が含まれています")
            txtCustomerName.BackColor = Color.FromArgb(255, 235, 238)
            Return
        End If

        If ContainsForbiddenWords(cleaned) Then
            SetErr(txtCustomerName, "禁止語が含まれています")
            txtCustomerName.BackColor = Color.FromArgb(255, 235, 238)
            Return
        End If

        ClearErr(txtCustomerName)
        txtCustomerName.BackColor = Color.FromArgb(235, 255, 235)
    End Sub

    Private Sub TxtAddress_TextChanged_Strict(sender As Object, e As EventArgs)
        Dim raw = txtAddress.Text
        Dim cleaned = NormalizeSpaces(RemoveControlChars(raw))

        If cleaned <> raw Then
            Dim pos = txtAddress.SelectionStart
            txtAddress.Text = cleaned
            txtAddress.SelectionStart = Math.Min(pos, txtAddress.TextLength)
        End If

        extractedPrefecture = ExtractPrefecture(cleaned) ' 常に更新

        If cleaned = "" Then
            ClearErr(txtAddress)
            txtAddress.BackColor = Color.White
            Return
        End If

        If cleaned.Length > MAX_ADDRESS Then
            SetErr(txtAddress, $"住所は最大{MAX_ADDRESS}文字です")
            txtAddress.BackColor = Color.FromArgb(255, 235, 238)
            Return
        End If

        If Not RX_ADDR_ALLOWED.IsMatch(cleaned) Then
            SetErr(txtAddress, "住所に使用できない文字が含まれています")
            txtAddress.BackColor = Color.FromArgb(255, 235, 238)
            Return
        End If

        ' 都道府県が取れない場合は警告（保存はOK/NGどっちでも好み）
        If extractedPrefecture = "" Then
            SetErr(txtAddress, "都道府県名（○○県/東京都/大阪府/京都府 等）を住所に含めてください")
            txtAddress.BackColor = Color.FromArgb(255, 249, 230) ' 黄色：注意
            Return
        End If

        ClearErr(txtAddress)
        txtAddress.BackColor = Color.FromArgb(235, 255, 235)
    End Sub

    Private Sub TxtApartment_TextChanged_Strict(sender As Object, e As EventArgs)
        Dim raw = txtApartmentName.Text
        Dim cleaned = NormalizeSpaces(RemoveControlChars(raw))

        If cleaned <> raw Then
            Dim pos = txtApartmentName.SelectionStart
            txtApartmentName.Text = cleaned
            txtApartmentName.SelectionStart = Math.Min(pos, txtApartmentName.TextLength)
        End If

        If cleaned = "" Then
            ClearErr(txtApartmentName)
            txtApartmentName.BackColor = Color.White
            Return
        End If

        If cleaned.Length > MAX_APT Then
            SetErr(txtApartmentName, $"アパート名は最大{MAX_APT}文字です")
            txtApartmentName.BackColor = Color.FromArgb(255, 235, 238)
            Return
        End If

        If Not RX_ADDR_ALLOWED.IsMatch(cleaned) Then
            SetErr(txtApartmentName, "アパート名に使用できない文字が含まれています")
            txtApartmentName.BackColor = Color.FromArgb(255, 235, 238)
            Return
        End If

        ClearErr(txtApartmentName)
        txtApartmentName.BackColor = Color.FromArgb(235, 255, 235)
    End Sub

    Private Sub CmbPaymentMethod_Changed_Instant(sender As Object, e As EventArgs)
        If cmbPaymentMethod.SelectedIndex < 0 Then
            SetErr(cmbPaymentMethod, "支払方法を選択してください")
            Return
        End If
        Dim val = cmbPaymentMethod.SelectedItem.ToString()
        If Not cmbPaymentMethod.Items.Contains(val) Then
            SetErr(cmbPaymentMethod, "支払方法が不正です")
            Return
        End If
        ClearErr(cmbPaymentMethod)
    End Sub

    Private Sub BirthDate_Changed_Instant(sender As Object, e As EventArgs)
        If Not dtpBirthDate.Checked Then
            ClearErr(dtpBirthDate)
            Return
        End If

        Dim d = dtpBirthDate.Value.Date
        If d > Date.Today Then
            SetErr(dtpBirthDate, "生年月日に未来日は指定できません")
            Return
        End If

        Dim minDate = Date.Today.AddYears(-120)
        If d < minDate Then
            SetErr(dtpBirthDate, "生年月日が古すぎます")
            Return
        End If

        ClearErr(dtpBirthDate)
    End Sub

    '========================
    ' Final validate (before save)
    '========================
    Private Function ValidateAllInputs() As Boolean
        If err IsNot Nothing Then err.Clear()

        Dim code = txtCustomerCode.Text.Trim()
        If Not RX_CODE_6.IsMatch(code) Then
            SetErr(txtCustomerCode, "顧客コードは6桁の数字です")
            txtCustomerCode.Focus()
            txtCustomerCode.SelectAll()
            Return False
        End If

        Dim name = txtCustomerName.Text.Trim()
        If name = "" Then
            SetErr(txtCustomerName, "顧客名を入力してください")
            txtCustomerName.Focus()
            Return False
        End If
        If name.Length > MAX_NAME OrElse Not RX_NAME_ALLOWED.IsMatch(name) OrElse RX_NAME_ONLY_NUM.IsMatch(name) OrElse ContainsForbiddenWords(name) Then
            SetErr(txtCustomerName, "顧客名が不正です")
            txtCustomerName.Focus()
            txtCustomerName.SelectAll()
            Return False
        End If

        Dim phoneRaw = txtPhoneNumber.Text.Trim()
        Dim normalized As String = ""
        If phoneRaw = "" OrElse Not TryNormalizeAndValidatePhone(phoneRaw, normalized) Then
            SetErr(txtPhoneNumber, "電話番号が不正です")
            txtPhoneNumber.Focus()
            txtPhoneNumber.SelectAll()
            Return False
        End If

        Dim addr = txtAddress.Text.Trim()
        If addr <> "" Then
            If addr.Length > MAX_ADDRESS OrElse RX_CTRL.IsMatch(addr) OrElse Not RX_ADDR_ALLOWED.IsMatch(addr) Then
                SetErr(txtAddress, "住所が不正です")
                txtAddress.Focus()
                txtAddress.SelectAll()
                Return False
            End If
        End If

        ' 都道府県必須にするならここで必須化（今は必須にしてる）
        Dim pref = ExtractPrefecture(addr)
        If addr <> "" AndAlso pref = "" Then
            SetErr(txtAddress, "住所から都道府県を抽出できません（例：千葉県市川市...）")
            txtAddress.Focus()
            txtAddress.SelectAll()
            Return False
        End If

        Dim apt = txtApartmentName.Text.Trim()
        If apt <> "" Then
            If apt.Length > MAX_APT OrElse RX_CTRL.IsMatch(apt) OrElse Not RX_ADDR_ALLOWED.IsMatch(apt) Then
                SetErr(txtApartmentName, "アパート名が不正です")
                txtApartmentName.Focus()
                txtApartmentName.SelectAll()
                Return False
            End If
        End If

        If cmbPaymentMethod.SelectedIndex < 0 Then
            SetErr(cmbPaymentMethod, "支払方法を選択してください")
            cmbPaymentMethod.Focus()
            Return False
        End If

        If dtpBirthDate.Checked Then
            Dim d = dtpBirthDate.Value.Date
            If d > Date.Today Then
                SetErr(dtpBirthDate, "生年月日に未来日は指定できません")
                dtpBirthDate.Focus()
                Return False
            End If
            If d < Date.Today.AddYears(-120) Then
                SetErr(dtpBirthDate, "生年月日が古すぎます")
                dtpBirthDate.Focus()
                Return False
            End If
        End If

        ' 正規化を反映
        txtCustomerName.Text = NormalizeSpaces(RemoveControlChars(txtCustomerName.Text))
        txtAddress.Text = NormalizeSpaces(RemoveControlChars(txtAddress.Text))
        txtApartmentName.Text = NormalizeSpaces(RemoveControlChars(txtApartmentName.Text))
        txtPhoneNumber.Text = normalized

        Return True
    End Function

    '========================
    ' Domain validation
    '========================
    Private Function IsValidCustomerCode(code As String) As Boolean
        Return RX_CODE_6.IsMatch(If(code, ""))
    End Function

    Private Function IsValidCustomerName(name As String) As Boolean
        Dim s = If(name, "").Trim()
        If s = "" Then Return False
        If Not RX_NAME_ALLOWED.IsMatch(s) Then Return False
        If RX_NAME_ONLY_NUM.IsMatch(s) Then Return False
        If ContainsForbiddenWords(s) Then Return False
        Return True
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

    '========================
    ' DB
    '========================
    Private Function CustomerExists(conn As MySqlConnection, tx As MySqlTransaction, customerCode As String) As Boolean
        Dim sql As String = "SELECT COUNT(*) FROM customer_master WHERE customer_code = @code"
        Using cmd As New MySqlCommand(sql, conn, tx)
            cmd.Parameters.Add("@code", MySqlDbType.VarChar).Value = customerCode
            Dim cnt As Integer = Convert.ToInt32(cmd.ExecuteScalar())
            Return cnt > 0
        End Using
    End Function


    '========================
    ' Clear / Back
    '========================
    Private Sub ClearInputs()
        txtCustomerCode.Clear()
        txtCustomerName.Clear()
        txtPhoneNumber.Clear()
        txtAddress.Clear()
        txtApartmentName.Clear()
        cmbPaymentMethod.SelectedIndex = -1
        dtpBirthDate.Checked = False

        extractedPrefecture = ""

        ApplyCustomerCodeVisual("")
        ApplyPhoneVisual()

        txtCustomerCode.Focus()
    End Sub

    Private Sub btnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click
        If err IsNot Nothing Then err.Clear()
        ClearInputs()
    End Sub

    Private Sub btnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click
        Me.Close()
    End Sub

    '========================
    ' RoundedPanel
    '========================
    Private Class RoundedPanel
        Inherits Panel

        Public Property Radius As Integer = 16
        Public Property FillColor As Color = Color.White
        Public Property BorderColor As Color = Color.Gainsboro
        Public Property BorderWidth As Integer = 1

        Public Property ShadowColor As Color = Color.Black
        Public Property ShadowAlpha As Integer = 30
        Public Property ShadowOffset As Point = New Point(0, 6)

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias
            e.Graphics.Clear(Me.Parent.BackColor)

            Dim rectShadow As New Rectangle(ShadowOffset.X, ShadowOffset.Y, Me.Width - 1, Me.Height - 1)
            Using ps = RoundRect(rectShadow, Radius)
                Using sb As New SolidBrush(Color.FromArgb(ShadowAlpha, ShadowColor))
                    e.Graphics.FillPath(sb, ps)
                End Using
            End Using

            Dim rect As New Rectangle(0, 0, Me.Width - 1, Me.Height - 1)
            Using pth = RoundRect(rect, Radius)
                Using b As New SolidBrush(FillColor)
                    e.Graphics.FillPath(b, pth)
                End Using
                Using pen As New Pen(BorderColor, BorderWidth)
                    e.Graphics.DrawPath(pen, pth)
                End Using
            End Using

            MyBase.OnPaint(e)
        End Sub

        Private Function RoundRect(r As Rectangle, radius As Integer) As GraphicsPath
            Dim path As New GraphicsPath()
            Dim d As Integer = radius * 2
            path.AddArc(r.X, r.Y, d, d, 180, 90)
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90)
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90)
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90)
            path.CloseFigure()
            Return path
        End Function
    End Class

End Class
