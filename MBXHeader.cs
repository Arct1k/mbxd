using MBXD.Decompiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBXD.Decompiller
{
    public struct MBXFileHeaderStruct
    {
        public uint zero;
        public uint globalVarsOffset;
        public uint procsNamesOffset;
        public uint typesOffset;
        public uint procsVarNamesOffset;
        public uint dllTable;
        public ushort globalVarsCount;
        public ushort procsCount;
        public ushort typesCount;
        public ushort dllsCount;
        public ushort unknown;
    }

    public struct MBXProcInfo
    {

        public uint procOffset;
        public MBXString procName;
    }

    class MBXHeader
    {
        byte[] signature;
        MBXFileHeaderStruct header;
        MBXProcInfo[] proclist;
        string version;
        int iVersion;
        bool isMoreThen850Version;

        public MBXHeader(MBXFile self)
        {
            this.signature                   = self.mbxFileBinaryData.readByteArray(0, 0x100);
            this.version                     = System.Text.Encoding.ASCII.GetString(signature).Split('\n')[1].Split(' ')[1];
            this.iVersion                    = int.Parse(this.version);
            this.isMoreThen850Version        = this.iVersion > 850;
            this.header.zero                 = self.mbxFileBinaryData.readDWord(0x100);
            this.header.globalVarsOffset     = self.mbxFileBinaryData.readDWord(0x104);
            this.header.procsNamesOffset     = self.mbxFileBinaryData.readDWord(0x108);
            this.header.typesOffset          = self.mbxFileBinaryData.readDWord(0x10C);
            this.header.procsVarNamesOffset  = self.mbxFileBinaryData.readDWord(0x110);
            this.header.dllTable             = self.mbxFileBinaryData.readDWord(0x114);

            this.header.globalVarsCount      = self.mbxFileBinaryData.readWord(0x118);
            this.header.procsCount           = self.mbxFileBinaryData.readWord(0x11A);
            this.header.typesCount           = self.mbxFileBinaryData.readWord(0x11C);
            this.header.dllsCount            = self.mbxFileBinaryData.readWord(0x11E);
            this.header.unknown              = self.mbxFileBinaryData.readWord(0x120);

            readProcList(self);
        }

        private void readProcList(MBXFile self)
        {
            this.proclist = new MBXProcInfo[this.header.procsCount];
            uint curroffset = this.header.procsNamesOffset;

            for (int i = 0; i < this.header.procsCount; i++)
            {
                this.proclist[i].procOffset = self.mbxFileBinaryData.readDWord(curroffset);
                curroffset +=4;
                this.proclist[i].procName = self.mbxFileBinaryData.readMBXString(curroffset);
                curroffset +=(uint)this.proclist[i].procName.GetLength()+1;
            }
        }

        public int GetProcCount()
        {
            return this.header.procsCount;
        }

        public uint GetProcffset(int index)
        {
            return this.proclist[index].procOffset;
        }

        public uint GetVarNamesOffset()
        {
            return this.header.procsVarNamesOffset;
        }

        public uint GetTypeCount()
        {
            return this.header.typesCount;
        }

        public uint GetTypesOffset()
        {
            return this.header.typesOffset;
        }

        public uint GetGlobalVarsCount()
        {
            return this.header.globalVarsCount;
        }

        public uint GetGlobalVarsOffset()
        {
            return this.header.globalVarsOffset;
        }

        public string GetProcName(int index)
        {
            return this.proclist[index].procName.ToString();
        }

        public uint GetDllCount()
        {
            return this.header.dllsCount;
        }

        public uint GetDllOffset()
        {
            return this.header.dllTable;
        }

        public bool isNewVersion()
        {
            return this.isMoreThen850Version;
        }
    }
}
