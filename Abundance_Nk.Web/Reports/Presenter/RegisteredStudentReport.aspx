<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RegisteredStudentReport.aspx.cs" Inherits="Abundance_Nk.Web.Reports.Presenter.RegisteredStudentsReport" %>

<%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link href="../../Content/bootstrap.min.css" rel="stylesheet" />
    <title></title>
</head>
<body style="border:none; margin:0; padding:0">


    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" AsyncPostBackTimeout="60000">
     
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
                               <%-- <div class="form-inline">
                                <div class="form-group">
                                    <label  title="Sort Type">Report Type</label>
                                &nbsp;
                                </div>
                                    
                                <div class="form-group" >
                                    <asp:RadioButtonList ID="rblSortOption" runat="server" RepeatDirection="Horizontal" CssClass="radiobuttonlist2" >
                                        <asp:ListItem Value="1">Student Detail</asp:ListItem> 
                                        <asp:ListItem Value="2">Student Statistics</asp:ListItem>
                                    </asp:RadioButtonList>
                                </div>
                             </div>--%>
            <div class="form-inline">


                <div class="form-group">
                    <div class="form-group">
                        <label class="sr-only" for="ddlSession"></label>
                        <asp:DropDownList ID="ddlSession" CssClass="form-control" runat="server">
                        </asp:DropDownList>
                    </div>

                     <div class="form-group">
                        <label class="sr-only" for="ddlLevel"></label>
                        <asp:DropDownList ID="ddlLevel" CssClass="form-control"  runat="server" >
                    </asp:DropDownList>
                    </div>

                     <div class="form-group">
                        <label class="sr-only" for="ddlProgramme"></label>
                         <asp:DropDownList ID="ddlProgramme" CssClass="form-control" runat="server">
                    </asp:DropDownList>
                    </div>


                     <div class="form-group">
                        <label class="sr-only" for="ddlDepartment"></label>
                        <asp:DropDownList ID="ddlDepartment"   CssClass="form-control" runat="server">
                    </asp:DropDownList>
                    </div>


                     <div class="form-group">
                        <asp:Button ID="Display_Button" runat="server" Text="Display Report" Width="111px" class="btn btn-success " OnClick="Display_Button_Click1" />
            
                    </div>




                 
                    
                  
                   
                
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
             <rsweb:ReportViewer ID="ReportViewer1" runat="server" Width="100%">
        </rsweb:ReportViewer>
        </ContentTemplate>
</asp:UpdatePanel>
</div>
    <script src="~/Scripts/jquery-1.11.1.min.js"></script>
    <script src="../../Scripts/jquery-2.1.3.js"></script>
    <script src="../../Scripts/bootstrap.min.js"></script>
 
    </form>

    </body>
</html>
