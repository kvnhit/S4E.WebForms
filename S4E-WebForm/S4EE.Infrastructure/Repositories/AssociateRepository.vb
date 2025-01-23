Imports System.Data.SqlClient
Imports S4E.Domain

Public Class AssociateRepository
    Public Function AddAssociate(ByVal associado As Associate) As Integer
        Using connection As SqlConnection = DatabaseContext.GetConnection()
            connection.Open()

            Dim query As String = "INSERT INTO T_ASSOC (A_Name, A_Cpf, A_Birth) OUTPUT INSERTED.Id VALUES (@Name, @Cpf, @BirthDate)"
            Using command As New SqlCommand(query, connection)
                command.Parameters.AddWithValue("@Name", associado.Name)
                command.Parameters.AddWithValue("@Cpf", associado.Cpf)
                command.Parameters.AddWithValue("@BirthDate", associado.BirthDate)

                associado.Id = Convert.ToInt32(command.ExecuteScalar())
            End Using
        End Using

        If associado.Companies IsNot Nothing AndAlso associado.Companies.Count > 0 Then
            AddAssociateRelation(associado.Companies, associado.Id)
        End If

        Return associado.Id
    End Function
    Public Sub UpdateAssociate(ByVal associado As Associate)
        Using connection As SqlConnection = DatabaseContext.GetConnection()
            connection.Open()

            Dim query As New SqlCommand("UPDATE T_ASSOC SET A_Name = @Name, A_CPF = @Cpf, A_BIRTH = @BirthDate WHERE ID = @Id", connection)
            query.Parameters.AddWithValue("@Id", associado.Id)
            query.Parameters.AddWithValue("@Name", associado.Name)
            query.Parameters.AddWithValue("@Cpf", associado.Cpf)
            query.Parameters.AddWithValue("@BirthDate", associado.BirthDate)

            query.ExecuteNonQuery()

            MsgBox("Update realizado com sucesso", MsgBoxStyle.Information, "Message")

            connection.Close()
        End Using

        UpdateAssociateRelation(associado.Companies, associado.Id)
    End Sub
    Public Sub DeleteAssociate(ByVal associateId As Integer)
        Using connection As SqlConnection = DatabaseContext.GetConnection()
            connection.Open()

            Using deleteRelQuery As New SqlCommand("delete T_REL where A_ID = @Id", connection)
                deleteRelQuery.Parameters.AddWithValue("@Id", associateId)
                deleteRelQuery.ExecuteNonQuery()
            End Using
            Using deleteAssocQuery As New SqlCommand("delete T_ASSOC where ID = @Id", connection)
                deleteAssocQuery.Parameters.AddWithValue("@Id", associateId)
                deleteAssocQuery.ExecuteNonQuery()
            End Using
            connection.Close()
        End Using
    End Sub
    Private Sub UpdateAssociateRelation(companies As List(Of Company), associateId As Integer)
        Dim deleteQuery As String = "DELETE FROM T_REL WHERE A_ID = @a_id;"

        Using connection As SqlConnection = DatabaseContext.GetConnection()
            connection.Open()

            Using deleteCommand As New SqlCommand(deleteQuery, connection)
                deleteCommand.Parameters.AddWithValue("@a_id", associateId)
                deleteCommand.ExecuteNonQuery()
            End Using

            Dim insertQuery As String = "INSERT INTO T_REL (A_ID, C_ID) VALUES (@a_id, @c_id);"

            For Each company In companies
                Using insertCommand As New SqlCommand(insertQuery, connection)
                    insertCommand.Parameters.AddWithValue("@a_id", associateId)
                    insertCommand.Parameters.AddWithValue("@c_id", company.Id)
                    insertCommand.ExecuteNonQuery()
                End Using
            Next
            connection.Close()
        End Using
    End Sub
    Private Sub AddAssociateRelation(companies As List(Of Company), associateId As Integer)
        Dim query As String = "INSERT INTO T_REL (A_ID, C_ID) VALUES (@a_id, @c_id);"

        Using connection As SqlConnection = DatabaseContext.GetConnection()
            connection.Open()

            For Each company In companies
                Using command As New SqlCommand(query, connection)
                    command.Parameters.AddWithValue("@a_id", associateId)
                    command.Parameters.AddWithValue("@c_id", company.Id)
                    command.ExecuteNonQuery()
                End Using
            Next
        End Using
    End Sub
    Public Function GetAssociates() As DataTable
        Dim query As String = "SELECT A.Id, A.A_Name AS Nome, A.A_CPF AS CPF, A.A_BIRTH AS 'Data de Nascimento', " &
                              "Empresas = STUFF((SELECT ', ' + C.C_NAME " &
                              "FROM T_REL R INNER JOIN T_COMP C ON R.C_ID = C.ID " &
                              "WHERE R.A_ID = A.Id FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') " &
                              "FROM T_ASSOC A;"

        Dim dt As New DataTable()

        Using connection As SqlConnection = DatabaseContext.GetConnection()
            connection.Open()
            Using command As New SqlCommand(query, connection)
                ' Preenche o DataTable com os resultados da consulta
                Using reader As SqlDataReader = command.ExecuteReader()
                    dt.Load(reader)
                End Using
            End Using
        End Using

        Return dt
    End Function
    Public Function GetAssociatesById(id As Integer) As DataTable
        Dim queryString As String = "
            SELECT
                A.Id AS Id,
                A.A_NAME AS Nome,
                A.A_CPF AS CNPJ,
                A.A_BIRTH AS [Data de Nascimento],
                Empresas = STUFF((
                    SELECT ', ' + C.C_NAME
                    FROM T_REL R
                    INNER JOIN T_COMP C ON R.C_ID = C.ID
                    WHERE R.A_ID = A.Id
                    FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') 
            FROM
                T_ASSOC A
            WHERE
                A.Id = @id;"

        Dim dt As New DataTable()

        Using connection As SqlConnection = DatabaseContext.GetConnection()
            connection.Open()
            Using command As New SqlCommand(queryString, connection)
                command.Parameters.AddWithValue("@id", id)

                Using reader As SqlDataReader = command.ExecuteReader()
                    dt.Load(reader)
                End Using
            End Using
        End Using

        Return dt
    End Function
    Public Function GetAvailableAssociates(companyId As Integer, selectedAssociateIds As List(Of Integer)) As List(Of Associate)
        Dim associates As New List(Of Associate)
        Dim query As String = "
            SELECT ASSOC.ID, A_NAME 
            FROM T_ASSOC ASSOC 
            LEFT JOIN T_REL REL ON ASSOC.ID = REL.A_ID AND REL.C_ID = @CompanyId 
            WHERE REL.ID IS NULL"

        Using connection As SqlConnection = DatabaseContext.GetConnection()
            connection.Open()

            Using command As New SqlCommand(query, connection)
                command.Parameters.AddWithValue("@CompanyId", companyId)
                Using reader As SqlDataReader = command.ExecuteReader()

                    While reader.Read()
                        Dim id As Integer = Convert.ToInt32(reader("ID"))
                        Dim name As String = reader("A_NAME").ToString()

                        ' Verifica se o associado não foi selecionado anteriormente
                        If Not selectedAssociateIds.Contains(id) Then
                            associates.Add(New Associate() With {
                                .Id = id,
                                .Name = name
                            })
                        End If
                    End While
                End Using
            End Using
        End Using

        Return associates
    End Function
    Public Function GetSelectedAssociates(companyId As Integer) As List(Of Associate)
        Dim associates As New List(Of Associate)
        Dim query As String = "
            SELECT ASSOC.ID, A_NAME 
            FROM T_ASSOC ASSOC 
            INNER JOIN T_REL REL ON ASSOC.ID = REL.A_ID 
            WHERE REL.C_ID = @CompanyId"

        Using connection As SqlConnection = DatabaseContext.GetConnection()
            connection.Open()

            Using command As New SqlCommand(query, connection)
                command.Parameters.AddWithValue("@CompanyId", companyId)

                Using reader As SqlDataReader = command.ExecuteReader()
                    While reader.Read()
                        associates.Add(New Associate() With {
                            .Id = Convert.ToInt32(reader("ID")),
                            .Name = reader("A_NAME").ToString()
                        })
                    End While
                End Using
            End Using
        End Using

        Return associates
    End Function
    Public Function GetAssociatesByIds(idList As List(Of Integer)) As List(Of Associate)
        Dim associates As New List(Of Associate)()

        If idList Is Nothing OrElse idList.Count = 0 Then
            Return associates
        End If

        Dim query As String = $"SELECT * FROM T_ASSOC WHERE ID IN ({String.Join(",", idList)})"

        Using connection As SqlConnection = DatabaseContext.GetConnection()
            connection.Open()

            Using command As New SqlCommand(query, connection)
                Using reader As SqlDataReader = command.ExecuteReader()
                    While reader.Read()
                        Dim associate As New Associate()
                        associate.Id = Convert.ToInt32(reader("ID"))
                        associate.Name = Convert.ToString(reader("A_NAME"))
                        associate.Cpf = Convert.ToString(reader("A_CPF"))
                        associate.BirthDate = Convert.ToDateTime(reader("A_BIRTH"))
                        associates.Add(associate)
                    End While
                End Using
            End Using
        End Using

        Return associates
    End Function
End Class
