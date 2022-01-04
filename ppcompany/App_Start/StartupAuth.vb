Imports System.Globalization
Imports System.Threading.Tasks
'NUget 
Imports Microsoft.IdentityModel.Clients.ActiveDirectory
Imports System.IdentityModel.Claims


Imports Microsoft.Owin.Extensions
Imports Microsoft.Owin.Security
Imports Microsoft.Owin.Security.Cookies
Imports Microsoft.Owin.Security.OpenIdConnect
Imports Owin


Partial Public Class Startup
    Public Shared clientId As String = ConfigurationManager.AppSettings("ida:ClientId")
    Public Shared aadInstance As String = EnsureTrailingSlash(ConfigurationManager.AppSettings("ida:AADInstance"))
    Public Shared tenantId As String = ConfigurationManager.AppSettings("ida:TenantId")
    Public Shared postLogoutRedirectUri As String = ConfigurationManager.AppSettings("ida:PostLogoutRedirectUri")
    Public Shared authority As String = aadInstance & tenantId
    Public Shared tmp As String
    'eingefügt
    Private Shared ClientSecret As String = ConfigurationManager.AppSettings("ida:ClientSecret")
    ' Dies ist die Ressourcen-ID der AAD Graph-API. Diese wird benötigt, um ein Token zum Aufrufen der Graph-API anzufordern.
    Private graphResourceId As String = "https://graph.windows.net"

    Public Sub ConfigureAuth(app As IAppBuilder)
        app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType)

        app.UseCookieAuthentication(New CookieAuthenticationOptions())

        app.UseOpenIdConnectAuthentication(New OpenIdConnectAuthenticationOptions() With {
            .ClientId = clientId,
            .Authority = authority,
            .PostLogoutRedirectUri = postLogoutRedirectUri,
            .Notifications = New OpenIdConnectAuthenticationNotifications() With {
              .AuthorizationCodeReceived = Function(context)
                                               'Adaltokecache datei eingefügt
                                               'begin eingefügt
                                               Dim code = context.Code   '.code OpenIdConnect 4.2 AuthorizationCodeReceived
                                               Dim credential As New ClientCredential(clientId, ClientSecret)
                                               Dim signedInUserID As String = context.AuthenticationTicket.Identity.FindFirst(ClaimTypes.NameIdentifier).Value

                                               Dim authContext As New AuthenticationContext(authority, New ADALTokenCache(signedInUserID))
                                               Dim result As AuthenticationResult = authContext.AcquireTokenByAuthorizationCodeAsync(code, New Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)), credential, graphResourceId).Result
                                               'end eingefügt
                                               Return Task.FromResult(0)
                                           End Function
              }
        })
        ' Auf diese Weise wird Middleware, die oberhalb dieser Zeile definiert ist, ausgeführt, bevor die Autorisierungsregel in "web.config" angewendet wird.
        app.UseStageMarker(PipelineStage.Authenticate)
    End Sub

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