Imports Windows.Graphics.Imaging

Public Class ParkingLotPart

    Private _Image As SoftwareBitmap
    Public Property Image() As SoftwareBitmap
        Get
            Return _Image
        End Get
        Set(ByVal value As SoftwareBitmap)
            _Image = value
        End Set
    End Property

    Private _LastUpdate As String
    Public Property LastUpdate() As String
        Get
            Return _LastUpdate
        End Get
        Set(ByVal value As String)
            _LastUpdate = value
        End Set
    End Property

End Class

Public Class ParkingLot

    Private _Parts As List(Of ParkingLotPart)
    Public Property Parts() As List(Of ParkingLotPart)
        Get
            Return _Parts
        End Get
        Set(ByVal value As List(Of ParkingLotPart))
            _Parts = value
        End Set
    End Property

    Public Sub New()
        _Parts = New List(Of ParkingLotPart)

        ' Generate first 9 dummies for topline
        ' 6 dummies for bottomline
        Do Until _Parts.Count = 15
            _Parts.Add(New ParkingLotPart() With {.LastUpdate = SetLastUpdate()})
        Loop

    End Sub

    Private Function SetLastUpdate() As String
        Return DateTime.Now.ToString("ddd, HH:mm:ss")
    End Function

End Class