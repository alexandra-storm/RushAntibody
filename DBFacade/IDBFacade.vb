Public Interface IDBFacade
    Function CanConnect() As Boolean
    Function ExecuteSQL(ByVal sql As String) As DataSet
    Function ExecuteScalar(ByVal sql As String) As Object
    Function ExecuteScalar(ByVal procName As String, ByVal parameters As System.Collections.Generic.Dictionary(Of String, Object)) As Object
    Function ExecuteScalar(ByVal isProc As Boolean, ByVal procName As String, ByVal parameters As System.Collections.Generic.Dictionary(Of String, Object)) As Object
    Function ExecuteNonQuery(ByVal sql As String) As Integer
    Function ExecuteNonQuery(ByVal procName As String, ByVal parameters As System.Collections.Generic.Dictionary(Of String, Object)) As Integer
    Function ExecuteProc(ByVal procName As String) As DataSet
    Function ExecuteProc(ByVal procName As String, ByVal parameters As Dictionary(Of String, Object)) As DataSet
    Function ExecuteTransaction(ByVal sql As String) As Object
    Function ExecuteTransactionWithBulkInsert(ByVal procedures As List(Of String), ByVal parameters As List(Of Dictionary(Of String, Object)), ByVal commandTypes As List(Of String), ByRef dtbulkcopy As DataTable) As Boolean
    Function ExecuteTransaction(ByVal procedures As String, ByVal parameters As Dictionary(Of String, Object), ByVal commandTypes As String, ByRef _mytransaction As System.Data.SqlClient.SqlTransaction) As Object
    Function ExecuteTransaction(ByVal procedures As List(Of String), ByVal parameters As List(Of Dictionary(Of String, Object)), ByVal commandTypes As List(Of String)) As Boolean
    Function ExecuteProcTransaction(ByVal procedures As String, ByVal parameters As Dictionary(Of String, Object)) As DataSet
    Function ExecuteReader(ByVal procName As String, ByVal parameters As Dictionary(Of String, Object)) As SqlClient.SqlDataReader
    Function ExecuteReader(ByVal procName As String) As SqlClient.SqlDataReader
    Function ExecuteReader(ByVal command As String, ByVal commandType As Integer) As SqlClient.SqlDataReader
End Interface
