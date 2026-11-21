'MIT License
'Copyright (c) 2023 overdoingism Lab.
'https://github.com/overdoingism/Final-Account-Defense-Barrier

' UIArt painters: the parts of the UI artwork that are drawn by code.
' Coordinates are 96-DPI design pixels measured from the original artwork.
' Text is NOT drawn here - it comes from the layer PNG of each image (see UIArt.vb).

Imports System.Drawing
Imports System.Drawing.Drawing2D

Partial Public NotInheritable Class UIArt

    Private Const C_Orange As Integer = &HFF6000
    Private Const C_Amber As Integer = &HFFC000
    Private Const C_Cyan As Integer = &H5EDBE6
    Private Const C_Purple As Integer = &HA78AFD
    Private Const C_Red As Integer = &HCD0000
    Private Const C_Black As Integer = &H0
    Private Const C_White As Integer = &HFFFFFF

    Private Const C_Frame As Integer = &HFFB620     'window outline
    Private Const C_Text As Integer = &HFFAB00      'main amber
    Private Const C_Alert As Integer = &HFF0000     'pure red
    Private Const C_Mint As Integer = &H7EEDB0      'green rules

    Private Shared Function RGB(Hex As Integer, Optional Alpha As Integer = 255) As Color
        Return ArtCanvas.RGB(Hex, Alpha)
    End Function

    Private Shared Sub Reg(Name As String, Painter As Action(Of ArtCanvas))
        Painters(Name) = Painter
    End Sub

    Private Shared Sub RegisterPainters()

        '---------------- Large buttons (112 x 61)
        For Each N As String In {"button_Cancel", "button_Final", "button_HELP", "button_LOGOUT", "button_OpenF",
                                 "button_confirm", "button_genpwd", "button_no", "button_yes", "button_security_check"}
            Reg(N, Sub(cv) PaintButton(cv, C_Orange, C_Amber, C_Black, False))
        Next

        For Each N As String In {"button_L_delete", "button_L_fileInfo", "button_L_moveDWN", "button_L_moveUP",
                                 "button_L_save", "button_L_transKEY"}
            Reg(N, Sub(cv) PaintButton(cv, C_Purple, -1, C_Black, False))
        Next

        For Each N As String In {"button_COPY", "button_Launch", "button_view"}
            Reg(N, Sub(cv) PaintButton(cv, C_Cyan, -1, C_Black, True))
        Next

        For Each N As String In {"button_CAT_CSVEX", "button_CAT_CSVIM", "button_CAT_DEL", "button_CAT_TRANS"}
            Reg(N, Sub(cv) PaintButton(cv, C_Red, -1, C_White, False))
        Next

        '---------------- Small buttons (112 x 28)
        Reg("button_copy_small", Sub(cv) PaintSmallButton(cv, C_Cyan, -1, True))
        Reg("button_view_small", Sub(cv) PaintSmallButton(cv, C_Cyan, -1, True))
        Reg("button_HKO_small_off", Sub(cv) PaintSmallButton(cv, C_Cyan, -1, False))
        Reg("button_HKO_small_on", Sub(cv) PaintSmallButton(cv, C_Cyan, &H203A, False))

        '---------------- Licence pages (423 x 470): the black frame; everything else is lettering
        For Each N As String In {"Doc01P_PNG", "Doc02P_PNG", "Doc03P_PNG", "Doc04P_PNG"}
            Reg(N, AddressOf PaintDocPage)
        Next
        ' page 1 also has a rounded frame around its heading
        Reg("Doc01P_PNG", Sub(cv)
                              PaintDocPage(cv)
                              cv.RoundFrame(RGB(C_Black), 87, 89, 338, 130, 7, 2)
                              ClaimBorder(cv, 87, 89, 338, 130, 3, 8)
                          End Sub)

        '---------------- Progress cells of the key derivation window (28 x 22): a colour block with a dark two-step rim
        Reg("PWD_KDF_C", Sub(cv) PaintKdfCell(cv, &H58CBC7, &H2F6B69, &H0B1A1A, &H1C403E, &H070F0F))
        Reg("PWD_KDF_R", Sub(cv) PaintKdfCell(cv, &HFF0000, &H650000, &H220000, &H3C0000, &H050000))

        '---------------- "BORDER" line of the key derivation window (144 x 3): a green line with a dim rim around it.
        '                 It is a control of its own because it must stay visible over the progress cells; what the
        '                 window shows behind it (the scale it crosses) is copied into it, see UIArt.Derived.
        '                 The line is centred in the picture's device rows: 3 x 1.75 = 5 rows give 1 + 3 + 1.
        Reg("PWD_KDF_LINE", Sub(cv)
                                Dim Wd As Integer = CInt(Math.Round(144 * cv.Sc)), Hd As Integer = CInt(Math.Round(3 * cv.Sc))
                                Dim Core As Integer = Math.Max(1, CInt(Math.Round(cv.Sc)))
                                If (Hd - Core) Mod 2 <> 0 Then Core += 1
                                Dim Rim As Integer = Math.Max(0, (Hd - Core) \ 2)
                                Dim Px As Single = 1.0F / cv.Sc         'one device pixel in design pixels
                                cv.Box(RGB(C_Mint, 110), 0, 0, Wd * Px, Hd * Px)
                                cv.Box(RGB(C_Mint), Rim * Px, Rim * Px, (Wd - Rim) * Px, (Hd - Rim) * Px)
                                cv.Claim(0, 0, 144, 3)
                            End Sub)

        '---------------- "PASS" plate of the key derivation window (35 x 27): a frame of one pixel with the word
        '                 inside. Frame and word are both kept in the 200% layer (the frame is what the art tool
        '                 has left over here - everything but the frame's own pixels is claimed - and it puts
        '                 leftovers of this picture into that layer, see DoubledLeftovers there). The glow that the
        '                 artwork had painted in is the glow filter's now (UIArt.SelfGlowing), which needs an
        '                 opaque ground; the artwork is transparent there, so arttool compares without it.
        Reg("PWD_KDF_PASS", Sub(cv)
                                If Not BuildingLayers Then cv.Box(RGB(C_Black), 0, 0, 35, 27)
                                cv.Claim(0, 0, 35, 2)
                                cv.Claim(0, 25, 35, 27)
                                cv.Claim(0, 0, 2, 27)
                                cv.Claim(33, 0, 35, 27)
                                cv.Claim(3, 3, 32, 24)
                            End Sub)

        '---------------- Warning plates of the login window (134 x 22): a red line around a dark red ground
        For Each N As String In {"WARN_1", "WARN_2"}
            Reg(N, AddressOf PaintWarnPlate)
        Next

        '---------------- "READ ONLY" tag (36 x 17): red plate, its rim and corner pixels half transparent
        Reg("Read_Only_Img", Sub(cv)
                                 cv.Box(RGB(&HFF0000, 140), 0, 0, 36, 17)
                                 cv.Box(RGB(&HFF0000), 2, 1, 34, 16)
                                 cv.Box(RGB(&HFF0000), 1, 2, 35, 15)
                             End Sub)

        '---------------- Catalog management button (158 x 54): amber plate
        Reg("button_CATMAN", Sub(cv)
                                 OuterGlow(cv, &HFFAB00, 0.65F, 0.65F, 157.35F, 53.35F)
                                 cv.Box(RGB(&HFFAB00), 0.65F, 0.65F, 157.35F, 53.35F)
                                 ClaimBorder(cv, 0, 0, 158, 54, 3, 3)
                             End Sub)

        '---------------- Indicators (134 x 33): CAPS LOCK / CONTENT, lamp at the bottom (off, green) or top (on, red)
        Reg("caps_lock_off", Sub(cv) PaintIndicator(cv, C_Mint, False))
        Reg("PASSWORD_HIDE", Sub(cv) PaintIndicator(cv, C_Mint, False))
        Reg("caps_lock_on", Sub(cv) PaintIndicator(cv, C_Alert, True))
        Reg("PASSWORD_SHOW", Sub(cv) PaintIndicator(cv, C_Alert, True))

        '---------------- Dark plates of the main window
        Reg("EPTSLOT_313", AddressOf PaintEmptySlot)
        Reg("EPTSLOT_692", AddressOf PaintEmptySlot)
        Reg("button_HKM_Title", Sub(cv)
                                    cv.Box(RGB(&H1C1400), 0, 0, 112, 28)
                                    GlowLine(cv, True, 0.5F, 1.5F, 110.5F, 1.5F, 110.5F, 26.5F, 0.5F, 26.5F)
                                    ClaimBorder(cv, 0, 0, 112, 28, 4, 4)
                                End Sub)

        '---------------- Count-down panel (293 x 122): only the black ground under its lettering is drawn,
        '                 so that the lettering can be rebuilt; the rest stays a picture.
        Reg("Timer", Sub(cv)
                         cv.Box(RGB(C_Black), 31, 16, 194, 52)
                         cv.Box(RGB(C_Black), 133, 54, 144, 86)
                     End Sub)

        '---------------- Selector captions (three stacked rows of lettering on a plain ground)
        Reg("KDFS_Types", Sub(cv) cv.Box(RGB(&H000000), 0, 0, cv.BaseSize.Width, cv.BaseSize.Height))
        Reg("KDFT_Types", Sub(cv) cv.Box(RGB(&H000000), 0, 0, cv.BaseSize.Width, cv.BaseSize.Height))
        Reg("PWD_Gen", Sub(cv) cv.Box(RGB(&H000000), 0, 0, cv.BaseSize.Width, cv.BaseSize.Height))

        '---------------- Language selector: caption plate (108 x 22) and arrow (15 x 22) side by side form one amber
        '                 block with a soft rim on its outer sides; where plate and arrow meet, the block runs to the
        '                 picture's edge. The lettering comes from the PSD, the arrow is drawn here. The glow filter
        '                 sees plate and arrow together (UIArt.GlowBeside).
        For Each N As String In {"Lang_en", "Lang_jp", "Lang_zhtw", "Lang_zhcn"}
            Reg(N, Sub(cv) PaintLangPlate(cv, True, False))
        Next
        Reg("Lang_load", Sub(cv) PaintLangPlate(cv, True, True))          'shown alone, as wide as plate + arrow
        Reg("Lang_arr", Sub(cv)
                            PaintLangPlate(cv, False, True)
                            cv.PolyRaw(RGB(C_Black), 2.35F, 4.15F, 2.35F, 17.85F, 9.2F, 11.0F)
                        End Sub)
        Reg("Lang_arr_on", Sub(cv)
                               PaintLangPlate(cv, False, True)
                               cv.PolyRaw(RGB(&HA80000), 2.35F, 4.15F, 2.35F, 17.85F, 9.2F, 11.0F)
                           End Sub)

        '---------------- CSV import / export window (452 x 142): outline, two green rules and the red frame around
        '                 the "IMPORTING" / "EXPORTING" picture (135 x 41, black). The lettering comes from the PSD,
        '                 except the rows of binary digits, which SetDigits puts in.
        '                 The artwork had a glow painted in; here it is the program's glow filter (UIArt.SelfGlowing).
        Reg("CSV_Waiting", Sub(cv)
                               cv.Shape(RGB(C_Black), RGB(C_Text), 2, 0, 0, 7.5F, 452, 0, 7.5F, 452, 142, 7.5F, 0, 142, 7.5F)
                               cv.HLine(RGB(C_Mint), 8, 444, 39, 5)
                               cv.HLine(RGB(C_Mint), 8, 444, 101, 5)
                               cv.Stroke(RGB(C_Alert), 2, True, 304, 50, 443, 50, 443, 95, 304, 95)
                               cv.Claim(0, 0, 452, 142)
                           End Sub)
        AfterPainters("CSV_Waiting") = AddressOf SetDigits
        For Each N As String In {"CSV_IMP", "CSV_EXP"}
            Reg(N, Sub(cv)
                       cv.Box(RGB(C_Black), 0, 0, 135, 41)
                       cv.Claim(0, 0, 135, 41)
                   End Sub)
        Next

        '---------------- Count-down state tags (41 x 37): a hand-made 200% picture without lettering; the lettering
        '                 ("ACTIVE" / "INACTIVE" and the kanji above) is rebuilt from the PSD text on top of it.
        For Each N As String In {"TimerID_ACT", "TimerID_INACT"}
            Reg(N, Sub(cv)
                       cv.Picture("TimerID_ACT_New@2x", 0, 0, 41, 37)
                       cv.Claim(0, 0, 41, 37)
                   End Sub)
        Next

        '---------------- Small icons: minimise button, scroll arrows, "next" arrow, combo box arrow, page arrow
        Reg("WinMin", AddressOf PaintWinMin)
        Reg("SCB_S_UP", Sub(cv) PaintScrollArrow(cv, True))
        Reg("SCB_S_DW", Sub(cv) PaintScrollArrow(cv, False))
        Reg("NextClick", AddressOf PaintNextArrow)
        Reg("CBOX_AR", AddressOf PaintComboArrow)
        Reg("Arr_Left", AddressOf PaintPageArrow)

        '---------------- Count-down digits (31 x 55): only the black ground. The digit is lettering: DSEG7 Classic,
        '                 set from its outline like Zen Antique (the art tool fits it to the artwork). Its glow is
        '                 the glow filter's (UIArt.SelfGlowing). Nothing is claimed, so a digit the tool could not
        '                 rebuild would simply stay a layer picture.
        For Each N As String In {"DIGI_Y_0", "DIGI_Y_1", "DIGI_Y_2", "DIGI_Y_3", "DIGI_Y_4", "DIGI_Y_5",
                                 "DIGI_Y_6", "DIGI_Y_7", "DIGI_Y_8", "DIGI_Y_9", "DIGI_Y__"}
            Reg(N, Sub(cv) cv.G.Clear(RGB(C_Black)))
        Next

        '---------------- Window backgrounds
        Reg("Title_LOGIN", AddressOf PaintTitleLogin)
        Reg("Title_NORMAL", AddressOf PaintTitleNormal)
        Reg("Title_Password", AddressOf PaintTitlePassword)
        Reg("Form_Main", AddressOf PaintFormMain)
        Reg("Form_Config", AddressOf PaintFormConfig)
        Reg("mit_lic", Sub(cv) TabPanel(cv, 448, 524, 178.5F, 31))
        Reg("PWD_KDF", AddressOf PaintPwdKdf)
        Reg("File_expo_New", AddressOf PaintFileExplorer)

    End Sub

    '==========================================================================================
    ' Settings window (714 x 415)
    '==========================================================================================

    Private Shared Sub PaintFormConfig(cv As ArtCanvas)

        TabPanel(cv, 714, 415, 178.5F, 31)

        ' four setting boxes on the left, the catalog-operation box on the right
        Dim Boxes(,) As Single = {{22, 61, 419, 115}, {22, 126, 419, 216}, {22, 227, 419, 317}, {22, 328, 419, 382}, {439, 61, 692, 248}}
        For i As Integer = 0 To Boxes.GetLength(0) - 1
            cv.RoundFrame(RGB(&HFBA900), Boxes(i, 0), Boxes(i, 1), Boxes(i, 2), Boxes(i, 3), 11.5F, 2)
            ClaimBorder(cv, Boxes(i, 0), Boxes(i, 1), Boxes(i, 2), Boxes(i, 3), 3, 13)
        Next

        ' Ground of the "Ctrl" / "Alt" key caps. Their outline (one pixel, rounded corners) is in the 200%
        ' layer together with the lettering; the ground reaches to the middle of that outline, so that it
        ' neither shows outside it nor leaves a gap inside whatever the scale.
        For Each Top As Single In {180.0F, 281.0F}
            cv.RoundBox(RGB(&H3F2A00), 35.5F, Top - 0.5F, 67.5F, Top + 25.5F, 4.5F)
            cv.RoundBox(RGB(&H3F2A00), 85.5F, Top - 0.5F, 117.5F, Top + 25.5F, 4.5F)
        Next

    End Sub

    '==========================================================================================
    ' Key derivation progress window (155 x 316)
    '==========================================================================================

    Private Shared Sub PaintPwdKdf(cv As ArtCanvas)

        cv.Shape(RGB(C_Black), RGB(C_Text), 1.0F, 0, 0, 5.5F, 155, 0, 5.5F, 155, 316, 5.5F, 0, 316, 5.5F)
        ClaimBorder(cv, 0, 0, 155, 316, 2, 7)

        ' rule under the heading
        cv.HLine(RGB(C_Text), 0, 155, 35, 1)
        cv.Claim(0, 35, 155, 36)

    End Sub

    '==========================================================================================
    ' File explorer window (898 x 524)
    '==========================================================================================

    Private Shared Sub PaintFileExplorer(cv As ArtCanvas)

        Dim Line As Color = RGB(C_Text)

        TabPanel(cv, 898, 524, 178.5F, 31, &H180801)

        ' the three black areas: path box (with its cut corner), drive list, folder list
        cv.Poly(RGB(C_Black), 19, 49, 880, 49, 880, 95, 38, 95, 19, 76)
        cv.Box(RGB(C_Black), 19, 160, 248, 434)
        cv.Box(RGB(C_Black), 269, 160, 880, 506)

        ' coloured heading plates (path / drives / folder contents), each with one cut corner
        cv.Poly(RGB(&H2A4A27), 19, 49, 181, 49, 181, 95, 38, 95, 19, 76)
        cv.Poly(RGB(&H5F2811), 38, 114, 248, 114, 248, 159, 19, 159, 19, 133)
        cv.Poly(RGB(&H173A4D), 288, 114, 880, 114, 880, 159, 269, 159, 269, 133)

        ' thin frame lines with their corner ticks: y, x0, x1
        Dim HL(,) As Single = {{48, 10, 15}, {48, 18, 881}, {48, 884, 889},
                               {95, 10, 15}, {95, 38, 881}, {95, 884, 889},
                               {113, 10, 15}, {113, 38, 249}, {113, 288, 881}, {113, 884, 889},
                               {434, 10, 15}, {434, 18, 249}, {434, 252, 257},
                               {506, 260, 265}, {506, 268, 881}, {506, 884, 889}}
        For i As Integer = 0 To HL.GetLength(0) - 1
            cv.HLine(Line, HL(i, 1), HL(i, 2), HL(i, 0), 1)
        Next

        ' x, y0, y1
        Dim VL(,) As Single = {{18, 40, 45}, {18, 48, 76}, {18, 96, 113}, {18, 133, 435}, {18, 438, 443},
                               {248, 113, 435}, {248, 438, 443},
                               {268, 133, 507}, {268, 510, 515},
                               {880, 40, 45}, {880, 48, 96}, {880, 99, 110}, {880, 113, 507}, {880, 510, 515}}
        For i As Integer = 0 To VL.GetLength(0) - 1
            cv.VLine(Line, VL(i, 0), VL(i, 1), VL(i, 2), 1)
        Next

        ' cut corners (claimed: the stair-stepped line of the artwork must not come back on top of these)
        cv.Stroke(Line, 1, False, 18.5F, 76, 38.5F, 95.5F)
        cv.Stroke(Line, 1, False, 18.5F, 133, 38.5F, 113.5F)
        cv.Stroke(Line, 1, False, 268.5F, 133, 288.5F, 113.5F)
        cv.Claim(17, 75, 40, 97)
        cv.Claim(17, 112, 40, 134)
        cv.Claim(267, 112, 290, 134)

    End Sub

    '==========================================================================================
    ' Main window (992 x 695)
    '==========================================================================================

    Private Shared Sub PaintFormMain(cv As ArtCanvas)

        Dim Line As Color = RGB(&HFBA900)

        '--- window outline: left column with the title tab, right side in two panels
        cv.Shape(RGB(C_Black), RGB(C_Frame), 1.0F,
                 0, 0, 5.5F,
                 178.5F, 0, 4.0F,
                 178.5F + 31 * 0.8125F, 31, 5.0F,
                 335, 31, 5.5F,
                 335, 695, 5.5F,
                 0, 695, 5.5F)

        ' Where the two right-hand panels meet the left column the window is solid: the gap between
        ' their rounded corners is black with the column's outline running through.
        cv.Box(RGB(C_Black), 335, 598, 342, 612)
        cv.Box(RGB(C_Frame), 334, 598, 336, 612)

        cv.Shape(RGB(&H332500), RGB(C_Frame), 1.0F,
                 335, 31, 5.5F, 992, 31, 5.5F, 992, 605, 5.5F, 335, 605, 5.5F)
        cv.Shape(RGB(C_Black), RGB(C_Frame), 1.0F,
                 335, 605, 5.5F, 992, 605, 5.5F, 992, 695, 5.5F, 335, 695, 5.5F)

        cv.Claim(0, 0, 180, 2)
        cv.Claim(172, 0, 210, 33)
        cv.Claim(204, 31, 992, 33)
        cv.Claim(0, 0, 2, 695)
        cv.Claim(0, 693, 992, 695)
        cv.Claim(333, 31, 338, 695)
        cv.Claim(989, 31, 992, 695)
        cv.Claim(335, 604, 992, 608)
        cv.Claim(0, 0, 7, 7)
        cv.Claim(0, 688, 7, 695)
        cv.Claim(328, 31, 342, 38)
        cv.Claim(328, 599, 342, 613)
        cv.Claim(328, 688, 342, 695)
        cv.Claim(985, 31, 992, 38)
        cv.Claim(985, 599, 992, 613)
        cv.Claim(985, 688, 992, 695)

        ' the bar in front of the window title
        cv.Box(RGB(C_Text), 13.04F, 8.45F, 15.75F, 31.55F)
        cv.Claim(12, 7, 17, 33)

        '--- left column: four stacked panels sharing their dividing lines
        Dim Tops() As Single = {44, 186, 474, 606, 682}
        For i As Integer = 0 To 3
            cv.RoundFrame(Line, 14, Tops(i), 322, Tops(i + 1) + If(i < 3, 2, 0), 9.5F, 2)
            ClaimBorder(cv, 14, Tops(i), 322, Tops(i + 1) + If(i < 3, 2, 0), 3, 9)
        Next

        ' heading rules of the catalog and "last action" panels
        cv.Box(RGB(&HE24A00), 25, 66, 310, 68)
        cv.Box(RGB(&HE24A00), 25, 646, 310, 648)
        cv.Claim(24, 65.5F, 311, 68.5F)
        cv.Claim(24, 645.5F, 311, 648.5F)

        '--- right side: seven field boxes, the fourth (secret data) in red
        For i As Integer = 0 To 6
            Dim T As Single = 44 + i * 79
            Dim BoxLine As Color = If(i = 3, RGB(&HFB0000), Line)
            Dim Rule As Color = If(i = 3, RGB(&HFF0000), RGB(&HE24A00))
            cv.RoundBox(RGB(&H1F1600), 462, T - 2, 863, T + 76, 13.5F)
            cv.RoundBox(RGB(C_Black), 463, T - 1, 862, T + 75, 12.5F)
            cv.RoundFrame(BoxLine, 465, T, 862, T + 74, 11.5F, 2)
            cv.Box(Rule, 476, T + 22, 850, T + 24)
            ClaimBorder(cv, 461, T - 3, 865, T + 77, 6, 13)
            cv.Claim(475, T + 21.5F, 851, T + 24.5F)
        Next

        ' the plain brown area around the boxes
        cv.Claim(338, 33, 460, 604)
        cv.Claim(866, 33, 989, 604)

    End Sub

    '==========================================================================================
    ' Buttons
    '==========================================================================================

    ''' <summary>
    ''' Standard 112x61 button: flat plate, optional thin outline of another colour,
    ''' two horizontal rules, optional "play" triangle on the left.
    ''' </summary>
    Private Shared Sub PaintButton(cv As ArtCanvas, Fill As Integer, Outline As Integer, Rule As Integer, Triangle As Boolean)

        Dim W As Single = cv.BaseSize.Width
        Dim H As Single = cv.BaseSize.Height

        OuterGlow(cv, Fill, 1.65F, 1.65F, W - 1.65F, H - 1.65F)

        If Outline >= 0 Then
            OutlinedPlate(cv, Fill, Outline, 2.0F, 2.0F, W - 2.0F, H - 2.0F)
        Else
            cv.Box(RGB(Fill), 1.65F, 1.65F, W - 1.65F, H - 1.65F)
        End If

        cv.HLine(RGB(Rule), 8.75F, 103.15F, 7.75F, 1.5F)
        cv.HLine(RGB(Rule), 8.75F, 103.15F, 51.75F, 1.5F)

        If Triangle Then
            PlayTriangle(cv, 31.0F)
            cv.Claim(0, 0, 20, H)       'the triangle is all there is at the left: nothing is taken from the artwork there
        End If

        ' Everything outside the two rules is plate only.
        cv.Claim(0, 0, W, 11)
        cv.Claim(0, 50, W, H)
        cv.Claim(0, 0, 5, H)
        cv.Claim(W - 8, 0, W, H)

    End Sub

    ''' <summary>
    ''' Plate with the faint outline of the artwork: there it is a soft fringe only about a third of a
    ''' pixel wide. Above 100% it becomes one device pixel at the same low strength - drawn solid it
    ''' would show as a distinct coloured frame that the artwork does not have.
    ''' </summary>
    Private Shared Sub OutlinedPlate(cv As ArtCanvas, Fill As Integer, Outline As Integer, x0 As Single, y0 As Single, x1 As Single, y1 As Single)
        If cv.Snap Then
            Dim Px As Single = 1.0F / cv.Sc      'one device pixel, in design pixels
            cv.Box(RGB(Outline, 89), x0 - Px, y0 - Px, x1 + Px, y1 + Px)
        Else
            cv.RoundBox(RGB(Outline), x0 - 0.35F, y0 - 0.35F, x1 + 0.35F, y1 + 0.35F, 1.0F)
        End If
        cv.Box(RGB(Fill), x0, y0, x1, y1)
    End Sub

    ''' <summary>
    ''' The black "play" triangle of the cyan buttons, large and small alike: it points right, its two slanted
    ''' edges run at 45 degrees and its two sharp corners at the left are cut off a little, as in the artwork.
    ''' The lower half is the exact mirror image of the upper half: only the upper half is drawn (on a small
    ''' surface of its own, the axis on a pixel boundary), and it is copied once as it is and once upside down.
    ''' Drawing the whole triangle at once is not enough for that - the anti-aliasing of the two slanted edges
    ''' comes out a little different. (The artwork's triangle was not symmetric either, and the large buttons
    ''' used to carry its edge pixels as a layer picture on top of the drawn one.)
    ''' </summary>
    Private Shared Sub PlayTriangle(cv As ArtCanvas, AxisY As Single)

        Const LeftX As Single = 6.6F        'the upright edge
        Const TipX As Single = 17.65F
        Const HalfHeight As Single = 10.0F  'the full triangle would reach 11.05 above and below the axis

        Dim x0 As Single = cv.P(LeftX)
        Dim yc As Integer = CInt(Math.Round(cv.P(AxisY)))       'surface row the axis lies on (a whole design pixel: no fraction)
        Dim Reach As Single = cv.LRaw(TipX - LeftX)
        Dim hh As Single = cv.LRaw(HalfHeight)

        Dim OriginX As Integer = CInt(Math.Floor(x0)) - 1
        Dim W As Integer = CInt(Math.Ceiling(x0 + Reach)) + 1 - OriginX
        Dim H As Integer = CInt(Math.Ceiling(hh)) + 1
        Dim dx As Single = x0 - OriginX

        Using Half As New Bitmap(W, H, Imaging.PixelFormat.Format32bppPArgb)
            Using g As Graphics = Graphics.FromImage(Half)
                g.Clear(Color.FromArgb(0, 0, 0, 0))
                g.SmoothingMode = SmoothingMode.AntiAlias
                g.PixelOffsetMode = PixelOffsetMode.Half
                Using b As New SolidBrush(RGB(C_Black))
                    ' upper half, the axis at the bottom edge of the surface
                    g.FillPolygon(b, New PointF() {New PointF(dx, H - hh), New PointF(dx + Reach - hh, H - hh),
                                                   New PointF(dx + Reach, H), New PointF(dx, H)})
                End Using
            End Using
            Dim Saved As InterpolationMode = cv.G.InterpolationMode
            cv.G.InterpolationMode = InterpolationMode.NearestNeighbor
            cv.G.DrawImage(Half, New Rectangle(OriginX, yc - H, W, H), 0, 0, W, H, GraphicsUnit.Pixel)
            Half.RotateFlip(RotateFlipType.RotateNoneFlipY)
            cv.G.DrawImage(Half, New Rectangle(OriginX, yc, W, H), 0, 0, W, H, GraphicsUnit.Pixel)
            cv.G.InterpolationMode = Saved
        End Using

    End Sub

    ''' <summary>Small 112x28 button: plate, optional "play" triangle; Inner >= 0 = dark inside with the plate colour as outline.</summary>
    Private Shared Sub PaintSmallButton(cv As ArtCanvas, Fill As Integer, Inner As Integer, Triangle As Boolean)

        Dim W As Single = cv.BaseSize.Width
        Dim H As Single = cv.BaseSize.Height

        OuterGlow(cv, Fill, 1.65F, 0.65F, W - 1.65F, H - 0.65F)

        If Inner >= 0 Then
            OutlinedPlate(cv, Inner, Fill, 2.0F, 1.0F, W - 2.0F, H - 1.0F)
        Else
            cv.Box(RGB(Fill), 1.65F, 0.65F, W - 1.65F, H - 0.65F)
        End If

        If Triangle Then
            PlayTriangle(cv, 14.0F)
            cv.Claim(0, 0, 20, H)
        End If

        cv.Claim(0, 0, W, 2)
        cv.Claim(0, H - 2, W, H)
        cv.Claim(0, 0, 4, H)
        cv.Claim(W - 4, 0, W, H)

    End Sub

    ''' <summary>
    ''' A little light around a coloured plate: two device pixels, fading out. (Before the program was
    ''' DPI aware, Windows' stretching produced a similar soft edge.)
    ''' </summary>
    Private Shared Sub OuterGlow(cv As ArtCanvas, PlateColor As Integer, x0 As Single, y0 As Single, x1 As Single, y1 As Single)
        Dim Px As Single = 1.0F / cv.Sc
        cv.Box(RGB(PlateColor, 40), x0 - 2 * Px, y0 - 2 * Px, x1 + 2 * Px, y1 + 2 * Px)
        cv.Box(RGB(PlateColor, 90), x0 - Px, y0 - Px, x1 + Px, y1 + Px)
    End Sub

    ''' <summary>Thin amber-brown line with the two-step soft edge the artwork has on its dark plates.</summary>
    Private Shared Sub GlowLine(cv As ArtCanvas, Closed As Boolean, ParamArray Pts() As Single)
        cv.Stroke(RGB(&H261B00), 5, Closed, Pts)
        cv.Stroke(RGB(&H3A2800), 3, Closed, Pts)
        cv.Stroke(RGB(&H8E4A00), 1, Closed, Pts)
    End Sub

    ''' <summary>
    ''' Empty slot: a square-cornered frame and a diagonal from corner to corner, as one figure. The soft edge
    ''' is laid down for both first and the lines themselves last, so the edge of the frame does not cover
    ''' the ends of the diagonal: it runs into the corners of the frame.
    ''' </summary>
    Private Shared Sub PaintEmptySlot(cv As ArtCanvas)
        Dim Frame() As Single = {2.5F, 2.5F, 109.5F, 2.5F, 109.5F, 58.5F, 2.5F, 58.5F}
        Dim Diagonal() As Single = {2.5F, 58.5F, 109.5F, 2.5F}
        Dim Tone() As Integer = {&H261B00, &H3A2800, &H8E4A00}      'the three steps of GlowLine, outside in
        Dim Thick() As Single = {5, 3, 1}
        cv.Box(RGB(&H1C1400), 0, 0, 112, 61)
        For i As Integer = 0 To 2
            cv.Stroke(RGB(Tone(i)), Thick(i), True, Frame)
            cv.Stroke(RGB(Tone(i)), Thick(i), False, Diagonal)
        Next
        cv.Claim(0, 0, 112, 61)     'nothing of the plate is taken from the artwork
    End Sub

    ''' <summary>
    ''' Indicator: rounded outline and a lamp, both with the soft two-step edge of the artwork.
    ''' </summary>
    Private Shared Sub PaintIndicator(cv As ArtCanvas, Lamp As Integer, LampOnTop As Boolean)

        Const Frame As Integer = &HF9A703

        ' black inside the outline: the glow filter only lights opaque pixels
        ' (left out while arttool compares with the artwork, whose inside is transparent)
        If Not BuildingLayers Then cv.RoundBox(RGB(C_Black), 2, 2, 132, 31, 5)
        cv.RoundFrame(RGB(Frame, 35), 0, 0, 134, 33, 7, 5)
        cv.RoundFrame(RGB(Frame, 85), 1, 1, 133, 32, 6, 3)
        cv.RoundFrame(RGB(Frame), 2, 2, 132, 31, 5, 1)

        Dim y0 As Single = If(LampOnTop, 7, 16)
        Dim y1 As Single = If(LampOnTop, 16, 25)
        cv.RoundBox(RGB(Lamp, 35), 6, y0 - 2, 22, y1 + 2, 3.5F)
        cv.RoundBox(RGB(Lamp, 85), 7, y0 - 1, 21, y1 + 1, 2.5F)
        cv.RoundBox(RGB(Lamp), 8, y0, 20, y1, 1.5F)

        ClaimBorder(cv, 0, 0, 134, 33, 6, 9)
        cv.Claim(5, 4, 23, 29)

    End Sub

    ''' <summary>
    ''' Hand-made 200% pictures in AllResource\Layers: pictures that painters draw with ArtCanvas.Picture, and
    ''' pictures named "(picture)@2x" that are that picture's whole artwork (Render puts them on as its 200%
    ''' layer; there is no painter and no 100% layer then, and outline lettering is set on top).
    ''' (The art tool deletes and rebuilds every other "@2x" picture there; these it leaves alone.)
    ''' </summary>
    Public Shared ReadOnly HandMadePictures As String() = {"TimerID_ACT_New@2x", "exit_Pic@2x", "TOPSEC@2x", "SALT_ADD@2x"}

    '==========================================================================================
    ' Small icons
    '==========================================================================================

    ' The mint-green icons have a soft edge in the artwork: beside every line one pixel at about a third and
    ' one more at about a ninth of the colour. SoftAlpha(k) is the opacity laid down for step k (0 = the line
    ' itself); the steps are drawn widest first, each over the last, which adds up to those strengths.
    Private Shared ReadOnly SoftAlpha As Integer() = {255, 64, 29}

    ''' <summary>Size of the picture on the painter's surface (device size x supersampling).</summary>
    Private Shared Function SurfaceSize(cv As ArtCanvas) As Size
        Return New Size(Math.Max(1, CInt(Math.Round(cv.BaseSize.Width * cv.Sc))) * cv.K,
                        Math.Max(1, CInt(Math.Round(cv.BaseSize.Height * cv.Sc))) * cv.K)
    End Function

    ''' <summary>
    ''' A line of width t around the rectangle x0..x1, y0..y1 (outer edges, corner radius r) with the soft edge
    ''' on both sides, each step as wide as the line. All in surface coordinates: the icons measure from the
    ''' picture's real edges, so that the line is equally far from them on opposite sides at any scale.
    ''' </summary>
    Private Shared Sub SoftFrame(cv As ArtCanvas, Hex As Integer, x0 As Single, y0 As Single, x1 As Single, y1 As Single, r As Single, t As Single)
        For k As Integer = 2 To 0 Step -1
            Dim o As Single = k * t
            cv.RoundFrameRaw(RGB(Hex, SoftAlpha(k)), x0 - o, y0 - o, x1 + o, y1 + o, r + o, t + 2 * o)
        Next
    End Sub

    ''' <summary>A filled rectangle (surface coordinates) with the same soft edge around it.</summary>
    Private Shared Sub SoftBox(cv As ArtCanvas, Hex As Integer, x0 As Single, y0 As Single, x1 As Single, y1 As Single, t As Single)
        For k As Integer = 2 To 0 Step -1
            Dim o As Single = k * t
            cv.RoundBoxRaw(RGB(Hex, SoftAlpha(k)), x0 - o, y0 - o, x1 + o, y1 + o, o)
        Next
    End Sub

    ''' <summary>
    ''' Fills a polygon that is symmetric about an axis through the middle of the picture, so that its two halves
    ''' are exact mirror images: one half is drawn on a small surface of its own that ends at the axis, and it is
    ''' copied once as it is and once flipped. (Drawn in one go, the anti-aliasing of the two halves differs.)
    ''' Pts: the whole polygon in surface coordinates. Upright: the axis is upright (x = Axis), else level.
    ''' Soft > 0: the polygon gets the soft edge of the mint-green icons, two steps of that width.
    ''' </summary>
    Private Shared Sub MirrorFill(cv As ArtCanvas, Fill As Color, Upright As Boolean, Axis As Integer, Soft As Single, ParamArray Pts() As PointF)

        Dim Pad As Integer = CInt(Math.Ceiling(Soft * 2)) + 2
        Dim X0 As Integer = Integer.MaxValue, Y0 As Integer = Integer.MaxValue, X1 As Integer = Integer.MinValue, Y1 As Integer = Integer.MinValue
        For Each P As PointF In Pts
            X0 = Math.Min(X0, CInt(Math.Floor(P.X)) - Pad)
            Y0 = Math.Min(Y0, CInt(Math.Floor(P.Y)) - Pad)
            X1 = Math.Max(X1, CInt(Math.Ceiling(P.X)) + Pad)
            Y1 = Math.Max(Y1, CInt(Math.Ceiling(P.Y)) + Pad)
        Next
        If Upright Then X1 = Axis Else Y1 = Axis
        Dim W As Integer = X1 - X0, H As Integer = Y1 - Y0
        If W <= 0 OrElse H <= 0 Then Exit Sub

        Using Half As New Bitmap(W, H, Imaging.PixelFormat.Format32bppPArgb)
            Using g As Graphics = Graphics.FromImage(Half)
                g.Clear(Color.FromArgb(0, 0, 0, 0))
                g.SmoothingMode = SmoothingMode.AntiAlias
                g.PixelOffsetMode = PixelOffsetMode.Half
                g.TranslateTransform(-X0, -Y0)
                If Soft > 0 Then
                    For k As Integer = 2 To 1 Step -1
                        Using pn As New Pen(Color.FromArgb(SoftAlpha(k), Fill), Soft * 2 * k)
                            pn.LineJoin = LineJoin.Round
                            g.DrawPolygon(pn, Pts)
                        End Using
                    Next
                End If
                Using b As New SolidBrush(Fill)
                    g.FillPolygon(b, Pts)
                End Using
            End Using
            Dim Saved As InterpolationMode = cv.G.InterpolationMode
            cv.G.InterpolationMode = InterpolationMode.NearestNeighbor
            cv.G.DrawImage(Half, New Rectangle(X0, Y0, W, H), 0, 0, W, H, GraphicsUnit.Pixel)
            Half.RotateFlip(If(Upright, RotateFlipType.RotateNoneFlipX, RotateFlipType.RotateNoneFlipY))
            cv.G.DrawImage(Half, New Rectangle(If(Upright, Axis, X0), If(Upright, Y0, Axis), W, H), 0, 0, W, H, GraphicsUnit.Pixel)
            cv.G.InterpolationMode = Saved
        End Using

    End Sub

    ''' <summary>
    ''' Draws something that is centred in the picture so that its right half is the exact mirror image of its
    ''' left half (and, with Level, its lower half that of its upper half): it is drawn on a surface of its
    ''' own and the halves are copied over. Rounded corners need this - their anti-aliasing comes out a few
    ''' levels different from corner to corner. Flip: the result is put on upside down.
    ''' </summary>
    Private Shared Sub Mirrored(cv As ArtCanvas, Level As Boolean, Flip As Boolean, Paint As Action(Of ArtCanvas))
        Dim Sz As Size = SurfaceSize(cv)
        Using Tmp As New Bitmap(Sz.Width, Sz.Height, Imaging.PixelFormat.Format32bppPArgb)
            Using g As Graphics = Graphics.FromImage(Tmp)
                g.Clear(Color.FromArgb(0, 0, 0, 0))
                g.SmoothingMode = SmoothingMode.AntiAlias
                g.PixelOffsetMode = PixelOffsetMode.Half
                Paint(New ArtCanvas(g, cv.Sc, cv.K, cv.BaseSize))
            End Using
            For Pass As Integer = 0 To If(Level, 1, 0)
                Dim Part As Rectangle = If(Pass = 0, New Rectangle(0, 0, Sz.Width \ 2, Sz.Height), New Rectangle(0, 0, Sz.Width, Sz.Height \ 2))
                If Part.Width = 0 OrElse Part.Height = 0 Then Continue For
                Using Half As Bitmap = Tmp.Clone(Part, Imaging.PixelFormat.Format32bppPArgb)
                    Half.RotateFlip(If(Pass = 0, RotateFlipType.RotateNoneFlipX, RotateFlipType.RotateNoneFlipY))
                    Using g As Graphics = Graphics.FromImage(Tmp)
                        g.CompositingMode = CompositingMode.SourceCopy
                        g.InterpolationMode = InterpolationMode.NearestNeighbor
                        g.PixelOffsetMode = PixelOffsetMode.Half
                        g.DrawImage(Half, New Rectangle(Sz.Width - Part.Width, Sz.Height - Part.Height, Part.Width, Part.Height),
                                    0, 0, Part.Width, Part.Height, GraphicsUnit.Pixel)
                    End Using
                End Using
            Next
            If Flip Then Tmp.RotateFlip(RotateFlipType.RotateNoneFlipY)
            Dim Saved As InterpolationMode = cv.G.InterpolationMode
            cv.G.InterpolationMode = InterpolationMode.NearestNeighbor
            cv.G.DrawImage(Tmp, New Rectangle(0, 0, Sz.Width, Sz.Height), 0, 0, Sz.Width, Sz.Height, GraphicsUnit.Pixel)
            cv.G.InterpolationMode = Saved
        End Using
    End Sub

    ''' <summary>
    ''' Minimise button (30 x 26): a frame with slightly rounded corners, three pixels in from the picture's
    ''' edges, and a bar near its bottom line; both with the soft edge.
    ''' </summary>
    Private Shared Sub PaintWinMin(cv As ArtCanvas)
        Dim Sz As Size = SurfaceSize(cv)
        Dim t As Single = cv.L(1)
        Dim m As Single = cv.P(3)
        Mirrored(cv, True, False, Sub(c) SoftFrame(c, C_Mint, m, m, Sz.Width - m, Sz.Height - m, c.LRaw(2), t))
        ' the bar: 16 x 4, seven pixels in from the left and right edge, three above the frame's bottom line
        Dim bx As Single = cv.P(7)
        Dim by1 As Single = Sz.Height - m - t - cv.L(3)
        Dim by0 As Single = by1 - cv.L(4)
        Mirrored(cv, False, False, Sub(c) SoftBox(c, C_Mint, bx, by0, Sz.Width - bx, by1, t))
        cv.Claim(0, 0, 30, 26)
    End Sub

    ''' <summary>
    ''' Arrow button of the list scroll bar (17 x 17): a square frame one pixel in from the picture's edges and
    ''' a triangle (10 wide, 5 high, in the middle), both with the soft edge.
    ''' </summary>
    Private Shared Sub PaintScrollArrow(cv As ArtCanvas, Up As Boolean)
        Dim Sz As Size = SurfaceSize(cv)
        Dim t As Single = cv.L(1)
        Dim m As Single = cv.P(1)
        Mirrored(cv, True, False, Sub(c) SoftFrame(c, C_Mint, m, m, Sz.Width - m, Sz.Height - m, 0, t))
        ' the triangle is drawn pointing up; the "down" button is that picture upside down
        Dim ax As Integer = Sz.Width \ 2
        Dim h As Single = cv.LRaw(5)
        Dim yb As Single = cv.P(11)             'the triangle's base line
        Mirrored(cv, False, Not Up,
                 Sub(c) MirrorFill(c, RGB(C_Mint), True, ax, t, New PointF(ax, yb - h), New PointF(ax + h, yb), New PointF(ax - h, yb)))
        cv.Claim(0, 0, 17, 17)
    End Sub

    ''' <summary>"Next" arrow of the login window (20 x 24): a black triangle pointing right on a green plate.</summary>
    Private Shared Sub PaintNextArrow(cv As ArtCanvas)
        Dim Sz As Size = SurfaceSize(cv)
        cv.G.Clear(RGB(C_Mint))
        Dim ay As Integer = Sz.Height \ 2
        Dim x0 As Single = cv.P(6)
        Dim h As Single = cv.LRaw(8.5F)
        MirrorFill(cv, RGB(C_Black), False, ay, 0, New PointF(x0, ay - h), New PointF(x0 + h, ay), New PointF(x0, ay + h))
        cv.Claim(0, 0, 20, 24)
    End Sub

    ''' <summary>Arrow of the combo boxes (18 x 27): a small black triangle pointing down on a green plate.</summary>
    Private Shared Sub PaintComboArrow(cv As ArtCanvas)
        Dim Sz As Size = SurfaceSize(cv)
        cv.G.Clear(RGB(C_Mint))
        Dim ax As Integer = Sz.Width \ 2
        Dim y0 As Single = cv.P(11)
        Dim hw As Single = cv.LRaw(4.35F)
        MirrorFill(cv, RGB(C_Black), True, ax, 0, New PointF(ax - hw, y0), New PointF(ax + hw, y0), New PointF(ax, y0 + cv.LRaw(5.15F)))
        cv.Claim(0, 0, 18, 27)
    End Sub

    ''' <summary>
    ''' Page arrow of the licence window (56 x 25; the form mirrors it for "next page"): white plate, a black
    ''' frame of two pixels two pixels in from the picture's edges, and a black triangle pointing left whose two
    ''' corners at the right are cut off a little, as in the artwork.
    ''' </summary>
    Private Shared Sub PaintPageArrow(cv As ArtCanvas)
        Dim Sz As Size = SurfaceSize(cv)
        cv.G.Clear(RGB(C_White))
        Dim m As Single = cv.P(2)
        cv.RoundFrameRaw(RGB(C_Black), m, m, Sz.Width - m, Sz.Height - m, 0, cv.L(2))
        Dim ay As Integer = Sz.Height \ 2
        Dim xb As Single = cv.P(35.25F)                 'the upright edge at the right
        Dim hh As Single = cv.LRaw(5.75F)               'half its height
        Dim xs As Single = xb - cv.LRaw(2.25F)          'where the slanted edges begin
        Dim xt As Single = xs - 2 * hh                  'the tip: two pixels across for one pixel up or down
        MirrorFill(cv, RGB(C_Black), False, ay, 0, New PointF(xt, ay), New PointF(xs, ay - hh), New PointF(xb, ay - hh),
                   New PointF(xb, ay + hh), New PointF(xs, ay + hh))
        cv.Claim(0, 0, 56, 25)
    End Sub

    ''' <summary>
    ''' Warning plate: dark red ground inside a red line of one design pixel. The line is measured from the
    ''' picture's real edges in whole device pixels, so it is equally wide on all four sides at any scale
    ''' (134 x 1.75 = 234.5 is made 234: from the design size the right-hand line would lose half a pixel).
    ''' </summary>
    Private Shared Sub PaintWarnPlate(cv As ArtCanvas)
        Dim W As Single = CSng(Math.Round(cv.BaseSize.Width * cv.Sc)) / cv.Sc
        Dim H As Single = CSng(Math.Round(cv.BaseSize.Height * cv.Sc)) / cv.Sc
        Dim t As Single = If(cv.Snap, CSng(Math.Max(1.0, Math.Floor(cv.Sc + 0.5F))) / cv.Sc, 1.0F)
        cv.Box(RGB(C_Alert), 0, 0, W, H)
        cv.Box(RGB(&H180000), t, t, W - t, H - t)
        cv.Claim(0, 0, cv.BaseSize.Width, cv.BaseSize.Height)
    End Sub

    ''' <summary>
    ''' Amber block of the language selector with the soft rim of the artwork: two design pixels wide, the
    ''' outermost pixel at about 71% and the next at 92% of the full colour. It is drawn in steps of one device
    ''' pixel (half a pixel at 100%), so above 100% it is a finer gradient instead of two enlarged steps.
    ''' RimLeft / RimRight: False on the side where the picture joins its neighbour.
    ''' </summary>
    Private Shared Sub PaintLangPlate(cv As ArtCanvas, RimLeft As Boolean, RimRight As Boolean)
        ' The rim is measured from the picture's real edges: in device pixels the picture can be half a pixel
        ' smaller than its design size (22 x 1.75 = 38.5, made 38), which would cut the rim off on that side.
        Dim W As Single = CSng(Math.Round(cv.BaseSize.Width * cv.Sc)) / cv.Sc
        Dim H As Single = CSng(Math.Round(cv.BaseSize.Height * cv.Sc)) / cv.Sc
        Const RimWidth As Single = 2.0F
        Dim Steps As Integer = If(cv.Snap, Math.Max(2, CInt(Math.Round(RimWidth * cv.Sc))), 4)
        Dim StepLen As Single = If(cv.Snap, 1.0F / cv.Sc, RimWidth / Steps)
        For i As Integer = 0 To Steps
            ' brightness at the middle of step i (the last box is the full colour)
            Dim Level As Double = 1.0
            If i < Steps Then Level = 1.0 - 0.42 * Math.Pow(1.0 - (i + 0.5) / Steps, 1.3)
            Dim C As Color = Color.FromArgb(255, CInt(255 * Level), CInt(171 * Level), 0)
            Dim d As Single = i * StepLen
            cv.Box(C, If(RimLeft, d, 0.0F), d, If(RimRight, W - d, W), H - d)
        Next
        cv.Claim(0, 0, cv.BaseSize.Width, cv.BaseSize.Height)     'plate and arrow are all there is: no layer picture
    End Sub

    ' The four rows of binary digits of the CSV window. The program sets them itself from two glyph pictures,
    ' "0" and "1" (made by the art tool from the first row of the artwork), and gives every digit a brightness
    ' of its own, drawn anew each time the picture is made.
    Private Shared ReadOnly DigitRows As String() = {
        "0101101001111001011011010111010000110101011101010110",
        "1011001000101110010101110010100101011010110010100010",
        "1110101101001001110100001101010111010101101001101100",
        "0001001000110010010101110010100101011011010011011011"}
    Private Shared ReadOnly DigitRowTops As Single() = {0, 15, 102, 117}      'design pixels below the first row
    Private Const DigitPitch As Single = 8.364F                               'from digit to digit (measured on the artwork)
    Private Const DigitDimmest As Single = 0.45F                              'brightness of a digit: from this ... to 1
    Private Shared ReadOnly DigitDice As New Random()

    Private Shared Function DigitLevel() As Single
        SyncLock DigitDice
            Return DigitDimmest + (1.0F - DigitDimmest) * CSng(DigitDice.NextDouble())
        End SyncLock
    End Function

    Private Shared Sub SetDigits(Bmp As Bitmap, AtScale As Single)
        DrawGlyphRows(Bmp, "CSV_Waiting", AtScale, DigitRows, DigitRowTops, DigitPitch, AddressOf DigitLevel)
    End Sub

    ''' <summary>Progress cell: Fill inside, one pixel of Edge around it, one pixel of Rim around that; the corner pixels are darker.</summary>
    Private Shared Sub PaintKdfCell(cv As ArtCanvas, Fill As Integer, Edge As Integer, Rim As Integer, EdgeCorner As Integer, RimCorner As Integer)
        cv.Box(RGB(Rim), 0, 0, 28, 22)
        cv.Box(RGB(Edge), 1, 1, 27, 21)
        cv.Box(RGB(Fill), 2, 2, 26, 20)
        For Each X As Single In {0.0F, 27.0F}
            For Each Y As Single In {0.0F, 21.0F}
                cv.Box(RGB(RimCorner), X, Y, X + 1, Y + 1)
            Next
        Next
        For Each X As Single In {1.0F, 26.0F}
            For Each Y As Single In {1.0F, 20.0F}
                cv.Box(RGB(EdgeCorner), X, Y, X + 1, Y + 1)
            Next
        Next
    End Sub

    Private Shared Sub PaintDocPage(cv As ArtCanvas)
        Dim Ink As Color = RGB(C_Black)
        cv.HLine(Ink, 10, 413, 10, 2)
        cv.HLine(Ink, 10, 413, 458, 2)
        cv.VLine(Ink, 10, 10, 460, 2)
        cv.VLine(Ink, 411, 10, 460, 2)
        ClaimBorder(cv, 9, 9, 414, 461, 4, 4)
    End Sub

    '==========================================================================================
    ' Shared window parts
    '==========================================================================================

    ''' <summary>Black panel with the thin amber outline and rounded corners used by every window.</summary>
    Private Shared Sub WindowPanel(cv As ArtCanvas, x0 As Single, y0 As Single, x1 As Single, y1 As Single)
        cv.Shape(RGB(C_Black), RGB(C_Frame), 1.0F,
                 x0, y0, 5.5F, x1, y0, 5.5F, x1, y1, 5.5F, x0, y1, 5.5F)
        ClaimBorder(cv, x0, y0, x1, y1, 2, 7)
    End Sub

    ''' <summary>Rectangle with its four corners cut off at 45 degrees (Cut = length of the cut along each edge).</summary>
    Private Shared Sub Chamfered(cv As ArtCanvas, Fill As Color, Outline As Color, t As Single,
                                 x0 As Single, y0 As Single, x1 As Single, y1 As Single, Cut As Single)
        cv.Shape(Fill, Outline, t,
                 x0 + Cut, y0, 1, x1 - Cut, y0, 1, x1, y0 + Cut, 1, x1, y1 - Cut, 1,
                 x1 - Cut, y1, 1, x0 + Cut, y1, 1, x0, y1 - Cut, 1, x0, y0 + Cut, 1)
    End Sub

    ''' <summary>Claims a band of Width along the rectangle's edges plus Corner-sized squares in the corners.</summary>
    Private Shared Sub ClaimBorder(cv As ArtCanvas, x0 As Single, y0 As Single, x1 As Single, y1 As Single, Width As Single, Corner As Single)
        cv.Claim(x0, y0, x1, y0 + Width)
        cv.Claim(x0, y1 - Width, x1, y1)
        cv.Claim(x0, y0, x0 + Width, y1)
        cv.Claim(x1 - Width, y0, x1, y1)
        cv.Claim(x0, y0, x0 + Corner, y0 + Corner)
        cv.Claim(x1 - Corner, y0, x1, y0 + Corner)
        cv.Claim(x0, y1 - Corner, x0 + Corner, y1)
        cv.Claim(x1 - Corner, y1 - Corner, x1, y1)
    End Sub

    ''' <summary>
    ''' Top panel with the title tab: the outline rises at the left (tab from x=0 to TabX),
    ''' comes down a slope and continues at TabH.
    ''' </summary>
    Private Shared Sub TabPanel(cv As ArtCanvas, W As Single, Bottom As Single, TabX As Single, TabH As Single, Optional Fill As Integer = C_Black)

        ' The slope runs 0.8125 px sideways per px down (measured on the artwork).
        Dim SlopeEnd As Single = TabX + TabH * 0.8125F

        cv.Shape(RGB(Fill), RGB(C_Frame), 1.0F,
                 0, 0, 5.5F,
                 TabX, 0, 4.0F,
                 SlopeEnd, TabH, 5.0F,
                 W, TabH, 5.5F,
                 W, Bottom, 5.5F,
                 0, Bottom, 5.5F)

        cv.Claim(0, 0, TabX + 2, 2)
        cv.Claim(TabX - 6, 0, SlopeEnd + 6, TabH + 2)
        cv.Claim(SlopeEnd, TabH, W, TabH + 2)
        cv.Claim(0, 0, 2, Bottom)
        cv.Claim(W - 2, TabH, W, Bottom)
        cv.Claim(0, Bottom - 2, W, Bottom)
        cv.Claim(0, 0, 7, 7)
        cv.Claim(W - 7, TabH, W, TabH + 7)
        cv.Claim(0, Bottom - 7, 7, Bottom)
        cv.Claim(W - 7, Bottom - 7, W, Bottom)

        ' The bar in front of the window title
        cv.Box(RGB(C_Text), 13.04F, 8.45F, 15.75F, 31.55F)
        cv.Claim(12, 7, 17, 33)

    End Sub

    ''' <summary>
    ''' Hazard stripes: slanted bars clipped to a rectangle. LeftAtTop = x of one bar's left edge on
    ''' the clip's top line; Lean = sideways shift per px down (negative leans the other way).
    ''' </summary>
    Private Shared Sub Stripes(cv As ArtCanvas, c As Color, x0 As Single, y0 As Single, x1 As Single, y1 As Single,
                               LeftAtTop As Single, Lean As Single, BarW As Single, Period As Single)

        Dim dx0 As Single = cv.P(x0), dy0 As Single = cv.P(y0), dx1 As Single = cv.P(x1), dy1 As Single = cv.P(y1)
        Dim Saved As Region = cv.G.Clip
        cv.G.SetClip(New RectangleF(dx0, dy0, dx1 - dx0, dy1 - dy0), CombineMode.Intersect)

        Dim H As Single = y1 - y0
        Dim Shift As Single = Lean * H
        Dim First As Single = LeftAtTop
        Do While First - Period + BarW + Math.Max(0.0F, Shift) > x0
            First -= Period
        Loop
        Do While First + BarW + Math.Max(0.0F, Shift) < x0
            First += Period
        Loop

        Dim X As Single = First
        Do While X + Math.Min(0.0F, Shift) < x1
            cv.PolyRaw(c, X, y0, X + BarW, y0, X + BarW + Shift, y1, X + Shift, y1)
            X += Period
        Loop

        cv.G.Clip = Saved
        cv.Claim(x0 - 1, y0 - 1, x1 + 1, y1 + 1)

    End Sub

    ''' <summary>Filled plate with a 1 px halo outside (the artwork's baked-in soft edge).</summary>
    Private Shared Sub HaloPlate(cv As ArtCanvas, Fill As Integer, Halo As Integer, x0 As Single, y0 As Single, x1 As Single, y1 As Single, r As Single)
        cv.RoundBox(RGB(Halo), x0 - 1, y0 - 1, x1 + 1, y1 + 1, r + 1)
        cv.RoundBox(RGB(Fill), x0, y0, x1, y1, r)
    End Sub

    '==========================================================================================
    ' Login window (535 x 667)
    '==========================================================================================

    Private Shared Sub PaintTitleLogin(cv As ArtCanvas)

        TabPanel(cv, 535, 232, 178.5F, 31)
        WindowPanel(cv, 0, 232, 535, 535)
        WindowPanel(cv, 0, 535, 535, 575)
        WindowPanel(cv, 0, 575, 535, 667)

        LoginHeader(cv)

        KeyInputArea(cv, 281)
        SelectorStrip(cv, 535, 51, 213)
        NoticeBand(cv)

    End Sub

    '==========================================================================================
    ' Key / password entry windows (535 x 374): same parts as the login window, without header
    '==========================================================================================

    Private Shared Sub PaintTitleNormal(cv As ArtCanvas)
        TabPanel(cv, 535, 334, 178.5F, 31)
        WindowPanel(cv, 0, 334, 535, 374)
        KeyInputArea(cv, 80)
        SelectorStrip(cv, 334, 51, 213)
    End Sub

    Private Shared Sub PaintTitlePassword(cv As ArtCanvas)
        TabPanel(cv, 535, 334, 178.5F, 31)
        WindowPanel(cv, 0, 334, 535, 374)
        KeyInputArea(cv, 80)
        SelectorStrip(cv, 334, 19, 245)
    End Sub

    ''' <summary>
    ''' The two green rules that frame the entry boxes and the strength gauge between them.
    ''' Y = top of the upper rule (the lower rule is 129 px below, the gauge starts 45 px below).
    ''' </summary>
    Private Shared Sub KeyInputArea(cv As ArtCanvas, Y As Single)

        cv.Box(RGB(C_Mint), 17, Y, 517, Y + 2)
        cv.Box(RGB(C_Mint), 17, Y + 129, 517, Y + 131)
        cv.Claim(16, Y - 1, 518, Y + 3)
        cv.Claim(16, Y + 128, 518, Y + 132)

        StrengthGauge(cv, Y + 45)

    End Sub

    ''' <summary>
    ''' Password strength gauge: gradient bar (red to green) with a scale above and below.
    ''' Top = top of the end brackets; the whole gauge is 42 px high, x = 16 .. 518.
    ''' Its labels (-oo, -100, +-0 ...) are text.
    ''' </summary>
    Private Shared Sub StrengthGauge(cv As ArtCanvas, Top As Single)

        Dim Amber As Color = RGB(C_Text)
        Dim Mint As Color = RGB(&H75E6A8)

        Dim BarTop As Single = Top + 16      'bar: 10 px high
        Dim BarBot As Single = Top + 26

        ' gradient bar
        cv.GradBoxH(18, BarTop, 516, BarBot,
                    {0.0F, 0.1F, 0.2F, 0.3F, 0.4F, 0.5F, 0.6F, 0.7F, 0.8F, 0.9F, 1.0F},
                    {RGB(&HC00200), RGB(&HCF0F00), RGB(&HE12A00), RGB(&HF14E00), RGB(&HF67800), RGB(&HF29904),
                     RGB(&HDAAF12), RGB(&HB8BF2A), RGB(&H91CC4B), RGB(&H6DD671), RGB(&H58DD8F)})

        ' Up and down, the scale is measured from the bar, the same way above and below it: every stroke
        ' starts one gap (2 px) away from the bar and runs outwards by its length. (Snapping a stroke's
        ' far end and its length separately let neighbouring sections end one pixel apart.)
        Dim UpperEdge As Single = cv.P(BarTop)       'surface coordinates
        Dim LowerEdge As Single = cv.P(BarBot)
        Dim Gap As Single = cv.L(2)

        ' Across, the scale is made of 1 px ticks on a 2 px pitch. Where that pitch is a whole number of
        ' device pixels (100%, 150%, 200% ...) the ticks are snapped to whole pixels like every other rule.
        ' Where it is not (125%, 175%, 225% ...) snapping would leave gaps of two widths in turn and the
        ' scale would look bunched in pairs. There the ticks keep their exact place and width and are
        ' brought to the pixels through a soft filter, the way a much larger drawing is shrunk (SoftTicks).
        ' The whole scale is moved by a fraction of a pixel so that every tick lies symmetrically on the
        ' pixel grid (centred on a pixel edge or on a pixel centre): all gaps come out alike.
        ' The division lines, 25 px apart, stay on whole pixels (nearest to their place in that scale)
        ' so that they remain sharp.
        Dim Exact As Boolean = False
        Dim Shift As Single = 0          'device pixels
        If cv.Snap Then
            Dim Pitch As Double = 2.0 * cv.Sc
            Dim Phases As Integer = 0    'after how many ticks the pixel pattern repeats
            For q As Integer = 1 To 16
                If Math.Abs(Pitch * q - Math.Round(Pitch * q)) < 0.001 Then
                    Phases = q
                    Exit For
                End If
            Next
            Exact = Phases <> 1
            If Phases > 1 Then
                Dim Centre As Double = 19.5 * cv.Sc * Phases          'centre of the first tick, in 1/Phases pixels
                Shift = CSng((Math.Floor(Centre + 0.5) - Centre) / Phases)
            End If
        End If

        ' fine scale: one tick every 2 px, growing in four steps towards the "strong" end
        Dim TickX As New List(Of Single)          'design x of every tick
        Dim TickLen As New List(Of Single)        'length of its amber part
        Dim TickExtra As New List(Of Single)      'green extension on the strongest quarter
        For X As Integer = 19 To 515 Step 2
            TickX.Add(X)
            If X < 117 Then
                TickLen.Add(2) : TickExtra.Add(0)
            ElseIf X < 317 Then
                TickLen.Add(3) : TickExtra.Add(0)
            ElseIf X < 417 Then
                TickLen.Add(6) : TickExtra.Add(0)
            Else
                TickLen.Add(4) : TickExtra.Add(4)
            End If
        Next

        ' All sharp strokes of one colour are filled as one shape (they touch and overlap).
        Using AmberStrokes As New GraphicsPath(FillMode.Winding), MintStrokes As New GraphicsPath(FillMode.Winding)

            If Exact Then
                SoftTicks(cv, Amber, Mint, Shift, TickX, TickLen, TickExtra, UpperEdge, LowerEdge, Gap)
            Else
                For i As Integer = 0 To TickX.Count - 1
                    Dim sx As Single = cv.P(TickX(i)), sw As Single = cv.L(1)
                    Dim Len As Single = cv.L(TickLen(i)), Extra As Single = cv.L(TickExtra(i))
                    AmberStrokes.AddRectangle(New RectangleF(sx, UpperEdge - Gap - Len, sw, Len))
                    AmberStrokes.AddRectangle(New RectangleF(sx, LowerEdge + Gap, sw, Len))
                    If Extra > 0 Then
                        MintStrokes.AddRectangle(New RectangleF(sx, UpperEdge - Gap - Len - Extra, sw, Extra))
                        MintStrokes.AddRectangle(New RectangleF(sx, LowerEdge + Gap + Len, sw, Extra))
                    End If
                Next
            End If

            ' division lines every 25 px (through the bar), long ones every 100 px. They reach as far as
            ' the ticks of the first two sections (2 and 3 px long); the long ones 11 px beyond the gap.
            For i As Integer = 1 To 19
                Dim X As Single = 17 + i * 25
                Dim Reach As Single
                If i Mod 4 = 0 Then
                    Reach = Gap + cv.L(11)
                ElseIf X < 117 Then
                    Reach = Gap + cv.L(2)
                Else
                    Reach = Gap + cv.L(3)
                End If
                Dim sx As Single = cv.P(X)
                If Exact Then sx = CSng(Math.Floor(X * cv.Sc + Shift + 0.5F)) * cv.K
                AmberStrokes.AddRectangle(New RectangleF(sx, UpperEdge - Reach, cv.L(1), LowerEdge - UpperEdge + 2 * Reach))
            Next

            ' the long division lines run over the green tick ends: green first
            Using b As New SolidBrush(Mint)
                cv.G.FillPath(b, MintStrokes)
            End Using
            Using b As New SolidBrush(Amber)
                cv.G.FillPath(b, AmberStrokes)
            End Using

        End Using

        ' end brackets
        cv.Bar(Amber, 16, Top, 18, Top + 42)
        cv.Bar(Amber, 516, Top, 518, Top + 42)

        ' Scale and bar are entirely code-drawn (the labels sit above and below this band).
        cv.Claim(15, BarTop - 4, 519, BarBot + 4)
        cv.Claim(15, Top - 1, 19, Top + 43)
        cv.Claim(515, Top - 1, 519, Top + 43)

    End Sub

    ''' <summary>
    ''' How far the filter of SoftTicks reaches to each side, in design pixels (see there). Tried at 125% and
    ''' 175% with the glow on: 0.6 still shows thin and thick ticks in turn, 1.0 lets the ticks fade into a band.
    ''' </summary>
    Private Const TickSoftness As Double = 0.9

    ''' <summary>Area of the unit tent (a triangle from -1 to 1, area 1) to the left of t.</summary>
    Private Shared Function TentArea(t As Double) As Double
        If t <= -1 Then Return 0
        If t >= 1 Then Return 1
        If t < 0 Then Return (1 + t) * (1 + t) / 2
        Return 1 - (1 - t) * (1 - t) / 2
    End Function

    ''' <summary>
    ''' The gauge's ticks where their pitch is not a whole number of device pixels. Each tick lies at its
    ''' exact place (plus Shift device pixels) with its exact width; a pixel column gets the share of the
    ''' ticks that a tent-shaped filter around its centre sees, as when a much larger drawing is shrunk
    ''' with a good filter. The filter reaches TickSoftness design pixels to each side: wide enough that a
    ''' tick lying across two pixels and a tick lying on one look alike once the glow has filled the gaps.
    ''' Edge = the bar's upper / lower edge, Gap = the gap between bar and ticks (surface coordinates).
    ''' </summary>
    Private Shared Sub SoftTicks(cv As ArtCanvas, Amber As Color, Mint As Color, Shift As Single,
                                 TickX As List(Of Single), TickLen As List(Of Single), TickExtra As List(Of Single),
                                 UpperEdge As Single, LowerEdge As Single, Gap As Single)

        Dim Reach As Double = Math.Max(0.5, TickSoftness * cv.Sc)            'device pixels
        Dim First As Integer = CInt(Math.Floor(TickX(0) * cv.Sc + Shift - Reach))
        Dim Last As Integer = CInt(Math.Ceiling((TickX(TickX.Count - 1) + 1) * cv.Sc + Shift + Reach))

        Dim Tick As Integer = 0
        For Col As Integer = First To Last

            Dim Mid As Double = Col + 0.5

            ' the tick this column belongs to (its lengths are used): the nearest one
            Do While Tick < TickX.Count - 1 AndAlso
                     Math.Abs((TickX(Tick + 1) + 0.5) * cv.Sc + Shift - Mid) < Math.Abs((TickX(Tick) + 0.5) * cv.Sc + Shift - Mid)
                Tick += 1
            Loop

            ' what the filter sees of this tick and of its neighbours
            Dim Share As Double = 0
            For k As Integer = Math.Max(0, Tick - 2) To Math.Min(TickX.Count - 1, Tick + 2)
                Dim a As Double = TickX(k) * cv.Sc + Shift, b As Double = a + cv.Sc
                Share += TentArea((b - Mid) / Reach) - TentArea((a - Mid) / Reach)
            Next
            Dim Alpha As Integer = CInt(Math.Round(Math.Min(1.0, Share) * 255))
            If Alpha <= 0 Then Continue For

            Dim sx As Single = Col * cv.K, sw As Single = cv.K
            Dim Len As Single = cv.L(TickLen(Tick)), Extra As Single = cv.L(TickExtra(Tick))
            Using b As New SolidBrush(Color.FromArgb(Alpha, Amber))
                cv.G.FillRectangle(b, sx, UpperEdge - Gap - Len, sw, Len)
                cv.G.FillRectangle(b, sx, LowerEdge + Gap, sw, Len)
            End Using
            If Extra > 0 Then
                Using b As New SolidBrush(Color.FromArgb(Alpha, Mint))
                    cv.G.FillRectangle(b, sx, UpperEdge - Gap - Len - Extra, sw, Extra)
                    cv.G.FillRectangle(b, sx, LowerEdge + Gap + Len, sw, Extra)
                End Using
            End If

        Next

    End Sub

    ''' <summary>The narrow strip with a caption between two green bars and the selector box. PanelTop = top of its panel.</summary>
    Private Shared Sub SelectorStrip(cv As ArtCanvas, PanelTop As Single, Bar1X As Single, Bar2X As Single)

        Dim Mint As Color = RGB(C_Mint)

        cv.Box(Mint, Bar1X, PanelTop + 5, Bar1X + 4, PanelTop + 35)
        cv.Box(Mint, Bar2X, PanelTop + 5, Bar2X + 4, PanelTop + 35)
        cv.Claim(Bar1X - 1, PanelTop + 4, Bar1X + 5, PanelTop + 36)
        cv.Claim(Bar2X - 1, PanelTop + 4, Bar2X + 5, PanelTop + 36)

        ' selector box: frame 336..469 with a divider at 447
        Dim T As Single = PanelTop + 7, B As Single = PanelTop + 33
        cv.HLine(Mint, 336, 469, T, 1)
        cv.HLine(Mint, 336, 469, B - 1, 1)
        cv.VLine(Mint, 336, T, B, 1)
        cv.VLine(Mint, 447, T, B, 1)
        cv.VLine(Mint, 468, T, B, 1)
        cv.Claim(335, T - 1, 470, B + 1)

    End Sub

    ''' <summary>Login window, bottom panel: blue-green band with the two black notice boxes.</summary>
    Private Shared Sub NoticeBand(cv As ArtCanvas)

        cv.GradBoxH(2, 586, 533, 656,
                    {0.0F, 0.185F, 0.373F, 0.561F, 0.75F, 0.938F, 1.0F},
                    {RGB(&H4F96BE), RGB(&H4F96BE), RGB(&H4FA0BE), RGB(&H4FAEBF), RGB(&H51BCC3), RGB(&H57C8C6), RGB(&H58CAC7)})

        ' The boxes hold the notice text and a honeycomb pattern: those stay in the layer picture.
        cv.RoundBox(RGB(C_Black), 43, 592, 308, 649, 2)
        cv.RoundBox(RGB(C_Black), 317, 592, 501, 649, 2)

        ' plain band areas
        cv.Claim(22, 586, 533, 591)
        cv.Claim(22, 650, 533, 656)
        cv.Claim(309, 586, 316, 656)
        cv.Claim(502, 586, 533, 656)

    End Sub

    ''' <summary>The red "hazard" header of the login window (inside the tab panel).</summary>
    Private Shared Sub LoginHeader(cv As ArtCanvas)

        Dim Red As Color = RGB(C_Alert)

        ' "ENCRYPT WITH" frame
        cv.HLine(RGB(C_Text), 14, 166, 40, 1)
        cv.HLine(RGB(C_Text), 14, 166, 61, 1)
        cv.VLine(RGB(C_Text), 14, 40, 62, 1)
        cv.VLine(RGB(C_Text), 165, 40, 62, 1)
        cv.Claim(14, 40, 166, 41)
        cv.Claim(14, 61, 166, 62)
        cv.Claim(14, 40, 15, 62)
        cv.Claim(165, 40, 166, 62)

        ' side bars
        cv.Box(Red, 11, 69, 14, 208)
        cv.Box(Red, 521, 69, 524, 208)
        cv.Claim(10, 68, 15, 209)
        cv.Claim(520, 68, 525, 209)

        ' long stripe band: two halves leaning towards the middle
        Stripes(cv, Red, 19, 69, 267.5F, 83.3F, 28.33F, 1.3F, 11.76F, 21)
        Stripes(cv, Red, 267.5F, 69, 516, 83.3F, 535 - 28.33F - 11.76F, -1.3F, 11.76F, 21)

        ' short stripe blocks beside the A.T. Field button
        Stripes(cv, Red, 19, 192.47F, 57, 207, 27.82F, 1.3F, 11.77F, 21)
        Stripes(cv, Red, 478, 192.47F, 516, 207, 535 - 27.82F - 11.77F, -1.3F, 11.77F, 21)

        ' A.T. Field plate (its caption is text)
        HaloPlate(cv, C_Alert, &H200000, 63, 182, 264, 219, 4.5F)
        ClaimBorder(cv, 62, 181, 265, 220, 2, 6)

        ' dark slot for the deploy indicator
        cv.RoundBox(RGB(&H200000), 269, 181, 472, 220, 5.5F)
        cv.RoundBox(Red, 270, 182, 471, 219, 4.5F)
        cv.RoundBox(RGB(&H510000), 271, 183, 470, 218, 3.5F)
        cv.RoundBox(RGB(&H380000), 272, 184, 469, 217, 2.5F)
        cv.Claim(268, 180, 473, 221)

    End Sub

End Class
