// <author>Patrick Evers Bjoerkman</author>
// <Contact>puritymail@gmail.com</Contact>
// <lastupdate>01-03-2014</lastupdate>
// <summary>Options dialog for renaming files</summary>


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
    public partial class RenameBox : Form
    {
        public string ResultName = "";
        public RenameBox(string LastFileName)
        {
            InitializeComponent();
            Name_textbox.Text = LastFileName;
        }

        private void RenameBox_Load(object sender, EventArgs e)
        {

        }

        private void Rename_button_Click(object sender, EventArgs e)
        {
            if (Name_textbox.Text=="")
                {
                    MessageBox.Show("A name should be an length of at least one!");
                }
            else
            {
                ResultName = Name_textbox.Text;
                Close();
            }

        }

        private void Cancel_button_Click(object sender, EventArgs e)
        {
            ResultName = "";
            Close();
        }
    }
}
