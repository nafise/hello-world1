using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Data;

namespace HP_EYE
{
    public class LogCommandDB
    {
        #region Public Properties
        public long CommandID { get; set; }
        public long ScheduleID { get; set; }
        public string CommandDB { get; set; }
        public string TypeDB { get; set; }
        public DateTime CommandTimeDB { get; set; }
        public string Process { get; set; }
        public string DesiredCPU { get; set; }
        public string Partition { get; set; }
        #endregion

        /// <summary>
        /// Nafise: Adding CommandDBList with time to Database
        /// </summary>
        /// <param name="TempCommandList"></param>
        /// <returns></returns>
        public string AddLogCommandsList(List<LogCommandDB> TempCommandList)
        {
            string Result = "", CommandString;
            int AffectedRows = -1;
            // Add new commandsDB to database
            foreach (LogCommandDB o in TempCommandList)
            {
                CommandString = "INSERT INTO log_commands (COMMAND_ID,SCHEDULE_ID, LOG_COMMAND_DATETIME, IS_RESPONSE)"
                                + " Select " + o.CommandID.ToString() + ", " 
                                + o.ScheduleID.ToString() + " ,'" + o.CommandTimeDB.ToString("yyyy-MM-dd HH:mm:ss") + "', false"
                                + "  Where not exists(select * from log_commands where COMMAND_ID = " + o.CommandID.ToString() +
                                " AND SCHEDULE_ID =" + o.ScheduleID.ToString() + " AND LOG_COMMAND_DATETIME = '" 
                                + o.CommandTimeDB.ToString("yyyy-MM-dd HH:mm:ss") + "' and IS_RESPONSE = false) ";
                (new AdminDB()).CheckDB(CommandString,out AffectedRows);
                if (AffectedRows > 0)
                {
                    Result += " Added CommandsDB.";
                }
                else
                {
                    Result += " Adding CommandsDB failed!";
                }
            }
            return Result;
        }
    }

    public class LogCommandScript
    {
        #region Public Properties
        // public long CommandID { get; set; }
        public long ScriptID { get; set; }
        public string CommandDB { get; set; }
        public int ScriptCommandLine { get; set; }
        //public string TypeDB { get; set; }
        public DateTime CommandTimeDB { get; set; }
        public string Process { get; set; }
        #endregion

        /// <summary>
        /// Nafise: adding Script info to DB
        /// </summary>
        /// <param name="stSriptName"></param>
        /// <param name="stScriptPath"></param>
        /// <param name="systemID"></param>
        /// <param name="stOps"></param>
        /// <returns></returns>
        public bool AddScripttoDB(string stSriptName, string stScriptPath, string systemID, string stOps)
        {
            string CommandString;
            int AffectedRows = -1;

            CommandString = "Insert  into script_details (SCRIPT_NAME, SCRIPT_PATH, SYSTEM_ID,UPLOAD_BY ) values ('"
            + stSriptName + "', '" + stScriptPath + "' , " + systemID + " ,'" + stOps + "')";
            (new AdminDB()).CheckDB(CommandString, out AffectedRows);

            return (AffectedRows > 0 ? true : false);
        }

        /// <summary>
        /// Nafise: return ScriptID using ScriptPath
        /// </summary>
        /// <param name="stScriptPath"></param>
        /// <returns></returns>
        public long getScriptID(string stScriptPath)
        {
            long lgscriptID = 0;
            string CommandString;
            int AffectedRows = -1;
            DataTable dtScript;

            CommandString = "SELECT SCRIPT_ID FROM script_details where SCRIPT_PATH = '" + stScriptPath + "';";

            dtScript = (new AdminDB()).CheckDB(CommandString, out AffectedRows);
            if (dtScript.Rows.Count != 0)
            {
                return long.Parse(dtScript.Rows[0]["SCRIPT_ID"].ToString());
            }
            
            return lgscriptID;
        }
    }

    public class LogResponseDB
    {
        #region Public Properties
        public long CommandID { get; set; }
        public long ScheduleID { get; set; }
        public string ResponseDB { get; set; }
        public string TypeDB { get; set; }
        public DateTime RsponseTimeDB { get; set; }
        public string Process { get; set; }
        public string DesiredCPU { get; set; }
        public string WTOPC_Response { get; set; }
        public string Partition { get; set; }
        #endregion

        

        /// <summary>
        /// Nafise: Adding ResponseDBList with time to Database
        /// </summary>
        /// <param name="TempCommandList"></param>
        /// <returns></returns>
        public string AddLogResponseList(List<LogResponseDB> TempCommandList)
        {
            string Result = "", CommandString;
            int AffectedRows = -1;

            // Add new commandsDB to database
            foreach (LogResponseDB o in TempCommandList)
            {                
                CommandString = "INSERT INTO log_commands (COMMAND_ID,SCHEDULE_ID, LOG_COMMAND_DATETIME, IS_RESPONSE)"
                                + " Select " + o.CommandID.ToString() + ", " + o.ScheduleID.ToString() + " ,'" 
                                + o.RsponseTimeDB.ToString("yyyy-MM-dd HH:mm:ss") + "', true"
                                + "  Where not exists(select * from log_commands where COMMAND_ID = " + o.CommandID.ToString()
                                + " AND SCHEDULE_ID =" + o.ScheduleID.ToString() + " AND LOG_COMMAND_DATETIME = '" 
                                + o.RsponseTimeDB.ToString("yyyy-MM-dd HH:mm:ss") + "' and IS_RESPONSE = true) ";
                (new AdminDB()).CheckDB(CommandString, out AffectedRows);
                if (AffectedRows > 0)
                {
                    Result += " Added ResponsesDB.";
                }
                else
                {
                    Result += " Adding ResponsesDB failed.";
                }
            }            
            return Result;
        }

    }

    public class LogSingleTapeDB
    {
        #region Public Properties

        public long ScheduleID { get; set; }
        public long TapeID { get; set; }
        public string TapeName { get; set; }
        public string volser { get; set; }
        public string Device { get; set; }
        public string CPU { get; set; }
        public string Partition { get; set; }
        public string OS390JobName { get; set; }
        public bool IsInput { get; set; }
        public DateTime RunTapeTimeDB { get; set; }
        public DateTime TapeMountTimeDB { get; set; }
        public DateTime TapeRemoveTimeDB { get; set; }
        public DateTime TapeSubmitTimeDB { get; set; }
        public string BLockType { get; set; }
        public int BlockCount { get; set; }

        #endregion 

        /// <summary>
        /// Amin: add a tape from log
        /// </summary>
        /// <param name="Schedule_ID"></param>
        /// <returns></returns>
        public bool AddTapetoDB()
        {
            string CommandString = "";
            int AffectedRows = -1;
            
            if (TapeID > 0 && ScheduleID > 0 && GetScheudleTapeID() < 0)
            {
                if (TapeMountTimeDB == DateTime.MinValue
                    && this.RunTapeTimeDB == DateTime.MinValue)
                {
                    CommandString = "Insert into scheduled_tapes (TAPE_ID, SCHEDULE_ID, VOLSER) values ('"
                        + this.TapeID + "', " + ScheduleID + ", '" + this.volser + "')";
                }
                else if (TapeMountTimeDB != DateTime.MinValue
                    && this.RunTapeTimeDB == DateTime.MinValue)
                {
                    CommandString = "Insert into scheduled_tapes (TAPE_ID, SCHEDULE_ID, VOLSER, MOUNTED_AT) values ('"
                        + this.TapeID + "', " + ScheduleID + ", '" + this.volser + "', '"
                        + AdminDB.ChangeDateTimeFormat(this.TapeMountTimeDB) + "')";
                }
                
                //Adding Runtape
                else if (TapeMountTimeDB == DateTime.MinValue
                    && this.RunTapeTimeDB != DateTime.MinValue)
                {
                    CommandString = "Insert into scheduled_tapes (TAPE_ID, SCHEDULE_ID, VOLSER, RECEIVED_AT) values ('"
                        + this.TapeID + "', " + ScheduleID + ", '" + this.volser + "', '" 
                        + AdminDB.ChangeDateTimeFormat(this.RunTapeTimeDB) + "')";
                }
                else if (TapeMountTimeDB != DateTime.MinValue
                    && this.RunTapeTimeDB != DateTime.MinValue)
                {
                    CommandString = "Insert into scheduled_tapes (TAPE_ID, SCHEDULE_ID, VOLSER, MOUNTED_AT, RECEIVED_AT) values ('"
                        + this.TapeID + "', " + ScheduleID + ", '" + this.volser + "', '"
                        + AdminDB.ChangeDateTimeFormat(this.TapeMountTimeDB) + "', '"
                        + AdminDB.ChangeDateTimeFormat(this.RunTapeTimeDB) + "')";
                }

                (new AdminDB()).CheckDB(CommandString, out AffectedRows);
            }
            return (AffectedRows>0? true:false);
        }

        /// <summary>
        /// Nafise: adding runtape to Database
        /// </summary>
        /// <returns></returns>
        public bool AddRuntapetoDB()
        {
            string CommandString = "";
            int AffectedRows = -1;

            if ( ScheduleID > 0 )
            {
               CommandString = "Insert into schedule_runtapes (SCHEDULE_ID, VOLSER, RECEIVED_AT) select "
                         + ScheduleID + ", '" + this.volser + "', '"
                         + AdminDB.ChangeDateTimeFormat(this.RunTapeTimeDB) + "'"
                         + " Where not exists (select * from schedule_runtapes where SCHEDULE_ID = "
                         + ScheduleID + " and VOLSER = '" + this.volser + "'and RECEIVED_AT ='"
                         + AdminDB.ChangeDateTimeFormat(this.RunTapeTimeDB) + "')";
                (new AdminDB()).CheckDB(CommandString, out AffectedRows);
            }
            return (AffectedRows > 0 ? true : false);
        }




        /// <summary>
        /// Nafise: if it is a runsheet tape, we first added it to the DB with the recieved time in AddTapetoDB function and
        /// then will update mounted time
        /// </summary>
        /// <returns></returns>
        public bool UpdateTapeMountedtoDB()
        {
            int AffectedRows = -1;
            string CommandString;
            long IDToBeUpdated = GetScheudleTapeID();
            if (TapeID > 0 && ScheduleID > 0 && IDToBeUpdated > 0)
            {
                CommandString = "Update scheduled_tapes Set MOUNTED_AT='" + AdminDB.ChangeDateTimeFormat(this.TapeMountTimeDB) + "'"
                    + " Where SCHEDULED_TAPE_ID = " + IDToBeUpdated;
                (new AdminDB()).CheckDB(CommandString, out AffectedRows);
            }
            return (AffectedRows > 0 ? true : false);
        }

        /// <summary>
        /// Amin: Update camedown time of a tape from log
        /// </summary>
        /// <param name="Schedule_ID"></param>
        /// <returns></returns>
        public bool UpdateTapeCameDowntoDB()
        {
            int AffectedRows = -1;
            string CommandString;
            long IDToBeUpdated=GetScheudleTapeID();
            if (TapeID > 0 && ScheduleID > 0 && IDToBeUpdated > 0)
            {
                CommandString = "Update scheduled_tapes Set CAMEDOWN_AT='" + AdminDB.ChangeDateTimeFormat(this.TapeRemoveTimeDB) + "'"
                    + " Where SCHEDULED_TAPE_ID = "+  IDToBeUpdated;
                (new AdminDB()).CheckDB(CommandString, out AffectedRows);                
            }
            return (AffectedRows > 0 ? true : false);
        }

        /// <summary>
        /// Amin: Update Submit time of a tape from log
        /// </summary>
        /// <param name="Schedule_ID"></param>
        /// <returns></returns>
        public bool UpdateTapeSubmittoDB()
        {
            int AffectedRows = -1;
            string CommandString;
            long IDToBEUpdated = GetScheudleTapeID();
            if (TapeID > 0 && ScheduleID > 0 && IDToBEUpdated > 0)
            {
                CommandString = "Update scheduled_tapes Set SUBMITED_AT='" + AdminDB.ChangeDateTimeFormat(this.TapeSubmitTimeDB) + "'"
                    + " Where SCHEDULED_TAPE_ID = " + IDToBEUpdated;
                (new AdminDB()).CheckDB(CommandString, out AffectedRows);
            }
            return (AffectedRows > 0 ? true : false);
        }

        /// <summary>
        /// Amin: Delete a tape using log information
        /// </summary>
        /// <param name="Schedule_ID"></param>
        /// <returns></returns>
        public bool DeleteTapefromDB()
        {
            int AffectedRows = -1;
            string CommandString;
            long IDToBEUpdated = GetScheudleTapeID();
            if (TapeID > 0 && volser != "" && IDToBEUpdated > 0)
            {
                CommandString = "delete from scheduled_tapes Where SCHEDULED_TAPE_ID = " + IDToBEUpdated;
                (new AdminDB()).CheckDB(CommandString,out AffectedRows);
            }
            return (AffectedRows > 0 ? true : false);
        }

        /// <summary>
        /// Amin: to check a tape is on the Database, will return the Last SCHEDULED_TAPE_ID
        /// </summary>
        /// <returns></returns>
        public long GetScheudleTapeID()
        {
            string CommandString = "Select SCHEDULED_TAPE_ID from scheduled_tapes where";
            bool flag = false;
            int AffectedRows = -1;
            long result=-1;
            if (ScheduleID > 0)
            {
                flag = true;
                CommandString += " SCHEDULE_ID=" + ScheduleID;
            }

            if (TapeID > 0)
            {
                if (flag)
                {
                    CommandString += " AND";
                }
                flag = true;
                CommandString += " TAPE_ID='" + this.TapeID + "'";
            }

            if (volser != "")
            {
                if (flag)
                {
                    CommandString += " AND";
                }
                flag = true;
                CommandString += " VOLSER='" + this.volser + "'";
            }

            CommandString += " Order by TAPE_DATETIME DESC";

            if (flag)
            {
                DataTable T = (new AdminDB()).CheckDB(CommandString,out AffectedRows);
                if (T.Rows.Count > 0)
                {
                    result = long.Parse(T.Rows[0][0].ToString());
                }
            }
            return result;
        }

        /// <summary>
        /// Amin: To retrive a list of all tape related to one job on the published schedule
        /// </summary>
        /// <returns></returns>
        public DataTable TapelistByScheduleID(long ID)
        {
            string CommandString;
            int AffectedRows = -1;

            CommandString = "SELECT tapes_detail.*, scheduled_jobs.CPU_ID, CPU_NAME FROM scheduled_jobs INNER JOIN"
                + " job_times ON job_times.JOB_TIME_ID = scheduled_jobs.JOB_TIME_ID INNER JOIN tapes_detail ON"
                + " tapes_detail.JOB_ID = job_times.JOB_ID INNER JOIN tpf_cpu ON tpf_cpu.CPU_ID = scheduled_jobs.CPU_ID"
                + " where SCHEDULE_ID = " + ID;

            return (new AdminDB()).CheckDB(CommandString,out AffectedRows);
        }

    }

    public class LogMultipleTapeDB
    {
        #region Public Properties

        public long ScheduleID { get; set; }
        public long CommandID { get; set; }
        public string type { get; set; }
        public long TapeID { get; set; }
        public string TapeName { get; set; }
        public string volser { get; set; }
        public string CPU { get; set; }
        public string Partition { get; set; }
        public string startTime { get; set; }
        public string OS390JobName { get; set; }
        public bool lateMounted { get; set; }
        public bool IsInput { get; set; }

        #endregion
    }

    public class LogTapeCountDB
    {
        #region Public Properties

        public string TapeName { get; set; }
        public int Count { get; set; }
        public bool IsInput { get; set; }

        #endregion
    }

    public class WTOPC_ResponseDB
    {
        #region Public Properties

        public string WTOPC_Response { get; set; }

        #endregion
    }

    
}