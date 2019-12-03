Imports System.Data.Common
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Text
Imports System.Security.Cryptography
Imports System.Collections.Specialized
Imports Microsoft.Practices.EnterpriseLibrary.Data
Imports Microsoft.Practices.EnterpriseLibrary.ExceptionHandling
Imports Microsoft.Practices.EnterpriseLibrary.Common
Imports Microsoft.Practices.EnterpriseLibrary.Logging
Imports System.IO

Namespace EvolutionDBFacade
    Public Class DBFacade
        Implements IDBFacade, IDisposable

        Private _connString As String
        Private _conn As SqlConnection
        Private _command As SqlCommand
        Public Enum CmdType
            PROC = 1
            SQL = 2
        End Enum
        Public Function GetSqlConnectionString() As String
            Return _connString
        End Function
        Public Function GetConn() As SqlConnection
            Return _conn
        End Function
        ' tries to open connection, returns true iff no exception thrown
        ' assumes: _conn already contains connection info
        Public Function CanConnect() As Boolean Implements IDBFacade.CanConnect
            Try
                _conn.Open()
                _conn.Close()
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function
        Public Sub New()

            Dim configFileMap As New ExeConfigurationFileMap()
            If (Not File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\LIFECODES\External_matchit.config")) Then
                configFileMap.ExeConfigFilename = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\GenProbe\External_matchit.config"
            Else
                configFileMap.ExeConfigFilename = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\LIFECODES\External_matchit.config"
            End If
            Dim config As System.Configuration.Configuration = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None)

            'Dim config As System.Configuration.Configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
            _connString = DBFacade.Decrypt(config.AppSettings.Settings("appConnString").Value, "test")
            _conn = New SqlConnection(_connString)
            _command = New SqlCommand()
        End Sub
        Public Sub New(ByVal configFile As String)
            Dim configFileMap As New ExeConfigurationFileMap()
            If (Not File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\LIFECODES\External_matchit.config")) Then
                configFileMap.ExeConfigFilename = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\GenProbe\External_matchit.config"
            Else
                configFileMap.ExeConfigFilename = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\LIFECODES\External_matchit.config"
            End If
            Dim config As System.Configuration.Configuration = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None)

            'Dim config As System.Configuration.Configuration = ConfigurationManager.OpenExeConfiguration(String.Format("{0}\{1}", AppDomain.CurrentDomain.BaseDirectory, configFile))
            _connString = DBFacade.Decrypt(config.AppSettings.Settings("appConnString").Value, "test")
            _conn = New SqlConnection(_connString)
            _command = New SqlCommand()
        End Sub

        Public Sub New(ByVal configFile As String, ByVal isEncrypted As Boolean)
            Dim configFileMap As New ExeConfigurationFileMap()
            configFileMap.ExeConfigFilename = configFile
            'configFileMap.ExeConfigFilename = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\GenProbe\External_matchit.config"
            Dim config As System.Configuration.Configuration = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None)

            'Dim config As System.Configuration.Configuration = ConfigurationManager.OpenExeConfiguration(String.Format("{0}\{1}", AppDomain.CurrentDomain.BaseDirectory, configFile))
            If isEncrypted Then
                _connString = DBFacade.Decrypt(config.AppSettings.Settings("appConnString").Value, "test")
            Else
                _connString = config.AppSettings.Settings("appConnString").Value
            End If

            _conn = New SqlConnection(_connString)
            _command = New SqlCommand()
            _command.CommandTimeout = 0
        End Sub
        Public Sub New(ByVal configFile As String, ByVal isEncrypted As Boolean, ByVal ConnStringValue As String)
            Dim configFileMap As New ExeConfigurationFileMap()
            configFileMap.ExeConfigFilename = configFile
            'configFileMap.ExeConfigFilename = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\GenProbe\External_matchit.config"
            Dim config As System.Configuration.Configuration = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None)

            'Dim config As System.Configuration.Configuration = ConfigurationManager.OpenExeConfiguration(String.Format("{0}\{1}", AppDomain.CurrentDomain.BaseDirectory, configFile))
            If isEncrypted Then
                _connString = DBFacade.Decrypt(config.AppSettings.Settings(ConnStringValue).Value, "test")
            Else
                _connString = config.AppSettings.Settings(ConnStringValue).Value
            End If

            _conn = New SqlConnection(_connString)
            _command = New SqlCommand()
            _command.CommandTimeout = 0
        End Sub

        Public Sub New(ByVal Connectionstring As String, ByVal newstr As String)
            _connString = Connectionstring
            _conn = New SqlConnection(_connString)
            _command = New SqlCommand()
            _command.CommandTimeout = 0
        End Sub

        Public Function ExecuteProc(ByVal procName As String) As System.Data.DataSet Implements IDBFacade.ExecuteProc
            _command.CommandType = CommandType.StoredProcedure
            _command.CommandText = procName
            _command.Connection = _conn
            Return CType(Exec(0), DataSet)
        End Function

        Public Function ExecuteProc(ByVal procName As String, ByVal parameters As System.Collections.Generic.Dictionary(Of String, Object)) As System.Data.DataSet Implements IDBFacade.ExecuteProc
            _command.CommandType = CommandType.StoredProcedure
            _command.CommandText = procName
            _command.Connection = _conn
            _command.CommandTimeout = 600
            AddParameters(parameters)
            Return CType(Exec(0), DataSet)
        End Function

        Public Function ExecuteScalar(ByVal sql As String) As Object Implements IDBFacade.ExecuteScalar
            _command.CommandType = CommandType.Text
            _command.CommandText = sql
            _command.Connection = _conn
            Return Exec(1)
        End Function
        Public Function ExecuteScalar(ByVal isProc As Boolean, ByVal sql As String, ByVal parameters As System.Collections.Generic.Dictionary(Of String, Object)) As Object Implements IDBFacade.ExecuteScalar
            If (isProc) Then
                _command.CommandType = CommandType.StoredProcedure
            Else
                _command.CommandType = CommandType.Text
            End If

            _command.CommandText = sql
            _command.Connection = _conn
            AddParameters(parameters)
            Return Exec(1)
        End Function
        Public Function ExecuteScalar(ByVal procName As String, ByVal parameters As System.Collections.Generic.Dictionary(Of String, Object)) As Object Implements IDBFacade.ExecuteScalar
            _command.CommandType = CommandType.StoredProcedure
            _command.CommandText = procName
            _command.Connection = _conn
            AddParameters(parameters)
            Return Exec(1)
        End Function

        Public Function ExecuteNonQuery(ByVal sql As String) As Integer Implements IDBFacade.ExecuteNonQuery
            _command.CommandType = CommandType.Text
            _command.CommandText = sql
            _command.Connection = _conn
            Return CInt(Exec(2))
        End Function

        Public Function ExecuteNonQuery(ByVal procName As String, ByVal parameters As System.Collections.Generic.Dictionary(Of String, Object)) As Integer Implements IDBFacade.ExecuteNonQuery
            _command.CommandType = CommandType.StoredProcedure
            _command.CommandTimeout = 120
            _command.CommandText = procName
            _command.Connection = _conn
            AddParameters(parameters)
            Return CInt(Exec(2))
        End Function

        Public Function ExecuteSQL(ByVal sql As String) As System.Data.DataSet Implements IDBFacade.ExecuteSQL
            _command.CommandType = CommandType.Text
            _command.CommandText = sql
            _command.Connection = _conn
            Return CType(Exec(0), DataSet)
        End Function

        Public Function ExecuteTransaction(ByVal sql As String) As Object Implements IDBFacade.ExecuteTransaction
            _command.CommandType = CommandType.StoredProcedure
            _command.CommandText = "RunTransactionScript"
            _command.CommandTimeout = 0
            _command.Connection = _conn
            _command.Parameters.AddWithValue("@spText", sql)

            Return Exec(2)
        End Function
        Public Function ExecuteReader(ByVal procName As String) As System.Data.SqlClient.SqlDataReader Implements IDBFacade.ExecuteReader
            _command.CommandType = CommandType.StoredProcedure
            _command.CommandText = procName
            _command.Connection = _conn
            Return CType(Exec(3), SqlClient.SqlDataReader)
        End Function

        Public Function ExecuteReader(ByVal command As String, ByVal cmdType As Integer) As System.Data.SqlClient.SqlDataReader Implements IDBFacade.ExecuteReader
            If (cmdType = DBFacade.CmdType.PROC) Then
                _command.CommandType = CommandType.StoredProcedure
                _command.CommandText = command
            ElseIf (cmdType = DBFacade.CmdType.SQL) Then
                _command.CommandType = CommandType.Text
                _command.CommandText = command
            End If
            _command.Connection = _conn
            Return CType(Exec(3), SqlClient.SqlDataReader)
        End Function

        Public Function ExecuteReader(ByVal procName As String, ByVal parameters As System.Collections.Generic.Dictionary(Of String, Object)) As System.Data.SqlClient.SqlDataReader Implements IDBFacade.ExecuteReader
            _command.CommandType = CommandType.StoredProcedure
            _command.CommandText = procName
            _command.Connection = _conn
            AddParameters(parameters)
            Return CType(Exec(3), SqlClient.SqlDataReader)
        End Function
        ''' <summary>
        ''' Execute the procedures in the list as a transaction.
        ''' </summary>
        ''' <param name="procedures"></param>
        ''' <param name="parameters"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ExecuteTransaction(ByVal procedures As System.Collections.Generic.List(Of String), ByVal parameters As System.Collections.Generic.List(Of System.Collections.Generic.Dictionary(Of String, Object)), ByVal commandTypes As System.Collections.Generic.List(Of String)) As Boolean Implements IDBFacade.ExecuteTransaction
            Dim result As Boolean = True

            Dim _mytransaction As SqlTransaction
            _conn.Open()
            _mytransaction = _conn.BeginTransaction(IsolationLevel.Serializable)

            _command.Connection = _conn
            _command.Transaction = _mytransaction
            Dim i As Integer
            i = 0
            Try
                For Each item As String In procedures
                    If (commandTypes(i).ToLower() = "proc") Then
                        _command.CommandType = CommandType.StoredProcedure
                    ElseIf (commandTypes(i).ToLower() = "sql") Then
                        _command.CommandType = CommandType.Text
                    End If

                    _command.CommandText = item
                    If Not (parameters(i) Is Nothing) Then
                        AddParameters(parameters(i))
                    End If
                    _command.ExecuteNonQuery()
                    ClearParameters()
                    i = i + 1
                Next
                _mytransaction.Commit()
            Catch sqlEx As SqlException
                _mytransaction.Rollback()
                Dim rethrow As Boolean = ExceptionPolicy.HandleException(sqlEx, "Log Only Policy")

                If (rethrow) Then
                    Throw
                End If

                result = False
            Catch ex As Exception
                _mytransaction.Rollback()
                Dim rethrow As Boolean = ExceptionPolicy.HandleException(ex, "Log Only Policy")

                If (rethrow) Then
                    Throw
                End If

                result = False
            Finally
                If (_conn.State = ConnectionState.Open) Then
                    _conn.Close()
                End If
            End Try
            Return result
        End Function
        Public Function ExecuteTransaction(ByVal procedures As String, ByVal parameters As System.Collections.Generic.Dictionary(Of String, Object), ByVal commandTypes As String, ByRef _mytransaction As SqlTransaction) As Object Implements IDBFacade.ExecuteTransaction
            Dim result As Object = Nothing

            'Dim _mytransaction As SqlTransaction
            If _conn.State <> ConnectionState.Open Then
                _conn.Open()
            End If
            If _mytransaction Is Nothing Then
                _mytransaction = _conn.BeginTransaction(IsolationLevel.Serializable)
            End If

            _command.Connection = _conn
            _command.Transaction = _mytransaction

            Try
                'For Each item As String In procedures
                If (commandTypes.ToLower() = "proc") Then
                    _command.CommandType = CommandType.StoredProcedure
                ElseIf (commandTypes.ToLower() = "sql") Then
                    _command.CommandType = CommandType.Text
                End If

                _command.CommandText = procedures
                If Not (parameters Is Nothing) Then
                    AddParameters(parameters)
                End If
                result = _command.ExecuteScalar()
                ClearParameters()
                'Next
                '_mytransaction.Commit() 'manage outside
            Catch sqlEx As SqlException
                _mytransaction.Rollback()
                Dim rethrow As Boolean = ExceptionPolicy.HandleException(sqlEx, "Log Only Policy")

                If (rethrow) Then
                    Throw
                End If

                result = 0
            Catch ex As Exception
                _mytransaction.Rollback()
                Dim rethrow As Boolean = ExceptionPolicy.HandleException(ex, "Log Only Policy")

                If (rethrow) Then
                    Throw
                End If

                result = 0
            Finally
                'If (_conn.State = ConnectionState.Open) Then
                '    _conn.Close()
                'End If
            End Try
            If result = 0 Then
                Debug.Print("STOP")
            End If
            Return result
        End Function

        ''' <summary>
        ''' Execute the procedures in the list as a transaction.
        ''' </summary>
        ''' <param name="procedures">Set to TableName for bulkcopy</param>
        ''' <param name="parameters">Input columnname,index for bulkcopy. For bulkcopy, params cannot be null</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ExecuteTransactionWithBulkInsert(ByVal procedures As System.Collections.Generic.List(Of String),
                                            ByVal parameters As System.Collections.Generic.List(Of System.Collections.Generic.Dictionary(Of String, Object)),
                                            ByVal commandTypes As System.Collections.Generic.List(Of String),
                                            ByRef dtBulkCopy As DataTable) As Boolean Implements IDBFacade.ExecuteTransactionWithBulkInsert
            Dim result As Boolean = True
            Dim isBulkCopy As Boolean = False

            Dim _mytransaction As SqlTransaction
            _conn.Open()
            _mytransaction = _conn.BeginTransaction(IsolationLevel.Serializable)

            _command.Connection = _conn
            _command.Transaction = _mytransaction
            Dim i As Integer
            i = 0
            Try
                For Each item As String In procedures
                    isBulkCopy = False
                    If (commandTypes(i).ToLower() = "proc") Then
                        _command.CommandType = CommandType.StoredProcedure
                    ElseIf (commandTypes(i).ToLower() = "sql") Then
                        _command.CommandType = CommandType.Text
                    ElseIf (commandTypes(i).ToLower() = "bulkcopy") Then
                        isBulkCopy = True
                    End If

                    If isBulkCopy Then
                        Dim bulkcopy As SqlBulkCopy = New SqlBulkCopy(_conn, SqlBulkCopyOptions.Default, _mytransaction)
                        bulkcopy.BulkCopyTimeout = 30
                        bulkcopy.DestinationTableName = item
                        AddColumnMappingsForBulkCopy(bulkcopy, parameters(i))
                        bulkcopy.WriteToServer(dtBulkCopy)
                        bulkcopy.Close()
                        bulkcopy = Nothing
                        isBulkCopy = False
                    Else
                        _command.CommandText = item
                        If Not (parameters(i) Is Nothing) Then
                            AddParameters(parameters(i))
                        End If
                        _command.ExecuteNonQuery()
                        ClearParameters()
                    End If
                    i = i + 1
                Next
                _mytransaction.Commit()
            Catch sqlEx As SqlException
                _mytransaction.Rollback()
                Dim rethrow As Boolean = ExceptionPolicy.HandleException(sqlEx, "Log Only Policy")

                If (rethrow) Then
                    Throw
                End If

                result = False
            Finally
                If (_conn.State = ConnectionState.Open) Then
                    _conn.Close()
                End If
            End Try
            Return result
        End Function
        Private Sub AddColumnMappingsForBulkCopy(ByRef bulkcopy As SqlBulkCopy, parameters As Dictionary(Of String, Object))
            For Each param As String In parameters.Keys
                Try
                    If parameters(param) Is Nothing Then
                        Throw New Exception("No column mappings were specified for the sql bulk copy")
                    Else
                        bulkcopy.ColumnMappings.Add(parameters(param), param)
                    End If
                Catch ex As Exception
                    Throw ex
                End Try
            Next
        End Sub
        Public Function ExecuteProcTransaction(ByVal procedure As String, ByVal parameters As System.Collections.Generic.Dictionary(Of String, Object)) As DataSet Implements IDBFacade.ExecuteProcTransaction
            Dim dsresult As DataSet = New DataSet()
            Dim _mytransaction As SqlTransaction
            _conn.Open()
            _mytransaction = _conn.BeginTransaction(IsolationLevel.Serializable)

            _command.Connection = _conn
            _command.Transaction = _mytransaction
            _command.CommandType = CommandType.StoredProcedure
            _command.CommandText = procedure
            Try
                If Not (parameters Is Nothing) Then
                    AddParameters(parameters)
                End If
                'dsresult = CType(Exec(0), DataSet)
                Dim dsCmd As New SqlDataAdapter(_command)
                dsCmd.Fill(dsresult, "EvolutionTB")
                _mytransaction.Commit()
            Catch sqlEx As SqlException
                _mytransaction.Rollback()
                Dim rethrow As Boolean = ExceptionPolicy.HandleException(sqlEx, "Log Only Policy")

                If (rethrow) Then
                    Throw
                End If

            Catch ex As Exception
                _mytransaction.Rollback()
                Dim rethrow As Boolean = ExceptionPolicy.HandleException(ex, "Log Only Policy")

                If (rethrow) Then
                    Throw
                End If

            Finally
                If (_conn.State = ConnectionState.Open) Then
                    _conn.Close()
                End If
            End Try
            Return dsresult
        End Function

        Private Function Exec(ByVal execType As Integer) As Object
            Dim result As Object = Nothing
            Try
                _conn.Open()
                Select Case execType
                    Case 0
                        Dim _ds As New DataSet
                        Dim dsCmd As New SqlDataAdapter(_command)
                        dsCmd.Fill(_ds, "EvolutionTB")
                        result = _ds
                    Case 1
                        result = _command.ExecuteScalar()
                    Case 2
                        result = _command.ExecuteNonQuery()
                    Case 3
                        result = _command.ExecuteReader(CommandBehavior.CloseConnection)
                End Select
            Catch sqlEx As SqlException
                'error in sql call throws sqlexception and bubbles up
                'custom error
                Dim rethrow As Boolean = ExceptionPolicy.HandleException(sqlEx, "Log Only Policy")

                If (rethrow) Then
                    Throw
                End If

                result = Nothing
            Catch ex As Exception
                'general non-sqlerror throws exception and bubbles up
                'custom error
                'error in sql call throws sqlexception and bubbles up
                'custom error
                '  Dim rethrow As Boolean = ExceptionPolicy.HandleException(ex, "Log Only Policy")

                'If (rethrow) Then
                '    Throw
                'End If

                result = Nothing
            Finally
                If (execType <> 3) Then
                    If (_conn.State = ConnectionState.Open) Then
                        _conn.Close()
                    End If
                End If
                ClearParameters()
            End Try

            Return result
        End Function
        Private Function Exec(ByVal db As Database, ByVal dbCommand As DbCommand, ByVal execType As Integer) As Object
            Dim result As Object = Nothing
            Try
                _conn.Open()
                Select Case execType
                    Case 0
                        Dim _ds As New DataSet
                        Dim dsCmd As New SqlDataAdapter(_command)
                        dsCmd.Fill(_ds, "EvolutionTB")
                        result = _ds
                    Case 1
                        result = db.ExecuteScalar(dbCommand)
                    Case 2
                        result = _command.ExecuteNonQuery()
                        'result = db.ExecuteNonQuery(dbCommand)
                End Select
            Catch sqlEx As SqlException
                'error in sql call throws sqlexception and bubbles up
                'custom error
                Dim rethrow As Boolean = ExceptionPolicy.HandleException(sqlEx, "Log Only Policy")

                If (rethrow) Then
                    Throw
                End If

                result = Nothing
            Catch ex As Exception
                'general non-sqlerror throws exception and bubbles up
                'custom error
                'error in sql call throws sqlexception and bubbles up
                'custom error
                Dim rethrow As Boolean = ExceptionPolicy.HandleException(ex, "Log Only Policy")

                If (rethrow) Then
                    Throw
                End If

                result = Nothing
            Finally
                If (_conn.State = ConnectionState.Open) Then
                    _conn.Close()
                End If
            End Try

            Return result
        End Function
        'Public Shared Sub ExecuteBatch()
        '    If Commands.Count <= 0 Then
        '        Throw New Exception("No batch commands found")
        '    End If

        '    SetConnectionString()

        '    ' Use Transaction Here 
        '    Dim Trans As SqlTransaction

        '    Using Conn As New SqlConnection(ConnString)
        '        Conn.Open()
        '        Trans = Conn.BeginTransaction(IsolationLevel.Serializable)
        '        Try
        '            For Each oDACCommand As DAC In Commands
        '                Dim ParameterPairs As List(Of DAC.Command.Parameters.DacParameter) = oDACCommand.Params.MyParameters
        '                Dim arparams As SqlParameter() = New SqlParameter(ParameterPairs.Count - 1) {}

        '                Dim Count As Integer = 0
        '                For Each pObject As DAC.Command.Parameters.DacParameter In ParameterPairs
        '                    arparams(Count) = New SqlParameter(pObject.Name, pObject.Type)
        '                    arparams(Count).Value = pObject.Value

        '                    'Increment counter 
        '                    Count += 1
        '                Next

        '                ' Execute stored procedure 

        '                SqlHelper.ExecuteNonQuery(Trans, CommandType.StoredProcedure, oDACCommand.StoredProcedure, arparams)
        '            Next

        '            ' Commit Here 
        '            Trans.Rollback()
        '        Catch Ex As Exception
        '            Trans.Rollback()
        '            Throw (Ex)
        '        Finally
        '            If Conn.State = ConnectionState.Open Then
        '                Conn.Close()
        '                Conn.Dispose()
        '            End If
        '            Parameters.MyParameters.Clear()
        '        End Try
        '    End Using
        'End Sub

        Private Sub AddParameters(ByVal parameters As Dictionary(Of String, Object))
            For Each param As String In parameters.Keys
                Try
                    If parameters(param) Is Nothing Then
                        _command.Parameters.AddWithValue("@" + param, DBNull.Value)
                    Else
                        _command.Parameters.AddWithValue("@" + param, parameters(param))
                    End If
                Catch ex As Exception
                    _command.Parameters.AddWithValue("@" + param, parameters(param))
                End Try
            Next
        End Sub
        Private Sub ClearParameters()
            _command.Parameters.Clear()
        End Sub
        Private Function AddParameters(ByVal parameters As Dictionary(Of String, Object), ByVal db As Database, ByVal dbCommand As DbCommand) As Database
            For Each param As String In parameters.Keys
                Try
                    db.AddInParameter(dbCommand, "@" + param, GetDBType(parameters(param).GetType()), parameters(param))
                Catch ex As Exception
                    db.AddInParameter(dbCommand, "@" + param, DbType.Object, parameters(param))
                End Try
            Next
            Return db
        End Function

        Private Function GetDBType(ByVal t As Type) As DbType
            Dim dbt As DbType
            Try
                dbt = DirectCast([Enum].Parse(GetType(DbType), t.Name), DbType)
            Catch
                dbt = DbType.[Object]
            End Try
            Return dbt
        End Function
        Public Shared Function Encrypt(ByVal strText As String, ByVal strEncrKey As String) As String
            Dim IV() As Byte = {&H12, &H34, &H56, &H78, &H90, &HAB, &HCD, &HEF}
            strEncrKey = "genProbe$%Lifecodes@#"
            Try
                Dim bykey() As Byte = System.Text.Encoding.UTF8.GetBytes(strEncrKey.Substring(0, 8))
                Dim InputByteArray() As Byte = System.Text.Encoding.UTF8.GetBytes(strText)
                Dim des As New DESCryptoServiceProvider
                Dim ms As New MemoryStream
                Dim cs As New CryptoStream(ms, des.CreateEncryptor(bykey, IV), CryptoStreamMode.Write)
                cs.Write(InputByteArray, 0, InputByteArray.Length)
                cs.FlushFinalBlock()
                Return Convert.ToBase64String(ms.ToArray())
            Catch ex As Exception
                Return ex.Message
            End Try
        End Function
        Public Shared Function Decrypt(ByVal strText As String, ByVal sDecrKey As String) As String
            Dim IV() As Byte = {&H12, &H34, &H56, &H78, &H90, &HAB, &HCD, &HEF}
            Dim inputByteArray(strText.Length) As Byte
            sDecrKey = "genProbe$%Lifecodes@#"
            Try
                Dim byKey() As Byte = System.Text.Encoding.UTF8.GetBytes(sDecrKey.Substring(0, 8))
                Dim des As New DESCryptoServiceProvider
                inputByteArray = Convert.FromBase64String(strText)
                Dim ms As New MemoryStream
                Dim cs As New CryptoStream(ms, des.CreateDecryptor(byKey, IV), CryptoStreamMode.Write)
                cs.Write(inputByteArray, 0, inputByteArray.Length)
                cs.FlushFinalBlock()
                Dim encoding As System.Text.Encoding = System.Text.Encoding.UTF8
                Return encoding.GetString(ms.ToArray())
            Catch ex As Exception
                Return ex.Message
            End Try
        End Function



        Private disposedValue As Boolean = False        ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: free other state (managed objects).
                    _conn.Dispose()
                    _command.Dispose()
                End If

                ' TODO: free your own state (unmanaged objects).
                ' TODO: set large fields to null.
                GC.Collect()
            End If
            Me.disposedValue = True
        End Sub

#Region " IDisposable Support "
        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class

End Namespace