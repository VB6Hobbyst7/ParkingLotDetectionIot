Imports Windows.Media.Capture
Imports Windows.Media.Capture.Frames
Imports CSharpBridgeCollection
Imports OpenCVBridge
Imports Windows.Graphics.Imaging
Imports Windows.Media.MediaProperties
Imports ParkingLotCollection
Imports Windows.Storage
Imports Windows.Storage.Streams

Public NotInheritable Class MainPage
    Inherits Page

    Private _mediaCapture As MediaCapture = Nothing
    Private WithEvents _reader As MediaFrameReader = Nothing
    Private _previewRenderer As FrameRenderer = Nothing
    Private _outputRenderer As FrameRenderer = Nothing

    Private _frameCount As Integer = 0

    Private Const IMAGE_ROWS As Integer = 480
    Private Const IMAGE_COLS As Integer = 640

    Private _helper As OpenCVHelper

    Private _parkingLots As ParkingLot


    Enum OperationType
        Blur = 0
        HoughLines = 1
        Contours = 2
        Histogram = 3
        MotionDetector = 4
    End Enum

    Private currentOperation As OperationType = OperationType.Contours

    Public Sub New()

        InitializeComponent()

        _parkingLots = New ParkingLot

        _previewRenderer = New FrameRenderer(PreviewImage)
        _outputRenderer = New FrameRenderer(OutputImage)
        lvLots.ItemsSource = _parkingLots.Parts

        _helper = New OpenCVHelper()

    End Sub

    Private Async Function InitializeMediaCaptureAsync(sourceGroup As MediaFrameSourceGroup) As Task
        If _mediaCapture IsNot Nothing Then
            Return
        End If

        _mediaCapture = New MediaCapture()
        Dim settings = New MediaCaptureInitializationSettings() With
            {
                .SourceGroup = sourceGroup,
                .SharingMode = MediaCaptureSharingMode.ExclusiveControl,
                .StreamingCaptureMode = StreamingCaptureMode.Video,
                .MemoryPreference = MediaCaptureMemoryPreference.Cpu
            }
        Await _mediaCapture.InitializeAsync(settings)

    End Function

    Private Async Sub CleanupMediaCaptureAsync()
        If _mediaCapture IsNot Nothing Then
            Await _reader.StopAsync()
            _reader.Dispose()
            _mediaCapture = Nothing
        End If
    End Sub

    Private Sub _reader_FrameArrived(sender As MediaFrameReader, args As MediaFrameArrivedEventArgs) Handles _reader.FrameArrived
        Dim originalBitmap As SoftwareBitmap = Nothing
        Dim inputBitmap As SoftwareBitmap = Nothing
        Dim outputBitmap As SoftwareBitmap = Nothing
        Try
            Dim frame = sender.TryAcquireLatestFrame()
            If frame IsNot Nothing Then
                inputBitmap = frame.VideoMediaFrame?.SoftwareBitmap
                If inputBitmap IsNot Nothing Then
                    originalBitmap = SoftwareBitmap.Convert(inputBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied)
                    '   outputBitmap = New SoftwareBitmap(BitmapPixelFormat.Bgra8, originalBitmap.PixelWidth, originalBitmap.PixelHeight, BitmapAlphaMode.Premultiplied)

                    ' Operate on the image in the manner chosen by the user.
                    'If currentOperation = OperationType.Blur Then
                    '    _helper.Blur(originalBitmap, outputBitmap)
                    'ElseIf currentOperation = OperationType.HoughLines Then
                    '    _helper.HoughLines(originalBitmap, outputBitmap)
                    'ElseIf currentOperation = OperationType.Contours Then
                    '    _helper.Contours(originalBitmap, outputBitmap)
                    'ElseIf currentOperation = OperationType.Histogram Then
                    '    _helper.Histogram(originalBitmap, outputBitmap)
                    'ElseIf currentOperation = OperationType.MotionDetector Then
                    '    _helper.MotionDetector(originalBitmap, outputBitmap)
                    'End If

                    '' Display both the original bitmap And the processed bitmap.
                    _previewRenderer.RenderFrame(originalBitmap)
                    ' _outputRenderer.RenderFrame(outputBitmap)

                    'For Each part In _parkingLots.Parts
                    '   Dim photoFile As StorageFile = Await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync("photoCV.jpg", CreationCollisionOption.ReplaceExisting)
                    '  Await BitmapHelper.SaveSoftwareBitmapToFile(outputBitmap, photoFile)

                    '    part.Image = originalBitmap
                    '    part.LastUpdate = DateTime.Now.ToShortTimeString
                    'Next

                End If
            End If

        Catch ex As Exception

        Finally
            originalBitmap = Nothing
            inputBitmap = Nothing
            outputBitmap = Nothing
        End Try
    End Sub

    Private Async Sub ProcessFrame()
        Dim originalBitmap As SoftwareBitmap = Nothing
        Dim inputBitmap As SoftwareBitmap = Nothing
        Dim outputBitmap As SoftwareBitmap = Nothing
        Try
            Dim frame = Await _mediaCapture.GetPreviewFrameAsync()
            If frame IsNot Nothing Then
                inputBitmap = frame.SoftwareBitmap
                If inputBitmap IsNot Nothing Then
                    originalBitmap = SoftwareBitmap.Convert(inputBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied)
                    outputBitmap = New SoftwareBitmap(BitmapPixelFormat.Bgra8, originalBitmap.PixelWidth, originalBitmap.PixelHeight, BitmapAlphaMode.Premultiplied)

                    ' Operate on the image in the manner chosen by the user.
                    If currentOperation = OperationType.Blur Then
                        _helper.Blur(originalBitmap, outputBitmap)
                    ElseIf currentOperation = OperationType.HoughLines Then
                        _helper.HoughLines(originalBitmap, outputBitmap)
                    ElseIf currentOperation = OperationType.Contours Then
                        _helper.Contours(originalBitmap, outputBitmap)
                    ElseIf currentOperation = OperationType.Histogram Then
                        _helper.Histogram(originalBitmap, outputBitmap)
                    ElseIf currentOperation = OperationType.MotionDetector Then
                        _helper.MotionDetector(originalBitmap, outputBitmap)
                    End If

                    ' Display both the original bitmap And the processed bitmap.
                    _previewRenderer.RenderFrame(originalBitmap)
                    _outputRenderer.RenderFrame(outputBitmap)

                    'For Each part In _parkingLots.Parts
                    Dim photoFile As StorageFile = Await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync("photoCV.jpg", CreationCollisionOption.ReplaceExisting)
                    Await BitmapHelper.SaveSoftwareBitmapToFile(outputBitmap, photoFile)

                    '    part.Image = originalBitmap
                    '    part.LastUpdate = DateTime.Now.ToShortTimeString
                    'Next

                End If
            End If

        Catch ex As Exception

        Finally
            originalBitmap = Nothing
            inputBitmap = Nothing
            outputBitmap = Nothing
        End Try
    End Sub

    Private Async Sub StartMediaCaptureAsync()
        ' Find the sources 
        Dim allGroups = Await MediaFrameSourceGroup.FindAllAsync()
        Dim selectedSource = allGroups.FirstOrDefault()

        ' Initialize MediaCapture
        Try
            Await InitializeMediaCaptureAsync(selectedSource)
        Catch ex As Exception
            Debug.WriteLine("MediaCapture initialization error: " & ex.Message)
            CleanupMediaCaptureAsync()
            Return
        End Try

        ' Create the frame reader
        Dim frameSource As MediaFrameSource = _mediaCapture.FrameSources(selectedSource.SourceInfos.FirstOrDefault().Id)
        Dim Size As BitmapSize = New BitmapSize() With ' Choose a lower resolution to make the image processing more performant
            {
                .Height = IMAGE_ROWS,
                .Width = IMAGE_COLS
            }

        '   _reader = Await _mediaCapture.CreateFrameReaderAsync(frameSource, MediaEncodingSubtypes.Bgra8, Size)
        ''  Await _reader.StartAsync()

        ProcessFrame()

    End Sub

    Private Sub MainPage_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        StartMediaCaptureAsync()
    End Sub

    Private Async Sub btnCapture_Click(sender As Object, e As RoutedEventArgs)
        Dim file = Await ApplicationData.Current.LocalFolder.CreateFileAsync("photo.jpg", CreationCollisionOption.ReplaceExisting)

        Using captureStream = New InMemoryRandomAccessStream
            Await _mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg, captureStream)

            Using fileStream = Await file.OpenAsync(FileAccessMode.ReadWrite)
                Dim decoder = Await BitmapDecoder.CreateAsync(captureStream)
                Dim encoder = Await BitmapEncoder.CreateForTranscodingAsync(fileStream, decoder)

                'Await encoder.BitmapProperties.
                Await encoder.FlushAsync()
            End Using
        End Using
    End Sub
End Class
