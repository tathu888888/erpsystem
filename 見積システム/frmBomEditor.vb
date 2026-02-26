<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmBomEditor
    Inherits System.Windows.Forms.Form

    Private components As System.ComponentModel.IContainer

    Friend WithEvents dgvLines As DataGridView
    Friend WithEvents btnAddLine As Button
    Friend WithEvents btnSaveHeader As Button
    Friend WithEvents btnSaveLines As Button

    Friend WithEvents txtItemCode As TextBox
    Friend WithEvents txtItemName As TextBox
    Friend WithEvents txtBomId As TextBox
    Friend WithEvents txtBomCode As TextBox
    Friend WithEvents txtRevision As TextBox
    Friend WithEvents chkIsActive As CheckBox
    Friend WithEvents dtpFrom As DateTimePicker
    Friend WithEvents dtpTo As DateTimePicker
    Friend WithEvents txtNotes As TextBox

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.dgvLines = New DataGridView()
        Me.btnAddLine = New Button()
        Me.btnSaveHeader = New Button()
        Me.btnSaveLines = New Button()

        Me.txtItemCode = New TextBox()
        Me.txtItemName = New TextBox()
        Me.txtBomId = New TextBox()
        Me.txtBomCode = New TextBox()
        Me.txtRevision = New TextBox()
        Me.chkIsActive = New CheckBox()
        Me.dtpFrom = New DateTimePicker()
        Me.dtpTo = New DateTimePicker()
        Me.txtNotes = New TextBox()

        CType(Me.dgvLines, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()

        Me.txtItemCode.Location = New Point(12, 12)
        Me.txtItemCode.Size = New Size(140, 23)

        Me.txtItemName.Location = New Point(160, 12)
        Me.txtItemName.Size = New Size(260, 23)

        Me.txtBomId.Location = New Point(12, 45)
        Me.txtBomId.Size = New Size(90, 23)
        Me.txtBomId.ReadOnly = True

        Me.txtBomCode.Location = New Point(110, 45)
        Me.txtBomCode.Size = New Size(140, 23)

        Me.txtRevision.Location = New Point(260, 45)
        Me.txtRevision.Size = New Size(60, 23)

        Me.chkIsActive.Location = New Point(330, 47)
        Me.chkIsActive.AutoSize = True
        Me.chkIsActive.Text = "有効"

        Me.dtpFrom.Location = New Point(12, 78)
        Me.dtpFrom.Size = New Size(200, 23)
        Me.dtpFrom.ShowCheckBox = True

        Me.dtpTo.Location = New Point(220, 78)
        Me.dtpTo.Size = New Size(200, 23)
        Me.dtpTo.ShowCheckBox = True

        Me.txtNotes.Location = New Point(12, 110)
        Me.txtNotes.Size = New Size(408, 70)
        Me.txtNotes.Multiline = True

        Me.btnSaveHeader.Location = New Point(430, 12)
        Me.btnSaveHeader.Size = New Size(110, 28)
        Me.btnSaveHeader.Text = "ヘッダ保存"

        Me.btnAddLine.Location = New Point(430, 46)
        Me.btnAddLine.Size = New Size(110, 28)
        Me.btnAddLine.Text = "明細追加"

        Me.btnSaveLines.Location = New Point(430, 80)
        Me.btnSaveLines.Size = New Size(110, 28)
        Me.btnSaveLines.Text = "明細保存"

        Me.dgvLines.Location = New Point(12, 190)
        Me.dgvLines.Size = New Size(528, 240)
        Me.dgvLines.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right

        Me.AutoScaleMode = AutoScaleMode.Font
        Me.ClientSize = New Size(560, 450)
        Me.Text = "frmBomEditor"

        Me.Controls.Add(Me.txtItemCode)
        Me.Controls.Add(Me.txtItemName)
        Me.Controls.Add(Me.txtBomId)
        Me.Controls.Add(Me.txtBomCode)
        Me.Controls.Add(Me.txtRevision)
        Me.Controls.Add(Me.chkIsActive)
        Me.Controls.Add(Me.dtpFrom)
        Me.Controls.Add(Me.dtpTo)
        Me.Controls.Add(Me.txtNotes)

        Me.Controls.Add(Me.btnSaveHeader)
        Me.Controls.Add(Me.btnAddLine)
        Me.Controls.Add(Me.btnSaveLines)
        Me.Controls.Add(Me.dgvLines)

        CType(Me.dgvLines, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub
End Class