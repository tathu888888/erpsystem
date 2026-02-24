<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Customerlist
    Inherits System.Windows.Forms.Form

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
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
        Me.lvCustomers = New System.Windows.Forms.ListView()
        Me.SuspendLayout()
        '
        ' lvCustomers
        '
        Me.lvCustomers.Location = New System.Drawing.Point(12, 12)
        Me.lvCustomers.Name = "lvCustomers"
        Me.lvCustomers.Size = New System.Drawing.Size(1180, 520)
        Me.lvCustomers.TabIndex = 0
        Me.lvCustomers.UseCompatibleStateImageBehavior = False
        '
        ' Customerlist
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1200, 550)
        Me.Controls.Add(Me.lvCustomers)
        Me.Name = "Customerlist"
        Me.Text = "顧客一覧"
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents lvCustomers As ListView
End Class
