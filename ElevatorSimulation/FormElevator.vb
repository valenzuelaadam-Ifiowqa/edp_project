' 3-Story Elevator Simulator — Visual Basic .NET

Imports System.Threading
Imports System.Drawing.Drawing2D
Imports System.Linq

Public Class FormElevator

    ' ── State ──────────────────────────────────────────────────
    Private currentFloor As Integer = 1
    Private targetQueue As New Queue(Of Integer)
    Private isMoving As Boolean = False
    Private emergencyStop As Boolean = False
    Private doorHeld As Boolean = False
    Private doorOpen As Boolean = False
    Private cabinY As Single = 0
    Private cabinTargetY As Single = 0

    ' ── Door animation ─────────────────────────────────────────
    Private doorAnim As Single = 0.0F
    Private doorAnimTarget As Single = 0.0F
    Private doorAnimSpeed As Single = 0.055F

    ' ── Glow / flash ───────────────────────────────────────────
    Private glowPulse As Single = 0.0F
    Private glowPulseDir As Single = 0.02F
    Private arrivalFlash As Single = 0.0F

    ' ── Animated background particles ──────────────────────────
    Private bgParticles() As PointF
    Private bgParticleSpeeds() As Single

    ' ── Window flicker state ───────────────────────────────────
    ' windowLit(floor, windowIndex) = brightness 0.0-1.0
    Private windowLit(3, 5) As Single
    Private windowFlickerTimer As Integer = 0

    ' ── Floor names & colors ────────────────────────────────────
    Private ReadOnly FloorNames() As String = {"", "🏛  Lobby", "💼  Office", "🌿  Rooftop"}
    Private ReadOnly FloorColors() As Color = {
        Color.Empty,
        Color.FromArgb(30, 120, 200),
        Color.FromArgb(30, 160, 120),
        Color.FromArgb(140, 80, 200)
    }

    ' ── Passenger system ───────────────────────────────────────
    Private totalWeight As Integer = 0
    Private passengerCount As Integer = 0
    Private Shared rng As New Random()
    Const MAX_WEIGHT As Integer = 500
    Const WARN_WEIGHT As Integer = 450
    Const TRAVEL_MS As Integer = 2000
    Const DOOR_MS As Integer = 1200

    ' ── Timer ──────────────────────────────────────────────────
    Private animTimer As New System.Windows.Forms.Timer()

    ' ── Sound ──────────────────────────────────────────────────
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
    Private Sub PlayDoorOpen()
        Try
            Dim t As New Thread(Sub()
                                    Console.Beep(600, 60)
                                    Thread.Sleep(40)
                                    Console.Beep(800, 80)
                                End Sub)
            t.IsBackground = True : t.Start()
        Catch : End Try
    End Sub
    Private Sub PlayDoorClose()
        Try
            Dim t As New Thread(Sub()
                                    Console.Beep(800, 60)
                                    Thread.Sleep(40)
                                    Console.Beep(500, 100)
                                End Sub)
            t.IsBackground = True : t.Start()
        Catch : End Try
    End Sub
    Private Sub PlayMovementTick()
        Try
            Dim t As New Thread(Sub() Console.Beep(200, 80))
            t.IsBackground = True : t.Start()
        Catch : End Try
    End Sub
    Private Sub PlayWarning()
        Try
            Dim t As New Thread(Sub()
                                    For i As Integer = 1 To 3
                                        Console.Beep(440, 100)
                                        Thread.Sleep(80)
                                    Next
                                End Sub)
            t.IsBackground = True : t.Start()
        Catch : End Try
    End Sub

    ' ── Form Load ──────────────────────────────────────────────
    Private Sub FormElevator_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "🛗  Elevator Simulator"
        Me.BackColor = Color.FromArgb(18, 18, 28)
        Me.ForeColor = Color.White
        Me.MinimumSize = New Size(1100, 660)
        Me.DoubleBuffered = True

        InitBackground()
        InitWindows()
        StyleControls()

        animTimer.Interval = 16
        AddHandler animTimer.Tick, AddressOf AnimTimer_Tick
        animTimer.Start()

        Dim floorH As Single = picShaft.Height / 3.0F
        cabinY = picShaft.Height - (1 * floorH)
        cabinTargetY = cabinY

        UpdateFloorDisplay()
        UpdateWeightDisplay()
        UpdateFloorButtons()
        LogEvent("🟢 System online. Elevator at " & FloorNames(1) & ".")
    End Sub

    Private Sub InitBackground()
        Dim count As Integer = 55
        ReDim bgParticles(count - 1)
        ReDim bgParticleSpeeds(count - 1)
        For i As Integer = 0 To count - 1
            bgParticles(i) = New PointF(rng.Next(0, Me.Width), rng.Next(0, Me.Height))
            bgParticleSpeeds(i) = CSng(rng.NextDouble() * 0.4 + 0.1)
        Next
    End Sub

    Private Sub InitWindows()
        ' Randomize starting window brightness per floor
        For f As Integer = 1 To 3
            For w As Integer = 0 To 3
                windowLit(f, w) = CSng(rng.NextDouble() * 0.5 + 0.1)
            Next
        Next
    End Sub

    ' ── Style controls ─────────────────────────────────────────
    Private Sub StyleControls()
        pnlCapacityBg.BackColor = Color.FromArgb(40, 40, 60)
        pnlCapacityBg.Height = 16
        lblCapacityBar.BackColor = Color.FromArgb(40, 200, 100)
        lblCapacityBar.Text = ""
        lblCapacityBar.Height = 16

        For Each lbl As Label In New Label() {lblCurrentFloor, lblStatus, lblWeightVal,
                                               lblPassengerCount, lblDoorStatus}
            lbl.ForeColor = Color.FromArgb(180, 210, 255)
            lbl.Font = New Font("Segoe UI", 9)
            lbl.BackColor = Color.Transparent
        Next
        lblCurrentFloor.Font = New Font("Segoe UI", 18, FontStyle.Bold)
        lblCurrentFloor.ForeColor = Color.FromArgb(100, 200, 255)
        lblStatus.Font = New Font("Segoe UI", 9, FontStyle.Italic)
        lblDoorStatus.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        lblDoorStatus.ForeColor = Color.FromArgb(255, 200, 80)
        lblDoorStatus.Text = "🚪 CLOSED"

        UpdateFloorButtons()

        StyleButton(btnEmergency, Color.FromArgb(200, 40, 40))
        StyleButton(btnHoldDoor, Color.FromArgb(180, 130, 20))
        StyleButton(btnAddPassengers, Color.FromArgb(30, 140, 80))
        StyleButton(btnClearPassengers, Color.FromArgb(80, 60, 130))
        btnAddPassengers.Text = "➕ Board Passengers"
        btnClearPassengers.Text = "🚶 Disembark All"

        For Each btn As Button In New Button() {btnEmergency, btnHoldDoor, btnAddPassengers, btnClearPassengers}
            btn.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        Next

        nudPassengers.BackColor = Color.FromArgb(30, 30, 50)
        nudPassengers.ForeColor = Color.FromArgb(180, 220, 255)
        nudPassengers.Font = New Font("Segoe UI", 9)
        nudPassengers.Minimum = 1 : nudPassengers.Maximum = 8 : nudPassengers.Value = 1

        lstLog.BackColor = Color.FromArgb(12, 12, 22)
        lstLog.ForeColor = Color.FromArgb(140, 220, 140)
        lstLog.Font = New Font("Consolas", 8.5)
        lstLog.BorderStyle = BorderStyle.None

        picShaft.BackColor = Color.FromArgb(22, 22, 38)
        picDoor.BackColor = Color.FromArgb(16, 16, 30)
        picBuilding.BackColor = Color.FromArgb(10, 10, 20)
    End Sub

    Private Sub UpdateFloorButtons()
        Dim floorBtns() As Button = {btnFloor1, btnFloor2, btnFloor3}
        For i As Integer = 0 To 2
            Dim f As Integer = i + 1
            StyleButton(floorBtns(i), FloorColors(f))
            floorBtns(i).Font = New Font("Segoe UI", 9, FontStyle.Bold)
            floorBtns(i).Size = New Size(130, 48)
            floorBtns(i).Text = FloorNames(f)
            floorBtns(i).FlatAppearance.BorderSize = If(f = currentFloor, 2, 0)
            If f = currentFloor Then
                floorBtns(i).FlatAppearance.BorderColor = Color.FromArgb(200, 230, 255)
            End If
        Next
    End Sub

    Private Sub StyleButton(btn As Button, baseColor As Color)
        btn.BackColor = baseColor
        btn.ForeColor = Color.White
        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderSize = 0
        btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(baseColor, 0.2)
        btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(baseColor, 0.2)
        btn.Cursor = Cursors.Hand
    End Sub

    ' ── Master animation tick ───────────────────────────────────
    Private Sub AnimTimer_Tick(sender As Object, e As EventArgs)
        Dim needsShaft As Boolean = False
        Dim needsDoor As Boolean = False
        Dim needsBuilding As Boolean = False

        ' Cabin lerp
        If Math.Abs(cabinY - cabinTargetY) > 0.5 Then
            cabinY += (cabinTargetY - cabinY) * 0.08F
            needsShaft = True
            needsBuilding = True
        End If

        ' Door anim lerp
        If Math.Abs(doorAnim - doorAnimTarget) > 0.005F Then
            doorAnim += (doorAnimTarget - doorAnim) * doorAnimSpeed
            doorAnim = Math.Max(0, Math.Min(1, doorAnim))
            needsDoor = True
            needsBuilding = True
        End If

        ' Glow pulse
        If Not isMoving Then
            glowPulse += glowPulseDir
            If glowPulse > 1.0F Then glowPulseDir = -0.02F
            If glowPulse < 0.0F Then glowPulseDir = 0.02F
            needsShaft = True
        End If

        ' Arrival flash fade
        If arrivalFlash > 0 Then
            arrivalFlash -= 0.04F
            If arrivalFlash < 0 Then arrivalFlash = 0
            needsDoor = True : needsShaft = True : needsBuilding = True
        End If

        ' Window flicker
        windowFlickerTimer += 1
        If windowFlickerTimer >= 8 Then
            windowFlickerTimer = 0
            For f As Integer = 1 To 3
                For w As Integer = 0 To 3
                    If rng.NextDouble() < 0.04 Then
                        windowLit(f, w) = CSng(rng.NextDouble() * 0.5 + 0.1)
                    End If
                Next
            Next
            needsBuilding = True
        End If

        ' Background particles
        If bgParticles IsNot Nothing Then
            For i As Integer = 0 To bgParticles.Length - 1
                bgParticles(i) = New PointF(bgParticles(i).X, bgParticles(i).Y - bgParticleSpeeds(i))
                If bgParticles(i).Y < -5 Then
                    bgParticles(i) = New PointF(rng.Next(0, Me.ClientSize.Width), Me.ClientSize.Height + 5)
                End If
            Next
        End If

        If needsShaft Then picShaft.Invalidate()
        If needsDoor Then picDoor.Invalidate()
        If needsBuilding Then picBuilding.Invalidate()
        Me.Invalidate(False)
    End Sub

    ' ── Background (form) ───────────────────────────────────────
    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        If bgParticles Is Nothing Then Return
        Dim g As Graphics = e.Graphics
        For i As Integer = 0 To bgParticles.Length - 1
            Dim alpha As Integer = rng.Next(15, 50)
            Dim sz As Single = CSng(rng.NextDouble() * 2 + 1)
            Using pb As New SolidBrush(Color.FromArgb(alpha, 80, 120, 200))
                g.FillEllipse(pb, bgParticles(i).X, bgParticles(i).Y, sz, sz)
            End Using
        Next
        Using gridPen As New Pen(Color.FromArgb(7, 60, 80, 140))
            For x As Integer = 0 To Me.Width Step 40
                g.DrawLine(gridPen, x, 0, x, Me.Height)
            Next
            For y As Integer = 0 To Me.Height Step 40
                g.DrawLine(gridPen, 0, y, Me.Width, y)
            Next
        End Using
    End Sub

    ' ── BUILDING PAINT ──────────────────────────────────────────
    Private Sub picBuilding_Paint(sender As Object, e As PaintEventArgs) Handles picBuilding.Paint
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.Clear(Color.FromArgb(10, 10, 20))

        Dim W As Integer = picBuilding.Width
        Dim H As Integer = picBuilding.Height

        ' ── Sky gradient ──
        Using skyBrush As New LinearGradientBrush(
            New Rectangle(0, 0, W, H),
            Color.FromArgb(8, 10, 25),
            Color.FromArgb(18, 20, 40),
            LinearGradientMode.Vertical)
            g.FillRectangle(skyBrush, 0, 0, W, H)
        End Using

        ' ── Stars ──
        Dim starPositions() As Point = {
            New Point(20, 18), New Point(55, 8), New Point(90, 22),
            New Point(130, 12), New Point(160, 28), New Point(195, 6),
            New Point(220, 18), New Point(250, 32), New Point(15, 45),
            New Point(175, 40), New Point(240, 10)
        }
        For Each sp As Point In starPositions
            Dim alpha As Integer = rng.Next(80, 200)
            Dim sz As Single = CSng(rng.NextDouble() * 1.5 + 0.5)
            Using sb As New SolidBrush(Color.FromArgb(alpha, 255, 255, 255))
                g.FillEllipse(sb, sp.X, sp.Y, sz, sz)
            End Using
        Next

        ' ── Moon ──
        Using moonBrush As New SolidBrush(Color.FromArgb(220, 255, 255, 200))
            g.FillEllipse(moonBrush, W - 40, 15, 22, 22)
        End Using
        Using moonShadow As New SolidBrush(Color.FromArgb(10, 10, 20))
            g.FillEllipse(moonShadow, W - 35, 12, 22, 22)
        End Using

        ' ── Ground ──
        Dim groundY As Integer = H - 30
        Using groundBrush As New LinearGradientBrush(
            New Rectangle(0, groundY, W, 30),
            Color.FromArgb(22, 25, 45),
            Color.FromArgb(12, 14, 28),
            LinearGradientMode.Vertical)
            g.FillRectangle(groundBrush, 0, groundY, W, 30)
        End Using
        Using groundLine As New Pen(Color.FromArgb(35, 45, 75), 1)
            g.DrawLine(groundLine, 0, groundY, W, groundY)
        End Using

        ' ── Building dimensions ──
        Dim bLeft As Integer = CInt(W * 0.12)
        Dim bRight As Integer = CInt(W * 0.88)
        Dim bWidth As Integer = bRight - bLeft
        Dim bTop As Integer = CInt(H * 0.06)
        Dim bBottom As Integer = groundY
        Dim bHeight As Integer = bBottom - bTop
        Dim floorH As Integer = bHeight \ 3

        ' ── Building body ──
        Using buildBrush As New LinearGradientBrush(
            New Rectangle(bLeft, bTop, bWidth, bHeight),
            Color.FromArgb(28, 32, 55),
            Color.FromArgb(18, 20, 38),
            LinearGradientMode.Vertical)
            g.FillRectangle(buildBrush, bLeft, bTop, bWidth, bHeight)
        End Using
        Using buildBorder As New Pen(Color.FromArgb(50, 65, 110), 2)
            g.DrawRectangle(buildBorder, bLeft, bTop, bWidth, bHeight)
        End Using

        ' ── Rooftop details ──
        g.FillRectangle(New SolidBrush(Color.FromArgb(35, 40, 65)), bLeft, bTop - 10, bWidth, 12)
        g.DrawRectangle(New Pen(Color.FromArgb(50, 65, 110), 1), bLeft, bTop - 10, bWidth, 12)
        ' Parapet bumps
        For x As Integer = bLeft To bRight - 14 Step 18
            g.FillRectangle(New SolidBrush(Color.FromArgb(40, 48, 75)), x, bTop - 18, 12, 10)
        Next
        ' Antenna
        Dim antX As Integer = bLeft + CInt(bWidth * 0.72)
        g.DrawLine(New Pen(Color.FromArgb(55, 70, 110), 1.5), antX, bTop - 10, antX, bTop - 36)
        Using redBlink As New SolidBrush(Color.FromArgb(200, 255, 60, 60))
            g.FillEllipse(redBlink, antX - 3, bTop - 39, 6, 6)
        End Using
        ' Crossbars
        g.DrawLine(New Pen(Color.FromArgb(45, 60, 95), 1), antX - 10, bTop - 28, antX + 10, bTop - 28)
        g.DrawLine(New Pen(Color.FromArgb(45, 60, 95), 1), antX - 7, bTop - 20, antX + 7, bTop - 20)

        ' Water tower
        Dim wtX As Integer = bLeft + CInt(bWidth * 0.18)
        g.FillRectangle(New SolidBrush(Color.FromArgb(30, 36, 58)), wtX - 14, bTop - 24, 28, 16)
        g.DrawRectangle(New Pen(Color.FromArgb(50, 65, 100), 1), wtX - 14, bTop - 24, 28, 16)
        g.DrawLine(New Pen(Color.FromArgb(45, 58, 95), 1), wtX - 14, bTop - 8, wtX - 6, bTop - 10)
        g.DrawLine(New Pen(Color.FromArgb(45, 58, 95), 1), wtX + 14, bTop - 8, wtX + 6, bTop - 10)

        ' ── Floor dividers ──
        For f As Integer = 1 To 2
            Dim fy As Integer = bBottom - (f * floorH)
            Using floorLine As New Pen(Color.FromArgb(38, 50, 85), 1.5)
                g.DrawLine(floorLine, bLeft, fy, bRight, fy)
            End Using
        Next

        ' ── Windows per floor ──
        For floor As Integer = 1 To 3
            Dim fy As Integer = bBottom - (floor * floorH)
            DrawFloorWindows(g, floor, bLeft, fy, bWidth, floorH)
        Next

        ' ── Shaft cut-out ──
        Dim shaftCX As Integer = bLeft + bWidth \ 2
        Dim shaftW As Integer = CInt(bWidth * 0.14)
        Dim shaftLeft As Integer = shaftCX - shaftW \ 2
        ' Shaft background
        Using shaftBg As New LinearGradientBrush(
            New Rectangle(shaftLeft, bTop, shaftW, bHeight),
            Color.FromArgb(8, 10, 20),
            Color.FromArgb(12, 15, 28),
            LinearGradientMode.Vertical)
            g.FillRectangle(shaftBg, shaftLeft, bTop, shaftW, bHeight)
        End Using
        Using shaftBorder As New Pen(Color.FromArgb(30, 50, 100), 2)
            g.DrawRectangle(shaftBorder, shaftLeft, bTop, shaftW, bHeight)
        End Using
        ' Rail lines inside shaft
        Using railPen As New Pen(Color.FromArgb(22, 35, 65), 1)
            g.DrawLine(railPen, shaftLeft + 4, bTop, shaftLeft + 4, bBottom)
            g.DrawLine(railPen, shaftLeft + shaftW - 4, bTop, shaftLeft + shaftW - 4, bBottom)
        End Using
        ' Floor markers on shaft
        For f As Integer = 1 To 2
            Dim fy As Integer = bBottom - (f * floorH)
            g.DrawLine(New Pen(Color.FromArgb(35, 55, 100), 1), shaftLeft, fy, shaftLeft + shaftW, fy)
        Next

        ' ── CABIN in building shaft ──
        ' Map cabinY (picShaft coords) to building shaft coords
        Dim shaftPixels As Single = picShaft.Height
        Dim buildShaftH As Single = bHeight
        Dim cabinRatio As Single = If(shaftPixels > 0, cabinY / shaftPixels, 0)
        Dim buildCabinY As Single = bTop + cabinRatio * buildShaftH
        Dim buildFloorH As Single = buildShaftH / 3.0F
        Dim buildCabinH As Single = buildFloorH - 6

        Dim cabinPad As Integer = 3
        Dim cabinRect As New RectangleF(
            shaftLeft + cabinPad,
            buildCabinY + cabinPad,
            shaftW - cabinPad * 2,
            buildCabinH - cabinPad * 2)

        ' Cabin color based on weight
        Dim pct As Double = totalWeight / MAX_WEIGHT
        Dim cc1 As Color = If(pct >= 1.0, Color.FromArgb(140, 30, 30),
                           If(pct >= 0.9, Color.FromArgb(140, 90, 15),
                           Color.FromArgb(30, 90, 180)))
        Dim cc2 As Color = If(pct >= 1.0, Color.FromArgb(90, 15, 15),
                           If(pct >= 0.9, Color.FromArgb(90, 55, 8),
                           Color.FromArgb(15, 55, 120)))

        ' Cabin glow
        If Not isMoving Then
            Dim gPulse As Integer = CInt(20 + glowPulse * 40)
            Dim gExpand As Single = glowPulse * 4
            Using gBrush As New SolidBrush(Color.FromArgb(gPulse, 60, 130, 255))
                g.FillRectangle(gBrush,
                    cabinRect.X - gExpand, cabinRect.Y - gExpand,
                    cabinRect.Width + gExpand * 2, cabinRect.Height + gExpand * 2)
            End Using
        End If

        ' Cabin body
        If cabinRect.Width > 0 AndAlso cabinRect.Height > 0 Then
            Using cabinBrush As New LinearGradientBrush(cabinRect, cc1, cc2, LinearGradientMode.Vertical)
                g.FillRectangle(cabinBrush, cabinRect)
            End Using
        End If

        ' Door animation on cabin
        Dim doorOpenAmt As Single = doorAnim
        Dim halfW As Single = cabinRect.Width / 2
        Dim leftDoorW As Single = halfW * (1 - doorOpenAmt)
        Dim rightDoorX As Single = cabinRect.Left + halfW + halfW * doorOpenAmt

        If leftDoorW > 0 Then
            Using doorBrush As New SolidBrush(Color.FromArgb(50, 75, 130))
                g.FillRectangle(doorBrush, cabinRect.Left, cabinRect.Top, leftDoorW, cabinRect.Height)
            End Using
        End If
        If cabinRect.Right - rightDoorX > 0 Then
            Using doorBrush As New SolidBrush(Color.FromArgb(50, 75, 130))
                g.FillRectangle(doorBrush, rightDoorX, cabinRect.Top, cabinRect.Right - rightDoorX, cabinRect.Height)
            End Using
        End If

        ' Cabin border glow
        Dim borderA As Integer = If(isMoving, 80, CInt(80 + glowPulse * 140))
        Using cabinBorder As New Pen(Color.FromArgb(borderA, 70, 150, 255), 1.5)
            g.DrawRectangle(cabinBorder, cabinRect.X, cabinRect.Y, cabinRect.Width, cabinRect.Height)
        End Using

        ' Interior glow when door open
        If doorAnim > 0.2F Then
            Dim ia As Integer = CInt(doorAnim * 60)
            Using intLight As New SolidBrush(Color.FromArgb(ia, 180, 210, 255))
                g.FillRectangle(intLight, cabinRect.Left, cabinRect.Top, cabinRect.Width, cabinRect.Height)
            End Using
        End If

        ' Stick figures inside cabin (small scale)
        If passengerCount > 0 AndAlso doorAnim < 0.8F Then
            Dim shown As Integer = Math.Min(passengerCount, 3)
            Dim spacing As Single = cabinRect.Width / (shown + 1)
            For i As Integer = 1 To shown
                Dim px As Single = cabinRect.Left + spacing * i
                Dim py As Single = cabinRect.Bottom - CInt(buildFloorH * 0.18)
                Dim figAlpha As Integer = CInt(160 * (1 - doorAnim))
                DrawSmallFigure(g, px, py, figAlpha)
            Next
        End If

        ' Arrival flash
        If arrivalFlash > 0 Then
            Using flashBrush As New SolidBrush(Color.FromArgb(CInt(arrivalFlash * 60), 200, 230, 255))
                g.FillRectangle(flashBrush, shaftLeft, bTop, shaftW, bHeight)
            End Using
        End If

        ' ── Floor name labels beside building ──
        Dim labelFont As New Font("Segoe UI", 7.5, FontStyle.Bold)
        For f As Integer = 1 To 3
            Dim fy As Integer = bBottom - (f * floorH) + floorH \ 2
            Dim nameColor As Color = If(f = currentFloor, FloorColors(f), Color.FromArgb(45, 60, 95))
            Using nameBrush As New SolidBrush(nameColor)
                ' Left label
                Dim lbl As String = "F" & f
                g.DrawString(lbl, labelFont, nameBrush, bLeft - 22, fy - 7)
                ' Right label — floor name
                Dim fname As String = FloorNames(f).Replace("🏛  ", "").Replace("💼  ", "").Replace("🌿  ", "")
                g.DrawString(fname, labelFont, nameBrush, bRight + 5, fy - 7)
            End Using
            ' Indicator dot
            Dim dotColor As Color = If(f = currentFloor, FloorColors(f), Color.FromArgb(30, 45, 75))
            Using dotBrush As New SolidBrush(dotColor)
                g.FillEllipse(dotBrush, bLeft - 10, fy - 4, 7, 7)
            End Using
        Next

        ' ── Moving arrow on shaft ──
        If isMoving Then
            Dim arrowY As Single = buildCabinY + buildCabinH / 2
            Dim arrowX As Integer = shaftLeft + shaftW \ 2
            Dim arrowPts() As Point
            If cabinTargetY < cabinY Then
                arrowPts = New Point() {New Point(arrowX, CInt(arrowY) - 14),
                                        New Point(arrowX - 5, CInt(arrowY) - 6),
                                        New Point(arrowX + 5, CInt(arrowY) - 6)}
            Else
                arrowPts = New Point() {New Point(arrowX, CInt(arrowY) + 14),
                                        New Point(arrowX - 5, CInt(arrowY) + 6),
                                        New Point(arrowX + 5, CInt(arrowY) + 6)}
            End If
            Using arrowBrush As New SolidBrush(Color.FromArgb(150, 100, 200, 100))
                g.FillPolygon(arrowBrush, arrowPts)
            End Using
        End If

        ' ── Building title ──
        Using titleFont As New Font("Segoe UI", 8, FontStyle.Bold)
            Using titleBrush As New SolidBrush(Color.FromArgb(50, 70, 110))
                Dim title As String = "NEXUS TOWER"
                Dim tsz As SizeF = g.MeasureString(title, titleFont)
                g.DrawString(title, titleFont, titleBrush, (W - tsz.Width) / 2, bTop - 52)
            End Using
        End Using
    End Sub

    Private Sub DrawFloorWindows(g As Graphics, floor As Integer, bLeft As Integer,
                                  fy As Integer, bWidth As Integer, floorH As Integer)
        ' Window layout: 2 left of shaft, 2 right of shaft
        Dim shaftCX As Integer = bLeft + bWidth \ 2
        Dim shaftW As Integer = CInt(bWidth * 0.14)
        Dim shaftLeft As Integer = shaftCX - shaftW \ 2
        Dim shaftRight As Integer = shaftCX + shaftW \ 2

        Dim winW As Integer = CInt(bWidth * 0.1)
        Dim winH As Integer = CInt(floorH * 0.42)
        Dim winY As Integer = fy + CInt(floorH * 0.22)

        ' Left side windows
        Dim leftZone As Integer = shaftLeft - bLeft - 10
        Dim leftStep As Integer = leftZone \ 2
        For i As Integer = 0 To 1
            Dim wx As Integer = bLeft + 8 + i * leftStep + (leftStep - winW) \ 2
            DrawWindow(g, wx, winY, winW, winH, floor, i)
        Next

        ' Right side windows
        Dim rightZone As Integer = bLeft + bWidth - shaftRight - 10
        Dim rightStep As Integer = rightZone \ 2
        For i As Integer = 0 To 1
            Dim wx As Integer = shaftRight + 8 + i * rightStep + (rightStep - winW) \ 2
            DrawWindow(g, wx, winY, winW, winH, floor, i + 2)
        Next
    End Sub

    Private Sub DrawWindow(g As Graphics, x As Integer, y As Integer,
                            w As Integer, h As Integer, floor As Integer, idx As Integer)
        ' Window frame
        g.FillRectangle(New SolidBrush(Color.FromArgb(15, 20, 38)), x, y, w, h)
        g.DrawRectangle(New Pen(Color.FromArgb(40, 55, 95), 1), x, y, w, h)

        ' Window glow based on lit level
        Dim lit As Single = windowLit(floor, idx)
        Dim warmAlpha As Integer = CInt(lit * 180)
        Using winGlow As New SolidBrush(Color.FromArgb(warmAlpha, 255, 220, 120))
            g.FillRectangle(winGlow, x + 1, y + 1, w - 2, h - 2)
        End Using

        ' Window pane dividers
        Using panePen As New Pen(Color.FromArgb(25, 35, 65), 0.5)
            g.DrawLine(panePen, x + w \ 2, y, x + w \ 2, y + h)
            g.DrawLine(panePen, x, y + h \ 2, x + w, y + h \ 2)
        End Using

        ' Reflection sheen
        Using sheen As New SolidBrush(Color.FromArgb(15, 180, 210, 255))
            g.FillRectangle(sheen, x + 1, y + 1, w \ 3, h \ 2)
        End Using
    End Sub

    Private Sub DrawSmallFigure(g As Graphics, px As Single, py As Single, alpha As Integer)
        If alpha <= 0 Then Return
        Using fb As New SolidBrush(Color.FromArgb(alpha, 160, 200, 255))
            g.FillEllipse(fb, px - 2, py - 7, 5, 5)
        End Using
        Using fp As New Pen(Color.FromArgb(alpha, 160, 200, 255), 1)
            g.DrawLine(fp, px, py - 2, px, py + 5)
            g.DrawLine(fp, px - 3, py + 1, px + 3, py + 1)
            g.DrawLine(fp, px, py + 5, px - 2, py + 9)
            g.DrawLine(fp, px, py + 5, px + 2, py + 9)
        End Using
    End Sub

    ' ── Door PictureBox paint ───────────────────────────────────
    Private Sub picDoor_Paint(sender As Object, e As PaintEventArgs)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.Clear(Color.FromArgb(10, 10, 20))
        Dim w = picDoor.Width
        Dim h = picDoor.Height
        Dim t = doorAnim
        Using bgBrush As New LinearGradientBrush(New Rectangle(0, 0, w, h),
            Color.FromArgb(22, 28, 45), Color.FromArgb(10, 12, 22),
            LinearGradientMode.Vertical)
            g.FillRectangle(bgBrush, 0, 0, w, h)
        End Using
        If arrivalFlash > 0 Then
            Using fb As New SolidBrush(Color.FromArgb(arrivalFlash * 180, 200, 230, 255))
                g.FillRectangle(fb, 0, 0, w, h)
            End Using
        End If
        Dim fn = FloorNames(currentFloor)
        Dim fnFont As New Font("Segoe UI", 7.5, FontStyle.Bold)
        Dim fnSz = g.MeasureString(fn, fnFont)
        Using fnBrush As New SolidBrush(Color.FromArgb(60, 90, 140))
            g.DrawString(fn, fnFont, fnBrush, (w - fnSz.Width) / 2, 6)
        End Using
        If t > 0.1F Then
            Dim ia As Integer = Math.Min(255, t * 300)
            Using intBrush As New LinearGradientBrush(New RectangleF(0, 20, w, h - 20),
                Color.FromArgb(ia, 30, 50, 90), Color.FromArgb(ia, 15, 25, 50),
                LinearGradientMode.Vertical)
                g.FillRectangle(intBrush, 0, 20, w, h - 20)
            End Using
            Using lb As New SolidBrush(Color.FromArgb(t * 120, 180, 220, 255))
                g.FillEllipse(lb, CInt(w / 2) - 30, 22, 60, 12)
            End Using
            Using lp As New Pen(Color.FromArgb(t * 180, 180, 220, 255), 1)
                g.DrawEllipse(lp, CInt(w / 2) - 30, 22, 60, 12)
            End Using
            Using wb As New SolidBrush(Color.FromArgb(t * 80, 50, 80, 130))
                g.FillRectangle(wb, 0, 20, 18, h - 20)
                g.FillRectangle(wb, w - 18, 20, 18, h - 20)
            End Using
            Using flb As New LinearGradientBrush(New RectangleF(0, h - 20, w, 20),
                Color.FromArgb(t * 150, 40, 60, 100),
                Color.FromArgb(t * 80, 20, 35, 60),
                LinearGradientMode.Vertical)
                g.FillRectangle(flb, 0, h - 20, w, 20)
            End Using
            If passengerCount > 0 Then
                Dim shown = Math.Min(passengerCount, 4)
                Dim spacing As Single = (w - 40) / (shown + 1)
                For i = 1 To shown
                    DrawStickFigure(g, 20 + spacing * i, h - 38, t * 220)
                Next
            End If
        End If
        Using fp As New Pen(Color.FromArgb(80, 100, 150), 3)
            g.DrawRectangle(fp, 2, 18, w - 4, h - 20)
        End Using
        Dim maxSlide = w / 2.0F - 2
        Dim slide = t * maxSlide
        Dim leftW = maxSlide - slide
        If leftW > 0 Then DrawDoorPanel(g, New RectangleF(2, 19, leftW, h - 21), True)
        Dim rightX = w / 2.0F + slide
        Dim rightW = w - rightX - 2
        If rightW > 0 Then DrawDoorPanel(g, New RectangleF(rightX, 19, rightW, h - 21), False)
        If t > 0.05F Then
            Dim gapW = Math.Max(slide * 2, 1)
            Dim gapX = w / 2.0F - gapW / 2
            Using gb As New LinearGradientBrush(New RectangleF(gapX, 19, gapW, h - 21),
                Color.FromArgb(t * 40, 180, 220, 255), Color.Transparent,
                LinearGradientMode.Horizontal)
                g.FillRectangle(gb, gapX, 19, gapW, h - 21)
            End Using
        End If
        DrawFloorIndicator(g, w, t)
    End Sub

    Private Sub DrawDoorPanel(g As Graphics, rect As RectangleF, isLeft As Boolean)
        If rect.Width <= 0 Then Return
        Using db As New LinearGradientBrush(rect,
            Color.FromArgb(70, 90, 130), Color.FromArgb(40, 55, 85),
            LinearGradientMode.Horizontal)
            g.FillRectangle(db, rect)
        End Using
        Dim ls As Single = rect.Width / 4
        Using gp As New Pen(Color.FromArgb(30, 180, 210, 255), 1)
            For i As Integer = 1 To 3
                Dim lx As Single = rect.Left + ls * i
                g.DrawLine(gp, lx, rect.Top + 8, lx, rect.Bottom - 8)
            Next
        End Using
        Dim sr As New RectangleF(rect.Left, rect.Top, rect.Width * 0.3F, rect.Height)
        Using sb As New LinearGradientBrush(sr,
            Color.FromArgb(40, 255, 255, 255), Color.Transparent,
            LinearGradientMode.Horizontal)
            g.FillRectangle(sb, sr)
        End Using
        Using bp As New Pen(Color.FromArgb(90, 120, 180), 1)
            g.DrawRectangle(bp, rect.X, rect.Y, rect.Width - 1, rect.Height - 1)
        End Using
    End Sub

    Private Sub DrawFloorIndicator(g As Graphics, w As Integer, t As Single)
        Using bb As New LinearGradientBrush(New Rectangle(0, 0, w, 18),
            Color.FromArgb(30, 40, 70), Color.FromArgb(18, 24, 44),
            LinearGradientMode.Vertical)
            g.FillRectangle(bb, 0, 0, w, 18)
        End Using
        Dim startX As Integer = w / 2 - 20
        For f As Integer = 1 To 3
            Dim dx As Integer = startX + (f - 1) * 20
            If f = currentFloor Then
                Using ab As New SolidBrush(Color.FromArgb(80, 200, 255))
                    g.FillEllipse(ab, dx, 5, 7, 7)
                End Using
            Else
                Using ib As New SolidBrush(Color.FromArgb(40, 60, 100))
                    g.FillEllipse(ib, dx, 5, 7, 7)
                End Using
            End If
        Next
        If isMoving Then
            Using arrowBrush As New SolidBrush(Color.FromArgb(100, 200, 100))
                Dim pts() As Point
                If cabinTargetY < cabinY Then
                    pts = New Point() {New Point(8, 13), New Point(13, 5), New Point(18, 13)}
                Else
                    pts = New Point() {New Point(8, 5), New Point(18, 5), New Point(13, 13)}
                End If
                g.FillPolygon(arrowBrush, pts)
            End Using
        End If
    End Sub

    Private Sub DrawStickFigure(g As Graphics, px As Single, py As Single, alpha As Integer)
        Using pb As New SolidBrush(Color.FromArgb(alpha, 180, 210, 255))
            g.FillEllipse(pb, px - 4, py - 10, 8, 8)
        End Using
        Using pp As New Pen(Color.FromArgb(alpha, 180, 210, 255), 1.5)
            g.DrawLine(pp, px, py - 2, px, py + 8)
            g.DrawLine(pp, px - 4, py + 2, px + 4, py + 2)
            g.DrawLine(pp, px, py + 8, px - 3, py + 14)
            g.DrawLine(pp, px, py + 8, px + 3, py + 14)
        End Using
    End Sub

    ' ── Shaft paint ─────────────────────────────────────────────
    Private Sub picShaft_Paint(sender As Object, e As PaintEventArgs)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.Clear(Color.FromArgb(14, 14, 26))
        Dim w = picShaft.Width
        Dim h = picShaft.Height
        Dim fh = h \ 3
        Dim sl = 12
        Dim sw = w - 24
        Using wp As New Pen(Color.FromArgb(60, 80, 120), 2)
            g.DrawRectangle(wp, sl, 0, sw, h - 1)
        End Using
        For f = 1 To 3
            Dim fy = h - f * fh
            Using lp As New Pen(Color.FromArgb(40, 60, 100), 1)
                If f < 3 Then g.DrawLine(lp, sl, fy, sl + sw, fy)
            End Using
            Using lb As New SolidBrush(Color.FromArgb(55, 75, 115))
                g.DrawString(FloorNames(f), New Font("Segoe UI", 6.5, FontStyle.Bold), lb, sl + 3, fy + 3)
            End Using
            Dim dc = If(f = currentFloor, FloorColors(f), Color.FromArgb(30, 50, 80))
            Using db As New SolidBrush(dc)
                g.FillEllipse(db, sl + sw - 14, fy + 6, 9, 9)
            End Using
        Next
        Dim cp = 4
        Dim cr As New RectangleF(sl + cp, cabinY + cp, sw - cp * 2, fh - cp * 2)
        If Not isMoving Then
            Dim pa As Integer = 30 + glowPulse * 60
            Dim pe = glowPulse * 6
            Using gb As New SolidBrush(Color.FromArgb(pa, 80, 160, 255))
                g.FillRectangle(gb, cr.X - pe, cr.Y - pe, cr.Width + pe * 2, cr.Height + pe * 2)
            End Using
        End If
        If arrivalFlash > 0 Then
            Using fb As New SolidBrush(Color.FromArgb(arrivalFlash * 80, 200, 230, 255))
                g.FillRectangle(fb, sl, 0, sw, h)
            End Using
        End If
        Dim pct = totalWeight / MAX_WEIGHT
        Dim c1 = If(pct >= 1.0, Color.FromArgb(160, 40, 40),
                          If(pct >= 0.9, Color.FromArgb(160, 100, 20),
                          Color.FromArgb(40, 110, 200)))
        Dim c2 = If(pct >= 1.0, Color.FromArgb(100, 20, 20),
                          If(pct >= 0.9, Color.FromArgb(100, 60, 10),
                          Color.FromArgb(20, 70, 140)))
        If cr.Width > 0 AndAlso cr.Height > 0 Then
            Using cb As New LinearGradientBrush(cr, c1, c2, LinearGradientMode.Vertical)
                g.FillRectangle(cb, cr)
            End Using
        End If
        Dim ba = If(isMoving, 80, CInt(80 + glowPulse * 120))
        Using cbp As New Pen(Color.FromArgb(ba, 80, 160, 255), If(isMoving, 1.5F, 2.0F))
            g.DrawRectangle(cbp, cr.X, cr.Y, cr.Width, cr.Height)
        End Using
        Dim nf As New Font("Segoe UI", 11, FontStyle.Bold)
        Dim ns = g.MeasureString(currentFloor.ToString, nf)
        Using nb As New SolidBrush(Color.FromArgb(200, 230, 255))
            g.DrawString(currentFloor.ToString, nf, nb,
                         cr.Left + (cr.Width - ns.Width) / 2, cr.Top + 4)
        End Using
    End Sub

    ' ── Door animations ─────────────────────────────────────────
    Private Sub AnimateDoorOpen()
        doorAnimTarget = 1.0F
        PlayDoorOpen()
        Me.Invoke(Sub()
                      lblDoorStatus.Text = "🚪 OPENING..."
                      lblDoorStatus.ForeColor = Color.FromArgb(255, 200, 60)
                  End Sub)
        Thread.Sleep(DOOR_MS)
        Me.Invoke(Sub()
                      lblDoorStatus.Text = "🚪 OPEN"
                      lblDoorStatus.ForeColor = Color.FromArgb(60, 220, 120)
                  End Sub)
    End Sub

    Private Sub AnimateDoorClose()
        doorAnimTarget = 0.0F
        PlayDoorClose()
        Me.Invoke(Sub()
                      lblDoorStatus.Text = "🚪 CLOSING..."
                      lblDoorStatus.ForeColor = Color.FromArgb(255, 160, 40)
                  End Sub)
        Thread.Sleep(DOOR_MS)
        Me.Invoke(Sub()
                      lblDoorStatus.Text = "🚪 CLOSED"
                      lblDoorStatus.ForeColor = Color.FromArgb(255, 200, 80)
                  End Sub)
    End Sub

    ' ── Passengers ─────────────────────────────────────────────
    Private Sub btnAddPassengers_Click(sender As Object, e As EventArgs) Handles btnAddPassengers.Click
        Dim count As Integer = CInt(nudPassengers.Value)
        Dim added As Integer = 0, skipped As Integer = 0
        For i As Integer = 1 To count
            Dim pw As Integer = rng.Next(45, 111)
            If totalWeight + pw > MAX_WEIGHT Then
                skipped += 1
                LogEvent("🚫 Passenger refused — max capacity!")
                PlayWarning()
            Else
                totalWeight += pw : passengerCount += 1 : added += 1
                LogEvent("🧍 Passenger " & passengerCount & " boarded (" & pw & " kg)")
            End If
        Next
        If skipped > 0 Then LogEvent("⚠️ " & skipped & " could not board — elevator full!")
        If added > 0 Then LogEvent("📊 Total: " & passengerCount & " pax | " & totalWeight & " kg")
        UpdateWeightDisplay()
        CheckWeightWarning()
        picDoor.Invalidate()
        picBuilding.Invalidate()
    End Sub

    Private Sub btnClearPassengers_Click(sender As Object, e As EventArgs) Handles btnClearPassengers.Click
        If passengerCount = 0 Then LogEvent("ℹ️ No passengers aboard.") : Return
        LogEvent("🚶 " & passengerCount & " disembarked at " & FloorNames(currentFloor) & ".")
        passengerCount = 0 : totalWeight = 0
        UpdateWeightDisplay()
        SetStatus("✅ Passengers disembarked. Ready.")
        picDoor.Invalidate() : picBuilding.Invalidate()
    End Sub

    Private Sub UpdateWeightDisplay()
        Dim pct As Double = totalWeight / MAX_WEIGHT
        lblCapacityBar.Width = CInt(Math.Max(0, Math.Min(pct, 1.0) * pnlCapacityBg.Width))
        lblCapacityBar.BackColor = If(pct < 0.6, Color.FromArgb(40, 200, 100),
                                   If(pct < 0.9, Color.FromArgb(220, 160, 20),
                                   Color.FromArgb(220, 50, 50)))
        lblWeightVal.Text = totalWeight & " / " & MAX_WEIGHT & " kg"
        lblPassengerCount.Text = "👥 " & passengerCount & " passenger(s) aboard"
    End Sub

    Private Sub CheckWeightWarning()
        If totalWeight >= MAX_WEIGHT Then
            SetStatus("🚫 OVERLOADED — Doors will not close!")
            lblStatus.ForeColor = Color.FromArgb(255, 80, 80)
        ElseIf totalWeight >= WARN_WEIGHT Then
            SetStatus("⚠️ Near capacity — " & totalWeight & " / " & MAX_WEIGHT & " kg")
            lblStatus.ForeColor = Color.FromArgb(255, 200, 60)
        Else
            lblStatus.ForeColor = Color.FromArgb(180, 210, 255)
        End If
    End Sub

    ' ── Floor buttons ───────────────────────────────────────────
    Private Sub btnFloor1_Click(sender As Object, e As EventArgs) Handles btnFloor1.Click
        RequestFloor(1)
    End Sub
    Private Sub btnFloor2_Click(sender As Object, e As EventArgs) Handles btnFloor2.Click
        RequestFloor(2)
    End Sub
    Private Sub btnFloor3_Click(sender As Object, e As EventArgs) Handles btnFloor3.Click
        RequestFloor(3)
    End Sub

    Private Sub RequestFloor(floor As Integer)
        If emergencyStop Then LogEvent("⛔ Emergency stop active.") : Return
        If totalWeight >= MAX_WEIGHT Then
            LogEvent("🚫 Cannot move — overloaded!") : Return
        End If
        If floor = currentFloor AndAlso Not isMoving Then
            LogEvent("ℹ️ Already at " & FloorNames(floor) & ".") : Return
        End If
        If Not targetQueue.Contains(floor) Then
            targetQueue.Enqueue(floor)
            LogEvent("🔘 " & FloorNames(floor) & " queued.")
        End If
        If Not isMoving Then
            Dim t As New Thread(AddressOf ProcessQueue)
            t.IsBackground = True : t.Start()
        End If
    End Sub

    ' ── Smart scan movement ─────────────────────────────────────
    Private Sub ProcessQueue()
        isMoving = True
        PlayMovementTick()
        Do While targetQueue.Count > 0 AndAlso Not emergencyStop
            Dim sl As New List(Of Integer)(targetQueue)
            Dim dir As Integer = If(sl(0) >= currentFloor, 1, -1)
            Dim inD As List(Of Integer)
            Dim opp As List(Of Integer)
            If dir = 1 Then
                inD = sl.Where(Function(f) f >= currentFloor).OrderBy(Function(f) f).ToList()
                opp = sl.Where(Function(f) f < currentFloor).OrderByDescending(Function(f) f).ToList()
            Else
                inD = sl.Where(Function(f) f <= currentFloor).OrderByDescending(Function(f) f).ToList()
                opp = sl.Where(Function(f) f > currentFloor).OrderBy(Function(f) f).ToList()
            End If
            Dim ordered As New List(Of Integer)
            ordered.AddRange(inD) : ordered.AddRange(opp)
            targetQueue.Clear()
            For Each f In ordered : targetQueue.Enqueue(f) : Next

            Dim target As Integer = targetQueue.Dequeue()
            Do While currentFloor <> target AndAlso Not emergencyStop
                Dim d As Integer = If(target > currentFloor, 1, -1)
                SetStatus(If(d = 1, "▲", "▼") & " Moving " & If(d = 1, "Up", "Down") & " → " & FloorNames(target))
                Thread.Sleep(TRAVEL_MS)
                If Not emergencyStop Then
                    currentFloor += d
                    Me.Invoke(Sub()
                                  UpdateFloorDisplay()
                                  UpdateCabinTarget()
                                  UpdateFloorButtons()
                              End Sub)
                    LogEvent("🏢 Passing " & FloorNames(currentFloor))
                    PlayMovementTick()
                    If targetQueue.Contains(currentFloor) Then
                        targetQueue = New Queue(Of Integer)(targetQueue.Where(Function(f) f <> currentFloor))
                        LogEvent("🛑 Pickup stop at " & FloorNames(currentFloor) & "!")
                        target = currentFloor
                    End If
                End If
            Loop
            If Not emergencyStop Then OpenDoor()
        Loop
        isMoving = False
        If Not emergencyStop Then SetStatus("✅ Idle at " & FloorNames(currentFloor))
    End Sub

    ' ── Door sequence ───────────────────────────────────────────
    Private Sub OpenDoor()
        SetStatus("🚪 Arrived at " & FloorNames(currentFloor) & " — Opening...")
        LogEvent("✅ Arrived at " & FloorNames(currentFloor) & ".")
        PlayDing()
        Me.Invoke(Sub() arrivalFlash = 1.0F)
        doorOpen = True
        AnimateDoorOpen()
        Me.Invoke(Sub() AutoDisembark())
        Dim waited As Integer = 0
        Do While (doorHeld OrElse waited < 2500) AndAlso Not emergencyStop
            Thread.Sleep(100)
            If Not doorHeld Then waited += 100
        Loop
        SetStatus("🚪 Closing doors...")
        AnimateDoorClose()
        doorOpen = False
        LogEvent("🔒 Doors closed at " & FloorNames(currentFloor) & ".")
    End Sub

    Private Sub AutoDisembark()
        If passengerCount = 0 Then Return
        Dim exiting As Integer = rng.Next(1, passengerCount + 1)
        Dim weightRemoved As Integer = 0
        For i As Integer = 1 To exiting
            If passengerCount > 0 Then
                Dim wt As Integer = Math.Min(rng.Next(45, 111), totalWeight)
                totalWeight = Math.Max(0, totalWeight - wt)
                weightRemoved += wt : passengerCount -= 1
            End If
        Next
        LogEvent("🚶 " & exiting & " exited at " & FloorNames(currentFloor) &
                 " (-" & weightRemoved & " kg) | " & passengerCount & " left, " & totalWeight & " kg")
        UpdateWeightDisplay() : CheckWeightWarning()
        picDoor.Invalidate() : picBuilding.Invalidate()
    End Sub

    ' ── Emergency / Hold ────────────────────────────────────────
    Private Sub btnEmergency_Click(sender As Object, e As EventArgs) Handles btnEmergency.Click
        If Not emergencyStop Then
            emergencyStop = True : targetQueue.Clear() : PlayWarning()
            Me.Invoke(Sub()
                          lblStatus.ForeColor = Color.FromArgb(255, 80, 80)
                          lblStatus.Text = "🚨 EMERGENCY STOP ACTIVATED"
                          btnEmergency.Text = "🔓 Reset Emergency"
                          btnEmergency.BackColor = Color.FromArgb(80, 20, 20)
                      End Sub)
            LogEvent("🚨 EMERGENCY STOP!")
        Else
            emergencyStop = False
            Me.Invoke(Sub()
                          lblStatus.ForeColor = Color.FromArgb(180, 210, 255)
                          btnEmergency.Text = "🚨 Emergency Stop"
                          btnEmergency.BackColor = Color.FromArgb(200, 40, 40)
                      End Sub)
            SetStatus("✅ System reset. Ready.")
            LogEvent("🟢 Emergency reset. System ready.")
        End If
    End Sub

    Private Sub btnHoldDoor_MouseDown(sender As Object, e As MouseEventArgs) Handles btnHoldDoor.MouseDown
        doorHeld = True : LogEvent("🤚 Door hold active.")
    End Sub
    Private Sub btnHoldDoor_MouseUp(sender As Object, e As MouseEventArgs) Handles btnHoldDoor.MouseUp
        doorHeld = False : LogEvent("✅ Door hold released.")
    End Sub

    ' ── Helpers ─────────────────────────────────────────────────
    Private Sub UpdateCabinTarget()
        cabinTargetY = picShaft.Height - (currentFloor * (picShaft.Height / 3.0F))
    End Sub

    Private Sub FormElevator_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        UpdateCabinTarget()
        cabinY = cabinTargetY
        picShaft.Invalidate() : picDoor.Invalidate() : picBuilding.Invalidate()
        InitBackground()
    End Sub

    Private Sub SetStatus(msg As String)
        Me.Invoke(Sub() lblStatus.Text = msg)
    End Sub

    Private Sub UpdateFloorDisplay()
        lblCurrentFloor.Text = "Floor  " & currentFloor
        UpdateCabinTarget()
        picShaft.Invalidate() : picDoor.Invalidate() : picBuilding.Invalidate()
    End Sub

    Private Sub LogEvent(msg As String)
        Me.Invoke(Sub()
                      lstLog.Items.Insert(0, DateTime.Now.ToString("HH:mm:ss") & "  " & msg)
                      If lstLog.Items.Count > 80 Then lstLog.Items.RemoveAt(80)
                  End Sub)
    End Sub

    Private Sub picDoor_Click(sender As Object, e As EventArgs) Handles picDoor.Click

    End Sub
End Class