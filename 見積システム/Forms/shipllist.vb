Imports System.Windows.Forms
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports MySql.Data.MySqlClient
Imports System.Collections.Generic

Public Class shipllist
    Inherits Form

    ' =========================
    ' DB接続
    ' =========================
    Private ReadOnly connectionString As String =
        "Server=127.0.0.1;Port=3306;Database=sunstar;Uid=root;Pwd=1234;SslMode=Disabled;"

    ' =========================
    ' ページング
    ' =========================
    Private Const PageSize As Integer = 8
    Private currentPage As Integer = 1
    Private totalRows As Integer = 0
    Private totalPages As Integer = 1

    Private pageLinksPanel As FlowLayoutPanel
    Private Const PageLinkMax As Integer = 9

    ' =========================
    ' Controls（Designerを使わない）
    ' =========================
    Private titleLabel As Label
    Private shutdownBtn As Button

    Private cardFilter As UiCardPanel
    Private cardList As UiCardPanel
    Private cardFooter As UiCardPanel

    Friend WithEvents listViewEstimates As ListView
    Friend WithEvents cmbMonth As ComboBox
    Friend WithEvents cmbPref As ComboBox
    Friend WithEvents rbMonth As RadioButton
    Friend WithEvents rbPref As RadioButton
    Friend WithEvents rbMonthPref As RadioButton
    Friend WithEvents btnPrev As Button
    Friend WithEvents btnNext As Button
    Friend WithEvents btnDelete As Button

    Friend WithEvents lblPage As Label

    ' フォント/色
    Private ReadOnly fontTitle As New Font("Segoe UI", 16, FontStyle.Bold)
    Private ReadOnly fontLabel As New Font("Segoe UI", 10, FontStyle.Bold)
    Private ReadOnly fontInput As New Font("Segoe UI", 10, FontStyle.Regular)

    Private ReadOnly colorBg As Color = Color.FromArgb(245, 246, 250)
    Private ReadOnly colorCard As Color = Color.White
    Private ReadOnly colorBorder As Color = Color.FromArgb(220, 224, 232)
    Private ReadOnly colorAccent As Color = Color.FromArgb(65, 105, 225) ' RoyalBlue
    Private ReadOnly colorText As Color = Color.FromArgb(25, 25, 25)

    ' =========================
    ' Load
    ' =========================
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)

        BuildUI()
        InitializeListView()
        InitFilters()

        rbMonth.Checked = True
        RefreshList()

        ApplyDeletePermission()
    End Sub

    ' =========================
    ' UI構築（カード型）
    ' =========================
    Private Sub BuildUI()
        Me.Text = "発送一覧（shipllist）"
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = colorBg
        Me.Font = fontInput
        Me.MinimumSize = New Size(980, 620)
        Me.DoubleBuffered = True
        Me.WindowState = FormWindowState.Maximized

        ' --- タイトル
        titleLabel = New Label() With {
            .Text = "発送一覧",
            .Font = fontTitle,
            .AutoSize = True,
            .ForeColor = colorText,
            .Location = New Point(24, 18)
        }
        Me.Controls.Add(titleLabel)

        ' --- 閉じる（右上）
        shutdownBtn = New Button() With {
            .Text = "×",
            .Font = New Font("Segoe UI", 14, FontStyle.Bold),
            .Size = New Size(44, 36),
            .FlatStyle = FlatStyle.Flat,
            .BackColor = Color.White,
            .ForeColor = Color.FromArgb(90, 90, 90),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Right
        }
        shutdownBtn.FlatAppearance.BorderColor = colorBorder
        shutdownBtn.FlatAppearance.BorderSize = 1
        shutdownBtn.Location = New Point(Me.ClientSize.Width - shutdownBtn.Width - 18, 16)
        AddHandler shutdownBtn.Click, Sub() Me.Close()
        Me.Controls.Add(shutdownBtn)

        AddHandler Me.Resize, Sub()
                                  shutdownBtn.Location = New Point(Me.ClientSize.Width - shutdownBtn.Width - 18, 16)
                              End Sub

        ' --- フィルタカード
        cardFilter = New UiCardPanel() With {
            .BackColor = colorCard,
            .BorderColor = colorBorder,
            .Radius = 16,
            .Location = New Point(18, 70),
            .Size = New Size(Me.ClientSize.Width - 36, 96),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        }
        Me.Controls.Add(cardFilter)

        Dim lblMonth As New Label() With {.Text = "月", .Font = fontLabel, .AutoSize = True, .Location = New Point(18, 18), .ForeColor = colorText}
        cmbMonth = New ComboBox() With {.DropDownStyle = ComboBoxStyle.DropDownList, .Font = fontInput, .Location = New Point(18, 44), .Size = New Size(120, 30)}

        Dim lblPref As New Label() With {.Text = "都道府県", .Font = fontLabel, .AutoSize = True, .Location = New Point(160, 18), .ForeColor = colorText}
        cmbPref = New ComboBox() With {.DropDownStyle = ComboBoxStyle.DropDownList, .Font = fontInput, .Location = New Point(160, 44), .Size = New Size(180, 30)}

        rbMonth = New RadioButton() With {.Text = "月別", .Font = fontInput, .AutoSize = True, .Location = New Point(370, 46)}
        rbPref = New RadioButton() With {.Text = "都道府県別", .Font = fontInput, .AutoSize = True, .Location = New Point(450, 46)}
        rbMonthPref = New RadioButton() With {.Text = "月＋都道府県", .Font = fontInput, .AutoSize = True, .Location = New Point(590, 46)}

        cardFilter.Controls.Add(lblMonth)
        cardFilter.Controls.Add(cmbMonth)
        cardFilter.Controls.Add(lblPref)
        cardFilter.Controls.Add(cmbPref)
        cardFilter.Controls.Add(rbMonth)
        cardFilter.Controls.Add(rbPref)
        cardFilter.Controls.Add(rbMonthPref)

        ' --- Listカード
        cardList = New UiCardPanel() With {
            .BackColor = colorCard,
            .BorderColor = colorBorder,
            .Radius = 16,
            .Location = New Point(18, 180),
            .Size = New Size(Me.ClientSize.Width - 36, Me.ClientSize.Height - 180 - 90),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        }
        Me.Controls.Add(cardList)

        listViewEstimates = New ListView() With {
            .View = View.Details,
            .FullRowSelect = True,
            .HideSelection = False,
            .BorderStyle = BorderStyle.None,
            .Location = New Point(12, 12),
            .Size = New Size(cardList.Width - 24, cardList.Height - 24),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        }
        cardList.Controls.Add(listViewEstimates)

        ' --- フッターカード（ページング）
        cardFooter = New UiCardPanel() With {
            .BackColor = colorCard,
            .BorderColor = colorBorder,
            .Radius = 16,
            .Location = New Point(18, Me.ClientSize.Height - 80),
            .Size = New Size(Me.ClientSize.Width - 36, 62),
            .Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        }
        Me.Controls.Add(cardFooter)

        btnPrev = MakePrimaryButton("← 前", False)
        btnPrev.Location = New Point(14, 14)

        btnNext = MakePrimaryButton("次 →", True)
        btnNext.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnNext.Location = New Point(cardFooter.Width - btnNext.Width - 14, 14)

        lblPage = New Label() With {
            .Text = "1/1（0件）",
            .Font = fontInput,
            .AutoSize = True,
            .ForeColor = Color.FromArgb(60, 60, 60),
            .Location = New Point(160, 20)
        }

        cardFooter.Controls.Add(btnPrev)

        btnDelete = MakePrimaryButton("削除", False)
        btnDelete.Size = New Size(120, 34)
        btnDelete.Location = New Point(14 + btnPrev.Width + 12, 14)
        btnDelete.Enabled = False
        cardFooter.Controls.Add(btnDelete)

        cardFooter.Controls.Add(lblPage)
        cardFooter.Controls.Add(btnNext)

        ' --- ページ番号リンク（中央）
        pageLinksPanel = New FlowLayoutPanel() With {
            .AutoSize = False,
            .WrapContents = False,
            .FlowDirection = FlowDirection.LeftToRight,
            .BackColor = Color.Transparent,
            .Height = 34
        }
        cardFooter.Controls.Add(pageLinksPanel)

        AddHandler cardFooter.Resize, Sub()
                                          btnNext.Location = New Point(cardFooter.Width - btnNext.Width - 14, 14)
                                          LayoutShipListFooter()
                                      End Sub

        LayoutShipListFooter()

        ' ListView オーナードロー
        listViewEstimates.OwnerDraw = True
        AddHandler listViewEstimates.DrawColumnHeader, AddressOf Lv_DrawColumnHeader
        AddHandler listViewEstimates.DrawItem, AddressOf Lv_DrawItem
        AddHandler listViewEstimates.DrawSubItem, AddressOf Lv_DrawSubItem
        AddHandler listViewEstimates.DoubleClick, AddressOf listViewEstimates_DoubleClick

        ' ページング
        AddHandler btnPrev.Click, AddressOf btnPrev_Click
        AddHandler btnNext.Click, AddressOf btnNext_Click
        AddHandler btnDelete.Click, AddressOf btnDelete_Click
        AddHandler listViewEstimates.SelectedIndexChanged, AddressOf listViewEstimates_SelectedIndexChanged
    End Sub

    Private Sub LayoutShipListFooter()
        If cardFooter Is Nothing Then Return
        If btnPrev Is Nothing OrElse btnNext Is Nothing OrElse lblPage Is Nothing OrElse pageLinksPanel Is Nothing Then Return

        btnNext.Location = New Point(cardFooter.Width - btnNext.Width - 14, 14)

        Dim leftEnd As Integer
        If btnDelete IsNot Nothing AndAlso btnDelete.Visible Then
            leftEnd = btnDelete.Right + 12
        Else
            leftEnd = btnPrev.Right + 12
        End If

        lblPage.AutoSize = True
        lblPage.Location = New Point(leftEnd, 20)

        leftEnd = lblPage.Right + 12
        Dim rightStart As Integer = btnNext.Left - 12
        Dim avail As Integer = Math.Max(0, rightStart - leftEnd)

        Dim w As Integer = Math.Min(520, avail)
        pageLinksPanel.Size = New Size(w, 34)
        pageLinksPanel.Location = New Point(leftEnd + (avail - w) \ 2, 14)

        pageLinksPanel.BringToFront()
        lblPage.BringToFront()
    End Sub

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
            If startP > 2 Then pageLinksPanel.Controls.Add(MakeDotsLabel())
        End If

        For p As Integer = startP To endP
            pageLinksPanel.Controls.Add(MakePageLink(p))
        Next

        If endP < tp Then
            If endP < tp - 1 Then pageLinksPanel.Controls.Add(MakeDotsLabel())
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
                                             RefreshList()
                                         End If
                                     End Sub

        Return link
    End Function

    ' =========================
    ' 権限で削除ボタンを制御（管理者のみ）
    ' =========================
    Private Sub ApplyDeletePermission()
        If Not AppSession2.IsLoggedIn Then
            btnDelete.Visible = False
            btnDelete.Enabled = False
            Return
        End If

        If AppSession2.IsAdmin Then
            btnDelete.Visible = True
            btnDelete.Enabled = (listViewEstimates IsNot Nothing AndAlso listViewEstimates.SelectedItems.Count > 0)
        Else
            btnDelete.Visible = False
            btnDelete.Enabled = False
        End If
    End Sub

    Private Function MakePrimaryButton(text As String, primary As Boolean) As Button
        Dim b As New Button() With {
            .Text = text,
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .Size = New Size(120, 34),
            .FlatStyle = FlatStyle.Flat
        }
        b.FlatAppearance.BorderSize = 1
        b.FlatAppearance.BorderColor = If(primary, colorAccent, colorBorder)
        b.BackColor = If(primary, colorAccent, Color.White)
        b.ForeColor = If(primary, Color.White, Color.FromArgb(50, 50, 50))
        Return b
    End Function

    ' =========================
    ' ListView 初期化（列定義）
    ' =========================
    Private Sub InitializeListView()
        listViewEstimates.Columns.Clear()

        listViewEstimates.Columns.Add("トランザクションID", 150, HorizontalAlignment.Left)
        listViewEstimates.Columns.Add("発送日", 140, HorizontalAlignment.Left)
        listViewEstimates.Columns.Add("顧客コード", 120, HorizontalAlignment.Left)
        listViewEstimates.Columns.Add("顧客名", 160, HorizontalAlignment.Left)

        ' ★追加：商品名（代表）
        listViewEstimates.Columns.Add("商品名(代表)", 220, HorizontalAlignment.Left)

        listViewEstimates.Columns.Add("単位(代表)", 110, HorizontalAlignment.Left)
        listViewEstimates.Columns.Add("数量(合計)", 110, HorizontalAlignment.Right)
        listViewEstimates.Columns.Add("数量2(合計)", 110, HorizontalAlignment.Right)
        listViewEstimates.Columns.Add("単価", 90, HorizontalAlignment.Right)
        listViewEstimates.Columns.Add("金額(合計)", 120, HorizontalAlignment.Right)
        listViewEstimates.Columns.Add("都道府県", 120, HorizontalAlignment.Left)
        listViewEstimates.Columns.Add("備考", 280, HorizontalAlignment.Left)

        listViewEstimates.Font = New Font("Segoe UI", 10, FontStyle.Regular)
    End Sub
    ' =========================
    ' フィルタ初期化（年月・都道府県）
    ' =========================
    Private Sub InitFilters()
        cmbMonth.Items.Clear()
        For m As Integer = 1 To 12
            cmbMonth.Items.Add(m.ToString("00"))
        Next
        cmbMonth.SelectedIndex = Date.Today.Month - 1

        cmbPref.Items.Clear()
        cmbPref.Items.Add("（全て）")

        Dim sql As String =
            "SELECT DISTINCT prefecture " &
            "FROM customer_master " &
            "WHERE prefecture IS NOT NULL AND prefecture <> '' " &
            "ORDER BY prefecture"

        Using conn As New MySqlConnection(connectionString)
            conn.Open()
            Using cmd As New MySqlCommand(sql, conn)
                Using rdr = cmd.ExecuteReader()
                    While rdr.Read()
                        cmbPref.Items.Add(rdr("prefecture").ToString())
                    End While
                End Using
            End Using
        End Using

        cmbPref.SelectedIndex = 0

        AddHandler cmbMonth.SelectedIndexChanged, AddressOf FilterChanged
        AddHandler cmbPref.SelectedIndexChanged, AddressOf FilterChanged
        AddHandler rbMonth.CheckedChanged, AddressOf FilterChanged
        AddHandler rbPref.CheckedChanged, AddressOf FilterChanged
        AddHandler rbMonthPref.CheckedChanged, AddressOf FilterChanged
    End Sub

    Private Sub FilterChanged(sender As Object, e As EventArgs)
        currentPage = 1
        RefreshList()
    End Sub

    ' =========================
    ' 前/次ページ
    ' =========================
    Private Sub btnPrev_Click(sender As Object, e As EventArgs)
        If currentPage > 1 Then
            currentPage -= 1
            RefreshList()
        End If
    End Sub

    Private Sub btnNext_Click(sender As Object, e As EventArgs)
        If currentPage < totalPages Then
            currentPage += 1
            RefreshList()
        End If
    End Sub

    ' =========================
    ' ダブルクリックで detail を開く
    ' =========================
    Private Sub listViewEstimates_DoubleClick(sender As Object, e As EventArgs)
        If listViewEstimates.SelectedItems.Count = 0 Then Return

        Dim item = listViewEstimates.SelectedItems(0)
        Dim idText As String = item.SubItems(0).Text

        Dim batchId As Long
        If Not Long.TryParse(idText, batchId) Then
            MessageBox.Show("shipment_batch_id が取得できません。")
            Return
        End If

        Dim f As New shiplistdetail()
        f.ShipmentBatchId = batchId
        f.ShowDialog()

        RefreshList()
    End Sub

    ' =========================
    ' 一覧再描画（shipments_header + detail集計）
    ' =========================
    Private Sub RefreshList()
        If cmbMonth.SelectedItem Is Nothing OrElse cmbPref.SelectedItem Is Nothing Then Return

        listViewEstimates.BeginUpdate()
        listViewEstimates.Items.Clear()

        Dim whereSql As String = ""
        Dim params As New List(Of MySqlParameter)()

        Dim selectedMonth As Integer = Integer.Parse(cmbMonth.SelectedItem.ToString())
        Dim selectedPref As String = cmbPref.SelectedItem.ToString()
        Dim selectedYear As Integer = Date.Today.Year

        If rbMonth.Checked Then
            whereSql = "WHERE h.shipment_date IS NOT NULL AND YEAR(h.shipment_date)=@y AND MONTH(h.shipment_date)=@m"
            params.Add(New MySqlParameter("@y", selectedYear))
            params.Add(New MySqlParameter("@m", selectedMonth))

        ElseIf rbPref.Checked Then
            If selectedPref <> "（全て）" Then
                whereSql = "WHERE c.prefecture=@pref"
                params.Add(New MySqlParameter("@pref", selectedPref))
            Else
                whereSql = ""
            End If

        ElseIf rbMonthPref.Checked Then
            whereSql = "WHERE h.shipment_date IS NOT NULL AND YEAR(h.shipment_date)=@y AND MONTH(h.shipment_date)=@m"
            params.Add(New MySqlParameter("@y", selectedYear))
            params.Add(New MySqlParameter("@m", selectedMonth))

            If selectedPref <> "（全て）" Then
                whereSql &= " AND c.prefecture=@pref"
                params.Add(New MySqlParameter("@pref", selectedPref))
            End If
        End If

        ' COUNT（ヘッダ件数）
        Dim countSql As String =
            "SELECT COUNT(*) " &
            "FROM shipments_header h " &
            "LEFT JOIN customer_master c ON c.customer_code = h.customer_code " &
            whereSql

        Using conn As New MySqlConnection(connectionString)
            conn.Open()

            Using cmdCount As New MySqlCommand(countSql, conn)
                For Each p In params
                    cmdCount.Parameters.AddWithValue(p.ParameterName, p.Value)
                Next
                totalRows = Convert.ToInt32(cmdCount.ExecuteScalar())
            End Using

            totalPages = Math.Max(1, CInt(Math.Ceiling(totalRows / CDbl(PageSize))))
            If currentPage > totalPages Then currentPage = totalPages

            Dim offset As Integer = (currentPage - 1) * PageSize

            ' ★ヘッダ + 明細集計：unit / quantity2 追加
            Dim dataSql As String =
    "SELECT " &
    " h.shipment_batch_id, h.shipment_date, h.customer_code, " &
    " COALESCE(c.customer_name,'') AS customer_name, " &
    " COALESCE(c.prefecture,'') AS prefecture, " &
    " COUNT(d.shipment_detail_id) AS items, " &
    " COALESCE(SUM(d.quantity),0) AS total_quantity, " &
    " COALESCE(SUM(d.quantity2),0) AS total_quantity2, " &
    " CASE " &
    "   WHEN COUNT(d.shipment_detail_id)=0 THEN '' " &
    "   WHEN MIN(COALESCE(m.item_name,'')) = MAX(COALESCE(m.item_name,'')) THEN MIN(COALESCE(m.item_name,'')) " &
    "   ELSE NULL " &
    " END AS item_name_one, " &
    " CASE " &
    "   WHEN COUNT(d.shipment_detail_id)=0 THEN '' " &
    "   WHEN MIN(d.unit)=MAX(d.unit) THEN MIN(d.unit) " &
    "   ELSE NULL " &
    " END AS unit_one, " &
    " CASE " &
    "   WHEN COUNT(d.shipment_detail_id)=0 THEN '' " &
    "   WHEN MIN(d.unit_price)=MAX(d.unit_price) THEN MIN(d.unit_price) " &
    "   ELSE NULL " &
    " END AS unit_price_one, " &
    " COALESCE(SUM(d.amount),0) AS total_amount, " &
    " COALESCE(GROUP_CONCAT(NULLIF(d.remark,'') ORDER BY d.line_no SEPARATOR ' / '),'') AS remarks " &
    "FROM shipments_header h " &
    "LEFT JOIN customer_master c ON c.customer_code = h.customer_code " &
    "LEFT JOIN shipments_detail d ON d.shipment_batch_id = h.shipment_batch_id " &
    "LEFT JOIN item_master m ON m.item_id = d.item_id " &
    whereSql & " " &
    "GROUP BY h.shipment_batch_id, h.shipment_date, h.customer_code, c.customer_name, c.prefecture " &
    "ORDER BY h.shipment_date DESC, h.shipment_batch_id DESC " &
    "LIMIT @limit OFFSET @offset"



            Using cmd As New MySqlCommand(dataSql, conn)
                For Each p In params
                    cmd.Parameters.AddWithValue(p.ParameterName, p.Value)
                Next
                cmd.Parameters.AddWithValue("@limit", PageSize)
                cmd.Parameters.AddWithValue("@offset", offset)

                Using rdr = cmd.ExecuteReader()
                    While rdr.Read()
                        Dim lvItem As New ListViewItem(SafeStr(rdr("shipment_batch_id")))
                        lvItem.SubItems.Add(SafeDateStr(rdr("shipment_date")))
                        lvItem.SubItems.Add(SafeStr(rdr("customer_code")))
                        lvItem.SubItems.Add(SafeStr(rdr("customer_name")))

                        ' ★追加：商品名（代表）
                        Dim itemName As String
                        If rdr("item_name_one") Is DBNull.Value Then
                            itemName = "複数"
                        Else
                            itemName = SafeStr(rdr("item_name_one"))
                        End If
                        lvItem.SubItems.Add(itemName)

                        ' ★単位（代表）
                        Dim u As String
                        If rdr("unit_one") Is DBNull.Value Then
                            u = "複数"
                        Else
                            u = SafeStr(rdr("unit_one"))
                            If u = "" Then u = ""
                        End If
                        lvItem.SubItems.Add(u)

                        lvItem.SubItems.Add(SafeStr(rdr("total_quantity")))
                        lvItem.SubItems.Add(SafeStr(rdr("total_quantity2")))

                        Dim up As String
                        If rdr("unit_price_one") Is DBNull.Value Then
                            up = "複数"
                        Else
                            up = SafeStr(rdr("unit_price_one"))
                        End If
                        lvItem.SubItems.Add(up)

                        lvItem.SubItems.Add(SafeStr(rdr("total_amount")))
                        lvItem.SubItems.Add(SafeStr(rdr("prefecture")))
                        lvItem.SubItems.Add(SafeStr(rdr("remarks")))

                        listViewEstimates.Items.Add(lvItem)
                    End While

                End Using
            End Using
        End Using

        lblPage.Text = $"{currentPage}/{totalPages}（{totalRows}件）"
        RebuildPageLinks()
        LayoutShipListFooter()

        listViewEstimates.EndUpdate()
        listViewEstimates.Invalidate()
    End Sub

    ' =========================
    ' 選択変更
    ' =========================
    Private Sub listViewEstimates_SelectedIndexChanged(sender As Object, e As EventArgs)
        If btnDelete Is Nothing Then Return

        If Not AppSession2.IsAdmin Then
            btnDelete.Enabled = False
            Return
        End If

        btnDelete.Enabled = (listViewEstimates.SelectedItems.Count > 0)
    End Sub

    ' =========================
    ' 削除（ヘッダ削除 → CASCADEで明細削除）
    ' =========================
    ' =========================
    ' 削除（複数選択OK） + 在庫戻し（lot_unit等）
    ' =========================
    Private Sub btnDelete_Click(sender As Object, e As EventArgs)
        If Not AppSession2.IsAdmin Then
            MessageBox.Show("削除権限がありません。")
            Return
        End If
        If listViewEstimates.SelectedItems.Count = 0 Then
            MessageBox.Show("削除する行を選択してください。")
            Return
        End If

        ' 選択された shipment_batch_id を全て取得
        Dim batchIds As New List(Of Long)()
        For Each it As ListViewItem In listViewEstimates.SelectedItems
            Dim idText As String = it.SubItems(0).Text
            Dim id As Long
            If Long.TryParse(idText, id) Then
                batchIds.Add(id)
            End If
        Next

        If batchIds.Count = 0 Then
            MessageBox.Show("shipment_batch_id が取得できません。")
            Return
        End If

        Dim r = MessageBox.Show(
        $"選択した {batchIds.Count} 件を削除し、在庫を戻しますか？" & vbCrLf &
        "（明細・割当・台帳も関連して戻します）",
        "削除確認",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Warning
    )
        If r <> DialogResult.Yes Then Return

        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()

                Using tx = conn.BeginTransaction()
                    Try
                        For Each batchId In batchIds
                            RestoreInventoryAndDeleteShipment(conn, tx, batchId)
                        Next

                        tx.Commit()
                    Catch
                        tx.Rollback()
                        Throw
                    End Try
                End Using
            End Using

            RefreshList()

        Catch ex As Exception
            MessageBox.Show("削除に失敗しました: " & ex.Message)
        End Try
    End Sub

    ' =========================================================
    ' 1件分：在庫を戻す → 割当を消す → 台帳を消す/戻す → ヘッダ削除（CASCADEで明細削除）
    ' =========================================================
    ' =========================================================
    ' 1件分：在庫を戻す → 個体を戻す → 割当を消す → 台帳を消す/VOID → ヘッダ削除
    ' =========================================================
    Private Sub RestoreInventoryAndDeleteShipment(conn As MySqlConnection, tx As MySqlTransaction, batchId As Long)

        ' -------------------------
        ' 0) この出荷に紐づく unit_id（個体）を先に集める（最優先の根拠）
        ' -------------------------
        Dim unitIds As List(Of Long) = QueryLongListSafe(conn, tx,
        "SELECT sua.unit_id " &
        "FROM shipment_unit_alloc sua " &
        "JOIN shipments_detail d ON d.shipment_detail_id = sua.shipment_detail_id " &
        "WHERE d.shipment_batch_id = @bid",
        New Dictionary(Of String, Object) From {{"@bid", batchId}}
    )

        ' -------------------------
        ' 1) unitが取れるなら unit根拠で item / lot を戻す（最も確実）
        '    unitが無い出荷だけ shipments_detail SUM(quantity) で戻す（保険）
        ' -------------------------
        If unitIds IsNot Nothing AndAlso unitIds.Count > 0 Then

            ' (A) lot別に戻す個数 = unit数
            Dim lotAggFromUnit As New Dictionary(Of Long, Integer)()

            Using cmd As New MySqlCommand(
            "SELECT lu.lot_id, COUNT(*) AS qty " &
            "FROM lot_unit lu " &
            "WHERE lu.unit_id IN (" & String.Join(",", unitIds.Select(Function(x, i) "@u" & i)) & ") " &
            "GROUP BY lu.lot_id", conn, tx)

                For i As Integer = 0 To unitIds.Count - 1
                    cmd.Parameters.AddWithValue("@u" & i, unitIds(i))
                Next

                Using rdr = cmd.ExecuteReader()
                    While rdr.Read()
                        Dim lotId As Long = Convert.ToInt64(rdr("lot_id"))
                        Dim qty As Integer = Convert.ToInt32(rdr("qty"))
                        If qty > 0 Then lotAggFromUnit(lotId) = qty
                    End While
                End Using
            End Using

            If lotAggFromUnit.Count > 0 Then
                Using cmdUp As New MySqlCommand(
                "UPDATE lot SET qty_on_hand_pieces = qty_on_hand_pieces + @q WHERE lot_id=@id", conn, tx)

                    cmdUp.Parameters.Add("@q", MySqlDbType.Int32)
                    cmdUp.Parameters.Add("@id", MySqlDbType.Int64)

                    For Each kv In lotAggFromUnit
                        cmdUp.Parameters("@q").Value = kv.Value
                        cmdUp.Parameters("@id").Value = kv.Key
                        cmdUp.ExecuteNonQuery()
                    Next
                End Using
            End If

            ' (B) item別に戻す個数 = unit数（lot→itemで紐付け）
            Dim itemAggFromUnit As New Dictionary(Of Long, Integer)()

            Using cmd As New MySqlCommand(
            "SELECT l.item_id, COUNT(*) AS qty " &
            "FROM lot_unit lu " &
            "JOIN lot l ON l.lot_id = lu.lot_id " &
            "WHERE lu.unit_id IN (" & String.Join(",", unitIds.Select(Function(x, i) "@u" & i)) & ") " &
            "GROUP BY l.item_id", conn, tx)

                For i As Integer = 0 To unitIds.Count - 1
                    cmd.Parameters.AddWithValue("@u" & i, unitIds(i))
                Next

                Using rdr = cmd.ExecuteReader()
                    While rdr.Read()
                        Dim itemId As Long = Convert.ToInt64(rdr("item_id"))
                        Dim qty As Integer = Convert.ToInt32(rdr("qty"))
                        If qty > 0 Then itemAggFromUnit(itemId) = qty
                    End While
                End Using
            End Using

            If itemAggFromUnit.Count > 0 Then
                Using cmdUp As New MySqlCommand(
                "UPDATE item_master SET quantity1 = quantity1 + @q WHERE item_id=@id", conn, tx)

                    cmdUp.Parameters.Add("@q", MySqlDbType.Int32)
                    cmdUp.Parameters.Add("@id", MySqlDbType.Int64)

                    For Each kv In itemAggFromUnit
                        cmdUp.Parameters("@q").Value = kv.Value
                        cmdUp.Parameters("@id").Value = kv.Key
                        cmdUp.ExecuteNonQuery()
                    Next
                End Using
            End If

        Else
            ' -------------------------
            ' 1-保険) unitが無い出荷 → shipments_detail SUM(quantity) で戻す
            ' -------------------------
            Dim itemAgg As New Dictionary(Of Long, Integer)()
            Dim lotAgg As New Dictionary(Of Long, Integer)()

            Using cmd As New MySqlCommand(
            "SELECT item_id, SUM(quantity) AS qty " &
            "FROM shipments_detail " &
            "WHERE shipment_batch_id=@bid AND item_id IS NOT NULL " &
            "GROUP BY item_id", conn, tx)

                cmd.Parameters.AddWithValue("@bid", batchId)
                Using rdr = cmd.ExecuteReader()
                    While rdr.Read()
                        Dim itemId As Long = Convert.ToInt64(rdr("item_id"))
                        Dim qty As Integer = Convert.ToInt32(rdr("qty"))
                        If qty > 0 Then itemAgg(itemId) = qty
                    End While
                End Using
            End Using

            Using cmd As New MySqlCommand(
            "SELECT lot_id, SUM(quantity) AS qty " &
            "FROM shipments_detail " &
            "WHERE shipment_batch_id=@bid AND lot_id IS NOT NULL " &
            "GROUP BY lot_id", conn, tx)

                cmd.Parameters.AddWithValue("@bid", batchId)
                Using rdr = cmd.ExecuteReader()
                    While rdr.Read()
                        Dim lotId As Long = Convert.ToInt64(rdr("lot_id"))
                        Dim qty As Integer = Convert.ToInt32(rdr("qty"))
                        If qty > 0 Then lotAgg(lotId) = qty
                    End While
                End Using
            End Using

            If itemAgg.Count > 0 Then
                Using cmdUp As New MySqlCommand(
                "UPDATE item_master SET quantity1 = quantity1 + @q WHERE item_id=@id", conn, tx)

                    cmdUp.Parameters.Add("@q", MySqlDbType.Int32)
                    cmdUp.Parameters.Add("@id", MySqlDbType.Int64)

                    For Each kv In itemAgg
                        cmdUp.Parameters("@q").Value = kv.Value
                        cmdUp.Parameters("@id").Value = kv.Key
                        cmdUp.ExecuteNonQuery()
                    Next
                End Using
            End If

            If lotAgg.Count > 0 Then
                Using cmdUp As New MySqlCommand(
                "UPDATE lot SET qty_on_hand_pieces = qty_on_hand_pieces + @q WHERE lot_id=@id", conn, tx)

                    cmdUp.Parameters.Add("@q", MySqlDbType.Int32)
                    cmdUp.Parameters.Add("@id", MySqlDbType.Int64)

                    For Each kv In lotAgg
                        cmdUp.Parameters("@q").Value = kv.Value
                        cmdUp.Parameters("@id").Value = kv.Key
                        cmdUp.ExecuteNonQuery()
                    Next
                End Using
            End If
        End If

        ' -------------------------
        ' 2) lot_unit を ON_HAND に戻す（個体在庫を戻す）
        ' -------------------------
        If unitIds IsNot Nothing AndAlso unitIds.Count > 0 Then
            Dim inParams As New List(Of String)()
            Dim paramMap As New Dictionary(Of String, Object)()

            For i As Integer = 0 To unitIds.Count - 1
                Dim pn = "@u" & i
                inParams.Add(pn)
                paramMap(pn) = unitIds(i)
            Next

            ExecuteNonQuerySafe(conn, tx,
            "UPDATE lot_unit SET status='ON_HAND' " &
            "WHERE unit_id IN (" & String.Join(",", inParams) & ")",
            paramMap
        )
        End If

        ' -------------------------
        ' 3) 割当テーブルを削除（FK対策で先に）
        ' -------------------------
        ExecuteNonQuerySafe(conn, tx,
        "DELETE sua FROM shipment_unit_alloc sua " &
        "JOIN shipments_detail d ON d.shipment_detail_id = sua.shipment_detail_id " &
        "WHERE d.shipment_batch_id = @bid",
        New Dictionary(Of String, Object) From {{"@bid", batchId}}
    )

        ExecuteNonQuerySafe(conn, tx,
        "DELETE sla FROM shipment_lot_alloc sla " &
        "JOIN shipments_detail d ON d.shipment_detail_id = sla.shipment_detail_id " &
        "WHERE d.shipment_batch_id = @bid",
        New Dictionary(Of String, Object) From {{"@bid", batchId}}
    )

        ' -------------------------
        ' 4) 在庫台帳（inventory_ledger）を削除（あなたの設計は ref_type/ref_id）
        ' -------------------------
        ExecuteNonQuerySafe(conn, tx,
        "DELETE FROM inventory_ledger WHERE ref_type='SHIPMENT' AND ref_id=@bid",
        New Dictionary(Of String, Object) From {{"@bid", batchId}}
    )

        ' -------------------------
        ' 5) 最後にヘッダ削除（CASCADEで明細削除）
        ' -------------------------
        ExecuteNonQuerySafe(conn, tx,
        "DELETE FROM shipments_header WHERE shipment_batch_id = @bid",
        New Dictionary(Of String, Object) From {{"@bid", batchId}}
    )

    End Sub
    ' =========================================================
    ' 安全に NonQuery 実行（テーブル/列が無い等はスキップ）
    ' =========================================================
    Private Sub ExecuteNonQuerySafe(conn As MySqlConnection, tx As MySqlTransaction, sql As String, params As Dictionary(Of String, Object))
        Try
            Using cmd As New MySqlCommand(sql, conn, tx)
                If params IsNot Nothing Then
                    For Each kv In params
                        cmd.Parameters.AddWithValue(kv.Key, kv.Value)
                    Next
                End If
                cmd.ExecuteNonQuery()
            End Using
        Catch ex As MySqlException
            ' 1146: table doesn't exist
            ' 1054: unknown column
            ' 1091: can't drop ... (類似)
            If ex.Number = 1146 OrElse ex.Number = 1054 OrElse ex.Number = 1091 Then
                ' 環境差（テーブル/列無し）は無視して続行
                Return
            End If
            Throw
        End Try
    End Sub

    ' =========================================================
    ' 安全に SELECT（Longリスト）取得（テーブル/列が無ければ空で返す）
    ' =========================================================
    Private Function QueryLongListSafe(conn As MySqlConnection, tx As MySqlTransaction, sql As String, params As Dictionary(Of String, Object)) As List(Of Long)
        Dim res As New List(Of Long)()
        Try
            Using cmd As New MySqlCommand(sql, conn, tx)
                If params IsNot Nothing Then
                    For Each kv In params
                        cmd.Parameters.AddWithValue(kv.Key, kv.Value)
                    Next
                End If
                Using rdr = cmd.ExecuteReader()
                    While rdr.Read()
                        If rdr(0) IsNot DBNull.Value Then
                            Dim v As Long
                            If Long.TryParse(rdr(0).ToString(), v) Then
                                res.Add(v)
                            End If
                        End If
                    End While
                End Using
            End Using
        Catch ex As MySqlException
            If ex.Number = 1146 OrElse ex.Number = 1054 Then
                Return New List(Of Long)()
            End If
            Throw
        End Try
        Return res
    End Function

    ' =========================
    ' 安全関数
    ' =========================
    Private Function SafeStr(v As Object) As String
        If v Is Nothing OrElse v Is DBNull.Value Then Return ""
        Return v.ToString()
    End Function

    Private Function SafeDateStr(v As Object) As String
        If v Is Nothing OrElse v Is DBNull.Value Then Return ""
        Dim d As DateTime = Convert.ToDateTime(v)
        Return d.ToString("yyyy-MM-dd")
    End Function

    ' =========================
    ' ListView OwnerDraw
    ' =========================
    Private Sub Lv_DrawColumnHeader(sender As Object, e As DrawListViewColumnHeaderEventArgs)
        Using b As New SolidBrush(Color.FromArgb(250, 250, 252))
            e.Graphics.FillRectangle(b, e.Bounds)
        End Using

        Using p As New Pen(colorBorder)
            e.Graphics.DrawLine(p, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1)
        End Using

        TextRenderer.DrawText(e.Graphics, e.Header.Text, New Font("Segoe UI", 10, FontStyle.Bold),
                              e.Bounds, Color.FromArgb(70, 70, 70),
                              TextFormatFlags.VerticalCenter Or TextFormatFlags.Left)
    End Sub

    Private Sub Lv_DrawItem(sender As Object, e As DrawListViewItemEventArgs)
        ' DrawSubItem側で描画するのでここは空
    End Sub

    Private Sub Lv_DrawSubItem(sender As Object, e As DrawListViewSubItemEventArgs)
        Dim lv As ListView = CType(sender, ListView)
        Dim isSelected As Boolean = e.Item.Selected

        Dim baseBg As Color = If(e.ItemIndex Mod 2 = 0, Color.White, Color.FromArgb(248, 249, 252))
        Dim bg As Color = If(isSelected, Color.FromArgb(225, 235, 255), baseBg)

        Using b As New SolidBrush(bg)
            e.Graphics.FillRectangle(b, e.Bounds)
        End Using

        Dim flags As TextFormatFlags = TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis
        Dim align = lv.Columns(e.ColumnIndex).TextAlign
        If align = HorizontalAlignment.Right Then
            flags = flags Or TextFormatFlags.Right
        Else
            flags = flags Or TextFormatFlags.Left
        End If

        Dim textColor As Color = If(isSelected, Color.FromArgb(20, 60, 160), Color.FromArgb(40, 40, 40))
        TextRenderer.DrawText(e.Graphics, e.SubItem.Text, lv.Font, e.Bounds, textColor, flags)

        Using p As New Pen(Color.FromArgb(235, 238, 244))
            e.Graphics.DrawLine(p, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1)
        End Using
    End Sub

End Class

' =========================
' 角丸カードパネル（衝突しない名前）
' =========================
Public Class UiCardPanel
    Inherits Panel

    Public Property Radius As Integer = 16
    Public Property BorderColor As Color = Color.Gainsboro

    Public Sub New()
        Me.DoubleBuffered = True
        Me.BackColor = Color.White
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias

        Dim rect As Rectangle = Me.ClientRectangle
        rect.Inflate(-1, -1)

        Using path = GetRoundRectPath(rect, Radius)
            Using b As New SolidBrush(Me.BackColor)
                e.Graphics.FillPath(b, path)
            End Using
            Using p As New Pen(BorderColor, 1)
                e.Graphics.DrawPath(p, path)
            End Using
        End Using
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        Me.Invalidate()
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
