<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="TxtToPDFConverter.Default" %>

<!doctype html public '-//W3C//DTD HTML 4.01//EN'
  'http://www.w3.org/TR/html4/strict.dtd'>
<html>
  <head>
    <title>Convert to PDF Web App</title>
    <link rel="stylesheet" href="https://www.w3.org/StyleSheets/Core/Chocolate" type="text/css">
  </head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="sm1" runat="server" />
        <h1>Chris' Convert to PDF Web App</h1>
        <div>
            Upload file:
            <asp:FileUpload ID="upload" runat="server" />
            <asp:Button ID="BtnSubmit" runat="server" Text="Submit" OnClick="BtnSubmit_Click" />
        </div>
        <asp:Button ID="RefreshButton" runat="server" Text="Refresh" onclick="RefreshButton_Click" />
        <div>
            <asp:UpdatePanel ID="up1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                
                <ContentTemplate>
                    <asp:GridView ID="gvPDFs" runat="server" AutoGenerateColumns="False" ShowFooter="True" GridLines="Vertical" CellPadding="4">
                        <Columns>
                         <asp:TemplateField>
                <ItemTemplate>
                    <a href='<%# Eval("Url") %>'><%# Eval("Title") %></a>
                </ItemTemplate>
            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                    
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </form>
</body>
</html>