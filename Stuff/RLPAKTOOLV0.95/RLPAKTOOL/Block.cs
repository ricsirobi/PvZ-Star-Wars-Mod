// <author>Patrick Evers Bjoerkman</author>
// <Contact>puritymail@gmail.com</Contact>
// <lastupdate>01-03-2014</lastupdate>
// <summary>Script entry block class</summary>

namespace RLPAKTOOL_NameSpace
{
    //This class describes information about a dialog or command of the PAK/SCRIPT FILE
    public class Block
    {
        public byte X;
        public byte Y;
        public int Maxlength;
        public int Stln;
        public byte[] Inf = new byte[0];

    }
}
