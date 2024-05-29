using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Management;

namespace DisableContextMenuItems
{
    public partial class frmMain : DisableContextMenuItems.CustomForm
    {
        public static frmMain Instance = null;

        public frmMain()
        {
            string requirepass = RegistryHelper.GetAppRegistryKeyValue("RequirePassword", true);
            
   

            if (requirepass != null && XOREncrypter.EncryptDecrypt(requirepass).StartsWith("True"))
            {
                frmEncryptionPassword f = new frmEncryptionPassword();

                if (f.ShowDialog() != DialogResult.OK)
                {
                    Environment.Exit(0);
                }

            }

            InitializeComponent();

            Instance = this;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            SetupOnLoad();

            if (Properties.Settings.Default.CheckWeek)
            {
                UpdateHelper.InitializeCheckVersionWeek();
            }

            ResizeControls();
        }

        private bool InSetupOnLoad = false;
        
        private void SetupOnLoad()
        {
            try
            {
                InSetupOnLoad = true;

                this.Text = Module.ApplicationTitle + " - " + TranslateHelper.Translate("Config Screen");

                GetListOfUsers();

                lstUsers.Items.Clear();

                for (int k = 0; k < lUsers.Count; k++)
                {
                    lstUsers.Items.Add(lUsers[k]);
                }

                RepositionResize();



                DownloadSuggestionsHelper ds = new DownloadSuggestionsHelper();
                ds.SetupDownloadMenuItems(tsiDownload);

                string reqpas = RegistryHelper.GetAppRegistryKeyValue("RequirePassword", true);


                if (reqpas != null)
                {
                    reqpas = XOREncrypter.EncryptDecrypt(reqpas);

                    if (reqpas.StartsWith("True"))
                    {
                        requirePasswordToolStripMenuItem.Checked = true;
                        changePasswordToolStripMenuItem.Enabled = true;
                    }
                    else
                    {
                        changePasswordToolStripMenuItem.Enabled = false;
                    }
                }

                chkDoubleClickOpen.Checked = Properties.Settings.Default.SetDoubleClick;

                checkForNewVersionEachWeekToolStripMenuItem.Checked = Properties.Settings.Default.CheckWeek;
            }
            finally
            {
                InSetupOnLoad = false;
            }
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            for (int k = 0; k < lstUsers.Items.Count; k++)
            {
                lstUsers.SetItemChecked(k, true);
            }
        }

        private void btnUnselectAll_Click(object sender, EventArgs e)
        {
            for (int k = 0; k < lstUsers.Items.Count; k++)
            {
                lstUsers.SetItemChecked(k, false);
            }
        }

        private void btnInvertSelection_Click(object sender, EventArgs e)
        {
            for (int k = 0; k < lstUsers.Items.Count; k++)
            {
                lstUsers.SetItemChecked(k,!lstUsers.GetItemChecked(k));
            }
        }

        private static List<string> lUsers = new List<string>();
        private static List<string> lSID=new List<string>();

        public static void GetListOfUsers()
        {
            string str = "";

            try
            {
                lUsers.Clear();
                lSID.Clear();

                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_UserAccount");// where name='Galia'");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    Console.WriteLine("-----------------------------------");
                    Console.WriteLine("Win32_UserAccount instance");
                    Console.WriteLine("-----------------------------------");
                    Console.WriteLine("Name: {0}", queryObj["Name"]);
                    Console.WriteLine("SID: {0}", queryObj["SID"]);

                    lUsers.Add(queryObj["Name"].ToString());
                    lSID.Add(queryObj["SID"].ToString());
                }
            }
            catch (ManagementException e)
            {
                MessageBox.Show("An error occurred while querying for WMI data: " + e.Message);
            }
        }

        private void btnAddCurrentUser_Click(object sender, EventArgs e)
        {
            for (int k = 0; k < lstUsers.Items.Count; k++)
            {
                if (lstUsers.Items[k].ToString() == Environment.UserName)
                {
                    lstUsers.SetItemChecked(k, true);

                    break;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnSelAll_Click(object sender, EventArgs e)
        {
            chkCut.Checked = true;
            chkCopy.Checked = true;
            chkDelete.Checked = true;
            chkRename.Checked = true;
            chkProperties.Checked = true;
        }

        private void btnUnSelAll_Click(object sender, EventArgs e)
        {
            chkCut.Checked = false;
            chkCopy.Checked = false;
            chkDelete.Checked = false;
            chkRename.Checked = false;
            chkProperties.Checked = false;
        }

        private void btnInvSel_Click(object sender, EventArgs e)
        {
            chkCut.Checked = !chkCut.Checked;
            chkCopy.Checked = !chkCopy.Checked;
            chkDelete.Checked = !chkDelete.Checked;
            chkRename.Checked = !chkRename.Checked;
            chkProperties.Checked = !chkProperties.Checked;
        }

        private void chkSelectUsers_CheckedChanged(object sender, EventArgs e)
        {
            groupBox1.Enabled = chkSelectUsers.Checked;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string cm = "";

            if (chkCopy.Checked) cm += "Copy";
            if (chkCut.Checked) cm += "Cut";
            if (chkDelete.Checked) cm += "Delete";
            if (chkProperties.Checked) cm += "Properties";
            if (chkRename.Checked) cm += "Rename";

            string users = "";

            if (chkAllUsers.Checked)
            {
                users = "|||All Users|||";
            }
            else if (chkCurrentUser.Checked)
            {
                users = "|||" + Environment.UserName + "|||";
            }
            else
            {
                users="|||";

                for (int k = 0; k < lstUsers.Items.Count; k++)
                {
                    if (lstUsers.GetItemChecked(k))
                    {
                        users += lstUsers.Items[k].ToString() + "|||";
                    }
                }
            }

            string dbl = "None";

            if (Properties.Settings.Default.SetDoubleClick != chkDoubleClickOpen.Checked)
            {
                if (chkDoubleClickOpen.Checked)
                {
                    dbl = bool.TrueString;
                }
                else
                {
                    dbl = bool.FalseString;
                }
            }

            bool suc=Module.RunAdminAction("-rightclickdisabler \"" + cm + "\" \"" + users + "\" \"" + dbl + "\"");

            if (!suc)
            {
                Module.ShowMessage("Error !");
            }
            else
            {
                Properties.Settings.Default.SetDoubleClick = chkDoubleClickOpen.Checked;
                Properties.Settings.Default.Save();

                Module.ShowMessage("Operation completed successfully !");
            }

           
        }

        #region Help

        private void helpUsersManualToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //System.Diagnostics.Process.Start(Application.StartupPath + "\\Video Cutter Joiner Expert - User's Manual.chm");
            System.Diagnostics.Process.Start(Module.HelpURL);
        }

        private void pleaseDonateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.4dots-software.com/donate.php");
        }


        private void dotsSoftwarePRODUCTCATALOGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.4dots-software.com/downloads/4dots-Software-PRODUCT-CATALOG.pdf");
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAbout f = new frmAbout();
            f.ShowDialog();
        }

        private void feedbackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*
            frmUninstallQuestionnaire f = new frmUninstallQuestionnaire(false);
            f.ShowDialog();
            */

            System.Diagnostics.Process.Start("https://www.4dots-software.com/support/bugfeature.php?app=" + System.Web.HttpUtility.UrlEncode(Module.ShortApplicationTitle));
        }

        private void followUsOnTwitterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.twitter.com/4dotsSoftware");
        }

        private void visit4dotsSoftwareWebpageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.4dots-software.com");
        }

        private void checkForNewVersionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Module.CheckVersion(false);
            UpdateHelper.CheckVersion(false);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Application.Exit();
            }
            catch { }
        }

        #endregion


        #region Share

        private void shareOnFacebookToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShareHelper.ShareFacebook();
        }

        private void shareOnTwitterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShareHelper.ShareTwitter();
        }

        private void shareOnGooglePlusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShareHelper.ShareGooglePlus();
        }

        private void shareOnLinkedinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShareHelper.ShareLinkedIn();
        }

        private void shareWithEmailToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShareHelper.ShareEmail();
        }

        #endregion        

        #region Localization
      

        
                
        #endregion

        private void SavePositionSize()
        {
            Properties.Settings.Default.Maximized = (this.WindowState == FormWindowState.Maximized);
            Properties.Settings.Default.Left = this.Left;
            Properties.Settings.Default.Top = this.Top;
            Properties.Settings.Default.Width = this.Width;
            Properties.Settings.Default.Height = this.Height;
            Properties.Settings.Default.Save();
        }

        private void RepositionResize()
        {
            if (Properties.Settings.Default.Maximized)
            {
                this.WindowState = FormWindowState.Maximized;
            }
            else
            {

                if (Properties.Settings.Default.Width != -1)
                {
                    this.Width = Properties.Settings.Default.Width;
                }


                if (Properties.Settings.Default.Height != -1)
                {
                    this.Height = Properties.Settings.Default.Height;
                }

                if (Properties.Settings.Default.Left != -1)
                {
                    this.Left = Properties.Settings.Default.Left;
                }

                if (Properties.Settings.Default.Top != -1)
                {
                    this.Top = Properties.Settings.Default.Top;
                }
            }

            if (this.Width < 300)
            {
                this.Width = 300;
            }

            if (this.Height < 300)
            {
                this.Height = 300;
            }

            if (this.Top < 0)
            {
                this.Top = 0;
            }

            if (this.Left < 0)
            {
                this.Left = 0;
            }

            this.Show();
            this.BringToFront();
            this.Visible = true;
        }

        private void requirePasswordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!InSetupOnLoad)
            {
                bool setpass = false;

                if (!requirePasswordToolStripMenuItem.Checked)
                {
                    string oldpass = RegistryHelper.GetAppRegistryKeyValue("Password", true);

                    if (oldpass == null)
                    {
                        setpass = true;
                    }
                }

                if (setpass)
                {
                    frmChangePassword f = new frmChangePassword(true);

                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        if (RegistryHelper.SetAppRegistryKeyValue("RequirePassword", XOREncrypter.EncryptDecrypt((!requirePasswordToolStripMenuItem.Checked).ToString() + DateTime.Now.ToString()), true))
                        {
                            requirePasswordToolStripMenuItem.Checked = !requirePasswordToolStripMenuItem.Checked;
                        }
                    }
                }
                else
                {
                    if (RegistryHelper.SetAppRegistryKeyValue("RequirePassword", XOREncrypter.EncryptDecrypt((!requirePasswordToolStripMenuItem.Checked).ToString() + DateTime.Now.ToString()), true))
                    {
                        requirePasswordToolStripMenuItem.Checked = !requirePasswordToolStripMenuItem.Checked;
                    }
                }
            }

            changePasswordToolStripMenuItem.Enabled = requirePasswordToolStripMenuItem.Checked;
        }

        private void changePasswordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmChangePassword f = new frmChangePassword(false);
            f.ShowDialog(this);
        }        

        private void buyApplicationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Module.BuyURL);
        }

        private void enterLicenseKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        protected override void WndProc(ref Message m)
        {            
            if (m.Msg == MessageHelper.WM_COPYDATA)
            {
                MessageHelper.COPYDATASTRUCT mystr = new MessageHelper.COPYDATASTRUCT();
                Type mytype = mystr.GetType();
                mystr = (MessageHelper.COPYDATASTRUCT)m.GetLParam(mytype);

                string arg = mystr.lpData;

                if (arg == "SHOW")
                {
                    this.Show();
                    this.WindowState = FormWindowState.Normal;
                    this.Show();
                    this.BringToFront();
                }
            }
            else if (m.Msg == MessageHelper.WM_ACTIVATEAPP)
            {
                this.Show();

                this.WindowState = FormWindowState.Normal;
                this.Show();
                this.BringToFront();
            }
            else
            {
                base.WndProc(ref m);
            }

        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.CheckWeek = checkForNewVersionEachWeekToolStripMenuItem.Checked;

            Properties.Settings.Default.Save();
        }
    }
}
