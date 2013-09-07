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

namespace pgpskype
{
    public class Settings
    {
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

        public Settings()
        {
        }

        // This is done because I don't know how to return by ref from GetUserSettings
        // Otherwise I could do something like GetUserSettings(handle).m_handle = strHandle;
        private void UpdateUserSettings(TUserSettings u)
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
    }
}
