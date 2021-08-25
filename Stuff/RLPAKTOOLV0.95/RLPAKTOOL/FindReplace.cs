// <author>Patrick Evers Bjoerkman</author>
// <Contact>puritymail@gmail.com</Contact>
// <lastupdate>01-03-2014</lastupdate>
// <summary>Options dialog for find and replace</summary>


using System;
using System.Windows.Forms;

namespace RLPAKTOOL_NameSpace
{
    public partial class FindReplace : Form
    {
        #region Fields
        public bool Process;
        public string FindThisText = "";
        public string ReplaceWithText = "";
        #endregion
        #region Constructor
        public FindReplace()
        {
            InitializeComponent();
        }
        #endregion
        #region Methods
        private void Replaceall_button_Click(object sender, EventArgs e)
        {
            Process = true;
            FindThisText = TextToFind_Textbox.Text;
            ReplaceWithText = ReplaceWith_Textbox.Text;
            Close();
        }
        private void Cancel_button_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion
    }
}
