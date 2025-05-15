using System;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NESDecompiler.GUI.ViewModels
{
    /// <summary>
    /// Helper class for exporting to Visual Studio 2022 solution
    /// </summary>
    public class VisualStudioExporter
    {
        private readonly string solutionName;
        private readonly string rootPath;
        private readonly string cCode;
        private readonly string headerCode;

        /// <summary>
        /// Creates a new Visual Studio exporter
        /// </summary>
        /// <param name="solutionName">The name of the solution</param>
        /// <param name="rootPath">The root directory path</param>
        /// <param name="cCode">The C code to export</param>
        /// <param name="headerCode">The header code to export</param>
        public VisualStudioExporter(string solutionName, string rootPath, string cCode, string headerCode)
        {
            this.solutionName = solutionName;
            this.rootPath = rootPath;
            this.cCode = cCode;
            this.headerCode = headerCode;
        }

        /// <summary>
        /// Exports the decompiled code as a Visual Studio 2022 solution
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        public bool ExportToVisualStudio()
        {
            try
            {
                string solutionDir = Path.Combine(rootPath, solutionName);
                Directory.CreateDirectory(solutionDir);

                string sourceDir = Path.Combine(solutionDir, "src");
                Directory.CreateDirectory(sourceDir);

                File.WriteAllText(Path.Combine(sourceDir, $"{solutionName}.c"), cCode);
                File.WriteAllText(Path.Combine(sourceDir, $"{solutionName}.h"), headerCode);

                CreateNesPlatformHeader(sourceDir);

                CreateMainFile(sourceDir);

                CreateCMakeFile(solutionDir);

                CreateSolutionFile(solutionDir);

                CreateProjectFile(solutionDir);

                CreateFiltersFile(solutionDir);

                CreateReadmeFile(solutionDir);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to export to Visual Studio: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Creates a NES platform header file with platform-specific declarations
        /// </summary>
        private void CreateNesPlatformHeader(string sourceDir)
        {
            string content = @"#ifndef NES_PLATFORM_H
#define NES_PLATFORM_H

#include <stdint.h>
#include <stdbool.h>

// NES PPU registers enum (for use in switch statements)
typedef enum {
    PPUCTRL_REG      = 0x2000,
    PPUMASK_REG      = 0x2001,
    PPUSTATUS_REG    = 0x2002,
    OAMADDR_REG      = 0x2003,
    OAMDATA_REG      = 0x2004,
    PPUSCROLL_REG    = 0x2005,
    PPUADDR_REG      = 0x2006,
    PPUDATA_REG      = 0x2007,
    OAMDMA_REG       = 0x4014
} NESRegisters;

// NES PPU registers (Memory mapped hardware registers)
#define PPUCTRL      0x2000
#define PPUMASK      0x2001
#define PPUSTATUS    0x2002
#define OAMADDR      0x2003
#define OAMDATA      0x2004
#define PPUSCROLL    0x2005
#define PPUADDR      0x2006
#define PPUDATA      0x2007
#define OAMDMA       0x4014

// NES APU registers
#define APU_PULSE1_1 0x4000
#define APU_PULSE1_2 0x4001
#define APU_PULSE1_3 0x4002
#define APU_PULSE1_4 0x4003
#define APU_PULSE2_1 0x4004
#define APU_PULSE2_2 0x4005
#define APU_PULSE2_3 0x4006
#define APU_PULSE2_4 0x4007
#define APU_TRI_1    0x4008
#define APU_TRI_2    0x4009
#define APU_TRI_3    0x400A
#define APU_TRI_4    0x400B
#define APU_NOISE_1  0x400C
#define APU_NOISE_2  0x400D
#define APU_NOISE_3  0x400E
#define APU_NOISE_4  0x400F
#define APU_DMC_1    0x4010
#define APU_DMC_2    0x4011
#define APU_DMC_3    0x4012
#define APU_DMC_4    0x4013
#define APU_STATUS   0x4015
#define APU_FRAMECNT 0x4017

// NES Joypad registers
#define JOY1         0x4016
#define JOY2         0x4017

// CPU Status Flag Masks (same as in the decompiled code)
#define CARRY_FLAG     0x01
#define ZERO_FLAG      0x02
#define INTERRUPT_FLAG 0x04
#define DECIMAL_FLAG   0x08
#define BREAK_FLAG     0x10
#define UNUSED_FLAG    0x20
#define OVERFLOW_FLAG  0x40
#define NEGATIVE_FLAG  0x80

// Memory map constants
#define RAM_START      0x0000
#define RAM_SIZE       0x0800
#define RAM_MIRROR_END 0x1FFF
#define PPU_START      0x2000
#define PPU_SIZE       0x0008
#define PPU_MIRROR_END 0x3FFF
#define APU_IO_START   0x4000
#define APU_IO_END     0x4017
#define APU_IO_SIZE    (APU_IO_END - APU_IO_START + 1)
#define CART_START     0x4020
#define CART_END       0xFFFF

// Platform abstraction layer
typedef struct {
    uint8_t memory[0x10000];    // 64KB of addressable memory
    uint8_t ppu_memory[0x4000]; // 16KB of PPU memory
    
    // CPU registers
    uint8_t a;       // Accumulator
    uint8_t x;       // X register
    uint8_t y;       // Y register
    uint8_t sp;      // Stack pointer
    uint8_t status;  // Status register
    uint16_t pc;     // Program counter
    
    // Input state
    uint8_t joy1_state;
    uint8_t joy2_state;

    // Timing
    uint64_t cycles;
    
    // Debugger state
    bool debug_enabled;
    bool breakpoints[0x10000];  // One per memory address
} NESPlatform;

// Initialize the NES platform
void nes_platform_init(NESPlatform* platform);

// Reset the NES platform
void nes_platform_reset(NESPlatform* platform);

// Read a byte from memory
uint8_t nes_platform_read(NESPlatform* platform, uint16_t address);

// Write a byte to memory
void nes_platform_write(NESPlatform* platform, uint16_t address, uint8_t value);

// Push a byte to the stack
void nes_platform_push(NESPlatform* platform, uint8_t value);

// Pop a byte from the stack
uint8_t nes_platform_pop(NESPlatform* platform);

// Execute a single instruction
void nes_platform_execute(NESPlatform* platform);

// Platform abstraction for running decompiled code
void nes_platform_run_decompiled(NESPlatform* platform);

// Debug functions
void nes_platform_dump_memory(NESPlatform* platform, uint16_t start, uint16_t end);
void nes_platform_set_breakpoint(NESPlatform* platform, uint16_t address, bool enabled);
void nes_platform_dump_cpu_state(NESPlatform* platform);

#endif
";
            File.WriteAllText(Path.Combine(sourceDir, "nes_platform.h"), content);
        }

        /// <summary>
        /// Creates a main.c file that includes the decompiled code
        /// </summary>
        private void CreateMainFile(string sourceDir)
        {
            string content = $@"#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include ""nes_platform.h""
#include ""{solutionName}.h""

// Implementation of platform abstraction layer
NESPlatform nes;

// Debug flag
bool debug_enabled = false;

// Read a byte from NES memory with proper memory mapping
uint8_t nes_platform_read(NESPlatform* platform, uint16_t address) {{
    // Apply memory mirroring for RAM
    if (address < 0x2000) {{
        // 2KB RAM with 3 mirrors (0x0000-0x07FF repeated 4 times)
        return platform->memory[address & 0x07FF];
    }}
    // PPU registers with mirrors
    else if (address < 0x4000) {{
        // 8 bytes mirrored throughout 0x2000-0x3FFF
        return platform->memory[0x2000 + (address & 0x7)];
    }}
    // Direct reads for everything else
    else {{
        return platform->memory[address];
    }}
}}

// Write a byte to NES memory with proper memory mapping
void nes_platform_write(NESPlatform* platform, uint16_t address, uint8_t value) {{
    if (platform->debug_enabled) {{
        printf(""Write: 0x%04X = 0x%02X\n"", address, value);
    }}

    // Apply memory mirroring for RAM
    if (address < 0x2000) {{
        // 2KB RAM with 3 mirrors (0x0000-0x07FF repeated 4 times)
        platform->memory[address & 0x07FF] = value;
    }}
    // PPU registers with mirrors
    else if (address < 0x4000) {{
        // 8 bytes mirrored throughout 0x2000-0x3FFF
        uint16_t register_addr = 0x2000 + (address & 0x7);
        platform->memory[register_addr] = value;
        
        // Special handling for certain PPU registers
        switch (register_addr) {{
            case PPUCTRL_REG:
                // Handle PPUCTRL writes
                break;
            case PPUMASK_REG:
                // Handle PPUMASK writes
                break;
            case PPUSTATUS_REG:
                // Handle PPUSTATUS writes
                break;
            case OAMADDR_REG:
                // Handle OAMADDR writes
                break;
            case OAMDATA_REG:
                // Handle OAMDATA writes
                break;
            case PPUSCROLL_REG:
                // Handle PPUSCROLL writes
                break;
            case PPUADDR_REG:
                // Handle PPUADDR writes
                break;
            case PPUDATA_REG:
                // Handle PPUDATA writes
                break;
            default:
                // Other registers (shouldn't happen with mask)
                break;
        }}
    }}
    // APU and IO registers
    else if (address >= 0x4000 && address <= 0x4017) {{
        platform->memory[address] = value;
        
        // Special handling for certain registers
        if (address == OAMDMA_REG) {{
            // Handle OAMDMA (would copy 256 bytes from CPU memory to OAM)
            uint16_t oam_src = value << 8; // High byte of address
            for (int i = 0; i < 256; i++) {{
                platform->memory[0x2004] = nes_platform_read(platform, oam_src + i);
            }}
        }}
        else if (address == JOY1 || address == JOY2) {{
            // Handle controller writes
        }}
    }}
    // Cartridge space
    else {{
        platform->memory[address] = value;
    }}
}}

// Push a byte to the stack
void nes_platform_push(NESPlatform* platform, uint8_t value) {{
    platform->memory[0x0100 + platform->sp] = value;
    platform->sp--;
}}

// Pop a byte from the stack
uint8_t nes_platform_pop(NESPlatform* platform) {{
    platform->sp++;
    return platform->memory[0x0100 + platform->sp];
}}

void nes_platform_init(NESPlatform* platform) {{
    memset(platform, 0, sizeof(NESPlatform));
    
    // Initialize CPU registers
    platform->a = 0;
    platform->x = 0;
    platform->y = 0;
    platform->sp = 0xFD;  // Initial stack pointer
    platform->status = 0x34;  // Initial status (I flag and U flag set)
    platform->pc = 0x8000;  // Start of PRG ROM
    
    // Initialize memory
    memset(platform->memory, 0, sizeof(platform->memory));
    memset(platform->ppu_memory, 0, sizeof(platform->ppu_memory));
    
    // Enable debugging if requested
    platform->debug_enabled = debug_enabled;
}}

void nes_platform_reset(NESPlatform* platform) {{
    if (platform->debug_enabled) {{
        printf(""Resetting NES platform...\n"");
    }}
    
    // Read reset vector
    uint16_t reset_vector = platform->memory[0xFFFC] | (platform->memory[0xFFFD] << 8);
    
    if (platform->debug_enabled) {{
        printf(""Reset vector: $%04X\n"", reset_vector);
    }}
    
    // Set program counter to reset vector
    platform->pc = reset_vector;
    
    // Set stack pointer
    platform->sp = 0xFD;
    
    // Set status register (I flag and U flag set)
    platform->status = 0x34;
}}

// Debug: Dump memory region
void nes_platform_dump_memory(NESPlatform* platform, uint16_t start, uint16_t end) {{
    printf(""Memory dump from $%04X to $%04X:\n"", start, end);
    
    for (uint16_t addr = start; addr <= end; addr++) {{
        if ((addr % 16) == 0) {{
            printf(""\n$%04X: "", addr);
        }}
        printf(""%02X "", nes_platform_read(platform, addr));
    }}
    printf(""\n"");
}}

// Debug: Set breakpoint
void nes_platform_set_breakpoint(NESPlatform* platform, uint16_t address, bool enabled) {{
    platform->breakpoints[address] = enabled;
    printf(""Breakpoint at $%04X %s\n"", address, enabled ? ""enabled"" : ""disabled"");
}}

// Debug: Dump CPU state
void nes_platform_dump_cpu_state(NESPlatform* platform) {{
    printf(""CPU State:\n"");
    printf(""  PC: $%04X\n"", platform->pc);
    printf(""  A:  $%02X\n"", platform->a);
    printf(""  X:  $%02X\n"", platform->x);
    printf(""  Y:  $%02X\n"", platform->y);
    printf(""  SP: $%02X\n"", platform->sp);
    printf(""  Status: $%02X [%c%c%c%c%c%c%c%c]\n"", 
           platform->status,
           (platform->status & NEGATIVE_FLAG) ? 'N' : '.',
           (platform->status & OVERFLOW_FLAG) ? 'V' : '.',
           (platform->status & UNUSED_FLAG) ? 'U' : '.',
           (platform->status & BREAK_FLAG) ? 'B' : '.',
           (platform->status & DECIMAL_FLAG) ? 'D' : '.',
           (platform->status & INTERRUPT_FLAG) ? 'I' : '.',
           (platform->status & ZERO_FLAG) ? 'Z' : '.',
           (platform->status & CARRY_FLAG) ? 'C' : '.');
    
    uint8_t opcode = nes_platform_read(platform, platform->pc);
    printf(""  Next: $%04X: $%02X ...\n"", platform->pc, opcode);
}}

// Run the decompiled code using the platform abstraction
void nes_platform_run_decompiled(NESPlatform* platform) {{
    printf(""==========================================================\n"");
    printf(""        {solutionName} - NES Decompiled Code Runner        \n"");
    printf(""==========================================================\n\n"");
    
    // Print detected entry points
    printf(""Trying to identify entry points...\n"");
    
    // Check if reset vector points to a valid location
    uint16_t reset_vector = platform->memory[0xFFFC] | (platform->memory[0xFFFD] << 8);
    printf(""Reset vector (from 0xFFFC): 0x%04X\n"", reset_vector);
    
    // Try calling entry point functions if they exist
    bool entry_point_found = false;
    
    // Define the most common entry points to try
    typedef void (*EntryFunc)();
    struct {{
        const char* name;
        EntryFunc func;
        bool exists;
    }} entry_points[] = {{
        #ifdef main
        {{ ""main"", (EntryFunc)main, true }},
        #else
        {{ ""main"", NULL, false }},
        #endif
        
        #ifdef reset_handler
        {{ ""reset_handler"", reset_handler, true }},
        #else
        {{ ""reset_handler"", NULL, false }},
        #endif
        
        #ifdef nmi_handler
        {{ ""nmi_handler"", nmi_handler, true }},
        #else
        {{ ""nmi_handler"", NULL, false }},
        #endif
        
        // Common entry points based on memory location
        #ifdef sub_8000
        {{ ""sub_8000"", sub_8000, true }},
        #else
        {{ ""sub_8000"", NULL, false }},
        #endif
        
        #ifdef sub_C000
        {{ ""sub_C000"", sub_C000, true }},
        #else
        {{ ""sub_C000"", NULL, false }},
        #endif
        
        // Try to match the reset vector if defined as a function
        {{ ""reset_vector_func"", NULL, false }}
    }};
    
    const int num_entry_points = sizeof(entry_points) / sizeof(entry_points[0]);
    
    // Try each entry point
    for (int i = 0; i < num_entry_points; i++) {{
        // Special handling for reset vector
        if (strcmp(entry_points[i].name, ""reset_vector_func"") == 0) {{
            // Try to find a function that matches the reset vector
            char reset_func_name[32];
            sprintf(reset_func_name, ""sub_%04X"", reset_vector);
            printf(""  Checking for function matching reset vector: %s"", reset_func_name);

            printf(""  (manual call required)\n"");
            continue;
        }}
        
        // Check if this entry point exists
        if (entry_points[i].exists && entry_points[i].func != NULL) {{
            printf(""  Found entry point: %s - Calling...\n"", entry_points[i].name);
            
            // Call the entry point function
            entry_points[i].func();
            entry_point_found = true;
            
            printf(""  Finished executing %s\n"", entry_points[i].name);
            break;
        }}
        else {{
            printf(""  Entry point not found: %s\n"", entry_points[i].name);
        }}
    }}
    
    if (!entry_point_found) {{
        printf(""\n==========================================================\n"");
        printf(""WARNING: Could not find any valid entry point function!\n"");
        printf(""You need to manually call one of the decompiled functions.\n"");
        printf(""Common entry points may include:\n"");
        printf(""  - sub_8000 (if ROM execution starts at 0x8000)\n"");
        printf(""  - sub_C000 (if ROM execution starts at 0xC000)\n"");
        printf(""  - A function corresponding to the reset vector address\n"");
        printf(""    e.g., sub_%04X\n"", reset_vector);
        printf(""==========================================================\n"");
    }}
    
    printf(""\nDecompiled code execution finished.\n"");
}}

// Main function
int main(int argc, char* argv[]) {{
    for (int i = 1; i < argc; i++) {{
        if (strcmp(argv[i], ""--debug"") == 0) {{
            debug_enabled = true;
            printf(""Debug mode enabled\n"");
        }}
    }}

    nes_platform_init(&nes);
    
    nes_platform_run_decompiled(&nes);
    
    // If debugging, dump final state
    if (debug_enabled) {{
        nes_platform_dump_cpu_state(&nes);
    }}
    
    return 0;
}}
";
            File.WriteAllText(Path.Combine(sourceDir, "main.c"), content);
        }

        /// <summary>
        /// Creates a CMakeLists.txt file for better project configuration
        /// </summary>
        private void CreateCMakeFile(string solutionDir)
        {
            string content = $@"cmake_minimum_required(VERSION 3.10)
project({solutionName} C)

# Set C standard
set(CMAKE_C_STANDARD 99)
set(CMAKE_C_STANDARD_REQUIRED ON)

# Add source files
file(GLOB SOURCES src/*.c)
file(GLOB HEADERS src/*.h)

# Create executable
add_executable(${{PROJECT_NAME}} ${{SOURCES}} ${{HEADERS}})

# Add include directories
target_include_directories(${{PROJECT_NAME}} PRIVATE src)

# Set warning level
if(MSVC)
    target_compile_options(${{PROJECT_NAME}} PRIVATE /W4)
else()
    target_compile_options(${{PROJECT_NAME}} PRIVATE -Wall -Wextra -pedantic)
endif()

# Output binary to bin directory
set_target_properties(${{PROJECT_NAME}} PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY ${{CMAKE_BINARY_DIR}}/bin
)

# Install target
install(TARGETS ${{PROJECT_NAME}} DESTINATION bin)
";
            File.WriteAllText(Path.Combine(solutionDir, "CMakeLists.txt"), content);
        }

        /// <summary>
        /// Creates the Visual Studio solution file
        /// </summary>
        private void CreateSolutionFile(string solutionDir)
        {
            // Generate a unique GUID for the solution
            string solutionGuid = Guid.NewGuid().ToString("B").ToUpper();
            string projectGuid = Guid.NewGuid().ToString("B").ToUpper();

            string content = $@"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.32014.148
MinimumVisualStudioVersion = 10.0.40219.1
Project(""{projectGuid}"") = ""{solutionName}"", ""{solutionName}.vcxproj"", ""{Guid.NewGuid():B}""
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|x64 = Debug|x64
		Debug|x86 = Debug|x86
		Release|x64 = Release|x64
		Release|x86 = Release|x86
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{{{Guid.NewGuid()}}}.Debug|x64.ActiveCfg = Debug|x64
		{{{Guid.NewGuid()}}}.Debug|x64.Build.0 = Debug|x64
		{{{Guid.NewGuid()}}}.Debug|x86.ActiveCfg = Debug|Win32
		{{{Guid.NewGuid()}}}.Debug|x86.Build.0 = Debug|Win32
		{{{Guid.NewGuid()}}}.Release|x64.ActiveCfg = Release|x64
		{{{Guid.NewGuid()}}}.Release|x64.Build.0 = Release|x64
		{{{Guid.NewGuid()}}}.Release|x86.ActiveCfg = Release|Win32
		{{{Guid.NewGuid()}}}.Release|x86.Build.0 = Release|Win32
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
	GlobalSection(ExtensibilityGlobals) = postSolution
		SolutionGuid = {solutionGuid}
	EndGlobalSection
EndGlobal
";
            File.WriteAllText(Path.Combine(solutionDir, $"{solutionName}.sln"), content);
        }

        /// <summary>
        /// Creates the Visual Studio project file
        /// </summary>
        private void CreateProjectFile(string solutionDir)
        {
            string projectGuid = Guid.NewGuid().ToString().ToUpper();

            string content = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project DefaultTargets=""Build"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <ItemGroup Label=""ProjectConfigurations"">
    <ProjectConfiguration Include=""Debug|Win32"">
      <Configuration>Debug</Configuration>
      <Platform>Win32</Platform>
    </ProjectConfiguration>
    <ProjectConfiguration Include=""Release|Win32"">
      <Configuration>Release</Configuration>
      <Platform>Win32</Platform>
    </ProjectConfiguration>
    <ProjectConfiguration Include=""Debug|x64"">
      <Configuration>Debug</Configuration>
      <Platform>x64</Platform>
    </ProjectConfiguration>
    <ProjectConfiguration Include=""Release|x64"">
      <Configuration>Release</Configuration>
      <Platform>x64</Platform>
    </ProjectConfiguration>
  </ItemGroup>
  <PropertyGroup Label=""Globals"">
    <VCProjectVersion>16.0</VCProjectVersion>
    <Keyword>Win32Proj</Keyword>
    <ProjectGuid>{{{projectGuid}}}</ProjectGuid>
    <RootNamespace>{solutionName}</RootNamespace>
    <WindowsTargetPlatformVersion>10.0</WindowsTargetPlatformVersion>
  </PropertyGroup>
  <Import Project=""$(VCTargetsPath)\Microsoft.Cpp.Default.props"" />
  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Debug|Win32'"" Label=""Configuration"">
    <ConfigurationType>Application</ConfigurationType>
    <UseDebugLibraries>true</UseDebugLibraries>
    <PlatformToolset>v143</PlatformToolset>
    <CharacterSet>Unicode</CharacterSet>
  </PropertyGroup>
  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Release|Win32'"" Label=""Configuration"">
    <ConfigurationType>Application</ConfigurationType>
    <UseDebugLibraries>false</UseDebugLibraries>
    <PlatformToolset>v143</PlatformToolset>
    <WholeProgramOptimization>true</WholeProgramOptimization>
    <CharacterSet>Unicode</CharacterSet>
  </PropertyGroup>
  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Debug|x64'"" Label=""Configuration"">
    <ConfigurationType>Application</ConfigurationType>
    <UseDebugLibraries>true</UseDebugLibraries>
    <PlatformToolset>v143</PlatformToolset>
    <CharacterSet>Unicode</CharacterSet>
  </PropertyGroup>
  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Release|x64'"" Label=""Configuration"">
    <ConfigurationType>Application</ConfigurationType>
    <UseDebugLibraries>false</UseDebugLibraries>
    <PlatformToolset>v143</PlatformToolset>
    <WholeProgramOptimization>true</WholeProgramOptimization>
    <CharacterSet>Unicode</CharacterSet>
  </PropertyGroup>
  <Import Project=""$(VCTargetsPath)\Microsoft.Cpp.props"" />
  <ImportGroup Label=""ExtensionSettings"">
  </ImportGroup>
  <ImportGroup Label=""Shared"">
  </ImportGroup>
  <ImportGroup Label=""PropertySheets"" Condition=""'$(Configuration)|$(Platform)'=='Debug|Win32'"">
    <Import Project=""$(UserRootDir)\Microsoft.Cpp.$(Platform).user.props"" Condition=""exists('$(UserRootDir)\Microsoft.Cpp.$(Platform).user.props')"" Label=""LocalAppDataPlatform"" />
  </ImportGroup>
  <ImportGroup Label=""PropertySheets"" Condition=""'$(Configuration)|$(Platform)'=='Release|Win32'"">
    <Import Project=""$(UserRootDir)\Microsoft.Cpp.$(Platform).user.props"" Condition=""exists('$(UserRootDir)\Microsoft.Cpp.$(Platform).user.props')"" Label=""LocalAppDataPlatform"" />
  </ImportGroup>
  <ImportGroup Label=""PropertySheets"" Condition=""'$(Configuration)|$(Platform)'=='Debug|x64'"">
    <Import Project=""$(UserRootDir)\Microsoft.Cpp.$(Platform).user.props"" Condition=""exists('$(UserRootDir)\Microsoft.Cpp.$(Platform).user.props')"" Label=""LocalAppDataPlatform"" />
  </ImportGroup>
  <ImportGroup Label=""PropertySheets"" Condition=""'$(Configuration)|$(Platform)'=='Release|x64'"">
    <Import Project=""$(UserRootDir)\Microsoft.Cpp.$(Platform).user.props"" Condition=""exists('$(UserRootDir)\Microsoft.Cpp.$(Platform).user.props')"" Label=""LocalAppDataPlatform"" />
  </ImportGroup>
  <PropertyGroup Label=""UserMacros"" />
  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Debug|Win32'"">
    <LinkIncremental>true</LinkIncremental>
    <OutDir>$(SolutionDir)bin\$(Platform)\$(Configuration)\</OutDir>
    <IntDir>$(SolutionDir)obj\$(Platform)\$(Configuration)\</IntDir>
  </PropertyGroup>
  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Release|Win32'"">
    <LinkIncremental>false</LinkIncremental>
    <OutDir>$(SolutionDir)bin\$(Platform)\$(Configuration)\</OutDir>
    <IntDir>$(SolutionDir)obj\$(Platform)\$(Configuration)\</IntDir>
  </PropertyGroup>
  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Debug|x64'"">
    <LinkIncremental>true</LinkIncremental>
    <OutDir>$(SolutionDir)bin\$(Platform)\$(Configuration)\</OutDir>
    <IntDir>$(SolutionDir)obj\$(Platform)\$(Configuration)\</IntDir>
  </PropertyGroup>
  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Release|x64'"">
    <LinkIncremental>false</LinkIncremental>
    <OutDir>$(SolutionDir)bin\$(Platform)\$(Configuration)\</OutDir>
    <IntDir>$(SolutionDir)obj\$(Platform)\$(Configuration)\</IntDir>
  </PropertyGroup>
  <ItemDefinitionGroup Condition=""'$(Configuration)|$(Platform)'=='Debug|Win32'"">
    <ClCompile>
      <WarningLevel>Level3</WarningLevel>
      <SDLCheck>true</SDLCheck>
      <PreprocessorDefinitions>WIN32;_DEBUG;_CONSOLE;%(PreprocessorDefinitions)</PreprocessorDefinitions>
      <ConformanceMode>true</ConformanceMode>
      <AdditionalIncludeDirectories>$(ProjectDir)src;%(AdditionalIncludeDirectories)</AdditionalIncludeDirectories>
    </ClCompile>
    <Link>
      <SubSystem>Console</SubSystem>
      <GenerateDebugInformation>true</GenerateDebugInformation>
    </Link>
  </ItemDefinitionGroup>
  <ItemDefinitionGroup Condition=""'$(Configuration)|$(Platform)'=='Release|Win32'"">
    <ClCompile>
      <WarningLevel>Level3</WarningLevel>
      <FunctionLevelLinking>true</FunctionLevelLinking>
      <IntrinsicFunctions>true</IntrinsicFunctions>
      <SDLCheck>true</SDLCheck>
      <PreprocessorDefinitions>WIN32;NDEBUG;_CONSOLE;%(PreprocessorDefinitions)</PreprocessorDefinitions>
      <ConformanceMode>true</ConformanceMode>
      <AdditionalIncludeDirectories>$(ProjectDir)src;%(AdditionalIncludeDirectories)</AdditionalIncludeDirectories>
    </ClCompile>
    <Link>
      <SubSystem>Console</SubSystem>
      <EnableCOMDATFolding>true</EnableCOMDATFolding>
      <OptimizeReferences>true</OptimizeReferences>
      <GenerateDebugInformation>true</GenerateDebugInformation>
    </Link>
  </ItemDefinitionGroup>
  <ItemDefinitionGroup Condition=""'$(Configuration)|$(Platform)'=='Debug|x64'"">
    <ClCompile>
      <WarningLevel>Level3</WarningLevel>
      <SDLCheck>true</SDLCheck>
      <PreprocessorDefinitions>_DEBUG;_CONSOLE;%(PreprocessorDefinitions)</PreprocessorDefinitions>
      <ConformanceMode>true</ConformanceMode>
      <AdditionalIncludeDirectories>$(ProjectDir)src;%(AdditionalIncludeDirectories)</AdditionalIncludeDirectories>
    </ClCompile>
    <Link>
      <SubSystem>Console</SubSystem>
      <GenerateDebugInformation>true</GenerateDebugInformation>
    </Link>
  </ItemDefinitionGroup>
  <ItemDefinitionGroup Condition=""'$(Configuration)|$(Platform)'=='Release|x64'"">
    <ClCompile>
      <WarningLevel>Level3</WarningLevel>
      <FunctionLevelLinking>true</FunctionLevelLinking>
      <IntrinsicFunctions>true</IntrinsicFunctions>
      <SDLCheck>true</SDLCheck>
      <PreprocessorDefinitions>NDEBUG;_CONSOLE;%(PreprocessorDefinitions)</PreprocessorDefinitions>
      <ConformanceMode>true</ConformanceMode>
      <AdditionalIncludeDirectories>$(ProjectDir)src;%(AdditionalIncludeDirectories)</AdditionalIncludeDirectories>
    </ClCompile>
    <Link>
      <SubSystem>Console</SubSystem>
      <EnableCOMDATFolding>true</EnableCOMDATFolding>
      <OptimizeReferences>true</OptimizeReferences>
      <GenerateDebugInformation>true</GenerateDebugInformation>
    </Link>
  </ItemDefinitionGroup>
  <ItemGroup>
    <ClCompile Include=""src\main.c"" />
    <ClCompile Include=""src\{solutionName}.c"" />
  </ItemGroup>
  <ItemGroup>
    <ClInclude Include=""src\nes_platform.h"" />
    <ClInclude Include=""src\{solutionName}.h"" />
  </ItemGroup>
  <Import Project=""$(VCTargetsPath)\Microsoft.Cpp.targets"" />
  <ImportGroup Label=""ExtensionTargets"">
  </ImportGroup>
</Project>
";
            File.WriteAllText(Path.Combine(solutionDir, $"{solutionName}.vcxproj"), content);
        }

        /// <summary>
        /// Creates the Visual Studio project filters file
        /// </summary>
        private void CreateFiltersFile(string solutionDir)
        {
            string content = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <ItemGroup>
    <Filter Include=""Source Files"">
      <UniqueIdentifier>{{{Guid.NewGuid()}}}</UniqueIdentifier>
      <Extensions>cpp;c;cc;cxx;c++;cppm;ixx;def;odl;idl;hpj;bat;asm;asmx</Extensions>
    </Filter>
    <Filter Include=""Header Files"">
      <UniqueIdentifier>{{{Guid.NewGuid()}}}</UniqueIdentifier>
      <Extensions>h;hh;hpp;hxx;h++;hm;inl;inc;ipp;xsd</Extensions>
    </Filter>
    <Filter Include=""Resource Files"">
      <UniqueIdentifier>{{{Guid.NewGuid()}}}</UniqueIdentifier>
      <Extensions>rc;ico;cur;bmp;dlg;rc2;rct;bin;rgs;gif;jpg;jpeg;jpe;resx;tiff;tif;png;wav;mfcribbon-ms</Extensions>
    </Filter>
  </ItemGroup>
  <ItemGroup>
    <ClCompile Include=""src\main.c"">
      <Filter>Source Files</Filter>
    </ClCompile>
    <ClCompile Include=""src\{solutionName}.c"">
      <Filter>Source Files</Filter>
    </ClCompile>
  </ItemGroup>
  <ItemGroup>
    <ClInclude Include=""src\nes_platform.h"">
      <Filter>Header Files</Filter>
    </ClInclude>
    <ClInclude Include=""src\{solutionName}.h"">
      <Filter>Header Files</Filter>
    </ClInclude>
  </ItemGroup>
</Project>
";
            File.WriteAllText(Path.Combine(solutionDir, $"{solutionName}.vcxproj.filters"), content);
        }

        /// <summary>
        /// Creates a README file with instructions
        /// </summary>
        private void CreateReadmeFile(string solutionDir)
        {
            string content = $@"# {solutionName} - Decompiled NES ROM

This project contains a decompiled version of a NES ROM, converted to C code that can be compiled and run on modern platforms.

## Project Structure

- `src/{solutionName}.c` - The main decompiled code
- `src/{solutionName}.h` - Header file with function and variable declarations
- `src/nes_platform.h` - Platform abstraction layer for NES hardware
- `src/main.c` - Entry point that sets up the environment and runs the decompiled code

## Building the Project

### Using Visual Studio 2022
1. Open the solution file ({solutionName}.sln) in Visual Studio 2022
2. Select the desired build configuration (Debug/Release) and platform (x86/x64)
3. Build the solution (F7 or Build > Build Solution)
4. Run the program (F5 or Debug > Start Debugging)

### Using CMake
```
mkdir build
cd build
cmake ..
cmake --build .
```

## Running the Decompiled Code

The decompiled code is encapsulated in a wrapper that provides the necessary NES hardware emulation layer. This allows the code to run on modern systems without requiring a full NES emulator.

## Adapting for Other Platforms

To port this code to other platforms:

1. Modify the `nes_platform.h` file to match the target platform's capabilities
2. Update memory access patterns in `main.c` if needed
3. Replace platform-specific code with equivalents for the target platform

## Notes on Decompilation

The decompilation process attempts to recreate the original source code based on the binary ROM. Some aspects of the original code may be approximated or reconstructed in a way that produces equivalent behavior but doesn't match the original source exactly.

- Function boundaries may not exactly match the original source
- Variable names are chosen by the decompiler and may not match original names
- Control flow structures may be simplified or restructured
- Code might not Compile directly and needs Modification, in this current state its more Pseudo-C than anything :p

## License

This decompiled code is provided for educational and research purposes only. All intellectual property rights to the original software remain with their respective owners.
";
            File.WriteAllText(Path.Combine(solutionDir, "README.md"), content);
        }

        /// <summary>
        /// Opens the solution in Visual Studio if available
        /// </summary>
        /// <param name="solutionPath">The path to the solution file</param>
        public static void OpenInVisualStudio(string solutionPath)
        {
            try
            {
                if (File.Exists(solutionPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = solutionPath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open solution in Visual Studio: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}