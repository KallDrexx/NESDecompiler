using NESDecompiler.Core.Disassembly;

namespace NESDecompiler.Core.Decompilation;

/// <summary>
/// Represents an independently decompiled function
/// </summary>
public class DecompiledFunction
{
    /// <summary>
    /// The CPU address where the address' first instruction is located
    /// </summary>
    public ushort Address { get; }

    /// <summary>
    /// The instructions that make up this function in the correct order in which they should be
    /// executed.
    /// </summary>
    public IReadOnlyList<DisassembledInstruction> OrderedInstructions { get; }

    /// <summary>
    /// Location and the labels of all jump and branch targets within this function
    /// </summary>
    public IReadOnlyDictionary<ushort, string> JumpTargets { get; }

    public DecompiledFunction(
        ushort address,
        IReadOnlyList<DisassembledInstruction> instructions,
        IReadOnlySet<ushort> jumpTargets)
    {
        Address = address;
        JumpTargets = instructions
            .Where(x => jumpTargets.Contains(x.CPUAddress))
            .Where(x => x.Label != null)
            .Where(x => x.SubAddressOrder == 0) // only real instructions should be jumped to
            .ToDictionary(x => x.CPUAddress, x => x.Label!);

        // We need to order the instructions so that the starting instruction is the first one encountered.
        // We can't just rely on the CPU address, because a function may jump to a code point earlier than
        // the first instruction.
        var entryPointInstructions = instructions.Where(x => x.CPUAddress == address)
            .Where(x => x.SubAddressOrder >= 0);

        var initialInstructions = instructions
            .Where(x => x.CPUAddress > address)
            .OrderBy(x => x.CPUAddress)
            .ThenBy(x => x.SubAddressOrder);

        var trailingInstructions = instructions
            .Where(x => x.CPUAddress < address)
            .OrderBy(x => x.CPUAddress)
            .ThenBy(x => x.SubAddressOrder); // real instructions before virtual ones

        // If there was a loopback jump point at the function address, put that here. This is required
        // because if an emulator is executing a virtual loopback instruction and an IRQ occurs, this
        // will cause the virtual instruction to be saved to the stack, and that can cause the entry
        // point to be wrong.
        var loopbackInstructions = instructions.Where(x => x.CPUAddress == address)
            .Where(x => x.SubAddressOrder < 0);

        OrderedInstructions = entryPointInstructions
            .Concat(initialInstructions)
            .Concat(trailingInstructions)
            .Concat(loopbackInstructions)
            .ToArray();
    }
}