Imports ColorThiefDotNet

Public Class ImageColor

    Private _ImgC As SolidColorBrush
    Public Property ImgC() As SolidColorBrush
        Get
            Return _ImgC
        End Get
        Set(ByVal value As SolidColorBrush)
            _ImgC = value
        End Set
    End Property

    Public Sub New()

    End Sub

    Public Sub New(ByVal dColor As Color)
        _ImgC = New SolidColorBrush(ConvertDrawingColorToMediaColor(dColor))
    End Sub

    Private Function ConvertDrawingColorToMediaColor(ByVal dColor As Color) As Windows.UI.Color
        Return Windows.UI.Color.FromArgb(dColor.A, dColor.R, dColor.G, dColor.B)
    End Function

End Class
