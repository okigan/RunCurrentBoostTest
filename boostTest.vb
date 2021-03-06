' Run the currently selected Boost.Test
' \author: Igor Okulist
'
' How to install 
' http://code.google.com/p/chromium/wiki/WindowsVisualStudioMacros#How_to_install_one
'
' Adapted from 'Run the currently selected google test' 
' http://code.google.com/p/chromium/wiki/WindowsVisualStudioMacros#Run_the_currently_selected_google_test

Imports System
Imports EnvDTE
Imports EnvDTE80
Imports EnvDTE90
Imports EnvDTE90a
Imports EnvDTE100
Imports System.Diagnostics
Imports System.Text.RegularExpressions

Public Module boostTest
    Sub RunCurrentBoostTest()
        ' From the active source file, find the test that the user wants to run'
        ' based on the current cursor position. Set the project containing this'
        ' source file as the startup project, changes the command line to run'
        ' only this test, compile the project and starts it under the debugger.'
        ' Doesn\'t change any breakpoint.'
        Dim ActiveDoc As Document = DTE.ActiveDocument

        ' Try to guess the test to run.'
        Dim TestName As String = ""
        Dim selection As TextSelection = CType(ActiveDoc.Selection(), TextSelection)
        Dim toppoint As EditPoint = selection.TopPoint.CreateEditPoint()
        Dim bottompoint As EditPoint = selection.BottomPoint.CreateEditPoint()
        Dim ranges As TextRanges = selection.TextRanges
        Dim line As Integer = selection.TopPoint.Line
        ' selection.FindPattern() is crummy.'
        While line <> 0
            selection.GotoLine(line)
            selection.SelectLine()
            Dim expression As String = "BOOST_AUTO_TEST_CASE.*?\((.*)\)"

            Dim match As Match = Regex.Match(selection.Text, expression)
            If Not match Is match.Empty Then
                TestName = match.Groups.Item(1).Value.Trim()
                Exit While
            End If
            line = line - 1
        End While
        ' Cheap way to try to restore the old selection. Isn\'t 100% correct.'
        selection.MoveToLineAndOffset(toppoint.Line, toppoint.LineCharOffset)
        selection.MoveToLineAndOffset(bottompoint.Line, bottompoint.LineCharOffset, True)

        ' From the current active document, find the project and the active configuration.'
        Dim Proj As Project = ActiveDoc.ProjectItem.ContainingProject
        Dim config As Configuration = Proj.ConfigurationManager.ActiveConfiguration

        ' Fix the command line argument.'
        Dim CmdLine As EnvDTE.Property = config.Properties.Item("CommandArguments")
        If TestName <> "" Then
            CmdLine.Value = "--run_test=" & TestName
        Else
            ' Run all'
            CmdLine.Value = ""
        End If

        ' Set it as startup project.'
        Dim SoluBuild As SolutionBuild = DTE.Solution.SolutionBuild
        Dim StartupProject As String
        StartupProject = Proj.UniqueName
        SoluBuild.StartupProjects = StartupProject

        ' Start it. (will be prompted to build if needed)'
        DTE.Debugger.Go()
    End Sub

End Module
