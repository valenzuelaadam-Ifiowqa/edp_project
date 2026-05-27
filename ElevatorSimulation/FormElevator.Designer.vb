<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormElevator
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        lblCurrentFloor = New Label()
        lblStatus = New Label()
        btnFloor1 = New Button()
        BackgroundWorker1 = New ComponentModel.BackgroundWorker()
        btnFloor3 = New Button()
        btnFloor2 = New Button()
        picShaft = New PictureBox()
        pnlCapacityBg = New Panel()
        lblWeightVal = New Label()
        lblCapacityBar = New Label()
        lstLog = New ListBox()
        btnEmergency = New Button()
        btnHoldDoor = New Button()
        btnAddPassengers = New Button()
        btnClearPassengers = New Button()
        nudPassengers = New NumericUpDown()
        lblPassengerCount = New Label()
        picDoor = New PictureBox()
        lblDoorStatus = New Label()
        picBuilding = New PictureBox()
        CType(picShaft, ComponentModel.ISupportInitialize).BeginInit()
        pnlCapacityBg.SuspendLayout()
        CType(nudPassengers, ComponentModel.ISupportInitialize).BeginInit()
        CType(picDoor, ComponentModel.ISupportInitialize).BeginInit()
        CType(picBuilding, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' lblCurrentFloor
        ' 
        lblCurrentFloor.AutoSize = True
        lblCurrentFloor.Location = New Point(278, 26)
        lblCurrentFloor.Name = "lblCurrentFloor"
        lblCurrentFloor.Size = New Size(41, 15)
        lblCurrentFloor.TabIndex = 0
        lblCurrentFloor.Text = "Label1"
        ' 
        ' lblStatus
        ' 
        lblStatus.AutoSize = True
        lblStatus.Location = New Point(392, 38)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(41, 15)
        lblStatus.TabIndex = 1
        lblStatus.Text = "Label1"
        ' 
        ' btnFloor1
        ' 
        btnFloor1.Location = New Point(881, 268)
        btnFloor1.Name = "btnFloor1"
        btnFloor1.Size = New Size(122, 48)
        btnFloor1.TabIndex = 2
        btnFloor1.Text = "Floor 1"
        btnFloor1.UseVisualStyleBackColor = True
        ' 
        ' btnFloor3
        ' 
        btnFloor3.Location = New Point(881, 160)
        btnFloor3.Name = "btnFloor3"
        btnFloor3.Size = New Size(122, 48)
        btnFloor3.TabIndex = 3
        btnFloor3.Text = "Floor 3"
        btnFloor3.UseVisualStyleBackColor = True
        ' 
        ' btnFloor2
        ' 
        btnFloor2.Location = New Point(881, 214)
        btnFloor2.Name = "btnFloor2"
        btnFloor2.Size = New Size(122, 48)
        btnFloor2.TabIndex = 4
        btnFloor2.Text = "Floor 2"
        btnFloor2.UseVisualStyleBackColor = True
        ' 
        ' picShaft
        ' 
        picShaft.Location = New Point(560, 148)
        picShaft.Name = "picShaft"
        picShaft.Size = New Size(257, 295)
        picShaft.TabIndex = 6
        picShaft.TabStop = False
        ' 
        ' pnlCapacityBg
        ' 
        pnlCapacityBg.Controls.Add(lblWeightVal)
        pnlCapacityBg.Controls.Add(lblCapacityBar)
        pnlCapacityBg.Location = New Point(881, 404)
        pnlCapacityBg.Name = "pnlCapacityBg"
        pnlCapacityBg.Size = New Size(137, 57)
        pnlCapacityBg.TabIndex = 8
        ' 
        ' lblWeightVal
        ' 
        lblWeightVal.AutoSize = True
        lblWeightVal.Location = New Point(21, 19)
        lblWeightVal.Name = "lblWeightVal"
        lblWeightVal.Size = New Size(41, 15)
        lblWeightVal.TabIndex = 1
        lblWeightVal.Text = "Label1"
        ' 
        ' lblCapacityBar
        ' 
        lblCapacityBar.AutoSize = True
        lblCapacityBar.Location = New Point(81, 19)
        lblCapacityBar.Name = "lblCapacityBar"
        lblCapacityBar.Size = New Size(41, 15)
        lblCapacityBar.TabIndex = 0
        lblCapacityBar.Text = "Label1"
        ' 
        ' lstLog
        ' 
        lstLog.FormattingEnabled = True
        lstLog.ItemHeight = 15
        lstLog.Location = New Point(12, 156)
        lstLog.Name = "lstLog"
        lstLog.Size = New Size(249, 364)
        lstLog.TabIndex = 11
        ' 
        ' btnEmergency
        ' 
        btnEmergency.Location = New Point(1061, 425)
        btnEmergency.Name = "btnEmergency"
        btnEmergency.Size = New Size(124, 34)
        btnEmergency.TabIndex = 12
        btnEmergency.Text = "Emergency Stop"
        btnEmergency.UseVisualStyleBackColor = True
        ' 
        ' btnHoldDoor
        ' 
        btnHoldDoor.Location = New Point(1061, 383)
        btnHoldDoor.Name = "btnHoldDoor"
        btnHoldDoor.Size = New Size(124, 36)
        btnHoldDoor.TabIndex = 13
        btnHoldDoor.Text = "Hold Door"
        btnHoldDoor.UseVisualStyleBackColor = True
        ' 
        ' btnAddPassengers
        ' 
        btnAddPassengers.Location = New Point(1050, 193)
        btnAddPassengers.Name = "btnAddPassengers"
        btnAddPassengers.Size = New Size(126, 60)
        btnAddPassengers.TabIndex = 14
        btnAddPassengers.Text = "➕ Board Passengers"
        btnAddPassengers.UseVisualStyleBackColor = True
        ' 
        ' btnClearPassengers
        ' 
        btnClearPassengers.Location = New Point(1050, 268)
        btnClearPassengers.Name = "btnClearPassengers"
        btnClearPassengers.Size = New Size(126, 60)
        btnClearPassengers.TabIndex = 15
        btnClearPassengers.Text = "🚶  Disembark All"
        btnClearPassengers.UseVisualStyleBackColor = True
        ' 
        ' nudPassengers
        ' 
        nudPassengers.Location = New Point(1050, 158)
        nudPassengers.Name = "nudPassengers"
        nudPassengers.Size = New Size(126, 23)
        nudPassengers.TabIndex = 16
        nudPassengers.TextAlign = HorizontalAlignment.Center
        ' 
        ' lblPassengerCount
        ' 
        lblPassengerCount.AutoSize = True
        lblPassengerCount.Location = New Point(1054, 347)
        lblPassengerCount.Name = "lblPassengerCount"
        lblPassengerCount.Size = New Size(41, 15)
        lblPassengerCount.TabIndex = 17
        lblPassengerCount.Text = "Label1"
        ' 
        ' picDoor
        ' 
        picDoor.Location = New Point(560, 145)
        picDoor.Name = "picDoor"
        picDoor.Size = New Size(257, 300)
        picDoor.TabIndex = 18
        picDoor.TabStop = False
        ' 
        ' lblDoorStatus
        ' 
        lblDoorStatus.AutoSize = True
        lblDoorStatus.Location = New Point(881, 343)
        lblDoorStatus.Name = "lblDoorStatus"
        lblDoorStatus.Size = New Size(41, 15)
        lblDoorStatus.TabIndex = 19
        lblDoorStatus.Text = "Label1"
        ' 
        ' picBuilding
        ' 
        picBuilding.Location = New Point(278, 67)
        picBuilding.Name = "picBuilding"
        picBuilding.Size = New Size(597, 453)
        picBuilding.TabIndex = 20
        picBuilding.TabStop = False
        ' 
        ' FormElevator
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1211, 523)
        Controls.Add(picBuilding)
        Controls.Add(lblDoorStatus)
        Controls.Add(picDoor)
        Controls.Add(lblPassengerCount)
        Controls.Add(nudPassengers)
        Controls.Add(btnClearPassengers)
        Controls.Add(btnAddPassengers)
        Controls.Add(btnHoldDoor)
        Controls.Add(btnEmergency)
        Controls.Add(lstLog)
        Controls.Add(pnlCapacityBg)
        Controls.Add(picShaft)
        Controls.Add(btnFloor2)
        Controls.Add(btnFloor3)
        Controls.Add(btnFloor1)
        Controls.Add(lblStatus)
        Controls.Add(lblCurrentFloor)
        Name = "FormElevator"
        Text = "FormElevator"
        CType(picShaft, ComponentModel.ISupportInitialize).EndInit()
        pnlCapacityBg.ResumeLayout(False)
        pnlCapacityBg.PerformLayout()
        CType(nudPassengers, ComponentModel.ISupportInitialize).EndInit()
        CType(picDoor, ComponentModel.ISupportInitialize).EndInit()
        CType(picBuilding, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents lblCurrentFloor As Label
    Friend WithEvents lblStatus As Label
    Friend WithEvents btnFloor1 As Button
    Friend WithEvents BackgroundWorker1 As System.ComponentModel.BackgroundWorker
    Friend WithEvents btnFloor3 As Button
    Friend WithEvents btnFloor2 As Button
    Friend WithEvents picShaft As PictureBox
    Friend WithEvents pnlCapacityBg As Panel
    Friend WithEvents lstLog As ListBox
    Friend WithEvents btnEmergency As Button
    Friend WithEvents btnHoldDoor As Button
    Friend WithEvents lblWeightVal As Label
    Friend WithEvents lblCapacityBar As Label
    Friend WithEvents btnAddPassengers As Button
    Friend WithEvents btnClearPassengers As Button
    Friend WithEvents nudPassengers As NumericUpDown
    Friend WithEvents lblPassengerCount As Label
    Friend WithEvents picDoor As PictureBox
    Friend WithEvents lblDoorStatus As Label
    Friend WithEvents picBuilding As PictureBox

End Class
