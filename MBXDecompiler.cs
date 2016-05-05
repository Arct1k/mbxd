using MBXD.Decompiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace MBXD.Decompiller
{

    class MBXDNode
    {
        MBXInstruction[] nodeInstructions;
        MBXFile self;
        public bool isDecompiled;
        public bool isRemoved;

        uint                 nodeStartOffset;
        uint                 GOTOoffset = 0xFFFFFFFF;
        uint                 NEXToffset = 0xFFFFFFFF;

        int GOTOIndex =-1;
        int NEXTIndex =-1;

        List<int> FROMIndexes;
        public List<string> sourceCode;

        public MBXDNode(MBXFile file,MBXInstruction[] nodeAsm,uint procStartOffset)
        {
            this.nodeInstructions = nodeAsm;
            this.nodeStartOffset = this.nodeInstructions[0].offset;
            this.FROMIndexes = new List<int>();
            this.sourceCode = new List<string>();
            this.isDecompiled = false;
            this.self = file;

            if (this.nodeInstructions[nodeInstructions.Length - 1].instructionType != MBXInstructionType.itJmp)
            {
                this.NEXToffset = this.nodeInstructions[nodeInstructions.Length - 1].offset + (uint)this.nodeInstructions[nodeInstructions.Length - 1].dump.Length; ;
                return;
            }
            this.GOTOoffset = this.nodeInstructions[nodeInstructions.Length - 1].arguments[0].DWORD + procStartOffset;
            if (this.nodeInstructions[nodeInstructions.Length - 1].mnemonic == "JMP")
                this.NEXToffset = 0xFFFFFFFF;
            else
                this.NEXToffset = this.nodeInstructions[nodeInstructions.Length - 1].offset + (uint)this.nodeInstructions[nodeInstructions.Length - 1].dump.Length;
        }

        public string GetNodeAsText(int procIndex)
        {
            string result = "";
            MBXDataType lastDataType = MBXDataType.dtInt;
            string args = "";
            for (int i = 0; i < this.nodeInstructions.Length; i++)
            {
                result += "0x" + this.nodeInstructions[i].offset.ToString("X8") + "  ::  " + this.nodeInstructions[i].mnemonic;
                args = "";
                if (this.nodeInstructions[i].arguments != null)
                {
                    
                    foreach (MBXInstructionArg arg in this.nodeInstructions[i].arguments)
                    {
                        uint lastDllName = 0;
                        if (!arg.isString)
                            switch (arg.argType)
                            {
                                case MBXInstructionArgType.atByte:
                                    args += "0x" + arg.DWORD.ToString("X2") + " ";
                                    break;
                                case MBXInstructionArgType.atWord:
                                    args += "0x" + arg.DWORD.ToString("X4") + " ";
                                    break;
                                case MBXInstructionArgType.atDword:
                                    args += "0x" + arg.DWORD.ToString("X8") + " ";
                                    break;
                                case MBXInstructionArgType.atDataType:
                                    lastDataType = (MBXDataType)arg.DWORD;
                                    args += "0x" + arg.DWORD.ToString("X2") + "(" + Converter.DecodeDataType((MBXDataType)arg.DWORD) + ") ";
                                    break;
                                case MBXInstructionArgType.atLocalVariable:
                                    args += "0x" + arg.DWORD.ToString("X4") + "(\"" + self.names.GetProcVariableName((uint)procIndex, arg.DWORD) + "\")";
                                    break;
                                case MBXInstructionArgType.atIntegratedFunction:
                                    args += "0x" + arg.DWORD.ToString("X4") + "(\"" + Converter.GetIntegratedFunctionName((ushort)arg.DWORD) + "\")";
                                    break;
                                case MBXInstructionArgType.atUserFunction:
                                    args += "0x" + arg.DWORD.ToString("X4") + "(\"" + self.procs.GetProcNameByOffset((uint)((int)arg.DWORD + (int)self.procs.GetProcCodeStartOffset(procIndex))) + "\")";
                                    break;
                                case MBXInstructionArgType.atDllName:
                                    args += "0x" + arg.DWORD.ToString("X4") + "(\"" + self.names.GetDllName(arg.DWORD) + "\")";
                                    lastDllName = arg.DWORD;
                                    break;
                                case MBXInstructionArgType.atDllFunction:
                                    args += "0x" + arg.DWORD.ToString("X4") + "(\"" + self.names.GetDLLFunctionName(lastDllName, arg.DWORD) + "\")";
                                    break;
                                case MBXInstructionArgType.atLocalConstant:
                                    switch (lastDataType)
                                    {
                                        case MBXDataType.dtPChar:
                                        case MBXDataType.dtProcName:
                                            args += "\"" + self.procs.GetProcConstPChar(procIndex, arg.DWORD) + "\"";
                                            break;
                                        case MBXDataType.dtInt:
                                        case MBXDataType.dtInteger:
                                            args += "0x" + arg.DWORD.ToString("X4") + "(" + self.procs.GetProcConstInteger(procIndex, arg.DWORD).ToString() + ") ";
                                            break;
                                        case MBXDataType.dtFloat:
                                        case MBXDataType.dtDouble:
                                            args += "0x" + arg.DWORD.ToString("X4") + "(" + self.procs.GetProcConstFloat(procIndex, arg.DWORD).ToString() + ") ";
                                            break;
                                    }
                                    break;
                            }
                        else args += arg.str;
                    }
                }
                result += " " + args + "\n";
            }
            return result;
        }

        public MBXPrimaryInstructionCode GetInstructionCode(int index)
        {
            return (MBXPrimaryInstructionCode)this.nodeInstructions[index].dump[0];
        }

        public uint GetInstructionArg(int isntrIndex,int argIndex)
        {
            return this.nodeInstructions[isntrIndex].arguments[argIndex].DWORD;
        }

        public void SetGotoIndex(int toNode)
        {
            this.GOTOIndex = toNode;
        }

        public void SetNextIndex(int toNode)
        {
            this.NEXTIndex = toNode;
        }

        public void AddFromIndex(int index)
        {
            this.FROMIndexes.Add(index);
        }

        public uint GetGotoOffset()
        {
            return this.GOTOoffset;
        }

        public uint GetNextOffset()
        {
            return this.NEXToffset;
        }

        public uint GetNodeStartOffset()
        {
            return this.nodeStartOffset;
        }

        public int GetGotoIndex()
        {
            return this.GOTOIndex;
        }

        public int GetNextIndex()
        {
            return this.NEXTIndex;
        }

        public int GetInstructionsCount()
        {
            return this.nodeInstructions.Length;
        }

        public uint GetInsstructionArg(int instruction,int arg)
        {
            return this.nodeInstructions[instruction].arguments[arg].DWORD;
        }

        public MBXInstructionType GetInstructionType(int index)
        {
            return this.nodeInstructions[index].instructionType;
        }

        public string GetStackloadedData(int procIndex,int instructionIndex)
        {
            if (this.nodeInstructions[instructionIndex].instructionType != MBXInstructionType.itPush) return "!ERROR!";

            switch ((MBXPrimaryInstructionCode)this.nodeInstructions[instructionIndex].dump[0])
            {
                case MBXPrimaryInstructionCode.LOADARG:
                    return self.names.GetProcVariableName((uint)procIndex, this.nodeInstructions[instructionIndex].arguments[1].DWORD);
                case MBXPrimaryInstructionCode.LOADUGLOBAL:
                    return "GlobalVar_"+this.nodeInstructions[instructionIndex].arguments[1].DWORD.ToString("X8");
                case MBXPrimaryInstructionCode.LOADGLOBAL:
                    return self.names.GetGlobalVariableName((int)this.nodeInstructions[instructionIndex].arguments[1].DWORD);
                case MBXPrimaryInstructionCode.LOADLOCALCONST:
                    switch ((MBXDataType)this.nodeInstructions[instructionIndex].arguments[0].DWORD)
                    {
                        case MBXDataType.dtProcName:
                            return self.procs.GetProcConstPChar(procIndex, this.nodeInstructions[instructionIndex].arguments[1].DWORD);
                        case MBXDataType.dtPChar   :
                            return "\""+self.procs.GetProcConstPChar(procIndex, this.nodeInstructions[instructionIndex].arguments[1].DWORD)+"\"";
                        case MBXDataType.dtInt:
                        case MBXDataType.dtInteger:
                            return self.procs.GetProcConstInteger(procIndex, this.nodeInstructions[instructionIndex].arguments[1].DWORD).ToString();
                        case MBXDataType.dtFloat:
                        case MBXDataType.dtDouble:
                            return self.procs.GetProcConstFloat(procIndex, this.nodeInstructions[instructionIndex].arguments[1].DWORD).ToString();
                    }
                    break;
            }
            return "!ERROR WHILE GET STACK DATA ON PROC : 0x"+procIndex.ToString("X8")+"::INSTRUCTION 0x"+instructionIndex.ToString("X8")+"!";
        }

        public uint GetArgTypeNum(int procIndex, int instructionIndex)
        {
            uint result = 0xFFFF;
            switch ((MBXPrimaryInstructionCode)this.nodeInstructions[instructionIndex].dump[0])
            {
                case MBXPrimaryInstructionCode.LOADARG:
                    result = self.procs.GetVarUserDefinedVar(this.nodeInstructions[instructionIndex].arguments[1].DWORD, (uint)procIndex);
                    break;
                case MBXPrimaryInstructionCode.LOADUGLOBAL:
                    result = self.gVars.GetUVarType((int)this.nodeInstructions[instructionIndex].arguments[1].DWORD);
                    break;
                case MBXPrimaryInstructionCode.LOADGLOBAL:
                    result = self.gVars.GetVarType((int)this.nodeInstructions[instructionIndex].arguments[1].DWORD);
                    break;
            }
            return result;
        }

        public int GetArgArraysTypesCount(int procIndex, int instructionIndex)
        {
            int result = 0;
            switch ((MBXPrimaryInstructionCode)this.nodeInstructions[instructionIndex].dump[0])
            {
                case MBXPrimaryInstructionCode.LOADARG:
                case MBXPrimaryInstructionCode.LOADUGLOBAL:
                case MBXPrimaryInstructionCode.LOADGLOBAL:
                    result = (int)this.nodeInstructions[instructionIndex].arguments[2].DWORD;
                    break;
            }
            return result;
        }



        public string GetMathSymbol(int instructionIndex)
        {
            if (this.nodeInstructions[instructionIndex].instructionType != MBXInstructionType.itMath) return "!ERROR WHILE GET MATH SYMBOL ON INSTRUCTION 0x" + instructionIndex.ToString("X8") + "!";
            switch ((MBXPrimaryInstructionCode)this.nodeInstructions[instructionIndex].dump[0])
            {
                case MBXPrimaryInstructionCode.ADD: return         " + ";
                case MBXPrimaryInstructionCode.SUB: return         " - ";
                case MBXPrimaryInstructionCode.IMUL: return        " * ";
                case MBXPrimaryInstructionCode.DIV: return         " / ";
                case MBXPrimaryInstructionCode.IDIV: return        " \\ ";
                case MBXPrimaryInstructionCode.MOD: return         " MOD ";
                case MBXPrimaryInstructionCode.POW: return         " ^ ";
                case MBXPrimaryInstructionCode.CONCATINATE: return " + ";
            }
            return "!ERROR WHILE GET MATH SYMBOL ON INSTRUCTION 0x" + instructionIndex.ToString("X8") + "!";
        }

        public string GetLogicSymbol(int instructionIndex)
        {
            if (this.nodeInstructions[instructionIndex].instructionType != MBXInstructionType.itLogic) return "!ERROR WHILE GET LOGIC SYMBOL ON INSTRUCTION 0x" + instructionIndex.ToString("X8") + "!";
            switch ((MBXPrimaryInstructionCode)this.nodeInstructions[instructionIndex].dump[0])
            {
                case MBXPrimaryInstructionCode.CMPBW: return " BETWEEN ";
                case MBXPrimaryInstructionCode.CMPEQ: return " = ";
                case MBXPrimaryInstructionCode.CMPGE: return " >= ";
                case MBXPrimaryInstructionCode.CMPLE: return " <= ";
                case MBXPrimaryInstructionCode.CMPLT: return " < ";
                case MBXPrimaryInstructionCode.CMPGT: return " > ";
                case MBXPrimaryInstructionCode.CMPNE: return " <> ";
            }
            return "!ERROR WHILE GET LOGIC SYMBOL ON INSTRUCTION 0x" + instructionIndex.ToString("X8") + "!";
        }

        public string GetStatemnt(int procIndex, int InstructionIndex)
        {
            if ((MBXPrimaryInstructionCode)this.nodeInstructions[InstructionIndex].dump[0] == MBXPrimaryInstructionCode.ENDSTATE) return "";
            return Converter.GetIntegratedFunctionName((ushort)this.nodeInstructions[InstructionIndex].arguments[0].DWORD);
        }

        public string GetCallName(int index,int procIndex)
        {
            switch (this.nodeInstructions[index].instructionType)
            {
                case MBXInstructionType.itCallF:
                    switch ((MBXPrimaryInstructionCode)this.nodeInstructions[index].dump[0])
                    {
                        case MBXPrimaryInstructionCode.CALL:
                            return Converter.GetIntegratedFunctionName((ushort)this.nodeInstructions[index].arguments[0].DWORD);
                        case MBXPrimaryInstructionCode.CALLUSERF:
                            return self.procs.GetProcNameByOffset((uint)((int)this.nodeInstructions[index].arguments[0].DWORD + (int)self.procs.GetProcCodeStartOffset(procIndex)));
                        case MBXPrimaryInstructionCode.CALLDLLF:
                            return self.names.GetDLLFunctionName((ushort)this.nodeInstructions[index].arguments[0].DWORD, (ushort)this.nodeInstructions[index].arguments[1].DWORD);
                        default: return "!ERROR!";
                    }
                case MBXInstructionType.itCallP:
                    //return "!!FIX IT !! ITS RESOLVER CALL USER P";
                    return self.procs.GetProcNameByOffset((uint)((int)this.nodeInstructions[index].arguments[0].DWORD + (int)self.procs.GetProcCodeStartOffset(procIndex)));
                default:
                    return "!ERROR!";
            }
        }

        public int GetCallArgCount(int index, int procIndex)
        {
            switch ((MBXPrimaryInstructionCode)this.nodeInstructions[index].dump[0])
            {
                case MBXPrimaryInstructionCode.CALL:      //return (int)this.nodeInstructions[index].arguments[1].DWORD;
                case MBXPrimaryInstructionCode.CALLUSERF: //return (int)this.nodeInstructions[index].arguments[1].DWORD;
                case MBXPrimaryInstructionCode.CALLUSERP: //return (int)this.nodeInstructions[index].arguments[1].DWORD;
                case MBXPrimaryInstructionCode.CALLUSERP2:  return (int)this.nodeInstructions[index].arguments[1].DWORD;
                case MBXPrimaryInstructionCode.CALLDLLF:    return (int)this.nodeInstructions[index].arguments[2].DWORD;
            }

            return 0;
        }
    }

    class MBXDecompiler
    {
        private MBXFile self;
        private MBXInstruction[] inputAsmCode;
        private MBXDNode[] graph;

        private MBXDNode[] graphOriginal;

        private int lastProcIndex;


        public MBXDecompiler(MBXFile self)
        {
            this.self = self;
        }

        public string[] decompileProc(int procIndex)
        {
            
            this.lastProcIndex = procIndex;
            this.inputAsmCode = self.dasm.DisassembleProc(procIndex);
            this.MarkLabels();
            this.BuildGraph();
            
            if (!self.isAutoDec) return null;
            if (graph.Length == 1)
            {
                DecompileNode(ref graph[0]);
            }
            else
                while (true)
                {
                    if (ProcessDoCase()) continue;
                    if (ProcessIfThen()) continue;
                    if (ProcessIfThenElse()) continue;
                    if (ProcessWhileWhend()) continue;
                    if (ProcessDoLoop()) continue;
                    if (ProcessForNext()) continue;
                    if (ProcessSimpleNext()) continue;
                    break;
                }
            for (int i = 0; i < graph[0].sourceCode.Count; i++)
            {
                if (graph[0].sourceCode[i].Trim() == "") graph[0].sourceCode.RemoveAt(i);
            }

            // building header 

            if (!graph[0].isDecompiled) DecompileNode(ref graph[0]);
            switch (self.procs.GetResultType(procIndex))
            {
                case MBXDataType.dtVoid:
                    string args = "";
                    if (self.procs.GetArgCount(procIndex) == 0)
                        graph[0].sourceCode.Insert(0, "SUB " + self.procs.GetProcName(procIndex));
                    else
                    {
                        
                        for (int a = 0; a < self.procs.GetArgCount(procIndex); a++)
                        {
                            args += self.names.GetProcVariableName((uint)procIndex, (uint)a) + " as " + Converter.DecodeDataType((MBXDataType)self.procs.GetLocalVarType((uint)procIndex, (uint)a)) + ",";
                        }
                        args = args.Remove(args.Length - 1);
                        graph[0].sourceCode.Insert(0, "SUB " + self.procs.GetProcName(procIndex) + "(" + args + ")");
                    }
                    break;
                default:
                    string ar = "";
                    if (self.procs.GetArgCount(procIndex) == 0)
                        graph[0].sourceCode.Insert(0, "FUNCTION " + self.procs.GetProcName(procIndex));
                    else
                    {
                        
                        for (int a = 0; a < self.procs.GetArgCount(procIndex); a++)
                        {
                            ar += self.names.GetProcVariableName((uint)procIndex, (uint)a) + " as " + Converter.DecodeDataType((MBXDataType)self.procs.GetLocalVarType((uint)procIndex, (uint)a)) + ",";
                        }
                        args = ar.Remove(ar.Length - 1);
                        graph[0].sourceCode.Insert(0, "SUB " + self.procs.GetProcName(procIndex) + "(" + ar + ")");
                    }
                    break;
            }
            //building local vars 
            for (int a = self.procs.GetArgCount(procIndex); a < self.procs.GetProcVarCount(procIndex); a++)
            {
                if (!self.names.GetProcVariableName((uint)procIndex, (uint)a).Contains("__MapBasic__")) 
                graph[0].sourceCode.Insert(1, "DIM " + self.names.GetProcVariableName((uint)procIndex, (uint)a) + " as " + Converter.DecodeDataType((MBXDataType)self.procs.GetLocalVarType((uint)procIndex, (uint)a)));
            }

            return graph[0].sourceCode.ToArray();
        }

        private void MarkLabels()
        {
            uint startOffset = this.inputAsmCode[0].offset;
            for (int i = 0; i < this.inputAsmCode.Length; i++)
            {
                if (this.inputAsmCode[i].instructionType == MBXInstructionType.itJmp)
                {
                    this.inputAsmCode[i + 1].isLabel = true;
                    for (int ii = 0; ii < this.inputAsmCode.Length; ii++)
                    {
                        if (this.inputAsmCode[ii].offset == this.inputAsmCode[i].arguments[0].DWORD + startOffset)
                        {
                            this.inputAsmCode[ii].isLabel = true;
                        }
                    }
                }
            }
        }


        public void BuildGraph()
        {
            List<MBXDNode> result        = new List<MBXDNode>();
            List<MBXInstruction> tmpNode = new List<MBXInstruction>();
            uint startOffset = this.inputAsmCode[0].offset;

            for (int i = 0; i < this.inputAsmCode.Length; i++)
            {
                if (this.inputAsmCode[i].isLabel)
                {
                    if (tmpNode.Count > 0)
                    {
                        result.Add(new MBXDNode(self, tmpNode.ToArray(), startOffset));
                        tmpNode.Clear();
                    }
                }
                tmpNode.Add(this.inputAsmCode[i]);
            }
            result.Add(new MBXDNode(self,tmpNode.ToArray(), startOffset));//add last node


            for (int i = 0; i < result.Count; i++)
            {
                for (int ii = 0; ii < result.Count; ii++)
                {
                    if (result[ii].GetNodeStartOffset() == result[i].GetGotoOffset())
                    {
                        result[ii].AddFromIndex(i);
                        result[i].SetGotoIndex(ii);
                    }
                    if (result[ii].GetNodeStartOffset() == result[i].GetNextOffset())
                    {
                        result[ii].AddFromIndex(i);
                        result[i].SetNextIndex(ii);
                    }
                }
            }

            this.graph = result.ToArray();
        }

        private void removeNode(int index)
        {
            graph[index].SetGotoIndex(-1);
            graph[index].SetNextIndex(-1);
            graph[index].isRemoved = true;
        }

        public MBXDNode[] GetLastGraph()
        {
            return this.graph;            
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void DecompileNode(ref MBXDNode node)
        {
            Stack<string> stack;
            Stack<uint> lastType;

            string tmpString = "";
            bool isSetValue = false;
            bool isSetFArg = false;
            int exOpened = 0;
            int isArrayOrType = 0;
            int neededArgs = 0;
            bool isRedim = false;

            stack = new Stack<string>();
            lastType = new Stack<uint>();

            if (node.isDecompiled) return;

            for (int i = 0; i < node.GetInstructionsCount(); i++)
            {
                switch (node.GetInstructionType(i))
                {
                    case MBXInstructionType.itSet:
                    case MBXInstructionType.itCase:
                        isSetValue = true;
                        break;
                    case MBXInstructionType.itSetRes:
                        node.sourceCode.Add(tmpString);
                        tmpString = self.procs.GetProcName(lastProcIndex)+" = ";
                        break;
                    case MBXInstructionType.itSetFArg:
                        isSetFArg = true;
                        break;
                    case MBXInstructionType.itPush:
                        if (node.GetArgTypeNum(lastProcIndex, i)!= 0xFFFF) lastType.Push(node.GetArgTypeNum(lastProcIndex, i));
                        isArrayOrType += node.GetArgArraysTypesCount(lastProcIndex, i);
                        if ((isSetValue) && (isArrayOrType == 0))
                        {
                            tmpString += node.GetStackloadedData(lastProcIndex, i) + " = ";
                            isSetValue = false;
                        }
                        else if ((isSetValue) && (isArrayOrType > 0))
                        {
                            stack.Push(node.GetStackloadedData(lastProcIndex, i));
                        }
                        else if ((isSetFArg) && (!(exOpened > 0)))
                        {
                            tmpString += node.GetStackloadedData(lastProcIndex, i) + ",";
                            neededArgs--;
                            isSetFArg = false;
                            if (neededArgs == 0) tmpString = tmpString.Remove(tmpString.Length - 1) + ")";
                        }
                        else
                            if (exOpened > 0)
                                stack.Push(node.GetStackloadedData(lastProcIndex, i));
                            else
                                if (isSetFArg)
                                {
                                    tmpString += node.GetStackloadedData(lastProcIndex, i) + ",";
                                    neededArgs--;
                                    isSetFArg = false;
                                    if (neededArgs == 0) tmpString = tmpString.Remove(tmpString.Length - 1) + ")";
                                }
                                else if ((node.GetArgTypeNum(lastProcIndex, i)!= 0xFFFF))
                                {
                                    stack.Push(node.GetStackloadedData(lastProcIndex, i));
                                }
                                else
                                {
                                    tmpString += node.GetStackloadedData(lastProcIndex, i);
                                }
                        break;
                    case MBXInstructionType.itInfo:
                        string st = "";
                        while (stack.Count > 0) st = stack.Pop() + " " + st;
                        tmpString += st;
                        node.sourceCode.Add(tmpString);
                        tmpString = "";
                        break;
                    case MBXInstructionType.itEx:
                        if (node.GetInstructionCode(i) == MBXPrimaryInstructionCode.EXEND)
                        {
                            if (isArrayOrType > 0)
                            {
                                if (isRedim)
                                {
                                    tmpString += "(" + stack.Pop() + ")";
                                    isRedim = false;
                                    isArrayOrType--;
                                }
                                else
                                {
                                    string arrIndex = stack.Pop();
                                    string arr = stack.Pop() + "(" + arrIndex + ")";
                                    isArrayOrType--;
                                    if ((isArrayOrType == 0) && (isSetValue))
                                    {
                                        tmpString += arr + " = ";
                                        isSetValue = false;
                                    }
                                    else stack.Push(arr);
                                }
                            }
                            else if (isSetFArg)
                            {
                                tmpString += stack.Pop() + ",";
                                neededArgs--;
                                isSetFArg = false;
                                if (neededArgs == 0) tmpString = tmpString.Remove(tmpString.Length - 1) + ")";
                            }
                            else
                            {
                                tmpString += stack.Pop();
                            }
                            exOpened --;
                        }
                        else if (node.GetInstructionCode(i) == MBXPrimaryInstructionCode.EXSTART)
                        {
                            exOpened ++;
                        }
                        break;

                    case MBXInstructionType.itType:
                        uint sT = lastType.Pop();
                        stack.Push(stack.Pop() +"."+ self.names.GetTypeFieldName((int)sT,(int)node.GetInstructionArg(i,0)));
                        if (self.types.GetTypeFieldType((int)sT, (int)node.GetInstructionArg(i, 0)) != 0xFFFF)
                        {
                            lastType.Push(self.types.GetTypeFieldType((int)sT, (int)node.GetInstructionArg(i, 0)));
                        }
                        isArrayOrType--;
                        if ((isSetValue) && (isArrayOrType == 0))
                        {
                            tmpString = stack.Pop() + " = ";
                            isSetValue = false;
                        }
                        break;
                    case MBXInstructionType.itMath:
                        string s = "";
                        if (exOpened > 0)
                        {
                            s = stack.Pop();
                            s = node.GetMathSymbol(i) + s;
                            s = "(" + stack.Pop() + s + ")";
                            stack.Push(s);
                        }
                        else tmpString += node.GetMathSymbol(i);
                        break;
                    case MBXInstructionType.itLogic:
                        s = "";
                        s = stack.Pop();
                        s = node.GetLogicSymbol(i) + s;
                        s = "(" + stack.Pop() + s + ")";
                        stack.Push(s);
                        break;
                    case MBXInstructionType.itBool:
                        s = "";
                        switch (node.GetInstructionCode(i))
                        {
                            case MBXPrimaryInstructionCode.AND:
                                s = stack.Pop();
                                s = stack.Pop() + " AND " + s;
                                stack.Push(s);
                                break;
                            case MBXPrimaryInstructionCode.OR:
                                s = stack.Pop();
                                s = stack.Pop() + " OR " + s;
                                stack.Push(s);
                                break;
                            case MBXPrimaryInstructionCode.NOT:
                                stack.Push("NOT " + stack.Pop());
                                break;
                            case MBXPrimaryInstructionCode.NEG:
                                stack.Push(" - " + stack.Pop());
                                break;
                        }
                        break;
                    case MBXInstructionType.itCallF:
                        s = ")";
                        for (int arg = 0; arg < node.GetCallArgCount(i,lastProcIndex); arg++)
                        {
                            s = "," + stack.Pop() + s; 
                        }
                        if (s.Length > 1)
                            s = node.GetCallName(i, lastProcIndex) + "(" + s.Remove(0, 1);
                        else
                            s = node.GetCallName(i, lastProcIndex) + "(" + s;
                        stack.Push(s);
                        break;
                    case MBXInstructionType.itCallP:
                        node.sourceCode.Add(tmpString);
                        neededArgs = node.GetCallArgCount(i, lastProcIndex);
                        if (neededArgs == 0)
                        {
                            tmpString = "CALL " + node.GetCallName(i, lastProcIndex) + "()";
                            node.sourceCode.Add(tmpString);
                            tmpString = "";
                        }
                        else
                        {
                            tmpString = "CALL " + node.GetCallName(i, lastProcIndex) + "(";
                        }
                        break;
                    case MBXInstructionType.itState:
                        string stt = node.GetStatemnt(lastProcIndex, i);
                        tmpString += " " + stt + " ";
                        if (stt == "redim") isRedim = true;
                        //stack.Push(node.GetStatemnt(lastProcIndex, i) + " ");
                        break;
                    case MBXInstructionType.itExit:
                        switch (node.GetInstructionCode(i))
                        {
                            case MBXPrimaryInstructionCode.EXITSUB:
                                node.sourceCode.Add(tmpString);
                                node.sourceCode.Add("EXIT SUB");
                                tmpString = "";
                                break;
                            case MBXPrimaryInstructionCode.ENDPROGRAM:
                                node.sourceCode.Add(tmpString);
                                node.sourceCode.Add("END PROGRAM");
                                tmpString = "";
                                break;
                        }
                        break;
                }
            }
            if (stack.Count > 0)
                tmpString += stack.Pop();
            node.sourceCode.Add(tmpString);    
            node.isDecompiled = true;
            for (int i = node.sourceCode.Count - 1; i >= 0; i--)
            {
                if (node.sourceCode[i].Trim() == "") node.sourceCode.RemoveAt(i);
            }
        }

        private bool ProcessIfThen()
        {
            

            for (int node = 0; node < graph.Length; node++)
            {
                if ((!graph[node].isRemoved)
                    && (graph[node].GetInstructionCode(graph[node].GetInstructionsCount()-1) == MBXPrimaryInstructionCode.JMPNOK)
                    && (graph[node].GetNextIndex() != -1) 
                    && (graph[node].GetGotoIndex() != -1) 
                    && (graph[graph[node].GetNextIndex()].GetNextIndex() != -1) 
                    && (graph[graph[node].GetNextIndex()].GetGotoIndex() == -1))
                {
                    if (graph[node].GetGotoIndex() == graph[graph[node].GetNextIndex()].GetNextIndex())
                    {
                        this.DecompileNode(ref graph[graph[node].GetNextIndex()]);
                        this.DecompileNode(ref graph[node]);
                        graph[node].sourceCode[graph[node].sourceCode.Count - 1] = "IF " + graph[node].sourceCode[graph[node].sourceCode.Count - 1] + " THEN";
                        graph[node].sourceCode.AddRange(graph[graph[node].GetNextIndex()].sourceCode);
                        graph[node].sourceCode.Add("END IF");
                        graph[graph[node].GetNextIndex()].isRemoved = true;
                        graph[graph[node].GetNextIndex()].SetGotoIndex(-1);
                        graph[graph[node].GetNextIndex()].SetNextIndex(-1);
                        graph[node].SetNextIndex(graph[node].GetGotoIndex());
                        graph[node].SetGotoIndex(-1);                      
                        
                        return true;
                    }
                }
            }
            return false;
        }

        private bool ProcessIfThenElse()
        {
            
            for (int node = 0; node < graph.Length; node++)
            {
                if ((!graph[node].isRemoved) 
                    && (graph[node].GetInstructionCode(graph[node].GetInstructionsCount()-1) == MBXPrimaryInstructionCode.JMPNOK)
                    && (graph[node].GetNextIndex() != -1)
                    && (graph[node].GetGotoIndex() != -1)
                    && (graph[graph[node].GetNextIndex()].GetNextIndex() == -1)
                    && (graph[graph[node].GetNextIndex()].GetGotoIndex() != -1)
                    && (graph[graph[node].GetGotoIndex()].GetNextIndex() != -1)
                    && (graph[graph[node].GetGotoIndex()].GetGotoIndex() == -1))
                {
                    if (graph[graph[node].GetNextIndex()].GetGotoIndex() == graph[graph[node].GetGotoIndex()].GetNextIndex())
                    {
                        this.DecompileNode(ref graph[node]);
                        this.DecompileNode(ref graph[graph[node].GetNextIndex()]);
                        this.DecompileNode(ref graph[graph[node].GetGotoIndex()]);
                        graph[node].sourceCode[graph[node].sourceCode.Count - 1] = "IF " + graph[node].sourceCode[graph[node].sourceCode.Count - 1] + " THEN";
                        graph[node].sourceCode.AddRange(graph[graph[node].GetNextIndex()].sourceCode);
                        graph[node].sourceCode.Add("ELSE");
                        graph[node].sourceCode.AddRange(graph[graph[node].GetGotoIndex()].sourceCode);
                        graph[node].sourceCode.Add("END IF");
                        graph[graph[node].GetNextIndex()].isRemoved = true;
                        graph[graph[node].GetGotoIndex()].isRemoved = true;
                        int o = graph[graph[node].GetNextIndex()].GetGotoIndex();
                        graph[graph[node].GetNextIndex()].SetGotoIndex(-1);
                        graph[graph[node].GetGotoIndex()].SetNextIndex(-1);

                        graph[node].SetNextIndex(o);
                        
                        graph[node].SetGotoIndex(-1);                        
                        return true;
                    }
                }
            }
            return false;
        }

        private bool ProcessWhileWhend()
        {

            for (int node = 0; node < graph.Length; node++)
            {
                if ((!graph[node].isRemoved)
                    && (graph[node].GetNextIndex() != -1)
                    && (graph[node].GetGotoIndex() != -1)
                    && (graph[graph[node].GetNextIndex()].GetNextIndex() == -1)
                    && (graph[graph[node].GetNextIndex()].GetGotoIndex() != -1))
                {
                    if (graph[graph[node].GetNextIndex()].GetGotoIndex() == node)
                    {
                        this.DecompileNode(ref graph[node]);
                        this.DecompileNode(ref graph[graph[node].GetNextIndex()]);
                        graph[node].sourceCode[graph[node].sourceCode.Count - 1] = "WHILE " + graph[node].sourceCode[graph[node].sourceCode.Count - 1];
                        graph[node].sourceCode.AddRange(graph[graph[node].GetNextIndex()].sourceCode);
                        graph[node].sourceCode.Add("WEND");
                        graph[graph[node].GetNextIndex()].isRemoved = true;
                        graph[graph[node].GetNextIndex()].SetGotoIndex(-1);
                        graph[graph[node].GetNextIndex()].SetNextIndex(-1);
                        graph[node].SetNextIndex(graph[node].GetGotoIndex());
                        graph[node].SetGotoIndex(-1);
                        return true;
                    }
                }
            }
            return false;
        }

        private bool ProcessSimpleNext()
        {

            for (int node = 0; node < graph.Length; node++)
            {
                if (graph[node].GetGotoIndex() != -1) continue;
                if (graph[node].GetNextIndex() == -1) continue;
                int next = graph[node].GetNextIndex();
                bool isPresent = false;
                for (int a = 0; a < graph.Length; a++)
                {
                    if (((graph[a].GetNextIndex() == next)
                        | (graph[a].GetGotoIndex() == next))
                        && (a != node))
                    {
                        isPresent = true;
                        break;
                    }
                }
                if (!isPresent)
                {
                    this.DecompileNode(ref graph[node]);
                    this.DecompileNode(ref graph[graph[node].GetNextIndex()]);
                    graph[node].sourceCode.AddRange(graph[next].sourceCode);
                    graph[next].isRemoved = true;
                    graph[node].SetGotoIndex(graph[next].GetGotoIndex());
                    graph[node].SetNextIndex(graph[next].GetNextIndex());
                    graph[next].SetGotoIndex(-1);
                    graph[next].SetNextIndex(-1);
                    return true;
                }
            }
            return false;
        }

        private bool ProcessDoLoop()
        {

            for (int node = 0; node < graph.Length; node++)
            {
                if ((graph[node].GetGotoIndex() != -1)
                    && (graph[node].GetNextIndex() != -1))
                {
                    if (graph[node].GetGotoIndex() == node)
                    {
                        this.DecompileNode(ref graph[node]);
                        graph[node].sourceCode.Insert(0, "DO");
                        graph[node].sourceCode[graph[node].sourceCode.Count - 1] = "LOOP WHILE " + graph[node].sourceCode[graph[node].sourceCode.Count - 1];
                        graph[node].SetGotoIndex(-1);
                    }
                }
            }
            return false;
        }

        private bool ProcessForNext()
        {

            for (int node = 0; node < graph.Length; node++)
            {
                if ((!graph[node].isRemoved)
                    &&(graph[node].GetNextIndex() == -1)
                    &&(graph[node].GetGotoIndex() != -1))
                {
                    if ((graph[graph[node].GetGotoIndex()].GetGotoIndex() != -1)
                        && (graph[graph[node].GetGotoIndex()].GetNextIndex() != -1))
                    {
                        if ((graph[graph[graph[node].GetGotoIndex()].GetGotoIndex()].GetNextIndex() != -1)
                            && (graph[graph[graph[node].GetGotoIndex()].GetGotoIndex()].GetGotoIndex() != -1)
                            && (graph[graph[graph[node].GetGotoIndex()].GetNextIndex()].GetGotoIndex() != -1)
                            && (graph[graph[graph[node].GetGotoIndex()].GetNextIndex()].GetNextIndex() != -1))
                        {
                            if ((graph[graph[graph[graph[node].GetGotoIndex()].GetGotoIndex()].GetNextIndex()].GetNextIndex() == -1)
                                && (graph[graph[graph[graph[node].GetGotoIndex()].GetGotoIndex()].GetNextIndex()].GetGotoIndex() != -1)
                                && (graph[graph[graph[graph[node].GetGotoIndex()].GetGotoIndex()].GetGotoIndex()].GetNextIndex() != -1)
                                && (graph[graph[graph[graph[node].GetGotoIndex()].GetGotoIndex()].GetGotoIndex()].GetGotoIndex() == -1)
                                && (graph[graph[graph[graph[node].GetGotoIndex()].GetNextIndex()].GetGotoIndex()].GetGotoIndex() != -1)
                                && (graph[graph[graph[graph[node].GetGotoIndex()].GetNextIndex()].GetGotoIndex()].GetNextIndex() != -1)
                                && (graph[graph[graph[graph[node].GetGotoIndex()].GetNextIndex()].GetNextIndex()].GetGotoIndex() != -1)
                                && (graph[graph[graph[graph[node].GetGotoIndex()].GetNextIndex()].GetNextIndex()].GetNextIndex() == -1))
                            {   
                                int endindex = graph[graph[graph[graph[node].GetGotoIndex()].GetGotoIndex()].GetNextIndex()].GetGotoIndex();
                                int codeIndex = graph[graph[graph[node].GetGotoIndex()].GetGotoIndex()].GetGotoIndex();
                                if (graph[graph[graph[graph[node].GetGotoIndex()].GetNextIndex()].GetGotoIndex()].GetNextIndex() == endindex)
                                {
                                    this.DecompileNode(ref graph[node]);
                                    
                                    
                                    if (graph[node].sourceCode[graph[node].sourceCode.Count - 1] == "1")
                                    {
                                        string[] start = graph[node].sourceCode[graph[node].sourceCode.Count - 2].Split(new string[1] { "__MapBasic__" }, StringSplitOptions.RemoveEmptyEntries);
                                        graph[node].sourceCode[graph[node].sourceCode.Count - 2] = "";
                                        graph[node].sourceCode[graph[node].sourceCode.Count - 1] = "FOR " + start[0] + " TO " + start[1].Split('=')[1];// +" STEP 1";
                                    }
                                    else
                                    {
                                        string[] start = graph[node].sourceCode[graph[node].sourceCode.Count - 1].Split(new string[1] { "__MapBasic__" }, StringSplitOptions.RemoveEmptyEntries);
                                        graph[node].sourceCode[graph[node].sourceCode.Count - 1] = "FOR " + start[0] + " TO " + start[1].Split('=')[1] + " STEP " + start[2].Split('=')[1];
                                    }
                                    if (!graph[codeIndex].isDecompiled)
                                    {
                                        this.DecompileNode(ref graph[codeIndex]);
                                        graph[codeIndex].sourceCode.RemoveAt(graph[codeIndex].sourceCode.Count - 1);
                                    } 
                                    graph[node].sourceCode.AddRange(graph[codeIndex].sourceCode);
                                    graph[node].sourceCode.Add("NEXT");
                                    graph[graph[graph[graph[node].GetGotoIndex()].GetGotoIndex()].GetNextIndex()].isRemoved = true;
                                    graph[graph[graph[graph[node].GetGotoIndex()].GetGotoIndex()].GetNextIndex()].SetGotoIndex(-1);
                                    graph[graph[graph[graph[node].GetGotoIndex()].GetGotoIndex()].GetNextIndex()].SetNextIndex(-1);
                                    graph[graph[graph[graph[node].GetGotoIndex()].GetGotoIndex()].GetGotoIndex()].isRemoved = true;
                                    graph[graph[graph[graph[node].GetGotoIndex()].GetGotoIndex()].GetGotoIndex()].SetGotoIndex(-1);
                                    graph[graph[graph[graph[node].GetGotoIndex()].GetGotoIndex()].GetGotoIndex()].SetNextIndex(-1);
                                    graph[graph[graph[graph[node].GetGotoIndex()].GetNextIndex()].GetGotoIndex()].isRemoved = true;
                                    graph[graph[graph[graph[node].GetGotoIndex()].GetNextIndex()].GetGotoIndex()].SetGotoIndex(-1);
                                    graph[graph[graph[graph[node].GetGotoIndex()].GetNextIndex()].GetGotoIndex()].SetNextIndex(-1);
                                    graph[graph[graph[graph[node].GetGotoIndex()].GetNextIndex()].GetNextIndex()].isRemoved = true;
                                    graph[graph[graph[graph[node].GetGotoIndex()].GetNextIndex()].GetNextIndex()].SetGotoIndex(-1);
                                    graph[graph[graph[graph[node].GetGotoIndex()].GetNextIndex()].GetNextIndex()].SetNextIndex(-1);
                                    graph[graph[graph[node].GetGotoIndex()].GetGotoIndex()].isRemoved = true;
                                    graph[graph[graph[node].GetGotoIndex()].GetGotoIndex()].SetGotoIndex(-1);
                                    graph[graph[graph[node].GetGotoIndex()].GetGotoIndex()].SetNextIndex(-1);
                                    graph[graph[graph[node].GetGotoIndex()].GetNextIndex()].isRemoved = true;
                                    graph[graph[graph[node].GetGotoIndex()].GetNextIndex()].SetGotoIndex(-1);
                                    graph[graph[graph[node].GetGotoIndex()].GetNextIndex()].SetNextIndex(-1);
                                    graph[graph[node].GetGotoIndex()].isRemoved = true;
                                    graph[graph[node].GetGotoIndex()].SetNextIndex(-1);
                                    graph[graph[node].GetGotoIndex()].SetGotoIndex(-1);
                                    graph[node].SetGotoIndex(-1);
                                    graph[node].SetNextIndex(endindex);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        private bool checkCaseSignature(int nodeIndex)
        {
            int caseCnt = 0;
            int index;
            for (index = 0; index < graph[nodeIndex].GetInstructionsCount(); index++)
            {
                if (graph[nodeIndex].GetInstructionCode(index) == MBXPrimaryInstructionCode.CASECMP) caseCnt++;
            }
            if (caseCnt != 2) return false;
            index--;
            //
            if (graph[nodeIndex].GetInstructionCode(index--) != MBXPrimaryInstructionCode.JMPOK) return false;
            if (graph[nodeIndex].GetInstructionCode(index--) != MBXPrimaryInstructionCode.EXEND) return false;
            if (graph[nodeIndex].GetInstructionCode(index--) != MBXPrimaryInstructionCode.CMPEQ) return false;
            if (graph[nodeIndex].GetInstructionCode(index--) != MBXPrimaryInstructionCode.LOADARG) return false;
            if (graph[nodeIndex].GetInstructionCode(index--) != MBXPrimaryInstructionCode.LOADARG) return false;
            if (graph[nodeIndex].GetInstructionCode(index++) != MBXPrimaryInstructionCode.EXSTART) return false;
            if (!self.names.GetProcVariableName((uint)this.lastProcIndex, graph[nodeIndex].GetInstructionArg(index++, 1)).Contains("__MapBasic__")) return false;
            if (!self.names.GetProcVariableName((uint)this.lastProcIndex, graph[nodeIndex].GetInstructionArg(index, 1)).Contains("__MapBasic__")) return false;
            return true;
        }

        private bool ProcessDoCase()
        {
            //return false;
            //int caseElse = -1;
            int rem = -1;
            for (int node = 0; node < graph.Length; node++)
            {
                if (checkCaseSignature(node))
                {
                    if (graph[node].GetNextIndex() == -1) return false;
                    if (graph[graph[node].GetNextIndex()].GetGotoIndex() == -1) return false;
                    int caseEnd = graph[graph[node].GetGotoIndex()].GetGotoIndex();
                    
                    int currCase = node;
                    
                    this.DecompileNode(ref graph[node]);
                    string varName = graph[node].sourceCode[graph[node].sourceCode.Count - 2].Split(new string[1] { "__MapBasic__" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    graph[node].sourceCode[graph[node].sourceCode.Count - 3] = graph[node].sourceCode[graph[node].sourceCode.Count - 3].Split(new string[1] { "__MapBasic__" }, StringSplitOptions.None)[0];
                    string caseVal = graph[node].sourceCode[graph[node].sourceCode.Count - 1].Split(new string[1] { "(__MapBasic__" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    graph[node].sourceCode.RemoveRange(graph[node].sourceCode.Count - 2, 2);
                    graph[node].sourceCode.Add("DO CASE " + varName);
                    graph[node].sourceCode.Add("CASE " + caseVal);
                    int caseExpr = graph[currCase].GetGotoIndex();
                    currCase = graph[currCase].GetNextIndex();
                    do
                    {
                        if (graph[currCase].GetGotoIndex() == caseExpr)//if same case 
                        {
                            this.DecompileNode(ref graph[currCase]);
                            caseVal = graph[currCase].sourceCode[graph[currCase].sourceCode.Count - 1].Split(new string[1] { "(__MapBasic__" }, StringSplitOptions.RemoveEmptyEntries)[0].Split('=')[1];
                            graph[node].sourceCode[graph[node].sourceCode.Count - 1] = graph[node].sourceCode[graph[node].sourceCode.Count - 1] + "," + caseVal;
                            rem = currCase;
                            currCase = graph[currCase].GetNextIndex();
                            this.removeNode(rem);
                            continue;
                        }
                        else
                        {
                            this.DecompileNode(ref graph[caseExpr]);
                            graph[node].sourceCode.AddRange(graph[caseExpr].sourceCode);
                            rem = currCase;
                            currCase = graph[currCase].GetGotoIndex();
                            this.removeNode(rem);
                        }
                        if ((currCase == caseEnd))
                        {
                            //case end
                            graph[node].sourceCode.Add("END CASE");
                            this.removeNode(caseExpr);
                            break;
                        }
                        if ((graph[currCase].GetNextIndex() == caseEnd))
                        {
                            //case else
                            this.DecompileNode(ref graph[currCase]);
                            graph[node].sourceCode.Add("CASE ELSE");
                            graph[node].sourceCode.AddRange(graph[currCase].sourceCode);
                            graph[node].sourceCode.Add("END CASE");
                            this.removeNode(caseExpr);
                            this.removeNode(currCase);
                            break;
                        }
                        this.DecompileNode(ref graph[currCase]);
                        caseVal = graph[currCase].sourceCode[graph[currCase].sourceCode.Count - 1].Split(new string[1] { "(__MapBasic__" }, StringSplitOptions.RemoveEmptyEntries)[0];
                        graph[node].sourceCode.Add("CASE " + caseVal);
                        rem = caseExpr;
                        caseExpr = graph[currCase].GetGotoIndex();
                        this.removeNode(rem);
                        rem = currCase;
                        currCase = graph[currCase].GetNextIndex();
                        this.removeNode(rem);
                    } while (true);
                    graph[node].SetGotoIndex(-1);
                    graph[node].SetNextIndex(caseEnd);
                    
                }
            }
            return false;
        }
    }
}
