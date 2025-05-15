using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace NESDecompiler.GUI.Themes
{
    public class ThemeManager
    {
        private static ThemeManager _instance;
        public static ThemeManager Instance => _instance ??= new ThemeManager();

        private readonly Dictionary<string, object> _themeResources = new Dictionary<string, object>();

        private ThemeManager()
        {
            IsDarkTheme = true;
        }

        public bool IsDarkTheme { get; private set; }

        public void ToggleTheme()
        {
            IsDarkTheme = !IsDarkTheme;
            ApplyTheme();
        }

        public void SetTheme(bool isDarkTheme)
        {
            if (IsDarkTheme != isDarkTheme)
            {
                IsDarkTheme = isDarkTheme;
                ApplyTheme();
            }
        }

        public void ApplyTheme()
        {
            try
            {
                var appResources = Application.Current.Resources.MergedDictionaries;

                _themeResources.Clear();

                for (int i = appResources.Count - 1; i >= 0; i--)
                {
                    var resourceUri = appResources[i].Source?.ToString();
                    if (resourceUri != null &&
                        (resourceUri.Contains("DarkTheme") || resourceUri.Contains("LightTheme")))
                    {
                        appResources.RemoveAt(i);
                    }
                }

                var themeDictionary = new ResourceDictionary
                {
                    Source = new Uri(
                        $"pack://application:,,,/Resources/Themes/{(IsDarkTheme ? "Dark" : "Light")}Theme.xaml",
                        UriKind.Absolute)
                };

                appResources.Insert(0, themeDictionary);

                UpdateResourcesExplicitly(themeDictionary);

                ThemeChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying theme: {ex.Message}", "Theme Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateResourcesExplicitly(ResourceDictionary themeDictionary)
        {
            UpdateResourceIfExists(themeDictionary, "PrimaryBackgroundBrush");
            UpdateResourceIfExists(themeDictionary, "SecondaryBackgroundBrush");
            UpdateResourceIfExists(themeDictionary, "TertiaryBackgroundBrush");
            UpdateResourceIfExists(themeDictionary, "PrimaryForegroundBrush");
            UpdateResourceIfExists(themeDictionary, "SecondaryForegroundBrush");
            UpdateResourceIfExists(themeDictionary, "AccentBrush");
            UpdateResourceIfExists(themeDictionary, "BorderBrush");
        }

        private void UpdateResourceIfExists(ResourceDictionary dictionary, string key)
        {
            if (dictionary.Contains(key))
            {
                _themeResources[key] = dictionary[key];

                Application.Current.Resources[key] = dictionary[key];
            }
        }

        public object GetResource(string key)
        {
            if (_themeResources.TryGetValue(key, out var resource))
            {
                return resource;
            }

            return Application.Current.Resources[key];
        }

        public event EventHandler ThemeChanged;
    }
}