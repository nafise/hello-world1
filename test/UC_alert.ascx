<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UC_alert.ascx.cs" Inherits="HP_EYE.UC_alert" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>

<style type="text/css">


.txtboxTemp5
{
    background-repeat: repeat-x;
    border: 3px solid #5B9CFD;
    color: #333333;
    padding: 3px;
    margin: 5px;
    border-radius: 5px;
    font-family: 'hp Simplified','GT Walsheim Pro Light','Arial';
    background-color: #EFFCFB;
}

</style>

<cc1:TabContainer ID= "TC1" runat="server" ActiveTabIndex="5" 

    VerticalStripWidth="500px" style="margin-right: 608px" Width="100%" 
    AutoPostBack="True" onactivetabchanged="TC1_ActiveTabChanged" 
    >
    <cc1:TabPanel runat="server" HeaderText="Inactive Jobs on the Schedule" 
        ID="tpIactiveJobs">
        <HeaderTemplate>
            Inactive Jobs on the Schedule
        </HeaderTemplate>
        <ContentTemplate>
            <asp:Label ID="Label1" runat="server" Text="Out-of-date Jobs on the Schedule:" CssClass="infolabel"></asp:Label><br />
            <asp:GridView ID="gvInactJobSchd" runat="server" AutoGenerateColumns="False" CellPadding="4"
                DataKeyNames="JOB_ID" ForeColor="#333333" GridLines="None" CssClass="txtboxTemp1">
                <AlternatingRowStyle BackColor="White" />
                <Columns>
                    <asp:BoundField DataField="JOB_ID" HeaderText="JOB_ID" Visible="False" />
                    <asp:HyperLinkField DataNavigateUrlFields="JOB_ID" DataNavigateUrlFormatString="~/JobDetails.aspx?Jid={0}"
                        DataTextField="JOB_NAME" HeaderText="Job Name" Target="_blank" />
                    <asp:BoundField DataField="OPS_JOB_NAME" HeaderText="Ops. Job Name" />
                    <asp:BoundField DataField="SCHEDULE_DATE" HeaderText="Schedule Date" />
                    <asp:BoundField DataField="START_SCHD_TIME" HeaderText="Start Time" />
                    <asp:BoundField DataField="END_SCHD_TIME" HeaderText="End Time" />
                    <asp:HyperLinkField HeaderText="Scheduled by" DataNavigateUrlFormatString="~/UserDetails.aspx?un={0}"
                                    DataNavigateUrlFields="SCHEDULED_BY" Target="_blank" DataTextField="SCHEDULED_BY" />
                    <asp:BoundField DataField="JOB_STATUS" HeaderText="Job Status" />
                    <asp:BoundField DataField="CPU_NAME" HeaderText="Processor" />
                </Columns>
                <EditRowStyle BackColor="#7C6F57" />
                <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
                <RowStyle BackColor="#E3EAEB" />
                <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
                <SortedAscendingCellStyle BackColor="#F8FAFA" />
                <SortedAscendingHeaderStyle BackColor="#246B61" />
                <SortedDescendingCellStyle BackColor="#D4DFE1" />
                <SortedDescendingHeaderStyle BackColor="#15524A" />
            </asp:GridView>
        </ContentTemplate>
    </cc1:TabPanel>
    <cc1:TabPanel ID="tpWarningMessages" runat="server" HeaderText="Warning Messages" TabIndex="1">
        <ContentTemplate>

            <asp:Label ID="Label2" runat="server" Text="Morning-After Warning Messages:" CssClass="infolabel"></asp:Label>
            <asp:Label ID="lblWarnningsNumber" runat="server" CssClass="ErrorLable"></asp:Label>
            <br />
            <br />
             <asp:Button ID="btn_ACK" runat="server" Text="Acknowledged" 
                CssClass="buttonTemp1" onclick="btn_ACK_Click" Visible="False" />
            <br />
            <br />
            <asp:GridView ID="gvNote" runat="server" AutoGenerateColumns="False" 
                CellPadding="4" CssClass="txtboxTemp1" ForeColor="#333333" 
                GridLines="None" DataKeyNames="JOB_ID,NOTE_ID" 
                onrowdatabound="gvNote_RowDataBound">
                <AlternatingRowStyle BackColor="White" />
                <Columns>
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:CheckBox ID="chACK" runat="server"  />
                        </ItemTemplate>
                    </asp:TemplateField>
                <asp:HyperLinkField DataNavigateUrlFields="JOB_ID" 
                                                        DataNavigateUrlFormatString="~/JobDetails.aspx?Jid={0}" 
                                                        DataTextField="JOB_NAME" HeaderText="Job Name" Target="_blank" />
                <asp:BoundField DataField="OPS_JOB_NAME" HeaderText="Ops. Job Name" />
                    <asp:BoundField DataField="SCHEDULE_NOTE" HeaderText="Note" />
                 
                    <asp:HyperLinkField HeaderText="From" DataNavigateUrlFormatString="~/UserDetails.aspx?un={0}"
                                    DataNavigateUrlFields="OPERATOR" Target="_blank" DataTextField="OPERATOR" />
                    <asp:BoundField DataField="SENDTO" HeaderText="To" />
                    <asp:BoundField DataField="NOTE_DATE" HeaderText="Date Time" />
                    <asp:BoundField DataField="IS_ACKNOWLEDGE" HeaderText="Acknowledged" />
                    <asp:HyperLinkField HeaderText="Acknowledge by" DataNavigateUrlFormatString="~/UserDetails.aspx?un={0}"
                                    DataNavigateUrlFields="ACKNOWLEDGE_BY" Target="_blank" DataTextField="ACKNOWLEDGE_BY" />
                                    <asp:BoundField DataField="CPU_NAME" HeaderText="Processor" />
                </Columns>
                <EditRowStyle BackColor="#7C6F57" />
                <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                <HeaderStyle BackColor="#999999" ForeColor="White" />
                <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
                <RowStyle BackColor="#E3EAEB" />
                <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
                <SortedAscendingCellStyle BackColor="#F8FAFA" />
                <SortedAscendingHeaderStyle BackColor="#246B61" />
                <SortedDescendingCellStyle BackColor="#D4DFE1" />
                <SortedDescendingHeaderStyle BackColor="#15524A" />
            </asp:GridView>
            <br />
            <asp:Label ID="lblErrAutoMsg" Visible = "False" runat="server" 
               Text="Nothing to display!"></asp:Label>
            
           
            
            </ContentTemplate>
    </cc1:TabPanel>
    <cc1:TabPanel ID="tpInbox" runat="server" HeaderText="Inbox">
     <ContentTemplate>

            <asp:Label ID="Label3" runat="server" Text="Messages based on your roles:" CssClass="infolabel"></asp:Label>
            <asp:Label ID="lblInboxNumber" runat="server" CssClass="ErrorLable"></asp:Label>
            <br />
            <br />
            <asp:Button ID="btnACKRole" runat="server" Text="Acknowledged" 
                CssClass="buttonTemp3"  Visible="False" onclick="btnACKRole_Click" />
            <br />
            <br />

            <asp:GridView ID="gvNoteRole" runat="server" AutoGenerateColumns="False" 
                CellPadding="4" CssClass="txtboxTemp1" ForeColor="#333333" 
                GridLines="None" DataKeyNames="JOB_ID,NOTE_ID" 
                onrowdatabound="gvNoteRole_RowDataBound">
                <AlternatingRowStyle BackColor="White" />
                <Columns>
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:CheckBox ID="chACK" runat="server"  />
                        </ItemTemplate>
                    </asp:TemplateField>
                <asp:HyperLinkField DataNavigateUrlFields="JOB_ID" 
                                                        DataNavigateUrlFormatString="~/JobDetails.aspx?Jid={0}" 
                                                        DataTextField="JOB_NAME" HeaderText="Job Name" Target="_blank" />
                <asp:BoundField DataField="OPS_JOB_NAME" HeaderText="Ops. Job Name" />
                    <asp:BoundField DataField="SCHEDULE_NOTE" HeaderText="Note" />
                    <asp:HyperLinkField HeaderText="From" DataNavigateUrlFormatString="~/UserDetails.aspx?un={0}"
                                    DataNavigateUrlFields="OPERATOR" Target="_blank" DataTextField="OPERATOR" />
                    <asp:BoundField DataField="SENDTO" HeaderText="To" />
                    <asp:BoundField DataField="NOTE_DATE" HeaderText="Date Time" />
                    <asp:BoundField DataField="IS_ACKNOWLEDGE" HeaderText="Acknowledged" />
                    <asp:HyperLinkField HeaderText="Acknowledge by" DataNavigateUrlFormatString="~/UserDetails.aspx?un={0}"
                                    DataNavigateUrlFields="ACKNOWLEDGE_BY" Target="_blank" DataTextField="ACKNOWLEDGE_BY" />
                    <asp:BoundField DataField="CPU_NAME" HeaderText="Processor" />                    
                </Columns>
                <EditRowStyle BackColor="#7C6F57" />
                <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                <HeaderStyle BackColor="#A586AC" ForeColor="White" />
                <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
                <RowStyle BackColor="#E3EAEB" />
                <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
                <SortedAscendingCellStyle BackColor="#F8FAFA" />
                <SortedAscendingHeaderStyle BackColor="#246B61" />
                <SortedDescendingCellStyle BackColor="#D4DFE1" />
                <SortedDescendingHeaderStyle BackColor="#15524A" />
            </asp:GridView>
            
            <br />
            <asp:Label ID="lblInbox" runat="server" Visible = "False"  
                Text="Nothing to display!" ></asp:Label>
            
            
            </ContentTemplate>
    </cc1:TabPanel>
    <cc1:TabPanel ID="tpAutoEnd" runat="server" HeaderText="TabPanel4">
        <HeaderTemplate>
            Auto-End
        </HeaderTemplate>
        <ContentTemplate>
            <asp:Label ID="Label4" runat="server" CssClass="infolabel" 
                Text="Auto_ended Job List:"></asp:Label>
            <asp:Label ID="lblEndJobCount" runat="server" CssClass="ErrorLable"></asp:Label>
            <br />
            <br />
            <asp:GridView ID="gvNoteEndJob" runat="server" AutoGenerateColumns="False" 
                CellPadding="4" CssClass="txtboxTemp1" DataKeyNames="JOB_ID,NOTE_ID" 
                ForeColor="#333333" GridLines="None">
                <AlternatingRowStyle BackColor="White" />
                <Columns>
                    <asp:HyperLinkField DataNavigateUrlFields="JOB_ID" 
                        DataNavigateUrlFormatString="~/JobDetails.aspx?Jid={0}" 
                        DataTextField="JOB_NAME" HeaderText="Job Name" Target="_blank" />
                    <asp:BoundField DataField="OPS_JOB_NAME" HeaderText="Ops. Job Name" />
                    <asp:BoundField DataField="SCHEDULE_NOTE" HeaderText="Note" />
                    <asp:BoundField DataField="OPERATOR" HeaderText="Processor" />
                </Columns>
                <EditRowStyle BackColor="#7C6F57" />
                <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                <HeaderStyle BackColor="#1C5E55" ForeColor="White" Font-Bold="True" />
                <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
                <RowStyle BackColor="#E3EAEB" />
                <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
                <SortedAscendingCellStyle BackColor="#F8FAFA" />
                <SortedAscendingHeaderStyle BackColor="#246B61" />
                <SortedDescendingCellStyle BackColor="#D4DFE1" />
                <SortedDescendingHeaderStyle BackColor="#15524A" />
            </asp:GridView>
            <br />
            <asp:Label ID="lblendjob" runat="server" Text="Nothing to display!" 
                Visible="False"></asp:Label>
        </ContentTemplate>
    </cc1:TabPanel>
    <cc1:TabPanel ID="TabPanel1" runat="server" HeaderText="TabPanel1">
        <HeaderTemplate>
            Time manually entered
        </HeaderTemplate>
        <ContentTemplate>
            <asp:Label ID="ManuallyJobCount" runat="server" CssClass="infolabel" Text=" Count:
            "></asp:Label>
            <asp:Label ID="lblManuallyJobCount" runat="server" CssClass="ErrorLable"></asp:Label>
            <br />
            <br />
            <asp:GridView ID="gvNoteManually" runat="server" AutoGenerateColumns="False" 
                CellPadding="3" CssClass="txtboxTemp1" DataKeyNames="JOB_ID,NOTE_ID" 
                BackColor="White" BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px">
                <Columns>
                    <asp:HyperLinkField DataNavigateUrlFields="JOB_ID" 
                        DataNavigateUrlFormatString="~/JobDetails.aspx?Jid={0}" 
                        DataTextField="JOB_NAME" HeaderText="Job Name" Target="_blank" />
                    <asp:BoundField DataField="OPS_JOB_NAME" HeaderText="Ops. Job Name" />
                    <asp:BoundField DataField="SCHEDULE_NOTE" HeaderText="Action" />
                    <asp:HyperLinkField HeaderText="Operator" DataNavigateUrlFormatString="~/UserDetails.aspx?un={0}"
                                    DataNavigateUrlFields="OPERATOR" Target="_blank" DataTextField="OPERATOR" />
                    <asp:BoundField DataField="CPU_NAME" HeaderText="Processor" />
                </Columns>
                <FooterStyle BackColor="White" ForeColor="#000066" />
                <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                <PagerStyle BackColor="White" ForeColor="#000066" HorizontalAlign="Left" />
                <RowStyle ForeColor="#000066" />
                <SelectedRowStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
                <SortedAscendingCellStyle BackColor="#F1F1F1" />
                <SortedAscendingHeaderStyle BackColor="#007DBB" />
                <SortedDescendingCellStyle BackColor="#CAC9C9" />
                <SortedDescendingHeaderStyle BackColor="#00547E" />
            </asp:GridView>
            <br />
            <asp:Label ID="lblmanuallyjob" runat="server" Text="Nothing to display!" 
                Visible="False"></asp:Label>
        </ContentTemplate>
    </cc1:TabPanel>
    <cc1:TabPanel ID="TabPanel2" runat="server" HeaderText="TabPanel2">
        <HeaderTemplate>
            Runtape
        </HeaderTemplate>
        <ContentTemplate>
            <asp:DropDownList ID="drSystemName" runat="server" AutoPostBack="True" 
                CssClass="txtboxTemp5" ToolTip="Select a system to see the Runtape" 
                onselectedindexchanged="drSystemName_SelectedIndexChanged">
            </asp:DropDownList>
            <br />
           <asp:Label ID="RuntapeJobCount" runat="server" CssClass="infolabel" Text=" Count:
            "></asp:Label>
            <asp:Label ID="lblRuntapeJobCount" runat="server" CssClass="ErrorLable"></asp:Label>
            <br />
            <br />
            <asp:GridView ID="gvNoteRunTape" runat="server" 
                BackColor="#CCCCCC" BorderColor="#999999" BorderStyle="Solid" BorderWidth="3px" 
                CellPadding="4" CssClass="txtboxTemp1" DataKeyNames="JOB_ID,SCHEDULE_ID" 
                ForeColor="Black" CellSpacing="2" EmptyDataText="Nothing to display" 
                AutoGenerateColumns="False">
                <Columns>
                    <asp:HyperLinkField DataNavigateUrlFields="JOB_ID" 
                                                        DataNavigateUrlFormatString="~/JobDetails.aspx?Jid={0}" 
                                                        DataTextField="JOB_NAME" HeaderText="Job Name" Target="_blank" />
                     <asp:BoundField DataField="OPS_JOB_NAME" HeaderText="Job OPS Name" />
                    <asp:BoundField DataField="JOB_STATUS" HeaderText="Job Status" />
                    <asp:BoundField DataField="RECEIVED_ATDATE" HeaderText="Received At" />
                    <asp:BoundField DataField="RECEIVED_ATTIME" />
                    <asp:BoundField DataField="VOLSER" HeaderText="VOLSER" />
                </Columns>
                <FooterStyle BackColor="#CCCCCC" />
                <HeaderStyle BackColor="Black" Font-Bold="True" ForeColor="White" />
                <PagerStyle BackColor="#CCCCCC" ForeColor="Black" HorizontalAlign="Left" />
                <RowStyle BackColor="White" />
                <SelectedRowStyle BackColor="#000099" Font-Bold="True" ForeColor="White" />
                <SortedAscendingCellStyle BackColor="#F1F1F1" />
                <SortedAscendingHeaderStyle BackColor="Gray" />
                <SortedDescendingCellStyle BackColor="#CAC9C9" />
                <SortedDescendingHeaderStyle BackColor="#383838" />
            </asp:GridView>
        </ContentTemplate>
    </cc1:TabPanel>
</cc1:TabContainer>

