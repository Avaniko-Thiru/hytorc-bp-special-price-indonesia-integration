namespace BPSpecialPriceForm_INDO
{
    partial class BPSpecialPriceForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.Et_SqlServer = new System.Windows.Forms.TextBox();
            this.Et_SqlUser = new System.Windows.Forms.TextBox();
            this.Et_SqlPwd = new System.Windows.Forms.TextBox();
            this.Et_ServiceUrl = new System.Windows.Forms.TextBox();
            this.Et_DBName = new System.Windows.Forms.TextBox();
            this.Et_DBUser = new System.Windows.Forms.TextBox();
            this.Et_DBPwd = new System.Windows.Forms.TextBox();
            this.Cb_FolderType = new System.Windows.Forms.ComboBox();
            this.Btn_Save = new System.Windows.Forms.Button();
            this.Btn_Cancel = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.Et_LicenseServer = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.Cmb_SQLServerType = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "SQL Credentials";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(13, 121);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "SAP Credentials";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(33, 38);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(90, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "SQL ServerName";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(33, 60);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(81, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "SQL UserName";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(33, 82);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "SQL Password";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(33, 179);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(118, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Service Layer EndPoint";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(33, 201);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(74, 13);
            this.label8.TabIndex = 0;
            this.label8.Text = "SAP DBName";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(33, 223);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(81, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "SAP UserName";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(33, 245);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(77, 13);
            this.label10.TabIndex = 0;
            this.label10.Text = "SAP Password";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(33, 267);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(63, 13);
            this.label11.TabIndex = 0;
            this.label11.Text = "Folder Type";
            // 
            // Et_SqlServer
            // 
            this.Et_SqlServer.Location = new System.Drawing.Point(173, 38);
            this.Et_SqlServer.Name = "Et_SqlServer";
            this.Et_SqlServer.Size = new System.Drawing.Size(222, 20);
            this.Et_SqlServer.TabIndex = 1;
            // 
            // Et_SqlUser
            // 
            this.Et_SqlUser.Location = new System.Drawing.Point(173, 60);
            this.Et_SqlUser.Name = "Et_SqlUser";
            this.Et_SqlUser.Size = new System.Drawing.Size(222, 20);
            this.Et_SqlUser.TabIndex = 2;
            // 
            // Et_SqlPwd
            // 
            this.Et_SqlPwd.Location = new System.Drawing.Point(173, 82);
            this.Et_SqlPwd.Name = "Et_SqlPwd";
            this.Et_SqlPwd.Size = new System.Drawing.Size(222, 20);
            this.Et_SqlPwd.TabIndex = 3;
            this.Et_SqlPwd.UseSystemPasswordChar = true;
            // 
            // Et_ServiceUrl
            // 
            this.Et_ServiceUrl.Location = new System.Drawing.Point(176, 179);
            this.Et_ServiceUrl.Name = "Et_ServiceUrl";
            this.Et_ServiceUrl.Size = new System.Drawing.Size(222, 20);
            this.Et_ServiceUrl.TabIndex = 5;
            // 
            // Et_DBName
            // 
            this.Et_DBName.Location = new System.Drawing.Point(176, 201);
            this.Et_DBName.Name = "Et_DBName";
            this.Et_DBName.Size = new System.Drawing.Size(222, 20);
            this.Et_DBName.TabIndex = 6;
            // 
            // Et_DBUser
            // 
            this.Et_DBUser.Location = new System.Drawing.Point(176, 223);
            this.Et_DBUser.Name = "Et_DBUser";
            this.Et_DBUser.Size = new System.Drawing.Size(222, 20);
            this.Et_DBUser.TabIndex = 7;
            // 
            // Et_DBPwd
            // 
            this.Et_DBPwd.Location = new System.Drawing.Point(176, 245);
            this.Et_DBPwd.Name = "Et_DBPwd";
            this.Et_DBPwd.Size = new System.Drawing.Size(222, 20);
            this.Et_DBPwd.TabIndex = 8;
            this.Et_DBPwd.UseSystemPasswordChar = true;
            // 
            // Cb_FolderType
            // 
            this.Cb_FolderType.FormattingEnabled = true;
            this.Cb_FolderType.Items.AddRange(new object[] {
            "NormalPath",
            "SharedPath"});
            this.Cb_FolderType.Location = new System.Drawing.Point(176, 267);
            this.Cb_FolderType.Name = "Cb_FolderType";
            this.Cb_FolderType.Size = new System.Drawing.Size(222, 21);
            this.Cb_FolderType.TabIndex = 9;
            // 
            // Btn_Save
            // 
            this.Btn_Save.Location = new System.Drawing.Point(16, 322);
            this.Btn_Save.Name = "Btn_Save";
            this.Btn_Save.Size = new System.Drawing.Size(75, 23);
            this.Btn_Save.TabIndex = 25;
            this.Btn_Save.Text = "Save";
            this.Btn_Save.UseVisualStyleBackColor = true;
            this.Btn_Save.Click += new System.EventHandler(this.Btn_Save_Click);
            // 
            // Btn_Cancel
            // 
            this.Btn_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Btn_Cancel.Location = new System.Drawing.Point(97, 322);
            this.Btn_Cancel.Name = "Btn_Cancel";
            this.Btn_Cancel.Size = new System.Drawing.Size(75, 23);
            this.Btn_Cancel.TabIndex = 26;
            this.Btn_Cancel.Text = "Cancel";
            this.Btn_Cancel.UseVisualStyleBackColor = true;
            this.Btn_Cancel.Click += new System.EventHandler(this.Btn_Cancel_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(33, 289);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "License Server";
            // 
            // Et_LicenseServer
            // 
            this.Et_LicenseServer.Location = new System.Drawing.Point(176, 289);
            this.Et_LicenseServer.Name = "Et_LicenseServer";
            this.Et_LicenseServer.Size = new System.Drawing.Size(222, 20);
            this.Et_LicenseServer.TabIndex = 10;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(33, 157);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(89, 13);
            this.label12.TabIndex = 0;
            this.label12.Text = "SQL Server Type";
            // 
            // Cmb_SQLServerType
            // 
            this.Cmb_SQLServerType.FormattingEnabled = true;
            this.Cmb_SQLServerType.Items.AddRange(new object[] {
            "MSSQL",
            "MSSQL2005",
            "MSSQL2008",
            "MSSQL2012",
            "MSSQL2014",
            "MSSQL2016",
            "MSSQL2017",
            "MSSQL2019"});
            this.Cmb_SQLServerType.Location = new System.Drawing.Point(176, 157);
            this.Cmb_SQLServerType.Name = "Cmb_SQLServerType";
            this.Cmb_SQLServerType.Size = new System.Drawing.Size(222, 21);
            this.Cmb_SQLServerType.TabIndex = 4;
            // 
            // BPSpecialPriceForm
            // 
            this.AcceptButton = this.Btn_Save;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Btn_Cancel;
            this.ClientSize = new System.Drawing.Size(577, 390);
            this.Controls.Add(this.Cmb_SQLServerType);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.Et_LicenseServer);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Btn_Cancel);
            this.Controls.Add(this.Btn_Save);
            this.Controls.Add(this.Cb_FolderType);
            this.Controls.Add(this.Et_DBPwd);
            this.Controls.Add(this.Et_DBUser);
            this.Controls.Add(this.Et_DBName);
            this.Controls.Add(this.Et_ServiceUrl);
            this.Controls.Add(this.Et_SqlPwd);
            this.Controls.Add(this.Et_SqlUser);
            this.Controls.Add(this.Et_SqlServer);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "BPSpecialPriceForm";
            this.Text = "BP Special Price Setup";
            this.Load += new System.EventHandler(this.BPSpecialPriceForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox Et_SqlServer;
        private System.Windows.Forms.TextBox Et_SqlUser;
        private System.Windows.Forms.TextBox Et_SqlPwd;
        private System.Windows.Forms.TextBox Et_ServiceUrl;
        private System.Windows.Forms.TextBox Et_DBName;
        private System.Windows.Forms.TextBox Et_DBUser;
        private System.Windows.Forms.TextBox Et_DBPwd;
        private System.Windows.Forms.ComboBox Cb_FolderType;
        private System.Windows.Forms.Button Btn_Save;
        private System.Windows.Forms.Button Btn_Cancel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox Et_LicenseServer;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ComboBox Cmb_SQLServerType;
    }
}

