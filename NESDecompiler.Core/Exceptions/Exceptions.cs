using System;

namespace NESDecompiler.Core.Exceptions
{
    /// <summary>
    /// Base exception class for ROM-related errors
    /// </summary>
    public class ROMException : Exception
    {
        public ROMException(string message) : base(message)
        {
        }

        public ROMException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when a ROM has an invalid format
    /// </summary>
    public class InvalidROMFormatException : ROMException
    {
        public InvalidROMFormatException(string message) : base(message)
        {
        }

        public InvalidROMFormatException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown during the disassembly process
    /// </summary>
    public class DisassemblyException : Exception
    {
        public DisassemblyException(string message) : base(message)
        {
        }

        public DisassemblyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown during the decompilation process
    /// </summary>
    public class DecompilationException : Exception
    {
        public DecompilationException(string message) : base(message)
        {
        }

        public DecompilationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}