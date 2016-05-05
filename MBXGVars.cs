using MBXD.Decompiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBXD.Decompiller
{
    class MBXGVars
    {
        MBXProcVar[] globalVars;
        MBXProcVar[] uglobalVars;

        public MBXGVars(MBXFile self)
        {
            List<MBXProcVar> temp_list = new List<MBXProcVar>();
            uint offset = self.procs.GetLaspProcDataEnd();
            if (self.procs.GetLaspProcDataEnd() < self.header.GetGlobalVarsOffset())
            {
                while (offset < self.header.GetGlobalVarsOffset())
                {
                    MBXProcVar tmpVar = new MBXProcVar();
                    tmpVar.dataType = (MBXDataType)self.mbxFileBinaryData.readWord(offset); offset += 2;
                    tmpVar.arraySize = self.mbxFileBinaryData.readWord(offset); offset += 2;
                    tmpVar.typeNum = self.mbxFileBinaryData.readWord(offset); offset += 2;
                    tmpVar.zero = self.mbxFileBinaryData.readWord(offset); offset += 2;
                    temp_list.Add(tmpVar);
                }
            }

            this.uglobalVars = new MBXProcVar[temp_list.Count];
            temp_list.CopyTo(this.uglobalVars);

            this.globalVars = new MBXProcVar[self.header.GetGlobalVarsCount()];
            offset = self.header.GetGlobalVarsOffset();
            for (int i = 0; i < this.globalVars.Length; i++)
            {                
                this.globalVars[i].dataType = (MBXDataType)self.mbxFileBinaryData.readWord(offset); offset += 2;
                this.globalVars[i].arraySize = self.mbxFileBinaryData.readWord(offset); offset += 2;
                this.globalVars[i].typeNum = self.mbxFileBinaryData.readWord(offset); offset += 2;
                this.globalVars[i].zero = self.mbxFileBinaryData.readWord(offset); offset += 2;
            }
        }

        public bool VarIsArray(int index)
        {
            if (index > this.globalVars.Length - 1) return false;
            return this.globalVars[index].arraySize != 0xFFFF;
        }

        public bool VarIsType(int index)
        {
            if (index > this.globalVars.Length - 1) return false;
            return this.globalVars[index].dataType == MBXDataType.dtType;
        }

        public uint GetVarType(int index)
        {
            uint result = 0xFFFF;
            if (this.globalVars[index].dataType == MBXDataType.dtType) result = this.globalVars[index].typeNum;
            return result;
        }

        public uint GetUVarType(int index)
        {
            uint result = 0xFFFF;
            if (index > this.uglobalVars.Length)
            {
                if (index <= this.globalVars.Length)
                {
                    if (this.globalVars[index - 1].dataType == MBXDataType.dtType)
                        result = this.globalVars[index - 1].typeNum;
                }
                else
                {
                    if (this.uglobalVars[index - this.globalVars.Length].dataType == MBXDataType.dtType)
                        result = this.uglobalVars[index - this.globalVars.Length].typeNum;
                }
            }
            else
            {
                if (this.uglobalVars[index].dataType == MBXDataType.dtType) result = this.uglobalVars[index].typeNum;
            }
            return result;
        }
        
    }
}
