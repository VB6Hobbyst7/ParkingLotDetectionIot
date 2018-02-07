Imports System.Data.SqlClient

Public Class Datenbase

    Private Const ConnString As String = "Server=tcp:superhans205.database.windows.net,1433;Initial Catalog=free-sql-db;Persist Security Info=False;User ID=superhans205;Password=DasDasDas3;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

    Public Shared Async Function SetRecordAsync(ByVal img As Byte(), Optional ByVal topLine As Boolean = True) As Task(Of Integer)
        Try
            Using sqlConn As New SqlConnection(ConnString)
                Await sqlConn.OpenAsync()

                Using sqlCmd As New SqlCommand("INSERT INTO ParkingLotRecords (LastUpdate, TopLine, Thumbnail) VALUES (@LastUpdate, @TopLine, @Thumbnail)", sqlConn)
                    sqlCmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now)
                    sqlCmd.Parameters.AddWithValue("@TopLine", topLine)
                    sqlCmd.Parameters.AddWithValue("@Thumbnail", img)

                    Return Await sqlCmd.ExecuteNonQueryAsync()

                End Using
            End Using

            Return 0

        Catch ex As Exception
            Debug.WriteLine($"ERROR: {ex.Message}")
            Return 0
        End Try
    End Function

    Public Shared Async Function GetAllRecords() As Task(Of List(Of Record))
        Dim mRecords As List(Of Record)
        Try
            mRecords = New List(Of Record)

            Using sqlConn As New SqlConnection(ConnString)
                Await sqlConn.OpenAsync()

                Using sqlCmd As New SqlCommand("SELECT * FROM ParkingLotRecords", sqlConn)
                    Using sqlReader As SqlDataReader = Await sqlCmd.ExecuteReaderAsync()
                        Do While sqlReader.Read
                            mRecords.Add(New Record With {.LastUpdate = sqlReader("LastUpdate"),
                                         .Thumbnail = sqlReader("Thumbnail"),
                                         .TopLine = sqlReader("TopLine")})
                        Loop
                    End Using
                End Using
            End Using

            Return mRecords

        Catch ex As Exception
            Return New List(Of Record)
        End Try
    End Function

End Class