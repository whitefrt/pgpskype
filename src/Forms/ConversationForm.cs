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

namespace pgpskype
{
    public partial class ConversationForm : Form
    {
        public string m_conversationHandle;
        public string m_conversationUserFullname;

        public ConversationForm(string user)
        {
            InitializeComponent();

            // Extra handlers
            this.FormClosed += new FormClosedEventHandler(closed_event);
            this.inputTextBox.KeyDown += new KeyEventHandler(inputTextBox_KeyDown);
            this.convoTextBox.LinkClicked += new LinkClickedEventHandler(mLinkClicked);

            // Apply fonts
            ApplyFonts();

            // Set the handle
            SetHandle(user);

             // Send public key first thing
            if (Program.g_settings.GetSettingBool("AutoSendPgpConvo"))
                SendPublicPGP();
        }

        public void ApplyFonts()
        {
            this.convoTextBox.Font = Program.g_settings.m_ConversationFont;
            this.inputTextBox.Font = this.convoTextBox.Font;
        }

        public void SetHandle(string user)
        {
            m_conversationHandle = user;
            m_conversationUserFullname = Program.g_skype.DisplayNameFromHandle(m_conversationHandle);
            this.Text = m_conversationUserFullname;
        }

        public void AddConversationText(string capt, string text, bool bEncrypted)
        {
            if (capt == m_conversationUserFullname && !Win32.IsForegroundWindow(this))
            {
                // Flash the window
                Win32.FlashWindow.Flash(this, 2);
            }

            // Remove the last new line
            if (text[text.Length - 1] == '\n')
                text = text.Substring(0, text.Length - 1);

            if (this.convoTextBox.Text.Length != 0)
                this.convoTextBox.AppendText("\n");

            Font FontOldFont = this.convoTextBox.Font;

            if (Program.g_settings.GetSettingBool("ShowTimestamps"))
            {
                string strHeader = "[" + DateTime.Now.ToString("HH:MM") + "]";
//                 if (bEncrypted == true)
//                     strHeader += "[E]";
                this.convoTextBox.AppendText(strHeader + " ");
            }
            else
            {
//                 if (bEncrypted == true)
//                     this.convoTextBox.AppendText("[E] ");
            }

            this.convoTextBox.SelectionFont = Program.g_settings.m_ConversationFontBold;
            this.convoTextBox.AppendText(capt + ": ");
            this.convoTextBox.SelectionFont = FontOldFont;
            this.convoTextBox.AppendText(text/* + "\n"*/);

            this.convoTextBox.SelectionStart = this.convoTextBox.Text.Length;
            this.convoTextBox.ScrollToCaret();
            this.convoTextBox.Refresh();
        }

        /************************************************************************/
        /* Internal Files                                                        */
        /************************************************************************/

        private void SendBoxText(bool bEncrypt = true)
        {
            string text = this.inputTextBox.Text;
            if (text.Length != 0)
            {
                // Send to user
                Program.TransmitMessage(m_conversationHandle, text, bEncrypt);
                AddConversationText(Program.m_localUserName, text, bEncrypt);
            }
            this.inputTextBox.Clear();
            this.inputTextBox.ResetText();
            this.inputTextBox.SelectionStart = 0;
            this.inputTextBox.Select(0, 0);
        }

        public void SendPublicPGP()
        {
            // Send the public key to the other user
            this.inputTextBox.Text = Program.g_settings.m_strPublicKey;
            SendBoxText(false); // not encrypted
        }

        private void inputTextBox_KeyDown(object sender, KeyEventArgs args)
        {
            if (args.Control == false && args.KeyCode == Keys.Enter)
            {
                SendBoxText();
                args.Handled = true;
            }
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            SendBoxText();
            this.inputTextBox.Focus();
        }

        private void closed_event(object sender, EventArgs e)
        {
            // Notify program we've closed
            Program.ConversationClosed(m_conversationHandle);
        }

        private void SendPGP_Click(object sender, EventArgs e)
        {
            if (Program.g_settings.m_strPublicKey != null)
            {
                SendPublicPGP();
            }
        }

        private void mLinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }
    }
}
