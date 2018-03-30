<%@ Page Title="Today Schedule" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="HP_EYE._Default"  %>

<%@ Register Src="UC_Job.ascx" TagName="UC_Job" TagPrefix="uc2" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<%@ Register src="UC_Chat.ascx" tagname="UC_Chat" tagprefix="uc1" %>
<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <%--<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
     <meta http-equiv="refresh" content="10" />--%>    <%--<asp:UpdatePanel ID="UpdatePanel1" runat="server"
         >--%>       <%-- <ContentTemplate>--%>            <%--<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
     <meta http-equiv="refresh" content="10" />--%>            <%--<asp:Timer ID="Timer1" runat="server" ontick="Timer1_Tick" Enabled="False" 
             Interval="1000">
         </asp:Timer>--%>
         
            
            <table style="width: 100%;">
                <tr>
                    <td valign="top" style="width: 250px;">
                        <table style="width:100%;">
                            <tr>
                                <td >
                                    <uc1:UC_Chat ID="UC_Chat2" runat="server" />
                                </td>
                            </tr>
                            <tr>
                                <td >
                                    <asp:Label ID="Label2" runat="server" 
                            Text="Outstanding Job Panel" CssClass="PanelTitle"></asp:Label></td>
                            </tr>
                            <tr>
                                <td>
                                <asp:Label ID="lblRuntape" runat="server" CssClass="infolabelBold" 
                                        Text="Runtape Jobs:" Visible="False"></asp:Label>
                                        <asp:GridView ID="gvRuntape" runat="server" AllowSorting="True" 
                                        AutoGenerateColumns="False" CellPadding="4" 
                                        DataKeyNames="JOB_ID,START_SCHD_TIME,JOB_STATUS,SCHEDULE_ID,SLA,IS_TIME_SENSETIVE" 
                                        ForeColor="#333333" GridLines="None">
                                        <AlternatingRowStyle BackColor="#FFF4FF" />
                                        <Columns>
                                            <asp:BoundField DataField="JOB_ID" HeaderText="JOB_ID" Visible="False" />
                                            <asp:TemplateField HeaderText="Ops. Job Name" SortExpression="OPS_JOB_NAME">
                                                <ItemTemplate>
                                                    <asp:HyperLink ID="HyperLink1" runat="server" 
                                                        NavigateUrl='<%# String.Format("~/Default.aspx?schtype=tod&schID={0}#{0}",Eval("SCHEDULE_ID")) %>' 
                                                        Text='<%# Eval("OPS_JOB_NAME") %>'></asp:HyperLink>
                                                </ItemTemplate>
                                                <ItemStyle Width="130px" />
                                            </asp:TemplateField>
                                            <asp:BoundField DataField="VOLSER" HeaderText="VOLSER" 
                                                SortExpression="VOLSER" />
                                            <asp:BoundField DataField="RECEIVED_ATDATE" HeaderText="Received At" 
                                                SortExpression="RECEIVED_AT">
                                            <ItemStyle Width="30px" />
                                            </asp:BoundField>
                                            <asp:BoundField DataField="RECEIVED_ATTIME" HeaderText="" />
                                            <asp:BoundField DataField="JOB_STATUS" HeaderText="Job Status" 
                                                Visible="False" />
                                        </Columns>
                                        <EditRowStyle BackColor="#7C6F57" />
                                        <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                                        <HeaderStyle BackColor="#CC66FF" Font-Bold="True" ForeColor="White" />
                                        <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
                                        <RowStyle BackColor="#E9D8EB" />
                                        <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
                                        <SortedAscendingCellStyle BackColor="#F8FAFA" />
                                        <SortedAscendingHeaderStyle BackColor="#246B61" />
                                        <SortedDescendingCellStyle BackColor="#D4DFE1" />
                                        <SortedDescendingHeaderStyle BackColor="#15524A" />
                                    </asp:GridView>
                                      <br />
                                    <asp:Label ID="lblOnHoldJobs" runat="server" CssClass="infolabelBold" 
                                        Text="On-Hold Jobs:"></asp:Label>
                                        <asp:GridView ID="gvHoldJobList" runat="server" AllowSorting="True" 
                                        AutoGenerateColumns="False" CellPadding="4" 
                                        DataKeyNames="JOB_ID,START_SCHD_TIME,JOB_STATUS,SCHEDULE_ID,SLA,IS_PAUSABLE,IS_TIME_SENSETIVE,NUMBEROFTAPES,HAS_INPUT,IS_TFT,IS_STANDALONE,JOB_TIME_ID" 
                                        ForeColor="#333333" GridLines="None">
                                        <AlternatingRowStyle BackColor="#FFF4EA" />
                                        <Columns>
                                            <asp:BoundField DataField="JOB_ID" HeaderText="JOB_ID" Visible="False" />
                                            <asp:TemplateField HeaderText="Ops. Job Name" SortExpression="OPS_JOB_NAME">
                                                <ItemTemplate>
                                                    <asp:HyperLink ID="HyperLink1" runat="server" 
                                                        NavigateUrl='<%# String.Format("~/Default.aspx?schtype=tod&schID={0}#{0}",Eval("SCHEDULE_ID")) %>' 
                                                        Text='<%# Eval("OPS_JOB_NAME") %>'></asp:HyperLink>
                                                </ItemTemplate>
                                                <ItemStyle Width="130px" />
                                            </asp:TemplateField>
                                            <asp:BoundField DataField="START_SCHD_TIME" HeaderText="Schd Start" 
                                                SortExpression="START_SCHD_TIME">
                                            <ItemStyle Width="30px" />
                                            </asp:BoundField>
                                            <asp:BoundField DataField="JOB_STATUS" HeaderText="Job Status" 
                                                Visible="False" />
                                        </Columns>
                                        <EditRowStyle BackColor="#7C6F57" />
                                        <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                                        <HeaderStyle BackColor="#FF9933" Font-Bold="True" ForeColor="White" />
                                        <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
                                        <RowStyle BackColor="#FFE7CE" />
                                        <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
                                        <SortedAscendingCellStyle BackColor="#F8FAFA" />
                                        <SortedAscendingHeaderStyle BackColor="#246B61" />
                                        <SortedDescendingCellStyle BackColor="#D4DFE1" />
                                        <SortedDescendingHeaderStyle BackColor="#15524A" />
                                    </asp:GridView>
                                    <br />
                                    &nbsp;<asp:Label ID="lblyesterdaysList" runat="server" CssClass="infolabelBold" 
                                        Text="Ourtstanding jobs of Days ago:"></asp:Label>
                                    <asp:GridView ID="gvYesterdaysJobList" runat="server" AllowSorting="True" 
                                        AutoGenerateColumns="False" CellPadding="4" 
                                        DataKeyNames="JOB_ID,START_SCHD_TIME,JOB_STATUS,SCHEDULE_ID,SLA,IS_PAUSABLE,IS_TIME_SENSETIVE,NUMBEROFTAPES,HAS_INPUT,IS_TFT,IS_STANDALONE,JOB_TIME_ID" 
                                        ForeColor="#333333" GridLines="None" 
                                        OnRowDataBound="gvYesterdaysJobList_RowDataBound" 
                                        onsorting="gvYesterdaysJobList_Sorting">
                                        <AlternatingRowStyle BackColor="#FFF4EA" />
                                        <Columns>
                                            <asp:BoundField DataField="JOB_ID" HeaderText="JOB_ID" Visible="False" />
                                            <asp:TemplateField HeaderText="Ops. Job Name" SortExpression="OPS_JOB_NAME">
                                                <ItemTemplate>
                                                    <asp:HyperLink runat="server" 
                                                        NavigateUrl='<%# String.Format("~/Default.aspx?schtype=outsch&schID={0}#{0}",Eval("SCHEDULE_ID")) %>' 
                                                        Text='<%# Eval("OPS_JOB_NAME") %>'></asp:HyperLink>
                                                </ItemTemplate>
                                                <ItemStyle Width="100px" />
                                            </asp:TemplateField>
                                            <asp:BoundField DataField="START_SCHD_TIME" HeaderText="Schd Start" 
                                                SortExpression="START_SCHD_TIME">
                                            <ItemStyle Width="30px" />
                                            </asp:BoundField>
                                            <asp:BoundField DataField="JOB_STATUS" HeaderText="Job Status" 
                                                Visible="False" />
                                            <asp:TemplateField>
                                                <ItemTemplate>
                                                    <asp:Image ID="Stand" runat="server" Height="20px" 
                                                        ImageUrl="~/Styles/stand3.png" ToolTip="Should run alone" />
                                                    <asp:Image ID="SLA" runat="server" Height="20px" ImageUrl="~/Styles/SLA54.png" 
                                                        ToolTip="SLA" />
                                                    <asp:Image ID="Pause" runat="server" ImageUrl="~/Styles/PauseIfIncom2.png" 
                                                        ToolTip="Pause if incompatible" Width="20px" />
                                                    <asp:Image ID="Rush" runat="server" ImageUrl="~/Styles/c0.gif" 
                                                        ToolTip="Time to Run" Width="20px" />
                                                    <asp:Image ID="OnTime" runat="server" ImageUrl="~/Styles/0.gif" 
                                                        ToolTip="Run at Scheduled time" Width="20px" />
                                                    <asp:Image ID="Chain" runat="server" ImageUrl="~/Styles/Chain1.png" 
                                                        ToolTip="Chained Job" Width="20px" />
                                                    <asp:Image ID="ShakyChain" runat="server" Height="20px" 
                                                        ImageUrl="~/Styles/s0.gif" ToolTip="Run Chained Job" />
                                                    <asp:Image ID="Input" runat="server" ImageUrl="~/Styles/tape_in3.png" 
                                                        ToolTip="Input Tape" Width="20px" />
                                                    <asp:Image ID="Output" runat="server" ImageUrl="~/Styles/tape_out3.png" 
                                                        ToolTip="Output Tape" Width="20px" />
                                                    <asp:Image ID="TimeFunction" runat="server" ImageUrl="~/Styles/TimeInit4.png" 
                                                        ToolTip="Time initiated job" Width="20px" />
                                                </ItemTemplate>
                                                <ItemStyle Width="120px" />
                                            </asp:TemplateField>
                                        </Columns>
                                        <EditRowStyle BackColor="#7C6F57" />
                                        <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                                        <HeaderStyle BackColor="#DD5C4A" Font-Bold="True" ForeColor="White" />
                                        <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
                                        <RowStyle BackColor="#FFE7CE" />
                                        <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
                                        <SortedAscendingCellStyle BackColor="#F8FAFA" />
                                        <SortedAscendingHeaderStyle BackColor="#246B61" />
                                        <SortedDescendingCellStyle BackColor="#D4DFE1" />
                                        <SortedDescendingHeaderStyle BackColor="#15524A" />
                                    </asp:GridView>
                                    <br />
                                    <asp:Label ID="lblTodayList" runat="server" CssClass="infolabelBold" 
                                        Text="Ourtstanding jobs of Today:"></asp:Label>
                                    <asp:GridView ID="gvTodayJobList" runat="server" AllowSorting="True" 
                                        AutoGenerateColumns="False" CellPadding="4" 
                                        DataKeyNames="JOB_ID,START_SCHD_TIME,JOB_STATUS,SCHEDULE_ID,SLA,IS_PAUSABLE,IS_TIME_SENSETIVE,NUMBEROFTAPES,HAS_INPUT,IS_TFT,IS_STANDALONE,JOB_TIME_ID" 
                                        ForeColor="#333333" GridLines="None" 
                                        OnRowDataBound="gvTodayJobList_RowDataBound" onsorting="gvTodayJobList_Sorting">
                                        <AlternatingRowStyle BackColor="WhiteSmoke" />
                                        <Columns>
                                            <asp:BoundField DataField="JOB_ID" HeaderText="JOB_ID" Visible="False" />
                                            <asp:TemplateField HeaderText="Ops. Job Name" SortExpression="OPS_JOB_NAME">
                                                <ItemTemplate>
                                                    <asp:HyperLink runat="server" 
                                                        NavigateUrl='<%# String.Format("~/Default.aspx?schtype=todsch&schID={0}#{0}",Eval("SCHEDULE_ID")) %>' 
                                                        Text='<%# Eval("OPS_JOB_NAME") %>'></asp:HyperLink>
                                                </ItemTemplate>
                                                <ItemStyle Width="100px" />
                                            </asp:TemplateField>
                                            <asp:BoundField DataField="START_SCHD_TIME" HeaderText="Schd Start" 
                                                SortExpression="START_SCHD_TIME">
                                            <ItemStyle Width="30px" />
                                            </asp:BoundField>
                                            <asp:BoundField DataField="JOB_STATUS" HeaderText="Job Status" 
                                                Visible="False" />
                                            <asp:TemplateField>
                                                <ItemTemplate>
                                                    <asp:Image ID="Stand" runat="server" Height="20px" 
                                                        ImageUrl="~/Styles/stand3.png" ToolTip="Should run alone" />
                                                    <asp:Image ID="SLA" runat="server" Height="20px" ImageUrl="~/Styles/SLA54.png" 
                                                        ToolTip="SLA" />
                                                    <asp:Image ID="Pause" runat="server" ImageUrl="~/Styles/PauseIfIncom2.png" 
                                                        ToolTip="Pause if incompatible" Width="20px" />
                                                    <asp:Image ID="Rush" runat="server" ImageUrl="~/Styles/c0.gif" 
                                                        ToolTip="Time to Run" Width="20px" />
                                                    <asp:Image ID="OnTime" runat="server" ImageUrl="~/Styles/0.gif" 
                                                        ToolTip="Run at Scheduled time" Width="20px" />
                                                    <asp:Image ID="Chain" runat="server" ImageUrl="~/Styles/Chain1.png" 
                                                        ToolTip="Chained Job" Width="20px" />
                                                    <asp:Image ID="ShakyChain" runat="server" Height="20px" 
                                                        ImageUrl="~/Styles/s0.gif" ToolTip="Run Chained Job" />
                                                    <asp:Image ID="Input" runat="server" ImageUrl="~/Styles/tape_in3.png" 
                                                        ToolTip="Input Tape" Width="20px" />
                                                    <asp:Image ID="InputChecked" runat="server" ImageUrl="~/Styles/tape_in35.png" 
                                                        ToolTip="Input Tape Mounted" Width="20px" />
                                                    <asp:Image ID="Output" runat="server" ImageUrl="~/Styles/tape_out3.png" 
                                                        ToolTip="Output Tape" Width="20px" />
                                                    <asp:Image ID="OutputChecked" runat="server" ImageUrl="~/Styles/tape_out35.png" 
                                                        ToolTip="Output Tape Mounted" Width="20px" />
                                                    <asp:Image ID="TimeFunction" runat="server" ImageUrl="~/Styles/TimeInit4.png" 
                                                        ToolTip="Time initiated job" Width="20px" />
                                                </ItemTemplate>
                                                <ItemStyle Width="120px" />
                                            </asp:TemplateField>
                                        </Columns>
                                        <EditRowStyle BackColor="#7C6F57" />
                                        <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                                        <HeaderStyle BackColor="#809FA4" Font-Bold="True" ForeColor="White" />
                                        <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
                                        <RowStyle BackColor="#E3EAEB" />
                                        <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
                                        <SortedAscendingCellStyle BackColor="#F8FAFA" />
                                        <SortedAscendingHeaderStyle BackColor="#246B61" />
                                        <SortedDescendingCellStyle BackColor="#D4DFE1" />
                                        <SortedDescendingHeaderStyle BackColor="#15524A" />
                                    </asp:GridView>
                                    <br />
                                    <asp:Label ID="lblTomorrowList" runat="server" CssClass="infolabelBold" 
                                        Text="Ourtstanding jobs of Tomorrow:"></asp:Label>
                                    <asp:GridView ID="gvTomorrowJobList" runat="server" AllowSorting="True" 
                                        AutoGenerateColumns="False" CellPadding="4" 
                                        DataKeyNames="JOB_ID,START_SCHD_TIME,JOB_STATUS,SCHEDULE_ID,SLA,IS_PAUSABLE,IS_TIME_SENSETIVE,NUMBEROFTAPES,HAS_INPUT,IS_TFT,IS_STANDALONE,JOB_TIME_ID" 
                                        ForeColor="#333333" GridLines="None" 
                                        OnRowDataBound="gvTomorrowJobList_RowDataBound" 
                                        onsorting="gvTomorrowJobList_Sorting">
                                        <AlternatingRowStyle BackColor="#ECF4EA" />
                                        <Columns>
                                            <asp:BoundField DataField="JOB_ID" HeaderText="JOB_ID" Visible="False" />
                                            <asp:TemplateField HeaderText="Ops. Job Name" SortExpression="OPS_JOB_NAME">
                                                <ItemTemplate>
                                                    <asp:HyperLink runat="server" 
                                                        NavigateUrl='<%# String.Format("~/Default.aspx?schtype=tomsch&schID={0}#{0}",Eval("SCHEDULE_ID")) %>' 
                                                        Text='<%# Eval("OPS_JOB_NAME") %>'></asp:HyperLink>
                                                </ItemTemplate>
                                                <ItemStyle Width="100px" />
                                            </asp:TemplateField>
                                            <asp:BoundField DataField="START_SCHD_TIME" HeaderText="Schd Start" 
                                                SortExpression="START_SCHD_TIME">
                                            <ItemStyle Width="30px" />
                                            </asp:BoundField>
                                            <asp:BoundField DataField="JOB_STATUS" HeaderText="Job Status" 
                                                Visible="False" />
                                            <asp:TemplateField>
                                                <ItemTemplate>
                                                    <asp:Image ID="Rush" runat="server" ImageUrl="~/Styles/c0.gif" 
                                                        ToolTip="Time to Run" Width="20px" />
                                                    <asp:Image ID="OnTime" runat="server" ImageUrl="~/Styles/0.gif" 
                                                        ToolTip="Run at Scheduled time" Width="20px" />
                                                    <asp:Image ID="Chain" runat="server" ImageUrl="~/Styles/Chain1.png" 
                                                        ToolTip="Chained Job" Width="20px" />
                                                    <asp:Image ID="ShakyChain" runat="server" Height="20px" 
                                                        ImageUrl="~/Styles/s0.gif" ToolTip="Run Chained Job" />
                                                    <asp:Image ID="SLA" runat="server" Height="20px" ImageUrl="~/Styles/SLA54.png" 
                                                        ToolTip="SLA" />
                                                    <asp:Image ID="Input" runat="server" ImageUrl="~/Styles/tape_in3.png" 
                                                        ToolTip="Input Tape" Width="20px" />
                                                    <asp:Image ID="Output" runat="server" ImageUrl="~/Styles/tape_out3.png" 
                                                        ToolTip="Output Tape" Width="20px" />
                                                    <asp:Image ID="Stand" runat="server" Height="20px" 
                                                        ImageUrl="~/Styles/stand3.png" ToolTip="Should run alone" />
                                                    <asp:Image ID="TimeFunction" runat="server" ImageUrl="~/Styles/TimeInit4.png" 
                                                        ToolTip="Time initiated job" Width="20px" />
                                                    <asp:Image ID="Pause" runat="server" ImageUrl="~/Styles/PauseIfIncom2.png" 
                                                        ToolTip="Pause if incompatible" Width="20px" />
                                                </ItemTemplate>
                                                <ItemStyle Width="120px" />
                                            </asp:TemplateField>
                                        </Columns>
                                        <EditRowStyle BackColor="#7C6F57" />
                                        <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                                        <HeaderStyle BackColor="#85A77C" Font-Bold="True" ForeColor="White" />
                                        <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
                                        <RowStyle BackColor="#CCE1C6" />
                                        <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
                                        <SortedAscendingCellStyle BackColor="#F8FAFA" />
                                        <SortedAscendingHeaderStyle BackColor="#246B61" />
                                        <SortedDescendingCellStyle BackColor="#D4DFE1" />
                                        <SortedDescendingHeaderStyle BackColor="#15524A" />
                                    </asp:GridView>
                                </td>
                            </tr>
                        </table></td>
                    <td >&nbsp;</td>
                    <td valign="top">
                        <table style="width:100%;">
                            <tr>
                                <td valign="top">
                                    <asp:DropDownList ID="drSystemName" runat="server" AutoPostBack="True" 
                                        CssClass="txtboxTemp5" 
                                        OnSelectedIndexChanged="drSystemName_SelectedIndexChanged" 
                                        ToolTip="Select a system to see the schedules">
                                    </asp:DropDownList>
                                </td>
                                <td valign="top" align="left">
                                    <asp:CheckBoxList ID="chblCPU" runat="server" AutoPostBack="True" 
                                        CssClass="txtboxTemp5" Enabled="False" 
                                        onselectedindexchanged="chblCPU_SelectedIndexChanged" RepeatColumns="4">
                                    </asp:CheckBoxList>
                                </td>
                                <td valign="top">
                                    <asp:CheckBox ID="chOperator" runat="server" AutoPostBack="True" 
                                        oncheckedchanged="chOperator_CheckedChanged" 
                                        Text="I want JSE to update job's Start/Tape information from TOS log" />
                                        <br/>
                                    <asp:CheckBox ID="chOperatorEndTime" runat="server" AutoPostBack="True" 
                                        oncheckedchanged="chOperatorEndTime_CheckedChanged" 
                                        Text="I want JSE to update job's End time from TOS log" />
                                    <br/>
                                    <asp:CheckBox ID="chHideEndedJobs" runat="server" AutoPostBack="True" 
                                        CssClass="infolabelBold" oncheckedchanged="chHideEndedJobs_CheckedChanged" 
                                        Text="Hide Ended Jobs " />
                                </td>
                                <td align="right" valign="top">
                                    <table class="PageNavigator">
                                        <tr>
                                            <td>
                                                <asp:Button ID="btnPrev" runat="server" CssClass="buttonTemp3" 
                                                    onclick="btnPrev_Click" Text="&lt;&lt;" />
                                                &nbsp;<asp:Label ID="lblPage0" runat="server" CssClass="infolabel" 
                                                    Text="page No. "></asp:Label>
                                                <asp:DropDownList ID="drPage" runat="server" AutoPostBack="True" 
                                                    CssClass="txtboxTemp6" OnSelectedIndexChanged="drPage_SelectedIndexChanged" 
                                                    ToolTip="Select a page from here">
                                                </asp:DropDownList>
                                                <asp:Button ID="btnNext" runat="server" CssClass="buttonTemp3" 
                                                    onclick="btnNext_Click" Text="&gt;&gt;" />
                                            </td>
                                            
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="4" valign="top">
                                    <table style="width: 100%;">
                                        <tr>
                                            <td>
                                               <asp:Panel ID="pnlPrint" runat="server">
                                        <asp:PlaceHolder ID="phJob" runat="server"></asp:PlaceHolder>
                                    </asp:Panel>
                                            </td>
                                            <td>
                                            </td>
                                            <td valign="top" align="right" width="30px">
                                        <asp:GridView ID="gvAuto_ENDJobList" runat="server" AllowSorting="True" 
                                        AutoGenerateColumns="False" CellPadding="4" 
                                        DataKeyNames="END_TIME,JOB_STATUS,SCHEDULE_ID,JOB_TIME_ID,START_SCHD_TIME" 
                                        ForeColor="#333333" GridLines="None" Caption="<b>Last hour </br> Ended Jobs</b>">
                                        <AlternatingRowStyle BackColor="#DADADA" />
                                        <Columns>
                                            <asp:BoundField DataField="JOB_ID" HeaderText="JOB_ID" Visible="False" />
                                            <asp:TemplateField HeaderText="Job" SortExpression="OPS_JOB_NAME">
                                                <ItemTemplate>
                                                    <asp:HyperLink ID="HyperLink2" runat="server" 
                                                        NavigateUrl='<%# String.Format("~/Default.aspx?schtype=tod&schID={0}#{0}",Eval("SCHEDULE_ID")) %>' 
                                                        Text='<%# Eval("OPS_JOB_NAME") %>'></asp:HyperLink>
                                                </ItemTemplate>
                                                <ItemStyle Width="80px" />
                                            </asp:TemplateField>
                                            <asp:BoundField DataField="START_SCHD_TIME" HeaderText="Schd Start" 
                                                SortExpression="START_SCHD_TIME" Visible="False">
                                            </asp:BoundField>
                                            <asp:BoundField DataField="JOB_STATUS" HeaderText="Job Status" 
                                                Visible="False" />
                                        </Columns>
                                        <EditRowStyle BackColor="#7C6F57" />
                                        <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                                        <HeaderStyle BackColor="#666666" Font-Bold="True" ForeColor="White" />
                                        <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
                                        <RowStyle BackColor="Silver" />
                                        <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
                                        <SortedAscendingCellStyle BackColor="#F8FAFA" />
                                        <SortedAscendingHeaderStyle BackColor="#246B61" />
                                        <SortedDescendingCellStyle BackColor="#D4DFE1" />
                                        <SortedDescendingHeaderStyle BackColor="#15524A" />
                                    </asp:GridView>
                                            </td>
                                        </tr>
                                    </table>
                                                
                                           
                                    
                                </td>
                               
                            </tr>
                        </table>
                    </td>
                </tr>
               
                        </table>
                    </td>
                </tr>
                
               
            </table>
            <asp:Label ID="lblUser" runat="server"></asp:Label>
            <asp:HiddenField ID="ListOfOpens" runat="server" ClientIDMode="Static" />
    <%-- </ContentTemplate>--%>    <%--</asp:UpdatePanel>--%>
</asp:Content>
