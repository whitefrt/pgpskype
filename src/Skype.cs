/************************************************************************/
/* pgpSkype                                                             */
/************************************************************************/
// copyright (c) 2013 white_frt
// public domain

using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using SKYPE4COMLib;

namespace pgpskype
{
    public class CSkypeManager
    {
        Skype m_skype = null;
        bool m_attached = false;

        public CSkypeManager()
        {
            m_skype = new Skype();
            try
            {
                // Handlers
                ((_ISkypeEvents_Event)m_skype).AttachmentStatus += new _ISkypeEvents_AttachmentStatusEventHandler(skype_AttachmentStatus);
                m_skype.OnlineStatus += new _ISkypeEvents_OnlineStatusEventHandler(skype_OnlineStatus);
                m_skype.MessageStatus += new _ISkypeEvents_MessageStatusEventHandler(skype_MessageStatus);
                m_skype.PluginMenuItemClicked += new _ISkypeEvents_PluginMenuItemClickedEventHandler(skype_MenuItemClicked);

                // Start client if not running
                if (!m_skype.Client.IsRunning)
                {
                    m_skype.Client.Start();
                }
            }
            catch
            {
                m_skype = null;
                m_attached = false;
            }
        }

        public bool Attach()
        {
            if (m_attached == true)
                return true;
            if (m_skype == null)
                return false;
            try
            {
                int pro = m_skype.Protocol; // 5
                m_skype.Attach(pro, true);

                Program.m_localUserName = GetUserDisplayName(m_skype.CurrentUser);

                return true;
            }
            catch
            {
                m_attached = false;
            }
            return false;
        }

        public bool Available() { return m_skype != null; }

        public void SendMessage(string handle, string msg)
        {
            if (m_attached)
                m_skype.SendMessage(handle, msg);
        }

        void RefreshForm()
        {
            if (Program.g_mainform != null)
                Program.g_mainform.RefreshForm();
        }

        /************************************************************************/
        /* Helpers                                                              */
        /************************************************************************/

        public struct TSkypeUser
        {
            public string Handle;
            public string DisplayName;
        };

        public List<TSkypeUser> GetOnlineUsers()
        {
            List<TSkypeUser> l = new List<TSkypeUser>();
            if (!m_attached)
                return l;
            foreach (User u in m_skype.Friends)
            {
                if (u.OnlineStatus != TOnlineStatus.olsOffline)
                {
                    TSkypeUser user;
                    user.Handle = u.Handle;
                    user.DisplayName = GetUserDisplayName(u);
                    l.Add(user);
                }
            }
            return l;
        }

        public string DisplayNameFromHandle(string handle)
        {
            if (!m_attached)
                return handle;
            foreach (User u in m_skype.Friends)
            {
                if (u.Handle != handle)
                    continue;
                return GetUserDisplayName(u);
            }
            return handle;
        }

        // TODO: support Display name
        static public string GetUserDisplayName(User u)
        {
            if (u.DisplayName != "")
                return u.DisplayName;
            return (u.FullName != "") ? u.FullName : u.Handle;
        }

        /************************************************************************/
        /* Callbacks                                                            */
        /************************************************************************/

        private void skype_AttachmentStatus(TAttachmentStatus status)
        {
            if (status == TAttachmentStatus.apiAttachAvailable)
                Attach();
            else if (status == TAttachmentStatus.apiAttachSuccess)
            {
                m_attached = true;
            }
            else
            {
                m_attached = false;
            }
            RefreshForm();
        }

        private void skype_OnlineStatus(User pUser, TOnlineStatus Status)
        {
            RefreshForm();
        }

        private void skype_MenuItemClicked(PluginMenuItem pMenuItem, UserCollection pUsers, TPluginContext PluginContext, string ContextId)
        {
        }

        private void skype_MessageStatus(ChatMessage msg, TChatMessageStatus status)
        {
            // We only care about received messages
            if (status != TChatMessageStatus.cmsReceived)
                return;

            // Ignore messages sent from us
            if (msg.Sender.Handle == m_skype.CurrentUserHandle)
            {
                msg.Seen = true; // Make sure our own messages are marked as seen
                return;
            }

            // Handle encrypted messages / misc
            if (Program.MessageReceived(msg.Sender.Handle, msg.Body))
            {
                // If we ate the message, set it as seen
                msg.Seen = true;

                // Close the conversation window if needed
                if (Program.g_settings.GetSettingBool("AutoCloseConversations"))
                {
                    // leave the conversation, todo: think of a better way - this doesn't work at all
                    if (msg.Chat != null)
                        msg.Chat.Leave();

                    // Try to close the window
                    Program.CloseSkypeConversationWindow(GetUserDisplayName(msg.Sender));
                }
            }
        }
    }
}