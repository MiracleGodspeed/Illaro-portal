<%@ Page Async="true" Language="C#" AutoEventWireup="true" CodeBehind="AttendanceReportBulk.aspx.cs" Inherits="Abundance_Nk.Web.Reports.Presenter.AttendanceReportBulk" %>
<%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">

    <title></title>
    <link href="../../../Content/bootstrap.css" rel="stylesheet" />
    <script src="../../../Scripts/bootstrap.js"></script>
    <link href="../../../Content/misc.css" rel="stylesheet" />
</head>
<body style="border:none; margin:0; padding:0">


    <form id="form1" runat="server">
        <div >
        <asp:ScriptManager ID="ScriptManager1" runat="server" AsyncPostBackTimeout="6000000">
     
        </asp:ScriptManager>
        <div>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <p>
                <asp:Label ID="lblMessage" runat="server"></asp:Label>
                </p>
            <div class="contentpanel">
                        <div class="panel panel-default">
                            <div class="panel-body">
                <div class="form-inline">
                        <div class="form-group">
                            <asp:DropDownList ID="ddlSession" runat="server" AutoPostBack="True" class="form-control"  OnSelectedIndexChanged="ddlSession_SelectedIndexChanged">
                            </asp:DropDownList>
                            </div>
                        <div class="form-group">
                            <asp:DropDownList ID="ddlSemester" runat="server" CssClass="form-control" >
                            </asp:DropDownList>
                        </div>



                        <div class="form-group">
                        <asp:DropDownList ID="ddlProgramme" runat="server" AutoPostBack="True" CssClass="form-control" OnSelectedIndexChanged="ddlProgramme_SelectedIndexChanged1" Width="200px">
                            </asp:DropDownList>
                            </div>
                        <div class="form-group">
                            <asp:DropDownList ID="ddlDepartment" runat="server" CssClass="form-control"  AutoPostBack="True" OnSelectedIndexChanged="ddlDepartment_SelectedIndexChanged">
                            </asp:DropDownList>
                        </div>
                        <div class="form-group">
                                <asp:DropDownList ID="ddlDepartmentOption" CssClass="form-control" runat="server"></asp:DropDownList>
                            </div>
                        <div class="form-group">
                            <asp:DropDownList ID="ddlLevel" runat="server" CssClass="form-control">
                            </asp:DropDownList>
                        </div>
                        <div class="form-group">
                            <asp:Button ID="Display_Button" runat="server" CssClass="btn btn-success mr5" OnClick="Display_Button_Click1" Text="Prepare Sheets" Width="135px" />
                            </div>

                            <div class="form-group">
                                <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="UpdatePanel1">
                                    <ProgressTemplate>
                                        <asp:Image ID="Image1" runat="server" ImageUrl="~/Content/Images/bx_loader.gif" />
                                        Loading...
                                    </ProgressTemplate>
                                </asp:UpdateProgress>
                            </div>
                        
                </div>
                     </div>
                            </div>
                </div>

                                


        </ContentTemplate>
</asp:UpdatePanel>
</div>
    <%--<script src="~/Scripts/jquery-1.11.1.min.js"></script>
    <script src="../../Scripts/jquery-2.1.3.js"></script>
    <script src="../../Scripts/bootstrap.min.js"></script>--%>
 
  

       <%-- <rsweb:ReportViewer ID="ReportViewer1" runat="server" Width="100%">
        </rsweb:ReportViewer>--%>
 
      

</div>
    </form>

    </body>
</html>

