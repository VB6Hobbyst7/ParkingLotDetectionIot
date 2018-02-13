Public Class Snapshot

    Private _Thumbnail As SoftwareBitmapSource
    Public Property Thumbnail() As SoftwareBitmapSource
        Get
            Return _Thumbnail
        End Get
        Set(ByVal value As SoftwareBitmapSource)
            _Thumbnail = value
        End Set
    End Property

    Private _Value As Integer
    Public Property Value() As Integer
        Get
            Return _Value
        End Get
        Set(ByVal value As Integer)
            _Value = value
        End Set
    End Property

    Private _Text As String
    Public Property Text() As String
        Get
            Return _Text
        End Get
        Set(ByVal value As String)
            _Text = value
        End Set
    End Property

    Public Sub New()

    End Sub

    Public Sub New(ByVal thumbnail As SoftwareBitmapSource, ByVal value As Integer, ByVal text As String)
        _Thumbnail = thumbnail
        _Value = value
        _Text = text
    End Sub

End Class
