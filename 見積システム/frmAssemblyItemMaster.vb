<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmAssemblyItemMaster
    Inherits System.Windows.Forms.Form

    Private components As System.ComponentModel.IContainer

    Friend WithEvents txtSearch As TextBox
    Friend WithEvents btnSearch As Button
    Friend WithEvents chkActiveOnly As CheckBox
    Friend WithEvents chkAssemblyOnly As CheckBox
    Friend WithEvents dgvList As DataGridView

    Friend WithEvents txtItemCode As TextBox
    Friend WithEvents txtItemName As TextBox
    Friend WithEvents chkIsActive As CheckBox
    Friend WithEvents chkIsAssembly As CheckBox

    Friend WithEvents cmbDefaultBom As ComboBox
    Friend WithEvents cmbDefaultWarehouse As ComboBox
    Friend WithEvents numStdBuildQty As NumericUpDown
    Friend WithEvents cmbLotPolicy As ComboBox
    Friend WithEvents txtNotes As TextBox

    Friend WithEvents btnNew As Button
    Friend WithEvents btnSave As Button

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

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.txtSearch = New TextBox()
        Me.btnSearch = New Button()
        Me.chkActiveOnly = New CheckBox()
        Me.chkAssemblyOnly = New CheckBox()
        Me.dgvList = New DataGridView()

        Me.txtItemCode = New TextBox()
        Me.txtItemName = New TextBox()
        Me.chkIsActive = New CheckBox()
        Me.chkIsAssembly = New CheckBox()

        Me.cmbDefaultBom = New ComboBox()
        Me.cmbDefaultWarehouse = New ComboBox()
        Me.numStdBuildQty = New NumericUpDown()
        Me.cmbLotPolicy = New ComboBox()
        Me.txtNotes = New TextBox()

        Me.btnNew = New Button()
        Me.btnSave = New Button()

        CType(Me.dgvList, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numStdBuildQty, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()

        ' txtSearch
        Me.txtSearch.Location = New Point(12, 12)
        Me.txtSearch.Size = New Size(240, 23)

        ' btnSearch
        Me.btnSearch.Location = New Point(260, 12)
        Me.btnSearch.Size = New Size(90, 23)
        Me.btnSearch.Text = "検索"

        ' chkActiveOnly
        Me.chkActiveOnly.Location = New Point(360, 12)
        Me.chkActiveOnly.AutoSize = True
        Me.chkActiveOnly.Text = "有効のみ"

        ' chkAssemblyOnly
        Me.chkAssemblyOnly.Location = New Point(460, 12)
        Me.chkAssemblyOnly.AutoSize = True
        Me.chkAssemblyOnly.Text = "アセンブリのみ"

        ' dgvList
        Me.dgvList.Location = New Point(12, 45)
        Me.dgvList.Size = New Size(560, 380)
        Me.dgvList.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left

        ' Editor controls (右側)
        Dim xR As Integer = 590
        Dim wR As Integer = 260

        Me.txtItemCode.Location = New Point(xR, 45)
        Me.txtItemCode.Size = New Size(wR, 23)

        Me.txtItemName.Location = New Point(xR, 75)
        Me.txtItemName.Size = New Size(wR, 23)

        Me.chkIsActive.Location = New Point(xR, 105)
        Me.chkIsActive.AutoSize = True
        Me.chkIsActive.Text = "有効"

        Me.chkIsAssembly.Location = New Point(xR + 80, 105)
        Me.chkIsAssembly.AutoSize = True
        Me.chkIsAssembly.Text = "アセンブリ"

        Me.cmbDefaultBom.Location = New Point(xR, 135)
        Me.cmbDefaultBom.Size = New Size(wR, 23)
        Me.cmbDefaultBom.DropDownStyle = ComboBoxStyle.DropDownList

        Me.cmbDefaultWarehouse.Location = New Point(xR, 165)
        Me.cmbDefaultWarehouse.Size = New Size(wR, 23)
        Me.cmbDefaultWarehouse.DropDownStyle = ComboBoxStyle.DropDownList

        Me.numStdBuildQty.Location = New Point(xR, 195)
        Me.numStdBuildQty.Size = New Size(120, 23)
        Me.numStdBuildQty.DecimalPlaces = 2
        Me.numStdBuildQty.Minimum = 0
        Me.numStdBuildQty.Maximum = 999999

        Me.cmbLotPolicy.Location = New Point(xR, 225)
        Me.cmbLotPolicy.Size = New Size(160, 23)
        Me.cmbLotPolicy.DropDownStyle = ComboBoxStyle.DropDownList

        Me.txtNotes.Location = New Point(xR, 255)
        Me.txtNotes.Size = New Size(wR, 120)
        Me.txtNotes.Multiline = True

        Me.btnNew.Location = New Point(xR, 390)
        Me.btnNew.Size = New Size(90, 28)
        Me.btnNew.Text = "新規"

        Me.btnSave.Location = New Point(xR + 100, 390)
        Me.btnSave.Size = New Size(90, 28)
        Me.btnSave.Text = "保存"

        ' Form
        Me.AutoScaleMode = AutoScaleMode.Font
        Me.ClientSize = New Size(870, 440)
        Me.Text = "frmAssemblyItemMaster"

        Me.Controls.Add(Me.txtSearch)
        Me.Controls.Add(Me.btnSearch)
        Me.Controls.Add(Me.chkActiveOnly)
        Me.Controls.Add(Me.chkAssemblyOnly)
        Me.Controls.Add(Me.dgvList)

        Me.Controls.Add(Me.txtItemCode)
        Me.Controls.Add(Me.txtItemName)
        Me.Controls.Add(Me.chkIsActive)
        Me.Controls.Add(Me.chkIsAssembly)
        Me.Controls.Add(Me.cmbDefaultBom)
        Me.Controls.Add(Me.cmbDefaultWarehouse)
        Me.Controls.Add(Me.numStdBuildQty)
        Me.Controls.Add(Me.cmbLotPolicy)
        Me.Controls.Add(Me.txtNotes)
        Me.Controls.Add(Me.btnNew)
        Me.Controls.Add(Me.btnSave)

        CType(Me.dgvList, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numStdBuildQty, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub
End Class