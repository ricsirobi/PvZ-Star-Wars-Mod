using System.Collections.Generic;

namespace RLPAKTOOL_NameSpace.Fileformats
{
    public class ProgressiveLookUp
    {
        readonly Dictionary<byte, ProgressiveLookUp> _content;
        public bool CanEnd;
        public ProgressiveLookUp()
        {
            _content = new Dictionary<byte, ProgressiveLookUp>();
        }
        public void Add(int startpos, byte[] input)
        {
            if (startpos >= input.Length)
            {
                CanEnd = true;
                return;
            }
            byte tol = input[startpos];
            if (_content.ContainsKey(tol))
            {
                _content[tol].Add(startpos + 1, input);
            }
            else
            {
                ProgressiveLookUp nwl = new ProgressiveLookUp();
                nwl.Add(startpos + 1, input);
                _content.Add(tol, nwl);
            }

        }
        public ProgressiveLookUp Contains(int pos, byte[] original)
        {
            if (pos >= original.Length)
            {
                if (CanEnd)
                    return this;
                return null;
            }
            byte tol = original[pos];
            if (_content.ContainsKey(tol))
                return _content[tol].Contains(pos + 1, original);
            return null;
        }
        public ProgressiveLookUp Contains(byte lookUp)
        {
            if (_content.ContainsKey(lookUp))
            {
                return _content[lookUp];
            }
            return null;
        }
    }
}