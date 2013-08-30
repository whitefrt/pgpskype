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

namespace pgpskype
{
    static class Program
    {
        // Globals - yuck
        static public string m_localUserName = null;
        static public CSkypeManager g_skype = null;
        static public Settings g_settings = null;
        static public MainForm g_mainform = null;
        static List<ConversationForm> g_listConversations = new List<ConversationForm>();

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
                    TransmitMessage(handle, "Please send your public PGP key, users are unable to send you encrypted messages. (click on PGP Code)", false);
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

        static public void MessageReceived(string handle, string msg)
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
                return;
            }


            // We must have a conv by now
            if (conv == null)
                return;

            if (IsPGPPublic)
            {
                if (g_settings.AddPublicPrivateKeys(handle, msg))
                {
                    conv.AddConversationText("STATUS", "Successfully imported public key for user " + handle, false);
//                     // Optional: Send it back
//                     conv.SendPublicPGP();
                    return;
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
                        ConversationError(handle, "Failed to decrypt message!");
                }
            }

            // Add the message to the conversation
            conv.AddConversationText(conv.m_conversationUserFullname, msg, bEncrypted);
        }

        public static string RandomString()
        {
            RandomNumberGenerator rng = new RNGCryptoServiceProvider();
            byte[] tokenData = new byte[32];
            rng.GetBytes(tokenData);
            string token = Convert.ToBase64String(tokenData);
//            Console.WriteLine(token);
            return token;
        }

        public static bool GenerateRandomKeys()
        {
            string ident = RandomString();
            if (ident == null)
                return false;
            string pw = RandomString();
            if (pw == null)
                return false;
            PGP.KeyPairOut kp = PGP.GenerateKeyPair(ident, pw);
            if (kp == null)
                return false;
            if (false == Program.g_settings.AddPublicPrivateKeys(null, kp.strPublic + kp.strPrivate))
                return false;
            Program.g_settings.m_strPrivatePass = pw;
            return true;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Kick off settings
            g_settings = new Settings();

            // Generate keys
            if (!GenerateRandomKeys())
            {
                MessageBox.Show("FAILED TO GENERATE KEYS!");
                return;
            }

            // Initialize skype
            g_skype = new CSkypeManager();
            if (!g_skype.Available())
            {
                MessageBox.Show("Skype not available!");
                return;
            }

            // Initialize the main form
            g_mainform = new MainForm();
            
            // Boom
            Application.Run(g_mainform);
        }
    }
}
