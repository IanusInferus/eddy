<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormMain
    Inherits System.Windows.Forms.Form

    'Form 重写 Dispose，以清理组件列表。
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows 窗体设计器所必需的
    Private components As System.ComponentModel.IContainer

    '注意: 以下过程是 Windows 窗体设计器所必需的
    '可以使用 Windows 窗体设计器修改它。
    '不要使用代码编辑器修改它。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormMain))
        Me.SplitContainer_Main = New System.Windows.Forms.SplitContainer()
        Me.DataGridView_Multiview = New System.Windows.Forms.DataGridView()
        Me.Panel_Work = New System.Windows.Forms.Panel()
        Me.NumericUpDown_TextNumber = New System.Windows.Forms.NumericUpDown()
        Me.ToolStrip_Tools = New System.Windows.Forms.ToolStrip()
        Me.Label_TextName = New System.Windows.Forms.Label()
        Me.Label_LOCBoxTip = New System.Windows.Forms.Label()
        Me.Panel_LocalizationBoxes = New System.Windows.Forms.Panel()
        Me.LocalizationTextBox3 = New Eddy.WinForm.LocalizationTextBox()
        Me.Splitter2 = New System.Windows.Forms.Splitter()
        Me.LocalizationTextBox2 = New Eddy.WinForm.LocalizationTextBox()
        Me.Splitter1 = New System.Windows.Forms.Splitter()
        Me.LocalizationTextBox1 = New Eddy.WinForm.LocalizationTextBox()
        Me.ComboBox_TextName = New System.Windows.Forms.ComboBox()
        Me.Button_Open = New System.Windows.Forms.Button()
        Me.Button_NextFile = New System.Windows.Forms.Button()
        Me.Label_NextFile = New System.Windows.Forms.Label()
        Me.Button_PreviousFile = New System.Windows.Forms.Button()
        Me.VScrollBar_Bar = New System.Windows.Forms.VScrollBar()
        Me.Label_PreviousFile = New System.Windows.Forms.Label()
        Me.Label_Bar = New System.Windows.Forms.Label()
        Me.Panel_Background = New System.Windows.Forms.Panel()
        CType(Me.SplitContainer_Main, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer_Main.Panel1.SuspendLayout()
        Me.SplitContainer_Main.Panel2.SuspendLayout()
        Me.SplitContainer_Main.SuspendLayout()
        CType(Me.DataGridView_Multiview, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Panel_Work.SuspendLayout()
        CType(Me.NumericUpDown_TextNumber, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Panel_LocalizationBoxes.SuspendLayout()
        Me.Panel_Background.SuspendLayout()
        Me.SuspendLayout()
        '
        'SplitContainer_Main
        '
        Me.SplitContainer_Main.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer_Main.Location = New System.Drawing.Point(0, 0)
        Me.SplitContainer_Main.Margin = New System.Windows.Forms.Padding(0)
        Me.SplitContainer_Main.Name = "SplitContainer_Main"
        '
        'SplitContainer_Main.Panel1
        '
        Me.SplitContainer_Main.Panel1.Controls.Add(Me.DataGridView_Multiview)
        Me.SplitContainer_Main.Panel1MinSize = 0
        '
        'SplitContainer_Main.Panel2
        '
        Me.SplitContainer_Main.Panel2.Controls.Add(Me.Panel_Work)
        Me.SplitContainer_Main.Panel2MinSize = 0
        Me.SplitContainer_Main.Size = New System.Drawing.Size(792, 566)
        Me.SplitContainer_Main.SplitterDistance = 328
        Me.SplitContainer_Main.SplitterWidth = 3
        Me.SplitContainer_Main.TabIndex = 14
        '
        'DataGridView_Multiview
        '
        Me.DataGridView_Multiview.AllowUserToAddRows = False
        Me.DataGridView_Multiview.AllowUserToDeleteRows = False
        Me.DataGridView_Multiview.AllowUserToResizeRows = False
        Me.DataGridView_Multiview.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle1.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        DataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        DataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.DataGridView_Multiview.DefaultCellStyle = DataGridViewCellStyle1
        Me.DataGridView_Multiview.Dock = System.Windows.Forms.DockStyle.Fill
        Me.DataGridView_Multiview.Location = New System.Drawing.Point(0, 0)
        Me.DataGridView_Multiview.Name = "DataGridView_Multiview"
        Me.DataGridView_Multiview.ReadOnly = True
        DataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight
        DataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control
        DataGridViewCellStyle2.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        DataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        DataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.DataGridView_Multiview.RowHeadersDefaultCellStyle = DataGridViewCellStyle2
        Me.DataGridView_Multiview.RowTemplate.Height = 23
        Me.DataGridView_Multiview.ShowCellErrors = False
        Me.DataGridView_Multiview.ShowCellToolTips = False
        Me.DataGridView_Multiview.ShowEditingIcon = False
        Me.DataGridView_Multiview.ShowRowErrors = False
        Me.DataGridView_Multiview.Size = New System.Drawing.Size(328, 566)
        Me.DataGridView_Multiview.TabIndex = 0
        Me.DataGridView_Multiview.VirtualMode = True
        '
        'Panel_Work
        '
        Me.Panel_Work.Controls.Add(Me.NumericUpDown_TextNumber)
        Me.Panel_Work.Controls.Add(Me.ToolStrip_Tools)
        Me.Panel_Work.Controls.Add(Me.Label_TextName)
        Me.Panel_Work.Controls.Add(Me.Label_LOCBoxTip)
        Me.Panel_Work.Controls.Add(Me.Panel_LocalizationBoxes)
        Me.Panel_Work.Controls.Add(Me.ComboBox_TextName)
        Me.Panel_Work.Controls.Add(Me.Button_Open)
        Me.Panel_Work.Controls.Add(Me.Button_NextFile)
        Me.Panel_Work.Controls.Add(Me.Label_NextFile)
        Me.Panel_Work.Controls.Add(Me.Button_PreviousFile)
        Me.Panel_Work.Controls.Add(Me.VScrollBar_Bar)
        Me.Panel_Work.Controls.Add(Me.Label_PreviousFile)
        Me.Panel_Work.Controls.Add(Me.Label_Bar)
        Me.Panel_Work.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel_Work.Location = New System.Drawing.Point(0, 0)
        Me.Panel_Work.Name = "Panel_Work"
        Me.Panel_Work.Size = New System.Drawing.Size(461, 566)
        Me.Panel_Work.TabIndex = 13
        '
        'NumericUpDown_TextNumber
        '
        Me.NumericUpDown_TextNumber.Location = New System.Drawing.Point(8, 35)
        Me.NumericUpDown_TextNumber.Maximum = New Decimal(New Integer() {0, 0, 0, 0})
        Me.NumericUpDown_TextNumber.Name = "NumericUpDown_TextNumber"
        Me.NumericUpDown_TextNumber.Size = New System.Drawing.Size(59, 21)
        Me.NumericUpDown_TextNumber.TabIndex = 14
        Me.NumericUpDown_TextNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'ToolStrip_Tools
        '
        Me.ToolStrip_Tools.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.ToolStrip_Tools.Dock = System.Windows.Forms.DockStyle.None
        Me.ToolStrip_Tools.Location = New System.Drawing.Point(4, 538)
        Me.ToolStrip_Tools.Name = "ToolStrip_Tools"
        Me.ToolStrip_Tools.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
        Me.ToolStrip_Tools.Size = New System.Drawing.Size(111, 25)
        Me.ToolStrip_Tools.TabIndex = 13
        Me.ToolStrip_Tools.Text = "ToolStrip_Tools"
        '
        'Label_TextName
        '
        Me.Label_TextName.AutoSize = True
        Me.Label_TextName.Location = New System.Drawing.Point(6, 12)
        Me.Label_TextName.Name = "Label_TextName"
        Me.Label_TextName.Size = New System.Drawing.Size(29, 12)
        Me.Label_TextName.TabIndex = 0
        Me.Label_TextName.Text = "文本"
        '
        'Label_LOCBoxTip
        '
        Me.Label_LOCBoxTip.AutoSize = True
        Me.Label_LOCBoxTip.Location = New System.Drawing.Point(190, 42)
        Me.Label_LOCBoxTip.Name = "Label_LOCBoxTip"
        Me.Label_LOCBoxTip.Size = New System.Drawing.Size(149, 12)
        Me.Label_LOCBoxTip.TabIndex = 10
        Me.Label_LOCBoxTip.Text = "按鼠标滚轮切换图形文本框"
        '
        'Panel_LocalizationBoxes
        '
        Me.Panel_LocalizationBoxes.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Panel_LocalizationBoxes.Controls.Add(Me.LocalizationTextBox3)
        Me.Panel_LocalizationBoxes.Controls.Add(Me.Splitter2)
        Me.Panel_LocalizationBoxes.Controls.Add(Me.LocalizationTextBox2)
        Me.Panel_LocalizationBoxes.Controls.Add(Me.Splitter1)
        Me.Panel_LocalizationBoxes.Controls.Add(Me.LocalizationTextBox1)
        Me.Panel_LocalizationBoxes.Location = New System.Drawing.Point(8, 57)
        Me.Panel_LocalizationBoxes.Name = "Panel_LocalizationBoxes"
        Me.Panel_LocalizationBoxes.Size = New System.Drawing.Size(422, 470)
        Me.Panel_LocalizationBoxes.TabIndex = 9
        '
        'LocalizationTextBox3
        '
        Me.LocalizationTextBox3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.LocalizationTextBox3.Dock = System.Windows.Forms.DockStyle.Fill
        Me.LocalizationTextBox3.Location = New System.Drawing.Point(0, 260)
        Me.LocalizationTextBox3.Name = "LocalizationTextBox3"
        Me.LocalizationTextBox3.Size = New System.Drawing.Size(422, 210)
        Me.LocalizationTextBox3.TabIndex = 4
        '
        'Splitter2
        '
        Me.Splitter2.BackColor = System.Drawing.SystemColors.InactiveBorder
        Me.Splitter2.Dock = System.Windows.Forms.DockStyle.Top
        Me.Splitter2.Location = New System.Drawing.Point(0, 257)
        Me.Splitter2.Name = "Splitter2"
        Me.Splitter2.Size = New System.Drawing.Size(422, 3)
        Me.Splitter2.TabIndex = 3
        Me.Splitter2.TabStop = False
        '
        'LocalizationTextBox2
        '
        Me.LocalizationTextBox2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.LocalizationTextBox2.Dock = System.Windows.Forms.DockStyle.Top
        Me.LocalizationTextBox2.Location = New System.Drawing.Point(0, 137)
        Me.LocalizationTextBox2.Name = "LocalizationTextBox2"
        Me.LocalizationTextBox2.Size = New System.Drawing.Size(422, 120)
        Me.LocalizationTextBox2.TabIndex = 2
        '
        'Splitter1
        '
        Me.Splitter1.BackColor = System.Drawing.SystemColors.InactiveBorder
        Me.Splitter1.Dock = System.Windows.Forms.DockStyle.Top
        Me.Splitter1.Location = New System.Drawing.Point(0, 134)
        Me.Splitter1.Name = "Splitter1"
        Me.Splitter1.Size = New System.Drawing.Size(422, 3)
        Me.Splitter1.TabIndex = 1
        Me.Splitter1.TabStop = False
        '
        'LocalizationTextBox1
        '
        Me.LocalizationTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.LocalizationTextBox1.Dock = System.Windows.Forms.DockStyle.Top
        Me.LocalizationTextBox1.Location = New System.Drawing.Point(0, 0)
        Me.LocalizationTextBox1.Name = "LocalizationTextBox1"
        Me.LocalizationTextBox1.Size = New System.Drawing.Size(422, 134)
        Me.LocalizationTextBox1.TabIndex = 0
        '
        'ComboBox_TextName
        '
        Me.ComboBox_TextName.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ComboBox_TextName.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend
        Me.ComboBox_TextName.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems
        Me.ComboBox_TextName.Location = New System.Drawing.Point(41, 9)
        Me.ComboBox_TextName.Name = "ComboBox_TextName"
        Me.ComboBox_TextName.Size = New System.Drawing.Size(283, 20)
        Me.ComboBox_TextName.TabIndex = 1
        '
        'Button_Open
        '
        Me.Button_Open.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Button_Open.Location = New System.Drawing.Point(330, 7)
        Me.Button_Open.Name = "Button_Open"
        Me.Button_Open.Size = New System.Drawing.Size(59, 23)
        Me.Button_Open.TabIndex = 2
        Me.Button_Open.Text = "打开(&O)"
        Me.Button_Open.UseVisualStyleBackColor = True
        '
        'Button_NextFile
        '
        Me.Button_NextFile.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Button_NextFile.Location = New System.Drawing.Point(427, 7)
        Me.Button_NextFile.Name = "Button_NextFile"
        Me.Button_NextFile.Size = New System.Drawing.Size(26, 23)
        Me.Button_NextFile.TabIndex = 6
        Me.Button_NextFile.Text = "->"
        Me.Button_NextFile.UseVisualStyleBackColor = True
        '
        'Label_NextFile
        '
        Me.Label_NextFile.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label_NextFile.AutoSize = True
        Me.Label_NextFile.Location = New System.Drawing.Point(431, 29)
        Me.Label_NextFile.Name = "Label_NextFile"
        Me.Label_NextFile.Size = New System.Drawing.Size(17, 12)
        Me.Label_NextFile.TabIndex = 5
        Me.Label_NextFile.Text = "F7"
        '
        'Button_PreviousFile
        '
        Me.Button_PreviousFile.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Button_PreviousFile.Location = New System.Drawing.Point(395, 7)
        Me.Button_PreviousFile.Name = "Button_PreviousFile"
        Me.Button_PreviousFile.Size = New System.Drawing.Size(26, 23)
        Me.Button_PreviousFile.TabIndex = 4
        Me.Button_PreviousFile.Text = "<-"
        Me.Button_PreviousFile.UseVisualStyleBackColor = True
        '
        'VScrollBar_Bar
        '
        Me.VScrollBar_Bar.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.VScrollBar_Bar.LargeChange = 1
        Me.VScrollBar_Bar.Location = New System.Drawing.Point(433, 57)
        Me.VScrollBar_Bar.Maximum = 0
        Me.VScrollBar_Bar.Name = "VScrollBar_Bar"
        Me.VScrollBar_Bar.Size = New System.Drawing.Size(20, 470)
        Me.VScrollBar_Bar.TabIndex = 12
        '
        'Label_PreviousFile
        '
        Me.Label_PreviousFile.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label_PreviousFile.AutoSize = True
        Me.Label_PreviousFile.Location = New System.Drawing.Point(399, 29)
        Me.Label_PreviousFile.Name = "Label_PreviousFile"
        Me.Label_PreviousFile.Size = New System.Drawing.Size(17, 12)
        Me.Label_PreviousFile.TabIndex = 3
        Me.Label_PreviousFile.Text = "F6"
        '
        'Label_Bar
        '
        Me.Label_Bar.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label_Bar.AutoSize = True
        Me.Label_Bar.Location = New System.Drawing.Point(394, 42)
        Me.Label_Bar.Name = "Label_Bar"
        Me.Label_Bar.Size = New System.Drawing.Size(59, 12)
        Me.Label_Bar.TabIndex = 11
        Me.Label_Bar.Text = "PgUp/PgDn"
        '
        'Panel_Background
        '
        Me.Panel_Background.BackColor = System.Drawing.SystemColors.Control
        Me.Panel_Background.Controls.Add(Me.SplitContainer_Main)
        Me.Panel_Background.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel_Background.Location = New System.Drawing.Point(0, 0)
        Me.Panel_Background.Name = "Panel_Background"
        Me.Panel_Background.Size = New System.Drawing.Size(792, 566)
        Me.Panel_Background.TabIndex = 0
        '
        'FormMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.SystemColors.Control
        Me.ClientSize = New System.Drawing.Size(792, 566)
        Me.Controls.Add(Me.Panel_Background)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.KeyPreview = True
        Me.Name = "FormMain"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "漩涡文本本地化工具(Firefly.Eddy)"
        Me.SplitContainer_Main.Panel1.ResumeLayout(False)
        Me.SplitContainer_Main.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer_Main, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer_Main.ResumeLayout(False)
        CType(Me.DataGridView_Multiview, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Panel_Work.ResumeLayout(False)
        Me.Panel_Work.PerformLayout()
        CType(Me.NumericUpDown_TextNumber, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Panel_LocalizationBoxes.ResumeLayout(False)
        Me.Panel_Background.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents SplitContainer_Main As System.Windows.Forms.SplitContainer
    Friend WithEvents DataGridView_Multiview As System.Windows.Forms.DataGridView
    Friend WithEvents Panel_Work As System.Windows.Forms.Panel
    Friend WithEvents Label_TextName As System.Windows.Forms.Label
    Friend WithEvents Label_LOCBoxTip As System.Windows.Forms.Label
    Friend WithEvents Panel_LocalizationBoxes As System.Windows.Forms.Panel
    Friend WithEvents LocalizationTextBox3 As LocalizationTextBox
    Friend WithEvents Splitter2 As System.Windows.Forms.Splitter
    Friend WithEvents LocalizationTextBox2 As LocalizationTextBox
    Friend WithEvents Splitter1 As System.Windows.Forms.Splitter
    Friend WithEvents LocalizationTextBox1 As LocalizationTextBox
    Friend WithEvents ComboBox_TextName As System.Windows.Forms.ComboBox
    Friend WithEvents Button_Open As System.Windows.Forms.Button
    Friend WithEvents Button_NextFile As System.Windows.Forms.Button
    Friend WithEvents Label_NextFile As System.Windows.Forms.Label
    Friend WithEvents Button_PreviousFile As System.Windows.Forms.Button
    Friend WithEvents VScrollBar_Bar As System.Windows.Forms.VScrollBar
    Friend WithEvents Label_PreviousFile As System.Windows.Forms.Label
    Friend WithEvents Label_Bar As System.Windows.Forms.Label
    Friend WithEvents Panel_Background As System.Windows.Forms.Panel
    Friend WithEvents ToolStrip_Tools As System.Windows.Forms.ToolStrip
    Friend WithEvents NumericUpDown_TextNumber As System.Windows.Forms.NumericUpDown

End Class
