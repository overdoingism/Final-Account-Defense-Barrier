'MIT License
'Copyright (c) 2023 overdoingism Labs.
'https://github.com/overdoignism/Final-Account-Defense-Barrier

' UIArt glow ("shine") filter.
'
' Glow1x is the original The_Shine_Visual_Filter2, unchanged: a 7x7 light-spread kernel.
' The kernel size is in pixels, so applying it directly to a picture drawn at 175% would give a
' glow that is 1.75 times thinner. Glow() therefore works like this above 100%:
'
'   1. shrink the sharp picture to 100% and run the original filter on it,
'   2. keep only the light that the filter added around / on top of the shapes,
'   3. enlarge that soft light back and merge it with the sharp picture (per-channel maximum).
'
' The result has the same glow as the 100% picture, while the shapes keep their sharp edges.
' The cost is the same as at 100%, whatever the DPI.

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices

Partial Public NotInheritable Class UIArt

    Public Shared Function Glow(SrcBitmap As Bitmap) As Bitmap
        Return Glow(SrcBitmap, Scale)
    End Function

    Public Shared Function Glow(SrcBitmap As Bitmap, AtScale As Single) As Bitmap

        If Math.Abs(AtScale - 1.0F) < 0.001F Then Return Glow1x(SrcBitmap)

        Dim BigW As Integer = SrcBitmap.Width
        Dim BigH As Integer = SrcBitmap.Height
        Dim LowW As Integer = Math.Max(1, CInt(Math.Round(BigW / AtScale)))
        Dim LowH As Integer = Math.Max(1, CInt(Math.Round(BigH / AtScale)))

        '--- 1. 100% copy and its filtered version
        Dim LowPix() As Byte
        Dim FltPix() As Byte
        Dim FltW As Integer, FltH As Integer

        Using Low As New Bitmap(LowW, LowH, PixelFormat.Format32bppArgb)

            Using g As Graphics = Graphics.FromImage(Low)
                g.CompositingMode = CompositingMode.SourceCopy
                g.InterpolationMode = InterpolationMode.HighQualityBilinear
                g.PixelOffsetMode = PixelOffsetMode.Half
                Using ia As New ImageAttributes()
                    ia.SetWrapMode(WrapMode.TileFlipXY)
                    g.DrawImage(SrcBitmap, New Rectangle(0, 0, LowW, LowH), 0, 0, BigW, BigH, GraphicsUnit.Pixel, ia)
                End Using
            End Using

            LowPix = GetPixels(Low)

            Using Flt As Bitmap = Glow1x(Low)
                FltW = Flt.Width
                FltH = Flt.Height
                FltPix = GetPixels(Flt)
            End Using

        End Using

        '--- 2. Light field: what the filter produced where it is brighter than the picture itself.
        '       Pixels that the picture dominates take the strongest light of their neighbours,
        '       so the enlarged light reaches right up to the sharp edge of a shape.
        Dim Light(FltW * FltH * 3 - 1) As Byte
        Dim Lit(FltW * FltH * 3 - 1) As Boolean

        For y As Integer = 0 To FltH - 1
            For x As Integer = 0 To FltW - 1
                Dim fi As Integer = (y * FltW + x) * 4
                Dim li As Integer = (y * FltW + x) * 3
                Dim InLow As Boolean = (x < LowW AndAlso y < LowH)
                Dim pi As Integer = (y * LowW + x) * 4
                For c As Integer = 0 To 2
                    Dim OrgV As Integer = 0
                    If InLow AndAlso LowPix(pi + 3) > 0 Then OrgV = LowPix(pi + c)
                    If FltPix(fi + c) > OrgV Then
                        Light(li + c) = FltPix(fi + c)
                        Lit(li + c) = True
                    End If
                Next
            Next
        Next

        Dim Light2(Light.Length - 1) As Byte
        Array.Copy(Light, Light2, Light.Length)

        For y As Integer = 0 To FltH - 1
            For x As Integer = 0 To FltW - 1
                Dim li As Integer = (y * FltW + x) * 3
                For c As Integer = 0 To 2
                    If Not Lit(li + c) Then
                        Dim Best As Byte = 0
                        For ny As Integer = Math.Max(0, y - 1) To Math.Min(FltH - 1, y + 1)
                            For nx As Integer = Math.Max(0, x - 1) To Math.Min(FltW - 1, x + 1)
                                Dim ni As Integer = (ny * FltW + nx) * 3 + c
                                If Lit(ni) AndAlso Light(ni) > Best Then Best = Light(ni)
                            Next
                        Next
                        Light2(li + c) = Best
                    End If
                Next
            Next
        Next

        '--- 3. Merge the enlarged light with the sharp picture
        Dim Margin As Integer = Math.Max(1, CInt(Math.Round(2 * AtScale)))
        Dim OutBmp As New Bitmap(BigW + Margin, BigH + Margin, PixelFormat.Format32bppArgb)

        Using g As Graphics = Graphics.FromImage(OutBmp)
            g.Clear(Color.FromArgb(0, 0, 0, 0))
            g.CompositingMode = CompositingMode.SourceCopy
            g.InterpolationMode = InterpolationMode.NearestNeighbor
            g.PixelOffsetMode = PixelOffsetMode.Half
            g.DrawImage(SrcBitmap, New Rectangle(0, 0, BigW, BigH), 0, 0, BigW, BigH, GraphicsUnit.Pixel)
        End Using

        Dim Rect As New Rectangle(0, 0, OutBmp.Width, OutBmp.Height)
        Dim BD As BitmapData = OutBmp.LockBits(Rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb)
        Dim Stride As Integer = BD.Stride
        Dim OutPix(Stride * OutBmp.Height - 1) As Byte
        Marshal.Copy(BD.Scan0, OutPix, 0, OutPix.Length)

        Dim StepX As Single = CSng(LowW) / CSng(BigW)
        Dim StepY As Single = CSng(LowH) / CSng(BigH)

        ' Horizontal sampling positions are the same for every row.
        Dim X0(BigW - 1) As Integer
        Dim X1(BigW - 1) As Integer
        Dim XF(BigW - 1) As Single
        For x As Integer = 0 To BigW - 1
            Dim sx As Single = (x + 0.5F) * StepX - 0.5F
            If sx < 0 Then sx = 0
            Dim ix As Integer = CInt(Math.Floor(sx))
            If ix > FltW - 1 Then ix = FltW - 1
            X0(x) = ix * 3
            X1(x) = Math.Min(ix + 1, FltW - 1) * 3
            XF(x) = sx - ix
        Next

        For y As Integer = 0 To BigH - 1

            Dim sy As Single = (y + 0.5F) * StepY - 0.5F
            If sy < 0 Then sy = 0
            Dim iy As Integer = CInt(Math.Floor(sy))
            If iy > FltH - 1 Then iy = FltH - 1
            Dim fy As Single = sy - iy
            Dim Row0 As Integer = iy * FltW * 3
            Dim Row1 As Integer = Math.Min(iy + 1, FltH - 1) * FltW * 3
            Dim OutRow As Integer = y * Stride

            For x As Integer = 0 To BigW - 1

                Dim oi As Integer = OutRow + x * 4
                If OutPix(oi + 3) = 0 Then Continue For 'transparent: nothing would show

                'Keep the pure green (form transparency key) untouched, as the original filter does.
                If OutPix(oi + 1) = 255 AndAlso OutPix(oi) = 0 AndAlso OutPix(oi + 2) = 0 Then Continue For

                Dim fx As Single = XF(x)
                Dim a As Integer = Row0 + X0(x)
                Dim b As Integer = Row0 + X1(x)
                Dim c2 As Integer = Row1 + X0(x)
                Dim d As Integer = Row1 + X1(x)

                For c As Integer = 0 To 2
                    Dim Top As Single = Light2(a + c) + (CSng(Light2(b + c)) - Light2(a + c)) * fx
                    Dim Bot As Single = Light2(c2 + c) + (CSng(Light2(d + c)) - Light2(c2 + c)) * fx
                    Dim V As Integer = CInt(Top + (Bot - Top) * fy)
                    If V > OutPix(oi + c) Then OutPix(oi + c) = CByte(Math.Min(255, V))
                Next

            Next
        Next

        Marshal.Copy(OutPix, 0, BD.Scan0, OutPix.Length)
        OutBmp.UnlockBits(BD)

        Return OutBmp

    End Function

    Private Shared Function GetPixels(Bmp As Bitmap) As Byte()
        Dim Rect As New Rectangle(0, 0, Bmp.Width, Bmp.Height)
        Dim BD As BitmapData = Bmp.LockBits(Rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
        Dim Pix(Bmp.Width * Bmp.Height * 4 - 1) As Byte
        For y As Integer = 0 To Bmp.Height - 1
            Marshal.Copy(IntPtr.Add(BD.Scan0, y * BD.Stride), Pix, y * Bmp.Width * 4, Bmp.Width * 4)
        Next
        Bmp.UnlockBits(BD)
        Return Pix
    End Function

    '=========================================================================================
    ' The original filter (formerly VisualModule.The_Shine_Visual_Filter2), logic unchanged.
    '=========================================================================================

    Private Shared Function CenterBitmapInNewImage(ByVal originalBmp As Bitmap) As Bitmap

        ' 設定新圖片的大小，寬高各加 6
        Dim newWidth As Integer = originalBmp.Width + 6
        Dim newHeight As Integer = originalBmp.Height + 6
        Dim newBmp As New Bitmap(newWidth, newHeight)

        ' 使用 Graphics 將原始圖片繪製到新圖片的中間位置
        Using g As Graphics = Graphics.FromImage(newBmp)

            g.Clear(Color.FromArgb(0, 0, 0, 0))

            ' 計算置中的位置
            Dim x As Integer = (newWidth - originalBmp.Width) \ 2
            Dim y As Integer = (newHeight - originalBmp.Height) \ 2

            ' 繪製原始圖片到新圖片中 (明確指定尺寸，避免圖檔 DPI 標記造成縮放)
            g.DrawImage(originalBmp, x, y, originalBmp.Width, originalBmp.Height)
        End Using

        ' 返回新圖片
        Return newBmp
    End Function

    Private Shared Function CropBitmap(ByVal original As Bitmap) As Bitmap
        ' 定義要裁剪的區域 (原圖尺寸減去上下左右各 2 像素)
        Dim cropArea As New Rectangle(3, 3, original.Width - 4, original.Height - 4)

        ' 創建一個新 Bitmap，尺寸為裁剪區域的尺寸
        Dim croppedBitmap As New Bitmap(cropArea.Width, cropArea.Height)

        ' 使用 Graphics 將原始圖片的裁剪區域繪製到新 Bitmap 上
        Using g As Graphics = Graphics.FromImage(croppedBitmap)
            g.DrawImage(original, New Rectangle(0, 0, croppedBitmap.Width, croppedBitmap.Height), cropArea, GraphicsUnit.Pixel)
        End Using

        ' 返回裁剪後的 Bitmap
        Return croppedBitmap
    End Function

    Public Shared Function Glow1x(orgbitmap As Bitmap) As Bitmap

        Dim bmp1 As Bitmap = CenterBitmapInNewImage(orgbitmap)

        ' 鎖定 Bitmap 的位元
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

        Dim rgbValues2 As Single()
        rgbValues2 = Byte2Single(rgbValues1)


        Dim TheMartix() As Single = {0.003F, 0.006F, 0.012F, 0.018F, 0.012F, 0.006F, 0.003F,
                                     0.006F, 0.012F, 0.018F, 0.038F, 0.018F, 0.012F, 0.006F,
                                     0.012F, 0.018F, 0.038F, 0.12F, 0.038F, 0.018F, 0.012F,
                                     0.018F, 0.038F, 0.12F, 0.0F, 0.12F, 0.038F, 0.018F,
                                     0.012F, 0.018F, 0.038F, 0.12F, 0.038F, 0.018F, 0.012F,
                                     0.006F, 0.012F, 0.018F, 0.038F, 0.018F, 0.012F, 0.006F,
                                     0.003F, 0.006F, 0.012F, 0.018F, 0.012F, 0.006F, 0.003F}


        Dim xyPos, xyPosShift As Integer
        Dim xyPosRGB(2) As Single
        Dim Stride7Pos() As Integer = {St1 * -3, St1 * -2, St1 * -1, 0, St1 * 1, St1 * 2, St1 * 3}
        Dim Y_Scan_Pos As Integer
        Dim TheMartixCount As Integer

        Dim ByteWidth As Integer = 4
        Dim Factory1 As Single = 0.9787
        Dim passMode As Integer

        'Search Area Brightest
        Dim BrightestMap(rgbValues1.GetUpperBound(0)) As Byte
        Dim BrightestMapSng() As Single
        For IDX_Y As Integer = 3 To bmp1.Height - 4
            For IDX_X As Integer = 3 To bmp1.Width - 4
                xyPos = (IDX_Y * St1) + (IDX_X * ByteWidth)
                For Y_Scan As Integer = St1 * -3 To St1 * 3 Step St1
                    Y_Scan_Pos = xyPos + Y_Scan
                    For X_Scan_Pos As Integer = -12 To 12 Step ByteWidth
                        xyPosShift = Y_Scan_Pos + X_Scan_Pos
                        BrightestMap(xyPos) = Math.Max(rgbValues1(xyPosShift), BrightestMap(xyPos))
                        BrightestMap(xyPos + 1) = Math.Max(rgbValues1(xyPosShift + 1), BrightestMap(xyPos + 1))
                        BrightestMap(xyPos + 2) = Math.Max(rgbValues1(xyPosShift + 2), BrightestMap(xyPos + 2))
                    Next
                Next

            Next
        Next
        BrightestMapSng = Byte2Single(BrightestMap)

        'Pass1
        For IDX_Y As Integer = 3 To bmp1.Height - 4
            For IDX_X As Integer = 3 To bmp1.Width - 4

                xyPos = (IDX_Y * St1) + (IDX_X * ByteWidth)

                passMode = 0
                If rgbValues1(xyPos) = 0 AndAlso rgbValues1(xyPos + 1) = 0 AndAlso rgbValues1(xyPos + 2) = 0 Then
                    passMode = 1
                ElseIf rgbValues1(xyPos + 3) = 0 Then
                    passMode = 1
                End If

                If passMode = 1 Then

                    rgbValues2(0) = CSng(rgbValues1(xyPos))
                    rgbValues2(1) = CSng(rgbValues1(xyPos + 1))
                    rgbValues2(2) = CSng(rgbValues1(xyPos + 2))

                Else

                    xyPosRGB(0) = CSng(rgbValues1(xyPos))
                    xyPosRGB(1) = CSng(rgbValues1(xyPos + 1))
                    xyPosRGB(2) = CSng(rgbValues1(xyPos + 2))

                    TheMartixCount = 0
                    For Y_Scan As Integer = 0 To 6

                        Y_Scan_Pos = xyPos + Stride7Pos(Y_Scan)

                        For X_Scan_Pos As Integer = -12 To 12 Step ByteWidth

                            xyPosShift = Y_Scan_Pos + X_Scan_Pos

                            rgbValues2(xyPosShift) = Math.Min((rgbValues2(xyPosShift) + (xyPosRGB(0) * TheMartix(TheMartixCount))) * Factory1, BrightestMapSng(xyPosShift))
                            rgbValues2(xyPosShift + 1) = Math.Min((rgbValues2(xyPosShift + 1) + (xyPosRGB(1) * TheMartix(TheMartixCount))) * Factory1, BrightestMapSng(xyPosShift + 1))
                            rgbValues2(xyPosShift + 2) = Math.Min((rgbValues2(xyPosShift + 2) + (xyPosRGB(2) * TheMartix(TheMartixCount))) * Factory1, BrightestMapSng(xyPosShift + 2))

                            TheMartixCount += 1
                        Next
                    Next
                End If

            Next
        Next

        'Pass2
        For IDX_Y As Integer = 0 To bmp1.Height - 1
            For IDX_X As Integer = 0 To bmp1.Width - 1

                xyPos = (IDX_Y * St1) + (IDX_X * ByteWidth)
                passMode = 0

                If CInt(rgbValues1(xyPos)) + CInt(rgbValues1(xyPos + 2)) = 0 Then
                    If rgbValues1(xyPos + 1) = 255 Then
                        rgbValues2(xyPos) = 0
                        rgbValues2(xyPos + 1) = 255
                        rgbValues2(xyPos + 2) = 0
                        passMode = 1
                    End If
                End If


                If passMode = 0 Then
                    rgbValues2(xyPos) = Math.Max(rgbValues1(xyPos), rgbValues2(xyPos))
                    rgbValues2(xyPos + 1) = Math.Max(rgbValues1(xyPos + 1), rgbValues2(xyPos + 1))
                    rgbValues2(xyPos + 2) = Math.Max(rgbValues1(xyPos + 2), rgbValues2(xyPos + 2))
                End If

            Next
        Next

        rgbValues1 = Single2Byte(rgbValues2)

        ' 將修改過的 RGB 值複製回 Bitmap
        System.Runtime.InteropServices.Marshal.Copy(rgbValues1, 0, ptr1, bytes1)

        ' 解鎖位元
        bmp1.UnlockBits(bmpData1)

        Dim Result As Bitmap = CropBitmap(bmp1)
        bmp1.Dispose()

        Return Result

    End Function

    Private Shared Function Byte2Single(InputByte() As Byte) As Single()

        Dim UpCount As Integer = InputByte.Length - 1
        Dim TmpSingle(UpCount) As Single

        For IDX As Integer = 0 To UpCount
            TmpSingle(IDX) = CSng(InputByte(IDX))
        Next

        Return TmpSingle

    End Function

    Private Shared Function Single2Byte(InputSng() As Single) As Byte()

        Dim UpCount As Integer = InputSng.Length - 1
        Dim TmpByte(UpCount) As Byte

        For IDX As Integer = 0 To UpCount
            TmpByte(IDX) = CByte(InputSng(IDX))
        Next

        Return TmpByte

    End Function

End Class
