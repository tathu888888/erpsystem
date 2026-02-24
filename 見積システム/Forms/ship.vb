' ==========================================================
' ship.vb（完全版：数量2=箱数で金額計算 + conversion_qtyポップアップで数量1(個数)へ反映 + 在庫チェック）
'
' 仕様：
'  - 画面の「数量2」に 1箱 / 2箱 ... を入力
'  - 入力後に conversion_qty（1箱あたり個数）をポップアップ表示（手修正OK）
'  - OK押下で「数量1（個数）= 箱数 × conversion_qty」を DGVの quantity 列へ反映
'  - amount は「数量2（箱）× unit_price（箱単価）」で自動計算
'  - 保存は quantity2（箱数）が入っている行を明細として保存
'  - 保存時の quantity（個数）は、DGV.quantity に値があればそれを優先（ポップアップ反映値）
'    空なら conversion_qty で計算（保険）
'  - quantity2 入力時に在庫チェック（item_master.quantity1 を個数在庫として比較）
'
' 事前にDBへ列追加：
'   ALTER TABLE shipments_detail ADD COLUMN quantity2 INT UNSIGNED NOT NULL DEFAULT 0 AFTER quantity;
'
'   （推奨）item_master に conversion_qty を追加：
'   ALTER TABLE item_master ADD COLUMN conversion_qty INT UNSIGNED NOT NULL DEFAULT 1 AFTER unit2;
' ==========================================================

Imports MySql.Data.MySqlClient
Imports System.Windows.Forms
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Text.RegularExpressions
Imports System.ComponentModel
Imports System.Diagnostics

Partial Public Class ship

    ' ===== item master cache =====
    Private itemTable As DataTable = Nothing

    Private ReadOnly connectionString As String =
        "Server=127.0.0.1;Port=3306;Database=sunstar;Uid=root;Pwd=1234;SslMode=Disabled;"

    ' =========================
    ' Form Load
    ' =========================
    Private Sub ship_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ApplyTheme()
        SetupInstantValidation()
        SetupItemColumns()     ' アイテム列＋単位列＋数量2列
        LoadItemMaster()       ' item_master読込（conversion_qty含む）
        BindItemComboColumn()
        txtCustomerCode.Focus()
    End Sub

    ' =========================
    ' 保存ボタン（header + detail INSERT）
    ' =========================
    ' =========================
    ' 保存ボタン（header + detail INSERT）
    ' =========================
    ' =========================
    ' 保存ボタン（header + detail INSERT）
    ' =========================




    ' =========================
    ' シャットダウン / 戻る / 画面遷移
    ' =========================
    Private Sub shutdownBtn_Click(sender As Object, e As EventArgs) Handles shutdownBtn.Click
        Dim result As DialogResult = MessageBox.Show(
            "シャットダウンしますか？",
            "確認",
            MessageBoxButtons.OKCancel,
            MessageBoxIcon.Exclamation,
            MessageBoxDefaultButton.Button2
        )

        If result = DialogResult.OK Then
            Application.Exit()
        End If
    End Sub

    Private Sub backBtn_Click(sender As Object, e As EventArgs) Handles backBtn.Click
        Me.Close()
    End Sub

    Private Sub listBtn_Click(sender As Object, e As EventArgs) Handles listBtn.Click
        Using f As New shipllist
            Me.Hide()
            f.ShowDialog()
            Me.Show()
        End Using
    End Sub

    Private Sub customerBtn_Click(sender As Object, e As EventArgs) Handles customerBtn.Click
        Using f As New Customer
            Me.Hide()
            f.ShowDialog()
            Me.Show()
        End Using
    End Sub

    ' ==============================
    ' 顧客コード → 顧客名 自動反映（Enter）
    ' ==============================
    Private Sub txtCustomerCode_KeyDown(sender As Object, e As KeyEventArgs) Handles txtCustomerCode.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            LoadCustomerName(txtCustomerCode, txtCustomerName)

            estimateDataGridView.Focus()
            If estimateDataGridView.Rows.Count > 0 Then
                If estimateDataGridView.Columns.Contains("quantity2") Then
                    estimateDataGridView.CurrentCell = estimateDataGridView.Rows(0).Cells("quantity2")
                ElseIf estimateDataGridView.Columns.Contains("quantity") Then
                    estimateDataGridView.CurrentCell = estimateDataGridView.Rows(0).Cells("quantity")
                End If
                estimateDataGridView.BeginEdit(True)
            End If
        End If
    End Sub

    Private Sub LoadCustomerName(codeTextBox As TextBox, nameTextBox As TextBox)
        Dim code As String = If(codeTextBox.Text, "").Trim()

        If code = "" Then
            nameTextBox.Text = ""
            Exit Sub
        End If

        Dim sql As String = "SELECT customer_name FROM customer_master WHERE customer_code = @code"

        Try
            Using conn As New MySqlConnection(connectionString)
                Using cmd As New MySqlCommand(sql, conn)
                    cmd.Parameters.Add("@code", MySqlDbType.VarChar).Value = code
                    conn.Open()

                    Dim result As Object = cmd.ExecuteScalar()
                    If result IsNot Nothing Then
                        nameTextBox.Text = result.ToString()
                    Else
                        nameTextBox.Text = ""
                        MessageBox.Show("該当する顧客コードが存在しません")
                        codeTextBox.Focus()
                    End If
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show($"顧客名取得エラー: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ==========================================================
    ' ここから下：クリエイティブUI（ApplyTheme一式）
    ' ==========================================================

    ' ========= Theme colors =========
    Private ReadOnly cBg As Color = Color.FromArgb(245, 246, 250)
    Private ReadOnly cCard As Color = Color.White
    Private ReadOnly cPrimary As Color = Color.FromArgb(46, 98, 238)
    Private ReadOnly cPrimaryDark As Color = Color.FromArgb(36, 78, 200)
    Private ReadOnly cText As Color = Color.FromArgb(25, 28, 36)
    Private ReadOnly cSubText As Color = Color.FromArgb(120, 125, 140)
    Private ReadOnly cBorder As Color = Color.FromArgb(225, 228, 236)

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

    Private Class RoundedPanel
        Inherits Panel
        Public Property Radius As Integer = 18
        Public Property BorderColor As Color = Color.Gainsboro
        Public Property BorderWidth As Integer = 1
        Public Property FillColor As Color = Color.White
        Public Property Shadow As Boolean = True

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            MyBase.OnPaint(e)
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias

            Dim rect As Rectangle = Me.ClientRectangle
            rect.Inflate(-1, -1)

            If Shadow Then
                Using shadowBrush As New SolidBrush(Color.FromArgb(22, 0, 0, 0))
                    Dim sRect As Rectangle = rect
                    sRect.Offset(0, 4)
                    sRect.Inflate(2, 2)
                    Using sp As New GraphicsPath()
                        Dim r As Integer = Radius
                        Dim d As Integer = r * 2
                        sp.StartFigure()
                        sp.AddArc(sRect.X, sRect.Y, d, d, 180, 90)
                        sp.AddArc(sRect.Right - d, sRect.Y, d, d, 270, 90)
                        sp.AddArc(sRect.Right - d, sRect.Bottom - d, d, d, 0, 90)
                        sp.AddArc(sRect.X, sRect.Bottom - d, d, d, 90, 90)
                        sp.CloseFigure()
                        e.Graphics.FillPath(shadowBrush, sp)
                    End Using
                End Using
            End If

            Using path As New GraphicsPath()
                Dim r2 As Integer = Radius
                Dim d2 As Integer = r2 * 2
                path.StartFigure()
                path.AddArc(rect.X, rect.Y, d2, d2, 180, 90)
                path.AddArc(rect.Right - d2, rect.Y, d2, d2, 270, 90)
                path.AddArc(rect.Right - d2, rect.Bottom - d2, d2, d2, 0, 90)
                path.AddArc(rect.X, rect.Bottom - d2, d2, d2, 90, 90)
                path.CloseFigure()

                Using fill As New SolidBrush(FillColor)
                    e.Graphics.FillPath(fill, path)
                End Using

                Using pen As New Pen(BorderColor, BorderWidth)
                    e.Graphics.DrawPath(pen, path)
                End Using
            End Using
        End Sub
    End Class

    Private Sub StyleButton(btn As Button, Optional primary As Boolean = True)
        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderSize = 0
        btn.BackColor = If(primary, cPrimary, Color.White)
        btn.ForeColor = If(primary, Color.White, cText)
        btn.Font = New Font("Yu Gothic UI", 12.0F, FontStyle.Bold, GraphicsUnit.Point)
        btn.Cursor = Cursors.Hand

        AddHandler btn.MouseEnter, Sub()
                                       btn.BackColor = If(primary, cPrimaryDark, Color.FromArgb(245, 246, 250))
                                   End Sub
        AddHandler btn.MouseLeave, Sub()
                                       btn.BackColor = If(primary, cPrimary, Color.White)
                                   End Sub

        AddHandler btn.Resize, Sub()
                                   Dim rect = New Rectangle(0, 0, btn.Width, btn.Height)
                                   Using gp = RoundRectPath(rect, 14)
                                       btn.Region = New Region(gp)
                                   End Using
                               End Sub
    End Sub

    Private Sub StyleChipButton(btn As Button)
        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderSize = 1
        btn.FlatAppearance.BorderColor = cBorder
        btn.BackColor = Color.White
        btn.ForeColor = cText
        btn.Font = New Font("Yu Gothic UI", 11.0F, FontStyle.Bold)
        btn.Cursor = Cursors.Hand

        AddHandler btn.MouseEnter, Sub() btn.BackColor = Color.FromArgb(245, 246, 250)
        AddHandler btn.MouseLeave, Sub() btn.BackColor = Color.White

        AddHandler btn.Resize, Sub()
                                   Dim rect = New Rectangle(0, 0, btn.Width, btn.Height)
                                   Using gp = RoundRectPath(rect, 18)
                                       btn.Region = New Region(gp)
                                   End Using
                               End Sub
    End Sub

    Private Sub StyleTextBox(tb As TextBox, Optional readOnlyStyle As Boolean = False)
        tb.BorderStyle = BorderStyle.FixedSingle
        tb.BackColor = If(readOnlyStyle, Color.FromArgb(248, 249, 252), Color.White)
        tb.ForeColor = cText
        tb.Font = New Font("Yu Gothic UI", 13.0F, FontStyle.Regular)
    End Sub

    Private Sub StyleLabel(lbl As Label)
        lbl.ForeColor = cSubText
        lbl.Font = New Font("Yu Gothic UI", 11.5F, FontStyle.Bold)
    End Sub

    Private Sub StyleDgv(dgv As DataGridView)
        dgv.BackgroundColor = Color.White
        dgv.BorderStyle = BorderStyle.None
        dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
        dgv.GridColor = cBorder
        dgv.EnableHeadersVisualStyles = False

        dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 246, 250)
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = cText
        dgv.ColumnHeadersDefaultCellStyle.Font = New Font("Yu Gothic UI", 11.5F, FontStyle.Bold)
        dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft
        dgv.ColumnHeadersHeight = 44

        dgv.DefaultCellStyle.BackColor = Color.White
        dgv.DefaultCellStyle.ForeColor = cText
        dgv.DefaultCellStyle.Font = New Font("Yu Gothic UI", 11.5F, FontStyle.Regular)
        dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 235, 255)
        dgv.DefaultCellStyle.SelectionForeColor = cText
        dgv.RowTemplate.Height = 40

        dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 253)
        dgv.RowHeadersVisible = False
    End Sub

    Private Sub ApplyTheme()
        Me.BackColor = cBg
        Me.DoubleBuffered = True

        LabelTitle.ForeColor = cText
        LabelTitle.Font = New Font("Yu Gothic UI", 24.0F, FontStyle.Bold)

        shutdownBtn.Text = "✕"
        shutdownBtn.FlatStyle = FlatStyle.Flat
        shutdownBtn.FlatAppearance.BorderSize = 0
        shutdownBtn.BackColor = Color.White
        shutdownBtn.ForeColor = cSubText
        shutdownBtn.Cursor = Cursors.Hand
        shutdownBtn.Size = New Size(44, 44)

        AddHandler shutdownBtn.Resize, Sub()
                                           Dim rect = New Rectangle(0, 0, shutdownBtn.Width, shutdownBtn.Height)
                                           Using gp = RoundRectPath(rect, 22)
                                               shutdownBtn.Region = New Region(gp)
                                           End Using
                                       End Sub

        AddHandler shutdownBtn.MouseEnter, Sub()
                                               shutdownBtn.BackColor = Color.FromArgb(255, 235, 238)
                                               shutdownBtn.ForeColor = Color.FromArgb(198, 40, 40)
                                           End Sub
        AddHandler shutdownBtn.MouseLeave, Sub()
                                               shutdownBtn.BackColor = Color.White
                                               shutdownBtn.ForeColor = cSubText
                                           End Sub

        StyleLabel(lblShipmentDate)
        StyleLabel(lblCustomerCode)
        StyleLabel(lblCustomerName)

        StyleTextBox(txtCustomerCode)
        StyleTextBox(txtCustomerName, readOnlyStyle:=True)

        dtpShipmentDate.Font = New Font("Yu Gothic UI", 13.0F, FontStyle.Regular)

        StyleButton(saveBtn, primary:=True)
        StyleButton(backBtn, primary:=False)
        StyleChipButton(listBtn)
        StyleChipButton(customerBtn)

        StyleDgv(estimateDataGridView)

        If estimateDataGridView.Columns.Contains("amount") Then
            estimateDataGridView.Columns("amount").ReadOnly = True
            estimateDataGridView.Columns("amount").DefaultCellStyle.BackColor = Color.FromArgb(248, 249, 252)
        End If
        If estimateDataGridView.Columns.Contains("quantity") Then
            estimateDataGridView.Columns("quantity").ReadOnly = False
        End If

        ' ========= カード化 =========
        Dim card As New RoundedPanel() With {
            .Radius = 20,
            .BorderColor = cBorder,
            .BorderWidth = 1,
            .FillColor = cCard,
            .Shadow = True,
            .Location = New Point(40, 110),
            .Size = New Size(980, 740),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Bottom
        }

        Me.Controls.Remove(lblShipmentDate) : card.Controls.Add(lblShipmentDate)
        Me.Controls.Remove(dtpShipmentDate) : card.Controls.Add(dtpShipmentDate)
        Me.Controls.Remove(lblCustomerCode) : card.Controls.Add(lblCustomerCode)
        Me.Controls.Remove(txtCustomerCode) : card.Controls.Add(txtCustomerCode)
        Me.Controls.Remove(lblCustomerName) : card.Controls.Add(lblCustomerName)
        Me.Controls.Remove(txtCustomerName) : card.Controls.Add(txtCustomerName)
        Me.Controls.Remove(estimateDataGridView) : card.Controls.Add(estimateDataGridView)

        Dim xL As Integer = 28
        Dim xI As Integer = 220
        Dim yy As Integer = 26
        Dim gap As Integer = 56

        lblShipmentDate.Location = New Point(xL, yy)
        dtpShipmentDate.Location = New Point(xI, yy)

        yy += gap
        lblCustomerCode.Location = New Point(xL, yy)
        txtCustomerCode.Location = New Point(xI, yy)

        yy += gap
        lblCustomerName.Location = New Point(xL, yy)
        txtCustomerName.Location = New Point(xI, yy)

        estimateDataGridView.Location = New Point(28, yy + 62)
        estimateDataGridView.Size = New Size(card.Width - 56, card.Height - (yy + 90))
        estimateDataGridView.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Bottom

        Me.Controls.Add(card)
        card.BringToFront()

        ' ========= フッター =========
        Dim footer As New RoundedPanel() With {
            .Radius = 22,
            .BorderColor = cBorder,
            .BorderWidth = 1,
            .FillColor = cCard,
            .Shadow = True,
            .Height = 84,
            .Width = Me.ClientSize.Width - 80,
            .Left = 40,
            .Top = Me.ClientSize.Height - 104,
            .Anchor = AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Bottom
        }
        Me.Controls.Add(footer)
        footer.BringToFront()

        Me.Controls.Remove(backBtn) : footer.Controls.Add(backBtn)
        Me.Controls.Remove(saveBtn) : footer.Controls.Add(saveBtn)
        Me.Controls.Remove(listBtn) : footer.Controls.Add(listBtn)
        Me.Controls.Remove(customerBtn) : footer.Controls.Add(customerBtn)

        backBtn.Location = New Point(20, 20)
        saveBtn.Location = New Point(170, 20)
        listBtn.Location = New Point(560, 20)
        customerBtn.Location = New Point(730, 20)
    End Sub

    ' =========================
    ' Instant Validation (TextBox + DGV)
    ' =========================
    Private ReadOnly ep As New ErrorProvider()
    Private Shared ReadOnly RX_ALLOWED_CUSTOMER_CHARS As New Regex("[^A-Za-z0-9\-_]", RegexOptions.Compiled)

    Private Sub SetupInstantValidation()
        ep.BlinkStyle = ErrorBlinkStyle.NeverBlink
        ep.ContainerControl = Me

        AddHandler txtCustomerCode.KeyPress, AddressOf TxtCustomerCode_KeyPress_Block
        AddHandler txtCustomerCode.TextChanged, AddressOf TxtCustomerCode_TextChanged_Instant

        ' 金額自動計算（数量2×単価）
        AddHandler estimateDataGridView.CellEndEdit, AddressOf Dgv_CellEndEdit_AutoAmount

        ' 編集中コントロール（数字制限）
        AddHandler estimateDataGridView.EditingControlShowing, AddressOf Dgv_EditingControlShowing

        ' 確定時に弾く
        AddHandler estimateDataGridView.CellValidating, AddressOf Dgv_CellValidating_All
        AddHandler estimateDataGridView.DataError, AddressOf Dgv_DataError_All
    End Sub

    Private Sub Dgv_DataError_All(sender As Object, e As DataGridViewDataErrorEventArgs)
        e.ThrowException = False
        If e.RowIndex >= 0 AndAlso e.ColumnIndex >= 0 Then
            Dim dgv = estimateDataGridView
            dgv.Rows(e.RowIndex).Cells(e.ColumnIndex).ErrorText = "入力値が不正です。選択し直してください。"
            dgv.Rows(e.RowIndex).ErrorText = "入力エラーがあります"
            MessageBox.Show("入力値が不正です。選択し直してください。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            e.Cancel = True
        End If
    End Sub

    Private Sub Dgv_CellValidating_All(sender As Object, e As DataGridViewCellValidatingEventArgs)
        Dim dgv = estimateDataGridView
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Return

        Dim row = dgv.Rows(e.RowIndex)
        If row Is Nothing OrElse row.IsNewRow Then Return

        Dim colName = dgv.Columns(e.ColumnIndex).Name
        Dim input As String = If(e.FormattedValue, "").ToString().Trim()

        ' クリア
        row.ErrorText = ""
        dgv.Rows(e.RowIndex).Cells(e.ColumnIndex).ErrorText = ""

        ' この行が入力対象か？（数量2が入ってる行だけ厳格）
        Dim q2Str As String = ""
        If dgv.Columns.Contains("quantity2") Then
            q2Str = If(row.Cells("quantity2").EditedFormattedValue, row.Cells("quantity2").Value)
            q2Str = If(q2Str, "").ToString().Trim()
        End If
        Dim hasQ2 As Boolean = (q2Str <> "")

        ' 1) item_id（必須：数量2が入ってる行だけ）
        If colName = "item_id" Then
            If Not hasQ2 Then Return
            If input = "" Then
                RejectCell(dgv, e, "アイテムを選択してください。")
                Return
            End If
            Dim tmp As Integer
            If Not Integer.TryParse(input, tmp) Then
                RejectCell(dgv, e, "アイテムの選択が不正です。")
                Return
            End If
        End If

        ' 2) quantity2（箱数）：空OK、入れるなら1以上の整数 + 在庫チェック
        If colName = "quantity2" Then
            If input = "" Then Return

            Dim boxCnt As Integer
            If Not Integer.TryParse(input, boxCnt) Then
                RejectCell(dgv, e, "数量2（箱数）は数値を入力してください。")
                Return
            End If
            If boxCnt <= 0 Then
                RejectCell(dgv, e, "数量2（箱数）は1以上を入力してください。")
                Return
            End If

            ' 先に item が必要
            Dim itemIdObj = row.Cells("item_id").Value
            If itemIdObj Is Nothing OrElse itemIdObj Is DBNull.Value OrElse itemIdObj.ToString().Trim() = "" Then
                RejectCell(dgv, e, "先にアイテムを選択してください。")
                Return
            End If

            Dim itemId As Integer
            If Not Integer.TryParse(itemIdObj.ToString(), itemId) Then
                RejectCell(dgv, e, "アイテムIDが不正です。")
                Return
            End If

            Dim conv As Integer = GetConversionQtyFromMaster(itemId)
            Dim reqPieces As Long = CLng(boxCnt) * CLng(conv)

            Debug.WriteLine($"itemId={itemId}, boxCnt={boxCnt}, conv={conv}, reqPieces={reqPieces}")

            If reqPieces > Integer.MaxValue Then
                RejectCell(dgv, e, "数量が大きすぎます。")
                Return
            End If

            Dim availablePieces As Integer = GetAvailableQtyPieces(itemId)
            If CInt(reqPieces) > availablePieces Then
                RejectCell(dgv, e, $"在庫がありません。要求={reqPieces}個 / 在庫={availablePieces}個")
                Return
            End If
        End If

        ' 3) unit_price（単価）：数量2が入ってる行だけ厳格（空OK）
        If colName = "unit_price" Then
            If Not hasQ2 Then Return
            If input = "" Then Return

            Dim v As Integer
            If Not Integer.TryParse(input, v) Then
                RejectCell(dgv, e, "単価は数値を入力してください。")
                Return
            End If
            If v < 0 Then
                RejectCell(dgv, e, "単価は0以上を入力してください。")
                Return
            End If
        End If

        ' 4) quantity（個数）：数量2が入ってる行だけ厳格（空OK）
        If colName = "quantity" Then
            If Not hasQ2 Then Return
            If input = "" Then Return

            Dim v As Integer
            If Not Integer.TryParse(input, v) Then
                RejectCell(dgv, e, "数量1（個数）は数値を入力してください。")
                Return
            End If
            If v <= 0 Then
                RejectCell(dgv, e, "数量1（個数）は1以上を入力してください。")
                Return
            End If
        End If

        ' 5) unit（単位）：数量2が入ってる行だけ必須
        If colName = "unit" Then
            If Not hasQ2 Then Return
            If input = "" Then
                RejectCell(dgv, e, "単位を選択してください。")
                Return
            End If
        End If

        ' 6) remark（備考）：200文字制限
        If colName = "remark" Then
            If input.Length > 200 Then
                RejectCell(dgv, e, "備考は200文字以内で入力してください。")
                Return
            End If
        End If
    End Sub

    Private Sub RejectCell(dgv As DataGridView, e As DataGridViewCellValidatingEventArgs, msg As String)
        dgv.Rows(e.RowIndex).Cells(e.ColumnIndex).ErrorText = msg
        dgv.Rows(e.RowIndex).ErrorText = "入力エラーがあります"
        MessageBox.Show(msg, "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        e.Cancel = True
    End Sub

    Private Sub Dgv_EditingControlShowing(sender As Object, e As DataGridViewEditingControlShowingEventArgs)
        Dim dgv = estimateDataGridView
        Dim colName = dgv.CurrentCell.OwningColumn.Name

        RemoveHandler e.Control.KeyPress, AddressOf NumericEditing_KeyPress

        If colName = "quantity2" OrElse colName = "unit_price" OrElse colName = "quantity" Then
            AddHandler e.Control.KeyPress, AddressOf NumericEditing_KeyPress
        End If
    End Sub

    Private Sub NumericEditing_KeyPress(sender As Object, e As KeyPressEventArgs)
        If Char.IsControl(e.KeyChar) Then Return
        If Not Char.IsDigit(e.KeyChar) Then e.Handled = True
    End Sub

    Private Sub TxtCustomerCode_KeyPress_Block(sender As Object, e As KeyPressEventArgs)
        If Char.IsControl(e.KeyChar) Then Return
        Dim ch As Char = e.KeyChar
        Dim ok As Boolean = Char.IsLetterOrDigit(ch) OrElse ch = "-"c OrElse ch = "_"c
        If Not ok Then
            e.Handled = True
            ep.SetError(txtCustomerCode, "使用できない文字です（英数字と - _ のみ）")
        End If
    End Sub

    Private Sub TxtCustomerCode_TextChanged_Instant(sender As Object, e As EventArgs)
        Dim raw As String = If(txtCustomerCode.Text, "")
        Dim up = raw.ToUpperInvariant()
        Dim cleaned = RX_ALLOWED_CUSTOMER_CHARS.Replace(up, "")
        If cleaned.Length > 20 Then cleaned = cleaned.Substring(0, 20)

        If cleaned <> txtCustomerCode.Text Then
            Dim pos = txtCustomerCode.SelectionStart
            txtCustomerCode.Text = cleaned
            txtCustomerCode.SelectionStart = Math.Min(pos, txtCustomerCode.TextLength)
        End If

        If cleaned = "" Then
            ep.SetError(txtCustomerCode, "")
        ElseIf cleaned.Length < 1 OrElse cleaned.Length > 20 Then
            ep.SetError(txtCustomerCode, "顧客コードは1～20文字です")
        Else
            ep.SetError(txtCustomerCode, "")
        End If
    End Sub

    ' =========================
    ' DGV amount auto-calc（数量2×単価）
    '  + quantity2 編集終了時に conversion_qty ポップアップ→数量1へ反映
    ' =========================
    Private Sub Dgv_CellEndEdit_AutoAmount(sender As Object, e As DataGridViewCellEventArgs)
        Dim dgv = estimateDataGridView
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Return

        Dim colName = dgv.Columns(e.ColumnIndex).Name
        Dim row = dgv.Rows(e.RowIndex)
        If row Is Nothing OrElse row.IsNewRow Then Return

        ' quantity2 編集終了でポップアップ
        If colName = "quantity2" Then
            ShowConversionPopupAndReflect(row)
            ' 続けて金額計算もする
        End If

        If colName <> "quantity2" AndAlso colName <> "unit_price" Then Return
        RecalcAmountRow(row)
    End Sub

    Private Sub RecalcAmountRow(row As DataGridViewRow)
        Dim q2Str As String = SafeCellStr(row, "quantity2")
        Dim pStr As String = SafeCellStr(row, "unit_price")

        If Not estimateDataGridView.Columns.Contains("amount") Then Return

        If q2Str = "" OrElse pStr = "" Then
            row.Cells("amount").Value = ""
            Return
        End If

        Dim q2 As Long, p As Long
        If Not Long.TryParse(q2Str, q2) OrElse Not Long.TryParse(pStr, p) Then
            row.Cells("amount").Value = ""
            Return
        End If

        Try
            Dim a As Long = CheckedMul(q2, p)
            row.Cells("amount").Value = a.ToString()
        Catch
            row.Cells("amount").Value = ""
            row.ErrorText = "金額が大きすぎます"
        End Try
    End Sub

    Private Function CheckedMul(a As Long, b As Long) As Long
        If a = 0 OrElse b = 0 Then Return 0
        If a > Long.MaxValue \ b Then Throw New OverflowException()
        Return a * b
    End Function

    ' ==========================================================
    ' conversion_qty：ポップアップ → OKで数量1へ反映
    ' ==========================================================
    Private Sub ShowConversionPopupAndReflect(row As DataGridViewRow)
        If row Is Nothing OrElse row.IsNewRow Then Return
        If Not estimateDataGridView.Columns.Contains("item_id") Then Return
        If Not estimateDataGridView.Columns.Contains("quantity2") Then Return
        If Not estimateDataGridView.Columns.Contains("quantity") Then Return

        Dim itemIdObj = row.Cells("item_id").Value
        If itemIdObj Is Nothing OrElse itemIdObj Is DBNull.Value Then Return

        Dim itemId As Integer
        If Not Integer.TryParse(itemIdObj.ToString(), itemId) Then Return

        Dim boxStr As String = SafeCellStr(row, "quantity2")
        Dim boxCnt As Integer
        If boxStr = "" OrElse Not Integer.TryParse(boxStr, boxCnt) OrElse boxCnt <= 0 Then
            row.Cells("quantity").Value = ""
            Return
        End If

        Dim defaultConv As Integer = GetConversionQtyFromMaster(itemId)
        Dim itemName As String = GetItemNameFromMaster(itemId)

        ' 親フォームを隠す → ダイアログ → 戻す（フォーカス崩れ対策）
        Me.Hide()
        Try
            Using dlg As New ConversionQtyDialog(itemName, defaultConv)
                Dim res = dlg.ShowDialog()
                If res <> DialogResult.OK Then Return

                Dim conv As Integer = dlg.ConversionQty
                Dim tmp As Long = CLng(boxCnt) * CLng(conv)
                If tmp > Integer.MaxValue Then
                    MessageBox.Show("数量が大きすぎます。")
                    row.Cells("quantity").Value = ""
                    Return
                End If

                row.Cells("quantity").Value = CInt(tmp).ToString()
            End Using
        Finally
            Me.Show()
            Me.Activate()
        End Try
    End Sub

    ' =========================
    ' DGV：アイテム/単位/数量2の列と自動反映
    ' =========================
    Private Sub SetupItemColumns()
        Dim dgv = estimateDataGridView

        ' 1) アイテム（ComboBox）
        If Not dgv.Columns.Contains("item_id") Then
            Dim col As New DataGridViewComboBoxColumn() With {
                .Name = "item_id",
                .HeaderText = "アイテム",
                .Width = 260,
                .DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
                .FlatStyle = FlatStyle.Flat
            }
            dgv.Columns.Insert(0, col)
        End If

        ' 2) 数量2（箱数）
        If Not dgv.Columns.Contains("quantity2") Then
            Dim q2col As New DataGridViewTextBoxColumn() With {
                .Name = "quantity2",
                .HeaderText = "数量2(箱)",
                .Width = 90
            }

            If dgv.Columns.Contains("quantity") Then
                Dim idx As Integer = dgv.Columns("quantity").Index + 1
                dgv.Columns.Insert(idx, q2col)
            Else
                dgv.Columns.Add(q2col)
            End If
        End If

        ' 3) 単位（ComboBox）※quantity2 の右隣
        If Not dgv.Columns.Contains("unit") Then
            Dim ucol As New DataGridViewComboBoxColumn() With {
                .Name = "unit",
                .HeaderText = "単位",
                .Width = 90,
                .DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
                .FlatStyle = FlatStyle.Flat
            }

            If dgv.Columns.Contains("quantity2") Then
                Dim idx As Integer = dgv.Columns("quantity2").Index + 1
                dgv.Columns.Insert(idx, ucol)
            Else
                dgv.Columns.Add(ucol)
            End If
        Else
            If dgv.Columns.Contains("quantity2") Then
                Dim targetIndex As Integer = dgv.Columns("quantity2").Index + 1
                dgv.Columns("unit").DisplayIndex = targetIndex
            End If
        End If


        ' 追加：ロット（ComboBox）
        If Not dgv.Columns.Contains("lot_id") Then
            Dim lcol As New DataGridViewComboBoxColumn() With {
        .Name = "lot_id",
        .HeaderText = "ロット",
        .Width = 140,
        .DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
        .FlatStyle = FlatStyle.Flat
    }

            ' item_id の右に置く例
            Dim idx As Integer = dgv.Columns("item_id").Index + 1
            dgv.Columns.Insert(idx, lcol)
        End If


        ' amount は read-only
        If dgv.Columns.Contains("amount") Then
            dgv.Columns("amount").ReadOnly = True
            dgv.Columns("amount").DefaultCellStyle.BackColor = Color.FromArgb(248, 249, 252)
        End If

        If Not estimateDataGridView.Columns.Contains("row_key") Then
            Dim kcol As New DataGridViewTextBoxColumn() With {
        .Name = "row_key",
        .HeaderText = "row_key",
        .Visible = False
    }
            estimateDataGridView.Columns.Add(kcol)
        End If

        ' 追加：ロット割当サマリ（表示）
        If Not estimateDataGridView.Columns.Contains("lot_summary") Then
            Dim scol As New DataGridViewTextBoxColumn() With {
        .Name = "lot_summary",
        .HeaderText = "ロット割当",
        .Width = 180,
        .ReadOnly = True
    }
            estimateDataGridView.Columns.Add(scol)
        End If

        ' 追加：ロット割当ボタン
        If Not estimateDataGridView.Columns.Contains("btn_lot_alloc") Then
            Dim bcol As New DataGridViewButtonColumn() With {
        .Name = "btn_lot_alloc",
        .HeaderText = "ロット",
        .Text = "割当...",
        .UseColumnTextForButtonValue = True,
        .Width = 80
    }
            estimateDataGridView.Columns.Add(bcol)
        End If


        ' 追加：ロット管理フラグ（行ごと）
        If Not estimateDataGridView.Columns.Contains("is_lot_item") Then
            Dim fcol As New DataGridViewTextBoxColumn() With {
        .Name = "is_lot_item",
        .HeaderText = "is_lot_item",
        .Visible = False
    }
            estimateDataGridView.Columns.Add(fcol)
        End If


        ' 行生成時に row_key を自動付与（RowsAdded）
        AddHandler estimateDataGridView.RowsAdded,
    Sub(s, e)
        For i = e.RowIndex To e.RowIndex + e.RowCount - 1
            Dim r = estimateDataGridView.Rows(i)
            If r Is Nothing OrElse r.IsNewRow Then Continue For
            If r.Cells("row_key").Value Is Nothing OrElse r.Cells("row_key").Value.ToString() = "" Then
                r.Cells("row_key").Value = Guid.NewGuid().ToString()
            End If
        Next
    End Sub

        ' ボタンクリックでポップアップ
        RemoveHandler estimateDataGridView.CellContentClick, AddressOf Dgv_CellContentClick_LotAlloc
        AddHandler estimateDataGridView.CellContentClick, AddressOf Dgv_CellContentClick_LotAlloc

        ' アイテム選択/単位変更で単価補完
        RemoveHandler dgv.CellValueChanged, AddressOf Dgv_CellValueChanged_ItemSelected
        AddHandler dgv.CellValueChanged, AddressOf Dgv_CellValueChanged_ItemSelected

        RemoveHandler dgv.CurrentCellDirtyStateChanged, AddressOf Dgv_CurrentCellDirtyStateChanged
        AddHandler dgv.CurrentCellDirtyStateChanged, AddressOf Dgv_CurrentCellDirtyStateChanged

        RemoveHandler dgv.CellValueChanged, AddressOf Dgv_CellValueChanged_UnitChanged
        AddHandler dgv.CellValueChanged, AddressOf Dgv_CellValueChanged_UnitChanged
    End Sub

    Private Sub Dgv_CurrentCellDirtyStateChanged(sender As Object, e As EventArgs)
        Dim dgv = estimateDataGridView
        If dgv.IsCurrentCellDirty Then
            dgv.CommitEdit(DataGridViewDataErrorContexts.Commit)
        End If
    End Sub

    Private Sub LoadItemMaster()
        Dim sql As String =
    "SELECT item_id, item_name, " &
    "unit1, quantity1, unit1_price, " &
    "unit2, quantity2, unit2_price, " &
    "COALESCE(conversion_qty,1) AS conversion_qty, " &
    "COALESCE(is_lot_item,'F') AS is_lot_item, " &  ' ★追加
    "default_price " &
    "FROM item_master " &
    "WHERE is_active = 1 " &
    "ORDER BY item_name;"

        itemTable = New DataTable()

        Try
            Using conn As New MySqlConnection(connectionString)
                conn.Open()
                Using da As New MySqlDataAdapter(sql, conn)
                    da.Fill(itemTable)
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show($"アイテムマスタ読込エラー: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            itemTable = Nothing
        End Try
    End Sub

    Private Sub BindItemComboColumn()
        If itemTable Is Nothing Then Return
        Dim dgv = estimateDataGridView
        Dim col = TryCast(dgv.Columns("item_id"), DataGridViewComboBoxColumn)
        If col Is Nothing Then Return

        col.DataSource = itemTable
        col.ValueMember = "item_id"
        col.DisplayMember = "item_name"
    End Sub

    Private Sub Dgv_CellValueChanged_ItemSelected(sender As Object, e As DataGridViewCellEventArgs)
        Dim dgv = estimateDataGridView
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Return
        If dgv.Columns(e.ColumnIndex).Name <> "item_id" Then Return

        Dim row = dgv.Rows(e.RowIndex)
        If row Is Nothing OrElse row.IsNewRow Then Return

        Dim v = row.Cells("item_id").Value
        If v Is Nothing OrElse v Is DBNull.Value Then Return


        Dim itemId As Integer
        If Not Integer.TryParse(v.ToString(), itemId) Then Return
        If itemTable Is Nothing Then Return

        Dim found = itemTable.Select($"item_id={itemId}")
        If found.Length = 0 Then Return

        Dim unit1 As String = found(0)("unit1").ToString()
        Dim unit2 As String = found(0)("unit2").ToString()

        Dim p1 As Integer = 0, p2 As Integer = 0, defPrice As Integer = 0
        Integer.TryParse(found(0)("unit1_price").ToString(), p1)
        Integer.TryParse(found(0)("unit2_price").ToString(), p2)
        Integer.TryParse(found(0)("default_price").ToString(), defPrice)

        If p1 = 0 Then p1 = defPrice
        If p2 = 0 Then p2 = defPrice

        Dim unitCell = TryCast(row.Cells("unit"), DataGridViewComboBoxCell)
        If unitCell IsNot Nothing Then
            unitCell.Items.Clear()
            If unit1 <> "" Then unitCell.Items.Add(unit1)
            If unit2 <> "" AndAlso unit2 <> unit1 Then unitCell.Items.Add(unit2)

            Dim chosenUnit As String = If(unit2 <> "", unit2, unit1) ' 箱優先
            row.Cells("unit").Value = chosenUnit

            Dim price As Integer
            If chosenUnit = unit2 Then
                price = p2
            ElseIf chosenUnit = unit1 Then
                price = p1
            Else
                price = defPrice
            End If

            row.Cells("unit_price").Value = price.ToString()
        End If

        RecalcAmountRow(row)
        FillLotComboForRow(row, itemId)

        Dim isLot As String = "F"
        If itemTable.Columns.Contains("is_lot_item") Then
            isLot = found(0)("is_lot_item").ToString().Trim().ToUpperInvariant()
            If isLot <> "T" Then isLot = "F"
        End If
        row.Cells("is_lot_item").Value = isLot

        ' ロット管理しないなら：ロット割当情報をクリアしてUIも空に
        If isLot = "F" Then
            row.Cells("lot_summary").Value = ""
            Dim key As String = If(row.Cells("row_key").Value, "").ToString()
            If key <> "" AndAlso LotAllocMap.ContainsKey(key) Then LotAllocMap.Remove(key)
            row.Cells("lot_id").Value = Nothing
        Else
            ' ロット管理するなら、ロット候補をロード（表示しない運用でもOK）
            FillLotComboForRow(row, itemId)
        End If


    End Sub






    Private Sub Dgv_CellValueChanged_UnitChanged(sender As Object, e As DataGridViewCellEventArgs)
        Dim dgv = estimateDataGridView
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Return
        If dgv.Columns(e.ColumnIndex).Name <> "unit" Then Return

        Dim row = dgv.Rows(e.RowIndex)
        If row Is Nothing OrElse row.IsNewRow Then Return

        Dim itemIdObj = row.Cells("item_id").Value
        If itemIdObj Is Nothing OrElse itemIdObj Is DBNull.Value Then Return

        Dim itemId As Integer
        If Not Integer.TryParse(itemIdObj.ToString(), itemId) Then Return
        If itemTable Is Nothing Then Return

        Dim found = itemTable.Select($"item_id={itemId}")
        If found.Length = 0 Then Return

        Dim unit1 As String = found(0)("unit1").ToString()
        Dim unit2 As String = found(0)("unit2").ToString()

        Dim p1 As Integer = 0, p2 As Integer = 0, defPrice As Integer = 0
        Integer.TryParse(found(0)("unit1_price").ToString(), p1)
        Integer.TryParse(found(0)("unit2_price").ToString(), p2)
        Integer.TryParse(found(0)("default_price").ToString(), defPrice)

        If p1 = 0 Then p1 = defPrice
        If p2 = 0 Then p2 = defPrice

        Dim chosenUnit As String = SafeCellStr(row, "unit")

        Dim price As Integer
        If chosenUnit = unit2 Then
            price = p2
        ElseIf chosenUnit = unit1 Then
            price = p1
        Else
            price = defPrice
        End If

        row.Cells("unit_price").Value = price.ToString()
        RecalcAmountRow(row)
    End Sub



    Private Sub FillLotComboForRow(row As DataGridViewRow, itemId As Integer)
        Dim dt As New DataTable()
        Dim sql As String =
        "SELECT lot_id, lot_no FROM lot " &
        "WHERE item_id=@item_id AND is_active=1 AND qty_on_hand_pieces>0 " &
        "ORDER BY received_date, lot_no;"

        Using conn As New MySqlConnection(connectionString)
            conn.Open()
            Using da As New MySqlDataAdapter(sql, conn)
                da.SelectCommand.Parameters.AddWithValue("@item_id", itemId)
                da.Fill(dt)
            End Using
        End Using

        Dim cell = TryCast(row.Cells("lot_id"), DataGridViewComboBoxCell)
        If cell Is Nothing Then Return

        cell.DataSource = dt
        cell.ValueMember = "lot_id"
        cell.DisplayMember = "lot_no"

        ' 先頭を自動選択（必要なら）
        If dt.Rows.Count > 0 Then
            row.Cells("lot_id").Value = dt.Rows(0)("lot_id")
        Else
            row.Cells("lot_id").Value = Nothing
        End If
    End Sub


    ' =========================
    ' item_master helpers
    ' =========================
    Private Function GetItemNameFromMaster(itemId As Integer) As String
        If itemTable Is Nothing Then Return ""
        Dim found = itemTable.Select($"item_id={itemId}")
        If found.Length = 0 Then Return ""
        Return found(0)("item_name").ToString()
    End Function

    Private Function GetConversionQtyFromMaster(itemId As Integer) As Integer
        If itemTable IsNot Nothing Then
            Dim found = itemTable.Select($"item_id={itemId}")
            If found.Length > 0 AndAlso itemTable.Columns.Contains("conversion_qty") Then
                Dim conv As Integer = 1
                Integer.TryParse(found(0)("conversion_qty").ToString(), conv)
                If conv <= 0 Then conv = 1
                Return conv
            End If
        End If

        ' フォールバック（DB直読み）
        Using conn As New MySqlConnection(connectionString)
            conn.Open()
            Dim sql As String = "SELECT COALESCE(conversion_qty,1) FROM item_master WHERE item_id=@id LIMIT 1"
            Using cmd As New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@id", itemId)
                Dim r = cmd.ExecuteScalar()
                Dim v As Integer = 1
                If r IsNot Nothing AndAlso r IsNot DBNull.Value Then Integer.TryParse(r.ToString(), v)
                If v <= 0 Then v = 1
                Return v
            End Using
        End Using
    End Function

    Private Sub ApplyInventoryLedger_ForShipmentUpdate(
    batchId As Long,
    details As List(Of DetailRow),
    conn As MySqlConnection,
    tx As MySqlTransaction
)
        ' 0) header をロックして save_seq を進める
        Dim newSeq As Integer
        Dim oldSeq As Integer

        Using cmdLock As New MySqlCommand(
        "SELECT save_seq FROM shipments_header WHERE shipment_batch_id=@id FOR UPDATE", conn, tx)
            cmdLock.Parameters.AddWithValue("@id", batchId)
            Dim r = cmdLock.ExecuteScalar()
            oldSeq = If(r Is Nothing OrElse IsDBNull(r), 0, Convert.ToInt32(r))
        End Using

        newSeq = oldSeq + 1

        Using cmdUpd As New MySqlCommand(
        "UPDATE shipments_header SET save_seq=@seq WHERE shipment_batch_id=@id", conn, tx)
            cmdUpd.Parameters.AddWithValue("@seq", newSeq)
            cmdUpd.Parameters.AddWithValue("@id", batchId)
            cmdUpd.ExecuteNonQuery()
        End Using

        ' 1) 旧台帳(ISSUE)を集計し、item_master.quantity1 を戻す（＋）
        Dim oldAgg As New Dictionary(Of Integer, Integer)() ' itemId -> sum(issue_qty_abs)

        Using cmdOld As New MySqlCommand(
        "SELECT item_id, SUM(-qty_delta) AS qty_out " &
        "FROM inventory_ledger " &
        "WHERE ref_type='SHIPMENT' AND ref_id=@id AND save_seq=@seq " &
        "  AND entry_type='ISSUE' AND is_void=0 " &
        "GROUP BY item_id", conn, tx)

            cmdOld.Parameters.AddWithValue("@id", batchId)
            cmdOld.Parameters.AddWithValue("@seq", oldSeq)

            Using rdr = cmdOld.ExecuteReader()
                While rdr.Read()
                    Dim itemId = Convert.ToInt32(rdr("item_id"))
                    Dim qtyOut = Convert.ToInt32(rdr("qty_out")) ' 出庫個数（正の値）
                    If qtyOut > 0 Then oldAgg(itemId) = qtyOut
                End While
            End Using
        End Using

        ' 1-1) 対象itemをロック（同時更新対策）
        If oldAgg.Count > 0 OrElse details.Count > 0 Then
            Dim lockIds As New HashSet(Of Integer)()

            For Each kv In oldAgg
                lockIds.Add(kv.Key)
            Next
            For Each d In details
                If d.ItemId IsNot Nothing AndAlso Not IsDBNull(d.ItemId) Then
                    lockIds.Add(Convert.ToInt32(Convert.ToInt64(d.ItemId)))
                End If
            Next

            Dim idList = lockIds.ToList()
            If idList.Count > 0 Then
                Dim inParams As New List(Of String)()
                Dim cmdText As String = "SELECT item_id FROM item_master WHERE item_id IN ("
                Using cmdLockItems As New MySqlCommand("", conn, tx)
                    For i = 0 To idList.Count - 1
                        Dim pName = "@p" & i
                        inParams.Add(pName)
                        cmdLockItems.Parameters.AddWithValue(pName, idList(i))
                    Next
                    cmdText &= String.Join(",", inParams) & ") FOR UPDATE"
                    cmdLockItems.CommandText = cmdText
                    cmdLockItems.ExecuteNonQuery()
                End Using
            End If
        End If

        ' 1-2) quantity1 を戻す（＋）
        Using cmdAdd As New MySqlCommand(
        "UPDATE item_master SET quantity1 = quantity1 + @delta WHERE item_id=@item_id", conn, tx)
            cmdAdd.Parameters.Add("@delta", MySqlDbType.Int32)
            cmdAdd.Parameters.Add("@item_id", MySqlDbType.Int32)

            For Each kv In oldAgg
                cmdAdd.Parameters("@delta").Value = kv.Value
                cmdAdd.Parameters("@item_id").Value = kv.Key
                cmdAdd.ExecuteNonQuery()
            Next
        End Using

        ' 1-3) 旧台帳を無効化（消さない）
        If oldSeq > 0 Then
            Using cmdVoid As New MySqlCommand(
            "UPDATE inventory_ledger " &
            "SET is_void=1, voided_at=NOW(), void_reason='shipment re-save' " &
            "WHERE ref_type='SHIPMENT' AND ref_id=@id AND save_seq=@seq AND is_void=0", conn, tx)
                cmdVoid.Parameters.AddWithValue("@id", batchId)
                cmdVoid.Parameters.AddWithValue("@seq", oldSeq)
                cmdVoid.ExecuteNonQuery()
            End Using
        End If

        ' 2) 新明細を ISSUE として台帳に積む（qty_deltaはマイナス）
        Dim insertedCount As Integer = 0

        Using cmdIns As New MySqlCommand(
        "INSERT INTO inventory_ledger " &
        "(item_id, qty_delta, ref_type, ref_id, ref_line_no, entry_type, save_seq) " &
        "VALUES (@item_id, @qty_delta, 'SHIPMENT', @ref_id, @line_no, 'ISSUE', @save_seq)", conn, tx)

            cmdIns.Parameters.Add("@item_id", MySqlDbType.Int32)
            cmdIns.Parameters.Add("@qty_delta", MySqlDbType.Int32)
            cmdIns.Parameters.Add("@ref_id", MySqlDbType.Int64).Value = batchId
            cmdIns.Parameters.Add("@line_no", MySqlDbType.Int32)
            cmdIns.Parameters.Add("@save_seq", MySqlDbType.Int32).Value = newSeq

            Dim lineNo As Integer = 0

            For Each d In details
                If d.ItemId Is Nothing OrElse IsDBNull(d.ItemId) Then Continue For
                Dim itemId As Integer = Convert.ToInt32(Convert.ToInt64(d.ItemId))

                Dim pieces As Integer = CInt(Math.Truncate(d.Quantity))
                If pieces <= 0 Then Continue For

                lineNo += 1
                cmdIns.Parameters("@item_id").Value = itemId
                cmdIns.Parameters("@qty_delta").Value = -pieces
                cmdIns.Parameters("@line_no").Value = lineNo
                cmdIns.ExecuteNonQuery()

                insertedCount += 1
            Next
        End Using

        ' ★超重要：INSERT 0件なら「VOIDだけ残る」ので、ここで止めてRollbackさせる
        If insertedCount = 0 Then
            Throw New ApplicationException("出荷明細が0件のため、台帳更新を中断しました（quantityが全て0/空の可能性）。")
        End If

        ' 3) item_master.quantity1 を出庫分だけ減らす（－）
        Using cmdSub As New MySqlCommand(
        "UPDATE item_master SET quantity1 = quantity1 - @delta WHERE item_id=@item_id", conn, tx)
            cmdSub.Parameters.Add("@delta", MySqlDbType.Int32)
            cmdSub.Parameters.Add("@item_id", MySqlDbType.Int32)

            For Each d In details
                If d.ItemId Is Nothing OrElse IsDBNull(d.ItemId) Then Continue For
                Dim itemId As Integer = Convert.ToInt32(Convert.ToInt64(d.ItemId))
                Dim pieces As Integer = CInt(Math.Truncate(d.Quantity))
                If pieces <= 0 Then Continue For

                cmdSub.Parameters("@delta").Value = pieces
                cmdSub.Parameters("@item_id").Value = itemId
                cmdSub.ExecuteNonQuery()
            Next
        End Using
    End Sub


    ' 親行ごとのロット割当を保持
    Private LotAllocMap As New Dictionary(Of String, List(Of LotAlloc))()

    Private Class LotAlloc
        Public Property LotId As Long
        Public Property LotNo As String
        Public Property Available As Integer
        Public Property QtyPieces As Integer
    End Class



    ' 在庫（個数）をDBから取得：item_master.quantity1 を在庫個数として扱う
    Private Function GetAvailableQtyPieces(itemId As Integer) As Integer
        Dim sql As String =
            "SELECT quantity1 " &
            "FROM item_master " &
            "WHERE item_id = @item_id;"

        Using conn As New MySqlConnection(connectionString),
              cmd As New MySqlCommand(sql, conn)

            cmd.Parameters.Add("@item_id", MySqlDbType.Int32).Value = itemId
            conn.Open()

            Dim result = cmd.ExecuteScalar()
            If result Is Nothing OrElse IsDBNull(result) Then
                Throw New Exception($"在庫が取得できません（item_id={itemId}）")
            End If

            Dim v As Integer = 0
            Integer.TryParse(result.ToString(), v)
            If v < 0 Then v = 0
            Return v
        End Using
    End Function

    Private Sub Dgv_CellContentClick_LotAlloc(sender As Object, e As DataGridViewCellEventArgs)
        Dim dgv = estimateDataGridView

        Dim row = dgv.Rows(e.RowIndex)

        Dim isLot As String = If(row.Cells("is_lot_item").Value, "F").ToString().Trim().ToUpperInvariant()
        If isLot <> "T" Then
            MessageBox.Show("このアイテムはロット管理ではありません。ロット割当は不要です。")
            Return
        End If

        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Return
        If dgv.Columns(e.ColumnIndex).Name <> "btn_lot_alloc" Then Return

        If row Is Nothing OrElse row.IsNewRow Then Return

        ' row_key
        Dim key As String = If(row.Cells("row_key").Value, "").ToString()
        If key = "" Then
            key = Guid.NewGuid().ToString()
            row.Cells("row_key").Value = key
        End If

        ' item_id 必須
        Dim itemObj = row.Cells("item_id").Value
        If itemObj Is Nothing OrElse itemObj Is DBNull.Value OrElse itemObj.ToString().Trim() = "" Then
            MessageBox.Show("先にアイテムを選択してください。")
            Return
        End If

        Dim itemId As Integer
        If Not Integer.TryParse(itemObj.ToString(), itemId) Then
            MessageBox.Show("アイテムIDが不正です。")
            Return
        End If

        ' 必要個数（個数列優先、無ければ 箱数×conversion）
        Dim requiredPieces As Integer = GetRequiredPiecesFromRow(row, itemId)
        If requiredPieces <= 0 Then
            MessageBox.Show("必要数量がありません。（数量2/数量 を入力してください）")
            Return
        End If

        Dim itemName As String = GetItemNameFromMaster(itemId)

        ' ===== ここがポイント：ship.LotAlloc → LotAllocForm.LotAlloc に変換して渡す =====
        Dim existingForForm As New List(Of LotAllocForm.LotAlloc)()

        If LotAllocMap.ContainsKey(key) AndAlso LotAllocMap(key) IsNot Nothing Then
            existingForForm = LotAllocMap(key).Select(Function(x) New LotAllocForm.LotAlloc With {
            .LotId = x.LotId,
            .LotNo = x.LotNo,
            .Available = x.Available,
            .QtyPieces = x.QtyPieces
        }).ToList()
        End If

        Using dlg As New LotAllocForm(connectionString, itemId, itemName, requiredPieces, existingForForm)

            dlg.StartPosition = FormStartPosition.CenterParent
            dlg.TopMost = True

            AddHandler dlg.Shown, Sub()
                                      dlg.TopMost = False
                                      dlg.Activate()
                                  End Sub

            Dim res As DialogResult = dlg.ShowDialog(Me)
            If res <> DialogResult.OK Then Return

            Dim allocs As List(Of LotAlloc) =
        dlg.ResultAllocs.Select(Function(a) New LotAlloc With {
            .LotId = a.LotId,
            .LotNo = a.LotNo,
            .Available = a.Available,
            .QtyPieces = a.QtyPieces
        }).ToList()

            LotAllocMap(key) = allocs
            row.Cells("lot_summary").Value =
        $"{allocs.Count}ロット / 合計{allocs.Sum(Function(x) x.QtyPieces)}個"
        End Using
    End Sub


    Private Function GetRequiredPiecesFromRow(row As DataGridViewRow, itemId As Integer) As Integer
        ' quantity（個数）が入っていればそれを優先
        Dim qStr = SafeCellStr(row, "quantity")
        Dim q As Integer
        If qStr <> "" AndAlso Integer.TryParse(qStr, q) AndAlso q > 0 Then Return q

        ' 無ければ quantity2（箱）×conversion
        Dim q2Str = SafeCellStr(row, "quantity2")
        Dim q2 As Integer
        If q2Str <> "" AndAlso Integer.TryParse(q2Str, q2) AndAlso q2 > 0 Then
            Dim conv As Integer = GetConversionQtyFromMaster(itemId)
            Dim tmp As Long = CLng(q2) * CLng(conv)
            If tmp > Integer.MaxValue Then Return 0
            Return CInt(tmp)
        End If

        Return 0
    End Function


    Private Function GetParentAmount_BoxPrice(row As DataGridViewRow) As Long
        Dim aStr = SafeCellStr(row, "amount")
        Dim a As Long
        If aStr <> "" AndAlso Long.TryParse(aStr, a) AndAlso a > 0 Then Return a

        Dim q2Str = SafeCellStr(row, "quantity2")
        Dim pStr = SafeCellStr(row, "unit_price")

        Dim q2 As Long, p As Long
        If q2Str = "" OrElse pStr = "" Then Return 0
        If Not Long.TryParse(q2Str, q2) Then Return 0
        If Not Long.TryParse(pStr, p) Then Return 0
        If q2 <= 0 OrElse p < 0 Then Return 0

        ' 箱数×箱単価
        Try
            Return CheckedMul(q2, p)
        Catch
            Return 0
        End Try
    End Function
    ' =========================
    ' small utils
    ' =========================
    Private Function SafeCellStr(row As DataGridViewRow, colName As String) As String
        Try
            If row Is Nothing Then Return ""
            If Not estimateDataGridView.Columns.Contains(colName) Then Return ""
            Dim o = row.Cells(colName).Value
            If o Is Nothing OrElse o Is DBNull.Value Then Return ""
            Return o.ToString().Trim()
        Catch
            Return ""
        End Try
    End Function

    Private Sub saveBtn_ClientSizeChanged(sender As Object, e As EventArgs) Handles saveBtn.ClientSizeChanged

    End Sub


    Private Sub saveBtn_Click(sender As Object, e As EventArgs) Handles saveBtn.Click

        Try
            SaveShipment_New()
        Catch ex As Exception
            'MessageBox.Show($"保存エラー: {ex.Message}", "Error",
            '                MessageBoxButtons.OK, MessageBoxIcon.Error)
            'MessageBox.Show(ToUserMessage(ex), "Error",
            '       MessageBoxButtons.OK, MessageBoxIcon.Error)

            MessageBox.Show(ex.ToString(), "保存エラー詳細", MessageBoxButtons.OK, MessageBoxIcon.Error)

        End Try
    End Sub


    Private Function ToUserMessage(ex As Exception) As String
        Dim m As String = ""
        If ex IsNot Nothing AndAlso ex.Message IsNot Nothing Then
            m = ex.Message
        End If

        ' BIGINT UNSIGNED out of range（だいたい「マイナスになった」系）
        If m.IndexOf("BIGINT UNSIGNED value is out of range", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Dim minusPos = m.LastIndexOf("-"c)
            Dim deltaStr As String = ""
            If minusPos >= 0 AndAlso minusPos + 1 < m.Length Then
                deltaStr = m.Substring(minusPos + 1).Trim()
            End If

            Dim delta As Integer
            If Integer.TryParse(deltaStr, delta) AndAlso delta > 0 Then
                Return $"在庫が足りません（{delta}個分の出庫で在庫がマイナスになります）"
            End If

            Return "在庫が足りません（出庫すると在庫がマイナスになります）"
        End If

        ' Duplicate entry（ユニーク制約）
        If m.IndexOf("Duplicate entry", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return "同じデータが既に存在します（重複登録はできません）"
        End If

        ' Data too long
        If m.IndexOf("Data too long", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return "入力が長すぎます（文字数制限を超えました）"
        End If

        ' フォールバック
        Return $"保存に失敗しました：{m}"
    End Function



    Private Function ReserveLotUnits(lotId As Long, n As Integer, conn As MySqlConnection, tx As MySqlTransaction) As List(Of Long)
    Dim ids As New List(Of Long)()
    Dim nSafe As Integer = Math.Max(0, Math.Min(n, 100000))

    Dim sql As String =
"SELECT unit_id
   FROM lot_unit
  WHERE lot_id=@lot_id
    AND status='ON_HAND'
  ORDER BY unit_id
  LIMIT " & nSafe & "
  FOR UPDATE;"

    Using cmd As New MySqlCommand(sql, conn, tx)
        cmd.Parameters.AddWithValue("@lot_id", lotId)
        Using rdr = cmd.ExecuteReader()
            While rdr.Read()
                ids.Add(Convert.ToInt64(rdr("unit_id")))
            End While
        End Using
    End Using

    If ids.Count <> nSafe Then
        Throw New Exception($"lot_unit 不足:lot_id={lotId}, 要求={nSafe}, 取得={ids.Count}")
    End If

        ' ALLOCATED に更新
        Dim inParams = String.Join(",", ids.Select(Function(x, i) "@u" & i))
        Using cmdUpd As New MySqlCommand("UPDATE lot_unit SET status='ALLOCATED' WHERE unit_id IN (" & inParams & ")", conn, tx)
            For i = 0 To ids.Count - 1
                cmdUpd.Parameters.AddWithValue("@u" & i, ids(i))
            Next
            cmdUpd.ExecuteNonQuery()
        End Using

        Return ids
End Function


    Private Sub InsertUnitAlloc(detailId As Long, unitIds As List(Of Long), conn As MySqlConnection, tx As MySqlTransaction)
        Using cmd As New MySqlCommand(
        "INSERT INTO shipment_unit_alloc (shipment_detail_id, unit_id) VALUES (@d, @u)", conn, tx)
            cmd.Parameters.Add("@d", MySqlDbType.Int64).Value = detailId
            cmd.Parameters.Add("@u", MySqlDbType.Int64)

            For Each uid In unitIds
                cmd.Parameters("@u").Value = uid
                cmd.ExecuteNonQuery()
            Next
        End Using
    End Sub


    Private Sub MarkUnitsIssued(unitIdsAll As List(Of Long), conn As MySqlConnection, tx As MySqlTransaction)
        If unitIdsAll.Count = 0 Then Return
        Dim inParams = String.Join(",", unitIdsAll.Select(Function(x, i) "@i" & i))
        Using cmd As New MySqlCommand("UPDATE lot_unit SET status='ISSUED' WHERE unit_id IN (" & inParams & ")", conn, tx)
            For i = 0 To unitIdsAll.Count - 1
                cmd.Parameters.AddWithValue("@i" & i, unitIdsAll(i))
            Next
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Private Function GetAccountCodeFromItem(itemId As Integer, kind As String, conn As MySqlConnection, tx As MySqlTransaction) As String
        ' kind: "COGS" / "INVENTORY" / "REVENUE"
        Dim col As String
        Select Case kind.ToUpperInvariant()
            Case "COGS" : col = "cogs_account_id"
            Case "INVENTORY" : col = "inventory_account_id"
            Case "REVENUE" : col = "revenue_account_id"
            Case Else
                Throw New Exception("GetAccountCodeFromItem: kind が不正です: " & kind)
        End Select

        ' 1) item_master から account_id を取る
        Dim accountId As Long = 0
        Using cmd As New MySqlCommand($"SELECT {col} FROM item_master WHERE item_id=@item_id LIMIT 1", conn, tx)
            cmd.Parameters.AddWithValue("@item_id", itemId)
            Dim r = cmd.ExecuteScalar()
            If r Is Nothing OrElse IsDBNull(r) Then
                Throw New Exception($"item_master に {col} がありません（item_id={itemId}）")
            End If
            Long.TryParse(r.ToString(), accountId)
        End Using
        If accountId <= 0 Then
            Throw New Exception($"item_master.{col} が未設定です（item_id={itemId}）")
        End If

        ' 2) account_master から account_code を引く（テーブル名はあなたの環境に合わせて）
        Using cmd As New MySqlCommand("SELECT account_code FROM account_master WHERE account_id=@id LIMIT 1", conn, tx)
            cmd.Parameters.AddWithValue("@id", accountId)
            Dim r = cmd.ExecuteScalar()
            If r Is Nothing OrElse IsDBNull(r) Then
                Throw New Exception($"account_master に account_id={accountId} が見つかりません")
            End If
            Return r.ToString()
        End Using
    End Function


    Private Function GetCostPerPiece(itemId As Integer, conn As MySqlConnection, tx As MySqlTransaction) As Integer
        Using cmd As New MySqlCommand("SELECT COALESCE(cost_per_piece,0) FROM item_master WHERE item_id=@id LIMIT 1", conn, tx)
            cmd.Parameters.AddWithValue("@id", itemId)
            Dim r = cmd.ExecuteScalar()
            Dim v As Integer = 0
            If r IsNot Nothing AndAlso Not IsDBNull(r) Then Integer.TryParse(r.ToString(), v)
            Return Math.Max(0, v)
        End Using
    End Function


    Private Function InsertGlHeader(refType As String, refId As Long, journalDate As Date, memo As String,
                                saveSeq As Integer,
                                conn As MySqlConnection, tx As MySqlTransaction) As Long

        Using cmd As New MySqlCommand(
        "INSERT INTO gl_journal_header " &
        "(tran_id, save_seq, journal_date, tran_type, ref_type, ref_id, memo, posted) " &
        "VALUES (@tran_id, @save_seq, @d, @tran, @ref_type, @ref_id, @memo, 1);", conn, tx)

            cmd.Parameters.Add("@tran_id", MySqlDbType.Int64).Value = refId     ' ★ shipment_batch_id を tran_id に
            cmd.Parameters.Add("@save_seq", MySqlDbType.Int32).Value = saveSeq  ' ★ shipments_header.save_seq と同じ
            cmd.Parameters.Add("@d", MySqlDbType.Date).Value = journalDate
            cmd.Parameters.Add("@tran", MySqlDbType.VarChar).Value = "SHIPMENT"
            cmd.Parameters.Add("@ref_type", MySqlDbType.VarChar).Value = refType
            cmd.Parameters.Add("@ref_id", MySqlDbType.Int64).Value = refId
            cmd.Parameters.Add("@memo", MySqlDbType.VarChar).Value = If(memo, "")
            cmd.ExecuteNonQuery()
        End Using

        Using cmdId As New MySqlCommand("SELECT LAST_INSERT_ID();", conn, tx)
            Return Convert.ToInt64(cmdId.ExecuteScalar())
        End Using
    End Function




    Private Sub CreateGlForShipment_CogsOnly(
    shipmentBatchId As Long,
    shipmentDate As Date,
    customerCode As String,
    parentRows As List(Of DataGridViewRow),
    saveSeq As Integer,                ' ★追加
    conn As MySqlConnection,
    tx As MySqlTransaction
)

        ' 1) 仕訳金額を account_code ごとに集計
        Dim debitMap As New Dictionary(Of String, Decimal)()   ' COGS
        Dim creditMap As New Dictionary(Of String, Decimal)()  ' INVENTORY

        For Each row In parentRows
            Dim itemId As Integer = Convert.ToInt32(row.Cells("item_id").Value)

            Dim requiredPieces As Integer = GetRequiredPiecesFromRow(row, itemId)
            If requiredPieces <= 0 Then Continue For

            Dim cpp As Integer = GetCostPerPiece(itemId, conn, tx)
            If cpp <= 0 Then
                Throw New Exception($"cost_per_piece が未設定です（item_id={itemId}）。COGS仕訳を作れません。")
            End If

            Dim amount As Decimal = CDec(requiredPieces) * CDec(cpp)
            If amount <= 0D Then Continue For

            Dim cogsCode As String = GetAccountCodeFromItem(itemId, "COGS", conn, tx)
            Dim invCode As String = GetAccountCodeFromItem(itemId, "INVENTORY", conn, tx)

            If Not debitMap.ContainsKey(cogsCode) Then debitMap(cogsCode) = 0D
            debitMap(cogsCode) += amount

            If Not creditMap.ContainsKey(invCode) Then creditMap(invCode) = 0D
            creditMap(invCode) += amount
        Next

        If debitMap.Count = 0 AndAlso creditMap.Count = 0 Then
            Throw New Exception("COGS仕訳の作成対象がありません（原価0 or 数量0の可能性）。")
        End If

        ' 2) ヘッダ作成
        Dim memo As String = $"SHIPMENT COGS ONLY / customer={customerCode}"
Dim journalId As Long = InsertGlHeader(
    refType:="SHIPMENT_COGS",
    refId:=shipmentBatchId,
    journalDate:=shipmentDate,
    memo:=memo,
    saveSeq:=saveSeq,
    conn:=conn,
    tx:=tx
)

        ' 3) ライン INSERT
        Dim lineNo As Integer = 0

        Using cmd As New MySqlCommand(
        "INSERT INTO gl_journal_line " &
        "(journal_id, line_no, account_code, debit, credit, customer_code, item_id, dept_code) " &
        "VALUES (@jid, @ln, @ac, @d, @c, @cust, NULL, NULL);", conn, tx)

            cmd.Parameters.Add("@jid", MySqlDbType.Int64).Value = journalId
            cmd.Parameters.Add("@ln", MySqlDbType.Int32)
            cmd.Parameters.Add("@ac", MySqlDbType.VarChar)
            cmd.Parameters.Add("@d", MySqlDbType.Decimal)
            cmd.Parameters.Add("@c", MySqlDbType.Decimal)
            cmd.Parameters.Add("@cust", MySqlDbType.VarChar).Value = If(customerCode, "")

            ' 借方：COGS
            For Each kv In debitMap
                If kv.Value = 0D Then Continue For
                lineNo += 1
                cmd.Parameters("@ln").Value = lineNo
                cmd.Parameters("@ac").Value = kv.Key
                cmd.Parameters("@d").Value = kv.Value
                cmd.Parameters("@c").Value = 0D
                cmd.ExecuteNonQuery()
            Next

            ' 貸方：INVENTORY
            For Each kv In creditMap
                If kv.Value = 0D Then Continue For
                lineNo += 1
                cmd.Parameters("@ln").Value = lineNo
                cmd.Parameters("@ac").Value = kv.Key
                cmd.Parameters("@d").Value = 0D
                cmd.Parameters("@c").Value = kv.Value
                cmd.ExecuteNonQuery()
            Next
        End Using
    End Sub


    'Private Sub DeleteGlByRef(refType As String, refId As Long,
    '                      conn As MySqlConnection, tx As MySqlTransaction)

    '    ' refType/refId に紐づくGLを全削除（再生成前提）
    '    Dim sqlLines As String =
    '    "DELETE gl " &
    '    "FROM gl_journal_line gl " &
    '    "JOIN gl_journal_header gh ON gh.journal_id = gl.journal_id " &
    '    "WHERE gh.ref_type=@rt AND gh.ref_id=@rid;"

    '    Using cmd As New MySqlCommand(sqlLines, conn, tx)
    '        cmd.Parameters.AddWithValue("@rt", refType)
    '        cmd.Parameters.AddWithValue("@rid", refId)
    '        cmd.ExecuteNonQuery()
    '    End Using

    '    Using cmd As New MySqlCommand(
    '    "DELETE FROM gl_journal_header WHERE ref_type=@rt AND ref_id=@rid;", conn, tx)
    '        cmd.Parameters.AddWithValue("@rt", refType)
    '        cmd.Parameters.AddWithValue("@rid", refId)
    '        cmd.ExecuteNonQuery()
    '    End Using
    'End Sub

    Private Function InsertGlHeader(refType As String, refId As Long, journalDate As Date, memo As String,
                                saveSeq As Integer,
                                conn As MySqlConnection, tx As MySqlTransaction,
                                Optional isReversal As Integer = 0,
                                Optional reversedJournalId As Object = Nothing) As Long

        Using cmd As New MySqlCommand(
        "INSERT INTO gl_journal_header " &
        "(tran_id, save_seq, journal_date, tran_type, ref_type, ref_id, memo, posted, is_reversal, reversed_journal_id) " &
        "VALUES (@tran_id, @save_seq, @d, @tran, @ref_type, @ref_id, @memo, 1, @is_rev, @rev_id);", conn, tx)

            cmd.Parameters.Add("@tran_id", MySqlDbType.Int64).Value = refId
            cmd.Parameters.Add("@save_seq", MySqlDbType.Int32).Value = saveSeq
            cmd.Parameters.Add("@d", MySqlDbType.Date).Value = journalDate
            cmd.Parameters.Add("@tran", MySqlDbType.VarChar).Value = "SHIPMENT"
            cmd.Parameters.Add("@ref_type", MySqlDbType.VarChar).Value = refType
            cmd.Parameters.Add("@ref_id", MySqlDbType.Int64).Value = refId
            cmd.Parameters.Add("@memo", MySqlDbType.VarChar).Value = If(memo, "")
            cmd.Parameters.Add("@is_rev", MySqlDbType.Int32).Value = If(isReversal <> 0, 1, 0)

            If reversedJournalId Is Nothing OrElse reversedJournalId Is DBNull.Value Then
                cmd.Parameters.Add("@rev_id", MySqlDbType.Int64).Value = DBNull.Value
            Else
                cmd.Parameters.Add("@rev_id", MySqlDbType.Int64).Value = Convert.ToInt64(reversedJournalId)
            End If

            cmd.ExecuteNonQuery()
        End Using

        Using cmdId As New MySqlCommand("SELECT LAST_INSERT_ID();", conn, tx)
            Return Convert.ToInt64(cmdId.ExecuteScalar())
        End Using
    End Function

    Private Sub CreateGlForShipment_Sales(
    shipmentBatchId As Long,
    shipmentDate As Date,
    customerCode As String,
    parentRows As List(Of DataGridViewRow),
    saveSeq As Integer,
    conn As MySqlConnection,
    tx As MySqlTransaction
)
        ' ===== 設定（必要なら将来テーブル化）=====
        Const AR_CODE As String = "1100"      ' 売掛金
        Const TAX_CODE As String = "2100"     ' 仮受消費税
        Const TAX_RATE As Decimal = 0.1D      ' 10%  (軽減税率等あるなら行単位で持つ設計へ)

        ' 1) 税抜売上を、revenue科目ごとに集計
        Dim creditRevenue As New Dictionary(Of String, Decimal)() ' revenue_code -> net_amount
        Dim netTotal As Decimal = 0D

        For Each row In parentRows
            Dim itemId As Integer = Convert.ToInt32(row.Cells("item_id").Value)

            ' あなたの定義：amount は「箱数×箱単価」（税抜前提）
            Dim net As Decimal = CDec(GetParentAmount_BoxPrice(row))
            If net <= 0D Then Continue For

            Dim revCode As String = GetAccountCodeFromItem(itemId, "REVENUE", conn, tx)

            If Not creditRevenue.ContainsKey(revCode) Then creditRevenue(revCode) = 0D
            creditRevenue(revCode) += net
            netTotal += net
        Next

        If netTotal <= 0D Then
            Throw New Exception("売上仕訳の作成対象がありません（売上0の可能性）。")
        End If

        ' 2) 消費税計算（伝票単位）
        '    ※丸めはあなたの運用に合わせて：Round/Truncate/Floor などに変更OK
        Dim taxAmount As Decimal = Decimal.Round(netTotal * TAX_RATE, 0, MidpointRounding.AwayFromZero)
        Dim grossTotal As Decimal = netTotal + taxAmount

        ' 3) 既存の売上GLを削除して再生成（同一shipmentで再保存対応）
        'DeleteGlByRef("SHIPMENT_SALES", shipmentBatchId, conn, tx)

        ' 4) ヘッダ作成
        Dim memo As String = $"SHIPMENT SALES / customer={customerCode}"
        Dim journalId As Long = InsertGlHeader(
        refType:="SHIPMENT_SALES",
        refId:=shipmentBatchId,
        journalDate:=shipmentDate,
        memo:=memo,
        saveSeq:=saveSeq,
        conn:=conn,
        tx:=tx
    )

        ' 5) ライン INSERT
        Dim lineNo As Integer = 0

        Using cmd As New MySqlCommand(
        "INSERT INTO gl_journal_line " &
        "(journal_id, line_no, account_code, debit, credit, customer_code, item_id, dept_code) " &
        "VALUES (@jid, @ln, @ac, @d, @c, @cust, NULL, NULL);", conn, tx)

            cmd.Parameters.Add("@jid", MySqlDbType.Int64).Value = journalId
            cmd.Parameters.Add("@ln", MySqlDbType.Int32)
            cmd.Parameters.Add("@ac", MySqlDbType.VarChar)
            cmd.Parameters.Add("@d", MySqlDbType.Decimal)
            cmd.Parameters.Add("@c", MySqlDbType.Decimal)
            cmd.Parameters.Add("@cust", MySqlDbType.VarChar).Value = If(customerCode, "")

            ' 借方：売掛金（税込）
            lineNo += 1
            cmd.Parameters("@ln").Value = lineNo
            cmd.Parameters("@ac").Value = AR_CODE
            cmd.Parameters("@d").Value = grossTotal
            cmd.Parameters("@c").Value = 0D
            cmd.ExecuteNonQuery()

            ' 貸方：売上（科目別・税抜）
            For Each kv In creditRevenue
                If kv.Value = 0D Then Continue For
                lineNo += 1
                cmd.Parameters("@ln").Value = lineNo
                cmd.Parameters("@ac").Value = kv.Key
                cmd.Parameters("@d").Value = 0D
                cmd.Parameters("@c").Value = kv.Value
                cmd.ExecuteNonQuery()
            Next

            ' 貸方：仮受消費税
            If taxAmount <> 0D Then
                lineNo += 1
                cmd.Parameters("@ln").Value = lineNo
                cmd.Parameters("@ac").Value = TAX_CODE
                cmd.Parameters("@d").Value = 0D
                cmd.Parameters("@c").Value = taxAmount
                cmd.ExecuteNonQuery()
            End If
        End Using
    End Sub



    Private Function FindLatestPostedJournalId(refType As String, refId As Long,
                                          conn As MySqlConnection, tx As MySqlTransaction) As Long

        Using cmd As New MySqlCommand(
        "SELECT journal_id " &
        "FROM gl_journal_header " &
        "WHERE ref_type=@rt AND ref_id=@rid " &
        "  AND posted=1 AND is_reversal=0 " &
        "ORDER BY journal_id DESC " &
        "LIMIT 1;", conn, tx)

            cmd.Parameters.AddWithValue("@rt", refType)
            cmd.Parameters.AddWithValue("@rid", refId)

            Dim r = cmd.ExecuteScalar()
            If r Is Nothing OrElse IsDBNull(r) Then Return 0
            Return Convert.ToInt64(r)
        End Using
    End Function


    Private Function CreateReversalJournal(oldJournalId As Long,
                                       refType As String, refId As Long,
                                       journalDate As Date, saveSeq As Integer,
                                       memo As String,
                                       conn As MySqlConnection, tx As MySqlTransaction) As Long

        If oldJournalId <= 0 Then Return 0

        ' 逆仕訳ヘッダ
        Dim revJournalId As Long = InsertGlHeader(
        refType:=refType,
        refId:=refId,
        journalDate:=journalDate,
        memo:=memo,
        saveSeq:=saveSeq,
        conn:=conn,
        tx:=tx,
        isReversal:=1,
        reversedJournalId:=oldJournalId
    )

        ' ライン：debit/credit を入れ替えて複製
        Using cmd As New MySqlCommand(
        "INSERT INTO gl_journal_line " &
        "(journal_id, line_no, account_code, debit, credit, customer_code, item_id, dept_code) " &
        "SELECT @new_jid, line_no, account_code, credit AS debit, debit AS credit, customer_code, item_id, dept_code " &
        "FROM gl_journal_line " &
        "WHERE journal_id=@old_jid;", conn, tx)

            cmd.Parameters.AddWithValue("@new_jid", revJournalId)
            cmd.Parameters.AddWithValue("@old_jid", oldJournalId)
            cmd.ExecuteNonQuery()
        End Using

        Return revJournalId
    End Function



    ' ==========================================================
    ' 保存（新規作成）：header + detail(親行) + lot_alloc + lot減算 + item減算 + 台帳
    ' ==========================================================

    Private Sub SaveShipment_New()


        ' ★ 編集確定（ComboBox未確定対策）
        estimateDataGridView.EndEdit()
        estimateDataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit)

        Dim shipmentDate As Date = dtpShipmentDate.Value.Date
        Dim customerCode As String = If(txtCustomerCode.Text, "").Trim()
        If customerCode = "" Then
            MessageBox.Show("顧客コードを入力してください。")
            txtCustomerCode.Focus()
            Return
        End If

        ' --------------------------
        ' 対象行の収集＆事前検証
        ' --------------------------
        Dim parentRows As New List(Of DataGridViewRow)()

        For Each row As DataGridViewRow In estimateDataGridView.Rows
            If row Is Nothing OrElse row.IsNewRow Then Continue For

            Dim q2s = SafeCellStr(row, "quantity2")
            Dim qs = SafeCellStr(row, "quantity")

            ' 入力が無い行は無視
            If q2s = "" AndAlso qs = "" Then Continue For

            Dim itemObj = row.Cells("item_id").Value
            If itemObj Is Nothing OrElse itemObj Is DBNull.Value OrElse itemObj.ToString().Trim() = "" Then
                Throw New Exception($"アイテム未選択の行があります（行 {row.Index + 1}）")
            End If

            Dim itemId As Integer
            If Not Integer.TryParse(itemObj.ToString(), itemId) Then
                Throw New Exception($"アイテムIDが不正です（行 {row.Index + 1}）")
            End If

            Dim requiredPieces As Integer = GetRequiredPiecesFromRow(row, itemId)
            If requiredPieces <= 0 Then
                Throw New Exception($"数量がありません（行 {row.Index + 1}）")
            End If

            Dim isLot As String = If(row.Cells("is_lot_item").Value, "F").ToString().Trim().ToUpperInvariant()
            If isLot <> "T" Then isLot = "F"

            Dim key As String = If(row.Cells("row_key").Value, "").ToString()

            ' ロット管理(T)だけ割当必須
            If isLot = "T" Then
                If key = "" OrElse Not LotAllocMap.ContainsKey(key) Then
                    Throw New Exception($"ロット割当がされていません（行 {row.Index + 1}）")
                End If

                Dim allocs = LotAllocMap(key)
                Dim sumAlloc As Integer = allocs.Sum(Function(x) x.QtyPieces)
                If sumAlloc <> requiredPieces Then
                    Throw New Exception($"ロット割当合計が一致しません（行 {row.Index + 1}）必要={requiredPieces} / 合計={sumAlloc}")
                End If
            Else
                ' 非ロット(F)：ロット割当が残ってたら消す（保険）
                If key <> "" AndAlso LotAllocMap.ContainsKey(key) Then LotAllocMap.Remove(key)
            End If

            parentRows.Add(row)
        Next

        If parentRows.Count = 0 Then
            Throw New Exception("保存対象の明細がありません。")
        End If

        ' --------------------------
        ' SQL
        ' --------------------------
        Dim sqlHeader As String =
        "INSERT INTO shipments_header (shipment_date, customer_code, save_seq) " &
        "VALUES (@d, @c, 1);"

        Dim sqlGetId As String = "SELECT LAST_INSERT_ID();"

        ' ★ 画面の1行＝明細1行（lot_idはNULL、quantity=個数、quantity2=箱数）
        Dim sqlDetail As String =
        "INSERT INTO shipments_detail " &
        "(shipment_batch_id, line_no, item_id, lot_id, quantity, quantity2, unit_price, unit, remark) " &
        "VALUES " &
        "(@batch_id, @line_no, @item_id, NULL, @quantity, @quantity2, @unit_price, @unit, @remark);"

        ' ★ ロット割当（ロット管理のみ）
        Dim sqlAlloc As String =
        "INSERT INTO shipment_lot_alloc (shipment_detail_id, lot_id, qty_pieces) " &
        "VALUES (@detail_id, @lot_id, @qty);"

        ' ★ lot 減算（ロット管理のみ）
        Dim sqlUpdLot As String =
        "UPDATE lot " &
        "SET qty_on_hand_pieces = qty_on_hand_pieces - @delta " &
        "WHERE lot_id=@lot_id AND qty_on_hand_pieces >= @delta;"

        ' ★ item 減算（全アイテム共通）
        Dim sqlUpdItem As String =
        "UPDATE item_master " &
        "SET quantity1 = quantity1 - @delta, " &
        "    quantity2 = (quantity1 - @delta) DIV conversion_qty " &
        "WHERE item_id=@item_id AND quantity1 >= @delta;"

        ' ★ 台帳（ロット管理なら lot_id、非ロットなら lot_id=NULL）
        Dim sqlInsLedger As String =
        "INSERT INTO inventory_ledger " &
        "(item_id, lot_id, qty_delta, ref_type, ref_id, ref_line_no, entry_type, save_seq) " &
        "VALUES " &
        "(@item_id, @lot_id, @qty_delta, 'SHIPMENT', @ref_id, @line_no, 'ISSUE', @save_seq);"

        Using conn As New MySqlConnection(connectionString)


            conn.Open()

            Using tx = conn.BeginTransaction()
                Try
                    ' ======================
                    ' 1) header
                    ' ======================
                    Using cmdH As New MySqlCommand(sqlHeader, conn, tx)
                        cmdH.Parameters.Add("@d", MySqlDbType.Date).Value = shipmentDate
                        cmdH.Parameters.Add("@c", MySqlDbType.VarChar).Value = customerCode
                        cmdH.ExecuteNonQuery()
                    End Using


                    Dim batchId As Long
                    Using cmdId As New MySqlCommand(sqlGetId, conn, tx)
                        batchId = Convert.ToInt64(cmdId.ExecuteScalar())
                    End Using

                    Dim saveSeq As Integer = 1
                    Dim lineNo As Integer = 0

                    ' ★ ロット管理で確保した unit_id を最後に ISSUED にする用
                    Dim issuedUnitIdsAll As New List(Of Long)()

                    Using cmdD As New MySqlCommand(sqlDetail, conn, tx)
                        cmdD.Parameters.Add("@batch_id", MySqlDbType.Int64).Value = batchId
                        cmdD.Parameters.Add("@line_no", MySqlDbType.Int32)
                        cmdD.Parameters.Add("@item_id", MySqlDbType.Int32)
                        cmdD.Parameters.Add("@quantity", MySqlDbType.Int32)
                        cmdD.Parameters.Add("@quantity2", MySqlDbType.Int32)
                        cmdD.Parameters.Add("@unit_price", MySqlDbType.Int32)
                        cmdD.Parameters.Add("@unit", MySqlDbType.VarChar)
                        cmdD.Parameters.Add("@remark", MySqlDbType.VarChar)

                        Using cmdAlloc As New MySqlCommand(sqlAlloc, conn, tx)
                            cmdAlloc.Parameters.Add("@detail_id", MySqlDbType.Int64)
                            cmdAlloc.Parameters.Add("@lot_id", MySqlDbType.Int64)
                            cmdAlloc.Parameters.Add("@qty", MySqlDbType.Int32)

                            Using cmdLot As New MySqlCommand(sqlUpdLot, conn, tx)
                                cmdLot.Parameters.Add("@delta", MySqlDbType.Int32)
                                cmdLot.Parameters.Add("@lot_id", MySqlDbType.Int64)

                                Using cmdItem As New MySqlCommand(sqlUpdItem, conn, tx)
                                    cmdItem.Parameters.Add("@delta", MySqlDbType.Int32)
                                    cmdItem.Parameters.Add("@item_id", MySqlDbType.Int32)

                                    Using cmdL As New MySqlCommand(sqlInsLedger, conn, tx)
                                        cmdL.Parameters.Add("@item_id", MySqlDbType.Int32)
                                        cmdL.Parameters.Add("@lot_id", MySqlDbType.Int64) ' ※NULL入れるなら後で DBNull.Value に変更
                                        cmdL.Parameters.Add("@qty_delta", MySqlDbType.Int32)
                                        cmdL.Parameters.Add("@ref_id", MySqlDbType.Int64).Value = batchId
                                        cmdL.Parameters.Add("@line_no", MySqlDbType.Int32)
                                        cmdL.Parameters.Add("@save_seq", MySqlDbType.Int32).Value = saveSeq

                                        For Each prow In parentRows
                                            lineNo += 1

                                            Dim itemId As Integer = Convert.ToInt32(prow.Cells("item_id").Value)
                                            Dim requiredPieces As Integer = GetRequiredPiecesFromRow(prow, itemId)

                                            Dim q2 As Integer = 0
                                            Integer.TryParse(SafeCellStr(prow, "quantity2"), q2)

                                            Dim unitPrice As Integer = 0
                                            Integer.TryParse(SafeCellStr(prow, "unit_price"), unitPrice)

                                            Dim unit As String = SafeCellStr(prow, "unit").Replace(",", "，")
                                            Dim remark As String = SafeCellStr(prow, "remark").Replace(",", "，")

                                            Dim isLot As String = If(prow.Cells("is_lot_item").Value, "F").ToString().Trim().ToUpperInvariant()
                                            If isLot <> "T" Then isLot = "F"

                                            ' ---------- 親明細 INSERT ----------
                                            cmdD.Parameters("@line_no").Value = lineNo
                                            cmdD.Parameters("@item_id").Value = itemId
                                            cmdD.Parameters("@quantity").Value = requiredPieces
                                            cmdD.Parameters("@quantity2").Value = q2
                                            cmdD.Parameters("@unit_price").Value = unitPrice
                                            cmdD.Parameters("@unit").Value = unit
                                            cmdD.Parameters("@remark").Value = remark
                                            cmdD.ExecuteNonQuery()

                                            Dim detailId As Long
                                            Using cmdLast As New MySqlCommand(sqlGetId, conn, tx)
                                                detailId = Convert.ToInt64(cmdLast.ExecuteScalar())
                                            End Using

                                            ' ---------- 同時更新対策：item をロック ----------
                                            Using cmdLockItem As New MySqlCommand(
                                            "SELECT item_id FROM item_master WHERE item_id=@id FOR UPDATE", conn, tx)
                                                cmdLockItem.Parameters.AddWithValue("@id", itemId)
                                                cmdLockItem.ExecuteScalar()
                                            End Using

                                            If isLot = "T" Then
                                                ' ===== ロット管理：割当がある前提 =====
                                                Dim key As String = prow.Cells("row_key").Value.ToString()
                                                Dim allocs = LotAllocMap(key)

                                                ' lotロック
                                                Dim lotIds = allocs.Select(Function(x) x.LotId).Distinct().ToList()
                                                If lotIds.Count > 0 Then
                                                    Dim ps As New List(Of String)()
                                                    Using cmdLockLots As New MySqlCommand("", conn, tx)
                                                        For i = 0 To lotIds.Count - 1
                                                            Dim pn = "@p" & i
                                                            ps.Add(pn)
                                                            cmdLockLots.Parameters.AddWithValue(pn, lotIds(i))
                                                        Next
                                                        cmdLockLots.CommandText =
                                                        "SELECT lot_id FROM lot WHERE lot_id IN (" &
                                                        String.Join(",", ps) & ") FOR UPDATE"
                                                        cmdLockLots.ExecuteNonQuery()
                                                    End Using
                                                End If

                                                ' 割当 INSERT / lot減算 / 台帳（ロット単位）
                                                For Each al In allocs
                                                    If al.QtyPieces <= 0 Then Continue For

                                                    Dim unitIds As List(Of Long) = ReserveLotUnits(al.LotId, al.QtyPieces, conn, tx)
                                                    InsertUnitAlloc(detailId, unitIds, conn, tx)

                                                    cmdAlloc.Parameters("@detail_id").Value = detailId
                                                    cmdAlloc.Parameters("@lot_id").Value = al.LotId
                                                    cmdAlloc.Parameters("@qty").Value = al.QtyPieces
                                                    cmdAlloc.ExecuteNonQuery()

                                                    cmdLot.Parameters("@delta").Value = al.QtyPieces
                                                    cmdLot.Parameters("@lot_id").Value = al.LotId
                                                    Dim affectedLot = cmdLot.ExecuteNonQuery()
                                                    If affectedLot = 0 Then
                                                        Throw New Exception($"ロット在庫が不足しています（lot_id={al.LotId}, 要求={al.QtyPieces}個）")
                                                    End If

                                                    cmdL.Parameters("@item_id").Value = itemId
                                                    cmdL.Parameters("@lot_id").Value = al.LotId
                                                    cmdL.Parameters("@qty_delta").Value = -al.QtyPieces
                                                    cmdL.Parameters("@line_no").Value = lineNo
                                                    cmdL.ExecuteNonQuery()

                                                    issuedUnitIdsAll.AddRange(unitIds)
                                                Next

                                            Else
                                                ' ===== 非ロット：lotを触らず、台帳は lot_id=NULL で合計1行 =====
                                                cmdL.Parameters("@item_id").Value = itemId
                                                cmdL.Parameters("@lot_id").Value = DBNull.Value
                                                cmdL.Parameters("@qty_delta").Value = -requiredPieces
                                                cmdL.Parameters("@line_no").Value = lineNo
                                                cmdL.ExecuteNonQuery()
                                            End If

                                            ' ---------- item 減算（合計で1回） ----------
                                            cmdItem.Parameters("@delta").Value = requiredPieces
                                            cmdItem.Parameters("@item_id").Value = itemId
                                            Dim affectedItem = cmdItem.ExecuteNonQuery()
                                            If affectedItem = 0 Then
                                                Throw New Exception($"アイテム在庫が不足しています（item_id={itemId}, 要求={requiredPieces}個）")
                                            End If
                                        Next
                                    End Using
                                End Using
                            End Using
                        End Using
                    End Using

                    ' ★ ロット管理の unit を ISSUED に確定
                    ' ★ ロット管理の unit を ISSUED に確定
                    MarkUnitsIssued(issuedUnitIdsAll, conn, tx)

                    CreateGlForShipment_CogsOnly(
                        shipmentBatchId:=batchId,
                        shipmentDate:=shipmentDate,
                        customerCode:=customerCode,
                        saveSeq:=saveSeq,
                        parentRows:=parentRows,
                        conn:=conn,
                        tx:=tx
                    )

                    ' 必要なら売上も（外側に定義済みのメソッドを呼ぶだけ）
                    'CreateGlForShipment_Sales(
                    '    shipmentBatchId:=batchId,
                    '    shipmentDate:=shipmentDate,
                    '    customerCode:=customerCode,
                    '    saveSeq:=saveSeq,
                    '    parentRows:=parentRows,
                    '    conn:=conn,
                    '    tx:=tx
                    ')

                    tx.Commit()
                    MessageBox.Show($"保存しました。BatchID={batchId}", "OK",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information)
                Catch
                    Try : tx.Rollback() : Catch : End Try
                    Throw
                End Try
            End Using
        End Using
    End Sub



    ' =========================
    ' conversion_qty 編集ダイアログ
    ' =========================
    Private Class ConversionQtyDialog
        Inherits Form

        Public ReadOnly Property ConversionQty As Integer
            Get
                Dim v As Integer = 1
                Integer.TryParse(txtConv.Text.Trim(), v)
                If v <= 0 Then v = 1
                Return v
            End Get
        End Property

        Private lbl As Label
        Private txtConv As TextBox
        Private btnOk As Button
        Private btnCancel As Button

        Public Sub New(itemName As String, defaultConv As Integer)
            Me.Text = "単位換算（1箱あたり個数）"
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.StartPosition = FormStartPosition.CenterParent
            Me.ClientSize = New Size(420, 190)

            lbl = New Label() With {
                .AutoSize = False,
                .Text = $"[{itemName}]" & vbCrLf & "conversion_qty（1箱あたり個数）を確認/修正してください。",
                .Left = 16, .Top = 14, .Width = 388, .Height = 60
            }

            txtConv = New TextBox() With {
                .Left = 16, .Top = 84, .Width = 160
            }
            txtConv.Text = If(defaultConv <= 0, "1", defaultConv.ToString())

            btnOk = New Button() With {
                .Text = "OK",
                .Left = 220, .Top = 130, .Width = 90, .Height = 34
            }
            btnCancel = New Button() With {
                .Text = "キャンセル",
                .Left = 314, .Top = 130, .Width = 90, .Height = 34
            }

            AddHandler btnOk.Click, Sub()
                                        Dim v As Integer
                                        If Not Integer.TryParse(txtConv.Text.Trim(), v) OrElse v <= 0 Then
                                            MessageBox.Show("1以上の整数を入力してください。")
                                            txtConv.Focus()
                                            Return
                                        End If
                                        Me.DialogResult = DialogResult.OK
                                        Me.Close()
                                    End Sub

            AddHandler btnCancel.Click, Sub()
                                            Me.DialogResult = DialogResult.Cancel
                                            Me.Close()
                                        End Sub

            Me.Controls.Add(lbl)
            Me.Controls.Add(txtConv)
            Me.Controls.Add(btnOk)
            Me.Controls.Add(btnCancel)

            Me.AcceptButton = btnOk
            Me.CancelButton = btnCancel
        End Sub




    End Class

    Private Class DetailRow
        Public Property LineNo As Integer
        Public Property ItemId As Object   ' ★ DBNull.Value または Long
        Public Property Unit As String
        Public Property Quantity As Decimal
        Public Property Quantity2 As Decimal
        Public Property UnitPrice As Decimal
        Public Property Amount As Decimal
        Public Property Remark As String
    End Class


End Class
