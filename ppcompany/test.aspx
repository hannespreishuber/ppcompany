﻿<%@ Page Title="Benutzerinformationen" Language="VB" MasterPageFile="~/Site.Master" 
    AutoEventWireup="true" Async="true" CodeBehind="test.aspx.vb" Inherits="Test" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %>.</h2>
    <asp:Panel ID="GetToken" runat="server" Visible="false">
        <p>Das Token für den Zugriff auf die Graph-API ist abgelaufen. Klicken Sie <asp:LinkButton runat="server" OnClick="Unnamed_Click">hier,</asp:LinkButton> um sich anzumelden und ein neues Zugriffstoken abzurufen.</p>
    </asp:Panel>
    <asp:Panel ID="ShowData" runat="server">
          <asp:FormView ID="UserData" runat="server" 
        ItemType="Microsoft.Azure.ActiveDirectory.GraphClient.IUser" 
        RenderOuterTable="false" DefaultMode="ReadOnly"
        ViewStateMode="Disabled">
        <ItemTemplate>
            <table class="table table-bordered table-striped">
                <tr>
                    <td>Anzeigename</td>
                    <td><%#: Item.DisplayName %></td>
                </tr>
                <tr>
                    <td>Vorname</td>
                    <td><%#: Item.GivenName %></td>
                </tr>
                <tr>
                    <td>Nachname</td>
                    <td><%#: Item.Surname %></td>
                </tr>
            </table>
        </ItemTemplate>
    </asp:FormView>
    </asp:Panel>
    <asp:Button ID="Button1" runat="server" Text="Button" />
  
</asp:Content>
