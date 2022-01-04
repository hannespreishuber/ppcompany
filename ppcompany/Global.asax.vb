Imports System.Web.Optimization
Imports System.IdentityModel.Services

Public Class Global_asax
    Inherits HttpApplication

    Private Sub Application_Start(sender As Object, e As EventArgs)
        ' Code, der beim Anwendungsstart ausgeführt wird
        RouteConfig.RegisterRoutes(RouteTable.Routes)
        BundleConfig.RegisterBundles(BundleTable.Bundles)
    End Sub
End Class
