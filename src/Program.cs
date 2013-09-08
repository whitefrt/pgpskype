/************************************************************************/
/* pgpSkype                                                             */
/************************************************************************/
// copyright (c) 2013 white_frt
// public domain

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace pgpskype
{
    static class Program
    {
        /************************************************************************/
        /* Globals - yuck                                                       */
        /************************************************************************/

        static public string m_localUserName = null;
        static public CSkypeManager g_skype = null;
        static public Settings g_settings = null;
        static public MainForm g_mainform = null;
        static List<ConversationForm> g_listConversations = new List<ConversationForm>();

        /************************************************************************/
        /* Constants                                                            */
        /************************************************************************/

        // Version
        const int PGP_SKYPE_VERSION = 3;

        // Messages
        public const string strPGPSkype = "pgpSkype";
        const string strPGPRequired = "[pgpSkype]: Please send your public PGP key, users are unable to send you encrypted messages.";

        // Configuration
        const int RSA_NUM_BITS = 2048;
        const int SECRET_PHRASE_BYTES = 128;

        /************************************************************************/
        /*                                                                      */
        /************************************************************************/

        static public void ConversationClosed(string handle)
        {
            for (int i = 0; i < g_listConversations.Count; i++)
            {
                if (g_listConversations[i].m_conversationHandle == handle)
                {
                    // Remove this
                    g_listConversations.RemoveAt(i);
                    return;
                }
            }
        }

        static public ConversationForm GetConversation(string handle, bool bAdd = true)
        {
            ConversationForm f = null;
            foreach (ConversationForm ff in g_listConversations)
                if (ff.m_conversationHandle == handle)
                    return ff;
            if (bAdd == false)
                return null;
            f = new ConversationForm(handle);
            g_listConversations.Add(f);
            f.Show();
            return f;
        }

        static public void ConversationError(string handle, string text)
        {
            ConversationForm convo = GetConversation(handle, false);
            if (convo != null)
            {
                convo.AddConversationText("ERROR", text, false);
            }
        }

        static public bool TransmitMessage(string handle, string msg, bool bNeedEncrypt = true)
        {
//            bNeedEncrypt = false;
            if (bNeedEncrypt)
            {
                // Need to encrypt the message, see if we have the public key
                Settings.TUserSettings settings = g_settings.GetUserSettings(handle);
                if (settings.m_publicKey == null)
                {
                    // NO public key, can't encrypt
                    ConversationError(handle, String.Format("User {0} does not have public key settings! Message not sent, user notified.", handle));
                    // Notify the user...
                    TransmitMessage(handle, strPGPRequired, false);
                    return false;
                }

                // Encrypt msg
                msg = PGP.Encrypt(msg, settings.m_publicKey);
                if (msg == null)
                {
                    ConversationError(handle, "Unable to encrypt message! Message not sent.");
                    return false;
                }
            }

            // Send message to handle
            g_skype.SendMessage(handle, msg);
            return true;
        }

        static public bool MessageReceived(string handle, string msg)
        {
            // Find the conversation (or create a new one) - DONT CREATE ONE YET
            ConversationForm conv = GetConversation(handle, false);
//             if (conv == null)
//                 return;

            bool bEncrypted = false;

            // Check if the user sent us his public key
            bool IsPGPPublic = (-1!=msg.IndexOf("-----BEGIN PGP PUBLIC KEY BLOCK-----", StringComparison.OrdinalIgnoreCase));
            int iPGPMsg = msg.IndexOf("-----BEGIN PGP MESSAGE-----", StringComparison.OrdinalIgnoreCase);

            if (IsPGPPublic || (iPGPMsg != -1))
            {
                // This is an encrypted message
                // Create the convo if we don't have one
                conv = GetConversation(handle, true);
            }
            else
            {
                // Non encrypted messages won't appear in our chat window at all
                // Check if this is one of our messages
                if (msg == strPGPRequired)
                {
                    // Create a new conversation window (this will automatically send the pgp key)
                    if (conv == null)
                        conv = GetConversation(handle, true);
                    // We already have a window, send the pgp key
                    else
                        conv.SendPublicPGP();
                    return true; // we ate the message
                }
                return false;
            }


            // We must have a conv by now
            if (conv == null)
                return false;

            if (IsPGPPublic)
            {
                if (g_settings.AddPublicPrivateKeys(handle, msg))
                {
                    conv.AddConversationText("STATUS", "Successfully imported public key for user " + handle + " , sending messages enabled.", false);
//                     // Optional: Send it back
//                     conv.SendPublicPGP();
                    return true;
                }
            }

            if (iPGPMsg != -1)
            {
                if (g_settings.m_secretBundle == null || g_settings.m_strPrivatePass == null)
                    ConversationError(handle, "Unable to decrypt, no secret key/pass supplied!");
                else
                {
                    // Try to decrypt
                    string nmsg = PGP.Decrypt(msg.Substring(iPGPMsg), g_settings.m_secretBundle, g_settings.m_strPrivatePass);
                    if (nmsg != null)
                    {
                        // Successfully decrypted
                        msg = nmsg;
                        //                        handle += " [E]"; // Mark encrypted
                        bEncrypted = true;
                    }
                    else
                    {
                        // Failed to decrypt, resend the public PGP in case it changed
                        ConversationError(handle, "Failed to decrypt message!, resending key");
                        conv.SendPublicPGP();
                        return false;
                    }
                }
            }

            // Add the message to the conversation
            conv.AddConversationText(conv.m_conversationUserFullname, msg, bEncrypted);
            return true;
        }

        /************************************************************************/
        /* Helpers                                                              */
        /************************************************************************/

        public static void CloseSkypeConversationWindow(string handle)
        {
            // Try to find window
            IntPtr wnd = Win32.FindWindow("TConversationForm", handle);
            if (wnd != IntPtr.Zero)
            {
                Win32.SendMessage(wnd, Win32.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }
        }

        /************************************************************************/
        /* Key generation wrappers                                              */
        /************************************************************************/

        public static string RandomString(int bytes)
        {
            RandomNumberGenerator rng = new RNGCryptoServiceProvider();
            byte[] tokenData = new byte[bytes];
            rng.GetBytes(tokenData);
            string token = Convert.ToBase64String(tokenData);
            return token;
        }

        public static bool GenerateRandomKeys()
        {
            // Generate a random identity
            string ident = RandomString(SECRET_PHRASE_BYTES);
            if (ident == null)
                return false;
            // Generate a random password
            string pw = RandomString(SECRET_PHRASE_BYTES);
            if (pw == null)
                return false;
            // Generate a randomized key pair
            PGP.KeyPairOut kp = PGP.GenerateKeyPair(RSA_NUM_BITS, ident, pw);
            if (kp == null)
                return false;
            // Add the keys
            if (false == Program.g_settings.AddPublicPrivateKeys(null, kp.strPublic + kp.strPrivate))
                return false;
            // Set the main pw
            Program.g_settings.m_strPrivatePass = pw;
            return true;
        }

        /************************************************************************/
        /* Automatic updates                                                    */
        /************************************************************************/

        static bool IsURLValid(string url)
        {
            bool result = true;
            try
            {
                WebRequest webRequest = WebRequest.Create(url);
                webRequest.Timeout = 5000; // miliseconds
                webRequest.Method = "HEAD";
                webRequest.GetResponse();
            }
            catch
            {
                result = false;
            }
            return result;
        }

        // URL: http://github.com/whitefrt/pgpskype/raw/master/bin/pgpSkype001-win32-bin.zip
        static void CheckIfNewVersionAvailable()
        {
            string url = string.Format("http://github.com/whitefrt/pgpskype/raw/master/bin/pgpSkype{0:D3}-win32-bin.zip", PGP_SKYPE_VERSION+1);
            if (IsURLValid(url))
            {
                string msg = string.Format("A new version is available ({0:D3}):\n\n{1}\n\nWould you like to download the new version?", PGP_SKYPE_VERSION + 1, url);
                DialogResult res = MessageBox.Show(msg, strPGPSkype, MessageBoxButtons.YesNo);
                if (res == DialogResult.Yes)
                    System.Diagnostics.Process.Start(url);
            }
        }

        /************************************************************************/
        /* Main program                                                         */
        /************************************************************************/
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // C# gui
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Use the thread pool to asynchronously check if a new version is available
            ThreadPool.QueueUserWorkItem(unused => CheckIfNewVersionAvailable());

            // Initialize the skype client
            g_skype = new CSkypeManager();

            // Kick off settings
            g_settings = new Settings();

            // Generate keys - this takes a while
            if (!GenerateRandomKeys())
            {
                MessageBox.Show("FAILED TO GENERATE KEYS!", strPGPSkype);
                return;
            }

            // Try to attach
            if (!g_skype.Attach() || !g_skype.Available())
            {
                MessageBox.Show("Skype is not available!", strPGPSkype);
                return;
            }

            // Initialize the main form
            g_mainform = new MainForm();
            
            // Boom
            Application.Run(g_mainform);
        }
    }
}
