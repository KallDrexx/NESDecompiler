using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using NESDecompiler.GUI.ViewModels;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Xml;
using System.IO;
using System.Reflection;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using static NESDecompiler.GUI.ViewModels.ProjectItemViewModel;
using System.Windows.Media;
using NESDecompiler.GUI.Themes;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit;

namespace NESDecompiler.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();

            viewModel = new MainViewModel();
            DataContext = viewModel;

            LoadSyntaxHighlighting();

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        /// <summary>
        /// Loads custom syntax highlighting for ASM 6502
        /// </summary>
        private void LoadSyntaxHighlighting()
        {
            try
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NESDecompiler.GUI.Resources.ASM6502.xshd"))
                {
                    if (stream != null)
                    {
                        using (var reader = new XmlTextReader(stream))
                        {
                            var highlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                            HighlightingManager.Instance.RegisterHighlighting("ASM6502", new[] { ".asm" }, highlighting);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load syntax highlighting: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel.Initialize();

            if (viewModel is INotifyPropertyChanged notifyViewModel)
            {
                notifyViewModel.PropertyChanged += ViewModel_PropertyChanged;
            }

            cCodeEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
            headerEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");

            try
            {
                disassemblyEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("ASM6502");
            }
            catch (Exception)
            {
                disassemblyEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XML");
            }
            ThemeManager.Instance.ThemeChanged += (s, args) => ApplyThemeToEditors();
            ApplyThemeToEditors();
            SetupEditorEvents();
        }

        private void ApplyThemeToEditors()
        {
            bool isDarkTheme = ThemeManager.Instance.IsDarkTheme;

            SolidColorBrush backgroundBrush = Application.Current.Resources["SecondaryBackgroundBrush"] as SolidColorBrush;
            SolidColorBrush foregroundBrush = Application.Current.Resources["PrimaryForegroundBrush"] as SolidColorBrush;

            if (backgroundBrush != null && foregroundBrush != null)
            {
                ApplyThemeToEditor(disassemblyEditor, backgroundBrush, foregroundBrush);
                ApplyThemeToEditor(cCodeEditor, backgroundBrush, foregroundBrush);
                ApplyThemeToEditor(headerEditor, backgroundBrush, foregroundBrush);
            }
        }

        private void ApplyThemeToEditor(TextEditor editor, SolidColorBrush background, SolidColorBrush foreground)
        {
            if (editor != null)
            {
                editor.Background = background;
                editor.Foreground = foreground;
            }
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.DisassemblyText))
            {
                disassemblyEditor.Text = viewModel.DisassemblyText;
            }
            else if (e.PropertyName == nameof(MainViewModel.CCodeText))
            {
                cCodeEditor.Text = viewModel.CCodeText;
            }
            else if (e.PropertyName == nameof(MainViewModel.HeaderText))
            {
                headerEditor.Text = viewModel.HeaderText;
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (viewModel is INotifyPropertyChanged notifyViewModel)
            {
                notifyViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }

            if (viewModel.HasUnsavedChanges)
            {
                var result = MessageBox.Show(
                    "You have unsaved changes. Do you want to save before exiting?",
                    "Save Changes",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }

                if (result == MessageBoxResult.Yes)
                {
                    viewModel.SaveAllCommand.Execute(null);
                }
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var selectedItem = e.NewValue as ProjectItemViewModel;

            var previousSelection = viewModel.SelectedProjectItem;

            viewModel.SelectedProjectItem = selectedItem;

            if (selectedItem != null)
            {
                if (sender is TreeView treeView)
                {
                    if (selectedItem.Tag is ROMProject currentRom && previousSelection?.Tag is ROMProject prevRom)
                    {
                        currentRom.IsExpanded = true;
                        prevRom.IsExpanded = true;

                        KeepTreeViewItemsExpanded(treeView);
                    }
                }

                if (selectedItem.Tag is ROMProject selectedRom)
                {
                    if (viewModel.ActiveROM != selectedRom)
                    {
                        viewModel.ActiveROM = selectedRom;
                    }
                }

                switch (selectedItem.Type)
                {
                    case ProjectItemType.Disassembly:
                        ActivateTab("Disassembly");
                        break;
                    case ProjectItemType.CCode:
                        ActivateTab("C Code");
                        break;
                    case ProjectItemType.Header:
                        ActivateTab("Header");
                        break;
                    case ProjectItemType.Variables:
                        viewModel.ViewVariablesCommand.Execute(null);
                        break;
                    case ProjectItemType.Functions:
                        viewModel.ViewFunctionsCommand.Execute(null);
                        break;
                    case ProjectItemType.ROMInfo:
                        viewModel.ViewROMInfoCommand.Execute(null);
                        break;
                    default:
                        break;
                }
            }
        }

        private void KeepTreeViewItemsExpanded(TreeView treeView)
        {
            var expandedItems = FindExpandedItems(treeView);

            foreach (var item in expandedItems)
            {
                item.IsExpanded = true;
            }
        }

        private List<TreeViewItem> FindExpandedItems(DependencyObject parent)
        {
            var result = new List<TreeViewItem>();

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is TreeViewItem item && item.IsExpanded)
                {
                    result.Add(item);
                }

                result.AddRange(FindExpandedItems(child));
            }

            return result;
        }

        private void RecentFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem is string filePath)
            {
                viewModel.OpenRecentFile(filePath);
            }
        }

        private void RecentProjects_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListView listView && listView.SelectedItem is RecentProjectInfo projectInfo)
            {
                viewModel.OpenRecentProject(projectInfo);
            }
        }

        private void RecentWorkspaces_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListView listView && listView.SelectedItem is string filePath)
            {
                viewModel.OpenRecentWorkspace(filePath);
            }
        }

        public void ActivateTab(string tabName)
        {
            foreach (TabItem tab in mainTabControl.Items)
            {
                if (tab.Name == tabName + "Tab" || tab.Header.ToString() == tabName)
                {
                    tab.IsSelected = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Set up text change event handlers for editor controls
        /// </summary>
        private void SetupEditorEvents()
        {
            disassemblyEditor.TextChanged += (s, e) => {
                if (viewModel.DisassemblyText != disassemblyEditor.Text)
                {
                    viewModel.DisassemblyText = disassemblyEditor.Text;
                }
            };

            cCodeEditor.TextChanged += (s, e) => {
                if (viewModel.CCodeText != cCodeEditor.Text)
                {
                    viewModel.CCodeText = cCodeEditor.Text;
                }
            };

            headerEditor.TextChanged += (s, e) => {
                if (viewModel.HeaderText != headerEditor.Text)
                {
                    viewModel.HeaderText = headerEditor.Text;
                }
            };
        }
    }
}