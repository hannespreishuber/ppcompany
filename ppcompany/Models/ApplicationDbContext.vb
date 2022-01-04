Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.Data.Entity
Imports System.Linq
Imports System.Web

Public Class ApplicationDbContext 
    Inherits DbContext

    Public Sub New()
        MyBase.New("DefaultConnection")
    End Sub
    
    Public Property UserTokenCacheList As DBSet(Of UserTokenCache)
End Class

Public Class UserTokenCache
    <Key>
    Public Property UserTokenCacheId As Integer    
    Public Property webUserUniqueId As String
    Public Property cacheBits As Byte()
    Public Property LastWrite As DateTime
End Class
