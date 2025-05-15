using NESDecompiler.Core.Decompilation;
using NESDecompiler.Core.ROM;

public class Project
{
    public string Name { get; set; }
    public string FilePath { get; set; }
    public List<ROMProject> ROMs { get; set; } = new List<ROMProject>();
    public DateTime LastModified { get; set; }
}

public class ROMProject
{
    public string Name { get; set; }
    public string FilePath { get; set; }
    public bool IsDisassembled { get; set; }
    public bool IsDecompiled { get; set; }
    public ROMInfo ROMInfo { get; set; }
    public string DisassemblyText { get; set; } = string.Empty;
    public string CCodeText { get; set; } = string.Empty;
    public string HeaderText { get; set; } = string.Empty;
    public Dictionary<string, VariableWorkspaceData> Variables { get; set; } = new Dictionary<string, VariableWorkspaceData>();
    public Dictionary<string, FunctionWorkspaceData> Functions { get; set; } = new Dictionary<string, FunctionWorkspaceData>();
    public bool IsExpanded { get; set; } = true;
}