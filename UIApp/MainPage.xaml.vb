Imports System.Drawing
Imports ColorThiefDotNet
Imports OpenCVBridge
Imports Windows.Devices.Enumeration
Imports Windows.Graphics.Imaging
Imports Windows.Media.Capture
Imports Windows.Media.MediaProperties
Imports Windows.Storage
Imports Windows.Storage.FileProperties
Imports Windows.Storage.Streams
Imports Windows.System.Display
Imports Windows.System.Threading

Public NotInheritable Class MainPage
    Inherits Page

    Private _mediaCapture As MediaCapture
    Private isPreviewing As Boolean
    Private _displayRequest As DisplayRequest
    Private WithEvents _application As Application
    Private _periodicTask As ThreadPoolTimer
    Private _openHelper As OpenCVHelper
    Private _colorThief As ColorThief

    Private RectGrid As New List(Of RectDect)

    Private RectInWidth As Integer = 16
    Private RectInHeight As Integer = 10

    Public Sub New()

        ' Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent()

        ' Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        _application = Application.Current
        _colorThief = New ColorThief

    End Sub

    Private Async Sub MainPage_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Try
            SliderHeight.Value = RectInHeight
            SliderWidth.Value = RectInWidth

            Await StartPreviewAsync()

        Catch ex As Exception
            DebugMessage(ex.Message)
        End Try
    End Sub

    Protected Overrides Async Sub OnNavigatingFrom(e As NavigatingCancelEventArgs)
        Await StopPreviewAsync()
    End Sub

    Private Async Sub PeriodicTask()
        Await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, Async Sub()
                                                                                     Await CaptureSnapshot()
                                                                                 End Sub)
    End Sub


    Private Async Function StartPreviewAsync() As Task
        Dim props As VideoEncodingProperties = Nothing
        Try
            ' Initialize OpenCVBridge
            _openHelper = New OpenCVHelper

            ' Start webcam preview
            _mediaCapture = New MediaCapture
            Await _mediaCapture.InitializeAsync()

            ' Dont let the display go to standby
            _displayRequest = New DisplayRequest
            _displayRequest.RequestActive()
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape

            ' Preview webcam in fontend
            PreviewControl.Source = _mediaCapture
            Await _mediaCapture.StartPreviewAsync()
            isPreviewing = True

            ' Detect video resolution
            props = _mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview)
            PreviewControl.Width = props.Width
            PreviewControl.Height = props.Height
            LayerControl.Width = props.Width
            LayerControl.Height = props.Height
            DebugMessage("Resolution " & props.Width & "x" & props.Height, "INFO")

            ' Periodic Task
            _periodicTask = ThreadPoolTimer.CreatePeriodicTimer(AddressOf PeriodicTask, TimeSpan.FromMinutes(5))

            ' 16 Rectangles in Width
            Dim width As Integer = props.Width / RectInWidth

            ' 10 Rectangles in Height
            Dim height As Integer = props.Height / RectInHeight

            Dim RectCount As Integer = RectInWidth * RectInHeight

            ' Sum 40 Boxes
            Dim indexWidth As Integer = 1
            Dim indexHeight As Integer = 1
            Do Until RectGrid.Count = RectCount

                If indexWidth > RectInWidth Then
                    indexWidth = 1
                End If

                If indexHeight > RectInHeight Then
                    indexHeight = 1
                End If

                RectGrid.Add(New RectDect With {.IndexWidth = indexWidth,
                    .IndexHeight = indexHeight,
                             .IsSelected = False,
                             .BorderColor = New Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Red),
                             .Width = width,
                             .Height = height,
                             .X = GetCoordinateX(.IndexWidth, .Width),
                             .Y = GetCoordinateY(.IndexHeight, .Height)})

                indexWidth += 1
                If indexWidth > RectInWidth Then
                    indexHeight += 1
                End If

            Loop

            LayerControl.ItemsSource = Nothing
            LayerControl.ItemsSource = RectGrid


        Catch ex As Exception
            DebugMessage(ex.Message)
            _displayRequest.RequestRelease()
        Finally
            props = Nothing
        End Try
    End Function

    'Private Function GetIndex(ByVal totalCount As Integer, ByVal MaxIndex As Integer) As Integer
    '    Dim r = totalCount Mod MaxIndex
    '    If r = 0 Then
    '        Return MaxIndex
    '    Else
    '        Return r
    '    End If
    'End Function

    Private Function GetCoordinateX(ByVal index As Integer, ByVal width As Integer) As Integer
        If index = 1 Then
            Return 0
        Else
            Return width * (index - 1)
        End If
    End Function

    Private Function GetCoordinateY(ByVal index As Integer, ByVal height As Integer) As Integer
        If index = 1 Then
            Return 0
        Else
            Return height * (index - 1)
        End If
    End Function

    Private Async Function StopPreviewAsync() As Task
        Try
            If _mediaCapture IsNot Nothing Then
                If isPreviewing Then
                    isPreviewing = False

                    If _mediaCapture IsNot Nothing Then
                        Await _mediaCapture.StopPreviewAsync()
                        _mediaCapture.Dispose()
                    End If
                    _mediaCapture = Nothing

                    Await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, Sub()
                                                                                                 PreviewControl.Source = Nothing
                                                                                             End Sub)

                    If _periodicTask IsNot Nothing Then
                        _periodicTask.Cancel()
                    End If
                    _periodicTask = Nothing

                    _openHelper = Nothing

                    If _displayRequest IsNot Nothing Then
                        _displayRequest.RequestRelease()
                    End If
                    _displayRequest = Nothing
                End If
            End If

        Catch ex As Exception
            DebugMessage(ex.Message)
        End Try
    End Function

    Private Sub DebugMessage(ByVal Message As String, Optional ByVal Title As String = "ERROR")
        Debug.WriteLine(Title & ": " & Message)
    End Sub

    Private Async Sub _application_Suspending(sender As Object, e As SuspendingEventArgs) Handles _application.Suspending
        Dim deferral As SuspendingDeferral = Nothing
        Try
            If Frame.CurrentSourcePageType() = GetType(MainPage) Then
                deferral = e.SuspendingOperation.GetDeferral()
                Await StopPreviewAsync()
                deferral.Complete()
            End If
        Catch ex As Exception
            DebugMessage(ex.Message)
        Finally
            deferral = Nothing
        End Try
    End Sub

    Private Async Function CapturePhotoAsync() As Task(Of SoftwareBitmap)
        Dim _lowLagCapture As LowLagPhotoCapture = Nothing
        Dim _capturedPhoto As CapturedPhoto = Nothing
        Dim _softwareBitmap As SoftwareBitmap = Nothing
        Try
            _lowLagCapture = Await _mediaCapture.PrepareLowLagPhotoCaptureAsync(ImageEncodingProperties.CreateUncompressed(MediaPixelFormat.Bgra8))
            _capturedPhoto = Await _lowLagCapture.CaptureAsync()
            _softwareBitmap = _capturedPhoto.Frame.SoftwareBitmap

            Await _lowLagCapture.FinishAsync()

            Return _softwareBitmap

        Catch ex As Exception
            DebugMessage(ex.Message)
            Return Nothing
        Finally
            _lowLagCapture = Nothing
            _capturedPhoto = Nothing
            _softwareBitmap = Nothing
        End Try
    End Function

    Private Async Function CapturePhotoFileAsync() As Task(Of StorageFile)
        Dim _pictureFolder As StorageLibrary = Nothing
        Dim _photoFile As StorageFile = Nothing
        Dim Decoder As BitmapDecoder = Nothing
        Dim Encoder As BitmapEncoder = Nothing
        Try
            _pictureFolder = Await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Pictures)
            _photoFile = Await _pictureFolder.SaveFolder.CreateFileAsync("photo.jpg", CreationCollisionOption.ReplaceExisting)

            Using CaptureStream = New InMemoryRandomAccessStream
                Await _mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), CaptureStream)

                Using FileStream = Await _photoFile.OpenAsync(FileAccessMode.ReadWrite)
                    Decoder = Await BitmapDecoder.CreateAsync(CaptureStream)
                    Encoder = Await BitmapEncoder.CreateForTranscodingAsync(FileStream, Decoder)

                    Await Encoder.FlushAsync()

                End Using
            End Using

            Return _photoFile

        Catch ex As Exception
            DebugMessage(ex.Message)
            Return Nothing
        Finally
            _pictureFolder = Nothing
            _photoFile = Nothing
            Decoder = Nothing
            Encoder = Nothing
        End Try
    End Function

    Private Async Function ConvertSoftwareBitmapToSoftwareBitmapSourceAsync(ByVal sbitmap As SoftwareBitmap) As Task(Of SoftwareBitmapSource)
        Dim _source As SoftwareBitmapSource = Nothing
        Try

            If sbitmap.BitmapPixelFormat <> BitmapPixelFormat.Bgra8 Or sbitmap.BitmapAlphaMode = BitmapAlphaMode.Straight Then
                sbitmap = SoftwareBitmap.Convert(sbitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied)
            End If

            _source = New SoftwareBitmapSource
            Await _source.SetBitmapAsync(sbitmap)

            Return _source

        Catch ex As Exception
            DebugMessage(ex.Message)
            Return Nothing
        Finally
            _source = Nothing
        End Try
    End Function

    Private Async Function ConvertSoftwareBitmapToStorageFileAsync(ByVal sbitmap As SoftwareBitmap) As Task(Of StorageFile)
        Dim _pictureFolder As StorageLibrary = Nothing
        Dim _photoFile As StorageFile = Nothing
        Dim Encoder As BitmapEncoder = Nothing
        Try
            _pictureFolder = Await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Pictures)
            _photoFile = Await _pictureFolder.SaveFolder.CreateFileAsync("photoCont.jpg", CreationCollisionOption.ReplaceExisting)

            Using Stream As IRandomAccessStream = Await _photoFile.OpenAsync(FileAccessMode.ReadWrite)
                ' Create an encoder with the desired format
                Encoder = Await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, Stream)

                ' Set the software bitmap
                Encoder.SetSoftwareBitmap(sbitmap)

                ' Set additional encoding parameters, if needed
                Encoder.IsThumbnailGenerated = True

                Await Encoder.FlushAsync()

            End Using

            Return _photoFile

        Catch ex As Exception
            DebugMessage(ex.Message)
            Return Nothing
        Finally
            _pictureFolder = Nothing
            _photoFile = Nothing
            Encoder = Nothing
        End Try
    End Function

    Private Async Function GetColorsFromImage(ByVal photoFile As StorageFile) As Task(Of List(Of ImageColor))
        Dim Colors As List(Of ImageColor) = Nothing
        Dim Decoder As BitmapDecoder = Nothing
        Dim topColor As QuantizedColor = Nothing
        Try
            Colors = New List(Of ImageColor)

            Using Stream = Await photoFile.OpenStreamForReadAsync()
                Decoder = Await BitmapDecoder.CreateAsync(Stream.AsRandomAccessStream())

                ' Get Mixedup Top Color
                topColor = Await _colorThief.GetColor(Decoder)
                Colors.Add(New ImageColor(topColor.Color))

                ' Get Top 2 Colors
                For Each result In Await _colorThief.GetPalette(Decoder, 7, 10, True)
                    Colors.Add(New ImageColor(result.Color))
                Next

                Dim CarDetectionValue As Integer = topColor.Color.B
                If CarDetectionValue > 90 Then
                    txtCarDetectionText.Text = "something in the way..."
                Else
                    txtCarDetectionText.Text = "free space..."
                End If

                txtCarDetectionIndex.Text = CarDetectionValue.ToString

                Return Colors

            End Using

        Catch ex As Exception
            Return Colors
        Finally
            Colors = Nothing
            Decoder = Nothing
            topColor = Nothing
        End Try
    End Function

    Private Async Function GetPartOfImage(ByVal photoFile As StorageFile, ByVal width As Integer, ByVal height As Integer, ByVal x As Integer, ByVal y As Integer) As Task(Of SoftwareBitmap)
        Dim Decoder As BitmapDecoder = Nothing
        Dim Encoder As BitmapEncoder = Nothing
        Dim Bounds As BitmapBounds = Nothing
        Try
            Using Stream = Await photoFile.OpenStreamForReadAsync()
                Decoder = Await BitmapDecoder.CreateAsync(Stream.AsRandomAccessStream())

                ' create a New stream And encoder for the New image
                Using ras As InMemoryRandomAccessStream = New InMemoryRandomAccessStream()
                    Encoder = Await BitmapEncoder.CreateForTranscodingAsync(ras, Decoder)

                    ' convert the entire bitmap to a 100px by 100px bitmap
                    Encoder.BitmapTransform.ScaledHeight = Decoder.PixelHeight
                    Encoder.BitmapTransform.ScaledWidth = Decoder.PixelWidth

                    Bounds = New BitmapBounds()
                    Bounds.Height = height
                    Bounds.Width = width
                    Bounds.X = x
                    Bounds.Y = y
                    Encoder.BitmapTransform.Bounds = Bounds

                    Await Encoder.FlushAsync()

                    Dim bImg As BitmapImage = New BitmapImage()
                    bImg.SetSource(ras)
                    CropedControl.Source = bImg

                    ' Get the SoftwareBitmap representation of the file
                    Decoder = Await BitmapDecoder.CreateAsync(ras)
                    Return Await Decoder.GetSoftwareBitmapAsync()

                End Using

                Return Nothing

            End Using

        Catch ex As Exception
            Return Nothing
        Finally
            Decoder = Nothing
            Encoder = Nothing
            Bounds = Nothing
        End Try
    End Function

    Private Async Sub btnCamera_Click(sender As Object, e As RoutedEventArgs)
        Await CaptureSnapshot()
    End Sub

    Private Async Sub btnResume_Click(sender As Object, e As RoutedEventArgs)
        Await StartPreviewAsync()
    End Sub

    Private Async Function CaptureSnapshot() As Task
        Dim PhotoFile As StorageFile = Nothing
        Dim PrcessedSoftwareBitmap As SoftwareBitmap = Nothing
        Dim OriginalSoftwareBitmap As SoftwareBitmap = Nothing
        Dim InputBitmap As SoftwareBitmap = Nothing
        Try
            ' Create SoftwareBitmap
            SnapshotControl.Source = Await ConvertSoftwareBitmapToSoftwareBitmapSourceAsync(Await CapturePhotoAsync())
            DebugMessage("SoftwareBitmap created", "Picture")

            ' Create Photofile
            PhotoFile = Await CapturePhotoFileAsync()
            DebugMessage(PhotoFile?.Path, "Picture created")

            ' Crop Photofile
            For Each Item As RectDect In LayerControl.Items
                If Item.IsSelected = True Then
                    InputBitmap = Await GetPartOfImage(PhotoFile, Item.Width, Item.Height, Item.X, Item.Y)
                End If
            Next

            ' Create contours with MSOpenCVBridge
            'InputBitmap = Await CapturePhotoAsync()

            OriginalSoftwareBitmap = SoftwareBitmap.Convert(InputBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied)
            PrcessedSoftwareBitmap = New SoftwareBitmap(BitmapPixelFormat.Bgra8, OriginalSoftwareBitmap.PixelWidth, OriginalSoftwareBitmap.PixelHeight, BitmapAlphaMode.Premultiplied)
            _openHelper.Contours(OriginalSoftwareBitmap, PrcessedSoftwareBitmap)
            ProcessedControl.Source = Await ConvertSoftwareBitmapToSoftwareBitmapSourceAsync(PrcessedSoftwareBitmap)
            DebugMessage("MS OpenCV Bridge processed", "Picture processed")

            PhotoFile = Await ConvertSoftwareBitmapToStorageFileAsync(PrcessedSoftwareBitmap)
            DebugMessage(PhotoFile?.Path, "Picture created")

            gvColors.ItemsSource = Nothing
            gvColors.ItemsSource = Await GetColorsFromImage(PhotoFile)

        Catch ex As Exception
            DebugMessage(ex.Message)
        Finally
            PhotoFile = Nothing
            PrcessedSoftwareBitmap = Nothing
            OriginalSoftwareBitmap = Nothing
            InputBitmap = Nothing
        End Try
    End Function

    Private Sub LayerControl_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles LayerControl.SelectionChanged
        If DirectCast(sender, GridView).SelectedItems.Count > 0 Then
            For Each Item In DirectCast(sender, GridView).SelectedItems
                If DirectCast(Item, RectDect).IsSelected = False Then
                    DirectCast(Item, RectDect).IsSelected = True
                    DirectCast(Item, RectDect).BorderColor = New Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.LightGreen)
                Else
                    DirectCast(Item, RectDect).IsSelected = False
                    DirectCast(Item, RectDect).BorderColor = New Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Red)
                End If
                DirectCast(sender, GridView).SelectedIndex = -1
            Next
        End If
    End Sub

    'Private Sub SliderWidth_ValueChanged(sender As Object, e As RangeBaseValueChangedEventArgs) Handles SliderWidth.ValueChanged
    '    RectInWidth = SliderWidth.Value
    'End Sub

    'Private Sub SliderHeight_ValueChanged(sender As Object, e As RangeBaseValueChangedEventArgs) Handles SliderHeight.ValueChanged
    '    RectInHeight = SliderHeight.Value
    'End Sub
End Class
