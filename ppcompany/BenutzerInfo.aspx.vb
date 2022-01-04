
Imports System
Imports System.Collections.Generic
Imports System.Configuration
Imports System.Linq
Imports System.Security.Claims
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Threading.Tasks
Imports Microsoft.Azure.ActiveDirectory.GraphClient
Imports Microsoft.IdentityModel.Clients.ActiveDirectory
Imports Microsoft.Owin.Extensions
Imports Microsoft.Owin.Security
Imports Microsoft.Owin.Security.OpenIdConnectPublic
Imports Microsoft.Owin.Security.OpenIdConnect
Public Class BenutzerInfo
    Inherits System.Web.UI.Page
    Private db As New ApplicationDbContext()
    Private Shadows clientId As String = ConfigurationManager.AppSettings("ida:ClientId")
    Private appKey As String = ConfigurationManager.AppSettings("ida:ClientSecret")
    Private aadInstance As String = EnsureTrailingSlash(ConfigurationManager.AppSettings("ida:AADInstance"))
    Private graphResourceID As String = "https://graph.windows.net"

    Protected Sub Page_Load(sender As Object, e As EventArgs)
        RegisterAsyncTask(New PageAsyncTask(New Func(Of Task)(AddressOf GetUserData)))
    End Sub

    Public Function GetUserData() As Task
        Return Task.Run(Sub()
                            Dim tenantID As String = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value
                            Dim userObjectID As String = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value
                            Try
                                Dim servicePointUri As New Uri(graphResourceID)
                                Dim serviceRoot As New Uri(servicePointUri, tenantID)
                                Dim activeDirectoryClient As New ActiveDirectoryClient(serviceRoot, Async Function() Await GetTokenForApplication())

                                ' das Token zum Abfragen von Graph zum Abrufen der Benutzerdetails verwenden
                                Dim user As IUser = activeDirectoryClient.Users.Where(Function(u) u.ObjectId.Equals(userObjectID)).ExecuteAsync().Result.CurrentPage.ToList().First()

                                Dim userList As New List(Of IUser)
                                userList.Add(user)
                                UserData.DataSource = userList
                                UserData.DataBind()
                                ' wenn oben ein Fehler aufgetreten ist, muss sich der Benutzer explizit erneut für die App authentifizieren, um das erforderliche Token abzurufen
                            Catch generatedExceptionName As AdalException
                                GetToken.Visible = True
                                ' wenn oben ein Fehler aufgetreten ist, muss sich der Benutzer explizit erneut für die App authentifizieren, um das erforderliche Token abzurufen
                            Catch generatedExceptionName As Exception
                                ShowData.Visible = False
                                GetToken.Visible = True
                            End Try
                        End Sub)
    End Function

    Protected Sub Unnamed_Click(sender As Object, e As EventArgs)
        ShowData.Visible = False
        HttpContext.Current.GetOwinContext().Authentication.Challenge(New AuthenticationProperties With {.RedirectUri = "/UserInfo"},
        OpenIdConnectAuthenticationDefaults.AuthenticationType)
    End Sub

    Public Async Function GetTokenForApplication() As Task(Of String)
        Dim signedInUserID As String = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value
        Dim tenantID As String = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value
        Dim userObjectID As String = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value

        ' ein Token für Graph abrufen, ohne dass eine Benutzerinteraktion ausgelöst wird (aus dem Cache, über ein Aktualisierungstoken für mehrere multi-Ressourcen usw.)
        Dim clientcred As New ClientCredential(clientId, appKey)
        ' "AuthenticationContext" mit dem Tokencache des aktuell angemeldeten Benutzers initialisieren, der in der Datenbank der App gespeichert ist
        Dim authenticationContext As New AuthenticationContext(aadInstance & tenantID, New ADALTokenCache(signedInUserID))
        Dim authenticationResult As AuthenticationResult = Await authenticationContext.AcquireTokenSilentAsync(graphResourceID, clientcred, New UserIdentifier(userObjectID, UserIdentifierType.UniqueId))
        Return authenticationResult.AccessToken
    End Function

    Private Shared Function EnsureTrailingSlash(ByRef value As String) As String
        If (IsNothing(value)) Then
            value = String.Empty
        End If

        If (Not value.EndsWith("/", StringComparison.Ordinal)) Then
            Return value & "/"
        End If

        Return value
    End Function

End Class