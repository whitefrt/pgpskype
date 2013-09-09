/************************************************************************/
/* pgpSkype                                                             */
/************************************************************************/
// copyright (c) 2013 white_frt
// public domain

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.IO;
using Org.BouncyCastle.Bcpg;
using System.Xml;
using System.Drawing;

namespace pgpskype
{
    public class Settings
    {
        public Settings()
        {
            InitializeSettings();

            // Try to load the settings
            Load();
            Save();
        }

        /************************************************************************/
        /*  Config Settings                                                     */
        /************************************************************************/

        // Settings
        public class TConfigSetting
        {
            public TConfigSetting(string name, string value, string desc)
            {
                m_strName = name;
                m_strValue = value;
                m_strDesc = desc;
            }

            public bool BoolValue()
            {
                return Settings.StringToBool(m_strValue);
            }

            public string StringValue()
            {
                return m_strValue;
            }

            public string m_strName;
            public string m_strValue;
            public string m_strDesc;
        }

        public List<TConfigSetting>m_arrSettings = new List<TConfigSetting>();

        void AddSetting(string name, string value, string desc)
        {
            m_arrSettings.Add(new TConfigSetting(name.ToUpper(), value, desc));
        }

        public bool GetSettingBool(string name)
        {
            name = name.ToUpper();
            foreach (TConfigSetting c in m_arrSettings)
                if (c.m_strName == name)
                    return c.BoolValue();
            return false;
        }

        void InitializeSettings()
        {
            AddSetting("ShowTimestamps", "false", "Show timestamps");
            AddSetting("AutoCloseConversations", "true", "Automatically close Skype conversations");
            AddSetting("AutoSendPgpConvo", "true", "Automatically send PGP code when starting a conversation");
            AddSetting("ShowTaskbar", "false", "Show in taskbar");
            AddSetting("AlwaysOnTop", "false", "Always on top");
        }

        /************************************************************************/
        /* Fonts                                                                */
        /************************************************************************/

        public Font m_ConversationFont = new Font("Tahoma", 8);
        public Font m_ConversationFontBold = new Font("Tahoma", 8, FontStyle.Bold);
//         public Font m_ConversationFont = new Font(FontFamily.GenericSansSerif, 8);
//         public Font m_ConversationFontBold = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold);

        public void InitializeFonts(string strFont)
        {
            if (strFont == null || strFont == "")
                return;
            // Convert string to font
            m_ConversationFont = StringToFont(strFont);
            m_ConversationFontBold = new Font(m_ConversationFont.FontFamily, m_ConversationFont.Size, FontStyle.Bold);
        }

        /************************************************************************/
        /*                                                                      */
        /************************************************************************/

        public class TUserSettings
        {
            public string m_handle;
            public PgpPublicKey m_publicKey;
            public string m_strPublicKey;

            public TUserSettings()
            {
                m_handle = null;
                m_publicKey = null;
                m_strPublicKey = null;
            }
        };

        // Public
        public string m_strPublicKey = null;
        public PgpPublicKey m_publicKeyGlobal = null;

        // Private
        public string m_strPrivatePass = null;
        public PgpSecretKeyRingBundle m_secretBundle = null;

        // All Public
        public List<TUserSettings> m_arrUserSettings = new List<TUserSettings>();

        // This is done because I don't know how to return by ref from GetUserSettings
        // Otherwise I could do something like GetUserSettings(handle).m_handle = strHandle;
        void UpdateUserSettings(TUserSettings u)
        {
            for (int i = 0; i < m_arrUserSettings.Count; i++)
            {
                if (m_arrUserSettings[i].m_handle == u.m_handle)
                {
                    // Update
                    m_arrUserSettings[i] = u;
                    return;
                }
            }
            // Just add to the list if we didn't find anything
            m_arrUserSettings.Add(u);
        }

        public TUserSettings GetUserSettings(string handle)
        {
            foreach (TUserSettings s in m_arrUserSettings)
                if (s.m_handle == handle)
                    return s;
            // Add new settings for this user
            TUserSettings us = new TUserSettings();
            us.m_handle = handle;
            m_arrUserSettings.Add(us);
            return us;
        }

        /************************************************************************/
        /* PGP keys                                                             */
        /************************************************************************/

        public bool AddPublicPrivateKeys(string handle, string strkey)
        {
            bool bPublicOk = false;
            // Find if we have a public key
            int iPublicStart = strkey.IndexOf("-----BEGIN PGP PUBLIC KEY BLOCK-----", StringComparison.OrdinalIgnoreCase);
            if (iPublicStart != -1)
            {
                const string strPublicEnd = "-----END PGP PUBLIC KEY BLOCK-----";
                int iPublicEnd = strkey.IndexOf(strPublicEnd, StringComparison.OrdinalIgnoreCase);
                if (iPublicEnd != -1)
                {
                    // Test public
                    string strPublic = strkey.Substring(iPublicStart, iPublicEnd - iPublicStart + strPublicEnd.Length);
                    bPublicOk = AddPublicKey(handle, strPublic);
                }
            }

            bool bPrivateOk = false;
            // Find if we have a private key
            int iPrivateStart = strkey.IndexOf("-----BEGIN PGP PRIVATE KEY BLOCK-----", StringComparison.OrdinalIgnoreCase);
            if (iPrivateStart != -1)
            {
                const string strPrivateEnd = "-----END PGP PRIVATE KEY BLOCK-----";
                int iPrivateEnd = strkey.IndexOf(strPrivateEnd, StringComparison.OrdinalIgnoreCase);
                if (iPrivateEnd != -1)
                {
                    // Test public
                    string strPrivate = strkey.Substring(iPrivateStart, iPrivateEnd - iPrivateStart + strPrivateEnd.Length);
                    bPrivateOk = AddPrivateKey(strPrivate);
                }
            }

            string success = "";
            if (bPublicOk)
                success += "Successfully imported public key!\n";
            if (bPrivateOk)
                success += "Successfully imported private key!\n";
            if (success == "")
                success = "Failed to import keys!";
            return (bPrivateOk == true || bPublicOk == true);
        }

        public bool AddPrivateKey(string strkey)
        {
            try
            {
                // Convert to memory stream
                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(strkey));
                PgpSecretKeyRingBundle secretBundle = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(ms));
                if (secretBundle != null)
                {
                    m_secretBundle = secretBundle;
                    return true;
                }
            }
            catch
            {

            }
            return false;
        }

        public bool AddPublicKey(string handle, string strkey)
        {
            PgpPublicKeyRingBundle bundle = null;
            try
            {
                // Convert to memory stream
                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(strkey));
                bundle = new PgpPublicKeyRingBundle(PgpUtilities.GetDecoderStream(ms));
                if (bundle == null)
                    return false;
            }
            catch
            {
                return false;
            }

            // Find the public key
            PgpPublicKey pubKey = null;
            foreach (PgpPublicKeyRing keyRing in bundle.GetKeyRings())
            {
                foreach (PgpPublicKey key in keyRing.GetPublicKeys())
                {
                    if (key.IsEncryptionKey)
                    {
                        pubKey = key;
                        break;
                    }
                }
            }

            // No public key found!
            if (null == pubKey)
                return false;

            // Global public key
            if (null == handle)
            {
                m_strPublicKey = strkey;
                m_publicKeyGlobal = pubKey;
            }
            else
            {
                // Update the user
                TUserSettings NewUser = GetUserSettings(handle);
                NewUser.m_publicKey = pubKey;
                NewUser.m_strPublicKey = strkey;
                UpdateUserSettings(NewUser);
            }
            return true;
        }

        /************************************************************************/
        /* Save / Load                                                          */
        /************************************************************************/

        string GetSettingsLocation()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "pgpSkype.xml");
        }

        public bool Load()
        {
            XmlDocument w = new XmlDocument();
            try
            {
                w.Load(GetSettingsLocation());
            }
            catch
            {
                return false;
            }

            // Find the "Settings" node
            XmlNode root = null;
            if (w.ChildNodes != null)
            {
                foreach (XmlNode c in w.ChildNodes)
                    if (c.Name.ToUpper() == "SETTINGS")
                    {
                        root = c;
                        break;
                    }
            }
            if (root == null)
                return false;

            foreach (TConfigSetting c in m_arrSettings)
                XmlReadString(root, c.m_strName, ref c.m_strValue);

            // Fonts
            string strFont = "";
            XmlReadString(root, "ConversationFont", ref strFont);
            InitializeFonts(strFont);

            return true;
        }

        public bool Save()
        {
            XmlWriter w = XmlWriter.Create(GetSettingsLocation());
            if (w == null)
                return false;

            w.WriteStartDocument();

            // Settings
            w.WriteStartElement("Settings");

            // Elements
            foreach (TConfigSetting c in m_arrSettings)
                w.WriteElementString(c.m_strName, c.m_strValue);

            // Fonts
            w.WriteElementString("ConversationFont", FontToString(m_ConversationFont));

            // End Settings
            w.WriteEndElement();

            w.WriteEndDocument();
            w.Close();

            return true;
        }

        /************************************************************************/
        /*  Helpers                                                             */
        /************************************************************************/

        void XmlReadString(XmlNode root, string element, ref string value)
        {
            element = element.ToUpper();
            if (root.ChildNodes != null)
                foreach (XmlNode x in root.ChildNodes)
                {
                    if (x.Name.ToUpper() == element)
                    {
                        value = (x.InnerText);
                        return;
                    }
                }
        }

        void XmlReadBool(XmlNode root, string element, ref bool value)
        {
            element = element.ToUpper();
            if (root.ChildNodes != null)
                foreach (XmlNode x in root.ChildNodes)
                {
                    if (x.Name.ToUpper() == element)
                    {
                        value = StringToBool(x.InnerText);
                        return;
                    }
                }
        }

        public static string BoolToString(bool b)
        {
            return (b) ? "true" : "false";
        }

         public static bool StringToBool(string b)
        {
            return (b == "true") ? true : false;
        }

        public static Font StringToFont(string str)
        {
            var cvt = new FontConverter();
            return cvt.ConvertFromString(str) as Font;              
        }

        public static string FontToString(Font f)
        {
            var cvt = new FontConverter();
            return cvt.ConvertToString(f);
        }
    }
}
