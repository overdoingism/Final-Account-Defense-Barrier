Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices

Module VisualModule

    '================ The program's own icon as the window icon ==============
    '
    ' The icon Explorer shows (fadb4.ico, compiled into the exe as its Win32 icon) is also the icon of the
    ' windows. It is read from the exe at start-up instead of being stored a second time in FormMain's resources.
    '
    ' That icon file keeps all its sizes PNG-compressed, which is fine for Windows but not for System.Drawing.Icon:
    ' it reads a small PNG-compressed size as if it were a plain bitmap and draws noise. So every size below
    ' 256 pixels is unpacked here into the plain 32-bit bitmap form (the form the icon in the resources had);
    ' the 256-pixel size stays as it is, as it was there too.

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Function FindResource(hModule As IntPtr, lpName As IntPtr, lpType As IntPtr) As IntPtr
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Function LoadResource(hModule As IntPtr, hResInfo As IntPtr) As IntPtr
    End Function

    <DllImport("kernel32.dll")>
    Private Function LockResource(hResData As IntPtr) As IntPtr
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Function SizeofResource(hModule As IntPtr, hResInfo As IntPtr) As Integer
    End Function

    Private Const RT_ICON As Integer = 3
    Private Const RT_GROUP_ICON As Integer = 14
    Private Const IDI_APPLICATION As Integer = 32512       'the id the compiler gives the application icon

    Private _programIcon As Icon = Nothing
    Private _programIconTried As Boolean = False

    Private Function Win32Resource(hModule As IntPtr, Id As Integer, ResType As Integer) As Byte()
        Dim hInfo As IntPtr = FindResource(hModule, New IntPtr(Id), New IntPtr(ResType))
        If hInfo = IntPtr.Zero Then Return Nothing
        Dim Size As Integer = SizeofResource(hModule, hInfo)
        Dim hData As IntPtr = LoadResource(hModule, hInfo)
        If hData = IntPtr.Zero OrElse Size <= 0 Then Return Nothing
        Dim P As IntPtr = LockResource(hData)
        If P = IntPtr.Zero Then Return Nothing
        Dim B(Size - 1) As Byte
        Marshal.Copy(P, B, 0, Size)
        Return B
    End Function

    ''' <summary>One PNG-compressed icon picture as the plain bitmap form of an icon file: header, 32-bit pixels, mask.</summary>
    Private Function IconPictureAsBitmap(Png() As Byte) As Byte()
        Using Ms As New IO.MemoryStream(Png)
            Using Src As New Bitmap(Ms)
                Dim W As Integer = Src.Width, H As Integer = Src.Height
                Dim Pix(W * H * 4 - 1) As Byte
                Dim BD As BitmapData = Src.LockBits(New Rectangle(0, 0, W, H), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
                For y As Integer = 0 To H - 1
                    Marshal.Copy(IntPtr.Add(BD.Scan0, y * BD.Stride), Pix, y * W * 4, W * 4)
                Next
                Src.UnlockBits(BD)
                Dim MaskRow As Integer = ((W + 31) \ 32) * 4
                Dim Res(40 + W * H * 4 + MaskRow * H - 1) As Byte
                Array.Copy(BitConverter.GetBytes(40), 0, Res, 0, 4)              'BITMAPINFOHEADER
                Array.Copy(BitConverter.GetBytes(W), 0, Res, 4, 4)
                Array.Copy(BitConverter.GetBytes(H * 2), 0, Res, 8, 4)           'picture and mask, one above the other
                Array.Copy(BitConverter.GetBytes(CShort(1)), 0, Res, 12, 2)
                Array.Copy(BitConverter.GetBytes(CShort(32)), 0, Res, 14, 2)
                Array.Copy(BitConverter.GetBytes(W * H * 4), 0, Res, 20, 4)      'size of the pixels, as icon editors write it
                For y As Integer = 0 To H - 1                                    'rows run bottom-up
                    Array.Copy(Pix, (H - 1 - y) * W * 4, Res, 40 + y * W * 4, W * 4)
                    For x As Integer = 0 To W - 1
                        If Pix(((H - 1 - y) * W + x) * 4 + 3) = 0 Then
                            Res(40 + W * H * 4 + y * MaskRow + x \ 8) = Res(40 + W * H * 4 + y * MaskRow + x \ 8) Or CByte(&H80 >> (x Mod 8))
                        End If
                    Next
                Next
                Return Res
            End Using
        End Using
    End Function

    ''' <summary>
    ''' The program's own icon (all its sizes), or Nothing if it cannot be read - the windows then keep the
    ''' default icon. Read once.
    ''' </summary>
    Public Function ProgramIcon() As Icon

        If _programIconTried Then Return _programIcon
        _programIconTried = True

        Try
            Dim hModule As IntPtr = Marshal.GetHINSTANCE(GetType(VisualModule).Module)
            If hModule = IntPtr.Zero OrElse hModule = New IntPtr(-1) Then Return Nothing

            ' the icon group lists the sizes; each size is an icon resource of its own
            Dim Group() As Byte = Win32Resource(hModule, IDI_APPLICATION, RT_GROUP_ICON)
            If Group Is Nothing OrElse Group.Length < 6 Then Return Nothing
            Dim Count As Integer = BitConverter.ToUInt16(Group, 4)
            If Count = 0 OrElse Group.Length < 6 + Count * 14 Then Return Nothing

            Dim Pictures As New List(Of Byte())
            Dim Unpacked As New List(Of Boolean)
            For i As Integer = 0 To Count - 1
                Dim Pic() As Byte = Win32Resource(hModule, BitConverter.ToUInt16(Group, 6 + i * 14 + 12), RT_ICON)
                If Pic Is Nothing Then Return Nothing
                Dim IsPng As Boolean = Pic.Length > 8 AndAlso Pic(0) = &H89 AndAlso Pic(1) = &H50 AndAlso Pic(2) = &H4E AndAlso Pic(3) = &H47
                Dim Unpack As Boolean = IsPng AndAlso Group(6 + i * 14) <> 0                      'width 0 = 256 pixels: stays PNG
                If Unpack Then Pic = IconPictureAsBitmap(Pic)
                Pictures.Add(Pic)
                Unpacked.Add(Unpack)
            Next

            ' put together as an icon file: header, one entry per size, the pictures
            Using Ms As New IO.MemoryStream()
                Dim Bw As New IO.BinaryWriter(Ms)
                Bw.Write(Group, 0, 6)
                Dim Offset As Integer = 6 + Count * 16
                For i As Integer = 0 To Count - 1
                    Bw.Write(Group, 6 + i * 14, 4)                              'width, height, colours, reserved
                    If Unpacked(i) Then
                        Bw.Write(CUShort(1))
                        Bw.Write(CUShort(32))
                    Else
                        Bw.Write(Group, 6 + i * 14 + 4, 4)                      'planes, bits per pixel
                    End If
                    Bw.Write(CUInt(Pictures(i).Length))
                    Bw.Write(CUInt(Offset))
                    Offset += Pictures(i).Length
                Next
                For Each Pic As Byte() In Pictures
                    Bw.Write(Pic)
                Next
                Bw.Flush()
                Ms.Position = 0
                _programIcon = New Icon(Ms)
            End Using

        Catch ex As Exception
            _programIcon = Nothing
        End Try

        Return _programIcon

    End Function

    '================ For window grayed out visual ==============

    <DllImport("Shcore.dll", SetLastError:=True, CharSet:=CharSet.Unicode)>
    Public Function GetScaleFactorForMonitor(hMon As IntPtr, ByRef pScale As DEVICE_SCALE_FACTOR) As Integer 'HRESULT
    End Function

    <DllImport("User32.dll", SetLastError:=True, CharSet:=CharSet.Unicode)>
    Public Function MonitorFromWindow(ByVal hwnd As IntPtr, ByVal dwFlags As Integer) As IntPtr
    End Function

    Public Enum DEVICE_SCALE_FACTOR
        SCALE_100_PERCENT = 100
        SCALE_120_PERCENT = 120
        SCALE_125_PERCENT = 125
        SCALE_140_PERCENT = 140
        SCALE_150_PERCENT = 150
        SCALE_160_PERCENT = 160
        SCALE_175_PERCENT = 175
        SCALE_180_PERCENT = 180
        SCALE_200_PERCENT = 200
        SCALE_225_PERCENT = 225
        SCALE_250_PERCENT = 250
        SCALE_300_PERCENT = 300
        SCALE_350_PERCENT = 350
        SCALE_400_PERCENT = 400
        SCALE_450_PERCENT = 450
        SCALE_500_PERCENT = 500
    End Enum

    Public Const MONITOR_DEFAULTTONULL As Integer = &H0
    Public Const MONITOR_DEFAULTTOPRIMARY As Integer = &H1
    Public Const MONITOR_DEFAULTTONEAREST As Integer = &H2

    Public ScaleFolat As Single = 1.0F

    Public Sub GetMonScale(ByRef WhatForm As Form)

        Dim hMon As IntPtr = MonitorFromWindow(WhatForm.Handle, MONITOR_DEFAULTTOPRIMARY)
        Dim scaleFactor As DEVICE_SCALE_FACTOR
        GetScaleFactorForMonitor(hMon, scaleFactor)
        ScaleFolat = scaleFactor / 100

        ' A DPI-aware process already works in real screen pixels: no conversion is needed for screen capture.
        If UIArt.DpiAware Then ScaleFolat = 1.0F

    End Sub

    Public Sub MakeWindowsMono(ByRef WhatForm As Form, ByRef WhatImgToPut As PictureBox)

        SetWindowDisplayAffinity(WhatForm.Handle, 0)

        Dim NewPos As Point
        NewPos.X = WhatForm.Location.X * ScaleFolat
        NewPos.Y = WhatForm.Location.Y * ScaleFolat
        Dim NewSize As Size
        NewSize.Width = WhatForm.Size.Width * ScaleFolat
        NewSize.Height = WhatForm.Size.Height * ScaleFolat

        Dim bmpOrg As New Bitmap(NewSize.Width, NewSize.Height)

        Using g1 As Graphics = Graphics.FromImage(bmpOrg)
            g1.CopyFromScreen(NewPos, Point.Empty, NewSize)
        End Using

        WhatImgToPut.Image = MonoBitmap(bmpOrg) 'newBitmap
        WhatImgToPut.BringToFront()
        WhatImgToPut.Visible = True

        If Not Sys_Chk.Screen_Capture_Allowed Then 'Disable Screen Capture
            SetWindowDisplayAffinity(WhatForm.Handle, WDA_EXCLUDEFROMCAPTURE)
        End If

    End Sub

    Public Sub UnMakeWindowsMono(ByRef WhatImgToPut As PictureBox)
        WhatImgToPut.Visible = False
        WhatImgToPut.SendToBack()
        System.Windows.Forms.Application.DoEvents()
    End Sub


    Public Function Make_Button_brighter(ByRef Orig_Bitmap As Bitmap, Optional BrightPN As Single = 1.3) As Bitmap

        ' 建立目標圖像的副本
        Dim targetBitmap As New Bitmap(Orig_Bitmap.Width, Orig_Bitmap.Height)

        ' 創建ColorMatrix，用於調整圖像亮度
        Dim brightenMatrix As New Imaging.ColorMatrix(New Single()() _
            {New Single() {BrightPN, 0, 0, 0, 0},
             New Single() {0, BrightPN, 0, 0, 0},
             New Single() {0, 0, BrightPN, 0, 0},
             New Single() {0, 0, 0, 1, 0},
             New Single() {0.12, 0.12, 0.12, 0, 1}})

        ' 創建ImageAttributes對象，並設定ColorMatrix
        Dim imageAttributes As New Imaging.ImageAttributes()
        imageAttributes.SetColorMatrix(brightenMatrix)

        ' 創建Graphics對象，並使用ImageAttributes繪製目標圖像
        Dim graphics As Graphics = Graphics.FromImage(targetBitmap)
        graphics.DrawImage(Orig_Bitmap, New Rectangle(0, 0, Orig_Bitmap.Width, Orig_Bitmap.Height), 0, 0, Orig_Bitmap.Width, Orig_Bitmap.Height, GraphicsUnit.Pixel, imageAttributes)

        ' 釋放資源
        graphics.Dispose()

        ' Let the brightened plate shine a little into its dark lettering (the same glow filter as the windows).
        Try
            ' Half the reach of the window glow: the light seeps in from the edges of the dark
            ' lettering while the middle of the strokes stays black.
            Using Glowed As Bitmap = UIArt.Glow(targetBitmap, Math.Max(0.5F, UIArt.Scale * 0.5F))
                ' blended in at half strength: the full glow is too strong on a small button
                Using g2 As Graphics = Graphics.FromImage(targetBitmap)
                    Dim Half As New Imaging.ColorMatrix()
                    Half.Matrix33 = 0.5F
                    Using ia As New Imaging.ImageAttributes()
                        ia.SetColorMatrix(Half)
                        g2.DrawImage(Glowed, New Rectangle(0, 0, targetBitmap.Width, targetBitmap.Height), 0, 0, targetBitmap.Width, targetBitmap.Height, GraphicsUnit.Pixel, ia)
                    End Using
                End Using
            End Using
        Catch ex As Exception
        End Try

        ' 顯示調整後的圖像
        Return targetBitmap

    End Function

    Public Function Make_Button_Gray(ByRef Orig_Bitmap As Bitmap, Optional LowDimm As Single = 0) As Bitmap

        ' 建立目標圖像的副本
        Dim targetBitmap As New Bitmap(Orig_Bitmap.Width, Orig_Bitmap.Height)

        ' 創建ColorMatrix，用於調整圖像亮度
        Dim brightenMatrix As New Imaging.ColorMatrix(New Single()() _
            {New Single() {0.3F, 0.3F, 0.3F, 0, 0},
             New Single() {0.59F, 0.59F, 0.59F, 0, 0},
             New Single() {0.11F, 0.11F, 0.11F, 0, 0},
             New Single() {0, 0, 0, 1, 0},
             New Single() {LowDimm, LowDimm, LowDimm, 0, 1}})

        ' 創建ImageAttributes對象，並設定ColorMatrix
        Dim imageAttributes As New Imaging.ImageAttributes()
        imageAttributes.SetColorMatrix(brightenMatrix)

        ' 創建Graphics對象，並使用ImageAttributes繪製目標圖像
        Dim graphics As Graphics = Graphics.FromImage(targetBitmap)
        graphics.DrawImage(Orig_Bitmap, New Rectangle(0, 0, Orig_Bitmap.Width, Orig_Bitmap.Height), 0, 0, Orig_Bitmap.Width, Orig_Bitmap.Height, GraphicsUnit.Pixel, imageAttributes)

        ' 釋放資源
        graphics.Dispose()

        ' 顯示調整後的圖像
        Return targetBitmap

    End Function

    Public Function Make_Button_HueChange(ByVal Orig_Bitmap As Bitmap, HueAngle As Single) As Bitmap

        ' 建立目標圖像的副本
        Dim r As Double = HueAngle * Math.PI / 180 ' degrees to radians

        ' 創建ColorMatrix，用於調整圖像亮度
        Dim theta As Single = HueAngle / 360 * 2 * Math.PI ' Degrees --> Radians
        Dim c As Single = Math.Cos(theta)
        Dim s As Single = Math.Sin(theta)

        Dim A00 As Single = 0.213 + 0.787 * c - 0.213 * s
        Dim A01 As Single = 0.213 - 0.213 * c + 0.413 * s
        Dim A02 As Single = 0.213 - 0.213 * c - 0.787 * s

        Dim A10 As Single = 0.715 - 0.715 * c - 0.715 * s
        Dim A11 As Single = 0.715 + 0.285 * c + 0.14 * s
        Dim A12 As Single = 0.715 - 0.715 * c + 0.715 * s

        Dim A20 As Single = 0.072 - 0.072 * c + 0.928 * s
        Dim A21 As Single = 0.072 - 0.072 * c - 0.283 * s
        Dim A22 As Single = 0.072 + 0.928 * c + 0.072 * s

        ' 建立目標圖像的副本
        Dim targetBitmap As New Bitmap(Orig_Bitmap.Width, Orig_Bitmap.Height)

        ' 創建ColorMatrix，用於調整圖像亮度
        Dim hueMatrix As New Imaging.ColorMatrix(New Single()() _
            {New Single() {A00, A01, A02, 0, 0},
             New Single() {A10, A11, A12, 0, 0},
             New Single() {A20, A21, A22, 0, 0},
             New Single() {0, 0, 0, 1, 0},
             New Single() {0, 0, 0, 0, 1}})

        ' 創建ImageAttributes對象，並設定ColorMatrix
        Dim imageAttributes As New Imaging.ImageAttributes()
        imageAttributes.SetColorMatrix(hueMatrix)

        ' 創建Graphics對象，並使用ImageAttributes繪製目標圖像
        Dim graphics As Graphics = Graphics.FromImage(targetBitmap)
        graphics.DrawImage(Orig_Bitmap, New Rectangle(0, 0, Orig_Bitmap.Width, Orig_Bitmap.Height), 0, 0, Orig_Bitmap.Width, Orig_Bitmap.Height, GraphicsUnit.Pixel, imageAttributes)

        ' 釋放資源
        graphics.Dispose()

        ' 顯示調整後的圖像
        Return targetBitmap

    End Function

    Public Function ResizeBitmap(originalBitmap As Bitmap, ScaleX As Single, ScaleY As Single) As Bitmap
        Dim newWidth As Integer = originalBitmap.Width * ScaleX
        Dim newHeight As Integer = originalBitmap.Height * ScaleY

        Dim resizedBitmap As New Bitmap(newWidth, newHeight)

        Using g As Graphics = Graphics.FromImage(resizedBitmap)
            g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            g.DrawImage(originalBitmap, New Rectangle(0, 0, newWidth, newHeight))
        End Using

        Return resizedBitmap
    End Function

    Public Function MonoBitmap(ByRef bmpOrg As Bitmap) As Bitmap

        Dim newBitmap As New Bitmap(bmpOrg.Width, bmpOrg.Height)
        Dim g As Graphics = Graphics.FromImage(newBitmap)

        '創建ColorMatrix
        Dim colorMatrix As New Imaging.ColorMatrix(New Single()() _
        {
            New Single() {0.3F, 0.3F, 0.3F, 0, 0},
            New Single() {0.59F, 0.59F, 0.59F, 0, 0},
            New Single() {0.11F, 0.11F, 0.11F, 0, 0},
            New Single() {0, 0, 0, 1, 0},
            New Single() {-0.4F, -0.4F, -0.4F, 0, 1}
        })
        '創建ImageAttributes
        Dim imageAttributes As New Imaging.ImageAttributes()
        imageAttributes.SetColorMatrix(colorMatrix)

        bmpOrg = ResizeBitmap(bmpOrg, 1 / ScaleFolat, 1 / ScaleFolat)
        '使用Graphics將原始圖片繪製到Bitmap中，同時應用ImageAttributes
        g.DrawImage(bmpOrg, New Rectangle(0, 0, bmpOrg.Width, bmpOrg.Height), 0, 0, bmpOrg.Width, bmpOrg.Height, GraphicsUnit.Pixel, imageAttributes)
        g.Dispose()

        ' The darkening above also removes the faint glow around the shapes. When Windows still
        ' stretched (blurred) the whole window that went unnoticed; on a sharp picture the result looks
        ' like the artwork before the glow filter. So the glow is applied again to the grey picture.
        Try
            Using Glowed As Bitmap = UIArt.Glow(newBitmap)
                Dim Result As Bitmap = Glowed.Clone(New Rectangle(0, 0, newBitmap.Width, newBitmap.Height), Imaging.PixelFormat.Format32bppArgb)
                newBitmap.Dispose()
                Return Result
            End Using
        Catch ex As Exception
        End Try

        Return newBitmap

    End Function

    ' The glow filter itself moved to UIArtGlow.vb, where it also handles high-DPI pictures.
    Public Function The_Shine_Visual_Filter2(orgbitmap As Bitmap) As Bitmap
        Return UIArt.Glow(orgbitmap)
    End Function

    Public FormLoginBitmap0 As Bitmap
    Public FormLoginBitmap13 As Bitmap
    Public FormLoginBitmap2 As Bitmap
    Public FormConfigBitmap As Bitmap
    Public FormKDFBitmap As Bitmap
    Public FormMainBitmap As Bitmap
    Public FormFileExpBitmap As Bitmap
    Public FormMITBitmap As Bitmap

    ''' <summary>
    ''' Starts rendering the UI pictures on background threads as soon as the program starts, so that
    ''' they are ready (or well under way) when the windows ask for them. Order: what the login window
    ''' needs first, then the small pictures (buttons ...), then the windows that come after login.
    ''' Nothing depends on this having finished: a picture that is not ready is rendered on demand.
    ''' </summary>
    Public Sub Start_PreRender(ThreadCount As Integer)

        Dim Jobs As New Queue(Of Action)
        Jobs.Enqueue(Sub() Load_Bitmap_For_Threads(0))      'login window
        Jobs.Enqueue(Sub() Load_Bitmap_For_Threads(7))      'file explorer (opened from the login window)
        For Each N As String In UIArt.CacheableNames()
            Dim PicName As String = N
            Jobs.Enqueue(Sub() UIArt.Img(PicName).Dispose())
        Next
        Jobs.Enqueue(Sub() Load_Bitmap_For_Threads(5))      'key derivation window
        Jobs.Enqueue(Sub() Load_Bitmap_For_Threads(6))      'main window

        For i As Integer = 1 To Math.Max(1, ThreadCount)
            Dim T As New Threading.Thread(
                Sub()
                    Do
                        Dim Job As Action
                        SyncLock Jobs
                            If Jobs.Count = 0 Then Exit Do
                            Job = Jobs.Dequeue()
                        End SyncLock
                        Try
                            Job()
                        Catch ex As Exception
                            'whatever failed here is rendered again (and reported) when it is needed
                        End Try
                    Loop
                End Sub)
            T.IsBackground = True
            T.Start()
        Next

    End Sub

    Private ReadOnly BitmapLoadLocks() As Object = {New Object, New Object, New Object, New Object, New Object,
                                                   New Object, New Object, New Object, New Object}

    ' Each background is produced once; a second request (another thread, or the same form opened
    ' again) waits for / reuses the first result instead of rendering and filtering it again.
    Public Sub Load_Bitmap_For_Threads(WhatToLoad As Integer)

        SyncLock BitmapLoadLocks(WhatToLoad)
        Select Case WhatToLoad

            Case 0
                If FormLoginBitmap0 Is Nothing Then FormLoginBitmap0 = The_Shine_Visual_Filter2(UIArt.Window("Title_LOGIN"))
            Case 1
                If FormLoginBitmap13 Is Nothing Then FormLoginBitmap13 = The_Shine_Visual_Filter2(UIArt.Window("Title_NORMAL"))
            Case 2
                If FormLoginBitmap2 Is Nothing Then FormLoginBitmap2 = The_Shine_Visual_Filter2(UIArt.Window("Title_Password"))
            Case 4
                If FormConfigBitmap Is Nothing Then FormConfigBitmap = The_Shine_Visual_Filter2(UIArt.Window("Form_Config"))
            Case 5
                If FormKDFBitmap Is Nothing Then FormKDFBitmap = The_Shine_Visual_Filter2(UIArt.Window("PWD_KDF"))
            Case 6
                If FormMainBitmap Is Nothing Then FormMainBitmap = The_Shine_Visual_Filter2(UIArt.Window("Form_Main"))
            Case 7
                If FormFileExpBitmap Is Nothing Then FormFileExpBitmap = The_Shine_Visual_Filter2(UIArt.Window("File_expo_New"))
            Case 8
                If FormMITBitmap Is Nothing Then FormMITBitmap = The_Shine_Visual_Filter2(UIArt.Window("mit_lic"))
        End Select
        End SyncLock

    End Sub

    Public Function CountDownOnImg(OrigBmp As Bitmap, WhatStrOn As String, FontSize As Integer) As Bitmap

        ' Font size in pixels so that it follows the DPI-scaled button picture.
        Dim RndFnt() As Font = {New System.Drawing.Font("Arial", CSng(FontSize * 96.0F / 72.0F * UIArt.Scale), FontStyle.Bold, GraphicsUnit.Pixel)}

        Dim BitA As New Bitmap(OrigBmp)
        Dim GrA As Graphics = Graphics.FromImage(BitA)
        Dim Brs1 As Brush = New System.Drawing.SolidBrush(Color.FromArgb(168, 0, 118, 232))
        Dim Brs2 As New SolidBrush(Color.FromArgb(128, 255, 255, 255))

        'GrA.Clear(Color.White)

        GrA.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias

        Dim textSize As SizeF = GrA.MeasureString(WhatStrOn, RndFnt(0), Nothing, Nothing, 1, 1)
        'GrA.DrawString(WhatStrOn, RndFnt(0), Brs1, (BitA.Width / 2) - (textSize.Width / 2), (BitA.Height / 2) - (textSize.Height / 2) + 5)

        Dim borderSize As Single = 2.0F * UIArt.Scale

        For x As Integer = -1 To 1
            For y As Integer = -1 To 1
                GrA.DrawString(WhatStrOn, RndFnt(0), Brs2, x + borderSize, y + borderSize)
            Next
        Next
        GrA.DrawString(WhatStrOn, RndFnt(0), Brs1, borderSize, borderSize)

        Return BitA 'DirectCast(BitA.Clone(), Image)

    End Function


    Public Function CreateGradientBitmap(ByVal width As Integer, ByVal height As Integer) As Bitmap

        Dim bmp As New Bitmap(width, height)

        ' 使用 Graphics 對象來繪製
        Using g As Graphics = Graphics.FromImage(bmp)
            ' 定義漸層範圍
            Dim rect As New Rectangle(0, 0, width, height)

            ' 創建 LinearGradientBrush，從上方（藍色）到下方（白色）漸變
            Using lgb As New Drawing2D.LinearGradientBrush(rect, Color.FromArgb(239, 232, 200), Color.White, Drawing2D.LinearGradientMode.Vertical)
                ' 使用 Graphics 將漸層填充到矩形區域
                g.FillRectangle(lgb, rect)
            End Using

        End Using

        Return bmp

    End Function

    Public Function TwoBmpStack(Bmp1 As Bitmap, Bmp2 As Bitmap) As Bitmap

        Dim NewBmp As New Bitmap(Bmp1.Width, Bmp1.Height)

        Using g As Graphics = Graphics.FromImage(NewBmp)
            g.DrawImage(Bmp1, 0, 0)
            g.DrawImage(Bmp2, 0, 0)
        End Using

        Return NewBmp

    End Function

    Public Function MirrorBitmap(originalBitmap As Bitmap, HorV As Integer) As Bitmap

        Dim mirroredBitmap As New Bitmap(originalBitmap.Width, originalBitmap.Height)

        ' 使用 Graphics 繪製到新 Bitmap 上
        Using g As Graphics = Graphics.FromImage(mirroredBitmap)
            ' 設置翻轉變換
            g.TranslateTransform(originalBitmap.Width, 0) ' 移動到右側

            If HorV = 0 Then '=0 水平 =1 垂直
                g.ScaleTransform(-1, 1) ' 水平翻轉
            Else
                g.ScaleTransform(1, -1) ' 水平翻轉
            End If

            ' 繪製原始 Bitmap
            g.DrawImage(originalBitmap, 0, 0)
        End Using

        Return mirroredBitmap

    End Function

    Public Function MakeRedArrow(bmp0 As Bitmap) As Bitmap

        Dim bmp1 As Bitmap = bmp0.Clone(New Rectangle(0, 0, bmp0.Width, bmp0.Height), PixelFormat.Format32bppArgb)

        Dim rect As New Rectangle(0, 0, bmp1.Width, bmp1.Height)
        Dim bmpData1 As System.Drawing.Imaging.BitmapData = bmp1.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp1.PixelFormat)
        Dim St1 As Integer = bmpData1.Stride

        ' 取得第一行的位址
        Dim ptr1 As IntPtr = bmpData1.Scan0

        ' 宣告一個陣列來儲存 Bitmap 的字節
        Dim bytes1 As Integer = Math.Abs(St1) * bmp1.Height
        Dim rgbValues1(bytes1 - 1) As Byte

        ' 將 RGB 值複製到陣列中
        System.Runtime.InteropServices.Marshal.Copy(ptr1, rgbValues1, 0, bytes1)

        Dim xyPos As Integer
        Dim ByteWidth As Integer = 4

        For IDX_Y As Integer = 0 To bmp1.Height - 1
            For IDX_X As Integer = 2 To (St1 - 1 - ByteWidth) Step ByteWidth

                xyPos = (IDX_Y * St1) + IDX_X
                If rgbValues1(xyPos) <= 168 Then rgbValues1(xyPos) = 168
                'rgbValues1(xyPos + 1) = 255
            Next
        Next

        ' 將修改過的 RGB 值複製回 Bitmap
        System.Runtime.InteropServices.Marshal.Copy(rgbValues1, 0, ptr1, bytes1)

        ' 解鎖位元
        bmp1.UnlockBits(bmpData1)

        Dim graphics As Graphics = graphics.FromImage(bmp1)

        graphics.Dispose()

        Return bmp1

    End Function


End Module



