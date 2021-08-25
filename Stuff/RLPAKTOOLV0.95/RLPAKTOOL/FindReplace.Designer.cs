namespace RLPAKTOOL_NameSpace
{
    partial class FindReplace
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FindReplace));
            this.label1 = new System.Windows.Forms.Label();
            this.TextToFind_Textbox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ReplaceWith_Textbox = new System.Windows.Forms.TextBox();
            this.Replaceall_button = new System.Windows.Forms.Button();
            this.Cancel_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Find following text";
            // 
            // TextToFind_Textbox
            // 
            this.TextToFind_Textbox.Location = new System.Drawing.Point(15, 29);
            this.TextToFind_Textbox.Name = "TextToFind_Textbox";
            this.TextToFind_Textbox.Size = new System.Drawing.Size(471, 22);
            this.TextToFind_Textbox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(92, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Replace with:";
            // 
            // ReplaceWith_Textbox
            // 
            this.ReplaceWith_Textbox.Location = new System.Drawing.Point(15, 74);
            this.ReplaceWith_Textbox.Name = "ReplaceWith_Textbox";
            this.ReplaceWith_Textbox.Size = new System.Drawing.Size(471, 22);
            this.ReplaceWith_Textbox.TabIndex = 3;
            // 
            // Replaceall_button
            // 
            this.Replaceall_button.Location = new System.Drawing.Point(382, 102);
            this.Replaceall_button.Name = "Replaceall_button";
            this.Replaceall_button.Size = new System.Drawing.Size(104, 36);
            this.Replaceall_button.TabIndex = 4;
            this.Replaceall_button.Text = "Replace all";
            this.Replaceall_button.UseVisualStyleBackColor = true;
            this.Replaceall_button.Click += new System.EventHandler(this.Replaceall_button_Click);
            // 
            // Cancel_button
            // 
            this.Cancel_button.Location = new System.Drawing.Point(272, 102);
            this.Cancel_button.Name = "Cancel_button";
            this.Cancel_button.Size = new System.Drawing.Size(104, 36);
            this.Cancel_button.TabIndex = 5;
            this.Cancel_button.Text = "Cancel";
            this.Cancel_button.UseVisualStyleBackColor = true;
            this.Cancel_button.Click += new System.EventHandler(this.Cancel_button_Click);
            // 
            // FindReplace
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(502, 150);
            this.Controls.Add(this.Cancel_button);
            this.Controls.Add(this.Replaceall_button);
            this.Controls.Add(this.ReplaceWith_Textbox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.TextToFind_Textbox);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FindReplace";
            this.Text = "FindReplace";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TextToFind_Textbox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox ReplaceWith_Textbox;
        private System.Windows.Forms.Button Replaceall_button;
        private System.Windows.Forms.Button Cancel_button;
    }
}