Imports System.Data.SqlClient
Imports S4E.Domain

Public Class CompanyRepository
    Public Function AddCompany(ByVal company As Company) As Integer
        Using connection As SqlConnection = DatabaseContext.GetConnection()
            connection.Open()

            Dim query As String = "INSERT INTO T_COMP (C_NAME, C_CNPJ) OUTPUT INSERTED.ID VALUES (@Name, @Cnpj)"
            Using command As New SqlCommand(query, connection)
                command.Parameters.AddWithValue("@Name", company.Name)
                command.Parameters.AddWithValue("@Cnpj", company.Cnpj)

                company.Id = Convert.ToInt32(command.ExecuteScalar())
            End Using
        End Using
        AddCompanyRelation(company.Id, company.Associates)

        Return company.Id
    End Function
    Public Sub UpdateCompany(ByVal company As Company)
        Using connection As SqlConnection = DatabaseContext.GetConnection()
            connection.Open()

            Dim query As New SqlCommand("UPDATE T_COMP SET C_Name = @Name, C_CNPJ = @Cnpj WHERE ID = @Id", connection)
            query.Parameters.AddWithValue("@Id", company.Id)
            query.Parameters.AddWithValue("@Name", company.Name)
            query.Parameters.AddWithValue("@Cnpj", company.Cnpj)

            query.ExecuteNonQuery()

            MsgBox("Update realizado com sucesso", MsgBoxStyle.Information, "Message")

            connection.Close()
        End Using

        UpdateCompanyRelation(company.Id, company.Associates)
    End Sub
    Public Sub DeleteCompany(ByVal companyId As Integer)
        Using connection As SqlConnection = DatabaseContext.GetConnection()
            connection.Open()

            Using deleteRelQuery As New SqlCommand("delete T_REL where C_ID = @Id", connection)
                deleteRelQuery.Parameters.AddWithValue("@Id", companyId)
                deleteRelQuery.ExecuteNonQuery()
            End Using
            Using deleteAssocQuery As New SqlCommand("delete T_COMP where ID = @Id", connection)
                deleteAssocQuery.Parameters.AddWithValue("@Id", companyId)
                deleteAssocQuery.ExecuteNonQuery()
            End Using
            connection.Close()
        End Using
    End Sub
    Private Sub UpdateCompanyRelation(companyId As Integer, associates As List(Of Associate))
        Dim deleteQuery As String = "DELETE FROM T_REL WHERE C_ID = @c_id;"

        Using connection As SqlConnection = DatabaseContext.GetConnection()
            connection.Open()

            Using deleteCommand As New SqlCommand(deleteQuery, connection)
                deleteCommand.Parameters.AddWithValue("@c_id", companyId)
                deleteCommand.ExecuteNonQuery()
            End Using

            Dim insertQuery As String = "INSERT INTO T_REL (A_ID, C_ID) VALUES (@a_id, @c_id);"

            For Each associate In associates
                Using insertCommand As New SqlCommand(insertQuery, connection)
                    insertCommand.Parameters.AddWithValue("@a_id", associate.Id)
                    insertCommand.Parameters.AddWithValue("@c_id", companyId)
                    insertCommand.ExecuteNonQuery()
                End Using
            Next
            connection.Close()
        End Using
    End Sub
    Private Sub AddCompanyRelation(companyId As Integer, associates As List(Of Associate))
        Dim query As String = "INSERT INTO T_REL (A_ID, C_ID) VALUES (@a_id, @c_id);"

        Using connection As SqlConnection = DatabaseContext.GetConnection()
            connection.Open()

            For Each associate In associates
                Using command As New SqlCommand(query, connection)
                    command.Parameters.AddWithValue("@a_id", associate.Id)
                    command.Parameters.AddWithValue("@c_id", companyId)
                    command.ExecuteNonQuery()
                End Using
            Next
        End Using
    End Sub
    Public Function GetAvailableCompanies(assocId As Integer) As List(Of Company)
        Dim query As String = "SELECT COMP.ID, COMP.C_NAME FROM T_COMP COMP " &
                              "LEFT JOIN T_REL REL ON COMP.ID = REL.C_ID AND REL.A_ID = @AssocId " &
                              "WHERE REL.ID IS NULL"

        Dim companies As New List(Of Company)

        Using connection As SqlConnection = DatabaseContext.GetConnection()
            connection.Open()
            Using command As New SqlCommand(query, connection)
                command.Parameters.AddWithValue("@AssocId", assocId)
                Dim reader As SqlDataReader = command.ExecuteReader()

                While reader.Read()
                    Dim company As New Company() With {
                        .Id = Convert.ToInt32(reader("ID")),
                        .Name = reader("C_NAME").ToString()
                    }
                    companies.Add(company)
                End While

                reader.Close()
            End Using
        End Using

        Return companies
    End Function
    Public Function GetSelectedCompanies(assocId As Integer) As List(Of Company)
        Dim query As String = "SELECT COMP.ID, COMP.C_NAME FROM T_COMP COMP " &
                              "INNER JOIN T_REL REL ON COMP.ID = REL.C_ID " &
                              "WHERE REL.A_ID = @AssocId"

        Dim companies As New List(Of Company)

        Using connection As SqlConnection = DatabaseContext.GetConnection()
            connection.Open()
            Using command As New SqlCommand(query, connection)
                command.Parameters.AddWithValue("@AssocId", assocId)
                Dim reader As SqlDataReader = command.ExecuteReader()

                While reader.Read()
                    Dim company As New Company() With {
                        .Id = Convert.ToInt32(reader("ID")),
                        .Name = reader("C_NAME").ToString()
                    }
                    companies.Add(company)
                End While

                reader.Close()
            End Using
        End Using

        Return companies
    End Function
    Public Function GetCompaniesByIds(idList As List(Of Integer)) As List(Of Company)
        Dim query As String = "SELECT * FROM T_COMP WHERE ID IN (" & String.Join(",", idList) & ")"

        Dim companies As New List(Of Company)

        Using connection As SqlConnection = DatabaseContext.GetConnection()
            connection.Open()
            Using command As New SqlCommand(query, connection)
                Dim reader As SqlDataReader = command.ExecuteReader()
                While reader.Read()
                    Dim company As New Company() With {
                        .Id = Convert.ToInt32(reader("ID")),
                        .Name = Convert.ToString(reader("C_NAME")),
                        .Cnpj = Convert.ToString(reader("C_CNPJ"))
                    }
                    companies.Add(company)
                End While

                reader.Close()
            End Using
        End Using

        Return companies
    End Function
    Public Function GetCompanies() As DataTable
        Dim query As String = "
        SELECT
            C.ID AS Id,
            C.C_NAME AS Nome,
            C.C_CNPJ AS CNPJ,
            Associados = STUFF((
                SELECT ', ' + A.A_NAME
                FROM T_REL R
                INNER JOIN T_ASSOC A ON R.A_ID = A.ID
                WHERE R.C_ID = C.ID
                FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '')
        FROM
            T_COMP C;"

        Dim dt As New DataTable()

        Using connection As SqlConnection = DatabaseContext.GetConnection()
            connection.Open()
            Using command As New SqlCommand(query, connection)
                Using adapter As New SqlDataAdapter(command)
                    adapter.Fill(dt)
                End Using
            End Using
        End Using

        Return dt
    End Function
    Public Function GetCompanyById(companyId As Integer) As DataTable
        Dim query As String = "
            SELECT
                C.ID AS Id,
                C.C_NAME AS Nome,
                C.C_CNPJ AS CNPJ,
                Associados = STUFF((
                    SELECT ', ' + A.A_NAME
                    FROM T_REL R
                    INNER JOIN T_ASSOC A ON R.A_ID = A.Id
                    WHERE R.C_ID = C.ID
                    FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '')
            FROM
                T_COMP C
            WHERE
                C.ID = @companyId;"

        Dim dt As New DataTable()

        Using connection As SqlConnection = DatabaseContext.GetConnection()
            connection.Open()
            Using command As New SqlCommand(query, connection)
                command.Parameters.AddWithValue("@companyId", companyId)
                Dim adapter As New SqlDataAdapter(command)
                adapter.Fill(dt)
            End Using
        End Using

        Return dt
    End Function
End Class
