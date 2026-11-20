'MIT License 
'Copyright (c) 2023 overdoingism Labs.
'https://github.com/overdoignism/Final-Account-Defense-Barrier

' UIArt image names. One property per UI picture, plus its size at 100% (96 DPI).

Imports System.Drawing

Partial Public NotInheritable Class UIArt

    Private Shared ReadOnly BaseSizes As New Dictionary(Of String, Size)(StringComparer.Ordinal) From {
        {"Arr_Left", New Size(56, 25)},
        {"button_Cancel", New Size(112, 61)},
        {"button_CAT_CSVEX", New Size(112, 61)},
        {"button_CAT_CSVIM", New Size(112, 61)},
        {"button_CAT_DEL", New Size(112, 61)},
        {"button_CAT_on", New Size(112, 61)},
        {"button_CAT_TRANS", New Size(112, 61)},
        {"button_CATMAN", New Size(158, 54)},
        {"button_confirm", New Size(112, 61)},
        {"button_COPY", New Size(112, 61)},
        {"button_copy_small", New Size(112, 28)},
        {"button_Final", New Size(112, 61)},
        {"button_genpwd", New Size(112, 61)},
        {"button_HELP", New Size(112, 61)},
        {"button_HKM_Title", New Size(112, 28)},
        {"button_HKO_small_off", New Size(112, 28)},
        {"button_HKO_small_on", New Size(112, 28)},
        {"button_L_delete", New Size(112, 61)},
        {"button_L_fileInfo", New Size(112, 61)},
        {"button_L_moveDWN", New Size(112, 61)},
        {"button_L_moveUP", New Size(112, 61)},
        {"button_L_save", New Size(112, 61)},
        {"button_L_transKEY", New Size(112, 61)},
        {"button_Launch", New Size(112, 61)},
        {"button_LOGOUT", New Size(112, 61)},
        {"button_no", New Size(112, 61)},
        {"button_OpenF", New Size(112, 61)},
        {"button_security_check", New Size(112, 61)},
        {"button_view", New Size(112, 61)},
        {"button_view_small", New Size(112, 28)},
        {"button_yes", New Size(112, 61)},
        {"caps_lock_off", New Size(134, 33)},
        {"caps_lock_on", New Size(134, 33)},
        {"CBOX_AR", New Size(18, 27)},
        {"CSV_EXP", New Size(135, 41)},
        {"CSV_IMP", New Size(135, 41)},
        {"CSV_Waiting", New Size(452, 142)},
        {"DIGI_Y_0", New Size(31, 55)},
        {"DIGI_Y_1", New Size(31, 55)},
        {"DIGI_Y_2", New Size(31, 55)},
        {"DIGI_Y_3", New Size(31, 55)},
        {"DIGI_Y_4", New Size(31, 55)},
        {"DIGI_Y_5", New Size(31, 55)},
        {"DIGI_Y_6", New Size(31, 55)},
        {"DIGI_Y_7", New Size(31, 55)},
        {"DIGI_Y_8", New Size(31, 55)},
        {"DIGI_Y_9", New Size(31, 55)},
        {"DIGI_Y__", New Size(31, 55)},
        {"Doc01P_PNG", New Size(423, 470)},
        {"Doc02P_PNG", New Size(423, 470)},
        {"Doc03P_PNG", New Size(423, 470)},
        {"Doc04P_PNG", New Size(423, 470)},
        {"EPTSLOT_313", New Size(112, 61)},
        {"EPTSLOT_692", New Size(112, 61)},
        {"exit_Pic", New Size(40, 40)},
        {"File_expo_New", New Size(898, 524)},
        {"Form_Config", New Size(714, 415)},
        {"Form_Main", New Size(992, 695)},
        {"KDFS_Types", New Size(108, 66)},
        {"KDFT_Types", New Size(36, 39)},
        {"Lang_arr", New Size(15, 22)},
        {"Lang_arr_on", New Size(15, 22)},
        {"Lang_en", New Size(108, 22)},
        {"Lang_jp", New Size(108, 22)},
        {"Lang_load", New Size(123, 22)},
        {"Lang_zhcn", New Size(108, 22)},
        {"Lang_zhtw", New Size(108, 22)},
        {"Message_CRI_PNG", New Size(659, 409)},
        {"Message_CRI_TXTB_PNG", New Size(636, 233)},
        {"Message_OK_PNG", New Size(659, 409)},
        {"Message_OK_TXTB_PNG", New Size(636, 233)},
        {"Message_VRF_PNG", New Size(659, 409)},
        {"Message_VRF_TXTB_PNG", New Size(636, 233)},
        {"mit_Btn", New Size(124, 36)},
        {"mit_lic", New Size(448, 524)},
        {"NextClick", New Size(20, 24)},
        {"PASSWORD_HIDE", New Size(134, 33)},
        {"PASSWORD_SHOW", New Size(134, 33)},
        {"PFWS_OFF", New Size(160, 33)},
        {"PFWS_ON", New Size(160, 33)},
        {"PFWS_RA", New Size(160, 33)},
        {"PWD_Gen", New Size(108, 66)},
        {"PWD_KDF", New Size(155, 316)},
        {"PWD_KDF_C", New Size(28, 22)},
        {"PWD_KDF_LINE", New Size(144, 3)},
        {"PWD_KDF_PASS", New Size(35, 27)},
        {"PWD_KDF_R", New Size(28, 22)},
        {"Read_Only_Img", New Size(36, 17)},
        {"SALT_ADD", New Size(38, 32)},
        {"SCB_BAR", New Size(17, 59)},
        {"SCB_BARV", New Size(17, 454)},
        {"SCB_S_DW", New Size(17, 17)},
        {"SCB_S_UP", New Size(17, 17)},
        {"Timer", New Size(293, 122)},
        {"TimerID_ACT", New Size(41, 37)},
        {"TimerID_INACT", New Size(41, 37)},
        {"Title_LOGIN", New Size(535, 667)},
        {"Title_NORMAL", New Size(535, 374)},
        {"Title_Password", New Size(535, 374)},
        {"TOPSEC", New Size(370, 22)},
        {"WARN_1", New Size(134, 22)},
        {"WARN_2", New Size(134, 22)},
        {"WinMin", New Size(30, 26)}
    }

    Public Shared ReadOnly Property Arr_Left As Bitmap
        Get
            Return Img("Arr_Left")
        End Get
    End Property

    Public Shared ReadOnly Property button_Cancel As Bitmap
        Get
            Return Img("button_Cancel")
        End Get
    End Property

    Public Shared ReadOnly Property button_CAT_CSVEX As Bitmap
        Get
            Return Img("button_CAT_CSVEX")
        End Get
    End Property

    Public Shared ReadOnly Property button_CAT_CSVIM As Bitmap
        Get
            Return Img("button_CAT_CSVIM")
        End Get
    End Property

    Public Shared ReadOnly Property button_CAT_DEL As Bitmap
        Get
            Return Img("button_CAT_DEL")
        End Get
    End Property

    Public Shared ReadOnly Property button_CAT_on As Bitmap
        Get
            Return Img("button_CAT_on")
        End Get
    End Property

    Public Shared ReadOnly Property button_CAT_TRANS As Bitmap
        Get
            Return Img("button_CAT_TRANS")
        End Get
    End Property

    Public Shared ReadOnly Property button_CATMAN As Bitmap
        Get
            Return Img("button_CATMAN")
        End Get
    End Property

    Public Shared ReadOnly Property button_confirm As Bitmap
        Get
            Return Img("button_confirm")
        End Get
    End Property

    Public Shared ReadOnly Property button_COPY As Bitmap
        Get
            Return Img("button_COPY")
        End Get
    End Property

    Public Shared ReadOnly Property button_copy_small As Bitmap
        Get
            Return Img("button_copy_small")
        End Get
    End Property

    Public Shared ReadOnly Property button_Final As Bitmap
        Get
            Return Img("button_Final")
        End Get
    End Property

    Public Shared ReadOnly Property button_genpwd As Bitmap
        Get
            Return Img("button_genpwd")
        End Get
    End Property

    Public Shared ReadOnly Property button_HELP As Bitmap
        Get
            Return Img("button_HELP")
        End Get
    End Property

    Public Shared ReadOnly Property button_HKM_Title As Bitmap
        Get
            Return Img("button_HKM_Title")
        End Get
    End Property

    Public Shared ReadOnly Property button_HKO_small_off As Bitmap
        Get
            Return Img("button_HKO_small_off")
        End Get
    End Property

    Public Shared ReadOnly Property button_HKO_small_on As Bitmap
        Get
            Return Img("button_HKO_small_on")
        End Get
    End Property

    Public Shared ReadOnly Property button_L_delete As Bitmap
        Get
            Return Img("button_L_delete")
        End Get
    End Property

    Public Shared ReadOnly Property button_L_fileInfo As Bitmap
        Get
            Return Img("button_L_fileInfo")
        End Get
    End Property

    Public Shared ReadOnly Property button_L_moveDWN As Bitmap
        Get
            Return Img("button_L_moveDWN")
        End Get
    End Property

    Public Shared ReadOnly Property button_L_moveUP As Bitmap
        Get
            Return Img("button_L_moveUP")
        End Get
    End Property

    Public Shared ReadOnly Property button_L_save As Bitmap
        Get
            Return Img("button_L_save")
        End Get
    End Property

    Public Shared ReadOnly Property button_L_transKEY As Bitmap
        Get
            Return Img("button_L_transKEY")
        End Get
    End Property

    Public Shared ReadOnly Property button_Launch As Bitmap
        Get
            Return Img("button_Launch")
        End Get
    End Property

    Public Shared ReadOnly Property button_LOGOUT As Bitmap
        Get
            Return Img("button_LOGOUT")
        End Get
    End Property

    Public Shared ReadOnly Property button_no As Bitmap
        Get
            Return Img("button_no")
        End Get
    End Property

    Public Shared ReadOnly Property button_OpenF As Bitmap
        Get
            Return Img("button_OpenF")
        End Get
    End Property

    Public Shared ReadOnly Property button_security_check As Bitmap
        Get
            Return Img("button_security_check")
        End Get
    End Property

    Public Shared ReadOnly Property button_view As Bitmap
        Get
            Return Img("button_view")
        End Get
    End Property

    Public Shared ReadOnly Property button_view_small As Bitmap
        Get
            Return Img("button_view_small")
        End Get
    End Property

    Public Shared ReadOnly Property button_yes As Bitmap
        Get
            Return Img("button_yes")
        End Get
    End Property

    Public Shared ReadOnly Property caps_lock_off As Bitmap
        Get
            Return Img("caps_lock_off")
        End Get
    End Property

    Public Shared ReadOnly Property caps_lock_on As Bitmap
        Get
            Return Img("caps_lock_on")
        End Get
    End Property

    Public Shared ReadOnly Property CBOX_AR As Bitmap
        Get
            Return Img("CBOX_AR")
        End Get
    End Property

    Public Shared ReadOnly Property CSV_EXP As Bitmap
        Get
            Return Img("CSV_EXP")
        End Get
    End Property

    Public Shared ReadOnly Property CSV_IMP As Bitmap
        Get
            Return Img("CSV_IMP")
        End Get
    End Property

    Public Shared ReadOnly Property CSV_Waiting As Bitmap
        Get
            Return Img("CSV_Waiting")
        End Get
    End Property

    Public Shared ReadOnly Property DIGI_Y_0 As Bitmap
        Get
            Return Img("DIGI_Y_0")
        End Get
    End Property

    Public Shared ReadOnly Property DIGI_Y_1 As Bitmap
        Get
            Return Img("DIGI_Y_1")
        End Get
    End Property

    Public Shared ReadOnly Property DIGI_Y_2 As Bitmap
        Get
            Return Img("DIGI_Y_2")
        End Get
    End Property

    Public Shared ReadOnly Property DIGI_Y_3 As Bitmap
        Get
            Return Img("DIGI_Y_3")
        End Get
    End Property

    Public Shared ReadOnly Property DIGI_Y_4 As Bitmap
        Get
            Return Img("DIGI_Y_4")
        End Get
    End Property

    Public Shared ReadOnly Property DIGI_Y_5 As Bitmap
        Get
            Return Img("DIGI_Y_5")
        End Get
    End Property

    Public Shared ReadOnly Property DIGI_Y_6 As Bitmap
        Get
            Return Img("DIGI_Y_6")
        End Get
    End Property

    Public Shared ReadOnly Property DIGI_Y_7 As Bitmap
        Get
            Return Img("DIGI_Y_7")
        End Get
    End Property

    Public Shared ReadOnly Property DIGI_Y_8 As Bitmap
        Get
            Return Img("DIGI_Y_8")
        End Get
    End Property

    Public Shared ReadOnly Property DIGI_Y_9 As Bitmap
        Get
            Return Img("DIGI_Y_9")
        End Get
    End Property

    Public Shared ReadOnly Property DIGI_Y__ As Bitmap
        Get
            Return Img("DIGI_Y__")
        End Get
    End Property

    Public Shared ReadOnly Property Doc01P_PNG As Bitmap
        Get
            Return Img("Doc01P_PNG")
        End Get
    End Property

    Public Shared ReadOnly Property Doc02P_PNG As Bitmap
        Get
            Return Img("Doc02P_PNG")
        End Get
    End Property

    ''' <summary>Licence window, page 4 (fonts). It has no artwork: all of it is lettering from art_text_extra.txt.</summary>
    Public Shared ReadOnly Property Doc04P_PNG As Bitmap
        Get
            Return Img("Doc04P_PNG")
        End Get
    End Property

    Public Shared ReadOnly Property Doc03P_PNG As Bitmap
        Get
            Return Img("Doc03P_PNG")
        End Get
    End Property

    Public Shared ReadOnly Property EPTSLOT_313 As Bitmap
        Get
            Return Img("EPTSLOT_313")
        End Get
    End Property

    Public Shared ReadOnly Property EPTSLOT_692 As Bitmap
        Get
            Return Img("EPTSLOT_692")
        End Get
    End Property

    Public Shared ReadOnly Property exit_Pic As Bitmap
        Get
            Return Img("exit_Pic")
        End Get
    End Property

    Public Shared ReadOnly Property File_expo_New As Bitmap
        Get
            Return Img("File_expo_New")
        End Get
    End Property

    Public Shared ReadOnly Property Form_Config As Bitmap
        Get
            Return Img("Form_Config")
        End Get
    End Property

    Public Shared ReadOnly Property Form_Main As Bitmap
        Get
            Return Img("Form_Main")
        End Get
    End Property

    Public Shared ReadOnly Property KDFS_Types As Bitmap
        Get
            Return Img("KDFS_Types")
        End Get
    End Property

    Public Shared ReadOnly Property KDFT_Types As Bitmap
        Get
            Return Img("KDFT_Types")
        End Get
    End Property

    Public Shared ReadOnly Property Lang_arr As Bitmap
        Get
            Return Img("Lang_arr")
        End Get
    End Property

    ''' <summary>The language arrow while the mouse is over it (red arrow). Drawn by code only, no artwork.</summary>
    Public Shared ReadOnly Property Lang_arr_on As Bitmap
        Get
            Return Img("Lang_arr_on")
        End Get
    End Property

    Public Shared ReadOnly Property Lang_en As Bitmap
        Get
            Return Img("Lang_en")
        End Get
    End Property

    Public Shared ReadOnly Property Lang_jp As Bitmap
        Get
            Return Img("Lang_jp")
        End Get
    End Property

    Public Shared ReadOnly Property Lang_load As Bitmap
        Get
            Return Img("Lang_load")
        End Get
    End Property

    Public Shared ReadOnly Property Lang_zhcn As Bitmap
        Get
            Return Img("Lang_zhcn")
        End Get
    End Property

    Public Shared ReadOnly Property Lang_zhtw As Bitmap
        Get
            Return Img("Lang_zhtw")
        End Get
    End Property

    Public Shared ReadOnly Property Message_CRI_PNG As Bitmap
        Get
            Return Img("Message_CRI_PNG")
        End Get
    End Property

    Public Shared ReadOnly Property Message_CRI_TXTB_PNG As Bitmap
        Get
            Return Img("Message_CRI_TXTB_PNG")
        End Get
    End Property

    Public Shared ReadOnly Property Message_OK_PNG As Bitmap
        Get
            Return Img("Message_OK_PNG")
        End Get
    End Property

    Public Shared ReadOnly Property Message_OK_TXTB_PNG As Bitmap
        Get
            Return Img("Message_OK_TXTB_PNG")
        End Get
    End Property

    Public Shared ReadOnly Property Message_VRF_PNG As Bitmap
        Get
            Return Img("Message_VRF_PNG")
        End Get
    End Property

    Public Shared ReadOnly Property Message_VRF_TXTB_PNG As Bitmap
        Get
            Return Img("Message_VRF_TXTB_PNG")
        End Get
    End Property

    Public Shared ReadOnly Property mit_Btn As Bitmap
        Get
            Return Img("mit_Btn")
        End Get
    End Property

    Public Shared ReadOnly Property mit_lic As Bitmap
        Get
            Return Img("mit_lic")
        End Get
    End Property

    Public Shared ReadOnly Property NextClick As Bitmap
        Get
            Return Img("NextClick")
        End Get
    End Property

    Public Shared ReadOnly Property PASSWORD_HIDE As Bitmap
        Get
            Return Img("PASSWORD_HIDE")
        End Get
    End Property

    Public Shared ReadOnly Property PASSWORD_SHOW As Bitmap
        Get
            Return Img("PASSWORD_SHOW")
        End Get
    End Property

    Public Shared ReadOnly Property PFWS_OFF As Bitmap
        Get
            Return Img("PFWS_OFF")
        End Get
    End Property

    Public Shared ReadOnly Property PFWS_ON As Bitmap
        Get
            Return Img("PFWS_ON")
        End Get
    End Property

    Public Shared ReadOnly Property PFWS_RA As Bitmap
        Get
            Return Img("PFWS_RA")
        End Get
    End Property

    Public Shared ReadOnly Property PWD_Gen As Bitmap
        Get
            Return Img("PWD_Gen")
        End Get
    End Property

    Public Shared ReadOnly Property PWD_KDF As Bitmap
        Get
            Return Img("PWD_KDF")
        End Get
    End Property

    Public Shared ReadOnly Property PWD_KDF_C As Bitmap
        Get
            Return Img("PWD_KDF_C")
        End Get
    End Property

    Public Shared ReadOnly Property PWD_KDF_LINE As Bitmap
        Get
            Return Img("PWD_KDF_LINE")
        End Get
    End Property

    Public Shared ReadOnly Property PWD_KDF_PASS As Bitmap
        Get
            Return Img("PWD_KDF_PASS")
        End Get
    End Property

    Public Shared ReadOnly Property PWD_KDF_R As Bitmap
        Get
            Return Img("PWD_KDF_R")
        End Get
    End Property

    Public Shared ReadOnly Property Read_Only_Img As Bitmap
        Get
            Return Img("Read_Only_Img")
        End Get
    End Property

    Public Shared ReadOnly Property SALT_ADD As Bitmap
        Get
            Return Img("SALT_ADD")
        End Get
    End Property

    Public Shared ReadOnly Property SCB_BAR As Bitmap
        Get
            Return Img("SCB_BAR")
        End Get
    End Property

    Public Shared ReadOnly Property SCB_BARV As Bitmap
        Get
            Return Img("SCB_BARV")
        End Get
    End Property

    Public Shared ReadOnly Property SCB_S_DW As Bitmap
        Get
            Return Img("SCB_S_DW")
        End Get
    End Property

    Public Shared ReadOnly Property SCB_S_UP As Bitmap
        Get
            Return Img("SCB_S_UP")
        End Get
    End Property

    Public Shared ReadOnly Property Timer As Bitmap
        Get
            Return Img("Timer")
        End Get
    End Property

    Public Shared ReadOnly Property TimerID_ACT As Bitmap
        Get
            Return Img("TimerID_ACT")
        End Get
    End Property

    Public Shared ReadOnly Property TimerID_INACT As Bitmap
        Get
            Return Img("TimerID_INACT")
        End Get
    End Property

    Public Shared ReadOnly Property Title_LOGIN As Bitmap
        Get
            Return Img("Title_LOGIN")
        End Get
    End Property

    Public Shared ReadOnly Property Title_NORMAL As Bitmap
        Get
            Return Img("Title_NORMAL")
        End Get
    End Property

    Public Shared ReadOnly Property Title_Password As Bitmap
        Get
            Return Img("Title_Password")
        End Get
    End Property

    Public Shared ReadOnly Property TOPSEC As Bitmap
        Get
            Return Img("TOPSEC")
        End Get
    End Property

    Public Shared ReadOnly Property WARN_1 As Bitmap
        Get
            Return Img("WARN_1")
        End Get
    End Property

    Public Shared ReadOnly Property WARN_2 As Bitmap
        Get
            Return Img("WARN_2")
        End Get
    End Property

    Public Shared ReadOnly Property WinMin As Bitmap
        Get
            Return Img("WinMin")
        End Get
    End Property

End Class
