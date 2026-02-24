<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class menu
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
        Label1 = New Label()
        CustomerBtn = New Button()
        CustomerlistBtn = New Button()

        shipBtn = New Button()
        shutdownBtn = New Button()
        SuspendLayout()
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Font = New Font("Yu Gothic UI", 20.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(128))
        Label1.Location = New Point(280, 53)
        Label1.Name = "Label1"
        Label1.Size = New Size(226, 54)
        Label1.TabIndex = 0
        Label1.Text = "顧客管理システム"
        ' 
        ' hearingBtn
        ' 

        ' 
        shipBtn.Location = New Point(249, 164)
        shipBtn.Name = "transactionBtn"
        shipBtn.Size = New Size(257, 58)
        shipBtn.TabIndex = 2
        shipBtn.Text = "日次処理"
        shipBtn.UseVisualStyleBackColor = True


        CustomerBtn.Location = New Point(249, 264)
        CustomerBtn.Name = "CustomerBtn"
        CustomerBtn.Size = New Size(257, 58)
        CustomerBtn.TabIndex = 1
        CustomerBtn.Text = "顧客"
        CustomerBtn.UseVisualStyleBackColor = True
        ' 
        CustomerlistBtn.Location = New Point(249, 354)
        CustomerlistBtn.Name = "CustomerMastaBtn"
        CustomerlistBtn.Size = New Size(257, 58)
        CustomerlistBtn.TabIndex = 1
        CustomerlistBtn.Text = "顧客マスタ"
        CustomerlistBtn.UseVisualStyleBackColor = True
        ' 
        ' estimateBtn
        ' 
        shutdownBtn.Font = New Font("Yu Gothic UI", 18.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(128))
        shutdownBtn.Location = New Point(647, 53)
        shutdownBtn.Name = "shutdownBtn"
        shutdownBtn.Size = New Size(54, 54)
        shutdownBtn.TabIndex = 3
        shutdownBtn.Text = "×"
        shutdownBtn.UseVisualStyleBackColor = True
        ' 
        ' menu
        ' 
        AutoScaleDimensions = New SizeF(10.0F, 25.0F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1980, 1080)
        Controls.Add(shutdownBtn)
        Controls.Add(shipBtn)
        Controls.Add(CustomerBtn)
        Controls.Add(CustomerlistBtn)
        Controls.Add(Label1)
        Name = "menu"
        Text = "Form1"
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents CustomerBtn As Button
    Friend WithEvents CustomerlistBtn As Button

    Friend WithEvents shipBtn As Button
    Friend WithEvents shutdownBtn As Button
End Class
