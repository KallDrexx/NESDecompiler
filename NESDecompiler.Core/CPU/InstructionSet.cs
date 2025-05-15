using System;
using System.Collections.Generic;

namespace NESDecompiler.Core.CPU
{
    /// <summary>
    /// Addressing modes for 6502 CPU instructions
    /// </summary>
    public enum AddressingMode
    {
        Implied,            // No operand (e.g., RTS)
        Accumulator,        // Operand is the accumulator (e.g., LSR A)
        Immediate,          // Operand is 8-bit value (e.g., LDA #$12)
        ZeroPage,           // Operand is 8-bit address (e.g., LDA $12)
        ZeroPageX,          // Operand is 8-bit address, X-indexed (e.g., LDA $12,X)
        ZeroPageY,          // Operand is 8-bit address, Y-indexed (e.g., LDX $12,Y)
        Relative,           // Operand is 8-bit signed offset (e.g., BNE $12)
        Absolute,           // Operand is 16-bit address (e.g., LDA $1234)
        AbsoluteX,          // Operand is 16-bit address, X-indexed (e.g., LDA $1234,X)
        AbsoluteY,          // Operand is 16-bit address, Y-indexed (e.g., LDA $1234,Y)
        Indirect,           // Operand is 16-bit address pointing to the address (e.g., JMP ($1234))
        IndexedIndirect,    // Operand is 8-bit address, X-indexed, pointing to 16-bit address (e.g., LDA ($12,X))
        IndirectIndexed     // Operand is 8-bit address pointing to 16-bit address, Y-indexed (e.g., LDA ($12),Y)
    }

    /// <summary>
    /// Instruction type categories
    /// </summary>
    public enum InstructionType
    {
        Load,               // Load operations (LDA, LDX, LDY)
        Store,              // Store operations (STA, STX, STY)
        Transfer,           // Register transfers (TAX, TXA, etc.)
        Stack,              // Stack operations (PHA, PLA, PHP, PLP, TSX, TXS)
        Arithmetic,         // Arithmetic operations (ADC, SBC)
        Increment,          // Increment operations (INC, INX, INY)
        Decrement,          // Decrement operations (DEC, DEX, DEY)
        Shift,              // Shift operations (ASL, LSR, ROL, ROR)
        Logic,              // Logic operations (AND, EOR, ORA)
        Compare,            // Compare operations (CMP, CPX, CPY)
        Branch,             // Branch operations (BCC, BCS, BEQ, BMI, BNE, BPL, BVC, BVS)
        Jump,               // Jump operations (JMP, JSR)
        Return,             // Return operations (RTS, RTI)
        Set,                // Set flag operations (SEC, SED, SEI)
        Clear,              // Clear flag operations (CLC, CLD, CLI, CLV)
        Interrupt,          // Interrupt operations (BRK)
        Other               // Other operations (NOP, ???)
    }

    /// <summary>
    /// Information about a 6502 CPU instruction
    /// </summary>
    public class InstructionInfo
    {
        /// <summary>
        /// The opcode value (0x00-0xFF)
        /// </summary>
        public byte Opcode { get; set; }

        /// <summary>
        /// The mnemonic for this instruction (e.g., "LDA", "STA")
        /// </summary>
        public string Mnemonic { get; set; }

        /// <summary>
        /// The addressing mode for this instruction
        /// </summary>
        public AddressingMode AddressingMode { get; set; }

        /// <summary>
        /// The size of this instruction in bytes (including operands)
        /// </summary>
        public byte Size { get; set; }

        /// <summary>
        /// The number of cycles this instruction takes to execute
        /// </summary>
        public byte Cycles { get; set; }

        /// <summary>
        /// Whether this instruction can take an additional cycle on page boundary crossing
        /// </summary>
        public bool ExtraCycleOnPageCross { get; set; }

        /// <summary>
        /// The instruction type category
        /// </summary>
        public InstructionType Type { get; set; }

        /// <summary>
        /// Whether this instruction is a valid 6502 instruction
        /// </summary>
        public bool IsValid { get; set; } = true;

        public InstructionInfo(byte opcode, string mnemonic, AddressingMode addressingMode,
            byte size, byte cycles, bool extraCycleOnPageCross, InstructionType type)
        {
            Opcode = opcode;
            Mnemonic = mnemonic;
            AddressingMode = addressingMode;
            Size = size;
            Cycles = cycles;
            ExtraCycleOnPageCross = extraCycleOnPageCross;
            Type = type;
        }

        /// <summary>
        /// Returns a string representation of the operand format for this addressing mode
        /// </summary>
        public string GetOperandFormat()
        {
            return AddressingMode switch
            {
                AddressingMode.Implied => "",
                AddressingMode.Accumulator => "A",
                AddressingMode.Immediate => "#$%02X",
                AddressingMode.ZeroPage => "$%02X",
                AddressingMode.ZeroPageX => "$%02X,X",
                AddressingMode.ZeroPageY => "$%02X,Y",
                AddressingMode.Relative => "$%02X",  // Will be processed specially for branches
                AddressingMode.Absolute => "$%04X",
                AddressingMode.AbsoluteX => "$%04X,X",
                AddressingMode.AbsoluteY => "$%04X,Y",
                AddressingMode.Indirect => "($%04X)",
                AddressingMode.IndexedIndirect => "($%02X,X)",
                AddressingMode.IndirectIndexed => "($%02X),Y",
                _ => "???"
            };
        }
    }

    /// <summary>
    /// Defines the 6502 CPU instruction set
    /// </summary>
    public static class InstructionSet
    {
        // The full 6502 instruction set
        private static readonly Dictionary<byte, InstructionInfo> instructions = new Dictionary<byte, InstructionInfo>();

        // Maps mnemonic + addressing mode to an instruction info object
        private static readonly Dictionary<(string, AddressingMode), InstructionInfo> mnemonicMap =
            new Dictionary<(string, AddressingMode), InstructionInfo>();

        /// <summary>
        /// Static constructor to initialize the instruction set
        /// </summary>
        static InstructionSet()
        {
            InitializeInstructionSet();
        }

        /// <summary>
        /// Gets an instruction by its opcode
        /// </summary>
        /// <param name="opcode">The opcode value</param>
        /// <returns>Information about the instruction</returns>
        public static InstructionInfo GetInstruction(byte opcode)
        {
            if (instructions.TryGetValue(opcode, out var instruction))
            {
                return instruction;
            }

            // Return unknown instruction
            return new InstructionInfo(
                opcode, "???", AddressingMode.Implied, 1, 2, false, InstructionType.Other)
            {
                IsValid = false
            };
        }

        /// <summary>
        /// Gets an instruction by its mnemonic and addressing mode
        /// </summary>
        /// <param name="mnemonic">The instruction mnemonic</param>
        /// <param name="addressingMode">The addressing mode</param>
        /// <returns>Information about the instruction, or null if not found</returns>
        public static InstructionInfo? GetInstruction(string mnemonic, AddressingMode addressingMode)
        {
            var key = (mnemonic.ToUpper(), addressingMode);
            if (mnemonicMap.TryGetValue(key, out var instruction))
            {
                return instruction;
            }
            return null;
        }

        /// <summary>
        /// Initializes the 6502 instruction set
        /// </summary>
        private static void InitializeInstructionSet()
        {
            // Load/Store Operations
            Add(0xA9, "LDA", AddressingMode.Immediate, 2, 2, false, InstructionType.Load);
            Add(0xA5, "LDA", AddressingMode.ZeroPage, 2, 3, false, InstructionType.Load);
            Add(0xB5, "LDA", AddressingMode.ZeroPageX, 2, 4, false, InstructionType.Load);
            Add(0xAD, "LDA", AddressingMode.Absolute, 3, 4, false, InstructionType.Load);
            Add(0xBD, "LDA", AddressingMode.AbsoluteX, 3, 4, true, InstructionType.Load);
            Add(0xB9, "LDA", AddressingMode.AbsoluteY, 3, 4, true, InstructionType.Load);
            Add(0xA1, "LDA", AddressingMode.IndexedIndirect, 2, 6, false, InstructionType.Load);
            Add(0xB1, "LDA", AddressingMode.IndirectIndexed, 2, 5, true, InstructionType.Load);

            Add(0xA2, "LDX", AddressingMode.Immediate, 2, 2, false, InstructionType.Load);
            Add(0xA6, "LDX", AddressingMode.ZeroPage, 2, 3, false, InstructionType.Load);
            Add(0xB6, "LDX", AddressingMode.ZeroPageY, 2, 4, false, InstructionType.Load);
            Add(0xAE, "LDX", AddressingMode.Absolute, 3, 4, false, InstructionType.Load);
            Add(0xBE, "LDX", AddressingMode.AbsoluteY, 3, 4, true, InstructionType.Load);

            Add(0xA0, "LDY", AddressingMode.Immediate, 2, 2, false, InstructionType.Load);
            Add(0xA4, "LDY", AddressingMode.ZeroPage, 2, 3, false, InstructionType.Load);
            Add(0xB4, "LDY", AddressingMode.ZeroPageX, 2, 4, false, InstructionType.Load);
            Add(0xAC, "LDY", AddressingMode.Absolute, 3, 4, false, InstructionType.Load);
            Add(0xBC, "LDY", AddressingMode.AbsoluteX, 3, 4, true, InstructionType.Load);

            Add(0x85, "STA", AddressingMode.ZeroPage, 2, 3, false, InstructionType.Store);
            Add(0x95, "STA", AddressingMode.ZeroPageX, 2, 4, false, InstructionType.Store);
            Add(0x8D, "STA", AddressingMode.Absolute, 3, 4, false, InstructionType.Store);
            Add(0x9D, "STA", AddressingMode.AbsoluteX, 3, 5, false, InstructionType.Store);
            Add(0x99, "STA", AddressingMode.AbsoluteY, 3, 5, false, InstructionType.Store);
            Add(0x81, "STA", AddressingMode.IndexedIndirect, 2, 6, false, InstructionType.Store);
            Add(0x91, "STA", AddressingMode.IndirectIndexed, 2, 6, false, InstructionType.Store);

            Add(0x86, "STX", AddressingMode.ZeroPage, 2, 3, false, InstructionType.Store);
            Add(0x96, "STX", AddressingMode.ZeroPageY, 2, 4, false, InstructionType.Store);
            Add(0x8E, "STX", AddressingMode.Absolute, 3, 4, false, InstructionType.Store);

            Add(0x84, "STY", AddressingMode.ZeroPage, 2, 3, false, InstructionType.Store);
            Add(0x94, "STY", AddressingMode.ZeroPageX, 2, 4, false, InstructionType.Store);
            Add(0x8C, "STY", AddressingMode.Absolute, 3, 4, false, InstructionType.Store);

            // Register Transfers
            Add(0xAA, "TAX", AddressingMode.Implied, 1, 2, false, InstructionType.Transfer);
            Add(0x8A, "TXA", AddressingMode.Implied, 1, 2, false, InstructionType.Transfer);
            Add(0xA8, "TAY", AddressingMode.Implied, 1, 2, false, InstructionType.Transfer);
            Add(0x98, "TYA", AddressingMode.Implied, 1, 2, false, InstructionType.Transfer);
            Add(0xBA, "TSX", AddressingMode.Implied, 1, 2, false, InstructionType.Transfer);
            Add(0x9A, "TXS", AddressingMode.Implied, 1, 2, false, InstructionType.Transfer);

            // Stack Operations
            Add(0x48, "PHA", AddressingMode.Implied, 1, 3, false, InstructionType.Stack);
            Add(0x68, "PLA", AddressingMode.Implied, 1, 4, false, InstructionType.Stack);
            Add(0x08, "PHP", AddressingMode.Implied, 1, 3, false, InstructionType.Stack);
            Add(0x28, "PLP", AddressingMode.Implied, 1, 4, false, InstructionType.Stack);

            // Logical Operations
            Add(0x29, "AND", AddressingMode.Immediate, 2, 2, false, InstructionType.Logic);
            Add(0x25, "AND", AddressingMode.ZeroPage, 2, 3, false, InstructionType.Logic);
            Add(0x35, "AND", AddressingMode.ZeroPageX, 2, 4, false, InstructionType.Logic);
            Add(0x2D, "AND", AddressingMode.Absolute, 3, 4, false, InstructionType.Logic);
            Add(0x3D, "AND", AddressingMode.AbsoluteX, 3, 4, true, InstructionType.Logic);
            Add(0x39, "AND", AddressingMode.AbsoluteY, 3, 4, true, InstructionType.Logic);
            Add(0x21, "AND", AddressingMode.IndexedIndirect, 2, 6, false, InstructionType.Logic);
            Add(0x31, "AND", AddressingMode.IndirectIndexed, 2, 5, true, InstructionType.Logic);

            Add(0x49, "EOR", AddressingMode.Immediate, 2, 2, false, InstructionType.Logic);
            Add(0x45, "EOR", AddressingMode.ZeroPage, 2, 3, false, InstructionType.Logic);
            Add(0x55, "EOR", AddressingMode.ZeroPageX, 2, 4, false, InstructionType.Logic);
            Add(0x4D, "EOR", AddressingMode.Absolute, 3, 4, false, InstructionType.Logic);
            Add(0x5D, "EOR", AddressingMode.AbsoluteX, 3, 4, true, InstructionType.Logic);
            Add(0x59, "EOR", AddressingMode.AbsoluteY, 3, 4, true, InstructionType.Logic);
            Add(0x41, "EOR", AddressingMode.IndexedIndirect, 2, 6, false, InstructionType.Logic);
            Add(0x51, "EOR", AddressingMode.IndirectIndexed, 2, 5, true, InstructionType.Logic);

            Add(0x09, "ORA", AddressingMode.Immediate, 2, 2, false, InstructionType.Logic);
            Add(0x05, "ORA", AddressingMode.ZeroPage, 2, 3, false, InstructionType.Logic);
            Add(0x15, "ORA", AddressingMode.ZeroPageX, 2, 4, false, InstructionType.Logic);
            Add(0x0D, "ORA", AddressingMode.Absolute, 3, 4, false, InstructionType.Logic);
            Add(0x1D, "ORA", AddressingMode.AbsoluteX, 3, 4, true, InstructionType.Logic);
            Add(0x19, "ORA", AddressingMode.AbsoluteY, 3, 4, true, InstructionType.Logic);
            Add(0x01, "ORA", AddressingMode.IndexedIndirect, 2, 6, false, InstructionType.Logic);
            Add(0x11, "ORA", AddressingMode.IndirectIndexed, 2, 5, true, InstructionType.Logic);

            Add(0x24, "BIT", AddressingMode.ZeroPage, 2, 3, false, InstructionType.Logic);
            Add(0x2C, "BIT", AddressingMode.Absolute, 3, 4, false, InstructionType.Logic);

            // Arithmetic Operations
            Add(0x69, "ADC", AddressingMode.Immediate, 2, 2, false, InstructionType.Arithmetic);
            Add(0x65, "ADC", AddressingMode.ZeroPage, 2, 3, false, InstructionType.Arithmetic);
            Add(0x75, "ADC", AddressingMode.ZeroPageX, 2, 4, false, InstructionType.Arithmetic);
            Add(0x6D, "ADC", AddressingMode.Absolute, 3, 4, false, InstructionType.Arithmetic);
            Add(0x7D, "ADC", AddressingMode.AbsoluteX, 3, 4, true, InstructionType.Arithmetic);
            Add(0x79, "ADC", AddressingMode.AbsoluteY, 3, 4, true, InstructionType.Arithmetic);
            Add(0x61, "ADC", AddressingMode.IndexedIndirect, 2, 6, false, InstructionType.Arithmetic);
            Add(0x71, "ADC", AddressingMode.IndirectIndexed, 2, 5, true, InstructionType.Arithmetic);

            Add(0xE9, "SBC", AddressingMode.Immediate, 2, 2, false, InstructionType.Arithmetic);
            Add(0xE5, "SBC", AddressingMode.ZeroPage, 2, 3, false, InstructionType.Arithmetic);
            Add(0xF5, "SBC", AddressingMode.ZeroPageX, 2, 4, false, InstructionType.Arithmetic);
            Add(0xED, "SBC", AddressingMode.Absolute, 3, 4, false, InstructionType.Arithmetic);
            Add(0xFD, "SBC", AddressingMode.AbsoluteX, 3, 4, true, InstructionType.Arithmetic);
            Add(0xF9, "SBC", AddressingMode.AbsoluteY, 3, 4, true, InstructionType.Arithmetic);
            Add(0xE1, "SBC", AddressingMode.IndexedIndirect, 2, 6, false, InstructionType.Arithmetic);
            Add(0xF1, "SBC", AddressingMode.IndirectIndexed, 2, 5, true, InstructionType.Arithmetic);

            Add(0xC9, "CMP", AddressingMode.Immediate, 2, 2, false, InstructionType.Compare);
            Add(0xC5, "CMP", AddressingMode.ZeroPage, 2, 3, false, InstructionType.Compare);
            Add(0xD5, "CMP", AddressingMode.ZeroPageX, 2, 4, false, InstructionType.Compare);
            Add(0xCD, "CMP", AddressingMode.Absolute, 3, 4, false, InstructionType.Compare);
            Add(0xDD, "CMP", AddressingMode.AbsoluteX, 3, 4, true, InstructionType.Compare);
            Add(0xD9, "CMP", AddressingMode.AbsoluteY, 3, 4, true, InstructionType.Compare);
            Add(0xC1, "CMP", AddressingMode.IndexedIndirect, 2, 6, false, InstructionType.Compare);
            Add(0xD1, "CMP", AddressingMode.IndirectIndexed, 2, 5, true, InstructionType.Compare);

            Add(0xE0, "CPX", AddressingMode.Immediate, 2, 2, false, InstructionType.Compare);
            Add(0xE4, "CPX", AddressingMode.ZeroPage, 2, 3, false, InstructionType.Compare);
            Add(0xEC, "CPX", AddressingMode.Absolute, 3, 4, false, InstructionType.Compare);

            Add(0xC0, "CPY", AddressingMode.Immediate, 2, 2, false, InstructionType.Compare);
            Add(0xC4, "CPY", AddressingMode.ZeroPage, 2, 3, false, InstructionType.Compare);
            Add(0xCC, "CPY", AddressingMode.Absolute, 3, 4, false, InstructionType.Compare);

            // Increments & Decrements
            Add(0xE6, "INC", AddressingMode.ZeroPage, 2, 5, false, InstructionType.Increment);
            Add(0xF6, "INC", AddressingMode.ZeroPageX, 2, 6, false, InstructionType.Increment);
            Add(0xEE, "INC", AddressingMode.Absolute, 3, 6, false, InstructionType.Increment);
            Add(0xFE, "INC", AddressingMode.AbsoluteX, 3, 7, false, InstructionType.Increment);

            Add(0xE8, "INX", AddressingMode.Implied, 1, 2, false, InstructionType.Increment);
            Add(0xC8, "INY", AddressingMode.Implied, 1, 2, false, InstructionType.Increment);

            Add(0xC6, "DEC", AddressingMode.ZeroPage, 2, 5, false, InstructionType.Decrement);
            Add(0xD6, "DEC", AddressingMode.ZeroPageX, 2, 6, false, InstructionType.Decrement);
            Add(0xCE, "DEC", AddressingMode.Absolute, 3, 6, false, InstructionType.Decrement);
            Add(0xDE, "DEC", AddressingMode.AbsoluteX, 3, 7, false, InstructionType.Decrement);

            Add(0xCA, "DEX", AddressingMode.Implied, 1, 2, false, InstructionType.Decrement);
            Add(0x88, "DEY", AddressingMode.Implied, 1, 2, false, InstructionType.Decrement);

            // Shifts
            Add(0x0A, "ASL", AddressingMode.Accumulator, 1, 2, false, InstructionType.Shift);
            Add(0x06, "ASL", AddressingMode.ZeroPage, 2, 5, false, InstructionType.Shift);
            Add(0x16, "ASL", AddressingMode.ZeroPageX, 2, 6, false, InstructionType.Shift);
            Add(0x0E, "ASL", AddressingMode.Absolute, 3, 6, false, InstructionType.Shift);
            Add(0x1E, "ASL", AddressingMode.AbsoluteX, 3, 7, false, InstructionType.Shift);

            Add(0x4A, "LSR", AddressingMode.Accumulator, 1, 2, false, InstructionType.Shift);
            Add(0x46, "LSR", AddressingMode.ZeroPage, 2, 5, false, InstructionType.Shift);
            Add(0x56, "LSR", AddressingMode.ZeroPageX, 2, 6, false, InstructionType.Shift);
            Add(0x4E, "LSR", AddressingMode.Absolute, 3, 6, false, InstructionType.Shift);
            Add(0x5E, "LSR", AddressingMode.AbsoluteX, 3, 7, false, InstructionType.Shift);

            Add(0x2A, "ROL", AddressingMode.Accumulator, 1, 2, false, InstructionType.Shift);
            Add(0x26, "ROL", AddressingMode.ZeroPage, 2, 5, false, InstructionType.Shift);
            Add(0x36, "ROL", AddressingMode.ZeroPageX, 2, 6, false, InstructionType.Shift);
            Add(0x2E, "ROL", AddressingMode.Absolute, 3, 6, false, InstructionType.Shift);
            Add(0x3E, "ROL", AddressingMode.AbsoluteX, 3, 7, false, InstructionType.Shift);

            Add(0x6A, "ROR", AddressingMode.Accumulator, 1, 2, false, InstructionType.Shift);
            Add(0x66, "ROR", AddressingMode.ZeroPage, 2, 5, false, InstructionType.Shift);
            Add(0x76, "ROR", AddressingMode.ZeroPageX, 2, 6, false, InstructionType.Shift);
            Add(0x6E, "ROR", AddressingMode.Absolute, 3, 6, false, InstructionType.Shift);
            Add(0x7E, "ROR", AddressingMode.AbsoluteX, 3, 7, false, InstructionType.Shift);

            // Jumps & Calls
            Add(0x4C, "JMP", AddressingMode.Absolute, 3, 3, false, InstructionType.Jump);
            Add(0x6C, "JMP", AddressingMode.Indirect, 3, 5, false, InstructionType.Jump);
            Add(0x20, "JSR", AddressingMode.Absolute, 3, 6, false, InstructionType.Jump);
            Add(0x60, "RTS", AddressingMode.Implied, 1, 6, false, InstructionType.Return);
            Add(0x40, "RTI", AddressingMode.Implied, 1, 6, false, InstructionType.Return);

            // Branches
            Add(0x90, "BCC", AddressingMode.Relative, 2, 2, true, InstructionType.Branch);
            Add(0xB0, "BCS", AddressingMode.Relative, 2, 2, true, InstructionType.Branch);
            Add(0xF0, "BEQ", AddressingMode.Relative, 2, 2, true, InstructionType.Branch);
            Add(0x30, "BMI", AddressingMode.Relative, 2, 2, true, InstructionType.Branch);
            Add(0xD0, "BNE", AddressingMode.Relative, 2, 2, true, InstructionType.Branch);
            Add(0x10, "BPL", AddressingMode.Relative, 2, 2, true, InstructionType.Branch);
            Add(0x50, "BVC", AddressingMode.Relative, 2, 2, true, InstructionType.Branch);
            Add(0x70, "BVS", AddressingMode.Relative, 2, 2, true, InstructionType.Branch);

            // Status Flag Changes
            Add(0x18, "CLC", AddressingMode.Implied, 1, 2, false, InstructionType.Clear);
            Add(0xD8, "CLD", AddressingMode.Implied, 1, 2, false, InstructionType.Clear);
            Add(0x58, "CLI", AddressingMode.Implied, 1, 2, false, InstructionType.Clear);
            Add(0xB8, "CLV", AddressingMode.Implied, 1, 2, false, InstructionType.Clear);
            Add(0x38, "SEC", AddressingMode.Implied, 1, 2, false, InstructionType.Set);
            Add(0xF8, "SED", AddressingMode.Implied, 1, 2, false, InstructionType.Set);
            Add(0x78, "SEI", AddressingMode.Implied, 1, 2, false, InstructionType.Set);

            // System Functions
            Add(0x00, "BRK", AddressingMode.Implied, 1, 7, false, InstructionType.Interrupt);
            Add(0xEA, "NOP", AddressingMode.Implied, 1, 2, false, InstructionType.Other);

            // Unofficial/Illegal opcodes are added here if needed by further forks, however this should cover all of the NES 6502 instructions
        }

        /// <summary>
        /// Adds an instruction to the instruction set
        /// </summary>
        private static void Add(byte opcode, string mnemonic, AddressingMode addressingMode,
            byte size, byte cycles, bool extraCycleOnPageCross, InstructionType type)
        {
            var instruction = new InstructionInfo(
                opcode, mnemonic, addressingMode, size, cycles, extraCycleOnPageCross, type);

            instructions[opcode] = instruction;
            mnemonicMap[(mnemonic, addressingMode)] = instruction;
        }
    }
}