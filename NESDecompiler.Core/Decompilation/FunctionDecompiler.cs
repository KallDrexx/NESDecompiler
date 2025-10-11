using NESDecompiler.Core.CPU;
using NESDecompiler.Core.Disassembly;

namespace NESDecompiler.Core.Decompilation;

public static class FunctionDecompiler
{
    /// <summary>
    /// Traces and decompiles a single function
    /// </summary>
    /// <param name="functionAddress">The CPU address of the entry point of the function to decompile</param>
    /// <param name="codeRegions">All available regions of bytes that could contain instructions for the function</param>
    public static DecompiledFunction Decompile(ushort functionAddress, IReadOnlyList<CodeRegion> codeRegions)
    {
        var instructions = new List<DisassembledInstruction>();
        var jumpAddresses = new HashSet<ushort>();
        var seenInstructions = new HashSet<ushort>();
        var addressQueue = new Queue<ushort>([functionAddress]);

        while (addressQueue.TryDequeue(out var nextAddress))
        {
            if (!seenInstructions.Add(nextAddress))
            {
                continue;
            }

            var instruction = GetNextInstruction(nextAddress, codeRegions);
            instructions.Add(instruction);

            if (IsEndOfFunction(instruction))
            {
                continue;
            }

            if (instruction.TargetAddress != null)
            {
                jumpAddresses.Add(instruction.TargetAddress.Value);
                addressQueue.Enqueue(instruction.CPUAddress);
            }

            if (!instruction.IsJump)
            {
                addressQueue.Enqueue((ushort)(nextAddress + instruction.Info.Size));
            }
        }

        return new DecompiledFunction(functionAddress, instructions, jumpAddresses);
    }

    private static DisassembledInstruction GetNextInstruction(ushort address, IReadOnlyList<CodeRegion> regions)
    {
        var relevantRegion = regions
            .Where(x => x.BaseAddress < address)
            .Where(x => x.BaseAddress + x.Bytes.Length > address)
            .FirstOrDefault();

        if (relevantRegion == null)
        {
            var message = $"No code region contained the address 0x{address:X4}";
            throw new InvalidOperationException(message);
        }

        var offset = address - relevantRegion.BaseAddress;
        var bytes = relevantRegion.Bytes.Span[offset..];
        var info = InstructionSet.GetInstruction(bytes[0]);
        if (!info.IsValid)
        {
            var message = $"Attempted to get instruction at address 0x{address:X4}, but byte 0x{bytes[0]:X4} " +
                          $"is not a valid/known opcode";

            throw new InvalidOperationException(message);
        }

        if (bytes.Length < info.Size)
        {
            var message = $"Opcode {info.Mnemonic} at address 0x{address:X4} requires {info.Size} bytes, but only " +
                          $"{bytes.Length} are available";

            throw new InvalidOperationException(message);
        }

        var instruction = new DisassembledInstruction
        {
            Address = (ushort)offset,
            CPUAddress = address,
            Info = info,
            Bytes = bytes[..info.Size].ToArray(),
        };

        Disassembler.CalculateTargetAddress(instruction);

        return instruction;
    }

    private static bool IsEndOfFunction(DisassembledInstruction instruction)
    {
        // RTI and RTS are obviously the end of a function. We consider BRK and JSR
        // to be the end of a function as well because an RTI or RTS will do a function
        // call into the next instruction. This is required because RTI/RTS could be
        // returning based on a modified stack, and therefore we are not guaranteed to
        // be returning to the expected spot.
        if (instruction.Info.Mnemonic is "JSR" or "BRK" or "RTI" or "RTS")
        {
            return true;
        }

        // Since we don't know where we are jumping at compile time, this will be treated
        // as a function call, thus we consider it the end of the function.
        if (instruction.Info.AddressingMode == AddressingMode.Indirect)
        {
            return true;
        }

        return false;
    }
}