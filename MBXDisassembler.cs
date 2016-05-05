using MBXD.Decompiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBXD.Decompiller
{
    public enum MBXPrimaryInstructionCode
    {
        SETFUNCARG1 = 0x01,
        SETFUNCARG3 = 0x03,
        SETFUNCARG8 = 0x08,
        IDIV        = 0x0A,
        MOD         = 0x0B,                            // MOD 2 args from stack and push result
        ADD         = 0x0C,                            // Summ 2 args from stack and push result
        SUB         = 0x0D,                            // Sub 2 args from stack and push result
        IMUL        = 0x0E,                            // Mul 2 args from stack and push result
        DIV         = 0x0F,                            // Division 2 args from stack and push result
        NEG         = 0x10,                            // Negative stack value
        OR          = 0x11,                            // logic or
        AND         = 0x12,                            // logic and 
        NOT         = 0x13,                            // logic not
        CMPEQ       = 0x14,                            // compare if equal
        CMPNE       = 0x15,                            // compare if not equal
        CMPLE       = 0x16,                            // compare less|equal
        CMPGE       = 0x17,                            // compare great|equal
        CMPLT       = 0x18,                            // compare less then
        CMPGT       = 0x19,                            // compare great then
        CMPBW       = 0x1A,                            // compare between
        POW         = 0x1E,                            // pow 2 args 

        CONCATINATE = 0x25,                            // concatenation strings

        LOADUGLOBAL = 0xE1,                            // push global var to stack
        ENDPROGRAM    = 0xE3,                            // terminate program
        CALLDLLF      = 0xE5,                            // call dll function
        SETRESULT     = 0xE7,                            // set function result
        CALLUSERF2    = 0xE8,
        RESUME        = 0xE9,                            // on resume goto
        ONERROR       = 0xEA,                            // check is error
        SETVALUE      = 0xEB,                            // set value to next stack argument
        TYPEELEMENT   = 0xEC,                            // case type member 
        CALLUSERP2    = 0xED,                            // call user procedure
        CALLUSERP     = 0xEE,                            // call self user function
        JMPOK         = 0xEF,                            // case jump


        LOADLOCALCONST = 0xF1,                            // loading local constant to stack
        LOADARG        = 0xF2,                            // Loading argument to stack
        LOADGLOBAL     = 0xF3,                            // loading global argument 
        CALLUSERF      = 0xF4,                            // call user function , dword offset , byte arg count , return datatype , [arg datatypes]
        CALL           = 0xF5,                            // Call function 
        JMP            = 0xF6,                            // jump if condition
        JMPNOK         = 0xF7,                            // jump if not condition
        EXITSUB        = 0xF8,                            // Exit sub
        CASECMP        = 0xF9,                            // case branch
        SRCLINE        = 0xFA,                            // Indicate source line number
        STATESTART     = 0xFC,                            // call statement
        EXSTART        = 0xFD,                            // Expr start
        ENDSTATE       = 0xFE,                            // End statement call
        EXEND          = 0xFF,                            // Expr End
    }

    public enum MBXInstructionType
    {
        itMath,
        itJmp,
        itPush,
        itCallF,
        itCallP,
        itInfo,
        itSet,
        itSetRes,
        itSetFArg,
        itEx,
        itLogic,
        itExit,
        itState,
        itBool,
        itCase,
        itType,
        itUnknown
    }

    public enum MBXInstructionArgType
    {
        atByte,
        atWord,
        atDword,
        atDataType,
        atGlobalVariable,
        atLocalVariable,
        atLocalConstant,
        atIntegratedFunction,
        atUserFunction,
        atDllFunction,
        atDllName
    }

    public struct MBXInstructionArg
    {
        public MBXInstructionArgType argType;
        public bool isString;
        public uint DWORD;
        public string str;
    }

    public struct MBXInstruction
    {
        public bool isLabel;
        public uint offset;
        public byte[] dump;
        public MBXInstructionType instructionType;
        public string mnemonic;
        public MBXInstructionArg[] arguments;
        public string comment;
    }


    class MBXDisassembler
    {
        private MBXFile self;
        private MBXInstruction[] lastProc;

        public MBXDisassembler(MBXFile d)
        {
            this.self = d;
        }

        public MBXInstruction DisassembleInstruction(ref uint offset,int isExpr,bool isState)
        {
            MBXInstruction result;
            MBXPrimaryInstructionCode currentInstruction;

            result.isLabel = false;
            result.offset = offset;
            result.dump = self.mbxFileBinaryData.readByteArray(offset,1);
            currentInstruction = (MBXPrimaryInstructionCode)self.mbxFileBinaryData.readByte(offset); offset += 1;
            result.instructionType = MBXInstructionType.itUnknown;
            result.mnemonic = "UNKNWM";
            result.comment = " ERROR !!! UNKNOWN INSTRUCTION !!!";
            result.arguments = null;

            

            switch (currentInstruction)
            {
                case MBXPrimaryInstructionCode.ADD:
                    result.instructionType = MBXInstructionType.itMath;
                    result.mnemonic = "ADD";
                    result.comment = "summ 2 args from stack";
                    return result;
                case MBXPrimaryInstructionCode.SRCLINE:
                    result.dump = self.mbxFileBinaryData.readByteArray(offset - 1, 3);
                    result.instructionType = MBXInstructionType.itInfo;
                    result.mnemonic = "SRCLINE";
                    result.comment = "Source code line number";
                    result.arguments = new MBXInstructionArg[1];
                    result.arguments[0].argType = MBXInstructionArgType.atWord;
                    result.arguments[0].isString = false;
                    result.arguments[0].DWORD = self.mbxFileBinaryData.readWord(offset);
                    offset += 2;
                    return result;
                case MBXPrimaryInstructionCode.TYPEELEMENT:
                    result.dump = self.mbxFileBinaryData.readByteArray(offset - 1, 3);
                    result.instructionType = MBXInstructionType.itType;
                    result.mnemonic = "TYPEELEMENT";
                    result.comment = "Type element number";
                    result.arguments = new MBXInstructionArg[1];
                    result.arguments[0].argType = MBXInstructionArgType.atWord;
                    result.arguments[0].isString = false;
                    result.arguments[0].DWORD = self.mbxFileBinaryData.readWord(offset);
                    offset += 2;
                    return result;
                case MBXPrimaryInstructionCode.SETVALUE:
                    result.instructionType = MBXInstructionType.itSet;
                    result.mnemonic = "SETVALUE";
                    result.comment = "set value for next loaded variable";
                    return result;
                case MBXPrimaryInstructionCode.LOADARG:
                    result.dump = self.mbxFileBinaryData.readByteArray(offset - 1, 5);
                    result.instructionType = MBXInstructionType.itPush;
                    result.mnemonic = "LOADARG";
                    result.comment = "Loading local variable to stack";
                    result.arguments = new MBXInstructionArg[3];
                    result.arguments[0].argType = MBXInstructionArgType.atByte;
                    result.arguments[0].DWORD = self.mbxFileBinaryData.readByte(offset);
                    result.arguments[1].argType = MBXInstructionArgType.atLocalVariable;
                    result.arguments[1].DWORD = self.mbxFileBinaryData.readWord(offset+1);
                    result.arguments[2].argType = MBXInstructionArgType.atByte;
                    result.arguments[2].DWORD = self.mbxFileBinaryData.readByte(offset+3);
                    offset += 4;
                    return result;
                case MBXPrimaryInstructionCode.LOADUGLOBAL:
                case MBXPrimaryInstructionCode.LOADGLOBAL:
                    result.dump = self.mbxFileBinaryData.readByteArray(offset - 1, 5);
                    result.instructionType = MBXInstructionType.itPush;
                    result.mnemonic = "LOADGLOBALARG";
                    result.comment = "Loading global variable to stack";
                    result.arguments = new MBXInstructionArg[3];
                    result.arguments[0].argType = MBXInstructionArgType.atByte;
                    result.arguments[0].DWORD = self.mbxFileBinaryData.readByte(offset);
                    result.arguments[1].argType = MBXInstructionArgType.atGlobalVariable;
                    result.arguments[1].DWORD = self.mbxFileBinaryData.readWord(offset + 1);
                    result.arguments[2].argType = MBXInstructionArgType.atByte;
                    result.arguments[2].DWORD = self.mbxFileBinaryData.readByte(offset + 3);
                    offset += 4;
                    return result;
                case MBXPrimaryInstructionCode.LOADLOCALCONST:
                    result.instructionType = MBXInstructionType.itPush;
                    result.dump = self.mbxFileBinaryData.readByteArray(offset - 1, 4);
                    result.arguments = new MBXInstructionArg[2];
                    result.arguments[0].argType = MBXInstructionArgType.atDataType;
                    result.arguments[0].DWORD = self.mbxFileBinaryData.readByte(offset);
                    result.arguments[1].argType = MBXInstructionArgType.atLocalConstant;
                    result.arguments[1].DWORD = self.mbxFileBinaryData.readWord(offset + 1);
                    result.mnemonic = "LOADLOCALCONST";
                    result.comment = "loading constant to stack";
                    offset += 3;
                    return result;
                case MBXPrimaryInstructionCode.CALL:
                    result.instructionType = MBXInstructionType.itCallF;
                    result.dump = self.mbxFileBinaryData.readByteArray(offset - 1, 4);
                    result.arguments = new MBXInstructionArg[2];
                    result.arguments[0].argType = MBXInstructionArgType.atIntegratedFunction;
                    result.arguments[0].DWORD = self.mbxFileBinaryData.readWord(offset);
                    result.arguments[1].argType = MBXInstructionArgType.atByte;
                    result.arguments[1].DWORD = self.mbxFileBinaryData.readByte(offset + 2);
                    result.mnemonic = "CALL";
                    result.comment = "call integrated function";
                    offset += 3;
                    return result;
                case MBXPrimaryInstructionCode.CALLDLLF:
                    result.instructionType = MBXInstructionType.itCallF;
                    byte argc = self.mbxFileBinaryData.readByte(offset + 4);
                    result.arguments = new MBXInstructionArg[argc + 4];
                    result.arguments[0].argType = MBXInstructionArgType.atDllName;
                    result.arguments[0].DWORD = self.mbxFileBinaryData.readWord(offset);
                    result.arguments[1].argType = MBXInstructionArgType.atDllFunction;
                    result.arguments[1].DWORD = self.mbxFileBinaryData.readWord(offset+2);
                    result.arguments[2].DWORD = self.mbxFileBinaryData.readByte(offset + 4);
                    result.arguments[2].argType = MBXInstructionArgType.atByte;
                    for (int i = 0; i <= result.arguments[2].DWORD; i++)
                    {
                        result.arguments[i+3].DWORD = self.mbxFileBinaryData.readByte(offset + (uint)(5+i));
                        result.arguments[i+3].argType = MBXInstructionArgType.atDataType;
                    }                    
                    result.mnemonic = "CALLDLLF";
                    result.comment = "call DLL function";
                    result.dump = self.mbxFileBinaryData.readByteArray(offset - 1, (uint)argc + 7);
                    offset += (uint)argc + 6;
                    return result;
                case MBXPrimaryInstructionCode.CALLUSERF:
                case MBXPrimaryInstructionCode.CALLUSERF2:
                    result.instructionType = MBXInstructionType.itCallF;
                    argc = self.mbxFileBinaryData.readByte(offset + 4);
                    result.arguments = new MBXInstructionArg[argc + 3];
                    result.arguments[0].DWORD = self.mbxFileBinaryData.readDWord(offset);
                    result.arguments[0].argType = MBXInstructionArgType.atUserFunction;
                    result.arguments[1].DWORD = self.mbxFileBinaryData.readByte(offset + 4);
                    result.arguments[1].argType = MBXInstructionArgType.atByte;
                    for (int i = 0; i <= result.arguments[1].DWORD; i++)
                    {
                        result.arguments[i + 2].DWORD = self.mbxFileBinaryData.readByte(offset + (uint)(5 + i));
                        result.arguments[i + 2].argType = MBXInstructionArgType.atDataType;
                    }
                    result.mnemonic = "CALLUSERF";
                    result.comment = "call user function";
                    result.dump = self.mbxFileBinaryData.readByteArray(offset - 1, (uint)argc + 7);
                    offset += (uint)argc + 6;
                    return result;
                case MBXPrimaryInstructionCode.CALLUSERP:
                    result.instructionType = MBXInstructionType.itCallP;
                    result.dump = self.mbxFileBinaryData.readByteArray(offset - 1, 7);
                    result.arguments = new MBXInstructionArg[2];
                    result.arguments[0].argType = MBXInstructionArgType.atUserFunction;
                    result.arguments[0].DWORD = self.mbxFileBinaryData.readDWord(offset);
                    result.arguments[1].argType = MBXInstructionArgType.atByte;
                    result.arguments[1].DWORD = self.mbxFileBinaryData.readWord(offset + 4);
                    result.mnemonic = "CALLUSERP";
                    result.comment = "call user function";
                    offset += 6;
                    return result;
                case MBXPrimaryInstructionCode.JMP:
                    result.dump = self.mbxFileBinaryData.readByteArray(offset - 1, 3);
                    result.instructionType = MBXInstructionType.itJmp;
                    result.mnemonic = "JMP";
                    result.comment = "Jump to offset";
                    result.arguments = new MBXInstructionArg[1];
                    result.arguments[0].argType = MBXInstructionArgType.atWord;
                    result.arguments[0].isString = false;
                    result.arguments[0].DWORD = self.mbxFileBinaryData.readWord(offset);
                    offset += 2;
                    return result;
                case MBXPrimaryInstructionCode.JMPNOK:
                    result.dump = self.mbxFileBinaryData.readByteArray(offset - 1, 3);
                    result.instructionType = MBXInstructionType.itJmp;
                    result.mnemonic = "JMPNOK";
                    result.comment = "Jump to offset if not condition";
                    result.arguments = new MBXInstructionArg[1];
                    result.arguments[0].argType = MBXInstructionArgType.atWord;
                    result.arguments[0].isString = false;
                    result.arguments[0].DWORD = self.mbxFileBinaryData.readWord(offset);
                    offset += 2;
                    return result;
                case MBXPrimaryInstructionCode.JMPOK:
                    result.dump = self.mbxFileBinaryData.readByteArray(offset - 1, 3);
                    result.instructionType = MBXInstructionType.itJmp;
                    result.mnemonic = "JMPOK";
                    result.comment = "Jump to offset if condition";
                    result.arguments = new MBXInstructionArg[1];
                    result.arguments[0].argType = MBXInstructionArgType.atWord;
                    result.arguments[0].isString = false;
                    result.arguments[0].DWORD = self.mbxFileBinaryData.readWord(offset);
                    offset += 2;
                    return result;
                case MBXPrimaryInstructionCode.EXSTART:
                    result.instructionType = MBXInstructionType.itEx;
                    result.mnemonic = "EXSTART";
                    result.comment = "Expr start";
                    return result;
                case MBXPrimaryInstructionCode.STATESTART:
                    result.instructionType = MBXInstructionType.itState;
                    result.dump = self.mbxFileBinaryData.readByteArray(offset - 1, 3);
                    result.mnemonic = "STATESTART";
                    result.comment = "start statement";
                    result.arguments = new MBXInstructionArg[1];
                    result.arguments[0].argType = MBXInstructionArgType.atIntegratedFunction;
                    result.arguments[0].isString = false;
                    result.arguments[0].DWORD = self.mbxFileBinaryData.readWord(offset);
                    offset += 2;
                    return result;
                case MBXPrimaryInstructionCode.SETFUNCARG1:
                case MBXPrimaryInstructionCode.SETFUNCARG3:
                case MBXPrimaryInstructionCode.SETFUNCARG8:
                    if (!isState)
                        if (isExpr==0)
                        {
                            result.instructionType = MBXInstructionType.itSetFArg;
                            result.mnemonic = "SETFUNCARG";
                            result.comment = "Set function argument";
                            return result;
                        }
                        else
                        {
                            result.dump = self.mbxFileBinaryData.readByteArray(offset - 1, 2);
                            result.instructionType = MBXInstructionType.itState;
                            result.mnemonic = "STATEMENT";
                            result.arguments = new MBXInstructionArg[1];
                            result.arguments[0].argType = MBXInstructionArgType.atIntegratedFunction;
                            result.arguments[0].isString = false;
                            result.arguments[0].DWORD = self.mbxFileBinaryData.readWord(offset - 1);
                            result.comment = "statement command \"" + Converter.GetIntegratedFunctionName((ushort)result.arguments[0].DWORD) + "\"";
                            offset += 1;
                        }
                    break;
                case MBXPrimaryInstructionCode.EXEND:
                    result.instructionType = MBXInstructionType.itEx;
                    result.mnemonic = "EXEND";
                    result.comment = "Expr end";
                    return result;
                case MBXPrimaryInstructionCode.ENDSTATE:
                    result.instructionType = MBXInstructionType.itState;
                    result.mnemonic = "ENDSTATE";
                    result.comment = "End statement";
                    return result;
                case MBXPrimaryInstructionCode.CASECMP:
                    result.instructionType = MBXInstructionType.itCase;
                    result.mnemonic = "CASECMP";
                    result.comment = "case compare";
                    return result;
                case MBXPrimaryInstructionCode.EXITSUB:
                    result.instructionType = MBXInstructionType.itExit;
                    result.mnemonic = "EXITSUB";
                    result.comment = "exit sub";
                    return result;
                case MBXPrimaryInstructionCode.SETRESULT:
                    result.instructionType = MBXInstructionType.itSetRes;
                    result.mnemonic = "SETRESULT";
                    result.comment = "set function result";
                    return result;
                case MBXPrimaryInstructionCode.ENDPROGRAM:
                    result.instructionType = MBXInstructionType.itExit;
                    result.mnemonic = "ENDPROGRAM";
                    result.comment = "exit app";
                    return result;
                case MBXPrimaryInstructionCode.ONERROR:
                    result.instructionType = MBXInstructionType.itExit;
                    result.mnemonic = "ONERROR";
                    switch (self.mbxFileBinaryData.readByte(offset))
                    {
                        case 0xF6 :
                            result.dump = self.mbxFileBinaryData.readByteArray(offset - 1, 4);
                            result.arguments = new MBXInstructionArg[1];
                            result.arguments[0].argType = MBXInstructionArgType.atWord;
                            result.arguments[0].isString = false;
                            result.arguments[0].DWORD = self.mbxFileBinaryData.readWord(offset+1);
                            result.comment = "on error goto";
                            offset += 3;
                            break;
                        case 0xE2:
                            result.dump = self.mbxFileBinaryData.readByteArray(offset - 1, 2);
                            result.arguments = new MBXInstructionArg[1];
                            result.arguments[0].argType = MBXInstructionArgType.atWord;
                            result.arguments[0].isString = false;
                            result.arguments[0].DWORD = 0;//self.mbxFileBinaryData.readWord(offset + 1);
                            result.comment = "on error disable";
                            offset += 1;
                            break;
                    }
                    
                    return result;
                case MBXPrimaryInstructionCode.RESUME:
                    result.instructionType = MBXInstructionType.itExit;
                    result.mnemonic = "RESUME";
                    
                    switch (self.mbxFileBinaryData.readByte(offset))
                    {
                        case 0x03:
                            result.dump = self.mbxFileBinaryData.readByteArray(offset - 1, 4);
                            result.arguments = new MBXInstructionArg[1];
                            result.arguments[0].argType = MBXInstructionArgType.atWord;
                            result.arguments[0].isString = false;
                            result.arguments[0].DWORD = self.mbxFileBinaryData.readWord(offset + 1);
                            offset += 3;
                            result.comment = "resume goto label";
                            break;
                        case 0x02:
                            result.dump = self.mbxFileBinaryData.readByteArray(offset - 1, 2);
                            result.arguments = new MBXInstructionArg[1];
                            result.arguments[0].argType = MBXInstructionArgType.atWord;
                            result.arguments[0].isString = false;
                            result.arguments[0].DWORD = 0;//self.mbxFileBinaryData.readWord(offset + 1);
                            offset += 1;
                            result.comment = "resume retry";
                            break;
                        case 0x01:
                            result.dump = self.mbxFileBinaryData.readByteArray(offset - 1, 2);
                            result.arguments = new MBXInstructionArg[1];
                            result.arguments[0].argType = MBXInstructionArgType.atWord;
                            result.arguments[0].isString = false;
                            result.arguments[0].DWORD = 0xFFFF;//self.mbxFileBinaryData.readWord(offset + 1);
                            result.comment = "resume next";
                            offset += 1;
                            break;
                    }
                    return result;
            }

            if (isExpr>0)
                switch (currentInstruction)
                {
                    case MBXPrimaryInstructionCode.CMPBW:
                        if (isExpr==0) break;
                        result.instructionType = MBXInstructionType.itLogic;
                        result.mnemonic = "CMPBTW";
                        result.comment = "compare between";
                        return result;
                    case MBXPrimaryInstructionCode.CMPEQ:
                        result.instructionType = MBXInstructionType.itLogic;
                        result.mnemonic = "CMPEQ";
                        result.comment = "compare equal";
                        return result;
                    case MBXPrimaryInstructionCode.CMPGE:
                        result.instructionType = MBXInstructionType.itLogic;
                        result.mnemonic = "CMPGE";
                        result.comment = "compare great or equal";
                        return result;
                    case MBXPrimaryInstructionCode.CMPGT:
                        result.instructionType = MBXInstructionType.itLogic;
                        result.mnemonic = "CMPGT";
                        result.comment = "compare great then";
                        return result;
                    case MBXPrimaryInstructionCode.CMPLE:
                        result.instructionType = MBXInstructionType.itLogic;
                        result.mnemonic = "CMPLE";
                        result.comment = "compare less or equal";
                        return result;
                    case MBXPrimaryInstructionCode.CMPLT:
                        result.instructionType = MBXInstructionType.itLogic;
                        result.mnemonic = "CMPLT";
                        result.comment = "compare less then";
                        return result;
                    case MBXPrimaryInstructionCode.CMPNE:
                        result.instructionType = MBXInstructionType.itLogic;
                        result.mnemonic = "CMPNE";
                        result.comment = "compare not equal";
                        return result;
                    case MBXPrimaryInstructionCode.EXITSUB:
                        result.instructionType = MBXInstructionType.itExit;
                        result.mnemonic = "EXITSUB";
                        result.comment = "exit sub proc";
                        return result;
                    case MBXPrimaryInstructionCode.CASECMP:
                        result.instructionType = MBXInstructionType.itCase;
                        result.mnemonic = "CASECMP";
                        result.comment = "case comparing";
                        return result;
                    case MBXPrimaryInstructionCode.NEG:
                        result.instructionType = MBXInstructionType.itBool;
                        result.mnemonic = "NEG";
                        result.comment = "Negative stack value";
                        return result;
                    case MBXPrimaryInstructionCode.POW:
                        result.instructionType = MBXInstructionType.itMath;
                        result.mnemonic = "POW";
                        result.comment = "POW 2 args from stack";
                        return result;
                    case MBXPrimaryInstructionCode.MOD:
                        result.instructionType = MBXInstructionType.itMath;
                        result.mnemonic = "MOD";
                        result.comment = "mod 2 args from stack";
                        return result;
                    case MBXPrimaryInstructionCode.ADD:
                        result.instructionType = MBXInstructionType.itMath;
                        result.mnemonic = "ADD";
                        result.comment = "summ 2 args from stack";
                        return result;
                    case MBXPrimaryInstructionCode.SUB:
                        result.instructionType = MBXInstructionType.itMath;
                        result.mnemonic = "SUB";
                        result.comment = "sub 2 args from stack";
                        return result;
                    case MBXPrimaryInstructionCode.IMUL:
                        result.instructionType = MBXInstructionType.itMath;
                        result.mnemonic = "IMUL";
                        result.comment = "imul 2 args from stack";
                        return result;
                    case MBXPrimaryInstructionCode.DIV:
                        result.instructionType = MBXInstructionType.itMath;
                        result.mnemonic = "DIV";
                        result.comment = "division 2 args from stack";
                        return result;
                    case MBXPrimaryInstructionCode.IDIV:
                        result.instructionType = MBXInstructionType.itMath;
                        result.mnemonic = "IDIV";
                        result.comment = "integer divide (drop remainder) 2 args from stack";
                        return result;
                    case MBXPrimaryInstructionCode.CONCATINATE:
                        result.instructionType = MBXInstructionType.itMath;
                        result.mnemonic = "CONCATINATE";
                        result.comment = "concatinate 2 args from stack";
                        return result;
                    case MBXPrimaryInstructionCode.OR:
                        result.instructionType = MBXInstructionType.itBool;
                        result.mnemonic = "OR";
                        result.comment = "Logic OR";
                        return result;
                    case MBXPrimaryInstructionCode.AND:
                        result.instructionType = MBXInstructionType.itBool;
                        result.mnemonic = "AND";
                        result.comment = "Logic AND";
                        return result;
                    case MBXPrimaryInstructionCode.NOT:
                        result.instructionType = MBXInstructionType.itBool;
                        result.mnemonic = "NOT";
                        result.comment = "Logic NOT";
                        return result;
                    default:
                        return result;
                }
            else
            {
                    result.dump = self.mbxFileBinaryData.readByteArray(offset - 1, 2);
                    result.instructionType = MBXInstructionType.itState;
                    result.mnemonic = "STATEMENT";
                    result.arguments = new MBXInstructionArg[1];
                    result.arguments[0].argType = MBXInstructionArgType.atIntegratedFunction;
                    result.arguments[0].isString = false;
                    result.arguments[0].DWORD = self.mbxFileBinaryData.readWord(offset - 1);
                    result.comment = "statement command \"" + Converter.GetIntegratedFunctionName((ushort)result.arguments[0].DWORD) + "\"";
                    offset += 1;
            }



            return result;
        }


        public MBXInstruction[] DisassembleProc(int procIndex)
        {
            List<MBXInstruction> result = new List<MBXInstruction>();

            uint currOffset = self.procs.GetProcCodeStartOffset(procIndex);
            uint endOffset = self.procs.GetProcCodeEndOffset(procIndex);
            int isExpr = 0;
            bool isState = false;

            while (currOffset < endOffset)
            {
                
                result.Add(DisassembleInstruction(ref currOffset,isExpr,isState));
                if (result[result.Count - 1].mnemonic == "EXSTART") isExpr++;
                else if (result[result.Count - 1].mnemonic == "EXEND") isExpr--;
                else if (result[result.Count - 1].mnemonic == "STATESTART") isState = true;
                else if (result[result.Count - 1].mnemonic == "ENDSTATE") isState = false;
            }
            this.lastProc = result.ToArray();
            return this.lastProc;
        }

        public MBXInstruction[] GetLastDisasmed()
        {
            return this.lastProc;
        }

    }
}
