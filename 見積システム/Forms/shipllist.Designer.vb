

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class shipllist
    Inherits System.Windows.Forms.Form

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

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        'Me.listViewEstimates = New System.Windows.Forms.ListView()
        'Me.cmbMonth = New System.Windows.Forms.ComboBox()
        'Me.cmbPref = New System.Windows.Forms.ComboBox()
        'Me.rbMonth = New System.Windows.Forms.RadioButton()
        'Me.rbPref = New System.Windows.Forms.RadioButton()
        'Me.rbMonthPref = New System.Windows.Forms.RadioButton()
        'Me.btnPrev = New System.Windows.Forms.Button()
        'Me.btnNext = New System.Windows.Forms.Button()
        'Me.lblPage = New System.Windows.Forms.Label()
        'Me.SuspendLayout()
        ''
        '' cmbMonth
        ''
        'Me.cmbMonth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        'Me.cmbMonth.FormattingEnabled = True
        'Me.cmbMonth.Location = New System.Drawing.Point(12, 12)
        'Me.cmbMonth.Name = "cmbMonth"
        'Me.cmbMonth.Size = New System.Drawing.Size(140, 33)
        'Me.cmbMonth.TabIndex = 0
        ''
        '' cmbPref
        ''
        'Me.cmbPref.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        'Me.cmbPref.FormattingEnabled = True
        'Me.cmbPref.Location = New System.Drawing.Point(165, 12)
        'Me.cmbPref.Name = "cmbPref"
        'Me.cmbPref.Size = New System.Drawing.Size(160, 33)
        'Me.cmbPref.TabIndex = 1
        ''
        '' rbMonth
        ''
        'Me.rbMonth.AutoSize = True
        'Me.rbMonth.Location = New System.Drawing.Point(340, 15)
        'Me.rbMonth.Name = "rbMonth"
        'Me.rbMonth.Size = New System.Drawing.Size(93, 29)
        'Me.rbMonth.TabIndex = 2
        'Me.rbMonth.TabStop = True
        'Me.rbMonth.Text = "月別表示"
        'Me.rbMonth.UseVisualStyleBackColor = True
        ''
        '' rbPref
        ''
        'Me.rbPref.AutoSize = True
        'Me.rbPref.Location = New System.Drawing.Point(440, 15)
        'Me.rbPref.Name = "rbPref"
        'Me.rbPref.Size = New System.Drawing.Size(141, 29)
        'Me.rbPref.TabIndex = 3
        'Me.rbPref.TabStop = True
        'Me.rbPref.Text = "都道府県別表示"
        'Me.rbPref.UseVisualStyleBackColor = True
        ''
        '' rbMonthPref
        ''
        'Me.rbMonthPref.AutoSize = True
        'Me.rbMonthPref.Location = New System.Drawing.Point(590, 15)
        'Me.rbMonthPref.Name = "rbMonthPref"
        'Me.rbMonthPref.Size = New System.Drawing.Size(196, 29)
        'Me.rbMonthPref.TabIndex = 4
        'Me.rbMonthPref.TabStop = True
        'Me.rbMonthPref.Text = "月別＋都道府県別表示"
        'Me.rbMonthPref.UseVisualStyleBackColor = True
        ''
        '' listViewEstimates
        ''
        'Me.listViewEstimates.HideSelection = False
        'Me.listViewEstimates.Location = New System.Drawing.Point(12, 55)
        'Me.listViewEstimates.Name = "listViewEstimates"
        'Me.listViewEstimates.Size = New System.Drawing.Size(776, 355)
        'Me.listViewEstimates.TabIndex = 5
        'Me.listViewEstimates.UseCompatibleStateImageBehavior = False

        'Me.listViewEstimates.Anchor = CType(
        '    (System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom Or
        '     System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right),
        '    System.Windows.Forms.AnchorStyles)

        ''
        '' btnPrev
        ''
        'Me.btnPrev.Location = New System.Drawing.Point(12, 415)
        'Me.btnPrev.Name = "btnPrev"
        'Me.btnPrev.Size = New System.Drawing.Size(120, 35)
        'Me.btnPrev.TabIndex = 6
        'Me.btnPrev.Text = "前ページ"
        'Me.btnPrev.UseVisualStyleBackColor = True
        ''
        '' btnNext
        ''
        'Me.btnNext.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        'Me.btnNext.Location = New System.Drawing.Point(668, 415)
        'Me.btnNext.Name = "btnNext"
        'Me.btnNext.Size = New System.Drawing.Size(120, 35)
        'Me.btnNext.TabIndex = 8
        'Me.btnNext.Text = "次ページ"
        'Me.btnNext.UseVisualStyleBackColor = True
        ''
        '' lblPage
        ''
        'Me.lblPage.AutoSize = True
        'Me.lblPage.Location = New System.Drawing.Point(150, 421)
        'Me.lblPage.Name = "lblPage"
        'Me.lblPage.Size = New System.Drawing.Size(87, 25)
        'Me.lblPage.TabIndex = 7
        'Me.lblPage.Text = "1/1（0件）"
        ''
        ' estimatellist
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(10.0!, 25.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 460)
        'Me.Controls.Add(Me.lblPage)
        'Me.Controls.Add(Me.btnNext)
        'Me.Controls.Add(Me.btnPrev)
        'Me.Controls.Add(Me.listViewEstimates)
        'Me.Controls.Add(Me.rbMonthPref)
        'Me.Controls.Add(Me.rbPref)
        'Me.Controls.Add(Me.rbMonth)
        'Me.Controls.Add(Me.cmbPref)
        'Me.Controls.Add(Me.cmbMonth)
        'Me.Name = "shiptlist"
        Me.Text = "shiplist"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    'Friend WithEvents listViewEstimates As System.Windows.Forms.ListView
    'Friend WithEvents cmbMonth As System.Windows.Forms.ComboBox
    'Friend WithEvents cmbPref As System.Windows.Forms.ComboBox
    'Friend WithEvents rbMonth As System.Windows.Forms.RadioButton
    'Friend WithEvents rbPref As System.Windows.Forms.RadioButton
    'Friend WithEvents rbMonthPref As System.Windows.Forms.RadioButton
    'Friend WithEvents btnPrev As System.Windows.Forms.Button
    'Friend WithEvents btnNext As System.Windows.Forms.Button
    'Friend WithEvents lblPage As System.Windows.Forms.Label

End Class
