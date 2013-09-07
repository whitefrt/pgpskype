/************************************************************************/
/* pgpSkype                                                             */
/************************************************************************/
// copyright (c) 2013 white_frt
// public domain

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace pgpskype
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            // Set us
            this.Text = Program.strPGPSkype;// (Program.m_localUserName != null) ? Program.m_localUserName : "DefaultUser";
            this.OnlineListBox.MouseDoubleClick += new MouseEventHandler(OnlineListBox_DoubleClick);
            this.Resize += new EventHandler(MainForm_Resize);

            this.notifyIcon.Visible = false;
            this.notifyIcon.Text = Program.strPGPSkype;
            AddOnlineUsers();

            // Focus
            this.Show();
            this.Focus();
        }

        public class LBItem
        {
            public string m_strDisplay;
            public string m_strHandle;
            public string DisplayName { get { return m_strDisplay; } } 
        }

        public void AddOnlineUsers()
        {
            this.OnlineListBox.DataSource = null;
            this.OnlineListBox.Items.Clear();
            this.OnlineListBox.DisplayMember = "DisplayName";
            List<CSkypeManager.TSkypeUser> listUsers = Program.g_skype.GetOnlineUsers();

            foreach (CSkypeManager.TSkypeUser u in listUsers)
            {
                LBItem item = new LBItem();
                item.m_strDisplay = (u.FullName != "") ? u.FullName : u.Handle;
                item.m_strHandle = u.Handle;
                this.OnlineListBox.Items.Add(item);
            }
        }

        private void OnlineListBox_DoubleClick(object sender, MouseEventArgs args)
        {
            if (this.OnlineListBox.SelectedItem != null)
            {
                LBItem item = (LBItem)this.OnlineListBox.SelectedItem;
//                MessageBox.Show(item.DisplayName);
                Program.GetConversation(item.m_strHandle);
            }
        }

        private void OnlineListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            notifyIcon.BalloonTipTitle = Program.strPGPSkype;
            notifyIcon.BalloonTipText = Program.strPGPSkype;
            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;

            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon.Visible = true;
//                notifyIcon1.ShowBalloonTip(500);
                this.Hide();
            }
            else if (FormWindowState.Normal == this.WindowState)
            {
                notifyIcon.Visible = false;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }
    }
}
