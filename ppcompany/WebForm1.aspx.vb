Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Security.Claims
Imports Microsoft.Graph
Imports Microsoft.Identity.Client

Public Class WebForm1
    Inherits System.Web.UI.Page
    Public Shared daToken As String
    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load

    End Sub

    Protected Async Sub Buttonx_ClickAsync(sender As Object, e As EventArgs)



    End Sub


    Protected Async Sub Button1_Click(sender As Object, e As EventArgs)
        Dim authorityUri = $"https://login.microsoftonline.com/{Startup.tenantId}/v2.0"

        Dim redirectUri = "http://localhost:44355"
        Dim scopes = {"https://graph.microsoft.com/.default"}
        'scopes =user.read Client credential flows must have a scope value with /.default suffixed to the resource identifier 

        'https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Client-Applications#public-client-and-confidential-client-applications

        '        Dim publicClient = PublicClientApplicationBuilder.Create(Startup.clientId) _
        '               .WithAuthority(New Uri(authorityUri)) _
        '  .WithCacheOptions(CacheOptions.EnableSharedCacheOptions) _
        '.WithRedirectUri(redirectUri).Build()
        'https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-web-app-call-api-acquire-token?tabs=aspnet
        Dim publicClient = ConfidentialClientApplicationBuilder.Create(Startup.clientId) _
            .WithClientSecret(ConfigurationManager.AppSettings("ida:ClientSecret")) _
               .WithAuthority(New Uri(authorityUri)) _
        .WithRedirectUri(redirectUri).Build()

        'Dim app As IConfidentialClientApplication = MsalAppBuilder.BuildConfidentialClientApplication()
        'Dim a = Await app.GetAccountsAsync()



        Dim accounts = Await publicClient.GetAccountsAsync() 'Empty wenn Tokencahce leer?

        Try
            Dim res1 = publicClient.AcquireTokenSilent(scopes, accounts.FirstOrDefault) _
                .WithForceRefresh(True) _
                .ExecuteAsync().Result
            daToken = res1.AccessToken
        Catch ex As Exception

            Try

                '   Dim accessTokenRequest = publicClient.AcquireToken(scopes)
                Dim accessTokenRequest = publicClient.AcquireTokenForClient(scopes)
                Dim res = accessTokenRequest.ExecuteAsync().Result
                daToken = res.AccessToken

            Catch ex2 As Exception
                Dim x = ex2

            End Try


        End Try




        Dim _httpClient = New HttpClient()
        _httpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Bearer", daToken)
        _httpClient.DefaultRequestHeaders.Accept.Add(New MediaTypeWithQualityHeaderValue("application/json"))

        Dim graphClient As GraphServiceClient = New GraphServiceClient(_httpClient) With {
    .AuthenticationProvider = New DelegateAuthenticationProvider(Async Function(requestMessage)
                                                                     requestMessage.Headers.Authorization = New AuthenticationHeaderValue("Bearer", daToken)


                                                                 End Function)
}


        ' /me request is only valid with delegated authentication flow.



        Dim Profile = Await graphClient.Me _
        .Request() _
        .GetAsync()

    End Sub
End Class