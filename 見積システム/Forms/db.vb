Imports MySql.Data.MySqlClient
Imports System.Data

Public Module Db
    ' ★ここを自分の接続情報に置換
    Private ReadOnly _connStr As String =
        "Server=localhost;Port=3306;Database=sunstar;Uid=root;Pwd=your_password;Charset=utf8mb4;"

    Public Function GetDataTable(sql As String, params As List(Of MySqlParameter)) As DataTable
        Dim dt As New DataTable()
        Using conn As New MySqlConnection(_connStr)
            conn.Open()
            Using cmd As New MySqlCommand(sql, conn)
                If params IsNot Nothing Then cmd.Parameters.AddRange(params.ToArray())
                Using adp As New MySqlDataAdapter(cmd)
                    adp.Fill(dt)
                End Using
            End Using
        End Using
        Return dt
    End Function

    Public Function ExecScalar(Of T)(sql As String, params As List(Of MySqlParameter)) As T
        Using conn As New MySqlConnection(_connStr)
            conn.Open()
            Using cmd As New MySqlCommand(sql, conn)
                If params IsNot Nothing Then cmd.Parameters.AddRange(params.ToArray())
                Dim v = cmd.ExecuteScalar()
                If v Is Nothing OrElse v Is DBNull.Value Then
                    Return Nothing
                End If
                Return CType(Convert.ChangeType(v, GetType(T)), T)
            End Using
        End Using
    End Function
End Module