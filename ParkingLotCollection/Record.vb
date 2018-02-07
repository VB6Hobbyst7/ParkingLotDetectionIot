Public Class Record

    Private _LastUpdate As DateTime
    Public Property LastUpdate() As DateTime
        Get
            Return _LastUpdate
        End Get
        Set(ByVal value As DateTime)
            _LastUpdate = value
        End Set
    End Property

    Private _TopLine As Boolean
    Public Property TopLine() As Boolean
        Get
            Return _TopLine
        End Get
        Set(ByVal value As Boolean)
            _TopLine = value
        End Set
    End Property

    Private _Thumbnail As Byte()
    Public Property Thumbnail() As Byte()
        Get
            Return _Thumbnail
        End Get
        Set(ByVal value As Byte())
            _Thumbnail = value
        End Set
    End Property

    Public Sub New()

    End Sub

End Class
