# NES Decompiler

A decompilation and disassembly tool for Nintendo Entertainment System (NES) ROMs, providing both assembly output and C-like code generation.

## Features

- **Disassembly**: Convert NES ROM binary data to 6502 assembly code
- **Decompilation**: Generate C-like code from NES ROMs
- **Control Flow Analysis**: Analyze and reconstruct program flow
- **Symbol Recognition**: Identify and label functions and variables
- **Code Navigation**: View and navigate through disassembled/decompiled code
- **Project Management**: Save and load projects with multiple ROMs
- **Workspace System**: Save your progress and continue later
- **Visual Studio Export**: Export decompiled code as a VS2022 solution
- **GUI Interface**: User-friendly interface with syntax highlighting
- **Command-Line Interface**: Automate decompilation tasks

## Requirements

- .NET 8.0 or higher
- Windows with WPF support (for GUI version)

## Installation

1. Download the latest release from the [Releases](https://github.com/apfelteesaft/NESDecompiler/releases) page
2. Extract the files to a directory of your choice
3. Run `NESDecompiler.GUI.exe` for the graphical interface or `NESDecompiler.CLI.exe` for command-line usage

## Usage

### GUI

1. Launch `NESDecompiler.GUI.exe`
2. Open a NES ROM file using File → Open ROM...
3. The application will automatically disassemble and decompile the ROM
4. Navigate through different views (Disassembly, C Code, Variables, Functions)
5. Save your project or export to Visual Studio

### Command Line

```
NESDecompiler.CLI.exe -i input.nes -o output_directory [-d] [-c] [-v]
```

Options:
- `-i, --input`: Input NES ROM file path (required)
- `-o, --output`: Output directory for generated files
- `-d, --disassemble`: Generate disassembly output (default: false)
- `-c, --decompile`: Generate C code output (default: true)
- `-v, --verbose`: Set output to verbose messages

Example:
```
NESDecompiler.CLI.exe -i SuperMario.nes -o mario_src -d -c -v
```

## Project Structure

- **NESDecompiler.Core**: Core decompilation and analysis engine
  - CPU: 6502 CPU instruction set and emulation components
  - Disassembly: Conversion of binary to assembly
  - Decompilation: Conversion of assembly to C code
  - ROM: NES ROM format parsing and handling
  
- **NESDecompiler.CLI**: Command-line interface application

- **NESDecompiler.GUI**: Graphical user interface
  - WPF-based interface with dockable panels
  - Syntax highlighting for both assembly and C code
  - Project management features

## Technical Notes

- The decompiled C code is a best-effort representation and may not be 100% accurate
- The C code is intended as a pseudocode representation to assist in understanding the original assembly
- Some ROMs may contain custom hardware or timing-critical code that doesn't translate well to C
- The generated Visual Studio projects include a NES hardware abstraction layer to allow testing the decompiled code

## Limitations

- Some features are not fully implemented or may contain bugs
- The C code generation is approximate and might require manual adjustments
- Some complex control flow structures might not be correctly identified
- Custom mappers and ROM formats might not be fully supported

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgements

- Thanks to the NES development community for documentation on the 6502 CPU and NES hardware
- This project was inspired by existing tools like [FCEUX](http://www.fceux.com/web/home.html) and [Ghidra](https://ghidra-sre.org/)

## Disclaimer

This tool is intended for educational purposes and personal use only. Decompiling commercial ROMs may violate copyright laws in your country. Use at your own risk and only with ROMs you own or have permission to analyze.