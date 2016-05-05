using MBXD.Decompiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBXD.Decompiller
{
    
    class MBXTypes
    {
        MBXType[] types;
        MBXFile self;

        public MBXTypes(MBXFile self)
        {
            this.self = self;
            this.types = new MBXType[self.header.GetTypeCount()];
            uint offset = self.header.GetTypesOffset();
            for (int i = 0; i < this.types.Length; i++)
            {
                this.types[i].fieldsCount = self.mbxFileBinaryData.readWord(offset); offset += 2;
                this.types[i].fields = new MBXProcVar[this.types[i].fieldsCount];
                for (int ii = 0; ii < this.types[i].fieldsCount; ii++)
                {
                    this.types[i].fields[ii].dataType = (MBXDataType)self.mbxFileBinaryData.readWord(offset); offset += 2;
                    this.types[i].fields[ii].arraySize = self.mbxFileBinaryData.readWord(offset); offset += 2;
                    this.types[i].fields[ii].typeNum = self.mbxFileBinaryData.readWord(offset); offset += 2;
                    this.types[i].fields[ii].zero = self.mbxFileBinaryData.readWord(offset); offset += 2;
                }
            }
        }

        public uint GetTypeFieldType(int tIndex, int fIndex)
        {
            uint result = 0xFFFF;
            if (tIndex > this.types.Length) return result;
            if (self.header.isNewVersion())
            {
                if (fIndex > this.types[tIndex].fields.Length) return result;
                if (this.types[tIndex].fields[fIndex - 1].dataType == MBXDataType.dtType)
                    result = this.types[tIndex].fields[fIndex - 1].typeNum;
                return result;
            }
            else
            {
                if (fIndex > this.types[tIndex+1].fields.Length) return result;
                if (this.types[tIndex+1].fields[fIndex - 1].dataType == MBXDataType.dtType)
                    result = this.types[tIndex + 1].fields[fIndex - 1].typeNum;
                return result;
            }
        }
    }
}
