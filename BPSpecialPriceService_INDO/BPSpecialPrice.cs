using log4net;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using SAPB1;
//using SAPbouiCOM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using DWORD = System.UInt32;
using LPWSTR = System.String;
using NET_API_STATUS = System.UInt32;

namespace BPSpecialPriceService_INDO
{
    public partial class BPSpecialPrice : ServiceBase
    {

        #region Declaration

        System.Timers.Timer timer = new System.Timers.Timer();
        private static string Timer_Period = "";
        private static string oConnectionStr = "";
        private static string SQLServerName = "", SQLUserName = "", SQLPassword = "", SQLServerType  = "";
        private static string FolderType = "";
        public static string ServiceLayerUrl = "", SAPDBName = "", SAPUserName = "", SAPPassword = "", LicenseServer = "";
        private static int SQL_TimeOut = 60;
        private static int? SessionTimeOut = 0;
        private static DateTime LastLoginDateTime = DateTime.Now;
        ServiceLayerService sLayer = null;
        public static bool AdvanceDebug = false;

        ILog log = LogManager.GetLogger(typeof(BPSpecialPrice));

        SAPbobsCOM.Company oCompany;

        #endregion Declaration

        public BPSpecialPrice()
        { 
            InitializeComponent();
            this.CanStop = true;
            this.CanPauseAndContinue = true;
        }

        #region OnDebug

        public void OnDebug()
        {
            OnStart(null);
        }

        #endregion OnDebug

        #region OnStart

        protected override void OnStart(string[] args)
        {
            log.Info("BPSpecialPrice Service  -  Started");

            Timer_Period = ConfigurationManager.AppSettings["timer"];
            SQL_TimeOut = Convert.ToInt32(ConfigurationManager.AppSettings["SQLConnectionTimeOut"]);
            AdvanceDebug = ConfigurationManager.AppSettings["AdvanceDebug"].ToString() == "Y" ? true : false;

            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            string path = System.IO.Path.Combine(Environment.CurrentDirectory + "\\Credentials.json");

            if (File.Exists(path))
            {
                using (StreamReader oReader = new StreamReader("Credentials.json"))
                {
                    CredentialModel oModel = JsonConvert.DeserializeObject<CredentialModel>(oReader.ReadToEnd());

                    FolderType = oModel.FolderType;
                    ServiceLayerUrl = oModel.ServiceLayerUrl;
                    SAPDBName = oModel.SAPDBName;
                    SAPUserName = oModel.SAPUserName;
                    SAPPassword = Cls_Encrypt_Helper.Decrypt(oModel.SAPPassword);
                    SQLServerName = oModel.SQLServerName;
                    SQLUserName = oModel.SQLUserName;
                    SQLPassword = Cls_Encrypt_Helper.Decrypt(oModel.SQLPassword);
                    SQLServerType = oModel.SQLServerType;
                    LicenseServer = oModel.LicenseServer;
                    oConnectionStr = string.Format("Data Source = {0}; User ID = {1}; Password = {2}", SQLServerName, SQLUserName, SQLPassword);
                    SqlConnection oSQLConn = new SqlConnection(oConnectionStr);
                    try
                    {
                        oSQLConn.Open();
                        oConnectionStr = "Data Source = " + SQLServerName + "; Initial Catalog = {0} ;User ID = " + SQLUserName + "; Password = " + SQLPassword + "";

                        oCompany = new SAPbobsCOM.Company();

                        if (SQLServerType == "MSSQL")
                        {
                            oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL;
                        }
                        else if (SQLServerType == "MSSQL2005")
                        {
                            oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2005;
                        }
                        else if(SQLServerType == "MSSQL2008")
                        {
                            oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2008;
                        }
                        else if (SQLServerType == "MSSQL2012")
                        {
                            oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2012;
                        }
                        else if (SQLServerType == "MSSQL2014")
                        {
                            oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2014;
                        }
                        else if (SQLServerType == "MSSQL2016")
                        {
                            oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2016;
                        }
                        else if (SQLServerType == "MSSQL2017")
                        {
                            oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2017;
                        }
                        else if (SQLServerType == "MSSQL2019")
                        {
                            oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2019;
                        }

                        oCompany.Server = SQLServerName;
                        oCompany.LicenseServer = LicenseServer;//"sapprodsql19.hytorcnj.local:30000";
                        //oCompany.SLDServer = "sapdevsql.hytorcnj.local:30000";
                        oCompany.DbUserName = SQLUserName;
                        oCompany.DbPassword = SQLPassword;
                        oCompany.CompanyDB = SAPDBName;
                        oCompany.UserName = SAPUserName;
                        oCompany.Password = SAPPassword;
                        int RetCod = oCompany.Connect();
                        if (RetCod == 0)
                        {
                            log.Info("Company Connected to DBName : " + oCompany.CompanyDB + "\n");
                        }
                        else
                        {
                            string sErrMsg = oCompany.GetLastErrorDescription();
                            log.Error("Company Connection Failed : " + sErrMsg);
                        }

                    }
                    catch (Exception ex)
                    {
                        log.Error("SQL Connection : " + Program.GetFullMessage(ex));
                        return;
                    }
                    finally
                    {
                        oSQLConn.Dispose();
                    }
                }
            }
            else
            {
                log.Error("Credentials.json File is Missing");
                return;
            }

            if (Timer_Period != null)
            {
                timer.Enabled = true;
                timer.Interval = Convert.ToInt32(Timer_Period);
                timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            }
        }

        #endregion OnStart

        #region Timer Elapsed

        protected void timer_Elapsed(object source, System.Timers.ElapsedEventArgs aa)
        {
            timer.Enabled = false;
            try
            {
                string oConn = string.Format(oConnectionStr, SAPDBName);
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

                DataTable oDT = Program.Execute_Query("EXEC dbo.AVA_BPSpecialPriceUpdate", oConn);
                if (oDT.Rows.Count > 0)
                {
                    foreach (DataRow oRow in oDT.Rows)
                    {
                        if (AdvanceDebug)
                            log.Info("Timer Started For BPSpecialPrice Creation Method Id : " + oRow["Id"].ToString());
                        stopwatch = new System.Diagnostics.Stopwatch();
                        stopwatch.Start();

                        log.Info("BP Special price Creation started for CardCode : " + oRow["CardCode"].ToString());

                        BPSpecialPriceCreation(oRow["Id"].ToString(), oRow["User"].ToString(), oRow["CardCode"].ToString(), oRow["FileName"].ToString(), oRow["FilePath"].ToString(), oRow["Type"].ToString(), oRow["Domain"].ToString(), oRow["UserName"].ToString(), oRow["Password"].ToString());

                        log.Info("BP Special price Creation ended for CardCode : " + oRow["CardCode"].ToString());

                        stopwatch.Stop();
                        if (AdvanceDebug)
                            log.Info("Timer Stopped For BPSpecialPrice Creation Method Time : " + stopwatch.Elapsed);
                    }
                }

                DataTable oDT1 = Program.Execute_Query("EXEC dbo.AVA_MSAPriceUploadCustomer", oConn);
                if (oDT1.Rows.Count > 0)
                {
                    foreach (DataRow oRow in oDT1.Rows)
                    {
                        if (AdvanceDebug)
                            log.Info("Timer Started For GeneralService_For_UDO_MSAAgreement Add Method for MSA Price Upload Code : " + oRow["MSAPUCode"].ToString());
                        stopwatch = new System.Diagnostics.Stopwatch();
                        stopwatch.Start();

                        log.Info("GeneralService_For_UDO_MSAAgreement_AddMethod Creation started for MSA Price Upload Code : " + oRow["MSAPUCode"].ToString());

                        GeneralService_For_UDO_MSAAgreement_AddMethod(oRow["MSAPUCode"].ToString(), oRow["MSAPACode"].ToString(), oRow["CardCode"].ToString(), oRow["CardName"].ToString(), oRow["MSAAgreementNo"].ToString(), oRow["Active"].ToString(), oRow["CreateDate"].ToString(), oRow["CreateUser"].ToString(), oRow["ShipToCode"].ToString(), oRow["Street"].ToString(), oRow["City"].ToString(), oRow["State"].ToString(), oRow["ZipCode"].ToString(), oRow["Comments"].ToString(), oRow["UpdateDate"].ToString(), oRow["UpdateUser"].ToString(), oRow["MSAPULine"].ToString());

                        log.Info("GeneralService_For_UDO_MSAAgreement_AddMethod Creation ended for MSA Price Upload Code : " + oRow["MSAPUCode"].ToString());

                        stopwatch.Stop();
                        if (AdvanceDebug)
                            log.Info("Timer Stopped For GeneralService_For_UDO_MSAAgreement Add Method Time : " + stopwatch.Elapsed);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Timer Elapsed : " + Program.GetFullMessage(ex));
            }
            finally
            {
                timer.Enabled = true;
            }
        }

        #endregion Timer Elapsed

        #region BPSpecialPriceCreation
        public void BPSpecialPriceCreation(string Id, string User, string CardCode, string FileName, string FilePath, string Type, string Server, string UserName, string Password)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            string oConn = string.Format(oConnectionStr, SAPDBName);
            try
            {
                if (FolderType == "S")
                {
                    UNCAccessWithCredentials unc1 = new UNCAccessWithCredentials();
                    if (unc1.NetUseWithCredentials(FilePath, UserName, Server, Cls_Encrypt_Helper.Decrypt(Password)))
                    {
                        if (AdvanceDebug)
                            log.Info("File Path Server Credentials Succeeded");
                    }
                    else
                    {
                        Save_Response("Update [@AVA_SPLLOG] SET U_AVA_Status=@Status,U_AVA_InProg=@Progress,U_AVA_Error=@Error where Code=@Id", Id, "F", "C", "Server Credentials Failed for Path - " + FilePath, oConn);
                        log.Error("Server Credentials Failed for File Path - " + FilePath);
                        return;
                    }
                }
                string File = FilePath + "//" + FileName;
                if (System.IO.File.Exists(File))
                {
                    XSSFWorkbook hssfworkbook = new XSSFWorkbook(File);
                    ISheet sheet = hssfworkbook.GetSheetAt(0);

                    DataTable dt = new DataTable();
                    IRow headerRow = sheet.GetRow(0);
                    IEnumerator rows = sheet.GetRowEnumerator();

                    int colCount = headerRow.LastCellNum;
                    int rowCount = sheet.LastRowNum;

                    dt.Columns.Add("ITEMCODE");
                    dt.Columns.Add("PRICELISTCODE");
                    dt.Columns.Add("DISCOUNTPERCENTAGE", typeof(decimal));
                    dt.Columns.Add("PRICE", typeof(decimal));
                    dt.Columns.Add("AUTOPRICE");

                    while (rows.MoveNext())
                    {
                        IRow row = (XSSFRow)rows.Current;

                        ICell cell = row.GetCell(0);
                        if (cell != null)
                        {
                            if (!string.IsNullOrEmpty(cell.ToString()) && cell.ToString().ToUpper() != "ITEMCODE")
                            {
                                DataRow dr = dt.NewRow();
                                for (int i = 0; i < colCount; i++)
                                {
                                    cell = row.GetCell(i);

                                    if (cell != null)

                                        dr[i] = cell.ToString();
                                }
                                dt.Rows.Add(dr);
                            }
                        }
                    }

                    hssfworkbook.Close();

                    BPSpecialPriceAdd_ServiceLayer(dt, User, Type, Id, CardCode, oConn);
                }
                else
                {
                    Save_Response("Update [@AVA_SPLLOG] SET U_AVA_Status=@Status,U_AVA_InProg=@Progress,U_AVA_Error=@Error where Code=@Id", Id, "F", "C", "File is not found in this Path - " + FilePath, oConn);
                    log.Error("File is not found in this Path - " + FilePath);
                }
            }
            catch (Exception ex)
            {
                Save_Response("Update [@AVA_SPLLOG] SET U_AVA_Status=@Status,U_AVA_InProg=@Progress,U_AVA_Error=@Error where Code=@Id", Id, "F", "C", ex.Message.ToString(), oConn);
                log.Error("BPSpecialPriceCreation : " + Program.GetFullMessage(ex));
            }
        }
        #endregion

        #region BPSpecialPriceAdd ServiceLayer
        public void BPSpecialPriceAdd_ServiceLayer(DataTable oDt, string User, string Type, string Id, string CardCode, string oConn)
        {
            try
            {
                if (oDt != null)
                {
                    int i = 0;
                    bool FailStatus = false, SuccessStatus = false;
                    while (i < oDt.Rows.Count)
                    {
                        DataRow oRow = oDt.Rows[i];

                        string ItemCode = oRow["ITEMCODE"].ToString();
                        string PriceList = oRow["PRICELISTCODE"].ToString();
                        string DiscountPercent = oRow["DISCOUNTPERCENTAGE"].ToString();
                        string Price = oRow["PRICE"].ToString();
                        string AutoPrice = oRow["AUTOPRICE"].ToString();

                        TimeSpan span = DateTime.Now.Subtract(LastLoginDateTime);
                        bool LoginStatus = false;

                        if (span.TotalMinutes >= SessionTimeOut)
                        {
                           LoginStatus = Login();
                        }
                        else
                        {
                            LoginStatus = true;
                        }

                        if (LoginStatus)
                        {
                            if (Type == "A")
                            {
                                SpecialPrice SP = new SpecialPrice();

                                bool getretrivestatus = true;
                                var getrespMsg = "";
                                try
                                {
                                    SP = sLayer.GetSpecialPrice(CardCode, ItemCode);
                                    getretrivestatus = true;
                                }
                                catch (Exception ex)
                                {
                                    getretrivestatus = false;
                                    getrespMsg = Program.GetFullMessage(ex);

                                    //if (getrespMsg.ToString().Contains("Invalid Session ID"))
                                    //{
                                    //    try
                                    //    {
                                    //        if (Login())
                                    //        {
                                    //            SP = sLayer.GetSpecialPrice(CardCode, ItemCode);
                                    //            getretrivestatus = true;
                                    //        }
                                    //        else
                                    //        {
                                    //            getretrivestatus = false;
                                    //        }
                                    //    }
                                    //    catch (Exception ex1)
                                    //    {
                                    //        getretrivestatus = false;
                                    //        getrespMsg = Program.GetFullMessage(ex1);
                                    //    }
                                    //}
                                }
                                if (getretrivestatus == false)
                                {
                                    SP.CardCode = CardCode;
                                    SP.Price = Convert.ToDouble(Price);
                                    SP.ItemCode = ItemCode;
                                    SP.PriceListNum = Convert.ToInt32(PriceList);
                                    SP.DiscountPercent = Convert.ToDouble(DiscountPercent);
                                    SP.AutoUpdate = AutoPrice;
                                    SP.U_AVA_UpdateDate = DateTime.Now;
                                    SP.U_AVA_UserName = User;

                                    string addBPSP = sLayer.AddSpecialPrice(SP);
                                    if (addBPSP != "Created")
                                    {
                                        FailStatus = true;
                                        log.Error("Error while adding Special Price for CardCode : " + CardCode + " & ItemCode : " + ItemCode + " - " + addBPSP);
                                    }
                                    else
                                    {
                                        SuccessStatus = true;
                                        if (AdvanceDebug)
                                            log.Error("Special Price Added Successfully for CardCode : " + CardCode + " & ItemCode : " + ItemCode);
                                    }
                                }
                                else
                                {
                                    SP.Price = Convert.ToDouble(Price);
                                    SP.PriceListNum = Convert.ToInt32(PriceList);
                                    SP.DiscountPercent = Convert.ToDouble(DiscountPercent);
                                    SP.AutoUpdate = AutoPrice;
                                    SP.U_AVA_UpdateDate = DateTime.Now;
                                    SP.U_AVA_UserName = User;

                                    string updateBPSP = sLayer.UpdateSpecialPrice(SP);

                                    if (updateBPSP != "Updated")
                                    {
                                        FailStatus = true;
                                        log.Error("Error while updating Special Price for CardCode : " + CardCode + " & ItemCode : " + ItemCode + " - " + updateBPSP);
                                    }
                                    else
                                    {
                                        SuccessStatus = true;
                                        if (AdvanceDebug)
                                            log.Error("Special Price Updated Successfully for CardCode : " + CardCode + " & ItemCode : " + ItemCode);
                                    }
                                }

                            }
                            else if (Type == "D")
                            {
                                SpecialPrice SP = new SpecialPrice();
                                bool getretrivestatus = true;
                                var getrespMsg = "";
                                try
                                {
                                    SP = sLayer.GetSpecialPrice(CardCode, ItemCode);
                                    getretrivestatus = true;
                                }
                                catch (Exception ex)
                                {
                                    getretrivestatus = false;
                                    getrespMsg = Program.GetFullMessage(ex);
                                }

                                if (getretrivestatus == true)
                                {
                                    string deleteBPSP = sLayer.DeleteSpecialPrice(SP);
                                    if (deleteBPSP != "Deleted")
                                    {
                                        FailStatus = true;
                                        log.Error("Error while Deleting Special Price for CardCode : " + CardCode + " & ItemCode : " + ItemCode + " - " + deleteBPSP);
                                    }
                                    else
                                    {
                                        SuccessStatus = true;
                                        if (AdvanceDebug)
                                            log.Error("Special Price Deleted Successfully for CardCode : " + CardCode + " & ItemCode : " + ItemCode);
                                    }
                                }
                                else
                                {
                                    FailStatus = true;
                                    log.Error("Error while getting Special Price for CardCode : " + CardCode + " & ItemCode : " + ItemCode + " - " + getrespMsg);
                                }
                            }
                        }

                        i++;
                    }

                    string Response = "Success";
                    string Status = "S";
                    if (FailStatus == false && SuccessStatus == true)
                    {
                        Response = "Success";
                        Status = "S";
                    }
                    else if (FailStatus == true && SuccessStatus == true)
                    {
                        Response = "Partial Success";
                        Status = "PS";
                    }
                    else
                    {
                        Response = "Failed";
                        Status = "F";
                    }
                    Save_Response("Update [@AVA_SPLLOG] SET U_AVA_Status=@Status,U_AVA_InProg=@Progress,U_AVA_Error=@Error where Code=@Id", Id, Status, "C", Response, oConn);
                }
            }
            catch(Exception ex)
            {
                Save_Response("Update [@AVA_SPLLOG] SET U_AVA_Status=@Status,U_AVA_InProg=@Progress,U_AVA_Error=@Error where Code=@Id", Id, "F", "C", ex.Message.ToString(), oConn);
                log.Error("BPSpecialPriceAdd_ServiceLayer Error : " + Program.GetFullMessage(ex));
            }
        }
        #endregion BPSpecialPriceAdd ServiceLayer

        #region Login

        public bool Login()
        {
            try
            {
                string sessionid = "";

                sLayer = new ServiceLayerService();
                sLayer.InitServiceContainer(ServiceLayerUrl);

                LastLoginDateTime = DateTime.Now;
                B1Session session = sLayer.LoginServer(new SboCred(SAPUserName, SAPPassword, SAPDBName));
                sessionid = session.SessionId;
                SessionTimeOut = session.SessionTimeout;

                if (sessionid != null && sessionid != "")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                log.Error("Login Error : " + Program.GetFullMessage(ex));
                return false;
            }
        }

        #endregion

        #region Save Response
        public static void Save_Response(string sql, string Id, string Status, string Progress, string Error, string oConStr)
        {
            ILog log = LogManager.GetLogger(typeof(BPSpecialPrice));
            try
            {
                using (SqlConnection myConnection = new SqlConnection(oConStr))
                {
                    myConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, myConnection))
                    {
                        cmd.CommandTimeout = SQL_TimeOut;

                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@Id", Id);
                        cmd.Parameters.AddWithValue("@Status", Status);
                        cmd.Parameters.AddWithValue("@Error", Error);
                        cmd.Parameters.AddWithValue("@Progress", Progress);
                        cmd.ExecuteNonQuery();
                    }
                    myConnection.Close();
                }
            }
            catch (Exception ex)
            {
                log.Error("Save Response : " + Program.GetFullMessage(ex));
            }
        }
        #endregion Save Response

        #region UNCAccessWithCredentials

        public class UNCAccessWithCredentials : IDisposable
        {
            internal static class NativeMethods
            {
                [DllImport("NetApi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
                internal static extern NET_API_STATUS NetUseAdd(
                    LPWSTR UncServerName,
                    DWORD Level,
                    ref USE_INFO_2 Buf,
                    out DWORD ParmError);

                [DllImport("NetApi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
                internal static extern NET_API_STATUS NetUseDel(
                    LPWSTR UncServerName,
                    LPWSTR UseName,
                    DWORD ForceCond);
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            internal struct USE_INFO_2
            {
                internal LPWSTR ui2_local;
                internal LPWSTR ui2_remote;
                internal LPWSTR ui2_password;
                internal DWORD ui2_status;
                internal DWORD ui2_asg_type;
                internal DWORD ui2_refcount;
                internal DWORD ui2_usecount;
                internal LPWSTR ui2_username;
                internal LPWSTR ui2_domainname;
            }



            private bool disposed = false;

            private string sUNCPath;
            private string sUser;
            private string sPassword;
            private string sDomain;
            private int iLastError;

            /// <summary>
            /// A disposeable class that allows access to a UNC resource with credentials.
            /// </summary>
            public UNCAccessWithCredentials()
            {
            }
            /// <summary>
            /// The last system error code returned from NetUseAdd or NetUseDel.  Success = 0
            /// </summary>
            public int LastError
            {
                get { return iLastError; }
            }

            /// <summary>
            /// Dispose
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }


            /// <summary>
            /// Dispose Implementation
            /// </summary>
            /// <param name="isDisposing"></param>
            protected virtual void Dispose(bool isDisposing)
            {
                if (disposed)
                    return;

                if (isDisposing)
                {
                    //NetUseDelete();
                }
                disposed = true;
            }

            /// <summary>
            /// Connects to a UNC path using the credentials supplied.
            /// </summary>
            /// <param name="UNCPath">Fully qualified domain name UNC path</param>
            /// <param name="User">A user with sufficient rights to access the path.</param>
            /// <param name="Domain">Domain of User.</param>
            /// <param name="Password">Password of User</param>
            /// <returns>True if mapping succeeds.  Use LastError to get the system error code.</returns>
            public bool NetUseWithCredentials(string UNCPath, string User, string Domain, string Password)
            {
                sUNCPath = UNCPath;
                sUser = User;
                sPassword = Password;
                sDomain = Domain;
                return NetUseWithCredentials();
            }

            private bool NetUseWithCredentials()
            {
                uint returncode;
                try
                {
                    USE_INFO_2 useinfo = new USE_INFO_2();

                    useinfo.ui2_remote = sUNCPath;
                    useinfo.ui2_username = sUser;
                    useinfo.ui2_domainname = sDomain;
                    useinfo.ui2_password = sPassword;
                    useinfo.ui2_asg_type = 0;
                    useinfo.ui2_usecount = 1;
                    uint paramErrorIndex;
                    returncode = NativeMethods.NetUseAdd(null, 2, ref useinfo, out paramErrorIndex);
                    iLastError = (int)returncode;
                }
                catch
                {
                    iLastError = Marshal.GetLastWin32Error();
                }

                //If 0 it means there's no error
                if (iLastError == 0)
                {
                    return true;
                }
                //Default error codes that are being ignored.
                //1394 = No Session Key
                //64   = The Specified Network name is no longer available
                //1326,1219 Multiple connections to a server or shared resource by the same user, using more than one user name, are not allowed. Disconnect all previous connections to the server or shared resource and try again. It means that there's already connection to this folder.
                // else if (ApplicationConstant.GetIgnoredErrorCodes().Contains(iLastError.ToString()))
                //{
                //    //Error is being ignored
                //    iLastError = 0;
                //    return true;
                //}
                else
                    return false;
            }

            /// <summary>
            /// Ends the connection to the remote resource 
            /// </summary>
            /// <returns>True if it succeeds.  Use LastError to get the system error code</returns>
            public bool NetUseDelete()
            {
                uint returncode;
                try
                {
                    returncode = NativeMethods.NetUseDel(null, sUNCPath, 2);
                    iLastError = (int)returncode;
                    return (returncode == 0);
                }
                catch
                {
                    iLastError = Marshal.GetLastWin32Error();
                    return false;
                }
            }
        }

        #endregion UNCAccessWithCredentials

        #region OnStop

        protected override void OnStop()
        {
            log.Info("BPSpecialPrice Service  -  Stopped");
        }

        #endregion OnStop

        #region General Service For UDO MSA Agreement Add Method
        public void GeneralService_For_UDO_MSAAgreement_AddMethod(string MSAPUCode,string MSAPACode,string CardCode,string CardName, string MSAAggNo, string Active, string CDate,string CUser, string ShipToCode,string Street, string City, string State, string ZipCode, string Comments, string UpdateDate , string UpdateUser, string MSAPULine)
        {
            string oConn = string.Format(oConnectionStr, SAPDBName);
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            SAPbobsCOM.GeneralService oGeneralService;
            SAPbobsCOM.GeneralData oGeneralData;
            SAPbobsCOM.GeneralData oChild;
            SAPbobsCOM.GeneralDataCollection oChildren;
            //SAPbobsCOM.GeneralDataParams oGeneralParams;

            try
            {
                SAPbobsCOM.CompanyService sCmp;
                sCmp = oCompany.GetCompanyService();

                //Get GeneralService (oCmpSrv is the CompanyService)

                oGeneralService = sCmp.GetGeneralService("AVA_MSAPA");

                //Create data for new row in main UDO

                oGeneralData = oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);

                //string MSAPACode1 = "SELECT CONCAT('MSAPA-', RIGHT(REPLICATE('0', 5) + (ISNULL(MAX(CAST(right(code, LEN(code) - CHARINDEX('-', code)) AS INT)) + 1, 1)), 5)) AS Code FROM[@AVA_MSAPAH]";

                oGeneralData.SetProperty("Code", "" + Query_Execute("SELECT CONCAT('MSAPA-', RIGHT(REPLICATE('0', 5) + (ISNULL(MAX(CAST(right(code, LEN(code) - CHARINDEX('-', code)) AS INT)) + 1, 1)), 5)) AS Code FROM[@AVA_MSAPAH]") + "");

                string MSAAgrCode = Query_Execute("SELECT CONCAT('MSAPA-', RIGHT(REPLICATE('0', 5) + (ISNULL(MAX(CAST(right(code, LEN(code) - CHARINDEX('-', code)) AS INT)) + 1, 1)), 5)) AS Code FROM[@AVA_MSAPAH]");
                
                //oGeneralData.SetProperty("Code", "" + MSAPACode1 + "");
                oGeneralData.SetProperty("U_AVA_CCode", "" + CardCode + "");
                oGeneralData.SetProperty("U_AVA_CName", "" + CardName + "");
                oGeneralData.SetProperty("U_AVA_MSAAg", "" + MSAAggNo + "");
                oGeneralData.SetProperty("U_AVA_CDate", "" + CDate + "");
                oGeneralData.SetProperty("U_AVA_CUser", "" + CUser + "");
                oGeneralData.SetProperty("U_AVA_Act", "" + Active + "");

                oGeneralData.SetProperty("U_AVA_STCode", "" + ShipToCode + "");
                oGeneralData.SetProperty("U_AVA_Str", "" + Street + "");
                oGeneralData.SetProperty("U_AVA_City", "" + City + "");

                oGeneralData.SetProperty("U_AVA_State", "" + State + "");
                oGeneralData.SetProperty("U_AVA_ZCode", "" + ZipCode + "");
                oGeneralData.SetProperty("U_AVA_Commn", "" + Comments + "");
                oGeneralData.SetProperty("U_AVA_UDate", "" + UpdateDate + "");
                oGeneralData.SetProperty("U_AVA_UUser", "" + UpdateUser + "");

                //Create data for a row in the child table

                DataTable oDT2 = Program.Execute_Query("EXEC dbo.AVA_MSAPriceUploadItem '" + MSAPUCode + "', '" + MSAPULine + "'", oConn);
                if (oDT2.Rows.Count > 0)
                {
                    //int i = 1;
                    foreach (DataRow oRow in oDT2.Rows)
                    {
                        oChildren = oGeneralData.Child("AVA_MSAPAL");
                        oChild = oChildren.Add();
                        //oChild.SetProperty("LineId", "" + i.ToString() + "");
                        oChild.SetProperty("U_AVA_ItmCd", "" + oRow["ItemCode"].ToString() + "");
                        oChild.SetProperty("U_AVA_FPrc", "" + oRow["FixedPrice"].ToString() + "");
                        oChild.SetProperty("U_AVA_DiscP", "" + oRow["DiscountPercent"].ToString() + "");
                        oChild.SetProperty("U_AVA_PLNm", "" + oRow["PriceListName"].ToString() + "");
                        oChild.SetProperty("U_AVA_SubFm", "" + oRow["SubFamily"].ToString() + "");
                        oChild.SetProperty("U_AVA_ItmBr", "" + oRow["ItemBrand"].ToString() + "");
                        oChild.SetProperty("U_AVA_TCls", "" + oRow["ToolClass"].ToString() + "");
                        oChild.SetProperty("U_AVA_CPrc", "" + oRow["CalculatedPrice"].ToString() + "");
                        //i++;
                    }
                }

                //Add the new row, including children, to database

                oGeneralService.Add(oGeneralData);

                Save_Response("Update [@AVA_MSAPUC] SET U_AVA_AgCode='" + MSAAgrCode + "',U_AVA_LINESTATUS=@Status,U_AVA_InProg=@Progress,U_AVA_Error=@Error where Code=@Id AND LineId = '" + MSAPULine + "'", MSAPUCode, "C", "C", "MSA Price Agreement Code - " + MSAAgrCode + " - Created Successfully", oConn);
                log.Info("GeneralService_For_UDO_MSAAgreement_AddMethod : MSA Price Agreement Code - " + MSAAgrCode + " - Created Successfully against MSA Price Upload Code - " + MSAPUCode + "");
            }
            catch (Exception ex)
            {
                Save_Response("Update [@AVA_MSAPUC] SET U_AVA_LINESTATUS=@Status,U_AVA_InProg=@Progress,U_AVA_Error=@Error where Code=@Id AND LineId = '" + MSAPULine + "'", MSAPUCode, "F", "C", ex.Message.ToString(), oConn);
                log.Error("GeneralService_For_UDO_MSAAgreement_AddMethod : " + Program.GetFullMessage(ex));
            }

        }
        #endregion

        #region Execute Query
        public string Query_Execute(string Str)
        {
            try
            {
                string Output_Str = "";
                SAPbobsCOM.Recordset Orec;
                Orec = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                Orec.DoQuery(Str);
                if (Orec.RecordCount > 0)
                {
                    Output_Str = Convert.ToString(Orec.Fields.Item(0).Value);
                }
                return Output_Str;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        #endregion
    }
}
