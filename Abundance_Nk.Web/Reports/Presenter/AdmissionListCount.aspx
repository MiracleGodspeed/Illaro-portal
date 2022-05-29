﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdmissionListCount.aspx.cs" Inherits="Abundance_Nk.Web.Reports.Presenter.AdmissionListCount" %>
<%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>

        <link href="../../Content/bootstrap.css" rel="stylesheet" />
    <script src="../../Scripts/bootstrap.js"></script>
     <link href="../../Content/misc.css" rel="stylesheet" />

</head>
<body style="border:none; margin:0; padding:0">
    <form id="form1" runat="server">
     <div>
         <asp:ScriptManager ID="ScriptManager1" runat="server" AsyncPostBackTimeOut="60000">
         </asp:ScriptManager>
         <asp:UpdatePanel ID="UpdatePanel1" runat="server">
             <ContentTemplate>

                    <p>
                        <asp:Label ID="lblMessage" runat="server" BackColor="Yellow" ForeColor="Red" ></asp:Label>
         </p>

                 <div class="contentpanel">
                <div class="panel panel-default">
                    <div class="panel-body">

                        <div class="row col-md-offset-1">
                             <div class="col-md-12">
                                <div class="form-inline">
                                    <div class="form-group">
                                        <label class="sr-only" for="ddlSession"></label>
                                        <asp:DropDownList ID="ddlSession" cssclass="form-control" runat="server"></asp:DropDownList>
                                    </div>
                                    
                                     <div class="form-group">
                                    <asp:Button ID="btnDisplayReport" cssclass="btn btn-danger mr5" runat="server" Text="Display Report" OnClick="btnDisplayReport_Click" />
                                     </div>
                                    
                                     <div class="form-group">
                                         <asp:UpdateProgress ID="UpdateProgress1" AssociatedUpdatePanelID="UpdatePanel1" runat="server">
                                        <ProgressTemplate>
                                            <asp:Image ID="Image1" runat="server" ImageUrl="~/Content/Images/bx_loader.gif" /> Loading...
                                        </ProgressTemplate>
                                    </asp:UpdateProgress>
                                         </div>
                                </div>
                            </div>
                    </div>
                    </div>
                </div>
        </div>
                 
        <rsweb:ReportViewer ID="rv" width="100%" runat="server">
        </rsweb:ReportViewer>

                   </ContentTemplate>
         </asp:UpdatePanel>

    </div>
    </form>
</body>
</html>

