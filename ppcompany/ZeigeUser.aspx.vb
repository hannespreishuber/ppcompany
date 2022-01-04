Imports Microsoft.Graph

Public Class ZeigeUser
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Protected Async Function Button1_ClickAsync(sender As Object, e As EventArgs) As Threading.Tasks.Task Handles Button1.Click
        'Dim graphClient = New GraphServiceClient(authProvider)

        'Dim Profile = Await graphClient.Me.Profile
        '.Request()
        '.GetAsync()
    End Function
End Class