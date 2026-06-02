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
        btnP1 = New Button()
        btnP2 = New Button()
        btnP3 = New Button()
        btnP4 = New Button()
        btnOpenDoor = New Button()
        btnCloseDoor = New Button()
        btnBoard = New Button()
        btnDisembark = New Button()
        lblBuildingTitle = New Label()
        pnlPassengers = New Panel()
        nudPassengers = New NumericUpDown()
        btnEmergency = New Button()
        lblCurFloorTitle = New Label()
        lblDoorStatus = New Label()
        lblStatus = New Label()
        lblWeightVal = New Label()
        lblPassengerCount = New Label()
        picBuilding = New PictureBox()
        lstLog = New ListBox()
        pnlStatus = New Panel()
        lblStatusTitle = New Label()
        lblDoorTitle = New Label()
        lblDirTitle = New Label()
        lblCurrentFloor = New Label()
        lblControlsTitle = New Label()
        lblFloorSelTitle = New Label()
        pnlCapacityBg = New Panel()
        lblCapacityBar = New Label()
        btnStart = New Button()
        btnPause = New Button()
        pnlPassengers.SuspendLayout()
        CType(nudPassengers, ComponentModel.ISupportInitialize).BeginInit()
        CType(picBuilding, ComponentModel.ISupportInitialize).BeginInit()
        pnlStatus.SuspendLayout()
        pnlCapacityBg.SuspendLayout()
        SuspendLayout()
        ' 
        ' btnP1
        ' 
        btnP1.Location = New Point(1113, 220)
        btnP1.Name = "btnP1"
        btnP1.Size = New Size(136, 49)
        btnP1.TabIndex = 0
        btnP1.Text = "Floor 1"
        btnP1.UseVisualStyleBackColor = True
        ' 
        ' btnP2
        ' 
        btnP2.Location = New Point(1113, 159)
        btnP2.Name = "btnP2"
        btnP2.Size = New Size(136, 49)
        btnP2.TabIndex = 1
        btnP2.Text = "Floor 2"
        btnP2.UseVisualStyleBackColor = True
        ' 
        ' btnP3
        ' 
        btnP3.Location = New Point(1113, 99)
        btnP3.Name = "btnP3"
        btnP3.Size = New Size(136, 49)
        btnP3.TabIndex = 2
        btnP3.Text = "Floor 3"
        btnP3.UseVisualStyleBackColor = True
        ' 
        ' btnP4
        ' 
        btnP4.Location = New Point(1113, 41)
        btnP4.Name = "btnP4"
        btnP4.Size = New Size(136, 49)
        btnP4.TabIndex = 3
        btnP4.Text = "Floor 4"
        btnP4.UseVisualStyleBackColor = True
        ' 
        ' btnOpenDoor
        ' 
        btnOpenDoor.Location = New Point(1036, 400)
        btnOpenDoor.Name = "btnOpenDoor"
        btnOpenDoor.Size = New Size(93, 49)
        btnOpenDoor.TabIndex = 4
        btnOpenDoor.Text = "Open Door"
        btnOpenDoor.UseVisualStyleBackColor = True
        ' 
        ' btnCloseDoor
        ' 
        btnCloseDoor.Location = New Point(1150, 400)
        btnCloseDoor.Name = "btnCloseDoor"
        btnCloseDoor.Size = New Size(93, 49)
        btnCloseDoor.TabIndex = 5
        btnCloseDoor.Text = "Close Door"
        btnCloseDoor.UseVisualStyleBackColor = True
        ' 
        ' btnBoard
        ' 
        btnBoard.Location = New Point(44, 48)
        btnBoard.Name = "btnBoard"
        btnBoard.Size = New Size(117, 23)
        btnBoard.TabIndex = 12
        btnBoard.Text = "Board Passengers"
        btnBoard.UseVisualStyleBackColor = True
        ' 
        ' btnDisembark
        ' 
        btnDisembark.Location = New Point(44, 84)
        btnDisembark.Name = "btnDisembark"
        btnDisembark.Size = New Size(117, 23)
        btnDisembark.TabIndex = 13
        btnDisembark.Text = "Exit Passengers"
        btnDisembark.UseVisualStyleBackColor = True
        ' 
        ' lblBuildingTitle
        ' 
        lblBuildingTitle.AutoSize = True
        lblBuildingTitle.Location = New Point(12, 9)
        lblBuildingTitle.Name = "lblBuildingTitle"
        lblBuildingTitle.Size = New Size(107, 15)
        lblBuildingTitle.TabIndex = 18
        lblBuildingTitle.Text = "4-STORY BUILDING"
        ' 
        ' pnlPassengers
        ' 
        pnlPassengers.Controls.Add(btnDisembark)
        pnlPassengers.Controls.Add(btnBoard)
        pnlPassengers.Controls.Add(nudPassengers)
        pnlPassengers.Controls.Add(btnEmergency)
        pnlPassengers.Location = New Point(797, 367)
        pnlPassengers.Name = "pnlPassengers"
        pnlPassengers.Size = New Size(200, 154)
        pnlPassengers.TabIndex = 23
        ' 
        ' nudPassengers
        ' 
        nudPassengers.Location = New Point(44, 19)
        nudPassengers.Name = "nudPassengers"
        nudPassengers.Size = New Size(117, 23)
        nudPassengers.TabIndex = 31
        ' 
        ' btnEmergency
        ' 
        btnEmergency.Location = New Point(63, 120)
        btnEmergency.Name = "btnEmergency"
        btnEmergency.Size = New Size(75, 23)
        btnEmergency.TabIndex = 12
        btnEmergency.Text = "Button7"
        btnEmergency.UseVisualStyleBackColor = True
        ' 
        ' lblCurFloorTitle
        ' 
        lblCurFloorTitle.AutoSize = True
        lblCurFloorTitle.Location = New Point(108, 28)
        lblCurFloorTitle.Name = "lblCurFloorTitle"
        lblCurFloorTitle.Size = New Size(98, 15)
        lblCurFloorTitle.TabIndex = 24
        lblCurFloorTitle.Text = "CURRENT FLOOR"
        ' 
        ' lblDoorStatus
        ' 
        lblDoorStatus.AutoSize = True
        lblDoorStatus.Location = New Point(124, 246)
        lblDoorStatus.Name = "lblDoorStatus"
        lblDoorStatus.Size = New Size(41, 15)
        lblDoorStatus.TabIndex = 25
        lblDoorStatus.Text = "Label7"
        ' 
        ' lblStatus
        ' 
        lblStatus.AutoSize = True
        lblStatus.Location = New Point(86, 179)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(41, 15)
        lblStatus.TabIndex = 26
        lblStatus.Text = "Label7"
        ' 
        ' lblWeightVal
        ' 
        lblWeightVal.AutoSize = True
        lblWeightVal.Location = New Point(127, 284)
        lblWeightVal.Name = "lblWeightVal"
        lblWeightVal.Size = New Size(41, 15)
        lblWeightVal.TabIndex = 27
        lblWeightVal.Text = "Label7"
        ' 
        ' lblPassengerCount
        ' 
        lblPassengerCount.AutoSize = True
        lblPassengerCount.Location = New Point(127, 311)
        lblPassengerCount.Name = "lblPassengerCount"
        lblPassengerCount.Size = New Size(41, 15)
        lblPassengerCount.TabIndex = 28
        lblPassengerCount.Text = "Label7"
        ' 
        ' picBuilding
        ' 
        picBuilding.Location = New Point(4, 2)
        picBuilding.Name = "picBuilding"
        picBuilding.Size = New Size(748, 540)
        picBuilding.TabIndex = 29
        picBuilding.TabStop = False
        ' 
        ' lstLog
        ' 
        lstLog.FormattingEnabled = True
        lstLog.ItemHeight = 15
        lstLog.Location = New Point(1003, 455)
        lstLog.Name = "lstLog"
        lstLog.Size = New Size(307, 49)
        lstLog.TabIndex = 32
        ' 
        ' pnlStatus
        ' 
        pnlStatus.Controls.Add(lblStatusTitle)
        pnlStatus.Controls.Add(lblDoorTitle)
        pnlStatus.Controls.Add(lblDirTitle)
        pnlStatus.Controls.Add(lblCurrentFloor)
        pnlStatus.Controls.Add(lblCurFloorTitle)
        pnlStatus.Controls.Add(lblPassengerCount)
        pnlStatus.Controls.Add(lblDoorStatus)
        pnlStatus.Controls.Add(lblStatus)
        pnlStatus.Controls.Add(lblWeightVal)
        pnlStatus.Location = New Point(758, 3)
        pnlStatus.Name = "pnlStatus"
        pnlStatus.Size = New Size(319, 335)
        pnlStatus.TabIndex = 35
        ' 
        ' lblStatusTitle
        ' 
        lblStatusTitle.AutoSize = True
        lblStatusTitle.Location = New Point(106, 6)
        lblStatusTitle.Name = "lblStatusTitle"
        lblStatusTitle.Size = New Size(100, 15)
        lblStatusTitle.TabIndex = 37
        lblStatusTitle.Text = "ELEVATOR STATUS"
        ' 
        ' lblDoorTitle
        ' 
        lblDoorTitle.AutoSize = True
        lblDoorTitle.Location = New Point(115, 217)
        lblDoorTitle.Name = "lblDoorTitle"
        lblDoorTitle.Size = New Size(81, 15)
        lblDoorTitle.TabIndex = 36
        lblDoorTitle.Text = "DOOR STATUS"
        ' 
        ' lblDirTitle
        ' 
        lblDirTitle.AutoSize = True
        lblDirTitle.Location = New Point(120, 147)
        lblDirTitle.Name = "lblDirTitle"
        lblDirTitle.Size = New Size(41, 15)
        lblDirTitle.TabIndex = 26
        lblDirTitle.Text = "Label7"
        ' 
        ' lblCurrentFloor
        ' 
        lblCurrentFloor.AutoSize = True
        lblCurrentFloor.Location = New Point(117, 64)
        lblCurrentFloor.Name = "lblCurrentFloor"
        lblCurrentFloor.Size = New Size(41, 15)
        lblCurrentFloor.TabIndex = 25
        lblCurrentFloor.Text = "Label7"
        ' 
        ' lblControlsTitle
        ' 
        lblControlsTitle.AutoSize = True
        lblControlsTitle.Location = New Point(1083, 324)
        lblControlsTitle.Name = "lblControlsTitle"
        lblControlsTitle.Size = New Size(122, 15)
        lblControlsTitle.TabIndex = 36
        lblControlsTitle.Text = "ELEVATOR CONTROLS"
        ' 
        ' lblFloorSelTitle
        ' 
        lblFloorSelTitle.AutoSize = True
        lblFloorSelTitle.Location = New Point(1101, 15)
        lblFloorSelTitle.Name = "lblFloorSelTitle"
        lblFloorSelTitle.Size = New Size(106, 15)
        lblFloorSelTitle.TabIndex = 37
        lblFloorSelTitle.Text = "FLOOR SELECTION"
        ' 
        ' pnlCapacityBg
        ' 
        pnlCapacityBg.Controls.Add(lblCapacityBar)
        pnlCapacityBg.Location = New Point(1043, 508)
        pnlCapacityBg.Name = "pnlCapacityBg"
        pnlCapacityBg.Size = New Size(200, 13)
        pnlCapacityBg.TabIndex = 32
        ' 
        ' lblCapacityBar
        ' 
        lblCapacityBar.AutoSize = True
        lblCapacityBar.Location = New Point(76, 12)
        lblCapacityBar.Name = "lblCapacityBar"
        lblCapacityBar.Size = New Size(41, 15)
        lblCapacityBar.TabIndex = 38
        lblCapacityBar.Text = "Label1"
        ' 
        ' btnStart
        ' 
        btnStart.Location = New Point(1036, 345)
        btnStart.Name = "btnStart"
        btnStart.Size = New Size(93, 49)
        btnStart.TabIndex = 38
        btnStart.Text = "Start"
        btnStart.UseVisualStyleBackColor = True
        ' 
        ' btnPause
        ' 
        btnPause.Location = New Point(1150, 345)
        btnPause.Name = "btnPause"
        btnPause.Size = New Size(93, 49)
        btnPause.TabIndex = 39
        btnPause.Text = "Pause"
        btnPause.UseVisualStyleBackColor = True
        ' 
        ' FormElevator
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1364, 523)
        Controls.Add(btnPause)
        Controls.Add(btnStart)
        Controls.Add(pnlCapacityBg)
        Controls.Add(lblFloorSelTitle)
        Controls.Add(lblControlsTitle)
        Controls.Add(btnP4)
        Controls.Add(btnP2)
        Controls.Add(btnP3)
        Controls.Add(btnP1)
        Controls.Add(lblBuildingTitle)
        Controls.Add(btnOpenDoor)
        Controls.Add(btnCloseDoor)
        Controls.Add(lstLog)
        Controls.Add(picBuilding)
        Controls.Add(pnlPassengers)
        Controls.Add(pnlStatus)
        Name = "FormElevator"
        Text = "FormElevator"
        pnlPassengers.ResumeLayout(False)
        CType(nudPassengers, ComponentModel.ISupportInitialize).EndInit()
        CType(picBuilding, ComponentModel.ISupportInitialize).EndInit()
        pnlStatus.ResumeLayout(False)
        pnlStatus.PerformLayout()
        pnlCapacityBg.ResumeLayout(False)
        pnlCapacityBg.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents btnP1 As Button
    Friend WithEvents btnP2 As Button
    Friend WithEvents btnP3 As Button
    Friend WithEvents btnP4 As Button
    Friend WithEvents btnOpenDoor As Button
    Friend WithEvents btnCloseDoor As Button
    Friend WithEvents btnBoard As Button
    Friend WithEvents btnDisembark As Button
    Friend WithEvents lblBuildingTitle As Label
    Friend WithEvents pnlPassengers As Panel
    Friend WithEvents lblCurFloorTitle As Label
    Friend WithEvents lblDoorStatus As Label
    Friend WithEvents lblStatus As Label
    Friend WithEvents lblWeightVal As Label
    Friend WithEvents lblPassengerCount As Label
    Friend WithEvents picBuilding As PictureBox
    Friend WithEvents btnEmergency As Button
    Friend WithEvents nudPassengers As NumericUpDown
    Friend WithEvents lstLog As ListBox
    Friend WithEvents pnlStatus As Panel
    Friend WithEvents lblCurrentFloor As Label
    Friend WithEvents lblDirTitle As Label
    Friend WithEvents lblDoorTitle As Label
    Friend WithEvents lblStatusTitle As Label
    Friend WithEvents lblControlsTitle As Label
    Friend WithEvents lblFloorSelTitle As Label
    Friend WithEvents pnlCapacityBg As Panel
    Friend WithEvents lblCapacityBar As Label
    Friend WithEvents btnStart As Button
    Friend WithEvents btnPause As Button

End Class
