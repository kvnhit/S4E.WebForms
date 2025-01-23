Imports System.Configuration
Imports System.Data.SqlClient
Public Class DatabaseContext
    ''' <summary>
    ''' Retorna uma instância de SqlConnection usando a string de conexão configurada no arquivo de configuração.
    ''' </summary>
    ''' <returns>Uma nova instância de SqlConnection.</returns>
    ''' <exception cref="InvalidOperationException">Lançada se a string de conexão não for encontrada.</exception>
    Public Shared Function GetConnection() As SqlConnection
        Dim connectionString As String = ConfigurationManager.ConnectionStrings("S4E_Challenge")?.ConnectionString

        If String.IsNullOrEmpty(connectionString) Then
            Throw New InvalidOperationException("A string de conexão 'S4E_Challenge' não foi encontrada no arquivo de configuração.")
        End If

        Return New SqlConnection(connectionString)
    End Function
End Class

