Imports System.IO
Imports System.Threading
Imports System.ComponentModel
Imports System.Text
Imports System.Security.Cryptography



Public Class Main_Screen

    Private busyworking As Boolean = False

    Private lastinputline As String = ""
    Private inputlines As Long = 0
    Private highestPercentageReached As Integer = 0
    Private inputlinesprecount As Long = 0
    Private pretestdone As Boolean = False
    Private primary_PercentComplete As Integer = 0
    Private percentComplete As Integer

    Private SelectedIndex As Integer = 0

    Private backupdirectory As String = ""
    Private savedirectory As String = ""

    Private AlertMessage As String = ""




    Private Sub Error_Handler(ByVal ex As Exception, Optional ByVal identifier_msg As String = "")
        Try
            If ex.Message.IndexOf("Thread was being aborted") < 0 Then
                Dim Display_Message1 As New Display_Message()
                Display_Message1.Message_Textbox.Text = "The Application encountered the following problem: " & vbCrLf & identifier_msg & ": " & ex.Message.ToString

                Display_Message1.Timer1.Interval = 1000
                Display_Message1.ShowDialog()
                Dim dir As System.IO.DirectoryInfo = New System.IO.DirectoryInfo((Application.StartupPath & "\").Replace("\\", "\") & "Error Logs")
                If dir.Exists = False Then
                    dir.Create()
                End If
                dir = Nothing
                Dim filewriter As System.IO.StreamWriter = New System.IO.StreamWriter((Application.StartupPath & "\").Replace("\\", "\") & "Error Logs\" & Format(Now(), "yyyyMMdd") & "_Error_Log.txt", True)
                filewriter.WriteLine("#" & Format(Now(), "dd/MM/yyyy hh:mm:ss tt") & " - " & identifier_msg & ": " & ex.ToString)
                filewriter.WriteLine("")
                filewriter.Flush()
                filewriter.Close()
                filewriter = Nothing
            End If
            ex = Nothing
            identifier_msg = Nothing
        Catch exc As Exception
            MsgBox("An error occurred in the application's error handling routine. The application will try to recover from this serious error.", MsgBoxStyle.Critical, "Critical Error Encountered")
        End Try
    End Sub




   



    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Dim result As DialogResult
        result = FolderBrowserDialog1.ShowDialog
        If result = Windows.Forms.DialogResult.OK Then
            TextBox1.Text = FolderBrowserDialog1.SelectedPath
        End If
    End Sub


    


    Private Sub cancelAsyncButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cancelAsyncButton.Click

        ' Cancel the asynchronous operation.
        Me.BackgroundWorker1.CancelAsync()

        ' Disable the Cancel button.
        cancelAsyncButton.Enabled = False
        sender = Nothing
        e = Nothing
    End Sub 'cancelAsyncButton_Click

    Private Sub PreCount_Function(ByVal worker As BackgroundWorker)
        Try
            inputlinesprecount = 0
            inputlines = 0
            Dim dinfo As DirectoryInfo
            dinfo = New DirectoryInfo(TextBox1.Text)
            'Dim backupfolder As String = (Application.StartupPath & "\").Replace("\\", "\") & "WP7D Backup " & Format(Now, "yyyyMMddHHmmss")
            'backupdirectory = backupfolder
            'If My.Computer.FileSystem.DirectoryExists(backupfolder) = False Then
            '    My.Computer.FileSystem.CreateDirectory(backupfolder)
            'End If

            For Each finfo As DirectoryInfo In dinfo.GetDirectories
                'If My.Computer.FileSystem.FileExists((finfo.FullName & "\Build.txt").Replace("\\", "\")) Then
                '    Dim mfinfo As FileInfo = New FileInfo((finfo.FullName & "\Build.txt").Replace("\\", "\"))
                '    mfinfo.CopyTo((backupfolder & "\" & finfo.Name & " - Build.txt").Replace("\\", "\"))
                '    lastinputline = "Backed up: " & mfinfo.Name
                'Else
                '    AlertMessage = AlertMessage & "Missing Build.txt File: " & finfo.Name & vbCrLf
                'End If
                inputlinesprecount = inputlinesprecount + 1
                inputlines = inputlines + 1
                worker.ReportProgress(0)
                finfo = Nothing
            Next

            'If inputlinesprecount < 1 Then
            '    My.Computer.FileSystem.DeleteDirectory(backupfolder, FileIO.DeleteDirectoryOption.DeleteAllContents)
            'End If

        Catch ex As Exception
            Error_Handler(ex, "PreCount_Function")
        End Try
    End Sub

    Private Sub startAsyncButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles startAsyncButton.Click
        Try
            If busyworking = False Then
                If My.Computer.FileSystem.DirectoryExists(TextBox1.Text) Then
                    If My.Computer.FileSystem.FileExists(TextBox2.Text) Then

                        busyworking = True


                        inputlines = 0
                        lastinputline = ""
                        highestPercentageReached = 0
                        inputlinesprecount = 0

                        backupdirectory = ""
                        savedirectory = ""
                        pretestdone = False

                        TextBox1.Enabled = False
                        TextBox2.Enabled = False
                        Button1.Enabled = False
                        Button2.Enabled = False
                        startAsyncButton.Enabled = False
                        cancelAsyncButton.Enabled = True
                        ' Start the asynchronous operation.
                        AlertMessage = ""

                        BackgroundWorker1.RunWorkerAsync(TextBox1.Text)
                    Else
                        MsgBox("Please ensure that you select an existing Advanced Installer 1.9 executable to use", MsgBoxStyle.Information, "Invalid Directory Selected")
                    End If
                Else
                    MsgBox("Please ensure that you select an existing directory to process", MsgBoxStyle.Information, "Invalid Directory Selected")
                End If
                End If
        Catch ex As Exception
            Error_Handler(ex, "StartWorker")
        End Try
    End Sub

    ' This event handler is where the actual work is done.
    Private Sub backgroundWorker1_DoWork(ByVal sender As Object, ByVal e As DoWorkEventArgs) Handles BackgroundWorker1.DoWork

        ' Get the BackgroundWorker object that raised this event.
        Dim worker As BackgroundWorker = CType(sender, BackgroundWorker)

        ' Assign the result of the computation
        ' to the Result property of the DoWorkEventArgs
        ' object. This is will be available to the 
        ' RunWorkerCompleted eventhandler.
        e.Result = MainWorkerFunction(worker, e)
        sender = Nothing
        e = Nothing
        worker.Dispose()
        worker = Nothing
    End Sub 'backgroundWorker1_DoWork

    ' This event handler deals with the results of the
    ' background operation.
    Private Sub backgroundWorker1_RunWorkerCompleted(ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        busyworking = False


        ' First, handle the case where an exception was thrown.
        If Not (e.Error Is Nothing) Then
            Error_Handler(e.Error, "backgroundWorker1_RunWorkerCompleted")
        ElseIf e.Cancelled Then
            ' Next, handle the case where the user canceled the 
            ' operation.
            ' Note that due to a race condition in 
            ' the DoWork event handler, the Cancelled
            ' flag may not have been set, even though
            ' CancelAsync was called.
            Me.ToolStripStatusLabel1.Text = "Operation Cancelled" & "   (" & inputlines & " of " & inputlinesprecount & ")"
            Me.ProgressBar1.Value = 0

        Else
            ' Finally, handle the case where the operation succeeded.
            Me.ToolStripStatusLabel1.Text = "Operation Completed" & "   (" & inputlines & " of " & inputlinesprecount & ")"
            Me.ProgressBar1.Value = 100
            If AlertMessage.Length > 0 Then
                'MsgBox("The following alerts were raised during the operation. If you wish to save these alerts, press Ctrl+C and paste it into NotePad." & vbCrLf & vbCrLf & "********************" & vbCrLf & vbCrLf & AlertMessage, MsgBoxStyle.Information, "Raised Alerts")
                MsgBox(AlertMessage & " copies of AutoUpdate.ico were distributed", MsgBoxStyle.Information, "Copies Distributed")
            End If
        End If

        TextBox1.Enabled = True
        TextBox2.Enabled = True
        Button1.Enabled = True
        Button2.Enabled = True
        startAsyncButton.Enabled = True
        cancelAsyncButton.Enabled = False

        sender = Nothing
        e = Nothing


    End Sub 'backgroundWorker1_RunWorkerCompleted

    Private Sub backgroundWorker1_ProgressChanged(ByVal sender As Object, ByVal e As ProgressChangedEventArgs) Handles BackgroundWorker1.ProgressChanged


        Me.ProgressBar1.Value = e.ProgressPercentage
        'If lastinputline.StartsWith("Operation Completed") Then
        'Me.ToolStripStatusLabel1.Text = lastinputline
        'Else
        Me.ToolStripStatusLabel1.Text = lastinputline & "   (" & inputlines & " of " & inputlinesprecount & ")"
        'End If


        sender = Nothing
        e = Nothing
    End Sub

    Function MainWorkerFunction(ByVal worker As BackgroundWorker, ByVal e As DoWorkEventArgs) As Boolean
        Dim result As Boolean = False
        Try
            If Me.pretestdone = False Then
                primary_PercentComplete = 0
                worker.ReportProgress(0)
                PreCount_Function(worker)
                Me.pretestdone = True
            End If

            If worker.CancellationPending Then
                e.Cancel = True
                Return False
            End If

            primary_PercentComplete = 0
            worker.ReportProgress(0)

            inputlines = 0
            lastinputline = ""


            Dim dinfo As DirectoryInfo
            dinfo = New DirectoryInfo(TextBox1.Text)

            If dinfo.Exists Then
                For Each subdir As DirectoryInfo In dinfo.GetDirectories
                    lastinputline = "Processing: " & subdir.Name
                    ' Report progress as a percentage of the total task.
                    percentComplete = 0
                    If inputlinesprecount > 0 Then
                        percentComplete = CSng(inputlines) / CSng(inputlinesprecount) * 100
                    Else
                        percentComplete = 100
                    End If
                    primary_PercentComplete = percentComplete
                    If percentComplete > 100 Then
                        percentComplete = 100
                    End If
                    If percentComplete = 100 Then
                        lastinputline = "Operation Completed"
                    End If
                    If percentComplete > highestPercentageReached Then
                        highestPercentageReached = percentComplete
                        worker.ReportProgress(percentComplete)
                    End If
                    If worker.CancellationPending Then
                        e.Cancel = True
                        Exit For
                        Return False
                    End If

                    Try

                        Dim sourcecodedir As String = ""
                        sourcecodedir = subdir.Name.Substring(0, subdir.Name.LastIndexOf(" - "))

                        If My.Computer.FileSystem.FileExists((subdir.FullName & "\Source Code\" & sourcecodedir & " Installer\" & sourcecodedir & " Installer.aip").Replace("\\", "\")) = True Then
                            If My.Computer.FileSystem.DirectoryExists((subdir.FullName & "\Release\").Replace("\\", "\")) Then
                                Dim dinfo2 As DirectoryInfo = New DirectoryInfo((subdir.FullName & "\Release\").Replace("\\", "\"))
                                For Each finfo2 As FileInfo In dinfo2.GetFiles
                                    If finfo2.Extension.ToLower = ".cab" Then
                                        finfo2.Delete()
                                    End If
                                    If finfo2.Name = sourcecodedir & " Installer.exe" Then
                                        finfo2.Delete()
                                    End If
                                    If finfo2.Name = sourcecodedir & " Installer.msi" Then
                                        finfo2.Delete()
                                    End If
                                    finfo2 = Nothing
                                Next
                                dinfo2 = Nothing
                            Else
                                My.Computer.FileSystem.CreateDirectory((subdir.FullName & "\Release\").Replace("\\", "\"))
                            End If
                            If My.Computer.FileSystem.FileExists(TextBox2.Text) = True Then
                                If My.Computer.FileSystem.DirectoryExists((subdir.FullName & "\Source Code\" & sourcecodedir & " Installer\Release\").Replace("\\", "\")) Then
                                    Dim dinfo3 As DirectoryInfo = New DirectoryInfo((subdir.FullName & "\Source Code\" & sourcecodedir & " Installer\Release\").Replace("\\", "\"))
                                    For Each finfo2 As FileInfo In dinfo3.GetFiles
                                        If finfo2.Extension.ToLower = ".cab" Then
                                            finfo2.Delete()
                                        End If
                                        If finfo2.Name = sourcecodedir & " Installer.exe" Then
                                            finfo2.Delete()
                                        End If
                                        If finfo2.Name = sourcecodedir & " Installer.msi" Then
                                            finfo2.Delete()
                                        End If
                                        finfo2 = Nothing
                                    Next
                                    dinfo3 = Nothing
                                Else
                                    My.Computer.FileSystem.CreateDirectory((subdir.FullName & "\Source Code\" & sourcecodedir & " Installer\Release\").Replace("\\", "\"))
                                End If
                                Dim info As System.Diagnostics.ProcessStartInfo = New System.Diagnostics.ProcessStartInfo
                                info.Arguments = "/build " & """" & (subdir.FullName & "\Source Code\" & sourcecodedir & " Installer\" & sourcecodedir & " Installer.aip").Replace("\\", "\") & """"
                                info.CreateNoWindow = True
                                info.ErrorDialog = True
                                info.FileName = (TextBox2.Text)
                                Try
                                    Dim proc As System.Diagnostics.Process = Process.Start(info)
                                    While proc.HasExited = False
                                        'wait
                                    End While
                                    proc = Nothing
                                Catch ex As Exception
                                    Error_Handler(ex, "Running Advanced Installer")
                                End Try
                                Dim dinfo2 As DirectoryInfo = New DirectoryInfo((subdir.FullName & "\Source Code\" & sourcecodedir & " Installer\Release\").Replace("\\", "\"))
                                For Each finfo2 As FileInfo In dinfo2.GetFiles
                                    If finfo2.Extension.ToLower = ".cab" Then
                                        finfo2.MoveTo((subdir.FullName & "\Release\" & finfo2.Name).Replace("\\", "\"))
                                    End If
                                    If finfo2.Name = sourcecodedir & " Installer.exe" Then
                                        finfo2.MoveTo((subdir.FullName & "\Release\" & finfo2.Name).Replace("\\", "\"))
                                    End If
                                    If finfo2.Name = sourcecodedir & " Installer.msi" Then
                                        finfo2.MoveTo((subdir.FullName & "\Release\" & finfo2.Name).Replace("\\", "\"))
                                    End If
                                    finfo2 = Nothing
                                Next
                                dinfo2 = Nothing
                            End If
                        End If
                    Catch ex As Exception
                        Error_Handler(ex, "MainWorkerFunction")
                    End Try
                    inputlines = inputlines + 1
                    lastinputline = "Processed: " & (subdir.Name & "\Build.txt").Replace("\\", "\")
                    ' Report progress as a percentage of the total task.
                    percentComplete = 0
                    If inputlinesprecount > 0 Then
                        percentComplete = CSng(inputlines) / CSng(inputlinesprecount) * 100
                    Else
                        percentComplete = 100
                    End If
                    primary_PercentComplete = percentComplete
                    If percentComplete > 100 Then
                        percentComplete = 100
                    End If
                    If percentComplete = 100 Then
                        lastinputline = "Operation Completed"
                    End If
                    If percentComplete > highestPercentageReached Then
                        highestPercentageReached = percentComplete
                        worker.ReportProgress(percentComplete)
                    End If
                    subdir = Nothing
                    If worker.CancellationPending Then
                        e.Cancel = True
                        Exit For
                        dinfo = Nothing
                        Return False
                    End If
                Next
            End If
            dinfo = Nothing




        Catch ex As Exception
            Error_Handler(ex, "MainWorkerFunction")
        End Try
        worker.Dispose()
        worker = Nothing
        e = Nothing
        Return result

    End Function

    Private Sub Form1_Close(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed
        Try
            Me.ToolStripStatusLabel1.Text = "Application Closing"
            SaveSettings()
        Catch ex As Exception
            Error_Handler(ex, "Application Close")
        End Try
    End Sub

    Private Sub LoadSettings()
        Try
            Dim configfile As String = (Application.StartupPath & "\config.sav").Replace("\\", "\")
            If My.Computer.FileSystem.FileExists(configfile) Then
                Dim reader As StreamReader = New StreamReader(configfile)
                Dim lineread As String
                Dim variablevalue As String
                While reader.Peek <> -1
                    lineread = reader.ReadLine
                    If lineread.IndexOf("=") <> -1 Then

                        variablevalue = lineread.Remove(0, lineread.IndexOf("=") + 1)

                        If lineread.StartsWith("ImageFolder=") Then
                            Dim dinfo As DirectoryInfo = New DirectoryInfo(variablevalue)
                            If dinfo.Exists Then
                                FolderBrowserDialog1.SelectedPath = variablevalue
                                TextBox1.Text = variablevalue
                            End If
                            dinfo = Nothing
                        End If
                        If lineread.StartsWith("Executable=") Then
                            Dim dinfo As FileInfo = New FileInfo(variablevalue)
                            If dinfo.Exists Then
                                OpenFileDialog1.FileName = variablevalue
                                TextBox2.Text = variablevalue
                            End If
                            dinfo = Nothing
                        End If

                    
                    End If
                End While
                reader.Close()
                reader = Nothing
            End If
        Catch ex As Exception
            Error_Handler(ex, "Load Settings")
        End Try
    End Sub

    Private Sub SaveSettings()
        Try
            Dim configfile As String = (Application.StartupPath & "\config.sav").Replace("\\", "\")

            Dim writer As StreamWriter = New StreamWriter(configfile, False)

            If TextBox1.Text.Length > 0 Then
                Dim dinfo As DirectoryInfo = New DirectoryInfo(TextBox1.Text)
                If dinfo.Exists Then
                    writer.WriteLine("ImageFolder=" & TextBox1.Text)
                End If
                dinfo = Nothing
            End If
            If TextBox2.Text.Length > 0 Then
                Dim dinfo As FileInfo = New FileInfo(TextBox2.Text)
                If dinfo.Exists Then
                    writer.WriteLine("Executable=" & TextBox2.Text)
                End If
                dinfo = Nothing
            End If
         
            writer.Flush()
            writer.Close()
            writer = Nothing

        Catch ex As Exception
            Error_Handler(ex, "Save Settings")
        End Try
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Try
            Control.CheckForIllegalCrossThreadCalls = False
            Me.Text = My.Application.Info.ProductName & " " & Format(My.Application.Info.Version.Major, "0000") & Format(My.Application.Info.Version.Minor, "00") & Format(My.Application.Info.Version.Build, "00") & "." & Format(My.Application.Info.Version.Revision, "00") & ""
            LoadSettings()
            Me.ToolStripStatusLabel1.Text = "Application Loaded"
        Catch ex As Exception
            Error_Handler(ex, "Application Load")
        End Try

    End Sub




    Private Sub AboutToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AboutToolStripMenuItem.Click
        Try
            Me.ToolStripStatusLabel1.Text = "About displayed"
            AboutBox1.ShowDialog()
        Catch ex As Exception
            Error_Handler(ex, "Display About Screen")
        End Try
    End Sub

    Private Sub HelpToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles HelpToolStripMenuItem.Click
        Try
            Me.ToolStripStatusLabel1.Text = "Help displayed"
            HelpBox1.ShowDialog()
        Catch ex As Exception
            Error_Handler(ex, "Display Help Screen")
        End Try
    End Sub



    Private Sub TextBox1_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles TextBox1.DragDrop
        Try
            If e.Data.GetDataPresent(DataFormats.FileDrop) Then
                Dim MyFiles() As String
                Dim i As Integer

                ' Assign the files to an array.
                MyFiles = e.Data.GetData(DataFormats.FileDrop)
                ' Loop through the array and add the files to the list.
                'For i = 0 To MyFiles.Length - 1
                If MyFiles.Length > 0 Then
                    Dim finfo As DirectoryInfo = New DirectoryInfo(MyFiles(0))
                    If finfo.Exists = True Then
                        TextBox1.Text = (MyFiles(0))
                        FolderBrowserDialog1.SelectedPath = (MyFiles(0))
                    End If
                End If
                'Next
            End If
        Catch ex As Exception
            Error_Handler(ex)
        End Try
    End Sub

    Private Sub TextBox1_DragEnter(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles TextBox1.DragEnter
        Try
            If e.Data.GetDataPresent(DataFormats.FileDrop) Then
                e.Effect = DragDropEffects.All
            End If
        Catch ex As Exception
            Error_Handler(ex)
        End Try
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Try
            Dim result As DialogResult
            result = OpenFileDialog1.ShowDialog
            If result = Windows.Forms.DialogResult.OK Then
                TextBox2.Text = OpenFileDialog1.FileName
            End If
        Catch ex As Exception
            Error_Handler(ex, "Select Advanced Installer Executable")
        End Try
    End Sub

    Private Sub TextBox2_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles TextBox2.DragDrop
        Try
            If e.Data.GetDataPresent(DataFormats.FileDrop) Then
                Dim MyFiles() As String
                Dim i As Integer

                ' Assign the files to an array.
                MyFiles = e.Data.GetData(DataFormats.FileDrop)
                ' Loop through the array and add the files to the list.
                'For i = 0 To MyFiles.Length - 1
                If MyFiles.Length > 0 Then
                    Dim finfo As FileInfo = New FileInfo(MyFiles(0))
                    If finfo.Exists = True Then
                        TextBox2.Text = (MyFiles(0))
                        OpenFileDialog1.FileName = (MyFiles(0))
                    End If
                End If
                'Next
            End If
        Catch ex As Exception
            Error_Handler(ex)
        End Try
    End Sub

    Private Sub TextBox2_DragEnter(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles TextBox2.DragEnter
        Try
            If e.Data.GetDataPresent(DataFormats.FileDrop) Then
                e.Effect = DragDropEffects.All
            End If
        Catch ex As Exception
            Error_Handler(ex)
        End Try
    End Sub
End Class
