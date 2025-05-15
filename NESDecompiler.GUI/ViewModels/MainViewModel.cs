using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using NESDecompiler.Core.ROM;
using NESDecompiler.Core.Disassembly;
using NESDecompiler.Core.Decompilation;
using NESDecompiler.Core.Exceptions;
using NESDecompiler.GUI.Commands;
using System.Diagnostics;
using static NESDecompiler.GUI.ViewModels.ProjectItemViewModel;
using NESDecompiler.GUI.Themes;

namespace NESDecompiler.GUI.ViewModels
{
    /// <summary>
    /// Main view model for the application
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Fields

        private string statusText = "Ready";
        private bool isProcessing = false;
        private string processingText = "";
        private bool isROMLoaded = false;
        private bool isDisassembled = false;
        private bool isDecompiled = false;
        private string disassemblyText = "";
        private string cCodeText = "";
        private string headerText = "";
        private bool hasUnsavedChanges = false;
        private string currentFilePath = "";
        private ProjectItemViewModel? selectedProjectItem;
        private List<string> recentFiles = new List<string>();
        private ROMInfo? romInfo;
        private Disassembler? disassembler;
        private Decompiler? decompiler;
        private const string WORKSPACE_DIRECTORY = "Workspaces";
        private const string DEFAULT_WORKSPACE_FILENAME = "default.nesworkspace";
        private ObservableCollection<VariableViewModel> variableList = new ObservableCollection<VariableViewModel>();
        private ObservableCollection<FunctionViewModel> functionList = new ObservableCollection<FunctionViewModel>();
        private VariableViewModel? selectedVariable;
        private FunctionViewModel? selectedFunction;
        private string variableSearchText = string.Empty;
        private string functionSearchText = string.Empty;
        private Project currentProject;
        private ROMProject activeROM;
        private ObservableCollection<Project> recentProjects = new ObservableCollection<Project>();
        private const int MAX_RECENT_ITEMS = 10;
        private List<string> recentWorkspaces = new List<string>();
        private List<RecentProjectInfo> recentProjectsList = new List<RecentProjectInfo>();

        #endregion

        #region Properties

        /// <summary>
        /// Whether the application is using dark theme
        /// </summary>
        public bool IsDarkTheme
        {
            get => ThemeManager.Instance.IsDarkTheme;
            set
            {
                ThemeManager.Instance.SetTheme(value);
                OnPropertyChanged(nameof(IsDarkTheme));
            }
        }

        /// <summary>
        /// Status text for the status bar
        /// </summary>
        public string StatusText
        {
            get => statusText;
            set
            {
                statusText = value;
                OnPropertyChanged(nameof(StatusText));
            }
        }

        /// <summary>
        /// Whether a long-running process is in progress
        /// </summary>
        public bool IsProcessing
        {
            get => isProcessing;
            set
            {
                isProcessing = value;
                OnPropertyChanged(nameof(IsProcessing));
            }
        }

        /// <summary>
        /// Text to display during processing
        /// </summary>
        public string ProcessingText
        {
            get => processingText;
            set
            {
                processingText = value;
                OnPropertyChanged(nameof(ProcessingText));
            }
        }

        /// <summary>
        /// Whether a ROM is loaded
        /// </summary>
        public bool IsROMLoaded
        {
            get => isROMLoaded;
            set
            {
                isROMLoaded = value;
                OnPropertyChanged(nameof(IsROMLoaded));
            }
        }

        /// <summary>
        /// Whether the ROM has been disassembled
        /// </summary>
        public bool IsDisassembled
        {
            get => isDisassembled;
            set
            {
                isDisassembled = value;
                OnPropertyChanged(nameof(IsDisassembled));
            }
        }

        /// <summary>
        /// Whether the ROM has been decompiled
        /// </summary>
        public bool IsDecompiled
        {
            get => isDecompiled;
            set
            {
                isDecompiled = value;
                OnPropertyChanged(nameof(IsDecompiled));
            }
        }

        /// <summary>
        /// The disassembly text
        /// </summary>
        public string DisassemblyText
        {
            get => disassemblyText;
            set
            {
                disassemblyText = value;
                OnPropertyChanged(nameof(DisassemblyText));
            }
        }

        /// <summary>
        /// The C code text
        /// </summary>
        public string CCodeText
        {
            get => cCodeText;
            set
            {
                cCodeText = value;
                OnPropertyChanged(nameof(CCodeText));
            }
        }

        /// <summary>
        /// The header text
        /// </summary>
        public string HeaderText
        {
            get => headerText;
            set
            {
                headerText = value;
                OnPropertyChanged(nameof(HeaderText));
            }
        }

        /// <summary>
        /// Whether there are unsaved changes
        /// </summary>
        public bool HasUnsavedChanges
        {
            get => hasUnsavedChanges;
            set
            {
                hasUnsavedChanges = value;
                OnPropertyChanged(nameof(HasUnsavedChanges));
            }
        }

        /// <summary>
        /// Currently selected project item
        /// </summary>
        public ProjectItemViewModel? SelectedProjectItem
        {
            get => selectedProjectItem;
            set
            {
                selectedProjectItem = value;
                OnPropertyChanged(nameof(SelectedProjectItem));

                if (selectedProjectItem != null)
                {
                }
            }
        }

        /// <summary>
        /// Project items for the tree view
        /// </summary>
        public ObservableCollection<ProjectItemViewModel> ProjectItems { get; } = new ObservableCollection<ProjectItemViewModel>();

        /// <summary>
        /// Recent files list
        /// </summary>
        public List<string> RecentFiles
        {
            get => recentFiles;
            set
            {
                recentFiles = value;
                OnPropertyChanged(nameof(RecentFiles));
            }
        }

        public ObservableCollection<VariableViewModel> VariableList
        {
            get => variableList;
            set
            {
                variableList = value;
                OnPropertyChanged(nameof(VariableList));
            }
        }

        public ObservableCollection<FunctionViewModel> FunctionList
        {
            get => functionList;
            set
            {
                functionList = value;
                OnPropertyChanged(nameof(FunctionList));
            }
        }

        public VariableViewModel? SelectedVariable
        {
            get => selectedVariable;
            set
            {
                selectedVariable = value;
                OnPropertyChanged(nameof(SelectedVariable));
            }
        }

        public FunctionViewModel? SelectedFunction
        {
            get => selectedFunction;
            set
            {
                selectedFunction = value;
                OnPropertyChanged(nameof(SelectedFunction));
            }
        }

        public string VariableSearchText
        {
            get => variableSearchText;
            set
            {
                variableSearchText = value;
                OnPropertyChanged(nameof(VariableSearchText));
            }
        }

        public string FunctionSearchText
        {
            get => functionSearchText;
            set
            {
                functionSearchText = value;
                OnPropertyChanged(nameof(FunctionSearchText));
            }
        }

        public Project CurrentProject
        {
            get => currentProject;
            set
            {
                currentProject = value;
                OnPropertyChanged(nameof(CurrentProject));
                OnPropertyChanged(nameof(HasProject));
            }
        }

        public ROMProject ActiveROM
        {
            get => activeROM;
            set
            {
                if (activeROM != null && value != activeROM)
                {
                    activeROM.DisassemblyText = DisassemblyText;
                    activeROM.CCodeText = CCodeText;
                    activeROM.HeaderText = HeaderText;
                }

                activeROM = value;
                OnPropertyChanged(nameof(ActiveROM));

                if (activeROM != null)
                {
                    currentFilePath = activeROM.FilePath;
                    romInfo = activeROM.ROMInfo;

                    IsROMLoaded = true;
                    IsDisassembled = activeROM.IsDisassembled;
                    IsDecompiled = activeROM.IsDecompiled;

                    if (!string.IsNullOrEmpty(activeROM.DisassemblyText))
                        DisassemblyText = activeROM.DisassemblyText;

                    if (!string.IsNullOrEmpty(activeROM.CCodeText))
                        CCodeText = activeROM.CCodeText;

                    if (!string.IsNullOrEmpty(activeROM.HeaderText))
                        HeaderText = activeROM.HeaderText;

                    if (activeROM.Variables != null && activeROM.Variables.Count > 0)
                    {
                        VariableList.Clear();
                        foreach (var varData in activeROM.Variables.Values)
                        {
                            VariableList.Add(new VariableViewModel
                            {
                                Address = GetAddressFromDescription(varData.Description),
                                Name = varData.Name,
                                TypeName = varData.Type,
                                Size = GetSizeFromDescription(varData.Description),
                                Description = varData.Description
                            });
                        }
                    }

                    if (activeROM.Functions != null && activeROM.Functions.Count > 0)
                    {
                        FunctionList.Clear();
                        foreach (var funcData in activeROM.Functions.Values)
                        {
                            FunctionList.Add(new FunctionViewModel
                            {
                                Address = GetAddressFromDescription(funcData.Description),
                                Name = funcData.Name,
                                InstructionCount = GetInstructionCountFromDescription(funcData.Description),
                                Description = funcData.Description
                            });
                        }
                    }

                    OnPropertyChanged(nameof(IsDisassembled));
                    OnPropertyChanged(nameof(IsDecompiled));

                    UpdateProjectExplorer();
                }
                else
                {
                    IsROMLoaded = false;
                    IsDisassembled = false;
                    IsDecompiled = false;
                    DisassemblyText = string.Empty;
                    CCodeText = string.Empty;
                    HeaderText = string.Empty;
                }
            }
        }

        private ushort GetAddressFromDescription(string description)
        {
            try
            {
                var match = System.Text.RegularExpressions.Regex.Match(description, @"Address: 0x([0-9A-F]{4})");
                if (match.Success && match.Groups.Count > 1)
                {
                    return Convert.ToUInt16(match.Groups[1].Value, 16);
                }
            }
            catch { }
            return 0;
        }

        private int GetSizeFromDescription(string description)
        {
            try
            {
                var match = System.Text.RegularExpressions.Regex.Match(description, @"Size: (\d+)");
                if (match.Success && match.Groups.Count > 1)
                {
                    return int.Parse(match.Groups[1].Value);
                }
            }
            catch { }
            return 1;
        }

        private int GetInstructionCountFromDescription(string description)
        {
            try
            {
                var match = System.Text.RegularExpressions.Regex.Match(description, @"Instructions: (\d+)");
                if (match.Success && match.Groups.Count > 1)
                {
                    return int.Parse(match.Groups[1].Value);
                }
            }
            catch { }
            return 0;
        }

        public ObservableCollection<Project> RecentProjects
        {
            get => recentProjects;
            set
            {
                recentProjects = value;
                OnPropertyChanged(nameof(RecentProjects));
            }
        }

        public List<string> RecentWorkspaces
        {
            get => recentWorkspaces;
            set
            {
                recentWorkspaces = value;
                OnPropertyChanged(nameof(RecentWorkspaces));
            }
        }

        public List<RecentProjectInfo> RecentProjectsList
        {
            get => recentProjectsList;
            set
            {
                recentProjectsList = value;
                OnPropertyChanged(nameof(RecentProjectsList));
            }
        }

        public bool HasProject => CurrentProject != null;

        #endregion

        #region Commands

        public ICommand OpenROMCommand { get; }
        public ICommand SaveDisassemblyCommand { get; }
        public ICommand SaveCCodeCommand { get; }
        public ICommand SaveHeaderCommand { get; }
        public ICommand SaveAllCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand DisassembleCommand { get; }
        public ICommand DecompileCommand { get; }
        public ICommand ViewROMInfoCommand { get; }
        public ICommand ViewVariablesCommand { get; }
        public ICommand ViewFunctionsCommand { get; }
        public ICommand AboutCommand { get; }
        public ICommand ExportToVSCommand { get; }
        public ICommand JumpToVariableDefinitionCommand { get; }
        public ICommand FindVariableReferencesCommand { get; }
        public ICommand RenameVariableCommand { get; }
        public ICommand JumpToFunctionDefinitionCommand { get; }
        public ICommand FindFunctionReferencesCommand { get; }
        public ICommand RenameFunctionCommand { get; }
        public ICommand SearchVariablesCommand { get; }
        public ICommand SearchFunctionsCommand { get; }
        public ICommand SaveWorkspaceCommand { get; }
        public ICommand NewProjectCommand { get; }
        public ICommand OpenProjectCommand { get; }
        public ICommand SaveProjectCommand { get; }
        public ICommand SaveProjectAsCommand { get; }
        public ICommand AddROMCommand { get; }
        public ICommand RemoveROMCommand { get; }
        public ICommand ToggleThemeCommand { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of the MainViewModel
        /// </summary>
        public MainViewModel()
        {
            OpenROMCommand = new RelayCommand(OpenROM);
            SaveDisassemblyCommand = new RelayCommand(SaveDisassembly, () => IsDisassembled);
            SaveCCodeCommand = new RelayCommand(SaveCCode, () => IsDecompiled);
            SaveHeaderCommand = new RelayCommand(SaveHeader, () => IsDecompiled);
            SaveAllCommand = new RelayCommand(SaveAll, () => IsDisassembled || IsDecompiled);
            ExitCommand = new RelayCommand(Exit);
            DisassembleCommand = new RelayCommand(Disassemble, () => IsROMLoaded);
            DecompileCommand = new RelayCommand(Decompile, () => IsDisassembled);
            ViewROMInfoCommand = new RelayCommand(ViewROMInfo, () => IsROMLoaded);
            ViewVariablesCommand = new RelayCommand(ViewVariables, () => IsDecompiled);
            ViewFunctionsCommand = new RelayCommand(ViewFunctions, () => IsDecompiled);
            AboutCommand = new RelayCommand(ShowAbout);
            ExportToVSCommand = new RelayCommand(ExportToVisualStudio, () => IsDecompiled);
            JumpToVariableDefinitionCommand = new RelayCommand(JumpToVariableDefinition, () => SelectedVariable != null);
            FindVariableReferencesCommand = new RelayCommand(FindVariableReferences, () => SelectedVariable != null);
            RenameVariableCommand = new RelayCommand(RenameVariable, () => SelectedVariable != null);
            JumpToFunctionDefinitionCommand = new RelayCommand(JumpToFunctionDefinition, () => SelectedFunction != null);
            FindFunctionReferencesCommand = new RelayCommand(FindFunctionReferences, () => SelectedFunction != null);
            RenameFunctionCommand = new RelayCommand(RenameFunction, () => SelectedFunction != null);
            SearchVariablesCommand = new RelayCommand(SearchVariables, () => !string.IsNullOrEmpty(VariableSearchText));
            SearchFunctionsCommand = new RelayCommand(SearchFunctions, () => !string.IsNullOrEmpty(FunctionSearchText));
            SaveWorkspaceCommand = new RelayCommand(SaveWorkspace, () => IsROMLoaded);
            NewProjectCommand = new RelayCommand(NewProject);
            OpenProjectCommand = new RelayCommand(OpenProject);
            SaveProjectCommand = new RelayCommand(SaveProject, () => HasProject);
            SaveProjectAsCommand = new RelayCommand(SaveProjectAs, () => HasProject);
            AddROMCommand = new RelayCommand(AddROM, () => HasProject);
            ToggleThemeCommand = new RelayCommand(ToggleTheme);
            RemoveROMCommand = new RelayCommand(RemoveROM, () => IsROMLoaded);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the view model
        /// </summary>
        public void Initialize()
        {
            ThemeManager.Instance.ApplyTheme();
            LoadRecentFiles();
            LoadRecentWorkspaces();
            LoadRecentProjects();
            if (CurrentProject == null)
            {
                CurrentProject = new Project
                {
                    Name = "New Project",
                    FilePath = "",
                    LastModified = DateTime.Now
                };
                UpdateProjectExplorer();
            }
        }

        /// <summary>
        /// Loads the list of recent files
        /// </summary>
        private void LoadRecentFiles()
        {
            try
            {
                string workspaceDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "NESDecompiler", WORKSPACE_DIRECTORY);

                string workspacePath = Path.Combine(workspaceDir, DEFAULT_WORKSPACE_FILENAME);

                if (!Directory.Exists(workspaceDir))
                {
                    Directory.CreateDirectory(workspaceDir);
                }

                if (File.Exists(workspacePath))
                {
                    string json = File.ReadAllText(workspacePath);
                    var workspace = System.Text.Json.JsonSerializer.Deserialize<WorkspaceFile>(json);

                    if (workspace != null)
                    {
                        RecentFiles = workspace.RecentFiles ?? new List<string>();

                        if (!string.IsNullOrEmpty(workspace.CurrentFilePath) && File.Exists(workspace.CurrentFilePath))
                        {
                            Task.Run(() => Application.Current.Dispatcher.Invoke(() =>
                                LoadROMFile(workspace.CurrentFilePath)));
                        }
                    }
                }
                else
                {
                    RecentFiles = new List<string>();
                    SaveRecentFiles();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load workspace: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                RecentFiles = new List<string>();
            }
        }

        /// <summary>
        /// Saves the list of recent files
        /// </summary>
        private void SaveRecentFiles()
        {
            try
            {
                string workspaceDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "NESDecompiler", WORKSPACE_DIRECTORY);

                string workspacePath = Path.Combine(workspaceDir, DEFAULT_WORKSPACE_FILENAME);

                if (!Directory.Exists(workspaceDir))
                {
                    Directory.CreateDirectory(workspaceDir);
                }

                var workspace = new WorkspaceFile
                {
                    CurrentFilePath = currentFilePath,
                    RecentFiles = recentFiles,
                    IsDisassembled = isDisassembled,
                    IsDecompiled = isDecompiled
                };

                if (decompiler != null)
                {
                    foreach (var variable in decompiler.Variables.Values)
                    {
                        workspace.Variables[variable.Name] = new VariableWorkspaceData
                        {
                            Name = variable.Name,
                            Type = variable.Type.ToString(),
                            Description = $"Address: 0x{variable.Address:X4}, Size: {variable.Size}"
                        };
                    }

                    foreach (var function in decompiler.Functions.Values)
                    {
                        workspace.Functions[function.Name] = new FunctionWorkspaceData
                        {
                            Name = function.Name,
                            ReturnType = "void",
                            Parameters = new List<string>(),
                            Description = $"Address: 0x{function.Address:X4}, Instructions: {function.Instructions.Count}"
                        };
                    }
                }

                string json = System.Text.Json.JsonSerializer.Serialize(workspace, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(workspacePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save workspace: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Adds a file to the recent files list
        /// </summary>
        /// <param name="filePath">The file path to add</param>
        private void AddRecentFile(string filePath)
        {
            try
            {
                recentFiles.Remove(filePath);

                recentFiles.Insert(0, filePath);

                if (recentFiles.Count > 10)
                {
                    recentFiles.RemoveAt(recentFiles.Count - 1);
                }

                OnPropertyChanged(nameof(RecentFiles));

                SaveRecentFiles();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add recent file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Opens a ROM file
        /// </summary>
        private async void OpenROM()
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Title = "Open NES ROM",
                    Filter = "NES ROM Files (*.nes)|*.nes|All Files (*.*)|*.*",
                    FilterIndex = 1,
                    Multiselect = true
                };

                if (dialog.ShowDialog() == true)
                {
                    if (CurrentProject == null)
                    {
                        CurrentProject = new Project
                        {
                            Name = "New Project",
                            FilePath = "",
                            LastModified = DateTime.Now
                        };
                    }

                    foreach (string filePath in dialog.FileNames)
                    {
                        await LoadROMFile(filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open ROM: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText = "Failed to open ROM";
            }
        }

        /// <summary>
        /// Opens a recent file
        /// </summary>
        /// <param name="filePath">The file path to open</param>
        public async void OpenRecentFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    await LoadROMFile(filePath);
                }
                else
                {
                    MessageBox.Show($"The file '{filePath}' no longer exists.", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Error);

                    recentFiles.Remove(filePath);
                    OnPropertyChanged(nameof(RecentFiles));
                    SaveRecentFiles();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText = "Failed to open file";
            }
        }

        /// <summary>
        /// Loads a ROM file
        /// </summary>
        /// <param name="filePath">The file path to load</param>
        private async Task LoadROMFile(string filePath)
        {
            try
            {
                IsProcessing = true;
                ProcessingText = "Loading ROM...";
                StatusText = $"Loading {Path.GetFileName(filePath)}...";

                if (CurrentProject.ROMs.Any(r => r.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show($"ROM {Path.GetFileName(filePath)} is already loaded in this project.",
                        "ROM Already Loaded", MessageBoxButton.OK, MessageBoxImage.Information);

                    ActiveROM = CurrentProject.ROMs.First(r => r.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));
                    IsProcessing = false;
                    return;
                }

                var romProject = new ROMProject
                {
                    Name = Path.GetFileName(filePath),
                    FilePath = filePath
                };

                bool loadSuccess = false;

                await Task.Run(() =>
                {
                    try
                    {
                        var romLoader = new ROMLoader();
                        romProject.ROMInfo = romLoader.LoadFromFile(filePath);

                        byte[] prgRomData = romLoader.GetPRGROMData();
                        if (prgRomData == null || prgRomData.Length == 0)
                        {
                            throw new InvalidOperationException("ROM contains no program data");
                        }

                        loadSuccess = true;
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"Error loading ROM: {ex.Message}", "ROM Loading Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                    }
                });

                if (loadSuccess)
                {
                    CurrentProject.ROMs.Add(romProject);

                    ActiveROM = romProject;

                    StatusText = $"Loaded {Path.GetFileName(filePath)}";
                    AddRecentFile(filePath);

                    UpdateProjectExplorer();

                    await DisassembleAsync();
                }
                else
                {
                    StatusText = "Failed to load ROM";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load ROM: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText = "Failed to load ROM";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private void UpdateProjectExplorer()
        {
            var expandedPaths = new List<string>();

            foreach (var rootItem in ProjectItems)
            {
                CollectExpandedPaths(rootItem, "", expandedPaths);
            }

            ProjectItems.Clear();

            if (CurrentProject == null)
                return;

            var projectRoot = new ProjectItemViewModel
            {
                Name = CurrentProject.Name,
                Type = ProjectItemType.Project,
                Path = CurrentProject.FilePath
            };

            ProjectItems.Add(projectRoot);

            foreach (var rom in CurrentProject.ROMs)
            {
                var romItem = new ProjectItemViewModel
                {
                    Name = rom.Name,
                    Type = ProjectItemType.ROM,
                    Path = rom.FilePath,
                    Tag = rom,
                    IsExpanded = rom.IsExpanded
                };

                projectRoot.Children.Add(romItem);

                romItem.Children.Add(new ProjectItemViewModel
                {
                    Name = "ROM Info",
                    Type = ProjectItemType.ROMInfo,
                    Path = rom.FilePath,
                    Tag = rom
                });

                if (rom.IsDisassembled)
                {
                    romItem.Children.Add(new ProjectItemViewModel
                    {
                        Name = "Disassembly",
                        Type = ProjectItemType.Disassembly,
                        Path = rom.FilePath,
                        Tag = rom
                    });
                }

                if (rom.IsDecompiled)
                {
                    romItem.Children.Add(new ProjectItemViewModel
                    {
                        Name = "C Code",
                        Type = ProjectItemType.CCode,
                        Path = rom.FilePath,
                        Tag = rom
                    });

                    romItem.Children.Add(new ProjectItemViewModel
                    {
                        Name = "Header",
                        Type = ProjectItemType.Header,
                        Path = rom.FilePath,
                        Tag = rom
                    });

                    romItem.Children.Add(new ProjectItemViewModel
                    {
                        Name = "Variables",
                        Type = ProjectItemType.Variables,
                        Path = rom.FilePath,
                        Tag = rom
                    });

                    romItem.Children.Add(new ProjectItemViewModel
                    {
                        Name = "Functions",
                        Type = ProjectItemType.Functions,
                        Path = rom.FilePath,
                        Tag = rom
                    });
                }
            }

            RestoreExpandedPaths(ProjectItems[0], "", expandedPaths);
        }

        private void CollectExpandedPaths(ProjectItemViewModel item, string path, List<string> expandedPaths)
        {
            if (item == null) return;

            string currentPath = string.IsNullOrEmpty(path) ? item.Name : path + "/" + item.Name;

            if (item.Tag is ROMProject rom)
            {
                if (rom.IsExpanded)
                {
                    expandedPaths.Add(currentPath);
                }
            }
            else if (item.IsExpanded)
            {
                expandedPaths.Add(currentPath);
            }

            foreach (var child in item.Children)
            {
                CollectExpandedPaths(child, currentPath, expandedPaths);
            }
        }

        private void RestoreExpandedPaths(ProjectItemViewModel item, string path, List<string> expandedPaths)
        {
            if (item == null) return;

            string currentPath = string.IsNullOrEmpty(path) ? item.Name : path + "/" + item.Name;

            if (expandedPaths.Contains(currentPath))
            {
                item.IsExpanded = true;

                if (item.Tag is ROMProject rom)
                {
                    rom.IsExpanded = true;
                }
            }

            foreach (var child in item.Children)
            {
                RestoreExpandedPaths(child, currentPath, expandedPaths);
            }
        }

        /// <summary>
        /// Saves the disassembly to a file
        /// </summary>
        private void SaveDisassembly()
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Title = "Save Disassembly",
                    Filter = "Assembly Files (*.asm)|*.asm|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                    FilterIndex = 1,
                    FileName = Path.GetFileNameWithoutExtension(currentFilePath) + ".asm"
                };

                if (dialog.ShowDialog() == true)
                {
                    File.WriteAllText(dialog.FileName, DisassemblyText);
                    StatusText = $"Saved disassembly to {Path.GetFileName(dialog.FileName)}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save disassembly: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText = "Failed to save disassembly";
            }
        }

        /// <summary>
        /// Saves the C code to a file
        /// </summary>
        private void SaveCCode()
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Title = "Save C Code",
                    Filter = "C Files (*.c)|*.c|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                    FilterIndex = 1,
                    FileName = Path.GetFileNameWithoutExtension(currentFilePath) + ".c"
                };

                if (dialog.ShowDialog() == true)
                {
                    File.WriteAllText(dialog.FileName, CCodeText);
                    StatusText = $"Saved C code to {Path.GetFileName(dialog.FileName)}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save C code: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText = "Failed to save C code";
            }
        }

        /// <summary>
        /// Saves the header to a file
        /// </summary>
        private void SaveHeader()
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Title = "Save Header",
                    Filter = "C Header Files (*.h)|*.h|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                    FilterIndex = 1,
                    FileName = Path.GetFileNameWithoutExtension(currentFilePath) + ".h"
                };

                if (dialog.ShowDialog() == true)
                {
                    File.WriteAllText(dialog.FileName, HeaderText);
                    StatusText = $"Saved header to {Path.GetFileName(dialog.FileName)}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save header: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText = "Failed to save header";
            }
        }

        /// <summary>
        /// Saves all files
        /// </summary>
        private void SaveAll()
        {
            try
            {
                string outputDir = Path.Combine(
                    Path.GetDirectoryName(currentFilePath) ?? "",
                    Path.GetFileNameWithoutExtension(currentFilePath));

                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                if (IsDisassembled)
                {
                    string asmFile = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(currentFilePath) + ".asm");
                    File.WriteAllText(asmFile, DisassemblyText);
                }

                if (IsDecompiled)
                {
                    string cFile = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(currentFilePath) + ".c");
                    File.WriteAllText(cFile, CCodeText);

                    string hFile = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(currentFilePath) + ".h");
                    File.WriteAllText(hFile, HeaderText);
                }

                StatusText = $"Saved all files to {outputDir}";
                HasUnsavedChanges = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save files: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText = "Failed to save files";
            }
        }

        /// <summary>
        /// Exits the application
        /// </summary>
        private void Exit()
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Disassembles the ROM
        /// </summary>
        private async void Disassemble()
        {
            await DisassembleAsync();
        }

        /// <summary>
        /// Disassembles the ROM asynchronously
        /// </summary>
        private async Task DisassembleAsync()
        {
            try
            {
                if (ActiveROM == null)
                    return;

                IsProcessing = true;
                ProcessingText = "Disassembling...";
                StatusText = "Disassembling ROM...";

                await Task.Run(() =>
                {
                    if (ActiveROM.ROMInfo == null)
                        throw new InvalidOperationException("No ROM loaded");

                    var romLoader = new ROMLoader();

                    if (string.IsNullOrEmpty(ActiveROM.FilePath) || !File.Exists(ActiveROM.FilePath))
                        throw new InvalidOperationException("ROM file not found");

                    var romInfo = romLoader.LoadFromFile(ActiveROM.FilePath);

                    if (romInfo == null)
                        throw new InvalidOperationException("Failed to load ROM info");

                    byte[] prgRomData = romLoader.GetPRGROMData();

                    if (prgRomData == null || prgRomData.Length == 0)
                        throw new InvalidOperationException("No ROM data loaded");

                    var disassembler = new Disassembler(romInfo, prgRomData);
                    disassembler.Disassemble();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ActiveROM.DisassemblyText = disassembler.ToAssemblyString();
                        DisassemblyText = ActiveROM.DisassemblyText;

                        this.disassembler = disassembler;
                        this.romInfo = romInfo;
                    });
                });

                ActiveROM.IsDisassembled = true;
                IsDisassembled = true;
                HasUnsavedChanges = true;
                StatusText = "Disassembly complete";

                UpdateProjectExplorer();

                await DecompileAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to disassemble ROM: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText = "Failed to disassemble ROM";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        /// <summary>
        /// Exports the decompiled code as a Visual Studio 2022 solution
        /// </summary>
        private void ExportToVisualStudio()
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Title = "Export to Visual Studio 2022",
                    Filter = "Visual Studio Solution (*.sln)|*.sln",
                    FileName = Path.GetFileNameWithoutExtension(currentFilePath ?? "Decompiled"),
                    OverwritePrompt = true
                };

                if (dialog.ShowDialog() == true)
                {
                    string rootPath = Path.GetDirectoryName(dialog.FileName) ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string solutionName = Path.GetFileNameWithoutExtension(dialog.FileName);

                    IsProcessing = true;
                    ProcessingText = "Exporting to Visual Studio...";
                    StatusText = "Exporting to Visual Studio...";

                    var exporter = new VisualStudioExporter(solutionName, rootPath, CCodeText, HeaderText);

                    if (exporter.ExportToVisualStudio())
                    {
                        StatusText = "Exported to Visual Studio successfully";

                        // broken for me, idk why
                        var result = MessageBox.Show(
                            "The project has been exported successfully. Would you like to open it in Visual Studio?",
                            "Export Complete",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            string solutionPath = Path.Combine(rootPath, solutionName, $"{solutionName}.sln");
                            VisualStudioExporter.OpenInVisualStudio(solutionPath);
                        }
                    }
                    else
                    {
                        StatusText = "Failed to export to Visual Studio";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to export to Visual Studio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText = "Failed to export to Visual Studio";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        /// <summary>
        /// Decompiles the ROM
        /// </summary>
        private async void Decompile()
        {
            await DecompileAsync();
        }

        /// <summary>
        /// Decompiles the ROM asynchronously
        /// </summary>
        private async Task DecompileAsync()
        {
            try
            {
                if (ActiveROM == null)
                    return;

                IsProcessing = true;
                ProcessingText = "Decompiling...";
                StatusText = "Decompiling ROM...";

                string cCodeResult = string.Empty;
                string headerTextResult = string.Empty;

                await Task.Run(() =>
                {
                    try
                    {
                        if (romInfo == null || disassembler == null)
                            throw new InvalidOperationException("No ROM disassembled");

                        decompiler = new Decompiler(romInfo, disassembler);
                        decompiler.Decompile();

                        cCodeResult = decompiler.GenerateCCode();
                        headerTextResult = GenerateHeaderFile(decompiler);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            VariableList.Clear();
                            foreach (var variable in decompiler.Variables.Values)
                            {
                                VariableList.Add(new VariableViewModel
                                {
                                    Address = variable.Address,
                                    Name = variable.Name,
                                    TypeName = variable.Type.ToString(),
                                    Size = variable.Size,
                                    IsRead = variable.IsRead,
                                    IsWritten = variable.IsWritten,
                                    Description = $"Location: {(variable.Address < 0x100 ? "Zero Page" : variable.Address < 0x800 ? "RAM" : "ROM")}"
                                });

                                ActiveROM.Variables[variable.Name] = new VariableWorkspaceData
                                {
                                    Name = variable.Name,
                                    Type = variable.Type.ToString(),
                                    Description = $"Address: 0x{variable.Address:X4}, Size: {variable.Size}"
                                };
                            }

                            FunctionList.Clear();
                            foreach (var function in decompiler.Functions.Values)
                            {
                                FunctionList.Add(new FunctionViewModel
                                {
                                    Address = function.Address,
                                    Name = function.Name,
                                    InstructionCount = function.Instructions.Count,
                                    VariableCount = function.VariablesAccessed.Count,
                                    CalledFunctionCount = function.CalledFunctions.Count,
                                    Description = $"Entry point: {(decompiler.ROMInfo.EntryPoints.Contains(function.Address) ? "Yes" : "No")}"
                                });

                                ActiveROM.Functions[function.Name] = new FunctionWorkspaceData
                                {
                                    Name = function.Name,
                                    ReturnType = "void",
                                    Parameters = new List<string>(),
                                    Description = $"Address: 0x{function.Address:X4}, Instructions: {function.Instructions.Count}"
                                };
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            throw new Exception($"Decompilation failed: {ex.Message}", ex);
                        });
                    }
                });

                CCodeText = cCodeResult;
                HeaderText = headerTextResult;

                ActiveROM.CCodeText = cCodeResult;
                ActiveROM.HeaderText = headerTextResult;
                ActiveROM.IsDecompiled = true;
                IsDecompiled = true;

                OnPropertyChanged(nameof(IsDecompiled));

                if (ProjectItems.Count > 0)
                {
                    UpdateProjectExplorer();
                }

                HasUnsavedChanges = true;
                StatusText = "Decompilation complete";

                SaveRecentWorkspaces();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to decompile ROM: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText = "Failed to decompile ROM";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        /// <summary>
        /// Generates a C header file for the decompiled ROM
        /// </summary>
        /// <param name="decompiler">The decompiler instance</param>
        /// <returns>The generated header code</returns>
        private string GenerateHeaderFile(Decompiler decompiler)
        {
            StringWriter writer = new StringWriter();

            // Add header
            writer.WriteLine("/*");
            writer.WriteLine(" * Decompiled NES ROM");
            writer.WriteLine($" * ROM: {decompiler.ROMInfo}");
            writer.WriteLine(" */");
            writer.WriteLine();

            // Include guard
            string guardName = Path.GetFileNameWithoutExtension(currentFilePath).ToUpper() + "_H";
            writer.WriteLine($"#ifndef {guardName}");
            writer.WriteLine($"#define {guardName}");
            writer.WriteLine();

            // Add includes
            writer.WriteLine("#include <stdint.h>");
            writer.WriteLine("#include <stdbool.h>");
            writer.WriteLine();

            // Add NES hardware definitions
            writer.WriteLine("// NES Hardware Registers");
            writer.WriteLine("#define PPUCTRL   (*((volatile uint8_t*)0x2000))");
            writer.WriteLine("#define PPUMASK   (*((volatile uint8_t*)0x2001))");
            writer.WriteLine("#define PPUSTATUS (*((volatile uint8_t*)0x2002))");
            writer.WriteLine("#define OAMADDR   (*((volatile uint8_t*)0x2003))");
            writer.WriteLine("#define OAMDATA   (*((volatile uint8_t*)0x2004))");
            writer.WriteLine("#define PPUSCROLL (*((volatile uint8_t*)0x2005))");
            writer.WriteLine("#define PPUADDR   (*((volatile uint8_t*)0x2006))");
            writer.WriteLine("#define PPUDATA   (*((volatile uint8_t*)0x2007))");
            writer.WriteLine("#define OAMDMA    (*((volatile uint8_t*)0x4014))");
            writer.WriteLine("#define SND_CHN   (*((volatile uint8_t*)0x4015))");
            writer.WriteLine("#define JOY1      (*((volatile uint8_t*)0x4016))");
            writer.WriteLine("#define JOY2      (*((volatile uint8_t*)0x4017))");
            writer.WriteLine();

            // Add external variable declarations
            writer.WriteLine("// Variables");
            foreach (var variable in decompiler.Variables.Values)
            {
                if (variable.Address < 0x2000 || variable.Address >= 0x8000)
                {
                    if (variable.Type == VariableType.Array)
                    {
                        var cType = variable.GetCType();
                        var baseType = cType.Contains("[") ? cType.Substring(0, cType.IndexOf("[")) : cType;
                        writer.WriteLine($"extern {baseType} {variable.Name}[256]; // Size estimated");
                    }

                    else
                    {
                        writer.WriteLine($"extern {variable.GetCType()} {variable.Name};");
                    }
                }
            }
            writer.WriteLine();

            // Add function declarations
            writer.WriteLine("// Functions");
            foreach (var function in decompiler.Functions.Values)
            {
                writer.WriteLine($"void {function.Name}();");
            }
            writer.WriteLine();

            // Close include guard
            writer.WriteLine($"#endif // {guardName}");

            return writer.ToString();
        }

        /// <summary>
        /// Views the ROM information
        /// </summary>
        private void ViewROMInfo()
        {
            try
            {
                if (romInfo == null)
                    return;

                string info = romInfo.ToString();
                MessageBox.Show(info, "ROM Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to view ROM info: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Views the variables
        /// </summary>
        private void ViewVariables()
        {
            try
            {
                if (decompiler == null)
                    return;

                VariableList.Clear();

                foreach (var variable in decompiler.Variables.Values)
                {
                    VariableList.Add(new VariableViewModel
                    {
                        Address = variable.Address,
                        Name = variable.Name,
                        TypeName = variable.Type.ToString(),
                        Size = variable.Size,
                        IsRead = variable.IsRead,
                        IsWritten = variable.IsWritten,
                        Description = $"Location: {(variable.Address < 0x100 ? "Zero Page" : variable.Address < 0x800 ? "RAM" : "ROM")}"
                    });
                }

                ActivateTab("Variables");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to view variables: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Views the functions
        /// </summary>
        private void ViewFunctions()
        {
            try
            {
                if (decompiler == null)
                    return;

                FunctionList.Clear();

                foreach (var function in decompiler.Functions.Values)
                {
                    FunctionList.Add(new FunctionViewModel
                    {
                        Address = function.Address,
                        Name = function.Name,
                        InstructionCount = function.Instructions.Count,
                        VariableCount = function.VariablesAccessed.Count,
                        CalledFunctionCount = function.CalledFunctions.Count,
                        Description = $"Entry point: {(decompiler.ROMInfo.EntryPoints.Contains(function.Address) ? "Yes" : "No")}"
                    });
                }

                ActivateTab("Functions");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to view functions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Shows the about dialog
        /// </summary>
        private void ShowAbout()
        {
            MessageBox.Show(
                "NES Decompiler\n" +
                "==============\n\n" +
                "A static decompiler for NES/6502 ROMs.\n\n" +
                "Created with <3 by ApfelTeeSaft",
                "About NES Decompiler",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        /// <summary>
        /// Activate a Tab
        /// </summary>
        private void ActivateTab(string tabName)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.ActivateTab(tabName);
            }
        }
        private void JumpToVariableDefinition()
        {
            if (SelectedVariable == null)
                return;

            string variableName = SelectedVariable.Name;
            string pattern = $"[\\s\\n\\r]({variableName})[\\s\\n\\r;]";

            FindAndHighlightInCode(pattern, true, "cCodeEditor");
        }

        private void FindVariableReferences()
        {
            if (SelectedVariable == null)
                return;

            string variableName = SelectedVariable.Name;
            string pattern = $"[\\s\\n\\r]({variableName})[\\s\\n\\r;]";

            FindAllOccurrencesInCode(pattern, "Variable References", "cCodeEditor");
        }

        private void RenameVariable()
        {
            if (SelectedVariable == null)
                return;

            string oldName = SelectedVariable.Name;

            var dialog = new InputDialog("Rename Variable", "Enter new name:", oldName);

            if (dialog.ShowDialog() == true)
            {
                string newName = dialog.ResponseText;

                if (string.IsNullOrWhiteSpace(newName))
                {
                    MessageBox.Show("Variable name cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                SelectedVariable.Name = newName;

                ReplaceAllInCode(oldName, newName, "cCodeEditor");

                ReplaceAllInCode(oldName, newName, "headerEditor");

                HasUnsavedChanges = true;

                if (decompiler != null && decompiler.Variables.ContainsKey(SelectedVariable.Address))
                {
                    decompiler.Variables[SelectedVariable.Address].Name = newName;
                }

                // Save workspace
                SaveRecentFiles();
            }
        }

        private void JumpToFunctionDefinition()
        {
            if (SelectedFunction == null)
                return;

            string functionName = SelectedFunction.Name;
            string pattern = $"void[\\s\\n\\r]+({functionName})[\\s\\n\\r]*\\(";

            FindAndHighlightInCode(pattern, true, "cCodeEditor");
        }

        private void FindFunctionReferences()
        {
            if (SelectedFunction == null)
                return;

            string functionName = SelectedFunction.Name;
            string pattern = $"[\\s\\n\\r]({functionName})\\(";

            FindAllOccurrencesInCode(pattern, "Function References", "cCodeEditor");
        }

        private void RenameFunction()
        {
            if (SelectedFunction == null)
                return;

            string oldName = SelectedFunction.Name;

            var dialog = new InputDialog("Rename Function", "Enter new name:", oldName);

            if (dialog.ShowDialog() == true)
            {
                string newName = dialog.ResponseText;

                if (string.IsNullOrWhiteSpace(newName))
                {
                    MessageBox.Show("Function name cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                SelectedFunction.Name = newName;

                ReplaceAllInCode(oldName, newName, "cCodeEditor");

                ReplaceAllInCode(oldName, newName, "headerEditor");

                HasUnsavedChanges = true;

                if (decompiler != null && decompiler.Functions.ContainsKey(SelectedFunction.Address))
                {
                    decompiler.Functions[SelectedFunction.Address].Name = newName;
                }

                SaveRecentFiles();
            }
        }

        private void SearchVariables()
        {
            if (string.IsNullOrEmpty(VariableSearchText))
                return;

            var filteredList = new ObservableCollection<VariableViewModel>(
                decompiler?.Variables.Values
                    .Where(v => v.Name.Contains(VariableSearchText, StringComparison.OrdinalIgnoreCase))
                    .Select(v => new VariableViewModel
                    {
                        Address = v.Address,
                        Name = v.Name,
                        TypeName = v.Type.ToString(),
                        Size = v.Size,
                        IsRead = v.IsRead,
                        IsWritten = v.IsWritten,
                        Description = $"Location: {(v.Address < 0x100 ? "Zero Page" : v.Address < 0x800 ? "RAM" : "ROM")}"
                    })
                    ?? Array.Empty<VariableViewModel>());

            VariableList = filteredList;
        }

        private void SearchFunctions()
        {
            if (string.IsNullOrEmpty(FunctionSearchText))
                return;

            var filteredList = new ObservableCollection<FunctionViewModel>(
                decompiler?.Functions.Values
                    .Where(f => f.Name.Contains(FunctionSearchText, StringComparison.OrdinalIgnoreCase))
                    .Select(f => new FunctionViewModel
                    {
                        Address = f.Address,
                        Name = f.Name,
                        InstructionCount = f.Instructions.Count,
                        VariableCount = f.VariablesAccessed.Count,
                        CalledFunctionCount = f.CalledFunctions.Count,
                        Description = $"Entry point: {(decompiler.ROMInfo.EntryPoints.Contains(f.Address) ? "Yes" : "No")}"
                    })
                    ?? Array.Empty<FunctionViewModel>());

            FunctionList = filteredList;
        }

        private void FindAndHighlightInCode(string pattern, bool useRegex, string editorName)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow == null)
                return;

            var editor = mainWindow.GetType().GetField(editorName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(mainWindow) as ICSharpCode.AvalonEdit.TextEditor;
            if (editor == null)
                return;

            if (editorName == "cCodeEditor")
                ActivateTab("C Code");
            else if (editorName == "headerEditor")
                ActivateTab("Header");
            else if (editorName == "disassemblyEditor")
                ActivateTab("Disassembly");

            string text = editor.Text;
            int startIndex = 0;

            System.Text.RegularExpressions.Match match;
            if (useRegex)
            {
                var regex = new System.Text.RegularExpressions.Regex(pattern);
                match = regex.Match(text, startIndex);
            }
            else
            {
                int index = text.IndexOf(pattern, startIndex, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    match = System.Text.RegularExpressions.Regex.Match(text, pattern,
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase,
                        System.TimeSpan.FromSeconds(1));
                }
                else
                {
                    match = System.Text.RegularExpressions.Match.Empty;
                }
            }

            if (match.Success)
            {
                editor.Select(match.Index, match.Length);
                editor.ScrollToLine(editor.Document.GetLineByOffset(match.Index).LineNumber);
            }
            else
            {
                MessageBox.Show($"Could not find the pattern in the code.", "Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void FindAllOccurrencesInCode(string pattern, string title, string editorName)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow == null)
                return;

            var editor = mainWindow.GetType().GetField(editorName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(mainWindow) as ICSharpCode.AvalonEdit.TextEditor;
            if (editor == null)
                return;

            string text = editor.Text;

            var regex = new System.Text.RegularExpressions.Regex(pattern);
            var matches = regex.Matches(text);

            if (matches.Count > 0)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"Found {matches.Count} occurrences:");
                sb.AppendLine();

                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    int lineNumber = editor.Document.GetLineByOffset(match.Index).LineNumber;
                    var docLine = editor.Document.GetLineByNumber(lineNumber);
                    string line = editor.Document.GetText(docLine.Offset, docLine.Length).Trim();
                    sb.AppendLine($"Line {lineNumber}: {line}");
                }

                MessageBox.Show(sb.ToString(), title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"No occurrences found.", title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ReplaceAllInCode(string oldText, string newText, string editorName)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow == null)
                return;

            var editor = mainWindow.GetType().GetField(editorName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(mainWindow) as ICSharpCode.AvalonEdit.TextEditor;
            if (editor == null)
                return;

            string text = editor.Text;

            var regex = new System.Text.RegularExpressions.Regex($"\\b{System.Text.RegularExpressions.Regex.Escape(oldText)}\\b");
            string newContent = regex.Replace(text, newText);

            if (text != newContent)
            {
                editor.Text = newContent;

                if (editorName == "cCodeEditor")
                    CCodeText = newContent;
                else if (editorName == "headerEditor")
                    HeaderText = newContent;
                else if (editorName == "disassemblyEditor")
                    DisassemblyText = newContent;
            }
        }

        private void SaveWorkspace()
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Title = "Save Workspace",
                    Filter = "NES Decompiler Workspace (*.nesworkspace)|*.nesworkspace",
                    DefaultExt = ".nesworkspace",
                    FileName = Path.GetFileNameWithoutExtension(currentFilePath) + ".nesworkspace"
                };

                if (dialog.ShowDialog() == true)
                {
                    string workspacePath = dialog.FileName;

                    var workspace = new WorkspaceFile
                    {
                        CurrentFilePath = currentFilePath,
                        RecentFiles = recentFiles,
                        IsDisassembled = isDisassembled,
                        IsDecompiled = isDecompiled
                    };

                    if (decompiler != null)
                    {
                        foreach (var variable in decompiler.Variables.Values)
                        {
                            workspace.Variables[variable.Name] = new VariableWorkspaceData
                            {
                                Name = variable.Name,
                                Type = variable.Type.ToString(),
                                Description = $"Address: 0x{variable.Address:X4}, Size: {variable.Size}"
                            };
                        }

                        foreach (var function in decompiler.Functions.Values)
                        {
                            workspace.Functions[function.Name] = new FunctionWorkspaceData
                            {
                                Name = function.Name,
                                ReturnType = "void",
                                Parameters = new List<string>(),
                                Description = $"Address: 0x{function.Address:X4}, Instructions: {function.Instructions.Count}"
                            };
                        }
                    }

                    string json = System.Text.Json.JsonSerializer.Serialize(workspace, new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                    File.WriteAllText(workspacePath, json);

                    AddRecentWorkspace(workspacePath);

                    StatusText = $"Workspace saved to {Path.GetFileName(workspacePath)}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save workspace: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText = "Failed to save workspace";
            }
        }

        private void SaveWorkspaceToFile(string filePath)
        {
            try
            {
                var workspace = new WorkspaceFile
                {
                    CurrentFilePath = currentFilePath,
                    RecentFiles = recentFiles,
                    IsDisassembled = isDisassembled,
                    IsDecompiled = isDecompiled
                };

                if (decompiler != null)
                {
                    foreach (var variable in decompiler.Variables.Values)
                    {
                        workspace.Variables[variable.Name] = new VariableWorkspaceData
                        {
                            Name = variable.Name,
                            Type = variable.Type.ToString(),
                            Description = $"Address: 0x{variable.Address:X4}, Size: {variable.Size}"
                        };
                    }

                    foreach (var function in decompiler.Functions.Values)
                    {
                        workspace.Functions[function.Name] = new FunctionWorkspaceData
                        {
                            Name = function.Name,
                            ReturnType = "void",
                            Parameters = new List<string>(),
                            Description = $"Address: 0x{function.Address:X4}, Instructions: {function.Instructions.Count}"
                        };
                    }
                }

                string json = System.Text.Json.JsonSerializer.Serialize(workspace, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(filePath, json);

                AddRecentWorkspace(filePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save workspace to file: {ex.Message}", ex);
            }
        }

        private void NewProject()
        {
            if (HasUnsavedChanges)
            {
                var result = MessageBox.Show(
                    "You have unsaved changes. Do you want to save before creating a new project?",
                    "Save Changes",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel)
                    return;

                if (result == MessageBoxResult.Yes)
                    SaveProject();
            }

            CurrentProject = new Project
            {
                Name = "New Project",
                FilePath = "",
                LastModified = DateTime.Now
            };

            ActiveROM = null;
            IsROMLoaded = false;
            IsDisassembled = false;
            IsDecompiled = false;
            DisassemblyText = "";
            CCodeText = "";
            HeaderText = "";
            HasUnsavedChanges = false;

            UpdateProjectExplorer();

            StatusText = "New project created";
        }

        private void OpenProject()
        {
            try
            {
                if (HasUnsavedChanges)
                {
                    var result = MessageBox.Show(
                        "You have unsaved changes. Do you want to save before opening another project?",
                        "Save Changes",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Cancel)
                        return;

                    if (result == MessageBoxResult.Yes)
                        SaveProject();
                }

                var dialog = new OpenFileDialog
                {
                    Title = "Open Project",
                    Filter = "NES Decompiler Project (*.nesproj)|*.nesproj",
                    DefaultExt = ".nesproj"
                };

                if (dialog.ShowDialog() == true)
                {
                    LoadProject(dialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open project: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText = "Failed to open project";
            }
        }

        private void SaveProject()
        {
            try
            {
                if (CurrentProject == null)
                    return;

                if (string.IsNullOrEmpty(CurrentProject.FilePath))
                {
                    SaveProjectAs();
                    return;
                }

                SaveProjectToFile(CurrentProject.FilePath);
                StatusText = $"Project saved to {Path.GetFileName(CurrentProject.FilePath)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save project: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText = "Failed to save project";
            }
        }

        private void SaveProjectAs()
        {
            try
            {
                if (CurrentProject == null)
                    return;

                var dialog = new SaveFileDialog
                {
                    Title = "Save Project As",
                    Filter = "NES Decompiler Project (*.nesproj)|*.nesproj",
                    DefaultExt = ".nesproj",
                    FileName = string.IsNullOrEmpty(CurrentProject.Name) ? "New Project" : CurrentProject.Name
                };

                if (dialog.ShowDialog() == true)
                {
                    CurrentProject.FilePath = dialog.FileName;
                    CurrentProject.Name = Path.GetFileNameWithoutExtension(dialog.FileName);

                    SaveProjectToFile(CurrentProject.FilePath);

                    UpdateProjectExplorer();

                    StatusText = $"Project saved to {Path.GetFileName(CurrentProject.FilePath)}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save project: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText = "Failed to save project";
            }
        }

        private void AddROM()
        {
            OpenROM();
        }

        private void RemoveROM()
        {
            try
            {
                if (ActiveROM == null || CurrentProject == null)
                    return;

                var result = MessageBox.Show(
                    $"Are you sure you want to remove '{ActiveROM.Name}' from the project?",
                    "Remove ROM",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    CurrentProject.ROMs.Remove(ActiveROM);

                    ActiveROM = CurrentProject.ROMs.FirstOrDefault();

                    UpdateProjectExplorer();

                    StatusText = "ROM removed from project";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to remove ROM: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText = "Failed to remove ROM";
            }
        }

        private void SaveProjectToFile(string filePath)
        {
            try
            {
                if (CurrentProject == null)
                    return;

                CurrentProject.LastModified = DateTime.Now;

                string json = System.Text.Json.JsonSerializer.Serialize(CurrentProject, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(filePath, json);

                HasUnsavedChanges = false;

                AddRecentProject(filePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save project to file: {ex.Message}", ex);
            }
        }

        private void LoadProject(string filePath)
        {
            try
            {
                IsProcessing = true;
                ProcessingText = "Loading project...";
                StatusText = $"Loading {Path.GetFileName(filePath)}...";

                string json = File.ReadAllText(filePath);
                var project = System.Text.Json.JsonSerializer.Deserialize<Project>(json);

                if (project == null)
                    throw new Exception("Invalid project file");

                CurrentProject = project;

                foreach (var rom in CurrentProject.ROMs.ToList())
                {
                    if (!File.Exists(rom.FilePath))
                    {
                        var result = MessageBox.Show(
                            $"ROM file '{rom.FilePath}' could not be found. Would you like to remove it from the project?",
                            "ROM File Not Found",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            CurrentProject.ROMs.Remove(rom);
                        }
                    }
                }

                ActiveROM = CurrentProject.ROMs.FirstOrDefault();

                UpdateProjectExplorer();

                AddRecentProject(filePath);

                StatusText = $"Project '{CurrentProject.Name}' loaded";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load project: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText = "Failed to load project";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        /// <summary>
        /// Saves recent workspaces to application settings
        /// </summary>
        private void SaveRecentWorkspacesToSettings()
        {
            try
            {
                string appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "NESDecompiler");

                if (!Directory.Exists(appDataPath))
                    Directory.CreateDirectory(appDataPath);

                string filePath = Path.Combine(appDataPath, "recentworkspaces.json");

                string json = System.Text.Json.JsonSerializer.Serialize(RecentWorkspaces);

                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving recent workspaces: {ex.Message}");
            }
        }

        /// <summary>
        /// Opens a recent workspace file
        /// </summary>
        /// <param name="filePath">The workspace file path to open</param>
        public void OpenRecentWorkspace(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    MessageBox.Show($"The workspace file '{filePath}' could not be found.",
                        "File Not Found",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    var updatedList = new List<string>(RecentWorkspaces);
                    updatedList.Remove(filePath);
                    RecentWorkspaces = updatedList;
                    SaveRecentWorkspacesToSettings();
                    return;
                }

                LoadWorkspaceFile(filePath);

                AddRecentWorkspace(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open workspace: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                StatusText = "Failed to open workspace";
            }
        }

        /// <summary>
        /// Loads a workspace file
        /// </summary>
        /// <param name="filePath">The workspace file path to load</param>
        private void LoadWorkspaceFile(string filePath)
        {
            try
            {
                IsProcessing = true;
                ProcessingText = "Loading workspace...";
                StatusText = $"Loading {Path.GetFileName(filePath)}...";

                string json = File.ReadAllText(filePath);
                var workspace = System.Text.Json.JsonSerializer.Deserialize<WorkspaceFile>(json);

                if (workspace == null)
                    throw new Exception("Invalid workspace file");

                if (!string.IsNullOrEmpty(workspace.CurrentFilePath) && File.Exists(workspace.CurrentFilePath))
                {
                    Task.Run(() => Application.Current.Dispatcher.Invoke(async () =>
                        await LoadROMFile(workspace.CurrentFilePath)
                    )).Wait();

                    if (IsROMLoaded)
                    {
                        IsDisassembled = workspace.IsDisassembled;
                        IsDecompiled = workspace.IsDecompiled;

                        if (IsDecompiled && decompiler != null)
                        {
                            foreach (var varEntry in workspace.Variables)
                            {
                                var variable = decompiler.Variables.Values.FirstOrDefault(v => v.Name == varEntry.Key);
                                if (variable != null)
                                {
                                    variable.Name = varEntry.Value.Name;
                                    if (Enum.TryParse<VariableType>(varEntry.Value.Type, out var type))
                                    {
                                        variable.Type = type;
                                    }
                                }
                            }

                            foreach (var funcEntry in workspace.Functions)
                            {
                                var function = decompiler.Functions.Values.FirstOrDefault(f => f.Name == funcEntry.Key);
                                if (function != null)
                                {
                                    function.Name = funcEntry.Value.Name;
                                }
                            }

                            VariableList.Clear();
                            foreach (var variable in decompiler.Variables.Values)
                            {
                                VariableList.Add(new VariableViewModel
                                {
                                    Address = variable.Address,
                                    Name = variable.Name,
                                    TypeName = variable.Type.ToString(),
                                    Size = variable.Size,
                                    IsRead = variable.IsRead,
                                    IsWritten = variable.IsWritten,
                                    Description = workspace.Variables.TryGetValue(variable.Name, out var varData) ?
                                        varData.Description :
                                        $"Location: {(variable.Address < 0x100 ? "Zero Page" : variable.Address < 0x800 ? "RAM" : "ROM")}"
                                });
                            }

                            FunctionList.Clear();
                            foreach (var function in decompiler.Functions.Values)
                            {
                                FunctionList.Add(new FunctionViewModel
                                {
                                    Address = function.Address,
                                    Name = function.Name,
                                    InstructionCount = function.Instructions.Count,
                                    VariableCount = function.VariablesAccessed.Count,
                                    CalledFunctionCount = function.CalledFunctions.Count,
                                    Description = workspace.Functions.TryGetValue(function.Name, out var funcData) ?
                                        funcData.Description :
                                        $"Entry point: {(romInfo?.EntryPoints.Contains(function.Address) ?? false ? "Yes" : "No")}"
                                });
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show($"The ROM file '{workspace.CurrentFilePath}' referenced in the workspace could not be found.",
                        "ROM File Not Found",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }

                if (workspace.RecentFiles != null)
                {
                    RecentFiles = workspace.RecentFiles.Where(File.Exists).ToList();
                    SaveRecentFiles();
                }

                StatusText = $"Workspace '{Path.GetFileName(filePath)}' loaded";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load workspace: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                StatusText = "Failed to load workspace";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        /// <summary>
        /// Adds a project to the recent projects list
        /// </summary>
        /// <param name="filePath">The project file path to add</param>
        private void AddRecentProject(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return;

                string projectName = (CurrentProject != null && CurrentProject.FilePath == filePath) ?
                    CurrentProject.Name : Path.GetFileNameWithoutExtension(filePath);

                var projectInfo = new RecentProjectInfo
                {
                    FilePath = filePath,
                    Name = projectName,
                    LastOpened = DateTime.Now
                };

                var updatedList = new List<RecentProjectInfo>(RecentProjectsList);

                updatedList.RemoveAll(p => p.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));

                updatedList.Insert(0, projectInfo);

                if (updatedList.Count > MAX_RECENT_ITEMS)
                    updatedList = updatedList.Take(MAX_RECENT_ITEMS).ToList();

                RecentProjectsList = updatedList;

                SaveRecentProjectsToSettings();

                StatusText = $"Added '{projectName}' to recent projects";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding recent project: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads recent projects from application settings
        /// </summary>
        private void LoadRecentProjects()
        {
            try
            {
                string appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "NESDecompiler");

                string filePath = Path.Combine(appDataPath, "recentprojects.json");

                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var projects = System.Text.Json.JsonSerializer.Deserialize<List<RecentProjectInfo>>(json);

                    if (projects != null)
                    {
                        RecentProjectsList = projects.Where(p => File.Exists(p.FilePath)).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading recent projects: {ex.Message}");
                RecentProjectsList = new List<RecentProjectInfo>();
            }
        }

        /// <summary>
        /// Saves recent projects to application settings
        /// </summary>
        private void SaveRecentProjectsToSettings()
        {
            try
            {
                string appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "NESDecompiler");

                if (!Directory.Exists(appDataPath))
                    Directory.CreateDirectory(appDataPath);

                string filePath = Path.Combine(appDataPath, "recentprojects.json");

                string json = System.Text.Json.JsonSerializer.Serialize(RecentProjectsList);

                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving recent projects: {ex.Message}");
            }
        }

        /// <summary>
        /// Opens a recent project
        /// </summary>
        /// <param name="projectInfo">The project info to open</param>
        public void OpenRecentProject(RecentProjectInfo projectInfo)
        {
            try
            {
                if (projectInfo == null)
                    return;

                if (!File.Exists(projectInfo.FilePath))
                {
                    MessageBox.Show($"The project file '{projectInfo.FilePath}' could not be found.",
                        "File Not Found",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    var updatedList = new List<RecentProjectInfo>(RecentProjectsList);
                    updatedList.RemoveAll(p => p.FilePath.Equals(projectInfo.FilePath, StringComparison.OrdinalIgnoreCase));
                    RecentProjectsList = updatedList;
                    SaveRecentProjectsToSettings();
                    return;
                }

                if (HasUnsavedChanges)
                {
                    var result = MessageBox.Show(
                        "You have unsaved changes. Do you want to save before opening another project?",
                        "Save Changes",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Cancel)
                        return;

                    if (result == MessageBoxResult.Yes)
                        SaveProject();
                }

                LoadProject(projectInfo.FilePath);

                projectInfo.LastOpened = DateTime.Now;

                AddRecentProject(projectInfo.FilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open project: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                StatusText = "Failed to open project";
            }
        }

        /// <summary>
        /// Saves the list of recent workspaces
        /// </summary>
        private void SaveRecentWorkspaces()
        {
            try
            {
                string appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "NESDecompiler");

                if (!Directory.Exists(appDataPath))
                    Directory.CreateDirectory(appDataPath);

                string filePath = Path.Combine(appDataPath, "recentworkspaces.json");

                string json = System.Text.Json.JsonSerializer.Serialize(RecentWorkspaces);

                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save recent workspaces: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Adds a workspace file to the recent workspaces list
        /// </summary>
        /// <param name="filePath">The workspace file path to add</param>
        private void AddRecentWorkspace(string filePath)
        {
            try
            {
                var updatedList = new List<string>(RecentWorkspaces);
                updatedList.RemoveAll(p => p.Equals(filePath, StringComparison.OrdinalIgnoreCase));

                updatedList.Insert(0, filePath);

                if (updatedList.Count > MAX_RECENT_ITEMS)
                    updatedList = updatedList.Take(MAX_RECENT_ITEMS).ToList();

                RecentWorkspaces = updatedList;

                SaveRecentWorkspaces();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add recent workspace: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Loads the list of recent workspaces
        /// </summary>
        private void LoadRecentWorkspaces()
        {
            try
            {
                string appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "NESDecompiler");

                string filePath = Path.Combine(appDataPath, "recentworkspaces.json");

                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var workspaces = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json);

                    if (workspaces != null)
                    {
                        RecentWorkspaces = workspaces.Where(File.Exists).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load recent workspaces: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                RecentWorkspaces = new List<string>();
            }
        }

        /// <summary>
        /// Toggles between dark and light theme
        /// </summary>
        private void ToggleTheme()
        {
            ThemeManager.Instance.ToggleTheme();
            OnPropertyChanged(nameof(IsDarkTheme));
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}