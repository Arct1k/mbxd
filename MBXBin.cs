using MBXD.Decompiller;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBXD.Decompiler
{
    class MBXFileBinaryDataData
    {
        
        byte[] bin;

        public MBXFileBinaryDataData(MBXFile self)
        {
            FileStream stream;
            //loading to array and unlocking file
            stream = new FileStream(self.GetFileName(), System.IO.FileMode.Open);
            bin = new byte[stream.Length];
            stream.Read(bin, 0, (int)stream.Length);
            stream.Close();
        }

        public byte readByte(uint offset)
        {
            return bin[offset];
        }

        public UInt16 readWord(uint offset)
        {
            return BitConverter.ToUInt16(bin, (int)offset);
        }

        public UInt32 readDWord(uint offset)
        {
            return BitConverter.ToUInt32(bin, (int)offset);
        }

        public byte[] readByteArray(uint offset, uint size)
        {
            byte[] result = new byte[size];
            Array.Copy(bin, offset, result, 0, size);
            return result;
        }

        public MBXString readMBXString(uint offset)
        {
            return new MBXString(this.bin, offset);
        }

        public void rewriteBytesAtOffset(byte[] data, int offset)
        {
            Array.Copy(data, 0, bin, offset, data.Length);
        }

        public void saveDataToFile(string fileName)
        {
            File.WriteAllBytes(fileName, this.bin);
        }
    }
}
