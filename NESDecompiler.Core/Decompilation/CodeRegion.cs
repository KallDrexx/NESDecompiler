namespace NESDecompiler.Core.Decompilation;

/// <summary>
/// A set of code that may contain executable code
/// </summary>
/// <param name="BaseAddress">Where the first byte of the region can be found from the CPU's memory map</param>
/// <param name="Bytes">The set of data to pull code out of</param>
public record CodeRegion(ushort BaseAddress, ReadOnlyMemory<byte> Bytes);