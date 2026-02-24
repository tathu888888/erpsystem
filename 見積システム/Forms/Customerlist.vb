Imports MySql.Data.MySqlClient
Imports System.Windows.Forms
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Text.RegularExpressions
Imports System.Reflection

Public Class Customerlist

    Private ReadOnly connectionString As String =
        "Server=127.0.0.1;Port=3306;Database=sunstar;Uid=root;Pwd=1234;SslMode=Disabled;"

    ' ===== 追加UI（Designerに無いものだけ）=====
    Private titleLabel As Label
    Private btnClose As Button

    Private cardTop As RoundedPanel
    Private cardList As RoundedPanel

    Private txtSearch As TextBox
    Private lblCount As Label
    Private btnReload As Button

    ' ===== ページング =====
    Private Const PageSize As Integer = 8
    Private currentPage As Integer = 1
    Private totalRows As Integer = 0
    Private totalPages As Integer = 1

    ' ===== フッターUI =====
    Private cardFooter As RoundedPanel
    Private btnPrev As Button
    Private btnNext As Button
    Private btnDelete As Button
    Private lblPage As Label

    Private sortColumn As Integer = -1
    Private sortAscending As Boolean = True

    ' 全件（DBロード結果）
    Private allItems As New List(Of ListViewItem)
    ' フィルタ後の候補（ページング対象）
    Private viewItems As New List(Of ListViewItem)

    ' ★ページ番号リンク用
    Private pageLinksPanel As FlowLayoutPanel
    Private Const PageLinkMax As Integer = 9

    Private ReadOnly colorAccent As Color = Color.FromArgb(59, 130, 246)

    ' ========= バリデーション設定 =========
    ' 顧客コード：英数字 + _ - だけ、1〜20文字（必要なら調整）
    Private ReadOnly rxCustomerCode As New Regex("^[A-Za-z0-9_-]{1,20}$", RegexOptions.Compiled)

    ' 電話番号：数字とハイフンのみ、桁数は 10〜13（ハイフン含まず）を想定
    Private ReadOnly rxPhoneChars As New Regex("^[0-9\-]+$", RegexOptions.Compiled)

    ' 禁止文字（SQL/入力事故の温床になりやすいもの）
    Private ReadOnly ForbiddenChars As Char() = {ControlChars.NullChar, ControlChars.Cr, ControlChars.Lf, ControlChars.Tab,
                                                "'"c, """"c, ";"c, "\"c, "/"c, "|"c, "<"c, ">"c}

    ' バリデーション違反があったら一度だけアラートするため
    Private hasShownValidationAlert As Boolean = False

    ' フォームロード
    Private Sub Customerlist_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.FormBorderStyle = FormBorderStyle.None
        Me.BackColor = Color.FromArgb(245, 247, 250)
        Me.WindowState = FormWindowState.Maximized

        BuildUI_UsingDesignerListView() ' lvCustomersはDesignerのものを使う
        InitListView()

        AddHandler lvCustomers.ColumnClick, AddressOf lvCustomers_ColumnClick

        LoadCustomers()

        ' ★追加：削除ボタンの権限チェック（管理者のみ表示）
        ApplyDeletePermission()
    End Sub

    ' =========================
    ' UI構築（lvCustomersは作らない）
    ' =========================
    Private Sub lvCustomers_ColumnClick(sender As Object, e As ColumnClickEventArgs)
        If sortColumn = e.Column Then
            sortAscending = Not sortAscending
        Else
            sortColumn = e.Column
            sortAscending = True
        End If
        ApplySort()
    End Sub

    Private Sub ApplySort()
        If sortColumn < 0 OrElse viewItems Is Nothing OrElse viewItems.Count = 0 Then Return

        viewItems.Sort(Function(a, b)
                           Dim sa As String = GetSortableValue(a, sortColumn)
                           Dim sb As String = GetSortableValue(b, sortColumn)
                           Dim cmp As Integer = CompareSmart(sa, sb)
                           If Not sortAscending Then cmp = -cmp
                           Return cmp
                       End Function)

        currentPage = 1
        RenderPage()
    End Sub

    Private Sub BuildUI_UsingDesignerListView()

        ' --- Title ---
        titleLabel = New Label() With {
            .Text = "顧客一覧",
            .AutoSize = False,
            .Font = New Font("Segoe UI", 16, FontStyle.Bold),
            .ForeColor = Color.FromArgb(30, 41, 59),
            .Location = New Point(22, 16),
            .Size = New Size(400, 32)
        }

        btnClose = New Button() With {
            .Text = "×",
            .Font = New Font("Segoe UI", 14, FontStyle.Bold),
            .Size = New Size(44, 36),
            .Location = New Point(Me.ClientSize.Width - 60, 12),
            .FlatStyle = FlatStyle.Flat,
            .BackColor = Color.White,
            .ForeColor = Color.FromArgb(51, 65, 85),
            .Cursor = Cursors.Hand,
            .Anchor = AnchorStyles.Top Or AnchorStyles.Right
        }
        btnClose.FlatAppearance.BorderSize = 0
        AddHandler btnClose.Click, Sub() Me.Close()

        ' --- Top card ---
        cardTop = New RoundedPanel() With {
            .CornerRadius = 18,
            .BackColor = Color.White,
            .BorderColor = Color.FromArgb(226, 232, 240),
            .BorderThickness = 1,
            .Location = New Point(18, 62),
            .Size = New Size(Me.ClientSize.Width - 36, 78),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        }

        Dim lblSearch As New Label() With {
            .Text = "検索",
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .ForeColor = Color.FromArgb(51, 65, 85),
            .AutoSize = True,
            .Location = New Point(18, 14)
        }

        txtSearch = New TextBox() With {
            .Font = New Font("Segoe UI", 11, FontStyle.Regular),
            .Location = New Point(18, 36),
            .Size = New Size(360, 28)
        }
        AddHandler txtSearch.TextChanged, AddressOf ApplyFilter

        lblCount = New Label() With {
            .Text = "0 件",
            .Font = New Font("Segoe UI", 10, FontStyle.Regular),
            .ForeColor = Color.FromArgb(100, 116, 139),
            .AutoSize = True,
            .Location = New Point(400, 40)
        }

        btnReload = MakeAccentButton("更新", New Size(90, 34))
        btnReload.Location = New Point(cardTop.Width - 110, 26)
        btnReload.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        AddHandler btnReload.Click, Sub()
                                        txtSearch.Text = ""
                                        LoadCustomers()
                                    End Sub

        cardTop.Controls.Add(lblSearch)
        cardTop.Controls.Add(txtSearch)
        cardTop.Controls.Add(lblCount)
        cardTop.Controls.Add(btnReload)

        ' --- List card ---
        cardList = New RoundedPanel() With {
            .CornerRadius = 18,
            .BackColor = Color.White,
            .BorderColor = Color.FromArgb(226, 232, 240),
            .BorderThickness = 1,
            .Location = New Point(18, 154),
            .Size = New Size(Me.ClientSize.Width - 36, Me.ClientSize.Height - 154 - 74 - 12),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        }

        ' ★DesignerのlvCustomersをカードの中へ移動
        Me.Controls.Remove(lvCustomers)
        lvCustomers.Parent = cardList
        lvCustomers.Location = New Point(14, 14)
        lvCustomers.Size = New Size(cardList.Width - 28, cardList.Height - 28)
        lvCustomers.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right

        AddHandler cardList.Resize, Sub()
                                        lvCustomers.Size = New Size(cardList.Width - 28, cardList.Height - 28)
                                    End Sub

        ' --- Footer ---
        cardFooter = New RoundedPanel() With {
            .CornerRadius = 18,
            .BackColor = Color.White,
            .BorderColor = Color.FromArgb(226, 232, 240),
            .BorderThickness = 1,
            .Location = New Point(18, Me.ClientSize.Height - 74),
            .Size = New Size(Me.ClientSize.Width - 36, 56),
            .Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        }

        btnPrev = MakeAccentButton("← 前", New Size(90, 34))
        btnPrev.BackColor = Color.White
        btnPrev.ForeColor = Color.FromArgb(51, 65, 85)
        btnPrev.FlatAppearance.BorderSize = 1
        btnPrev.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240)
        btnPrev.Location = New Point(12, 11)

        btnDelete = MakeAccentButton("削除", New Size(90, 34))
        btnDelete.BackColor = Color.White
        btnDelete.ForeColor = Color.FromArgb(220, 38, 38)
        btnDelete.FlatAppearance.BorderSize = 1
        btnDelete.FlatAppearance.BorderColor = Color.FromArgb(254, 202, 202)
        btnDelete.Location = New Point(12 + btnPrev.Width + 10, 11)
        btnDelete.Enabled = False

        lblPage = New Label() With {
            .Text = "1/1（0件）",
            .AutoSize = True,
            .Font = New Font("Segoe UI", 10, FontStyle.Regular),
            .ForeColor = Color.FromArgb(100, 116, 139),
            .Location = New Point(12 + btnPrev.Width + 10 + btnDelete.Width + 16, 18)
        }

        btnNext = MakeAccentButton("次 →", New Size(90, 34))
        btnNext.Location = New Point(cardFooter.Width - btnNext.Width - 12, 11)
        btnNext.Anchor = AnchorStyles.Top Or AnchorStyles.Right

        ' --- ページ番号リンク（中央）
        pageLinksPanel = New FlowLayoutPanel() With {
            .AutoSize = False,
            .WrapContents = False,
            .FlowDirection = FlowDirection.LeftToRight,
            .Location = New Point(160, 14),
            .Size = New Size(cardFooter.Width - 160 - 160, 34),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right,
            .BackColor = Color.Transparent
        }
        cardFooter.Controls.Add(pageLinksPanel)

        cardFooter.Controls.Add(btnPrev)
        cardFooter.Controls.Add(btnDelete)
        cardFooter.Controls.Add(lblPage)
        cardFooter.Controls.Add(btnNext)

        LayoutFooter()

        AddHandler cardFooter.Resize, Sub()
                                          LayoutFooter()
                                      End Sub

        ' ★イベント
        AddHandler btnPrev.Click, AddressOf btnPrev_Click
        AddHandler btnNext.Click, AddressOf btnNext_Click
        AddHandler btnDelete.Click, AddressOf btnDelete_Click
        AddHandler lvCustomers.SelectedIndexChanged, AddressOf lvCustomers_SelectedIndexChanged

        ' --- Formへ追加 ---
        Me.Controls.Add(titleLabel)
        Me.Controls.Add(btnClose)
        Me.Controls.Add(cardTop)
        Me.Controls.Add(cardList)
        Me.Controls.Add(cardFooter)

        EnableFormDrag(Me, titleLabel)
        EnableFormDrag(Me, cardTop)
    End Sub

    Private Function GetSortableValue(it As ListViewItem, col As Integer) As String
        If it Is Nothing OrElse it.SubItems.Count <= col Then Return ""
        Return If(it.SubItems(col).Text, "").Trim()
    End Function

    Private Sub RebuildPageLinks()
        If pageLinksPanel Is Nothing Then Return

        pageLinksPanel.SuspendLayout()
        pageLinksPanel.Controls.Clear()

        Dim tp As Integer = Math.Max(1, totalPages)
        Dim cp As Integer = Math.Max(1, Math.Min(currentPage, tp))

        Dim half As Integer = PageLinkMax \ 2
        Dim startP As Integer = Math.Max(1, cp - half)
        Dim endP As Integer = Math.Min(tp, startP + PageLinkMax - 1)
        startP = Math.Max(1, endP - PageLinkMax + 1)

        If startP > 1 Then
            pageLinksPanel.Controls.Add(MakePageLink(1))
            If startP > 2 Then
                pageLinksPanel.Controls.Add(MakeDotsLabel())
            End If
        End If

        For p As Integer = startP To endP
            pageLinksPanel.Controls.Add(MakePageLink(p))
        Next

        If endP < tp Then
            If endP < tp - 1 Then
                pageLinksPanel.Controls.Add(MakeDotsLabel())
            End If
            pageLinksPanel.Controls.Add(MakePageLink(tp))
        End If

        pageLinksPanel.ResumeLayout()
    End Sub

    Private Function MakeDotsLabel() As Control
        Return New Label() With {
            .Text = "…",
            .AutoSize = True,
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .ForeColor = Color.FromArgb(120, 120, 120),
            .Margin = New Padding(6, 6, 6, 0)
        }
    End Function

    Private Function MakePageLink(pageNo As Integer) As Control
        Dim isCurrent = (pageNo = currentPage)

        If isCurrent Then
            Return New Label() With {
                .Text = pageNo.ToString(),
                .AutoSize = True,
                .Font = New Font("Segoe UI", 10, FontStyle.Bold),
                .ForeColor = colorAccent,
                .Margin = New Padding(6, 6, 6, 0)
            }
        End If

        Dim link As New LinkLabel() With {
            .Text = pageNo.ToString(),
            .AutoSize = True,
            .Font = New Font("Segoe UI", 10, FontStyle.Regular),
            .LinkColor = Color.FromArgb(60, 60, 60),
            .ActiveLinkColor = colorAccent,
            .VisitedLinkColor = Color.FromArgb(60, 60, 60),
            .Margin = New Padding(6, 6, 6, 0),
            .Tag = pageNo
        }

        AddHandler link.LinkClicked, Sub(sender As Object, e As LinkLabelLinkClickedEventArgs)
                                         Dim p As Integer = CInt(DirectCast(sender, Control).Tag)
                                         If p <> currentPage Then
                                             currentPage = p
                                             RenderPage()
                                         End If
                                     End Sub
        Return link
    End Function

    Private Function CompareSmart(a As String, b As String) As Integer
        a = If(a, "")
        b = If(b, "")

        If a = "" AndAlso b = "" Then Return 0
        If a = "" Then Return 1
        If b = "" Then Return -1

        Dim da As DateTime, db As DateTime
        If DateTime.TryParse(a, da) AndAlso DateTime.TryParse(b, db) Then
            Return DateTime.Compare(da, db)
        End If

        Dim na As Decimal, nb As Decimal
        If Decimal.TryParse(a, na) AndAlso Decimal.TryParse(b, nb) Then
            Return Decimal.Compare(na, nb)
        End If

        Return String.Compare(a, b, StringComparison.CurrentCultureIgnoreCase)
    End Function

    Private Function MakeAccentButton(text As String, size As Size) As Button
        Dim b As New Button() With {
            .Text = text,
            .Size = size,
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .FlatStyle = FlatStyle.Flat,
            .BackColor = Color.FromArgb(59, 130, 246),
            .ForeColor = Color.White,
            .Cursor = Cursors.Hand
        }
        b.FlatAppearance.BorderSize = 0
        Return b
    End Function

    ' =========================
    ' ListView 初期化（10項目）
    ' =========================
    Private Sub InitListView()
        lvCustomers.View = View.Details
        lvCustomers.FullRowSelect = True
        lvCustomers.GridLines = False
        lvCustomers.HideSelection = False
        lvCustomers.MultiSelect = False
        lvCustomers.Font = New Font("Segoe UI", 10, FontStyle.Regular)

        EnableDoubleBuffer(lvCustomers)

        lvCustomers.Columns.Clear()
        lvCustomers.Columns.Add("顧客コード", 110, HorizontalAlignment.Left)
        lvCustomers.Columns.Add("顧客名", 150, HorizontalAlignment.Left)
        lvCustomers.Columns.Add("電話番号", 130, HorizontalAlignment.Left)
        lvCustomers.Columns.Add("住所", 260, HorizontalAlignment.Left)
        lvCustomers.Columns.Add("アパート名など", 170, HorizontalAlignment.Left)
        lvCustomers.Columns.Add("生年月日", 110, HorizontalAlignment.Left)
        lvCustomers.Columns.Add("支払方法", 90, HorizontalAlignment.Left)
        lvCustomers.Columns.Add("購入開始日", 110, HorizontalAlignment.Left)
        lvCustomers.Columns.Add("購入目的", 180, HorizontalAlignment.Left)
        lvCustomers.Columns.Add("定期状況", 110, HorizontalAlignment.Left)

        AddHandler lvCustomers.ItemActivate, AddressOf lvCustomers_ItemActivate

        lvCustomers.OwnerDraw = True
        AddHandler lvCustomers.DrawColumnHeader, AddressOf Lv_DrawColumnHeader
        AddHandler lvCustomers.DrawItem, AddressOf Lv_DrawItem
        AddHandler lvCustomers.DrawSubItem, AddressOf Lv_DrawSubItem
    End Sub

    ' =========================
    ' DB: customer_master + 最新subscription をJOINして表示
    '   + 顧客コード/電話番号のバリデーション違反はアラート
    ' =========================
    Private Sub LoadCustomers()
        lvCustomers.BeginUpdate()
        lvCustomers.Items.Clear()
        allItems.Clear()
        viewItems.Clear()

        Dim invalidMessages As New List(Of String)

        Dim sql As String =
            "SELECT " &
            " c.customer_code, c.customer_name, c.phone_number, c.address, c.apartment_name, " &
            " c.birth_date, c.payment_method, c.purchase_start, c.purchase_reason, " &
            " CASE " &
            "   WHEN s.status IS NULL THEN '定期なし' " &
            "   WHEN s.status IN ('active','ACTIVE') THEN '定期購読中' " &
            "   WHEN s.status IN ('paused','PAUSED') THEN '一時停止' " &
            "   WHEN s.status IN ('canceled','CANCELED','cancelled','CANCELLED') THEN '解約' " &
            "   WHEN s.status IN ('ended','ENDED','expired','EXPIRED') THEN '終了' " &
            "   ELSE CONCAT('不明(', s.status, ')') " &
            " END AS subscription_status_jp " &
            "FROM customer_master c " &
            "LEFT JOIN ( " &
            "   SELECT x.customer_code, x.status " &
            "   FROM subscriptions x " &
            "   INNER JOIN ( " &
            "       SELECT customer_code, MAX(subscription_id) AS max_id " &
            "       FROM subscriptions " &
            "       GROUP BY customer_code " &
            "   ) latest ON latest.customer_code = x.customer_code AND latest.max_id = x.subscription_id " &
            ") s ON s.customer_code = c.customer_code " &
            "ORDER BY c.customer_code"

        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()
                Using cmd As New MySqlCommand(sql, conn)
                    Using rdr As MySqlDataReader = cmd.ExecuteReader()
                        While rdr.Read()
                            Dim customerCode As String = SafeStr(rdr("customer_code")).Trim()
                            Dim phone As String = SafeStr(rdr("phone_number")).Trim()

                            ' ★バリデーション
                            Dim err As String = ValidateKeyFields(customerCode, phone)
                            If err <> "" Then
                                invalidMessages.Add($"[{customerCode}] {err}（電話:{phone}）")
                            End If

                            Dim item As New ListViewItem(customerCode)
                            item.SubItems.Add(SafeStr(rdr("customer_name")))
                            item.SubItems.Add(phone)
                            item.SubItems.Add(SafeStr(rdr("address")))
                            item.SubItems.Add(SafeStr(rdr("apartment_name")))
                            item.SubItems.Add(SafeDate(rdr("birth_date")))
                            item.SubItems.Add(SafeStr(rdr("payment_method")))
                            item.SubItems.Add(SafeDate(rdr("purchase_start")))
                            item.SubItems.Add(SafeStr(rdr("purchase_reason")))
                            item.SubItems.Add(SafeStr(rdr("subscription_status_jp")))

                            allItems.Add(item)
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("顧客一覧の読み込みに失敗しました: " & ex.Message, "DBエラー", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

        viewItems.AddRange(allItems)
        currentPage = 1

        If sortColumn >= 0 Then
            ApplySort()
        Else
            RenderPage()
        End If

        lvCustomers.EndUpdate()
        UpdateCountAndPager()

        ' ★違反があればアラート（多すぎると邪魔なので先頭だけ表示）
        If invalidMessages.Count > 0 AndAlso Not hasShownValidationAlert Then
            hasShownValidationAlert = True

            Dim showN As Integer = Math.Min(15, invalidMessages.Count)
            Dim body As String = String.Join(Environment.NewLine, invalidMessages.Take(showN))

            Dim tail As String = If(invalidMessages.Count > showN,
                                    Environment.NewLine & $"…他 {invalidMessages.Count - showN} 件",
                                    "")

            MessageBox.Show(
                "顧客コード / 電話番号にバリデーション違反が見つかりました。" & Environment.NewLine &
                "（登録データの修正を推奨）" & Environment.NewLine & Environment.NewLine &
                body & tail,
                "バリデーション警告",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )
        End If
    End Sub

    ' =========================
    ' ★顧客コード/電話番号バリデーション
    ' =========================
    Private Function ValidateKeyFields(customerCode As String, phone As String) As String
        ' 顧客コード
        If customerCode = "" Then
            Return "顧客コードが空です"
        End If
        If ContainsForbidden(customerCode) Then
            Return "顧客コードに禁止文字が含まれます"
        End If
        If Not rxCustomerCode.IsMatch(customerCode) Then
            Return "顧客コード形式が不正です（英数字と _ - のみ/最大20）"
        End If

        ' 電話番号（空を許すならここを調整）
        If phone = "" Then
            Return "電話番号が空です"
        End If
        If ContainsForbidden(phone) Then
            Return "電話番号に禁止文字が含まれます"
        End If
        If Not rxPhoneChars.IsMatch(phone) Then
            Return "電話番号形式が不正です（数字とハイフンのみ）"
        End If

        Dim digits As String = phone.Replace("-", "")
        If digits.Length < 10 OrElse digits.Length > 13 OrElse Not digits.All(Function(ch) Char.IsDigit(ch)) Then
            Return "電話番号の桁数が不正です（10〜13桁）"
        End If

        Return ""
    End Function

    Private Function ContainsForbidden(s As String) As Boolean
        If s Is Nothing Then Return False
        For Each ch As Char In ForbiddenChars
            If s.IndexOf(ch) >= 0 Then Return True
        Next
        Return False
    End Function

    ' =========================
    ' ページ描画
    ' =========================
    Private Sub RenderPage()
        lvCustomers.BeginUpdate()
        lvCustomers.Items.Clear()

        totalRows = viewItems.Count
        totalPages = Math.Max(1, CInt(Math.Ceiling(totalRows / CDbl(PageSize))))
        If currentPage > totalPages Then currentPage = totalPages
        If currentPage < 1 Then currentPage = 1

        Dim startIdx As Integer = (currentPage - 1) * PageSize
        Dim endIdx As Integer = Math.Min(startIdx + PageSize, totalRows)

        For i As Integer = startIdx To endIdx - 1
            lvCustomers.Items.Add(DirectCast(viewItems(i).Clone(), ListViewItem))
        Next

        lvCustomers.EndUpdate()
        UpdateCountAndPager()
    End Sub

    Private Sub UpdateCountAndPager()
        If lblCount IsNot Nothing Then
            lblCount.Text = $"{viewItems.Count} 件"
        End If

        If lblPage IsNot Nothing Then
            lblPage.Text = $"{currentPage}/{totalPages}（{viewItems.Count}件）"
            RebuildPageLinks()
        End If

        If btnPrev IsNot Nothing Then btnPrev.Enabled = (currentPage > 1)
        If btnNext IsNot Nothing Then btnNext.Enabled = (currentPage < totalPages)
        If btnDelete IsNot Nothing Then btnDelete.Enabled = (lvCustomers.SelectedItems.Count > 0)
    End Sub

    ' =========================
    ' 検索フィルタ
    ' =========================
    Private Sub ApplyFilter(sender As Object, e As EventArgs)
        Dim q As String = If(txtSearch.Text, "").Trim()
        viewItems.Clear()

        If q = "" Then
            viewItems.AddRange(allItems)
        Else
            Dim lower As String = q.ToLowerInvariant()
            For Each it In allItems
                Dim hit As Boolean = False
                For i As Integer = 0 To it.SubItems.Count - 1
                    Dim t As String = If(it.SubItems(i).Text, "")
                    If t.ToLowerInvariant().Contains(lower) Then
                        hit = True
                        Exit For
                    End If
                Next
                If hit Then viewItems.Add(it)
            Next
        End If

        currentPage = 1

        If sortColumn >= 0 Then
            ApplySort()
        Else
            RenderPage()
        End If
    End Sub

    ' =========================
    ' 詳細へ（ダブルクリック/Enter）
    ' =========================
    Private Sub lvCustomers_ItemActivate(sender As Object, e As EventArgs)
        If lvCustomers.SelectedItems.Count = 0 Then Return
        Dim it As ListViewItem = lvCustomers.SelectedItems(0)

        Dim f As New Customerlistdetail()
        f.CustomerCode = it.SubItems(0).Text
        f.ShowDialog()

        ' 再読み込み
        hasShownValidationAlert = False ' 更新後も再通知したい場合
        LoadCustomers()
        ApplyDeletePermission()
    End Sub

    ' =========================
    ' Prev/Next
    ' =========================
    Private Sub btnPrev_Click(sender As Object, e As EventArgs)
        If currentPage > 1 Then
            currentPage -= 1
            RenderPage()
        End If
    End Sub

    Private Sub btnNext_Click(sender As Object, e As EventArgs)
        If currentPage < totalPages Then
            currentPage += 1
            RenderPage()
        End If
    End Sub

    ' =========================
    ' 削除
    ' =========================
    Private Sub btnDelete_Click(sender As Object, e As EventArgs)
        If Not AppSession2.IsAdmin Then
            MessageBox.Show("削除権限がありません。")
            Return
        End If

        If lvCustomers.SelectedItems.Count = 0 Then Return
        Dim code As String = lvCustomers.SelectedItems(0).SubItems(0).Text
        If code = "" Then Return

        Dim ok = MessageBox.Show($"顧客コード {code} を削除しますか？", "削除確認",
                                 MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
        If ok <> DialogResult.Yes Then Return

        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()
                Dim sql As String = "DELETE FROM customer_master WHERE customer_code=@code"
                Using cmd As New MySqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@code", code)
                    Dim affected = cmd.ExecuteNonQuery()
                    MessageBox.Show($"削除しました（{affected}件）")
                End Using
            End Using

            hasShownValidationAlert = False
            LoadCustomers()
            ApplyDeletePermission()

        Catch ex As Exception
            MessageBox.Show("削除に失敗しました: " & ex.Message)
        End Try
    End Sub

    Private Sub lvCustomers_SelectedIndexChanged(sender As Object, e As EventArgs)
        If btnDelete Is Nothing Then Return

        If Not AppSession2.IsAdmin Then
            btnDelete.Enabled = False
            Return
        End If

        btnDelete.Enabled = (lvCustomers.SelectedItems.Count > 0)
    End Sub

    ' =========================
    ' NULL対策
    ' =========================
    Private Function SafeStr(v As Object) As String
        If v Is Nothing OrElse v Is DBNull.Value Then Return ""
        Return v.ToString()
    End Function

    Private Function SafeDate(v As Object) As String
        If v Is Nothing OrElse v Is DBNull.Value Then Return ""
        Dim d As DateTime
        If DateTime.TryParse(v.ToString(), d) Then
            Return d.ToString("yyyy-MM-dd")
        End If
        Return ""
    End Function

    ' =========================
    ' OwnerDraw（今っぽい）
    ' =========================
    Private Sub Lv_DrawColumnHeader(sender As Object, e As DrawListViewColumnHeaderEventArgs)
        Using bg As New SolidBrush(Color.FromArgb(248, 250, 252))
            e.Graphics.FillRectangle(bg, e.Bounds)
        End Using
        Using p As New Pen(Color.FromArgb(226, 232, 240))
            e.Graphics.DrawLine(p, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1)
        End Using
        TextRenderer.DrawText(e.Graphics, e.Header.Text, New Font("Segoe UI", 9, FontStyle.Bold),
                              New Rectangle(e.Bounds.X + 10, e.Bounds.Y + 6, e.Bounds.Width - 10, e.Bounds.Height),
                              Color.FromArgb(51, 65, 85), TextFormatFlags.Left Or TextFormatFlags.VerticalCenter)
    End Sub

    Private Sub Lv_DrawItem(sender As Object, e As DrawListViewItemEventArgs)
        Dim selected As Boolean = e.Item.Selected
        Dim bgColor As Color = If(selected, Color.FromArgb(219, 234, 254), Color.White)

        Using bg As New SolidBrush(bgColor)
            e.Graphics.FillRectangle(bg, e.Bounds)
        End Using

        Using p As New Pen(Color.FromArgb(241, 245, 249))
            e.Graphics.DrawLine(p, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1)
        End Using
    End Sub

    Private Sub Lv_DrawSubItem(sender As Object, e As DrawListViewSubItemEventArgs)
        Dim selected As Boolean = e.Item.Selected
        Dim textColor As Color = If(selected, Color.FromArgb(30, 64, 175), Color.FromArgb(15, 23, 42))
        Dim font As Font = If(e.ColumnIndex = 0, New Font("Segoe UI", 10, FontStyle.Bold), New Font("Segoe UI", 10, FontStyle.Regular))

        Dim r As Rectangle = e.Bounds
        r.X += 10
        r.Width -= 10

        TextRenderer.DrawText(e.Graphics, e.SubItem.Text, font, r, textColor,
                              TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
    End Sub

    ' =========================
    ' チラつき防止
    ' =========================
    Private Sub EnableDoubleBuffer(ctrl As Control)
        Dim t As Type = ctrl.GetType()
        Dim pi = t.GetProperty("DoubleBuffered", BindingFlags.Instance Or BindingFlags.NonPublic)
        If pi IsNot Nothing Then pi.SetValue(ctrl, True, Nothing)
    End Sub

    ' =========================
    ' ドラッグ移動
    ' =========================
    Private dragging As Boolean = False
    Private dragStart As Point

    Private Sub EnableFormDrag(targetForm As Form, dragHandle As Control)
        AddHandler dragHandle.MouseDown, Sub(s, e)
                                             If e.Button = MouseButtons.Left Then
                                                 dragging = True
                                                 dragStart = New Point(e.X, e.Y)
                                             End If
                                         End Sub
        AddHandler dragHandle.MouseMove, Sub(s, e)
                                             If dragging Then
                                                 Dim p = targetForm.PointToScreen(New Point(e.X, e.Y))
                                                 targetForm.Location = New Point(p.X - dragStart.X, p.Y - dragStart.Y)
                                             End If
                                         End Sub
        AddHandler dragHandle.MouseUp, Sub(s, e) dragging = False
    End Sub

    ' =========================
    ' 権限チェック：削除は管理者のみ
    ' =========================
    Private Sub ApplyDeletePermission()
        If Not AppSession2.IsLoggedIn Then
            If btnDelete IsNot Nothing Then
                btnDelete.Visible = False
                btnDelete.Enabled = False
            End If
            Return
        End If

        If AppSession2.IsAdmin Then
            If btnDelete IsNot Nothing Then
                btnDelete.Visible = True
                btnDelete.Enabled = (lvCustomers IsNot Nothing AndAlso lvCustomers.SelectedItems.Count > 0)
            End If
        Else
            If btnDelete IsNot Nothing Then
                btnDelete.Visible = False
                btnDelete.Enabled = False
            End If
        End If
    End Sub

    Private Sub LayoutFooter()
        If cardFooter Is Nothing Then Return
        If btnPrev Is Nothing OrElse btnDelete Is Nothing OrElse btnNext Is Nothing Then Return
        If pageLinksPanel Is Nothing Then Return

        btnNext.Location = New Point(cardFooter.Width - btnNext.Width - 12, 11)

        Dim leftEnd As Integer = btnDelete.Right + 12
        Dim rightStart As Integer = btnNext.Left - 12
        Dim avail As Integer = Math.Max(0, rightStart - leftEnd)

        Dim w As Integer = Math.Min(520, avail)
        pageLinksPanel.Size = New Size(w, 34)
        pageLinksPanel.Location = New Point(leftEnd + (avail - w) \ 2, 11)

        lblPage.AutoSize = True
        lblPage.Location = New Point(pageLinksPanel.Right + 12, 18)

        If lblPage.Right > rightStart Then
            lblPage.Visible = False
        Else
            lblPage.Visible = True
        End If

        pageLinksPanel.BringToFront()
        lblPage.BringToFront()
    End Sub

End Class

' =========================
' 角丸パネル（カード）
' =========================
Public Class RoundedPanel
    Inherits Panel

    Public Property CornerRadius As Integer = 18
    Public Property BorderColor As Color = Color.FromArgb(226, 232, 240)
    Public Property BorderThickness As Integer = 1

    Public Sub New()
        Me.DoubleBuffered = True
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias

        Dim rect As Rectangle = Me.ClientRectangle
        rect.Width -= 1
        rect.Height -= 1

        Using path As GraphicsPath = GetRoundRectPath(rect, CornerRadius)
            Using b As New SolidBrush(Me.BackColor)
                e.Graphics.FillPath(b, path)
            End Using
            Using p As New Pen(BorderColor, BorderThickness)
                e.Graphics.DrawPath(p, path)
            End Using
        End Using
    End Sub

    Private Function GetRoundRectPath(r As Rectangle, radius As Integer) As GraphicsPath
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
