using log4net;
using Microsoft.Data.OData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SAPB1;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BPSpecialPriceService_INDO
{
    class SboCred
    {
        public SboCred()
        { }

        public SboCred(string user, string pass, string company)
        {
            UserName = user;
            Password = pass;
            CompanyDB = company;
        }

        public bool IsValid()
        {
            return (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(CompanyDB));
        }

        public string GetJsonString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public string UserName = string.Empty;
        public string Password = string.Empty;
        public string CompanyDB = string.Empty;
    }

    //Currently Supported Actions
    enum DocActiontType
    {
        Close = 1,
        Cancel = 2
    }

    //The property types when formatting JSON in WCF client.
    enum PropertyType
    {
        SimpleEdmx = 0,
        ComplexType = 1,
        Collection = 2              //Collection of complex types
    }

    enum UpdateSemantics
    {
        PUT = 0,
        PATCH = 1
    }

    class PageLinks
    {
        public DataServiceQueryContinuation<Document> currentLink = null;
        public DataServiceQueryContinuation<Document> prevLink = null;
        public DataServiceQueryContinuation<Document> nextLink = null;

        public PageLinks(DataServiceQueryContinuation<Document> cLink, DataServiceQueryContinuation<Document> pLink, DataServiceQueryContinuation<Document> nLink)
        {
            currentLink = cLink;
            prevLink = pLink;
            nextLink = nLink;
        }
    }

    class ServiceLayerService
    {
        #region Declaration
        static ILog log = LogManager.GetLogger(typeof(ServiceLayerService));
        #endregion Declaration

        //Changed to Public
        public ServiceLayerService()
        {

        }

        private string strCurrentServerURL = string.Empty;
        private string strCurrentSessionGUID = string.Empty;
        private string strCurrentRouteIDString = string.Empty;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        private ServiceLayer currentServiceContainer = null;
        private int currentDefaultPagingSizing = 10;

        private StringBuilder sbHttpRequestHeaders = new StringBuilder();
        private StringBuilder sbHttpResponseHeaders = new StringBuilder();
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        int currentPage = -1;
        private Hashtable pageLinksList = new Hashtable();
        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get Current Paging size (stored in internal variable)
        /// </summary>
        /// <returns></returns>
        public int GetCurrentPagingSize()
        {
            return currentDefaultPagingSizing;
        }

        private PropertyType GetPropertyType(ODataProperty prop)
        {
            PropertyType retType = PropertyType.SimpleEdmx;
            if (null != prop.Value)
            {
                Type propType = prop.Value.GetType();
                switch (propType.Name)
                {
                    case "ODataComplexValue":
                        retType = PropertyType.ComplexType;
                        break;
                    case "ODataCollectionValue":
                        retType = PropertyType.Collection;
                        break;
                    default:
                        break;
                }
            }

            return retType;
        }

        /// <summary>
        /// Create new ODataComplexValue from the old, to discard all null/zero values
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private ODataComplexValue RebuildComplexValue(ODataComplexValue source)
        {
            ODataComplexValue newVal = new ODataComplexValue();
            newVal.TypeName = source.TypeName;

            List<ODataProperty> complexSons = source.Properties.ToList();

            //Filter to get new list
            List<ODataProperty> filteredSons = new List<ODataProperty>();
            foreach (ODataProperty prop in complexSons)
            {
                PropertyType retType = GetPropertyType(prop);
                switch (retType)
                {
                    case PropertyType.SimpleEdmx:
                        {
                            if (null != prop.Value)
                            {
                                if (prop.Value.GetType().Name == "Int32")
                                {
                                    //Check the value now.
                                    bool bInclude = false;
                                    try
                                    {
                                        //TODO: You cannot simply do this, potential bugs there maybe.
                                        //Use your own logics the determine if need to ignore ZEORs or not.
                                        int val = Convert.ToInt32(prop.Value);
                                        bInclude = (0 != val);
                                    }
                                    catch (Exception)
                                    {
                                    }

                                    if (bInclude)
                                        filteredSons.Add(prop);
                                }
                                else
                                    filteredSons.Add(prop);
                            }
                        }
                        break;

                    case PropertyType.ComplexType:
                        {
                            //Recursively
                            ODataComplexValue comx = RebuildComplexValue((ODataComplexValue)prop.Value);
                            if (comx.Properties.Count() > 0)
                            {
                                prop.Value = comx;
                                filteredSons.Add(prop);
                            }
                        }
                        break;

                    case PropertyType.Collection:
                        {
                            ODataCollectionValue coll = RebuildCollectionValue((ODataCollectionValue)prop.Value);
                            List<ODataComplexValue> listSubs = (List<ODataComplexValue>)coll.Items;
                            if (listSubs.Count > 0)
                            {
                                prop.Value = coll;
                                filteredSons.Add(prop);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            //Re-Assign sons
            newVal.Properties = filteredSons;

            return newVal;
        }

        /// <summary>
        /// Create new ODataCollectionValue from the old, to discard all null/empty values
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private ODataCollectionValue RebuildCollectionValue(ODataCollectionValue source)
        {
            ODataCollectionValue newVal = new ODataCollectionValue();
            newVal.TypeName = source.TypeName;

            List<ODataComplexValue> listComplexValues = new List<ODataComplexValue>();
            foreach (ODataComplexValue complex in source.Items)
            {
                ODataComplexValue comx = RebuildComplexValue(complex);
                listComplexValues.Add(comx);
            }

            newVal.Items = listComplexValues;

            return newVal;
        }

        /// <summary>
        /// Create new top level property set, to filter all empty collection, null/zero values if "bIgnore" is true.
        /// </summary>
        /// <param name="listSource"></param>
        /// <param name="bIgnore"></param>
        /// <returns></returns>
        private List<ODataProperty> FilterNullValues(List<ODataProperty> listSource, bool bIgnore = false)
        {
            List<ODataProperty> listResults = new List<ODataProperty>();

            if (bIgnore)
            {
                foreach (ODataProperty prop in listSource)
                {
                    PropertyType retType = GetPropertyType(prop);

                    switch (retType)
                    {
                        case PropertyType.SimpleEdmx:
                            {
                                if (null != prop.Value)
                                    listResults.Add(prop);
                            }
                            break;

                        case PropertyType.ComplexType:
                            {
                                ODataComplexValue complex = RebuildComplexValue((ODataComplexValue)prop.Value);
                                if (complex.Properties.Count() > 0)
                                {
                                    prop.Value = complex;
                                    listResults.Add(prop);
                                }
                            }
                            break;

                        case PropertyType.Collection:
                            {
                                ODataCollectionValue coll = RebuildCollectionValue((ODataCollectionValue)prop.Value);
                                List<ODataComplexValue> listSubs = (List<ODataComplexValue>)coll.Items;
                                if (listSubs.Count > 0)
                                {
                                    prop.Value = coll;
                                    listResults.Add(prop);
                                }
                            }
                            break;

                        default:
                            break;
                    }
                }
            }
            else
            {
                listResults.AddRange(listSource);
            }

            return listResults;
        }

        private static bool RemoteSSLTLSCertificateValidate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors ssl)
        {
            //accept
            return true;
        }

        /// <summary>
        /// Create new entity container for our business usage.
        /// </summary>
        /// <param name="strServerURL"></param>
        public void InitServiceContainer(string strServerURL)
        {
            //Cache any way, this one maybe redirected
            if (strServerURL.EndsWith("/"))
                strCurrentServerURL = strServerURL;
            else
                strCurrentServerURL = strServerURL + "/";

            if (null == currentServiceContainer)
            {
                Uri service = new Uri(strCurrentServerURL);
                currentServiceContainer = new ServiceLayer(service);
                if (null != currentServiceContainer)
                {
                    //Indicate the WCF to Json Format. Default is Atom.
                    currentServiceContainer.Format.UseJson();
                    currentServiceContainer.Format.UseJson();

                    //This flag : Seems not work
                    currentServiceContainer.IgnoreMissingProperties = true;

                    //Chance for us to filter propeties using our own logics.
                    currentServiceContainer.Configurations.RequestPipeline.OnEntryStarting((arg) =>
                    {
                        //For exam: Make all null properties [top level] under entity ignored
                        //arg.Entry.Properties = arg.Entry.Properties.Where((prop) => prop.Value != null);

                        if (!(arg.Entity is BusinessPartner))
                        {
                            //WCF & .NET rules:
                            //A. All value types are initialized with ZEROs, and reference types with null.
                            //B. For primitive types in WCF entities:
                            //==>non-nullable properties are raw types, nullable types are wrapped to be nullable reference types.
                            if (arg.Entity is Document)
                            {
                                Document doc = (Document)arg.Entity;
                                if (doc.DocEntry == 0)
                                    arg.Entry.Properties = FilterNullValues(arg.Entry.Properties.ToList(), true);
                            }
                            else
                                arg.Entry.Properties = FilterNullValues(arg.Entry.Properties.ToList(), true);
                        }
                    });

                    //Attach or revise the headers for carring the sesssion id, or set the paging size.
                    currentServiceContainer.SendingRequest2 += currentServiceContainer_SendingRequest;
                    currentServiceContainer.BuildingRequest += (sender, eventArgs) =>
                    {
                        eventArgs.RequestUri = new Uri(eventArgs.RequestUri.ToString().Replace("SAPB1.ServiceLayer.", ""));
                    };

                    currentServiceContainer.ReceivingResponse += currentServiceContainer_ReceivingResponse;

                    //work around WCF client cache mechanism.
                    currentServiceContainer.MergeOption = MergeOption.OverwriteChanges;

                    //SSL, TLS certificate
                    ServicePointManager.ServerCertificateValidationCallback += RemoteSSLTLSCertificateValidate;
                }
            }
        }

        /// <summary>
        /// Method being call for each response received from Service Layer
        /// Take care of B1SESSION and ROUTEID details
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void currentServiceContainer_ReceivingResponse(object sender, ReceivingResponseEventArgs e)
        {
            if (null == e.ResponseMessage)
                return;

            string strMessage = e.ResponseMessage.GetHeader("Set-Cookie");
            //Format of the Set-Cookie content in response of login action
            //B1SESSION=146eae44-fc3a-11e3-8000-047d7ba5aff2;HttpOnly;,ROUTEID=.node2; path=/b1s

            //Format of the cookie to be sent in request
            //Cookie: B1SESSION=57a86a60-fc3a-11e3-8000-047d7ba5aff2; ROUTEID=.node1

            if (false == string.IsNullOrEmpty(strMessage))
            {
                //The ROUTEID information will be returned during login, if sever is configured to be "Clustered" Mode.
                int idx = strMessage.IndexOf("ROUTEID=");
                if (idx > 0)
                {
                    string strSubString = strMessage.Substring(idx);
                    int idxSplitter = strSubString.IndexOf(";");
                    if (idxSplitter > 0)
                    {
                        strCurrentRouteIDString = strSubString.Substring(0, idxSplitter);
                    }
                    else
                    {
                        strCurrentRouteIDString = string.Empty;
                    }
                }
            }

            // Just for login
            BuildResponseStringContent(e.ResponseMessage);
        }

        /// <summary>
        /// Create Header
        /// </summary>
        /// <param name="strName"></param>
        /// <param name="strValue"></param>
        /// <returns></returns>
        private string CreateHeaderItem(string strName, string strValue)
        {
            string strFormat = "{0} : {1}\n";
            return string.Format(strFormat, strName, strValue);
        }

        /// <summary>
        /// Build the HTTP response headers to be string for checking usage, on GUI.
        /// </summary>
        /// <param name="response"></param>
        private void BuildResponseStringContent(IODataResponseMessage response)
        {
            sbHttpResponseHeaders.Clear();

            if (null != response)
            {
                sbHttpResponseHeaders.Append(CreateHeaderItem("StatusCode", response.StatusCode.ToString()));
                if (response.GetHeader("DataServiceVersion") != "")
                    sbHttpResponseHeaders.Append(CreateHeaderItem("DataServiceVersion", response.GetHeader("DataServiceVersion")));
                if (response.GetHeader("Date") != "")
                    sbHttpResponseHeaders.Append(CreateHeaderItem("Date", response.GetHeader("Date")));
                sbHttpResponseHeaders.Append("\n\n");
            }
        }

        /// <summary>
        /// Build the HTTP request headers to be string for checking usage, on GUI.
        /// </summary>
        /// <param name="request"></param>
        private void BuildRequestStringContent(HttpWebRequest request)
        {
            sbHttpRequestHeaders.Clear();

            if (null != request)
            {
                sbHttpRequestHeaders.Append(CreateHeaderItem("Method", request.Method.ToString()));
                //sbHttpRequestHeaders.Append(CreateHeaderItem("ServerURI", request.ServicePoint.Address.AbsoluteUri.ToString()));
                //sbHttpRequestHeaders.Append(CreateHeaderItem("Address", request.Address.ToString()));
                sbHttpRequestHeaders.Append(CreateHeaderItem("RequestUri", request.RequestUri.ToString()));
                sbHttpRequestHeaders.Append(CreateHeaderItem("Accept", request.Accept));
                sbHttpRequestHeaders.Append(CreateHeaderItem("Keep-Alive", request.KeepAlive.ToString()));
                sbHttpRequestHeaders.Append(CreateHeaderItem("ContentType", request.ContentType));
                sbHttpRequestHeaders.Append(CreateHeaderItem("ContentLength", request.ContentLength.ToString()));
                sbHttpRequestHeaders.Append(CreateHeaderItem("Connection", request.Connection));
                sbHttpRequestHeaders.Append(CreateHeaderItem("UserAgent", request.UserAgent));
                sbHttpRequestHeaders.Append(CreateHeaderItem("Timeout", request.Timeout.ToString()));
                sbHttpRequestHeaders.Append(CreateHeaderItem("ProtocolVersion", request.ProtocolVersion.ToString()));

                sbHttpRequestHeaders.Append(CreateHeaderItem("Cookie", request.Headers["Cookie"]));
                sbHttpRequestHeaders.Append(CreateHeaderItem("Prefer", request.Headers["Prefer"]));

                sbHttpRequestHeaders.Append("\n\n");
            }
        }

        /// <summary>
        /// Get request header
        /// </summary>
        /// <returns></returns>
        public string GetRequestHeaders()
        {
            return sbHttpRequestHeaders.ToString();
        }

        /// <summary>
        /// Get response header
        /// </summary>
        /// <returns></returns>
        public string GetResponsetHeaders()
        {
            return sbHttpResponseHeaders.ToString();
        }

        /// <summary>
        /// Method called before each request is sent to Service Layer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void currentServiceContainer_SendingRequest(object sender, System.Data.Services.Client.SendingRequest2EventArgs e)
        {

            HttpWebRequestMessage request = (HttpWebRequestMessage)e.RequestMessage;

            if (null != request)
            {
                request.HttpWebRequest.Accept = "application/json;odata=minimalmetadata";
                request.HttpWebRequest.KeepAlive = true;                               //keep alive
                request.HttpWebRequest.ServicePoint.Expect100Continue = false;        //content
                request.HttpWebRequest.AllowAutoRedirect = true;
                request.HttpWebRequest.ContentType = "application/json;odata=minimalmetadata;charset=utf8";
                request.Timeout = 10000000;    //number of seconds before considering a request as timeout (consider to change it for batch operations)

                //This way works to bring additional information with request headers
                if (false == string.IsNullOrEmpty(strCurrentSessionGUID))
                {
                    string strB1Session = "B1SESSION=" + strCurrentSessionGUID;
                    if (!string.IsNullOrEmpty(strCurrentRouteIDString))
                        strB1Session += "; " + strCurrentRouteIDString;

                    e.RequestMessage.SetHeader("Cookie", strB1Session);
                }

                //Only works for get requests, but we can always use this, even it will be ignored by other request types.
                e.RequestMessage.SetHeader("Prefer", "odata.maxpagesize=" + currentDefaultPagingSizing.ToString());


                //For GUI, non-functional
                //BuildRequestStringContent(request);
            }
            else
                throw new Exception("Failed to intercept the sending request");
        }

        /// <summary>
        /// Empty next link value
        /// </summary>
        public void ReinitPages()
        {
            currentPage = -1;
            pageLinksList = new Hashtable();
        }

        public int GetCurrentPage()
        {
            return currentPage;
        }

        /// <summary>
        /// Change page sizing 
        /// </summary>
        /// <param name="pgSize"></param>
        public void SetPagingSize(int pgSize)
        {
            currentDefaultPagingSizing = pgSize;
        }

        public string getSession()
        {
            return strCurrentSessionGUID;
        }

        #region Get User
        public User GetUser(int intkey)
        {
            try
            {
                User tm = currentServiceContainer.Users.Where(cursor => cursor.InternalKey == intkey).ToList()[0];
                return tm;
            }
            catch (Exception ex)
            {
                log.Error("Error in GetUser " + ex.Message + ex.Message != null ? ex.InnerException.Message : "");
                throw ex;
            }

        }
        #endregion Get User

        #region Update User
        public User UpdateUser(User doc)
        {
            try
            {
                foreach (UserActionRecordItem user in doc.UserActionRecord.ToList())
                {
                    doc.UserActionRecord.Remove(user);
                }

                currentServiceContainer.UpdateObject(doc);
                SaveChangesOptions updateSemantics = SaveChangesOptions.PatchOnUpdate;

                currentServiceContainer.SaveChanges(updateSemantics);
                return doc;
            }
            catch (Exception ex)
            {

                if (null != doc)
                    currentServiceContainer.Detach(doc);
                throw ex;
            }
        }
        #endregion Update User

        #region LoginServer
        public B1Session LoginServer(SboCred cred)
        {
            B1Session session = null;
            try
            {
                //Discard last login information
                strCurrentSessionGUID = string.Empty;

                Uri login = new Uri(strCurrentServerURL + "Login");
                BodyOperationParameter[] body = new BodyOperationParameter[3];
                body[0] = new BodyOperationParameter("UserName", cred.UserName);
                body[1] = new BodyOperationParameter("Password", cred.Password);
                body[2] = new BodyOperationParameter("CompanyDB", cred.CompanyDB);

                session = (B1Session)currentServiceContainer.Execute<B1Session>(login, "POST", true, body).SingleOrDefault();
                if (null != session)
                {
                    strCurrentSessionGUID = session.SessionId;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error in LoginServer: " + Program.GetFullMessage(ex));
                strCurrentSessionGUID = string.Empty;
                throw ex;
            }
            return session;
        }
        #endregion LoginServer

        #region LogoutServer
        public void LogoutServer()
        {
            if (!string.IsNullOrEmpty(strCurrentSessionGUID))
            {
                try
                {
                    string strRequstCmd = strCurrentServerURL + "Logout";
                    Uri cmdQuery = new Uri(strRequstCmd);

                    string strCommandType = "POST";

                    UriOperationParameter[] queryOptions = null;

                    currentServiceContainer.Execute(cmdQuery, strCommandType, queryOptions);

                    strCurrentSessionGUID = string.Empty;
                }
                catch (Exception ex)
                {
                    log.Error("Error in LogoutServer: " + Program.GetFullMessage(ex));
                }
            }
        }
        #endregion LogoutServer

        #region Add SpecialPrice

        public string AddSpecialPrice(SpecialPrice oDoc)
        {
            string Msg = "";
            try
            {
                var uri = BPSpecialPrice.ServiceLayerUrl.ToString() + "SpecialPrices";

                var setting = new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                };

                var json = JsonConvert.SerializeObject(oDoc, setting);

                var cookieContainer = new CookieContainer();
                cookieContainer.Add(new Uri(uri), new Cookie("B1SESSION", strCurrentSessionGUID));
                using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
                {
                    using (HttpClient Client = new HttpClient(handler))
                    {
                        var req = new HttpRequestMessage(new HttpMethod("POST"), uri)
                        {
                            Content = new StringContent(json)
                        };
                        req.Headers.ExpectContinue = false;
                        HttpResponseMessage response = Client.SendAsync(req).Result;
                        JObject RespObj = (JObject)JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);

                        if (RespObj != null)
                        {
                            if (response.StatusCode.ToString() == "Created")
                            {
                                Msg = "Created";
                            }
                            else
                            {
                                string RespErr = JObject.Parse(RespObj.ToString())["error"].ToString();
                                string Code = JObject.Parse(RespErr)["code"].ToString();
                                string ErrMsg = JObject.Parse(JObject.Parse(RespErr)["message"].ToString())["value"].ToString();

                                Msg = "Code : " + Code + " Msg = : " + ErrMsg;
                            }
                        }
                        else
                        {
                            Msg = response.ReasonPhrase;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error in AddSpecialPrice " + Program.GetFullMessage(ex));
                if (null != oDoc)
                    currentServiceContainer.Detach(oDoc);
                throw ex;
            }
            return Msg;
        }

        #endregion Add SpecialPrice

        #region GET SpecialPrice

        public SpecialPrice GetSpecialPrice(string CardCode, string ItemCode)
        {
            SpecialPrice SP = null;
            try
            {
                SP = currentServiceContainer.SpecialPrices.Where(cursor => cursor.CardCode == CardCode && cursor.ItemCode == ItemCode).SingleOrDefault();
            }
            catch (Exception ex)
            {
                if (BPSpecialPrice.AdvanceDebug)
                    log.Error("Error in GetSpecialPrice " + Program.GetFullMessage(ex));
                throw ex;
            }
            return SP;
        }

        #endregion GET SpecialPrice

        #region Update SpecialPrice

        public string UpdateSpecialPrice(SpecialPrice SP)
        {
            string json = null;
            try
            {
                Error mg = new Error();
                string msg = "Updated";
                var uri = BPSpecialPrice.ServiceLayerUrl.ToString();
                var setting = new JsonSerializerSettings()
                {
                    NullValueHandling =
                        NullValueHandling.Ignore
                };
                json = JsonConvert.SerializeObject(SP, setting);
                var cookieContainer = new CookieContainer();

                using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })

                using (HttpClient client = new HttpClient(handler))
                {
                    var req = new HttpRequestMessage(new HttpMethod("PATCH"), uri + "SpecialPrices(CardCode = '" + SP.CardCode + "', ItemCode = '" + SP.ItemCode + "')")
                    {
                        Content = new StringContent(json)
                    };
                    req.Headers.ExpectContinue = false;
                    var conte1 = new StringContent(json);
                    cookieContainer.Add(new Uri(uri), new Cookie("B1SESSION", strCurrentSessionGUID));
                    req.Headers.Add("B1S-ReplaceCollectionsOnPatch", "true");
                    var res = client.SendAsync(req).Result;

                    JObject test = (JObject)JsonConvert.DeserializeObject(res.Content.ReadAsStringAsync().Result);
                    if (test != null)
                    {
                        var respMsg = JObject.Parse(JObject.Parse(JObject.Parse(test.ToString())["error"].ToString())["message"].ToString())["value"].ToString();

                        if (respMsg != null && respMsg != "")
                        {
                            if (respMsg.Contains("-"))
                            {
                                mg.msg = respMsg.Remove(0, respMsg.IndexOf("-") + 1);
                            }
                            else
                            {
                                mg.msg = respMsg;
                            }
                        }
                        else
                        {
                            mg.msg = test.ToString(Formatting.None);
                        }
                        return mg.msg;
                    }
                    return msg;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error in UpdateSpecialPrice : " + Program.GetFullMessage(ex));
                log.Error("json " + json);
                if (null != SP)
                    currentServiceContainer.Detach(SP);
                throw ex;
            }
        }

        #endregion Update SpecialPrice

        #region Delete SpecialPrice

        public string DeleteSpecialPrice(SpecialPrice SP)
        {
            Error mg = new Error();
            string msg = "Deleted";
            try
            {
                var uri = BPSpecialPrice.ServiceLayerUrl.ToString();
                var setting = new JsonSerializerSettings()
                {
                    NullValueHandling =
                        NullValueHandling.Ignore
                };
                var cookieContainer = new CookieContainer();

                using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })

                using (HttpClient client = new HttpClient(handler))
                {
                    var req = new HttpRequestMessage(new HttpMethod("DELETE"), uri + "SpecialPrices(CardCode = '" + SP.CardCode + "', ItemCode = '" + SP.ItemCode + "')");
                    req.Headers.ExpectContinue = false;
                    cookieContainer.Add(new Uri(uri), new Cookie("B1SESSION", strCurrentSessionGUID));
                    var res = client.SendAsync(req).Result;

                    JObject test = (JObject)JsonConvert.DeserializeObject(res.Content.ReadAsStringAsync().Result);
                    if (test != null)
                    {
                        var respMsg = JObject.Parse(JObject.Parse(JObject.Parse(test.ToString())["error"].ToString())["message"].ToString())["value"].ToString();

                        if (respMsg != null && respMsg != "")
                        {
                            if (respMsg.Contains("-"))
                            {
                                mg.msg = respMsg.Remove(0, respMsg.IndexOf("-") + 1);
                            }
                            else
                            {
                                mg.msg = respMsg;
                            }
                        }
                        else
                        {
                            mg.msg = test.ToString(Formatting.None);
                        }
                        return mg.msg;
                    }
                    return msg;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error in DeleteSpecialPrice " + Program.GetFullMessage(ex));
                throw ex;
            }
        }

        #endregion Delete SpecialPrice

        #region Error

        public class Error
        {
            public int code { get; set; }
            public Message message { get; set; }
            public string msg { get; set; }
        }

        #endregion Error

    }
}
