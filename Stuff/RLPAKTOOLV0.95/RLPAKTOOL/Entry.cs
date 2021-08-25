using System.Collections.Generic;

namespace RLPAKTOOL_NameSpace
{
    public class Entry
    {
        public bool Special = false;
        public byte[] inf= new byte[0];
        public string head = "";
        public string content ="";
        public int Length
        {
            get { return contentBytes.Length;}
        }
        public bool Removed = false;
        public byte LastCurrent = 0;
        public bool ForcedFour = false;
        public bool BeginningOfFile = false;
        public byte[] ChoicesBytes = new byte[6];
        public byte[] contentBytes = new byte[6];
        public byte[] LastCMDs = new byte[2];
        public List<string> Choices = new List<string>();
        public List<Entry> PointTo = new List<Entry>();
        public List<Entry> PointedFrom = new List<Entry>();
        public bool Added = false;
        public bool skip = false;
        public int UnsafeHexPos = 0;

    }
}