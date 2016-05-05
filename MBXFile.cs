using MBXD.Decompiller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBXD.Decompiler
{
    [Serializable]
    class MBXFile
    {
        public bool isAutoDec = false;

        private string               filename;
        public MBXFileBinaryDataData mbxFileBinaryData;
        public MBXHeader             header;
        public MBXProcs              procs;
        public MBXNames              names;
        public MBXTypes              types;
        public MBXGVars              gVars;
        public MBXDisassembler       dasm;
        public MBXDecompiler         decompiler;

        public MBXFile(string name)
        {
            this.filename = name;
            this.mbxFileBinaryData = new MBXFileBinaryDataData(this);
            this.header            = new MBXHeader(this);
            this.procs             = new MBXProcs(this);
            this.types             = new MBXTypes(this);
            this.gVars             = new MBXGVars(this);
            this.names             = new MBXNames(this);            
            this.dasm              = new MBXDisassembler(this);
            this.decompiler        = new MBXDecompiler(this);
        }

        public string GetFileName()
        {
            return this.filename;
        }

        
    }
}