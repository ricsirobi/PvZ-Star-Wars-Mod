// <author>Patrick Evers Bjoerkman</author>
// <summary>Mainclass for Real Life PAK tool</summary>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using RLPAKTOOL_NameSpace.Fileformats;

namespace RLPAKTOOL_NameSpace
{
    public partial class RlpakTool : Form
    {
        #region Fields
        #region Convertion Fields
        //Handles kana convertion
        readonly Dictionary<char, char> _thinkanaConvertion = new Dictionary<char, char>();

        private int currentPriority = 0;


        #endregion
        #region Variables

        BlockingCollection<bool> WaitingForEndLoadFile = new BlockingCollection<bool>(1); 

        //How much gap should there be between the files in the PAK archive?
        private const int ArchiveFileGap = 200;
        //This variable defines the colors of each row of the textentries listbox
        readonly List<Color> _textEntriesColors = new List<Color>();
        //The files of the opened PAK file is loaded into Files
        List<File> _files = new List<File>();
        //The path to the opened PAK file is OpenedFile_path
        public string OpenedFilePath = "";
        //The index of the entry in realentries being edited is CurrentEntryIndex
        int _currentEntryIndex = -1;
        //The index of the file in Files being edited is CurrentEntryIndex
        int _currentFileIndex;
        //realentries refers to the entries of the file that are listed in the TextEntries listbox at the moment.
        List<Entry> realentries = new List<Entry>();
        //enum that identificies the fileformat of a given file.
        public enum FileFormat { Unknown, Scriptfile, VariablesFile, CGim, Gim , Cz1, Cz2};
        //Enable Pointer debugging?
        #endregion
        #endregion
        #region Constructor
        //Initialize form
        public RlpakTool()
        {
            InitializeComponent();
        }
        //Void being invoked at load
        private void Form1_Load(object sender, EventArgs e)
        {
            BackPanel.Width = Width;
            BackPanel.Height = Height;
            string[] kanaconvContent = Properties.Resources.kanaconv.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in kanaconvContent)
            {
                string[] parts = line.Split('=');
                char key = parts[0].ToCharArray()[0];
                char value = parts[1].ToCharArray()[0];
                _thinkanaConvertion.Add(key, value);
            }
        }

        #endregion
        #region Methods
        #region ControlMethods
        //GIMCompressor browse directory button methods
        private void GIMCompressor_browsefolder_button_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                GIMCompressor_browsefile_textbox.Text = folderBrowserDialog1.SelectedPath;
            }
        }
        private void GIMCompressor_outputbrowsefolder_button_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                GIMCompressor_outputbrowsefile_textbox.Text = folderBrowserDialog1.SelectedPath;
            }

        }
        //Gim compression controls browsefile button
        private void GIMCompressor_browsefile_button_Click(object sender, EventArgs e)
        {
            openFileDialog1.Multiselect = false;
            openFileDialog1.Filter = @"Gim file (*.gim)|*.gim|Compressed GIM Image file (*.cgim)|*.cgim|PNG Image file (*.png)|*.png|CZ2 Image file (*.cz2)|*.cz2|CZ1 Image file (*.cz1)|*.cz1|All files (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK && openFileDialog1.FileName != "")
            {
                GIMCompressor_browsefile_textbox.Text = openFileDialog1.FileName;
            }
        }
        //Gim compression controls browsesavefile button
        private void GIMCompressor_outputbrowsefile_button_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = @"Gim file (*.gim)|*.gim|Compressed GIM Image file (*.cgim)|*.cgim|PNG Image file (*.png)|*.png|CZ2 Image file (*.cz2)|*.cz2|CZ1 Image file (*.cz1)|*.cz1|All files (*.*)|*.*";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK && saveFileDialog1.FileName != "")
            {
                GIMCompressor_outputbrowsefile_textbox.Text = saveFileDialog1.FileName;
            }
        }
        //Gim compression controls compress button
        private void GIMCompressor_compress_button_Click(object sender, EventArgs e)
        {

            if (GIMCompressor_browsefile_textbox.Text != "" && System.IO.Directory.Exists(GIMCompressor_browsefile_textbox.Text))
            {

                    Task.Run(() => CompressGim(System.IO.Directory.GetFiles(GIMCompressor_browsefile_textbox.Text), System.IO.Path.GetFullPath(GIMCompressor_outputbrowsefile_textbox.Text)));
                    GIMCompressor_browsefile_button.Enabled = false;
                    GIMCompressor_browsefolder_button.Enabled = false;
                    GIMCompressor_compress_button.Enabled = false;
                    GIMCompressor_decompress_button.Enabled = false;
                    GIMCompressor_outputbrowsefile_button.Enabled = false;
                    GIMCompressor_outputbrowsefolder_button.Enabled = false;
                    CompressPNGCZ2_button.Enabled = false;
                    DecompressCZ2PNG_button.Enabled = false;
            }
            else
            {

                if (GIMCompressor_browsefile_textbox.Text != "" && System.IO.File.Exists(GIMCompressor_browsefile_textbox.Text) && GIMCompressor_outputbrowsefile_textbox.Text != "")
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(GIMCompressor_browsefile_textbox.Text);
                    GimCompressor gimCompressor = new GimCompressor(bytes,System.IO.Path.GetFileNameWithoutExtension(GIMCompressor_browsefile_textbox.Text));
                    gimCompressor.PercentageChanged = PercentageChanged;
                    Task.Run(() => CompressGim(gimCompressor, GIMCompressor_outputbrowsefile_textbox.Text));
                    GIMCompressor_browsefile_button.Enabled = false;
                    GIMCompressor_browsefolder_button.Enabled = false;
                    GIMCompressor_compress_button.Enabled = false;
                    GIMCompressor_decompress_button.Enabled = false;
                    GIMCompressor_outputbrowsefile_button.Enabled = false;
                    GIMCompressor_outputbrowsefolder_button.Enabled = false;
                    CompressPNGCZ2_button.Enabled = false;
                    DecompressCZ2PNG_button.Enabled = false;
                }
            }
        }
        //Gim compression controls decompress button
        private void GIMCompressor_decompress_button_Click(object sender, EventArgs e)
        {
            if (GIMCompressor_browsefile_textbox.Text != "" && System.IO.Directory.Exists(GIMCompressor_browsefile_textbox.Text))
            {
                Task.Run(() => DeCompressGim(System.IO.Directory.GetFiles(GIMCompressor_browsefile_textbox.Text), System.IO.Path.GetFullPath(GIMCompressor_outputbrowsefile_textbox.Text)));
                GIMCompressor_browsefile_button.Enabled = false;
                GIMCompressor_browsefolder_button.Enabled = false;
                GIMCompressor_compress_button.Enabled = false;
                GIMCompressor_decompress_button.Enabled = false;
                GIMCompressor_outputbrowsefile_button.Enabled = false;
                GIMCompressor_outputbrowsefolder_button.Enabled = false;
                CompressPNGCZ2_button.Enabled = false;
                DecompressCZ2PNG_button.Enabled = false;
            }
            else
            {
                if (GIMCompressor_browsefile_textbox.Text != "" && System.IO.File.Exists(GIMCompressor_browsefile_textbox.Text) && GIMCompressor_outputbrowsefile_textbox.Text != "")
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(GIMCompressor_browsefile_textbox.Text);
                    GimCompressor gimCompressor = new GimCompressor(bytes, System.IO.Path.GetFileNameWithoutExtension(GIMCompressor_browsefile_textbox.Text));
                    gimCompressor.PercentageChanged = PercentageChanged;
                    Task.Run(() => DeCompressGim(gimCompressor, GIMCompressor_outputbrowsefile_textbox.Text));
                    GIMCompressor_browsefile_button.Enabled = false;
                    GIMCompressor_browsefolder_button.Enabled = false;
                    GIMCompressor_compress_button.Enabled = false;
                    GIMCompressor_decompress_button.Enabled = false;
                    GIMCompressor_outputbrowsefile_button.Enabled = false;
                    GIMCompressor_outputbrowsefolder_button.Enabled = false;
                    CompressPNGCZ2_button.Enabled = false;
                    DecompressCZ2PNG_button.Enabled = false;
                }
            }
        }
        //FindReplace button click
        private void FindAndReplace_button_Click(object sender, EventArgs e)
        {
            FindReplace dialog = new FindReplace();
            dialog.ShowDialog();
            if (dialog.Process)
            {
                _files[_currentFileIndex].changed = true;
                int count = 0;
                foreach (Entry entry in realentries)
                {
                    bool tocount = false;
                    if (EntryContainsText(entry, dialog.FindThisText))
                    {
                        entry.head = CastToShiftJis(entry.head.Replace(dialog.FindThisText, dialog.ReplaceWithText));
                        entry.head = AutoInsertContentTextBox.Text + removeSizeCMD(entry.head);

                        entry.content = CastToShiftJis(entry.content.Replace(dialog.FindThisText, dialog.ReplaceWithText));
                        entry.content = AutoInsertContentTextBox.Text + removeSizeCMD(entry.content);

                        entry.head = AutoInsertContentTextBox.Text + removeSizeCMD(entry.head);
                        entry.content = AutoInsertContentTextBox.Text + removeSizeCMD(entry.content);

                        CompileEntry(_files[_currentFileIndex], entry);
                        tocount = true;
                    } 
                    if (tocount)
                    {
                        count++;
                    }
                }
                FileBrowser_listbox_SelectedIndexChanged(null, null);
                MessageBox.Show(count + @" Replaced!");
            }
        }
        //Buttons for extracting files.
        private void ArchiveOperations_ExtractFile_button_Click(object sender, EventArgs e)
        {
            if (_currentFileIndex > -1)
            {
                File file = _files[_currentFileIndex];
                string filename = file.FileName;
                if (filename == "")
                {
                    filename = "noname";
                }
                saveFileDialog1.FileName = filename;
                saveFileDialog1.Filter = GetFileFilter(file.FileFormat);
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    Task.Run(() => ExtractFile(file));
                    SendPanelToFront("Extracting..",1);
                }
            }


        }
        private void ArchiveOperations_ExtractAll_button_Click(object sender, EventArgs e)
        {
            try
            {


                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    Task.Run(() => ExtractFiles());
                    SendPanelToFront("Extracting files", 1);
                }

            }
            catch
            {
                // ignored
            }
        }
        //This void describes what happens when you click the HEXConverter_TextToHex button, basicly it
        //involves converting the readable text of HEXConverter_text_textbox to HEX data and write it to HexConverter_HEX_textbox.
        private void HEXConverter_TextToHex_Button_Click(object sender, EventArgs e)
        {
            HexConverter_HEX_textbox.Text = BitConverter.ToString(Compile(HEXConverter_text_textbox.Text)).Replace("-", "");
        }
        //This void describes what happens when you click the HEXConverter_HexToText button, basicly it
        //involves converting the HEX data of HexConverter_HEX_textbox to readable text and write it to HEXConverter_text_textbox.
        private void HEXConverter_HexToText_Button_Click(object sender, EventArgs e)
        {
            try
            {
                HEXConverter_text_textbox.Text = Decompile(StringToByteArray(HexConverter_HEX_textbox.Text.Replace(" ", "").Replace("\n", "")));
            }
            catch (Exception)
            {
                // ignored
            }
        }
        //This method is not for the end user, but for the developer. It converts excel rows/columns to and array separated by comma.
        //it is used to get the inner body of ChangeFrom, ChangeTo, ChangeToBef, ChangeToBef2. Paste the excel data into HexConverter_HEX_textbox and click the BuildArrayFromExcel_Button.
        private void BuildArrayFromExcel_Button_Click(object sender, EventArgs e)
        {
            string[] spl = HexConverter_HEX_textbox.Text.Split(new[] { "\t\n" }, StringSplitOptions.RemoveEmptyEntries);
            HEXConverter_text_textbox.Text = "";
            HEXConverter_text_textbox.Text = string.Join(",", spl);
        }
        #endregion
        #region PAK Methods
        //This method encrypts readable text into the charecterset used by RealLife Engine
        public byte[] Compile(string inputbytes)
        {
            if (Invert_checkbox.Checked)
            {
                inputbytes = inputbytes.Replace("\n", "{UHEX:245}");

            }
            else
            {
                inputbytes = inputbytes.Replace("\n", "{UHEX:10}");

            }
            string[] specialP = inputbytes.Split(new[] { "{UHEX:" }, StringSplitOptions.None);
            byte[] bts = new byte[specialP.Length - 1];
            for (int i = 1; i < specialP.Length; i++)
            {
                try
                {
                    byte fieldvalue = Convert.ToByte(Math.Min(255, Convert.ToInt32(specialP[i].Substring(0, specialP[i].IndexOf("}", StringComparison.Ordinal)))));
                    bts[i - 1] = fieldvalue;
                    specialP[i] = specialP[i].Substring(specialP[i].IndexOf("}", StringComparison.Ordinal) + 1);
                }
                catch
                {
                    specialP[i] = "";
                    bts[i - 1] = 255;

                }
            }
            List<byte> resultfile = new List<byte>();
            for (int f = 0; f < specialP.Length; f++)
            {
                if (f > 0)
                {
                    resultfile.Add(bts[f - 1]);
                }

                char[] chars = specialP[f].ToCharArray();

                for (int i = 0; i < chars.Length; i++)
                {
                   
                        List<byte> bytes = System.Text.Encoding.GetEncoding("shift-jis").GetBytes(chars[i].ToString()).ToList();
                        if (!Invert_checkbox.Checked)
                        {
                                resultfile.AddRange(bytes);                            

                        }
                        else
                        {

                            foreach (byte x in bytes)
                            {
                                resultfile.Add(Convert.ToByte(Math.Max(0, Math.Min(255, 255 - x))));
                            }
                        }



                }

            }

            return resultfile.ToArray();

        }
        //This method decrypts the encrypted hex data used by RealLife Engine to readable text.
        public string Decompile(byte[] bytes)
        {

            if (!Invert_checkbox.Checked)
            {
                return HalfKanaConvert(System.Text.Encoding.GetEncoding("shift-jis").GetString(bytes));
              
            }
            int sta = 0;
            List<byte> resultfile = new List<byte>();
            if (bytes.Length > 0 && bytes[0] == 0)
            {
                sta = 1;
            }
            for (int i = sta; i < bytes.Length; i++)
            {
                byte x = bytes[i];
                    resultfile.Add(Convert.ToByte(Math.Max(0, Math.Min(255, 255 - x))));

            }


            return TextConvert(resultfile.ToArray(), bytes);

        }
        /// <summary>
        /// Fixes half kana issue.
        /// </summary>
        public string HalfKanaConvert(string input)
        {
            char[] parts = input.ToCharArray();
            //kana conversion fix
            foreach (KeyValuePair<char, char> item in _thinkanaConvertion)
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i] == item.Key)
                    {
                        parts[i] = item.Value;
                    }
                }
            }
           return new string(parts);
        }

        /// <summary>
        /// Converts byte array into string, any unconvertable character will get an {UHEX:??} string that the compiler will be able to pass.
        /// </summary>
        public string TextConvert(byte[] input)
        {
            Encoding encoding = System.Text.Encoding.GetEncoding(932, new EncoderReplacementFallback("{?UNKNOWN_HEX?}"), new DecoderReplacementFallback("{?UNKNOWN_HEX?}"));
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                byte value0 = input[i];
                if (value0 == 0)
                {

                    str.Append("{UHEX:0}");
                }
                if (value0 == 14)
                {
                    continue;
                }

                if (value0 == 241)
                {
                    continue;
                }
                if (i + 1 < input.Length)
                {
                    byte value1 = input[i + 1];
                    if (value1 == 1)
                    {

                        str.Append("{UHEX:" + value0 + "}{UHEX:1}");
                        i++;
                        continue;
                    }
                    string output = encoding.GetString(new[] { value0, value1 });
                    if (output.Length > 1)
                    {

                        string inp = encoding.GetString(new[] { value0 });
                        if (!inp.Contains("{?UNKNOWN_HEX?}"))
                        {

                            str.Append(inp);
                            continue;
                        }
                    }
                    if (output.Contains("{?UNKNOWN_HEX?}"))
                    {
                        if (output.StartsWith("{?UNKNOWN_HEX?}"))
                        {
                            byte v = value0;

                            output = "{UHEX:" + v + "}";
                        }
                        else
                        {

                            byte v = value1;

                            output = "{UHEX:" + v + "}";
                        }
                        output = output.Replace("{?UNKNOWN_HEX?}", "");
                    }

                    i++;
                    str.Append(output);
                    continue;
                }

                string output2 = encoding.GetString(new[] { value0 });
                if (output2.Contains("{?UNKNOWN_HEX?}"))
                {

                    continue;
                }

                str.Append(output2);
            }
            string finalOutput = HalfKanaConvert(str.ToString());

            if (Invert_checkbox.Checked)
                return finalOutput.Replace("{UHEX:245}", "\n").Replace("{UHEX:225}", "");
            return finalOutput.Replace("{UHEX:´10}", "\n").Replace("{UHEX:30}", "");
        }
        /// <summary>
        /// Converts byte array into string, any unconvertable character will get an {UHEX:??} string that the compiler will be able to pass.
        /// </summary>
        public string TextConvert(byte[] input, byte[] relations)
        {
            Encoding encoding = System.Text.Encoding.GetEncoding(932, new EncoderReplacementFallback("{?UNKNOWN_HEX?}"), new DecoderReplacementFallback("{?UNKNOWN_HEX?}"));
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                byte value0 = input[i];
               
                if (value0 == 14)
                {
                    continue;
                }
                if (i + 1 < input.Length)
                {
                    byte value1 = input[i + 1];
                  
                    string output = encoding.GetString(new[] { value0, value1 });
                    if (output.Length > 1)
                    {
                        string inp = HalfKanaConvert(encoding.GetString(new[] { value0 }));
                        if (inp.Contains("{?UNKNOWN_HEX?}") || value0 == 10)
                        {
                            byte v = relations[i];
                            inp = "{UHEX:" + v + "}";
                        }
                        //Hiragana characters can be represented by one by. Our algoritm would consider the result as katakana, so
                        //we have to force katakana characters to become hiragana when a character is represented by one byte.
                        str.Append(inp);
                        continue;
                    }
                    if (output.Contains("{?UNKNOWN_HEX?}") || value0 == 10)
                    {
                        byte v = relations[i];

                        output = "{UHEX:" + v + "}";
                    }

                    i++;
                    str.Append(HalfKanaConvert(output));
                    continue;
                }
                string output2 = encoding.GetString(new[] { value0 });
                if (output2.Contains("{?UNKNOWN_HEX?}") || value0 == 10)
                {

                    byte v = relations[i];

                    output2 = "{UHEX:" + v + "}";
                }

                str.Append(HalfKanaConvert(output2));
            }


            return str.ToString().Replace("{UHEX:245}", "\n").Replace("{UHEX:225}", "");
        }

       
        //Compile an entry.
        private void CompileEntry(File file, Entry entry)
        {

            if (entry.head.StartsWith("$S") && entry.head.Length == 5)
                entry.head = "";
            if (entry.content.StartsWith("$S") && entry.content.Length == 5)
                entry.content = "";

            if (entry.LastCurrent == 44)
            {
                if (entry.content.StartsWith("$S"))
                {
                    entry.content = "　" + entry.content;
                }
            }
            #region save
            if (entry.Removed)
            {
                //remove from script.pak
                if (entry.PointTo != null)
                {
                    foreach (Entry entrya in entry.PointTo)
                    {
                        entrya.PointedFrom.Remove(entry);
                    }
                }
                if (entry.PointedFrom.Count > 0)
                {
                    int entryPos = file.entries.IndexOf(entry);
                    foreach (Entry pointEntry in entry.PointedFrom)
                    {
                        pointEntry.PointTo.Remove(entry);
                        pointEntry.PointTo.Add(file.entries[entryPos + 1]);
                    }
                }
                file.entries.Remove(entry);

            }
            else
            {
                //getstructure
                byte[] bytesofhead;
                byte[] bytesofcontent;
                if (entry.content == "")
                {
                    entry.content = " ";
                }
                if (entry.LastCurrent == 22 || entry.LastCurrent == 44 || entry.LastCurrent == 45 || entry.LastCurrent == 54 || entry.LastCurrent == 55)
                {
                    List<byte> res = new List<byte>();

                    for (int lpos = 0; lpos < entry.head.Length; lpos++)
                    {
                        if (entry.head.Substring(lpos).StartsWith("{UHEX:"))
                        {
                            lpos += entry.head.Substring(lpos).IndexOf("}", StringComparison.Ordinal);
                        }
                        else
                        {
                            byte[] compiled = System.Text.Encoding.GetEncoding("shift-jis").GetBytes(entry.head.Substring(lpos,1));
                            res.AddRange(compiled);
                        }
                    }
                    List<byte> res2 = new List<byte>();
                    for (int lpos = 0; lpos < entry.content.Length; lpos++)
                    {
                        if (entry.content.Substring(lpos).StartsWith("{UHEX:"))
                        {
                            string from = entry.content.Substring(lpos+ entry.content.Substring(lpos).IndexOf(":", StringComparison.Ordinal) + 1);
                            string fromd = from.Substring(0,@from.IndexOf("}", StringComparison.Ordinal));
                            byte hex = Convert.ToByte(fromd);
                            res2.Add(hex);
                            lpos += entry.content.Substring(lpos).IndexOf("}", StringComparison.Ordinal);
                        }
                        else
                        {
                            byte[] compiled = System.Text.Encoding.GetEncoding("shift-jis").GetBytes(entry.content.Substring(lpos, 1));
                            res2.AddRange(compiled);
                        }
                    }
                    bytesofhead = res.ToArray();
                    bytesofcontent = res2.ToArray();


                }
                else
                {
                    bytesofhead = Compile(entry.head);
                    bytesofcontent = Compile(entry.content);
                }
                int su = 0;
                if (bytesofhead.Length > 0)
                {
                    su += 4;
                }

                Block block;
                List<byte> dataa = new List<byte>();
                if (entry.LastCurrent == 28)
                {
                    //choice
                    foreach (string choiceValue in entry.Choices)
                    {
                        List<byte> res = new List<byte>();
                        foreach (char letter in choiceValue)
                        {
                            byte[] compiled = System.Text.Encoding.GetEncoding("shift-jis").GetBytes(letter.ToString());
                            if (letter == ' ')
                            {
                                res.AddRange(new byte[]{129, 64});
                                continue;
                            }
                                res.AddRange(compiled);
                            
                        }
                        dataa.AddRange(res.ToArray());
                        dataa.Add(36);
                        dataa.Add(100);
                    }
                    if (dataa.Count > 0)
                    {
                        dataa.RemoveRange(dataa.Count - 2, 2);
                    }
                    block = GetBlock(8 + dataa.Count + entry.LastCMDs.Length, 28);
                }
                else
                {
                    if (entry.LastCurrent == 24)
                    {
                        block = GetBlock(7 + bytesofhead.Length + bytesofcontent.Length + su + entry.LastCMDs.Length, 24);
                    }
                    else
                    {
                        if (entry.LastCurrent == 54)
                        {
                            block = null;
                        }
                        else
                        {
                            if (entry.LastCurrent == 45)
                            {
                                block = GetBlock(2 + entry.inf.Length + bytesofhead.Length + bytesofcontent.Length + su + entry.LastCMDs.Length, 45);
                            }
                            else
                            {
                                if (entry.LastCurrent == 22)
                                {
                                    block = GetBlock(7 + bytesofhead.Length + bytesofcontent.Length + su, 22);
                                }
                                else
                                {
                                    if (entry.LastCurrent == 44)
                                    {
                                        block = GetBlock(12 + bytesofhead.Length + bytesofcontent.Length + su+ entry.LastCMDs.Length, 44);
                                    }
                                    else
                                    {

                                        if (entry.LastCurrent == 55)
                                        {
                                            block = GetBlock(11 + bytesofhead.Length + bytesofcontent.Length + su, 55);
                                        }
                                        else
                                        {

                                            if (entry.LastCurrent == 18)
                                            {
                                                block = GetBlock(bytesofcontent.Length+2, 55);
                                            }
                                            else
                                            {
                                                block = GetBlock(6 + bytesofhead.Length + bytesofcontent.Length + su + entry.LastCMDs.Length, 25);
                                                entry.LastCurrent = 25;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        }
                    

                }
                if (block == null && entry.LastCurrent != 54)
                {
                    int entryRealPos = realentries.IndexOf(entry);
                    TextEntries_listbox.Items[entryRealPos] = "[splitorig] " + entry.head.Replace("\n", "") + " { " + entry.content.Replace("\0", "").Replace("\n", "") + " }";

                    List<string> messages = new List<string>();
                    entry.head = removeSizeCMD(entry.head);
                    entry.content = removeSizeCMD(entry.content);

                    int currentsize = 0;
                    string buildmessage = "";
                    bool lastmessagemade = false;
                    foreach (char letter in entry.content)
                    {
                        byte[] compl = Compile(letter.ToString());
                        currentsize += compl.Length;
                        if (7 + bytesofhead.Length + currentsize + su > 500)
                        {
                            currentsize = compl.Length;
                            int lz = buildmessage.LastIndexOf("$S", 0, StringComparison.Ordinal);
                            if (lz > 0 && buildmessage.Length - lz >= "$S000".Length + 1)
                            {
                                buildmessage = buildmessage.Substring(0, lz);
                                messages.Add(buildmessage);
                                buildmessage = buildmessage.Substring(lz) + letter.ToString();
                            }
                            else
                            {
                                messages.Add(buildmessage);
                                buildmessage =letter.ToString();
                            }
                            lastmessagemade = true;
                        }
                        else
                        {

                            lastmessagemade = false;
                            buildmessage += letter;
                        }
                    }
                    if (!lastmessagemade)
                    {

                        messages.Add(buildmessage);
                    }
                    if (messages.Count > 0)
                    {
                        entry.content = messages[0] + "-";
                       
                            TextEntries_listbox.Items[entryRealPos] = "[splitorig] " + entry.head.Replace("\n", "") + " { " + entry.content.Replace("\0", "").Replace("\n", "") + " }";
                       
                        
                        
                        CompileEntry(_files[_currentFileIndex], entry);
                        messages.RemoveAt(0);
                        int cusa = 0;
                        foreach (string mes in messages)
                        {
                            string nwm = mes + "-";
                          

                            if (cusa == messages.Count - 1)
                            {
                                nwm = nwm.Substring(0, nwm.Length - 1);
                            }
                            //Insert new entry
                            Entry newEntry = new Entry();
                            newEntry.head = entry.head;
                            newEntry.content = nwm ;

                            newEntry.LastCurrent = 25; //A fixed 25 prevents voice from being played two times in a row.
                            _files[_currentFileIndex].entries.Insert(_files[_currentFileIndex].entries.IndexOf(realentries[entryRealPos]) + 1, newEntry);
                            TextEntries_listbox.Items.Insert(entryRealPos + 1, "[splitted] " + newEntry.head.Replace("\n", "") + " { " + newEntry.content.Replace("\0", "").Replace("\n", "") + " }");
                            _textEntriesColors.Insert(entryRealPos + 1, Color.Yellow);
                            realentries.Insert(entryRealPos + 1, newEntry);
                            CompileEntry(_files[_currentFileIndex], newEntry);
                            entryRealPos++;
                            cusa++;

                        }
                    }
                    return;
                }
                if (block != null)
                {
                    byte[] data = new byte[block.Maxlength];
                    data[0] = block.X;
                    data[1] = block.Y;
                    int pos = 2;
                    if (entry.LastCurrent == 25 || entry.LastCurrent == 22)
                    {
                        if (entry.inf.Length > 0)
                        {
                            data[2] = entry.inf[0];
                            data[3] = entry.inf[1];
                         
                        }
                        pos += 2;

                    }
                    if (entry.LastCurrent == 45)
                    {
                        if (entry.inf.Length > 0)
                        {
                            for (int i = 0; i < entry.inf.Length; i++)
                            {
                                data[2+i] = entry.inf[i];

                            }

                        }
                        pos += entry.inf.Length;

                    }
                    if (entry.LastCurrent == 54)
                    {
                        if (entry.inf.Length > 0)
                        {
                            for (int i = 0; i < entry.inf.Length; i++)
                            {
                                data[2 + i] = entry.inf[i];

                            }

                        }
                        pos += entry.inf.Length;

                    }
                    if (entry.LastCurrent == 24)
                    {
                        data[2] = entry.inf[0];
                        data[3] = entry.inf[1];
                        data[4] = entry.inf[2];
                        data[5] = entry.inf[3];

                        pos = 6;
                    }
                    if (entry.LastCurrent == 44)
                    {
                        try
                        {
                            data[2] = entry.inf[0];
                            data[3] = entry.inf[1];

                            data[4] = entry.inf[2];
                            data[5] = entry.inf[3];
                            data[6] = entry.inf[4];
                            data[7] = entry.inf[5];
                        }
                        catch
                        {
                            return;
                        }
                        pos = 8;
                    }
                    if (entry.LastCurrent == 55)
                    {
                        try
                        {
                            data[2] = entry.inf[0];
                            data[3] = entry.inf[1];
                            data[4] = entry.inf[2];
                            data[5] = entry.inf[3];
                            data[6] = entry.inf[4];
                            data[7] = entry.inf[5];
                            data[8] = entry.inf[6];
                            data[9] = entry.inf[7];
                        }
                        catch
                        {
                            return;
                        }
                        pos = 10;
                    }
                    if (entry.LastCurrent == 28)
                    {
                        foreach (byte Byte in entry.ChoicesBytes)
                        {
                            data[pos] = Byte;
                            pos++;
                        }
                    
                        foreach (byte Byte in dataa)
                        {
                            data[pos] = Byte;
                            pos++;
                        }

                    }
                    else
                    {

                        if (block.Inf.Length > 0)
                        {
                            foreach (byte b in block.Inf)
                            {
                                data[pos] = b;
                                pos++;
                            }
                        }
                        if (bytesofhead.Length > 0 && entry.LastCurrent != 54)
                        {
                            data[pos] = ConditinalInvert(81);
                            data[pos + 1] = ConditinalInvert(195);
                            data[pos + 2] = ConditinalInvert(159);
                            pos += 3;
                            for (int i = pos; i < pos + bytesofhead.Length; i++)
                            {
                                data[i] = bytesofhead[i - pos];
                            }
                            pos += bytesofhead.Length;
                            data[pos] = ConditinalInvert(219);
                            data[pos + 1] = ConditinalInvert(155);
                            pos += 2;
                        }
                        if (bytesofcontent.Length > 0)
                        {
                            for (int i = pos; i < pos + bytesofcontent.Length; i++)
                            {
                                data[i] = bytesofcontent[i - pos];
                            }
                        }
                    }
                 
                        for (int i=data.Length-entry.LastCMDs.Length; i<data.Length;i++)
                        {
                            data[i] = entry.LastCMDs[i - (data.Length - entry.LastCMDs.Length)];
                        }

                        int torev = 0;
                    if (data.Length > 5)
                    {
                        for (int i = data.Length - (entry.LastCMDs.Length + 3); i < data.Length - entry.LastCMDs.Length; i++)
                        {
                            if (data[i] == 0)
                            {

                                torev++;
                                
                            }
                        }
                    }
                    List<byte> bts = data.ToList();
                    for (int i = 0; i < torev; i++)
                    {
                        if (bts[data.Length - entry.LastCMDs.Length - torev] != 0)
                        {
                            if (torev-i == 1 && data[data.Length-2] == 0)
                            {

                                bts.RemoveAt(bts.Count -2);
                            }
                            else
                            {
                                throw new Exception("Fatal compile error!");
                            }
                        }
                        else
                        {
                            bts.RemoveAt(data.Length  - entry.LastCMDs.Length - torev);
                        }
                    }
                    if (bts.Count % 2 > 0)
                    {
                        if (entry.contentBytes.Count() > 3 && entry.contentBytes.Skip(entry.contentBytes.Count() - 3).Count(x => x == 0) == 2)
                        {
                            bts.RemoveAt(bts.Count - 1);
                        }
                        else
                        {
                            if (entry.LastCurrent == 22 || entry.LastCurrent == 44 || entry.LastCurrent == 45 ||
                                entry.LastCurrent == 54 || entry.LastCurrent == 55 || entry.LastCurrent == 28)
                            {
                                bts.InsertRange(bts.Count - entry.LastCMDs.Length, new Byte[] { 255-241});

                            }
                            else
                            {
                                bts.InsertRange(bts.Count - entry.LastCMDs.Length, new Byte[] { 241 });
                                
                            }
                        }
                    }
                   
                     bts[1] = Convert.ToByte(bts.Count / 2);

                     data = bts.ToArray();
                     if (entry.LastCurrent == 18)
                     {
                         data = new byte[] { 18, 6, 0, 0, 0, 0 };
                         int res;
                         Int32.TryParse(entry.content, out res);
                         byte[] bytes = BitConverter.GetBytes(res);
                         for (int i = 0; i < bytes.Length; i++)
                         {
                             data[i + 2] = bytes[i];
                         }
                     }
                    //update script.pak
                    entry.contentBytes = data;
                }
                if (entry.LastCurrent == 54 && entry.inf.Length >2)
                {

                    List<byte> r = new byte[]{54,0,entry.inf[0],entry.inf[1],entry.inf[2],entry.inf[3]}.ToList();
                    r.AddRange(bytesofcontent);
                    if (r[r.Count-1] != 0)
                    {
                        r.Add(0);
                    }
                    //update script.pak
                    entry.contentBytes = r.ToArray();
                }
            }



            #endregion
            entry.head = removeSizeCMD(entry.head);
            entry.content = removeSizeCMD(entry.content);
        }
        string removeSizeCMD(string input)
        {
            if (input.StartsWith("　$S"))
            {
                input = input.Substring(1);
            }
            if (input.StartsWith("$S"))
            {
                int number;
                int.TryParse(input.Substring(2,3),out number);
                if (number > -1)
                {
                    input = input.Substring(5);
                }
            }
            return input;
        }

        //This method finds the best suitable block (based on length and dialog type).
        Block GetBlock(int desiredlength, byte desiredst)
        {
            if (desiredst == 54)
            {
                Block block = new Block();
                block.X = desiredst;
                block.Maxlength = desiredlength;
                return block;
            }
            List<Block> blocks = new List<Block>();

            byte x = desiredst;
            for (byte y = 0; y < 255; y++)
            {
                cmd ln = GetBlockLength(y);
                if (ln.length >= desiredlength + ln.inf.Length)
                {
                    Block block = new Block();
                    block.X = x;
                    block.Y = y;
                    block.Maxlength = ln.length;
                    block.Stln = ln.stln;
                    block.Inf = ln.inf;
                    blocks.Add(block);
                }
            }
            blocks.Sort((blockA, blockB) => blockA.Maxlength.CompareTo(blockB.Maxlength));
            if (blocks.Count == 0)
            {

                return null;
            }
            return blocks[0];
        }
        //This method returns some information like length of a dialog message
        cmd GetBlockLength(byte input2)
        {
            cmd cmd = new cmd();

            cmd.length = input2 * 2;
            cmd.stln = 0;
            return cmd;
        }
        public List<byte> Entris = new List<byte>();
        //This void loads the text entries of a script file
        public void LoadFile(File file)
        {
            #region Read Files
            string lastmessage = "";

            int amount = -1;
            foreach (Entry entry in file.entries)
            {

                amount++;
                
                string nwmessage = "LOADING " + (Math.Round((100.0 / Convert.ToDouble(file.entries.Count)) * amount, 2)).ToString(CultureInfo.InvariantCulture) + "%";
                if (entry.ForcedFour)
                {
                    continue;
                }
                byte[] entrySource = entry.contentBytes;
                if (entry.LastCurrent == 22)
                {
                    if (nwmessage != lastmessage)
                    {
                        lastmessage = nwmessage;
                        LoadingUpdateLabel(1,nwmessage);
                    }
                    byte classifier = entrySource[1];
                    cmd blocklength = GetBlockLength(classifier);
                    if (blocklength.length > 0)
                    {
                        List<byte> textinside = new List<byte>();
                        blocklength.inf = new[] { entrySource[2], entrySource[3] };
                        for (int i2 = 4; i2 < blocklength.length - 1; i2++)
                        {

                            textinside.Add(entrySource[i2]);

                        }
                        var toProcess = textinside;
                        int ind = toProcess.IndexOf(ConditinalInvert(242));
                        if (ind > -1)
                        {
                            toProcess[ind] = 0;
                        }
                        if (toProcess.Count > 0 && toProcess[toProcess.Count - 1] == 0)
                        {
                            toProcess.RemoveAt(toProcess.Count - 1);
                        }
                        if (toProcess.Count > 0 && toProcess[toProcess.Count - 1] == 0)
                        {
                            toProcess.RemoveAt(toProcess.Count - 1);
                        }
                        entry.content = removeSizeCMD(TextConvert(toProcess.ToArray())).Replace("\0", "");
                    }

                    continue;
                }
                if (entry.LastCurrent == 54 && entry.Special )
                {
                    if (nwmessage != lastmessage)
                    {
                        lastmessage = nwmessage;
                        LoadingUpdateLabel(1,nwmessage);
                    }

                    List<byte> textinside = new List<byte>();
                    entry.inf = new[] { entrySource[2], entrySource[3], entrySource[4], entrySource[5] };
                        for (int i2 = 6; i2 < entrySource.Length; i2++)
                        {

                            textinside.Add(entrySource[i2]);

                        }
                            entry.LastCMDs = new byte[] { 0 };
                        
                        var toProcess = textinside;
                        int ind = toProcess.IndexOf(ConditinalInvert(242));
                        if (ind > -1)
                        {
                            toProcess[ind] = 0;
                        }
                        if (toProcess.Count > 0 && toProcess[toProcess.Count - 1] == 0)
                        {
                            toProcess.RemoveAt(toProcess.Count - 1);
                        }
                        if (toProcess.Count > 0 && toProcess[toProcess.Count - 1] == 0)
                        {
                            toProcess.RemoveAt(toProcess.Count - 1);
                        }
                        entry.content = removeSizeCMD(TextConvert(toProcess.ToArray())).Replace("\0", "");
                    

                    continue;
                }
                if (entry.LastCurrent == 45)
                {
                    if (nwmessage != lastmessage)
                    {
                        lastmessage = nwmessage;
                        LoadingUpdateLabel(1,nwmessage);
                    }
                    byte classifier = entrySource[1];
                    cmd blocklength = GetBlockLength(classifier);
                    if (blocklength.length > 5)
                    {                     
                      
                        if (entrySource[5] <20)
                        {
                            blocklength.inf = new[] { entrySource[2], entrySource[3], entrySource[4], entrySource[5] };
                        }
                        else
                        {

                            blocklength.inf = new[] { entrySource[2], entrySource[3]};
                        }
                        List<byte> textinside = new List<byte>();

                        for (int i2 = 4 + blocklength.inf.Length - 2; i2 < blocklength.length - 1; i2++)
                        {

                            textinside.Add(entrySource[i2]);

                        }
                        var toProcess = textinside;
                        int ind = toProcess.IndexOf(ConditinalInvert(242));
                        if (ind > -1)
                        {
                            toProcess[ind] = 0;
                        }
                        if (toProcess.Count > 0 && toProcess[toProcess.Count - 1] == 0)
                        {
                            toProcess.RemoveAt(toProcess.Count - 1);
                        }
                        if (toProcess.Count > 0 && toProcess[toProcess.Count - 1] == 0)
                        {
                            toProcess.RemoveAt(toProcess.Count - 1);
                        }
                        entry.LastCMDs = new byte[] { 0, entrySource[entrySource.Length - 1] };
                        entry.content = removeSizeCMD(TextConvert(toProcess.ToArray())).Replace("\0", "");
                        entry.inf = blocklength.inf;
                        blocklength.inf = new byte[0];
                    }

                    continue;
                }
                if (entry.LastCurrent == 18)
                {
                    if (nwmessage != lastmessage)
                    {
                        lastmessage = nwmessage;
                        LoadingUpdateLabel(1,nwmessage);
                    }
                    byte classifier = entrySource[1];
                    cmd blocklength = GetBlockLength(classifier);
                    if (blocklength.length > 5)
                    {

                        blocklength.inf = new byte[] { };
                        
                        List<byte> textinside = new List<byte>();

                        for (int i2 = 2; i2 < blocklength.length; i2++)
                        {

                            textinside.Add(entrySource[i2]);

                        }
                        entry.LastCMDs = new byte[] { };
                        entry.content = BitConverter.ToInt32(textinside.ToArray(),0).ToString();
                        entry.inf = blocklength.inf;
                        blocklength.inf = new byte[0];
                    }

                    continue;
                }
                if (entry.LastCurrent == 55)
                {
                    if (nwmessage != lastmessage)
                    {
                        lastmessage = nwmessage;
                        LoadingUpdateLabel(1,nwmessage);
                    }
                    byte classifier = entrySource[1];
                    cmd blocklength = GetBlockLength(classifier);
                    if (blocklength.length > 9)
                    {

                        blocklength.inf = new[] { entrySource[2], entrySource[3], entrySource[4], entrySource[5], entrySource[6], entrySource[7], entrySource[8], entrySource[9] };
                   
                        List<byte> textinside = new List<byte>();

                        for (int i2 = 4 + blocklength.inf.Length - 2; i2 < blocklength.length - 1; i2++)
                        {

                            textinside.Add(entrySource[i2]);

                        }
                        var toProcess = textinside;
                        int ind = toProcess.IndexOf(ConditinalInvert(242));
                        if (ind > -1)
                        {
                            toProcess[ind] = 0;
                        }
                        if (toProcess.Count > 0 && toProcess[toProcess.Count - 1] == 0)
                        {
                            toProcess.RemoveAt(toProcess.Count - 1);
                        }
                        if (toProcess.Count > 0 && toProcess[toProcess.Count - 1] == 0)
                        {
                            toProcess.RemoveAt(toProcess.Count - 1);
                        }
                        entry.LastCMDs = new byte[] { 0, entrySource[entrySource.Length - 1] };
                        entry.content = removeSizeCMD(TextConvert(toProcess.ToArray())).Replace("\0", "");
                        entry.inf = blocklength.inf;
                        blocklength.inf = new byte[0];
                    }
                    continue;
                }
                if (entry.LastCurrent == 25)
                {
                    if (nwmessage != lastmessage)
                    {
                        lastmessage = nwmessage;
                        LoadingUpdateLabel(1,nwmessage);
                    }
                    byte classifier = entrySource[1];
                    cmd blocklength = GetBlockLength(classifier);
                    blocklength.inf = new[] { entrySource[2], entrySource[3] };

                    if (blocklength.length > 0)
                    {
                        List<byte> textinside = new List<byte>();
                        List<byte> toProcess = new List<byte>();
                        List<byte> toProcess2 = new List<byte>();

                        for (int i2 = 4; i2 < blocklength.length; i2++)
                        {
                            textinside.Add(entrySource[i2]);
                     
                        }
                        if (textinside.Count > 3 && textinside[textinside.Count - 3] == 0 && textinside[textinside.Count - 2] != 0)
                        {

                            entry.LastCMDs = new[] { textinside[textinside.Count - 3], textinside[textinside.Count - 2], textinside[textinside.Count - 1] };
                            
                            textinside.RemoveAt(textinside.Count - 1);
                            textinside.RemoveAt(textinside.Count - 1);
                            textinside.RemoveAt(textinside.Count - 1);
                        }
                        else
                        {
                            entry.LastCMDs = new byte[] { 0, textinside[textinside.Count - 1] };
                            if (entry.LastCMDs[1] != 0)
                            {

                            }
                            textinside.RemoveAt(textinside.Count - 1);
                            if (textinside[textinside.Count - 1] == 0)
                            {
                                textinside.RemoveAt(textinside.Count - 1);
                            }

                        }
                        if (textinside.Count > 3 && (Decompile(textinside.Where(b => b != 0).ToArray()).Contains("$d")))
                        {

                            for (int i2 = 0; i2 < textinside.Count; i2++)
                            {
                                if (textinside[i2] != ConditinalInvert(159))
                                {
                                    textinside.RemoveAt(i2);
                                    i2--;
                                }
                                else
                                {
                                    textinside.RemoveAt(i2);
                                    break;

                                }
                            }
                            bool which = false;
                            for (int i2 = 0; i2 < textinside.Count; i2++)
                            {
                                if (textinside[i2] == ConditinalInvert(219)  && i2 + 1 < textinside.Count && textinside[i2 + 1] == 155)
                                {
                                    if (which)
                                    {
                                        toProcess2.Add(textinside[i2]);
                                        continue;
                                    }
                                    which = true;
                                    i2++;
                                }
                                else
                                {
                                    if (which)
                                    {
                                        toProcess2.Add(textinside[i2]);
                                    }
                                    else
                                    {

                                        toProcess.Add(textinside[i2]);
                                    }
                                }
                            }

                            int ind = toProcess.IndexOf(ConditinalInvert(242));
                            if (ind > -1)
                            {
                                toProcess[ind] = 0;
                            }
                            ind = toProcess2.IndexOf(ConditinalInvert(242));
                            if (ind > -1)
                            {
                                toProcess2[ind] = 0;
                            }

                            entry.head = Decompile(toProcess.ToArray()).Replace("\0", "");
                            if (entry.head.Contains("$d"))
                            {
                                entry.content = entry.head.Substring(entry.head.IndexOf("$d", StringComparison.Ordinal) + 2);
                                entry.head = entry.head.Substring(0, entry.head.IndexOf("$d", StringComparison.Ordinal));
                            }
                            entry.content += Decompile(toProcess2.ToArray()).Replace("\0", "");

                            entry.head = removeSizeCMD(entry.head);
                            entry.content = removeSizeCMD(entry.content);
                            entry.inf = blocklength.inf;
                            blocklength.inf = new byte[0];
                        }
                        else
                        {
                            toProcess = textinside;


                            int ind = toProcess.IndexOf(ConditinalInvert(242));
                            if (ind > -1)
                            {
                                toProcess[ind] = 0;
                            }

                            entry.content = Decompile(toProcess.ToArray()).Replace("\0", "");

                            entry.content = removeSizeCMD(entry.content);
                    
                            entry.inf = blocklength.inf;
                            blocklength.inf = new byte[0];
                        }

                    }

                    continue;
                }

                if (entry.LastCurrent == 44 && entry.contentBytes.Length > 7)
                {
                    if (nwmessage != lastmessage)
                    {
                        lastmessage = nwmessage;
                        LoadingUpdateLabel(1,nwmessage);
                    }
                    byte classifier = entrySource[1];
                    cmd blocklength = GetBlockLength(classifier);
                    blocklength.inf = new[] { entrySource[2], entrySource[3], entrySource[4], entrySource[5], entrySource[6], entrySource[7] };

                    if (blocklength.length > 8)
                    {

                        List<byte> textinside = new List<byte>();

                        for (int i2 = 8; i2 < blocklength.length - 1; i2++)
                        {
                            textinside.Add(entrySource[i2]);
                        }

                        if ( textinside.Count>3 && textinside[textinside.Count - 3] == 0 && textinside[textinside.Count - 2] != 0)
                        {

                            entry.LastCMDs = new[] { textinside[textinside.Count - 3], textinside[textinside.Count - 2], textinside[textinside.Count - 1] };

                            textinside.RemoveAt(textinside.Count - 1);
                            textinside.RemoveAt(textinside.Count - 1);
                            textinside.RemoveAt(textinside.Count - 1);
                        }
                        else
                        {
                            entry.LastCMDs = new byte[] { 0, textinside[textinside.Count - 1] };
                      
                            textinside.RemoveAt(textinside.Count - 1);
                            if (textinside.Count > 0&& textinside[textinside.Count - 1] == 0)
                            {
                                textinside.RemoveAt(textinside.Count - 1);
                            }

                        }
                        var toProcess = textinside;


                        int ind = toProcess.IndexOf(ConditinalInvert(242));
                        if (ind > -1)
                        {
                            toProcess[ind] = 0;
                        }

                        entry.content = removeSizeCMD(TextConvert(toProcess.ToArray())).Replace("\0", "");
                
                        entry.inf = blocklength.inf;
                        blocklength.inf = new byte[0];


                    }
                    continue;
                }
                if (entry.LastCurrent == 24)
                {
                    if (nwmessage != lastmessage)
                    {
                        lastmessage = nwmessage;
                        LoadingUpdateLabel(1,nwmessage);
                    }
                    byte classifier = entrySource[1];
                    cmd blocklength = GetBlockLength(classifier);

                    if (blocklength.length > 0)
                    {


                        blocklength.inf = new[] { entrySource[2], entrySource[3], entrySource[4], entrySource[5] };


                        List<byte> textinside = new List<byte>();
                        List<byte> toProcess = new List<byte>();
                        List<byte> toProcess2 = new List<byte>();
                        for (int i2 = 6; i2 < blocklength.length; i2++)
                        {
                            textinside.Add(entrySource[i2]);
                        }
                        if (textinside.Count > 3 &&textinside[textinside.Count - 3] == 0 && textinside[textinside.Count - 2] != 0)
                        {

                            entry.LastCMDs = new[] { textinside[textinside.Count - 3], textinside[textinside.Count - 2], textinside[textinside.Count - 1] };

                            textinside.RemoveAt(textinside.Count - 1);
                            textinside.RemoveAt(textinside.Count - 1);
                            textinside.RemoveAt(textinside.Count - 1);
                        }
                        else
                        {
                            entry.LastCMDs = new byte[] { 0, textinside[textinside.Count - 1] };
                            if (entry.LastCMDs[1] != 0)
                            {

                            }
                            textinside.RemoveAt(textinside.Count - 1);
                            if (textinside[textinside.Count - 1] == 0)
                            {
                                textinside.RemoveAt(textinside.Count - 1);
                            }

                        }
                        if (textinside.Count > 3 && (Decompile(textinside.Where(b => b != 0).ToArray()).Contains("$d")))
                        {
                        
                            for (int i2 = 0; i2 < textinside.Count; i2++)
                            {
                                if (textinside[i2] != ConditinalInvert(159))
                                {
                                    textinside.RemoveAt(i2);
                                    i2--;
                                }
                                else
                                {
                                    textinside.RemoveAt(i2);
                                    break;

                                }
                            }
                            bool which = false;
                            for (int i2 = 0; i2 < textinside.Count; i2++)
                            {
                                if (textinside[i2] == ConditinalInvert(219) && i2 + 1 < textinside.Count && textinside[i2 + 1] == ConditinalInvert(155))
                                {
                                    if (which)
                                    {
                                        toProcess2.Add(textinside[i2]);
                                        continue;

                                    }
                                    which = true;
                                    i2++;
                                }
                                else
                                {
                                    if (which)
                                    {
                                        toProcess2.Add(textinside[i2]);
                                    }
                                    else
                                    {

                                        toProcess.Add(textinside[i2]);
                                    }
                                }
                            }



                            int ind = toProcess.IndexOf(ConditinalInvert(242));
                            if (ind > -1)
                            {
                                toProcess[ind] = 0;
                            }
                            ind = toProcess2.IndexOf(ConditinalInvert(242));
                            if (ind > -1)
                            {
                                toProcess2[ind] = 0;
                            }
                            entry.inf = blocklength.inf;
                            blocklength.inf = new byte[0];

                            entry.head = Decompile(toProcess.ToArray()).Replace("\0", "");
                            if (entry.head.Contains("$d"))
                            {
                                entry.content = entry.head.Substring(entry.head.IndexOf("$d", StringComparison.Ordinal) + 2);
                                entry.head = entry.head.Substring(0, entry.head.IndexOf("$d", StringComparison.Ordinal));
                            }
                            entry.content += Decompile(toProcess2.ToArray()).Replace("\0", "");

                            entry.head = removeSizeCMD(entry.head);
                            entry.content = removeSizeCMD(entry.content);
                        }
                        else
                        {
                           
                            toProcess = textinside;
                          
                            if (toProcess.Count > 1 && toProcess[toProcess.Count - 2] == 0)
                            {
                                toProcess.RemoveAt(toProcess.Count - 1);
                            }
                            if (toProcess.Count > 0 && toProcess[toProcess.Count - 1] == 0)
                            {
                                toProcess.RemoveAt(toProcess.Count - 1);
                            }
                            int ind = toProcess.IndexOf(ConditinalInvert(242));
                            if (ind > -1)
                            {
                                toProcess[ind] = 0;
                            }

                            entry.content = Decompile(toProcess.ToArray()).Replace("\0", "");

                            entry.content = removeSizeCMD(entry.content);
                            entry.inf = blocklength.inf;
                            blocklength.inf = new byte[0];
                        }

                    }
                    continue;
                }
                if (entry.LastCurrent == 28)
                {
                    if (nwmessage != lastmessage)
                    {
                        lastmessage = nwmessage;
                        LoadingUpdateLabel(1,nwmessage);
                    }
                    byte classifier = entrySource[1];
                    cmd blocklength = GetBlockLength(classifier);

                    if (blocklength.length > 8)
                    {
                        List<byte> textinside = new List<byte>();

                        for (int i2 = 8; i2 < blocklength.length; i2++)
                        {
                            textinside.Add(entrySource[i2]);
                        }
                        if (textinside.Count > 3 && textinside[textinside.Count - 3] == 0 && textinside[textinside.Count - 2] != 0)
                        {

                            entry.LastCMDs = new[] { textinside[textinside.Count - 3], textinside[textinside.Count - 2], textinside[textinside.Count - 1] };

                            textinside.RemoveAt(textinside.Count - 1);
                            textinside.RemoveAt(textinside.Count - 1);
                            textinside.RemoveAt(textinside.Count - 1);
                        }
                        else
                        {
                            entry.LastCMDs = new byte[] { 0, textinside[textinside.Count - 1] };
                            textinside.RemoveAt(textinside.Count - 1);
                            if (textinside[textinside.Count - 1] == 0)
                            {
                                textinside.RemoveAt(textinside.Count - 1);
                            }

                        }
                        
                        var toProcess = textinside;
                        if (toProcess.Count > 0 && toProcess[toProcess.Count - 1] == 0)
                        {
                            toProcess.RemoveAt(toProcess.Count - 1);
                        }
                        int ind = toProcess.IndexOf(ConditinalInvert(242));
                        if (ind > -1)
                        {
                            toProcess[ind] = 0;
                        }
                        for (int aa = 2; aa < 8; aa++)
                        {
                            entry.ChoicesBytes[aa - 2] = entrySource[aa];
                        }
                        List<byte> choices = new List<byte>();
                        byte lastbyte = toProcess[0];
                        foreach (byte Byte in toProcess)
                        {
                            if (Byte == 100 && lastbyte == 36)
                            {
                                choices.RemoveAt(choices.Count - 1);
                                entry.Choices.Add(TextConvert(choices.ToArray()));
                                choices.Clear();
                                continue;
                            }
                            if (Byte != 0)
                            {
                                choices.Add(Byte);
                            }
                            lastbyte = Byte;
                        }
                        if (choices.Count > 0)
                        {

                            entry.Choices.Add(TextConvert(choices.ToArray()));
                        }

                    }
                }

            }


            #endregion
        }
         

        //This method exports a RLPAKTOOL script
        void ExportScriptFile(File file, string path)
        {
            if (file.FileFormat == FileFormat.Scriptfile ||
                    file.FileFormat == FileFormat.VariablesFile)
                {
                    file.entries =
                        GetBlocksOfFile(file.UnsafeSource).ToList();
                    LoadFile(file);

                }
            try
            {
                StringBuilder str = new StringBuilder();
                foreach (Entry entry in file.entries)
                {
                    switch (entry.LastCurrent)
                    {
                        case 28:
                            str.AppendLine("[CHOICE]");
                            foreach (string value in entry.Choices)
                            {
                                str.AppendLine("[NEWCHOICE]");
                                str.AppendLine(value);
                            }
                            str.AppendLine("[ENDCHOICE]");
                            break;
                        case 44:
                            str.AppendLine("[POPUPMESSAGE]");
                            str.AppendLine("[CONTENT]");
                            str.AppendLine(entry.head);
                            str.AppendLine("[ENDPOPUPMESSAGE]");
                            break;
                        default:
                            str.AppendLine("[ENTRY]");
                            str.AppendLine("[HEAD]");
                            str.AppendLine(entry.head);
                            str.AppendLine("[CONTENT]");
                            str.AppendLine(entry.content);
                            str.AppendLine("[ENDENTRY]");
                            break;
                    }
                }
                System.IO.File.WriteAllText(path, str.ToString());
            }
            catch
            {
            }
        }
        //This method saves a PAK file
        void SaveFunc()
        {
            LoadingUpdateLabel(1,"Saving..");
            if (OpenedFilePath == "")
            {
                ReloadFile();
                return;
            }
            try
            {
            string lastmessage = "";



            LoadingUpdateLabel(1,"Compiling..");

            for (int filen = 0; filen < _files.Count; filen++)
            {
                File file = _files[filen];
                if (file.changed)
                {
                    file.UnsafeSource = GetBytesOfEntries(file.entries.ToArray());
                    //is the file too big?
                    if ( file.UnsafeSource.Length > 138376)
                    {
                        string nwmessage = "ONE OF YOUR SCENARIO FILES ARE TOO LONG, SOFTWARE WILL NOW SPLIT IT" + Environment.NewLine + " INTO MULTIPLE FILES... THIS COULD TAKE A WHILE";
                        if (nwmessage != lastmessage)
                        {
                            lastmessage = nwmessage;
                            LoadingUpdateLabel(1,nwmessage);
                        }

                        //Create a new scenariofile and pass nessesary blocks to the new file.
                        //Get end blocks of last block, expects 15 and 12 blocks
                        int newfilename;
                        int.TryParse(file.FileName.Substring(1), out newfilename);
                        int origname = newfilename;
                        List<Entry> blocks = GetBlocksOfFile(file.UnsafeSource).ToList();
                        List<SFile> filesToAdd = new List<SFile>();
                        if (newfilename > 0)
                        {
                            do
                            {
                                newfilename += 1;
                            }
                            while (FilesContainsName(newfilename.ToString()));
                            //Split into two files
                            List<Entry> file0 = blocks.GetRange(0, blocks.Count / 2);
                            List<Entry> file1 = blocks.GetRange(blocks.Count / 2, blocks.Count - blocks.Count / 2);
                            SFile file0O = new SFile();
                            file0O.blocks = file0;
                            file0O.FileName = origname;
                            SFile file1O = new SFile();
                            file1O.blocks = file1;
                            file1O.HasEnd = true;
                            file1O.FileName = newfilename;
                            file0O.PointTo = file1O;
                            file0O.HasHeader = true;
                            filesToAdd.Add(file0O);
                            filesToAdd.Add(file1O);
                            //Create new files
                            for (int i = 0; i < filesToAdd.Count; i++)
                            {
                                SFile chkfile = filesToAdd[i];
                                //Does any entry point to another file
                                for (int ia2 = 0;ia2<chkfile.blocks.Count;ia2++)
                                {
                                    Entry entry = chkfile.blocks[ia2];
                                    if (entry.PointTo.Count > 0 )
                                    {
                                        //Find location of pointto
                                        for (int ia = 0; ia < entry.PointTo.Count;ia++ )
                                        {
                                            Entry entry2 = entry.PointTo[ia];
                                            if (!chkfile.blocks.Contains(entry2))
                                            {
                                                foreach (SFile sfile in filesToAdd)
                                                {
                                                    if (sfile.blocks.Contains(entry2))
                                                    {
                                                        if (entry2.BeginningOfFile)
                                                        {
                                                            //Link to this file

                                                            Entry entryPointer = new Entry();
                                                            entryPointer.LastCurrent = 11;

                                                            entryPointer.contentBytes = new byte[6];
                                                            entryPointer.contentBytes[0] = entryPointer.LastCurrent;
                                                            entryPointer.contentBytes[1] = 3;

                                                            int nxp = chkfile.blocks.IndexOf(entry);
                                                            if (nxp + 1 < chkfile.blocks.Count)
                                                            {
                                                                entryPointer.PointTo.Add(chkfile.blocks[chkfile.blocks.IndexOf(entry) + 1]);
                                                            }
                                                            Entry filePointer = new Entry();
                                                            filePointer.LastCurrent = 18;
                                                            List<byte> content = new byte[] { filePointer.LastCurrent, 0 }.ToList();
                                                            content.AddRange(BitConverter.GetBytes(sfile.FileName));
                                                            content[1] = Convert.ToByte(content.Count / 2);
                                                            filePointer.contentBytes = content.ToArray();
                                                            entry.PointTo[entry.PointTo.IndexOf(entry2)] = filePointer;
                                                            ia--;
                                                            int pos = chkfile.blocks.IndexOf(entry);
                                                            chkfile.blocks.Insert(pos + 1, filePointer);
                                                            if (nxp + 1 < chkfile.blocks.Count)
                                                            {
                                                                chkfile.blocks.Insert(pos + 1, entryPointer);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            //Split and link to new file 
                                                            do
                                                            {
                                                                newfilename += 1;
                                                            }
                                                            while (FilesContainsName(newfilename.ToString()));
                                                            SFile nwfile = new SFile();
                                                            nwfile.FileName = newfilename;
                                                            if (sfile.HasEnd)
                                                            {
                                                                sfile.HasEnd = false;
                                                                nwfile.HasEnd = true;
                                                            }
                                                            nwfile.PointTo = sfile.PointTo;
                                                            sfile.PointTo = nwfile;
                                                            entry2.BeginningOfFile = true;
                                                            nwfile.blocks = sfile.blocks.GetRange(sfile.blocks.IndexOf(entry2), sfile.blocks.Count - sfile.blocks.IndexOf(entry2));
                                                            sfile.blocks.RemoveRange(sfile.blocks.IndexOf(entry2), sfile.blocks.Count - sfile.blocks.IndexOf(entry2));
                                                            filesToAdd.Add(nwfile);

                                                            //Link to this file
                                                            Entry entryPointer = new Entry();
                                                            entryPointer.LastCurrent = 11;
                                                            entryPointer.contentBytes = new byte[6];
                                                            entryPointer.contentBytes[0] = entryPointer.LastCurrent;
                                                            entryPointer.contentBytes[1] = 3;
                                                            int nxp = chkfile.blocks.IndexOf(entry);
                                                            if (nxp + 1 < chkfile.blocks.Count)
                                                            {
                                                                entryPointer.PointTo.Add(chkfile.blocks[nxp + 1]);
                                                            }
                                                            Entry filePointer = new Entry();
                                                            filePointer.LastCurrent = 18;
                                                            List<byte> content = new byte[] { filePointer.LastCurrent, 0 }.ToList();
                                                            content.AddRange(BitConverter.GetBytes(nwfile.FileName));
                                                            content[1] = Convert.ToByte(content.Count / 2);

                                                            filePointer.contentBytes = content.ToArray();
                                                            entry.PointTo[entry.PointTo.IndexOf(entry2)] = filePointer;
                                                            ia--;
                                                            int pos = chkfile.blocks.IndexOf(entry);
                                                            chkfile.blocks.Insert(pos + 1, filePointer);
                                                            if (nxp + 1 < chkfile.blocks.Count)
                                                            {
                                                                chkfile.blocks.Insert(pos + 1, entryPointer);

                                                            }
                                                        }
                                                        i = -1;
                                                        break;
                                                    }
                                                    if (i == -1)
                                                    {
                                                        break;
                                                    }
                                                }
                                                if (i == -1)
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            //Splittings complete, add finishing like file endings and file pointers
                            foreach (SFile sfile in filesToAdd)
                            {
                                if (!sfile.HasEnd)
                                {
                                    Entry ender = new Entry();
                                    ender.LastCurrent = 21;
                                    ender.contentBytes = new byte[2];
                                    ender.contentBytes[0] = 21;
                                    ender.contentBytes[1] = 1;

                                    Entry starter = new Entry();
                                    starter.LastCurrent = 18;
                                    List<byte> content = new byte[] { starter.LastCurrent, 0 }.ToList();
                                    content.AddRange(BitConverter.GetBytes(sfile.PointTo.FileName));
                                    content[1] = Convert.ToByte(content.Count / 2);

                                    starter.contentBytes = content.ToArray();
                                    sfile.blocks.Add(starter);
                                    if (!(sfile.blocks[sfile.blocks.Count - 1].LastCurrent == 21 && sfile.blocks[sfile.blocks.Count - 1].contentBytes[1] == 1 && sfile.blocks[sfile.blocks.Count - 1].contentBytes.Length == 2))
                                    {
                                        sfile.blocks.Add(ender);

                                    }
                                }
                                if (!sfile.HasHeader)
                                {
                                    Entry starter = new Entry();
                                    starter.LastCurrent = 23;
                                    starter.contentBytes = new byte[] { 23, 3, 0, 0, 0, 0 };
                                    sfile.blocks.Insert(0, starter);
                                }
                            }
                            //Add to file list
                            int awa = _files.IndexOf(file);
                            _files.Remove(file);
                            filesToAdd.Sort(delegate(SFile c0, SFile c1) { return c0.FileName.CompareTo(c1.FileName); });
                            foreach (SFile sfile in filesToAdd)
                            {
                                //Insert file
                                File newfile2 = new File();
                                newfile2.changed = true;
                                newfile2.FileName = "S" + sfile.FileName.ToString().PadLeft(6, '0');
                                newfile2.FileFormat = FileFormat.Scriptfile;
                                newfile2.UnsafeSource = GetBytesOfEntries(sfile.blocks.ToArray());
                                newfile2.entries = sfile.blocks;
                                _files.Insert(awa, newfile2);
                                awa++;

                            }

                        }
                        filen--;
                    }
                }

            }
            int lastval = 0;
            foreach (File file in _files)
            {
                if (file.FileFormat == FileFormat.Scriptfile)
                {

                    file.SignatureValue = lastval;
                    byte[] conv = BitConverter.GetBytes(file.SignatureValue);
                    file.UnsafeSource[2] = conv[0];
                    file.UnsafeSource[3] = conv[1];
                    file.UnsafeSource[4] = conv[2];
                    file.UnsafeSource[5] = conv[3];
                    lastval += CalculateSignatureValue(file.UnsafeSource);

                }
            }
            LoadingUpdateLabel(1,"Generating PAK File..");
            GeneratePakFile(_files, OpenedFilePath);
         
              
            }
            catch
            {
                MessageBox.Show(@"Save failed!");
                SendPanelToBack(1);
                return;
            }

            //Reload
            ReloadFile();
        }
        //This method checks if a file of the opened archive has a filename equals specific parameter.
        bool FilesContainsName(string filename)
        {
            foreach (File file in _files)
            {
                if (file.FileName == "S" + (filename).PadLeft(6, '0'))
                {
                    return true;
                }
            }
            return false;
        }

        int CalculateSignatureValue(byte[] input)
        {
            int result = 0;
            for (int i = 0; i < input.Length; i++)
            {
                byte cmd = input[i];
                int length = input[i + 1] * 2;
                if (length == 0)
                {

                    length = 6;
                }
                if (cmd == 24 || cmd == 25 || cmd == 28)
                {
                    result++;
                }

                if (cmd == 44)
                {
                    int value = input[i + 2];
                    if (value != 4)
                    {
                        result++;
                    }
                }
                i += length - 1;
            }
            return result;
        }
        bool IsCountedCMD(byte[] input)
        {
                byte cmd = input[0]; 
                if (cmd == 24 || cmd == 25 ||cmd == 22)
                {
                    return true;
                }

            
            return false;
        }
        //Gets all entries of the script file
        Entry[] GetBlocksOfFile(byte[] input)
        {
            byte[] con2 = new byte[] { 0, 1, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 16, 17, 18, 19, 20, 21, 23, 26, 27, 29, 31, 32, 33, 34, 35, 38, 39, 40, 41, 42, 45, 46, 47, 48, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 112,143 };
            byte[] toSkip = new byte[] { 0, 1, 3, 4, 5, 6, 7, 8, 9, 10, 17, 18, 19, 38, 40, 41, 45, 20, 21, 23, 26, 27, 29, 31, 32, 33, 34, 35, 39, 42, 46, 47, 48, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 112 };
            List<int> watchForThese = new List<int>();
            List<Entry> watchForTheseB = new List<Entry>();
            List<Entry> entries = new List<Entry>();
            for (int i = 0; i < input.Length; i++)
            {
                byte cmd = input[i];
                int length = input[i + 1] * 2;
                Entry entry = new Entry();
                if (cmd == 143) //goto map structure
                {
                    length = 512 + length;
                }

                entry.UnsafeHexPos = i;
                if (cmd == 54 && length == 0)
                {
                    length = input.Length - i-2;
                    entry.Special = true;
                }
                if (length == 0)
                {
                    entry.ForcedFour = true;
                    length = 4;
                }
                entry.LastCurrent = cmd;
                if (con2.Contains(entry.LastCurrent) && !toSkip.Contains(entry.LastCurrent) && length > 4)
                {

                    entry.contentBytes = input.Skip(i).Take(length).ToArray();
                    int posOfGoto = 0;
                    if (entry.LastCurrent == 143)
                    {
                        for (int ia = 0; ia < entry.contentBytes.Length; ia++)
                        {
                            if (entry.contentBytes[ia] == 0)
                            {
                                posOfGoto = ia+1;
                                break;
                            }
                        }
                        if (posOfGoto < 1 || posOfGoto +3 > entry.contentBytes.Length)
                        {
                            i += length - 1;
                            continue;
                        }
                    }
                    else
                    {
                        for (int ia = 0; ia < entry.contentBytes.Length - 4; ia++)
                        {
                            if (entry.contentBytes[ia] == 0)
                            {
                                ia++;
                                //Are the rest zero?
                                bool found = false;
                                for (int ia2 = ia + 4; ia2 < entry.contentBytes.Length; ia2++)
                                {
                                    if (entry.contentBytes[ia2] != 0)
                                    {
                                        found = true;
                                    }
                                }
                                if (!found)
                                {
                                    posOfGoto = ia;
                                    break;
                                }
                            }
                        }
                        if (posOfGoto == 0)
                        {
                            posOfGoto = entry.contentBytes.Length - 4;
                        }
                    }
                    byte one = entry.contentBytes[posOfGoto];
                    byte two = entry.contentBytes[posOfGoto + 1];
                    byte tree = entry.contentBytes[posOfGoto + 2];
                    byte fourth = entry.contentBytes[posOfGoto + 3];
                    int total = BitConverter.ToInt32(new[] { one, two, tree, fourth }, 0);
                    watchForThese.Add(total);

                    watchForTheseB.Add(entry);
                    if (cmd == 143)
                    {
                        for (int ia = posOfGoto + 4; ia < length; ia+=4)
                        {
                            if (ia + 3 >= length)
                            {
                                break;
                            }
                            one = entry.contentBytes[ia];
                            two = entry.contentBytes[ia + 1];
                            tree = entry.contentBytes[ia + 2];
                            fourth = entry.contentBytes[ia + 3];
                             total = BitConverter.ToInt32(new[] { one, two, tree, fourth }, 0);

                             watchForThese.Add(total);

                             watchForTheseB.Add(entry);
                        }
                        
                    }
                }

                entry.contentBytes = input.Skip(i).Take(length).ToArray();

                i += length - 1;
                entries.Add(entry);
            }

            for (int index = 0; index < watchForThese.Count; index++)
            {

                int p = watchForThese[index];
            foreach (Entry entry in entries)
            {
                    if (entry.UnsafeHexPos == p)
                    {
                        watchForTheseB[index].PointTo.Add(entry);
                        entry.PointedFrom.Add(watchForTheseB[index]);
                        watchForThese.RemoveAt(index);
                        watchForTheseB.RemoveAt(index);
                        index--;
                        break;
                    }
                }
            }
        

            return entries.ToArray();
        }
        //Get the bytes of an entries array
        byte[] GetBytesOfEntries(Entry[] input)
        {
            int saveCount = 0;
            List<byte> bt = new List<byte>();
            foreach (Entry entry in input)
            {
                Block nwblock = GetBlock(entry.Length, entry.LastCurrent);
                if (entry.LastCurrent == 143)
                {
                    nwblock = new Block();
                    nwblock.Maxlength = entry.Length;
                }
                byte[] bl = new byte[nwblock.Maxlength];
                bl[0] = entry.LastCurrent;
                if ((entry.LastCurrent != 54 || !entry.Special) && entry.contentBytes[1] != 0 && entry.LastCurrent!= 143)
                {
                    bl[1] = Convert.ToByte(bl.Length / 2);
                }
                if (entry.LastCurrent == 143)
                {
                    bl[1] =Convert.ToByte( (bl.Length - 512) / 2);
                }
                if (entry.PointTo.Count > 0)
                {
                    //Calculate
                    List<int> newpointer =  new List<int>();
                    for (int index = 0; index < entry.PointTo.Count;index++ )
                    {
                        Entry entrya = entry.PointTo[index];
                        s:
                        int newpointervalue = 0;
                        foreach (Entry entry2 in input)
                        {
                            Block nwblock2 = GetBlock(entry2.Length, entry2.LastCurrent);
                            if (entrya == entry2)
                            {
                                newpointer.Add(newpointervalue);
                                break;
                            }
                            if (entry2.LastCurrent == 143)
                            {

                                newpointervalue += entry2.Length;
                            }
                            else
                            {
                                newpointervalue += nwblock2.Maxlength;
                            }
                        }
                        if (newpointervalue == 0)
                        {
                            newpointer.RemoveAt(newpointer.Count - 1);
                            entrya = input[input.Length - 2];
                            goto s;
                        }
                    }
                    int posOfGoto = 2;
                    if (entry.LastCurrent == 143)
                    {
                        for (int ia = 0; ia < entry.contentBytes.Length; ia++)
                        {
                            if (entry.contentBytes[ia] == 0)
                            {
                                posOfGoto = ia + 1;
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int ia = 2; ia < entry.contentBytes.Length - 4; ia++)
                        {
                            if (entry.contentBytes[ia] == 0)
                            {
                                ia++;
                                //Are the rest zero?
                                bool found = false;
                                for (int ia2 = ia + 4; ia2 < entry.contentBytes.Length; ia2++)
                                {
                                    if (entry.contentBytes[ia2] != 0)
                                    {
                                        found = true;
                                    }
                                }
                                if (!found)
                                {
                                    posOfGoto = ia;
                                    break;
                                }
                            }
                        }
                        if (posOfGoto == 0)
                        {
                            posOfGoto = entry.contentBytes.Length - 4;
                        }
                    }
                    for (int i = 2; i < entry.contentBytes.Length; i++)
                    {
                        bl[i] = entry.contentBytes[i];
                    }
                    foreach (int coordinate in newpointer)
                    {
                        byte[] nwpointer = BitConverter.GetBytes(coordinate);
                        for (int i = 0; i < 4; i++)
                        {
                            bl[i + posOfGoto] = nwpointer[i];
                        }
                        posOfGoto += 4;
                    }

                }
                else
                {
                    for (int i = 2; i < entry.contentBytes.Length; i++)
                    {
                        bl[i] = entry.contentBytes[i];
                    }

                    if (IsCountedCMD(entry.contentBytes))
                    {
                        byte[] bytesSaveCount = BitConverter.GetBytes(saveCount);
                        bl[2] = bytesSaveCount[0];
                        bl[3] = bytesSaveCount[1];
                        saveCount++;
                    }
                    if (entry.contentBytes[0] == 28)
                    {

                        byte[] bytesSaveCount = BitConverter.GetBytes(saveCount);
                        bl[6] = bytesSaveCount[0];
                        bl[7] = bytesSaveCount[1];
                        saveCount++;
                    }
                }
                bt.AddRange(bl);
            }
            return bt.ToArray();
        }
        //This void updates the header of the PAK file to match the modified file data,
        //more specificly it updates the starting position of all files in the PAK archive.
        void GeneratePakFile(List<File> files, string filename)
        {
            using (System.IO.FileStream result = new System.IO.FileStream(filename,System.IO.FileMode.Create))
            {
                //Write number of files
                byte[] newlength = BitConverter.GetBytes(files.Count);
                result.Write(newlength,0,4);
                //Add file length of zero for now.
                //Update filelength
                //Calculate Header length
                int headerLength = 8 + files.Count * 8;
                foreach (File script in files)
                {
                    if (script.FileName.StartsWith("[NONAME_"))
                    {

                        headerLength += 1;
                        continue;
                    }
                    headerLength += System.Text.Encoding.GetEncoding("shift-jis").GetBytes(script.FileName).Length + 1;
                }
                headerLength += 1676;
                int archiveLength = headerLength + files.Sum(script => script.UnsafeSource.Length + ArchiveFileGap);
                byte[] nlength = BitConverter.GetBytes(archiveLength);
                result.Write(nlength, 0, 4);

                //Add files start & length tree
                archiveLength = headerLength;
                foreach (File script in files)
                {
                    //update startpositions
                    byte[] nwpositions = BitConverter.GetBytes(archiveLength);
                    result.Write(nwpositions, 0, 4);
                    script.OriginalLength = script.UnsafeSource.Length;
                    byte[] nwlength = BitConverter.GetBytes(script.OriginalLength);
                    result.Write(nwlength, 0, 4);
                    archiveLength += script.OriginalLength + ArchiveFileGap;
                }
                //Add file name table
                foreach (File script in files)
                {
                    if (!script.FileName.StartsWith("[NONAME_"))
                    {
                        byte[] toWrite = System.Text.Encoding.GetEncoding("shift-jis").GetBytes(script.FileName);
                        result.Write(toWrite, 0, toWrite.Length);
                    }
                    result.Write(new byte[]{0},0,1);
                }
                //Add archivegab
                result.Write(new byte[1676], 0, 1676);
                //Write files
                foreach (File script in files)
                {
                    result.Write(script.UnsafeSource,0,script.UnsafeSource.Length);
                    result.Write(new byte[ArchiveFileGap],0,ArchiveFileGap);
                }
                result.Close();
            }
        }
        //This method opens the PAK archive.
        public void OpenFile(string file)
        {
            try
            {
                LoadingUpdateLabel(1,"LOADING FILE TO MEMORY");
                #region Preparation steps
                byte[] bytesOfFile = System.IO.File.ReadAllBytes(file);
                PrepareLoadSteps();
                realentries.Clear();
                _files.Clear();
                #endregion
                LoadingUpdateLabel(1,"PARSING HEADER..");
                int numberOfFiles = BitConverter.ToInt32(new[] { bytesOfFile[0], bytesOfFile[1], bytesOfFile[2], bytesOfFile[3] }, 0);
                if (numberOfFiles == 0)
                {
                    return;
                }

                #region Read File intervals
                //Search Header

                int firstFileStart = BitConverter.ToInt32(new[] { bytesOfFile[8], bytesOfFile[9], bytesOfFile[10], bytesOfFile[11] }, 0);
                int firstFileFilemax = BitConverter.ToInt32(new[] { bytesOfFile[12], bytesOfFile[13], bytesOfFile[14], bytesOfFile[15] }, 0);
                _files.Add(new File());
                _files[0].FileStart = firstFileStart;
                _files[0].FileMax = firstFileStart + firstFileFilemax;
                _files[0].OriginalLength = firstFileFilemax;
                byte[] bytesOfSource = new byte[firstFileFilemax];
                Array.Copy(bytesOfFile, _files[0].FileStart, bytesOfSource, 0, firstFileFilemax);
                _files[0].UnsafeSource = bytesOfSource;

                int cofile = 1;
                int positionOfFileTable = 0;
                for (int i = 16; i < firstFileStart; i += 8)
                {
                    if (cofile == numberOfFiles)
                    {
                        positionOfFileTable = i;
                        break;
                    }
                    int fileStart = BitConverter.ToInt32(new[] { bytesOfFile[i], bytesOfFile[i + 1], bytesOfFile[i + 2], bytesOfFile[i + 3] }, 0);

                    int fileMax = BitConverter.ToInt32(new[] { bytesOfFile[i + 4], bytesOfFile[i + 5], bytesOfFile[i + 6], bytesOfFile[i + 7] }, 0);
                    File scriptfile = new File();
                    _files.Add(scriptfile);
                    _files[cofile].FileMax = fileStart + fileMax;
                    _files[cofile].FileStart = fileStart;
                    _files[cofile].OriginalLength = fileMax;
                    byte[] bytesOfSource2 = new byte[fileMax];
                    Array.Copy(bytesOfFile, scriptfile.FileStart, bytesOfSource2, 0, fileMax);
                    _files[cofile].UnsafeSource = bytesOfSource2;
                    cofile++;
                }
                #endregion
                #region Read file names
                //Get files
                List<byte> collect = new List<byte>();
                cofile = 0;
                for (int i = positionOfFileTable; i < _files[0].FileStart; i++)
                {
                    if (bytesOfFile[i] == 0)
                    {
                        if (collect.Count > 0)
                        {
                            _files[cofile].FileName = System.Text.Encoding.GetEncoding("shift-jis").GetString(collect.ToArray());

                            cofile++;
                            if (cofile >= numberOfFiles)
                            {
                                break;
                            }
                        }
                        else
                        {

                            cofile++;
                        }
                        collect.Clear();
                    }
                    else
                    {
                        collect.Add(bytesOfFile[i]);
                    }
                }
                #endregion
                int count = 0;
                foreach (File currentFile in _files)
                {
                    if (currentFile.FileName == "")
                    {
                        currentFile.FileName = "[NONAME_" + count.ToString() + "]";
                        count++;
                    }
                    //Identify filetypes
                    if (currentFile.UnsafeSource[0] == 23 && currentFile.UnsafeSource[1] == 3)
                    {
                        //This is a script file
                        currentFile.FileFormat = FileFormat.Scriptfile;

                        //Get signature value
                        currentFile.SignatureValue = BitConverter.ToInt32(new[] { bytesOfFile[currentFile.FileStart + 2], bytesOfFile[currentFile.FileStart + 3], bytesOfFile[currentFile.FileStart + 4], bytesOfFile[currentFile.FileStart + 5] }, 0);
                   
                    }
                    else if (currentFile.UnsafeSource[0] == 22)
                    {
                        //This is a variables file
                        currentFile.FileFormat = FileFormat.VariablesFile;
                    }
                    else if (currentFile.UnsafeSource[0] == 16 && currentFile.UnsafeSource[1] == 0 && currentFile.UnsafeSource[2] == 0 && currentFile.UnsafeSource.Skip(8).Take(22).SequenceEqual(new byte[] { 77, 0, 73, 0, 71, 0, 46, 0, 48, 0, 48, 0, 46, 0, 49, 0, 80, 0, 83, 0, 80, 0 }))
                    {
                        //This is a compressed GIM file
                        currentFile.FileFormat = FileFormat.CGim;
                    }
                    else if (currentFile.UnsafeSource.Take(11).SequenceEqual(new byte[] { 77, 73, 71, 46, 48, 48, 46, 49, 80, 83, 80 }))
                    {
                        //This is a GIM file
                        currentFile.FileFormat = FileFormat.Gim;
                    }
                    else if (currentFile.UnsafeSource.Take(4).SequenceEqual(new byte[] { 67,90, 50,0}))
                    {
                        //This is a CZ2 file
                        currentFile.FileFormat = FileFormat.Cz2;
                    }
                    else if (currentFile.UnsafeSource.Take(4).SequenceEqual(new byte[] { 67, 90, 49, 0 }))
                    {
                        //This is a CZ1 file
                        currentFile.FileFormat = FileFormat.Cz1;
                    }
                }
                OpenedFilePath = file;
            }
            catch
            {
                OpenedFilePath = "";
                EndOpenFile(false);
                return;
            }
            EndOpenFile(true);

        }
        #endregion
        #region ControlMethods
        private void Invert_checkbox_CheckedChanged(object sender, EventArgs e)
        {
            if (MessageBox.Show(@"You will need to reload the file, all changes will be lost, continue?", @"Warning", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                ReloadFile_button_Click(sender, e);
            }
            else
            {
                Invert_checkbox.CheckedChanged -= Invert_checkbox_CheckedChanged;
                Invert_checkbox.Checked = !Invert_checkbox.Checked;
                Invert_checkbox.CheckedChanged += Invert_checkbox_CheckedChanged;
            }
        }





        private void HideCommands_checkbox_CheckedChanged(object sender, EventArgs e)
        {
            if (_currentFileIndex > -1)
                EndLoadFile();
        }

        private void CreatePointer_button_Click(object sender, EventArgs e)
        {
            if (HideCommands_checkbox.Checked)
                MessageBox.Show(@"Please uncheck hidecommands checkbox!");
            if (!HideCommands_checkbox.Checked && TextEntries_listbox.SelectedIndex > -1 && TextEntries_listbox.SelectedIndex + 1 < realentries.Count)
            {
                _files[_currentFileIndex].changed = true;
                Entry entryPointer = new Entry();
                entryPointer.LastCurrent = 11;

                entryPointer.contentBytes = new byte[6];
                entryPointer.contentBytes[0] = entryPointer.LastCurrent;
                entryPointer.contentBytes[1] = 3;
                entryPointer.PointTo.Add(realentries[TextEntries_listbox.SelectedIndex + 1]);
                TextEntries_listbox.Items.Insert(TextEntries_listbox.SelectedIndex + 1, "");
                _textEntriesColors.Insert(TextEntries_listbox.SelectedIndex + 1, Color.Pink);
                realentries.Insert(TextEntries_listbox.SelectedIndex + 1, entryPointer);

                _files[_currentFileIndex].entries.Insert(TextEntries_listbox.SelectedIndex + 1, entryPointer);
                int co = 0;
                for (int i = 0; i < realentries.Count; i++)
                {
                    if (TextEntries_listbox.Items[i].ToString() != "(IGNORED)")
                    {
                        co++;
                    }
                }

                TextEntries_label.Text = @"Content (" + co + @" entries)";

            }
        }

        private void PointerList_Listbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (PointerList_Listbox.SelectedIndex > -1)
            {
                PointTo_Textbox.Text = PointerList_Listbox.SelectedItem.ToString();
            }
        }

        private void PointerEditor_addButton_Click(object sender, EventArgs e)
        {
            int val;
            if (PointerList_Listbox.SelectedIndex > -1 && !HideCommands_checkbox.Checked && int.TryParse(PointTo_Textbox.Text, out val))
            {
                if (val > -1 && val < realentries.Count)
                {
                    _files[_currentFileIndex].changed = true;
                    realentries[_currentEntryIndex].PointTo[PointerList_Listbox.SelectedIndex] = realentries[val];
                    realentries[val].PointedFrom.Add(realentries[_currentEntryIndex].PointTo[PointerList_Listbox.SelectedIndex]);
                    PointerList_Listbox.Items.Clear();

                    foreach (Entry entry in realentries[TextEntries_listbox.SelectedIndex].PointTo)
                    {
                        PointerList_Listbox.Items.Add(realentries.IndexOf(entry));
                    }
                }
            }
        }


        private void PointTo_Textbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
               && !char.IsDigit(e.KeyChar)
               && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // only allow one decimal point
            var textBox = sender as TextBox;
            if (textBox != null && (e.KeyChar == '.'
                                              && textBox.Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }
        private void ArchiveOperations_RenameFile_button_Click(object sender, EventArgs e)
        {
            RenameBox dialogwindow = new RenameBox(_files[_currentFileIndex].FileName);
            dialogwindow.ShowDialog();
            string newname = dialogwindow.ResultName;
            if (newname != "")
            {
                _files[_currentFileIndex].FileName = newname;
                if (_files[_currentFileIndex].FileName != "")
                {
                    //Make sure the file doesn't exist and correct name!
                    bool foundName = false;
                    string origName = _files[_currentFileIndex].FileName;
                    int nmb = 0;
                    do
                    {
                        if (foundName)
                        {
                            nmb++;
                            foundName = false;
                            _files[_currentFileIndex].FileName = origName + "(" + nmb + ")";
                        }
                        foreach (File file in _files)
                        {
                            if (_files[_currentFileIndex] != file && file.FileName == _files[_currentFileIndex].FileName)
                            {
                                foundName = true;
                                break;
                            }
                        }
                    }
                    while (foundName);
                    if (nmb > 0)
                    {
                        MessageBox.Show(@"Software have changed the filename of your file since it equals one of the other files!");
                    }
                }
                FileBrowser_listbox.Items[_currentFileIndex] = newname + GetFileType(_files[_currentFileIndex].FileFormat);
            }
        }

        private void ArchiveOperations_FileUp_button_Click(object sender, EventArgs e)
        {
            if (_currentFileIndex > -1)
            {
                File file = _files[_currentFileIndex];
                _files.Remove(file);
                string content = FileBrowser_listbox.Items[_currentFileIndex].ToString();
                FileBrowser_listbox.Items.RemoveAt(_currentFileIndex);
                _files.Insert(Math.Max(_currentFileIndex - 1, 0), file);
                _currentFileIndex = Math.Max(_currentFileIndex - 1, 0);
                FileBrowser_listbox.Items.Insert(_currentFileIndex, content);
            }
        }

        private void ArchiveOperations_FileDown_button_Click(object sender, EventArgs e)
        {
            if (_currentFileIndex > -1)
            {
                File file = _files[_currentFileIndex];
                _files.Remove(file);
                string content = FileBrowser_listbox.Items[_currentFileIndex].ToString();
                FileBrowser_listbox.Items.RemoveAt(_currentFileIndex);
                _files.Insert(Math.Min(_currentFileIndex + 1, _files.Count - 1), file);
                _currentFileIndex = Math.Min(_currentFileIndex + 1, _files.Count - 1);
                FileBrowser_listbox.Items.Insert(_currentFileIndex, content);
            }
        }

        private void FindNextMisFormedLine_Click(object sender, EventArgs e)
        {

            for (int i = Math.Max(0, TextEntries_listbox.SelectedIndex); i < TextEntries_listbox.Items.Count; i++)
            {
                if (i + 1 < TextEntries_listbox.Items.Count && CountStringOccurrences(realentries[i].ToString().ToUpper(), @"""") == 1)
                {
                    if (CountStringOccurrences(realentries[i + 1].ToString().ToUpper(), @"""") != 1 && CountStringOccurrences(realentries[i - 1].ToString().ToUpper(), @"""") != 1)
                    {
                        TextEntries_listbox.SelectedIndex = i;
                        break;

                    }
                }
            }
        }
        private void SaveChoices_Button_Click(object sender, EventArgs e)
        {
            if (_currentEntryIndex > -1)
            {
            
                _files[_currentFileIndex].changed = true;
                if (realentries[TextEntries_listbox.SelectedIndex].Choices.Count > 0)
                {
                    
                    if (Editor_Choices_listbox.SelectedIndex != -1)
                    {
                      
                        Editor_Choices_listbox.Items[Editor_Choices_listbox.SelectedIndex] = Editor_choice_textbox.Text;
                        Editor_choice_textbox.Text = Editor_Choices_listbox.Items[Editor_Choices_listbox.SelectedIndex].ToString();
                        
                       
                        realentries[TextEntries_listbox.SelectedIndex].Choices[Editor_Choices_listbox.SelectedIndex] = Editor_choice_textbox.Text;
                    }
                }
                realentries[TextEntries_listbox.SelectedIndex].content = Editor_content_textbox.Text;
                realentries[TextEntries_listbox.SelectedIndex].head = Editor_head_textbox.Text;
                TextEntries_listbox_SelectedIndexChanged(null, null);
            
                CompileEntry(_files[_currentFileIndex], realentries[TextEntries_listbox.SelectedIndex]);

            }
        }

        private void PopUp_Checkbox_CheckedChanged(object sender, EventArgs e)
        {
            if (PopUp_Checkbox.Checked)
            {
                Editor_head_textbox.Text = "";
                Editor_head_textbox.Enabled = false;
            }
            else
            {
                Editor_head_textbox.Text = "";
                Editor_head_textbox.Enabled = true;

            }
        }
        private void ArchiveOperations_DeleteFile_button_Click(object sender, EventArgs e)
        {
            if (FileBrowser_listbox.SelectedIndices.Count > 0 && MessageBox.Show(@"Are you sure you want to delete the selected files?", @"File removal", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                for (int i = FileBrowser_listbox.SelectedIndices.Count - 1; i >= 0; i--)
                {
                    _files.RemoveAt(FileBrowser_listbox.SelectedIndices[i]);
                    FileBrowser_listbox.Items.RemoveAt(FileBrowser_listbox.SelectedIndices[i]);

                }
                ClearEntryEditor();
            }
        }
        //This region refers to all methods used by the GUI controls of the form.
        //This method exports a RLTOOL script .txt file
        public async void RLPAKTOOL_exportscript_Click(object sender, EventArgs e)
        {
            if (FileBrowser_listbox.SelectedIndices.Count == -1)
            {
                MessageBox.Show(@"Please select a file to export in the filebrowser!", @"Export error message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            List<KeyValuePair<File, string>> tasks = new List<KeyValuePair<File, string>>();
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                string nwmessage = "Preparing...";
                LoadingUpdateLabel(3, nwmessage);
                SendPanelToFront(nwmessage,3); 
                for (int i = 0; i < FileBrowser_listbox.SelectedIndices.Count; i++)
                {
                    File file = _files[FileBrowser_listbox.SelectedIndices[i]];
                    nwmessage = "Preparing: " + file.FileName;
                    LoadingUpdateLabel(3, nwmessage);
                    if (file.FileFormat == FileFormat.Scriptfile)
                    {
                        string outputPath = folderBrowserDialog1.SelectedPath + "/" + file.FileName + ".txt";
                        tasks.Add(new KeyValuePair<File,string>(file,outputPath)); 
                    }
                }
                nwmessage = "Exporting..: ";
                LoadingUpdateLabel(3, nwmessage);
                await Task.Run(() => Parallel.ForEach(tasks, currentFile => { ExportScriptFile(currentFile.Key, currentFile.Value); }));
                SendPanelToBack(3);


                MessageBox.Show(@"Export complete!", @"Export message", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
        }
        private void ArchiveOperations_ImportFile_button_Click(object sender, EventArgs e)
        {
            openFileDialog1.Multiselect = true;
            openFileDialog1.Filter = @"All files (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK && openFileDialog1.FileNames.Length > 0)
            {
                for (int i = openFileDialog1.FileNames.Length - 1; i > -1; i--)
                {
                    File importedfile = new File();
                    importedfile.UnsafeSource = System.IO.File.ReadAllBytes(openFileDialog1.FileNames[i]);
                    importedfile.OriginalLength = importedfile.UnsafeSource.Length;
                    importedfile.FileName = System.IO.Path.GetFileNameWithoutExtension(openFileDialog1.FileNames[i]);
                    if (importedfile.UnsafeSource[0] == 23 && importedfile.UnsafeSource[1] == 3)
                    {
                        //This is a script file
                        importedfile.FileFormat = FileFormat.Scriptfile;
                    }
                    else if (importedfile.UnsafeSource[0] == 22 && importedfile.UnsafeSource[1] == 6)
                    {
                        //This is a variables file
                        importedfile.FileFormat = FileFormat.VariablesFile;
                    }
                    else if (importedfile.UnsafeSource[0] == 16 && importedfile.UnsafeSource[1] == 0 && importedfile.UnsafeSource[2] == 0 && importedfile.UnsafeSource.Skip(8).Take(22).SequenceEqual(new byte[] { 77, 0, 73, 0, 71, 0, 46, 0, 48, 0, 48, 0, 46, 0, 49, 0, 80, 0, 83, 0, 80, 0 }))
                    {
                        //This is a compressed GIM file
                        importedfile.FileFormat = FileFormat.CGim;
                    }
                    else if (importedfile.UnsafeSource.Take(11).SequenceEqual(new byte[] { 77, 73, 71, 46, 48, 48, 46, 49, 80, 83, 80 }))
                    {
                        //This is a GIM file
                        importedfile.FileFormat = FileFormat.Gim;
                    }


                    if (importedfile.FileName != "")
                    {
                        //Make sure the file doesn't exist and correct name!
                        bool foundName = false;
                        string origName = importedfile.FileName;
                        int nmb = 0;
                        do
                        {
                            if (foundName)
                            {
                                nmb++;
                                foundName = false;
                                importedfile.FileName = origName + "(" + nmb + ")";
                            }
                            if (_files.Any(file => file.FileName == importedfile.FileName))
                            {
                                foundName = true;
                            }
                        } while (foundName);
                        if (nmb > 0)
                        {
                            MessageBox.Show(@"Software have changed the filename of your file since it equals one of the other files!");
                        }
                    }

                    
                    if (_currentFileIndex == -1)
                    {
                        _files.Add(importedfile);
                        FileBrowser_listbox.Items.Add(importedfile.FileName + GetFileType(importedfile.FileFormat));
                    }
                    else
                    {
                        _files.Insert(_currentFileIndex + 1, importedfile);
                        FileBrowser_listbox.Items.Insert(_currentFileIndex + 1, importedfile.FileName + GetFileType(importedfile.FileFormat));

                    }
                }

            }
        }
        private void OpenFile_button_Click(object sender, EventArgs e)
        {

            openFileDialog1.Multiselect = false;
            openFileDialog1.Filter = @"RealLife PAK file (*.PAK)|*.PAK|All files (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK && openFileDialog1.FileName != "")
            {
                FileBrowser_listbox.Items.Clear();
                LoadingUpdateLabel(1, "PREPARING TO LOAD FILE");
                SendPanelToFront("PREPARING TO LOAD FILE", 1);
                Task.Run(() =>  OpenFile(openFileDialog1.FileName));
            }

        }
        private void DeleteEntry_button_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(@"Warning! Removing an entry can cause the entire script to mallfunction if anything points to it.",@"Warning", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _files[_currentFileIndex].changed = true;
                if (TextEntries_listbox.SelectedItems.Count > -1)
                {
                    int godo = 0;
                    List<int> fo = TextEntries_listbox.SelectedIndices.Cast<int>().ToList();
                    foreach (int i1 in fo)
                    {
                        if (realentries[i1 - godo].Choices.Count > 0)
                        {
                            MessageBox.Show(@"You cannot delete a user choice!", @"Action error message", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            return;
                        }
                        realentries[i1 - godo].Removed = true;
                        CompileEntry(_files[_currentFileIndex], realentries[i1 - godo]);
                        _textEntriesColors.RemoveAt(i1 - godo);
                        realentries.RemoveAt(i1 - godo);

                        TextEntries_listbox.Items.RemoveAt(i1 - godo);
                        Editor_content_textbox.Text = "";
                        Editor_head_textbox.Text = "";
                        _currentEntryIndex = -1;
                        int co = 0;
                        for (int i = 0; i < realentries.Count; i++)
                        {
                            if (TextEntries_listbox.Items[i].ToString() != "(IGNORED)")
                            {
                                co++;
                            }
                        }
                        TextEntries_label.Text = @"Content (" + co + @" entries)";
                        godo++;
                    }
                    if (TextEntries_listbox.Items.Count > 0)
                        TextEntries_listbox.SelectedIndex = 0;
                }
            }
        }
        //Cast ASCII to ShiftJIS
        string CastToShiftJis(string input)
        {
            if (CastToShiftJIS_CheckBox.Checked)
            {
                string[] castFrom = new string[]
                {
                    "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t",
                    "u",
                    "v", "w", "x", "y", "z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E",
                    "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y",
                    "Z", "-", ",", ".", "!", @"”", "#", "¤", "%", "&", "/", "(", ")", "=", "\"", "½", "<", ">", " ", "?",
                    "'", "*", ":", "@", ";", "[", "]","~"
                };
                string[] castTo = new string[]
                {
                    "ａ", "ｂ", "ｃ", "ｄ", "ｅ", "ｆ", "ｇ", "ｈ", "ｉ", "ｊ", "ｋ", "ｌ", "ｍ", "ｎ", "ｏ", "ｐ", "ｑ", "ｒ", "ｓ", "ｔ",
                    "ｕ",
                    "ｖ", "ｗ", "ｘ", "ｙ", "ｚ", "０", "１", "２", "３", "４", "５", "６", "７", "８", "９", "Ａ", "Ｂ", "Ｃ", "Ｄ", "Ｅ",
                    "Ｆ", "Ｇ", "Ｈ", "Ｉ", "Ｊ", "Ｋ", "Ｌ", "Ｍ", "Ｎ", "Ｏ", "Ｐ", "Ｑ", "Ｒ", "Ｓ", "Ｔ", "Ｕ", "Ｖ", "Ｗ", "Ｘ", "Ｙ",
                    "Ｚ", "－", "，", "．", "！", "”", "＃", "¤", "％", "＆", "／", "（", "）", "＝",
                    "”", "½", "＜", "＞", "　", "？", "’", "＊", "：", "＠", "；", "［", "］","0x81F4"
                };
                char[] inputchars = input.ToCharArray();
                StringBuilder str = new StringBuilder();
                for (int i = 0; i < inputchars.Length; i++)
                {
                    char character = inputchars[i];
                    if (character == '$')
                    {
                        if (i + 1 < inputchars.Length)
                        {
                            switch (inputchars[i+1])
                            {
                                case 'S':
                                    i += 3;
                                    str.Append(input.Substring(i, 5));
                                    continue;
                                default:
                                    i++;
                                    if (i + 2 < input.Length)
                                    {
                                        str.Append(input.Substring(i, 2));

                                    }
                                    else
                                    {
                                        str.Append(input);

                                    } 
                                    continue;
                            }
                        }
                    }
                    if (character == '{')
                    {
                        if (i + 5 < inputchars.Length)
                        {
                            if (input.Substring(i, 5) == "{UHEX")
                            {
                                str.Append(input.Substring(i, input.Substring(i).IndexOf("}", StringComparison.Ordinal) + 1));
                                i +=  input.Substring(i).IndexOf("}", StringComparison.Ordinal);
                                continue;
                            }
                        }
                    }
                    if (castFrom.Contains(character.ToString()))
                    {
                        string v = castTo[Array.IndexOf(castFrom, character.ToString())];
                        if (v.StartsWith("0x"))
                        {
                            string hv = string.Join("", StringToByteArray(v.Substring(2)).Select(x => "{UHEX:" + x + "}").ToArray());
                            str.Append(hv);
                        }
                        else
                        {
                            str.Append(v);

                        } 
                    }
                    else
                    {
                        str.Append(character);

                    }
                }
                return str.ToString();
            }
            else
            {
                return input;
            }

        }

        private void SaveChanges_button_Click(object sender, EventArgs e)
        {
           
            if (_currentEntryIndex == -1)
                return;
            var conflict = (realentries[_currentEntryIndex].content == "" || realentries[_currentEntryIndex].content == "　" || !(new byte[]{24,25}).Contains(realentries[_currentEntryIndex].LastCurrent)) ;
            if (_currentEntryIndex > -1 && (!conflict ||  MessageBox.Show(@"This entry may be a command since it had no content from the beginning or is a pop up message, proceed anyway with overwrite/save?", @"Warning question", MessageBoxButtons.YesNo) == DialogResult.Yes))
            {
                _files[_currentFileIndex].changed = true;


                if (realentries[TextEntries_listbox.SelectedIndex].Choices.Count > 0)
                {
                    if (Editor_Choices_listbox.SelectedIndex != -1)
                    {

                        realentries[TextEntries_listbox.SelectedIndex].Choices[Editor_Choices_listbox.SelectedIndex] = CastToShiftJis(Editor_choice_textbox.Text);
                        Editor_Choices_listbox.Items[Editor_Choices_listbox.SelectedIndex] = CastToShiftJis(Editor_choice_textbox.Text);
                    }
                }
                realentries[TextEntries_listbox.SelectedIndex].content = CastToShiftJis(Editor_content_textbox.Text);
                realentries[TextEntries_listbox.SelectedIndex].head = CastToShiftJis(Editor_head_textbox.Text);

                byte endCode;
                byte.TryParse(Endcode_Value_TextBox.Text, out endCode);
                realentries[_currentEntryIndex].LastCMDs = new byte[] {0, endCode};

                if (PopUp_Checkbox.Checked && (realentries[TextEntries_listbox.SelectedIndex].LastCurrent != 24 || (realentries[TextEntries_listbox.SelectedIndex].LastCurrent == 24 && MessageBox.Show(@"This entry may have a voice track attached to it, saving it will remove the voicetrack as well, proceed?", @"Warning question", MessageBoxButtons.YesNo) == DialogResult.Yes)))
                {
                    if (realentries[TextEntries_listbox.SelectedIndex].LastCurrent != 44)
                    {
                        byte[] presetInfo = new byte[] { 21, 0, 0, 0, 3, 0 };
                        realentries[TextEntries_listbox.SelectedIndex].inf = presetInfo;
                        realentries[TextEntries_listbox.SelectedIndex].LastCurrent = 44;
                    }
                }
                else
                {
                    if (realentries[TextEntries_listbox.SelectedIndex].LastCurrent == 44)
                    {
                        realentries[TextEntries_listbox.SelectedIndex].inf = new byte[0];
                        realentries[TextEntries_listbox.SelectedIndex].LastCurrent = 25;
                    }
                }
        
                if (realentries[TextEntries_listbox.SelectedIndex].Choices.Count == 0)
                {
                    TextEntries_listbox.Items[_currentEntryIndex] = Editor_head_textbox.Text.Replace("\n", "") + " {" + Editor_content_textbox.Text.Replace("\n", "") + "}";
                }
                realentries[TextEntries_listbox.SelectedIndex].head = AutoInsertContentTextBox.Text + removeSizeCMD(realentries[TextEntries_listbox.SelectedIndex].head);
                realentries[TextEntries_listbox.SelectedIndex].content = AutoInsertContentTextBox.Text + removeSizeCMD(realentries[TextEntries_listbox.SelectedIndex].content);

              
                CompileEntry(_files[_currentFileIndex], realentries[_currentEntryIndex]);

                TextEntries_listbox.Refresh();
                Editor_choice_textbox.Text = "";
                Editor_content_textbox.Text = "";
                Editor_head_textbox.Text = "";
                _currentEntryIndex = -1;
                Editor_tabcontrol.SelectedIndex = 0;
                EnableTab(Editor_tabcontrol.TabPages[0], true);
                EnableTab(Editor_tabcontrol.TabPages[1], false);
              
            }
        }
        private void TextEntries_listbox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
                return;
            e.DrawBackground();
            Graphics g = e.Graphics;
            // draw the background color you want
            // mine is set to olive, change it to whatever you want
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                g.FillRectangle(SystemBrushes.Highlight, e.Bounds);

            }
            else
            {

                g.FillRectangle(new SolidBrush(_textEntriesColors[e.Index]), e.Bounds);
            }
            // draw the text of the list item, not doing this will only show
            // the background color
            // you will need to get the text of item to display

            g.DrawString("[" + e.Index + "] " + TextEntries_listbox.Items[e.Index], e.Font, new SolidBrush(e.ForeColor), new PointF(e.Bounds.X, e.Bounds.Y));

            e.DrawFocusRectangle();
        }
        private void TextEntries_listbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Editor_choice_textbox.Text = "";
            Editor_Choices_listbox.Items.Clear();
            PointerList_Listbox.Items.Clear();
            if (TextEntries_listbox.SelectedItems.Count == 1 && TextEntries_listbox.SelectedIndex > -1)
            {

                PopUp_Checkbox.Enabled = true;
                Editor_entrypositionvalue_label.Text = (_files[_currentFileIndex].FileStart + realentries[TextEntries_listbox.SelectedIndex].UnsafeHexPos).ToString();
                Editor_filestartvalue_label.Text = _files[_currentFileIndex].FileStart.ToString();
                Editor_fileendvalue_label.Text = _files[_currentFileIndex].FileMax.ToString();
                Editor_lengthvalue_label.Text = _files[_currentFileIndex].UnsafeSource.Length.ToString();
                Editor_pointedfromvalue_label.Text = realentries[TextEntries_listbox.SelectedIndex].PointedFrom.Count.ToString();
                if (realentries[TextEntries_listbox.SelectedIndex].PointTo.Count > 0)
                {
                    Editor_tabcontrol.SelectedIndex = 3;
                    EnableTab(Editor_tabcontrol.TabPages[0], false);
                    EnableTab(Editor_tabcontrol.TabPages[1], false);
                    EnableTab(Editor_tabcontrol.TabPages[3], true);
                    foreach (Entry entry in realentries[TextEntries_listbox.SelectedIndex].PointTo)
                    {
                        PointerList_Listbox.Items.Add(realentries.IndexOf(entry));
                    }
                }
                else if (realentries[TextEntries_listbox.SelectedIndex].LastCurrent == 22)
                {
                    PopUp_Checkbox.Enabled = false;
                    PopUp_Checkbox.Checked = false;
                    Editor_tabcontrol.SelectedIndex = 0;
                    EnableTab(Editor_tabcontrol.TabPages[0], true);
                    EnableTab(Editor_tabcontrol.TabPages[1], false);
                    EnableTab(Editor_tabcontrol.TabPages[3], false);
                    Editor_head_textbox.Text = "";
                 
                        Editor_content_textbox.Text = realentries[TextEntries_listbox.SelectedIndex].content;

                    

                    Editor_content_textbox.Enabled = true;
                    Editor_head_textbox.Enabled = false;

                    PopUp_Checkbox.Enabled = false;
                }
                else
                {
                    if (realentries[TextEntries_listbox.SelectedIndex].LastCurrent == 28)
                    {
                        PopUp_Checkbox.Enabled = false;
                        PopUp_Checkbox.Checked = false;
                        Editor_tabcontrol.SelectedIndex = 1;
                        EnableTab(Editor_tabcontrol.TabPages[1], true);
                        EnableTab(Editor_tabcontrol.TabPages[0], false);
                        EnableTab(Editor_tabcontrol.TabPages[3], false);
                        foreach (string item in realentries[TextEntries_listbox.SelectedIndex].Choices)
                        {
                         
                              
                                    Editor_Choices_listbox.Items.Add(item);
                                

                            
                        }
                    }
                    else
                    {
                        Editor_tabcontrol.SelectedIndex = 0;
                        EnableTab(Editor_tabcontrol.TabPages[0], true);
                        EnableTab(Editor_tabcontrol.TabPages[1], false);
                        EnableTab(Editor_tabcontrol.TabPages[3], false);
                       
                            Editor_content_textbox.Text = realentries[TextEntries_listbox.SelectedIndex].content;

                        

                            Editor_head_textbox.Text = realentries[TextEntries_listbox.SelectedIndex].head;

                        
                        Editor_content_textbox.Enabled = true;
                        Editor_head_textbox.Enabled = true;

                        PopUp_Checkbox.Enabled = true;
                    }
                }
                if (realentries[TextEntries_listbox.SelectedIndex].LastCurrent == 44)
                {
                    PopUp_Checkbox.Checked = true;
                }
                else
                {
                    PopUp_Checkbox.Checked = false;
                }
                _currentEntryIndex = TextEntries_listbox.SelectedIndex;
                SaveChanges_button.Enabled = true;
                AddEntry_button.Enabled = true;
                SearchGroupbox.Enabled = true;
                SplitMessages_Button.Enabled = true;
                FindAndReplace_button.Enabled = true;
                Recompile_all_button.Enabled = true;
                FindMisformedline_button.Enabled = true;
                Delete_button.Enabled = true;
                Ignore_button.Enabled = true;
                    Endcode_Value_TextBox.Text =
                        realentries[TextEntries_listbox.SelectedIndex].contentBytes.Last().ToString();
                PlainHex_TextBox.Text = BitConverter.ToString(realentries[TextEntries_listbox.SelectedIndex].contentBytes);

            }

        }
        
        bool a = false;
        private async void FileBrowser_listbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (a)
            {
                return;
            }
            if (FileBrowser_listbox.SelectedIndices.Count  == 1)
            {
                TextEntries_listbox.Items.Clear();
                _currentFileIndex = FileBrowser_listbox.SelectedIndex;
                _currentEntryIndex = -1;
                Editor_content_textbox.Text = "";
                Editor_Choices_listbox.Items.Clear();
                Editor_tabcontrol.SelectedIndex = 0;
                Editor_tabcontrol.Enabled = true;
                EnableTab(Editor_tabcontrol.TabPages[0], true);
                EnableTab(Editor_tabcontrol.TabPages[1], false);
                Editor_choice_textbox.Text = "";
                Editor_filestartvalue_label.Text = _files[FileBrowser_listbox.SelectedIndex].FileStart.ToString();
                Editor_fileendvalue_label.Text = _files[FileBrowser_listbox.SelectedIndex].FileMax.ToString();

                if (_files[FileBrowser_listbox.SelectedIndex].entries.Count == 0)
                {
                    if (_files[FileBrowser_listbox.SelectedIndex].FileFormat == FileFormat.Scriptfile || _files[FileBrowser_listbox.SelectedIndex].FileFormat == FileFormat.VariablesFile)
                    {
                        _files[FileBrowser_listbox.SelectedIndex].entries = GetBlocksOfFile(_files[FileBrowser_listbox.SelectedIndex].UnsafeSource).ToList();
                        SendPanelToFront("Preparing content",1);
                        await Task.Run(() => LoadFile(_files[_currentFileIndex]));

                        EndLoadFile();
                    }
                    else
                    {
                        ArchiveOperations_ExtractFile_button.Enabled = true;
                        ArchiveOperations_RenameFile_button.Enabled = true;
                        ArchiveOperations_DeleteFile_button.Enabled = true;
                        ArchiveOperations_FileUp_button.Enabled = true;
                        ArchiveOperations_FileDown_button.Enabled = true;
                    }
                }
                else
                {
                    EndLoadFile();
                }
            }
            else
            {

                if (FileBrowser_listbox.SelectedIndices.Count > 0)
                {
                    ArchiveOperations_ExtractFile_button.Enabled = false;
                    ArchiveOperations_RenameFile_button.Enabled = false;
                    ArchiveOperations_DeleteFile_button.Enabled = true;
                    ArchiveOperations_FileUp_button.Enabled = false;
                    ArchiveOperations_FileDown_button.Enabled = false;


                }
                else
                {
                    ArchiveOperations_ExtractFile_button.Enabled = false;
                    ArchiveOperations_RenameFile_button.Enabled = false;
                    ArchiveOperations_DeleteFile_button.Enabled = false;
                    ArchiveOperations_FileUp_button.Enabled = false;
                    ArchiveOperations_FileDown_button.Enabled = false;

                }
            }

        }
        private void SaveFile_button_Click(object sender, EventArgs e)
        {

            Editor_content_textbox.Text = "";
            Editor_head_textbox.Text = "";
            _currentEntryIndex = -1;
            Editor_tabcontrol.SelectedIndex = 0;
            EnableTab(Editor_tabcontrol.TabPages[0], true);
            EnableTab(Editor_tabcontrol.TabPages[1], false);
            string nwmessage = "LOADING 0%";

            LoadingUpdateLabel(1,nwmessage);

            SendPanelToFront(nwmessage,1);
            Task.Run(() => SaveFunc());
        }
        private void AddEntry_button_Click(object sender, EventArgs e)
        {
                _files[_currentFileIndex].changed = true;
                Editor_content_textbox.Text = "";
                Editor_head_textbox.Text = "";
                _currentEntryIndex = -1;
                Entry entry = new Entry();
                entry.Added = true;
                entry.head = "Ｎｅｗ　Ｃｏｎｔｅｎｔ";
                entry.content = "ｅｎｔｒｙ　ｃｏｎｔｅｎｔ";
        
                entry.LastCurrent = 25;
                if (TextEntries_listbox.SelectedIndex + 1 > -1)
                {
                    _files[FileBrowser_listbox.SelectedIndex].entries.Insert(_files[FileBrowser_listbox.SelectedIndex].entries.IndexOf(realentries[TextEntries_listbox.SelectedIndex]) + 1, entry);
                }
                TextEntries_listbox.Items.Insert(TextEntries_listbox.SelectedIndex + 1, entry.head.Replace("\n", "") + " { " + entry.content.Replace("\0", "").Replace("\n", "") + " }");
                _textEntriesColors.Insert(TextEntries_listbox.SelectedIndex + 1, Color.Yellow);
                realentries.Insert(TextEntries_listbox.SelectedIndex + 1, entry);
                int co = 0;
                for (int i = 0; i < realentries.Count; i++)
                {
                    if (TextEntries_listbox.Items[i].ToString() != "(IGNORED)")
                    {
                        co++;
                    }
                }

                CompileEntry(_files[_currentFileIndex], entry);
                TextEntries_label.Text = @"Content (" + co + @" entries)";
            
        }
        private void ExternalScript_OpenFile_button_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = @"RLDEV uncompressed file (*.uncompressed)|*.uncompressed";

            openFileDialog1.Multiselect = false;
            if (openFileDialog1.ShowDialog() == DialogResult.OK && openFileDialog1.FileName != "")
            {
                string extention = System.IO.Path.GetExtension(openFileDialog1.FileName);
                switch (extention)
                {

                    case ".uncompressed":
                        try
                        {
                            ExternalScript_entries_listbox.Items.Clear();
                            byte[] content = System.IO.File.ReadAllBytes(openFileDialog1.FileName);
                            for (int i = 0; i < content.Length; i++)
                            {
                                byte btcode = content[i];
                                if (btcode == 10)
                                {
                                    //Identify
                                    switch (content[i + 3])
                                    {
                                        case 32:
                                            if (content[i + 2] == 34)
                                            {
                                                i += 6;
                                            }
                                            else
                                            {
                                                i += 5;

                                            }


                                            break;
                                        case 64:
                                            i += 7;
                                            break;
                                        default:
                                            continue;
                                    }
                                    List<byte> btt = new List<byte>();
                                    for (int index = i; index < content.Length; index++)
                                    {
                                        if (content[index + 1] == 35)
                                        {
                                            i = index + 1;
                                            break;
                                        }
                                        btt.Add(content[index]);

                                    }
                                    ExternalScript_entries_listbox.Items.Add(System.Text.Encoding.GetEncoding("shift-jis").GetString(btt.ToArray()));
                                }

                            }



                            ExternalScript_entries_label.Text = @"Entries (" + ExternalScript_entries_listbox.Items.Count + @" entries)";
                            if (realentries.Count > 0)
                            {
                                SelectNextExternalScript_button.Enabled = true;
                                ReplaceNextExternalScript_button.Enabled = true;
                                ExternalScript_ReplaceAll_button.Enabled = true;
                                ExternalScriptSearch_textbox.Enabled = true;
                                SearchExternalScript_button.Enabled = true;
                                ExternalScript_RemoveSelected_button.Enabled = true;

                            }
                            else
                            {
                                SelectNextExternalScript_button.Enabled = false;
                                ReplaceNextExternalScript_button.Enabled = false;
                                ExternalScript_ReplaceAll_button.Enabled = false;
                                ExternalScriptSearch_textbox.Enabled = false;
                                SearchExternalScript_button.Enabled = false;
                                ExternalScript_RemoveSelected_button.Enabled = false;
                            }
                        }
                        catch
                        {
                            SelectNextExternalScript_button.Enabled = false;
                            ReplaceNextExternalScript_button.Enabled = false;
                            ExternalScriptSearch_textbox.Enabled = false;
                            SearchExternalScript_button.Enabled = false;
                            ExternalScript_ReplaceAll_button.Enabled = false;
                            MessageBox.Show(@"An error occurred when openning the file!", @"Error message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        break;
                }
            
            }
        }
        private void Ignore_button_Click(object sender, EventArgs e)
        {
            if (TextEntries_listbox.SelectedIndex > -1)
            {
                TextEntries_listbox.Items[TextEntries_listbox.SelectedIndex] = "(IGNORED)";
            }
            int co = 0;
            for (int i = 0; i < realentries.Count; i++)
            {
                if (TextEntries_listbox.Items[i].ToString() != "(IGNORED)")
                {
                    co++;
                }
            }
            TextEntries_label.Text = @"Content (" + co + @" entries)";
        }
        private void ExternalScript_entries_listbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ExternalScript_entries_listbox.SelectedIndex > -1)
            {
                string tow = ExternalScript_entries_listbox.Items[ExternalScript_entries_listbox.SelectedIndex].ToString();
                string head = "";
                if (tow.StartsWith(@"""【"))
                {
                    head = tow.Substring(tow.IndexOf("【", StringComparison.Ordinal) + 2);
                    head = head.Substring(0, head.IndexOf("】", StringComparison.Ordinal) - 1);
                    tow = tow.Substring(tow.IndexOf(@"】", StringComparison.Ordinal) + 1);
                }
                head = head.Replace("\\", "");
                tow = tow.Replace("\\", "");
                Editor_head_textbox.Text = head;
                if (tow.StartsWith(@""""))
                {
                    tow = tow.Substring(1);
                }
                Editor_content_textbox.Text = tow;
            }
        }
        private void ExternalScript_ReplaceAll_button_Click(object sender, EventArgs e)
        {
            if (TextEntries_listbox.Items.Count == 0)
            {
                return;
            }
            if (Counteditableentries() != ExternalScript_entries_listbox.Items.Count)
            {
                MessageBox.Show(@"Number of entries in external files has to be equals the number of entries in the script file!", @"Error message", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
            if (TextEntries_listbox.Items.Count > 0)
            {
                TextEntries_listbox.SelectedIndex = 0;
                ExternalScript_entries_listbox.SelectedIndex = 0;
                SaveChanges_button_Click(null, null);
                do
                {
                    while (TextEntries_listbox.Items[TextEntries_listbox.SelectedIndex].ToString() == "(IGNORED)")
                    {
                        TextEntries_listbox.SelectedIndex++;
                    }
                    while (!(new byte[] { 25, 24 }).Contains(realentries[TextEntries_listbox.SelectedIndex].LastCurrent))
                    {
                        TextEntries_listbox.SelectedIndex++;
                    }
                    SaveChanges_button_Click(null, null);
                    TextEntries_listbox.SelectedIndex++;
                    ExternalScript_entries_listbox.SelectedIndex++;
                }
                while (ExternalScript_entries_listbox.SelectedIndex + 1 < ExternalScript_entries_listbox.Items.Count);
            }

        }
        private void ExternalScript_RemoveSelected_button_Click(object sender, EventArgs e)
        {
            if (ExternalScript_entries_listbox.SelectedIndex > -1)
            {
                ExternalScript_entries_listbox.Items.RemoveAt(ExternalScript_entries_listbox.SelectedIndex);
                ExternalScript_entries_label.Text = @"Entries (" + ExternalScript_entries_listbox.Items.Count + @" entries)";
            }
        }
        private void SelectNextExternalScriptEntry_button_Click(object sender, EventArgs e)
        {
            if (ExternalScript_entries_listbox.SelectedIndex > -1 && ExternalScript_entries_listbox.SelectedIndex != ExternalScript_entries_listbox.Items.Count - 1 && TextEntries_listbox.SelectedIndex > -1 && TextEntries_listbox.SelectedIndex != TextEntries_listbox.Items.Count - 1)
            {
                TextEntries_listbox.SelectedIndex++;
                ExternalScript_entries_listbox.SelectedIndex++;

            }
        }
        private void ReplaceNextExternalScriptEntry_button_Click(object sender, EventArgs e)
        {
            SelectNextExternalScriptEntry_button_Click(null, null);
            SaveChanges_button_Click(null, null);
        }
        private void Editor_Choices_listbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Editor_Choices_listbox.SelectedIndex > -1)
            {
           
                    Editor_choice_textbox.Text = realentries[TextEntries_listbox.SelectedIndex].Choices[Editor_Choices_listbox.SelectedIndex];
                
            }
        }
        private void Search_button_Click(object sender, EventArgs e)
        {
            for (int i = Math.Max(0, TextEntries_listbox.SelectedIndex); i < TextEntries_listbox.Items.Count; i++)
            {
                if (EntryContainsText(realentries[i], Search_textbox.Text))
                {
                    TextEntries_listbox.SelectedIndex = i;
                    break;
                }
            }
        }

        private bool EntryContainsText(Entry entry, string searchString)
        {
            string a1Head = entry.head.ToUpper();
            string a1Content = entry.content.ToUpper();
            string s1S = searchString.ToUpper();
            string a1ContentDecoded =
    Encoding.GetEncoding(1252)
        .GetString(
            Encoding.GetEncoding(1252).GetBytes(a1Content));

            string a1HeadDecoded =
    Encoding.GetEncoding(1252)
        .GetString(
            Encoding.GetEncoding(1252).GetBytes(a1Head));

            string s1 = Encoding.GetEncoding(1252)
                    .GetString(
                        Encoding.GetEncoding(1252, new EncoderReplacementFallback("{INVALIDCHAR}<<"),
                            new DecoderReplacementFallback("{INVALIDCHAR}<<")).GetBytes(s1S))
                    .Replace("{INVALIDCHAR}<<", ""); ;

            if (((s1S.Length == s1.Length && a1ContentDecoded.Contains(s1)) || a1Content.Contains(s1S)) || ((s1S.Length == s1.Length && a1HeadDecoded.Contains(s1)) || a1Head.Contains(s1S)))
            {
                return true;
            }
            return false;
        }

        private void SearchExternalScript_button_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ExternalScript_entries_listbox.Items.Count; i++)
            {
                if (ExternalScript_entries_listbox.Items[i].ToString().ToUpper().Contains(ExternalScriptSearch_textbox.Text.ToUpper()))
                {
                    ExternalScript_entries_listbox.SelectedIndex = i;
                    break;
                }
            }
        }
        private void ReloadFile_button_Click(object sender, EventArgs e)
        {
            if (OpenedFilePath != "")
            {
                ReloadFile();
                Editor_Choices_listbox.Items.Clear();
                Editor_tabcontrol.SelectedIndex = 0;
                EnableTab(Editor_tabcontrol.TabPages[0], true);
                EnableTab(Editor_tabcontrol.TabPages[1], false);
                Editor_choice_textbox.Text = "";
                Editor_head_textbox.Text = "";
                Editor_content_textbox.Text = "";
                _currentEntryIndex = -1;
            }
        }
        private void SplitMessages_Button_Click(object sender, EventArgs e)
        {
            SplitMessagesDialog dialog = new SplitMessagesDialog();
            dialog.ShowDialog();
            if (dialog.Process)
            {
                _files[_currentFileIndex].changed = true;
                Editor_content_textbox.Text = "";
                Editor_head_textbox.Text = "";
                _currentEntryIndex = -1;
                int changedCount = 0;
                for (int i = 0; i < realentries.Count; i++)
                {
                    Entry entry = realentries[i];
                    if (entry.Choices.Count == 0 && entry.content.Length > dialog.MaxLength)
                    {
                        List<string> firstmessage = entry.content.Split('　').ToList();
                        List<string> secondmessage = new List<string>();
                        int len = 0;
                        for (int ia = 0; ia < firstmessage.Count; ia++)
                        {
                            if (len + firstmessage[ia].Length + 1 > dialog.MaxLength)
                            {
                                do
                                {
                                    secondmessage.Add(firstmessage[ia]);
                                    firstmessage.RemoveAt(ia);
                                }
                                while (ia < firstmessage.Count);
                                break;
                            }
                            else
                            {
                                len += firstmessage[ia].Length + 1;
                            }
                        }
                        if (firstmessage.Count > 0)
                        {
                            entry.content = string.Join("　", firstmessage.ToArray()) + dialog.SplitWithWord;
                          
                            //Insert new entry
                            Entry newEntry = new Entry();
                            newEntry.head = entry.head;                     
                            newEntry.content = string.Join("　", secondmessage.ToArray());                          
                            newEntry.LastCurrent = 25; //A fixed 25 prevents voice from being played two times in a row.

                            TextEntries_listbox.Items[i] = "[splitorig] " + entry.head.Replace("\0", "").Replace("\n", "") + " { " + entry.content.Replace("\0", "").Replace("\n", "") + " }";
                            _files[_currentFileIndex].entries.Insert(_files[_currentFileIndex].entries.IndexOf(realentries[i]) + 1, newEntry);
                            TextEntries_listbox.Items.Insert(i + 1, "[splitted] " +  newEntry.head.Replace("\n", "") + " { " + newEntry.content.Replace("\0", "").Replace("\n", "") + " }");
                            _textEntriesColors.Insert(i + 1, Color.Yellow);
                            realentries.Insert(i + 1, newEntry);
                            CompileEntry(_files[_currentFileIndex], newEntry);
                            CompileEntry(_files[_currentFileIndex], entry);
                            changedCount++;
                        }
                    }
                }

                TextEntries_label.Text = @"Content (" + Counteditableentries() + @" entries)";
                MessageBox.Show(changedCount + @" was changed.", @"Process information", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }

        }
        #endregion
        #endregion
        #region Other methods
        //CZ2 decompress thread
        public void DeCompressCz(CompressedCz czCompressor, string outputFile)
        {
            try
            {
                byte[] decompressedbytes = czCompressor.DecompressCz();
                System.IO.File.WriteAllBytes(outputFile, decompressedbytes);
            }
            catch
            {
                MessageBox.Show(@"GIM compression failed!", @"GimCompressor", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            EnableCompressionControls();

        }
        //GIM decompress thread
        public void DeCompressGim(GimCompressor gimCompressor, string outputFile)
        {
            try
            {
            byte[] decompressedbytes = gimCompressor.DecompressGim();
            System.IO.File.WriteAllBytes(outputFile, decompressedbytes);
            }
            catch
            {
                MessageBox.Show(@"GIM compression failed!", @"GimCompressor", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            EnableCompressionControls();

        }
        //GIM compress thread
        public void DeCompressGim(string[] files, string outputFolder)
        {
            for (int i = 0; i < files.Length; i++)
            {
                try
                {
                  
                        UpdateProgressBar(Convert.ToInt32(100.0 / Convert.ToDouble(files.Length) * i), System.IO.Path.GetFileName(files[i]));
                        GimCompressor gimCompressor = new GimCompressor(System.IO.File.ReadAllBytes(files[i]));
                        byte[] decompressedbytes = gimCompressor.DecompressGim();
                        System.IO.File.WriteAllBytes(outputFolder + @"\" + System.IO.Path.GetFileNameWithoutExtension(files[i]) + ".gim", decompressedbytes);
                    
                }
                catch
                {
                    // ignored
                }
            }
            EnableCompressionControls();
        }
        //GIM compress thread
        public void DeCompressCz(string[] files, string outputFolder)
        {
            for (int i = 0; i < files.Length; i++)
            {
                try
                {
                   
                        UpdateProgressBar(Convert.ToInt32(100.0 / Convert.ToDouble(files.Length) * i), System.IO.Path.GetFileName(files[i]));
                        CompressedCz gimCompressor = new CompressedCz(System.IO.File.ReadAllBytes(files[i]));
                        byte[] decompressedbytes = gimCompressor.DecompressCz();
                        System.IO.File.WriteAllBytes(outputFolder + @"\" + System.IO.Path.GetFileNameWithoutExtension(files[i]) + ".png", decompressedbytes);
                    
                }
                catch
                {
                    // ignored
                }
            }
            EnableCompressionControls();
        }
        //GIM compress thread
        public void CompressGim(string[] files,string outputFolder)
        {
            for (int i =0;i<files.Length;i++)
            {
                try
                {
                    if (System.IO.Path.GetExtension(files[i]) == ".gim")
                    {
                        UpdateProgressBar(Convert.ToInt32(100.0 / Convert.ToDouble(files.Length) * i), System.IO.Path.GetFileName(files[i]));
                        GimCompressor gimCompressor = new GimCompressor(System.IO.File.ReadAllBytes(files[i]));
                        byte[] compressedbytes = gimCompressor.CompressGim();
                        System.IO.File.WriteAllBytes(outputFolder + @"\" + System.IO.Path.GetFileNameWithoutExtension(files[i]) + ".cgim", compressedbytes);
                    }
                }
                catch
                {
                    // ignored
                }
            }
            EnableCompressionControls();

        }
        //CZ2 compress thread
        public void CompressCz2(string[] files, string outputFolder)
        {
            for (int i = 0; i < files.Length; i++)
            {
                try
                {
                    if (System.IO.Path.GetExtension(files[i]) == ".png")
                    {
                        UpdateProgressBar(Convert.ToInt32(100.0 / Convert.ToDouble(files.Length) * i), System.IO.Path.GetFileName(files[i]));
                        CompressedCz gimCompressor = new CompressedCz(System.IO.File.ReadAllBytes(files[i]));
                        byte[] compressedbytes = gimCompressor.CompressCz(true);
                        System.IO.File.WriteAllBytes(outputFolder + @"\" + System.IO.Path.GetFileNameWithoutExtension(files[i]) + ".cz2", compressedbytes);
                    }
                }
                catch
                {
                    // ignored
                }
            }
            EnableCompressionControls();

        }
        //GIM compress thread
        public void CompressCz2(CompressedCz gimCompressor, string outputFile, bool isCz2)
        {
            try
            {
                byte[] decompressedbytes = gimCompressor.CompressCz(isCz2);
                System.IO.File.WriteAllBytes(outputFile, decompressedbytes);
            }
            catch
            {
                MessageBox.Show(@"CZ2 compression failed!", @"GimCompressor", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            EnableCompressionControls();

        }
        //GIM compress thread
        public void CompressGim(GimCompressor gimCompressor, string outputFile)
        {
            try
            {
                byte[] decompressedbytes = gimCompressor.CompressGim();
                System.IO.File.WriteAllBytes(outputFile, decompressedbytes);
            }
            catch
            {
                MessageBox.Show(@"GIM compression failed!", @"GimCompressor", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            EnableCompressionControls();

        }
        //GIM compression, percentage changed
        public void PercentageChanged(object sender, EventArgs e)
        {
            if (sender.GetType() == typeof(GimCompressor))
                UpdateProgressBar(((GimCompressor)sender).Percentage, ((GimCompressor)sender).FileName);
            if (sender.GetType() == typeof(CompressedCz))
                UpdateProgressBar(((CompressedCz)sender).Percentage, ((CompressedCz)sender).FileName);
        }
        //Counts the number of string occurences
        public int CountStringOccurrences(string text, string pattern)
        {
            // Loop through all instances of the string 'text'.
            int count = 0;
            int i = 0;
            while ((i = text.IndexOf(pattern, i, StringComparison.Ordinal)) != -1)
            {
                i += pattern.Length;
                count++;
            }
            return count;
        }
        //This method converts a hex string to a byte array
        public byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
        //This cleans the formfields to default state
        void ClearEntryEditor()
        {
            PopUp_Checkbox.Checked = false;
            _currentFileIndex = -1;
            TextEntries_listbox.Items.Clear();
            realentries.Clear();
            TextEntries_label.Text = @"Content (0 entries)";
            AddEntry_button.Enabled = false;
            Delete_button.Enabled = false;
            Ignore_button.Enabled = false;
            SplitMessages_Button.Enabled = false;
            FileBrowser_listbox.SelectedIndex = -1;
            Editor_tabcontrol.Enabled = false;
            Editor_tabcontrol.SelectedIndex = 0;
            Editor_head_textbox.Text = "";
            Editor_content_textbox.Text = "";
            Editor_choice_textbox.Text = "";
            Editor_Choices_listbox.Items.Clear();
            Editor_entrypositionvalue_label.Text = @"?";
            Editor_filestartvalue_label.Text = @"?";
            Editor_fileendvalue_label.Text = @"?";
            Editor_pointedfromvalue_label.Text = @"?";
            Editor_lengthvalue_label.Text = @"?";
        }
        //Extract all files
        void ExtractFiles()
        {
            try
            {
                int nonameCount = 0;
                foreach (File file in _files)
                {
                    string filename = file.FileName;
                    if (filename == "")
                    {
                        filename = "noname_" + nonameCount;
                        nonameCount++;
                    }
                    LoadingUpdateLabel(1,"Extracting " + filename + GetFileType(file.FileFormat));
                    //Save the file
                    if (file.changed)
                    {
                        System.IO.File.WriteAllBytes(folderBrowserDialog1.SelectedPath + @"\" + filename + GetFileType(file.FileFormat), GetBytesOfEntries(file.entries.ToArray()));
                    }
                    else
                    {
                        System.IO.File.WriteAllBytes(folderBrowserDialog1.SelectedPath + @"\" + filename + GetFileType(file.FileFormat), file.UnsafeSource);

                    }
                }
                EndExtractFiles(true);
            }
            catch
            {

                EndExtractFiles(false);
            }
        }
        //Extract a given file
        void ExtractFile(File file)
        {
            try
            {
                if (file.FileFormat == FileFormat.Scriptfile)
                {
                    //Save the file
                    System.IO.File.WriteAllBytes(saveFileDialog1.FileName, file.UnsafeSource);
                }
                else
                {

                    System.IO.File.WriteAllBytes(saveFileDialog1.FileName, file.UnsafeSource);
                }
                EndExtractFile(true);
            }
            catch
            {
                EndExtractFile(false);

            }

        }
        //Count amount of editable entries
        int Counteditableentries()
        {
            int co = 0;
            for (int i = 0; i < realentries.Count; i++)
            {
                if (TextEntries_listbox.Items[i].ToString() != "(IGNORED)")
                {
                    co++;
                }
            }
            return co;
        }
        //This void updates the loading label shown when loading and saving.
        public void LoadingUpdateLabel(int priority, string text)
        {

            if (priority >= currentPriority)
            {
            if (InvokeRequired)
            {
                // It's on a different thread, so use Invoke.
                BeginInvoke(new MethodInvoker(() => LoadingUpdateLabel(priority,text)));
            }
            else
            {
                    label9.Text = text;
                    currentPriority = priority;

            }
            }
        }
        //This method is invoked when the header has been loaded for an archive.
        public void EndOpenFile(bool success)
        {
            if (InvokeRequired)
            {
                // It's on a different thread, so use Invoke.
                BeginInvoke(new MethodInvoker(() => EndOpenFile(success)));
            }
            else
            {
                if (success)
                {
                    foreach (File script in _files)
                    {
                        FileBrowser_listbox.Items.Add(script.FileName + GetFileType(script.FileFormat));
                    }

                    Editor_Choices_listbox.Items.Clear();
                    Editor_tabcontrol.SelectedIndex = 0;
                    EnableTab(Editor_tabcontrol.TabPages[0], true);
                    EnableTab(Editor_tabcontrol.TabPages[1], false);
                    Editor_choice_textbox.Text = "";
                    Editor_head_textbox.Text = "";
                    Editor_content_textbox.Text = "";
                    _currentEntryIndex = -1;
                    SaveFile_button.Enabled = true;
                    ReloadFile_button.Enabled = true;
                    EnableControls();
                }
                else
                {
                    DisableControls();
                    SaveFile_button.Enabled = false;
                    ReloadFile_button.Enabled = false;
                    MessageBox.Show(@"An error occurred when openning the file!", @"Error message", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
                SendPanelToBack(1);
            }
        }
        //This method is invoked when the header has been loaded for an archive.
        public void PrepareLoadSteps()
        {
            if (InvokeRequired)
            {
                // It's on a different thread, so use Invoke.
                BeginInvoke(new MethodInvoker(PrepareLoadSteps));
            }
            else
            {
                ClearEntryEditor();
                TextEntries_listbox.Items.Clear();
                FileBrowser_listbox.Items.Clear();
                Editor_content_textbox.Text = "";
                Editor_head_textbox.Text = "";
            }
        }
        //This void reloads the opened file
        public void ReloadFile()
        {
            if (InvokeRequired)
            {
                // It's on a different thread, so use Invoke.
                BeginInvoke(new MethodInvoker(() => ReloadFile()));
            }
            else
            {

                OpenFile(OpenedFilePath);
                SendPanelToBack(1);
            }
        }
        //This void removes the backpanel
        public void SendPanelToBack(int priority)
        {
            if (priority >= currentPriority)
            {
            if (InvokeRequired)
            {
                // It's on a different thread, so use Invoke.
                BeginInvoke(new MethodInvoker(() => SendPanelToBack(priority)));
            }
            else
            {
              
                    currentPriority = 0;
                    BackPanel.SendToBack();
                    BackPanel.Visible = false;
                }
            }
        }


        //This void removes the backpanel
        public void SendPanelToFront(string message, int priority)
        {
              if (priority >= currentPriority)
                {
            if (InvokeRequired)
            {
                // It's on a different thread, so use Invoke.
                BeginInvoke(new MethodInvoker(() => SendPanelToFront(message,priority)));
            }
            else
            {
                 
                    BackPanel.BringToFront();
                    BackPanel.Visible = true;
                    currentPriority = priority;

                }
            }
        }


        //This method is invoked when the file is loaded.
        public void EndExtractFiles(bool success)
        {
            if (InvokeRequired)
            {
                // It's on a different thread, so use Invoke.
                BeginInvoke(new MethodInvoker(() => EndExtractFiles(success)));
            }
            else
            {
                if (success)
                {
                    MessageBox.Show(@"Files was extracted to: " + folderBrowserDialog1.SelectedPath + @" successfully");
                    SendPanelToBack(1);
                }
                else
                {


                    MessageBox.Show(@"An error occurred while trying to extract the selected file!", @"Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
                SendPanelToBack(1);
            }
        }
        //This method is invoked when all files has been extracted.
        public void EndExtractFile(bool success)
        {
            if (InvokeRequired)
            {
                // It's on a different thread, so use Invoke.
                BeginInvoke(new MethodInvoker(() => EndExtractFile(success)));
            }
            else
            {
                if (success)
                {
                    MessageBox.Show(@"File was extracted to: " + saveFileDialog1.FileName + @" successfully");
                    SendPanelToBack(1);
                }
                else
                {

                    MessageBox.Show(@"An error occurred while trying to extract the selected file!", @"Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
            }
        }
        //This method is invoked when a file has been extracted.
        public void EndLoadFile()
        {
            if (InvokeRequired)
            {
                // It's on a different thread, so use Invoke.
                BeginInvoke(new MethodInvoker(() => EndLoadFile()));
            }
            else
            {
                TextEntries_listbox.Items.Clear();
                if (FileBrowser_listbox.SelectedIndex > -1)
                {
                    File script = _files[FileBrowser_listbox.SelectedIndex];
                    realentries.Clear();
                    _textEntriesColors.Clear();

                    foreach (Entry entry in script.entries)
                    {
                        if (!entry.Removed && (new byte[]{

44,
24,
25,
22,
28,
54,
                    45,55}).Contains(entry.LastCurrent) || !HideCommands_checkbox.Checked)
                        {

                            if (entry.Choices.Count > 0)
                            {

                                TextEntries_listbox.Items.Add("-> USER CHOICE <-");

                            }
                            else
                            {
                              
                                    TextEntries_listbox.Items.Add(StringTool.Truncate(entry.head.Replace("\n", ""), 20) + " { " + StringTool.Truncate(entry.content.Replace("\0", "").Replace("\n", ""), 80 - StringTool.Truncate(entry.head.Replace("\n", ""), 20).Length)+ " }");

                            }
                            AddColorCodeForRow(entry);
                            realentries.Add(entry);
                        }
                    }
                    if (realentries.Count > 0 && ExternalScript_entries_listbox.Items.Count > 0)
                    {
                        SelectNextExternalScript_button.Enabled = true;
                        ReplaceNextExternalScript_button.Enabled = true;
                    }
                    else
                    {
                        ReplaceNextExternalScript_button.Enabled = false;
                        SelectNextExternalScript_button.Enabled = false;
                    }
                    AddEntry_button.Enabled = false;
                    SearchGroupbox.Enabled = true;
                    FindAndReplace_button.Enabled = true;
                    Recompile_all_button.Enabled = true;
                    FindMisformedline_button.Enabled = true;
                    Delete_button.Enabled = false;
                    Ignore_button.Enabled = false;
                    SplitMessages_Button.Enabled = true;
                    SaveChanges_button.Enabled = false;
                    Editor_content_textbox.Enabled = false;
                    Editor_head_textbox.Enabled = false;
                    PopUp_Checkbox.Enabled = false;
                    TextEntries_label.Text = @"Content (" + realentries.Count + @" entries)";
                    ArchiveOperations_ExtractFile_button.Enabled = true;
                    ArchiveOperations_RenameFile_button.Enabled = true;
                    ArchiveOperations_DeleteFile_button.Enabled = true;
                    ArchiveOperations_FileUp_button.Enabled = true;
                    ArchiveOperations_FileDown_button.Enabled = true;
                    RLPAKTOOL_exportscript.Enabled = true;
                }
                SendPanelToBack(1);
                FileBrowser_listbox.Focus();
                
                if (!WaitingForEndLoadFile.Any())
                    WaitingForEndLoadFile.Add(true);
            }
        }

        //This method defines the desired color
        void AddColorCodeForRow(Entry entry)
        {
            if (entry.Choices.Count > 0)
            {
                //This is a choice block
                _textEntriesColors.Add(Color.LightBlue);
                return;
            }
            if (entry.LastCurrent == 44)
            {
                //This is a popup message.
                _textEntriesColors.Add(Color.LightGreen);
                return;
            }

            if (entry.LastCurrent == 45)
            {
                //This is a special message.
                _textEntriesColors.Add(Color.Red);
                return;
            }

            if (entry.LastCurrent == 55)
            {
                //This is a special message.

                _textEntriesColors.Add(Color.Red);
                return;
            }

            if (entry.LastCurrent == 18)
            {
                //This is a special message (file pointer).

                _textEntriesColors.Add(Color.Orange);
                return;
            }
            if (entry.PointTo.Count > 0)
            {

                _textEntriesColors.Add(Color.Pink);
                return;
            }
            _textEntriesColors.Add(Color.White);
        }
        //This method reenables the compression tab
        public void EnableCompressionControls()
        {
            if (InvokeRequired)
            {
                // It's on a different thread, so use Invoke.
                BeginInvoke(new MethodInvoker(EnableCompressionControls));
            }
            else
            {

                MessageBox.Show(@"Compression/Decompression complete!", @"Image Tools", MessageBoxButtons.OK, MessageBoxIcon.Information);
                GIMCompressor_browsefile_button.Enabled = true;
                GIMCompressor_browsefolder_button.Enabled = true;
                GIMCompressor_compress_button.Enabled = true;
                GIMCompressor_decompress_button.Enabled = true;
                GIMCompressor_outputbrowsefile_button.Enabled = true;
                GIMCompressor_outputbrowsefolder_button.Enabled = true;
                CompressPNGCZ2_button.Enabled = true;
                DecompressCZ2PNG_button.Enabled = true;
                UpdateProgressBar(0, "");
            }
        }
        //This method updates the progressbar of the compression tabpage
        public void UpdateProgressBar(int progress, string progressText)
        {
            if (InvokeRequired)
            {
                // It's on a different thread, so use Invoke.
                BeginInvoke(new MethodInvoker(() => UpdateProgressBar(progress, progressText)));
            }
            else
            {
                Percentage_bar.Value = progress;
                Percentage_bar_label.Text = @"Compression/decompression progress (" + progress + @"%): " + progressText;
            }
        }
        //These three method enables/disables some controls when needed.
        public void EnableControls()
        {
            Archiveoperations_groupbox.Enabled = true;
            RLPAKTOOL_exportscript.Enabled = true;
            ArchiveOperations_ExtractFile_button.Enabled = false;
            ArchiveOperations_RenameFile_button.Enabled = false;
            ArchiveOperations_DeleteFile_button.Enabled = false;
            ArchiveOperations_FileUp_button.Enabled = false;
            ArchiveOperations_FileDown_button.Enabled = false;
            TextEntries_listbox.Enabled = true;
            FileBrowser_listbox.Enabled = true;
            SearchGroupbox.Enabled = true;
            Editor_content_textbox.Enabled = false;
            Editor_head_textbox.Enabled = false;

            PopUp_Checkbox.Enabled = false;
            Delete_button.Enabled = false;
            AddEntry_button.Enabled = false;
            SplitMessages_Button.Enabled = false;
            FindAndReplace_button.Enabled = false;
            Recompile_all_button.Enabled = false;
            FindMisformedline_button.Enabled = false;
            Ignore_button.Enabled = false;
            SelectNextExternalScript_button.Enabled = false;
            ReplaceNextExternalScript_button.Enabled = false;
            SaveChanges_button.Enabled = false;
            TextEntries_listbox.SelectedIndex = -1;
        }
        public void DisableControls()
        {
            ArchiveOperations_ExtractFile_button.Enabled = false;
            RLPAKTOOL_exportscript.Enabled = false;
            ArchiveOperations_RenameFile_button.Enabled = false;
            ArchiveOperations_DeleteFile_button.Enabled = false;
            ArchiveOperations_FileUp_button.Enabled = false;
            ArchiveOperations_FileDown_button.Enabled = false;
            Archiveoperations_groupbox.Enabled = false;
            TextEntries_listbox.Enabled = false;
            FileBrowser_listbox.Enabled = false;
            Editor_content_textbox.Enabled = false;
            Editor_head_textbox.Enabled = false;

            PopUp_Checkbox.Enabled = false;
            Delete_button.Enabled = false;
            AddEntry_button.Enabled = false;
            SplitMessages_Button.Enabled = false;
            Ignore_button.Enabled = false;
            SelectNextExternalScript_button.Enabled = false;
            ReplaceNextExternalScript_button.Enabled = false;
            SaveChanges_button.Enabled = false;
            TextEntries_listbox.SelectedIndex = -1;

        }
        public  void EnableTab(TabPage page, bool enable)
        {
            foreach (Control ctl in page.Controls) ctl.Enabled = enable;

            CreatePointer_button.Enabled = true;
        }
        //This method returns a filter for use with savefiledialog and openfiledialog depending on the fileformat passed as argument.
        string GetFileFilter(FileFormat fileFormat)
        {
            switch (fileFormat)
            {
                case FileFormat.Scriptfile:
                    return "SCRIPTFILE (.script) | *.script";
                case FileFormat.VariablesFile:
                    return "VARIABLESFILE (.vars) | *.vars";
                case FileFormat.CGim:
                    return "Compressed GIM Image File (.cgim) | *.cgim";
                case FileFormat.Gim:
                    return "GIM Image File (.gim) | *.gim";
                case FileFormat.Cz2:
                    return "CZ2 Image File (.cz2) | *.cz2";
                case FileFormat.Cz1:
                    return "CZ1 Image File (.cz1) | *.cz1";


            }
            return null;
        }
        //This method returns a string converting fileformat enum to a string explaining file extension.
        string GetFileType(FileFormat fileFormat)
        {
            switch (fileFormat)
            {
                case FileFormat.Scriptfile:
                    return ".script";
                case FileFormat.VariablesFile:
                    return ".vars";
                case FileFormat.CGim:
                    return ".cgim";
                case FileFormat.Gim:
                    return ".gim";
                case FileFormat.Cz2:
                    return ".cz2";
                case FileFormat.Cz1:
                    return ".cz1";
            }
            return ".unknown";
        }
        public List<int> AllIndexesOf(string str, string value)
        {
            if (String.IsNullOrEmpty(value))
                throw new ArgumentException(@"the string to find may not be empty", "value");
            List<int> indexes = new List<int>();
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index, StringComparison.Ordinal);
                if (index == -1)
                    return indexes;
                indexes.Add(index);
            }
        }
        private byte ConditinalInvert(byte input)
        {
            if (Invert_checkbox.Checked)
                return input;
            return Convert.ToByte(255 - input);
        }
        #endregion

        private void DecompressCZ2PNG_button_Click(object sender, EventArgs e)
        {
            if (GIMCompressor_browsefile_textbox.Text != "" && System.IO.Directory.Exists(GIMCompressor_browsefile_textbox.Text))
            {

                Task.Run(() => DeCompressCz(System.IO.Directory.GetFiles(GIMCompressor_browsefile_textbox.Text), System.IO.Path.GetFullPath(GIMCompressor_outputbrowsefile_textbox.Text)));
                GIMCompressor_browsefile_button.Enabled = false;
                GIMCompressor_browsefolder_button.Enabled = false;
                GIMCompressor_compress_button.Enabled = false;
                GIMCompressor_decompress_button.Enabled = false;
                GIMCompressor_outputbrowsefile_button.Enabled = false;
                GIMCompressor_outputbrowsefolder_button.Enabled = false;
                CompressPNGCZ2_button.Enabled = false;
                DecompressCZ2PNG_button.Enabled = false;
            }
            else
            {
                if (GIMCompressor_browsefile_textbox.Text != "" && System.IO.File.Exists(GIMCompressor_browsefile_textbox.Text) && GIMCompressor_outputbrowsefile_textbox.Text != "")
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(GIMCompressor_browsefile_textbox.Text);
                    CompressedCz gimCompressor = new CompressedCz(bytes, System.IO.Path.GetFileNameWithoutExtension(GIMCompressor_browsefile_textbox.Text));
                    gimCompressor.PercentageChanged = PercentageChanged;
                    Task.Run(() => DeCompressCz(gimCompressor, GIMCompressor_outputbrowsefile_textbox.Text));
                    GIMCompressor_browsefile_button.Enabled = false;
                    GIMCompressor_browsefolder_button.Enabled = false;
                    GIMCompressor_compress_button.Enabled = false;
                    GIMCompressor_decompress_button.Enabled = false;
                    GIMCompressor_outputbrowsefile_button.Enabled = false;
                    GIMCompressor_outputbrowsefolder_button.Enabled = false;
                    CompressPNGCZ2_button.Enabled = false;
                    DecompressCZ2PNG_button.Enabled = false;
                }
            }
        }

        private void CompressCZ2_Click(object sender, EventArgs e)
        {
            if (GIMCompressor_browsefile_textbox.Text != "" && System.IO.Directory.Exists(GIMCompressor_browsefile_textbox.Text))
            {
                Task.Run(() => CompressCz2(System.IO.Directory.GetFiles(GIMCompressor_browsefile_textbox.Text), System.IO.Path.GetFullPath(GIMCompressor_outputbrowsefile_textbox.Text)));
                GIMCompressor_browsefile_button.Enabled = false;
                GIMCompressor_browsefolder_button.Enabled = false;
                GIMCompressor_compress_button.Enabled = false;
                GIMCompressor_decompress_button.Enabled = false;
                GIMCompressor_outputbrowsefile_button.Enabled = false;
                GIMCompressor_outputbrowsefolder_button.Enabled = false;
                CompressPNGCZ2_button.Enabled = false;
                DecompressCZ2PNG_button.Enabled = false;
            }
            else
            {

                if (GIMCompressor_browsefile_textbox.Text != "" && System.IO.File.Exists(GIMCompressor_browsefile_textbox.Text) && GIMCompressor_outputbrowsefile_textbox.Text != "")
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(GIMCompressor_browsefile_textbox.Text);
                    CompressedCz gimCompressor = new CompressedCz(bytes, System.IO.Path.GetFileNameWithoutExtension(GIMCompressor_browsefile_textbox.Text));
                    gimCompressor.PercentageChanged = PercentageChanged;
                    var extension = System.IO.Path.GetExtension(GIMCompressor_outputbrowsefile_textbox.Text);
                    bool isCz2 = extension != null && extension.EndsWith("cz1") == false;
                    Task.Run(()=> CompressCz2(gimCompressor, GIMCompressor_outputbrowsefile_textbox.Text,isCz2));
                    GIMCompressor_browsefile_button.Enabled = false;
                    GIMCompressor_browsefolder_button.Enabled = false;
                    GIMCompressor_compress_button.Enabled = false;
                    GIMCompressor_decompress_button.Enabled = false;
                    GIMCompressor_outputbrowsefile_button.Enabled = false;
                    GIMCompressor_outputbrowsefolder_button.Enabled = false;
                    CompressPNGCZ2_button.Enabled = false;
                    DecompressCZ2PNG_button.Enabled = false;
                }
            }
        }


        private void recompileAllButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < TextEntries_listbox.Items.Count; i++)
            {
                TextEntries_listbox.SelectedIndex = i;
                if (Editor_Choices_listbox.Items.Count > 0)
                {
                    for (int ia = 0; ia < Editor_Choices_listbox.Items.Count; ia++)
                    {
                        Editor_Choices_listbox.SelectedIndex = ia;
                        SaveChoices_Button_Click(null, null);
                    }
                }
                else if (Editor_content_textbox.Text != "" && Editor_content_textbox.Text != @" " )
                {
                    SaveChanges_button_Click(null, null);
                }
            }
        }

        public static SizeF MeasureString(string s, Font font)
        {
            SizeF result;
            using (var image = new Bitmap(1, 1))
            {
                using (var g = Graphics.FromImage(image))
                {
                    result = g.MeasureString(s, font);
                }
            }

            return result;
        }
        private async void SearchAll_Click(object sender, EventArgs e)
        {
            SendPanelToFront(string.Format("Searching for {0}",Search_textbox.Text),10);
            for (int ia = Math.Max(0,FileBrowser_listbox.SelectedIndex); ia < _files.Count; ia++)
            {
                LoadingUpdateLabel(10,string.Format("Searching for '{0}' in file '{1}'", Search_textbox.Text, _files[ia].FileName));
                
                FileBrowser_listbox.SelectedIndex = -1;
                FileBrowser_listbox.SelectedIndex = ia;
                await Task.Run(() => WaitForLoadCompletion());

            for (int i = Math.Max(0, TextEntries_listbox.SelectedIndex); i <realentries.Count; i++)
            {

                LoadingUpdateLabel(10, string.Format("Searching for '{0}' in file '{1}' dialog number: '{2}'", Search_textbox.Text, _files[ia].FileName,i));
                 

                if (EntryContainsText(realentries[i], Search_textbox.Text))
                {
                    TextEntries_listbox.SelectedIndex = i;
                    SendPanelToBack(10);
                    return;
                }
            }
            }
            SendPanelToBack(10);
        }

        void WaitForLoadCompletion()
        {
            WaitingForEndLoadFile.Take();
            Thread.Sleep(50);
        }
        private void SavePlain_Button_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] bytes = StringToByteArray(PlainHex_TextBox.Text.Replace("-", "").Replace(" ", ""));
                if (PlainHex_TextBox.Text.Length > 0)
                {
                    if (bytes.Length%2 > 0)
                    {
                        MessageBox.Show(@"The entered bytes must have a length dividable by 2.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    bytes[1] = Convert.ToByte(bytes.Length/2);
                    _files[_currentFileIndex].changed = true;
                    realentries[_currentEntryIndex].LastCurrent = bytes[0]; 
                    realentries[_currentEntryIndex].contentBytes = bytes;
                    return;
                }
            }
            catch (Exception)
            {
            }
            MessageBox.Show(@"The entered bytes are wrongly formated.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            byte a = 127; 
            for (int ia = 0; ia < 255; ia++)
            {
                _files[_currentFileIndex].changed = true;
                Editor_content_textbox.Text = "";
                Editor_head_textbox.Text = "";
                _currentEntryIndex = -1;
                Entry entry = new Entry();
                entry.Added = true;
                entry.head = "";
                entry.content = "{UHEX:"+a+"}"+"{UHEX:"+ia+"}";

                entry.LastCurrent = 25;
                if (TextEntries_listbox.SelectedIndex + 1 > -1)
                {
                    _files[FileBrowser_listbox.SelectedIndex].entries.Insert(_files[FileBrowser_listbox.SelectedIndex].entries.IndexOf(realentries[TextEntries_listbox.SelectedIndex]) + 1, entry);
                }
                TextEntries_listbox.Items.Insert(TextEntries_listbox.SelectedIndex + 1, entry.head.Replace("\n", "") + " { " + entry.content.Replace("\0", "").Replace("\n", "") + " }");
                _textEntriesColors.Insert(TextEntries_listbox.SelectedIndex + 1, Color.Yellow);
                realentries.Insert(TextEntries_listbox.SelectedIndex + 1, entry);
                int co = 0;
                for (int i = 0; i < realentries.Count; i++)
                {
                    if (TextEntries_listbox.Items[i].ToString() != "(IGNORED)")
                    {
                        co++;
                    }
                }

                CompileEntry(_files[_currentFileIndex], entry);
                TextEntries_label.Text = @"Content (" + co + @" entries)";
            }

        }




































    }
   }

