namespace pgpskype
{
    partial class ConversationForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.inputTextBox = new System.Windows.Forms.RichTextBox();
            this.SendButton = new System.Windows.Forms.Button();
            this.convoTextBox = new System.Windows.Forms.RichTextBox();
            this.SendPGP = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // inputTextBox
            // 
            this.inputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.inputTextBox.Location = new System.Drawing.Point(12, 407);
            this.inputTextBox.Name = "inputTextBox";
            this.inputTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.inputTextBox.Size = new System.Drawing.Size(666, 83);
            this.inputTextBox.TabIndex = 0;
            this.inputTextBox.Text = "";
            // 
            // SendButton
            // 
            this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SendButton.Location = new System.Drawing.Point(684, 415);
            this.SendButton.Name = "SendButton";
            this.SendButton.Size = new System.Drawing.Size(87, 40);
            this.SendButton.TabIndex = 1;
            this.SendButton.Text = "Send";
            this.SendButton.UseVisualStyleBackColor = true;
            this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
            // 
            // convoTextBox
            // 
            this.convoTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.convoTextBox.Location = new System.Drawing.Point(3, 12);
            this.convoTextBox.Name = "convoTextBox";
            this.convoTextBox.ReadOnly = true;
            this.convoTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.convoTextBox.Size = new System.Drawing.Size(774, 381);
            this.convoTextBox.TabIndex = 2;
            this.convoTextBox.Text = "";
            // 
            // SendPGP
            // 
            this.SendPGP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SendPGP.Location = new System.Drawing.Point(684, 461);
            this.SendPGP.Name = "SendPGP";
            this.SendPGP.Size = new System.Drawing.Size(87, 29);
            this.SendPGP.TabIndex = 3;
            this.SendPGP.Text = "PGP Code";
            this.SendPGP.UseVisualStyleBackColor = true;
            this.SendPGP.Click += new System.EventHandler(this.SendPGP_Click);
            // 
            // ConversationForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(783, 501);
            this.Controls.Add(this.SendPGP);
            this.Controls.Add(this.convoTextBox);
            this.Controls.Add(this.SendButton);
            this.Controls.Add(this.inputTextBox);
            this.Name = "ConversationForm";
            this.Text = "ConversationForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox inputTextBox;
        private System.Windows.Forms.Button SendButton;
        private System.Windows.Forms.RichTextBox convoTextBox;
        private System.Windows.Forms.Button SendPGP;
    }
}

