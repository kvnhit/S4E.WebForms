Imports System.Globalization
Imports S4EE.Infrastructure
Imports S4E.Domain

Public Class About
    Inherits System.Web.UI.Page
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        GetAvailableCompanies()
        GetSelectedCompanies()
        GetAssociates()
    End Sub
    Protected Sub Button1_Click(sender As Object, e As EventArgs) Handles btnInsert.Click
        Dim associateRepo As New AssociateRepository()
        Dim associate As New Associate()
        Dim companies As List(Of Company) = GetCompanies()

        associate.Name = txtName.Text
        associate.Cpf = txtCpf.Text
        associate.BirthDate = DateTime.ParseExact(txtBirth.Text, "dd/MM/yyyy", CultureInfo.InvariantCulture).Date
        associate.Companies = companies

        associateRepo.AddAssociate(associate)
        GetAssociates()
        ResetFields()
    End Sub
    Protected Sub btnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click
        For i As Integer = ListBox1.Items.Count - 1 To 0 Step -1
            If ListBox1.Items(i).Selected Then
                Dim newItem As New ListItem(ListBox1.Items(i).Text, ListBox1.Items(i).Value)
                ListBox2.Items.Add(newItem)
                ListBox1.Items.RemoveAt(i)
            End If
        Next
    End Sub
    Protected Sub btnRemove_Click(sender As Object, e As EventArgs)
        For i As Integer = ListBox2.Items.Count - 1 To 0 Step -1
            If ListBox2.Items(i).Selected Then
                Dim newItem As New ListItem(ListBox2.Items(i).Text, ListBox2.Items(i).Value)
                ListBox1.Items.Add(newItem)
                ListBox2.Items.RemoveAt(i)
            End If
        Next
    End Sub
    Protected Sub gridViewAssoc_SelectedIndexChanged(sender As Object, e As EventArgs) Handles gridViewAssoc.SelectedIndexChanged
        txtId.Text = HttpUtility.HtmlDecode(gridViewAssoc.SelectedRow.Cells.Item(1).Text.ToString)
        txtName.Text = HttpUtility.HtmlDecode(gridViewAssoc.SelectedRow.Cells.Item(2).Text.ToString)
        txtCpf.Text = HttpUtility.HtmlDecode(gridViewAssoc.SelectedRow.Cells.Item(3).Text.ToString)
        txtBirth.Text = HttpUtility.HtmlDecode(gridViewAssoc.SelectedRow.Cells.Item(4).Text.ToString)
        ListBox1.Items.Clear()
        ListBox2.Items.Clear()
        GetAvailableCompanies()
        GetSelectedCompanies()
    End Sub
    Protected Sub btnUpdate_Click(sender As Object, e As EventArgs) Handles btnUpdate.Click
        Dim associateRepo As New AssociateRepository()
        Dim associate As New Associate()
        Dim companies As List(Of Company) = GetCompanies()

        associate.Id = Integer.Parse(txtId.Text)
        associate.Name = txtName.Text
        associate.Cpf = txtCpf.Text
        associate.BirthDate = DateTime.Parse(txtBirth.Text).Date
        associate.Companies = companies

        associateRepo.UpdateAssociate(associate)
        GetAssociates()
        ResetFields()
    End Sub
    Protected Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click
        Dim associateRepo As New AssociateRepository()
        Dim associateId As Integer = txtId.Text

        associateRepo.DeleteAssociate(associateId)
        GetAssociates()
        ResetFields()
    End Sub
    Protected Sub btnIdFilter_Click(sender As Object, e As EventArgs) Handles btnIdFilter.Click
        Dim id As Integer
        If Integer.TryParse(txtIdFilter.Text, id) Then
            Dim associateRepo As New AssociateRepository()
            Dim dt As DataTable = associateRepo.GetAssociatesById(id)

            gridViewAssoc.DataSource = dt
            gridViewAssoc.DataBind()
        Else
            gridViewAssoc.DataSource = Nothing
            gridViewAssoc.DataBind()
            MsgBox("ID inválido!", MsgBoxStyle.Exclamation, "Erro")
        End If
    End Sub
    Private Sub ResetFields()
        txtId.Text = String.Empty
        txtName.Text = String.Empty
        txtCpf.Text = String.Empty
        txtBirth.Text = String.Empty

        ListBox1.Items.Clear()
        ListBox2.Items.Clear()
        GetAvailableCompanies()
        GetSelectedCompanies()

        txtId.Focus()
    End Sub
    Private Sub GetAvailableCompanies()
        Dim companyRepo As New CompanyRepository()

        Dim assocId As Integer = If(String.IsNullOrWhiteSpace(txtId.Text), 0, Convert.ToInt32(txtId.Text))

        Dim availableCompanies As List(Of Company) = companyRepo.GetAvailableCompanies(assocId)

        Dim SelectedValues As New List(Of String)
        For Each item As ListItem In ListBox2.Items
            SelectedValues.Add(item.Value)
        Next

        For Each company As Company In availableCompanies
            If Not SelectedValues.Contains(company.Id.ToString()) Then
                If ListBox1.Items.FindByValue(company.Id.ToString()) Is Nothing Then
                    ListBox1.Items.Add(New ListItem(company.Name, company.Id.ToString()))
                End If
            End If
        Next
    End Sub
    Private Sub GetSelectedCompanies()
        Dim companyRepo As New CompanyRepository()
        If Not String.IsNullOrWhiteSpace(txtId.Text) Then

            Dim assocId As Integer = Convert.ToInt32(txtId.Text)

            Dim selectedCompanies As List(Of Company) = companyRepo.GetSelectedCompanies(assocId)

            Dim selectedValues As New List(Of String)
            For Each item As ListItem In ListBox1.Items
                selectedValues.Add(item.Value)
            Next

            For Each company As Company In selectedCompanies
                If Not selectedValues.Contains(company.Id.ToString()) Then
                    If ListBox2.Items.FindByValue(company.Id.ToString()) Is Nothing Then
                        ListBox2.Items.Add(New ListItem(company.Name, company.Id.ToString()))
                    End If
                End If
            Next
        End If
    End Sub
    Private Function GetCompanies() As List(Of Company)
        Dim companyRepo As New CompanyRepository()

        Dim idList As New List(Of Integer)

        If ListBox2.Items IsNot Nothing AndAlso ListBox2.Items.Count > 0 Then
            For Each item As ListItem In ListBox2.Items
                idList.Add(Convert.ToInt32(item.Value))
            Next
        End If

        If idList Is Nothing OrElse idList.Count = 0 Then
            Return New List(Of Company)()
        Else
            Return companyRepo.GetCompaniesByIds(idList)
        End If
    End Function
    Private Sub GetAssociates()
        Dim associateRepo As New AssociateRepository()
        Dim dt As DataTable = associateRepo.GetAssociates()

        gridViewAssoc.DataSource = dt
        gridViewAssoc.DataBind()
    End Sub
End Class