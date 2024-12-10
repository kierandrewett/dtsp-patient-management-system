using PMS.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PMS.Controllers
{
    public class SettingsData
    {
        public bool? IsTTSEnabled { get; set; }
        public int? TTSSpeechRate { get; set; }

    }

    public class SettingsController
    {
        private SettingsData _StoredSettings;
        public SettingsData StoredSettings
        {
            get
            {
                _StoredSettings = ReadSettings();

                return _StoredSettings;
            }
        }
        private bool _SettingsStateChanged;
        public bool SettingsStateChanged { get => _SettingsStateChanged; }

        public static string GetSettingsPath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            string pmsFolder = System.IO.Path.Combine(appDataPath, "PMS");
            if (!Directory.Exists(pmsFolder))
            {
                Directory.CreateDirectory(pmsFolder);
            }

            string settingsPath = System.IO.Path.Combine(pmsFolder, "settings.json");

            if (!System.IO.Path.Exists(settingsPath))
            {
                File.WriteAllText(settingsPath, "{}");
            }

            return settingsPath;
        }

        public SettingsData ReadSettings()
        {
            string settingsPath = GetSettingsPath();
            string json = File.ReadAllText(settingsPath);

            SettingsData settings = new();

            try
            {
                SettingsData data = JsonSerializer.Deserialize<SettingsData>(json);

                if (data != null)
                {
                    settings = data;
                }
            }
            catch (Exception _)
            {

            }

            return settings;
        }

        public void WriteSettings(SettingsData settings)
        {
            string json = JsonSerializer.Serialize(
                settings,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                }
            );

            string settingsPath = GetSettingsPath();

            File.WriteAllText(settingsPath, json);

            Debug.WriteLine($"Writing settings to file {settingsPath}");

            _SettingsStateChanged = true;
        }
    }
}
