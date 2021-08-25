// <author>Patrick Evers Bjoerkman</author>
// <Contact>puritymail@gmail.com</Contact>
// <lastupdate>01-03-2014</lastupdate>
// <summary>Options dialog for splitting of messages</summary>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RLPAKTOOL_NameSpace
{
    public partial class SplitMessagesDialog : Form
    {
        #region Fields
        public bool Process = false;
        public string SplitWithWord = "";
        public int MaxLength = 0;
        #endregion
        #region Contructors
        public SplitMessagesDialog()
        {
            InitializeComponent();
        }
        private void SplitMessagesDialog_Load(object sender, EventArgs e)
        {

        }
        #endregion
        #region Methods
        private void MaxLengthTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
                && !char.IsDigit(e.KeyChar)
                && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if (e.KeyChar == '.'
                && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }
        private void ProcessButton_Click(object sender, EventArgs e)
        {
            int maxlength = 0;
            if (int.TryParse(MaxLength_textbox.Text, out maxlength))
            {
                if (maxlength < 3)
                {
                    MessageBox.Show("Maxlength must be over 2!", "Input problem detected!", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        
                }
                else
                {
                    Process = true;
                    SplitWithWord = SplitWord_textbox.Text;
                    MaxLength = maxlength;
                    Close();
                }
            }
            else
            {
                MessageBox.Show("Could not convert the provided maxlength to a digit, please provide a valid positive number!","Input problem detected!",MessageBoxButtons.OK,MessageBoxIcon.Hand);
            }
        }
        private void Cancel_button_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion
    }
}
