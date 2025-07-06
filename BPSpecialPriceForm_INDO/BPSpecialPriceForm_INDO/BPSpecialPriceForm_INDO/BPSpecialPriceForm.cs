using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BPSpecialPriceForm_INDO
{
    public partial class BPSpecialPriceForm : Form
    {
        public BPSpecialPriceForm()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            
        }

        private void Btn_Save_Click(object sender, EventArgs e)
        {
            if (Et_SqlServer.Text.Trim() == "")
            {
                MessageBox.Show("Please enter the SQL ServerName");
                return;
            }
            else if (Et_SqlUser.Text.Trim() == "")
            {
                MessageBox.Show("Please enter the SQL UserName");
                return;
            }
            else if (Et_SqlPwd.Text.Trim() == "")
            {
                MessageBox.Show("Please enter the SQL Password");
                return;
            }
            else if (Cmb_SQLServerType.Text.Trim() == "")
            {
                MessageBox.Show("Please Choose the SQL Server Type");
                return;
            }
            else if (Et_ServiceUrl.Text.Trim() == "")
            {
                MessageBox.Show("Please enter the ServiceLayer EndPoint Url");
                return;
            }
            else if (Et_DBName.Text.Trim() == "")
            {
                MessageBox.Show("Please enter the SAP DBName");
                return;
            }
            else if (Et_DBUser.Text.Trim() == "")
            {
                MessageBox.Show("Please enter the SAP UserName");
                return;
            }
            else if (Et_DBPwd.Text.Trim() == "")
            {
                MessageBox.Show("Please enter the SAP Password");
                return;
            }
            else if (Et_LicenseServer.Text.Trim() == "")
            {
                MessageBox.Show("Please enter the License Server");
                return;
            }

            CredentialModel oSQLModel = new CredentialModel();

            oSQLModel.SQLServerName = Et_SqlServer.Text.Trim();
            oSQLModel.SQLUserName = Et_SqlUser.Text.Trim();
            oSQLModel.SQLPassword = Cls_Encrypt_Helper.Encrypt(Et_SqlPwd.Text.Trim());
            oSQLModel.SQLServerType = Cmb_SQLServerType.Text.Trim();
            oSQLModel.FolderType = Cb_FolderType.SelectedText.Trim() == "NormalPath" ? "N" : "S";
            oSQLModel.ServiceLayerUrl = Et_ServiceUrl.Text.Trim();
            oSQLModel.SAPDBName = Et_DBName.Text.Trim();
            oSQLModel.SAPUserName = Et_DBUser.Text.Trim();
            oSQLModel.SAPPassword = Cls_Encrypt_Helper.Encrypt(Et_DBPwd.Text.Trim());
            oSQLModel.LicenseServer = Et_LicenseServer.Text.Trim();

            try
            {
                System.IO.File.WriteAllText("Credentials.json", JsonConvert.SerializeObject(oSQLModel));
                MessageBox.Show("Credentials Validated Successfully");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Btn_Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BPSpecialPriceForm_Load(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists("Credentials.json"))
                {
                    using (StreamReader oReader = new StreamReader("Credentials.json"))
                    {
                        CredentialModel oSQLModel = JsonConvert.DeserializeObject<CredentialModel>(oReader.ReadToEnd());

                        Et_SqlServer.Text = oSQLModel.SQLServerName.Trim();
                        Et_SqlUser.Text = oSQLModel.SQLUserName.Trim();
                        Et_SqlPwd.Text = Cls_Encrypt_Helper.Decrypt(oSQLModel.SQLPassword.Trim());
                        
                        Cb_FolderType.Text = oSQLModel.FolderType.Trim() == "N" ? "NormalPath" : "SharedPath";
                        Et_ServiceUrl.Text = oSQLModel.ServiceLayerUrl.Trim();
                        Et_DBName.Text = oSQLModel.SAPDBName.Trim();
                        Et_DBUser.Text = oSQLModel.SAPUserName.Trim();
                        Et_DBPwd.Text = Cls_Encrypt_Helper.Decrypt(oSQLModel.SAPPassword.Trim());
                        Cmb_SQLServerType.Text = oSQLModel.SQLServerType.Trim();
                        Et_LicenseServer.Text = oSQLModel.LicenseServer.Trim();
                    }
                }
                else
                {
                    Cb_FolderType.Text = "SharedPath";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
