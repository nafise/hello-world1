<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UC_Job.ascx.cs" Inherits="HP_EYE.UC_Job" %>
<%@ Register Src="UC_command.ascx" TagName="UC_command" TagPrefix="uc1" %>
<%@ Register Src="UC_tape.ascx" TagName="UC_tape" TagPrefix="uc2" %>
<%@ Register Assembly="HP-EYE" Namespace="HP_EYE" TagPrefix="cc1" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<link href="~/Styles/Site.css" rel="stylesheet" type="text/css" />
<table style="height: 20px">
    <tr>
        <td>
            <asp:HiddenField ID="UserControlHiddenScheduleID" runat="server" />
        </td>
        <td>
            <a id="TopOfThisJob" runat="server" name="top"></a>
        </td>
    </tr>
</table>
<asp:Panel ID="Panel1" runat="server" class="topborder" Font-Names="HP Simplified,GT Walsheim Pro Light,Arial">
    <table style="border-spacing: 0px;">
        <tr>
            <td id="lpc" runat="server" rowspan="2">
            </td>
            <td class="righttopborderBold">
                TPF Name:
            </td>
            <td class="righttopborderBold" colspan="2">
                Ops. Job Name:
            </td>
            <td class="righttopborderBold">
                <asp:Label ID="lbJobDoc" runat="server" Text="Documents:"></asp:Label>&nbsp;
                </td>
            <td class="righttopborderBoldMedium">
                Duration:
                </td>
            <td class="righttopborderBoldsmall">
                CPU:&nbsp;
            </td>
            <td class="righttopborderBoldbig">
                Description:
            </td>
            <td class="righttopborderBold">
                <asp:Label ID="lbJob_Status" runat="server" Text="Scheduled"></asp:Label>
            </td>
            <td rowspan="3">
                <%--<asp:Label ID="lbSLA" runat="server" Text="SLA" Visible="false"></asp:Label>--%>
                <asp:Image ID="ImSLA" runat="server" ImageUrl="~/Styles/SLA54.png" ToolTip="SLA"
                    Height="28px" />
                <asp:Image ID="ChainImage" runat="server" ImageUrl="~/Styles/Chain1.png" ToolTip="Chained job"
                    Height="28px" />
                <%--<asp:Label ID="lbIS_Pausable_Incompatibility" runat="server" Text="Pausable if Incompatible" Visible="false"></asp:Label>--%>
                <asp:Image ID="Im_Pausable_Incompatibility" runat="server" Height="28px" ImageUrl="~/Styles/PauseIfIncom2.png"
                    ToolTip="Pause this job if you wanna run another incompatible with this." />
                <asp:Image ID="Im_TFT" runat="server" Height="28px" ImageUrl="~/Styles/timeInit4.png"
                    ToolTip="Time Initiated Function" />
                <asp:Image ID="Im_Runatschdtime" runat="server" Height="28px" ImageUrl="~/Styles/0.gif"
                    ToolTip="Run this job at the Scheduled Time" />
                <asp:Image ID="Im_Notes" runat="server" Height="28px" ImageUrl="~/Styles/note.png"
                    ToolTip="This job has Notes" />
                <asp:Image ID="ImCopyManager" runat="server" Height="28px" 
                    ImageUrl="~/Styles/CopyManager.png" ToolTip="FOS copy" />
                <asp:Image ID="imRunSheet" runat="server" Height="28px" 
                    ImageUrl="~/Styles/Runsheet.png" ToolTip="Runsheet" />
                <asp:Image ID="ImOnRequest" runat="server" Height="28px" 
                    ImageUrl="~/Styles/onrequest.png" ToolTip="On request" />
                <%--<asp:Label ID="lbRunatschdtime" runat="server" Text="Run at Scheduled Time" Visible="false"></asp:Label>--%>
                <asp:Image ID="Im_Standalone" runat="server" Height="28px" 
                    ImageUrl="~/Styles/Stand3.png" ToolTip="Standalone" />
            </td>
        </tr>
        <tr>
            <td class="rightbottonborder">
                <asp:Label ID="lbJob_Name" runat="server" CssClass="infolabel" Text="TPF_Name"></asp:Label>
            </td>
            <td class="rightbottonborder" colspan="2">
                <asp:Label ID="lbJob_Name_OPS" runat="server" CssClass="infolabel" Text="Operator JobName"></asp:Label>
            </td>
            <td class="rightbottonborder" rowspan="2">
                <asp:DataList ID="dlJob_Documents" runat="server" class="bulletedlistCSS" 
                    CssClass="bulletedlistCSS" RepeatColumns="1" 
                    RepeatDirection="Horizontal" 
                    onitemdatabound="dlJob_Documents_ItemDataBound1">
                    <ItemTemplate>
                        <asp:HyperLink ID="HyperLink2" runat="server" CssClass="bulletedlistCSS" 
                            NavigateUrl='<%# "~/Scheduling/TPFJobDocuments/"+Container.DataItem.ToString()+".txt"%>' 
                            Text='<%# Container.DataItem.ToString()%>'></asp:HyperLink>
                    </ItemTemplate>
                </asp:DataList>
            </td>
            <td class="rightbottonborder" rowspan="2">
                <asp:Label ID="lbDuration" runat="server" CssClass="infolabel" Text="0 Min(s)"></asp:Label>
            </td>
            <td class="rightbottonborder" rowspan="2">
                <asp:Label ID="lbCPU_ID" runat="server" CssClass="infolabel" Text="CPU-ID"></asp:Label>
            </td>
            
            <td class="rightbottonborder" rowspan="2">
                <asp:Label ID="lbJob_Description" runat="server" CssClass="infolabel" Text="Job_Description"></asp:Label>
            </td>
            <td class="rightbottonborder" rowspan="2">
                <asp:Label ID="lbJob_Started_Time" runat="server" Text="Started @" 
                    ForeColor="#000066"></asp:Label>
                <asp:Label ID="lbJob_Ended_Time" runat="server" Text="Ended @" 
                    ForeColor="#000066"></asp:Label><br />
                <asp:Label ID="lbJob_Duration" runat="server" Text="Ran - min(s)" 
                    ForeColor="#000066"></asp:Label>
            </td>
        </tr>
        <tr>
            <td id="lpc1" runat="server" align="center" colspan="2">
                <asp:Label ID="lbJob_SCHD_Start_Time" runat="server" Text="Schd_Start" ToolTip="Scheduled Start Time"></asp:Label>
                -<asp:Label ID="lbJob_SCHD_End_Time" runat="server" Text="Schd_End" ToolTip="Scheduled End Time"></asp:Label>
            </td>
            <td id="lpc2" runat="server" align="left">
                <asp:Label ID="lbDataOfSchedule" runat="server" Text="Date"></asp:Label>
            </td>
            <td id="lpc3" runat="server" align="right">
                <asp:Image ID="imgArrows" runat="server" Height="16px" Width="16px" 
                    ImageUrl="~/Styles/downarrow.jpg" />
            </td>
        </tr>
    </table>
</asp:Panel>
<asp:Panel ID="Panel2" runat="server" Visible="true" Font-Names="HP Simplified,GT Walsheim Pro Light,Arial">
    <table style="border-spacing: 0px;">
        <tr>
            <td rowspan="10" class="leftinviscol">
            </td>
            <td class="leftrightborderbold">
                <asp:Label ID="lbrightafter" runat="server" Text="Predecessor:"></asp:Label>
            </td>
            <td class="rightborderbold">
                <asp:Label ID="lbrightBefore" runat="server" Text="Successor:"></asp:Label>
            </td>
            <td class="rightborderbold">
                Job Category:</td>
            <td class="rightborderlastbold">
                <asp:Label ID="lbtapes" runat="server" Text="Tape:"></asp:Label>
            </td>
        </tr>
        <tr>
            <td class="leftrightbottonborder">
                <asp:HyperLink ID="hlJobs_To_Be_Finished_Before" CssClass="bulletedlistCSS" runat="server" Enabled="false"></asp:HyperLink>
            </td>
            <td class="rightbottomborder">
                <asp:HyperLink ID="hlJobs_To_Be_Started_After" CssClass="bulletedlistCSS" Enabled="false" runat="server"></asp:HyperLink>
            </td>
            <td class="rightbottomborder">
                <asp:Label ID="lbJob_Category" runat="server" CssClass="bulletedlistCSS" 
                    Text="Job Category"></asp:Label>
            </td>
            <td class="rightbottomborderlast" rowspan="3">
                <asp:PlaceHolder ID="phJobTape" runat="server"></asp:PlaceHolder>
                <asp:GridView ID="gvLogTapeReader" runat="server" AutoGenerateColumns="False" 
                BackColor="LightGoldenrodYellow" BorderColor="Tan" BorderWidth="0px"
                CellPadding="2" ForeColor="Black" GridLines="None" EditRowStyle-HorizontalAlign="Center"
                onrowdatabound="gvLogTapeReader_RowDataBound" 
                    DataKeyNames="TapeName,IsInput,volser,CPU" 
                    onrowcommand="gvLogTapeReader_RowCommand">
                <AlternatingRowStyle BackColor="PaleGoldenrod" />
                <Columns>
                    <asp:BoundField DataField="IsInput" HeaderText="Input" Visible="False">
                    <ItemStyle BorderColor="Tan" BorderStyle="Solid" BorderWidth="1px" 
                        HorizontalAlign="Center" />
                    </asp:BoundField>
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:Image ID="Input" runat="server" ImageUrl="~/Styles/tape_in3.png" 
                                ToolTip="Input Tape" Width="20px" />
                            <asp:Image ID="Output" runat="server" ImageUrl="~/Styles/tape_out3.png" 
                                ToolTip="Output Tape" Width="20px" />
                        </ItemTemplate>
                        <ItemStyle BorderColor="Tan" BorderStyle="Solid" BorderWidth="1px" 
                        HorizontalAlign="Center" />
                    </asp:TemplateField>
                    <asp:BoundField DataField="CPU" HeaderText="CPU">
                    <ItemStyle BorderColor="Tan" BorderStyle="Solid" BorderWidth="1px" 
                        HorizontalAlign="Center" />
                    </asp:BoundField>
                    <asp:BoundField DataField="TapeName" HeaderText="Tape Name">
                    <ItemStyle BorderColor="Tan" BorderStyle="Solid" BorderWidth="1px" 
                        HorizontalAlign="Center" />
                    </asp:BoundField>
                     <asp:BoundField DataField="RunTapeTimeDB" HeaderText="Received at">
                    <ItemStyle BorderColor="Tan" BorderStyle="Solid" BorderWidth="1px" 
                        HorizontalAlign="Center" />
                    </asp:BoundField>
                    <asp:BoundField DataField="TapeMountTimeDB" HeaderText="Mounted at">
                    <ItemStyle BorderColor="Tan" BorderStyle="Solid" BorderWidth="1px" 
                        HorizontalAlign="Center" />
                    </asp:BoundField>
                    <asp:BoundField DataField="TapeRemoveTimeDB" HeaderText="Came down at">
                    <ItemStyle BorderColor="Tan" BorderStyle="Solid" BorderWidth="1px" 
                        HorizontalAlign="Center" />
                    </asp:BoundField>
                    <asp:BoundField DataField="TapeSubmitTimeDB" HeaderText="Submited at">
                    <ItemStyle BorderColor="Tan" BorderStyle="Solid" BorderWidth="1px" 
                        HorizontalAlign="Center" />
                    </asp:BoundField>
                    <asp:BoundField DataField="OS390JobName" HeaderText="zOS" >
                    <ItemStyle BorderColor="Tan" BorderStyle="Solid" BorderWidth="1px" 
                        HorizontalAlign="Center" />
                    </asp:BoundField>
                    <asp:BoundField DataField="volser" HeaderText="VOLSER">
                    <ItemStyle BorderColor="Tan" BorderStyle="Solid" BorderWidth="1px" 
                        HorizontalAlign="Center" />
                    </asp:BoundField>
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:Button ID="Button1" runat="server" CssClass="buttonTemp1" 
                                CommandArgument="<%# ((GridViewRow) Container).RowIndex %>" 
                                CommandName="ADD" Text="Add" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EditRowStyle HorizontalAlign="Center" />
                <FooterStyle BackColor="Tan" />
                <HeaderStyle BackColor="Tan" Font-Bold="True" />
                <PagerStyle BackColor="PaleGoldenrod" ForeColor="DarkSlateBlue" 
                    HorizontalAlign="Center" />
                <SelectedRowStyle BackColor="DarkSlateBlue" ForeColor="GhostWhite" />
                <SortedAscendingCellStyle BackColor="#FAFAE7" />
                <SortedAscendingHeaderStyle BackColor="#DAC09E" />
                <SortedDescendingCellStyle BackColor="#E1DB9C" />
                <SortedDescendingHeaderStyle BackColor="#C2A47B" />
            </asp:GridView>
            </td>
        </tr>
        <tr>
            <td class="leftrightborderbold" colspan="3">
                <asp:Label ID="lbincomjob" runat="server" Text="Incompatible Jobs:"></asp:Label>
            </td>
        </tr>
        <tr>
            <td class="leftrightbottonborder" colspan="3" valign="top">
                <asp:DataList ID="dlIncompatibilities" runat="server" 
                    DataKeyField="job_Schedule_ID" class="bulletedlistCSS"
                                                RepeatColumns="5" RepeatDirection="Horizontal" 
                    onitemdatabound="dlIncompatibilities_ItemDataBound" 
                    CssClass="bulletedlistCSS">
                                                <ItemTemplate>
                                                    <asp:HyperLink ID="HyperLink1" CssClass="bulletedlistCSS" runat="server" 
                                                            ToolTip='<%# Eval("job_Name_OPS")%>' Text='<%# Eval("job_Name")%>'
                                                            NavigateUrl='<%# Eval("job_Schedule_ID","~/Default.aspx?schtype={0}&schID={0}")%>'>
                                                            </asp:HyperLink>
                                                </ItemTemplate>
                                            </asp:DataList>
            </td>
        </tr>
        <tr>
            <td colspan="3" class="notejob">
                <asp:Label ID="lbCPU_IDCheck" runat="server" CssClass="infolabel" 
                    Text="Please change the CPU if necessary, before click on &quot;Check&quot; button"></asp:Label>
            </td>
            <td class="notejob" colspan="1" align="right" rowspan="2">
                <asp:Button ID="btnReset" runat="server" CssClass="buttonTemp2" OnClick="btnReset_Click"
                    Text="Reset to Scheduled Status" ToolTip="Reset this job status to Scheduled" />
                <ajax:ConfirmButtonExtender ID="ConfirmButtonExtender1" runat="server" ConfirmText="Are you sure you wanna RESET this Job?"
                    TargetControlID="btnReset" />
            </td>
        </tr>
        
        <tr>
            <td class="notejob" colspan="3">
                <asp:DropDownList ID="drCPU" runat="server" CssClass="txtboxTemp1">
                </asp:DropDownList>
                <asp:Button ID="btnCheck" runat="server" OnClick="btnChech_Click" Text="Check" 
                    ToolTip="Please check if any of Incompatible jobs/Predecessor are running/has not finished for this job. " />
                <asp:Label ID="lblCheck1" runat="server" Visible="False"></asp:Label>
            </td>
        </tr>
        
        <tr>
            <td id="TabCommands" runat="server" colspan="4">
                <ajax:TabContainer ID="TC1" runat="server" ActiveTabIndex="0" Width="100%" 
                    CssClass="MyTabStyle">
                    <ajax:TabPanel runat="server" ID="TabPreCom" TabIndex="0"><HeaderTemplate>Pre-Commands</HeaderTemplate>
                        <ContentTemplate><fieldset class="txtboxTemp6">
                                <legend>Pre-Commands:</legend>
                                <asp:PlaceHolder ID="phJobCommandPre" runat="server"></asp:PlaceHolder>
                            </fieldset>
                        </ContentTemplate></ajax:TabPanel>
                    <ajax:TabPanel ID="TabStartCom" runat="server" TabIndex="1"><HeaderTemplate>Start-Commands</HeaderTemplate>
                        <ContentTemplate>
                            <fieldset class="txtboxTemp7">
                                <legend>Start-Commands:</legend>
                                <asp:PlaceHolder ID="phJobCommandstart" runat="server"></asp:PlaceHolder>
                            </fieldset>
                        </ContentTemplate></ajax:TabPanel>
                    <ajax:TabPanel ID="TabPostCom" runat="server">
                        <HeaderTemplate>Post-Commands</HeaderTemplate>
                        <ContentTemplate>
                            <fieldset class="txtboxTemp6">
                                <legend>Post-Commands:</legend>
                                <asp:PlaceHolder ID="phJobCommandpost" runat="server"></asp:PlaceHolder>
                            </fieldset>
                        </ContentTemplate></ajax:TabPanel>
                    <ajax:TabPanel ID="TabRunningCom" runat="server">
                        <HeaderTemplate>Running-Commands</HeaderTemplate>
                        <ContentTemplate>
                            <fieldset class="txtboxTemp1">
                                <legend>Running Commands:</legend>
                                <asp:PlaceHolder ID="phJobCommandrunning" runat="server"></asp:PlaceHolder>
                            </fieldset>
                        </ContentTemplate></ajax:TabPanel>
                    <ajax:TabPanel ID="TabNote" runat="server" CssClass="notejob"><HeaderTemplate>Notes</HeaderTemplate>
                        <ContentTemplate>
                            <table>
                                <tr>
                                    <td>
                                        <asp:TextBox ID="txtJobnote" runat="server" CssClass="txtboxTemp1" 
                                            Height="102px" MaxLength="512" TextMode="MultiLine" 
                                            ToolTip="Please put your note here! only 500 characters" Width="195px"></asp:TextBox>
                                        <asp:Button ID="btn_Note" runat="server" CssClass="buttonTemp1" Height="26px" 
                                            OnClick="btn_Note_Click" Text="Add Note" 
                                            ToolTip="Add any note for this job here" />
                                        <ajax:ConfirmButtonExtender ID="cbe3" runat="server" 
                                            ConfirmText="Are you sure you wanna send this note?" 
                                            TargetControlID="btn_Note" />
                                    </td>
                                    <td valign="top">
                                        <asp:CheckBox ID="chcarryOver" runat="server" CssClass="infolabelBold" 
                                            Text="Carry over for 7 days" ToolTip="Carry over this Note for 7 days" />
                                        <asp:CheckBoxList ID="chlSendto" runat="server" CssClass="infolabel" 
                                            RepeatColumns="5" RepeatDirection="vertical" RepeatLayout="table" 
                                            ToolTip="Select 'who' you wanna send the note to">
                                                <asp:ListItem Value="Operator">Operators</asp:ListItem>
                                                <asp:ListItem Value="OpsLead">Ops. Lead</asp:ListItem>
                                                <asp:ListItem Value="RTC">RTC</asp:ListItem>
                                                <asp:ListItem Value="Scheduler">Scheduler</asp:ListItem>
                                                <asp:ListItem Value="Admin">Admin</asp:ListItem>
                                        </asp:CheckBoxList>
                                        <asp:Label ID="lblErrorNote" runat="server" CssClass="ErrorLable" 
                                            Visible="False"></asp:Label>
                                            <br />
                                            <asp:RegularExpressionValidator ID="rgConclusionValidator2" runat="server" 
                                            ControlToValidate="txtJobnote" CssClass="ErrorLable" Display="Dynamic" 
                                            ErrorMessage="Note can't exceed 512 characters!" SetFocusOnError="True" 
                                            ValidationExpression="^[\s\S]{0,512}$"></asp:RegularExpressionValidator>
                                    </td>
                                </tr>
                            </table>
                        </ContentTemplate></ajax:TabPanel>
                </ajax:TabContainer>
            </td>
        </tr>
        <tr>
            <td id="StartEndtd" runat="server" colspan="4">
                <table style="width: 100%; border-spacing: 0px;" class="StartEnd">
                    <tr>
                        <td>
                            <table id="tblStartPnl" runat="server" style="width: 100%; border-spacing: 0px;">
                                <tr>
                                    <td>
                                        <asp:Label ID="lblCheckStart" runat="server" Visible="False"></asp:Label>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        Start:
                                        <asp:TextBox ID="txtJob_Start_Date" runat="server" CssClass="txtboxTemp1" MaxLength="10"
                                            ToolTip="Select Actual Start Date" Width="93px"></asp:TextBox>
                                        <ajax:CalendarExtender ID="CalendarExtender1" runat="server" Format="yyyy-MM-dd"
                                            TargetControlID="txtJob_Start_Date" />
                                        <asp:TextBox ID="txtJob_Start_Time" runat="server" CssClass="txtboxTemp1" MaxLength="4"
                                            ToolTip="Put Actual Start Time" ValidationGroup="chStart" Width="30px"></asp:TextBox>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:Label ID="lblstart" runat="server" CssClass="infolabelframe"></asp:Label>
                                        <asp:Button ID="btnStart" runat="server" OnClick="btnStart_Click" Text="Show Started"
                                            ToolTip="Put the Started time from console" ValidationGroup="chStart" Visible="False" />
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:RegularExpressionValidator ID="regexStartTime" runat="server" ControlToValidate="txtJob_Start_Time"
                                            CssClass="ErrorLable" ErrorMessage="Valid in HHMM format" ValidationExpression="^([0-1][0-9]|[2][0-3])([0-5][0-9])$"
                                            ValidationGroup="chStart" />
                                        <asp:RequiredFieldValidator ID="RFVStart" runat="server" ControlToValidate="txtJob_Start_Time"
                                            CssClass="ErrorLable" ErrorMessage="Put Start time" ValidationGroup="chStart"></asp:RequiredFieldValidator>
                                    </td>
                                </tr>
                            </table>
                        </td>
                        <td class="verticalLine" align="left">
                        </td>
                        <td align="left">
                            &nbsp;
                        </td>
                        <td>
                            <table id="tblENDPnl" runat="server" style="width: 100%; border-spacing: 0px;">
                                <tr>
                                    <td>
                                        <asp:Label ID="lblCheckEnd" runat="server" Visible="False"></asp:Label>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        End:
                                        <asp:TextBox ID="txtJob_End_Date" runat="server" CssClass="txtboxTemp1" MaxLength="10"
                                            ToolTip="Select Actual End Date" Width="93px"></asp:TextBox>
                                        <ajax:CalendarExtender ID="CalendarExtender2" runat="server" Format="yyyy-MM-dd"
                                            TargetControlID="txtJob_End_Date" />
                                        <asp:TextBox ID="txtJob_End_Time" runat="server" CssClass="txtboxTemp1" MaxLength="4"
                                            ToolTip="Put Actual End Time" ValidationGroup="chEnd" Width="30px"></asp:TextBox>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:Label ID="lblEnd" runat="server" CssClass="infolabelframe"></asp:Label>
                                        <asp:DropDownList ID="drjobstatus" runat="server" CssClass="txtboxTemp1" ToolTip="Please select from the list">
                                            <asp:ListItem>--select End status--</asp:ListItem>
                                            <asp:ListItem>Successful</asp:ListItem>
                                            <asp:ListItem>Unsuccessful</asp:ListItem>
                                            <asp:ListItem>Skipped Job</asp:ListItem>
                                        </asp:DropDownList>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:CheckBox ID="ckboxPostCommands" runat="server" Text="I did all post commands of this job" />
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:Button ID="btnJobDone" runat="server" OnClick="btnJobCompleted_Click" Text="Show Completed"
                                            ToolTip="Put the Ended time from console" ValidationGroup="chEnd" />
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:RegularExpressionValidator ID="regexEndTime" runat="server" ControlToValidate="txtJob_End_Time"
                                            CssClass="ErrorLable" ErrorMessage="Valid in HHMM format" ValidationExpression="^([0-1][0-9]|[2][0-3])([0-5][0-9])$"
                                            ValidationGroup="chEnd" />
                                        <asp:RequiredFieldValidator ID="RFVEnd" runat="server" ControlToValidate="txtJob_End_Time"
                                            CssClass="ErrorLable" ErrorMessage="Put End time" ValidationGroup="chEnd"></asp:RequiredFieldValidator>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        
        <tr>
            <td colspan="4">
                <table id="tblNotePnl" runat="server" style="width: 100%; border-spacing: 0px;">
                    <tr class="notejob">
                        <td class="notejob">
                            <asp:Label ID="lbNoteLog" runat="server" CssClass="infolabelBold" Text="Note Log"></asp:Label>
                            <br />
                            <asp:GridView ID="gvNote" runat="server" AutoGenerateColumns="False" CellPadding="4"
                                CssClass="txtboxTemp1" ForeColor="#333333" GridLines="None" OnRowDataBound="gvNote_RowDataBound">
                                <AlternatingRowStyle BackColor="White" />
                                <Columns>
                                    <asp:BoundField DataField="SCHEDULE_NOTE" HeaderText="Note" />
                                    <asp:BoundField DataField="OPERATOR" HeaderText="From" />
                                    <asp:BoundField DataField="SENDTO" HeaderText="To" />
                                    <asp:BoundField DataField="NOTE_DATETIME" HeaderText="Date Time" />
                                    <asp:BoundField DataField="IS_CARRYOVER" HeaderText="Carry Over" />
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
                            &nbsp;
                        </td>
                       <td class="notejob" align="right" valign="top">
                                        <a id="GoTop5" runat="server" href="#top">Go To Top</a>&nbsp;
                                    </td>
                    </tr>
                </table>
            </td>
        </tr>
        
        <tr>
            <td colspan="4">
            </td>
        </tr>
    </table>
    <hr class="EndLine" />
</asp:Panel>
<ajax:CollapsiblePanelExtender ID="CollapsiblePanelExtender2" runat="server" CollapseControlID="imgArrows"
    Collapsed="true" ExpandControlID="imgArrows" TextLabelID="lblMessage" CollapsedText="Show"
    ExpandedText="Hide" ImageControlID="imgArrows" CollapsedImage="../Styles/downarrow.jpg"
    ExpandedImage="../Styles/uparrow.jpg" ExpandDirection="Vertical" TargetControlID="Panel2"
    ScrollContents="false" SuppressPostBack="False" AutoExpand="True"></ajax:CollapsiblePanelExtender>

