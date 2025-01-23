Imports Microsoft.Data.SqlClient
Imports S4E.Domain

Public Class AssociateRepository
    Private _connection As SqlConnection = DatabaseContext.GetConnection()
    Public Function AddAssociate(ByVal associado As Associate) As Integer
        Using connection As SqlConnection = _connection
            connection.Open()

            Dim query As String = "INSERT INTO T_ASSOC (Name, Cpf, BirthDate) OUTPUT INSERTED.Id VALUES (@Name, @Cpf, @BirthDate)"
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
        Using connection As SqlConnection = _connection
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
        Using connection As SqlConnection = _connection
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

        Using connection As SqlConnection = _connection
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

        Using connection As SqlConnection = _connection
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

End Class
