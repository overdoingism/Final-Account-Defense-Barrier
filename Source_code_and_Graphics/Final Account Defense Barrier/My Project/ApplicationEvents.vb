'MIT License
'Copyright (c) 2023 overdoingism Labs.
'https://github.com/overdoingism/Final-Account-Defense-Barrier

Imports Microsoft.VisualBasic.ApplicationServices

Namespace My

    Partial Friend Class MyApplication

        ' Runs before any form is created: declare the process DPI aware and read the UI scale,
        ' so that Windows no longer stretches (blurs) the windows on high-DPI screens.
        ' The command line argument NODPI keeps the old behaviour.
        Private Sub MyApplication_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
            UIArt.Init()
            ' render the UI pictures on 4 background threads while the program starts
            Start_PreRender(4)
        End Sub

    End Class

End Namespace
