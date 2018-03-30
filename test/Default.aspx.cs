using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Data;
using System.IO;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html.simpleparser;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.parser;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.pipeline.html;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.pipeline.end;
using System.Web.Security;
using System.Web.Configuration;

namespace HP_EYE
{
    public partial class _Default : System.Web.UI.Page
    {
        string  stSys = "";
        int pagecnt = 5;
        int Defaultpagecnt = 0;
        public string SortDireaction
        {
            get
            {
                if (ViewState["SortDireaction"] == null)
                    return string.Empty;
                else
                    return ViewState["SortDireaction"].ToString();
            }
            set
            {
                ViewState["SortDireaction"] = value;
            }
        }
        private string _sortDirection;
        List<string> lstSch_Jobs = new List<string>();
        List<string> Chained_Jobs = new List<string>();
        List<long> ScheduleShowOrderToday = new List<long>();
        List<long> ScheduleShowOrder = new List<long>();
        DataTable NotFinished_Chained_Jobs_ScheduleID = new DataTable();
        DataTable NotMountedTapes = new DataTable();
        
        protected void Page_Load(object sender, EventArgs e)
        {
            string stschID = null;
            bool flgEndHidjob = false;
            int ScheduleShowOrderCount = 0;
            Defaultpagecnt = pagecnt;
            if (Request.QueryString["schtype"] == null
                & (Request.QueryString["schID"] == null)
                & (Request.QueryString["p"] == null))
            {
                Response.Redirect("Default.aspx?schtype=todsch");
            }
            if (Session["HideEndedJobs"] != null && !IsPostBack)
            {
                chHideEndedJobs.Checked = bool.Parse(Session["HideEndedJobs"].ToString());
            }

            string stReq = "", stReqID = "";
            DateTime Starttodtom = DateTime.Now;
            DateTime Endtodtom = DateTime.Now;
            List<string> lstJobStatus = new List<string>();
            List<string> selecteditems = new List<string>();
            int cntJob = 0, numpg = 0;
            
            Label lbPageTitle = Page.Master.FindControl("lbMasterPageTitle") as Label;
            DataTable Scheduled_Job_Details = new DataTable();

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

                if (Session["SELECTED_SYSTEM"] == null)
                {
                    if (Request.Cookies["SELECTED_SYSTEM"] != null)
                    {
                        Session.Add("SELECTED_SYSTEM", Request.Cookies["SELECTED_SYSTEM"].Value);
                    }
                    else
                    {
                        Session.Add("SELECTED_SYSTEM", drSystemName.SelectedIndex);
                        HttpCookie Station_Cookie = new HttpCookie("SELECTED_SYSTEM");
                        Station_Cookie.Value = drSystemName.SelectedIndex.ToString();
                        Station_Cookie.Expires = DateTime.Now.AddDays(7);
                        Response.Cookies.Add(Station_Cookie);
                        if (drSystemName.SelectedIndex != -1)
                        {
                            HttpCookie Station_system_name = new HttpCookie("SELECTED_SYSTEM_NAME");
                            Station_system_name.Value = drSystemName.SelectedItem.Text;
                            Station_system_name.Expires = DateTime.Now.AddDays(7);
                            Response.Cookies.Add(Station_system_name);
                        }
                    }
                }
                drSystemName.SelectedIndex = int.Parse(Session["SELECTED_SYSTEM"].ToString());
                if (drSystemName.SelectedIndex != -1)
                {
                    FillCPU(long.Parse(drSystemName.SelectedValue));
                    if (Session["SELECTED_CPU_ID"] == null) // if session is null and still we have checked CPU_IDs
                    {
                        List<string> NonItem = new List<string>();
                        for (int i = 0; i < chblCPU.Items.Count; i++)
                        {
                            if (chblCPU.Items[i].Selected)
                            {
                                NonItem.Add(chblCPU.Items[i].Value);
                            }
                        }
                        Session["SELECTED_CPU_ID"] = NonItem;
                    }
                    selecteditems = (List<string>)Session["SELECTED_CPU_ID"];
                    if (selecteditems.Count == 0)
                    {
                        for (int i = 0; i < chblCPU.Items.Count; i++)
                        {
                            chblCPU.Items[i].Selected = true;
                        }
                    }
                    else
                    {

                        for (int j = 0; j < selecteditems.Count; j++)
                        {
                            for (int i = 0; i < chblCPU.Items.Count; i++)
                            {
                                if (chblCPU.Items[i].Value == selecteditems[j])
                                {
                                    chblCPU.Items[i].Selected = true;
                                }
                            }
                        }
                    }
                }
            }

            string IPsection = WebConfigurationManager.AppSettings["ECC_IP"];
            string UserIP = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            if (IPsection.Length==0 || IPsection.Contains(UserIP))
            {
                if (drSystemName.SelectedItem != null)
                {
                    switch (drSystemName.SelectedItem.Text)
                    {
                        //case "WNP":
                        //    chOperator.Visible = SiteMaster.QuickReaderWNP0.AdminAutoUpdate 
                        //        & SiteMaster.QuickReaderWNP1.AdminAutoUpdate;
                        //    break;
                        case "FCA":
                            chOperator.Visible = SiteMaster.QuickReaderFCA2.AdminAutoUpdate
                                & SiteMaster.QuickReaderFCA3.AdminAutoUpdate
                                & SiteMaster.QuickReaderFCA4.AdminAutoUpdate
                                & SiteMaster.QuickReaderFCA5.AdminAutoUpdate;
                            break;
                        case "FOS":
                            chOperator.Visible = SiteMaster.QuickReaderFOSI.AdminAutoUpdate
                                & SiteMaster.QuickReaderFOSJ.AdminAutoUpdate;
                            break;
                        case "PSS":
                            chOperator.Visible = SiteMaster.QuickReaderPSSA.AdminAutoUpdate
                                & SiteMaster.QuickReaderPSSB.AdminAutoUpdate
                                & SiteMaster.QuickReaderPSSC.AdminAutoUpdate
                                & SiteMaster.QuickReaderPSSD.AdminAutoUpdate
                                & SiteMaster.QuickReaderPSSE.AdminAutoUpdate
                                & SiteMaster.QuickReaderPSSF.AdminAutoUpdate
                                & SiteMaster.QuickReaderPSSG.AdminAutoUpdate
                                & SiteMaster.QuickReaderPSSH.AdminAutoUpdate;
                            break;
                    }
                    chOperatorEndTime.Visible = chOperator.Checked;
                }
            }
            else
            {
                chOperator.Visible = false;
                chOperatorEndTime.Visible = false;
            }

            string stOpsSystem = null ;
            byte AutoPilotLevel=0;
            if (drSystemName.SelectedItem != null && !IsPostBack )
            {
                switch (drSystemName.SelectedItem.Text)
                {
                    //case "WNP":
                    //    stOpsSystem = SiteMaster.QuickReaderWNP1.ReadOpsSystem();
                    //    //stOpsSystem = SiteMaster.QuickReaderWNP0.ReadOpsSystem();
                    //    AutoPilotLevel = SiteMaster.QuickReaderWNP1.ReadOpsAutoPilot();
                    //    break;
                    case "FCA":
                        stOpsSystem = SiteMaster.QuickReaderFCA2.ReadOpsSystem();
                        //stOpsSystem = SiteMaster.QuickReaderFCA3.ReadOpsSystem();
                        //stOpsSystem = SiteMaster.QuickReaderFCA4.ReadOpsSystem();
                        //stOpsSystem = SiteMaster.QuickReaderFCA5.ReadOpsSystem();
                        AutoPilotLevel = SiteMaster.QuickReaderFCA2.ReadOpsAutoPilot();
                        break;
                    case "FOS":
                        stOpsSystem = SiteMaster.QuickReaderFOSI.ReadOpsSystem();
                        //stOpsSystem = SiteMaster.QuickReaderFOSJ.ReadOpsSystem();
                        AutoPilotLevel = SiteMaster.QuickReaderFOSI.ReadOpsAutoPilot();
                        break;
                    case "PSS":
                        stOpsSystem = SiteMaster.QuickReaderPSSA.ReadOpsSystem();
                        //stOpsSystem = SiteMaster.QuickReaderFOSJ.ReadOpsSystem();
                        AutoPilotLevel = SiteMaster.QuickReaderPSSA.ReadOpsAutoPilot();
                        break;
                }
            }

            if (stOpsSystem != null && stOpsSystem == User.Identity.Name && !IsPostBack)
            {
                switch (AutoPilotLevel)
                {
                    case 2:
                        chOperator.Checked = true;
                        chOperatorEndTime.Visible = true;
                        chOperatorEndTime.Checked = true;
                        break;
                    case 1:
                        chOperator.Checked = true;
                        chOperatorEndTime.Visible = true;
                        chOperatorEndTime.Checked = false;
                        break;
                    case 0:
                        chOperator.Checked = false;
                        chOperatorEndTime.Checked = false;
                        chOperatorEndTime.Visible = false;
                        break;
                }
            }

            if (chOperator.Checked)
            {
                chOperator.CssClass = "";
            }
            else
            {
                chOperator.CssClass = "buttonCopy";
            }

            if (chOperatorEndTime.Checked)
            {
                chOperatorEndTime.Font.Bold = false;
            }
            else
            {
                chOperatorEndTime.Font.Bold = true;
            }

            if (Request.QueryString["sys"] != null)
            {
                stSys = Request.QueryString["sys"];
                drSystemName.SelectedIndex = int.Parse(stSys);
                stSys = drSystemName.Items[int.Parse(stSys)].Value;
            }
            else
            {
                stSys = drSystemName.SelectedValue;
                if (stSys == "")
                {
                    stSys = "0";
                }
            }

            lstJobStatus.Add("SCHEDULED");
            lstJobStatus.Add("STARTED");
            lstJobStatus.Add("ENDED-Unsuccessful");
            lstJobStatus.Add("ENDED-Successful");
            lstJobStatus.Add("Skipped Job");

            
            /// schtype
            if (Request.QueryString["schtype"] != null)
            {
                stReq = Request.QueryString["schtype"];

                // Today Schedule
                Starttodtom = DateTime.Now;
                Endtodtom = DateTime.Now;
                lstJobStatus.Clear();
                lstJobStatus.Add("SCHEDULED");
                lstJobStatus.Add("STARTED");
                lstJobStatus.Add("ENDED-Unsuccessful");
                lstJobStatus.Add("ENDED-Successful");
                lstJobStatus.Add("Skipped Job");
                lstJobStatus.Add("HOLD");

                
                lbPageTitle.Text = "Today Schedule";
                Scheduled_Job_Details = (new ScheduleJob()).Schedule_ID_FilterList(Starttodtom, selecteditems , Endtodtom, stSys, "", "", "", "", "", false, false, false,
lstJobStatus, "", "", "", "", "", false);
                ScheduleShowOrder = scheduling(Scheduled_Job_Details, stReq);
                ScheduleShowOrderToday = ScheduleShowOrder;

                if (stReq.Contains("yessch"))
                {
                    // Yesterday Schedule
                    Starttodtom = DateTime.Now.AddDays(-1);
                    Endtodtom = DateTime.Now.AddDays(-1);
                    lstJobStatus.Clear();
                    lstJobStatus.Add("SCHEDULED");
                    lstJobStatus.Add("STARTED");
                    lstJobStatus.Add("ENDED-Unsuccessful");
                    lstJobStatus.Add("ENDED-Successful");
                    lstJobStatus.Add("Skipped Job");
                    //if (chHideEndedJobs.Checked)
                    //{
                    //    lstJobStatus.Clear();
                    //    lstJobStatus.Add("SCHEDULED");
                    //    lstJobStatus.Add("STARTED");
                    //}
                    lbPageTitle.Text = "Yesterday Schedule";
                    lbPageTitle.ForeColor = Color.Red;
                    Scheduled_Job_Details = (new ScheduleJob()).Schedule_ID_FilterList(Starttodtom, selecteditems, Endtodtom, stSys, "", "", "", "", "", false, false, false,
lstJobStatus, "", "", "", "", "", false);
                    ScheduleShowOrder = scheduling(Scheduled_Job_Details, stReq);
                }
                else if (stReq.Contains("outsch"))
                { // jobs in progress of days ago
                    Starttodtom = DateTime.Now.AddDays(-30);
                    Endtodtom = DateTime.Now.AddDays(-1);
                    lstJobStatus.Clear();
                    lstJobStatus.Add("SCHEDULED");
                    lstJobStatus.Add("STARTED"); 
                    lstJobStatus.Add("HOLD");
                    Scheduled_Job_Details = (new ScheduleJob()).Schedule_ID_FilterList(Starttodtom, selecteditems, Endtodtom, stSys, "", "", "", "", "", false, false, false,
lstJobStatus, "", "", "", "", "", false);
                    ScheduleShowOrder = scheduling(Scheduled_Job_Details, stReq);
                    if (ScheduleShowOrder.Count == 0 && Request.QueryString["schID"] != null)
                    {
                        Response.Redirect("Default.aspx?schtype=todsch");
                    }
                    lbPageTitle.Text = "Pendings";
                    lbPageTitle.ForeColor = Color.Red;

                }
                else if (stReq.Contains("tomsch"))
                {
                    // Tomorrow Schedule
                    Starttodtom = DateTime.Now.AddDays(1);
                    Endtodtom = DateTime.Now.AddDays(1);
                    lstJobStatus.Clear();
                    lstJobStatus.Add("SCHEDULED");
                    lstJobStatus.Add("STARTED");
                    lstJobStatus.Add("ENDED-Unsuccessful");
                    lstJobStatus.Add("ENDED-Successful");
                    lstJobStatus.Add("Skipped Job");
                    Scheduled_Job_Details = (new ScheduleJob()).Schedule_ID_FilterList(Starttodtom, selecteditems, Endtodtom, stSys, "", "", "", "", "", false, false, false,
 lstJobStatus, "", "", "", "", "", false);
                    ScheduleShowOrder = scheduling(Scheduled_Job_Details, stReq);
                    if (ScheduleShowOrder.Count == 0 && Request.QueryString["schID"] != null)
                    {
                        Response.Redirect("Default.aspx?schtype=todsch&schID=" + Request.QueryString["schID"] + "#" + Request.QueryString["schID"]);
                    }
                    lbPageTitle.Text = "Tomorrow Schedule";
                    lbPageTitle.ForeColor = Color.Blue;
                }

                //-------- hide ended jobs
                
                if (Request.QueryString["schID"] != null)
                {
                        stschID = Request.QueryString["schID"];
                }
                if (chHideEndedJobs.Checked)
                {
                    for (int i = 0; i < Scheduled_Job_Details.Rows.Count; i++)
                    {
                        if (!Scheduled_Job_Details.Rows[i]["JOB_STATUS"].ToString().Contains("START") &&
                            !Scheduled_Job_Details.Rows[i]["JOB_STATUS"].ToString().Contains("SCHEDULE"))
                        {
                            if (Scheduled_Job_Details.Rows[i]["SCHEDULE_ID"].ToString() != stschID)
                            {
                                ScheduleShowOrder.Remove(long.Parse(Scheduled_Job_Details.Rows[i]["SCHEDULE_ID"].ToString()));
                                ScheduleShowOrder.Remove(-long.Parse(Scheduled_Job_Details.Rows[i]["SCHEDULE_ID"].ToString()));
                            }
                            else
                            {
                                flgEndHidjob = true;
                            }
                        }
                    }
                }

                ScheduleShowOrderCount = ScheduleShowOrder.Count;
                //if (flgEndHidjob)//if user click on the ended jobs link and hide job was checked too
                //{
                //    ScheduleShowOrderCount = ScheduleShowOrder.Count + 1;
                //}
                // Index of the head of next jobs to display
                if (drPage.SelectedIndex != -1)
                {
                    cntJob = drPage.SelectedIndex * pagecnt;
                }
                // Filling Todayjoblist gridview
                fillJobPannels();

                //filling page dropdown
                numpg = ScheduleShowOrderCount / pagecnt;
                int drpageselectInx = drPage.SelectedIndex;
                drPage.Items.Clear();
                int j = 0;
                for (j = 0; j < numpg; j++)
                {
                    drPage.Items.Add((j + 1).ToString());
                }

                if (ScheduleShowOrderCount % pagecnt != 0)
                    {
                        drPage.Items.Add((j + 1).ToString());
                    }

                if (drPage.Items.Count > 0)
                {
                    if (drPage.Items.Count < drpageselectInx)
                    {
                        drPage.SelectedIndex = 0;
                    }
                    else
                    {
                        drPage.SelectedIndex = drpageselectInx;
                    }
                }
            }

            ///////////////////////////////////////////
            Job selectedJob = new Job();
            JobTimes timefreq = new JobTimes();
            JobTape jobtape = new JobTape();


            UC_Job ucJob = new UC_Job();
            if ((ScheduleShowOrderCount - cntJob) < pagecnt)
            {
                pagecnt = ScheduleShowOrderCount;
                //btnNext.Visible = false;
            }
            else
            {
                pagecnt = cntJob + pagecnt;
            }


            //go to job
            /// schID
            if (!IsPostBack)
            {
                if (Request.QueryString["schID"] != null)
                {
                    if (drPage.SelectedIndex == -1)
                    {
                        stReqID = "0";
                    }
                    else
                    {
                        stReqID = Request.QueryString["schID"];
                    }

                    long temp = ScheduleShowOrder.IndexOf(long.Parse(stReqID));
                    if (temp == -1)
                    {
                        temp = ScheduleShowOrder.IndexOf(-long.Parse(stReqID));
                    }

                    if (temp == -1) // if Schedule_ID is not on the list
                    {
                        Response.Redirect("Default.aspx?schtype=todsch");                        
                    }

                    cntJob = (int)((temp / (long)pagecnt) * (long)pagecnt);
                    drPage.SelectedIndex = (int)((cntJob / (long)pagecnt));

                    if ((ScheduleShowOrderCount - cntJob) < pagecnt)
                    {
                        pagecnt = ScheduleShowOrderCount;
                        //btnNext.Visible = false;
                    }
                    else
                    {
                        pagecnt = cntJob + pagecnt;
                    }
                }
                
                /// page on dropdown
                if (Request.QueryString["p"] != null)
                {
                    stReqID = Request.QueryString["p"];
                    cntJob = (int.Parse(stReqID)) * pagecnt;

                    drPage.SelectedIndex = int.Parse(stReqID);

                    if ((ScheduleShowOrderCount - cntJob) < pagecnt)
                    {
                        pagecnt = ScheduleShowOrderCount;
                        //btnNext.Visible = false;
                    }
                    else
                    {
                        pagecnt = cntJob + pagecnt;
                    }
                }
            }

            if (drPage.SelectedIndex == 0)
            {
                btnPrev.Visible = false;
            }
            if (drPage.SelectedIndex == (drPage.Items.Count - 1))
            {
                btnNext.Visible = false;
            }
            if (drPage.Items.Count == 0)
            {
                btnPrev.Visible = false;
                btnNext.Visible = false;
            }



            if (flgEndHidjob 
                && ((pagecnt - cntJob) == Defaultpagecnt) 
                && (drPage.SelectedIndex != (drPage.Items.Count - 1)))//if user click on the ended jobs link and hide job was checked too and it's not the last page
            {
                pagecnt = pagecnt + 1;
            }

            for (int j = cntJob; j < pagecnt; j++)
            {
                ucJob = LoadControl("~/UC_Job.ascx") as UC_Job;
                ucJob.newschedulejob.Schedule_ID = Math.Abs(ScheduleShowOrder[j]);
                ucJob.newschedulejob.Read_ScheduleJob();


                //Add the SimpleControl to Placeholder
                if (ListOfOpens.Value.Contains("<" + ucJob.newschedulejob.Schedule_ID + ">"))
                    ucJob.CloseThis = true;
                else
                    ucJob.CloseThis = false;
                if (ScheduleShowOrder[j] < 0 || (j < ScheduleShowOrderCount - 1 && ScheduleShowOrder[j + 1] < 0))
                {
                    ucJob.IsChained = true;
                    if (ucJob.newschedulejob.Job_sch.Jobs_To_Be_Started_After.Count != 0)
                    {
                        ucJob.newschedulejob.Job_sch.Jobs_To_Be_Started_After[0].job_Schedule_ID = FetchScdeduleID(ucJob.newschedulejob.Job_sch.Jobs_To_Be_Started_After[0].Job_TIME_ID[0]
                                                                                                                   , ucJob.newschedulejob.Schedule_Date);
                        if (ucJob.newschedulejob.Job_sch.Jobs_To_Be_Started_After[0].job_Schedule_ID == 0)
                        {
                            ucJob.newschedulejob.Job_sch.Jobs_To_Be_Started_After[0].job_Schedule_ID = ucJob.newschedulejob.Schedule_ID;
                        }
                    }

                    if (ucJob.newschedulejob.Job_sch.Jobs_To_Be_Finished_Before.Count != 0)
                    {
                        ucJob.newschedulejob.Job_sch.Jobs_To_Be_Finished_Before[0].job_Schedule_ID = FetchScdeduleID(ucJob.newschedulejob.Job_sch.Jobs_To_Be_Finished_Before[0].Job_TIME_ID[0]
                                                                                                                   , ucJob.newschedulejob.Schedule_Date);
                        if (ucJob.newschedulejob.Job_sch.Jobs_To_Be_Finished_Before[0].job_Schedule_ID == 0)
                        {
                            ucJob.newschedulejob.Job_sch.Jobs_To_Be_Finished_Before[0].job_Schedule_ID = ucJob.newschedulejob.Schedule_ID;
                        }
                    }
                }

                for (int i = 0; i < ucJob.newschedulejob.Job_sch.Job_Incompatibles.Count; i++)
                {
                    ucJob.newschedulejob.Job_sch.Job_Incompatibles[i].job_Schedule_ID = FetchScdeduleIDList(ucJob.newschedulejob.Job_sch.Job_Incompatibles[i].Job_TIME_ID
                                                                                                                   , ucJob.newschedulejob.Schedule_Date);
                    if (ucJob.newschedulejob.Job_sch.Job_Incompatibles[i].job_Schedule_ID == 0 ||
                        ucJob.newschedulejob.Job_sch.Job_Incompatibles[i].job_Schedule_ID == -1)
                    {
                        ucJob.newschedulejob.Job_sch.Job_Incompatibles[i].job_Schedule_ID = ucJob.newschedulejob.Schedule_ID;
                    }
                }

                if (ucJob.newschedulejob.Schedule_ID.ToString() == stReqID && ucJob.newschedulejob.Job_Status != "HOLD")
                {
                    ucJob.CloseThis = true;
                    ListOfOpens.Value += "<" + ucJob.newschedulejob.Schedule_ID + ">";
                }

                // hook up event handler for exposed user control event
                ucJob.UserControlButtonClicked += new
                            EventHandler(UC_Job_UserControlButtonClicked);
                phJob.Controls.Add(ucJob);
            }
        }

        /// <summary>
        /// Nafise: Fetching ScheduledID from lstSch_Jobs based on jobID
        /// Amin: Change to have same date chain
        /// </summary>
        /// <param name="jobID"></param>
        /// <returns></returns>
        private long FetchScdeduleID(long jobID, string date)
        {
            long schID = -1;
            string[] stTemp;


            List<string> match = lstSch_Jobs.FindAll(stringToCheck => stringToCheck.Contains("<" + jobID.ToString() + "," + date + ","));
            if (match.Count == 1)
            {
                stTemp = match[0].Split(',');
                if (stTemp.Length == 3)
                {
                    return long.Parse(stTemp[2].Split('>')[0]);
                }
            }

            return schID;
        }

        /// <summary>
        /// Nafise: Fetching ScheduledID from lstSch_Jobs based on jobID
        /// Amin: Change to have same date chain
        /// Amin: To check list of job_time_IDs
        /// </summary>
        /// <param name="jobID"></param>
        /// <returns></returns>
        private long FetchScdeduleIDList(List<long> jobTimeID, string date)
        {
            long schID = -1;
            string[] stTemp;
            for (int i = 0; i < jobTimeID.Count; i++)
            {
                List<string> match = lstSch_Jobs.FindAll(stringToCheck => stringToCheck.Contains("<" + jobTimeID[i].ToString() + "," + date + ","));
                if (match.Count == 1)
                {
                    stTemp = match[0].Split(',');
                    if (stTemp.Length == 3)
                    {
                        return long.Parse(stTemp[2].Split('>')[0]);
                    }
                }
            }
            return schID;
        }




        private void fillJobPannels()
        {
            DateTime Starttodtom = DateTime.Now;
            DateTime Endtodtom = DateTime.Now;
            DateTime Endtodtom2 = DateTime.Now;
            int plustime = 06;
            List<string> lstJobStatus = new List<string>();
            List<string> selecteditems = new List<string>();
            DataTable Scheduled_Job_Details = new DataTable();
            string stTodaytillStart = "", stTomtillStart = "", stHoldtillStart = "";
            int exceedtime = 2400 - int.Parse(DateTime.Now.ToString("HHmm"));

            //Not passing
            if ((plustime * 100) <= exceedtime)
            {
                stTodaytillStart = DateTime.Now.AddHours(plustime).TimeOfDay.ToString().Replace(":", "").Substring(0, 4);
                stHoldtillStart = stTodaytillStart;
                Endtodtom = DateTime.Now;
            }
            else //passing midnight
            {
                stTodaytillStart = "2400";
                stTomtillStart = Math.Abs((plustime * 100) - exceedtime).ToString();
                stHoldtillStart = stTomtillStart;
                Endtodtom = DateTime.Now.AddDays(1);
            }
            //Runtape
            if (drSystemName.SelectedItem != null)
            {
                    Scheduled_Job_Details = new DataTable();
                    Scheduled_Job_Details = (new ScheduleJob()).Runtape_FilterList(stSys);
                    lblRuntape.Visible = true;
                    if (Scheduled_Job_Details.Rows.Count == 0)
                    {
                        lblRuntape.Visible = false;
                    }
                    Session["RuntapeJobPanelTable"] = Scheduled_Job_Details;
                    gvRuntape.DataSource = Session["RuntapeJobPanelTable"];
                    gvRuntape.DataBind();
            }
            // HOLD
            Starttodtom = DateTime.Now.AddDays(-30);
            
            lstJobStatus.Clear();
            lstJobStatus.Add("Hold");
            Scheduled_Job_Details = (new ScheduleJob()).Schedule_ID_FilterList(Starttodtom, selecteditems, Endtodtom, stSys, "", "", "", "", "", false, false, false,
lstJobStatus, "", "0000", stHoldtillStart, "", "", true);
            lblOnHoldJobs.Text = "Holds";
            lblOnHoldJobs.Visible = true;
            if (Scheduled_Job_Details.Rows.Count == 0)
            {
                lblOnHoldJobs.Visible = false;
            }
            Session["HoldJobPanelTable"] = Scheduled_Job_Details;
            gvHoldJobList.DataSource = Session["HoldJobPanelTable"];
            gvHoldJobList.DataBind();

            // last hour Auto-END
            Endtodtom = DateTime.Now.AddHours(-1);
            Endtodtom2 = DateTime.Now;

            lstJobStatus.Clear();
            lstJobStatus.Add("ENDED-Successful");
            lstJobStatus.Add("ENDED-Unsuccessful");
            lstJobStatus.Add("Skipped Job");
            Scheduled_Job_Details = (new ScheduleJob()).EndedJobsList(Endtodtom, Endtodtom2, lstJobStatus,stSys);
            //lbLastHourAuto_Endedjobs.Text = "Last hour Ended Jobs";
            //lbLastHourAuto_Endedjobs.Visible = true;
            //if (Scheduled_Job_Details.Rows.Count == 0)
            //{
            //    lbLastHourAuto_Endedjobs.Visible = false;
            //}
            Session["EndedJobPanelTable"] = Scheduled_Job_Details;
            gvAuto_ENDJobList.DataSource = Session["EndedJobPanelTable"];
            gvAuto_ENDJobList.DataBind();


            // Days ago Schedule
            Starttodtom = DateTime.Now.AddDays(-30);
            Endtodtom = DateTime.Now.AddDays(-1);
            lstJobStatus.Clear();
            lstJobStatus.Add("SCHEDULED");
            lstJobStatus.Add("STARTED");
            lstJobStatus.Add("Hold");
            NotFinished_Chained_Jobs_ScheduleID = (new ScheduleJob()).GetChainJobsScheduleID(stSys, Starttodtom, DateTime.Now);
            Scheduled_Job_Details = (new ScheduleJob()).Schedule_ID_FilterList(Starttodtom, selecteditems, Endtodtom, stSys, "", "", "", "", "", false, false, false,
lstJobStatus, "", "", "", "", "", true);
            lblyesterdaysList.Text = "Pendings";
            lblyesterdaysList.Visible = true;
            if (Scheduled_Job_Details.Rows.Count == 0)
            {
                lblyesterdaysList.Visible = false;
            }
            Session["YesterdayJobPanelTable"] = Scheduled_Job_Details;
            gvYesterdaysJobList.DataSource = Session["YesterdayJobPanelTable"];
            gvYesterdaysJobList.DataBind();

            // Today Schedule
            Starttodtom = DateTime.Now;
            Endtodtom = DateTime.Now;
            lstJobStatus.Clear();
            lstJobStatus.Add("SCHEDULED");
            lstJobStatus.Add("STARTED");
            Scheduled_Job_Details = (new ScheduleJob()).Schedule_ID_FilterList(Starttodtom, selecteditems, Endtodtom, stSys, "", "", "", "", "", false, false, false,
lstJobStatus, "", "0000", stTodaytillStart, "", "", true);

            NotMountedTapes = (new ScheduleJob()).NeedTapeToBeMounted(Starttodtom, stSys, stTodaytillStart);

            // --------------------------------------- to order left panel job for today
            DataTable NewTodayOrder=new DataTable();
            for (int i = 0; i < Scheduled_Job_Details.Columns.Count; i++)
            {
                NewTodayOrder.Columns.Add(Scheduled_Job_Details.Columns[i].ColumnName);
            }
            for (int i = 0; i < ScheduleShowOrderToday.Count; i++)
            {
                for (int j = 0; j < Scheduled_Job_Details.Rows.Count; j++)
                {
                    if (Scheduled_Job_Details.Rows[j]["SCHEDULE_ID"].ToString() == Math.Abs(ScheduleShowOrderToday[i]).ToString())
                    {
                        NewTodayOrder.Rows.Add(Scheduled_Job_Details.Rows[j].ItemArray);
                    }                    
                }
            }

            lblTodayList.Text = DateTime.Now.Date.ToShortDateString() ;
            Session["TodayJobPanelTable"] = NewTodayOrder;
            gvTodayJobList.DataSource = Session["TodayJobPanelTable"];
            gvTodayJobList.DataBind();

            // Tomorrow Schedule
            if ((plustime * 100) > exceedtime)
            {
                Starttodtom = DateTime.Now.AddDays(1);
                Endtodtom = DateTime.Now.AddDays(1);
                Scheduled_Job_Details = (new ScheduleJob()).Schedule_ID_FilterList(Starttodtom, selecteditems, Endtodtom, stSys, "", "", "", "", "", false, false, false,
        lstJobStatus, "", "0000", stTomtillStart, "", "", true);
                lblTomorrowList.Text = DateTime.Now.AddDays(1).Date.ToShortDateString();
                lblTomorrowList.Visible = true;
                Session["TomorrowJobPanelTable"] = Scheduled_Job_Details;
                gvTomorrowJobList.DataSource = Session["TomorrowJobPanelTable"];
                gvTomorrowJobList.DataBind();
            }
            else
            {
                lblTomorrowList.Visible = false;
            }
        }

        /// <summary>
        /// Nafise: updating the list after start/end a job
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UC_Job_UserControlButtonClicked(object sender, EventArgs e)
        {
            fillJobPannels();
        }


        /// <summary>
        /// Amin:ordering the scheduled jobs
        /// Nafise: modifying for tomorrow schedule
        /// Amin: Modify again for OUTSCH jobs
        /// </summary>
        /// <param name="Scheduled_Job_Details"></param>
        /// <param name="stReq"></param>
        /// <returns></returns>
        private List<long> scheduling(DataTable Scheduled_Job_Details, string stReq)
        {
            List<long> ScheduleShowOrder = new List<long>();
            //List<long> JobTimeOrders = new List<long>();
            DataTable JobOrders = (new AdminDB()).GetChainsWsysID(stSys);
            List<string> Scheduled_Jobs = new List<string>(), NotimeJobs = new List<string>();

            Chained_Jobs = new List<string>(); // <PRE_JOB_TIME_ID,job_TIME_ID>
            for (int i = 0; i < JobOrders.Rows.Count; i++)
            {
                Chained_Jobs.Add("<" + JobOrders.Rows[i]["predecessor_job_TIME_ID"].ToString()
                                    + "," + JobOrders.Rows[i]["job_TIME_ID"].ToString() + ">");
            }

            if (!stReq.Contains("outsch"))
            {
                for (int i = 0; i < Scheduled_Job_Details.Rows.Count; i++) // <JOB_TIME_ID,SCHEDULE_ID>
                {
                    if (Scheduled_Job_Details.Rows[i]["START_SCHD_TIME"].ToString() != "")
                    {
                        Scheduled_Jobs.Add("<" + Scheduled_Job_Details.Rows[i]["JOB_TIME_ID"].ToString()
                                            + "," + Scheduled_Job_Details.Rows[i]["SCHEDULE_ID"].ToString() + ">");
                        lstSch_Jobs.Add("<" + Scheduled_Job_Details.Rows[i]["JOB_TIME_ID"].ToString()
                                            + "," + Scheduled_Job_Details.Rows[i]["SCHEDULE_DATE"].ToString()
                                            + "," + Scheduled_Job_Details.Rows[i]["SCHEDULE_ID"].ToString() + ">");
                    }
                    else
                    {
                        NotimeJobs.Add("<" + Scheduled_Job_Details.Rows[i]["JOB_TIME_ID"].ToString()
                                            + "," + Scheduled_Job_Details.Rows[i]["SCHEDULE_DATE"].ToString()
                                            + "," + Scheduled_Job_Details.Rows[i]["SCHEDULE_ID"].ToString() + ">");
                    }
                }
                for (int i = 0; i < NotimeJobs.Count; i++)
                {
                    string[] SplitedString = NotimeJobs[i].Split('>')[0].Split('<')[1].Split(',');
                    Scheduled_Jobs.Add("<" + SplitedString[0] + "," + SplitedString[2] + ">");
                    lstSch_Jobs.Add(NotimeJobs[i]);
                }
                ScheduleShowOrder = AdminDB.CheckAndOrderJobs(Scheduled_Jobs, Chained_Jobs);
            }
            else
            {
                List<long> TemproryOrder = new List<long>();
                List<string> Dayswithunfinishedjobs = new List<string>();
                for (int i = 0; i < Scheduled_Job_Details.Rows.Count; i++) // to find days with unfinished jobs 
                {
                    string Aday = Scheduled_Job_Details.Rows[i]["SCHEDULE_DATE"].ToString();
                    if (!Dayswithunfinishedjobs.Contains(Aday))
                        Dayswithunfinishedjobs.Add(Aday);
                }

                for (int d = 0; d < Dayswithunfinishedjobs.Count; d++)
                {
                    NotimeJobs = new List<string>();
                    Scheduled_Jobs = new List<string>();
                    TemproryOrder = new List<long>();
                    for (int i = 0; i < Scheduled_Job_Details.Rows.Count; i++) // <JOB_ID,SCHEDULE_ID>
                    {
                        if (Scheduled_Job_Details.Rows[i]["SCHEDULE_DATE"].ToString() == Dayswithunfinishedjobs[d])
                        {
                            if (Scheduled_Job_Details.Rows[i]["START_SCHD_TIME"].ToString() != "")
                            {
                                Scheduled_Jobs.Add("<" + Scheduled_Job_Details.Rows[i]["JOB_ID"].ToString()
                                                    + "," + Scheduled_Job_Details.Rows[i]["SCHEDULE_ID"].ToString() + ">");
                                lstSch_Jobs.Add("<" + Scheduled_Job_Details.Rows[i]["JOB_ID"].ToString()
                                                    + "," + Scheduled_Job_Details.Rows[i]["SCHEDULE_DATE"].ToString()
                                                    + "," + Scheduled_Job_Details.Rows[i]["SCHEDULE_ID"].ToString() + ">");
                            }
                            else
                            {
                                NotimeJobs.Add("<" + Scheduled_Job_Details.Rows[i]["JOB_ID"].ToString()
                                                    + "," + Scheduled_Job_Details.Rows[i]["SCHEDULE_DATE"].ToString()
                                                    + "," + Scheduled_Job_Details.Rows[i]["SCHEDULE_ID"].ToString() + ">");
                            }
                        }
                    }
                    for (int i = 0; i < NotimeJobs.Count; i++)
                    {
                        string[] SplitedString = NotimeJobs[i].Split('>')[0].Split('<')[1].Split(',');
                        Scheduled_Jobs.Add("<" + SplitedString[0] + "," + SplitedString[2] + ">");
                        lstSch_Jobs.Add(NotimeJobs[i]);
                    }

                    TemproryOrder = AdminDB.CheckAndOrderJobs(Scheduled_Jobs, Chained_Jobs);
                    for (int i = 0; i < TemproryOrder.Count; i++)
                    {
                        ScheduleShowOrder.Add(TemproryOrder[i]);
                    }
                    TemproryOrder.Clear();
                    NotimeJobs.Clear();
                    Scheduled_Jobs.Clear();
                }
            }

            return ScheduleShowOrder;
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Random randonGen = new Random();
            Color randomColor = Color.FromArgb(randonGen.Next(255), randonGen.Next(255), randonGen.Next(255));
            //UC_Job1.Panel1.BackColor = randomColor;
            randomColor = Color.FromArgb(randonGen.Next(255), randonGen.Next(255), randonGen.Next(255));
            //UC_Job2.Panel1.BackColor = randomColor;
            //UC_Job1.SetWarnningColor();
        }

        protected void Timer1_Tick(object sender, EventArgs e)
        {
            //lbServerTime.Text = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
        }

        protected void drSystemName_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Response.Redirect("Default.aspx?schtype=todsch");
            string stOpsSystem = "";
            Session["SELECTED_SYSTEM"] = drSystemName.SelectedIndex;
            HttpCookie Station_Cookie = new HttpCookie("SELECTED_SYSTEM");
            HttpCookie Station_system_name = new HttpCookie("SELECTED_SYSTEM_NAME");
            Station_Cookie.Value = drSystemName.SelectedIndex.ToString();
            Station_system_name.Value = drSystemName.SelectedItem.Text;
            Station_Cookie.Expires = DateTime.Now.AddDays(7);
            Station_system_name.Expires = DateTime.Now.AddDays(7);
            if (drSystemName.SelectedItem != null)
            {
                switch (drSystemName.SelectedItem.Text)
                {
                    //case "WNP":
                    //    //stOpsSystem = SiteMaster.QuickReaderWNP0.ReadOpsSystem();
                    //    stOpsSystem = SiteMaster.QuickReaderWNP1.ReadOpsSystem();
                    //    break;
                    case "FCA":
                        stOpsSystem = SiteMaster.QuickReaderFCA2.ReadOpsSystem();
                        //stOpsSystem = SiteMaster.QuickReaderFCA3.ReadOpsSystem();
                        //stOpsSystem = SiteMaster.QuickReaderFCA4.ReadOpsSystem();
                        //stOpsSystem = SiteMaster.QuickReaderFCA5.ReadOpsSystem();
                        break;
                    case "FOS":
                        stOpsSystem = SiteMaster.QuickReaderFOSI.ReadOpsSystem();
                        //stOpsSystem = SiteMaster.QuickReaderFOSJ.ReadOpsSystem();
                        break;
                    case "PSS":
                        stOpsSystem = SiteMaster.QuickReaderPSSA.ReadOpsSystem();
                        break;
                }
            }
            FillCPU(long.Parse(drSystemName.SelectedValue));
            List<string> selecteditems = new List<string>();
            for (int i = 0; i < chblCPU.Items.Count; i++)
            {
                if (chblCPU.Items[i].Selected)
                {
                    selecteditems.Add(chblCPU.Items[i].Text);
                }
            }
            Session["SELECTED_CPU_ID"] = selecteditems;

           // Session["OperatorSemiAuto"] = chOperator.Checked;
            Response.Cookies.Add(Station_Cookie);
            Response.Cookies.Add(Station_system_name);
            Response.Redirect("Default.aspx");
        }

        
        /// <summary>
        /// Amin: Icons for grids
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        protected void GridIconManager(GridViewRowEventArgs e, GridView grid)
        {
            // Grid ICONS
            DateTime diffrence=DateTime.Now;
            string JobRunTime = grid.DataKeys[e.Row.RowIndex].Values["START_SCHD_TIME"].ToString();
            if (JobRunTime.Length == 4)
            {
                diffrence = DateTime.Now.Subtract(TimeSpan.Parse(JobRunTime.Substring(0, 2) + ":" + JobRunTime.Substring(2, 2)));
            }
            ((System.Web.UI.WebControls.Image)e.Row.FindControl("SLA")).Visible = (grid.DataKeys[e.Row.RowIndex].Values["SLA"].ToString() == "0" ? false : true);
            if (((System.Web.UI.WebControls.Image)e.Row.FindControl("SLA")).Visible)
            {
                e.Row.BackColor = Color.Pink;
            }
            ((System.Web.UI.WebControls.Image)e.Row.FindControl("Pause")).Visible = (grid.DataKeys[e.Row.RowIndex].Values["IS_PAUSABLE"].ToString() == "0" ? false : true);
            ((System.Web.UI.WebControls.Image)e.Row.FindControl("TimeFunction")).Visible = (grid.DataKeys[e.Row.RowIndex].Values["IS_TFT"].ToString() == "0" ? false : true);
            ((System.Web.UI.WebControls.Image)e.Row.FindControl("Stand")).Visible = (grid.DataKeys[e.Row.RowIndex].Values["IS_STANDALONE"].ToString() == "0" ? false : true);
            
            bool TimeSensetive = (grid.DataKeys[e.Row.RowIndex].Values["IS_TIME_SENSETIVE"].ToString() == "0" ? false : true);
            ((System.Web.UI.WebControls.Image)e.Row.FindControl("OnTime")).Visible = TimeSensetive;
            ((System.Web.UI.WebControls.Image)e.Row.FindControl("Rush")).Visible = TimeSensetive;

            if (TimeSensetive )
            {
                if (diffrence.Date < DateTime.Now.Date && !(diffrence.Hour==23 && diffrence.Minute >35)) // 25 min bifore next start time 
                {
                    ((System.Web.UI.WebControls.Image)e.Row.FindControl("Rush")).Visible = false;
                }
                else
                {
                    ((System.Web.UI.WebControls.Image)e.Row.FindControl("OnTime")).Visible = false;
                }

                if (diffrence.Date == DateTime.Now.Date &&
                    !grid.DataKeys[e.Row.RowIndex].Values["JOB_STATUS"].ToString().Contains("START") && grid.ID == "gvTodayJobList")
                {
                    e.Row.Font.Bold = true;
                    e.Row.BackColor = Color.Orange;  
                } 
            }
            
            if (grid.DataKeys[e.Row.RowIndex].Values["JOB_STATUS"].ToString() != "SCHEDULED")
            {
                ((System.Web.UI.WebControls.Image)e.Row.FindControl("OnTime")).Visible = TimeSensetive;
                ((System.Web.UI.WebControls.Image)e.Row.FindControl("Rush")).Visible = false;                
            }
            if (grid.ID == "gvYesterdaysJobList")
            {
                ((System.Web.UI.WebControls.Image)e.Row.FindControl("Rush")).Visible = TimeSensetive;
                ((System.Web.UI.WebControls.Image)e.Row.FindControl("OnTime")).Visible = false;
            } 
            
            if ( grid.ID == "gvTomorrowJobList")
            {
                ((System.Web.UI.WebControls.Image)e.Row.FindControl("Rush")).Visible = false;
                ((System.Web.UI.WebControls.Image)e.Row.FindControl("OnTime")).Visible = TimeSensetive;
            }

            ((System.Web.UI.WebControls.Image)e.Row.FindControl("Input")).Visible = false;
            ((System.Web.UI.WebControls.Image)e.Row.FindControl("Output")).Visible = false;
            int numberoftapes = int.Parse(grid.DataKeys[e.Row.RowIndex].Values["NUMBEROFTAPES"].ToString()),
                numberofinputs = int.Parse(grid.DataKeys[e.Row.RowIndex].Values["HAS_INPUT"].ToString());
            if (numberoftapes > 0)
            {
                if (numberofinputs == numberoftapes)
                {
                    ((System.Web.UI.WebControls.Image)e.Row.FindControl("Input")).Visible = true;
                }
                else
                {
                    ((System.Web.UI.WebControls.Image)e.Row.FindControl("Output")).Visible = true;
                    if (numberofinputs != 0)
                    {
                        ((System.Web.UI.WebControls.Image)e.Row.FindControl("Input")).Visible = true;
                    }
                }                
            }
            
            //------- check mounted tapes
            bool inputTapeFlag = false, outputTapeFlag = false;
            string thisRowScheduleID =grid.DataKeys[e.Row.RowIndex].Values["SCHEDULE_ID"].ToString();
            if (grid.ID == "gvTodayJobList")
            {
                for (int i = 0; i < NotMountedTapes.Rows.Count; i++)
                {
                    if (NotMountedTapes.Rows[i]["SCHEDULE_ID"].ToString() == thisRowScheduleID)
                    {
                        if (NotMountedTapes.Rows[i]["input"].ToString() == "0")
                        {
                            outputTapeFlag = true;
                        }
                        else
                        {
                            inputTapeFlag = true;
                        }
                    }
                }
                ((System.Web.UI.WebControls.Image)e.Row.FindControl("InputChecked")).Visible = ((System.Web.UI.WebControls.Image)e.Row.FindControl("Input")).Visible & !inputTapeFlag;
                ((System.Web.UI.WebControls.Image)e.Row.FindControl("Input")).Visible &= inputTapeFlag;
                ((System.Web.UI.WebControls.Image)e.Row.FindControl("OutputChecked")).Visible = ((System.Web.UI.WebControls.Image)e.Row.FindControl("Output")).Visible & !outputTapeFlag;
                ((System.Web.UI.WebControls.Image)e.Row.FindControl("Output")).Visible &= outputTapeFlag;
            }

            //-------------------------- shaky chain for clock
            ((System.Web.UI.WebControls.Image)e.Row.FindControl("ShakyChain")).Visible = false;
            ((System.Web.UI.WebControls.Image)e.Row.FindControl("Chain")).Visible = false;
            if (Chained_Jobs.FindIndex(item => item.Contains(grid.DataKeys[e.Row.RowIndex].Values["JOB_TIME_ID"].ToString()+">")) > -1)
            {
                ((System.Web.UI.WebControls.Image)e.Row.FindControl("ShakyChain")).Visible = true;
                if (diffrence.Date < DateTime.Now.Date && !(diffrence.Hour == 23 && diffrence.Minute > 35)) // 25 min bifore next start time 
                {
                    ((System.Web.UI.WebControls.Image)e.Row.FindControl("ShakyChain")).Visible = false;
                    ((System.Web.UI.WebControls.Image)e.Row.FindControl("Chain")).Visible = true;
                }
                else
                {
                    for (int k = 0; k < NotFinished_Chained_Jobs_ScheduleID.Rows.Count; k++)
                    {
                        if (NotFinished_Chained_Jobs_ScheduleID.Rows[k]["SCHEDULE_ID"].ToString() == grid.DataKeys[e.Row.RowIndex].Values["SCHEDULE_ID"].ToString())
                        {
                            ((System.Web.UI.WebControls.Image)e.Row.FindControl("ShakyChain")).Visible = false;
                            ((System.Web.UI.WebControls.Image)e.Row.FindControl("Chain")).Visible = true;
                            break;
                        }
                    }
                    if (grid.ID == "gvTomorrowJobList")
                    {
                        ((System.Web.UI.WebControls.Image)e.Row.FindControl("ShakyChain")).Visible = false;
                        ((System.Web.UI.WebControls.Image)e.Row.FindControl("Chain")).Visible = true;
                    }
                    
                }
            }
            else
            {
                ((System.Web.UI.WebControls.Image)e.Row.FindControl("Chain")).Visible = false;
            }
            return;
        }
        
        
        
        protected void gvTodayJobList_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {

                GridIconManager(e,gvTodayJobList);

                string lsDataKeyValue = gvTodayJobList.DataKeys[e.Row.RowIndex].Values["JOB_STATUS"].ToString();
                switch (lsDataKeyValue)
                {
                    case "ENDED-Successful":

                        ((HyperLink)e.Row.Cells[1].Controls[1]).CssClass = "HPL";
                        ((HyperLink)e.Row.Cells[1].Controls[1]).ForeColor = Color.Gray;

                        e.Row.Cells[2].ForeColor = Color.Gray;
                        e.Row.Cells[1].ToolTip = "ENDED-Successful";
                        e.Row.Cells[2].ToolTip = "ENDED-Successful";
                        //e.Row.Cells[1].Font.Bold = true;
                        //e.Row.Cells[2].Font.Bold = true;
                        break;
                    case "ENDED-Unsuccessful":
                        ((HyperLink)e.Row.Cells[1].Controls[1]).CssClass = "HPL";
                        ((HyperLink)e.Row.Cells[1].Controls[1]).ForeColor = System.Drawing.ColorTranslator.FromHtml("#FF0000");
                        e.Row.Cells[2].ForeColor = System.Drawing.ColorTranslator.FromHtml("#FF0000");
                        e.Row.Cells[1].ToolTip = "ENDED-Unsuccessful";
                        e.Row.Cells[2].ToolTip = "ENDED-Unsuccessful";
                        //e.Row.Cells[1].Font.Bold = true;
                        //e.Row.Cells[2].Font.Bold = true;
                        break;
                    case "Skipped Job":
                        ((HyperLink)e.Row.Cells[1].Controls[1]).CssClass = "HPL";
                        ((HyperLink)e.Row.Cells[1].Controls[1]).ForeColor = System.Drawing.ColorTranslator.FromHtml("#FF0000");
                        e.Row.Cells[2].ForeColor = System.Drawing.ColorTranslator.FromHtml("#FF0000");
                        e.Row.Cells[1].ToolTip = "Skipped Job";
                        e.Row.Cells[2].ToolTip = "Skipped Job";
                        //e.Row.Cells[1].Font.Bold = true;
                        //e.Row.Cells[2].Font.Bold = true;
                        break;
                    case "STARTED":
                        ((HyperLink)e.Row.Cells[1].Controls[1]).CssClass = "HPL";
                        ((HyperLink)e.Row.Cells[1].Controls[1]).ForeColor = System.Drawing.ColorTranslator.FromHtml("#14CF20");
                        e.Row.Cells[2].ForeColor = System.Drawing.ColorTranslator.FromHtml("#14CF20");
                        e.Row.Cells[1].ToolTip = "STARTED";
                        e.Row.Cells[2].ToolTip = "STARTED";
                        e.Row.Cells[1].Font.Bold = true;
                        e.Row.Cells[2].Font.Bold = true;
                        break;
                    case "SCHEDULED":

                        ((HyperLink)e.Row.Cells[1].Controls[1]).CssClass = "HPL";
                        ((HyperLink)e.Row.Cells[1].Controls[1]).ForeColor = System.Drawing.ColorTranslator.FromHtml("#0076FF");
                        e.Row.Cells[2].ForeColor = System.Drawing.ColorTranslator.FromHtml("#0076FF");
                        e.Row.Cells[1].ToolTip = "SCHEDULED";
                        e.Row.Cells[2].ToolTip = "SCHEDULED";
                        break;
                }

            }
        }

        protected void drPage_SelectedIndexChanged(object sender, EventArgs e)
        {
            Response.Redirect("Default.aspx?schtype=" + Request.QueryString["schtype"] + "&p=" + drPage.SelectedIndex.ToString() + "&sys=" + drSystemName.SelectedIndex.ToString());
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            Response.Redirect("Default.aspx?schtype=" + Request.QueryString["schtype"] + "&p=" + (drPage.SelectedIndex + 1).ToString() + "&sys=" + drSystemName.SelectedIndex.ToString());
        }

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            Response.Redirect("Default.aspx?schtype=" + Request.QueryString["schtype"] + "&p=" + (drPage.SelectedIndex - 1).ToString() + "&sys=" + drSystemName.SelectedIndex.ToString());
        }

        protected void gvTodayJobList_Sorting(object sender, GridViewSortEventArgs e)
        {
            DataTable dataTable = Session["TodayJobPanelTable"] as DataTable;
            SetSortDirection(SortDireaction);
            if (dataTable != null)
            {
                //Sort the data.
                dataTable.DefaultView.Sort = e.SortExpression + " " + _sortDirection;
                gvTodayJobList.DataSource = dataTable;
                gvTodayJobList.DataBind();
                SortDireaction = _sortDirection;
            }
        }

        protected void SetSortDirection(string sortDirection)
        {
            if (sortDirection == "ASC")
            {
                _sortDirection = "DESC";
            }
            else
            {
                _sortDirection = "ASC";
            }
        }

        protected void gvTomorrowJobList_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                GridIconManager(e,gvTomorrowJobList);
                string lsDataKeyValue = gvTomorrowJobList.DataKeys[e.Row.RowIndex].Values["JOB_STATUS"].ToString();

                switch (lsDataKeyValue)
                {
                    
                    case "ENDED-Successful":
                        ((HyperLink)e.Row.Cells[1].Controls[1]).CssClass = "HPL";
                        ((HyperLink)e.Row.Cells[1].Controls[1]).ForeColor = Color.Gray;
                        e.Row.Cells[2].ForeColor = Color.Gray;
                        e.Row.Cells[1].ToolTip = "ENDED-Successful";
                        e.Row.Cells[2].ToolTip = "ENDED-Successful";
                        //e.Row.Cells[1].Font.Bold = true;
                        //e.Row.Cells[2].Font.Bold = true;
                        break;
                    case "ENDED-Unsuccessful":
                        ((HyperLink)e.Row.Cells[1].Controls[1]).CssClass = "HPL";
                        ((HyperLink)e.Row.Cells[1].Controls[1]).ForeColor = System.Drawing.ColorTranslator.FromHtml("#FF0000");
                        e.Row.Cells[2].ForeColor = System.Drawing.ColorTranslator.FromHtml("#FF0000");
                        e.Row.Cells[1].ToolTip = "ENDED-Unsuccessful";
                        e.Row.Cells[2].ToolTip = "ENDED-Unsuccessful";
                        //e.Row.Cells[1].Font.Bold = true;
                        //e.Row.Cells[2].Font.Bold = true;
                        break;
                    case "Skipped Job":
                        ((HyperLink)e.Row.Cells[1].Controls[1]).CssClass = "HPL";
                        ((HyperLink)e.Row.Cells[1].Controls[1]).ForeColor = System.Drawing.ColorTranslator.FromHtml("#FF0000");
                        e.Row.Cells[2].ForeColor = System.Drawing.ColorTranslator.FromHtml("#FF0000");
                        e.Row.Cells[1].ToolTip = "Skipped Job";
                        e.Row.Cells[2].ToolTip = "Skipped Job";
                        //e.Row.Cells[1].Font.Bold = true;
                        //e.Row.Cells[2].Font.Bold = true;
                        break;
                    case "STARTED":
                        ((HyperLink)e.Row.Cells[1].Controls[1]).CssClass = "HPL";
                        ((HyperLink)e.Row.Cells[1].Controls[1]).ForeColor = System.Drawing.ColorTranslator.FromHtml("#14CF20");
                        e.Row.Cells[2].ForeColor = System.Drawing.ColorTranslator.FromHtml("#14CF20");
                        e.Row.Cells[1].ToolTip = "STARTED";
                        e.Row.Cells[2].ToolTip = "STARTED";
                        //e.Row.Cells[1].Font.Bold = true;
                        //e.Row.Cells[2].Font.Bold = true;
                        break;
                    case "SCHEDULED":
                        ((HyperLink)e.Row.Cells[1].Controls[1]).CssClass = "HPL";
                        ((HyperLink)e.Row.Cells[1].Controls[1]).ForeColor = System.Drawing.ColorTranslator.FromHtml("#0076FF");
                        e.Row.Cells[2].ForeColor = System.Drawing.ColorTranslator.FromHtml("#0076FF");
                        e.Row.Cells[1].ToolTip = "SCHEDULED";
                        e.Row.Cells[2].ToolTip = "SCHEDULED";
                        break;
                }

            }
        }        

        protected void gvYesterdaysJobList_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                GridIconManager(e,gvYesterdaysJobList);
                string lsDataKeyValue = gvYesterdaysJobList.DataKeys[e.Row.RowIndex].Values["JOB_STATUS"].ToString();

                switch (lsDataKeyValue)
                {
                    case "ENDED-Successful":
                        ((HyperLink)e.Row.Cells[1].Controls[1]).CssClass = "HPL";
                        ((HyperLink)e.Row.Cells[1].Controls[1]).ForeColor = Color.Gray;
                        e.Row.Cells[2].ForeColor = Color.Gray;
                        e.Row.Cells[1].ToolTip = "ENDED-Successful";
                        e.Row.Cells[2].ToolTip = "ENDED-Successful";
                        //e.Row.Cells[1].Font.Bold = true;
                        //e.Row.Cells[2].Font.Bold = true;
                        break;
                    case "ENDED-Unsuccessful":
                        ((HyperLink)e.Row.Cells[1].Controls[1]).CssClass = "HPL";
                        ((HyperLink)e.Row.Cells[1].Controls[1]).ForeColor = System.Drawing.ColorTranslator.FromHtml("#FF0000");
                        e.Row.Cells[2].ForeColor = System.Drawing.ColorTranslator.FromHtml("#FF0000");
                        e.Row.Cells[1].ToolTip = "ENDED-Unsuccessful";
                        e.Row.Cells[2].ToolTip = "ENDED-Unsuccessful";
                        //e.Row.Cells[1].Font.Bold = true;
                        //e.Row.Cells[2].Font.Bold = true;
                        break;
                    case "Skipped Job":
                        ((HyperLink)e.Row.Cells[1].Controls[1]).CssClass = "HPL";
                        ((HyperLink)e.Row.Cells[1].Controls[1]).ForeColor = System.Drawing.ColorTranslator.FromHtml("#FF0000");
                        e.Row.Cells[2].ForeColor = System.Drawing.ColorTranslator.FromHtml("#FF0000");
                        e.Row.Cells[1].ToolTip = "Skipped Job";
                        e.Row.Cells[2].ToolTip = "Skipped Job";
                        //e.Row.Cells[1].Font.Bold = true;
                        //e.Row.Cells[2].Font.Bold = true;
                        break;
                    case "STARTED":
                        ((HyperLink)e.Row.Cells[1].Controls[1]).CssClass = "HPL";
                        ((HyperLink)e.Row.Cells[1].Controls[1]).ForeColor = System.Drawing.ColorTranslator.FromHtml("#14CF20");
                        e.Row.Cells[2].ForeColor = System.Drawing.ColorTranslator.FromHtml("#14CF20");
                        e.Row.Cells[1].ToolTip = "STARTED";
                        e.Row.Cells[2].ToolTip = "STARTED";
                        //e.Row.Cells[1].Font.Bold = true;
                        //e.Row.Cells[2].Font.Bold = true;
                        break;
                    case "SCHEDULED":
                        ((HyperLink)e.Row.Cells[1].Controls[1]).CssClass = "HPL";
                        ((HyperLink)e.Row.Cells[1].Controls[1]).ForeColor = System.Drawing.ColorTranslator.FromHtml("#0076FF");
                        e.Row.Cells[2].ForeColor = System.Drawing.ColorTranslator.FromHtml("#0076FF");
                        e.Row.Cells[1].ToolTip = "SCHEDULED";
                        e.Row.Cells[2].ToolTip = "SCHEDULED";
                        break;
                }
                
                
            }
        }

        protected void gvYesterdaysJobList_Sorting(object sender, GridViewSortEventArgs e)
        {
            DataTable dataTable = Session["YesterdayJobPanelTable"] as DataTable;
            SetSortDirection(SortDireaction);
            if (dataTable != null)
            {
                //Sort the data.
                dataTable.DefaultView.Sort = e.SortExpression + " " + _sortDirection;
                gvYesterdaysJobList.DataSource = dataTable;
                gvYesterdaysJobList.DataBind();
                SortDireaction = _sortDirection;
            }
        }

        protected void gvTomorrowJobList_Sorting(object sender, GridViewSortEventArgs e)
        {
            DataTable dataTable = Session["TomorrowJobPanelTable"] as DataTable;
            SetSortDirection(SortDireaction);
            if (dataTable != null)
            {
                //Sort the data.
                dataTable.DefaultView.Sort = e.SortExpression + " " + _sortDirection;
                gvTomorrowJobList.DataSource = dataTable;
                gvTomorrowJobList.DataBind();
                SortDireaction = _sortDirection;
            }
        }

        protected void chHideEndedJobs_CheckedChanged(object sender, EventArgs e)
        {
            Session["HideEndedJobs"] = chHideEndedJobs.Checked;
        }

        /// <summary>
        /// Nafise: Fill chblCPU based on drsystem Value
        /// </summary>
        /// <param name="sysValue"></param>
        public void FillCPU(long sysValue)
        {
            DataTable CPUAll = new DataTable();
            CPUAll = (new AdminDB()).GetCPUList(sysValue, false);
            chblCPU.DataValueField = "CPU_ID";
            chblCPU.DataTextField = "CPU_NAME";
            chblCPU.DataSource = CPUAll;
            chblCPU.DataBind();            
        }

        protected void chOperator_CheckedChanged(object sender, EventArgs e)
        {
            if (drSystemName.SelectedItem != null)
            {
                switch (drSystemName.SelectedItem.Text)
                {
                    //case "WNP":
                    //    chOperator.Visible = SiteMaster.QuickReaderWNP0.UpdateOpsSystem(User.Identity.Name, chOperator.Checked)
                    //                        && SiteMaster.QuickReaderWNP1.UpdateOpsSystem(User.Identity.Name, chOperator.Checked);
                    //    break;
                    case "FCA":
                        chOperator.Visible = SiteMaster.QuickReaderFCA2.UpdateOpsSystem(User.Identity.Name, chOperator.Checked)
                                            && SiteMaster.QuickReaderFCA3.UpdateOpsSystem(User.Identity.Name, chOperator.Checked)
                                            && SiteMaster.QuickReaderFCA4.UpdateOpsSystem(User.Identity.Name, chOperator.Checked)
                                            && SiteMaster.QuickReaderFCA5.UpdateOpsSystem(User.Identity.Name, chOperator.Checked);
                        break;
                    case "FOS":
                        chOperator.Visible = SiteMaster.QuickReaderFOSI.UpdateOpsSystem(User.Identity.Name, chOperator.Checked)
                                            && SiteMaster.QuickReaderFOSJ.UpdateOpsSystem(User.Identity.Name, chOperator.Checked);
                        break;
                    case "PSS":
                        chOperator.Visible = SiteMaster.QuickReaderPSSA.UpdateOpsSystem(User.Identity.Name, chOperator.Checked)
                                            && SiteMaster.QuickReaderPSSB.UpdateOpsSystem(User.Identity.Name, chOperator.Checked)
                                            && SiteMaster.QuickReaderPSSC.UpdateOpsSystem(User.Identity.Name, chOperator.Checked)
                                            && SiteMaster.QuickReaderPSSD.UpdateOpsSystem(User.Identity.Name, chOperator.Checked)
                                            && SiteMaster.QuickReaderPSSE.UpdateOpsSystem(User.Identity.Name, chOperator.Checked)
                                            && SiteMaster.QuickReaderPSSF.UpdateOpsSystem(User.Identity.Name, chOperator.Checked)
                                            && SiteMaster.QuickReaderPSSG.UpdateOpsSystem(User.Identity.Name, chOperator.Checked)
                                            && SiteMaster.QuickReaderPSSH.UpdateOpsSystem(User.Identity.Name, chOperator.Checked);
                        break;
                }
                if (chOperator.Checked == false)
                {
                    chOperatorEndTime.Visible = false;
                    chOperatorEndTime.Checked = false;                            
                    switch (drSystemName.SelectedItem.Text)
                    {
                        //case "WNP":
                        //    SiteMaster.QuickReaderWNP0.UpdateOpsEnd(User.Identity.Name, false);
                        //    SiteMaster.QuickReaderWNP1.UpdateOpsEnd(User.Identity.Name, false);
                        //    break;
                        case "FCA":
                            SiteMaster.QuickReaderFCA2.UpdateOpsEnd(User.Identity.Name, false);
                            SiteMaster.QuickReaderFCA3.UpdateOpsEnd(User.Identity.Name, false);
                            SiteMaster.QuickReaderFCA4.UpdateOpsEnd(User.Identity.Name, false);
                            SiteMaster.QuickReaderFCA5.UpdateOpsEnd(User.Identity.Name, false);
                            break;
                        case "FOS":
                            SiteMaster.QuickReaderFOSI.UpdateOpsEnd(User.Identity.Name, false);
                            SiteMaster.QuickReaderFOSJ.UpdateOpsEnd(User.Identity.Name, false);
                            break;
                        case "PSS":
                            SiteMaster.QuickReaderPSSA.UpdateOpsEnd(User.Identity.Name, false);
                            SiteMaster.QuickReaderPSSB.UpdateOpsEnd(User.Identity.Name, false);
                            SiteMaster.QuickReaderPSSC.UpdateOpsEnd(User.Identity.Name, false);
                            SiteMaster.QuickReaderPSSD.UpdateOpsEnd(User.Identity.Name, false);
                            SiteMaster.QuickReaderPSSE.UpdateOpsEnd(User.Identity.Name, false);
                            SiteMaster.QuickReaderPSSF.UpdateOpsEnd(User.Identity.Name, false);
                            SiteMaster.QuickReaderPSSG.UpdateOpsEnd(User.Identity.Name, false);
                            SiteMaster.QuickReaderPSSH.UpdateOpsEnd(User.Identity.Name, false);
                            break;
                    }
                }
            }
            AdminDB.LogIt_UserActions(drSystemName.SelectedItem.Text + " User Auto-pilot: " 
                + chOperator.Checked.ToString() + " by " + HttpContext.Current.User.Identity.Name);
        }

        protected void chblCPU_SelectedIndexChanged(object sender, EventArgs e)
        {
            List<string> selecteditems = new List<string>();
            for (int i = 0; i < chblCPU.Items.Count; i++)
            {
                if (chblCPU.Items[i].Selected)
                {
                    selecteditems.Add(chblCPU.Items[i].Value);
                }
            }
            Session["SELECTED_CPU_ID"] = selecteditems;
            Response.Redirect("Default.aspx");
        }

        protected void chOperatorEndTime_CheckedChanged(object sender, EventArgs e)
        {
            if (drSystemName.SelectedItem != null && chOperator.Checked == true)
            {
                switch (drSystemName.SelectedItem.Text)
                {
                    //case "WNP":
                    //    chOperatorEndTime.Visible = SiteMaster.QuickReaderWNP0.UpdateOpsEnd(User.Identity.Name, chOperatorEndTime.Checked)
                    //                        && SiteMaster.QuickReaderWNP1.UpdateOpsEnd(User.Identity.Name, chOperatorEndTime.Checked);
                    //    break;
                    case "FCA":
                        chOperatorEndTime.Visible = SiteMaster.QuickReaderFCA2.UpdateOpsEnd(User.Identity.Name, chOperatorEndTime.Checked)
                                            && SiteMaster.QuickReaderFCA3.UpdateOpsEnd(User.Identity.Name, chOperatorEndTime.Checked)
                                            && SiteMaster.QuickReaderFCA4.UpdateOpsEnd(User.Identity.Name, chOperatorEndTime.Checked)
                                            && SiteMaster.QuickReaderFCA5.UpdateOpsEnd(User.Identity.Name, chOperatorEndTime.Checked);
                        break;
                    case "FOS":
                        chOperatorEndTime.Visible = SiteMaster.QuickReaderFOSI.UpdateOpsEnd(User.Identity.Name, chOperatorEndTime.Checked)
                                            && SiteMaster.QuickReaderFOSJ.UpdateOpsEnd(User.Identity.Name, chOperatorEndTime.Checked);
                        break;
                    case "PSS":
                        chOperatorEndTime.Visible = SiteMaster.QuickReaderPSSA.UpdateOpsEnd(User.Identity.Name, chOperatorEndTime.Checked)
                                            && SiteMaster.QuickReaderPSSB.UpdateOpsEnd(User.Identity.Name, chOperatorEndTime.Checked)
                                            && SiteMaster.QuickReaderPSSC.UpdateOpsEnd(User.Identity.Name, chOperatorEndTime.Checked)
                                            && SiteMaster.QuickReaderPSSD.UpdateOpsEnd(User.Identity.Name, chOperatorEndTime.Checked)
                                            && SiteMaster.QuickReaderPSSE.UpdateOpsEnd(User.Identity.Name, chOperatorEndTime.Checked)
                                            && SiteMaster.QuickReaderPSSF.UpdateOpsEnd(User.Identity.Name, chOperatorEndTime.Checked)
                                            && SiteMaster.QuickReaderPSSG.UpdateOpsEnd(User.Identity.Name, chOperatorEndTime.Checked)
                                            && SiteMaster.QuickReaderPSSH.UpdateOpsEnd(User.Identity.Name, chOperatorEndTime.Checked);
                        break;
                }
                chOperator.Visible = chOperatorEndTime.Visible;
            }
            else
            {
                chOperatorEndTime.Checked = false;
            }
            AdminDB.LogIt_UserActions(drSystemName.SelectedItem.Text + " User (Full) Auto-pilot: "
                + chOperator.Checked.ToString() + " by " + HttpContext.Current.User.Identity.Name);
        }


        
    }
}
