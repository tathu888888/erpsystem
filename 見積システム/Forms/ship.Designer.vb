<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ship
    Inherits System.Windows.Forms.Form

    Private components As System.ComponentModel.IContainer

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

    ' ====== Controls ======
    Friend WithEvents shutdownBtn As Button
    Friend WithEvents backBtn As Button
    Friend WithEvents saveBtn As Button
    'Friend WithEvents bottomButton As Button

    ' ★ここを分ける（上書きバグ防止）
    Friend WithEvents listBtn As Button           ' リスト表示
    Friend WithEvents customerBtn As Button       ' 顧客

    Friend WithEvents LabelTitle As Label
    Friend WithEvents estimateDataGridView As DataGridView

    Friend WithEvents lblShipmentDate As Label
    Friend WithEvents dtpShipmentDate As DateTimePicker

    Friend WithEvents lblCustomerCode As Label
    Friend WithEvents txtCustomerCode As TextBox

    Friend WithEvents lblCustomerName As Label
    Friend WithEvents txtCustomerName As TextBox

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()

        ' ========= Form =========
        Me.SuspendLayout()
        Me.FormBorderStyle = FormBorderStyle.None
        Me.WindowState = FormWindowState.Maximized
        Me.TopMost = True
        Me.AutoScaleDimensions = New SizeF(10.0F, 25.0F)
        Me.AutoScaleMode = AutoScaleMode.Font
        Me.ClientSize = New Size(1980, 1080)
        Me.Name = "ship"
        Me.Text = "Shipments"

        ' ========= Controls =========
        shutdownBtn = New Button()
        backBtn = New Button()
        saveBtn = New Button()
        'bottomButton = New Button()

        listBtn = New Button()
        customerBtn = New Button()
        LabelTitle = New Label()
        estimateDataGridView = New DataGridView()

        lblShipmentDate = New Label()
        dtpShipmentDate = New DateTimePicker()

        lblCustomerCode = New Label()
        txtCustomerCode = New TextBox()

        lblCustomerName = New Label()
        txtCustomerName = New TextBox()

        ' ========= Title =========
        LabelTitle.AutoSize = True
        LabelTitle.Font = New Font("Yu Gothic UI", 26.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(128))
        LabelTitle.Text = "サンスター 「緑のサラナ」 顧客管理システム"
        LabelTitle.Location = New Point(60, 40)

        ' ========= shutdown =========
        shutdownBtn.Font = New Font("Yu Gothic UI", 18.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(128))
        shutdownBtn.Size = New Size(48, 48)
        shutdownBtn.Text = "×"
        ' ★ClientSize確定後に置くのでズレにくい（右上）
        shutdownBtn.Location = New Point(Me.ClientSize.Width - 70, 20)
        shutdownBtn.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        shutdownBtn.CausesValidation = False

        ' ========= back =========
        backBtn.Font = New Font("Yu Gothic UI", 12.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(128))
        backBtn.Size = New Size(140, 45)
        backBtn.Text = "戻る"
        backBtn.Location = New Point(20, 900)
        backBtn.Anchor = AnchorStyles.Left Or AnchorStyles.Bottom
        backBtn.CausesValidation = False

        ' ========= save =========
        saveBtn.Font = New Font("Yu Gothic UI", 12.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(128))
        saveBtn.Size = New Size(160, 45)
        saveBtn.Text = "保存"
        saveBtn.Location = New Point(180, 900)
        saveBtn.Anchor = AnchorStyles.Left Or AnchorStyles.Bottom
        saveBtn.CausesValidation = False

        ' ========= add row =========
        'bottomButton.Font = New Font("Yu Gothic UI", 12.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(128))
        'bottomButton.Size = New Size(160, 45)
        'bottomButton.Text = "行追加"
        'bottomButton.Location = New Point(360, 900)
        'bottomButton.Anchor = AnchorStyles.Left Or AnchorStyles.Bottom
        'bottomButton.CausesValidation = False

        ' ========= list =========
        listBtn.Font = New Font("Yu Gothic UI", 12.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(128))
        listBtn.Size = New Size(160, 45)
        listBtn.Text = "リスト表示"
        listBtn.Location = New Point(560, 900)
        listBtn.Anchor = AnchorStyles.Left Or AnchorStyles.Bottom
        listBtn.CausesValidation = False

        ' ========= customer =========
        customerBtn.Font = New Font("Yu Gothic UI", 12.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(128))
        customerBtn.Size = New Size(160, 45)
        customerBtn.Text = "顧客"
        customerBtn.Location = New Point(720, 900)
        customerBtn.Anchor = AnchorStyles.Left Or AnchorStyles.Bottom
        customerBtn.CausesValidation = False

        ' ========= subscription =========

        ' ========= Layout base =========
        Dim leftLabelX As Integer = 60
        Dim leftInputX As Integer = 260
        Dim y As Integer = 120
        Dim gapY As Integer = 55
        Dim labelW As Integer = 180
        Dim inputW As Integer = 320 ' ※使ってないけど残してOK

        Dim labelFont As New Font("Yu Gothic UI", 13.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(128))
        Dim inputFont As New Font("Yu Gothic UI", 13.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(128))

        ' ========= shipment_date =========
        lblShipmentDate.AutoSize = False
        lblShipmentDate.Font = labelFont
        lblShipmentDate.TextAlign = ContentAlignment.MiddleLeft
        lblShipmentDate.Text = "発送年月日"
        lblShipmentDate.Location = New Point(leftLabelX, y)
        lblShipmentDate.Size = New Size(labelW, 35)

        dtpShipmentDate.Font = inputFont
        dtpShipmentDate.Location = New Point(leftInputX, y)
        dtpShipmentDate.Size = New Size(240, 35)
        dtpShipmentDate.Format = DateTimePickerFormat.Custom
        dtpShipmentDate.CustomFormat = "yyyy年MM月dd日"
        dtpShipmentDate.ShowUpDown = True


        ' ========= customer_code =========
        y += gapY
        lblCustomerCode.AutoSize = False
        lblCustomerCode.Font = labelFont
        lblCustomerCode.TextAlign = ContentAlignment.MiddleLeft
        lblCustomerCode.Text = "顧客コード(c6)"
        lblCustomerCode.Location = New Point(leftLabelX, y)
        lblCustomerCode.Size = New Size(labelW, 35)

        txtCustomerCode.Font = inputFont
        txtCustomerCode.Location = New Point(leftInputX, y)
        txtCustomerCode.Size = New Size(240, 35)

        ' ========= customer_name =========
        y += gapY
        lblCustomerName.AutoSize = False
        lblCustomerName.Font = labelFont
        lblCustomerName.TextAlign = ContentAlignment.MiddleLeft
        lblCustomerName.Text = "顧客名(c16)"
        lblCustomerName.Location = New Point(leftLabelX, y)
        lblCustomerName.Size = New Size(labelW, 35)

        txtCustomerName.Font = inputFont
        txtCustomerName.Location = New Point(leftInputX, y)
        txtCustomerName.Size = New Size(520, 35)
        txtCustomerName.ReadOnly = True

        ' ========= DataGridView =========
        estimateDataGridView.Location = New Point(60, y + 70)
        estimateDataGridView.Size = New Size(900, 600)
        estimateDataGridView.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Bottom

        estimateDataGridView.AllowUserToAddRows = False
        estimateDataGridView.AllowUserToDeleteRows = True
        estimateDataGridView.RowHeadersVisible = False
        estimateDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        estimateDataGridView.MultiSelect = False
        estimateDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill

        estimateDataGridView.Font = New Font("Yu Gothic UI", 12.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(128))
        estimateDataGridView.ColumnHeadersDefaultCellStyle.Font = New Font("Yu Gothic UI", 12.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(128))

        ' --- Columns（保存コードが参照するName） ---
        Dim colQty As New DataGridViewTextBoxColumn()
        colQty.Name = "quantity"
        colQty.HeaderText = "数量"
        colQty.ValueType = GetType(Integer)

        Dim colUnitPrice As New DataGridViewTextBoxColumn()
        colUnitPrice.Name = "unit_price"
        colUnitPrice.HeaderText = "単価"
        colUnitPrice.ValueType = GetType(Integer)

        Dim colAmount As New DataGridViewTextBoxColumn()
        colAmount.Name = "amount"
        colAmount.HeaderText = "金額"
        colAmount.ValueType = GetType(Integer)

        Dim colRemark As New DataGridViewTextBoxColumn()
        colRemark.Name = "remark"
        colRemark.HeaderText = "備考"
        colRemark.ValueType = GetType(String)

        estimateDataGridView.Columns.AddRange(
            New DataGridViewColumn() {colQty, colUnitPrice, colAmount, colRemark}
        )

        For i As Integer = 1 To 5
            estimateDataGridView.Rows.Add("", "", "", "")
        Next

        ' ========= Add Controls =========
        Me.Controls.Add(LabelTitle)
        Me.Controls.Add(shutdownBtn)

        Me.Controls.Add(lblShipmentDate)
        Me.Controls.Add(dtpShipmentDate)

        Me.Controls.Add(lblCustomerCode)
        Me.Controls.Add(txtCustomerCode)

        Me.Controls.Add(lblCustomerName)
        Me.Controls.Add(txtCustomerName)

        Me.Controls.Add(estimateDataGridView)

        Me.Controls.Add(backBtn)
        Me.Controls.Add(saveBtn)
        'Me.Controls.Add(bottomButton)

        ' ★3つ全部追加
        Me.Controls.Add(listBtn)
        Me.Controls.Add(customerBtn)

        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

End Class
