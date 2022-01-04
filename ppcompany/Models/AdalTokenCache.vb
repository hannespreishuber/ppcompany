Imports System.ComponentModel.DataAnnotations
Imports System.Data.Entity
Imports Microsoft.IdentityModel.Clients.ActiveDirectory

Public Class ADALTokenCache
    Inherits TokenCache
    Private db As New ApplicationDbContext()
    Private userId As String
    Private Cache As UserTokenCache

    Public Sub New(signedInUserId As String)
        ' den Cache dem aktuellen Benutzer der Web-App zuordnen
        userId = signedInUserId
        Me.AfterAccess = AddressOf AfterAccessNotification
        Me.BeforeAccess = AddressOf BeforeAccessNotification
        Me.BeforeWrite = AddressOf BeforeWriteNotification

        ' den Eintrag in der Datenbank nachschlagen
        Cache = db.UserTokenCacheList.FirstOrDefault(Function(c) c.WebUserUniqueId = userId)
        ' den Eintrag im Arbeitsspeicher speichern
        ' Deserialize(If((Cache Is Nothing), Nothing, MachineKey.Unprotect(Cache.cacheBits, "ADALCache"))) obsolet?

        DeserializeAdalV3(If((Cache Is Nothing), Nothing, MachineKey.Unprotect(Cache.cacheBits, "ADALCache")))
    End Sub

    ' die Datenbank bereinigen
    Public Overrides Sub Clear()
        MyBase.Clear()
        Dim cacheEntry = db.UserTokenCacheList.FirstOrDefault(Function(c) c.WebUserUniqueId = userId)
        db.UserTokenCacheList.Remove(cacheEntry)
        db.SaveChanges()
    End Sub

    ' Eine Benachrichtigung, die ausgelöst wird, bevor ADAL auf den Cache zugreift.
    ' Hier besteht die Möglichkeit, die In-Memory-Kopie aus der Datenbank zu aktualisieren, wenn die In-Memory-Version veraltet ist.
    Private Sub BeforeAccessNotification(args As TokenCacheNotificationArgs)
        If Cache Is Nothing Then
            ' erstmaliger Zugriff
            Cache = db.UserTokenCacheList.FirstOrDefault(Function(c) c.WebUserUniqueId = userId)
        Else
            ' letzten Schreibvorgang aus der Datenbank abrufen
            Dim status = From e In db.UserTokenCacheList Where (e.WebUserUniqueId = userId) Select New With {
                .LastWrite = e.LastWrite
            }
            ' wenn die In-Memory-Kopie älter als die persistente Kopie ist
            If status.First().LastWrite > Cache.LastWrite Then
                ' aus dem Speicher lesen, In-Memory-Kopie aktualisieren
                Cache = db.UserTokenCacheList.FirstOrDefault(Function(c) c.WebUserUniqueId = userId)
            End If
        End If
        'Me.Deserialize(If((Cache Is Nothing), Nothing, MachineKey.Unprotect(Cache.cacheBits, "ADALCache")))
        DeserializeAdalV3(If((Cache Is Nothing), Nothing, MachineKey.Unprotect(Cache.cacheBits, "ADALCache")))


    End Sub

    ' Eine Benachrichtigung, die ausgelöst wird, nachdem ADAL auf den Cache zugegriffen hat.
    ' Wenn die Kennzeichnung "HasStateChanged" festgelegt ist, hat ADAL den Inhalt des Caches geändert.
    Private Sub AfterAccessNotification(args As TokenCacheNotificationArgs)
        ' wenn sich der Zustand geändert hat
        If Me.HasStateChanged Then
            If Cache Is Nothing Then
                Cache = New UserTokenCache() With {
                    .WebUserUniqueId = userId
                }
            End If
            Cache.cacheBits = MachineKey.Protect(Me.SerializeAdalV3(), "ADALCache")
            Cache.LastWrite = DateTime.Now
            ' die Datenbank und den letzten Schreibvorgang aktualisieren 
            db.Entry(Cache).State = If(Cache.UserTokenCacheId = 0, EntityState.Added, EntityState.Modified)
            db.SaveChanges()
            Me.HasStateChanged = False
        End If
    End Sub

    Private Sub BeforeWriteNotification(args As TokenCacheNotificationArgs)
        ' wenn Sie sicherstellen möchten, dass kein gleichzeitiger Schreibvorgang stattfindet, verwenden Sie diese Benachrichtigung, um den Eintrag zu sperren.
    End Sub
    
    Public Overrides Sub DeleteItem(item As TokenCacheItem)
        MyBase.DeleteItem(item)
    End Sub
End Class
