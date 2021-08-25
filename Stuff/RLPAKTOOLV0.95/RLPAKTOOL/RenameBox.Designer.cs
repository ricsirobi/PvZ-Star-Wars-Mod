namespace RLPAKTOOL_NameSpace
{
    partial class RenameBox
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RenameBox));
            this.Disc_label = new System.Windows.Forms.Label();
            this.Name_textbox = new System.Windows.Forms.TextBox();
            this.Rename_button = new System.Windows.Forms.Button();
            this.Cancel_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Disc_label
            // 
            this.Disc_label.AutoSize = true;
            this.Disc_label.Location = new System.Drawing.Point(12, 9);
            this.Disc_label.Name = "Disc_label";
            this.Disc_label.Size = new System.Drawing.Size(210, 17);
            this.Disc_label.TabIndex = 0;
            this.Disc_label.Text = "Enter new name for selected file";
            // 
            // Name_textbox
            // 
            this.Name_textbox.Location = new System.Drawing.Point(15, 38);
            this.Name_textbox.Name = "Name_textbox";
            this.Name_textbox.Size = new System.Drawing.Size(505, 22);
            this.Name_textbox.TabIndex = 1;
            // 
            // Rename_button
            // 
            this.Rename_button.Location = new System.Drawing.Point(407, 80);
            this.Rename_button.Name = "Rename_button";
            this.Rename_button.Size = new System.Drawing.Size(113, 33);
            this.Rename_button.TabIndex = 2;
            this.Rename_button.Text = "Rename";
            this.Rename_button.UseVisualStyleBackColor = true;
            this.Rename_button.Click += new System.EventHandler(this.Rename_button_Click);
            // 
            // Cancel_button
            // 
            this.Cancel_button.Location = new System.Drawing.Point(288, 80);
            this.Cancel_button.Name = "Cancel_button";
            this.Cancel_button.Size = new System.Drawing.Size(113, 33);
            this.Cancel_button.TabIndex = 3;
            this.Cancel_button.Text = "Cancel";
            this.Cancel_button.UseVisualStyleBackColor = true;
            this.Cancel_button.Click += new System.EventHandler(this.Cancel_button_Click);
            // 
            // RenameBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(532, 125);
            this.Controls.Add(this.Cancel_button);
            this.Controls.Add(this.Rename_button);
            this.Controls.Add(this.Name_textbox);
            this.Controls.Add(this.Disc_label);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "RenameBox";
            this.Text = "RenameBox";
            this.Load += new System.EventHandler(this.RenameBox_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label Disc_label;
        private System.Windows.Forms.TextBox Name_textbox;
        private System.Windows.Forms.Button Rename_button;
        private System.Windows.Forms.Button Cancel_button;
    }
}