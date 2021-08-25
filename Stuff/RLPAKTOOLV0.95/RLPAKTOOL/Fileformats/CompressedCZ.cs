using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace RLPAKTOOL_NameSpace.Fileformats
{
    public class CompressedCz
    {
        int _headerLength = 24;
        public int FileLength;
        public int Percentage;
        public string FileName = "";
        readonly byte[] _inputfile;
        public EventHandler PercentageChanged;
        readonly Dictionary<int, byte[]> _pointerMemory = new Dictionary<int, byte[]>(); //For speed
        public CompressedCz(byte[] input)
        {
            FileLength = input.Length;
            Percentage = 0;
            _inputfile = input;
        }
        public CompressedCz(byte[] input, string fileName)
        {
            FileLength = input.Length;
            Percentage = 0;
            _inputfile = input;
            FileName = fileName;
        }
        public byte[] CompressCz(bool isCz2)
        {
            Stream stream = new MemoryStream(_inputfile);
            Bitmap bp = new Bitmap(stream);
            LockBitmap lc = new LockBitmap(bp);
            lc.LockBits();
            byte[] argb = lc.Pixels;
            uint[] argbInts = new uint[bp.Width * bp.Height];
            for (int i = 0; i < argb.Length; i+=4)
            {
                argbInts[i / 4] = BitConverter.ToUInt32(new[] { argb[i], argb[i + 1], argb[i + 2], argb[i + 3] }, 0);
            }
            uint[] encodedArgb = argbInts;
            if (isCz2)
            {
                encodedArgb = _CZ2_encode(argbInts, bp.Width, bp.Height);
            }
            else
            {
                encodedArgb = Bgra(encodedArgb);
            }
            byte[] decompressedData = new byte[argbInts.Length * 4];

            for (int i = 0; i < encodedArgb.Length; i++)
            {
                byte[] x = BitConverter.GetBytes(encodedArgb[i]);
                    decompressedData[i * 4] = x[0];
                    decompressedData[i * 4 + 1] = x[1];
                    decompressedData[i * 4 + 2] = x[2];
                    decompressedData[i * 4 + 3] = x[3];
                
            }
            lc.UnlockBits();
            List<int> filePartsStart = new List<int>();
            List<int> fileDecompressedLength = new List<int>();
            FileLength = _inputfile.Length;
            List<byte> compressedOutput = new List<byte>();

          
                _pointerMemory.Clear();
                int originalFileLength = decompressedData.Length;
                int partLength = 0;
                int step = 0;
                int offsetIndex = 0;
                Dictionary<byte[], GimCompressBlock> dictionary = new Dictionary<byte[], GimCompressBlock>(new ByteArrayComparer()); //lookupValue, Posiiton    
                List<byte> cinput = decompressedData.ToList();
                ProgressiveLookUp mainLookUp = new ProgressiveLookUp();
                for (int i = 0; i < decompressedData.Length; i++)
                {
                    if (Percentage != Convert.ToInt32(100.0 / Convert.ToDouble(originalFileLength) * (offsetIndex + i)))
                    {
                        Percentage = Convert.ToInt32(100.0 / Convert.ToDouble(originalFileLength) * (offsetIndex + i));
                        if (PercentageChanged != null)
                            PercentageChanged(this, null);
                    }
                    if (i == decompressedData.Length - 1)
                    {
                        compressedOutput.Add(0);
                        compressedOutput.Add(_inputfile[i]);
                        partLength ++;
                        break;
                    }
                    List<byte> lookUpValue = new List<byte>();
                    ProgressiveLookUp last = mainLookUp.Contains(decompressedData[i]);
                    if (last == null)
                    {
                        lookUpValue.Add(decompressedData[i]);
                    }
                    else
                    {
                        int fj = decompressedData.Length;
                        for (int j = i + 1; j < decompressedData.Length; j++)
                        {
                            last = last.Contains(decompressedData[j]);
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
                        byte[] positionArray = BitConverter.GetBytes(dictionary[lookUpValue.ToArray()].Position + 1);
                        positionArray[1]++;
                        compressedOutput.Add(positionArray[1]);
                        compressedOutput.Add(positionArray[0]);
                        partLength++;
                        i += lookUpValue.Count - 1;
                        if (i + 1 >= decompressedData.Length)
                        {
                            break;
                        }
                        if (partLength > 32767)
                        {
                            fileDecompressedLength.Add(i+1);
                            filePartsStart.Add(partLength);
                            offsetIndex += i - 1;
                            decompressedData = decompressedData.Skip(i+1).ToArray();
                            i = -1;
                            cinput = decompressedData.ToList();
                            _pointerMemory.Clear();
                            partLength = 0;
                            step = 0;
                            dictionary.Clear();
                            mainLookUp = new ProgressiveLookUp();
                            continue;
                        }
                        lookUpValue.Add(decompressedData[i + 1]);
                    }
                    else
                    {
                        compressedOutput.Add(0);
                        compressedOutput.Add(decompressedData[i]);
                        partLength ++;
                        if (partLength > 32767)
                        {
                            fileDecompressedLength.Add(i+1);
                            filePartsStart.Add(partLength);
                            offsetIndex += i-1;
                            decompressedData = decompressedData.Skip(i+1).ToArray();
                            i = -1;
                            cinput = decompressedData.ToList();
                            _pointerMemory.Clear();
                            partLength = 0;
                            step = 0;
                            dictionary.Clear();
                            mainLookUp = new ProgressiveLookUp();
                            continue;
                        }
                        lookUpValue.Add(decompressedData[i + 1]);
                    }
                    if (!dictionary.ContainsKey(lookUpValue.ToArray()))
                    {
                        byte[] va = lookUpValue.ToArray();
                        dictionary.Add(va, new GimCompressBlock(step));
                        mainLookUp.Add(0, va);
                    }
                    step++;
                }
                if (partLength > 0)
                {
                    fileDecompressedLength.Add(decompressedData.Length);
                    filePartsStart.Add(partLength);
                         
                }
            List<byte> header = new List<byte>();
            header.AddRange(isCz2 ? new byte[] {67, 90, 50, 0} : new byte[] {67, 90, 49, 0});
            byte[] w = BitConverter.GetBytes(bp.Width);
                byte[] h = BitConverter.GetBytes(bp.Height);
                header.Add(w[0]);
                header.Add(w[1]);
                header.Add(h[0]);
                header.Add(h[1]);
                header.AddRange(new byte[] { 32, 0, 0, 0 }); //32 bits
                byte[] splitsBytes = BitConverter.GetBytes(filePartsStart.Count);
                header.AddRange(new[] { splitsBytes[3], splitsBytes[2], splitsBytes[1], splitsBytes[0] }); //splittings
                for (int i = 0; i < filePartsStart.Count;i++ )
                {
                    header.AddRange(Endian(BitConverter.GetBytes(filePartsStart[i]))); //Compressed start pos
                    header.AddRange(Endian(BitConverter.GetBytes(fileDecompressedLength[i]))); //Decompressed length

                }
                compressedOutput.InsertRange(0, header);
            
            return compressedOutput.ToArray();
        }
        private byte[] Endian(byte[] input)
        {
            return input.Reverse().ToArray();
        }
        public byte[] DecompressCz()
        {
                _pointerMemory.Clear();
                List<byte> decompressedOutput = new List<byte>();
                int width = BitConverter.ToUInt16(new[] { _inputfile[4], _inputfile[5] }, 0);
                int height = BitConverter.ToUInt16(new[] { _inputfile[6], _inputfile[7] }, 0);    
                int splits = Convert.ToInt32(BitConverter.ToUInt32(new[] { _inputfile[15], _inputfile[14], _inputfile[13], _inputfile[12] }, 0)  );
                
                _headerLength = 16 + splits * 8;
                bool isCz2 = _inputfile[2] != 49; 
                
                byte[][] fileParts = new byte[(_headerLength - 16)/8+1][];
                int lastPoint = _headerLength;
                for (int i = 16; i < 16 + fileParts.Length * 8; i += 8)
                {
                    int startpos = Convert.ToInt32(BitConverter.ToUInt32(new[] { _inputfile[i+3], _inputfile[i+2], _inputfile[i+1], _inputfile[i] }, 0) * 2 ) + lastPoint;

                    fileParts[(i - 16) / 8] = _inputfile.Skip(lastPoint).Take(startpos - lastPoint).ToArray();
                    lastPoint = startpos;
                }
                int inputFileIndex = 0;
                foreach (byte[] part in fileParts)
                {
                    _pointerMemory.Clear();
                    for (int i = 0; i < part.Length; i += 2)
                    {
                        inputFileIndex += 2;
                        if (Percentage != Convert.ToInt32(100.0 / Convert.ToDouble(_inputfile.Length) * inputFileIndex))
                        {
                            Percentage = Convert.ToInt32(100.0 / Convert.ToDouble(_inputfile.Length) * inputFileIndex);
                            if (PercentageChanged != null)
                                PercentageChanged(this, null);
                        }
                        byte value = part[i + 1];
                        byte key = part[i];
                        switch (key)
                        {
                            case 0:
                                decompressedOutput.Add(value);
                                break;
                            default:
                                int pointTo = (BitConverter.ToUInt16(new[] { value, Convert.ToByte(key - 1) }, 0) - 1) * 2;
                                decompressedOutput.AddRange(GetChainResult(part, pointTo));
                                break;
                        }
                    }
                }
                Bitmap bp = new Bitmap(width, height, 
                        PixelFormat.Format32bppArgb);
                bp.MakeTransparent(Color.White);
                LockBitmap bitmap = new LockBitmap(bp);
                uint[] colorsAsIntegers = new uint[width*height];
                bitmap.LockBits();
                int pos = 0;
                    for (int i = 0; i < decompressedOutput.Count; i += 4)
                    {
                        uint v = BitConverter.ToUInt32(new[] { decompressedOutput[i], decompressedOutput[i + 1], decompressedOutput[i + 2], decompressedOutput[i + 3] }, 0);
                        colorsAsIntegers[pos++] = v;
                   
                    }

                    if (isCz2)
                    {
                        uint[] imgBytes = Bgra(_CZ2_decode(colorsAsIntegers, width, height));

                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                byte[] btBytes = BitConverter.GetBytes(imgBytes[y * width + x]);
                                byte a = btBytes[0];
                                byte r = btBytes[1];
                                byte g = btBytes[2];
                                byte b = btBytes[3];

                                bitmap.SetPixel(x, y, Color.FromArgb(a, r, g, b));
                            }
                        }
                    }
                    else
                    {
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                byte[] btBytes = BitConverter.GetBytes(colorsAsIntegers[y * width + x]);
                                byte a = btBytes[0];
                                byte r = btBytes[1];
                                byte g = btBytes[2];
                                byte b = btBytes[3];

                                bitmap.SetPixel(x, y, Color.FromArgb(a, r, g, b));
                            }
                        }
                    }
                bitmap.UnlockBits();
                MemoryStream stream = new MemoryStream();
                Image img = bp;
                img.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            

        }
        private byte[] GetChainResult(byte[] input, int pointTo)
        {

            List<byte> result = new List<byte>();
            byte[] sequence = new byte[] { input[pointTo], input[pointTo + 1], input[pointTo + 2], input[pointTo + 3] };
            byte key0 = sequence[0];
            byte key1 = sequence[2];
            bool takeKey0Value = false;
            if (key1 > 0 && (BitConverter.ToUInt16(new[] { sequence[3], Convert.ToByte(key1 - 1) }, 0) - 1) * 2 == pointTo)
            {
                //inheritance key0's value!
                takeKey0Value = true;

            }
            else
            {
                if (key1 == 0)
                {
                    result.Insert(0, sequence[3]);
                }
                else
                {
                    result.Insert(0, GetChainResultSub(input, (BitConverter.ToUInt16(new[] { sequence[3], Convert.ToByte(key1 - 1) }, 0) - 1) * 2)[0]);

                }
            }
            if (key0 > 0 && (BitConverter.ToUInt16(new[] { sequence[1], Convert.ToByte(key0 - 1) }, 0) - 1) * 2 == pointTo)
            {
                result.Insert(0, 0);

            }
            else
            {
                if (key0 > 0)
                {
                    result.InsertRange(0, GetChainResultSub(input, (BitConverter.ToUInt16(new[] { sequence[1], Convert.ToByte(key0 - 1) }, 0) - 1) * 2));
                }
                else
                {
                    result.Insert(0, sequence[1]);
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
            byte key0 = sequence[0];
            byte key1 = sequence[2];
            bool takeKey0Value = false;
            if (key1 > 0 && (BitConverter.ToUInt16(new[] { sequence[3], Convert.ToByte(key1 - 1) }, 0) - 1) * 2 == pointTo)
            {
                //inheritance key0's value!
                takeKey0Value = true;

            }
            else
            {
                if (key1 > 0)
                {
                    result.Insert(0, GetChainResultSub(input, (BitConverter.ToUInt16(new[] { sequence[3], Convert.ToByte(key1 - 1) }, 0) - 1) * 2)[0]);
                }
                else
                {
                    result.Insert( 0,sequence[3]);
                }
            }

            if (key0 > 0 && (BitConverter.ToUInt16(new[] { sequence[1], Convert.ToByte(key0 - 1) }, 0) - 1) * 2 == pointTo)
            {


                result.Insert(0, 0);


            }
            else
            {
                if (key0 > 0)
                {
                    result.InsertRange(0, GetChainResultSub(input, (BitConverter.ToUInt16(new[] { sequence[1], Convert.ToByte(key0 - 1) }, 0) - 1) * 2));
                }
                else
                {
                    result.Insert(0, sequence[1]);
                }
            }

            if (takeKey0Value)
            {
                result.Add(result[0]);
            }
            _pointerMemory.Add(pointTo, result.ToArray());
            return result.ToArray();
        }
        //Method for acquiring the ARGB data based on code from a russian programmer known as 'asidonus'.
        //This method is a C# version of his python script with a fix to prevent alpha from being affected by RGB exceeding 255.
        public uint[] _CZ2_decode(uint[] source,int width, int height)
        {
            uint[] bgra = Bgra(source);
            int  e = Convert.ToInt32((height+2)/3);
            for (int i = 0; i<height;i++)
            {
                int o = width*i;
                if (i%e!= 0)
                {
                    

            for (int j = 0; j<width;j++)
            {

                bgra[o + j] = Convert.ToUInt32((bgra[o + j] + bgra[j + o - width]) & 0xFFFFFFFF);
            }
                }
            }

            return bgra;
        }
        public uint Argb2Bgra(uint x)
        {
            return ((x & 0xFF) << 24) | ((x & 0xFF00) << 8) | ((x & 0xFF0000) >> 8) | (x >> 24);
        }

       
        public uint[] Bgra(uint[] source)
        {
            uint[] bgra = new uint[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
               bgra[i] = Argb2Bgra(source[i]);
            }
            return bgra;
        }
        public uint[] _CZ2_encode(uint[] bgra, int width, int height)
        {
            int e = Convert.ToInt32((height + 2) / 3);
            uint[] lineColors = new uint[width];
            for (int i = 0; i < height; i++)
            {
                int o = width * i;
                if (i % e != 0)
                {

                    for (int j = 0; j < width; j++)
                    {
                        byte[] colorParts = BitConverter.GetBytes(bgra[o + j] - lineColors[j]);
                        lineColors[j] = bgra[o + j];
                        bgra[o + j] = BitConverter.ToUInt32(colorParts, 0);
                    }
                }
                else
                {

                    for (int j = 0; j < width; j++)
                    {
                        byte[] colorParts = BitConverter.GetBytes(bgra[o + j]);
                        lineColors[j] = BitConverter.ToUInt32(colorParts, 0);
                        bgra[o + j] = lineColors[j];
                    }
                }
            }

            return Bgra(bgra);
        }
    

    }
  
}
