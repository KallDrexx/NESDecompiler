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
    /// Location of all jump and branch targets within this function
    /// </summary>
    public IReadOnlySet<ushort> JumpTargets { get; }

    public DecompiledFunction(
        ushort address,
        IReadOnlyList<DisassembledInstruction> instructions,
        IReadOnlySet<ushort> jumpTargets)
    {
        Address = address;
        JumpTargets = jumpTargets;

        // We need to order the instructions so that the starting instruction is the first one encountered.
        // We can't just rely on the CPU address, because a function may jump to a code point earlier than
        // the first instruction.
        var initialInstructions = instructions.Where(x => x.CPUAddress >= address);
        var trailingInstructions = instructions.Where(x => x.CPUAddress < address);

        OrderedInstructions = initialInstructions.Concat(trailingInstructions).ToArray();
    }
}