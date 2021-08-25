// <author>Patrick Evers Bjoerkman</author>
// <Contact>puritymail@gmail.com</Contact>
// <lastupdate>01-03-2014</lastupdate>
// <summary>Gim compression class</summary>

using System;
using System.Collections.Generic;
using System.Linq;

namespace RLPAKTOOL_NameSpace.Fileformats
{
    public class GimCompressor
    {
        public int FileLength;
        public int Percentage;
        public string FileName = "";
        readonly byte[] _inputfile;
        public EventHandler PercentageChanged;
        readonly Dictionary<int, byte[]> _pointerMemory = new Dictionary<int, byte[]>(); //For speed
        public GimCompressor(byte[] input)
        {
            FileLength = input.Length;
            Percentage = 0;
            _inputfile = input;
        }
        public GimCompressor(byte[] input, string fileName)
        {
            FileLength = input.Length;
            Percentage = 0;
            _inputfile = input;
            FileName = fileName;
        }
        public byte[] CompressGim()
        {
            _pointerMemory.Clear();
            FileLength = _inputfile.Length;
            List<byte> compressedOutput = new List<byte>();
            compressedOutput.AddRange(BitConverter.GetBytes(16)); //Add magic number
            compressedOutput.AddRange(BitConverter.GetBytes(_inputfile.Length)); //Add uncompressed length
            int step = 0;
            Dictionary<byte[], GimCompressBlock> dictionary = new Dictionary<byte[], GimCompressBlock>(new ByteArrayComparer()); //lookupValue, Posiiton    
            List<byte> cinput = _inputfile.ToList();
            ProgressiveLookUp mainLookUp = new ProgressiveLookUp();
            for (int i = 0; i < _inputfile.Length - 1; i++)
            {
                if (Percentage != Convert.ToInt32( 100.0 / Convert.ToDouble(_inputfile.Length) * i))
                {
                    Percentage = Convert.ToInt32( 100.0 / Convert.ToDouble(_inputfile.Length) * i);
                    if (PercentageChanged != null)
                        PercentageChanged(this, null);
                }
                if (i == _inputfile.Length)
                {
                    compressedOutput.Add(_inputfile[i]);
                    compressedOutput.Add(0);
                    break;
                }
                List<byte> lookUpValue = new List<byte>();
                ProgressiveLookUp last = mainLookUp.Contains(_inputfile[i]);
                if (last == null)
                {
                    lookUpValue.Add(_inputfile[i]);
                }
                else
                {
                    int fj = _inputfile.Length;
                    for (int j = i + 1; j < _inputfile.Length; j++)
                    {
                        last = last.Contains(_inputfile[j]);
                        if (last == null || !last.CanEnd)
                        {
                            fj = j;
                            break;
                        }
                    }
                    if (last == null || last.CanEnd)
                    {
                        lookUpValue.AddRange(cinput.GetRange(i, fj - i));
                    }
                }
                if (lookUpValue.Count > 1)
                {
                    byte[] positionArray = BitConverter.GetBytes(dictionary[lookUpValue.ToArray()].Position+1);
                    positionArray[1]++;
                    compressedOutput.Add(positionArray[0]);
                    compressedOutput.Add(positionArray[1]);
                    i += lookUpValue.Count-1;
                    if (i + 1 >= _inputfile.Length)
                    {
                        break;
                    }
                    lookUpValue.Add(_inputfile[i + 1]);
                }
                else
                {
                    compressedOutput.Add(_inputfile[i]);
                    compressedOutput.Add(0);
                    lookUpValue.Add(_inputfile[i + 1]);
                }
                if (!dictionary.ContainsKey(lookUpValue.ToArray()))
                {
                    byte[] va = lookUpValue.ToArray();
                    dictionary.Add(va, new GimCompressBlock(step));
                    mainLookUp.Add(0, va);
                }
                step++; 
            }
            return compressedOutput.ToArray();
        }
        public byte[] DecompressGim()
        {
            _pointerMemory.Clear();
            int headerLength = 8;
            List<byte> decompressedOutput = new List<byte>();
            for (int i = headerLength; i < _inputfile.Length; i += 2)
            {
                if (Percentage != Convert.ToInt32(100.0 / Convert.ToDouble(_inputfile.Length) * i))
                {
                    Percentage = Convert.ToInt32(100.0 / Convert.ToDouble(_inputfile.Length) * i);
                    if (PercentageChanged != null)
                        PercentageChanged(this, null);
                }
                byte value = _inputfile[i];
                byte key = _inputfile[i + 1];
                switch (key)
                {
                    case 0:
                        decompressedOutput.Add(value);
                        break;
                    default:
                        int pointTo = (BitConverter.ToUInt16(new[] { value, Convert.ToByte(key - 1) }, 0) - 1) * 2 + 8;
                        byte[] bts = GetChainResult(_inputfile, pointTo);
                        decompressedOutput.AddRange( bts);
                        break;
                }
            }
            return decompressedOutput.ToArray();
        }
        private byte[] GetChainResult(byte[] input, int pointTo)
        {
            List<byte> result = new List<byte>();
            byte[] sequence = new byte[] { input[pointTo], input[pointTo + 1], input[pointTo + 2], input[pointTo + 3] };
            byte key0 = sequence[1];
            byte key1 = sequence[3];
            bool takeKey0Value = false;
            if (key1 > 0 && (BitConverter.ToUInt16(new[] { sequence[2], Convert.ToByte(key1 - 1) }, 0) - 1) * 2 + 8 == pointTo)
            {
                //inheritance key0's value!
                takeKey0Value = true;

            }
            else
            {
                if (key1 == 0)
                {
                    result.Insert(0, sequence[2]);
                }
                else
                {
                    result.Insert(0, GetChainResultSub(input, (BitConverter.ToUInt16(new[] { sequence[2], Convert.ToByte(key1 - 1) }, 0) - 1) * 2 + 8)[0]);

                }
            }
            if (key0 > 0 && (BitConverter.ToUInt16(new[] { sequence[0], Convert.ToByte(key0 - 1) }, 0) - 1) * 2 + 8 == pointTo)
            {
                result.Insert(0, 0);

            }
            else
            {
                if (key0 > 0)
                {
                    result.InsertRange(0, GetChainResultSub(input, (BitConverter.ToUInt16(new[] { sequence[0], Convert.ToByte(key0 - 1) }, 0) - 1) * 2 + 8));
                }
                else
                {
                    result.Insert(0, sequence[0]);
                }
            }
            if (takeKey0Value)
            {
                result.Add(result[0]);
            }
            return result.ToArray();
        }
        private byte[] GetChainResultSub(byte[] input, int pointTo)
        {
            if (_pointerMemory.ContainsKey(pointTo))
                return _pointerMemory[pointTo];

            List<byte> result = new List<byte>();
            byte[] sequence = new byte[] { input[pointTo], input[pointTo + 1], input[pointTo + 2], input[pointTo + 3] };
            byte key0 = sequence[1];
            byte key1 = sequence[3];
            bool takeKey0Value = false;
                if (key1 > 0 && (BitConverter.ToUInt16(new[] { sequence[2], Convert.ToByte(key1 - 1) }, 0) - 1) * 2 + 8 == pointTo)
                {
                    //inheritance key0's value!
                    takeKey0Value = true;

                }
                else
                {
                    if (key1 > 0)
                    {
                        result.Insert(0, GetChainResultSub(input, (BitConverter.ToUInt16(new[] { sequence[2], Convert.ToByte(key1 - 1) }, 0) - 1) * 2 + 8)[0]);
                    }
                    else
                    {
                        result.Insert(0, sequence[2]);
                    }
                }
            
            if (key0 > 0 && (BitConverter.ToUInt16(new[] { sequence[0], Convert.ToByte(key0 - 1) }, 0) - 1) * 2 + 8 == pointTo)
            {


                result.Insert(0, 0);


            }
            else
            {
                if (key0 > 0)
                {
                    result.InsertRange(0, GetChainResultSub(input, (BitConverter.ToUInt16(new[] { sequence[0], Convert.ToByte(key0 - 1) }, 0) - 1) * 2 + 8));
                }
                else
                {
                    result.Insert(0, sequence[0]);
                }
            }

            if (takeKey0Value)
            {
                result.Add(result[0]);
            }
            _pointerMemory.Add(pointTo, result.ToArray());
            return result.ToArray();
        }

    }
}
