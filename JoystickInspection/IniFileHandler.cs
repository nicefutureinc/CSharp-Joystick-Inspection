using System.Collections.Generic;
using System.IO;

namespace JoystickInspection
{
    class IniFileHandler
    {
        private string _filePath;

        public IniFileHandler(string filePath)
        {
            _filePath = filePath;
        }

        // Reading values from INI files
        public Dictionary<string, string> ReadSettings()
        {
            var settings = new Dictionary<string, string>();

            if (!File.Exists(_filePath))
            {
                // If the file does not exist, create it with the default value.
                CreateDefaultIniFile();
            }

            string[] lines = File.ReadAllLines(_filePath);

            foreach (string line in lines)
            {
                // Ignore comments and blank lines
                if (line.StartsWith(";") || string.IsNullOrWhiteSpace(line))
                    continue;

                // Check if it is in “key=value” format.
                var parts = line.Split('=');

                if (parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();

                    //  Store keys and values in a dictionary
                    settings[key] = value;
                }
            }

            return settings;
        }

        // Create INI file by default
        private void CreateDefaultIniFile()
        {
            var defaultSettings = new Dictionary<string, string>
            {
                 { "left_id", "0C" }
                ,{ "left_pgn_button", "FDD8D0" }
                ,{ "left_pgn_thumbwheel", "FDD9D0" }
                ,{ "right_id", "0C" }
                ,{ "right_pgn_button", "FDD6D1" }
                ,{ "right_pgn_thumbwheel", "FDD7D1" }
                ,{ "left_joystick_max_left", "1000" }
                ,{ "left_joystick_max_right", "1000" }
                ,{ "right_joystick_max_left", "1000" }
                ,{ "right_joystick_max_right", "1000" }
                ,{ "left_joystick_max_forward", "1000" }
                ,{ "left_joystick_max_backward", "1000" }
                ,{ "right_joystick_max_forward", "1000" }
                ,{ "right_joystick_max_backward", "1000" }
                ,{ "left_wheel_max_left", "1000" }
                ,{ "left_wheel_max_right", "1000" }
                ,{ "right_wheel_max_left", "1000" }
                ,{ "right_wheel_max_right", "1000" }
                ,{ "save_log_file", "n" }
            };

            using (StreamWriter writer = new StreamWriter(_filePath))
            {
                writer.WriteLine("[Settings]");
                foreach (var setting in defaultSettings)
                {
                    writer.WriteLine($"{setting.Key}={setting.Value}");
                }
            }
        }

        // Save settings to INI file
        public void SaveSettings(Dictionary<string, string> settings)
        {
            using (StreamWriter writer = new StreamWriter(_filePath))
            {
                writer.WriteLine("[Settings]");
                foreach (var setting in settings)
                {
                    writer.WriteLine($"{setting.Key}={setting.Value}");
                }
            }
        }

    }
}
