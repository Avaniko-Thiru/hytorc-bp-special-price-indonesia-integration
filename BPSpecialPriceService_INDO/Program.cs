using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace BPSpecialPriceService_INDO
{
    class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>


        #region Declaration

        public static int SQL_TimeOut = Convert.ToInt32(ConfigurationManager.AppSettings["SQLConnectionTimeOut"]);

        #endregion Declaration

        #region Main

        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            log4net.Config.XmlConfigurator.Configure();
            #if DEBUG
                BPSpecialPrice oBPSP = new BPSpecialPrice();
                oBPSP.OnDebug();
                System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
            #else
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new BPSpecialPrice()
                };
                ServiceBase.Run(ServicesToRun);
            #endif
        }

        #endregion Main

        #region Execute Query

        public static DataTable Execute_Query(string sql, string oConStr)
        {
            ILog log = LogManager.GetLogger(typeof(Program));
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection myConnection = new SqlConnection(oConStr))
                {
                    using (SqlCommand cmd = new SqlCommand(sql, myConnection))
                    {
                        cmd.CommandTimeout = SQL_TimeOut;

                        cmd.CommandType = CommandType.Text;
                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        {
                            sda.Fill(dt);
                            return dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Execute Query : " + GetFullMessage(ex));
                return dt;
            }
        }

        #endregion Excute Query

        #region GetFullMessage

        public static string GetFullMessage(Exception ex)
        {
            return ex.InnerException == null ? ex.Message : ex.Message + " --> " + ex.InnerException.ToString();
        }

        #endregion GetFullMessage

        #region CurrentDomain_UnhandledException

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ILog log = LogManager.GetLogger(typeof(Program));
            log.Error("UnhandledException Found : " + e.ToString());
            Environment.Exit(1);
        }

        #endregion CurrentDomain_UnhandledException

    }
}
