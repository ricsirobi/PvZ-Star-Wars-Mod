namespace RLPAKTOOL_NameSpace
{
    partial class SplitMessagesDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplitMessagesDialog));
            this.TextLabel1 = new System.Windows.Forms.Label();
            this.TextLabel2 = new System.Windows.Forms.Label();
            this.MaxLength_textbox = new System.Windows.Forms.TextBox();
            this.TextLabel3 = new System.Windows.Forms.Label();
            this.SplitWord_textbox = new System.Windows.Forms.TextBox();
            this.ProcessButton = new System.Windows.Forms.Button();
            this.Cancel_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // TextLabel1
            // 
            this.TextLabel1.AutoSize = true;
            this.TextLabel1.Location = new System.Drawing.Point(12, 9);
            this.TextLabel1.Name = "TextLabel1";
            this.TextLabel1.Size = new System.Drawing.Size(503, 51);
            this.TextLabel1.TabIndex = 0;
            this.TextLabel1.Text = "This tool enables you to split too long messages to more. An example of usage\r\n c" +
                "ould be the PSP where the text goes out of the screen due\r\n to insufficient scre" +
                "enspace.";
            // 
            // TextLabel2
            // 
            this.TextLabel2.AutoSize = true;
            this.TextLabel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.TextLabel2.Location = new System.Drawing.Point(12, 89);
            this.TextLabel2.Name = "TextLabel2";
            this.TextLabel2.Size = new System.Drawing.Size(648, 17);
            this.TextLabel2.TabIndex = 1;
            this.TextLabel2.Text = "Max length of each message (dialog with length exceeding this value will be split" +
                " in two):";
            // 
            // MaxLength_textbox
            // 
            this.MaxLength_textbox.Location = new System.Drawing.Point(15, 109);
            this.MaxLength_textbox.Name = "MaxLength_textbox";
            this.MaxLength_textbox.Size = new System.Drawing.Size(645, 22);
            this.MaxLength_textbox.TabIndex = 2;
            this.MaxLength_textbox.Text = "120";
            this.MaxLength_textbox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MaxLengthTextBox_KeyPress);
            // 
            // TextLabel3
            // 
            this.TextLabel3.AutoSize = true;
            this.TextLabel3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.TextLabel3.Location = new System.Drawing.Point(12, 134);
            this.TextLabel3.Name = "TextLabel3";
            this.TextLabel3.Size = new System.Drawing.Size(328, 17);
            this.TextLabel3.TabIndex = 3;
            this.TextLabel3.Text = "Place this word at the end of splitted dialog:";
            // 
            // SplitWord_textbox
            // 
            this.SplitWord_textbox.Location = new System.Drawing.Point(15, 154);
            this.SplitWord_textbox.Name = "SplitWord_textbox";
            this.SplitWord_textbox.Size = new System.Drawing.Size(645, 22);
            this.SplitWord_textbox.TabIndex = 4;
            this.SplitWord_textbox.Text = "-";
            // 
            // ProcessButton
            // 
            this.ProcessButton.Location = new System.Drawing.Point(15, 182);
            this.ProcessButton.Name = "ProcessButton";
            this.ProcessButton.Size = new System.Drawing.Size(145, 34);
            this.ProcessButton.TabIndex = 5;
            this.ProcessButton.Text = "Process";
            this.ProcessButton.UseVisualStyleBackColor = true;
            this.ProcessButton.Click += new System.EventHandler(this.ProcessButton_Click);
            // 
            // Cancel_button
            // 
            this.Cancel_button.Location = new System.Drawing.Point(166, 182);
            this.Cancel_button.Name = "Cancel_button";
            this.Cancel_button.Size = new System.Drawing.Size(145, 34);
            this.Cancel_button.TabIndex = 6;
            this.Cancel_button.Text = "Cancel";
            this.Cancel_button.UseVisualStyleBackColor = true;
            this.Cancel_button.Click += new System.EventHandler(this.Cancel_button_Click);
            // 
            // SplitMessagesDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(708, 233);
            this.Controls.Add(this.Cancel_button);
            this.Controls.Add(this.ProcessButton);
            this.Controls.Add(this.SplitWord_textbox);
            this.Controls.Add(this.TextLabel3);
            this.Controls.Add(this.MaxLength_textbox);
            this.Controls.Add(this.TextLabel2);
            this.Controls.Add(this.TextLabel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "SplitMessagesDialog";
            this.Text = "SplitMessagesDialog";
            this.Load += new System.EventHandler(this.SplitMessagesDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label TextLabel1;
        private System.Windows.Forms.Label TextLabel2;
        private System.Windows.Forms.TextBox MaxLength_textbox;
        private System.Windows.Forms.Label TextLabel3;
        private System.Windows.Forms.TextBox SplitWord_textbox;
        private System.Windows.Forms.Button ProcessButton;
        private System.Windows.Forms.Button Cancel_button;
    }
}