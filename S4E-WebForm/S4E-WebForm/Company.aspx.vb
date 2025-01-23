Imports S4E.Domain
Imports S4EE.Infrastructure

Public Class Contact
    Inherits Page
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        GetAvailableAssociates()
        GetSelectedAssociates()
        GetCompanies()
    End Sub
    Protected Sub btnInsert_Click(sender As Object, e As EventArgs) Handles btnInsert.Click
        Dim companyRepo As New CompanyRepository()
        Dim company As New Company()
        Dim associates As List(Of Associate) = GetAssociates()

        company.Name = txtName.Text
        company.Cnpj = txtCnpj.Text
        company.Associates = associates

        companyRepo.AddCompany(company)
        GetCompanies()
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
    Protected Sub btnRemove_Click(sender As Object, e As EventArgs) Handles btnRemove.Click
        For i As Integer = ListBox2.Items.Count - 1 To 0 Step -1
            If ListBox2.Items(i).Selected Then
                Dim newItem As New ListItem(ListBox2.Items(i).Text, ListBox2.Items(i).Value)
                ListBox1.Items.Add(newItem)
                ListBox2.Items.RemoveAt(i)
            End If
        Next
    End Sub
    Protected Sub GridView1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles gridViewComp.SelectedIndexChanged
        txtId.Text = HttpUtility.HtmlDecode(gridViewComp.SelectedRow.Cells.Item(1).Text.ToString)
        txtName.Text = HttpUtility.HtmlDecode(gridViewComp.SelectedRow.Cells.Item(2).Text.ToString)
        txtCnpj.Text = HttpUtility.HtmlDecode(gridViewComp.SelectedRow.Cells.Item(3).Text.ToString)
        ListBox1.Items.Clear()
        ListBox2.Items.Clear()
        GetAvailableAssociates()
        GetSelectedAssociates()
    End Sub
    Protected Sub btnUpdate_Click(sender As Object, e As EventArgs) Handles btnUpdate.Click
        Dim companyRepo As New CompanyRepository()
        Dim company As New Company()
        Dim associates As List(Of Associate) = GetAssociates()

        company.Id = Integer.Parse(txtId.Text)
        company.Name = txtName.Text
        company.Cnpj = txtCnpj.Text
        company.Associates = associates

        companyRepo.UpdateCompany(company)
        GetCompanies()
        ResetFields()
    End Sub
    Protected Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click
        Dim companyRepo As New CompanyRepository()
        Dim companyId As Integer = txtId.Text

        companyRepo.DeleteCompany(companyId)
        GetCompanies()
        ResetFields()
    End Sub
    Protected Sub btnIdFilter_Click(sender As Object, e As EventArgs) Handles btnIdFilter.Click
        Dim companyRepo As New CompanyRepository()
        Dim companyId As Integer

        If Integer.TryParse(txtIdFilter.Text, companyId) Then
            Dim dt As DataTable = companyRepo.GetCompanyById(companyId)
            gridViewComp.DataSource = dt
            gridViewComp.DataBind()
        Else
        End If
    End Sub
    Private Sub GetCompanies()
        Dim repository As New CompanyRepository()
        Dim dt As DataTable = repository.GetCompanies()
        gridViewComp.DataSource = dt
        gridViewComp.DataBind()
    End Sub
    Private Sub ResetFields()
        txtId.Text = String.Empty
        txtName.Text = String.Empty
        txtCnpj.Text = String.Empty

        ListBox1.Items.Clear()
        ListBox2.Items.Clear()
        GetAvailableAssociates()
        GetSelectedAssociates()

        txtId.Focus()
    End Sub
    Private Sub GetAvailableAssociates()
        Dim selectedAssociateIds As New List(Of Integer)
        For Each item As ListItem In ListBox2.Items
            selectedAssociateIds.Add(Convert.ToInt32(item.Value))
        Next

        If Not String.IsNullOrWhiteSpace(txtId.Text) Then
            Dim associateRepo As New AssociateRepository()
            Dim availableAssociates As List(Of Associate) = associateRepo.GetAvailableAssociates(Convert.ToInt32(txtId.Text), selectedAssociateIds)

            For Each associate In availableAssociates
                If ListBox1.Items.FindByValue(associate.Id.ToString()) Is Nothing Then
                    ListBox1.Items.Add(New ListItem(associate.Name, associate.Id.ToString()))
                End If
            Next
        End If
    End Sub
    Private Sub GetSelectedAssociates()
        Dim selectedAssociateIds As New List(Of Integer)
        For Each item As ListItem In ListBox2.Items
            selectedAssociateIds.Add(Convert.ToInt32(item.Value))
        Next

        If Not String.IsNullOrWhiteSpace(txtId.Text) Then
            Dim associateRepo As New AssociateRepository()
            Dim selectedAssociates As List(Of Associate) = associateRepo.GetSelectedAssociates(Convert.ToInt32(txtId.Text))

            For Each associate In selectedAssociates
                If Not selectedAssociateIds.Contains(associate.Id) Then
                    If ListBox2.Items.FindByValue(associate.Id.ToString()) Is Nothing Then
                        ListBox2.Items.Add(New ListItem(associate.Name, associate.Id.ToString()))
                    End If
                End If
            Next
        End If
    End Sub
    Private Function GetAssociates() As List(Of Associate)
        Dim associateRepo As New AssociateRepository()

        Dim selectedIds As New List(Of Integer)()
        For Each item As ListItem In ListBox2.Items
            selectedIds.Add(Convert.ToInt32(item.Value))
        Next

        Dim associates As List(Of Associate) = associateRepo.GetAssociatesByIds(selectedIds)

        Return associates
    End Function
End Class