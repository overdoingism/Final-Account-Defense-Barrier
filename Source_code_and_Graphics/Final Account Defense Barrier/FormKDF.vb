'MIT License 
'Copyright (c) 2023 overdoingism Lab.
'https://github.com/overdoingism/Final-Account-Defense-Barrier

Public Class FormKDF

    Public Progass10L As Integer
    Public Progass10R As Integer
    Public Progass10L2 As Integer
    Public Progass10R2 As Integer
    Public KDF_Type As Integer = 1

    Dim PWD_KDF_C As New Bitmap(UIArt.PWD_KDF_C)
    Dim IDXL As Integer = 0
    Dim IDXR As Integer = 0

    ' The KDF type caption with the glow it has as a part of the window picture (made in FormKDF_Load)
    Dim TypeCaption As Bitmap

    Private Sub FormKDF_Load(sender As Object, e As EventArgs) Handles Me.Load

        ' These two pictures come from the form's resources at 100% size: bring them to the UI scale.
        PR10.Image = UIArt.Enlarge(PR10.Image)
        PL10.Image = UIArt.Enlarge(PL10.Image)

        ' The KDF type caption is a control of its own on top of the window picture, and its lettering fills
        ' it. To glow like the rest of the window it shows the piece of "window with the caption on it,
        ' filtered together" that it covers, and it is made 3 design pixels larger on every side so that
        ' this glow has room around the letters.
        Dim CaptionAt As Rectangle = PicKDF_TYPE.Bounds
        Dim Room As Integer = UIArt.S(3)
        PicKDF_TYPE.Bounds = Rectangle.Inflate(CaptionAt, Room, Room)
        PicKDF_TYPE.Image = Nothing
        TypeCaption = UIArt.SpriteOnWindow("PWD_KDF", "KDFT_Types", KDF_Type - 1, 3, CaptionAt, PicKDF_TYPE.Bounds)

        Me.Visible = True
        System.Windows.Forms.Application.DoEvents()

        Dim IDXL_Done(10) As Byte
        Dim IDXR_Done(10) As Byte
        Dim IDXL_AllDone, IDXR_AllDone As Boolean

        Dim TmpIDX As Integer

        Do

            IDXL = ((Progass10L + Progass10L2) / 10) + 1
            IDXR = ((Progass10R + Progass10R2) / 10) + 1

            Threading.Thread.Sleep(10)

            If IDXR > 10 Then IDXR = 10
            If Not IDXR_AllDone Then
                For TmpIDX = 1 To IDXR
                    If IDXR_Done(TmpIDX) = 0 Then
                        Me.Controls("PR" & TmpIDX.ToString("D2")).Visible = True
                        IDXR_Done(TmpIDX) = 1
                    End If
                Next
            End If
            If IDXR = 10 Then
                For IDX01 As Integer = 1 To 9
                    CType(Me.Controls("PR" & IDX01.ToString("D2")), PictureBox).Image = PWD_KDF_C
                Next
                PR10.Visible = True
                IDXR_AllDone = True
            End If

            If IDXL > 10 Then IDXL = 10
            If Not IDXL_AllDone Then
                For TmpIDX = 1 To IDXL
                    If IDXL_Done(TmpIDX) = 0 Then
                        Me.Controls("PL" & TmpIDX.ToString("D2")).Visible = True
                        IDXL_Done(TmpIDX) = 1
                    End If
                Next
            End If
            If IDXL = 10 Then
                For IDX01 As Integer = 1 To 9
                    CType(Me.Controls("PL" & IDX01.ToString("D2")), PictureBox).Image = PWD_KDF_C
                Next
                PL10.Visible = True
                IDXL_AllDone = True
            End If

            System.Windows.Forms.Application.DoEvents()

            If IDXR_AllDone And IDXL_AllDone Then
                PWD_PASS.Visible = True
                System.Windows.Forms.Application.DoEvents()
                Threading.Thread.Sleep(700)
                Exit Do
            End If

        Loop

        Me.DialogResult = DialogResult.OK

    End Sub

    Private Sub PicKDF_TYPE_Paint(sender As Object, e As PaintEventArgs) Handles PicKDF_TYPE.Paint
        If TypeCaption Is Nothing Then Exit Sub
        e.Graphics.InterpolationMode = Drawing2D.InterpolationMode.NearestNeighbor
        e.Graphics.PixelOffsetMode = Drawing2D.PixelOffsetMode.Half
        e.Graphics.DrawImage(TypeCaption, New Rectangle(0, 0, TypeCaption.Width, TypeCaption.Height),
                             0, 0, TypeCaption.Width, TypeCaption.Height, GraphicsUnit.Pixel)
    End Sub

    Private Sub FormKDF_Disposed(sender As Object, e As EventArgs) Handles Me.Disposed
        If TypeCaption IsNot Nothing Then TypeCaption.Dispose()
        TypeCaption = Nothing
    End Sub


End Class