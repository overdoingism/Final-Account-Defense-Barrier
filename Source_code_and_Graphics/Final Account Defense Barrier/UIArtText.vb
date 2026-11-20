'MIT License
'Copyright (c) 2023 overdoingism Labs.
'https://github.com/overdoignism/Final-Account-Defense-Barrier

' UIArt lettering.
'
' The lettering of the UI pictures is rebuilt from the original PSD text layers by
' the art tool (arttool, kept outside this repository), in two forms:
'
'   * Zen Antique (SIL OFL) text is drawn at run time from outlines. UIArtGlyphs.bin holds the
'     outlines of just the characters the UI uses; UIArtText.txt lists the "text runs": picture,
'     colour, text and the rectangle its ink occupies in the artwork (96-DPI design pixels).
'     The outline is scaled to fill exactly that rectangle. No font file is ever loaded.
'     The count-down digits and their colon are DSEG7 Classic (SIL OFL as well) and are kept the same
'     way; in both files its characters are U+E000 + character code, apart from Zen Antique's.
'
'   * Everything else (Arial, mixed fonts) is pre-rendered by arttool at 200% into the
'     "<picture>@2x" layer pictures, so the program does not depend on the fonts of the machine
'     it runs on and the lettering looks the same everywhere.
'
' Text that arttool could not reproduce closely enough simply stays in the 100% layer picture.

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.IO

Partial Public NotInheritable Class UIArt

    Public NotInheritable Class TextRun
        Public Ink As Color
        Public Box As RectangleF      'ink rectangle in design pixels
        Public Bold As Single         'extra outline thickness ("faux bold"), as a fraction of the em size
        Public Track As Single        'extra space after each character, as a fraction of the em size
        Public Tracks() As Single     'the same per character, added to Track (Nothing = none)
        Public Late As Boolean        'lies on a pattern of the layer picture: drawn after that picture
        Public LinePitch As Single    'distance between lines, as a fraction of the em size
        Public Text As String
        Public Ch As Char             'glyph picture: the character it shows
    End Class

    Private NotInheritable Class Glyph
        Public Advance As Single
        Public Points() As PointF
        Public Types() As Byte
    End Class

    ''' <summary>Line break inside the text of a run.</summary>
    Public Const LineBreak As Char = ChrW(&H2028)

    ''' <summary>The glyph outlines are stored at this em size.</summary>
    Public Const GlyphEm As Single = 1000.0F

    ' Point types in the glyph file besides the GDI+ ones (0 start, 1 line, 3 cubic, &H80 close figure):
    ' a quadratic curve is stored as its control point (QuadControl) and its end point (QuadEnd).
    Public Const QuadControl As Byte = 5
    Public Const QuadEnd As Byte = 6

    Private Shared _runs As Dictionary(Of String, List(Of TextRun)) = Nothing

    ' Plain background patches. A picture without a painter is one bitmap layer; where its lettering
    ' is redrawn, arttool cuts the old lettering out of the layer and the hole is backed by a patch
    ' of the (flat) background colour, drawn here underneath.
    Private Shared _fills As Dictionary(Of String, List(Of TextRun)) = Nothing

    ' Lettering kept as small 200% pictures of their own ("pieces"): Text = name of the layer picture,
    ' Box = where it lies. Several pictures can use the same piece (the word "PAGE" of the licence pages).
    Private Shared _pieces As Dictionary(Of String, List(Of TextRun)) = Nothing

    ' Characters kept as 200% pictures ("glyph pictures") that the program sets itself, see DrawGlyphRows:
    ' Text = name of the layer picture, Ch = the character, Box = where it lies as the first character of the first row.
    Private Shared _glyphPics As Dictionary(Of String, List(Of TextRun)) = Nothing
    Private Shared _glyphs As Dictionary(Of Char, Glyph) = Nothing
    Private Shared ReadOnly _textLock As New Object

    ''' <summary>Tool use: replaces the run list (format of UIArtText.txt).</summary>
    Public Shared Sub SetTextRuns(Content As String)
        SyncLock _textLock
            _fills = New Dictionary(Of String, List(Of TextRun))(StringComparer.Ordinal)
            _pieces = New Dictionary(Of String, List(Of TextRun))(StringComparer.Ordinal)
            _glyphPics = New Dictionary(Of String, List(Of TextRun))(StringComparer.Ordinal)
            _runs = ParseRuns(Content)
        End SyncLock
    End Sub

    ''' <summary>Tool use: replaces the glyph outlines (content of UIArtGlyphs.bin).</summary>
    Public Shared Sub SetGlyphData(Data() As Byte)
        SyncLock _textLock
            _glyphs = ParseGlyphs(Data)
        End SyncLock
    End Sub

    Private Shared Function EmbeddedBytes(ResName As String) As Byte()
        Using St As Stream = GetType(UIArt).Assembly.GetManifestResourceStream(ResName)
            If St Is Nothing Then Return Nothing
            Using Ms As New MemoryStream()
                St.CopyTo(Ms)
                Return Ms.ToArray()
            End Using
        End Using
    End Function

    ''' <summary>
    ''' One run per line: picture TAB colour(hex) TAB x0 TAB y0 TAB x1 TAB y1 TAB bold TAB track TAB linePitch TAB text
    ''' track is one number, or "number/count:value,count:value..." = further spacing for each stretch of characters.
    ''' A further column "L" marks lettering that is drawn after the layer picture (see DrawLateText).
    ''' A background patch has the text column "@FILL" (bold / track unused).
    ''' A piece has the text column "@IMG:" + name of its layer picture; its box is the area that picture covers.
    ''' A glyph picture has "@GLYPH:" + character + ":" + name of its layer picture (see DrawGlyphRows).
    ''' </summary>
    Private Shared Function ParseRuns(Content As String) As Dictionary(Of String, List(Of TextRun))

        Dim Res As New Dictionary(Of String, List(Of TextRun))(StringComparer.Ordinal)
        Dim Inv As Globalization.CultureInfo = Globalization.CultureInfo.InvariantCulture
        If Content Is Nothing Then Return Res

        For Each RawLine As String In Content.Split({ControlChars.Lf})
            Dim Line As String = RawLine.TrimEnd(ControlChars.Cr)
            If Line.Length = 0 OrElse Line.StartsWith("#") Then Continue For
            Dim P() As String = Line.Split(ControlChars.Tab)
            If P.Length < 10 Then Continue For
            Try
                Dim R As New TextRun
                R.Ink = ArtCanvas.RGB(Convert.ToInt32(P(1), 16))
                R.Box = RectangleF.FromLTRB(Single.Parse(P(2), Inv), Single.Parse(P(3), Inv), Single.Parse(P(4), Inv), Single.Parse(P(5), Inv))
                R.Bold = Single.Parse(P(6), Inv)
                Dim Tk() As String = P(7).Split("/"c)
                R.Track = Single.Parse(Tk(0), Inv)
                If Tk.Length > 1 Then
                    Dim Per As New List(Of Single)
                    For Each Item As String In Tk(1).Split(","c)
                        Dim Kv() As String = Item.Split(":"c)
                        For i As Integer = 1 To Integer.Parse(Kv(0))
                            Per.Add(Single.Parse(Kv(1), Inv))
                        Next
                    Next
                    R.Tracks = Per.ToArray()
                End If
                R.LinePitch = Single.Parse(P(8), Inv)
                R.Text = P(9)
                R.Late = (P.Length > 10 AndAlso P(10) = "L")
                Dim Target As Dictionary(Of String, List(Of TextRun)) = If(R.Text = "@FILL", _fills, Res)
                If R.Text.StartsWith("@IMG:") Then
                    Target = _pieces
                    R.Text = R.Text.Substring(5)
                ElseIf R.Text.StartsWith("@GLYPH:") Then
                    Target = _glyphPics
                    R.Ch = R.Text(7)
                    R.Text = R.Text.Substring(9)
                End If
                Dim L As List(Of TextRun) = Nothing
                If Not Target.TryGetValue(P(0), L) Then
                    L = New List(Of TextRun)
                    Target(P(0)) = L
                End If
                L.Add(R)
            Catch ex As Exception
                'a malformed line is ignored: that text simply is not drawn
            End Try
        Next

        Return Res

    End Function

    ''' <summary>
    ''' Glyph file: deflate stream of  count(Int32), then per glyph: char(UInt16), advance(Int16),
    ''' pointCount(UInt16), pointCount x (dx Int16, dy Int16), pointCount x pointType(Byte).
    ''' Coordinates are in the font's own units (1000 per em).
    ''' Each point is stored as the difference to the previous one (compresses much better).
    ''' </summary>
    Private Shared Function ParseGlyphs(Data() As Byte) As Dictionary(Of Char, Glyph)

        Dim Res As New Dictionary(Of Char, Glyph)
        If Data Is Nothing OrElse Data.Length < 4 Then Return Res

        Try
            Using Ms As New MemoryStream(Data)
                Using Ds As New Compression.DeflateStream(Ms, Compression.CompressionMode.Decompress)
                    Using Br As New BinaryReader(Ds)
                        Dim Count As Integer = Br.ReadInt32()
                        For i As Integer = 1 To Count
                            Dim Ch As Char = ChrW(Br.ReadUInt16())
                            Dim G As New Glyph
                            G.Advance = Br.ReadInt16()
                            Dim N As Integer = Br.ReadUInt16()
                            Dim Raw(N - 1) As PointF
                            Dim X As Integer = 0, Y As Integer = 0
                            For k As Integer = 0 To N - 1
                                X += Br.ReadInt16()
                                Y += Br.ReadInt16()
                                Raw(k) = New PointF(X, Y)
                            Next
                            Dim RawTy() As Byte = Br.ReadBytes(N)

                            ' back to what GDI+ wants: every quadratic curve as a cubic one
                            Dim Pts As New List(Of PointF)(N + N \ 2)
                            Dim Tys As New List(Of Byte)(N + N \ 2)
                            For k As Integer = 0 To N - 1
                                If RawTy(k) = QuadControl AndAlso k > 0 AndAlso k + 1 < N Then
                                    Dim P0 As PointF = Raw(k - 1), Q As PointF = Raw(k), P2 As PointF = Raw(k + 1)
                                    Pts.Add(New PointF(P0.X + (Q.X - P0.X) * 2 / 3, P0.Y + (Q.Y - P0.Y) * 2 / 3))
                                    Tys.Add(3)
                                    Pts.Add(New PointF(P2.X + (Q.X - P2.X) * 2 / 3, P2.Y + (Q.Y - P2.Y) * 2 / 3))
                                    Tys.Add(3)
                                ElseIf (RawTy(k) And 7) = QuadEnd Then
                                    Pts.Add(Raw(k))
                                    Tys.Add(CByte(3 Or (RawTy(k) And &H80)))
                                Else
                                    Pts.Add(Raw(k))
                                    Tys.Add(RawTy(k))
                                End If
                            Next
                            G.Points = Pts.ToArray()
                            G.Types = Tys.ToArray()
                            Res(Ch) = G
                        Next
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Res.Clear()
        End Try

        Return Res

    End Function

    Private Shared Sub EnsureText()
        If _runs IsNot Nothing AndAlso _glyphs IsNot Nothing Then Exit Sub
        SyncLock _textLock
            If _runs Is Nothing Then
                _fills = New Dictionary(Of String, List(Of TextRun))(StringComparer.Ordinal)
                _pieces = New Dictionary(Of String, List(Of TextRun))(StringComparer.Ordinal)
                _glyphPics = New Dictionary(Of String, List(Of TextRun))(StringComparer.Ordinal)
                Dim B() As Byte = EmbeddedBytes("UIArtText.txt")
                _runs = ParseRuns(If(B Is Nothing, Nothing, System.Text.Encoding.UTF8.GetString(B)))
            End If
            If _glyphs Is Nothing Then _glyphs = ParseGlyphs(EmbeddedBytes("UIArtGlyphs.bin"))
        End SyncLock
    End Sub

    Public Shared Function HasText(Name As String) As Boolean
        EnsureText()
        Return _runs.ContainsKey(Name) OrElse _fills.ContainsKey(Name)
    End Function

    ''' <summary>Outline of a Zen Antique string at GlyphEm size, or Nothing if a character has no stored outline.</summary>
    Public Shared Function ZenOutline(Text As String, Optional Track As Single = 0.0F, Optional LinePitch As Single = 1.2F, Optional Tracks() As Single = Nothing) As GraphicsPath

        EnsureText()
        Dim Gp As New GraphicsPath(FillMode.Winding)
        Dim Pen As Single = 0
        Dim PenY As Single = 0

        For i As Integer = 0 To Text.Length - 1
            Dim Ch As Char = Text(i)
            If Ch = LineBreak Then
                Pen = 0
                PenY += LinePitch * GlyphEm
                Continue For
            End If
            Dim G As Glyph = Nothing
            If Not _glyphs.TryGetValue(Ch, G) Then
                Gp.Dispose()
                Return Nothing
            End If
            If G.Points.Length > 0 Then
                Dim Pts(G.Points.Length - 1) As PointF
                For k As Integer = 0 To Pts.Length - 1
                    Pts(k) = New PointF(G.Points(k).X + Pen, G.Points(k).Y + PenY)
                Next
                Using One As New GraphicsPath(Pts, G.Types, FillMode.Winding)
                    Gp.AddPath(One, False)
                End Using
            End If
            Pen += G.Advance + Track * GlyphEm
            If Tracks IsNot Nothing AndAlso i < Tracks.Length Then Pen += Tracks(i) * GlyphEm
        Next

        If Gp.PointCount = 0 Then
            Gp.Dispose()
            Return Nothing
        End If

        Return Gp

    End Function

    ''' <summary>
    ''' Fills an outline so that its ink (including the extra PenWidth, given in outline units)
    ''' covers exactly Box (in surface pixels of G).
    ''' </summary>
    Public Shared Sub FillOutlineInBox(G As Graphics, Gp As GraphicsPath, PenWidth As Single, Box As RectangleF, Ink As Color)

        ' true ink bounds: the control points of the curves may lie outside the outline
        Dim B As RectangleF
        Using Flat As GraphicsPath = CType(Gp.Clone(), GraphicsPath)
            Flat.Flatten(Nothing, 0.5F)
            B = Flat.GetBounds()
        End Using
        B.Inflate(PenWidth / 2, PenWidth / 2)
        If B.Width <= 0 OrElse B.Height <= 0 OrElse Box.Width <= 0 OrElse Box.Height <= 0 Then Exit Sub

        Dim Saved As Drawing2D.GraphicsState = G.Save()
        Try
            G.TranslateTransform(Box.Left, Box.Top)
            G.ScaleTransform(Box.Width / B.Width, Box.Height / B.Height)
            G.TranslateTransform(-B.Left, -B.Top)
            Using Br As New SolidBrush(Ink)
                G.FillPath(Br, Gp)
            End Using
            If PenWidth > 0 Then
                Using Pn As New Pen(Ink, PenWidth)
                    Pn.LineJoin = LineJoin.Round
                    G.DrawPath(Pn, Gp)
                End Using
            End If
        Finally
            G.Restore(Saved)
        End Try

    End Sub

    ''' <summary>
    ''' Draws the Zen Antique text runs of a picture onto the painter surface
    ''' (Late = False: the ones under the layer picture, True: the ones over it).
    ''' </summary>
    Private Shared Sub DrawTextRuns(cv As ArtCanvas, Name As String, Optional Late As Boolean = False)

        EnsureText()
        Dim U As Single = cv.Sc * cv.K

        Dim Fills As List(Of TextRun) = Nothing
        If Not Late AndAlso _fills.TryGetValue(Name, Fills) Then
            For Each F As TextRun In Fills
                Using Br As New SolidBrush(F.Ink)
                    cv.G.FillRectangle(Br, F.Box.Left * U, F.Box.Top * U, F.Box.Width * U, F.Box.Height * U)
                End Using
            Next
        End If

        Dim Runs As List(Of TextRun) = Nothing
        If Not _runs.TryGetValue(Name, Runs) Then Exit Sub

        For Each R As TextRun In Runs
            If R.Late <> Late Then Continue For
            Using Gp As GraphicsPath = ZenOutline(R.Text, R.Track, R.LinePitch, R.Tracks)
                If Gp Is Nothing Then Continue For
                FillOutlineInBox(cv.G, Gp, R.Bold * GlyphEm,
                                 New RectangleF(R.Box.Left * U, R.Box.Top * U, R.Box.Width * U, R.Box.Height * U), R.Ink)
            End Using
        Next

    End Sub

    ''' <summary>
    ''' True when a layer picture of the picture itself - its 200% layer ("<picture>@2x") or what is left of the
    ''' artwork at 100% ("<picture>") - is listed as a piece: it then covers only the area given there instead
    ''' of the whole picture.
    ''' </summary>
    Private Shared Function LayerIsPlaced(Name As String, Layer As String) As Boolean
        EnsureText()
        Dim L As List(Of TextRun) = Nothing
        If Not _pieces.TryGetValue(Name, L) Then Return False
        For Each R As TextRun In L
            If String.Equals(R.Text, Layer, StringComparison.Ordinal) Then Return True
        Next
        Return False
    End Function

    ''' <summary>
    ''' Draws the pieces of a picture (see _pieces) onto it, in the order they are listed: Leftover = the
    ''' piece that is the picture's own 100% layer, Lettering = all the others.
    ''' </summary>
    Private Shared Sub DrawPieces(Bmp As Bitmap, Name As String, AtScale As Single, Leftover As Boolean, Lettering As Boolean)

        EnsureText()
        Dim L As List(Of TextRun) = Nothing
        If Not _pieces.TryGetValue(Name, L) Then Exit Sub

        Using g As Graphics = Graphics.FromImage(Bmp)
            g.CompositingMode = CompositingMode.SourceOver
            g.PixelOffsetMode = PixelOffsetMode.Half
            For Each R As TextRun In L
                If Not If(String.Equals(R.Text, Name, StringComparison.Ordinal), Leftover, Lettering) Then Continue For
                Using Pc As Bitmap = LayerProvider(R.Text)
                    If Pc Is Nothing Then Continue For
                    Dim X0 As Single = R.Box.Left * AtScale, Y0 As Single = R.Box.Top * AtScale
                    Dim X1 As Single = R.Box.Right * AtScale, Y1 As Single = R.Box.Bottom * AtScale
                    ' at the scale the piece was made for (200%, or 100% for a leftover) it is copied pixel by pixel
                    If Math.Abs(X1 - X0 - Pc.Width) < 0.01F AndAlso Math.Abs(Y1 - Y0 - Pc.Height) < 0.01F AndAlso
                       Math.Abs(X0 - CSng(Math.Round(X0))) < 0.01F AndAlso Math.Abs(Y0 - CSng(Math.Round(Y0))) < 0.01F Then
                        g.InterpolationMode = InterpolationMode.NearestNeighbor
                        g.DrawImage(Pc, New Rectangle(CInt(Math.Round(X0)), CInt(Math.Round(Y0)), Pc.Width, Pc.Height), 0, 0, Pc.Width, Pc.Height, GraphicsUnit.Pixel)
                    Else
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic
                        Using ia As New Imaging.ImageAttributes()
                            ia.SetWrapMode(WrapMode.TileFlipXY)
                            g.DrawImage(Pc, New PointF() {New PointF(X0, Y0), New PointF(X1, Y0), New PointF(X0, Y1)},
                                        New RectangleF(0, 0, Pc.Width, Pc.Height), GraphicsUnit.Pixel, ia)
                        End Using
                    End If
                End Using
            Next
        End Using

    End Sub

    ''' <summary>
    ''' Sets rows of characters from the glyph pictures of a picture ("@GLYPH:" lines). Arial lettering can only be
    ''' kept as pictures; where the program has to treat each character on its own, it gets one picture per
    ''' character instead of one of the whole text. RowTops: distance of each row below the first one, Pitch:
    ''' distance from character to character (design pixels); Level gives the brightness (0..1) of the next character.
    ''' </summary>
    Private Shared Sub DrawGlyphRows(Bmp As Bitmap, Name As String, AtScale As Single, Rows() As String, RowTops() As Single,
                                     Pitch As Single, Level As Func(Of Single))

        EnsureText()
        Dim L As List(Of TextRun) = Nothing
        If Not _glyphPics.TryGetValue(Name, L) Then Exit Sub

        Dim Pics As New Dictionary(Of Char, Bitmap)
        Dim Boxes As New Dictionary(Of Char, RectangleF)
        For Each R As TextRun In L
            Dim Pc As Bitmap = LayerProvider(R.Text)
            If Pc Is Nothing Then Continue For
            Pics(R.Ch) = Pc
            Boxes(R.Ch) = R.Box
        Next

        ' at 200% the pictures are copied pixel for pixel (placed on whole pixels), otherwise scaled
        Dim Exact As Boolean = Math.Abs(AtScale - 2.0F) < 0.001F

        Using g As Graphics = Graphics.FromImage(Bmp)
            g.CompositingMode = CompositingMode.SourceOver
            g.PixelOffsetMode = PixelOffsetMode.Half
            g.InterpolationMode = If(Exact, InterpolationMode.NearestNeighbor, InterpolationMode.HighQualityBicubic)
            Using ia As New Imaging.ImageAttributes()
                ia.SetWrapMode(WrapMode.TileFlipXY)
                For Row As Integer = 0 To Rows.Length - 1
                    For n As Integer = 0 To Rows(Row).Length - 1
                        Dim Pc As Bitmap = Nothing
                        If Not Pics.TryGetValue(Rows(Row)(n), Pc) Then Continue For
                        Dim B As RectangleF = Boxes(Rows(Row)(n))
                        Dim X0 As Single = (B.Left + n * Pitch) * AtScale, Y0 As Single = (B.Top + RowTops(Row)) * AtScale
                        Dim Lv As Single = Level()
                        Dim Dim1 As New Imaging.ColorMatrix()
                        Dim1.Matrix00 = Lv
                        Dim1.Matrix11 = Lv
                        Dim1.Matrix22 = Lv
                        ia.SetColorMatrix(Dim1)
                        If Exact Then
                            g.DrawImage(Pc, New Rectangle(CInt(Math.Round(X0)), CInt(Math.Round(Y0)), Pc.Width, Pc.Height),
                                        0, 0, Pc.Width, Pc.Height, GraphicsUnit.Pixel, ia)
                        Else
                            g.DrawImage(Pc, New PointF() {New PointF(X0, Y0), New PointF(X0 + B.Width * AtScale, Y0), New PointF(X0, Y0 + B.Height * AtScale)},
                                        New RectangleF(0, 0, Pc.Width, Pc.Height), GraphicsUnit.Pixel, ia)
                        End If
                    Next
                Next
            End Using
        End Using

        For Each Pc As Bitmap In Pics.Values
            Pc.Dispose()
        Next

    End Sub

    ''' <summary>
    ''' Lettering that lies on a pattern which only the layer picture has (the honeycomb behind the
    ''' notice of the login window): the pattern must stay visible between the letters, so this
    ''' lettering is drawn after the layer picture, on a small surface of its own.
    ''' </summary>
    Private Shared Sub DrawLateText(Bmp As Bitmap, Name As String, AtScale As Single, K As Integer, BaseSize As Size)

        EnsureText()
        Dim Runs As List(Of TextRun) = Nothing
        If Not _runs.TryGetValue(Name, Runs) Then Exit Sub

        Dim Any As Boolean = False
        Dim B As RectangleF
        For Each R As TextRun In Runs
            If Not R.Late Then Continue For
            B = If(Any, RectangleF.Union(B, R.Box), R.Box)
            Any = True
        Next
        If Not Any Then Exit Sub

        Dim X0 As Integer = Math.Max(0, CInt(Math.Floor(B.Left * AtScale)) - 2)
        Dim Y0 As Integer = Math.Max(0, CInt(Math.Floor(B.Top * AtScale)) - 2)
        Dim X1 As Integer = Math.Min(Bmp.Width, CInt(Math.Ceiling(B.Right * AtScale)) + 2)
        Dim Y1 As Integer = Math.Min(Bmp.Height, CInt(Math.Ceiling(B.Bottom * AtScale)) + 2)
        If X1 <= X0 OrElse Y1 <= Y0 Then Exit Sub

        Using Big As New Bitmap((X1 - X0) * K, (Y1 - Y0) * K, Imaging.PixelFormat.Format32bppPArgb)
            Using g As Graphics = Graphics.FromImage(Big)
                g.Clear(Color.FromArgb(0, 0, 0, 0))
                g.SmoothingMode = SmoothingMode.AntiAlias
                g.PixelOffsetMode = PixelOffsetMode.Half
                g.TranslateTransform(-X0 * K, -Y0 * K)
                DrawTextRuns(New ArtCanvas(g, AtScale, K, BaseSize), Name, True)
            End Using
            Using Small As Bitmap = BoxDown(Big, K)
                Using g As Graphics = Graphics.FromImage(Bmp)
                    g.CompositingMode = CompositingMode.SourceOver
                    g.InterpolationMode = InterpolationMode.NearestNeighbor
                    g.PixelOffsetMode = PixelOffsetMode.Half
                    g.DrawImage(Small, New Rectangle(X0, Y0, X1 - X0, Y1 - Y0), 0, 0, Small.Width, Small.Height, GraphicsUnit.Pixel)
                End Using
            End Using
        End Using

    End Sub

End Class
