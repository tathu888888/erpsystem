<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Customer
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

    'メモ: 以下のプロシージャは Windows フォーム デザイナーで必要です。
    'Windows フォーム デザイナーを使用して変更できます。
    'コード エディターを使って変更しないでください。
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.LabelTitle = New System.Windows.Forms.Label()
        Me.lblCustomerCode = New System.Windows.Forms.Label()
        Me.txtCustomerCode = New System.Windows.Forms.TextBox()
        Me.lblCustomerName = New System.Windows.Forms.Label()
        Me.txtCustomerName = New System.Windows.Forms.TextBox()
        Me.lblPhoneNumber = New System.Windows.Forms.Label()
        Me.txtPhoneNumber = New System.Windows.Forms.TextBox()
        Me.lblAddress = New System.Windows.Forms.Label()
        Me.txtAddress = New System.Windows.Forms.TextBox()
        Me.lblApartmentName = New System.Windows.Forms.Label()
        Me.txtApartmentName = New System.Windows.Forms.TextBox()
        Me.lblPaymentMethod = New System.Windows.Forms.Label()
        Me.cmbPaymentMethod = New System.Windows.Forms.ComboBox()
        Me.lblBirthDate = New System.Windows.Forms.Label()
        Me.dtpBirthDate = New System.Windows.Forms.DateTimePicker()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.btnClear = New System.Windows.Forms.Button()
        Me.btnBack = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        ' LabelTitle
        '
        Me.LabelTitle.AutoSize = True
        Me.LabelTitle.Font = New System.Drawing.Font("MS UI Gothic", 18.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(128, Byte))
        Me.LabelTitle.Location = New System.Drawing.Point(22, 18)
        Me.LabelTitle.Name = "LabelTitle"
        Me.LabelTitle.Size = New System.Drawing.Size(158, 24)
        Me.LabelTitle.TabIndex = 0
        Me.LabelTitle.Text = "顧客 登録画面"
        '
        ' lblCustomerCode
        '
        ' ---- 顧客コード ----
        Me.lblCustomerCode.AutoSize = True
        Me.lblCustomerCode.Location = New System.Drawing.Point(40, 32)
        Me.lblCustomerCode.Name = "lblCustomerCode"
        Me.lblCustomerCode.Size = New System.Drawing.Size(67, 12)
        Me.lblCustomerCode.TabIndex = 1
        Me.lblCustomerCode.Text = "顧客コード"

        Me.txtCustomerCode.Location = New System.Drawing.Point(170, 26) ' y-6
        Me.txtCustomerCode.Name = "txtCustomerCode"
        Me.txtCustomerCode.Size = New System.Drawing.Size(260, 36)
        Me.txtCustomerCode.TabIndex = 2

        ' ---- 顧客名 ----
        Me.lblCustomerName.AutoSize = True
        Me.lblCustomerName.Location = New System.Drawing.Point(40, 84)
        Me.lblCustomerName.Name = "lblCustomerName"
        Me.lblCustomerName.Size = New System.Drawing.Size(53, 12)
        Me.lblCustomerName.TabIndex = 3
        Me.lblCustomerName.Text = "顧客名"

        Me.txtCustomerName.Location = New System.Drawing.Point(170, 78)
        Me.txtCustomerName.Name = "txtCustomerName"
        Me.txtCustomerName.Size = New System.Drawing.Size(340, 36)
        Me.txtCustomerName.TabIndex = 4

        ' ---- 電話番号 ----
        Me.lblPhoneNumber.AutoSize = True
        Me.lblPhoneNumber.Location = New System.Drawing.Point(40, 136)
        Me.lblPhoneNumber.Name = "lblPhoneNumber"
        Me.lblPhoneNumber.Size = New System.Drawing.Size(53, 12)
        Me.lblPhoneNumber.TabIndex = 5
        Me.lblPhoneNumber.Text = "電話番号"

        Me.txtPhoneNumber.Location = New System.Drawing.Point(170, 130)
        Me.txtPhoneNumber.Name = "txtPhoneNumber"
        Me.txtPhoneNumber.Size = New System.Drawing.Size(260, 36)
        Me.txtPhoneNumber.TabIndex = 6

        ' ---- 住所 ----
        Me.lblAddress.AutoSize = True
        Me.lblAddress.Location = New System.Drawing.Point(40, 188)
        Me.lblAddress.Name = "lblAddress"
        Me.lblAddress.Size = New System.Drawing.Size(29, 12)
        Me.lblAddress.TabIndex = 7
        Me.lblAddress.Text = "住所"

        Me.txtAddress.Location = New System.Drawing.Point(170, 182)
        Me.txtAddress.Name = "txtAddress"
        Me.txtAddress.Size = New System.Drawing.Size(470, 36)
        Me.txtAddress.TabIndex = 8

        ' ---- 建物名 ----
        Me.lblApartmentName.AutoSize = True
        Me.lblApartmentName.Location = New System.Drawing.Point(40, 240)
        Me.lblApartmentName.Name = "lblApartmentName"
        Me.lblApartmentName.Size = New System.Drawing.Size(53, 12)
        Me.lblApartmentName.TabIndex = 9
        Me.lblApartmentName.Text = "建物名等"

        Me.txtApartmentName.Location = New System.Drawing.Point(170, 234)
        Me.txtApartmentName.Name = "txtApartmentName"
        Me.txtApartmentName.Size = New System.Drawing.Size(470, 36)
        Me.txtApartmentName.TabIndex = 10

        ' ---- 支払方法 ----
        Me.lblPaymentMethod.AutoSize = True
        Me.lblPaymentMethod.Location = New System.Drawing.Point(40, 292)
        Me.lblPaymentMethod.Name = "lblPaymentMethod"
        Me.lblPaymentMethod.Size = New System.Drawing.Size(53, 12)
        Me.lblPaymentMethod.TabIndex = 11
        Me.lblPaymentMethod.Text = "支払方法"

        Me.cmbPaymentMethod.Location = New System.Drawing.Point(170, 286)
        Me.cmbPaymentMethod.Name = "cmbPaymentMethod"
        Me.cmbPaymentMethod.Size = New System.Drawing.Size(260, 36)
        Me.cmbPaymentMethod.TabIndex = 12
        Me.cmbPaymentMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList

        ' ---- 生年月日 ----
        Me.lblBirthDate.AutoSize = True
        Me.lblBirthDate.Location = New System.Drawing.Point(40, 344)
        Me.lblBirthDate.Name = "lblBirthDate"
        Me.lblBirthDate.Size = New System.Drawing.Size(53, 12)
        Me.lblBirthDate.TabIndex = 13
        Me.lblBirthDate.Text = "生年月日"

        Me.dtpBirthDate.Location = New System.Drawing.Point(170, 338)
        Me.dtpBirthDate.Name = "dtpBirthDate"
        Me.dtpBirthDate.Size = New System.Drawing.Size(260, 36)
        Me.dtpBirthDate.TabIndex = 14
        Me.dtpBirthDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom
        Me.dtpBirthDate.CustomFormat = "yyyy年MM月dd日"


        '
        ' btnSave
        '
        Me.btnSave.Location = New System.Drawing.Point(140, 325)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(120, 40)
        Me.btnSave.TabIndex = 15
        Me.btnSave.Text = "登録"
        Me.btnSave.UseVisualStyleBackColor = True
        '
        ' btnClear
        '
        Me.btnClear.Location = New System.Drawing.Point(270, 325)
        Me.btnClear.Name = "btnClear"
        Me.btnClear.Size = New System.Drawing.Size(120, 40)
        Me.btnClear.TabIndex = 16
        Me.btnClear.Text = "クリア"
        Me.btnClear.UseVisualStyleBackColor = True
        '
        ' btnBack
        '
        Me.btnBack.Location = New System.Drawing.Point(400, 325)
        Me.btnBack.Name = "btnBack"
        Me.btnBack.Size = New System.Drawing.Size(160, 40)
        Me.btnBack.TabIndex = 17
        Me.btnBack.Text = "戻る"
        Me.btnBack.UseVisualStyleBackColor = True
        '
        ' customer_register
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1980, 1080)
        Me.Controls.Add(Me.btnBack)
        Me.Controls.Add(Me.btnClear)
        Me.Controls.Add(Me.btnSave)
        Me.Controls.Add(Me.dtpBirthDate)
        Me.Controls.Add(Me.lblBirthDate)
        Me.Controls.Add(Me.cmbPaymentMethod)
        Me.Controls.Add(Me.lblPaymentMethod)
        Me.Controls.Add(Me.txtApartmentName)
        Me.Controls.Add(Me.lblApartmentName)
        Me.Controls.Add(Me.txtAddress)
        Me.Controls.Add(Me.lblAddress)
        Me.Controls.Add(Me.txtPhoneNumber)
        Me.Controls.Add(Me.lblPhoneNumber)
        Me.Controls.Add(Me.txtCustomerName)
        Me.Controls.Add(Me.lblCustomerName)
        Me.Controls.Add(Me.txtCustomerCode)
        Me.Controls.Add(Me.lblCustomerCode)
        Me.Controls.Add(Me.LabelTitle)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.Name = "customer_register"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "顧客登録"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub




    Friend WithEvents LabelTitle As Label

    Friend WithEvents lblCustomerCode As Label
    Friend WithEvents txtCustomerCode As TextBox

    Friend WithEvents lblCustomerName As Label
    Friend WithEvents txtCustomerName As TextBox

    Friend WithEvents lblPhoneNumber As Label
    Friend WithEvents txtPhoneNumber As TextBox

    Friend WithEvents lblAddress As Label
    Friend WithEvents txtAddress As TextBox

    Friend WithEvents lblApartmentName As Label
    Friend WithEvents txtApartmentName As TextBox

    Friend WithEvents lblPaymentMethod As Label
    Friend WithEvents cmbPaymentMethod As ComboBox

    Friend WithEvents lblBirthDate As Label
    Friend WithEvents dtpBirthDate As DateTimePicker

    Friend WithEvents btnSave As Button
    Friend WithEvents btnClear As Button
    Friend WithEvents btnBack As Button

End Class
