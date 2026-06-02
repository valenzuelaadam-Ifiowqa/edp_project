
' 4-Story Elevator Simulator — Visual Basic .NET
' Final Project — Event-Driven Programming
Imports System.Threading
Imports System.Drawing.Drawing2D
Imports System.Linq

Public Class FormElevator

    ' ── State 
    Private currentFloor As Integer = 1
    Private targetQueue As New Queue(Of Integer)
    Private isMoving As Boolean = False
    Private isPaused As Boolean = False
    Private emergencyStop As Boolean = False
    Private doorOpen As Boolean = False
    Private doorHeld As Boolean = False
    Private currentDirection As Integer = 0  ' -1=down, 0=idle, 1=up

    ' ── Animation
    Private cabinY As Single = 0
    Private cabinYTarget As Single = 0
    Private doorAnim As Single = 0.0F
    Private doorAnimTarget As Single = 0.0F
    Private glowPulse As Single = 0.0F
    Private glowDir As Single = 0.02F
    Private arrivalFlash As Single = 0.0F
    Private indicatorBlink As Boolean = False
    Private blinkCount As Integer = 0

    ' ── Constants 
    Const FLOOR_COUNT As Integer = 4
    Const MAX_WEIGHT As Integer = 500
    Const WARN_WEIGHT As Integer = 450
    Const TRAVEL_MS As Integer = 1800
    Const DOOR_MS As Integer = 1000

    ' ── Floor data
    Private ReadOnly FloorNames() As String = {
        "", "1F · Lobby", "2F · Office", "3F · Lounge", "4F · Rooftop"
    }

    ' ── Passengers
    Private totalWeight As Integer = 0
    Private passengerCount As Integer = 0
    Private Shared rng As New Random()

    ' Timer 
    Private animTimer As New System.Windows.Forms.Timer()

    '  FORM LOAD
    Private Sub FormElevator_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.BackColor = Color.FromArgb(25, 28, 40)
        Me.ForeColor = Color.White
        Me.DoubleBuffered = True

        StyleControls()

        animTimer.Interval = 16
        AddHandler animTimer.Tick, AddressOf OnAnimTick
        animTimer.Start()

        ResetCabin()
        UpdateAllDisplays()
        Log("🟢 System ready. Elevator at Floor 1 - Lobby.")
    End Sub

    Private Sub ResetCabin()
        Dim fh As Single = picBuilding.Height / CSng(FLOOR_COUNT)
        cabinY = picBuilding.Height - fh
        cabinYTarget = cabinY
    End Sub

    '  STYLE CONTROLS
    Private Sub StyleControls()
        lblBuildingTitle.Font = New Font("Times New Roman", 9, FontStyle.Bold)
        lblBuildingTitle.ForeColor = Color.FromArgb(65, 70, 90)
        lblBuildingTitle.BackColor = Color.SkyBlue
        lblBuildingTitle.Text = "ADAM - INVINCICORP. (4-STORY BUILDING)"
        ' ── Status panel labels ──
        lblStatusTitle.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        lblStatusTitle.ForeColor = Color.FromArgb(180, 200, 255)
        lblStatusTitle.Text = "ELEVATOR STATUS"

        lblCurFloorTitle.Font = New Font("Segoe UI", 8, FontStyle.Bold)
        lblCurFloorTitle.ForeColor = Color.FromArgb(60, 220, 100)
        lblCurFloorTitle.Text = "CURRENT FLOOR"

        lblCurrentFloor.Font = New Font("Digital-7", 52, FontStyle.Bold)
        lblCurrentFloor.ForeColor = Color.FromArgb(60, 220, 100)
        lblCurrentFloor.Text = "1"
        lblCurrentFloor.TextAlign = ContentAlignment.MiddleCenter

        lblDirTitle.Font = New Font("Segoe UI", 8, FontStyle.Bold)
        lblDirTitle.ForeColor = Color.FromArgb(60, 220, 100)
        lblDirTitle.Text = "DIRECTION"

        lblDoorTitle.Font = New Font("Segoe UI", 8, FontStyle.Bold)
        lblDoorTitle.ForeColor = Color.FromArgb(60, 220, 100)
        lblDoorTitle.Text = "DOOR STATUS"

        lblDoorStatus.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        lblDoorStatus.ForeColor = Color.FromArgb(60, 220, 100)
        lblDoorStatus.Text = "CLOSED"

        ' ── Status panel background ──
        pnlStatus.BackColor = Color.FromArgb(8, 12, 8)
        pnlStatus.BorderStyle = BorderStyle.FixedSingle

        ' ── Controls title ──
        lblControlsTitle.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        lblControlsTitle.ForeColor = Color.FromArgb(180, 200, 255)
        lblControlsTitle.Text = "ELEVATOR CONTROLS"

        ' ── Elevator control buttons ──
        StyleButton(btnOpenDoor, Color.FromArgb(30, 140, 50))
        btnOpenDoor.Text = "◀▶" & vbCrLf & "OPEN DOOR"
        btnOpenDoor.Font = New Font("Segoe UI", 8, FontStyle.Bold)

        StyleButton(btnCloseDoor, Color.FromArgb(80, 85, 95))
        btnCloseDoor.Text = "▶◀" & vbCrLf & "CLOSE DOOR"
        btnCloseDoor.Font = New Font("Segoe UI", 8, FontStyle.Bold)

        StyleButton(btnStart, Color.FromArgb(80, 85, 95))
        btnStart.Text = vbCrLf & "Start"
        btnStart.Font = New Font("Segoe UI", 9, FontStyle.Bold)

        StyleButton(btnPause, Color.FromArgb(80, 85, 95))
        btnPause.Text = vbCrLf & "Pause"
        btnPause.Font = New Font("Segoe UI", 9, FontStyle.Bold)

        ' ── Floor selection ──
        lblFloorSelTitle.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        lblFloorSelTitle.ForeColor = Color.FromArgb(180, 200, 255)
        lblFloorSelTitle.Text = "FLOOR SELECTION"

        Dim panelBtns() As Button = {btnP4, btnP3, btnP2, btnP1}
        Dim floorNums() As String = {"4", "3", "2", "1"}
        For i As Integer = 0 To 3
            StyleButton(panelBtns(i), Color.FromArgb(65, 70, 82))
            panelBtns(i).Font = New Font("Segoe UI", 16, FontStyle.Bold)
            panelBtns(i).Text = floorNums(i)
            panelBtns(i).Size = New Size(70, 55)
        Next

        ' ── System controls ──
        StyleButton(btnEmergency, Color.FromArgb(200, 30, 30))
        btnEmergency.Text = "⏹  STOP"
        btnEmergency.Font = New Font("Segoe UI", 9, FontStyle.Bold)

        ' ── Passenger controls (small, bottom area) ──
        StyleButton(btnBoard, Color.FromArgb(30, 100, 60))
        btnBoard.Text = "Board"
        btnBoard.Font = New Font("Segoe UI", 8)

        StyleButton(btnDisembark, Color.FromArgb(80, 50, 120))
        btnDisembark.Text = "Exit"
        btnDisembark.Font = New Font("Segoe UI", 8)

        nudPassengers.BackColor = Color.FromArgb(30, 32, 48)
        nudPassengers.ForeColor = Color.FromArgb(180, 210, 255)
        nudPassengers.Minimum = 1
        nudPassengers.Maximum = 8

        lblWeightVal.Font = New Font("Segoe UI", 8)
        lblWeightVal.ForeColor = Color.FromArgb(140, 180, 255)
        lblWeightVal.Text = "0 / 500 kg"

        lblPassengerCount.Font = New Font("Segoe UI", 8)
        lblPassengerCount.ForeColor = Color.FromArgb(140, 180, 255)
        lblPassengerCount.Text = "0 aboard"

        pnlCapacityBg.BackColor = Color.FromArgb(35, 38, 55)
        pnlCapacityBg.Height = 10
        lblCapacityBar.BackColor = Color.FromArgb(40, 200, 100)
        lblCapacityBar.Text = ""
        lblCapacityBar.Height = 10

        ' ── Log ──
        lstLog.BackColor = Color.FromArgb(10, 12, 20)
        lstLog.ForeColor = Color.FromArgb(100, 200, 120)
        lstLog.Font = New Font("Consolas", 8)
        lstLog.BorderStyle = BorderStyle.None

        ' ── Building and shaft ──
        picBuilding.BackColor = Color.FromArgb(200, 195, 185)
    End Sub

    Private Sub StyleButton(btn As Button, base As Color)
        btn.BackColor = base
        btn.ForeColor = Color.White
        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderSize = 0
        btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(base, 0.2)
        btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(base, 0.2)
        btn.Cursor = Cursors.Hand
    End Sub

    ' ════════════════════════════════════════════════════════════
    '  ANIMATION TICK
    ' ════════════════════════════════════════════════════════════
    Private Sub OnAnimTick(sender As Object, e As EventArgs)
        Dim redraw As Boolean = False

        ' Cabin lerp
        If Math.Abs(cabinY - cabinYTarget) > 0.5F Then
            cabinY += (cabinYTarget - cabinY) * 0.09F
            redraw = True
        End If

        ' Door lerp
        If Math.Abs(doorAnim - doorAnimTarget) > 0.005F Then
            doorAnim += (doorAnimTarget - doorAnim) * 0.065F
            doorAnim = Math.Max(0, Math.Min(1, doorAnim))
            redraw = True
        End If

        ' Glow pulse
        glowPulse += glowDir
        If glowPulse > 1.0F Then glowDir = -0.02F
        If glowPulse < 0.0F Then glowDir = 0.02F

        ' Arrival flash
        If arrivalFlash > 0 Then
            arrivalFlash -= 0.04F
            If arrivalFlash < 0 Then arrivalFlash = 0
            redraw = True
        End If

        ' Blink indicator
        blinkCount += 1
        If blinkCount >= 30 Then
            blinkCount = 0
            indicatorBlink = Not indicatorBlink
            redraw = True
        End If

        If redraw Then
            picBuilding.Invalidate()
        End If
    End Sub

    '  BUILDING PAINT — matches reference design
    Private hallUpLit(4) As Boolean    ' floors 1-3 have up
    Private hallDownLit(4) As Boolean  ' floors 2-4 have down

    Private Sub picBuilding_Paint(sender As Object, e As PaintEventArgs) Handles picBuilding.Paint
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias

        Dim W As Integer = picBuilding.Width
        Dim H As Integer = picBuilding.Height
        Dim fh As Integer = H \ FLOOR_COUNT
        Dim shaftW As Integer = CInt(W * 0.28)
        Dim shaftX As Integer = W \ 2 - shaftW \ 2

        ' ── Draw each floor ──
        For f As Integer = FLOOR_COUNT To 1 Step -1
            Dim fy As Integer = H - (f * fh)
            Dim isCurrentFloor As Boolean = (f = currentFloor)

            ' Floor background
            Dim wallColor As Color = If(f Mod 2 = 0,
                Color.FromArgb(205, 200, 190),
                Color.FromArgb(195, 190, 182))
            g.FillRectangle(New SolidBrush(wallColor), 0, fy, W, fh)

            ' Ceiling line
            Using ceilPen As New Pen(Color.FromArgb(60, 62, 70), 3)
                g.DrawLine(ceilPen, 0, fy, W, fy)
            End Using

            ' Floor baseboard
            Using basePen As New Pen(Color.FromArgb(140, 132, 118), 4)
                g.DrawLine(basePen, 0, fy + fh - 2, shaftX, fy + fh - 2)
                g.DrawLine(basePen, shaftX + shaftW, fy + fh - 2, W, fy + fh - 2)
            End Using

            ' ── BIG INDICATOR LIGHT (behind floor number) ──────
            Dim numCX As Integer = 38
            Dim numCY As Integer = fy + fh \ 2

            If isCurrentFloor Then
                Dim glowSize As Integer = CInt(52 + glowPulse * 14)
                Dim gAlpha As Integer = CInt(55 + glowPulse * 80)
                Dim glowColor As Color = If(currentDirection = 1,
                    Color.FromArgb(gAlpha, 60, 220, 80),
                    If(currentDirection = -1,
                    Color.FromArgb(gAlpha, 80, 160, 255),
                    Color.FromArgb(gAlpha, 255, 220, 60)))
                Using ob As New SolidBrush(glowColor)
                    g.FillEllipse(ob, numCX - glowSize \ 2, numCY - glowSize \ 2, glowSize, glowSize)
                End Using
                Dim midSize As Integer = CInt(36 + glowPulse * 8)
                Dim mAlpha As Integer = CInt(110 + glowPulse * 100)
                Dim midColor As Color = If(currentDirection = 1,
                    Color.FromArgb(mAlpha, 60, 220, 80),
                    If(currentDirection = -1,
                    Color.FromArgb(mAlpha, 80, 160, 255),
                    Color.FromArgb(mAlpha, 255, 220, 60)))
                Using mb As New SolidBrush(midColor)
                    g.FillEllipse(mb, numCX - midSize \ 2, numCY - midSize \ 2, midSize, midSize)
                End Using
                Dim innerColor As Color = If(currentDirection = 1,
                    Color.FromArgb(220, 100, 255, 120),
                    If(currentDirection = -1,
                    Color.FromArgb(220, 120, 190, 255),
                    Color.FromArgb(220, 255, 240, 100)))
                Using ib As New SolidBrush(innerColor)
                    g.FillEllipse(ib, numCX - 17, numCY - 17, 34, 34)
                End Using
            Else
                Using db As New SolidBrush(Color.FromArgb(35, 60, 62, 70))
                    g.FillEllipse(db, numCX - 19, numCY - 19, 38, 38)
                End Using
            End If

            ' Floor text
            Using flFont As New Font("Segoe UI", 7, FontStyle.Bold)
                Using flBrush As New SolidBrush(
                    If(isCurrentFloor, Color.FromArgb(30, 30, 30), Color.FromArgb(90, 92, 100)))
                    Dim fs As SizeF = g.MeasureString("FLOOR", flFont)
                    g.DrawString("FLOOR", flFont, flBrush, numCX - fs.Width / 2, numCY - 24)
                End Using
            End Using
            Using bigFont As New Font("Segoe UI", 20, FontStyle.Bold)
                Using nb As New SolidBrush(
                    If(isCurrentFloor, Color.FromArgb(25, 25, 25), Color.FromArgb(80, 82, 90)))
                    Dim ns As SizeF = g.MeasureString(f.ToString(), bigFont)
                    g.DrawString(f.ToString(), bigFont, nb, numCX - ns.Width / 2, numCY - 10)
                End Using
            End Using

            ' ── Wall painting ──
            Dim paintX As Integer = 72
            Dim paintY As Integer = fy + CInt(fh * 0.15)
            Dim paintW As Integer = CInt(fh * 0.48)
            Dim paintH As Integer = CInt(fh * 0.62)
            DrawWallPainting(g, paintX, paintY, paintW, paintH, f)

            ' ── Floor door ──
            Dim doorW As Integer = CInt(W * 0.14)
            Dim doorH As Integer = CInt(fh * 0.72)
            Dim doorX As Integer = shaftX - doorW - 10
            Dim doorY As Integer = fy + fh - doorH - 2
            DrawFloorDoor(g, doorX, doorY, doorW, doorH)

            ' ── Indicator light above door ──
            Dim indCX As Integer = doorX + doorW \ 2
            Dim indY As Integer = doorY - 16
            DrawIndicatorLight(g, indCX, indY, f)

            ' ── Hall button panel RIGHT side ──
            Dim btnPX As Integer = shaftX + shaftW + 14
            Dim btnPY As Integer = fy + fh \ 2 - 24
            DrawHallButtonPanel(g, btnPX, btnPY, f)
            DrawRightWallDecorations(g, shaftX, shaftW, W, fy, fh, f)
        Next

        ' ── Shaft background ──
        Using shaftBg As New LinearGradientBrush(
            New Rectangle(shaftX, 0, shaftW, H),
            Color.FromArgb(55, 58, 68),
            Color.FromArgb(38, 40, 50),
            LinearGradientMode.Horizontal)
            g.FillRectangle(shaftBg, shaftX, 0, shaftW, H)
        End Using

        ' Shaft edge shadows
        Using ls As New LinearGradientBrush(New Rectangle(shaftX, 0, 16, H),
            Color.FromArgb(130, 0, 0, 0), Color.Transparent, LinearGradientMode.Horizontal)
            g.FillRectangle(ls, shaftX, 0, 16, H)
        End Using
        Using rs As New LinearGradientBrush(New Rectangle(shaftX + shaftW - 16, 0, 16, H),
            Color.Transparent, Color.FromArgb(130, 0, 0, 0), LinearGradientMode.Horizontal)
            g.FillRectangle(rs, shaftX + shaftW - 16, 0, 16, H)
        End Using

        ' ── ELEVATOR ROPES ──
        Dim fh2 As Integer = H \ FLOOR_COUNT
        Dim cabinPad As Integer = 8
        Dim cabinLeft As Single = shaftX + cabinPad
        Dim cabinRight As Single = shaftX + shaftW - cabinPad
        Dim cabinTop As Single = cabinY + cabinPad

        ' Rope attachment points (top of cabin)
        Dim rope1X As Single = cabinLeft + (cabinRight - cabinLeft) * 0.25F
        Dim rope2X As Single = cabinLeft + (cabinRight - cabinLeft) * 0.75F

        ' Draw ropes from top of shaft to top of cabin
        Using ropePen As New Pen(Color.FromArgb(160, 155, 140), 2)
            ropePen.DashStyle = Drawing2D.DashStyle.Solid
            ' Rope 1
            g.DrawLine(ropePen, rope1X, 0, rope1X, cabinTop)
            ' Rope 2
            g.DrawLine(ropePen, rope2X, 0, rope2X, cabinTop)
        End Using

        ' Rope highlights (metallic shine)
        Using ropeShine As New Pen(Color.FromArgb(80, 220, 215, 200), 0.5)
            g.DrawLine(ropeShine, rope1X - 0.5F, 0, rope1X - 0.5F, cabinTop)
            g.DrawLine(ropeShine, rope2X - 0.5F, 0, rope2X - 0.5F, cabinTop)
        End Using

        ' Rope pulley wheel at top
        Dim pulleyY As Integer = 6
        For Each rx As Single In New Single() {rope1X, rope2X}
            Using pulleyBg As New SolidBrush(Color.FromArgb(80, 82, 92))
                g.FillEllipse(pulleyBg, rx - 7, pulleyY, 14, 14)
            End Using
            Using pulleyRim As New Pen(Color.FromArgb(110, 112, 125), 1.5)
                g.DrawEllipse(pulleyRim, rx - 7, pulleyY, 14, 14)
            End Using
            Using pulleyCenter As New SolidBrush(Color.FromArgb(50, 52, 62))
                g.FillEllipse(pulleyCenter, rx - 3, pulleyY + 4, 6, 6)
            End Using
        Next

        ' Rope clamp at cabin top
        Using clampBrush As New SolidBrush(Color.FromArgb(90, 92, 102))
            g.FillRectangle(clampBrush, rope1X - 4, cabinTop - 5, 8, 6)
            g.FillRectangle(clampBrush, rope2X - 4, cabinTop - 5, 8, 6)
        End Using

        ' Shaft rails
        Using railPen As New Pen(Color.FromArgb(30, 33, 43), 3)
            g.DrawLine(railPen, shaftX + 5, 0, shaftX + 5, H)
            g.DrawLine(railPen, shaftX + shaftW - 5, 0, shaftX + shaftW - 5, H)
        End Using

        ' Floor lines on shaft
        For f As Integer = 1 To FLOOR_COUNT - 1
            Dim fy As Integer = H - (f * fh2)
            Using fp As New Pen(Color.FromArgb(46, 50, 60), 1)
                g.DrawLine(fp, shaftX, fy, shaftX + shaftW, fy)
            End Using
        Next

        ' ── Draw cabin AFTER ropes so it's on top ──
        DrawCabin(g, shaftX, shaftW, fh2)
    End Sub

    ' ══════════════════════════════════════════════════════════
    ' REPLACE DrawRightWallDecorations in your FormElevator.vb
    ' Each floor gets a clock + CCTV camera
    ' Call inside picBuilding_Paint after DrawHallButtonPanel:
    '   DrawRightWallDecorations(g, shaftX, shaftW, W, fy, fh, f)
    ' ══════════════════════════════════════════════════════════

    Private Sub DrawRightWallDecorations(g As Graphics, shaftX As Integer,
                                          shaftW As Integer, W As Integer,
                                          fy As Integer, fh As Integer, floor As Integer)
        Dim rightStart As Integer = shaftX + shaftW + 60
        Dim rightEnd As Integer = W - 10
        Dim zoneW As Integer = rightEnd - rightStart
        Dim centerX As Integer = rightStart + zoneW \ 2

        ' ── CCTV Camera — top corner of each floor ──────────
        Dim camX As Integer = rightEnd - 30
        Dim camY As Integer = fy + 8
        DrawSecurityCamera(g, camX, camY)

        ' ── Wall Clock — center of each floor ───────────────
        Dim clockCX As Integer = centerX - 10
        Dim clockCY As Integer = fy + fh \ 2
        Dim clockR As Integer = 20
        DrawWallClock(g, clockCX, clockCY, clockR)

        ' ── EXIT Sign — floor 1 and 3 only ──────────────────
        If floor = 1 OrElse floor = 3 Then
            Dim exitX As Integer = rightStart + 4
            Dim exitY As Integer = fy + 8
            DrawExitSign(g, exitX, exitY)
        End If
    End Sub

    ' ── Analog wall clock showing real time ─────────────────────
    Private Sub DrawWallClock(g As Graphics, cx As Integer, cy As Integer, r As Integer)
        ' Shadow
        Using shadowBrush As New SolidBrush(Color.FromArgb(40, 0, 0, 0))
            g.FillEllipse(shadowBrush, cx - r + 3, cy - r + 3, r * 2, r * 2)
        End Using

        ' Clock face
        Using faceBrush As New LinearGradientBrush(
            New Rectangle(cx - r, cy - r, r * 2, r * 2),
            Color.FromArgb(242, 238, 228),
            Color.FromArgb(210, 206, 194),
            LinearGradientMode.ForwardDiagonal)
            g.FillEllipse(faceBrush, cx - r, cy - r, r * 2, r * 2)
        End Using

        ' Outer rim
        Using rimPen As New Pen(Color.FromArgb(70, 65, 55), 3)
            g.DrawEllipse(rimPen, cx - r, cy - r, r * 2, r * 2)
        End Using

        ' Inner rim
        Using innerRim As New Pen(Color.FromArgb(145, 135, 115), 1)
            g.DrawEllipse(innerRim, cx - r + 3, cy - r + 3, (r - 3) * 2, (r - 3) * 2)
        End Using

        ' Hour markers
        For i As Integer = 0 To 11
            Dim angle As Double = (i * 30 - 90) * Math.PI / 180.0
            Dim isBig As Boolean = (i Mod 3 = 0)
            Dim innerDist As Double = If(isBig, r - 7, r - 5)
            Dim outerDist As Double = r - 3
            Dim x1 As Single = CSng(cx + Math.Cos(angle) * innerDist)
            Dim y1 As Single = CSng(cy + Math.Sin(angle) * innerDist)
            Dim x2 As Single = CSng(cx + Math.Cos(angle) * outerDist)
            Dim y2 As Single = CSng(cy + Math.Sin(angle) * outerDist)
            Using tickPen As New Pen(Color.FromArgb(55, 50, 42), If(isBig, 2.0F, 0.8F))
                g.DrawLine(tickPen, x1, y1, x2, y2)
            End Using
        Next

        ' Real time
        Dim now As DateTime = DateTime.Now
        Dim hrs As Integer = now.Hour Mod 12
        Dim mins As Integer = now.Minute
        Dim secs As Integer = now.Second

        ' Hour hand
        Dim hAngle As Double = ((hrs * 60 + mins) / 720.0 * 360 - 90) * Math.PI / 180.0
        Using hp As New Pen(Color.FromArgb(32, 28, 22), 2.8)
            hp.StartCap = Drawing2D.LineCap.Round
            hp.EndCap = Drawing2D.LineCap.Round
            g.DrawLine(hp, cx, cy,
                CSng(cx + Math.Cos(hAngle) * (r * 0.52)),
                CSng(cy + Math.Sin(hAngle) * (r * 0.52)))
        End Using

        ' Minute hand
        Dim mAngle As Double = (mins / 60.0 * 360 - 90) * Math.PI / 180.0
        Using mp As New Pen(Color.FromArgb(32, 28, 22), 1.8)
            mp.StartCap = Drawing2D.LineCap.Round
            mp.EndCap = Drawing2D.LineCap.Round
            g.DrawLine(mp, cx, cy,
                CSng(cx + Math.Cos(mAngle) * (r * 0.76)),
                CSng(cy + Math.Sin(mAngle) * (r * 0.76)))
        End Using

        ' Second hand (red)
        Dim sAngle As Double = (secs / 60.0 * 360 - 90) * Math.PI / 180.0
        Dim sxFwd As Single = CSng(cx + Math.Cos(sAngle) * (r * 0.84))
        Dim syFwd As Single = CSng(cy + Math.Sin(sAngle) * (r * 0.84))
        Dim sxBack As Single = CSng(cx - Math.Cos(sAngle) * (r * 0.22))
        Dim syBack As Single = CSng(cy - Math.Sin(sAngle) * (r * 0.22))
        Using sp As New Pen(Color.FromArgb(210, 38, 28), 1.0)
            sp.StartCap = Drawing2D.LineCap.Round
            sp.EndCap = Drawing2D.LineCap.Round
            g.DrawLine(sp, sxBack, syBack, sxFwd, syFwd)
        End Using

        ' Center cap
        Using capBrush As New SolidBrush(Color.FromArgb(50, 45, 38))
            g.FillEllipse(capBrush, cx - 3, cy - 3, 6, 6)
        End Using
        Using capRed As New SolidBrush(Color.FromArgb(210, 38, 28))
            g.FillEllipse(capRed, cx - 2, cy - 2, 4, 4)
        End Using

        ' Digital time below clock
        Using timeFont As New Font("Segoe UI", 5.5, FontStyle.Bold)
            Using timeBrush As New SolidBrush(Color.FromArgb(65, 60, 50))
                Dim timeStr As String = now.ToString("HH:mm:ss")
                Dim tsz As SizeF = g.MeasureString(timeStr, timeFont)
                g.DrawString(timeStr, timeFont, timeBrush,
                    cx - tsz.Width / 2, cy + r + 3)
            End Using
        End Using

        ' Clock label above
        Using lblFont As New Font("Segoe UI", 5, FontStyle.Regular)
            Using lblBrush As New SolidBrush(Color.FromArgb(100, 95, 82))
                Dim lbl As String = "LOCAL TIME"
                Dim lsz As SizeF = g.MeasureString(lbl, lblFont)
                g.DrawString(lbl, lblFont, lblBrush,
                    cx - lsz.Width / 2, cy - r - 12)
            End Using
        End Using
    End Sub

    ' ── Security camera with blinking REC ───────────────────────
    Private Sub DrawSecurityCamera(g As Graphics, x As Integer, y As Integer)
        ' Wall mount base plate
        Using baseBrush As New SolidBrush(Color.FromArgb(42, 44, 54))
            g.FillRectangle(baseBrush, x + 5, y - 3, 14, 6)
        End Using
        Using basePen As New Pen(Color.FromArgb(28, 30, 38), 1)
            g.DrawRectangle(basePen, x + 5, y - 3, 14, 6)
        End Using

        ' Mount arm
        Using armBrush As New SolidBrush(Color.FromArgb(48, 50, 60))
            g.FillRectangle(armBrush, x + 10, y + 2, 5, 9)
        End Using

        ' Camera body
        Using camBrush As New LinearGradientBrush(
            New Rectangle(x, y + 10, 30, 15),
            Color.FromArgb(58, 60, 72),
            Color.FromArgb(38, 40, 50),
            LinearGradientMode.Vertical)
            g.FillRectangle(camBrush, x, y + 10, 30, 15)
        End Using

        ' Camera body border
        Using camPen As New Pen(Color.FromArgb(25, 27, 36), 1)
            g.DrawRectangle(camPen, x, y + 10, 30, 15)
        End Using

        ' Body sheen
        Using sheenBrush As New LinearGradientBrush(
            New Rectangle(x, y + 10, 30, 6),
            Color.FromArgb(25, 255, 255, 255), Color.Transparent,
            LinearGradientMode.Vertical)
            g.FillRectangle(sheenBrush, x, y + 10, 30, 6)
        End Using

        ' Lens housing ring
        Using lensRingBrush As New SolidBrush(Color.FromArgb(28, 30, 38))
            g.FillEllipse(lensRingBrush, x + 2, y + 12, 14, 11)
        End Using
        Using lensRing As New Pen(Color.FromArgb(65, 68, 80), 1.5)
            g.DrawEllipse(lensRing, x + 2, y + 12, 14, 11)
        End Using

        ' Lens glass
        Using lensBrush As New SolidBrush(Color.FromArgb(25, 30, 55))
            g.FillEllipse(lensBrush, x + 4, y + 13, 10, 9)
        End Using
        Using lensGlass As New Pen(Color.FromArgb(55, 58, 75), 1)
            g.DrawEllipse(lensGlass, x + 4, y + 13, 10, 9)
        End Using

        ' Lens reflection
        Using reflBrush As New SolidBrush(Color.FromArgb(70, 160, 190, 255))
            g.FillEllipse(reflBrush, x + 5, y + 14, 4, 3)
        End Using

        ' IR LEDs around lens
        Using irBrush As New SolidBrush(Color.FromArgb(35, 160, 130, 255))
            g.FillEllipse(irBrush, x + 16, y + 12, 3, 3)
            g.FillEllipse(irBrush, x + 16, y + 19, 3, 3)
            g.FillEllipse(irBrush, x + 2, y + 12, 3, 3)
            g.FillEllipse(irBrush, x + 2, y + 19, 3, 3)
        End Using

        ' REC indicator LED (blinking)
        Dim recAlpha As Integer = If(indicatorBlink, 230, 50)
        Using recGlow As New SolidBrush(Color.FromArgb(If(indicatorBlink, 70, 0), 255, 50, 35))
            g.FillEllipse(recGlow, x + 24, y + 10, 9, 9)
        End Using
        Using recDot As New SolidBrush(Color.FromArgb(recAlpha, 230, 45, 30))
            g.FillEllipse(recDot, x + 25, y + 12, 6, 6)
        End Using
        Using recRing As New Pen(Color.FromArgb(80, 45, 35), 0.8)
            g.DrawEllipse(recRing, x + 25, y + 12, 6, 6)
        End Using

        ' REC text
        Using recFont As New Font("Segoe UI", 4.5, FontStyle.Bold)
            Using recBrush As New SolidBrush(Color.FromArgb(recAlpha, 225, 42, 30))
                g.DrawString("REC", recFont, recBrush, x + 20, y + 24)
            End Using
        End Using

        ' CCTV label
        Using lblFont As New Font("Segoe UI", 4.5, FontStyle.Regular)
            Using lblBrush As New SolidBrush(Color.FromArgb(85, 88, 100))
                g.DrawString("CCTV", lblFont, lblBrush, x + 2, y + 26)
            End Using
        End Using
    End Sub

    ' ── EXIT sign ───────────────────────────────────────────────
    Private Sub DrawExitSign(g As Graphics, x As Integer, y As Integer)
        ' Housing
        Using housingBrush As New SolidBrush(Color.FromArgb(32, 35, 42))
            g.FillRectangle(housingBrush, x - 3, y - 3, 56, 24)
        End Using

        ' Green background with pulse glow
        Dim gAlpha As Integer = CInt(55 + glowPulse * 80)
        Using glowBrush As New SolidBrush(Color.FromArgb(gAlpha, 35, 200, 85))
            g.FillRectangle(glowBrush, x - 5, y - 5, 60, 28)
        End Using
        Using greenBg As New SolidBrush(Color.FromArgb(18, 155, 58))
            g.FillRectangle(greenBg, x, y, 50, 18)
        End Using

        ' EXIT text
        Using exitFont As New Font("Segoe UI", 7, FontStyle.Bold)
            Using exitBrush As New SolidBrush(Color.White)
                g.DrawString("EXIT", exitFont, exitBrush, x + 3, y + 2)
            End Using
        End Using

        ' Running man stick figure
        Using manPen As New Pen(Color.White, 1.2)
            manPen.StartCap = Drawing2D.LineCap.Round
            manPen.EndCap = Drawing2D.LineCap.Round
            ' Head
            Using manBrush As New SolidBrush(Color.White)
                g.FillEllipse(manBrush, x + 36, y + 2, 5, 5)
            End Using
            ' Body
            g.DrawLine(manPen, x + 38, y + 7, x + 37, y + 13)
            ' Arms
            g.DrawLine(manPen, x + 34, y + 8, x + 41, y + 10)
            ' Legs
            g.DrawLine(manPen, x + 37, y + 13, x + 34, y + 17)
            g.DrawLine(manPen, x + 37, y + 13, x + 41, y + 17)
        End Using

        ' Arrow
        Using arrowPen As New Pen(Color.White, 1.5)
            arrowPen.StartCap = Drawing2D.LineCap.Round
            arrowPen.EndCap = Drawing2D.LineCap.ArrowAnchor
            g.DrawLine(arrowPen, x + 43, y + 9, x + 49, y + 9)
        End Using

        ' Sign border
        Using signBorder As New Pen(Color.FromArgb(12, 115, 42), 1)
            g.DrawRectangle(signBorder, x, y, 50, 18)
        End Using

        ' Screws
        Using screwBrush As New SolidBrush(Color.FromArgb(95, 100, 112))
            g.FillEllipse(screwBrush, x - 2, y - 2, 4, 4)
            g.FillEllipse(screwBrush, x + 48, y - 2, 4, 4)
        End Using
    End Sub
    Private Sub picBuilding_MouseClick(sender As Object, e As MouseEventArgs) Handles picBuilding.MouseClick
        Dim W As Integer = picBuilding.Width
        Dim H As Integer = picBuilding.Height
        Dim fh As Integer = H \ FLOOR_COUNT
        Dim shaftW As Integer = CInt(W * 0.28)
        Dim shaftX As Integer = W \ 2 - shaftW \ 2
        Dim btnPanelW As Integer = 36

        For floor As Integer = 1 To FLOOR_COUNT
            Dim fy As Integer = H - (floor * fh)
            Dim btnPX As Integer = shaftX + shaftW + 14
            Dim btnPY As Integer = fy + fh \ 2 - 24
            Dim hasUp As Boolean = (floor < FLOOR_COUNT)
            Dim hasDown As Boolean = (floor > 1)

            ' Check UP button click
            If hasUp Then
                Dim upRect As New Rectangle(btnPX + 5, btnPY + 5, btnPanelW - 10, 22)
                If upRect.Contains(e.Location) Then
                    hallUpLit(floor) = True
                    RequestFloor(floor)
                    picBuilding.Invalidate()
                    Return
                End If
            End If

            ' Check DOWN button click
            If hasDown Then
                Dim downBtnY As Integer = If(hasUp, btnPY + 34, btnPY + 5)
                Dim downRect As New Rectangle(btnPX + 5, downBtnY, btnPanelW - 10, 22)
                If downRect.Contains(e.Location) Then
                    hallDownLit(floor) = True
                    RequestFloor(floor)
                    picBuilding.Invalidate()
                    Return
                End If
            End If
        Next
    End Sub

    Private Sub DrawFloorDoor(g As Graphics, x As Integer, y As Integer, w As Integer, h As Integer)
        ' Door surround / architrave
        Using surroundBrush As New LinearGradientBrush(
            New Rectangle(x - 6, y - 4, w + 12, h + 4),
            Color.FromArgb(72, 68, 62),
            Color.FromArgb(52, 48, 42),
            LinearGradientMode.Horizontal)
            g.FillRectangle(surroundBrush, x - 6, y - 4, w + 12, h + 4)
        End Using
        Using surroundPen As New Pen(Color.FromArgb(40, 36, 30), 1)
            g.DrawRectangle(surroundPen, x - 6, y - 4, w + 12, h + 4)
        End Using

        ' Two door panels
        Dim halfW As Integer = w \ 2 - 1
        DrawWoodenDoorPanel(g, x, y, halfW, h, False)
        DrawWoodenDoorPanel(g, x + halfW + 2, y, halfW, h, True)

        ' Center gap
        Using gapBrush As New SolidBrush(Color.FromArgb(28, 24, 18))
            g.FillRectangle(gapBrush, x + halfW - 1, y, 4, h)
        End Using
    End Sub
    Private Sub DrawWallPainting(g As Graphics, x As Integer, y As Integer,
                                  w As Integer, h As Integer, floor As Integer)
        If w <= 0 OrElse h <= 0 Then Return

        ' Outer wood frame
        Using outerFrame As New SolidBrush(Color.FromArgb(55, 42, 28))
            g.FillRectangle(outerFrame, x - 6, y - 6, w + 12, h + 12)
        End Using

        ' Gold trim
        Using goldFrame As New SolidBrush(Color.FromArgb(160, 130, 55))
            g.FillRectangle(goldFrame, x - 3, y - 3, w + 6, h + 6)
        End Using

        ' Canvas
        Select Case floor
            Case 1  ' Lobby — warm sunset landscape
                ' Sky gradient
                Using skyBrush As New LinearGradientBrush(
                    New Rectangle(x, y, w, h \ 2 + 4),
                    Color.FromArgb(220, 140, 80),
                    Color.FromArgb(180, 100, 50),
                    LinearGradientMode.Vertical)
                    g.FillRectangle(skyBrush, x, y, w, h \ 2 + 4)
                End Using
                ' Ground
                Using groundBrush As New SolidBrush(Color.FromArgb(60, 90, 45))
                    g.FillRectangle(groundBrush, x, y + h \ 2, w, h \ 2)
                End Using
                ' Sun
                Using sunBrush As New SolidBrush(Color.FromArgb(255, 220, 80))
                    g.FillEllipse(sunBrush, x + w \ 2 - 8, y + 6, 16, 16)
                End Using
                ' Sun glow
                Using sunGlow As New SolidBrush(Color.FromArgb(60, 255, 200, 50))
                    g.FillEllipse(sunGlow, x + w \ 2 - 14, y + 2, 28, 24)
                End Using
                ' Mountains left
                Using mBrush As New SolidBrush(Color.FromArgb(80, 70, 55))
                    g.FillPolygon(mBrush, New Point() {
                        New Point(x, y + h),
                        New Point(x + w \ 4, y + h \ 2 + 6),
                        New Point(x + w \ 2 - 2, y + h)})
                End Using
                ' Mountains right
                Using mBrush2 As New SolidBrush(Color.FromArgb(95, 82, 62))
                    g.FillPolygon(mBrush2, New Point() {
                        New Point(x + w \ 3, y + h),
                        New Point(x + CInt(w * 0.65), y + h \ 2 + 4),
                        New Point(x + w, y + h)})
                End Using
                ' River/reflection
                Using riverBrush As New SolidBrush(Color.FromArgb(180, 140, 80))
                    g.FillRectangle(riverBrush, x + w \ 3, y + h \ 2, w \ 3, 4)
                End Using

            Case 2  ' Office — abstract geometric
                ' Dark background
                Using bgBrush As New SolidBrush(Color.FromArgb(30, 45, 90))
                    g.FillRectangle(bgBrush, x, y, w, h)
                End Using
                ' Geometric shapes
                Using sb1 As New SolidBrush(Color.FromArgb(80, 120, 200))
                    g.FillRectangle(sb1, x + 4, y + 4, w \ 3, h \ 3)
                End Using
                Using sb2 As New SolidBrush(Color.FromArgb(60, 180, 160))
                    g.FillRectangle(sb2, x + w \ 2, y + h \ 3, w \ 3, h \ 2)
                End Using
                Using sb3 As New SolidBrush(Color.FromArgb(140, 80, 180))
                    g.FillRectangle(sb3, x + w \ 4, y + CInt(h * 0.55), w \ 3, h \ 3)
                End Using
                Using sb4 As New SolidBrush(Color.FromArgb(200, 160, 40))
                    g.FillEllipse(sb4, x + CInt(w * 0.55), y + 5, w \ 4, w \ 4)
                End Using
                ' Grid lines
                Using linePen As New Pen(Color.FromArgb(60, 200, 210, 240), 0.8)
                    For i As Integer = 0 To 3
                        g.DrawLine(linePen, x, y + (h \ 4) * i, x + w, y + (h \ 4) * i)
                        g.DrawLine(linePen, x + (w \ 4) * i, y, x + (w \ 4) * i, y + h)
                    Next
                End Using

            Case 3  ' Lounge — ocean scene
                ' Sky
                Using skyBrush As New SolidBrush(Color.FromArgb(100, 160, 220))
                    g.FillRectangle(skyBrush, x, y, w, h \ 3)
                End Using
                ' Ocean gradient
                Using seaBrush As New LinearGradientBrush(
                    New Rectangle(x, y + h \ 3, w, h - h \ 3),
                    Color.FromArgb(40, 100, 180),
                    Color.FromArgb(15, 45, 100),
                    LinearGradientMode.Vertical)
                    g.FillRectangle(seaBrush, x, y + h \ 3, w, h - h \ 3)
                End Using
                ' Horizon line
                Using horizPen As New Pen(Color.FromArgb(180, 210, 235), 1)
                    g.DrawLine(horizPen, x, y + h \ 3, x + w, y + h \ 3)
                End Using
                ' Waves
                Using wavePen As New Pen(Color.FromArgb(140, 180, 220, 255), 1.2)
                    For i As Integer = 0 To 4
                        Dim wy As Integer = y + h \ 3 + 4 + i * (h \ 8)
                        g.DrawArc(wavePen, x + 2, wy - 2, w \ 3 - 2, 6, 0, 180)
                        g.DrawArc(wavePen, x + w \ 3, wy - 2, w \ 3 - 2, 6, 0, 180)
                        g.DrawArc(wavePen, x + CInt(w * 0.66), wy - 2, CInt(w * 0.3), 6, 0, 180)
                    Next
                End Using
                ' Sailboat
                Using boatBrush As New SolidBrush(Color.FromArgb(220, 215, 200))
                    g.FillPolygon(boatBrush, New Point() {
                        New Point(x + w \ 2 - 8, y + h \ 3 + 2),
                        New Point(x + w \ 2 + 8, y + h \ 3 + 2),
                        New Point(x + w \ 2 + 6, y + h \ 3 + 8),
                        New Point(x + w \ 2 - 6, y + h \ 3 + 8)})
                End Using
                Using sailBrush As New SolidBrush(Color.FromArgb(240, 235, 220))
                    g.FillPolygon(sailBrush, New Point() {
                        New Point(x + w \ 2, y + h \ 3 - 12),
                        New Point(x + w \ 2, y + h \ 3 + 2),
                        New Point(x + w \ 2 + 10, y + h \ 3 + 2)})
                End Using

            Case 4  ' Rooftop — starry night cityscape
                ' Night sky
                Using nightBrush As New LinearGradientBrush(
                    New Rectangle(x, y, w, h),
                    Color.FromArgb(8, 10, 38),
                    Color.FromArgb(18, 22, 65),
                    LinearGradientMode.Vertical)
                    g.FillRectangle(nightBrush, x, y, w, h)
                End Using
                ' Stars
                Dim starData() As Point = {
                    New Point(x + 6, y + 4), New Point(x + 20, y + 10),
                    New Point(x + 35, y + 3), New Point(x + 14, y + 18),
                    New Point(x + 42, y + 8), New Point(x + 28, y + 22),
                    New Point(x + 8, y + 28), New Point(x + 50, y + 14),
                    New Point(x + 38, y + 30), New Point(x + 18, y + 35)
                }
                For Each sp As Point In starData
                    If sp.X < x + w AndAlso sp.Y < y + h Then
                        Dim starAlpha As Integer = rng.Next(140, 220)
                        Using starBrush As New SolidBrush(Color.FromArgb(starAlpha, 220, 225, 255))
                            g.FillEllipse(starBrush, sp.X, sp.Y, 2, 2)
                        End Using
                    End If
                Next
                ' Moon
                Using moonBrush As New SolidBrush(Color.FromArgb(240, 235, 200))
                    g.FillEllipse(moonBrush, x + w - 18, y + 4, 14, 14)
                End Using
                Using moonShadow As New SolidBrush(Color.FromArgb(8, 10, 38))
                    g.FillEllipse(moonShadow, x + w - 15, y + 3, 14, 14)
                End Using
                ' City silhouette
                Dim cityH As Integer = CInt(h * 0.45)
                Dim cityPts() As Point = {
                    New Point(x, y + h),
                    New Point(x, y + h - cityH + 20),
                    New Point(x + 8, y + h - cityH + 20),
                    New Point(x + 8, y + h - cityH),
                    New Point(x + 16, y + h - cityH),
                    New Point(x + 16, y + h - cityH + 12),
                    New Point(x + 26, y + h - cityH + 12),
                    New Point(x + 26, y + h - cityH - 8),
                    New Point(x + 34, y + h - cityH - 8),
                    New Point(x + 34, y + h - cityH + 5),
                    New Point(x + 44, y + h - cityH + 5),
                    New Point(x + 44, y + h - cityH + 18),
                    New Point(x + w, y + h - cityH + 18),
                    New Point(x + w, y + h)
                }
                Using cityBrush As New SolidBrush(Color.FromArgb(12, 15, 42))
                    g.FillPolygon(cityBrush, cityPts)
                End Using
                ' Window lights in buildings
                Using winBrush As New SolidBrush(Color.FromArgb(180, 255, 240, 150))
                    g.FillRectangle(winBrush, x + 10, y + h - cityH + 4, 3, 3)
                    g.FillRectangle(winBrush, x + 28, y + h - cityH - 4, 3, 3)
                    g.FillRectangle(winBrush, x + 36, y + h - cityH + 8, 3, 3)
                End Using
        End Select

        ' Frame border
        Using borderPen As New Pen(Color.FromArgb(110, 90, 45), 1.5)
            g.DrawRectangle(borderPen, x, y, w, h)
        End Using

        ' Glass sheen on painting
        Using sheenBrush As New LinearGradientBrush(
            New Rectangle(x, y, Math.Max(w \ 2, 1), Math.Max(h, 1)),
            Color.FromArgb(15, 255, 255, 255), Color.Transparent,
            LinearGradientMode.Horizontal)
            g.FillRectangle(sheenBrush, x, y, w \ 2, h)
        End Using
    End Sub

    Private Sub DrawWoodenDoorPanel(g As Graphics, x As Integer, y As Integer,
                                     w As Integer, h As Integer, isRight As Boolean)
        ' Wood base
        Using woodBrush As New LinearGradientBrush(
            New Rectangle(x, y, Math.Max(w, 1), Math.Max(h, 1)),
            Color.FromArgb(100, 72, 48),
            Color.FromArgb(70, 48, 30),
            LinearGradientMode.Horizontal)
            g.FillRectangle(woodBrush, x, y, w, h)
        End Using

        ' Wood grain
        Using grainPen As New Pen(Color.FromArgb(18, 75, 50, 28), 0.8)
            For i As Integer = 0 To h Step 10
                g.DrawLine(grainPen, x, y + i, x + w, y + i + rng.Next(-2, 3))
            Next
        End Using

        ' Upper recessed panel
        Dim inset As Integer = 5
        Dim upPanH As Integer = CInt(h * 0.38)
        Using upPanBg As New SolidBrush(Color.FromArgb(20, 0, 0, 0))
            g.FillRectangle(upPanBg, x + inset, y + inset, w - inset * 2, upPanH)
        End Using
        Using upPanBorder As New Pen(Color.FromArgb(45, 30, 18), 1)
            g.DrawRectangle(upPanBorder, x + inset, y + inset, w - inset * 2, upPanH)
        End Using
        Using upPanHighlight As New Pen(Color.FromArgb(30, 120, 90, 55), 0.5)
            g.DrawLine(upPanHighlight, x + inset + 1, y + inset + 1, x + w - inset - 1, y + inset + 1)
            g.DrawLine(upPanHighlight, x + inset + 1, y + inset + 1, x + inset + 1, y + inset + upPanH - 1)
        End Using

        ' Lower recessed panel
        Dim loPanY As Integer = y + inset + upPanH + 8
        Dim loPanH As Integer = CInt(h * 0.38)
        Using loPanBg As New SolidBrush(Color.FromArgb(20, 0, 0, 0))
            g.FillRectangle(loPanBg, x + inset, loPanY, w - inset * 2, loPanH)
        End Using
        Using loPanBorder As New Pen(Color.FromArgb(45, 30, 18), 1)
            g.DrawRectangle(loPanBorder, x + inset, loPanY, w - inset * 2, loPanH)
        End Using
        Using loPanHighlight As New Pen(Color.FromArgb(30, 120, 90, 55), 0.5)
            g.DrawLine(loPanHighlight, x + inset + 1, loPanY + 1, x + w - inset - 1, loPanY + 1)
            g.DrawLine(loPanHighlight, x + inset + 1, loPanY + 1, x + inset + 1, loPanY + loPanH - 1)
        End Using

        ' Slim door handle bar (center edge, no knob)
        Dim handleX As Integer = If(isRight, x + 3, x + w - 6)
        Dim handleY As Integer = y + CInt(h * 0.42)
        Dim handleH As Integer = CInt(h * 0.16)
        Using handleBg As New LinearGradientBrush(
            New Rectangle(handleX, handleY, 4, Math.Max(handleH, 1)),
            Color.FromArgb(190, 175, 140),
            Color.FromArgb(140, 128, 100),
            LinearGradientMode.Horizontal)
            g.FillRectangle(handleBg, handleX, handleY, 4, handleH)
        End Using
        Using handlePen As New Pen(Color.FromArgb(110, 100, 75), 0.5)
            g.DrawRectangle(handlePen, handleX, handleY, 4, handleH)
        End Using

        ' Sheen
        Using sheenBrush As New LinearGradientBrush(
            New Rectangle(x, y, Math.Max(w \ 3, 1), Math.Max(h, 1)),
            Color.FromArgb(25, 255, 255, 255), Color.Transparent,
            LinearGradientMode.Horizontal)
            g.FillRectangle(sheenBrush, x, y, w \ 3, h)
        End Using
    End Sub
    Private Sub DrawIndicatorLight(g As Graphics, cx As Integer, y As Integer, floor As Integer)
        Dim isHere As Boolean = (floor = currentFloor)

        ' Panel background
        Using panBrush As New SolidBrush(Color.FromArgb(38, 40, 50))
            g.FillRectangle(panBrush, cx - 16, y - 5, 32, 20)
        End Using
        Using panPen As New Pen(Color.FromArgb(28, 30, 40), 1)
            g.DrawRectangle(panPen, cx - 16, y - 5, 32, 20)
        End Using

        ' Light color
        Dim lightColor As Color
        If isHere Then
            lightColor = If(currentDirection = 1, Color.FromArgb(60, 220, 80),
                         If(currentDirection = -1, Color.FromArgb(80, 160, 255),
                         Color.FromArgb(255, 220, 60)))
        Else
            lightColor = Color.FromArgb(30, 32, 42)
        End If

        ' Outer glow
        If isHere Then
            Dim gAlpha As Integer = CInt((0.4 + glowPulse * 0.6) * 100)
            Using glowBrush As New SolidBrush(Color.FromArgb(gAlpha, lightColor))
                g.FillEllipse(glowBrush, cx - 12, y - 3, 24, 16)
            End Using
        End If

        ' Main light bulb
        Using lb As New SolidBrush(lightColor)
            g.FillEllipse(lb, cx - 6, y + 1, 12, 10)
        End Using

        ' Shine on bulb
        If isHere Then
            Using shineBrush As New SolidBrush(Color.FromArgb(80, 255, 255, 255))
                g.FillEllipse(shineBrush, cx - 3, y + 2, 4, 3)
            End Using
        End If

        ' Direction arrow inside light
        If isHere AndAlso isMoving Then
            Using arrowBrush As New SolidBrush(Color.FromArgb(200, 255, 255, 255))
                Dim pts() As Point
                If currentDirection = 1 Then
                    pts = New Point() {
                        New Point(cx, y + 1),
                        New Point(cx - 4, y + 8),
                        New Point(cx + 4, y + 8)}
                Else
                    pts = New Point() {
                        New Point(cx, y + 10),
                        New Point(cx - 4, y + 3),
                        New Point(cx + 4, y + 3)}
                End If
                g.FillPolygon(arrowBrush, pts)
            End Using
        End If
    End Sub

    Private Sub DrawHallButtonPanel(g As Graphics, x As Integer, y As Integer, floor As Integer)
        Dim hasUp As Boolean = (floor < FLOOR_COUNT)
        Dim hasDown As Boolean = (floor > 1)
        Dim panH As Integer = If(hasUp AndAlso hasDown, 62, 32)
        Dim panW As Integer = 36

        ' Panel plate (stainless steel look)
        Using plateBrush As New LinearGradientBrush(
            New Rectangle(x, y, panW, panH),
            Color.FromArgb(175, 178, 185),
            Color.FromArgb(140, 143, 150),
            LinearGradientMode.Vertical)
            g.FillRectangle(plateBrush, x, y, panW, panH)
        End Using
        Using platePen As New Pen(Color.FromArgb(110, 113, 122), 1)
            g.DrawRectangle(platePen, x, y, panW, panH)
        End Using
        ' Plate sheen
        Using sheen As New LinearGradientBrush(
            New Rectangle(x, y, panW, panH \ 2),
            Color.FromArgb(40, 255, 255, 255), Color.Transparent,
            LinearGradientMode.Vertical)
            g.FillRectangle(sheen, x, y, panW, panH \ 2)
        End Using

        ' ── UP button ──
        If hasUp Then
            Dim btnY As Integer = y + 5
            Dim isLit As Boolean = hallUpLit(floor)
            DrawEmbeddedButton(g, x + 5, btnY, panW - 10, 22, True, isLit)
        End If

        ' ── DOWN button ──
        If hasDown Then
            Dim btnY As Integer = If(hasUp, y + 34, y + 5)
            Dim isLit As Boolean = hallDownLit(floor)
            DrawEmbeddedButton(g, x + 5, btnY, panW - 10, 22, False, isLit)
        End If
    End Sub

    Private Sub DrawEmbeddedButton(g As Graphics, x As Integer, y As Integer,
                                    w As Integer, h As Integer,
                                    isUp As Boolean, isLit As Boolean)
        ' Button recess shadow
        Using shadowBrush As New SolidBrush(Color.FromArgb(60, 0, 0, 0))
            g.FillRectangle(shadowBrush, x + 1, y + 1, w, h)
        End Using

        ' Button body
        Dim btnColor As Color
        If isLit Then
            btnColor = If(isUp, Color.FromArgb(40, 200, 70), Color.FromArgb(50, 130, 220))
        Else
            btnColor = Color.FromArgb(52, 55, 65)
        End If

        Using btnBrush As New LinearGradientBrush(
            New Rectangle(x, y, Math.Max(w, 1), Math.Max(h, 1)),
            ControlPaint.Light(btnColor, 0.15),
            btnColor,
            LinearGradientMode.Vertical)
            g.FillRectangle(btnBrush, x, y, w, h)
        End Using

        ' Button border
        Using btnPen As New Pen(Color.FromArgb(35, 38, 48), 1)
            g.DrawRectangle(btnPen, x, y, w, h)
        End Using

        ' Glow when lit
        If isLit Then
            Dim glowColor As Color = If(isUp, Color.FromArgb(80, 60, 220, 80), Color.FromArgb(80, 60, 140, 255))
            Using glowBrush As New SolidBrush(glowColor)
                g.FillRectangle(glowBrush, x - 2, y - 2, w + 4, h + 4)
            End Using
            Using glowBrush2 As New SolidBrush(btnColor)
                g.FillRectangle(glowBrush2, x, y, w, h)
            End Using
        End If

        ' Arrow symbol
        Dim arrowColor As Color = If(isLit, Color.White, Color.FromArgb(180, 190, 210))
        Using arrowBrush As New SolidBrush(arrowColor)
            Dim cx As Integer = x + w \ 2
            Dim cy As Integer = y + h \ 2
            Dim pts() As Point
            If isUp Then
                pts = New Point() {
                    New Point(cx, y + 4),
                    New Point(cx - 6, y + h - 4),
                    New Point(cx + 6, y + h - 4)}
            Else
                pts = New Point() {
                    New Point(cx, y + h - 4),
                    New Point(cx - 6, y + 4),
                    New Point(cx + 6, y + 4)}
            End If
            g.FillPolygon(arrowBrush, pts)
        End Using

        ' Inner highlight
        If isLit Then
            Using highlightPen As New Pen(Color.FromArgb(60, 255, 255, 255), 0.5)
                g.DrawLine(highlightPen, x + 1, y + 1, x + w - 1, y + 1)
                g.DrawLine(highlightPen, x + 1, y + 1, x + 1, y + h - 1)
            End Using
        End If
    End Sub

    Private Sub DrawCabin(g As Graphics, shaftX As Integer, shaftW As Integer, fh As Integer)
        Dim cabinPad As Integer = 8
        Dim cabinRect As New RectangleF(
            shaftX + cabinPad,
            cabinY + cabinPad,
            shaftW - cabinPad * 2,
            fh - cabinPad * 2)

        If cabinRect.Width <= 0 OrElse cabinRect.Height <= 0 Then Return

        ' Cabin body — metallic steel
        Using cabinBrush As New LinearGradientBrush(
            cabinRect,
            Color.FromArgb(185, 190, 200),
            Color.FromArgb(140, 145, 155),
            LinearGradientMode.Horizontal)
            g.FillRectangle(cabinBrush, cabinRect)
        End Using

        ' Cabin top bar
        Using topBrush As New SolidBrush(Color.FromArgb(80, 85, 95))
            g.FillRectangle(topBrush, cabinRect.Left, cabinRect.Top, cabinRect.Width, 8)
        End Using

        ' Cabin bottom bar
        Using botBrush As New SolidBrush(Color.FromArgb(80, 85, 95))
            g.FillRectangle(botBrush, cabinRect.Left, cabinRect.Bottom - 6, cabinRect.Width, 6)
        End Using

        ' Door tracks
        Using trackBrush As New SolidBrush(Color.FromArgb(60, 65, 75))
            g.FillRectangle(trackBrush, cabinRect.Left, cabinRect.Top + 8, 4, cabinRect.Height - 14)
            g.FillRectangle(trackBrush, cabinRect.Right - 4, cabinRect.Top + 8, 4, cabinRect.Height - 14)
        End Using

        ' Animated door panels
        Dim halfW As Single = (cabinRect.Width - 8) / 2
        Dim openAmt As Single = doorAnim * halfW
        Dim leftW As Single = halfW - openAmt

        ' Left door panel
        If leftW > 0 Then
            Dim leftRect As New RectangleF(cabinRect.Left + 4, cabinRect.Top + 8, leftW, cabinRect.Height - 14)
            DrawMetallicDoorPanel(g, leftRect)
        End If

        ' Right door panel
        Dim rightX As Single = cabinRect.Left + 4 + halfW + openAmt
        Dim rightW As Single = cabinRect.Right - 4 - rightX
        If rightW > 0 Then
            Dim rightRect As New RectangleF(rightX, cabinRect.Top + 8, rightW, cabinRect.Height - 14)
            DrawMetallicDoorPanel(g, rightRect)
        End If

        ' Interior light when door open
        If doorAnim > 0.15F Then
            Dim interiorAlpha As Integer = CInt(doorAnim * 120)
            Using intBrush As New SolidBrush(Color.FromArgb(interiorAlpha, 240, 230, 200))
                g.FillRectangle(intBrush,
                    cabinRect.Left + 4 + leftW, cabinRect.Top + 8,
                    openAmt * 2, cabinRect.Height - 14)
            End Using
            ' Ceiling light glow
            Using lightBrush As New SolidBrush(Color.FromArgb(CInt(doorAnim * 150), 255, 240, 200))
                g.FillEllipse(lightBrush,
                    cabinRect.Left + cabinRect.Width / 2 - 20,
                    cabinRect.Top + 8, 40, 10)
            End Using

            ' Stick figures inside
            If passengerCount > 0 Then
                Dim shown As Integer = Math.Min(passengerCount, 3)
                Dim spacing As Single = (cabinRect.Width - 20) / CSng(shown + 1)
                For i As Integer = 1 To shown
                    Dim px As Single = cabinRect.Left + spacing * i
                    Dim py As Single = cabinRect.Bottom - 10
                    DrawPerson(g, px, py, CInt(doorAnim * 200))
                Next
            End If
        End If

        ' Floor number display on cabin
        Using numFont As New Font("Segoe UI", 12, FontStyle.Bold)
            Using numBrush As New SolidBrush(Color.FromArgb(40, 45, 55))
                Dim numStr As String = currentFloor.ToString()
                Dim sz As SizeF = g.MeasureString(numStr, numFont)
                g.DrawString(numStr, numFont, numBrush,
                    cabinRect.Left + (cabinRect.Width - sz.Width) / 2,
                    cabinRect.Top + 10)
            End Using
        End Using

        ' Cabin border
        Using cabinPen As New Pen(Color.FromArgb(70, 75, 85), 2)
            g.DrawRectangle(cabinPen, cabinRect.X, cabinRect.Y, cabinRect.Width, cabinRect.Height)
        End Using

        ' Cabin shadow below
        Using shadowBrush As New LinearGradientBrush(
            New RectangleF(cabinRect.Left, cabinRect.Bottom, cabinRect.Width, 8),
            Color.FromArgb(80, 0, 0, 0), Color.Transparent,
            LinearGradientMode.Vertical)
            g.FillRectangle(shadowBrush, cabinRect.Left, cabinRect.Bottom, cabinRect.Width, 8)
        End Using
    End Sub

    Private Sub DrawMetallicDoorPanel(g As Graphics, rect As RectangleF)
        If rect.Width <= 0 Then Return
        ' Steel gradient
        Using panBrush As New LinearGradientBrush(
            rect,
            Color.FromArgb(175, 180, 190),
            Color.FromArgb(145, 150, 162),
            LinearGradientMode.Horizontal)
            g.FillRectangle(panBrush, rect)
        End Using
        ' Vertical grooves
        Dim grooves As Integer = Math.Max(1, CInt(rect.Width / 10))
        Dim spacing As Single = rect.Width / (grooves + 1)
        Using groovePen As New Pen(Color.FromArgb(40, 100, 105, 115), 1)
            For i As Integer = 1 To grooves
                Dim lx As Single = rect.Left + spacing * i
                g.DrawLine(groovePen, lx, rect.Top + 4, lx, rect.Bottom - 4)
            Next
        End Using
        ' Sheen
        Using sheen As New LinearGradientBrush(
            New RectangleF(rect.Left, rect.Top, Math.Max(rect.Width * 0.4F, 1), rect.Height),
            Color.FromArgb(35, 255, 255, 255), Color.Transparent,
            LinearGradientMode.Horizontal)
            g.FillRectangle(sheen, rect.Left, rect.Top, rect.Width * 0.4F, rect.Height)
        End Using
    End Sub

    Private Sub DrawPerson(g As Graphics, px As Single, py As Single, alpha As Integer)
        If alpha <= 0 Then Return
        Using pb As New SolidBrush(Color.FromArgb(alpha, 60, 65, 80))
            g.FillEllipse(pb, px - 4, py - 22, 8, 8)
        End Using
        Using pp As New Pen(Color.FromArgb(alpha, 60, 65, 80), 2)
            g.DrawLine(pp, px, py - 14, px, py - 2)
            g.DrawLine(pp, px - 4, py - 10, px + 4, py - 10)
            g.DrawLine(pp, px, py - 2, px - 3, py + 6)
            g.DrawLine(pp, px, py - 2, px + 3, py + 6)
        End Using
    End Sub

    Private Sub btnStart_Click(s As Object, e As EventArgs) Handles btnStart.Click
        ' Reset emergency if active
        If emergencyStop Then
            emergencyStop = False
            btnEmergency.BackColor = Color.FromArgb(200, 30, 30)
            btnEmergency.Text = "⏹  STOP"
        End If
        ' Resume if paused
        isPaused = False
        btnStart.BackColor = Color.FromArgb(20, 160, 60)   ' highlight green when active
        btnPause.BackColor = Color.FromArgb(180, 140, 0)   ' reset pause color
        SetStatus("▶ System running.")
        Log("▶ System started / resumed.")
    End Sub

    Private Sub btnPause_Click(s As Object, e As EventArgs) Handles btnPause.Click
        If emergencyStop Then Return  ' can't pause if emergency
        isPaused = Not isPaused
        If isPaused Then
            btnPause.BackColor = Color.FromArgb(220, 100, 0)  ' brighter when paused
            btnStart.BackColor = Color.FromArgb(30, 100, 200) ' dim start
            SetStatus("⏸ Paused — elevator will stop at next floor.")
            Log("⏸ System paused.")
        Else
            btnPause.BackColor = Color.FromArgb(180, 140, 0)
            btnStart.BackColor = Color.FromArgb(20, 160, 60)
            SetStatus("▶ Resumed.")
            Log("▶ System resumed.")
        End If
    End Sub
    '  BUTTON HANDLERS
    ' Inside panel floor buttonsbtn
    Private Sub btnP1_Click(s As Object, e As EventArgs) Handles btnP1.Click
        RequestFloor(1)
        HighlightPanelBtn(1)
    End Sub
    Private Sub btnP2_Click(s As Object, e As EventArgs) Handles btnP2.Click
        RequestFloor(2)
        HighlightPanelBtn(2)
    End Sub
    Private Sub btnP3_Click(s As Object, e As EventArgs) Handles btnP3.Click
        RequestFloor(3)
        HighlightPanelBtn(3)
    End Sub
    Private Sub btnP4_Click(s As Object, e As EventArgs) Handles btnP4.Click
        RequestFloor(4)
        HighlightPanelBtn(4)
    End Sub

    ' Hall buttons
    Private Sub btnUp1_Click(s As Object, e As EventArgs)
        RequestFloor(1)
    End Sub
    Private Sub btnUp2_Click(s As Object, e As EventArgs)
        RequestFloor(2)
    End Sub
    Private Sub btnUp3_Click(s As Object, e As EventArgs)
        RequestFloor(3)
    End Sub
    Private Sub btnDown2_Click(s As Object, e As EventArgs)
        RequestFloor(2)
    End Sub
    Private Sub btnDown3_Click(s As Object, e As EventArgs)
        RequestFloor(3)
    End Sub
    Private Sub btnDown4_Click(s As Object, e As EventArgs)
        RequestFloor(4)
    End Sub

    ' Open / Close door
    Private Sub btnOpenDoor_Click(s As Object, e As EventArgs) Handles btnOpenDoor.Click
        If isMoving Then Log("⚠️ Cannot open while moving.") : Return
        If emergencyStop Then Return
        If Not doorOpen Then
            Dim t As New Thread(Sub() OpenDoorManual())
            t.IsBackground = True : t.Start()
        End If
    End Sub

    Private Sub btnCloseDoor_Click(s As Object, e As EventArgs) Handles btnCloseDoor.Click
        If isMoving Then Return
        If doorOpen Then
            doorAnimTarget = 0.0F
            doorOpen = False
            UpdateDoorLabel("CLOSED")
            Log("🔒 Door closed manually.")
        End If
    End Sub

    Private Sub btnEmergency_Click(s As Object, e As EventArgs) Handles btnEmergency.Click
        If Not emergencyStop Then
            emergencyStop = True
            targetQueue.Clear()
            currentDirection = 0
            Me.Invoke(Sub()
                          btnEmergency.BackColor = Color.FromArgb(160, 20, 20)
                          btnEmergency.Text = "🔓  RESET"
                          lblStatus.Text = "🚨 STOP"
                          lblStatus.ForeColor = Color.FromArgb(255, 60, 60)
                          lblStatus.ForeColor = Color.FromArgb(255, 60, 60)
                          lblStatus.Text = "🚨 EMERGENCY STOP"
                      End Sub)
            Log("🚨 EMERGENCY STOP!")
        Else
            emergencyStop = False
            Me.Invoke(Sub()
                          ' Reset button back to original red STOP color
                          btnEmergency.BackColor = Color.FromArgb(200, 30, 30)
                          btnEmergency.Text = "⏹  STOP"
                          ' Reset direction label
                          lblStatus.ForeColor = Color.FromArgb(60, 220, 100)
                          lblStatus.Text = "— IDLE"
                          ' Reset status label
                          lblStatus.ForeColor = Color.FromArgb(60, 220, 100)
                          lblStatus.Text = "✅ System reset."
                          ' Reset start/pause buttons too
                          btnStart.BackColor = Color.FromArgb(30, 100, 200)
                          btnPause.BackColor = Color.FromArgb(180, 140, 0)
                      End Sub)
            Log("🟢 Emergency reset. System ready.")
        End If
    End Sub

    ' Passenger
    Private Sub btnBoard_Click(s As Object, e As EventArgs) Handles btnBoard.Click
        Dim count As Integer = nudPassengers.Value
        For i = 1 To count
            Dim pw = rng.Next(45, 111)
            If totalWeight + pw > MAX_WEIGHT Then
                Log("🚫 Passenger refused — overweight!") : Continue For
            End If
            totalWeight += pw : passengerCount += 1
            Log("🧍 Passenger " & passengerCount & " boarded (" & pw & " kg)")
        Next
        UpdateWeightDisplay()
        picBuilding.Invalidate()
    End Sub

    Private Sub btnDisembark_Click(s As Object, e As EventArgs) Handles btnDisembark.Click
        If passengerCount = 0 Then Log("ℹ️ No passengers.") : Return
        Log("🚶 " & passengerCount & " exited at Floor " & currentFloor)
        passengerCount = 0 : totalWeight = 0
        UpdateWeightDisplay()
        picBuilding.Invalidate()
    End Sub
    '  ELEVATOR MOVEMENT LOGIC
    Private Sub RequestFloor(floor As Integer)
        If emergencyStop Then Log("⛔ Emergency stop active.") : Return
        If totalWeight >= MAX_WEIGHT Then Log("🚫 Overloaded!") : Return
        If floor = currentFloor AndAlso Not isMoving Then
            Log("ℹ️ Already at Floor " & floor) : Return
        End If
        If Not targetQueue.Contains(floor) Then
            targetQueue.Enqueue(floor)
            Log("🔘 Floor " & floor & " queued.")
        End If
        If Not isMoving Then
            Dim t As New Thread(AddressOf ProcessQueue)
            t.IsBackground = True : t.Start()
        End If
    End Sub

    Private Sub ProcessQueue()
        isMoving = True
        Do While targetQueue.Count > 0 AndAlso Not emergencyStop
            ' Wait if paused
            Do While isPaused AndAlso Not emergencyStop
                Thread.Sleep(100)
            Loop

            ' Scan algorithm
            Dim lst As New List(Of Integer)(targetQueue)
            Dim dir As Integer = If(lst(0) >= currentFloor, 1, -1)
            Dim inDir As List(Of Integer)
            Dim opp As List(Of Integer)
            If dir = 1 Then
                inDir = lst.Where(Function(f) f >= currentFloor).OrderBy(Function(f) f).ToList()
                opp = lst.Where(Function(f) f < currentFloor).OrderByDescending(Function(f) f).ToList()
            Else
                inDir = lst.Where(Function(f) f <= currentFloor).OrderByDescending(Function(f) f).ToList()
                opp = lst.Where(Function(f) f > currentFloor).OrderBy(Function(f) f).ToList()
            End If
            Dim ordered As New List(Of Integer)
            ordered.AddRange(inDir) : ordered.AddRange(opp)
            targetQueue.Clear()
            For Each f In ordered : targetQueue.Enqueue(f) : Next

            Dim target As Integer = targetQueue.Dequeue()

            Do While currentFloor <> target AndAlso Not emergencyStop
                Do While isPaused AndAlso Not emergencyStop
                    Thread.Sleep(100)
                Loop
                Dim d As Integer = If(target > currentFloor, 1, -1)
                currentDirection = d
                Me.Invoke(Sub()
                              SetStatus(If(d = 1, "▲ Moving Up", "▼ Moving Down") & " → Floor " & target)
                          End Sub)
                Thread.Sleep(TRAVEL_MS)
                If Not emergencyStop Then
                    currentFloor += d
                    Me.Invoke(Sub()
                                  UpdateAllDisplays()
                                  UpdateCabinTarget()
                              End Sub)
                    Log("🏢 Passing Floor " & currentFloor)
                    If targetQueue.Contains(currentFloor) Then
                        targetQueue = New Queue(Of Integer)(
                            targetQueue.Where(Function(f) f <> currentFloor))
                        Log("🛑 Pickup at Floor " & currentFloor & "!")
                        target = currentFloor
                    End If
                End If
            Loop
            If Not emergencyStop Then ArriveAtFloor()
        Loop
        isMoving = False
        currentDirection = 0
        Me.Invoke(Sub()
                      SetStatus("✅ Idle — Floor " & currentFloor)
                      ResetPanelButtons()
                  End Sub)
    End Sub

    Private Sub ArriveAtFloor()
        arrivalFlash = 1.0F
        PlayDing()
        hallUpLit(currentFloor) = False
        hallDownLit(currentFloor) = False
        picBuilding.Invalidate()

        ' Brief pause before doors open (realistic delay)
        SetStatus("🏢 Arrived at Floor " & currentFloor & " — standby...")
        Thread.Sleep(600)

        ' Open door
        doorAnimTarget = 1.0F
        doorOpen = True
        UpdateDoorLabel("OPENING...")
        Thread.Sleep(DOOR_MS)
        UpdateDoorLabel("OPEN")
        Log("✅ Arrived at Floor " & currentFloor)

        ' Auto disembark
        Me.Invoke(Sub() AutoDisembark())
        Me.Invoke(Sub() ResetPanelButton(currentFloor))

        ' Wait with doors open
        Dim waited As Integer = 0
        Do While (doorHeld OrElse waited < 2500) AndAlso Not emergencyStop
            Thread.Sleep(100)
            If Not doorHeld Then waited += 100
        Loop

        ' Close door
        doorAnimTarget = 0.0F
        doorOpen = False
        UpdateDoorLabel("CLOSING...")
        Thread.Sleep(DOOR_MS)
        UpdateDoorLabel("CLOSED")
        Log("🔒 Door closed at Floor " & currentFloor)
    End Sub

    Private Sub AutoDisembark()
        If passengerCount = 0 Then Return
        Dim exiting As Integer = rng.Next(1, passengerCount + 1)
        Dim removed As Integer = 0
        For i As Integer = 1 To exiting
            If passengerCount > 0 Then
                Dim wt As Integer = Math.Min(rng.Next(45, 111), totalWeight)
                totalWeight = Math.Max(0, totalWeight - wt)
                removed += wt : passengerCount -= 1
            End If
        Next
        Log("🚶 " & exiting & " exited (-" & removed & " kg) | " & passengerCount & " left")
        UpdateWeightDisplay()
        picBuilding.Invalidate()
    End Sub

    Private Sub OpenDoorManual()
        doorOpen = True
        doorAnimTarget = 1.0F
        UpdateDoorLabel("OPENING...")
        Thread.Sleep(DOOR_MS)
        UpdateDoorLabel("OPEN")
        Log("🚪 Door opened at Floor " & currentFloor)
        Thread.Sleep(3000)
        If Not doorHeld Then
            doorAnimTarget = 0.0F
            doorOpen = False
            UpdateDoorLabel("CLOSED")
            Log("🔒 Door closed at Floor " & currentFloor)
        End If
    End Sub
    '  HELPERS
    Private Sub UpdateCabinTarget()
        Dim fh As Single = picBuilding.Height / CSng(FLOOR_COUNT)
        cabinYTarget = picBuilding.Height - (currentFloor * fh)
        ' Sync shaft cabin too
    End Sub

    Private Sub UpdateAllDisplays()
        lblCurrentFloor.Text = currentFloor.ToString()
        UpdateCabinTarget()
        HighlightPanelBtn(0)  ' clear all
        picBuilding.Invalidate()
    End Sub
    Private Sub UpdateDoorLabel(txt As String)
        Me.Invoke(Sub() lblDoorStatus.Text = txt)
    End Sub

    Private Sub UpdateWeightDisplay()
        Dim pct As Double = totalWeight / MAX_WEIGHT
        lblCapacityBar.Width = CInt(Math.Max(0, Math.Min(pct, 1.0) * pnlCapacityBg.Width))
        lblCapacityBar.BackColor = If(pct < 0.6, Color.FromArgb(40, 200, 100),
                                   If(pct < 0.9, Color.FromArgb(220, 160, 20),
                                   Color.FromArgb(220, 50, 50)))
        lblWeightVal.Text = totalWeight & " / " & MAX_WEIGHT & " kg"
        lblPassengerCount.Text = passengerCount & " aboard"
    End Sub

    Private Sub HighlightPanelBtn(floor As Integer)
        Me.Invoke(Sub()
                      Dim btns() As Button = {btnP1, btnP2, btnP3, btnP4}
                      For i As Integer = 0 To 3
                          Dim f As Integer = i + 1
                          If f = floor Then
                              btns(i).BackColor = Color.FromArgb(200, 170, 0)
                              btns(i).FlatAppearance.BorderSize = 2
                              btns(i).FlatAppearance.BorderColor = Color.White
                          Else
                              btns(i).BackColor = Color.FromArgb(65, 70, 82)
                              btns(i).FlatAppearance.BorderSize = 0
                          End If
                      Next
                  End Sub)
    End Sub

    Private Sub ResetPanelButton(floor As Integer)
        Dim btns() As Button = {btnP1, btnP2, btnP3, btnP4}
        If floor >= 1 AndAlso floor <= 4 Then
            btns(floor - 1).BackColor = Color.FromArgb(65, 70, 82)
            btns(floor - 1).FlatAppearance.BorderSize = 0
        End If
    End Sub

    Private Sub ResetPanelButtons()
        Dim btns() As Button = {btnP1, btnP2, btnP3, btnP4}
        For Each b In btns
            b.BackColor = Color.FromArgb(65, 70, 82)
            b.FlatAppearance.BorderSize = 0
        Next
    End Sub

    Private Sub SetStatus(msg As String)
        Me.Invoke(Sub()
                      If lblStatus IsNot Nothing Then lblStatus.Text = msg
                  End Sub)
    End Sub

    Private Sub Log(msg As String)
        Me.Invoke(Sub()
                      lstLog.Items.Insert(0, DateTime.Now.ToString("HH:mm:ss") & "  " & msg)
                      If lstLog.Items.Count > 100 Then lstLog.Items.RemoveAt(100)
                  End Sub)
    End Sub

    Private Sub FormElevator_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        UpdateCabinTarget()
        cabinY = cabinYTarget
        picBuilding.Invalidate()
    End Sub

    Private Sub PlayDing()
        Try
            Dim t As New Thread(Sub()
                                    Console.Beep(880, 120)
                                    Thread.Sleep(80)
                                    Console.Beep(1100, 200)
                                End Sub)
            t.IsBackground = True : t.Start()
        Catch : End Try
    End Sub

    Private Sub lblStatusTitle_Click(sender As Object, e As EventArgs) Handles lblStatusTitle.Click

    End Sub
End Class