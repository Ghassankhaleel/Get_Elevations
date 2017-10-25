Imports System
Imports System.IO
Imports System.Collections
Imports MapWinGIS
Public Class Form1
    Dim ManholePoint As String
    Dim ElevPoint As String
    Dim Fd_Indx As Integer
    Dim Fd2_Indx As Integer
    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        TextBox1.Text = Settings1.Default.Setting1
    End Sub

    Private Sub OpenShpFileToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OpenShpFileToolStripMenuItem.Click
        Dim sf As New MapWinGIS.Shapefile
        OpenFileDialog1.Multiselect = True
        OpenFileDialog1.Filter = "Shapefiles: (*.shp)|*.shp"

        If (OpenFileDialog1.ShowDialog = DialogResult.OK) Then
            ManholePoint = OpenFileDialog1.FileName
            CreatField(ManholePoint)
            OpenAltitudeShpFileToolStripMenuItem.Enabled = True
            OpenShpFileToolStripMenuItem.Enabled = False
        End If

    End Sub

    Private Sub OpenAltitudeShpFileToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OpenAltitudeShpFileToolStripMenuItem.Click


        OpenFileDialog1.Multiselect = True
        OpenFileDialog1.Filter = "Shapefiles: (*.shp)|*.shp"

        If (OpenFileDialog1.ShowDialog = DialogResult.OK) Then
            ElevPoint = OpenFileDialog1.FileName

            Dim shp As New MapWinGIS.Shapefile
            shp.Open(ElevPoint)
            Dim k As Integer
            ComboBox1.Items.Clear()
            For k = 0 To shp.NumFields - 1
                ComboBox1.Items.Add(shp.Field(k).Name)
            Next
            ComboBox1.SelectedIndex = shp.NumFields - 1
            Fd_Indx = (ComboBox1.Items.IndexOf(ComboBox1.SelectedItem()))
            Button1.Enabled = True
        End If
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        ReadShp()
        Button1.Enabled = False
        OpenShpFileToolStripMenuItem.Enabled = True
        OpenAltitudeShpFileToolStripMenuItem.Enabled = False
    End Sub

    Private Sub ReadShp()
        Dim manhole As New MapWinGIS.Shapefile
        Dim Elev As New MapWinGIS.Shapefile
        Dim pt As New MapWinGIS.Point
        manhole.Open(ManholePoint)
        manhole.StartEditingTable()
        Elev.Open(ElevPoint)
        ProgressBar1.Maximum = manhole.NumShapes
        Dim i As Integer
        For i = 0 To manhole.NumShapes - 1
            pt = manhole.QuickPoint(i, 0)
            Me.Text = "Processing :" + " " + Int((i / (manhole.NumShapes - 1)) * 100).ToString() + "  %"
            SearchInPolygon(pt, i, manhole, Elev)
            Application.DoEvents()
            ProgressBar1.Value = i
        Next
        manhole.StopEditingTable()
        manhole.Close()
        Elev.Close()
        Me.Text = "Completed"
        ProgressBar1.Refresh()
    End Sub

    Function Distance(ByVal Pt0 As MapWinGIS.Point, ByVal Pt1 As MapWinGIS.Point) As Double
        Distance = Math.Sqrt(((Pt1.x - Pt0.x) * (Pt1.x - Pt0.x)) + ((Pt1.y - Pt0.y) * (Pt1.y - Pt0.y)))
    End Function

   


    Private Sub TextBox1_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox1.TextChanged
        Settings1.Default.Setting1 = TextBox1.Text
        Settings1.Default.Save()
    End Sub

    Private Sub CreatField(ByVal ShpName As String)
        Dim fd As MapWinGIS.Field

        Dim sf As New MapWinGIS.Shapefile
        Try
            sf.Open(ShpName)
            sf.StartEditingTable()
            Fd2_Indx = sf.NumFields
            fd = New MapWinGIS.Field
            fd.Name = "Ground " + TextBox1.Text + "m"
            fd.Type = MapWinGIS.FieldType.DOUBLE_FIELD
            fd.Width = 15
            sf.EditInsertField(fd, Fd2_Indx)
            Dim i As Integer
            sf.StartEditingTable()
            ProgressBar1.Maximum = sf.NumShapes
            For i = 0 To sf.NumShapes - 1
                sf.EditCellValue(Fd2_Indx, i, 0)
                ProgressBar1.Value = i
                Application.DoEvents()
            Next
            sf.StopEditingTable()



        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try

        sf.StopEditingTable()
        sf.Close()

    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBox1.SelectedIndexChanged
        Fd_Indx = (ComboBox1.Items.IndexOf(ComboBox1.SelectedItem()))
    End Sub
    Private Sub SearchInPolygon(ByVal pt As MapWinGIS.Point, ByVal pInd As Long, ByVal manhole As MapWinGIS.Shapefile, ByVal elev As MapWinGIS.Shapefile)
        Dim Extent As New MapWinGIS.Extents

        Dim resultArr(100) As Integer
        Dim Dist(100) As Double
        Dim Elevpt(100) As MapWinGIS.Point
        Dim i As Integer
        Dim su As Boolean
        Dim min As Double
        Dim y2, Elv2 As Double
        Dim s As Integer
        y2 = 10000.0
        Dim v4 As Double



        Extent.SetBounds(pt.x - Val(TextBox1.Text), pt.y + Val(TextBox1.Text), 0, pt.x + Val(TextBox1.Text), pt.y - Val(TextBox1.Text), 0)
        su = elev.SelectShapes(Extent, 0, SelectMode.INCLUSION, resultArr)


        If su = True Then
             For i = 0 To resultArr.Length - 1
                Elevpt(i) = elev.QuickPoint(resultArr(i), 0)
                Dist(i) = Distance(Elevpt(i), pt)

            Next
 
            For i = 0 To resultArr.Length - 1
                v4 = Dist(i)
                If v4 < y2 Then
                    y2 = v4
                    min = y2
                    s = i

                End If
            Next i
            Elv2 = elev.CellValue(Fd_Indx, resultArr(s))
            manhole.EditCellValue(Fd2_Indx, pInd, Elv2)



        End If


    End Sub
     
    Private Sub Form1_FormClosing(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        Application.Exit()
    End Sub

    Private Sub ExitToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitToolStripMenuItem.Click
        Application.Exit()
    End Sub

    Private Sub RefreshToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RefreshToolStripMenuItem.Click
        OpenShpFileToolStripMenuItem.Enabled = True
        OpenAltitudeShpFileToolStripMenuItem.Enabled = False
        ComboBox1.Items.Clear()
        Button1.Enabled = False
        ElevPoint = ""
        ManholePoint = ""

    End Sub
End Class
