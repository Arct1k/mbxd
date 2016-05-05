using MBXD.Decompiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBXD.Decompiller
{
    public struct MBXNamesHeader
    {
        public uint globalVarsStart;
        public uint typesStart;
        public uint localVarsStart;
        public uint LocalVarsEnd;
        public uint C1;
    }

    public struct MBXProcVarNames
    {
        public uint parrentProcOffset;
        public uint dataEnd;
        public uint varsCount;
        public MBXString[] names;
    }

    public struct MBXTypeName
    {
        public MBXString     typeName;
        public MBXString[]   itemNames;
    }

    public struct MBXDllNames
    {
        public MBXString     dllName;
        public MBXString[]   functionNames;
    }

    class MBXNames
    {
        private MBXNamesHeader     namesHeader;
        private MBXProcVarNames[]  procVarNames;
        private MBXString[]        globalVarNames;
        private MBXTypeName[]      typeNames;
        private MBXDllNames[]      dllNames;
        private MBXFile self;
        

        public MBXNames(MBXFile self)
        {
            this.self = self;
            uint offset = self.header.GetVarNamesOffset();
            this.namesHeader.globalVarsStart     = self.mbxFileBinaryData.readDWord(offset); offset += 4;
            this.namesHeader.typesStart          = self.mbxFileBinaryData.readDWord(offset); offset += 4;
            this.namesHeader.localVarsStart      = self.mbxFileBinaryData.readDWord(offset); offset += 4;
            this.namesHeader.LocalVarsEnd        = self.mbxFileBinaryData.readDWord(offset); offset += 4;
            this.namesHeader.C1                  = self.mbxFileBinaryData.readDWord(offset); offset += 4;

            this.procVarNames = new MBXProcVarNames[self.header.GetProcCount()];
            offset = this.namesHeader.localVarsStart;
            for (int i = 0; i < this.procVarNames.Length; i++)
            {
                
                this.procVarNames[i].parrentProcOffset = self.mbxFileBinaryData.readDWord(offset); offset += 4;
                this.procVarNames[i].dataEnd = self.mbxFileBinaryData.readDWord(offset); offset += 4;
                this.procVarNames[i].varsCount = self.mbxFileBinaryData.readDWord(offset); offset += 4;
                this.procVarNames[i].names = new MBXString[this.procVarNames[i].varsCount];
                if (self.procs.GetProcVarCount(i) == 0) continue;
                for (int ii = 0; ii < this.procVarNames[i].varsCount; ii++)
                {
                    this.procVarNames[i].names[ii] = self.mbxFileBinaryData.readMBXString(offset); offset += (uint)1 + this.procVarNames[i].names[ii].GetLength();
                }
            }
            this.globalVarNames = new MBXString[self.header.GetGlobalVarsCount()];
            offset = this.namesHeader.globalVarsStart;
            for (int i = 0; i < this.globalVarNames.Length; i++)
            {
                this.globalVarNames[i] = self.mbxFileBinaryData.readMBXString(offset);
                offset += (uint)0x1 + this.globalVarNames[i].GetLength();
            }
            offset = this.namesHeader.typesStart;
            this.typeNames = new MBXTypeName[self.header.GetTypeCount()];
            for (int i = 0; i < this.typeNames.Length; i++)
            {
                offset += 4;//skip addr of next type names 
                uint itemCount = self.mbxFileBinaryData.readDWord(offset); offset += 4;
                this.typeNames[i].itemNames = new MBXString[itemCount - 1];
                this.typeNames[i].typeName = self.mbxFileBinaryData.readMBXString(offset); offset += (uint)1 + this.typeNames[i].typeName.GetLength();
                for (int ii = 0; ii < this.typeNames[i].itemNames.Length; ii++)
                {
                    this.typeNames[i].itemNames[ii] = self.mbxFileBinaryData.readMBXString(offset); offset += (uint)1 + this.typeNames[i].itemNames[ii].GetLength();
                }
            }

            this.dllNames = new MBXDllNames[self.header.GetDllCount()];
            offset = self.header.GetDllOffset();
            for (int i = 0; i < this.dllNames.Length; i++)
            {
                ushort namesCount = self.mbxFileBinaryData.readWord(offset); offset += 2;
                this.dllNames[i].functionNames = new MBXString[namesCount];
                this.dllNames[i].dllName = self.mbxFileBinaryData.readMBXString(offset); offset += (uint)1 + this.dllNames[i].dllName.GetLength();
                if (self.header.isNewVersion())
                    offset += 2;                
                for (int ii = 0; ii < namesCount; ii++)
                {
                    this.dllNames[i].functionNames[ii] = self.mbxFileBinaryData.readMBXString(offset); offset += (uint)1 + this.dllNames[i].functionNames[ii].GetLength();
                }
            }
        }


        public string GetProcVariableName(uint procIndex, uint varIndex)
        {
            //return "ITS DEBUG : PINDEX = "+procIndex.ToString()+" , VIndex = 0"+varIndex.ToString();
            if ((varIndex > this.procVarNames[procIndex].names.Length) || (this.procVarNames[procIndex].names.Length == 0))
            {
                return "var_" + varIndex.ToString("X8");
            }
            return this.procVarNames[procIndex].names[varIndex].ToString();
        }

        public string GetGlobalVariableName(int index)
        {
            if (this.globalVarNames.Length <= index) return "GlobalVariable_0x"+index.ToString("X8");
            return this.globalVarNames[index].ToString();
        }

        public string GetDllName(uint index)
        {
            return this.dllNames[index].dllName.ToString();
        }

        public string GetDLLFunctionName(uint indexDll, uint indexF)
        {
            return this.dllNames[indexDll].functionNames[indexF].ToString();
        }

        public string GetTypeFieldName(int typeIndex, int fieldIndex)
        {
            //if (this.self.header.isNewVersion())
            return this.typeNames[typeIndex].itemNames[fieldIndex-1].ToString();
            //else return this.typeNames[typeIndex+1].itemNames[fieldIndex-1].ToString();
        }
    }
}
