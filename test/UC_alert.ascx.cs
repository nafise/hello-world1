using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Data;

namespace HP_EYE
{
    public partial class UC_alert : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Roles.IsUserInRole("Operator"))
            {
                tpIactiveJobs.Visible = false;
               // tpWarningMessages.Visible = false;
                TC1.ActiveTabIndex = 2;
            }
            if (Roles.IsUserInRole("Operator")
                || Roles.IsUserInRole("Viewer")
                || Roles.IsUserInRole("NoAccess")
                || Roles.IsUserInRole("TSC")
                || Roles.IsUserInRole("CopyScheduler"))
            {
                btn_ACK.Enabled = false;
            }

            if (!IsPostBack)
            {
                DataTable systemAll = new DataTable();

                string[] temp = Roles.GetRolesForUser();
                if (temp.Length != 0)
                {
                    systemAll = (new AdminDB()).GetAlltpf_system(temp);
                }
                drSystemName.DataValueField = "SYSTEM_ID";
                drSystemName.DataTextField = "SYSTEM_NAME";
                drSystemName.DataSource = systemAll;
                drSystemName.DataBind();
                UpdateControl();
               
            }
        }

        protected void btn_ACK_Click(object sender, EventArgs e)
        {
            List<NoteScheduleJob> lstACKnote = new List<NoteScheduleJob>();
            NoteScheduleJob noteSch = new NoteScheduleJob();
            int i, chCount = 0;
            for (i = 0; i < gvNote.Rows.Count; i++)
            {
                CheckBox chACK = (CheckBox)gvNote.Rows[i].FindControl("chACK");
                if (chACK.Checked )
                {
                    noteSch = new NoteScheduleJob();
                    noteSch.Note_ID = long.Parse(gvNote.DataKeys[i].Values[1].ToString());
                    noteSch.Is_Acknowledge = true;
                    noteSch.Acknowledge_By = HttpContext.Current.User.Identity.Name;
                    lstACKnote.Add(noteSch);
                    chCount++;
                }
            }
            noteSch.Update_Scheduled_note(lstACKnote);
            DataTable dtMorningAfter = new DataTable();
            dtMorningAfter = (new NoteScheduleJob()).MorningAfterAutoMsg(false);
            gvNote.DataSource = dtMorningAfter;
            gvNote.DataBind();
            if (gvNote.Rows.Count == chCount)
            {
                HyperLink hplWarning = Page.Master.FindControl("hplWarning") as HyperLink;
                hplWarning.Visible = false;
            }

            if (gvNote.Rows.Count != 0)
            {
                btn_ACK.Visible = true;
            }
        }

        protected void gvNote_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                switch (e.Row.Cells[7].Text)
                {
                    case "0":
                        e.Row.Cells[7].Text = "No";
                        e.Row.Cells[7].ForeColor = System.Drawing.Color.Blue;
                        break;
                    case "1":
                        e.Row.Cells[7].Text = "Yes";
                        e.Row.Cells[7].ForeColor = System.Drawing.Color.Green;
                        e.Row.Cells[7].Font.Bold = true;
                        e.Row.Cells[8].ForeColor = System.Drawing.Color.Green;
                        e.Row.Cells[8].Font.Bold = true;

                        CheckBox chACK = (CheckBox)e.Row.FindControl("chACK");
                        chACK.Checked = true;
                        e.Row.Cells[0].Enabled = false;

                        break;
                }
            }
            if (Roles.IsUserInRole("Operator")
                || Roles.IsUserInRole("Viewer")
                || Roles.IsUserInRole("NoAccess")
                || Roles.IsUserInRole("TSC")
                || Roles.IsUserInRole("CopyScheduler"))
            {
                e.Row.Cells[0].Visible = false;
            }
        }

        /// <summary>
        /// Nafise: Acknowledged by Role
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnACKRole_Click(object sender, EventArgs e)
        {
            List<NoteScheduleJob> lstACKnote = new List<NoteScheduleJob>();
            NoteScheduleJob noteSch =new NoteScheduleJob();
            for (int i = 0; i < gvNoteRole.Rows.Count; i++)
            {
                CheckBox chACK = (CheckBox)gvNoteRole.Rows[i].FindControl("chACK");
                if (chACK.Checked)
                {
                    noteSch = new NoteScheduleJob();
                    noteSch.Note_ID = long.Parse(gvNoteRole.DataKeys[i].Values[1].ToString());
                    noteSch.Is_Acknowledge = true;
                    noteSch.Acknowledge_By = HttpContext.Current.User.Identity.Name;
                    lstACKnote.Add(noteSch);
                }
            }
            noteSch.Update_Scheduled_note(lstACKnote);
            UpdateControl();
        }

        protected void gvNoteRole_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                switch (e.Row.Cells[7].Text)
                {
                    case "0":
                        e.Row.Cells[7].Text = "No";
                        e.Row.Cells[7].ForeColor = System.Drawing.Color.Blue;
                        break;
                    case "1":
                        e.Row.Cells[7].Text = "Yes";
                        e.Row.Cells[7].ForeColor = System.Drawing.Color.Green;
                        e.Row.Cells[7].Font.Bold = true;
                        e.Row.Cells[8].ForeColor = System.Drawing.Color.Green;
                        e.Row.Cells[8].Font.Bold = true;

                        CheckBox chACK = (CheckBox)e.Row.FindControl("chACK");
                        chACK.Checked = true;
                        e.Row.Cells[0].Enabled = false;

                        break;
                }

            }
        }

        public void UpdateControl()
        {
            if (TC1.ActiveTabIndex == 0)
            {
                DataTable systemAll = new DataTable();
                systemAll = (new ScheduleJob()).InactJobActSchedule();
                gvInactJobSchd.DataSource = systemAll;
                gvInactJobSchd.DataBind();
            }
            else if (TC1.ActiveTabIndex == 1)
            {
                DataTable dtMorningAfter = new DataTable();
                dtMorningAfter = (new NoteScheduleJob()).MorningAfterAutoMsg(false);
                lblWarnningsNumber.Text = "(Total: " + dtMorningAfter.Rows.Count + ")";
                gvNote.DataSource = dtMorningAfter;
                gvNote.DataBind();
                if (gvNote.Rows.Count != 0)
                {
                    btn_ACK.Visible = true;
                }
                else
                {
                    lblErrAutoMsg.Visible = true;
                }
            }
            else if (TC1.ActiveTabIndex == 2)
            {
                DataTable dtMsgByRole = new DataTable();
                dtMsgByRole = (new NoteScheduleJob()).MsgByRole();
                lblInboxNumber.Text = "(Total: " + dtMsgByRole.Rows.Count + ")";
                gvNoteRole.DataSource = dtMsgByRole;
                gvNoteRole.DataBind();
                if (gvNoteRole.Rows.Count != 0)
                {
                    btnACKRole.Visible = true;
                }
                else
                {
                    lblInbox.Visible = true;
                }
            }
            else if (TC1.ActiveTabIndex == 3)
            {
                DataTable dtMsgEND = (new NoteScheduleJob()).EndJobAutoMsg();
                lblEndJobCount.Text = "(Total: " + dtMsgEND.Rows.Count + ")";
                gvNoteEndJob.DataSource = dtMsgEND;
                gvNoteEndJob.DataBind();
                if (gvNoteEndJob.Rows.Count == 0)
                {
                    lblendjob.Visible = true;
                }
                else
                {
                    lblendjob.Visible = false;
                }
            }
            else if (TC1.ActiveTabIndex == 4)
            {
                DataTable dtMsgManual = (new NoteScheduleJob()).ManuallyJobAutoMsg();
                lblManuallyJobCount.Text = "(Total: " + dtMsgManual.Rows.Count + ")";
                gvNoteManually.DataSource = dtMsgManual;
                gvNoteManually.DataBind();
                if (gvNoteManually.Rows.Count == 0)
                {
                    lblmanuallyjob.Visible = true;
                }
                else
                {
                    lblmanuallyjob.Visible = false;
                }
            }
            else if (TC1.ActiveTabIndex == 5) //RUNTAPE Messages
            {
                DataTable dtMsgRuntape = (new ScheduleJob()).Runtape_FilterList(drSystemName.SelectedItem.Value);
                lblRuntapeJobCount.Text = "(Total: " + dtMsgRuntape.Rows.Count + ")";
                gvNoteRunTape.DataSource = dtMsgRuntape;
                gvNoteRunTape.DataBind();
            }
        }

        protected void TC1_ActiveTabChanged(object sender, EventArgs e)
        {
            UpdateControl();
        }

        protected void drSystemName_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateControl();
        }
    }
}
