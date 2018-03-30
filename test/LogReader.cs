using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Diagnostics;
using System.Web.Configuration;
using System.Web.UI.WebControls;



namespace HP_EYE
{

    public class LogReader
    {
        string PDUexcTime = "-1";
        int PDUexcDurtation = -1;
        public bool RecoupActive = false;
        public static string RPO_Step;
        public static string RPO_LastRunTime;
        public List<SystemUtilityDiagrams> UtilityData = new List<SystemUtilityDiagrams>();
        public List<SystemStatusUnits> StatusData = new List<SystemStatusUnits>();
        static int threadNumber = 0;
        public static Dictionary<Thread, DateTime> AllThreadList = new Dictionary<Thread, DateTime>();
        Thread Reader;
        string Result = "";
        Stopwatch timer1 = new Stopwatch();
        int Counter, UsedLinesCounter, EmptyLines;
        string LastUpdate;
        bool FinishIt = false;
        public int sleeptime = 100;
        public bool AdminAutoUpdate = true;
        public bool UserAutoStartTapeUpdate = true;
        public bool UserAutoEndUpdate = true;
        string OpsUserName;
        string path = "", sysName = "", processName = "";
        DateTime CreatedOn;
        long Thread_sysID, Thread_ProcessID;
        public DateTime DateOfLog;
        string Thread_partitionName, Thread_processorName_fromLOG;
        static readonly object Threadlocker = new object();

        List<string> MultipleLinesDisplayStorage = new List<string>();

        List<JobCalendar> _LogCalendars = new List<JobCalendar>();
        public List<PoolInfo> PoolUsageExceededList = new List<PoolInfo>();
        public List<PoolInfo> PoolInUseList = new List<PoolInfo>();
        public List<PoolInfo> PoolMinuteList = new List<PoolInfo>();
        static List<LogCommandScript> _LogCommandScriptWNP = new List<LogCommandScript>();
        static List<LogCommandScript> _LogCommandScriptFCA = new List<LogCommandScript>();
        static List<LogCommandScript> _LogCommandScriptFOS = new List<LogCommandScript>();
        static List<LogCommandScript> _LogCommandScriptPSS = new List<LogCommandScript>();

        static List<LogMultipleTapeDB> DBTapeListMultiplesWNP = new List<LogMultipleTapeDB>();
        static List<LogMultipleTapeDB> DBTapeListMultiplesFCA = new List<LogMultipleTapeDB>();
        static List<LogMultipleTapeDB> DBTapeListMultiplesFOS = new List<LogMultipleTapeDB>();
        static List<LogMultipleTapeDB> DBTapeListMultiplesPSS = new List<LogMultipleTapeDB>();

        List<LogDump> _Dumplist = new List<LogDump>();
        List<string> DumpVolSer = new List<string>(); // sequence#,VolSer#
        List<LogSnapDump> _SnapDumplist = new List<LogSnapDump>();
        
        static List<LogTapeCountDB> DBTapeListCountWNP = new List<LogTapeCountDB>();
        static List<LogTapeCountDB> DBTapeListCountFCA = new List<LogTapeCountDB>();
        static List<LogTapeCountDB> DBTapeListCountFOS = new List<LogTapeCountDB>();
        static List<LogTapeCountDB> DBTapeListCountPSS = new List<LogTapeCountDB>();

        static List<LogSingleTapeDB> _DBFinalTapeListWNP = new List<LogSingleTapeDB>();
        static List<LogSingleTapeDB> _DBFinalTapeListFCA = new List<LogSingleTapeDB>();
        static List<LogSingleTapeDB> _DBFinalTapeListFOS = new List<LogSingleTapeDB>();
        static List<LogSingleTapeDB> _DBFinalTapeListPSS = new List<LogSingleTapeDB>();


        static List<LogCommandDB> _DBCommandListWNP = new List<LogCommandDB>();
        static List<LogCommandDB> _DBCommandListFCA = new List<LogCommandDB>();
        static List<LogCommandDB> _DBCommandListFOS = new List<LogCommandDB>();
        static List<LogCommandDB> _DBCommandListPSS = new List<LogCommandDB>();
        

        static List<WTOPC_ResponseDB> _DBWTOPC_ResponseListWNP = new List<WTOPC_ResponseDB>();
        static List<WTOPC_ResponseDB> _DBWTOPC_ResponseListFCA = new List<WTOPC_ResponseDB>();
        static List<WTOPC_ResponseDB> _DBWTOPC_ResponseListFOS = new List<WTOPC_ResponseDB>();
        static List<WTOPC_ResponseDB> _DBWTOPC_ResponseListPSS = new List<WTOPC_ResponseDB>();
        

        static List<LogResponseDB> _DBResponseListWNP = new List<LogResponseDB>();
        static List<LogResponseDB> _DBResponseListFCA = new List<LogResponseDB>();
        static List<LogResponseDB> _DBResponseListFOS = new List<LogResponseDB>();
        static List<LogResponseDB> _DBResponseListPSS = new List<LogResponseDB>();

        //static List<JobReference> _RuntapeFCA = new List<JobReference>();
        //static List<JobReference> _RuntapeFOS = new List<JobReference>();
        //static List<JobReference> _RuntapePSS = new List<JobReference>();


        public LogReader(string systemName, string proc, bool flgPathNote)
        {
            string year = DateTime.Now.Year.ToString("0000");
            string month = DateTime.Now.Month.ToString("00");
            string day = DateTime.Now.Day.ToString("00");
            processName = systemName + proc;
            if (systemName == "PSS")
            {
                processName = "RES" + proc;
            }
            sysName = systemName;

            Thread_sysID = (new AdminDB()).Gettpf_system(sysName);
            Thread_ProcessID = (new AdminDB()).GetCPU(processName, Thread_sysID);
            
            string stLOG_Path = WebConfigurationManager.AppSettings["LOG_Path"];
            path = stLOG_Path + "\\Y" + year + "\\M" + month + "\\D" + day
                + "\\Y" + year + ".M" + month + ".D" + day + ".SEA.PROD.PROD." + proc + "." + sysName + "." + (sysName == "WNP" ? "0" : proc) + ".LOG";
            if (!File.Exists(path))
            {
                if (flgPathNote)
                {
                    addNote(processName + " LOG path does not exist.", "Auto-Msg", false);
                }
                else
                {
                    //Thread.Sleep(100* (new Random()).Next(1,10));
                    AdminDB.LogIt(processName + " LOG path does not exist.");
                }
            }
            else
            {


                DateOfLog = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));
                if (Reader != null)
                {
                    AminTerminator();
                }
                Reader = new Thread(new ThreadStart(WorkThreadFunction));
                Reader.Name = threadNumber++.ToString() + "-" + processName;
                CreatedOn = DateTime.Now;
                AllThreadList.Add(Reader, CreatedOn); // to be add to the list of threads for investigation
            }
        }

        
        public List<LogCommandDB> DBCommandListWNP
        {
            get
            {
                return _DBCommandListWNP;
            }
            set
            {
                _DBCommandListWNP = value;
            }
        }
        public List<LogCommandDB> DBCommandListFCA
        {
            get
            {
                return _DBCommandListFCA;
            }
            set
            {
                _DBCommandListFCA = value;
            }
        }
        public List<LogCommandDB> DBCommandListFOS
        {
            get
            {
                return _DBCommandListFOS;
            }
            set
            {
                _DBCommandListFOS = value;
            }
        }
        public List<LogCommandDB> DBCommandListPSS
        {
            get
            {
                return _DBCommandListPSS;
            }
            set
            {
                _DBCommandListPSS = value;
            }
        }

        public List<LogResponseDB> DBResponseListWNP
        {
            get
            {
                return _DBResponseListWNP;
            }
            set
            {
                _DBResponseListWNP = value;
            }
        }
        public List<LogResponseDB> DBResponseListFCA
        {
            get
            {
                return _DBResponseListFCA;
            }
            set
            {
                _DBResponseListFCA = value;
            }
        }
        public List<LogResponseDB> DBResponseListFOS
        {
            get
            {
                return _DBResponseListFOS;
            }
            set
            {
                _DBResponseListFOS = value;
            }
        }
        public List<LogResponseDB> DBResponseListPSS
        {
            get
            {
                return _DBResponseListPSS;
            }
            set
            {
                _DBResponseListPSS = value;
            }
        }
        
        public List<LogSingleTapeDB> DBFinalTapeListWNP
        {
            get
            {
                return _DBFinalTapeListWNP;
            }
            set
            {
                _DBFinalTapeListWNP = value;
            }
        }     
        public List<LogSingleTapeDB> DBFinalTapeListFCA
        {
            get
            {
                return _DBFinalTapeListFCA;
            }
            set
            {
                _DBFinalTapeListFCA = value;
            }
        }
        public List<LogSingleTapeDB> DBFinalTapeListFOS
        {
            get
            {
                return _DBFinalTapeListFOS;
            }
            set
            {
                _DBFinalTapeListFOS = value;
            }
        }
        public List<LogSingleTapeDB> DBFinalTapeListPSS
        {
            get
            {
                return _DBFinalTapeListPSS;
            }
            set
            {
                _DBFinalTapeListPSS = value;
            }
        }

        public List<LogCommandScript> LogCommandScriptWNP
        {
            get
            {
                return _LogCommandScriptWNP;
            }
            set
            {
                _LogCommandScriptWNP = value;
            }
        }

        public List<LogCommandScript> LogCommandScriptFCA
        {
            get
            {
                return _LogCommandScriptFCA;
            }
            set
            {
                _LogCommandScriptFCA = value;
            }
        }
        public List<LogCommandScript> LogCommandScriptPSS
        {
            get
            {
                return _LogCommandScriptPSS;
            }
            set
            {
                _LogCommandScriptPSS = value;
            }
        }
        public List<LogCommandScript> LogCommandScriptFOS
        {
            get
            {
                return _LogCommandScriptFOS;
            }
            set
            {
                _LogCommandScriptFOS = value;
            }
        }

        //public List<JobReference> RuntapeFCA
        //{
        //    get
        //    {
        //        return _RuntapeFCA;
        //    }
        //    set
        //    {
        //        _RuntapeFCA = value;
        //    }
        //}
        //public List<JobReference> RuntapeFOS
        //{
        //    get
        //    {
        //        return _RuntapeFOS;
        //    }
        //    set
        //    {
        //        _RuntapeFOS = value;
        //    }
        //}
        //public List<JobReference> RuntapePSS
        //{
        //    get
        //    {
        //        return _RuntapePSS;
        //    }
        //    set
        //    {
        //        _RuntapePSS = value;
        //    }
        //}

        public List<LogDump> Dumplist
        {
            get
            {
                return _Dumplist;
            }
            set
            {
                _Dumplist = value;
            }
        }    
        public List<LogSnapDump> SnapDumplist
        {
            get
            {
                return _SnapDumplist;
            }
            set
            {
                _SnapDumplist = value;
            }
        }
        public string AminStart()
        {
            timer1.Start();
            FinishIt = false;
            if (Reader != null)
            {
                if (Reader.ThreadState == System.Threading.ThreadState.Unstarted)
                {
                    Reader.Start();
                    AdminDB.LogIt("Thread " + Reader.Name + " of " + processName + " was started at " + DateTime.Now);
                }
                //else if (Reader.ThreadState == System.Threading.ThreadState.Suspended)
                //{
                //    Reader.Resume();
                //}
            }
            return AminRead();
        }

        public string AminRead()
        {
            if (Reader != null)
            {
                return "<table><tr><td>Thread No.</td><td><b>" + this.Reader.Name + "</b></td></tr>"
                    + "<tr><td>finishIt flag: </td><td><b>" + FinishIt + "</b></td></tr>"
                    + "<tr><td>Admin Auto Update flag: </td><td><b>" + AdminAutoUpdate + "</b></td></tr>"
                    + "<tr><td>User Auto start/tape update flag: </td><td><b>" + UserAutoStartTapeUpdate + "</b></td></tr>"
                    + "<tr><td>User Auto end Update flag: </td><td><b>" + UserAutoEndUpdate + "</b></td></tr>"
                    + "<tr><td>System: </td><td><b>" + sysName + "</b></td></tr>"
                    + "<tr><td>CPU: </td><td><b>" + processName + "</b></td></tr>"
                    + "<tr><td>Thread status: </td><td><b>" + Reader.ThreadState.ToString() + "</b></td></tr>"
                    + "<tr><td>Utitlity read Count: </td><td><b>" + UtilityData.Count.ToString() + "</b></td></tr>"
                    + "<tr><td>Working for </td><td><b>" + timer1.Elapsed.ToString() + "</b></td></tr>"
                    + "<tr><td>Day to be processed: </td><td><b>" + DateOfLog.ToShortDateString() + "</b></td></tr>"
                    + "<tr><td>Created on: </td><td><b>" + CreatedOn.ToString() + "</b></td></tr>"
                    + "<tr><td>Last line of log: </td><td><b>" + Counter + "(Empty line count:" + EmptyLines + ")</b></td></tr>"
                    + "<tr><td>number of used lines from log: </td><td><b>" + UsedLinesCounter + " (" + (UsedLinesCounter * 100.0 / ((Counter - EmptyLines) + 1)).ToString(".00") + "%)</b></td></tr>"
                    + "<tr><td>Last processed line Time: </td><td><b>" + LastUpdate + "</b></td></tr>"
                    + "<tr><td>Last time on end of log file: </td><td><b>" + Result + "</b></td></tr>"
                    + "<tr><td colspan=2>Location of log file: <b>" + path + "</b></td></tr></table><hr/>";
            }
            else
            {
                return "Thread is not available!";
            }
        }

        public string AminLogTimeRead()
        {
            return (LastUpdate==null?DateTime.MinValue:DateTime.Parse(LastUpdate)).ToString("HH:mm:ss");
        }

        public string AminStop()
        {
            timer1.Stop();
            FinishIt = true;
            //if (Reader != null && Reader.ThreadState == ThreadState.Running)
            //{
            //    Reader.Suspend();
            //}
            return AminRead();
        }

        public void AminTerminator()
        {
            if (Reader != null)
            {
                Reader.Abort();
            }
        }

        /// <summary>
        /// Nafise: Read commands from DB and put them on the List<LogCommandDB>
        /// </summary>
        /// <returns></returns>
        private void ReadCommandResponseDB() 
        {
            JobCommand job_command = new JobCommand();
            DataTable dtCommand = new DataTable();
            DataTable dtResponse = new DataTable();
            DataTable dtWTOPC_Response = new DataTable();

            List<LogCommandDB> UDBCommandList = new List<LogCommandDB>();
            List<LogResponseDB> UDBResponseList = new List<LogResponseDB>();
            List<WTOPC_ResponseDB> UDBWTOPC_ResponseList = new List<WTOPC_ResponseDB>();

            if ((_DBCommandListWNP.Count == 0 && sysName == "WNP")
                || (_DBCommandListFCA.Count == 0 && sysName == "FCA")
                || (_DBCommandListFOS.Count == 0 && sysName == "FOS")
                || (_DBCommandListPSS.Count == 0 && sysName == "PSS"))
            {
                dtCommand = job_command.Read_JobCommands(Thread_sysID, DateOfLog);
                dtResponse = job_command.Read_JobResponses(Thread_sysID, DateOfLog);
                dtWTOPC_Response = job_command.Read_JobWTOPC_Response(Thread_sysID);

                UDBCommandList = (from DataRow dr in dtCommand.Rows
                                  select new LogCommandDB
                                  {
                                      CommandID = long.Parse(dr["COMMAND_ID"].ToString()),
                                      ScheduleID = long.Parse(dr["SCHEDULE_ID"].ToString()),
                                      CommandDB = dr["COMMAND"].ToString(),
                                      TypeDB = dr["TYPE"].ToString(),
                                      DesiredCPU = dr["CPU_NAME"].ToString(),
                                      Partition = dr["PARTITION_NAME"].ToString(),
                                      Process = null
                                  }).ToList();

                UDBResponseList = (from DataRow dr in dtResponse.Rows
                                   select new LogResponseDB
                                   {
                                       CommandID = long.Parse(dr["COMMAND_ID"].ToString()),
                                       ScheduleID = long.Parse(dr["SCHEDULE_ID"].ToString()),
                                       ResponseDB = dr["RESPONSE"].ToString(),
                                       WTOPC_Response = dr["WTOPC_RESPONSE"].ToString(),
                                       TypeDB = dr["TYPE"].ToString(),
                                       DesiredCPU = dr["CPU_NAME"].ToString(),
                                       Partition = dr["PARTITION_NAME"].ToString(),
                                       Process = null
                                   }).ToList();

                UDBWTOPC_ResponseList = (from DataRow dr in dtWTOPC_Response.Rows
                                         select new WTOPC_ResponseDB
                                         {
                                             WTOPC_Response = dr["WTOPC_RESPONSE"].ToString(),
                                         }).ToList();


                switch (sysName)
                {
                    case "WNP":
                        if (_DBCommandListWNP.Count == 0)
                        {
                            _DBCommandListWNP = UDBCommandList;
                            _DBResponseListWNP = UDBResponseList;
                            _DBWTOPC_ResponseListWNP = UDBWTOPC_ResponseList;
                        }
                        break;
                    case "FCA":
                        if (_DBCommandListFCA.Count == 0)
                        {
                            _DBCommandListFCA = UDBCommandList;
                            _DBResponseListFCA = UDBResponseList;
                            _DBWTOPC_ResponseListFCA = UDBWTOPC_ResponseList;
                        }
                        break;
                    case "FOS":
                        if (_DBCommandListFOS.Count == 0)
                        {
                            _DBCommandListFOS = UDBCommandList;
                            _DBResponseListFOS = UDBResponseList;
                            _DBWTOPC_ResponseListFOS = UDBWTOPC_ResponseList;
                        }
                        break;
                    case "PSS":
                        if (_DBCommandListPSS.Count == 0)
                        {
                            _DBCommandListPSS = UDBCommandList;
                            _DBResponseListPSS = UDBResponseList;
                            _DBWTOPC_ResponseListPSS = UDBWTOPC_ResponseList;
                        }
                        break;
                }
            }
        }

        private void InitialTapeInfo()
        {
            DataTable temp = new DataTable();
            JobTape TapeInitializer = new JobTape();
            List<LogTapeCountDB> UDBTapeListCount;
            List<LogMultipleTapeDB> UDBTapeListMultiples;
            int count = 0;

            switch (sysName)
            {
                case "WNP":
                    count = DBTapeListCountWNP.Count;
                    break;
                case "FCA":
                    count = DBTapeListCountFCA.Count;
                    break;
                case "FOS":
                    count = DBTapeListCountFOS.Count;
                    break;
                case "PSS":
                    count = DBTapeListCountPSS.Count;
                    break;
                default:
                    return;
            }


            if (count == 0)
            {
                temp = TapeInitializer.ReadTapeLableCountOfOneDay(sysName, DateOfLog);
                UDBTapeListCount = (from DataRow dr in temp.Rows
                                    select new LogTapeCountDB
                                    {
                                        TapeName = dr["name"].ToString(),
                                        Count = int.Parse(dr["count"].ToString()),
                                        IsInput = (dr["INPUT"].ToString() == "1" ? true : false)
                                    }).ToList();
                temp = new DataTable();
                temp = TapeInitializer.ReadTape_RelatedToCoupleOfJobs_CommandIDs(sysName, DateOfLog);
                UDBTapeListMultiples = (from DataRow dr in temp.Rows
                                        select new LogMultipleTapeDB
                                        {
                                            ScheduleID = long.Parse(dr["SCHEDULE_ID"].ToString()),
                                            TapeID = long.Parse(dr["TAPE_ID"].ToString()),
                                            TapeName = dr["Name"].ToString(),
                                            CPU = dr["CPU_NAME"].ToString(),
                                            Partition = dr["PARTITION_NAME"].ToString(),
                                            CommandID = long.Parse(dr["COMMAND_ID"].ToString()),
                                            lateMounted = (dr["IS_LATEMOUNT"].ToString() == "1" ? true : false),
                                            startTime = dr["START_SCHD_TIME"].ToString(),
                                            type = dr["Type"].ToString(),
                                            IsInput = (dr["INPUT"].ToString() == "1" ? true : false),
                                            OS390JobName = dr["NAME_OS390"].ToString()
                                        }).ToList();
                switch (sysName)
                {
                    case "WNP":
                        DBTapeListCountWNP = UDBTapeListCount;
                        DBTapeListMultiplesWNP = UDBTapeListMultiples;
                        break;
                    case "FCA":
                        DBTapeListCountFCA = UDBTapeListCount;
                        DBTapeListMultiplesFCA = UDBTapeListMultiples;
                        break;
                    case "FOS":
                        DBTapeListCountFOS = UDBTapeListCount; ;
                        DBTapeListMultiplesFOS = UDBTapeListMultiples;
                        break;
                    case "PSS":
                        DBTapeListCountPSS = UDBTapeListCount; ;
                        DBTapeListMultiplesPSS = UDBTapeListMultiples;
                        break;
                    default:
                        return;
                }
            }

            
        }

        /// <summary>
        /// Amin,Nafise: Main thread function: looping 
        /// </summary>
        public void WorkThreadFunction()  //------------------------------------------ Main Thread Function
        {
            int n = 20;
            bool IsFutureSchedulePublished = false;
            float[] array = new float[n];
            string comLine;
            int inxCom;
            WTOPC_ResponseDB findWTOPC = null;
            string WTOPC_Code = "", WTOPC_Code_2 = "";
            List<string> relatedLines = new List<string>();
            try
            {
                if (!File.Exists(path))
                {
                    addNote(processName + " LOG path does not exist.", "Auto-Msg", false);
                    Thread.Sleep(sleeptime * 3);
                    //Thread.Sleep(10000);
                }
                else
                {
                    // -------------- Initialization of TAPES, COMMANDS and RESPONSES
                    InitialTapeInfo();
                    ReadCommandResponseDB();

                    Counter = 0;
                    UsedLinesCounter = 0;
                    EmptyLines = 0;
                    FileStream tempFileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    StreamReader reader = new StreamReader(tempFileStream);
                    for (; !FinishIt; )
                    {
                        // ------------------------------- to auto publish the next 14 days schedule between 7 and 7:10 am 
                        // ------------------------------- if it has not been published yet
                        if (!IsFutureSchedulePublished && (processName == "FCA2" || processName == "WNP1" || processName == "FOSI")
                            && DateTime.Compare(DateTime.Now, Convert.ToDateTime("07:00:00")) > 0
                            && DateTime.Compare(DateTime.Now, Convert.ToDateTime("07:10:00")) < 0)
                        {
                            ScheduleJob Worker = new ScheduleJob();
                            List<long> job_times = new List<long>(), Ordered_job_times = new List<long>();
                            for (int day = 1; day < 8; day++)
                            {
                                job_times = Worker.TodayJobs(DateTime.Now.AddDays(day), Thread_sysID.ToString());
                                Ordered_job_times = Worker.OrderJobTimeIDs(job_times, Thread_sysID.ToString(), DateTime.Now.AddDays(day));
                                Worker.SchedulePublish(Ordered_job_times, DateTime.Now.AddDays(day), Thread_sysID.ToString(), "Auto-published (" + sysName + ")");
                            }
                            IsFutureSchedulePublished = true;
                        }

                        string line = "", time = "", date = "";
                        lock (Threadlocker)
                        {
                            while (!reader.EndOfStream)
                            {
                                Counter++;
                                line = reader.ReadLine().Trim().Replace('\uFFFD', ' ');
                                if (line.Length < 109) // to avoid empty lines
                                {
                                    EmptyLines++;
                                    continue;
                                }
                                time = line.Substring(11, 2) + ":" + line.Substring(14, 2) + ":" + line.Substring(17, 2);
                                date = line.Substring(0, 10);
                                LastUpdate = date + " " + time;
                                WTOPC_Code = line.Substring(99, 9);
                                if (line.Length > 119)
                                {
                                    WTOPC_Code_2 = line.Substring(109, 9);
                                }
                                else
                                {
                                    WTOPC_Code_2 = "";
                                }
                                //if (Counter == 201127)
                                //{

                                //}
                                
                                if (line.Substring(86, 1) == "6")
                                {
                                    if (line.Length > 125 && line.Contains("==>"))
                                    {
                                        if (line.ToUpper().Contains("ZRPDU CREATE")) //PDU Create entry on consol
                                        {
                                            UsedLinesCounter++;
                                            PDUexcDurtation = -1;
                                            PDUexcTime = "-1";
                                            string[] temp = line.ToUpper().Split(new string[] { "EXC" }, StringSplitOptions.RemoveEmptyEntries);
                                            if (temp.Length > 1)
                                            {
                                                string[] TimeDuration = temp[1].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                                if (TimeDuration.Length > 2)
                                                {
                                                    PDUexcTime = TimeDuration[1];
                                                    int.TryParse(temp[1].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[2], out PDUexcDurtation);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            inxCom = line.IndexOf("==>") + 4;
                                            if (line.Length > inxCom)
                                            {
                                                comLine = line.Substring(inxCom, line.Length - inxCom);
                                                switch (sysName)
                                                {
                                                    case "WNP":
                                                        if (LogCommandScriptWNP.Count != 0)
                                                        {
                                                            UsedLinesCounter++;
                                                            ScriptCommandFinder(DateTime.Parse(LastUpdate), comLine);
                                                        }
                                                        break;
                                                    case "FCA":
                                                        if (LogCommandScriptFCA.Count != 0)
                                                        {
                                                            UsedLinesCounter++;
                                                            ScriptCommandFinder(DateTime.Parse(LastUpdate), comLine);
                                                        }
                                                        break;
                                                    case "FOS":
                                                        if (LogCommandScriptFOS.Count != 0)
                                                        {
                                                            UsedLinesCounter++;
                                                            ScriptCommandFinder(DateTime.Parse(LastUpdate), comLine);
                                                        }
                                                        break;
                                                    case "PSS":
                                                        if (LogCommandScriptPSS.Count != 0)
                                                        {
                                                            UsedLinesCounter++;
                                                            ScriptCommandFinder(DateTime.Parse(LastUpdate), comLine);
                                                        }
                                                        break;
                                                    default:
                                                        break;
                                                }

                                                if (comLine.Trim() != "")
                                                {
                                                    bool RAVEN_Automation = line.Contains("RAVEN");
                                                    CommandFinder(DateTime.Parse(LastUpdate), comLine, RAVEN_Automation);
                                                    UsedLinesCounter++;
                                                }
                                            }
                                        }
                                    }
                                }


                                if (line.Substring(86, 1) == "5" && line.Length > 124 && line.Substring(109, 15).ToUpper() == "AUTO A RSCRIPT ")
                                {
                                    bool RAVEN_Automation = line.Contains("RAVEN");
                                    UsedLinesCounter++;
                                    comLine = line.Substring(109, line.Length - 109);
                                    CommandFinder(DateTime.Parse(LastUpdate), comLine, RAVEN_Automation);
                                }

                                switch (WTOPC_Code)
                                {
                                    case "RECP0833I":
                                        if (line.Contains("PROCESSING COMPLETED FOR ZRECP START"))           //Recoup Start
                                        {
                                            UsedLinesCounter++;
                                            RecoupActive = true;
                                        }
                                        break;
                                    case "RECP0080I":
                                        if (line.Contains("STARTING IN SECONDARY PROCESSOR"))               //Recoup Start
                                        {
                                            UsedLinesCounter++;
                                            RecoupActive = true;
                                        }
                                        break;
                                    case "RECP0840I":
                                        if (line.Contains("PHASE I: ACTIVE"))                               //Recoup Active
                                        {
                                            UsedLinesCounter++;
                                            RecoupActive = true;
                                        }
                                        break;
                                    case "RECP0301I":
                                        if (line.Contains("TPF RECOUP COMPLETED ON CPU"))                   //Recoup Chain chase end
                                        {
                                            UsedLinesCounter++;
                                            RecoupActive = false;
                                        }
                                        break;
                                    case "RECP00B0T":
                                        if (line.Contains("RECOUP RUN COMPETED"))                           //Recoup Chain chase end
                                        {
                                            UsedLinesCounter++;
                                            RecoupActive = false;
                                        }
                                        break;
                                    case "RECP001AT":
                                        if (line.Contains("RECOUP ABORTED"))                                //Recoup Chain chase end
                                        {
                                            UsedLinesCounter++;
                                            RecoupActive = false;
                                        }
                                        break;
                                    case "COTM0310I":
                                    case "COSK0310I":                                                       // VolSer Mount
                                        MultipleLinesDisplayStorage.Add(line);
                                        UsedLinesCounter++;
                                        if (line.Contains("+"))
                                        {
                                            TapeVolserFinder();
                                            MultipleLinesDisplayStorage.Clear();                                        
                                        }
                                        break;
                                    case "COTC0300A":
                                    case "COTG0300A":
                                    case "COTS0300A":
                                    case "COTC0087A":                                                       // remove tape time
                                        MultipleLinesDisplayStorage.Add(line);
                                        UsedLinesCounter++;
                                        if (line.Contains("+"))
                                        {
                                            TapeRemoveFinder();
                                            MultipleLinesDisplayStorage.Clear();
                                        }
                                        break;
                                    case "COTF0087A":
                                    case "COTG0087A":                                                       // ZTOFF tape
                                        MultipleLinesDisplayStorage.Add(line);
                                        UsedLinesCounter++;
                                        if (line.Contains("+"))
                                        {
                                            TapeZTOFF();
                                            MultipleLinesDisplayStorage.Clear();
                                        }
                                        break;
                                    case "AAES0009I":                                                       // --------------------------- TOS AUTOMATION GENERATED
                                        switch (WTOPC_Code_2)
                                        {
                                            case "AUTC1111I":                                               // TOS OK
                                                UsedLinesCounter++;
                                                break;
                                            case "AUTBAT02I":                                               // Autosubmit
                                                TapeAutoSubmit(line);
                                                UsedLinesCounter++;
                                                break;
                                            case "AAER0700I":                                               // Calendar Update from TOS log
                                                TosAutomation_CalendarUpdate(line); 
                                                UsedLinesCounter++;
                                                break;
                                            case "RSCRIPTOI":                                               // RPO Update
                                            case "RSCTRC03I":
                                            case "RSCTRC09I":
                                                if (line.Contains("RPO"))
                                                {
                                                    TosAutomation_RPO_Update(line);
                                                    UsedLinesCounter++;                                                    
                                                }
                                                break;
                                            case "AAER0414I":                                               // RPO Stop
                                                if (line.Contains("RPO"))
                                                {
                                                    RPO_Step = "Not Active";
                                                    UsedLinesCounter++;
                                                }
                                                break;
                                            case "RSSW0001I":
                                                TosAutomation_RunTape(line);
                                                UsedLinesCounter++;
                                                break;
                                            default:
                                                break;
                                        }
                                        break;
                                    case "RCRS0004I":
                                        switch (WTOPC_Code_2)
                                        {
                                            case "AUTBAT05I":                                               // TPFBAT
                                                TapeTPFBAT(line);
                                                UsedLinesCounter++;
                                                break;
                                            default:
                                                break;
                                        }
                                        break;
                                    case "STAT0018I":                                                       // system utility reads
                                        MultipleLinesDisplayStorage.Add(line);
                                        UsedLinesCounter++;
                                        if (line.Contains("+"))
                                        {
                                            SystemUtilityDiagram();
                                            MultipleLinesDisplayStorage.Clear();
                                        }
                                        break;
                                    case "STAT0014I":                                                       // system status reads
                                        MultipleLinesDisplayStorage.Add(line);
                                        UsedLinesCounter++;
                                        if (line.Contains("+"))
                                        {
                                            SystemStatusDiagram();
                                            MultipleLinesDisplayStorage.Clear();
                                        }
                                        break;
                                    case "CPSE0152E":                                                       // system Dumps
                                        MultipleLinesDisplayStorage.Add(line);
                                        UsedLinesCounter++;
                                        if (line.Contains("+"))
                                        {
                                            DumpLister();
                                            MultipleLinesDisplayStorage.Clear();
                                        }
                                        break;
                                    case "SNAP0003I":                                                       // system SNAP Dumps
                                        MultipleLinesDisplayStorage.Add(line);
                                        UsedLinesCounter++;
                                        if (line.Contains("+"))
                                        {
                                            SnapDumpLister();
                                            MultipleLinesDisplayStorage.Clear();
                                        }
                                        break;
                                    case "COSK0399I":                                                       // find VolSer# of each dump
                                        DumpVolSerFinder(line);
                                        UsedLinesCounter++;                                        
                                        break;
                                    case "CSMP0097I":                                                       // extracing Parttion Name
                                        UsedLinesCounter++;
                                        ExtractPartitionName(line);
                                        break;
                                    case "BCPE0010I":                                                       // ADDRESSES RETURNED
                                        UsedLinesCounter++;
                                        ReleaseUpdate(line, "RET");
                                        break;
                                    case "DYDE0005I":                                                       // MULTIPLE RELEASES
                                        UsedLinesCounter++;
                                        ReleaseUpdate(line, "MUL");
                                        break;
                                    case "BOFA0055I":                                                       // -----------------------------------Not sure it is a correct WTOPC
                                    case "BOFA0011I":                                                       // PDU CREATE RESPONSE
                                        UsedLinesCounter++;
                                        if (line.ToUpper().Contains("ONLINE PDU CREATE"))
                                        {
                                            long partition_ID = (new AdminDB()).GetPartitionID(Thread_partitionName, Thread_sysID);
                                            if (partition_ID != 0)
                                            {
                                                (new AdminDB()).AddPDURelease(PDUexcTime, PDUexcDurtation, DateTime.Parse(LastUpdate), partition_ID, "EXC");
                                            }
                                        }
                                        break;
                                    case "BOFA0008W":                                                       // PDU ABORT
                                        UsedLinesCounter++;
                                        if (line.Contains("PDU ABORT COMPLETE"))
                                        {
                                            PDU_Abort(line);
                                        }
                                        break;
                                    case "CYGM0011W":                                                      // Pool Usage Exceeded                                         
                                        PoolUsageExceed(line);                                        
                                        break;
                                    case "CYC10006I":                                                       // Pool In-use & Minute
                                        MultipleLinesDisplayStorage.Add(line);
                                        UsedLinesCounter++;
                                        if (line.Contains("+"))
                                        {
                                            PoolInUse();
                                            MultipleLinesDisplayStorage.Clear();
                                        }
                                        break;
                                    default:
                                        break;
                                }



                                switch (sysName)
                                {
                                    case "WNP":
                                        findWTOPC = _DBWTOPC_ResponseListWNP.Where(d => (d.WTOPC_Response.ToLower().Contains(WTOPC_Code.ToLower()))).FirstOrDefault();
                                        break;
                                    case "FCA":
                                        findWTOPC = _DBWTOPC_ResponseListFCA.Where(d => (d.WTOPC_Response.ToLower().Contains(WTOPC_Code.ToLower()))).FirstOrDefault();
                                        break;
                                    case "FOS":
                                        findWTOPC = _DBWTOPC_ResponseListFOS.Where(d => (d.WTOPC_Response.ToLower().Contains(WTOPC_Code.ToLower()))).FirstOrDefault();
                                        break;
                                    case "PSS":
                                        findWTOPC = _DBWTOPC_ResponseListPSS.Where(d => (d.WTOPC_Response.ToLower().Contains(WTOPC_Code.ToLower()))).FirstOrDefault();
                                        break;
                                    default:
                                        break;
                                }

                                if (findWTOPC != null && line.Substring(86, 1) != "6" && line.Substring(86, 1) != "5" && line.Substring(86, 1) != "7")
                                {
                                    comLine = line.Substring(109, line.Length - 109);

                                    string sPattern = "^\\d{2}.\\d{2}.\\d{2}$";
                                    if (comLine.Length >= 30
                                        && System.Text.RegularExpressions.Regex.IsMatch(comLine.Substring(WTOPC_Code.Length + 9, 9).Trim(), sPattern)
                                        && (comLine.Substring(WTOPC_Code.Length + 9, 9).Trim() == comLine.Substring(WTOPC_Code.Length, 9).Trim())
                                        )
                                    {
                                        var regex = new Regex(Regex.Escape(comLine.Substring(WTOPC_Code.Length + 9, 9)));
                                        comLine = regex.Replace(comLine, "", 1);
                                    }

                                    ResponseFinder(DateTime.Parse(LastUpdate), comLine, WTOPC_Code);
                                    UsedLinesCounter++;
                                }
                            }
                        }
                        Result = DateTime.Now.ToString();// + " " + DateTime.Now.ToLongTimeString();
                        Thread.Sleep(sleeptime);
                        if (DateOfLog.Date != DateTime.Now.Date)
                        {
                            break;
                        }
                    }
                    reader.Close();
                    tempFileStream.Close();
                    Reader.Abort();
                }
            }
            catch (Exception ex)
            {
                AdminDB.LogIt(ex.Message.ToString(), ex.StackTrace.ToString(), Counter, processName, Reader.Name);
                //throw;
            }
        }

        /// <summary>
        /// Nafise: adding note to Schedule_Note
        /// </summary>
        private void addNote(string stNote, string stSendto, bool flgcarryover)
        {
            NoteScheduleJob notesch = new NoteScheduleJob();
            //notesch.Schedule_ID = lgScheduleID;
            notesch.Schedule_Note = stNote;
            notesch.Operator = "Server";
            notesch.Sendto = stSendto;
            notesch.Carryover = flgcarryover;
            notesch.AddNote();
            
            ////sending note to the chatroom
            //ChatHub ch = new ChatHub();
            //ch.Send("Server", stNote);
        }

        public List<LogCommandScript> GetScriptCommandList(long scriptCmdID)
        {
            List<LogCommandScript> UDBScriptCommandList;

            switch (sysName)
            {
                case "WNP":
                    UDBScriptCommandList = _LogCommandScriptWNP;
                    break;
                case "FCA":
                    UDBScriptCommandList = _LogCommandScriptFCA;
                    break;
                case "FOS":
                    UDBScriptCommandList = _LogCommandScriptFOS;
                    break;
                case "PSS":
                    UDBScriptCommandList = _LogCommandScriptPSS;
                    break;
                default:
                    return null;
            }

            List<LogCommandScript> findScriptCommandList = null;
            findScriptCommandList = UDBScriptCommandList.Where(d => (d.ScriptID == scriptCmdID)).ToList();

            return findScriptCommandList;
        }

        /// <summary>
        /// Nafise: Show DBCommands based on the flg condition
        /// 0: Ran commands; 1: Commands didn't Run; 
        /// </summary>
        /// <param name="stCommType"></param>
        /// <param name="flg"></param>
        /// <param name="flgNot"></param>
        /// <param name="sys_Name"></param>
        /// <returns></returns>
        private List<LogCommandDB> ShowDBCommandList(List<string> selecteditems, int flg, bool flgNot, string sys_Name)
        {
            List<LogCommandDB> TempCommandList = new List<LogCommandDB>();
            List<LogCommandDB> UDBCommandList = null;
            switch (sys_Name)
            {
                case "WNP":
                    UDBCommandList = _DBCommandListWNP;
                    break;
                case "FCA":
                    UDBCommandList = _DBCommandListFCA;
                    break;
                case "FOS":
                    UDBCommandList = _DBCommandListFOS;
                    break;
                case "PSS":
                    UDBCommandList = _DBCommandListPSS;
                    break;
            }
            List<LogCommandDB> findCommand = null;

            if (flg == 0) //Ran Commands
            {
                if (selecteditems.Count == 0)
                {
                    if (!flgNot) //if not found
                    {
                        findCommand = UDBCommandList.Where(d => (d.CommandTimeDB == DateTime.MinValue) && (d.CommandDB != "")).ToList();
                    }
                    else // found
                    {
                        findCommand = UDBCommandList.Where(d => (d.CommandTimeDB != DateTime.MinValue) && (d.CommandDB != "")).ToList();
                    }
                }
                else
                {
                    if (!flgNot) //if not found
                    {
                        findCommand = UDBCommandList.Where(d => (d.CommandTimeDB == DateTime.MinValue) && (selecteditems.Contains(d.TypeDB)) && (d.CommandDB != "")).ToList();
                    }
                    else //found 
                    {
                        findCommand = UDBCommandList.Where(d => (d.CommandTimeDB != DateTime.MinValue) && (selecteditems.Contains(d.TypeDB)) && (d.CommandDB != "")).ToList();
                    }
                }
                if (findCommand != null) //found the whole Logcommand in DBCommand
                {
                    foreach (LogCommandDB o in findCommand)
                    {
                        TempCommandList.Add(o);
                    }
                }
            }
            else if (flg == 1) //Commands didn't Run
            {
                if (!flgNot) //if not found
                {
                    findCommand = UDBCommandList.Where(d => (d.CommandTimeDB == DateTime.MinValue) && (selecteditems.Contains(d.TypeDB)) && (d.CommandDB != "")).ToList();
                }
                else // found
                {
                    findCommand = UDBCommandList.Where(d => (d.CommandTimeDB != DateTime.MinValue) && (selecteditems.Contains(d.TypeDB)) && (d.CommandDB != "")).ToList();
                }
                if (findCommand != null) //find the whole Logcommand in DBCommand
                {
                    foreach (LogCommandDB o in findCommand)
                    {
                        TempCommandList.Add(o);
                    }
                }
            }
            return TempCommandList;
        }

        /// <summary>
        /// Nafise: Show DBResponse based on the flg condition
        /// 0: found Responses; 1: Not found Responses; 
        /// </summary>
        /// <param name="stCommType"></param>
        /// <param name="flg"></param>
        /// <returns></returns>
        private List<LogResponseDB> ShowResponseDBList(List<string> selecteditems, int flg, bool flgNot, string sys_Name)
        {

            List<LogResponseDB> TempCommandList = new List<LogResponseDB>();
            List<LogResponseDB> UDBResponseList = null;
            switch (sys_Name)
            {
                case "WNP":
                    UDBResponseList = _DBResponseListWNP;
                    break;
                case "FCA":
                    UDBResponseList = _DBResponseListFCA;
                    break;
                case "FOS":
                    UDBResponseList = _DBResponseListFOS;
                    break;
                case "PSS":
                    UDBResponseList = _DBResponseListPSS;
                    break;
            }
            List<LogResponseDB> findResponse = null;

            if (flg == 0) //Ran Commands
            {
                if (selecteditems.Count == 0)
                {
                    if (!flgNot) // if "Not found"
                    {
                        findResponse = UDBResponseList.Where(d => (d.RsponseTimeDB == DateTime.MinValue) && (d.ResponseDB != "")).ToList();
                    }
                    else //found
                    {
                        findResponse = UDBResponseList.Where(d => (d.RsponseTimeDB != DateTime.MinValue) && (d.ResponseDB != "")).ToList();
                    }
                }
                else
                {
                    if (!flgNot) // if "Not found"
                    {
                        findResponse = UDBResponseList.Where(d => (d.RsponseTimeDB == DateTime.MinValue) && (selecteditems.Contains(d.TypeDB)) && (d.ResponseDB != "")).ToList();
                    }
                    else //found
                    {
                        findResponse = UDBResponseList.Where(d => (d.RsponseTimeDB != DateTime.MinValue) && (selecteditems.Contains(d.TypeDB)) && (d.ResponseDB != "")).ToList();
                    }
                }
                if (findResponse != null) //found the whole Logcommand in ResponseDB
                {
                    foreach (LogResponseDB o in findResponse)
                    {
                        TempCommandList.Add(o);
                    }
                }
            }
            else if (flg == 1) //Responses Couldon't find
            {
                if (!flgNot) // if "Not found"
                {
                    findResponse = UDBResponseList.Where(d => (d.RsponseTimeDB == DateTime.MinValue) && (selecteditems.Contains(d.TypeDB)) && (d.ResponseDB != "")).ToList();
                }
                else //found
                {
                    findResponse = UDBResponseList.Where(d => (d.RsponseTimeDB == DateTime.MinValue) && (selecteditems.Contains(d.TypeDB)) && (d.ResponseDB != "")).ToList();
                }
                if (findResponse != null) //find the whole Logcommand in DBCommand
                {
                    foreach (LogResponseDB o in findResponse)
                    {
                        TempCommandList.Add(o);
                    }
                }
            }
            return TempCommandList;
        }

        /// <summary>
        /// Nafise: Display DB Response with conditions
        /// </summary>
        /// <param name="stCommType"></param>
        /// <param name="flg"></param>
        /// <returns></returns>
        public List<LogResponseDB> DispResponseList(List<string> selecteditems, int flg, bool flgnot, string sys_Name)
        {
            return ShowResponseDBList(selecteditems, flg, flgnot, sys_Name);
        }

        /// <summary>
        /// Nafise: Display DB commands with conditions
        /// </summary>
        /// <param name="selecteditems"></param>
        /// <param name="flg"></param>
        /// <param name="flgnot"></param>
        /// <param name="sys_name"></param>
        /// <returns></returns>
        public List<LogCommandDB> DispCommandList(List<string> selecteditems, int flg, bool flgnot, string sys_name)
        {
            return ShowDBCommandList(selecteditems, flg, flgnot, sys_name);
        }

        
        /// <summary>
        /// Amin: Display Logged Tapes
        /// </summary>
        /// <param name="stCommType"></param>
        /// <param name="flg"></param>
        /// <returns></returns>
        public List<LogSingleTapeDB> DispTapeList()
        {
            List<LogSingleTapeDB> UDBFInalTapeList = null;
            switch (sysName)
            {
                case "WNP": UDBFInalTapeList = DBFinalTapeListWNP; break;
                case "FCA": UDBFInalTapeList = DBFinalTapeListFCA; break;
                case "FOS": UDBFInalTapeList = DBFinalTapeListFOS; break;
                case "PSS": UDBFInalTapeList = DBFinalTapeListPSS; break;
            }
            return UDBFInalTapeList;
        }

        /// <summary>
        /// Nafise: adding note to Schedule_Note
        /// </summary>
        private void addNote(string stNote, long scheduled_ID, string Proc_Name, string stSendto, bool flgcarryover)
        {
            NoteScheduleJob notesch = new NoteScheduleJob();
            notesch.Schedule_ID = scheduled_ID;
            notesch.Schedule_Note = stNote;
            notesch.Operator = Proc_Name;
            notesch.Sendto = stSendto;
            notesch.Carryover = flgcarryover;
            notesch.AddNote();
        }

        /// <summary>
        /// Nafise: finding Script Command in the log and filling LogCommandScript
        /// </summary>
        /// <param name="timeLine"></param>
        /// <param name="stline"></param>
        private void ScriptCommandFinder(DateTime timeLine, string stline)
        {
            string stTemp = stline.ToLower().Trim();
            List<LogCommandScript> UDBScriptCommandList;

            switch (sysName)
            {
                case "WNP":
                    UDBScriptCommandList = _LogCommandScriptWNP;
                    break;
                case "FCA":
                    UDBScriptCommandList = _LogCommandScriptFCA;
                    break;
                case "FOS":
                    UDBScriptCommandList = _LogCommandScriptFOS;
                    break;
                case "PSS":
                    UDBScriptCommandList = _LogCommandScriptPSS;
                    break;
                default:
                    return;
            }

            LogCommandScript findScriptCommand = null;
            findScriptCommand = UDBScriptCommandList.Where(d => (d.CommandDB.ToLower() == stTemp)
                                   && (d.CommandTimeDB == DateTime.MinValue)).FirstOrDefault();

            //needs to check double command in less than 30 mins
            if (findScriptCommand != null)
            {
                findScriptCommand.CommandTimeDB = timeLine;
               // findScriptCommand.Process = processName;
            }
        }

        /// <summary>
        /// Nafise: Search for the Command line in DBcommandList, then update DBCommandList based on the Line time
        /// </summary>
        /// <param name="timeLine"></param>
        /// <param name="stline"></param>
        private void CommandFinder(DateTime timeLine, string stline, bool RAVEN)
        {
            string stTemp = stline.ToLower().Trim();
            LogCommandDB findCommand = null;
            LogCommandDB findCommandRepeat = null;
            LogCommandDB findCommandRepeatButRequested = null;

            List<LogCommandDB> UDBCommandList = null;
            switch (sysName)
            {
                case "WNP":
                    UDBCommandList = _DBCommandListWNP;
                    break;
                case "FCA":
                    UDBCommandList = _DBCommandListFCA;
                    break;
                case "FOS":
                    UDBCommandList = _DBCommandListFOS;
                    break;
                case "PSS":
                    UDBCommandList = _DBCommandListPSS;
                    break;
                default:
                    break;
            }

            findCommand = UDBCommandList.Where(d => (d.CommandDB.ToLower() == stTemp 
                                                     || (d.CommandDB.ToLower().Contains(stTemp) && d.CommandDB.ToLower().Contains("sendall"))
                                                        )
                        && (d.CommandTimeDB == DateTime.MinValue)
                        && (d.DesiredCPU == processName)
                        && (d.Process == null)).FirstOrDefault();
            if (findCommand == null)
            {
                findCommand = UDBCommandList.Where(d => (d.CommandDB.ToLower() == stTemp)
                                       && (d.CommandTimeDB == DateTime.MinValue)
                                       //&& (d.DesiredCPU == processName)  // -- to search other processors commands
                                       && (d.Process == null)).FirstOrDefault();            
            }

            if (findCommand != null) //find the whole Logcommand in DBCommand
            {
                findCommandRepeat = UDBCommandList.Where(d => (d.CommandDB.ToLower() == stTemp)
                            && (d.CommandTimeDB != DateTime.MinValue)
                            && (d.Process == processName)
                            && (d.CommandTimeDB.AddMinutes(+30) > timeLine)
                            && (d.CommandTimeDB <= timeLine)
                            ).FirstOrDefault();

                if (findCommandRepeat != null)
                {
                    findCommandRepeatButRequested = UDBCommandList.Where(d => (d.CommandDB == findCommand.CommandDB)
                                        && (d.ScheduleID == findCommandRepeat.ScheduleID)
                                        && (d.DesiredCPU == processName)
                                        && (d.CommandTimeDB == DateTime.MinValue)
                                        && (d.Process == null)).FirstOrDefault();
                    if (findCommandRepeatButRequested == null)
                    {
                        findCommandRepeatButRequested = UDBCommandList.Where(d => (d.CommandDB == findCommand.CommandDB)
                                            && (d.ScheduleID == findCommandRepeat.ScheduleID)
                            //&& (d.DesiredCPU == processName)  // -- to search other processors commands
                                            && (d.CommandTimeDB == DateTime.MinValue)
                                            && (d.Process == null)).FirstOrDefault();
                    }
                }


                if (findCommandRepeat == null || findCommandRepeatButRequested != null)
                {
                    findCommand.CommandTimeDB = timeLine;
                    findCommand.Process = processName;
                }
                else // found repeated command!
                {
                    if (findCommandRepeat.TypeDB == "START")
                    {
                        addNote("Second time seen START Entry: " + findCommandRepeat.CommandDB + " @ "
                            + timeLine.ToString("HH:mm:ss on MM/dd/yyyy"),
                            findCommandRepeat.ScheduleID, findCommandRepeat.Process, "Auto-Msg", false);
                    }
                }
            }
            else
            {
                if (  !(stTemp.ToLower().Contains("zd") || stTemp.ToLower().Contains("zstat")
                     || stTemp.ToLower().Contains("zpage") || stTemp.ToLower().Contains("zrcrs")
                     || stTemp.ToLower().Contains(" disp ") || stTemp.ToLower().Contains(" dis ")
                     || stTemp.ToLower().Contains("ztpsw") || stTemp.ToLower().Contains("zucdh") || RAVEN))
                {
                    AdminDB.LogIt_CommandsNotOnSchedule("" + sysName + ", " + processName + ":  " + stTemp + " (" + timeLine.ToString("HH:mm:ss") + ")");
                }
            }
            //else //!SENDALL
            //{
            //    LogCommandDB findCommand2 = null;
            //    findCommand2 = UDBCommandList.Where(d => (d.CommandDB.ToLower().Contains(stTemp))
            //                && (d.CommandDB.ToLower().Contains("sendall"))
            //                && (d.CommandTimeDB == DateTime.MinValue)
            //                && (d.Process == null)
            //                && (stTemp.Length > 4)).FirstOrDefault();

            //    if (findCommand2 != null) //find the whole Logcommand in DBCommand
            //    {
            //        findCommandRepeat = UDBCommandList.Where(d => (d.CommandDB.ToLower().Contains(stTemp)) && (d.CommandTimeDB != DateTime.MinValue) && (d.Process == processName)
            //                    && (d.CommandTimeDB.AddMinutes(+30) > timeLine)
            //                    && (d.CommandTimeDB <= timeLine)
            //                    ).FirstOrDefault();

            //        if (findCommandRepeat == null)
            //        {
            //            if (findCommand2.CommandDB.Contains("!SENDALL"))
            //            {
            //                findCommand2.CommandTimeDB = timeLine;
            //                findCommand2.Process = processName;
            //            }
            //        }
            //        else // found repeated command!
            //        {
            //            if (findCommandRepeat.TypeDB == "START")
            //            {
            //                addNote("Second time seen START Entry: " + findCommandRepeat.CommandDB + " @ "
            //                + timeLine.ToString("HH:mm:ss on MM/dd/yyyy"), findCommandRepeat.ScheduleID,
            //                findCommandRepeat.Process, "Auto-Msg", false);
            //            }
            //        }
            //    }
            //    else //double wrong
            //    {
            //        findCommandRepeat = UDBCommandList.Where(d => (d.CommandDB.ToLower() == stTemp)
            //                    && (d.CommandTimeDB != DateTime.MinValue)
            //                    && (d.Process == processName)
            //                    && (d.CommandTimeDB.AddMinutes(+30) > timeLine)
            //                    && (d.CommandTimeDB <= timeLine)).FirstOrDefault();

            //        if (findCommandRepeat != null) // found repeated command!
            //        {
            //            if (findCommandRepeat.TypeDB == "START")
            //            {
            //                addNote("Second time seen START Entry: " + findCommandRepeat.CommandDB + " @ "
            //                + timeLine.ToString("HH:mm:ss on MM/dd/yyyy"), findCommandRepeat.ScheduleID,
            //                findCommandRepeat.Process, "Auto-Msg", false);
            //            }
            //        }
            //    }
            //}
        }

        /// <summary>
        /// Amin: Search for the mounted tapes TAPE, then update volser
        /// </summary>
        /// <param name="l1"></param>
        /// <param name="l2"></param>
        private void TapeVolserFinder()
        {
            List<LogTapeCountDB> UDBTapeListCount;
            List<LogSingleTapeDB> UDBFinalTapeList;
            List<LogMultipleTapeDB> UDBTapeListMultiples;
            switch (sysName)
            {
                case "WNP":
                    UDBTapeListCount = DBTapeListCountWNP;
                    UDBFinalTapeList = DBFinalTapeListWNP;
                    UDBTapeListMultiples = DBTapeListMultiplesWNP;
                    break;
                case "FCA":
                    UDBTapeListCount = DBTapeListCountFCA;
                    UDBFinalTapeList = DBFinalTapeListFCA;
                    UDBTapeListMultiples = DBTapeListMultiplesFCA;
                    break;
                case "FOS":
                    UDBTapeListCount = DBTapeListCountFOS;
                    UDBFinalTapeList = DBFinalTapeListFOS;
                    UDBTapeListMultiples = DBTapeListMultiplesFOS;
                    break;
                case "PSS":
                    UDBTapeListCount = DBTapeListCountPSS;
                    UDBFinalTapeList = DBFinalTapeListPSS;
                    UDBTapeListMultiples = DBTapeListMultiplesPSS;
                    break;
                default:
                    return;
            }
            try
            {


                LogSingleTapeDB current = new LogSingleTapeDB();
                LogTapeCountDB currentTapeCount = null;
                current.TapeMountTimeDB = DateTime.Parse(LastUpdate);

                string[] temp = MultipleLinesDisplayStorage[0].Substring(109, MultipleLinesDisplayStorage[0].Length - 109).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                current.Partition = temp[3];
                current.TapeName = temp[5];
                current.Device = temp[9];
                current.IsInput = false;
                current.CPU = processName;
                if (Thread_processorName_fromLOG != processName && sysName != "WNP") // to adapt with ZSMP entry 
                {
                    return;
                }
                if (current.Partition == "BSS")
                {
                    current.Partition = Thread_partitionName;
                }

                if (!MultipleLinesDisplayStorage[1].Substring(109, MultipleLinesDisplayStorage[1].Length - 109).Contains("COMP"))
                {
                    current.IsInput = true;
                }
                if (current.TapeName == "POT")
                {

                }
                currentTapeCount = UDBTapeListCount.Where(x => (x.TapeName == current.TapeName && x.IsInput == current.IsInput)).FirstOrDefault();

                if (currentTapeCount != null)
                {
                    Regex regex = new Regex("VSN (.*?) ");
                    Match match = regex.Match(MultipleLinesDisplayStorage[1].Substring(109, MultipleLinesDisplayStorage[1].Length - 109));
                    current.volser = match.Groups[1].Value;
                    if (currentTapeCount.Count == 1)
                    {
                        LogSingleTapeDB CheckPreTape = null;
                        LogMultipleTapeDB tapeInfo = null;

                        CheckPreTape = UDBFinalTapeList.Where(z => z.TapeName == current.TapeName && z.Partition == current.Partition).FirstOrDefault();
                        tapeInfo = UDBTapeListMultiples.Where(x => (x.TapeName == current.TapeName && x.Partition == current.Partition
                                                                        && x.IsInput == current.IsInput)).FirstOrDefault();

                        if (CheckPreTape == null || current.IsInput)  // check to prevent double matching of tape close to the midnight for next day tapes
                        {
                            current.ScheduleID = tapeInfo.ScheduleID;
                            current.TapeID = tapeInfo.TapeID;
                        }
                        current.OS390JobName = tapeInfo.OS390JobName;
                    }
                    else //if (current.IsInput == true)
                    // Amin: because of change in the job publication process
                    //       (jobs scheduleIDs are in order ased on chain and job times)
                    {
                        List<LogMultipleTapeDB> tapeInfos = null;

                        var multiRunTape = UDBFinalTapeList.Where(d => d.TapeName == current.TapeName
                            // && d.Device == current.Device 
                                                  && d.IsInput == true
                                                  && d.Partition == current.Partition
                                                  && d.volser == current.volser
                                                  && d.RunTapeTimeDB != DateTime.MinValue).FirstOrDefault();

                        if (multiRunTape == null)
                        {


                            tapeInfos = UDBTapeListMultiples.Where(x => (x.TapeName == current.TapeName
                                                                        && x.IsInput == current.IsInput
                                                                        && x.Partition == current.Partition
                                                                        && x.CPU == current.CPU)).ToList();
                            if (tapeInfos.Count == 0) // if tape mounted on the diffrent CPU
                            {
                                tapeInfos = UDBTapeListMultiples.Where(x => (x.TapeName == current.TapeName
                                                                        && x.IsInput == current.IsInput
                                                                        && x.Partition == current.Partition)).ToList();
                            }

                            if (tapeInfos != null && tapeInfos.Count > 0)
                            {

                                long MinScheduleID = tapeInfos.Min(z => z.ScheduleID);
                                long MaxScheduleID = tapeInfos.Max(z => z.ScheduleID);
                                int FirstTime, LastTime;
                                int.TryParse(tapeInfos.Where(z => z.ScheduleID == MinScheduleID).FirstOrDefault().startTime, out FirstTime);
                                int.TryParse(tapeInfos.Where(z => z.ScheduleID == MaxScheduleID).FirstOrDefault().startTime, out LastTime);
                                if (LastTime > 0 && LastTime < FirstTime) // if some adhoc job added, the schedule_ID is not in order. it needs to pick biggest schedule_ID
                                {
                                    MinScheduleID = MaxScheduleID;
                                }
                                LogMultipleTapeDB tapeInfo = UDBTapeListMultiples.Where(x => (x.TapeName == current.TapeName
                                                                                                && x.IsInput == current.IsInput
                                                                                                && x.CPU == current.CPU
                                                                                                && x.Partition == current.Partition
                                                                                                && x.ScheduleID == MinScheduleID)).FirstOrDefault();
                                if (tapeInfo == null) // if tape mounted on the diffrent CPU
                                {
                                    tapeInfo = UDBTapeListMultiples.Where(x => (x.TapeName == current.TapeName
                                                                                                && x.IsInput == current.IsInput
                                                                                                && x.Partition == current.Partition
                                                                                                && x.ScheduleID == MinScheduleID)).FirstOrDefault();
                                }
                                current.ScheduleID = tapeInfo.ScheduleID;
                                current.TapeID = tapeInfo.TapeID;
                                current.OS390JobName = tapeInfo.OS390JobName;
                                int deleteCount = UDBTapeListMultiples.RemoveAll(x => (x.TapeName == current.TapeName
                                                             && x.IsInput == current.IsInput
                                                             && x.Partition == current.Partition
                                                             && x.ScheduleID == current.ScheduleID
                                                             && x.CPU == current.CPU));
                                if (deleteCount == 0) // if mounted on the diffrent CPU
                                {
                                    UDBTapeListMultiples.RemoveAll(x => (x.TapeName == current.TapeName
                                                               && x.IsInput == current.IsInput
                                                               && x.Partition == current.Partition
                                                               && x.ScheduleID == current.ScheduleID));
                                }
                            }

                        }
                        else
                        {
                            current.ScheduleID = multiRunTape.ScheduleID;
                            current.TapeID = multiRunTape.TapeID;
                            current.OS390JobName = multiRunTape.OS390JobName;
                        }
                    }
                    if ((current.ScheduleID == 69015)
                        || (current.ScheduleID == 69000)
                       || (current.ScheduleID == 68979)
                        )
                    {

                    }
                    var TargetTape = UDBFinalTapeList.Where(d => d.TapeName == current.TapeName
                        // && d.Device == current.Device 
                                                    && d.IsInput == true
                                                    && d.Partition == current.Partition
                                                    && d.volser == current.volser
                                                    && d.RunTapeTimeDB != DateTime.MinValue).FirstOrDefault();

                    if (TargetTape != null)
                    {
                        TargetTape.TapeMountTimeDB = current.TapeMountTimeDB;
                        TargetTape.CPU = current.CPU;
                        TargetTape.Device = current.Device;
                        current.UpdateTapeMountedtoDB();
                    }
                    else if (UDBFinalTapeList.Where(d => (d.TapeName == current.TapeName
                                                        && d.Device == current.Device
                                                        && d.Partition == current.Partition
                                                        && d.volser == current.volser
                                                        && d.TapeMountTimeDB == current.TapeMountTimeDB)).FirstOrDefault() == null)
                    {
                        UDBFinalTapeList.Add(current);
                        if (AdminAutoUpdate && UserAutoStartTapeUpdate)
                        {
                            current.AddTapetoDB();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AdminDB.LogIt(ex.Message, ex.StackTrace, Counter, processName);
            }
        }

        /// <summary>
        /// Nafise: RunTape
        /// </summary>
        /// <param name="input"></param>
        private void TosAutomation_RunTape(string input)
        {
            List<LogTapeCountDB> UDBTapeListCount;
            List<LogSingleTapeDB> UDBFinalTapeList;
            List<LogMultipleTapeDB> UDBTapeListMultiples;
            string stTPFjobname = "";
           // List<JobReference> UDBRuntape;
            switch (sysName)
            {
                case "FCA":
                    UDBTapeListCount = DBTapeListCountFCA;
                    UDBFinalTapeList = DBFinalTapeListFCA;
                    UDBTapeListMultiples = DBTapeListMultiplesFCA;
                   // UDBRuntape = RuntapeFCA;
                    break;
                case "FOS":
                    UDBTapeListCount = DBTapeListCountFOS;
                    UDBFinalTapeList = DBFinalTapeListFOS;
                    UDBTapeListMultiples = DBTapeListMultiplesFOS;
                 //   UDBRuntape = RuntapeFOS;
                    break;
                case "PSS":
                    UDBTapeListCount = DBTapeListCountPSS;
                    UDBFinalTapeList = DBFinalTapeListPSS;
                    UDBTapeListMultiples = DBTapeListMultiplesPSS;
                  //  UDBRuntape = RuntapePSS;
                    break;
                default:
                    return;
            }
            try
            {
                LogSingleTapeDB current = new LogSingleTapeDB();
                LogTapeCountDB currentTapeCount = null;

                //RSSW0001I 03.35.36 RUNTAPE: THE PSS AA O09112SG XCOMLK (112SG) HAS BEEN CREATED, PROCESS PER RUN SHEET
                //RSSW0001I 04.36.24 RUNTAPE: THE PSS 9W NBM TAPE 092964 (MHC9W3) HAS BEEN CREATED, PROCESS PER RUN SHEET
                string stLine = input.Substring(109, input.Length - 109);
                string[] temp2 = stLine.Split(new string[] { " ", "(", ")" }, StringSplitOptions.RemoveEmptyEntries);

                string time = temp2[1].Replace('.', ':');
                string date = LastUpdate.Substring(0, 10);
                string RuntapeTime = date + " " + time;
                if (temp2.Length > 8)
                {
                    current.Partition = temp2[5];
                   
                    current.IsInput = true;
                    current.RunTapeTimeDB = DateTime.Parse(RuntapeTime);

                    if (temp2[7] == "TAPE")
                    {
                        current.OS390JobName = temp2[9];
                        current.volser = temp2[8];
                        current.TapeName = temp2[6];
                    }
                    else if (temp2[7] == "XCOMLK")
                    {
                        current.OS390JobName = temp2[8];
                        stTPFjobname = temp2[6];
                        current.ScheduleID = (new ScheduleJob()).Get_Schedule_ID(stTPFjobname, DateTime.Now.Date.ToString("yyyy-MM-dd"));
                        if (current.ScheduleID > 0 && current.ScheduleID != null)
                        {
                            //Adding Runtape to DB
                            current.AddRuntapetoDB();
                        }
                    }
                }

                if (current.TapeName == "POT")
                {

                }

                currentTapeCount = UDBTapeListCount.Where(x => (x.TapeName == current.TapeName && x.IsInput == current.IsInput)).FirstOrDefault();

                if (currentTapeCount != null)
                {
                    if (currentTapeCount.Count == 1)
                    {
                        LogSingleTapeDB CheckPreTape = null;
                        LogMultipleTapeDB tapeInfo = null;

                        CheckPreTape = UDBFinalTapeList.Where(z => z.TapeName == current.TapeName && z.Partition == current.Partition).FirstOrDefault();
                        tapeInfo = UDBTapeListMultiples.Where(x => (x.TapeName == current.TapeName && x.Partition == current.Partition
                                                                        && x.IsInput == current.IsInput)).FirstOrDefault();

                        if (CheckPreTape == null || current.IsInput)  // check to prevent double matching of tape close to the midnight for next day tapes
                        {
                            current.ScheduleID = tapeInfo.ScheduleID;
                            current.TapeID = tapeInfo.TapeID;
                        }
                    }
                    else //if (current.IsInput == true)
                    // Amin: because of change in the job publication process
                    //       (jobs scheduleIDs are in order based on chain and job times)
                    {
                        List<LogMultipleTapeDB> tapeInfos = null;

                        tapeInfos = UDBTapeListMultiples.Where(x => (x.TapeName == current.TapeName
                                                                    && x.IsInput == current.IsInput
                                                                    && x.Partition == current.Partition
                                                                    && x.CPU == current.CPU)).ToList();
                        if (tapeInfos.Count == 0) // if tape mounted on the diffrent CPU
                        {
                            tapeInfos = UDBTapeListMultiples.Where(x => (x.TapeName == current.TapeName
                                                                    && x.IsInput == current.IsInput
                                                                    && x.Partition == current.Partition)).ToList();
                        }

                        if (tapeInfos != null && tapeInfos.Count > 0)
                        {
                            long MinScheduleID = tapeInfos.Min(z => z.ScheduleID);
                            long MaxScheduleID = tapeInfos.Max(z => z.ScheduleID);
                            int FirstTime, LastTime;
                            int.TryParse(tapeInfos.Where(z => z.ScheduleID == MinScheduleID).FirstOrDefault().startTime, out FirstTime);
                            int.TryParse(tapeInfos.Where(z => z.ScheduleID == MaxScheduleID).FirstOrDefault().startTime, out LastTime);
                            if (LastTime > 0 && LastTime < FirstTime) // if some adhoc job added, the schedule_ID is not in order. it needs to pick biggest schedule_ID
                            {
                                MinScheduleID = MaxScheduleID;
                            }
                            LogMultipleTapeDB tapeInfo = UDBTapeListMultiples.Where(x => (x.TapeName == current.TapeName
                                                                                            && x.IsInput == current.IsInput
                                                                                            && x.CPU == current.CPU
                                                                                            && x.Partition == current.Partition
                                                                                            && x.ScheduleID == MinScheduleID)).FirstOrDefault();
                            if (tapeInfo == null) // if tape mounted on the diffrent CPU
                            {
                                tapeInfo = UDBTapeListMultiples.Where(x => (x.TapeName == current.TapeName
                                                                                            && x.IsInput == current.IsInput
                                                                                            && x.Partition == current.Partition
                                                                                            && x.ScheduleID == MinScheduleID)).FirstOrDefault();
                            }
                            current.ScheduleID = tapeInfo.ScheduleID;
                            current.TapeID = tapeInfo.TapeID;
                            
                            int deleteCount = UDBTapeListMultiples.RemoveAll(x => (x.TapeName == current.TapeName
                                                         && x.IsInput == current.IsInput
                                                         && x.Partition == current.Partition
                                                         && x.ScheduleID == current.ScheduleID
                                                         && x.CPU == current.CPU));
                            if (deleteCount == 0) // if mounted on the diffrent CPU
                            {
                                UDBTapeListMultiples.RemoveAll(x => (x.TapeName == current.TapeName
                                                           && x.IsInput == current.IsInput
                                                           && x.Partition == current.Partition
                                                           && x.ScheduleID == current.ScheduleID));
                            }

                        }
                    }

                    if (UDBFinalTapeList.Where(d => (d.TapeName == current.TapeName
                                                        && d.Partition == current.Partition
                                                        && d.volser == current.volser
                                                        && d.RunTapeTimeDB == current.RunTapeTimeDB)).FirstOrDefault() == null)
                    {
                        UDBFinalTapeList.Add(current);
                        if (current.ScheduleID > 0 && current.ScheduleID != null)
                            {
                               //Adding Runtape to DB
                                current.AddRuntapetoDB();
                            }
                        if (AdminAutoUpdate && UserAutoStartTapeUpdate)
                        {
                            current.AddTapetoDB();
                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AdminDB.LogIt(ex.Message, ex.StackTrace, Counter, processName);
            }
        }

        /// <summary>
        /// Amin: Search for the removed tapes TAPE, then update volser
        /// </summary>
        /// <param name="l1"></param>
        /// <param name="l2"></param>
        private void TapeRemoveFinder()
        {
            List<LogTapeCountDB> UDBTapeListCount;
            List<LogSingleTapeDB> UDBFinalTapeList;
            List<LogMultipleTapeDB> UDBTapeListMultiples;
            List<LogCommandDB> UDBCommandList;
            switch (sysName)
            {
                case "WNP":
                    UDBTapeListCount = DBTapeListCountWNP;
                    UDBFinalTapeList = DBFinalTapeListWNP;
                    UDBTapeListMultiples = DBTapeListMultiplesWNP;
                    UDBCommandList = DBCommandListWNP;
                    break;
                case "FCA":
                    UDBTapeListCount = DBTapeListCountFCA;
                    UDBFinalTapeList = DBFinalTapeListFCA;
                    UDBTapeListMultiples = DBTapeListMultiplesFCA;
                    UDBCommandList = DBCommandListFCA;
                    break;
                case "FOS":
                    UDBTapeListCount = DBTapeListCountFOS;
                    UDBFinalTapeList = DBFinalTapeListFOS;
                    UDBTapeListMultiples = DBTapeListMultiplesFOS;
                    UDBCommandList = DBCommandListFOS;
                    break;
                case "PSS":
                    UDBTapeListCount = DBTapeListCountPSS;
                    UDBFinalTapeList = DBFinalTapeListPSS;
                    UDBTapeListMultiples = DBTapeListMultiplesPSS;
                    UDBCommandList = DBCommandListPSS;
                    break;
                default:
                    return;
            }
            try
            {
                LogSingleTapeDB current = new LogSingleTapeDB();
                LogTapeCountDB currentTapeCount = null;
                current.TapeRemoveTimeDB = DateTime.Parse(LastUpdate);

                string[] temp = MultipleLinesDisplayStorage[0].Substring(109, MultipleLinesDisplayStorage[0].Length - 109).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                current.Partition = temp[temp.Length - 6];
                current.TapeName = temp[temp.Length - 4];
                current.Device = temp[temp.Length - 1];
                current.CPU = processName;
                if (Thread_processorName_fromLOG != processName && sysName != "WNP") // to adapt with ZSMP entry 
                {
                    return;
                }                 

                if (current.Partition == "BSS")
                {
                    current.Partition = Thread_partitionName;
                }

                if (!MultipleLinesDisplayStorage[1].Substring(109, MultipleLinesDisplayStorage[1].Length - 109).Contains("COMP"))
                {
                    current.IsInput = true;
                }
                if (current.TapeName == "POT")
                {

                }
                //currentTapeCount = UDBTapeListCount.Where(x => (x.TapeName == current.TapeName && x.IsInput == false)).FirstOrDefault();
                currentTapeCount = UDBTapeListCount.Where(x => x.TapeName == current.TapeName).FirstOrDefault();

                if (current.IsInput==false && currentTapeCount != null)
                {
                    string[] temp2 = MultipleLinesDisplayStorage[1].Substring(109, MultipleLinesDisplayStorage[1].Length - 109).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                
                    //Regex regex = new Regex("VSN (.*?) ");
                    //Match match = regex.Match(MultipleLinesDisplayStorage[1]);
                    current.volser = temp2[1];
                    current.BLockType = temp2[6];
                    current.BlockCount = int.Parse(temp2[7]);


                    if (currentTapeCount.Count == 1)
                    {
                        LogMultipleTapeDB tapeInfo = UDBTapeListMultiples.Where(x => (x.TapeName == current.TapeName 
                                                                                        && x.Partition == current.Partition 
                                                                                        && x.IsInput == false)).FirstOrDefault();
                        current.ScheduleID = tapeInfo.ScheduleID;
                        current.TapeID = tapeInfo.TapeID;
                        current.OS390JobName = tapeInfo.OS390JobName;
                    }
                    else // find the tape that being used by more than one job
                    // Amin: because of order of schedule_IDs and change on the mounted tapes
                    // if tape mounted time was not recorded, we use the smallest ScheduleID
                    {
                        List<LogSingleTapeDB> TargetTapes = UDBFinalTapeList.Where(d => d.TapeName == current.TapeName
                                                                               && d.Device == current.Device
                                                                               && d.Partition == current.Partition 
                                                                               && d.volser == current.volser
                                                                               && d.TapeRemoveTimeDB == DateTime.MinValue).ToList();
                        if (TargetTapes.Count == 1)
                        {
                            current.ScheduleID = TargetTapes[0].ScheduleID;
                            current.TapeID = TargetTapes[0].TapeID;
                            current.OS390JobName = TargetTapes[0].OS390JobName;
                        }
                        else
                        {
                            List<LogMultipleTapeDB> tapeInfos = UDBTapeListMultiples.Where(x => (x.TapeName == current.TapeName
                                                                                                && x.Partition == current.Partition 
                                                                                                && x.IsInput == current.IsInput)).ToList();

                            if (tapeInfos != null && tapeInfos.Count > 0)
                            {
                                long MinScheduleID = tapeInfos.Min(z => z.ScheduleID);
                                long MaxScheduleID = tapeInfos.Max(z => z.ScheduleID);
                                int FirstTime, LastTime;
                                int.TryParse(tapeInfos.Where(z => z.ScheduleID == MinScheduleID).FirstOrDefault().startTime, out FirstTime);
                                int.TryParse(tapeInfos.Where(z => z.ScheduleID == MaxScheduleID).FirstOrDefault().startTime, out LastTime);
                                if (LastTime > 0 && LastTime < FirstTime) // if some adhoc job added, the schedule_ID is not in order. it needs to pick biggest schedule_ID
                                {
                                    MinScheduleID = MaxScheduleID;
                                }

                                LogMultipleTapeDB tapeInfo = UDBTapeListMultiples.Where(x => (x.TapeName == current.TapeName
                                                                                                && x.Partition == current.Partition 
                                                                                                && x.IsInput == current.IsInput
                                                                                                && x.CPU == current.CPU
                                                                                                && x.ScheduleID == MinScheduleID)).FirstOrDefault();
                                if (tapeInfo == null) // if tape mounted on the diffrent CPU
                                {
                                    tapeInfo = UDBTapeListMultiples.Where(x => (x.TapeName == current.TapeName
                                                                                                && x.Partition == current.Partition
                                                                                                && x.IsInput == current.IsInput
                                                                                                && x.ScheduleID == MinScheduleID)).FirstOrDefault();
                                } 
                                current.ScheduleID = tapeInfo.ScheduleID;
                                current.TapeID = tapeInfo.TapeID;
                                int deleteCount = UDBTapeListMultiples.RemoveAll(x => (x.TapeName == current.TapeName
                                                             && x.IsInput == current.IsInput
                                                             && x.Partition == current.Partition 
                                                             && x.ScheduleID == current.ScheduleID
                                                             && x.CPU == current.CPU));
                                if (deleteCount == 0) // if mounted on the dofrent CPU
                                {
                                    UDBTapeListMultiples.RemoveAll(x => (x.TapeName == current.TapeName
                                                             && x.IsInput == current.IsInput
                                                             && x.Partition == current.Partition
                                                             && x.ScheduleID == current.ScheduleID));
                                }

                            }
                        }
                    }


                    if (UDBFinalTapeList.Where(d => (d.TapeName == current.TapeName && d.IsInput == false
                                                                && d.Device == current.Device
                                                                && d.Partition == current.Partition 
                                                                && d.volser == current.volser
                                                                && d.TapeRemoveTimeDB == DateTime.MinValue)).FirstOrDefault() == null)
                    {
                        UDBFinalTapeList.Add(current);
                        if (AdminAutoUpdate && UserAutoStartTapeUpdate)
                        {
                            current.AddTapetoDB();
                            current.UpdateTapeCameDowntoDB();
                        }
                    }
                    else
                    {
                        var TargetTape = UDBFinalTapeList.Where(d => d.TapeName == current.TapeName
                                                && d.Device == current.Device && d.IsInput == false
                                                && d.Partition == current.Partition && d.volser == current.volser
                                                && d.TapeRemoveTimeDB == DateTime.MinValue).FirstOrDefault();
                        TargetTape.TapeRemoveTimeDB = current.TapeRemoveTimeDB;
                        TargetTape.ScheduleID = current.ScheduleID;
                        TargetTape.TapeID = current.TapeID;
                        TargetTape.Device = current.Device;
                        TargetTape.CPU = current.CPU;
                        TargetTape.BLockType = current.BLockType;
                        TargetTape.BlockCount = current.BlockCount;
                        if (AdminAutoUpdate && UserAutoStartTapeUpdate)
                        {
                            current.UpdateTapeCameDowntoDB();
                        }
                    }
                }
                else if (current.IsInput == true)
                {
                    // input tape
                    currentTapeCount = UDBTapeListCount.Where(x => (x.TapeName == current.TapeName && x.IsInput == true)).FirstOrDefault();
                    if (currentTapeCount != null)
                    {
                        string[] temp2 = MultipleLinesDisplayStorage[1].Substring(109, MultipleLinesDisplayStorage[1].Length - 109).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                        //Regex regex = new Regex("VSN (.*?) ");
                        //Match match = regex.Match(MultipleLinesDisplayStorage[1]);
                        current.volser = temp2[1];
                        current.BLockType = temp2[2];
                        current.BlockCount = int.Parse(temp2[3]);
                        if (currentTapeCount.Count == 1)
                        {
                            LogMultipleTapeDB tapeInfo = UDBTapeListMultiples.Where(x => (x.TapeName == current.TapeName
                                                                                            && x.Partition == current.Partition
                                                                                            && x.IsInput == true)).FirstOrDefault();
                            current.ScheduleID = tapeInfo.ScheduleID;
                            current.TapeID = tapeInfo.TapeID;
                            current.OS390JobName = tapeInfo.OS390JobName;
                            current.IsInput = true;
                        }
                        else
                        {
                            List<LogSingleTapeDB> TargetTapes = UDBFinalTapeList.Where(d => d.TapeName == current.TapeName
                                                                               && d.Device == current.Device
                                                                               && d.Partition == current.Partition
                                                                               && d.volser == current.volser
                                                                               && d.TapeRemoveTimeDB == DateTime.MinValue).ToList();
                            if (TargetTapes.Count == 1)
                            {
                                current.ScheduleID = TargetTapes[0].ScheduleID;
                                current.TapeID = TargetTapes[0].TapeID;
                                current.OS390JobName = TargetTapes[0].OS390JobName;
                            }
                            else
                            {
                                List<LogMultipleTapeDB> tapeInfos = UDBTapeListMultiples.Where(x => (x.TapeName == current.TapeName
                                                                                                   && x.Partition == current.Partition
                                                                                                   && x.IsInput == true)).ToList();

                                if (tapeInfos != null && tapeInfos.Count > 0)
                                {
                                    long MinScheduleID = tapeInfos.Min(z => z.ScheduleID);
                                    long MaxScheduleID = tapeInfos.Max(z => z.ScheduleID);
                                    int FirstTime, LastTime;
                                    int.TryParse(tapeInfos.Where(z => z.ScheduleID == MinScheduleID).FirstOrDefault().startTime, out FirstTime);
                                    int.TryParse(tapeInfos.Where(z => z.ScheduleID == MaxScheduleID).FirstOrDefault().startTime, out LastTime);
                                    if (LastTime > 0 && LastTime < FirstTime) // if some adhoc job added, the schedule_ID is not in order. it needs to pick biggest schedule_ID
                                    {
                                        MinScheduleID = MaxScheduleID;
                                    }

                                    LogMultipleTapeDB tapeInfo = UDBTapeListMultiples.Where(x => (x.TapeName == current.TapeName
                                                                                                    && x.Partition == current.Partition
                                                                                                    && x.IsInput == true
                                                                                                    && x.CPU == current.CPU
                                                                                                    && x.ScheduleID == MinScheduleID)).FirstOrDefault();
                                    if (tapeInfo == null) // if tape mounted on the diffrent CPU
                                    {
                                        tapeInfo = UDBTapeListMultiples.Where(x => (x.TapeName == current.TapeName
                                                                                                    && x.Partition == current.Partition
                                                                                                    && x.IsInput == true
                                                                                                    && x.ScheduleID == MinScheduleID)).FirstOrDefault();
                                    }
                                    current.ScheduleID = tapeInfo.ScheduleID;
                                    current.TapeID = tapeInfo.TapeID;
                                    int deleteCount = UDBTapeListMultiples.RemoveAll(x => (x.TapeName == current.TapeName
                                                                 && x.IsInput == true
                                                                 && x.Partition == current.Partition
                                                                 && x.ScheduleID == current.ScheduleID
                                                                 && x.CPU == current.CPU));
                                    if (deleteCount == 0) // if mounted on the dofrent CPU
                                    {
                                        UDBTapeListMultiples.RemoveAll(x => (x.TapeName == current.TapeName
                                                                 && x.IsInput == true
                                                                 && x.Partition == current.Partition
                                                                 && x.ScheduleID == current.ScheduleID));
                                    }

                                }
                            }
                        }

                        var TargetTape = UDBFinalTapeList.Where(d => d.TapeName == current.TapeName
                                                    && d.Device == current.Device
                                                    && d.IsInput == true
                                                    && d.Partition == current.Partition
                                                    && d.volser == current.volser
                                                    && d.TapeRemoveTimeDB == DateTime.MinValue).FirstOrDefault();
                        if (TargetTape == null)
                        {
                            UDBFinalTapeList.Add(current);
                            if (AdminAutoUpdate && UserAutoStartTapeUpdate)
                            {
                                current.AddTapetoDB();
                                current.UpdateTapeCameDowntoDB();
                            }
                        }
                        else
                        {
                            TargetTape.TapeRemoveTimeDB = current.TapeRemoveTimeDB;
                            TargetTape.BLockType = current.BLockType;
                            TargetTape.BlockCount = current.BlockCount;
                            if (AdminAutoUpdate && UserAutoStartTapeUpdate)
                            {
                                current.UpdateTapeCameDowntoDB();
                            }
                        }
                    }
                }
                if (current.ScheduleID > 0)
                {
                    EndJobCheck(current.ScheduleID, " Tape remove");
                }

            }
            catch (Exception ex)
            {
                AdminDB.LogIt(ex.Message, ex.StackTrace, Counter, processName);
            }
        }
        
        /// <summary>
        /// Amin: Search for the ZTOFF entry to remove tapes from system
        /// </summary>
        /// <param name="l1"></param>
        /// <param name="l2"></param>
        private void TapeZTOFF()
        {
            List<LogSingleTapeDB> UDBFinalTapeList;
            switch (sysName)
            {
                case "WNP":
                    UDBFinalTapeList = DBFinalTapeListWNP;
                    break;
                case "FCA":
                    UDBFinalTapeList = DBFinalTapeListFCA;
                    break;
                case "FOS":
                    UDBFinalTapeList = DBFinalTapeListFOS;
                    break;
                case "PSS":
                    UDBFinalTapeList = DBFinalTapeListPSS;
                    break;
                default:
                    return;
            }
            try
            {
                LogSingleTapeDB current = new LogSingleTapeDB();

                string[] temp = MultipleLinesDisplayStorage[0].Substring(109, MultipleLinesDisplayStorage[0].Length - 109).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                current.TapeName = temp[temp.Length - 4];
                current.Device = temp[temp.Length - 1];
                current.CPU = processName;
                if (Thread_processorName_fromLOG != processName && sysName != "WNP") // to adapt with ZSMP entry 
                {
                    return;
                } 
                Regex regex = new Regex("VSN (.*?) ");
                Match match = regex.Match(MultipleLinesDisplayStorage[1]);
                current.volser = match.Groups[1].Value;
                UDBFinalTapeList.RemoveAll(d => (d.TapeName == current.TapeName
                                                   && d.Device == current.Device
                                                   && d.volser == current.volser));
                if (AdminAutoUpdate && UserAutoStartTapeUpdate)
                {
                    current.DeleteTapefromDB();
                }
            }
            catch (Exception ex)
            {
                AdminDB.LogIt(ex.Message, ex.StackTrace, Counter, processName);
            }
        }

        /// <summary>
        /// Amin: Search for the Auto Submit TAPE
        /// </summary>
        /// <param name="l1"></param>
        /// <param name="l2"></param>
        private void TapeAutoSubmit(string l1)
        {
            List<LogSingleTapeDB> UDBFinalTapeList;
            switch (sysName)
            {
                case "WNP":
                    UDBFinalTapeList = DBFinalTapeListWNP;
                    break;
                case "FCA":
                    UDBFinalTapeList = DBFinalTapeListFCA;
                    break;
                case "FOS":
                    UDBFinalTapeList = DBFinalTapeListFOS;
                    break;
                case "PSS":
                    UDBFinalTapeList = DBFinalTapeListPSS;
                    break;
                default:
                    return;
            }
            try
            {
                LogSingleTapeDB current = new LogSingleTapeDB();
                
                string[] temp = l1.Substring(109, l1.Length - 109).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (temp[2] == "AUTO" && temp[3] == "SUBMIT" && temp[4] == "COMPLETED")
                {
                    current.TapeSubmitTimeDB = DateTime.Parse(LastUpdate);
                    current.volser = temp[13];
                    current.TapeName = temp[11];
                    current.OS390JobName = temp[12];
                    current.CPU = processName;
                    if (Thread_processorName_fromLOG != processName && sysName != "WNP") // to adapt with ZSMP entry 
                    {
                        return;
                    } 
                    current.Partition = temp[10];
                    if (current.Partition == "BSS")
                    {
                        current.Partition = Thread_partitionName;
                    }

                    LogSingleTapeDB findtape = UDBFinalTapeList.Where(d => (d.volser == current.volser
                                                            && d.TapeName == current.TapeName
                                                            && d.TapeSubmitTimeDB == DateTime.MinValue)).FirstOrDefault();
                    if (findtape != null)
                    {
                        current.TapeID = findtape.TapeID;
                        current.ScheduleID = findtape.ScheduleID;
                        findtape.TapeSubmitTimeDB = current.TapeSubmitTimeDB;
                        if (findtape.OS390JobName != current.OS390JobName
                           // && (findtape.TapeMountTimeDB == DateTime.MinValue
                           //         && !(findtape.OS390JobName == "" || findtape.OS390JobName == null))
                            )
                        {
                            AdminDB.LogIt("(TapeID:" + current.TapeID + ") Wrong OS390 Name of " + current.TapeName + " tape on " + sysName + ": "
                                + findtape.OS390JobName + " <-> " + current.OS390JobName);
                        }
                        findtape.OS390JobName = current.OS390JobName;
                        if (findtape.Partition != current.Partition)
                        {
                            AdminDB.LogIt("(TapeID:" + current.TapeID + ") Wrong patition of " + current.TapeName + " tape on " + sysName + ": "
                                + findtape.Partition + " <-> " + current.Partition);
                        }
                        findtape.Partition = current.Partition;
                        if (AdminAutoUpdate && UserAutoStartTapeUpdate)
                        {
                            current.UpdateTapeSubmittoDB();
                        }
                    }
                }
                if (current.ScheduleID > 0)
                {
                    EndJobCheck(current.ScheduleID, "Tape submit");
                }
            }
            catch (Exception ex)
            {
                AdminDB.LogIt(ex.Message, ex.StackTrace, Counter, processName);
            }
        }

        /// <summary>
        /// Amin: Search for the TPFBAT
        /// </summary>
        /// <param name="l1"></param>
        /// <param name="l2"></param>
        private void TapeTPFBAT(string l1)
        {
            List<LogTapeCountDB> UDBTapeListCount;
            List<LogSingleTapeDB> UDBFinalTapeList;
            List<LogMultipleTapeDB> UDBTapeListMultiples;
            switch (sysName)
            {
                case "WNP":
                    UDBTapeListCount = DBTapeListCountWNP;
                    UDBFinalTapeList = DBFinalTapeListWNP;
                    UDBTapeListMultiples = DBTapeListMultiplesWNP;
                    break;
                case "FCA":
                    UDBTapeListCount = DBTapeListCountFCA;
                    UDBFinalTapeList = DBFinalTapeListFCA;
                    UDBTapeListMultiples = DBTapeListMultiplesFCA;
                    break;
                case "FOS":
                    UDBTapeListCount = DBTapeListCountFOS;
                    UDBFinalTapeList = DBFinalTapeListFOS;
                    UDBTapeListMultiples = DBTapeListMultiplesFOS;
                    break;
                case "PSS":
                    UDBTapeListCount = DBTapeListCountPSS;
                    UDBFinalTapeList = DBFinalTapeListPSS;
                    UDBTapeListMultiples = DBTapeListMultiplesPSS;
                    break;
                default:
                    return;
            }
            try
            {
                LogSingleTapeDB current = new LogSingleTapeDB();

                string[] temp = l1.Substring(109, l1.Length - 109).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (temp[2] == "AUTO" && temp[3] == "SUBMIT" &&  temp[4] == "REQUESTED")
                {
                    current.TapeSubmitTimeDB = DateTime.Parse(LastUpdate);
                    current.volser = temp[11];
                    current.OS390JobName = temp[10];
                    current.CPU = processName;
                    if (Thread_processorName_fromLOG != processName && sysName != "WNP") // to adapt with ZSMP entry 
                    {
                        return;
                    } 
                    LogSingleTapeDB findtape = UDBFinalTapeList.Where(d => (d.volser == current.volser
                                                            && d.OS390JobName == current.OS390JobName
                                                            && d.TapeSubmitTimeDB == DateTime.MinValue)).FirstOrDefault();
                    if (findtape != null)
                    {
                        current.TapeID = findtape.TapeID;
                        current.ScheduleID = findtape.ScheduleID;
                        findtape.TapeSubmitTimeDB = current.TapeSubmitTimeDB;
                        findtape.OS390JobName = current.OS390JobName;
                        if (AdminAutoUpdate && UserAutoStartTapeUpdate)
                        {
                            current.UpdateTapeSubmittoDB();
                        }
                    }
                    else
                    {
                        findtape = UDBFinalTapeList.Where(d => (d.volser == current.volser
                                                            && d.OS390JobName.Length >1
                                                            && d.CPU == current.CPU
                                                            && d.TapeSubmitTimeDB != DateTime.MinValue)).FirstOrDefault();
                        if (findtape != null)
                        {
                            current.TapeID = findtape.TapeID;
                            current.ScheduleID = findtape.ScheduleID;
                            findtape.TapeSubmitTimeDB = current.TapeSubmitTimeDB;
                            if (AdminAutoUpdate && UserAutoStartTapeUpdate)
                            {
                                current.UpdateTapeSubmittoDB();
                            }
                            findtape.OS390JobName += " & " + current.OS390JobName;
                        }
                    }
                }
                if (current.ScheduleID > 0)
                {
                    EndJobCheck(current.ScheduleID, "Tape TPFbat");
                }
            }
            catch (Exception ex)
            {
                AdminDB.LogIt(ex.Message, ex.StackTrace, Counter, processName);
            }
        }

        /// <summary>
        /// Amin: to list a dump
        /// </summary>
        /// <param name="Lines"></param>
        private void DumpLister()
        {
            try
            {
                int seq = -1;
                if (MultipleLinesDisplayStorage.Count >= 3 && MultipleLinesDisplayStorage[0].Contains("SSU"))
                {
                    LogDump Current = new LogDump();
                    Current.Count = 1;
                    //first line
                    string Sequence = "";
                    string[] temp = MultipleLinesDisplayStorage[0].Substring(109, MultipleLinesDisplayStorage[0].Length - 109).Split(new string[] { " ", "IS-", "SS-", "SSU-", "SE-", "." },
                                                                                         StringSplitOptions.RemoveEmptyEntries);
                    Current.LastOne = new DateTime(DateOfLog.Year, DateOfLog.Month, DateOfLog.Day,
                                                    int.Parse(temp[1].ToString()), int.Parse(temp[2].ToString()), int.Parse(temp[3].ToString()));
                    Current.IStream = int.Parse(temp[4].ToString());
                    Current.SubSystem = temp[5];
                    Current.SubSystemUser = temp[6];
                    Sequence = temp[7];
                    Current.DumpType = temp[8];

                    //Second line
                    temp = MultipleLinesDisplayStorage[1].Substring(109, MultipleLinesDisplayStorage[1].Length - 109).Split(new string[] { " ", "TRC-" },
                                                                                StringSplitOptions.RemoveEmptyEntries);
                    Current.LNIATA = temp[0];
                    if (temp.Length > 1)
                    {
                        Current.Trace = temp[1];
                    }
                    else
                    {
                        Current.Trace = "";
                    }

                    //Third line
                    temp = MultipleLinesDisplayStorage[2].Substring(109, MultipleLinesDisplayStorage[2].Length - 109).Split(new string[] { " ", "OBJ-" },
                                                                                StringSplitOptions.RemoveEmptyEntries);
                    Current.Section = temp[0];
                    if (temp.Length > 2)
                    {
                        Current.ObjectName = temp[1];
                        Current.LoadSet = temp[2];
                    }
                    else
                    {
                        Current.LoadSet = temp[1];
                        Current.ObjectName = "";
                    }

                    Current.CPU = processName;

                    //Fourth Line
                    if (MultipleLinesDisplayStorage.Count > 3)
                    {
                        Current.message = MultipleLinesDisplayStorage[3].Substring(109, MultipleLinesDisplayStorage[3].Length - 109);
                    }
                    else
                    {
                        Current.message = "";
                    }

                    if (int.TryParse(Sequence, out seq))
                    {
                        Current.SequenceNo = seq;
                        Current.FirstOne = Current.LastOne;
                        string VSNo = DumpVolSer.Find(x => x.Contains(seq.ToString() + ","));
                        if (VSNo != null)
                        {
                            Current.VSN = VSNo.Split(',')[1];
                            DumpVolSer.Remove(VSNo);
                        }
                        Dumplist.Add(Current);
                    }
                    else
                    {
                        LogDump similarDump = Dumplist.Find(x => x.DumpType == Current.DumpType &&
                                               x.ObjectName == Current.ObjectName &&
                                               x.Trace == Current.Trace &&
                                               x.SubSystem == Current.SubSystem &&
                                               x.SubSystemUser == Current.SubSystemUser);
                        if (similarDump == null)
                        {
                            Current.FirstOne = Current.LastOne;
                            Current.SequenceNo = -1;
                            Dumplist.Add(Current);
                        }
                        else
                        {
                            similarDump.LastOne = Current.LastOne;
                            similarDump.Count++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AdminDB.LogIt(ex.Message, ex.StackTrace, Counter, processName);
            }
        }

        /// <summary>
        /// Amin: to list a SNAP dump
        /// </summary>
        /// <param name="Lines"></param>
        private void SnapDumpLister()
        {
            try
            {
                if (MultipleLinesDisplayStorage.Count >= 3 && MultipleLinesDisplayStorage[0].Contains("SSU"))
                {
                    LogSnapDump Current = new LogSnapDump();
                    Current.Count = 1;
                    //first line
                    string[] temp = MultipleLinesDisplayStorage[0].Substring(109, MultipleLinesDisplayStorage[0].Length - 109).Split(new string[] { " ", "IS-", "SS-", "SSU-", "." },
                                                                                         StringSplitOptions.RemoveEmptyEntries);
                    Current.LastOne = new DateTime(DateOfLog.Year, DateOfLog.Month, DateOfLog.Day,
                                                    int.Parse(temp[1].ToString()), int.Parse(temp[2].ToString()), int.Parse(temp[3].ToString()));
                    Current.IStream = int.Parse(temp[7].ToString());
                    Current.SubSystem = temp[5];
                    Current.SubSystemUser = temp[6];

                    //Second line
                    temp = MultipleLinesDisplayStorage[1].Substring(109, MultipleLinesDisplayStorage[1].Length - 109).Split(new string[] { "PSW-" },
                                                                                StringSplitOptions.RemoveEmptyEntries);
                    Current.ProgramStatusWord = temp[0];

                    //Third line
                    //temp = MultipleLinesDisplayStorage[2].Substring(109, MultipleLinesDisplayStorage[2].Length - 109).Split(new string[] { " ", "PGM-", "CODE-", "TERM-" },
                    //                                                            StringSplitOptions.RemoveEmptyEntries);
                    temp = MultipleLinesDisplayStorage[2].Substring(109, MultipleLinesDisplayStorage[2].Length - 109).Split(new string[] { " "}, StringSplitOptions.RemoveEmptyEntries);
                    if (temp[0].Length > 4)
                    {
                        Current.Program = temp[0].Split('-')[1];                    
                    }
                    if (temp[1].Length > 5)
                    {
                        Current.Code = temp[1].Split('-')[1];
                    }
                    if (temp[2].Length > 5)
                    {
                        Current.Term = temp[2].Split('-')[1];
                    }
                    Current.CPU = processName;

                    //Fourth Line
                    if (MultipleLinesDisplayStorage.Count > 3)
                    {
                        Current.message = MultipleLinesDisplayStorage[3].Substring(109, MultipleLinesDisplayStorage[3].Length - 109);
                    }
                    else
                    {
                        Current.message = "";
                    }

                    LogSnapDump similarDump = SnapDumplist.Find(x => x.Program == Current.Program &&
                                           x.ProgramStatusWord == Current.ProgramStatusWord &&
                                           x.Term == Current.Term &&
                                           x.Code == Current.Code &&
                                           x.message == Current.message &&
                                           x.SubSystem == Current.SubSystem &&
                                           x.SubSystemUser == Current.SubSystemUser);
                    if (similarDump == null)
                    {
                        Current.FirstOne = Current.LastOne;
                        SnapDumplist.Add(Current);
                    }
                    else
                    {
                        similarDump.LastOne = Current.LastOne;
                        similarDump.Count++;
                    }
                }
            }
            catch (Exception ex)
            {
                AdminDB.LogIt(ex.Message, ex.StackTrace, Counter, processName);
            }
        }

        /// <summary>
        /// Amin: Display Logged Dumps
        /// </summary>
        /// <param name="stCommType"></param>
        /// <param name="flg"></param>
        /// <returns></returns>
        public List<LogDump> DispDumpList(string ProgramName, string type, string partition)
        {
            string PN = ProgramName.ToUpper(), T = type.ToUpper(), P = partition.ToUpper();
            List<LogDump> Result = Dumplist.FindAll(y => (y.Section.ToUpper().Contains(PN) || y.Trace.ToUpper().Contains(PN) || y.ObjectName.ToUpper().Contains(PN))
                && y.SubSystemUser.ToUpper().Contains(P) && y.DumpType.ToUpper().Contains(T));
            if (Result.Count > 0)
            {
                Result = Result.OrderBy(x => -x.Count).ToList();
            }
            return Result;
        }

        /// <summary>
        /// Amin: Display Logged SnapDumps
        /// </summary>
        /// <param name="stCommType"></param>
        /// <param name="flg"></param>
        /// <returns></returns>
        public List<LogSnapDump> DispSnapDumpList(string ProgramName, string type, string partition)
        {
            string PN = ProgramName.ToUpper(), T = type.ToUpper(), P = partition.ToUpper();
            List<LogSnapDump> Result = SnapDumplist.FindAll(y => y.Program.ToUpper().Contains(PN)
                && y.SubSystemUser.ToUpper().Contains(P) && y.Code.ToUpper().Contains(T));
            if (Result.Count > 0)
            {
                Result = Result.OrderBy(x => -x.Count).ToList();
            }
            return Result;
        }

        /// <summary>
        /// Nafise: Insert Commands with time to DB
        /// </summary>
        public void AddDBCommands(string sys_Name)
        {
            //List<LogCommandDB> TempCommandList = new List<LogCommandDB>();
            //LogCommandDB logComDB = new LogCommandDB();
            //// finding Ran commands in all Commands TYPE
            //TempCommandList = ShowDBCommandList("", 0, true, sys_Name);

            ////Adding found commands with Times to DB
            //logComDB.AddLogCommandsList(TempCommandList);
        }

        /// <summary>
        /// Nafise: Insert Response with time to DB
        /// </summary>
        public void AddDBResponses(string sys_Name)
        {
            //List<LogResponseDB> TempResponseList = new List<LogResponseDB>();
            //LogResponseDB logResDB = new LogResponseDB();
            //// finding Ran commands in all Commands TYPE
            //TempResponseList = ShowResponseDBList("", 0, true, sys_Name);

            ////Adding found Response with Times to DB
            //logResDB.AddLogResponseList(TempResponseList);

        }

        /// <summary>
        /// Nafise: Search for the Response line in DBcommandList, then update DBCommandList based on the Line time
        /// </summary>
        /// <param name="stline"></param>
        private void ResponseFinder(DateTime timeLine, string stline, string WTOPC_Code)
        {
            stline = stline.Trim();
            try
            {
                List<LogResponseDB> UDBResponseList;
                List<LogCommandDB> UDBCommandList;
                List<LogSingleTapeDB> UDBFinalTapeList;
                switch (sysName)
                {
                    case "WNP":
                        UDBResponseList = _DBResponseListWNP;
                        UDBCommandList = _DBCommandListWNP;
                        UDBFinalTapeList = DBFinalTapeListWNP;
                        break;
                    case "FCA":
                        UDBResponseList = _DBResponseListFCA;
                        UDBCommandList = _DBCommandListFCA;
                        UDBFinalTapeList = DBFinalTapeListFCA;
                        break;
                    case "FOS":
                        UDBResponseList = _DBResponseListFOS;
                        UDBCommandList = _DBCommandListFOS;
                        UDBFinalTapeList = DBFinalTapeListFOS;
                        break;
                    case "PSS":
                        UDBResponseList = _DBResponseListPSS;
                        UDBCommandList = _DBCommandListPSS;
                        UDBFinalTapeList = DBFinalTapeListPSS;
                        break;
                    default:
                        return;
                }


                // LogResponseDB findRes = null;
                LogResponseDB findResponseRepeat = null;
                LogResponseDB findResponseRepeat1 = null;
                LogCommandDB findResponseNoCommand = null;
                LogResponseDB eb = null;
                string stMsg = "", stMsg2 = "";
                int intFstNum;
                bool nonNumber = false, WNumber = false;

                // MSG
                if (stline.Length > 19) // this format: SWLI0001I 02.30.10 xxxxxxxxxxxxx
                {
                    if (stline.Substring(12, 1) == ".")
                    {
                        stMsg = stline.Substring(19);
                    }
                    else if (stline.Substring(12, 1) == "/")
                    {
                        stMsg = stline.Substring(28);
                    }
                    else
                    {
                        stMsg = stline.Trim();
                    }

                }
                else// this format: xxxxxxxxxxxxxxxxx
                {
                    if (stline.Length > 1)
                    {
                        stMsg = stline.Substring(1, stline.Length - 1);
                    }
                }

                if (stMsg != "")
                {
                    if (stMsg.Length > 20)
                    {
                        //if ( stMsg.Length > 5)
                        {
                            stMsg2 = stMsg.Substring(1, stMsg.Length - 5);
                        }

                    }
                    else
                    {
                        if (stMsg.Length > 2)
                        {
                            stMsg2 = stMsg.Substring(1, stMsg.Length - 2);
                        }
                    }

                    var findResponse = UDBResponseList.Where(d => (d.WTOPC_Response == WTOPC_Code)
                                      && (d.RsponseTimeDB == DateTime.MinValue)
                                      && (d.Partition == Thread_partitionName)
                                      && (d.ResponseDB.Contains(stMsg2))
                                      && (d.DesiredCPU == processName)
                                      && (d.Process == null));
                    if (findResponse.Count() == 0)
                    {
                        findResponse = UDBResponseList.Where(d => (d.WTOPC_Response == WTOPC_Code)
                                      && (d.RsponseTimeDB == DateTime.MinValue)
                                      && (d.Partition == Thread_partitionName)
                                      //&& (d.DesiredCPU == processName)
                                      && (d.Process == null));
                    }

                    if (findResponse.Count() >= 1)
                    {
                        var findResponseSTG = findResponse.Where(d => (d.ResponseDB.Contains(stMsg2)));
                        long MinScheduleID;

                        if (findResponseSTG.Count() >=1 )
                        {
                             MinScheduleID = findResponseSTG.Min(z => z.ScheduleID);
                        }
                        else
                        {
                             MinScheduleID = findResponse.Min(z => z.ScheduleID);
                        }
                        //long MinScheduleID = findResponse.Where(z => z.ScheduleID && z.ResponseDB.Contains(stMsg2)); 

                        foreach (var ee in findResponse)
                        {
                            if (ee.ResponseDB.Contains(stMsg2) && (ee.ScheduleID == MinScheduleID) && (stMsg2.Length > 2))
                            {
                                eb = ee;
                                break;
                            }
                        }
                        if (eb == null)
                        {
                            intFstNum = stMsg2.IndexOf(Regex.Match(stMsg2, @"\d{2,}|\s\d\s").Value);
                            if (intFstNum != 0)
                            {
                                stMsg2 = stMsg2.Substring(1, intFstNum - 1);
                                if (stMsg2.Length > 3)
                                {

                                    foreach (var ee in findResponse)
                                    {
                                        if (ee.ResponseDB.Contains(stMsg2))
                                        {
                                            WNumber = true;
                                            eb = ee;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            nonNumber = true;
                        }
                        if (nonNumber || WNumber)
                        {
                            // check to find for double responses
                            findResponseRepeat = UDBResponseList.Where(d => (d.WTOPC_Response == WTOPC_Code)
                                                    && (d.ResponseDB == eb.ResponseDB)
                                                    && (d.RsponseTimeDB != DateTime.MinValue)
                                                    && (d.Partition == Thread_partitionName)
                                                    && (d.Process == processName)
                                                    && (d.RsponseTimeDB.AddMinutes(+30) > timeLine)
                                                    && (d.RsponseTimeDB <= timeLine)).FirstOrDefault();

                            if (findResponseRepeat != null)
                            {
                                findResponseRepeat1 = UDBResponseList.Where(d => (d.WTOPC_Response == WTOPC_Code)
                                                    && (d.ResponseDB == eb.ResponseDB)
                                                    && (d.ScheduleID == eb.ScheduleID)
                                                    && (d.Partition == eb.Partition)
                                                    && (d.RsponseTimeDB == DateTime.MinValue) 
                                                    && (d.Process == null)).FirstOrDefault();
                            }

                            if (findResponseRepeat == null || findResponseRepeat1 != null)
                            {

                                var findCommand = UDBCommandList.Where(d => ((d.TypeDB == "START" && d.CommandTimeDB != DateTime.MinValue && d.Process != null)
                                                                            || d.TypeDB == "TIME INITIATED")
                                                                            && (d.DesiredCPU == processName)
                                                                            && (d.ScheduleID == eb.ScheduleID)).FirstOrDefault();
                                if (findCommand == null)
                                {
                                    findCommand = UDBCommandList.Where(d => ((d.TypeDB == "START" && d.CommandTimeDB != DateTime.MinValue && d.Process != null)
                                                                            || d.TypeDB == "TIME INITIATED")
                                                                            //&& (d.DesiredCPU == processName)
                                                                            && (d.ScheduleID == eb.ScheduleID)).FirstOrDefault();
                                }

                                if (findCommand == null && eb.TypeDB == "START")
                                {

                                    var findCommand1 = UDBResponseList.Where(d => (d.WTOPC_Response == WTOPC_Code)
                                                             && (d.ResponseDB == eb.ResponseDB)
                                                             && (d.ScheduleID == eb.ScheduleID)
                                                             && (d.Partition == eb.Partition)
                                                             && (d.RsponseTimeDB == DateTime.MinValue) && (d.Process == null)).FirstOrDefault();
                                    if (findCommand1 != null)
                                    {
                                        findResponseNoCommand = UDBCommandList.Where(d =>(d.CommandID == findCommand1.CommandID)).FirstOrDefault();
                                    }
                                }

                                if (findCommand != null || findResponseNoCommand == null)
                                {
                                    eb.RsponseTimeDB = timeLine;
                                    eb.Process = processName;

                                    if (AdminAutoUpdate && UserAutoStartTapeUpdate && (eb.TypeDB == "START" || eb.TypeDB == "TIME INITIATED"))
                                    {
                                        string strStartTime = eb.RsponseTimeDB.ToString("HHmm").Substring(0, 2) + ":" + eb.RsponseTimeDB.ToString("HHmm").Substring(2, 2) + ":00";

                                        ScheduleJob schJobCheckUpdate = new ScheduleJob();
                                        schJobCheckUpdate.Schedule_ID = eb.ScheduleID;
                                        schJobCheckUpdate.Job_Start_Time = eb.RsponseTimeDB.Date.ToString("yyyy-MM-dd") + " " + strStartTime;
                                        schJobCheckUpdate.CPU_ID = Thread_ProcessID;
                                        schJobCheckUpdate.CPU_Name = processName;
                                        schJobCheckUpdate.System_ID = Thread_sysID;
                                        schJobCheckUpdate.System_Name = sysName;
                                        schJobCheckUpdate.Job_Status = "STARTED";
                                        if (OpsUserName == null || OpsUserName.Length < 8) // to prevent empty USER for started jobs
                                        {
                                            OpsUserName = (new AdminDB()).GetLastAutoPilotUser(sysName,1);
                                        }
                                        schJobCheckUpdate.Operator_Start = OpsUserName;

                                        AttachRelatedMountedTapes(eb.ScheduleID);
                                        if (schJobCheckUpdate.Operator_Start.Length < 8)
                                        {
                                            UserAutoStartTapeUpdate = false;
                                        }
                                        else
                                        {
                                            // set date of schedule for predecessor
                                            schJobCheckUpdate.Schedule_Date = DateOfLog.ToString("yyyy-MM-dd");
                                            schJobCheckUpdate.AutoStartResponseUpdate();
                                        }
                                    }
                                    else if (AdminAutoUpdate && UserAutoEndUpdate
                                        && (eb.TypeDB == "END"))
                                    {
                                        EndJobCheck(eb.ScheduleID, "END Response");
                                    }
                                }
                            }
                            else
                            {
                                //double responses happend because of double wrong command
                                AdminDB.LogIt(sysName + ", " + processName + ", " + Thread_partitionName
                                    + ", double Response: " + findResponseRepeat.WTOPC_Response + ", " + findResponseRepeat.ResponseDB + " (" + timeLine.ToString("HH:mm:ss") + ")");
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AdminDB.LogIt(ex.Message, ex.StackTrace, Counter, processName);
            }
        }
       
        /// <summary>
        /// Amin: flush all tapes to file
        /// </summary>
        public void WriteTapeList()
        {
            List<LogSingleTapeDB> UDBFinalTapeList;
            switch (sysName)
            {
                case "WNP":
                    UDBFinalTapeList = DBFinalTapeListWNP;
                    break;
                case "FCA":
                    UDBFinalTapeList = DBFinalTapeListFCA;
                    break;
                case "FOS":
                    UDBFinalTapeList = DBFinalTapeListFOS;
                    break;
                case "PSS":
                    UDBFinalTapeList = DBFinalTapeListPSS;
                    break;
                default:
                    return;
            }
            AdminDB worker = new AdminDB();
            if (UDBFinalTapeList.Count > 0)
            {
                AdminDB.LogIt("Tape list of " + sysName + " (Name,volser,ID,ScheduleID,CPU,Devise,Partition,IsInput,OS390,Mounted@,Done@,Submitted@)");
            }
            for (int i = 0; i < UDBFinalTapeList.Count; i++)
            {
                AdminDB.LogIt(UDBFinalTapeList[i].TapeName + " - " +
                    UDBFinalTapeList[i].volser + " - " +
                    UDBFinalTapeList[i].TapeID.ToString("00000000") + " - " +
                    UDBFinalTapeList[i].ScheduleID.ToString("00000000") + " - " +
                    UDBFinalTapeList[i].CPU + " - " +
                    UDBFinalTapeList[i].Device + " - " +
                    UDBFinalTapeList[i].Partition + " - " +
                    (UDBFinalTapeList[i].IsInput ? "Input" : "Output") + " - " +
                    UDBFinalTapeList[i].OS390JobName
                    + " - M:" +
                    (UDBFinalTapeList[i].TapeMountTimeDB == DateTime.MinValue ? "" : UDBFinalTapeList[i].TapeMountTimeDB.ToLongTimeString())
                    + " - D:" +
                    (UDBFinalTapeList[i].TapeRemoveTimeDB == DateTime.MinValue ? "" : UDBFinalTapeList[i].TapeRemoveTimeDB.ToLongTimeString())
                    + " - S:" +
                    (UDBFinalTapeList[i].TapeSubmitTimeDB == DateTime.MinValue ? "" : UDBFinalTapeList[i].TapeSubmitTimeDB.ToLongTimeString())
                    );
            }
        }
    
        /// <summary>
        /// Amin: clean the static lists of threads
        /// </summary>
        public void clearThreadLists()
        {
            _Dumplist.Clear();
            _SnapDumplist.Clear();

            switch (sysName)
            {
                case "WNP":
                    DBTapeListCountWNP.Clear();
                    DBTapeListMultiplesWNP.Clear();
                    _DBFinalTapeListWNP.Clear();

                    _DBCommandListWNP.Clear();
                    _DBResponseListWNP.Clear();
                    _DBWTOPC_ResponseListWNP.Clear();
                    break;
                case "FCA":
                    DBTapeListCountFCA.Clear();
                    DBTapeListMultiplesFCA.Clear();
                    _DBFinalTapeListFCA.Clear();

                    _DBCommandListFCA.Clear();
                    _DBResponseListFCA.Clear();
                    _DBWTOPC_ResponseListFCA.Clear();
                    break;
                case "FOS":
                    DBTapeListCountFOS.Clear();
                    DBTapeListMultiplesFOS.Clear();
                    _DBFinalTapeListFOS.Clear();

                    _DBCommandListFOS.Clear();
                    _DBResponseListFOS.Clear();
                    _DBWTOPC_ResponseListFOS.Clear();
                    break;
                case "PSS":
                    DBTapeListCountPSS.Clear();
                    DBTapeListMultiplesPSS.Clear();
                    _DBFinalTapeListPSS.Clear();

                    _DBCommandListPSS.Clear();
                    _DBResponseListPSS.Clear();
                    _DBWTOPC_ResponseListPSS.Clear();
                    break;
            }
            
        }

        /// <summary>
        /// Amin: to collect system utility informatopn from log
        /// </summary>
        /// <param name="Inputs"></param> 
        private void SystemUtilityDiagram()
        {
            try
            {
                SystemUtilityDiagrams LastRead = new SystemUtilityDiagrams(MultipleLinesDisplayStorage.Count - 4);
                LastRead.readTime = MultipleLinesDisplayStorage[0].Substring(11, 2) + ":" + MultipleLinesDisplayStorage[0].Substring(14, 2);
                for (int i = 3; i < MultipleLinesDisplayStorage.Count - 1; i++)
                {
                    string[] temp = MultipleLinesDisplayStorage[i].Substring(109, MultipleLinesDisplayStorage[i].Length - 109).Split(new string[] { " ", "/", "-" }, StringSplitOptions.RemoveEmptyEntries);
                    LastRead.IStreams[i - 3].IStreamNumber = int.Parse(temp[1]);
                    LastRead.IStreams[i - 3].Utility = float.Parse(temp[3]);
                    LastRead.IStreams[i - 3].AdjustUtility = float.Parse(temp[4]);
                    LastRead.IStreams[i - 3].CrosslistSize = int.Parse(temp[5]);
                    LastRead.IStreams[i - 3].ReadyListSize = int.Parse(temp[6]);
                    LastRead.IStreams[i - 3].InputListSize = int.Parse(temp[7]);
                    LastRead.IStreams[i - 3].virtualFileAccessListSize = int.Parse(temp[8]);
                    LastRead.IStreams[i - 3].SuspendListSize = int.Parse(temp[9]);
                    LastRead.IStreams[i - 3].DeferedListSize = int.Parse(temp[10]);
                    LastRead.IStreams[i - 3].ActiveECBNumber = int.Parse(temp[11]);
                }
                UtilityData.Add(LastRead);
                if (UtilityData.Count > 1440)
                {
                    UtilityData.RemoveAt(0);
                }
            }
            catch (Exception ex)
            {
                AdminDB.LogIt(ex.Message, ex.StackTrace, Counter, processName);
            }
        }
        
        /// <summary>
        /// Amin: to collect system status informatopn from log
        /// </summary>
        /// <param name="Inputs"></param>
        private void SystemStatusDiagram()
        {
            if (!(MultipleLinesDisplayStorage.Count >= 16 && MultipleLinesDisplayStorage[2].Contains("ALLOCATED") && MultipleLinesDisplayStorage[3].Contains("AVAILABLE") 
                && MultipleLinesDisplayStorage[4].Contains("ACTIVE ECBS") && MultipleLinesDisplayStorage[5].Contains("DLY/DFR ECBS")
                && MultipleLinesDisplayStorage[6].Contains("PROCESSED") && MultipleLinesDisplayStorage[7].Contains("LOW SPEED") 
                && MultipleLinesDisplayStorage[8].Contains("ROUTED") && MultipleLinesDisplayStorage[9].Contains("CREATED") && MultipleLinesDisplayStorage[10].Contains("SDO CREATED")
                && MultipleLinesDisplayStorage[11].Contains("SNA") && MultipleLinesDisplayStorage[12].Contains("THREADS CREATED") 
                && MultipleLinesDisplayStorage[13].Contains("TCP/IP INPUT") && MultipleLinesDisplayStorage[14].Contains("TCP/IP OUTPUT")))
                return;
            SystemStatusUnits LastRead = new SystemStatusUnits();
            LastRead.readTime = MultipleLinesDisplayStorage[0].Substring(11, 2) + ":" + MultipleLinesDisplayStorage[0].Substring(14, 2);

            string[] temp = MultipleLinesDisplayStorage[2].Substring(109, MultipleLinesDisplayStorage[2].Length - 109).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            LastRead.AllocatedIOBlock = int.Parse(temp[1]);
            LastRead.AllocatedFrame = int.Parse(temp[2]);
            LastRead.AllocatedCommon = int.Parse(temp[3]);
            LastRead.AllocatedSWB = int.Parse(temp[4]);
            LastRead.AllocatedECB = int.Parse(temp[5]);
            LastRead.AllocatedFrame1M = int.Parse(temp[6]);

            temp = MultipleLinesDisplayStorage[3].Substring(109, MultipleLinesDisplayStorage[3].Length - 109).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            LastRead.AvilableIOBlock = int.Parse(temp[1]);
            LastRead.AvilableFrame = int.Parse(temp[2]);
            LastRead.AvilableCommon = int.Parse(temp[3]);
            LastRead.AvilableSWB = int.Parse(temp[4]);
            LastRead.AvilableECB = int.Parse(temp[5]);
            LastRead.AvilableFrame1M = int.Parse(temp[6]);

            LastRead.ActiveECBs = int.Parse(MultipleLinesDisplayStorage[4].Substring(109, MultipleLinesDisplayStorage[4].Length - 109).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[2]);
            LastRead.DelayedDeffered = int.Parse(MultipleLinesDisplayStorage[5].Substring(109, MultipleLinesDisplayStorage[5].Length - 109).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[2]);
            LastRead.Processed = int.Parse(MultipleLinesDisplayStorage[6].Substring(109, MultipleLinesDisplayStorage[6].Length - 109).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[1]);
            LastRead.LowSpeed = int.Parse(MultipleLinesDisplayStorage[7].Substring(109, MultipleLinesDisplayStorage[7].Length - 109).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[2]);
            LastRead.Routed = int.Parse(MultipleLinesDisplayStorage[8].Substring(109, MultipleLinesDisplayStorage[8].Length - 109).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[1]);
            LastRead.Created = double.Parse(MultipleLinesDisplayStorage[9].Substring(109, MultipleLinesDisplayStorage[9].Length - 109).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[1]);
            LastRead.SDOCreated = int.Parse(MultipleLinesDisplayStorage[10].Substring(109, MultipleLinesDisplayStorage[10].Length - 109).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[2]);
            LastRead.SNA = int.Parse(MultipleLinesDisplayStorage[11].Substring(109, MultipleLinesDisplayStorage[11].Length - 109).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[1]);
            LastRead.ThreadsCreated = int.Parse(MultipleLinesDisplayStorage[12].Substring(109, MultipleLinesDisplayStorage[12].Length - 109).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[2]);
            string[] temp1 = MultipleLinesDisplayStorage[13].Substring(109, MultipleLinesDisplayStorage[13].Length - 109).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string number = "";
            for (int i = 2; i < temp1.Length; i++)
            {
                if (temp1[i] != "_")
                {
                    number += temp1[i];
                }
            }
            LastRead.TCPIPInput = double.Parse(number);

            string[] temp2 = MultipleLinesDisplayStorage[14].Substring(109, MultipleLinesDisplayStorage[14].Length - 109).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            number = "";
            for (int i = 2; i < temp2.Length; i++)
            {
                if (temp2[i] != "_")
                {
                    number += temp2[i];
                }
            }
            LastRead.TCPIPOutput = double.Parse(number);
            
            StatusData.Add(LastRead);
            if (StatusData.Count > 1440)
            {
                StatusData.RemoveAt(0);
            }
        }

        /// <summary>
        /// Nafise: add user to the system for auto updating start/tape log reader
        /// </summary>
        /// <param name="opsUsrNm"></param>
        public bool UpdateOpsSystem(string opsUsrNm, bool userFlg)
        {
            if (AdminAutoUpdate)
            {
                if (opsUsrNm == null || !userFlg)
                {
                    OpsUserName = null;
                    UserAutoStartTapeUpdate = false;
                    UserAutoEndUpdate = false;
                    (new AdminDB()).AutoPilotClear(sysName);
                }
                else
                {
                    OpsUserName = opsUsrNm;
                    UserAutoStartTapeUpdate = userFlg;
                    UserAutoEndUpdate = false;                    
                    (new AdminDB()).AutoPilotSet(sysName, opsUsrNm, 1);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Amin: add user to the system for auto end updating log reader
        /// </summary>
        /// <param name="opsUsrNm"></param>
        public bool UpdateOpsEnd(string opsUsrNm, bool userFlg)
        {
            if (AdminAutoUpdate)
            {
                if (opsUsrNm == null)
                {
                    OpsUserName = null;
                    (new AdminDB()).AutoPilotClear(sysName);
                }
                else if (!userFlg )
                {
                    OpsUserName = opsUsrNm;
                    UserAutoEndUpdate = false;
                    if (UserAutoStartTapeUpdate)
                    {
                        (new AdminDB()).AutoPilotSet(sysName, opsUsrNm, 1);
                    }
                    else
                    {
                        (new AdminDB()).AutoPilotClear(sysName);
                    }
                }
                else
                {
                    OpsUserName = opsUsrNm;
                    UserAutoEndUpdate = userFlg;
                    (new AdminDB()).AutoPilotSet(sysName, opsUsrNm, 2);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Nafise: reading the user name in the system who asked for auto updating log reader
        /// </summary>
        /// <returns></returns>
        public string ReadOpsSystem()
        {
            return OpsUserName;
        }

        /// <summary>
        /// Amin: reading the autoPilot Level
        /// </summary>
        /// <returns></returns>
        public byte ReadOpsAutoPilot()
        {
            if (UserAutoStartTapeUpdate == true && UserAutoEndUpdate == true)
            { 
                return 2; 
            }
            if (UserAutoStartTapeUpdate == true && UserAutoEndUpdate == false)
            {
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Nafise: Stop or start auto updating log reader
        /// </summary>
        /// <param name="flg"></param>
        /// <returns></returns>
        public bool UpdateAutoOpsSystemForAdmin(bool flg)
        {
            AdminAutoUpdate = flg;
            return AdminAutoUpdate;
        }

        private void AttachRelatedMountedTapes(long ID)
        {
            List<LogSingleTapeDB> UDBFinalTapeList;
            switch (sysName)
            {
                case "WNP":
                    UDBFinalTapeList = DBFinalTapeListWNP;
                    break;
                case "FCA":
                    UDBFinalTapeList = DBFinalTapeListFCA;
                    break;
                case "FOS":
                    UDBFinalTapeList = DBFinalTapeListFOS;
                    break;
                case "PSS":
                    UDBFinalTapeList = DBFinalTapeListPSS;
                    break;
                default:
                    return;
            } 
            LogSingleTapeDB relatedTape = new LogSingleTapeDB();
            DataTable tape_list = relatedTape.TapelistByScheduleID(ID);
            for (int i = 0; i < tape_list.Rows.Count; i++)
            {
                relatedTape = UDBFinalTapeList.Find(x => 
                       x.ScheduleID == 0 && x.TapeID == 0
                    && x.CPU == tape_list.Rows[i]["CPU_NAME"].ToString()
                    && x.IsInput == (tape_list.Rows[i]["INPUT"].ToString() == "0" ? false : true)
                    && x.TapeName == tape_list.Rows[i]["Name"].ToString()
                    );
                if (relatedTape != null)
                {
                    relatedTape.TapeID = long.Parse(tape_list.Rows[i]["TAPE_ID"].ToString());
                    relatedTape.ScheduleID = ID;
                    relatedTape.AddTapetoDB();
                }
            }
                                        
        }
        
        ///<summary>
        /// Amin: To Update calendars from log
        ///</summary>
        private void TosAutomation_CalendarUpdate(string input)
        {
            try
            {
                string[] temp2 = input.Substring(109, input.Length - 109).Split(new string[] { " ", "OF", "TOSESP" }, StringSplitOptions.RemoveEmptyEntries);
                bool LastDay = (temp2[5] == temp2[6]);
                JobCalendar NewCalendar = new JobCalendar(), tempCal = null;
                if (temp2[4] == "ESPCAL")
                {
                    NewCalendar.CalendarName = temp2[7];
                }
                else if (temp2[4] == "ESPHOL")
                {
                    NewCalendar.CalendarName = "Holiday";
                }
                NewCalendar.DaysofCalendar.Add(new DateTime(int.Parse(temp2[8].Substring(0, 4)),
                                                            int.Parse(temp2[8].Substring(4, 2)),
                                                            int.Parse(temp2[8].Substring(6, 2))));
                if (NewCalendar.UpdateOneDayofCalendar())
                {
                    addNote(processName + ": " + NewCalendar.DaysofCalendar[0].ToString("MM/dd/yyyy")
                                + " added to Calendar(" + NewCalendar.CalendarName + ") From Log", "Auto-Msg", false);
                }

                tempCal = _LogCalendars.Find(x => x.CalendarName == NewCalendar.CalendarName);
                if (tempCal == null)
                {
                    _LogCalendars.Add(NewCalendar);
                }
                else
                {
                    if (tempCal.DaysofCalendar.Find(x => x == NewCalendar.DaysofCalendar[0]) == DateTime.MinValue)
                    {
                        tempCal.DaysofCalendar.Add(NewCalendar.DaysofCalendar[0]);
                    }
                    if (LastDay)
                    {
                        string DD = tempCal.CheckDeletedDaysofCalendar();
                        if (DD.Length > 1)
                        {
                            addNote(processName + ": {" + DD + "} of " + tempCal.CalendarName + " Calendar did not receive on log of " 
                                + DateOfLog.ToString("MM/dd/yyyy") + "!", "Auto-Msg", false);
                            addNote(processName + ": {" + DD + "} of " + tempCal.CalendarName + " Calendar did not receive on log of "
                                + DateOfLog.ToString("MM/dd/yyyy") + "!", "Scheduler", false);
                            addNote(processName + ": {" + DD + "} of " + tempCal.CalendarName + " Calendar did not receive on log of "
                                + DateOfLog.ToString("MM/dd/yyyy") + "!", "Admin", false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AdminDB.LogIt(ex.Message, ex.StackTrace, Counter, processName);
            }
        }

       

        ///<summary>
        /// Amin: To Update RPO Script status
        ///</summary>
        private void TosAutomation_RPO_Update(string input)
        {
            try
            {   string[] tempStr = input.Substring(109, input.Length - 109).Split(new string[] { ":", ",", " " }, StringSplitOptions.RemoveEmptyEntries);
                RPO_Step = tempStr[5];
                if (RPO_Step == "6.1")
                {
                    RPO_LastRunTime = LastUpdate;
                }
            }
            catch (Exception ex)
            {
                AdminDB.LogIt(ex.Message, ex.StackTrace, Counter, processName);
            }
        }        

        /// <summary>
        /// Nafise: Updating PDU Release (ADDRESSES RETURNED)
        /// </summary>
        /// <param name="input"></param>
        private void ReleaseUpdate(string line, string releaseType)
        {
            string pool_Type = "";
            long pool_Count = 0;
            long partition_ID = 0;
            if (line.Contains("DEVA"))
            {
                pool_Type = line.Substring(109, 4).Trim();
                if (line.Substring(122).Trim() != "")
                {
                    long.TryParse(line.Substring(122, 25).Trim().Replace(" ", ""), out pool_Count);
                }

                string time = line.Substring(11, 2) + ":" + line.Substring(14, 2) + ":" + line.Substring(17, 2);
                string date = line.Substring(0, 10);
                LastUpdate = date + " " + time;

                partition_ID = (new AdminDB()).GetPartitionID(Thread_partitionName, Thread_sysID);
                if (partition_ID != 0)
                {
                    (new AdminDB()).AddPDURelease(pool_Type, pool_Count, DateTime.Parse(LastUpdate), partition_ID, releaseType);
                }
            }
        }
        
        /// <summary>
        /// Amin: Updating PDU Abort
        /// </summary>
        /// <param name="input"></param>
        private void PDU_Abort(string line)
        {
            string time = line.Substring(11, 2) + ":" + line.Substring(14, 2) + ":" + line.Substring(17, 2);
            string date = line.Substring(0, 10);
            LastUpdate = date + " " + time;
            long partition_ID = (new AdminDB()).GetPartitionID(Thread_partitionName, Thread_sysID);
            if (partition_ID != 0)
            {
                (new AdminDB()).AddPDURelease("SDP", -1, DateTime.Parse(LastUpdate), partition_ID, "ABO");
                (new AdminDB()).AddPDURelease("LDP", -1, DateTime.Parse(LastUpdate), partition_ID, "ABO");
                (new AdminDB()).AddPDURelease("4DP", -1, DateTime.Parse(LastUpdate), partition_ID, "ABO");
                (new AdminDB()).AddPDURelease("4D6", -1, DateTime.Parse(LastUpdate), partition_ID, "ABO");
            }
        }

        ///// <summary>
        ///// Nafise: updating MultipleRelease
        ///// </summary>
        ///// <param name="line"></param>
        //private void MultipleReleaseUpdate(string line)
        //{
        //    string pool_Type = "";
        //    long pool_Count = 0;
        //    if (line.Contains("DEVA"))
        //    {
        //        pool_Type = line.Substring(109, 4).Trim();
        //        if (line.Substring(122).Trim() != "")
        //        {
        //            long.TryParse(line.Substring(122, 25).Trim().Replace(" ", ""), out pool_Count);
        //        }

        //        string time = line.Substring(11, 2) + ":" + line.Substring(14, 2) + ":" + line.Substring(17, 2);
        //        string date = line.Substring(0, 10);
        //        LastUpdate = date + " " + time;

        //        partition_ID = (new AdminDB()).GetPartitionID(Thread_partitionName, Thread_sysID);
        //        if (partition_ID != 0)
        //        {
        //            (new AdminDB()).AddPDURelease(pool_Type, pool_Count, DateTime.Parse(LastUpdate), partition_ID, "MUL");
        //        }
        //    }
        //}

        private void ExtractPartitionName(string line)
        {
            try
            {
                if (line.Length > 147)
                {
                    Thread_partitionName = line.Substring(146, 2).Trim();
                    Thread_processorName_fromLOG = (sysName == "PSS" ? "RES" : sysName) + line.Substring(132, 1);
                }
            }
            catch (Exception ex)
            {
                AdminDB.LogIt(ex.Message, ex.StackTrace, Counter, processName);
            }
        }

        /// <summary>
        /// Amin: to find VolSer for each dump
        /// </summary>
        /// <param name="line"></param>
        private void DumpVolSerFinder(string line)
        {
            try
            {
                string[] temp = line.Substring(109, line.Length - 109).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                int sequenceNo;
                if (int.TryParse(temp[7], out sequenceNo))
                {
                    LogDump current = Dumplist.Find(x => x.SequenceNo == sequenceNo && x.DumpType == temp[12]);
                    if (current != null)
                    {
                        current.VSN = temp[11];
                    }
                    else
                    {
                        DumpVolSer.Add(sequenceNo + "," + temp[11]);
                    }
                }
            }
            catch (Exception ex)
            {
                AdminDB.LogIt(ex.Message, ex.StackTrace, Counter, processName);
            }
        }


        /// <summary>
        /// Amin: to find high usage pools with time
        /// </summary>
        /// <param name="line"></param>
        private void PoolUsageExceed(string line)
        {
            try
            {
                string[] temp = line.Substring(109, line.Length - 109).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (temp.Length > 9)
                {
                    string pooltype = temp[2];
                    long PoolCount = long.Parse(temp[9]);
                    string moment = DateTime.Parse(LastUpdate).ToString("HHmm");
                    PoolInfo CurrentPool = PoolUsageExceededList.Where(x => x.Time == moment).FirstOrDefault();
                    if (CurrentPool == null)
                    {
                        CurrentPool = new PoolInfo();
                        CurrentPool.Time = moment;
                        CurrentPool.CPU = processName;
                        CurrentPool.PoolType = pooltype;
                        CurrentPool.Count = PoolCount;
                        PoolUsageExceededList.Add(CurrentPool);
                    }
                    int Cutoff = int.Parse(DateTime.Parse(LastUpdate).AddHours(-6).ToString("HHmm"));
                    if (Cutoff > 1759)
                    {
                        Cutoff = 0;
                    }
                    PoolUsageExceededList.RemoveAll(x => int.Parse(x.Time) < Cutoff && x.PoolType == pooltype);
                }
            }
            catch (Exception ex)
            {
                AdminDB.LogIt(ex.Message, ex.StackTrace, Counter, processName);
            }
        }

        /// <summary>
        /// Amin: to find in use pools and minute of last cycle time
        /// </summary>
        /// <param name="line"></param>
        private void PoolInUse()
        {
            try
            {
                string[] temp1 = MultipleLinesDisplayStorage[0].Substring(109, MultipleLinesDisplayStorage[0].Length - 109).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                string[] temp2 = MultipleLinesDisplayStorage[1].Substring(109, MultipleLinesDisplayStorage[1].Length - 109).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                string pooltype = temp1[4];
                long PoolInUseCount = long.Parse(temp1[8]), PoolMinutes = long.Parse(temp2[5]);
                string moment = DateTime.Parse(LastUpdate).ToString("HHmm");
                PoolInfo CurrentPoolInUse = PoolUsageExceededList.Where(x => x.Time == moment).FirstOrDefault();
                PoolInfo CurrentPoolMinute = PoolUsageExceededList.Where(x => x.Time == moment).FirstOrDefault();
                if (CurrentPoolInUse == null)
                {
                    CurrentPoolInUse = new PoolInfo();
                    CurrentPoolInUse.Time = moment;
                    CurrentPoolInUse.CPU = processName;
                    CurrentPoolInUse.PoolType = pooltype;
                    CurrentPoolInUse.Count = PoolInUseCount;
                    PoolInUseList.Add(CurrentPoolInUse);
                }

                if (CurrentPoolMinute == null)
                {
                    CurrentPoolMinute = new PoolInfo();
                    CurrentPoolMinute.Time = moment;
                    CurrentPoolMinute.CPU = processName;
                    CurrentPoolMinute.PoolType = pooltype;
                    CurrentPoolMinute.Count = PoolMinutes;
                    PoolMinuteList.Add(CurrentPoolMinute);
                    CurrentPoolMinute.UpdateCycleCount(DateTime.Now, sysName);
                }
                int Cutoff = int.Parse(DateTime.Parse(LastUpdate).AddHours(-6).ToString("HHmm"));
                if (Cutoff > 1759)
                {
                    Cutoff = 0;
                }
                PoolMinuteList.RemoveAll(x => int.Parse(x.Time) < Cutoff && x.PoolType == pooltype);
                PoolInUseList.RemoveAll(x => int.Parse(x.Time) < Cutoff && x.PoolType == pooltype);
            }
            catch (Exception ex)
            {
                AdminDB.LogIt(ex.Message, ex.StackTrace, Counter, processName);
            }
        }

        /// <summary>
        /// Amin: to Auto-end A Job
        /// </summary>
        /// <param name="ScheduleID"></param>
        private void EndJobCheck(long ScheduleID, string Action)
        {
            if (AdminAutoUpdate && UserAutoEndUpdate)
            {
                try
                {
                    List<LogResponseDB> UDBResponseList;
                    List<LogSingleTapeDB> UDBFinalTapeList;
                    switch (sysName)
                    {
                        case "WNP":
                            UDBResponseList = _DBResponseListWNP;
                            UDBFinalTapeList = DBFinalTapeListWNP;
                            break;
                        case "FCA":
                            UDBResponseList = _DBResponseListFCA;
                            UDBFinalTapeList = DBFinalTapeListFCA;
                            break;
                        case "FOS":
                            UDBResponseList = _DBResponseListFOS;
                            UDBFinalTapeList = DBFinalTapeListFOS;
                            break;
                        case "PSS":
                            UDBResponseList = _DBResponseListPSS;
                            UDBFinalTapeList = DBFinalTapeListPSS;
                            break;
                        default:
                            return;
                    }
                    string checkResult = "";
                    ScheduleJob schJobCheckUpdate = new ScheduleJob();
                    LogSingleTapeDB TempTape = null;
                    LogResponseDB TempResponse = null;

                    string job_st = schJobCheckUpdate.GetStatusWScheduleID(ScheduleID);
                    if (job_st.Contains("END"))
                    {
                        checkResult += "Auto-End not required since job was " + job_st + " before.";
                    }
                    else if (job_st != "STARTED")
                    {
                        checkResult += "Job was not on STARTED state(" + job_st + ").";
                    }

                    schJobCheckUpdate.Schedule_ID = ScheduleID;
                    schJobCheckUpdate.Job_End_Time = LastUpdate;
                    schJobCheckUpdate.CPU_ID = Thread_ProcessID;
                    schJobCheckUpdate.System_ID = Thread_sysID;
                    schJobCheckUpdate.Job_Status = "ENDED-Successful";
                    schJobCheckUpdate.System_Name = sysName;
                    schJobCheckUpdate.Schedule_Date = DateOfLog.ToString("yyyy-MM-dd");


                    if (OpsUserName == null || OpsUserName.Length < 8) // to prevent empty USER for started jobs
                    {
                        OpsUserName = (new AdminDB()).GetLastAutoPilotUser(sysName, 2);
                    }
                    schJobCheckUpdate.Operator_End = OpsUserName;

                    if (schJobCheckUpdate.Operator_End.Length < 8)
                    {
                        UserAutoEndUpdate = false;
                        checkResult += "Auto Update Not Active";
                    }
                    else
                    {
                        schJobCheckUpdate.Job_sch.Read_Job_Check(ScheduleID);
                        if (schJobCheckUpdate.Job_sch.CPU_Name == "ALL")
                        {
                            checkResult += "Multi CPU.";
                        }
                        else
                        {
                            // Check Tape for Auto-End
                            if (schJobCheckUpdate.Job_sch.Job_Tape.Count > 0)
                            {
                                for (int i = 0; i < schJobCheckUpdate.Job_sch.Job_Tape.Count; i++)
                                {
                                    if (UDBFinalTapeList.Where(x => x.ScheduleID == schJobCheckUpdate.Schedule_ID
                                                                && x.TapeID == schJobCheckUpdate.Job_sch.Job_Tape[i].Tape_ID).FirstOrDefault() == null)
                                    {
                                        checkResult += "Tape ID " + schJobCheckUpdate.Job_sch.Job_Tape[i].Tape_ID + " ("
                                            + schJobCheckUpdate.Job_sch.Job_Tape[i].Name + ") did not mounted.";
                                        break;
                                    }
                                    TempTape = UDBFinalTapeList.Where(x => x.TapeID == schJobCheckUpdate.Job_sch.Job_Tape[i].Tape_ID
                                                                        && x.ScheduleID == schJobCheckUpdate.Schedule_ID
                                                                        && (x.TapeRemoveTimeDB == DateTime.MinValue
                                                                                || (x.TapeSubmitTimeDB == DateTime.MinValue
                                                                                        && schJobCheckUpdate.Job_sch.Job_Tape[i].IsSentOS390 == true))).FirstOrDefault();
                                    if (TempTape != null)
                                    {
                                        checkResult += "Tape ID " + TempTape.TapeID + " ("
                                            + TempTape.TapeName + ") did not remove/submit.";
                                        break;
                                    }
                                }
                            }
                            TempResponse = UDBResponseList.Where(x => x.RsponseTimeDB == DateTime.MinValue
                                                                                && x.ScheduleID == schJobCheckUpdate.Schedule_ID
                                                                                && x.TypeDB == "END").FirstOrDefault();
                            if (TempResponse != null)
                            {
                                checkResult += "Command ID " + TempResponse.CommandID + " has not seen on TOS log.";
                            }
                        }
                    }
                    if (checkResult == "")
                    {
                        schJobCheckUpdate.AutoEndResponseUpdate();
                        AdminDB.LogIt("System Name:" + schJobCheckUpdate.Job_sch.System_Name
                            + ", Schedule ID: " + schJobCheckUpdate.Schedule_ID
                            + ", Job ID:" + schJobCheckUpdate.Job_sch.Job_ID
                            + ", Job Name:" + schJobCheckUpdate.Job_sch.Job_Name_OPS
                            + ", Auto-End Job Done by " + Action + ".");
                        addNote(schJobCheckUpdate.Job_sch.Job_Name + " Auto-Ended on " + LastUpdate + " (" + OpsUserName + ")"
                                    , schJobCheckUpdate.Schedule_ID, processName, "Auto-End", false);
                    }
                    //else
                    //{
                    //    AdminDB.LogIt("System Name:" + schJobCheckUpdate.Job_sch.System_Name
                    //        + ", Schedule ID: " + schJobCheckUpdate.Schedule_ID
                    //        + ", Job ID:" + schJobCheckUpdate.Job_sch.Job_ID
                    //        + ", User: " + schJobCheckUpdate.Operator_End
                    //        + ", Job Name:" + schJobCheckUpdate.Job_sch.Job_Name_OPS
                    //        + ", Auto-End Job Error: " + checkResult);
                    //}
                }
                catch (Exception ex)
                {
                    AdminDB.LogIt(ex.Message, ex.StackTrace, Counter, processName);
                }
            }
        }
    }
}