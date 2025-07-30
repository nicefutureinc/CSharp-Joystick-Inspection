using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace JoystickInspection
{
    public partial class FormSetting: Form
    {
        private string[] sendValue = new string[18];

        public FormSetting()
        {
            InitializeComponent();
            labelMsg.Text = "";

            // Reading stored values from the setting.ini file
            GetSettingInfo();
        }

        // Reading stored values from the setting.ini file
        private void GetSettingInfo()
        {
            // INI file path
            string filePath = "setting.ini";

            // Create an IniFileHandler instance to read INI files.
            IniFileHandler iniHandler = new IniFileHandler(filePath);
            var settings = iniHandler.ReadSettings();

            // Read the settings and display them in the TextBox.
            if (settings.ContainsKey("left_id"))
                txtLeftID.Text = settings["left_id"];
            if (settings.ContainsKey("left_pgn_button"))
                txtLeftPgnButton.Text = settings["left_pgn_button"];
            if (settings.ContainsKey("left_pgn_thumbwheel"))
                txtLeftPgnThumbwheel.Text = settings["left_pgn_thumbwheel"];

            if (settings.ContainsKey("right_id"))
                txtRightID.Text = settings["right_id"];
            if (settings.ContainsKey("right_pgn_button"))
                txtRightPgnButton.Text = settings["right_pgn_button"];
            if (settings.ContainsKey("right_pgn_thumbwheel"))
                txtRightPgnThumbwheel.Text = settings["right_pgn_thumbwheel"];

            if (settings.ContainsKey("left_joystick_max_left"))
                txtLeftJoystickMaxLeft.Text = settings["left_joystick_max_left"];
            if (settings.ContainsKey("left_joystick_max_right"))
                txtLeftJoystickMaxRight.Text = settings["left_joystick_max_right"];
            if (settings.ContainsKey("right_joystick_max_left"))
                txtRightJoystickMaxLeft.Text = settings["right_joystick_max_left"];
            if (settings.ContainsKey("right_joystick_max_right"))
                txtRightJoystickMaxRight.Text = settings["right_joystick_max_right"];
            if (settings.ContainsKey("left_joystick_max_forward"))
                txtLeftJoystickMaxForward.Text = settings["left_joystick_max_forward"];
            if (settings.ContainsKey("left_joystick_max_backward"))
                txtLeftJoystickMaxBackward.Text = settings["left_joystick_max_backward"];
            if (settings.ContainsKey("right_joystick_max_forward"))
                txtRightJoystickMaxForward.Text = settings["right_joystick_max_forward"];
            if (settings.ContainsKey("right_joystick_max_backward"))
                txtRightJoystickMaxBackward.Text = settings["right_joystick_max_backward"];

            if (settings.ContainsKey("left_wheel_max_left"))
                txtLeftWheelMaxLeft.Text = settings["left_wheel_max_left"];
            if (settings.ContainsKey("left_wheel_max_right"))
                txtLeftWheelMaxRight.Text = settings["left_wheel_max_right"];
            if (settings.ContainsKey("right_wheel_max_left"))
                txtRightWheelMaxLeft.Text = settings["right_wheel_max_left"];
            if (settings.ContainsKey("right_wheel_max_right"))
                txtRightWheelMaxRight.Text = settings["right_wheel_max_right"];
        }

        public string[] PassValue
        {
            get
            {
                sendValue[0] = txtLeftID.Text.ToUpper();
                sendValue[1] = txtRightID.Text.ToUpper();
                sendValue[2] = txtLeftJoystickMaxLeft.Text;
                sendValue[3] = txtLeftJoystickMaxRight.Text;
                sendValue[4] = txtRightJoystickMaxLeft.Text;
                sendValue[5] = txtRightJoystickMaxRight.Text;
                sendValue[6] = txtLeftJoystickMaxForward.Text;
                sendValue[7] = txtLeftJoystickMaxBackward.Text;
                sendValue[8] = txtRightJoystickMaxForward.Text;
                sendValue[9] = txtRightJoystickMaxBackward.Text;
                sendValue[10] = txtLeftWheelMaxLeft.Text;
                sendValue[11] = txtLeftWheelMaxRight.Text;
                sendValue[12] = txtRightWheelMaxLeft.Text;
                sendValue[13] = txtRightWheelMaxRight.Text;

                sendValue[14] = txtLeftPgnButton.Text.ToUpper();
                sendValue[15] = txtLeftPgnThumbwheel.Text.ToUpper();
                sendValue[16] = txtRightPgnButton.Text.ToUpper();
                sendValue[17] = txtRightPgnThumbwheel.Text.ToUpper();

                return sendValue;
            }
        }

        // Save Joystick ID
        private void btnSaveID_Click(object sender, EventArgs e)
        {
            if (txtLeftID.Text == "")
            {
                labelMsg.Text = "Please enter the left joystick ID.";
                txtLeftID.Focus();
                return;
            }
            if (txtLeftPgnButton.Text == "")
            {
                labelMsg.Text = "Please enter PGN on the left joystick button.";
                txtLeftPgnButton.Focus();
                return;
            }
            if (txtLeftPgnThumbwheel.Text == "")
            {
                labelMsg.Text = "Please enter PGN on the left joystick thumbwheel.";
                txtLeftPgnThumbwheel.Focus();
                return;
            }

            if (txtRightID.Text == "")
            {
                labelMsg.Text = "Please enter the right joystick ID.";
                txtRightID.Focus();
                return;
            }
            if (txtRightPgnButton.Text == "")
            {
                labelMsg.Text = "Please enter PGN on the right joystick button.";
                txtRightPgnButton.Focus();
                return;
            }
            if (txtRightPgnThumbwheel.Text == "")
            {
                labelMsg.Text = "Please enter PGN on the right joystick thumbwheel.";
                txtRightPgnThumbwheel.Focus();
                return;
            }

            if (txtLeftJoystickMaxLeft.Text == "")
                txtLeftJoystickMaxLeft.Text = "1000";
            if (txtLeftJoystickMaxRight.Text == "")
                txtLeftJoystickMaxRight.Text = "1000";
            if (txtRightJoystickMaxLeft.Text == "")
                txtRightJoystickMaxLeft.Text = "1000";
            if (txtRightJoystickMaxRight.Text == "")
                txtRightJoystickMaxRight.Text = "1000";
            if (txtLeftJoystickMaxForward.Text == "")
                txtLeftJoystickMaxForward.Text = "1000";
            if (txtLeftJoystickMaxBackward.Text == "")
                txtLeftJoystickMaxBackward.Text = "1000";
            if (txtRightJoystickMaxForward.Text == "")
                txtRightJoystickMaxForward.Text = "1000";
            if (txtRightJoystickMaxBackward.Text == "")
                txtRightJoystickMaxBackward.Text = "1000";
            if (txtLeftWheelMaxLeft.Text == "")
                txtLeftWheelMaxLeft.Text = "1000";
            if (txtLeftWheelMaxRight.Text == "")
                txtLeftWheelMaxRight.Text = "1000";
            if (txtRightWheelMaxLeft.Text == "")
                txtRightWheelMaxLeft.Text = "1000";
            if (txtRightWheelMaxRight.Text == "")
                txtRightWheelMaxRight.Text = "1000";

            // Saving values in setting.ini
            SaveSettingValue();

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        // Saving values in setting.ini
        private void SaveSettingValue()
        {
            var settings = new Dictionary<string, string>
            {
                { "left_id", txtLeftID.Text }
                ,{ "left_pgn_button", txtLeftPgnButton.Text}
                ,{ "left_pgn_thumbwheel", txtLeftPgnThumbwheel.Text}
                ,{ "right_id", txtRightID.Text}
                ,{ "right_pgn_button", txtRightPgnButton.Text}
                ,{ "right_pgn_thumbwheel", txtRightPgnThumbwheel.Text}
                ,{ "left_joystick_max_left", txtLeftJoystickMaxLeft.Text }
                ,{ "left_joystick_max_right", txtLeftJoystickMaxRight.Text }
                ,{ "right_joystick_max_left", txtRightJoystickMaxLeft.Text }
                ,{ "right_joystick_max_right", txtRightJoystickMaxRight.Text }
                ,{ "left_joystick_max_forward", txtLeftJoystickMaxForward.Text }
                ,{ "left_joystick_max_backward", txtLeftJoystickMaxBackward.Text }
                ,{ "right_joystick_max_forward", txtRightJoystickMaxForward.Text }
                ,{ "right_joystick_max_backward", txtRightJoystickMaxBackward.Text }
                ,{ "left_wheel_max_left",txtLeftWheelMaxLeft.Text }
                ,{ "left_wheel_max_right", txtLeftWheelMaxRight.Text }
                ,{ "right_wheel_max_left", txtRightWheelMaxLeft.Text }
                ,{ "right_wheel_max_right", txtRightWheelMaxRight.Text }
            };

            string filePath = "setting.ini";

            IniFileHandler iniHandler = new IniFileHandler(filePath);
            iniHandler.SaveSettings(settings);
        }
    }
}
