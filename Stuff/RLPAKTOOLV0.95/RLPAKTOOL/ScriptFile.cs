// <author>Patrick Evers Bjoerkman</author>
// <Contact>puritymail@gmail.com</Contact>
// <lastupdate>01-03-2014</lastupdate>
// <summary>scriptclass for Real Life PAK tool</summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RLPAKTOOL_NameSpace
{
    public class File
    {
        public string FileName = "";
        public List<Entry> entries = new List<Entry>();
        public bool changed = false;
        public int FileStart = 0;
        public int FileMax = 0;
        public int Val = 0;
        public int OriginalLength = 0;
        public int SignatureValue = 0;
        public byte[] UnsafeSource = new byte[0];
        public RlpakTool.FileFormat FileFormat = RlpakTool.FileFormat.Unknown;

    }

    class cmd
    {
        public int length;
        public int stln;
        public byte[] inf = new byte[0];
    }
    public class SFile
    {
        public List<Entry> blocks;
        public int FileName = 0;
        public bool HasHeader = false;
        public bool HasEnd = false;
        public SFile PointTo;
    }
}
