using DBCD;
using DBDefsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WDBXEditor2.Misc;
using static DBDefsLib.Structs;

namespace WDBXEditor2.Views
{
    public partial class DefinitionSelect : Window
    {
        private List<DefinitionSelectData> DefinitionSelectData = new List<DefinitionSelectData>();

        private List<LocalSelectData> LocalSelectData = new List<LocalSelectData>()
        {
            new LocalSelectData() { DisplayName = "enUS", Locale = Locale.EnUS },
            new LocalSelectData() { DisplayName = "enGB", Locale = Locale.EnGB },
            new LocalSelectData() { DisplayName = "koKR", Locale = Locale.KoKR },
            new LocalSelectData() { DisplayName = "frFR", Locale = Locale.FrFR },
            new LocalSelectData() { DisplayName = "deDE", Locale = Locale.DeDE },
            new LocalSelectData() { DisplayName = "enCN", Locale = Locale.EnCN },
            new LocalSelectData() { DisplayName = "zhCN", Locale = Locale.ZhCN },
            new LocalSelectData() { DisplayName = "enTW", Locale = Locale.EnTW },
            new LocalSelectData() { DisplayName = "zhTW", Locale = Locale.ZhTW },
            new LocalSelectData() { DisplayName = "esES", Locale = Locale.EsES },
            new LocalSelectData() { DisplayName = "esMX", Locale = Locale.EsMX },
            new LocalSelectData() { DisplayName = "ruRU", Locale = Locale.RuRU },
            new LocalSelectData() { DisplayName = "ptPT", Locale = Locale.PtPT },
            new LocalSelectData() { DisplayName = "ptBR", Locale = Locale.PtBR },
            new LocalSelectData() { DisplayName = "itIT", Locale = Locale.ItIT }
        };

        public bool IsCanceled = false;
        public string SelectedVersion = null;
        public Locale SelectedLocale = Locale.EnUS;

        public DefinitionSelect()
        {
            InitializeComponent();
            DefinitionSelectList.Focus();
            DefinitionSelectList.ItemsSource = DefinitionSelectData;
            LocaleSelectList.ItemsSource = LocalSelectData;

            string lastLocaleSelectedIndexSetting = SettingStorage.Get("LastLocaleSelectedIndex");
            if (lastLocaleSelectedIndexSetting != null)
            {
                int lastLocaleSelectedIndex = Int32.Parse(lastLocaleSelectedIndexSetting);
                LocaleSelectList.SelectedIndex = lastLocaleSelectedIndex;
            }
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            SelectedVersion = (DefinitionSelectList.SelectedValue as DefinitionSelectData).Version;
            IsCanceled = false;
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SelectedVersion = null;
            IsCanceled = true;
            Close();
        }

        public void SetDB2Name(string db2Name)
        {
            this.db2Name.Content = db2Name;
            this.Title = String.Format("Select Definition: {0}", db2Name);
        }

        public void SetDefinitionFromVersionDefinitions(VersionDefinitions[] versionDefinitions)
        {
            foreach (VersionDefinitions versionDefinition in versionDefinitions)
            {
                if (versionDefinition.buildRanges.Length > 0)
                {
                    foreach (BuildRange buildRange in versionDefinition.buildRanges)
                    {
                        DefinitionSelectData.Add(new DefinitionSelectData()
                        {
                            DisplayName = String.Format("{0} - {1}", buildRange.minBuild, buildRange.maxBuild),
                            Version = buildRange.maxBuild.ToString()
                        });
                    }
                }
                else
                {
                    foreach (Build build in versionDefinition.builds)
                    {
                        DefinitionSelectData.Add(new DefinitionSelectData()
                        {
                            DisplayName = build.ToString(),
                            Version = build.ToString()
                        });
                    }
                }
            }

            DefinitionSelectList.ItemsSource = DefinitionSelectData
                .OrderByDescending(e => e.Version)
                .Prepend(new DefinitionSelectData() { DisplayName = "Autoselect", Version = null })
                .ToList();
        }

        private void LocaleSelected(object sender, SelectionChangedEventArgs e)
        {
            LocalSelectData localSelectData = (LocaleSelectList.SelectedItem as LocalSelectData);

            if (localSelectData.Locale != SelectedLocale)
            {
                SelectedLocale = localSelectData.Locale;
                SettingStorage.Store("LastLocaleSelectedIndex", LocaleSelectList.SelectedIndex.ToString());
            }
        }
    }

    class DefinitionSelectData
    {
        public string DisplayName { get; set; }
        public string Version { get; set; } = null;
    }

    class LocalSelectData
    {
        public string DisplayName { get; set; }
        public Locale Locale { get; set; }
    }
}
