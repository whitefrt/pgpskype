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
        private Skype m_skype = null;

        public CSkypeManager()
        {
            m_skype = new Skype();
            try
            {
                m_skype.Attach(7, true);
                m_skype.OnlineStatus += new _ISkypeEvents_OnlineStatusEventHandler(skype_OnlineStatus);
                m_skype.MessageStatus += new _ISkypeEvents_MessageStatusEventHandler(skype_MessageStatus);
                Program.m_localUserName = (m_skype.CurrentUser.FullName != "") ? m_skype.CurrentUser.FullName : m_skype.CurrentUser.Handle;
            } catch
            {
                m_skype = null;
            }
        }

        public bool Available() { return m_skype != null; }

        private void skype_OnlineStatus(User pUser, TOnlineStatus Status)
        {
            if (Program.g_mainform != null)
                Program.g_mainform.AddOnlineUsers();
        }

        private void skype_MessageStatus(ChatMessage msg, TChatMessageStatus status)
        {
            if (status != TChatMessageStatus.cmsReceived)
                return;
            // Ignore us
            if (msg.Sender.Handle == m_skype.CurrentUserHandle)
                return;
            Program.MessageReceived(msg.Sender.Handle, msg.Body);
        }

        public void SendMessage(string handle, string msg)
        {
            if (m_skype != null)
                m_skype.SendMessage(handle, msg);
        }

        public struct TSkypeUser
        {
            public string Handle;
            public string FullName;
        };

        public List<TSkypeUser> GetOnlineUsers()
        {
            List<TSkypeUser> l = new List<TSkypeUser>();
            if (m_skype == null)
                return l;
            foreach (User u in m_skype.Friends)
            {
                if (u.OnlineStatus != TOnlineStatus.olsOffline)
                {
                    TSkypeUser user;
                    user.Handle = u.Handle;
                    user.FullName = u.FullName;
                    l.Add(user);
                }
            }
            return l;
        }

        public string FullnameFromHandle(string handle)
        {
            if (m_skype == null)
                return handle;
            foreach (User u in m_skype.Friends)
            {
                if (u.Handle != handle)
                    continue;
                if (u.FullName != "")
                    return u.FullName;
                break;
            }
            return handle;
        }
    };
}