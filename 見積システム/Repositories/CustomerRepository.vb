Imports MySql.Data.MySqlClient

Public Class CustomerRepository
    Private ReadOnly _cs As String

    Public Sub New(connectionString As String)
        _cs = connectionString
    End Sub

    Public Function GetAllCustomers() As List(Of CustomerRow)
        Dim list As New List(Of CustomerRow)

        Dim sql As String =
            "SELECT customer_code, customer_name, phone_number, address, apartment_name, " &
            "birth_date, payment_method, purchase_start, purchase_reason " &
            "FROM customer_master ORDER BY customer_code"

        Using conn As New MySqlConnection(_cs)
            conn.Open()
            Using cmd As New MySqlCommand(sql, conn)
                Using rdr = cmd.ExecuteReader()
                    While rdr.Read()
                        Dim row As New CustomerRow With {
                            .CustomerCode = SafeStr(rdr("customer_code")),
                            .CustomerName = SafeStr(rdr("customer_name")),
                            .PhoneNumber = SafeStr(rdr("phone_number")),
                            .Address = SafeStr(rdr("address")),
                            .ApartmentName = SafeStr(rdr("apartment_name")),
                            .BirthDate = SafeNullableDate(rdr("birth_date")),
                            .PaymentMethod = SafeStr(rdr("payment_method")),
                            .PurchaseStart = SafeNullableDate(rdr("purchase_start")),
                            .PurchaseReason = SafeStr(rdr("purchase_reason"))
                        }
                        list.Add(row)
                    End While
                End Using
            End Using
        End Using

        Return list
    End Function

    Private Shared Function SafeStr(v As Object) As String
        If v Is Nothing OrElse v Is DBNull.Value Then Return ""
        Return v.ToString()
    End Function

    Private Shared Function SafeNullableDate(v As Object) As DateTime?
        If v Is Nothing OrElse v Is DBNull.Value Then Return Nothing
        Dim d As DateTime
        If DateTime.TryParse(v.ToString(), d) Then Return d
        Return Nothing
    End Function
End Class
