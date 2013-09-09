using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace pgpskype.Forms
{
    public partial class SettingsForm : Form
    {
        /************************************************************************/
        /* Items                                                                */
        /************************************************************************/

        class EBItem
        {
            public EBItem(string inter, string display)
            {
                m_strInternalName = inter;
                m_strDisplay = display;
            }
            public string m_strInternalName;
            public string m_strDisplay;
            public string DisplayName { get { return m_strDisplay; } }
        }

        void AddSettings(string inter, string display, string schecked)
        {
            this.SettingsListBox.Items.Add(new EBItem(inter, display), Settings.StringToBool(schecked));          
        }

        /************************************************************************/
        /*                                                                      */
        /************************************************************************/

        public SettingsForm()
        {
            InitializeComponent();
            this.SettingsListBox.CheckOnClick = true;
            this.SettingsListBox.Items.Clear();
            this.SettingsListBox.DisplayMember = "DisplayName";

            // Add settings
            foreach (Settings.TConfigSetting c in Program.g_settings.m_arrSettings)
                AddSettings(c.m_strName, c.m_strDesc, c.m_strValue);
        }

        void SaveToSettings()
        {
            for (int i = 0; i < SettingsListBox.Items.Count; i++ )
            {
                EBItem e = (EBItem)SettingsListBox.Items[i];

                // Save settings to items
                foreach (Settings.TConfigSetting c in Program.g_settings.m_arrSettings)
                {
                    if (c.m_strName == e.m_strInternalName)
                    {
                        bool bChecked = SettingsListBox.GetItemChecked(i);
                        c.m_strValue = Settings.BoolToString(bChecked);
                        break;
                    }
                }
            }

            // Save
            Program.g_settings.Save();
            
            // Updates

            // Update main form
            if (Program.g_mainform != null)
                Program.g_mainform.ApplySettings();
        }

        /************************************************************************/
        /* Handlers                                                             */
        /************************************************************************/

        private void SaveButton_Click(object sender, EventArgs e)
        {
            SaveToSettings();
            this.Close();
        }

        private void FontButton_Click(object sender, EventArgs e)
        {
            FontDialog f = new FontDialog();
            f.Font = Program.g_settings.m_ConversationFont;
            if (f.ShowDialog() != DialogResult.Cancel)
            {
                // Get and apply the font
                Program.g_settings.InitializeFonts(Settings.FontToString(f.Font));

                // Apply font to all conversations
                foreach (ConversationForm c in Program.g_listConversations)
                    c.ApplyFonts();
            }
        }
    }
}
