using System.ComponentModel;

public class VariableViewModel : INotifyPropertyChanged
{
    private ushort address;
    private string name;
    private string typeName;
    private int size;
    private bool isRead;
    private bool isWritten;
    private string description;

    public ushort Address
    {
        get => address;
        set { address = value; OnPropertyChanged(nameof(Address)); OnPropertyChanged(nameof(AddressHex)); }
    }

    public string AddressHex => $"0x{Address:X4}";

    public string Name
    {
        get => name;
        set { name = value; OnPropertyChanged(nameof(Name)); }
    }

    public string TypeName
    {
        get => typeName;
        set { typeName = value; OnPropertyChanged(nameof(TypeName)); }
    }

    public int Size
    {
        get => size;
        set { size = value; OnPropertyChanged(nameof(Size)); }
    }

    public bool IsRead
    {
        get => isRead;
        set { isRead = value; OnPropertyChanged(nameof(IsRead)); OnPropertyChanged(nameof(AccessType)); }
    }

    public bool IsWritten
    {
        get => isWritten;
        set { isWritten = value; OnPropertyChanged(nameof(IsWritten)); OnPropertyChanged(nameof(AccessType)); }
    }

    public string AccessType => $"{(IsRead ? "Read" : "")}{(IsRead && IsWritten ? "/" : "")}{(IsWritten ? "Write" : "")}";

    public string Description
    {
        get => description;
        set { description = value; OnPropertyChanged(nameof(Description)); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class FunctionViewModel : INotifyPropertyChanged
{
    private ushort address;
    private string name;
    private int instructionCount;
    private int variableCount;
    private int calledFunctionCount;
    private List<string> parameters = new List<string>();
    private string description;

    public ushort Address
    {
        get => address;
        set { address = value; OnPropertyChanged(nameof(Address)); OnPropertyChanged(nameof(AddressHex)); }
    }

    public string AddressHex => $"0x{Address:X4}";

    public string Name
    {
        get => name;
        set { name = value; OnPropertyChanged(nameof(Name)); }
    }

    public int InstructionCount
    {
        get => instructionCount;
        set { instructionCount = value; OnPropertyChanged(nameof(InstructionCount)); }
    }

    public int VariableCount
    {
        get => variableCount;
        set { variableCount = value; OnPropertyChanged(nameof(VariableCount)); }
    }

    public int CalledFunctionCount
    {
        get => calledFunctionCount;
        set { calledFunctionCount = value; OnPropertyChanged(nameof(CalledFunctionCount)); }
    }

    public List<string> Parameters
    {
        get => parameters;
        set { parameters = value; OnPropertyChanged(nameof(Parameters)); }
    }

    public string Description
    {
        get => description;
        set { description = value; OnPropertyChanged(nameof(Description)); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}