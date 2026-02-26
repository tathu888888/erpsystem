<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class BomEditForm
    Inherits System.Windows.Forms.Form

    Private components As System.ComponentModel.IContainer

    Friend WithEvents cmbAssemblyItem As ComboBox
    Friend WithEvents txtBomId As TextBox
    Friend WithEvents txtBomCode As TextBox
    Friend WithEvents txtRevision As TextBox
    Friend WithEvents chkIsActive As CheckBox
    Friend WithEvents dtpFrom As DateTimePicker
    Friend WithEvents dtpTo As DateTimePicker
    Friend WithEvents txtNotes As TextBox

    Friend WithEvents dgvLine As DataGridView

    Friend WithEvents btnAddLine As Button
    Friend WithEvents btnRemoveLine As Button
    Friend WithEvents btnUp As Button
    Friend WithEvents btnDown As Button
    Friend WithEvents btnLoad As Button
    Friend WithEvents btnSave As Button
    Friend WithEvents btnDelete As Button
    Friend WithEvents btnNew As Button



    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.cmbAssemblyItem = New ComboBox()
        Me.txtBomId = New TextBox()
        Me.txtBomCode = New TextBox()
        Me.txtRevision = New TextBox()
        Me.chkIsActive = New CheckBox()
        Me.dtpFrom = New DateTimePicker()
        Me.dtpTo = New DateTimePicker()
        Me.txtNotes = New TextBox()

        Me.dgvLine = New DataGridView()

        Me.btnAddLine = New Button()
        Me.btnRemoveLine = New Button()
        Me.btnUp = New Button()
        Me.btnDown = New Button()
        Me.btnLoad = New Button()
        Me.btnSave = New Button()
        Me.btnDelete = New Button()
        Me.btnNew = New Button()

        CType(Me.dgvLine, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()

        Me.cmbAssemblyItem.Location = New Point(12, 12)
        Me.cmbAssemblyItem.Size = New Size(220, 23)
        Me.cmbAssemblyItem.DropDownStyle = ComboBoxStyle.DropDownList

        Me.txtBomId.Location = New Point(240, 12)
        Me.txtBomId.Size = New Size(80, 23)

        Me.btnLoad.Location = New Point(328, 12)
        Me.btnLoad.Size = New Size(70, 23)
        Me.btnLoad.Text = "読込"

        Me.txtBomCode.Location = New Point(12, 45)
        Me.txtBomCode.Size = New Size(120, 23)

        Me.txtRevision.Location = New Point(140, 45)
        Me.txtRevision.Size = New Size(60, 23)

        Me.chkIsActive.Location = New Point(210, 47)
        Me.chkIsActive.AutoSize = True
        Me.chkIsActive.Text = "有効"

        Me.dtpFrom.Location = New Point(12, 78)
        Me.dtpFrom.Size = New Size(180, 23)
        Me.dtpFrom.ShowCheckBox = True

        Me.dtpTo.Location = New Point(200, 78)
        Me.dtpTo.Size = New Size(180, 23)
        Me.dtpTo.ShowCheckBox = True

        Me.txtNotes.Location = New Point(12, 110)
        Me.txtNotes.Size = New Size(386, 60)
        Me.txtNotes.Multiline = True

        Me.btnNew.Location = New Point(410, 12)
        Me.btnNew.Size = New Size(70, 23)
        Me.btnNew.Text = "新規"

        Me.btnSave.Location = New Point(488, 12)
        Me.btnSave.Size = New Size(70, 23)
        Me.btnSave.Text = "保存"

        Me.btnDelete.Location = New Point(566, 12)
        Me.btnDelete.Size = New Size(70, 23)
        Me.btnDelete.Text = "削除"

        Me.btnAddLine.Location = New Point(410, 45)
        Me.btnAddLine.Size = New Size(70, 23)
        Me.btnAddLine.Text = "追加"

        Me.btnRemoveLine.Location = New Point(488, 45)
        Me.btnRemoveLine.Size = New Size(70, 23)
        Me.btnRemoveLine.Text = "削除"

        Me.btnUp.Location = New Point(566, 45)
        Me.btnUp.Size = New Size(70, 23)
        Me.btnUp.Text = "上へ"

        Me.btnDown.Location = New Point(644, 45)
        Me.btnDown.Size = New Size(70, 23)
        Me.btnDown.Text = "下へ"

        Me.dgvLine.Location = New Point(12, 180)
        Me.dgvLine.Size = New Size(700, 260)
        Me.dgvLine.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right

        Me.ClientSize = New Size(730, 460)
        Me.Text = "BomEditForm"

        Me.Controls.Add(Me.cmbAssemblyItem)
        Me.Controls.Add(Me.txtBomId)
        Me.Controls.Add(Me.btnLoad)
        Me.Controls.Add(Me.txtBomCode)
        Me.Controls.Add(Me.txtRevision)
        Me.Controls.Add(Me.chkIsActive)
        Me.Controls.Add(Me.dtpFrom)
        Me.Controls.Add(Me.dtpTo)
        Me.Controls.Add(Me.txtNotes)

        Me.Controls.Add(Me.btnNew)
        Me.Controls.Add(Me.btnSave)
        Me.Controls.Add(Me.btnDelete)

        Me.Controls.Add(Me.btnAddLine)
        Me.Controls.Add(Me.btnRemoveLine)
        Me.Controls.Add(Me.btnUp)
        Me.Controls.Add(Me.btnDown)

        Me.Controls.Add(Me.dgvLine)

        CType(Me.dgvLine, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub
End Class