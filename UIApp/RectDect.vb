Public Class RectDect
    Implements INotifyPropertyChanged

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Private _IsSelected As Boolean
    Public Property IsSelected() As Boolean
        Get
            Return _IsSelected
        End Get
        Set(ByVal value As Boolean)
            _IsSelected = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("IsSelected"))
        End Set
    End Property

    Private _IndexWidth As Integer
    Public Property IndexWidth() As Integer
        Get
            Return _IndexWidth
        End Get
        Set(ByVal value As Integer)
            _IndexWidth = value
        End Set
    End Property

    Private _IndexHeight As Integer
    Public Property IndexHeight() As Integer
        Get
            Return _IndexHeight
        End Get
        Set(ByVal value As Integer)
            _IndexHeight = value
        End Set
    End Property

    Private _X As Integer
    Public Property X() As Integer
        Get
            Return _X
        End Get
        Set(ByVal value As Integer)
            _X = value
        End Set
    End Property

    Private _Y As Integer
    Public Property Y() As Integer
        Get
            Return _Y
        End Get
        Set(ByVal value As Integer)
            _Y = value
        End Set
    End Property

    Private _Width As Integer
    Public Property Width() As Integer
        Get
            Return _Width
        End Get
        Set(ByVal value As Integer)
            _Width = value
        End Set
    End Property

    Private _Height As Integer
    Public Property Height() As Integer
        Get
            Return _Height
        End Get
        Set(ByVal value As Integer)
            _Height = value
        End Set
    End Property

    Private _BorderColor As SolidColorBrush
    Public Property BorderColor() As SolidColorBrush
        Get
            Return _BorderColor
        End Get
        Set(ByVal value As SolidColorBrush)
            _BorderColor = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("BorderColor"))
        End Set
    End Property

    Public Sub New()

    End Sub

    Public Sub SetSelected(ByVal selected As Boolean)
        _IsSelected = selected

        If _IsSelected = True Then
            _BorderColor = New Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.LightGreen)
        Else
            _BorderColor = New Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Red)
        End If

    End Sub

End Class
