using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using System.IO;
using System.Data;
using System.Configuration;
using System.Web.Security;

namespace HP_EYE
{
    public class ScheduleJob
    {
        #region Private Properties

        private long _Schedule_ID;
        private string _Schedule_Date;
        private string _Job_Start_Time;
        private string _Job_End_Time;
        private long _CPU_ID;
        private string _CPU_Name;
        private long _System_ID;
        private string _System_Name;
        private string _Operator_Start;
        private string _Operator_End;
        private bool _Auto_Start;
        private bool _Auto_End;
        private string _Operator_Note;
        private string _Job_Status;
        private Job _job_sch = new Job();
        private List<NoteScheduleJob> _NoteSchedule = new List<NoteScheduleJob>();
        //private string _Job_Schedule_Status;
        #endregion

        #region Public Properties

        public long Schedule_ID
        {
            get
            {
                return _Schedule_ID;
            }
            set
            {
                _Schedule_ID = value;
            }
        }

        public string Schedule_Date
        {
            get
            {
                return _Schedule_Date;
            }
            set
            {
                _Schedule_Date = value;
            }
        }

        public Job Job_sch
        {
            get
            {
                return _job_sch;
            }
        }

        public string Job_Start_Time
        {
            get
            {
                return _Job_Start_Time;
            }
            set
            {
                _Job_Start_Time = value;
            }
        }

        public string Job_End_Time
        {
            get
            {
                return _Job_End_Time;
            }
            set
            {
                _Job_End_Time = value;
            }
        }

        public long CPU_ID
        {
            get
            {
                return _CPU_ID;
            }
            set
            {
                _CPU_ID = value;
            }
        }
        public string CPU_Name
        {
            get
            {
                return _CPU_Name;
            }
            set
            {
                _CPU_Name = value;
            }
        }

        public long System_ID
        {
            get
            {
                return _System_ID;
            }
            set
            {
                _System_ID = value;
            }
        }
        public string System_Name
        {
            get
            {
                return _System_Name;
            }
            set
            {
                _System_Name = value;
            }
        }

        public string Job_Status
        {
            get
            {
                return _Job_Status;
            }
            set
            {
                _Job_Status = value;
            }
        }

        public string Operator_Start
        {
            get
            {
                return _Operator_Start;
            }
            set
            {
                _Operator_Start = value;
            }
        }

        public string Operator_End
        {
            get
            {
                return _Operator_End;
            }
            set
            {
                _Operator_End = value;
            }
        }

        public bool Auto_Start
        {
            get
            {
                return _Auto_Start;
            }
            set
            {
                _Auto_Start = value;
            }
        }

        public bool Auto_End
        {
            get
            {
                return _Auto_End;
            }
            set
            {
                _Auto_End = value;
            }
        }

        public string Operator_Note
        {
            get
            {
                return _Operator_Note;
            }
            set
            {
                _Operator_Note = value;
            }
        }

        //public string Job_Schedule_Status
        //{
        //    get
        //    {
        //        return _Job_Schedule_Status;
        //    }
        //    set
        //    {
        //        _Job_Schedule_Status = value;
        //    }
        //}

        public List<NoteScheduleJob> NoteSchedule
        {
            get
            {
                return _NoteSchedule;
            }
            set
            {
                _NoteSchedule = value;
            }
        }
        #endregion


        /// <summary>
        /// Nafise: To read Note from the Schedule
        /// </summary>
        /// <returns></returns>
        private string ReadNote()
        {
            int AffectedRows = -1;
            string Result = "OK. Done.", CommandString;
            DataTable T = new DataTable();

            if (_Schedule_ID > 0)
            {
                //fill Schedule notes
                CommandString = "SELECT NOTE_ID, SENDTO, SCHEDULE_ID, SCHEDULE_NOTE, OPERATOR, DATE_FORMAT(NOTE_DATETIME, '%m/%d/%Y %T') as NOTE_DATETIME "
                + " FROM schedule_notes where SCHEDULE_ID =" + this._Schedule_ID;
                T = (new AdminDB()).CheckDB(CommandString, out AffectedRows);
                this.NoteSchedule.Clear();
                for (int i = 0; i < T.Rows.Count; i++)
                {
                    NoteScheduleJob NT = new NoteScheduleJob();
                    NT.Note_ID = long.Parse(T.Rows[i]["NOTE_ID"].ToString());
                    NT.Note_Datetime = T.Rows[i]["NOTE_DATETIME"].ToString();
                    NT.Operator = T.Rows[i]["OPERATOR"].ToString();
                    NT.Schedule_ID = long.Parse(T.Rows[i]["SCHEDULE_ID"].ToString());
                    NT.Schedule_Note = T.Rows[i]["SCHEDULE_NOTE"].ToString();
                    this._NoteSchedule.Add(NT);
                }
            }

            return Result;
        }     

        /// <summary>
        /// Nafise: To read information of one Schedule from database
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public string Read_ScheduleJob()
        {
            int AffectedRows = -1;
            string Result = "", CommandString = "";

            if (_Schedule_ID > 0)
            {
                // Fill job detail
                CommandString = "SELECT JOB_TIME_ID, SCHEDULE_DATE, JOB_STATUS, START_TIME, END_TIME, scheduled_jobs.CPU_ID,"
                    + " CPU_NAME, scheduled_jobs.SYSTEM_ID, SYSTEM_name, OPERATOR_START, OPERATOR_END FROM scheduled_jobs LEFT"
                    + " JOIN tpf_cpu ON scheduled_jobs.CPU_ID = tpf_cpu.CPU_ID LEFT JOIN tpf_system ON scheduled_jobs.SYSTEM_ID"
                    + " = tpf_system.SYSTEM_ID WHERE SCHEDULE_ID =" + this._Schedule_ID;

                DataTable T = (new AdminDB()).CheckDB(CommandString, out AffectedRows);
                
                if (T.Rows.Count != 0)
                {
                    //this._Schedule_ID = long.Parse(T.Rows[0]["SCHEDULE_ID"].ToString());
                    this._Job_Start_Time = T.Rows[0]["START_TIME"].ToString();
                    this._Job_End_Time = T.Rows[0]["END_TIME"].ToString();
                    this._Operator_Start = T.Rows[0]["OPERATOR_START"].ToString();
                    this._Operator_End = T.Rows[0]["OPERATOR_END"].ToString();
                    this._Schedule_Date = T.Rows[0]["SCHEDULE_DATE"].ToString();

                    if (T.Rows[0]["CPU_ID"].ToString() != "") // to avoid error for old published scheduled
                    {
                        this._CPU_ID = long.Parse(T.Rows[0]["CPU_ID"].ToString());
                    }
                    else
                    {
                        this._CPU_ID = 1;
                    }
                    this._CPU_Name = T.Rows[0]["CPU_NAME"].ToString();

                    if (T.Rows[0]["SYSTEM_ID"].ToString() != "") // to avoid error for old published scheduled
                    {
                        this._System_ID = long.Parse(T.Rows[0]["SYSTEM_ID"].ToString());
                    }
                    else
                    {
                        this._System_ID = 5;
                    }
                    this._System_Name = T.Rows[0]["SYSTEM_NAME"].ToString();

                    this._Job_Status = T.Rows[0]["JOB_STATUS"].ToString();
                    string[] tempDate = T.Rows[0]["SCHEDULE_DATE"].ToString().Split('/');
                    DateTime ScheduleJobDate= new DateTime(int.Parse(tempDate[2]),int.Parse(tempDate[0]), int.Parse(tempDate[1]));
                    this._job_sch.Read_JobWithJOB_TIME_ID(Convert.ToInt64(T.Rows[0]["JOB_TIME_ID"]),ScheduleJobDate);
                }
                //  Note
                Result += " Note:" + ReadNote();
            }
            else
            {
                Result = "No Schedule was found!";
            }
            return Result;
        }

        /// <summary>
        /// Nafise: getting schedule_ID with using job_name and schedule_date
        /// </summary>
        /// <param name="stJob_name"></param>
        /// <param name="stSchedule_Date"></param>
        /// <returns></returns>
        public long Get_Schedule_ID(string stJob_name, string stSchedule_Date)
        {
            long lgSchedule_ID = 0;

            int AffectedRows = -1;
            string CommandString;
            DataTable T = new DataTable();

            if (stJob_name != "")
            {
                CommandString = "SELECT  SCHEDULE_ID FROM   scheduled_jobs     INNER JOIN"
                                + " job_times ON job_times.JOB_TIME_ID = scheduled_jobs.JOB_TIME_ID"
                                + " inner join job_details on job_details.job_id = job_times.JOB_ID"
                                + " where JOB_NAME = '" + stJob_name + "' and SCHEDULE_DATE = '" + stSchedule_Date + "'";
                T = (new AdminDB()).CheckDB(CommandString, out AffectedRows);

                if (T.Rows.Count == 1)
                {
                    lgSchedule_ID = long.Parse(T.Rows[0]["SCHEDULE_ID"].ToString());
                }
            }

            return lgSchedule_ID;
        }


        public string ShortRead()
        {
            string Result = "", CommandString = "";
            int AffectedRows = -1;
            if (_Schedule_ID > 0)
            {
                // Fill job detail
                CommandString = "SELECT JOB_TIME_ID, SCHEDULE_DATE, JOB_STATUS, START_TIME, END_TIME, scheduled_jobs.CPU_ID,"
                    + " CPU_NAME, scheduled_jobs.SYSTEM_ID, SYSTEM_name, OPERATOR_START, OPERATOR_END FROM scheduled_jobs LEFT"
                    + " JOIN tpf_cpu ON scheduled_jobs.CPU_ID = tpf_cpu.CPU_ID LEFT JOIN tpf_system ON scheduled_jobs.SYSTEM_ID"
                    + " = tpf_system.SYSTEM_ID WHERE SCHEDULE_ID =" + this._Schedule_ID;

                DataTable T = (new AdminDB()).CheckDB(CommandString, out AffectedRows);

                if (T.Rows.Count != 0)
                {
                    //this._Schedule_ID = long.Parse(T.Rows[0]["SCHEDULE_ID"].ToString());
                    this._Job_Start_Time = T.Rows[0]["START_TIME"].ToString();
                    this._Job_End_Time = T.Rows[0]["END_TIME"].ToString();
                    this._Operator_Start = T.Rows[0]["OPERATOR_START"].ToString();
                    this._Operator_End = T.Rows[0]["OPERATOR_END"].ToString();
                    this._Schedule_Date = T.Rows[0]["SCHEDULE_DATE"].ToString();

                    if (T.Rows[0]["CPU_ID"].ToString() != "") // to avoid error for old published scheduled
                    {
                        this._CPU_ID = long.Parse(T.Rows[0]["CPU_ID"].ToString());
                    }
                    else
                    {
                        this._CPU_ID = 1;
                    }
                    this._CPU_Name = T.Rows[0]["CPU_NAME"].ToString();

                    if (T.Rows[0]["SYSTEM_ID"].ToString() != "") // to avoid error for old published scheduled
                    {
                        this._System_ID = long.Parse(T.Rows[0]["SYSTEM_ID"].ToString());
                    }
                    else
                    {
                        this._System_ID = 5;
                    }
                    this._System_Name = T.Rows[0]["SYSTEM_NAME"].ToString();

                    this._Job_Status = T.Rows[0]["JOB_STATUS"].ToString();
                    string[] tempDate = T.Rows[0]["SCHEDULE_DATE"].ToString().Split('/');
                    DateTime ScheduleJobDate = new DateTime(int.Parse(tempDate[2]), int.Parse(tempDate[0]), int.Parse(tempDate[1]));
                    this._job_sch.Read_JobWithJOB_TIME_ID(Convert.ToInt64(T.Rows[0]["JOB_TIME_ID"]),ScheduleJobDate);
                }
                //  Note
                // Result += " Note:" + ReadNote();
            }
            else
            {
                Result = "No Schedule was found!";
            }

            return Result;
        }

        /// <summary>
        /// Nafise
        /// Update scheduled_jobs for start time, end time, cpuId, and Operators
        /// </summary>
        /// <returns></returns>
        public string Update_Scheduled_jobs()
        {
            string Result = "", CommandString;
            int AffectedRows = -1;
            // Update Scheduled_jobs based on start time, end time, cpuId, and Operators
            CommandString = "update scheduled_jobs set JOB_STATUS= '" + this.Job_Status + "', CPU_ID =" + this.CPU_ID;
            if (this.Job_Status == "STARTED")
            {
                CommandString += ", START_TIME = '" + this.Job_Start_Time + "', OPERATOR_START='" + this.Operator_Start + "'";
                CommandString += ", AUTO_START = " + (this._Auto_Start == true ? "1" : "0");

            }
            else if (this.Job_Status.Contains("Skipped"))
            {
                CommandString += ", START_TIME = '" + this.Job_Start_Time + "', OPERATOR_START='" + this.Operator_Start + "'";
                CommandString += ", END_TIME = '" + this.Job_End_Time + "', OPERATOR_END='" + this.Operator_End + "'";
            }
            else if (this.Job_Status.Contains("ENDED"))
            {
                CommandString += ", END_TIME = '" + this.Job_End_Time + "', OPERATOR_END='" + this.Operator_End + "'";
                CommandString += ", AUTO_END = " + (this._Auto_End == true ? "1" : "0");
            }
            CommandString += " where SCHEDULE_ID =" + this.Schedule_ID;

            (new AdminDB()).CheckDB(CommandString, out AffectedRows);
            Result += " updated scheduled_jobs.";
            //---- Copy manager update CopyPlace
            if (Job_sch.Is_CopyManager && Job_Status.Contains("ENDED-Successful"))
            {
                (new CopyJobPlace()).SetProductionDate(Job_sch.Job_ID, this.Job_End_Time);   
            }
            
            return Result;
        }

        public void Update_Scheduled_ALLjobs(long cpuID2 )
        {
            string CommandString;
            DataTable dtSendall;
            int AffectedRows = -1;

            CommandString = "update scheduled_jobs set  CPU_ID= '" + cpuID2.ToString() + "', JOB_STATUS= '" + this.Job_Status + "'";
            if (this.Job_Status == "STARTED")
            {
                CommandString += ", START_TIME = '" + this.Job_Start_Time + "', OPERATOR_START='" + this.Operator_Start + "'";
            }
            if (this.Job_Status.Contains("ENDED") || this.Job_Status.Contains("Skipped"))
            {
                CommandString += ", START_TIME = '" + this.Job_Start_Time + "', OPERATOR_START='" + this.Operator_Start + "'";
                CommandString += ", END_TIME = '" + this.Job_End_Time + "', OPERATOR_END='" + this.Operator_End + "'";
            }
            CommandString += " where  CPU_ID != " + this.CPU_ID + " and SCHEDULE_ID =" + this.Schedule_ID + " and JOB_STATUS = 'STARTED'";

            dtSendall = (new AdminDB()).CheckDB(CommandString, out AffectedRows);
        }

       /// <summary>
       /// Nafise: Ended jobs base on END time range, Schedule Status
       /// </summary>
       /// <param name="dtEndSelectLess"></param>
       /// <param name="dtEndSelectMore"></param>
        /// <param name="lstJobstatus"></param>
       /// <param name="stSystem"></param>
       /// <returns></returns>
        public DataTable EndedJobsList(DateTime dtEndSelectLess,
            DateTime dtEndSelectMore,
            List<string> lstJobstatus,
            string stSystem)
        {
            string CommandString;
            int AffectedRows = -1;
            DataTable Result;

            CommandString = "SELECT distinct SCHEDULE_DATE, DATE_FORMAT(SCHEDULE_JOB_DATETIME, '%m/%d/%Y %T') as SCHEDULE_JOB_DATETIME,"
                + " DATE_FORMAT(START_TIME, '%m/%d/%Y %T') as START_TIME, DATE_FORMAT(START_TIME, '%T') as START_TIME1,"
                + " DATE_FORMAT(END_TIME, '%m/%d/%Y %T') as END_TIME, DATE_FORMAT(END_TIME, '%T') as END_TIME1,"
                + " scheduled_jobs.CPU_ID as RUN_CPU_ID, scheduled_jobs.JOB_TIME_ID, START_SCHD_TIME, "
                + " scheduled_jobs.OPERATOR_START, scheduled_jobs.OPERATOR_END, scheduled_jobs.JOB_STATUS, OPS_JOB_NAME, "
                + " scheduled_jobs.SCHEDULE_ID , scheduled_jobs.SCHEDULED_BY"
                + " FROM scheduled_jobs inner join job_times on job_times.JOB_TIME_ID = scheduled_jobs.JOB_TIME_ID "
                + "  inner join job_details on job_details.job_id = job_times.job_id"
                + " WHERE"
                + " (END_TIME > '" + dtEndSelectLess.ToString("yyyy-MM-dd HH:mm:00") + "'"
                + " AND END_TIME < '" + dtEndSelectMore.ToString("yyyy-MM-dd HH:mm:00") + "')"
                + " and job_times.IS_ACTIVE =1 and job_details.IS_ACTIVE =1 AND scheduled_jobs.SYSTEM_ID = " + stSystem;

            if (lstJobstatus.Count != 0)
            {
                CommandString += " AND (JOB_STATUS like '%" + lstJobstatus[0].ToUpper() + "%'";

                for (int i = 1; i < lstJobstatus.Count; i++)
                {
                    CommandString += " or JOB_STATUS like '%" + lstJobstatus[i].ToUpper() + "%'";
                }
                CommandString += ")";
            }

            Result = (new AdminDB()).CheckDB(CommandString, out AffectedRows);

            return Result;
        }

        /// <summary>
        /// Nafise: Filtering scheduled_ID with parameteres
        /// Amin: added Group by section to this query, Please check if you need to add columns 
        /// </summary>
        /// <param name="dtStartSelect"></param>
        /// <param name="dtEndSelect"></param>
        /// <param name="stSystem"></param>
        /// <param name="stJobname"></param>
        /// <param name="stOpsjobname"></param>
        /// <param name="stOS390Name"></param>
        /// <param name="stTapeName"></param>
        /// <param name="stTapeLabel"></param>
        /// <param name="flSLA"></param>
        /// <param name="flPausable"></param>
        /// <param name="flTimeSensetive"></param>
        /// <param name="lstJobstatus"></param>
        /// <param name="stJobcategory"></param>
        /// <param name="stStartfrom"></param>
        /// <param name="stStarttill"></param>
        /// <param name="stEndfrom"></param>
        /// <param name="stEndtill"></param>
        /// <param name="flgtapecheck"></param>
        /// <returns></returns>
        public DataTable Schedule_ID_FilterList(DateTime dtStartSelect,
            List<string> lstselecteditems, 
            DateTime dtEndSelect,
            string stSystem,
            string stJobname,
            string stOpsjobname,
            string stOS390Name,
            string stTapeName,
            string stTapeLabel,
            bool flSLA,
            bool flPausable,
            bool flTimeSensetive,
            List<string> lstJobstatus,
            string stJobcategory,
            string stStartfrom,
            string stStarttill,
            string stEndfrom,
            string stEndtill,
            bool flgtapecheck)
        {
            string CommandString;
            int AffectedRows = -1;
            DataTable Result;

            CommandString = "Select distinct SCHEDULE_DATE, DATE_FORMAT(SCHEDULE_JOB_DATETIME, '%m/%d/%Y %T') as SCHEDULE_JOB_DATETIME,"
                + " DATE_FORMAT(START_TIME, '%m/%d/%Y %T') as START_TIME, DATE_FORMAT(START_TIME, '%T') as START_TIME1,"
                + " DATE_FORMAT(END_TIME, '%m/%d/%Y %T') as END_TIME, DATE_FORMAT(END_TIME, '%T') as END_TIME1,"
                + " job_details.CPU_ID, t2.CPU_NAME, scheduled_jobs.CPU_ID as RUN_CPU_ID, t1.CPU_NAME as RUN_CPU_NAME,"
                + " scheduled_jobs.OPERATOR_START, scheduled_jobs.OPERATOR_END, scheduled_jobs.JOB_STATUS,"
                + " job_times.JOB_ID, scheduled_jobs.SCHEDULE_ID , job_details.JOB_NAME, job_details.OPS_JOB_NAME,"
                + " job_times.START_SCHD_TIME , job_times.END_SCHD_TIME, job_times.SYSTEM_STAT, scheduled_jobs.SCHEDULED_BY,"
                + " SLA, IS_PAUSABLE, IS_TIME_SENSETIVE, IS_COPYMANAGER, IS_TFT,IS_STANDALONE,job_times.JOB_TIME_ID,NO_DAYLIGHT_SAVINGS";
            if (flgtapecheck)
            {
                CommandString += ", COUNT(Name_OS390) AS NUMBEROFTAPES, ifnull(sum(INPUT),-1) AS HAS_INPUT ";
            }
            CommandString += " from scheduled_jobs inner join  job_times on scheduled_jobs.JOB_TIME_ID = job_times.JOB_TIME_ID"
                + " inner join job_details on job_times.JOB_ID = job_details.JOB_ID left outer JOIN tapes_detail ON"
                + " tapes_detail.JOB_ID = job_details.JOB_ID LEFT OUTER JOIN tpf_cpu AS t1 ON t1.CPU_ID = scheduled_jobs.CPU_ID"
                + " LEFT OUTER JOIN tpf_cpu AS t2 ON t2.CPU_ID = job_details.CPU_ID where scheduled_jobs.SCHEDULE_DATE BETWEEN '"
                + dtStartSelect.Date.ToString("yyyy-MM-dd") + "' and '" + dtEndSelect.Date.ToString("yyyy-MM-dd") + "'";
            if (stSystem != "ANY")
            {
                CommandString += " AND job_details.SYSTEM_ID = " + stSystem;
            }
            if (stJobname != "")
            {
                CommandString += " AND job_details.JOB_NAME LIKE '%" + stJobname + "%'";
            }
            if (flSLA)
            {
                CommandString += " AND job_details.SLA = 1";
            }
            if (flPausable)
            {
                CommandString += " AND job_details.IS_PAUSABLE = 1";
            }
            if (flTimeSensetive)
            {
                CommandString += " AND job_details.IS_TIME_SENSETIVE = 1";
            }
            if (stJobcategory != "")
            {
                CommandString += " AND job_details.JOB_BUSINESS_CATEGORY = '" + stJobcategory.ToUpper() + "'";
            }
            if (lstJobstatus.Count != 0)
            {
                CommandString += " AND (scheduled_jobs.JOB_STATUS like '%" + lstJobstatus[0].ToUpper() + "%'";

                for (int i = 1; i < lstJobstatus.Count; i++)
                {
                    CommandString += " or scheduled_jobs.JOB_STATUS like '%" + lstJobstatus[i].ToUpper() + "%'";
                }
                CommandString += ")";

            }
            if (lstselecteditems.Count != 0)
            {
                CommandString += " AND (job_details.CPU_ID like '" + lstselecteditems[0].ToUpper() + "'";

                for (int i = 1; i < lstselecteditems.Count; i++)
                {
                    CommandString += " or job_details.CPU_ID like '" + lstselecteditems[i].ToUpper() + "'";
                }
                CommandString += ")";
            }
            if (stOpsjobname != "")
            {
                CommandString += " AND job_details.OPS_JOB_NAME LIKE '%" + stOpsjobname + "%'";
            }
            if (stOS390Name != "")
            {
                CommandString += " AND tapes_detail.NAME_OS390 LIKE '%" + stOS390Name + "%'";
            }
            if (stTapeName != "")
            {
                CommandString += " AND tapes_detail.NAME LIKE '%" + stTapeName + "%'";
            }
            if (stStartfrom != "")
            {
                //CommandString += " AND cast( job_times.START_SCHD_TIME as DECIMAL) >= " + stStartfrom
                //    + " AND cast( job_times.START_SCHD_TIME as DECIMAL) <= " + stStarttill; ------------------------------------------- Amin: added scheduled_jobs.JOB_STATUS='STARTED' to
                                                                                            //-------------------------------------------       inculde any started job (need to be reviewed)
                CommandString += " AND (( cast( job_times.START_SCHD_TIME as DECIMAL) >=  " + stStartfrom
                               + " AND cast( job_times.START_SCHD_TIME as DECIMAL) <= " + stStarttill
                               + " ) or job_times.START_SCHD_TIME = '' or scheduled_jobs.JOB_STATUS='STARTED' )";
            }
            if (stEndfrom != "")
            {
                CommandString += " AND cast( job_times.END_SCHD_TIME as DECIMAL) >= " + stEndfrom
                    + " AND cast( job_times.END_SCHD_TIME as DECIMAL) <= " + stEndtill;
            }

            //checking tape
            if (flgtapecheck)
            {
                CommandString += " GROUP BY SCHEDULE_DATE , SCHEDULE_JOB_DATETIME , START_TIME , START_TIME1 , END_TIME ,"
                  + " END_TIME1 , job_details.CPU_ID , scheduled_jobs.OPERATOR_START , scheduled_jobs.OPERATOR_END ,"
                  + " scheduled_jobs.JOB_STATUS , job_times.JOB_ID , scheduled_jobs.SCHEDULE_ID , job_details.JOB_NAME ,"
                  + " job_details.OPS_JOB_NAME , job_times.START_SCHD_TIME , job_times.END_SCHD_TIME ,"
                  + " job_times.SYSTEM_STAT , scheduled_jobs.SCHEDULED_BY , SLA , IS_PAUSABLE , IS_TIME_SENSETIVE";
            }
            CommandString += " Order by SCHEDULE_DATE desc ,SCHEDULE_ID ,START_SCHD_TIME asc, job_details.JOB_NAME";
            Result = (new AdminDB()).CheckDB(CommandString, out AffectedRows);
            for (int i = 0; i < Result.Rows.Count; i++)
            {
                if (Result.Rows[i]["NO_DAYLIGHT_SAVINGS"].ToString() == "1")
                {
                    string[] DateSubStrings=Result.Rows[i]["SCHEDULE_DATE"].ToString().Split('/');
                    DateTime TempDate = new DateTime(int.Parse(DateSubStrings[2]), int.Parse(DateSubStrings[0]), int.Parse(DateSubStrings[1]));
                    Result.Rows[i]["START_SCHD_TIME"] = AdminDB.AdjustNonDaylightTime(Result.Rows[i]["START_SCHD_TIME"].ToString(),TempDate);
                    Result.Rows[i]["END_SCHD_TIME"] = AdminDB.AdjustNonDaylightTime(Result.Rows[i]["END_SCHD_TIME"].ToString(), TempDate);              
                }
            }
            Result.DefaultView.Sort = "SCHEDULE_DATE desc ,START_SCHD_TIME asc";
            Result = Result.DefaultView.ToTable();
            return Result;               
        }

        /// <summary>
        /// Amin: List of the Scheduled jobs for FOS Copy-manager
        /// </summary>
        /// <param name="dtStartSelect"></param>
        /// <param name="dtEndSelect"></param>
        /// <param name="stSystem"></param>
        /// <param name="stJobname"></param>
        /// <param name="stOpsjobname"></param>
        /// <param name="stOS390Name"></param>
        /// <param name="stTapeName"></param>
        /// <param name="stTapeLabel"></param>
        /// <param name="flSLA"></param>
        /// <param name="flPausable"></param>
        /// <param name="flTimeSensetive"></param>
        /// <param name="lstJobstatus"></param>
        /// <param name="stJobcategory"></param>
        /// <param name="stStartfrom"></param>
        /// <param name="stStarttill"></param>
        /// <param name="stEndfrom"></param>
        /// <param name="stEndtill"></param>
        /// <param name="flgtapecheck"></param>
        /// <returns></returns>
        public DataTable CopyManager_Schedule_ID_FilterList(DateTime dtStartSelect, DateTime dtEndSelect, string stSystem, string stJobname)
        {
            string CommandString;
            int AffectedRows = -1;

            CommandString = "Select distinct SCHEDULE_DATE, DATE_FORMAT(SCHEDULE_JOB_DATETIME, '%m/%d/%Y %T') as SCHEDULE_JOB_DATETIME,"
                + " DATE_FORMAT(START_TIME, '%m/%d/%Y %T') as START_TIME, DATE_FORMAT(START_TIME, '%T') as START_TIME1,"
                + " DATE_FORMAT(END_TIME, '%m/%d/%Y %T') as END_TIME, DATE_FORMAT(END_TIME, '%T') as END_TIME1, job_details.CPU_ID,"
                + " t2.CPU_NAME, scheduled_jobs.CPU_ID as RUN_CPU_ID, t1.CPU_NAME as RUN_CPU_NAME, scheduled_jobs.OPERATOR_START,"
                + " scheduled_jobs.OPERATOR_END, scheduled_jobs.JOB_STATUS, job_times.JOB_ID, scheduled_jobs.SCHEDULE_ID,"
                + " job_details.JOB_NAME, job_details.OPS_JOB_NAME, job_times.START_SCHD_TIME, job_times.END_SCHD_TIME,"
                + " job_times.SYSTEM_STAT, scheduled_jobs.SCHEDULED_BY, IS_COPYMANAGER, JOB_DESCRIPTION from scheduled_jobs inner join"
                + " job_times on scheduled_jobs.JOB_TIME_ID = job_times.JOB_TIME_ID inner join job_details on"
                + " job_times.JOB_ID = job_details.JOB_ID LEFT OUTER JOIN tpf_cpu AS t1 ON t1.CPU_ID = scheduled_jobs.CPU_ID LEFT OUTER JOIN"
                + " tpf_cpu AS t2 ON t2.CPU_ID = job_details.CPU_ID where IS_COPYMANAGER = 1 AND scheduled_jobs.SCHEDULE_DATE BETWEEN"
                + " '" + dtStartSelect.Date.ToString("yyyy-MM-dd") + "' and '" + dtEndSelect.Date.ToString("yyyy-MM-dd") + "'";

            if (stSystem != "ANY")
            {
                CommandString += " AND job_details.SYSTEM_ID = " + stSystem;
            }
            if (stJobname != "")
            {
                CommandString += " AND job_details.JOB_NAME LIKE '%" + stJobname + "%'";
            }
            CommandString += " Order by SCHEDULE_DATE desc ,START_SCHD_TIME asc, job_details.JOB_NAME";
            return (new AdminDB()).CheckDB(CommandString, out AffectedRows);
        }

        /// <summary>
        /// Amin: List of the Scheduled jobs for SLA
        /// </summary>
        /// <returns></returns>
        public DataTable SLA_FilterList(DateTime current, string stSystem, string stJobname)
        {
            string CommandString;
            int AffectedRows = -1;

            CommandString = "SELECT scheduled_jobs.JOB_TIME_ID, date_format(SCHEDULE_DATE,'%m/%d/%Y') as 'Date', job_details.JOB_NAME as 'TPF name',"
                + " job_details.OPS_JOB_NAME 'Job name', CONCAT(SUBSTRING(START_SCHD_TIME, 1, 2), ':', SUBSTRING(START_SCHD_TIME, 3, 2), ' - ',"
                + " SUBSTRING(end_SCHD_TIME, 1, 2), ':', SUBSTRING(end_SCHD_TIME, 3, 2)) AS 'SLA Times', JOB_STATUS, CONCAT(tapes_detail.NAME,"
                + " '(', tapes_detail.NAME_OS390, ',', tapes_detail.NAME2_OS390, ')') AS 'Tape(zOS)', CONCAT(DATE_FORMAT(START_TIME, '%H'), ':',"
                + " DATE_FORMAT(START_TIME, '%i'), ' - ', DATE_FORMAT(end_TIME, '%H'), ':', DATE_FORMAT(end_TIME, '%i')) AS 'Run Times' FROM"
                + " scheduled_jobs INNER JOIN job_times ON scheduled_jobs.JOB_TIME_ID = job_times.JOB_TIME_ID INNER JOIN job_details ON"
                + " job_details.JOB_ID = job_times.JOB_ID LEFT JOIN tapes_detail ON job_details.JOB_ID = tapes_detail.JOB_ID WHERE sla = 1 AND"
                + " SCHEDULE_DATE = '"+ current.ToString("yyyy-MM-dd") +"'";

            if (stSystem != "ANY")
            {
                CommandString += " AND job_details.SYSTEM_ID = " + stSystem;
            }
            if (stJobname != "")
            {
                CommandString += " AND job_details.JOB_NAME LIKE '%" + stJobname + "%'";
            }
            CommandString += " Order BY START_TIME";
            return (new AdminDB()).CheckDB(CommandString, out AffectedRows);
        }

        /// <summary>
        /// Nafise: returning list of runtape jobs on schedule page
        /// </summary>
        /// <param name="lsRuntape_Schedule_ID"></param>
        /// <returns></returns>
        public DataTable Runtape_FilterList(string stSys)
        {
            string CommandString;
            int AffectedRows = -1;

            CommandString = "SELECT  job_details.JOB_ID, job_details.JOB_NAME , job_times.START_SCHD_TIME, OPS_JOB_NAME, scheduled_jobs.JOB_STATUS, schedule_runtapes.SCHEDULE_ID,"
                            + " SLA, IS_TIME_SENSETIVE, DATE_FORMAT(RECEIVED_AT, '%Y/%m/%d') AS RECEIVED_ATDATE, "
                            + " DATE_FORMAT(RECEIVED_AT, '%I%i') AS RECEIVED_ATTIME, VOLSER"
                            + " FROM schedule_runtapes INNER JOIN  scheduled_jobs ON scheduled_jobs.SCHEDULE_ID = schedule_runtapes.SCHEDULE_ID"
                            + " INNER JOIN job_times ON job_times.JOB_TIME_ID = scheduled_jobs.JOB_TIME_ID"
                            + " INNER JOIN  job_details ON job_details.job_id = job_times.job_id"
                            + " WHERE scheduled_jobs.JOB_STATUS = 'SCHEDULED' and scheduled_jobs.SYSTEM_ID = " + stSys;
            CommandString += " Order BY RECEIVED_AT";
            return (new AdminDB()).CheckDB(CommandString, out AffectedRows);
        }

        /// <summary>
        /// Nafise: Return list of active incompatible jobs for this schedule_ID
        /// </summary>
        /// <returns></returns> 
        public DataTable Check_ActiveIncomp()
        {
            string CommandString;
            int AffectedRows = -1;

            // looking for those active incompatible jobs for the corresponding schedule_job jobID
            CommandString = "select job_times.JOB_ID, job_details.JOB_NAME, job_details.OPS_JOB_NAME ,scheduled_jobs.SCHEDULE_ID ,"
            + " scheduled_jobs.SCHEDULE_DATE, scheduled_jobs.JOB_STATUS, scheduled_jobs.CPU_ID "
            + " from scheduled_jobs inner join job_times on scheduled_jobs.JOB_TIME_ID = job_times.JOB_TIME_ID "
            + " inner join job_details on job_details.JOB_ID = job_times.JOB_ID where job_times.IS_ACTIVE = 1 and scheduled_jobs.JOB_STATUS = 'STARTED' and  job_times.JOB_ID "
            + " in ("
            + " Select distinct incompatible_jobs.JOB2_ID from  scheduled_jobs inner join job_times on"
            + " scheduled_jobs.JOB_TIME_ID = job_times.JOB_TIME_ID inner join incompatible_jobs on job_times.JOB_ID = incompatible_jobs.JOB1_ID"
            + " where scheduled_jobs.SCHEDULE_ID = " + this.Schedule_ID + " and incompatible_jobs.IS_ACTIVE=1 and job_times.IS_ACTIVE=1"
            + " union"
            + " Select distinct incompatible_jobs.JOB1_ID from scheduled_jobs inner join job_times on"
            + " scheduled_jobs.JOB_TIME_ID = job_times.JOB_TIME_ID inner join incompatible_jobs on job_times.JOB_ID = incompatible_jobs.JOB2_ID"
            + " where scheduled_jobs.SCHEDULE_ID = " + this.Schedule_ID + " and incompatible_jobs.IS_ACTIVE=1 and job_times.IS_ACTIVE=1 "
            + " ) order by scheduled_jobs.SCHEDULE_DATE";

            return (new AdminDB()).CheckDB(CommandString, out AffectedRows);
        }

        /// <summary>
        /// Nafise: this Function updates a job_Status
        /// </summary>
        /// <returns></returns>
        public int UpdateScheduleStatus()
        {
            string CommandString;
            int Result = -1;
            DataTable Temp = new DataTable();
            int AffectedRows = -1;

            if (this.Schedule_ID > 0)
            {
                if (this.Job_Status == "HOLD")
                {
                    CommandString = "SELECT * FROM scheduled_jobs WHERE SCHEDULE_ID=" + Schedule_ID + " AND JOB_STATUS !='SCHEDULED';";
                    (new AdminDB()).CheckDB(CommandString, out AffectedRows);
                    if (AffectedRows > 0)
                    {
                        //Unable to hold, Because this job is NOT on the 'SCHEDULED' status
                        Result = -1;
                        return Result;
                    }
                }
                else if (this.Job_Status == "SCHEDULED")
                {
                    CommandString = "SELECT * FROM scheduled_jobs WHERE SCHEDULE_ID=" + Schedule_ID + " AND JOB_STATUS !='HOLD';";
                    (new AdminDB()).CheckDB(CommandString, out AffectedRows);
                    if (AffectedRows > 0)
                    {
                        //Unable to hold, Because this job is NOT on the 'SCHEDULED' status
                        Result = -1;
                        return Result;
                    }
                }

                CommandString = "UPDATE scheduled_jobs set JOB_STATUS = '" + this.Job_Status + "'  WHERE SCHEDULE_ID=" + Schedule_ID + ";";
                (new AdminDB()).CheckDB(CommandString, out AffectedRows);
                if (AffectedRows > 0)
                    Result = 1;
            }
            return Result;
        }

        /// <summary>
        /// Nafise: this Function holds all jobs of one system on one particular day
        /// </summary>
        /// <returns></returns>
        public int UpdateAllDayScheduleStatus()
        {
            int Result = -1;
            DataTable Temp = new DataTable();
            int AffectedRows = -1;
            string CommandString = "UPDATE scheduled_jobs INNER JOIN job_times ON scheduled_jobs.JOB_TIME_ID = job_times.JOB_TIME_ID"
                    + " INNER JOIN job_details ON job_times.JOB_ID = job_details.JOB_ID  SET JOB_STATUS = '" + Job_Status
                    + "' WHERE scheduled_jobs.SYSTEM_ID = " + this.System_ID + " AND IS_COPYMANAGER=0 AND SCHEDULE_DATE = '" + Schedule_Date;
            if (Job_Status == "HOLD")
            {
                CommandString += "' AND JOB_STATUS = 'SCHEDULED'";
                (new AdminDB()).CheckDB(CommandString, out AffectedRows);
            }
            else if (Job_Status == "SCHEDULED")
            {
                CommandString += "' AND JOB_STATUS = 'HOLD'";
                (new AdminDB()).CheckDB(CommandString, out AffectedRows);             
            }
                  

            if (AffectedRows > 0)
                Result = 1;

            return Result;
        }

        /// <summary>
        /// Amin: delete one jobe from published schedule
        /// </summary>
        /// <returns></returns>
        public string DeleteOneSchedule()
        {
            string Result = " Requested job was deleted.", CommandString;
            DataTable Temp = new DataTable();
            int AffectedRows = -1;
            if (Schedule_ID > 0)
            {
                CommandString = "SELECT * FROM scheduled_jobs WHERE SCHEDULE_ID=" + Schedule_ID + " AND ( JOB_STATUS ='SCHEDULED' OR  JOB_STATUS ='HOLD' ) ;"; // 
                (new AdminDB()).CheckDB(CommandString, out AffectedRows);
                if (AffectedRows <= 0)
                {
                    Result = "Unable to Delete, Because this job is NOT on the 'SCHEDULED' or 'HOLD' status!";
                    return Result;
                }

                //CommandString = "SELECT * FROM schedule_notes WHERE SCHEDULE_ID=" + Schedule_ID + ";";
                //(new AdminDB()).CheckDB(CommandString, out AffectedRows);
                //if (AffectedRows > 0)
                //{
                //    Result = "Unable to Delete, Because there are some NOTES for this job!";
                //    return Result;
                //}

                CommandString = "SELECT * FROM scheduled_tapes WHERE SCHEDULE_ID=" + Schedule_ID + ";";
                (new AdminDB()).CheckDB(CommandString, out AffectedRows);
                if (AffectedRows > 0)
                {
                    Result = "Unable to Delete, Because there are some TAPES VOLSER for this job!";
                    return Result;
                }

                CommandString = "DELETE FROM schedule_notes WHERE SCHEDULE_ID=" + Schedule_ID + ";";
                (new AdminDB()).CheckDB(CommandString, out AffectedRows);
                CommandString = "DELETE FROM scheduled_jobs WHERE SCHEDULE_ID=" + Schedule_ID + ";";
                (new AdminDB()).CheckDB(CommandString, out AffectedRows);
                if (AffectedRows <= 0)
                    Result = "Unable to Delete, Schedule_ID: " + Schedule_ID;
            }
            else
            {
                Result = "Please Provide Schedule_ID!";
            }
            return Result;
        }

        /// <summary>
        /// Nafise: Get all undeletebale schedules for specific day.
        /// </summary>
        /// <param name="selectedDT"></param>
        /// <returns></returns>
        public DataTable GetUndeletableSchedules(DateTime selectedDT, string system_ID)
        {
            string CommandString;
            DataTable T = new DataTable();
            int AffectedRows = -1;

            // looking for those active incompatible jobs for the corresponding schedule_job jobID
            CommandString = "select job_details.OPS_JOB_NAME, job_details.JOB_ID,scheduled_jobs.SCHEDULE_ID  from job_details "
            + " inner join job_times on job_details.JOB_ID = job_times.JOB_ID inner join scheduled_jobs on "
            + " scheduled_jobs.JOB_TIME_ID = job_times.JOB_TIME_ID where scheduled_jobs.SCHEDULE_ID in (select SCHEDULE_ID from "
            + " scheduled_tapes where SCHEDULE_ID in ( SELECT SCHEDULE_ID FROM scheduled_jobs where SYSTEM_ID =" + system_ID + " AND"
            + " SCHEDULE_DATE = '" + selectedDT.ToString("yyyy-MM-dd") + "' ) union select SCHEDULE_ID from schedule_notes where"
            + " SCHEDULE_ID in ( SELECT SCHEDULE_ID FROM scheduled_jobs where SYSTEM_ID =" + system_ID + " AND SCHEDULE_DATE = '"
            + selectedDT.ToString("yyyy-MM-dd") + "') union SELECT SCHEDULE_ID FROM scheduled_jobs where SYSTEM_ID ="
            + system_ID + " AND SCHEDULE_DATE = '" + selectedDT.ToString("yyyy-MM-dd") + "' and JOB_STATUS !='SCHEDULED')";
            return (new AdminDB()).CheckDB(CommandString, out AffectedRows);
        }

        

        public string DeleteAllSchedules(DateTime selectedDT, string system_ID)
        {
            string Result = "", CommandString;
            int AffectedRows = -1;
            CommandString = "Delete FROM scheduled_jobs where SYSTEM_ID =" + system_ID
                            + " AND SCHEDULE_DATE = '" + selectedDT.ToString("yyyy-MM-dd") + "' ";
            Result = "The schedule of " + selectedDT.ToString("G") + ", was deleted.";
            (new AdminDB()).CheckDB(CommandString, out AffectedRows);
            if (AffectedRows <= 0)
                Result = "Unable to Delete this schedule on " + selectedDT.ToString("G");

            return Result;
        }


        /// <summary>
        /// Amin: add one job directly to one day (Current) schedule
        /// </summary>
        /// <param name="Job_Time_ID"></param>
        /// <param name="Current"></param>
        /// <param name="system_ID"></param>
        /// <returns></returns>
        public long AddOneJobToSchedule(long Job_Time_ID, DateTime Current, string system_ID, string CPU_ID, string publisher, string jobStatus)
        {
            string CommandString;
            int AffectedRows = -1;
            long lgScheduleID = -1;
            string cpuID = CPU_ID;
            DataTable dtResult = new DataTable(), T = new DataTable();
            if (Job_Time_ID > 0)
            {
                if (cpuID == null)
                {
                    CommandString = "SELECT job_details.CPU_ID FROM job_details INNER JOIN job_times ON"
                        + " job_details.JOB_ID = job_times.JOB_ID inner join tpf_cpu on tpf_cpu.CPU_ID=job_details.CPU_ID"
                        + " where JOB_TIME_ID =" + Job_Time_ID;
                    T = (new AdminDB()).CheckDB(CommandString, out AffectedRows);
                    if (AffectedRows > 0)
                    {
                        cpuID = T.Rows[0]["CPU_ID"].ToString();
                    }
                }

                CommandString = "INSERT INTO scheduled_jobs (JOB_TIME_ID,CPU_ID,SCHEDULE_DATE,SYSTEM_ID,SCHEDULED_BY,JOB_STATUS) VALUES ("
                    + Job_Time_ID + "," + cpuID + ",'" + Current.ToString("yyyy-MM-dd")
                    + "'," + system_ID + ",'" + publisher + "', '" + jobStatus + "')";
                (new AdminDB()).CheckDB(CommandString, out AffectedRows);

                //returning Schedule_ID
                CommandString = "Select SCHEDULE_ID from scheduled_jobs where JOB_TIME_ID =" + Job_Time_ID
                    + " and SCHEDULE_DATE = '" + Current.ToString("yyyy-MM-dd")
                    + "' and SYSTEM_ID = " + system_ID + " and CPU_ID = " + cpuID
                    + " and SCHEDULED_BY = '" + publisher + "' order by SCHEDULE_ID desc;";
                dtResult = (new AdminDB()).CheckDB(CommandString, out AffectedRows);
                if (dtResult.Rows.Count != 0)
                {
                    lgScheduleID = long.Parse(dtResult.Rows[0]["SCHEDULE_ID"].ToString());
                }
            }

            return lgScheduleID;
        }
        
        /// <summary>
        /// Nafise: Auto Update the schedule based on Ops user name
        /// It first checks if the job is ready and send a note if is necessary
        /// then update the schedule
        /// Also if the system has multi processor check for those jobs which running on different processor on the same time.
        /// </summary>
        /// <param name="OpsUserName"></param>
        public void AutoStartResponseUpdate()
        {

           string job_st  = GetStatusWScheduleID(this.Schedule_ID);
           long  cpuID2;

           if (job_st == "SCHEDULED" )
           {
               this._job_sch.Read_Job_Check(this.Schedule_ID);

               string checkResult = CheckJob();
               if (checkResult != "")
               {
                   addNote(Operator_Start, "Job Name:" + Job_sch.Job_Name_OPS + ", Error:" + checkResult + DateTime.Now.ToString(" (MM/dd/yyyy HH:mm)") , "Auto-Msg", false);
               }
               this._Auto_Start = true;
               Update_Scheduled_jobs();
               AdminDB.LogIt("System Name:" + Job_sch.System_Name
                        + ", Schedule ID: " + Schedule_ID
                        + ", Job ID:" + Job_sch.Job_ID
                        + ", Job Name:" + Job_sch.Job_Name_OPS
                        + ", Auto-Start Job Done.");
           }
           else if (job_st == "STARTED" && (this.System_Name == "FCA" || this.System_Name == "FOS" || this.System_Name == "PSS"))
           {
               this._job_sch.Read_Job_Check(this.Schedule_ID); 
               cpuID2 = (new AdminDB()).GetCPU("ALL", this.System_ID);
               if (cpuID2 != 0 && CPU_ID != cpuID2)
               {
                   this._Auto_Start = true;
                   Update_Scheduled_ALLjobs(cpuID2);
                   AdminDB.LogIt("System Name:" + Job_sch.System_Name
                        + ", Schedule ID: " + Schedule_ID
                        + ", Job ID:" + Job_sch.Job_ID
                        + ", Job Name:" + Job_sch.Job_Name_OPS
                        + ", CPU:" + CPU_Name
                        + ", Auto-Start-All Job Done.");
               }
           }
        }

        /// <summary>
        /// Amin: Auto Update End of a job
        /// </summary>
        /// <param name="OpsUserName"></param>
        public void AutoEndResponseUpdate()
        {
            string job_st = GetStatusWScheduleID(this.Schedule_ID);
            if (job_st == "STARTED")
            {
                this._Auto_End = true;
                Update_Scheduled_jobs();
            }
            else
            {
                AdminDB.LogIt("Schedule ID: " + Schedule_ID + ", Job ID:" + _job_sch.Job_ID
                             + ", Job Name:" + _job_sch.Job_Name_OPS + ", System Name:" + _job_sch.System_Name
                             + ", Auto End Job Error: Job was not started by " + Operator_End);
            }
        }

        /// <summary>
        /// Nafise: adding note to Schedule_Note
        /// </summary>
        private void addNote(string OpsUserName, string stNote, string stSendto, bool flgcarryover)
        {
            NoteScheduleJob notesch = new NoteScheduleJob();
            notesch.Schedule_ID = this.Schedule_ID;
            notesch.Schedule_Note = stNote;
            notesch.Operator = OpsUserName;
            notesch.Sendto = stSendto;
            notesch.Carryover = flgcarryover;
            notesch.AddNote();
        }

        /// <summary>
        /// Nafise: This method checks if the job is ready to start on the schedule. And will send the Notes for the warnnings.
        /// </summary>
        /// <returns></returns>
        private string CheckJob()
        {
            int flgReady = 0;
            string result = "";
            string[] stTemp;
            stTemp = Job_Start_Time.Split(' ');
            DataTable dtTape;

            // cheking no active Incompatible jobs
            DataTable dtActIncomp = Check_ActiveIncomp();

            if (stTemp.Length > 1)
            {
                if (Job_sch.Is_Time_Sensetive
                          && stTemp[0] != ""
                && (stTemp[1] == DateTime.Now.Date.ToString("yyyy-MM-dd")))
                {
                    if (int.Parse(stTemp[0]) > int.Parse(DateTime.Now.ToString("HHmm")))
                    {
                        result += " It is too early to start this job! + ";
                        //txtJob_Start_Time.Text = "";
                        flgReady = -2;
                    }
                }
            }
            string stName = "";

            //check added volser if has tapes: -5
            if (Job_sch.Job_Tape.Count != 0)
            {
                for (int i = 0; i < Job_sch.Job_Tape.Count; i++)
                {
                    dtTape = Job_sch.Job_Tape[i].readTapeVolserWdatatable(Schedule_ID);
                    if (!Job_sch.Job_Tape[i].IsLateMount)
                    {

                        if (dtTape.Rows.Count != 0)
                        {
                            if (Job_sch.Job_Tape[i].Input)
                            {
                                stName = Job_sch.Job_Tape[i].Name;
                            }
                            else
                            {
                                if (stName == Job_sch.Job_Tape[i].Name)
                                {
                                    flgReady = 0;
                                }
                            }
                        }
                        else //(dtTape.Rows.Count == 0)
                        {
                            if (stName == Job_sch.Job_Tape[i].Name)
                            {
                                flgReady = 0;
                                break;
                            }
                            flgReady = -5;
                        }
                    }
                }
                if (flgReady == -5)
                {
                    result += "Put the Volsers on the Tape section. + ";
                }
            }
            if (dtActIncomp.Rows.Count > 0)
            {
                result += " There is still active incompatible job on the system: ";

                for (int i = 0; i < dtActIncomp.Rows.Count; i++)
                {
                    result += "(" + dtActIncomp.Rows[i]["OPS_JOB_NAME"].ToString() + "), ";

                }
                result = result.Substring(0, result.Length - 2);
                result += " + ";
                flgReady = -1;
            }


            // checking Predecessor job is completed
            DataTable rtPred = findPredecessor();
            if (rtPred.Rows.Count > 0)
            {
                if (rtPred.Rows[0]["JOB_STATUS"].ToString() != ""
                    && !(rtPred.Rows[0]["JOB_STATUS"].ToString().Contains("ENDED")
                    || rtPred.Rows[0]["JOB_STATUS"].ToString().Contains("Skipped")))
                {
                    result += " Predecessor job is not ENDED or ABORTED on the system!  ";
                    flgReady = -1;
                }
                else
                {
                    if (flgReady == 0)
                    {
                        flgReady = 1;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Nafise: returning the schedule row of predessor of shceduleID
        /// </summary>
        /// <returns></returns>
        private DataTable findPredecessor()
        {
            DataTable dtResult;
            string CommandString;
            int AffectedRows = -1;
            CommandString = "SELECT * FROM scheduled_jobs where SCHEDULE_DATE = '" + this.Schedule_Date + "' and JOB_TIME_ID in ("
                + " SELECT job_time_orders.PRE_JOB_TIME_ID FROM scheduled_jobs"
                + " inner join job_times on job_times.JOB_TIME_ID = scheduled_jobs.JOB_TIME_ID "
                + " inner join job_time_orders on job_time_orders.JOB_TIME_ID = job_times.JOB_TIME_ID "
                + "  where schedule_id = " + this.Schedule_ID + " and job_time_orders.IS_ACTIVE = 1 " 
                + " and job_times.IS_ACTIVE = 1 )";         
            
            dtResult = (new AdminDB()).CheckDB(CommandString,out AffectedRows);
            return dtResult;
        }
        /// <summary>
        /// Nafise: Returning all jobs that are inactive but they were scheduled, and not ended
        /// </summary>
        /// <returns></returns>
        public DataTable InactJobActSchedule()
        {
            string CommandString;
            int AffectedRows = -1;
            
            // looking for those active incompatible jobs for the corresponding schedule_job jobID
            CommandString = "SELECT job_details.JOB_ID, job_details.JOB_NAME,job_details.OPS_JOB_NAME, job_details.IS_ACTIVE,"
                 + " SCHEDULE_DATE, DATE_FORMAT(SCHEDULE_JOB_DATETIME, '%m/%d/%Y %T') as SCHEDULE_JOB_DATETIME,"
                 + " scheduled_jobs.SCHEDULED_BY, DATE_FORMAT(START_TIME, '%m/%d/%Y %T') as START_TIME, DATE_FORMAT(END_TIME, '%m/%d/%Y %T')"
                 + " as END_TIME, scheduled_jobs.JOB_STATUS, scheduled_jobs.SCHEDULE_ID, START_SCHD_TIME, END_SCHD_TIME, CPU_NAME FROM"
                 + " scheduled_jobs inner join job_times on scheduled_jobs.JOB_TIME_ID = job_times.JOB_TIME_ID inner join job_details on"
                 + " job_times.JOB_ID = job_details.JOB_ID LEFT JOIN tpf_cpu ON tpf_cpu.CPU_ID = scheduled_jobs.CPU_ID where"
                 + " (job_details.IS_ACTIVE = 0 OR job_times.IS_ACTIVE = 0) and not (JOB_STATUS LIKE 'END%' OR JOB_STATUS LIKE 'Skip%')" 
                 + " order by SCHEDULE_ID";

            return (new AdminDB()).CheckDB(CommandString, out AffectedRows);
        }

        





        ///// <summary>
        ///// Nafise: Retun all jobs where not in the schedule.
        ///// </summary>
        ///// <param name="DTdatetime"></param>
        ///// <param name="name"></param>
        ///// <param name="systemID"></param>
        ///// <returns></returns>
        //public DataTable JobsNotinSchedule(DateTime DTdatetime, string name, string systemID)
        //{
        //    string CommandString;
        //    int AffectedRows = -1;          

        //    // looking for those active incompatible jobs for the corresponding schedule_job jobID
        //    CommandString = "select distinct JOB_ID ,JOB_NAME, OPS_JOB_NAME from job_details where job_details.IS_ACTIVE = 1 "
        //        + " and SYSTEM_ID = " + systemID + " and JOB_ID  not in  ( "
        //        + " SELECT job_times.JOB_ID FROM scheduled_jobs inner join job_times  on  scheduled_jobs.JOB_TIME_ID = job_times.JOB_TIME_ID"
        //        + " where job_times.IS_ACTIVE = 1 and  SCHEDULE_DATE = '" + DTdatetime.ToString("yyyy-MM-dd") + "' ) ";

        //    if (name != "")
        //    {
        //        CommandString += "and OPS_JOB_NAME LIKE '%" + name + "%'";
        //    }
        //    return (new AdminDB()).CheckDB(CommandString, out AffectedRows);
        //}

        /// <summary>
        /// Nafise: copy jobs which are not in the schedule
        /// </summary>
        /// <param name="DTdatetime"></param>
        /// <param name="name"></param>
        /// <param name="systemID"></param>
        /// <returns></returns>
        public DataTable CopyJobsNotinSchedule(DateTime DTdatetime, string name, string systemID, int copyManager)
        {
            string CommandString;
            int AffectedRows = -1;          

            // looking for those active incompatible jobs for the corresponding schedule_job jobID
            CommandString = "select distinct JOB_ID ,JOB_NAME, OPS_JOB_NAME from job_details where job_details.IS_ACTIVE = 1 and IS_COPYMANAGER = " + copyManager.ToString()   
                + " and SYSTEM_ID = " + systemID + " and JOB_ID  not in  ( "
                + " SELECT job_times.JOB_ID FROM scheduled_jobs inner join job_times  on  scheduled_jobs.JOB_TIME_ID = job_times.JOB_TIME_ID"
                + " where job_times.IS_ACTIVE = 1 and  SCHEDULE_DATE = '" + DTdatetime.ToString("yyyy-MM-dd") + "' ) ";

            if (name != "")
            {
                CommandString += "and OPS_JOB_NAME LIKE '%" + name + "%'";
            }
            return (new AdminDB()).CheckDB(CommandString, out AffectedRows);
        }

        /// <summary>
        /// Amin: return Schedule ID of chained jobs of one or more days
        /// </summary>
        /// <param name="sysID"></param>
        /// <param name="Day"></param>
        /// <returns></returns>
        public DataTable GetChainJobsScheduleID(string sysID, DateTime From_date, DateTime To_date)
        {
            int AffectedRows = -1;
            string CommandString = "SELECT ta.JOB_STATUS, ta.SCHEDULE_ID AS SCHEDULE_ID,"
                + " tb.SCHEDULE_ID AS predecessor_SCHEDULE_ID, ta.SCHEDULE_DATE FROM job_time_orders"
                + " INNER JOIN scheduled_jobs AS ta ON job_time_orders.JOB_TIME_ID = ta.JOB_TIME_ID"
                + " INNER JOIN scheduled_jobs AS tb ON job_time_orders.PRE_JOB_TIME_ID = tb.JOB_TIME_ID"
                + " WHERE job_time_orders.IS_ACTIVE = 1 AND ta.SCHEDULE_DATE = tb.SCHEDULE_DATE"
                + " AND ta.SYSTEM_ID = " + sysID + " AND tb.SYSTEM_ID = " + sysID + " AND"
                + " ta.SCHEDULE_DATE BETWEEN '" + From_date.ToString("yyyy-MM-dd") + "' AND '"
                + To_date.ToString("yyyy-MM-dd") + "' AND tb.JOB_STATUS NOT LIKE '%ENDED%'"
                + " AND tb.JOB_STATUS NOT LIKE '%skipped%';";

            return (new AdminDB()).CheckDB(CommandString, out AffectedRows);
        }

        /// <summary>
        /// Nafise: Returning job_status of schedule row with schedule_ID
        /// </summary>
        /// <param name="lgscheduleID"></param>
        /// <returns></returns>
        public string GetStatusWScheduleID(long lgscheduleID)
        {
            DataTable Result = new DataTable();
            int AffectedRows = -1;
            string stStatus = "";
            string CommandString = "SELECT SCHEDULE_ID, JOB_STATUS FROM   scheduled_jobs"
            + " WHERE  schedule_id =" + lgscheduleID.ToString();

            Result = (new AdminDB()).CheckDB(CommandString, out AffectedRows);

            if (Result.Rows.Count != 0)
            {
                stStatus = Result.Rows[0]["JOB_STATUS"].ToString();
            }
            return stStatus;
        }


        public DataTable ScheduleDB_Log_Commands(DateTime dtStartSelect, string SystemID)
        {
            string CommandString;
            int AffectedRows = -1;
            CommandString = "select DISTINCT job_command.JOB_ID, OPS_JOB_NAME, JOB_NAME, DATE_FORMAT(START_TIME, '%H%i') AS START_TIME1,"
                + " JOB_NAME, DATE_FORMAT(END_TIME, '%H%i') AS END_TIME1, DATE_FORMAT(LOG_COMMAND_DATETIME, '%H%i') AS LOG_COMMAND_DATETIME1,"
                + " type FROM log_commands INNER JOIN job_command ON log_commands.COMMAND_ID = job_command.COMMAND_ID INNER JOIN job_details"
                + " ON job_command.JOB_ID = job_details.JOB_ID INNER JOIN Job_times ON job_details.Job_ID = Job_times.JOB_ID INNER JOIN"
                + " scheduled_jobs ON scheduled_jobs.JOB_TIME_ID = Job_times.JOB_TIME_ID WHERE job_command.IS_ACTIVE = 1 AND"
                + " scheduled_jobs.SCHEDULE_DATE = '" + dtStartSelect.Date.ToString("yyyy-MM-dd") + "' AND job_times.IS_ACTIVE = 1"
                + " AND job_details.IS_ACTIVE = 1 AND RESPONSE != '' AND job_details.SYSTEM_ID = " + SystemID
                + " AND (Type = 'START' OR Type = 'END' OR Type = 'TIME INITIATED') AND IS_RESPONSE = 1 and scheduled_jobs.SCHEDULE_ID ="
                + " log_commands.SCHEDULE_ID ORDER BY job_details.JOB_NAME , SCHEDULE_DATE DESC , START_SCHD_TIME ASC";

            return (new AdminDB()).CheckDB(CommandString,out AffectedRows);;
        }


        /// <summary>
        /// Amin: To find list of scheduleIDs that need a tape but tape is not mounted
        /// </summary>
        /// <param name="date"></param>
        /// <param name="systemId"></param>
        /// <param name="till_time"></param>
        /// <returns></returns>
        public DataTable NeedTapeToBeMounted(DateTime date, string systemId, string till_time)
        {
            int AffectedRows = -1;
            string commandstring = "SELECT DISTINCT scheduled_jobs.SCHEDULE_ID,tapes_detail.input FROM scheduled_jobs INNER JOIN"
                + " job_times ON job_times.JOB_TIME_ID = scheduled_jobs.JOB_TIME_ID INNER JOIN tapes_detail ON"
                + " tapes_detail.JOB_ID = job_times.JOB_ID WHERE (SCHEDULE_ID,tape_ID) not in (select SCHEDULE_ID,TAPE_ID"
                + " from scheduled_tapes) and scheduled_jobs.SCHEDULE_DATE = '" + date.Date.ToString("yyyy-MM-dd") 
                + "' AND scheduled_jobs.SYSTEM_ID = " + systemId + " AND ( cast( job_times.START_SCHD_TIME as DECIMAL) <= " 
                + till_time + " or job_times.START_SCHD_TIME = '') AND scheduled_jobs.JOB_STATUS IN ('SCHEDULED' , 'STARTED')";

            return (new AdminDB()).CheckDB(commandstring, out AffectedRows);
        }

        /// <summary>
        /// Amin: to collect data for charts
        /// </summary>
        /// <param name="jobID"></param>
        /// <param name="StartQuery"></param>
        /// <param name="NumberOfRuns"></param>
        /// <returns></returns>
        public DataTable RunningTimeChartData(long jobID, bool StartQuery, int NumberOfRuns)
        {
            string TIME="END_TIME";
            int AffectedRows = -1;
            if (StartQuery)
            {
                TIME="START_TIME";
            }
            string commandstring = "SELECT DATE_FORMAT(" + TIME + ", '%H%i') AS S_time, count(DATE_FORMAT(" + TIME + ", '%H%i')) as S_Total FROM"
                + " (select " + TIME + " from scheduled_jobs INNER JOIN job_times ON scheduled_jobs.JOB_TIME_ID = job_times.JOB_TIME_ID where job_ID ="
                + jobID + " and " + TIME + " is not null order by SCHEDULE_DATE desc limit " + NumberOfRuns + " ) as t1 group by S_time";
            DataTable TimeOrder = (new AdminDB()).CheckDB(commandstring + " order by S_time", out AffectedRows);
            DataTable CountOrder = (new AdminDB()).CheckDB(commandstring + " order by S_Total desc", out AffectedRows);
            if (CountOrder.Rows.Count > 50)
            {
                for (int i = 50; i < CountOrder.Rows.Count; i++)
                {
                    string minTime=CountOrder.Rows[i]["S_time"].ToString();
                    for (int j = 0; j < TimeOrder.Rows.Count; j++)
                    {
                        if (TimeOrder.Rows[j]["S_time"].ToString() == minTime)
                        {
                            TimeOrder.Rows.RemoveAt(j);
                            break;
                        }
                    }
                }
            }
            return TimeOrder;
        }

        /// <summary>
        /// Amin: fill scheduled_jobs table for any day
        /// </summary>
        /// <param name="TodayDate"></param>
        /// <returns></returns>
        public List<long> TodayJobs(DateTime TodayDate, string system_id)
        {
            List<long> Results = new List<long>();
            List<string> today = (new AdminDB()).WhatIsToday(TodayDate);
            string CommandString, str1 = "", str2 = "";
            DataTable T_OR = new DataTable();
            int AffectedRows = -1;

            foreach (string t in today)
            {
                if (!t.Contains("Calendar"))
                {
                    str1 += "'" + t.ToString() + "',";
                }
                else
                {
                    str2 += "'" + t.Split('(')[1].Split(')')[0].ToString() + "',";
                }
            }
            str1 = str1.Remove(str1.Length - 1);

            //------------------------- TO BE ADDED WITH NO <AND> CONDITION

            CommandString = "SELECT distinct job_times.JOB_TIME_ID"
                + " FROM calendar_time inner join job_times"
                + " on calendar_time.JOB_TIME_ID=job_times.JOB_TIME_ID"
                + " inner join job_details on job_details.JOB_ID=job_times.JOB_ID"
                + " where SYSTEM_ID='" + system_id + "' and job_times.IS_ACTIVE=1"
                + " and IS_ADDED=1 and (FREQUENCY in (" + str1 + ")";
            if (str2.Length > 0)
            {
                str2 = str2.Remove(str2.Length - 1);
                CommandString += " or (FREQUENCY='CALENDAR' and CALENDAR_NAME in (" + str2 + "))";
            }
            CommandString += " ) and (job_times.JOB_ID not in (SELECT job_id FROM calendar_time"
                                    + " inner join job_times"
                                    + " on calendar_time.JOB_TIME_ID=job_times.JOB_TIME_ID"
                                    + " where LOGIC in ('AND')"
                                    + " and job_times.IS_ACTIVE=1 and IS_ADDED=1"
                                    + " and (FREQUENCY in (" + str1 + ")";
            if (str2.Length > 0)
            {
                CommandString += " or (FREQUENCY='CALENDAR' and CALENDAR_NAME in (" + str2 + "))";
            }
            CommandString += ")))";

            T_OR = (new AdminDB()).CheckDB(CommandString, out AffectedRows);
            for (int i = 0; i < T_OR.Rows.Count; i++)
                Results.Add(long.Parse(T_OR.Rows[i][0].ToString()));
            T_OR.Clear();


            //------------------------- TO BE ADDED WITH <AND> CONDITION

            CommandString = "Select distinct t2.JOB_TIME_ID"
                + " from (select JOB_TIME_ID from calendar_time"
                        + " where JOB_TIME_ID in ( select distinct JOB_TIME_ID from calendar_time"
                                                   + " where calendar_time.IS_ACTIVE=1 and IS_ADDED=1"
                                                   + " and LOGIC = 'AND' and (FREQUENCY in (" + str1 + ")";
            if (str2.Length > 0)
            {
                CommandString += " or (FREQUENCY='CALENDAR' and CALENDAR_NAME in (" + str2 + "))";
            }
            CommandString += ") )) as t2"
                + " inner join  job_times on t2.JOB_TIME_ID=job_times.JOB_TIME_ID"
                + " inner join ( select JOB_ID from job_details"
                                + " where job_details.IS_ACTIVE=1 and SYSTEM_ID='" + system_id
                                + "' ) as t3 on t3.JOB_ID=job_times.JOB_ID"
                + " Where t2.JOB_TIME_ID not in (select JOB_TIME_ID"
                                            + " from (select *"
                                                 + " from calendar_time"
                                                 + " where JOB_TIME_ID in (select distinct JOB_TIME_ID"
                                                                      + " from calendar_time"
                                                                      + " where calendar_time.IS_ACTIVE=1"
                                                                        + " and LOGIC = 'AND' and IS_ADDED=1)) as T1"
                                            + " where (FREQUENCY != 'CALENDAR'"
                                                    + " and FREQUENCY not in (" + str1 + "))";
            if (str2.Length > 0)
            {
                CommandString += " or (FREQUENCY='CALENDAR' and CALENDAR_NAME not in (" + str2 + "))";
            }
            CommandString += " )";

            T_OR = (new AdminDB()).CheckDB(CommandString, out AffectedRows);
            for (int i = 0; i < T_OR.Rows.Count; i++)
                Results.Add(long.Parse(T_OR.Rows[i][0].ToString()));
            T_OR.Clear();

            //------------------------- TO BE DELETED WITH NO <AND> CONDITION

            CommandString = "SELECT distinct job_times.JOB_TIME_ID"
                + " FROM calendar_time inner join job_times"
                + " on calendar_time.JOB_TIME_ID=job_times.JOB_TIME_ID"
                + " inner join job_details on job_details.JOB_ID=job_times.JOB_ID"
                + " where SYSTEM_ID='" + system_id + "' and job_times.IS_ACTIVE=1"
                + " and IS_ADDED=0 and (FREQUENCY in (" + str1 + ")";
            if (str2.Length > 0)
            {
                CommandString += " or (FREQUENCY='CALENDAR' and CALENDAR_NAME in (" + str2 + "))";
            }

            CommandString += " ) and (job_times.JOB_ID not in (SELECT job_id FROM calendar_time"
                                    + " inner join job_times"
                                    + " on calendar_time.JOB_TIME_ID=job_times.JOB_TIME_ID"
                                    + " where LOGIC in ('AND')"
                                    + " and job_times.IS_ACTIVE=1 and IS_ADDED=0"
                                    + " and (FREQUENCY in (" + str1 + ")";
            if (str2.Length > 0)
            {
                CommandString += " or (FREQUENCY='CALENDAR' and CALENDAR_NAME in (" + str2 + "))";
            }
            CommandString += ")))";

            T_OR = (new AdminDB()).CheckDB(CommandString, out AffectedRows);
            for (int i = 0; i < T_OR.Rows.Count; i++)
                Results.Remove(long.Parse(T_OR.Rows[i][0].ToString()));
            T_OR.Clear();

            //------------------------- TO BE DELETED WITH <AND> CONDITION

            CommandString = "Select distinct t2.JOB_TIME_ID"
                + " from (select JOB_TIME_ID from calendar_time"
                        + " where JOB_TIME_ID in ( select distinct JOB_TIME_ID from calendar_time"
                                                   + " where calendar_time.IS_ACTIVE=1 and IS_ADDED=0"
                                                   + " and LOGIC = 'AND' and (FREQUENCY in (" + str1 + ")";
            if (str2.Length > 0)
            {
                CommandString += " or (FREQUENCY='CALENDAR' and CALENDAR_NAME in (" + str2 + "))";
            }
            CommandString += ") )) as t2"
                + " inner join  job_times on t2.JOB_TIME_ID=job_times.JOB_TIME_ID"
                + " inner join ( select JOB_ID from job_details"
                                + " where job_details.IS_ACTIVE=1 and SYSTEM_ID='" + system_id
                                + "' ) as t3 on t3.JOB_ID=job_times.JOB_ID"
                + " Where t2.JOB_TIME_ID not in (select JOB_TIME_ID"
                                            + " from (select *"
                                                 + " from calendar_time"
                                                 + " where JOB_TIME_ID in (select distinct JOB_TIME_ID"
                                                                      + " from calendar_time"
                                                                      + " where calendar_time.IS_ACTIVE=1"
                                                                        + " and LOGIC = 'AND' and IS_ADDED=0)) as T1"
                                            + " where (FREQUENCY != 'CALENDAR'"
                                                    + " and FREQUENCY not in (" + str1 + "))";
            if (str2.Length > 0)
            {
                CommandString += " or (FREQUENCY='CALENDAR' and CALENDAR_NAME not in (" + str2 + "))";
            }
            CommandString += " )";

            T_OR = (new AdminDB()).CheckDB(CommandString, out AffectedRows);
            for (int i = 0; i < T_OR.Rows.Count; i++)
                Results.Add(long.Parse(T_OR.Rows[i][0].ToString()));
            T_OR.Clear();

            return Results;
        }

        /// <summary>
        /// Amin: To order job_time_ids based on their appearance on the schedule 
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="SystemID"></param>
        /// <returns></returns>
        public List<long> OrderJobTimeIDs(List<long> Input, string SystemID, DateTime DateOfSchedule)
        {
            int AffectedRows = -1;
            List<long> Output = new List<long>();
            DataTable Chains = (new AdminDB()).GetChainsWsysID(SystemID);
            List<long> CJobID = new List<long>(), CPreJobID = new List<long>();
            for (int i = 0; i < Chains.Rows.Count; i++)
            {
                CPreJobID.Add(long.Parse(Chains.Rows[i]["Predecessor_job_TIME_ID"].ToString())); //Predecessor_job_TIME_ID
                CJobID.Add(long.Parse(Chains.Rows[i]["job_TIME_ID"].ToString())); // job_TIME_ID
            }

            DataTable Jobs = new DataTable();
            string commandString = "SELECT JOB_TIME_ID, IF(START_SCHD_TIME= '', '9999', START_SCHD_TIME )"
                                    + " as start_time,NO_DAYLIGHT_SAVINGS FROM job_times";
            if (Input.Count > 0)
            {
                commandString += " where JOB_TIME_ID in (";
                for (int i = 0; i < Input.Count; i++)
                {
                    commandString += Input[i].ToString() + ",";
                }
                commandString = commandString.Remove(commandString.Length - 1);
                commandString += ")";
            }
            commandString += " ORDER BY start_time, JOB_TIME_ID";
            Jobs = (new AdminDB()).CheckDB(commandString, out AffectedRows);
            List<string> JobsTimesPairs = new List<string>();
            for (int i = 0; i < Jobs.Rows.Count; i++)
            {
                if (Jobs.Rows[i]["NO_DAYLIGHT_SAVINGS"].ToString() == "1")
                {
                    Jobs.Rows[i]["start_time"] = AdminDB.AdjustNonDaylightTime(Jobs.Rows[i]["start_time"].ToString(), DateOfSchedule);
                }
            }
            Jobs.DefaultView.Sort = "start_time";
            Jobs = Jobs.DefaultView.ToTable();
            for (int i = 0; i < Jobs.Rows.Count; i++) // JOB_TIME_ID,start_time
            {
                string Stime = Jobs.Rows[i]["start_time"].ToString();
                JobsTimesPairs.Add(Jobs.Rows[i]["JOB_TIME_ID"].ToString() + "," + (Stime == "9999" ? "" : Stime));
            }

            int current = 0;
            while (JobsTimesPairs.Count > 0)
            {
                long ID = long.Parse(JobsTimesPairs[current].Split(',')[0]);
                string Stime = JobsTimesPairs[current].Split(',')[1];
                if (CPreJobID.FindAll(x => x == ID).Count > 0) // check for non-chained jobs
                {
                    if (current < JobsTimesPairs.Count - 1 && Stime == JobsTimesPairs[current + 1].Split(',')[0])
                    {
                        current++;
                        continue;
                    }
                }
                Output.Add(ID);
                JobsTimesPairs.RemoveAt(current);

                long TIME_ID = ID;
                bool ChainCheck = true;
                if (CPreJobID.Count > 0)
                {
                    while (ChainCheck)
                    {
                        while (ChainCheck) // to order a chain of ID
                        {
                            for (int j = 0; j < CPreJobID.Count; j++)
                            {
                                if (TIME_ID == CPreJobID[j])
                                {
                                    TIME_ID = CJobID[j];
                                    string temp = JobsTimesPairs.Find(x => x.Contains(TIME_ID + ","));
                                    if (temp != null)
                                    {
                                        Output.Add(TIME_ID);
                                        JobsTimesPairs.Remove(temp);
                                        ChainCheck = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    ChainCheck = false;
                                }
                            }
                        }
                        for (int k = 0; k < CPreJobID.Count; k++) // to find next chain head
                        {
                            if (ID == CPreJobID[k] && JobsTimesPairs.Find(x => x.Contains(CJobID[k] + ",")) != null)
                            {
                                TIME_ID = CJobID[k];
                                ChainCheck = true;
                                break;
                            }
                            else
                            {
                                ChainCheck = false;
                            }
                        }
                    }

                }
                if (current > 0 && Stime != JobsTimesPairs[current].Split(',')[0])
                {
                    current--;
                    continue;
                }
            }
            return Output;
        }

        /// <summary>
        /// Amin: To publish one day schedule and keep the orders of jobs
        /// </summary>
        /// <param name="JobTimesIDs"></param>
        /// <param name="Current"></param>
        /// <param name="system_id"></param>
        /// <param name="publisher"></param>
        /// <returns></returns>
        public string SchedulePublish(List<long> JobTimesIDs, DateTime Current, string system_id, string publisher)
        {
            int AffectedRows = -1;
            long Temp_CPU_ID = -1;

            (new AdminDB()).CheckDB("SELECT * FROM scheduled_jobs WHERE SCHEDULE_DATE='"
                        + Current.ToString("yyyy-MM-dd") + "' AND SYSTEM_ID=" + system_id, out AffectedRows);
            if (AffectedRows > 0)
            {
                return "Unable to publish! Because The schedule of " + Current.ToString("MM/dd/yyyy")
                    + " has already been published. <br/> &nbsp;  To EDIT the schedule, please go to"
                    + " <a href='EditSchedule.aspx' target='_blank'>Edit Schedule</a> page to proceed.";
            }

            for (int i = 0; i < JobTimesIDs.Count; i++)
            {
                string CommandString = "SELECT job_details.CPU_ID FROM job_details INNER JOIN"
                                                        + " job_times ON job_details.JOB_ID = job_times.JOB_ID inner join"
                                                        + " tpf_cpu on tpf_cpu.CPU_ID=job_details.CPU_ID"
                                                        + " where JOB_TIME_ID =" + JobTimesIDs[i];
                DataTable T = new DataTable();
                T = (new AdminDB()).CheckDB(CommandString, out AffectedRows);

                if (AffectedRows > 0)
                {
                    long.TryParse(T.Rows[0]["CPU_ID"].ToString(), out Temp_CPU_ID);
                }
                //  long.TryParse(CheckDB(, out AffectedRows).Rows[0]["CPU_ID"].ToString()
                //  , out Temp_CPU_ID);

                (new AdminDB()).CheckDB("INSERT INTO scheduled_jobs (JOB_TIME_ID,CPU_ID,SCHEDULE_DATE,SYSTEM_ID,SCHEDULED_BY) VALUES ("
                    + JobTimesIDs[i] + "," + Temp_CPU_ID + ",'" + Current.ToString("yyyy-MM-dd")
                    + "'," + system_id + ",'" + publisher + "')", out AffectedRows);
            }
            return "The Schedule was published for " + Current.ToString("MM/dd/yyyy");
        }

        /// <summary>
        /// Nafise: Return jobs are daily
        /// </summary>
        /// <param name="job_times"></param>
        /// <returns></returns>
        public List<long> FreqDaily_Jobs(List<long> JobTimesIDs)
        {
            List<long> Results = new List<long>();
            DataTable T_Daily = new DataTable();
            int AffectedRows = -1;

            string str1 = "";
            for (int i = 0; i < JobTimesIDs.Count; i++)
                str1 += JobTimesIDs[i].ToString() + ",";
            if (str1.Length > 0)
            {
                str1 = str1.Remove(str1.Length - 1);
                string CommandString = "SELECT 	distinct JOB_TIME_ID, FREQUENCY FROM     calendar_time"
                    + " where JOB_TIME_ID in (" + str1 + ")  and FREQUENCY = 'DAILY'";

                T_Daily = (new AdminDB()).CheckDB(CommandString, out AffectedRows);
                for (int i = 0; i < T_Daily.Rows.Count; i++)
                    Results.Add(long.Parse(T_Daily.Rows[i][0].ToString()));
            }

            return Results;
        }

        public DataTable SchedulePreview(List<long> JobTimesIDs, DateTime DayOfSchedule)
        {
            DataTable Results = new DataTable();
            string str1 = "";
            int AffectedRows = -1;
            for (int i = 0; i < JobTimesIDs.Count; i++)
                str1 += JobTimesIDs[i].ToString() + ",";
            if (str1.Length > 0)
            {
                str1 = str1.Remove(str1.Length - 1);

                string CommandString = "SELECT job_times.JOB_ID, JOB_NAME,OPS_JOB_NAME,START_SCHD_TIME,END_SCHD_TIME"
                    + " ,SYSTEM_STAT,JOB_TIME_ID, job_details.CPU_ID, CPU_NAME, NO_DAYLIGHT_SAVINGS from job_details inner"
                    + " join job_times on job_details.JOB_ID=job_times.JOB_ID INNER JOIN tpf_cpu ON job_details.CPU_ID = tpf_cpu.CPU_ID"
                    + " where JOB_TIME_ID in (" + str1 + ") ORDER BY START_SCHD_TIME, NO_DAYLIGHT_SAVINGS DESC";

                Results = (new AdminDB()).CheckDB(CommandString, out AffectedRows);
                for (int i = 0; i < Results.Rows.Count; i++)
                {
                    if (Results.Rows[i]["NO_DAYLIGHT_SAVINGS"].ToString() == "1")
                    {
                        Results.Rows[i]["START_SCHD_TIME"] = AdminDB.AdjustNonDaylightTime(Results.Rows[i]["START_SCHD_TIME"].ToString(), DayOfSchedule);
                    }
                }
                Results.DefaultView.Sort = "START_SCHD_TIME";
            }
            return Results;
        }

        

    }
}