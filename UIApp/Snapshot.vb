Imports ColorThiefDotNet

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

    Private _Color As QuantizedColor
    Public Property Color() As QuantizedColor
        Get
            Return _Color
        End Get
        Set(ByVal value As QuantizedColor)
            _Color = value
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

    Public Sub New(ByVal thumbnail As SoftwareBitmapSource, ByVal color As QuantizedColor, ByVal text As String)
        _Thumbnail = thumbnail
        _Color = color
        _Value = _Color.Color.B
        _Text = text
    End Sub

End Class
