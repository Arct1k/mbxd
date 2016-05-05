using MBXD.Decompiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBXD.Decompiller
{
    public struct MBXProcHeader
    {
        public uint SelfBase   ;
        public uint CodeStart;
        public uint DataStart;
        public uint DataEnd;
        public ushort C1        ;
        public ushort LocConstCnt; // array of 3-byte recs at the end of DataB //Each rec is Byte DT, Word Ofs in the DataB
        public ushort LocVarCnt;
        public ushort ArgCnt;
        public MBXDataType ResType;
        public ushort C6   ;
        public uint X1;
        public uint Z2;
        public uint Size;
    }

    class MBXProc
    {
        MBXProcHeader procHeader;
        byte[] binCode;
        byte[] binData;
        MBXProcVar[] procVars;
        int selfIndex;
        string selfName;

        public MBXProc(MBXFile self,int index)
        {
            this.selfIndex = index;
            this.selfName = self.header.GetProcName(index);
            uint offset                 = self.header.GetProcffset(index);
            this.procHeader.SelfBase    = self.mbxFileBinaryData.readDWord(offset); offset += 4;
            this.procHeader.CodeStart   = self.mbxFileBinaryData.readDWord(offset); offset += 4;
            this.procHeader.DataStart   = self.mbxFileBinaryData.readDWord(offset); offset += 4;
            this.procHeader.DataEnd     = self.mbxFileBinaryData.readDWord(offset); offset += 4;
            this.procHeader.C1          = self.mbxFileBinaryData.readWord(offset); offset += 2;
            this.procHeader.LocConstCnt = self.mbxFileBinaryData.readWord(offset); offset += 2;
            this.procHeader.LocVarCnt   = self.mbxFileBinaryData.readWord(offset); offset += 2;
            this.procHeader.ArgCnt      = self.mbxFileBinaryData.readWord(offset); offset += 2;
            this.procHeader.ResType     = (MBXDataType)self.mbxFileBinaryData.readWord(offset); offset += 2;
            this.procHeader.C6          = self.mbxFileBinaryData.readWord(offset); offset += 2;
            this.procHeader.X1          = self.mbxFileBinaryData.readDWord(offset); offset += 4;
            this.procHeader.Z2          = self.mbxFileBinaryData.readDWord(offset); offset += 4;
            this.procHeader.Size        = self.mbxFileBinaryData.readDWord(offset); offset += 4;


            this.binCode = self.mbxFileBinaryData.readByteArray(this.procHeader.CodeStart + this.procHeader.SelfBase, this.procHeader.DataStart - this.procHeader.CodeStart);
            this.binData = self.mbxFileBinaryData.readByteArray(this.procHeader.DataStart + this.procHeader.SelfBase, this.procHeader.DataEnd - this.procHeader.DataStart);

            this.procVars = new MBXProcVar[this.procHeader.LocVarCnt];

            offset = this.procHeader.SelfBase + this.procHeader.DataEnd;

            for (int i = 0; i < this.procVars.Length; i++)
            {
                this.procVars[i].dataType  = (MBXDataType)self.mbxFileBinaryData.readWord(offset); offset += 2;
                this.procVars[i].arraySize = self.mbxFileBinaryData.readWord(offset); offset += 2;
                this.procVars[i].typeNum   = self.mbxFileBinaryData.readWord(offset); offset += 2;
                this.procVars[i].zero      = self.mbxFileBinaryData.readWord(offset); offset += 2;
            }
        }

        public int getVarVount()
        {
            return this.procHeader.LocVarCnt;
        }

        public string getProcName()
        {
            return selfName;
        }

        public uint GetProcCodeStartOffset()
        {
            return this.procHeader.CodeStart + this.procHeader.SelfBase;
        }

        public uint GetProcCodeEndOffset()
        {
            return this.procHeader.DataStart + this.procHeader.SelfBase;
        }

        public string GetConstPChar(uint dataoffset)
        {
            int i = 0;
            for (i =  (int)dataoffset; i < this.binData.Length; i++)
            {
                if (binData[i] == 0) break;
            }
            return System.Text.Encoding.ASCII.GetString(binData,(int)dataoffset,i - (int)dataoffset);
        }

        public int GetConstInteger(uint offset)
        {
            return BitConverter.ToInt32(binData, (int)offset);
        }

        public float GetConstFloat(uint offset)
        {
            return (float)BitConverter.ToDouble(binData, (int)offset);
        }

        public bool IsProcVarArray(uint varIndex)
        {
            return this.procVars[varIndex].arraySize != 0xFFFF;
        }

        public uint GetVarUserDefinedVar(uint varIndex)
        {
            uint result = 0xFFFF;
            if (this.procVars[varIndex].dataType == MBXDataType.dtType)
            {
                return this.procVars[varIndex].typeNum;
            }
            return result;
        }

        public MBXDataType GetLocalVarType(uint varIndex)
        {
            return this.procVars[varIndex].dataType;
        }

        public uint GetProcEnd()
        {
            return this.procHeader.SelfBase + this.procHeader.DataEnd;
        }

        public ushort GetArgCount()
        {
            return this.procHeader.ArgCnt;
        }

        public MBXDataType GetResultType()
        {
            return this.procHeader.ResType;
        }
    }

    class MBXProcs
    {
        private MBXProc[] procList;

        public MBXProcs(MBXFile self)
        {
            this.procList = new MBXProc[self.header.GetProcCount()];

            for (int i = 0; i < this.procList.Length; i++)
            {
                this.procList[i] = new MBXProc(self, i);
            }
        }

        public MBXDataType GetResultType(int index)
        {
            return procList[index].GetResultType();
        }

        public ushort GetArgCount(int index)
        {
            return procList[index].GetArgCount();
        }

        public int GetProcVarCount(int procIndex)
        {
            return procList[procIndex].getVarVount();
        }

        public string GetProcName(int index)
        {
            return this.procList[index].getProcName();
        }

        public uint GetProcCodeStartOffset(int procIndex)
        {
            return this.procList[procIndex].GetProcCodeStartOffset();
        }

        public uint GetProcCodeEndOffset(int procIndex)
        {
            return this.procList[procIndex].GetProcCodeEndOffset();
        }

        public string GetProcConstPChar(int procIndex, uint offset)
        {
            return this.procList[procIndex].GetConstPChar(offset);
        }

        public int GetProcConstInteger(int procIndex, uint offset)
        {
            return this.procList[procIndex].GetConstInteger(offset);
        }

        public float GetProcConstFloat(int procIndex, uint offset)
        {
            return this.procList[procIndex].GetConstFloat(offset);
        }
        public string GetProcNameByOffset(uint offset)
        {
            string result = "unresolved_function_0x" + offset.ToString("X8");

            for (int i = 0; i < this.procList.Length; i++)
            {
                if (this.procList[i].GetProcCodeStartOffset() == offset)
                {
                    result = this.procList[i].getProcName();
                    break;
                }
            }
            return result;
        }

        public bool IsProcVarArray(uint varIndex, uint procIndex)
        {
            return this.procList[procIndex].IsProcVarArray(varIndex);
        }

        public uint GetVarUserDefinedVar(uint varIndex, uint procIndex)
        {
            return this.procList[procIndex].GetVarUserDefinedVar(varIndex);
        }

        public uint GetLaspProcDataEnd()
        {
            return this.procList[this.procList.Length - 1].GetProcEnd();
        }

        public MBXDataType GetLocalVarType(uint procIndex,uint varIndex)
        {
            return this.procList[procIndex].GetLocalVarType(varIndex);
        }
    }
}
