using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Peak.Can.Basic;

using TPCANHandle = System.UInt16;

namespace JoystickInspection
{
    public partial class FormMain: Form, IMessageFilter
    {
        private Image imageGreenDot;


        private TPCANHandle _channel;
        private Thread _receiveThread;
        private volatile bool _running;
        private readonly System.Windows.Forms.Timer _uiTimer = new System.Windows.Forms.Timer();

        // Left joystick ID, PGN
        string LEFT_ID = "";
        string LEFT_PGN_BUTTON = "";
        string LEFT_PGN_THUMBWHEEL = "";

        // Right joystick ID, PGN
        string RIGHT_ID = "";
        string RIGHT_PGN_BUTTON = "";
        string RIGHT_PGN_THUMBWHEEL = "";

        // Whether to store logs
        string SAVE_LOG_FILE = "n";

        const int COORDINATE_DOT_SIZE = 14;

        // Whether the joystick or thumbwheel has exceeded the maximum value
        int iRightJoystickMaxLeft = 0;
        int iRightJoystickMaxRight = 0;
        int iRightJoystickMaxForward = 0;
        int iRightJoystickMaxBackward = 0;
        int iRightWheelMaxLeft = 0;
        int iRightWheelMaxRight = 0;
        int iLeftJoystickMaxLeft = 0;
        int iLeftJoystickMaxRight = 0;
        int iLeftJoystickMaxForward = 0;
        int iLeftJoystickMaxBackward = 0;
        int iLeftWheelMaxLeft = 0;
        int iLeftWheelMaxRight = 0;

        // ----------- Right joystick value global variable -----------
        // Right button
        public bool mem_right_button1 = false;
        public bool mem_right_button2 = false;
        public bool mem_right_button3 = false;
        public bool mem_right_button4 = false;
        // Right joystick
        public int mem_right_joystick_iX_value;
        public int mem_right_joystick_iY_value;
        public int mem_right_joystick_xFlag = 0;
        public int mem_right_joystick_yFlag = 0;
        // Right Thumbwheel
        public int mem_right_wheel_iX_value;
        public int mem_right_wheel_xFlag = 0;

        // ----------- Left joystick value global variable -----------
        // Left button
        public bool mem_left_button1 = false;
        public bool mem_left_button2 = false;
        public bool mem_left_button3 = false;
        public bool mem_left_button4 = false;
        // Left joystick
        public int mem_left_joystick_iX_value;
        public int mem_left_joystick_iY_value;
        public int mem_left_joystick_xFlag = 0;
        public int mem_left_joystick_yFlag = 0;
        // Left thumbwheel
        public int mem_left_wheel_iX_value;
        public int mem_left_wheel_xFlag = 0;

        //------------ Storing maximum values in memory
        int mem_labelLeftJoystickMaxLeft;
        int mem_labelLeftJoystickMaxRight;
        int mem_labelRightJoystickMaxLeft;
        int mem_labelRightJoystickMaxRight;
        int mem_labelLeftJoystickMaxForward;
        int mem_labelLeftJoystickMaxBackward;
        int mem_labelRightJoystickMaxForward;
        int mem_labelRightJoystickMaxBackward;
        int mem_labelLeftWheelMaxLeft;
        int mem_labelLeftWheelMaxRight;
        int mem_labelRightWheelMaxLeft;
        int mem_labelRightWheelMaxRight;

        // The most recent data received from can
        private string[] recentJoystickData = new string[4]; // (0) RIGHT_PGN_BUTTON (1) RIGHT_PGN_THUMBWHEEL (2) LEFT_PGN_BUTTON (3) LEFT_PGN_THUMBWHEEL

        // Check the time when the data was received.
        DateTime dtLeftLastReceivedTime;
        DateTime dtRightLastReceivedTime;

        // Set the default path for storing logs
        LogManager logManager;

        PictureBox pBoxRightDot;
        PictureBox pBoxLeftDot;

        public FormMain()
        {
            InitializeComponent();

            imageGreenDot = Image.FromFile(Path.Combine(Application.StartupPath, "Images", "green_circle14.png"));


            _uiTimer.Interval = 50; // UI refresh every 50 ms

            _uiTimer.Tick += UiTimer_Tick;

            btnDisconnect.Enabled = false; // Disable Disconnect at first

            Application.AddMessageFilter(this); // Detect every mouse click

            // Changing the background color of joystick buttons
            ChangeButtonColor();

            this.KeyPreview = true; //  Set the form to process key input first

            InitRecentJoystickData();

            // Creating joystick position points
            pBoxRightDot = new PictureBox();
            pBoxRightDot.SizeMode = PictureBoxSizeMode.StretchImage;
            pBoxRightDot.Size = new Size(COORDINATE_DOT_SIZE, COORDINATE_DOT_SIZE);
            pBoxRightDot.BackColor = Color.Transparent; // Set transparent background
            pBoxRightDot.Image = imageGreenDot;
            pBoxRightDot.Visible = false;
            pictureBoxCircleRight.Controls.Add(pBoxRightDot);

            pBoxLeftDot = new PictureBox();
            pBoxLeftDot.SizeMode = PictureBoxSizeMode.StretchImage;
            pBoxLeftDot.Size = new Size(COORDINATE_DOT_SIZE, COORDINATE_DOT_SIZE);
            pBoxLeftDot.BackColor = Color.Transparent; // Set transparent background
            pBoxLeftDot.Image = imageGreenDot;
            pBoxLeftDot.Visible = false;
            pictureBoxCircleLeft.Controls.Add(pBoxLeftDot);

            // Display initialization
            DisplayInit();

            // Reading stored values from the setting.ini file
            GetSettingInfo();


            timerRecvCheck.Enabled = true;
        }

        private void ReceiveLoop()
        {
            while (_running)
            {
                TPCANMsg msg;
                TPCANTimestamp timestamp;
                var status = PCANBasic.Read(_channel, out msg, out timestamp);
                if (status == TPCANStatus.PCAN_ERROR_OK)
                {
                    string dataStr = BitConverter.ToString(msg.DATA, 0, msg.LEN);
                    string text = $"ID: 0x{msg.ID:X8} LEN: {msg.LEN} DATA: {dataStr}";

                    String sCmd = $"{msg.ID:X8}".Substring(2, 6);

                    // Convert to data format
                    // e 0c fd d7 d1 8 01 00 00 00 00 00 00 00

                    String sUCanType = $"{msg.ID:X8}{msg.LEN}{dataStr.Replace("-", "")}";

                    if (SAVE_LOG_FILE == "y")
                        logManager?.Log(sUCanType);

                    if (sCmd == RIGHT_PGN_BUTTON)
                    {
                        recentJoystickData[0] = sUCanType;
                        dtRightLastReceivedTime = DateTime.Now;
                    }
                    else if (sCmd == RIGHT_PGN_THUMBWHEEL)
                    {
                        recentJoystickData[1] = sUCanType;
                        dtRightLastReceivedTime = DateTime.Now;
                    }
                    else if (sCmd == LEFT_PGN_BUTTON)
                    {
                        recentJoystickData[2] = sUCanType;
                        dtLeftLastReceivedTime = DateTime.Now;
                    }
                    else if (sCmd == LEFT_PGN_THUMBWHEEL)
                    {
                        recentJoystickData[3] = sUCanType;
                        dtLeftLastReceivedTime = DateTime.Now;
                    }

                }
                else if (status == TPCANStatus.PCAN_ERROR_QRCVEMPTY) // Receive buffer empty
                {
                    Thread.Sleep(5); // Preventing too fast calls
                }
                else
                {
                    Thread.Sleep(10); // Wait for other errors
                }
            }
        }

        private void UiTimer_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < 4; i++)
            {
                DataAnalyzeEachData(recentJoystickData[i]);
            }
        }

        private void InitRecentJoystickData()
        {
            for (int i = 0; i < 4; i++)
            {
                recentJoystickData[i] = "";
            }
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
            {
                LEFT_ID = settings["left_id"].ToUpper();
                txtLeftID.Text = LEFT_ID;
            }
            if (settings.ContainsKey("left_pgn_button"))
                LEFT_PGN_BUTTON = settings["left_pgn_button"].ToUpper();
            if (settings.ContainsKey("left_pgn_thumbwheel"))
                LEFT_PGN_THUMBWHEEL = settings["left_pgn_thumbwheel"].ToUpper();

            if (settings.ContainsKey("right_id"))
            {
                RIGHT_ID = settings["right_id"].ToUpper();
                txtRightID.Text = RIGHT_ID;
            }
            if (settings.ContainsKey("right_pgn_button"))
                RIGHT_PGN_BUTTON = settings["right_pgn_button"].ToUpper();
            if (settings.ContainsKey("right_pgn_thumbwheel"))
                RIGHT_PGN_THUMBWHEEL = settings["right_pgn_thumbwheel"].ToUpper();

            if (settings.ContainsKey("left_joystick_max_left"))
            {
                labelLeftJoystickMaxLeft.Text = settings["left_joystick_max_left"];
                mem_labelLeftJoystickMaxLeft = Convert.ToInt32(settings["left_joystick_max_left"]);
            }
            if (settings.ContainsKey("left_joystick_max_right"))
            {
                labelLeftJoystickMaxRight.Text = settings["left_joystick_max_right"];
                mem_labelLeftJoystickMaxRight = Convert.ToInt32(settings["left_joystick_max_right"]);
            }
            if (settings.ContainsKey("right_joystick_max_left"))
            {
                labelRightJoystickMaxLeft.Text = settings["right_joystick_max_left"];
                mem_labelRightJoystickMaxLeft = Convert.ToInt32(settings["right_joystick_max_left"]);
            }
            if (settings.ContainsKey("right_joystick_max_right"))
            {
                labelRightJoystickMaxRight.Text = settings["right_joystick_max_right"];
                mem_labelRightJoystickMaxRight = Convert.ToInt32(settings["right_joystick_max_right"]);
            }
            if (settings.ContainsKey("left_joystick_max_forward"))
            {
                labelLeftJoystickMaxForward.Text = settings["left_joystick_max_forward"];
                mem_labelLeftJoystickMaxForward = Convert.ToInt32(settings["left_joystick_max_forward"]);
            }
            if (settings.ContainsKey("left_joystick_max_backward"))
            {
                labelLeftJoystickMaxBackward.Text = settings["left_joystick_max_backward"];
                mem_labelLeftJoystickMaxBackward = Convert.ToInt32(settings["left_joystick_max_backward"]);
            }
            if (settings.ContainsKey("right_joystick_max_forward"))
            {
                labelRightJoystickMaxForward.Text = settings["right_joystick_max_forward"];
                mem_labelRightJoystickMaxForward = Convert.ToInt32(settings["right_joystick_max_forward"]);
            }
            if (settings.ContainsKey("right_joystick_max_backward"))
            {
                labelRightJoystickMaxBackward.Text = settings["right_joystick_max_backward"];
                mem_labelRightJoystickMaxBackward = Convert.ToInt32(settings["right_joystick_max_backward"]);
            }

            if (settings.ContainsKey("left_wheel_max_left"))
            {
                labelLeftWheelMaxLeft.Text = settings["left_wheel_max_left"];
                mem_labelLeftWheelMaxLeft = Convert.ToInt32(settings["left_wheel_max_left"]);
            }
            if (settings.ContainsKey("left_wheel_max_right"))
            {
                labelLeftWheelMaxRight.Text = settings["left_wheel_max_right"];
                mem_labelLeftWheelMaxRight = Convert.ToInt32(settings["left_wheel_max_right"]);
            }
            if (settings.ContainsKey("right_wheel_max_left"))
            {
                labelRightWheelMaxLeft.Text = settings["right_wheel_max_left"];
                mem_labelRightWheelMaxLeft = Convert.ToInt32(settings["right_wheel_max_left"]);
            }
            if (settings.ContainsKey("right_wheel_max_right"))
            {
                labelRightWheelMaxRight.Text = settings["right_wheel_max_right"];
                mem_labelRightWheelMaxRight = Convert.ToInt32(settings["right_wheel_max_right"]);
            }

            // Whether to save log files
            if (settings.ContainsKey("save_log_file"))
                SAVE_LOG_FILE = settings["save_log_file"].ToLower();
        }


        // Display initialization
        private void DisplayInit()
        {
            pBoxLeftConnect.Image = imageList.Images[0];
            pBoxRightConnect.Image = imageList.Images[0];
            pBoxLeftButton3.Image = imageList.Images[0];
            pBoxLeftButton4.Image = imageList.Images[0];
            pBoxLeftButton2.Image = imageList.Images[0];
            pBoxLeftButton1.Image = imageList.Images[0];
            pBoxLeftWheelRight.Image = imageList.Images[0];
            pBoxLeftWheelLeft.Image = imageList.Images[0];
            pBoxLeftJoystickForward.Image = imageList.Images[0];
            pBoxLeftJoystickBack.Image = imageList.Images[0];
            pBoxLeftJoystickLeft.Image = imageList.Images[0];
            pBoxLeftJoystickRight.Image = imageList.Images[0];
            pBoxRightButton4.Image = imageList.Images[0];
            pBoxRightButton3.Image = imageList.Images[0];
            pBoxRightButton2.Image = imageList.Images[0];
            pBoxRightButton1.Image = imageList.Images[0];
            pBoxRightWheelRight.Image = imageList.Images[0];
            pBoxRightWheelLeft.Image = imageList.Images[0];
            pBoxRightJoystickForward.Image = imageList.Images[0];
            pBoxRightJoystickBack.Image = imageList.Images[0];
            pBoxRightJoystickLeft.Image = imageList.Images[0];
            pBoxRightJoystickRight.Image = imageList.Images[0];

            btnConnect.BackColor = Color.FromArgb(253, 253, 253);
            btnConnect.FlatStyle = FlatStyle.Standard;
            btnConnect.Text = "Connect";


            labelRightConnect.Text = "None";
            labelLeftConnect.Text = "None";

            pBoxRightDot.Visible = false;
            pBoxLeftDot.Visible = false;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (txtLeftID.Text == "" || txtRightID.Text == "")
            {
                MessageBox.Show("Press the [Setting] button and enter the joystick ID.");
                return;
            }

            Connect connect = new Connect();
            Point parentPoint = this.Location;
            connect.StartPosition = FormStartPosition.Manual;
            connect.Location = new Point(parentPoint.X + 50, parentPoint.Y + 50);
            if (DialogResult.OK == connect.ShowDialog())
            {
                int usbIndex = int.Parse(connect.PassValue[0]) - 81; // Starting from “81” -81
                _channel = (TPCANHandle)(PCANBasic.PCAN_USBBUS1 + usbIndex);

                btnConnect.BackColor = ColorTranslator.FromHtml("#FFC000");
                btnConnect.FlatStyle = FlatStyle.Flat;
                btnConnect.FlatAppearance.BorderColor = ColorTranslator.FromHtml("#FFC000");
                btnConnect.FlatAppearance.BorderSize = 2; // Border thickness

                btnConnect.Text = "Connected";
                btnConnect.Enabled = false;

                this.Text = "E-Joystick Inspection (" + connect.PassValue[1] + ")";
                
                var result = PCANBasic.Initialize(_channel, TPCANBaudrate.PCAN_BAUD_500K);

                if (result != TPCANStatus.PCAN_ERROR_OK)
                {
                    MessageBox.Show("PCAN Initialize failed: " + result);
                    return;
                }

                _running = true;
                _receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
                _receiveThread.Start();

                _uiTimer.Start();

                btnConnect.Enabled = false;
                btnDisconnect.Enabled = true;

                logManager = new LogManager();
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            disconnect();
        }

        private void disconnect()
        {
            _running = false;
            _uiTimer.Stop();

            if (_receiveThread != null && _receiveThread.IsAlive)
            {
                _receiveThread.Join();
            }

            PCANBasic.Uninitialize(_channel);

            btnConnect.Enabled = true;
            btnDisconnect.Enabled = false;

            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.FlatStyle = FlatStyle.Standard;

            btnConnect.Text = "Connect";
        }


        public void DataAnalyzeEachData(String hexString)
        {
            // 0c fd d7 d1 8 01 00 00 00 00 00 00 00
            try
            {
                if (hexString.Length >= 25)
                {
                    hexString = hexString.Replace(" ", "");

                    // 0cfdd6d1 8 0100010000000000
                    //if (hexString[0] == 'e')
                    {
                        String sId = hexString.Substring(0, 2).ToUpper();
                        String sCmd = hexString.Substring(2, 6).ToUpper();
                        int iDataLength = Convert.ToInt32(hexString.Substring(8, 1));
                        String sData = hexString.Substring(9, iDataLength * 2);

                        if (iDataLength * 2 == sData.Length)
                        {
                            // Convert hexadecimal hex strings to byte arrays
                            byte[] byteArray = new byte[iDataLength];

                            for (int i = 0; i < sData.Length; i += 2)
                            {
                                // Cut two characters at a time and convert the hexadecimal value to bytes.
                                byteArray[i / 2] = Convert.ToByte(sData.Substring(i, 2), 16);
                            }

                            if (sId == RIGHT_ID && sCmd == RIGHT_PGN_BUTTON) // (Right) Button FDD6D1
                            {
                                //--------------------     Button     ------------------
                                if ((byteArray[5] & 0x10) != 0) // Button1  
                                {
                                    btnRight1.BackColor = Color.FromArgb(200, 239, 212);
                                    pBoxRightButton1.Image = imageList.Images[1];
                                }
                                else
                                {
                                    pBoxRightButton1.Image = imageList.Images[0];
                                }

                                if ((byteArray[5] & 0x40) != 0)  // Button2  
                                {
                                    btnRight2.BackColor = Color.FromArgb(200, 239, 212);
                                    pBoxRightButton2.Image = imageList.Images[1];
                                }
                                else
                                {
                                    pBoxRightButton2.Image = imageList.Images[0];
                                }

                                if ((byteArray[5] & 0x01) != 0) // Button3  
                                {
                                    btnRight3.BackColor = Color.FromArgb(200, 239, 212);
                                    pBoxRightButton3.Image = imageList.Images[1];
                                }
                                else
                                {
                                    pBoxRightButton3.Image = imageList.Images[0];
                                }

                                if ((byteArray[5] & 0x04) != 0) // Button4  
                                {
                                    btnRight4.BackColor = Color.FromArgb(200, 239, 212);
                                    pBoxRightButton4.Image = imageList.Images[1];
                                }
                                else
                                {
                                    pBoxRightButton4.Image = imageList.Images[0];
                                }

                                //-------------------------------------------------------
                                //                       joystick   
                                //-------------------------------------------------------
                                int iX_value = GetX_value(byteArray);
                                int iY_value = GetY_value(byteArray);
                                int xFlag = 0; // left and right directions (-1, 1)
                                int yFlag = 0; // up and down (-1, 1)

                                //  ----------------    X coordinates    ------------------------
                                if ((byteArray[0] & 0x01) != 0)  // Neutral
                                {
                                    xFlag = 1;

                                    labelRightPosX.Text = Convert.ToString(iX_value);

                                    pBoxRightJoystickLeft.Image = imageList.Images[iRightJoystickMaxLeft];
                                    pBoxRightJoystickRight.Image = imageList.Images[iRightJoystickMaxRight];
                                }
                                else if ((byteArray[0] & 0x10) != 0)  // Left
                                {
                                    xFlag = -1;

                                    labelRightPosX.Text = "L " + Convert.ToString(iX_value);

                                    pBoxRightJoystickRight.Image = imageList.Images[iRightJoystickMaxRight];

                                    if (iX_value >= mem_labelRightJoystickMaxLeft)
                                    {
                                        iRightJoystickMaxLeft = 1;

                                        pBoxRightJoystickLeft.Image = imageList.Images[iRightJoystickMaxLeft];
                                    }
                                    else
                                    {
                                        pBoxRightJoystickLeft.Image = imageList.Images[iRightJoystickMaxLeft];
                                    }

                                }
                                else if ((byteArray[0] & 0x04) != 0)  // Right
                                {
                                    xFlag = 1;

                                    labelRightPosX.Text = "R " + Convert.ToString(iX_value);

                                    pBoxRightJoystickLeft.Image = imageList.Images[iRightJoystickMaxLeft];

                                    if (iX_value >= mem_labelRightJoystickMaxRight)
                                    {
                                        iRightJoystickMaxRight = 1;

                                        pBoxRightJoystickRight.Image = imageList.Images[iRightJoystickMaxRight];
                                    }
                                    else
                                    {
                                        pBoxRightJoystickRight.Image = imageList.Images[iRightJoystickMaxRight];
                                    }
                                }

                                //  ----------------    Y coordinates    ------------------------
                                if ((byteArray[2] & 0x01) != 0)  // Neutral
                                {
                                    yFlag = 1;

                                    labelRightPosY.Text = Convert.ToString(iY_value);

                                    pBoxRightJoystickForward.Image = imageList.Images[iRightJoystickMaxForward];
                                    pBoxRightJoystickBack.Image = imageList.Images[iRightJoystickMaxBackward];
                                }
                                else if ((byteArray[2] & 0x10) != 0)  // Back
                                {
                                    yFlag = 1;

                                    labelRightPosY.Text = "B " + Convert.ToString(iY_value);

                                    pBoxRightJoystickForward.Image = imageList.Images[iRightJoystickMaxForward];

                                    if (iY_value >= mem_labelRightJoystickMaxBackward)
                                    {
                                        iRightJoystickMaxBackward = 1;

                                        pBoxRightJoystickBack.Image = imageList.Images[iRightJoystickMaxBackward];
                                    }
                                    else
                                    {
                                        pBoxRightJoystickBack.Image = imageList.Images[iRightJoystickMaxBackward];
                                    }
                                }
                                else if ((byteArray[2] & 0x04) != 0)  // Forward
                                {
                                    yFlag = -1;

                                    labelRightPosY.Text = "F " + Convert.ToString(iY_value);

                                    pBoxRightJoystickBack.Image = imageList.Images[iRightJoystickMaxBackward];

                                    if (iY_value >= mem_labelRightJoystickMaxForward)
                                    {
                                        iRightJoystickMaxForward = 1;

                                        pBoxRightJoystickForward.Image = imageList.Images[iRightJoystickMaxForward];
                                    }
                                    else
                                    {
                                        pBoxRightJoystickForward.Image = imageList.Images[iRightJoystickMaxForward];
                                    }
                                }

                                // Display coordinate values on screen
                                Display_xy_coordinate(iX_value, iY_value, xFlag, yFlag, "R");
                            }
                            else if (sId == RIGHT_ID && sCmd == RIGHT_PGN_THUMBWHEEL) // (Right) Thumbwheel FDD7D1
                            {
                                int iX_value = GetX_value(byteArray);

                                if ((byteArray[0] & 0x01) != 0)  // Neutral
                                {
                                    labelRightWheelPosX.Text = Convert.ToString(iX_value);
                                    pBoxRightWheelLeft.Image = imageList.Images[iRightWheelMaxLeft];
                                    pBoxRightWheelRight.Image = imageList.Images[iRightWheelMaxRight];
                                    trackBarRight.Value = 1000;

                                }
                                else if ((byteArray[0] & 0x04) != 0)  // Left
                                {
                                    labelRightWheelPosX.Text = "L " + Convert.ToString(iX_value);
                                    pBoxRightWheelRight.Image = imageList.Images[iRightWheelMaxRight];
                                    trackBarRight.Value = 1000 - iX_value;

                                    if (iX_value >= mem_labelRightWheelMaxLeft)
                                    {
                                        iRightWheelMaxLeft = 1;

                                        pBoxRightWheelLeft.Image = imageList.Images[iRightWheelMaxLeft];
                                    }
                                    else
                                    {
                                        pBoxRightWheelLeft.Image = imageList.Images[iRightWheelMaxLeft];
                                    }

                                }
                                else if ((byteArray[0] & 0x10) != 0)  // Right
                                {
                                    labelRightWheelPosX.Text = "R " + Convert.ToString(iX_value);
                                    pBoxRightWheelLeft.Image = imageList.Images[iRightWheelMaxLeft];
                                    trackBarRight.Value = 1000 + iX_value;

                                    if (iX_value >= mem_labelRightWheelMaxRight)
                                    {
                                        iRightWheelMaxRight = 1;

                                        pBoxRightWheelRight.Image = imageList.Images[iRightWheelMaxRight];
                                    }
                                    else
                                    {
                                        pBoxRightWheelRight.Image = imageList.Images[iRightWheelMaxRight];
                                    }
                                }
                            }
                            else if (sId == LEFT_ID && sCmd == LEFT_PGN_BUTTON) // (Left) Button FDD8D0
                            {
                                //--------------------     Button     ------------------
                                if ((byteArray[5] & 0x10) != 0) // Button1  
                                {
                                    btnLeft1.BackColor = Color.FromArgb(200, 239, 212);
                                    pBoxLeftButton1.Image = imageList.Images[1];
                                }
                                else
                                    pBoxLeftButton1.Image = imageList.Images[0];

                                if ((byteArray[5] & 0x40) != 0)  // Button2  
                                {
                                    btnLeft2.BackColor = Color.FromArgb(200, 239, 212);
                                    pBoxLeftButton2.Image = imageList.Images[1];
                                }
                                else
                                    pBoxLeftButton2.Image = imageList.Images[0];

                                if ((byteArray[5] & 0x01) != 0) // Button3  
                                {
                                    btnLeft3.BackColor = Color.FromArgb(200, 239, 212);
                                    pBoxLeftButton3.Image = imageList.Images[1];
                                }
                                else
                                    pBoxLeftButton3.Image = imageList.Images[0];

                                if ((byteArray[5] & 0x04) != 0) // Button4  
                                {
                                    btnLeft4.BackColor = Color.FromArgb(200, 239, 212);
                                    pBoxLeftButton4.Image = imageList.Images[1];
                                }
                                else
                                    pBoxLeftButton4.Image = imageList.Images[0];

                                //-------------------------------------------------------
                                //                       joystick   
                                //-------------------------------------------------------
                                int iX_value = GetX_value(byteArray);
                                int iY_value = GetY_value(byteArray);
                                int xFlag = 0; // left and right directions (-1, 1)
                                int yFlag = 0; // up and down (-1, 1)

                                //  ----------------    X coordinates    ------------------------
                                if ((byteArray[0] & 0x01) != 0)  // Neutral
                                {
                                    xFlag = 1;

                                    labelLeftPosX.Text = Convert.ToString(iX_value);

                                    pBoxLeftJoystickLeft.Image = imageList.Images[iLeftJoystickMaxLeft];
                                    pBoxLeftJoystickRight.Image = imageList.Images[iLeftJoystickMaxRight];
                                }
                                else if ((byteArray[0] & 0x10) != 0)  // Left
                                {
                                    xFlag = -1;

                                    labelLeftPosX.Text = "L " + Convert.ToString(iX_value);

                                    pBoxLeftJoystickRight.Image = imageList.Images[iLeftJoystickMaxRight];

                                    if (iX_value >= mem_labelLeftJoystickMaxLeft)
                                    {
                                        iLeftJoystickMaxLeft = 1;

                                        pBoxLeftJoystickLeft.Image = imageList.Images[iLeftJoystickMaxLeft];
                                    }
                                    else
                                    {
                                        pBoxLeftJoystickLeft.Image = imageList.Images[iLeftJoystickMaxLeft];
                                    }

                                }
                                else if ((byteArray[0] & 0x04) != 0)  // Right
                                {
                                    xFlag = 1;

                                    labelLeftPosX.Text = "R " + Convert.ToString(iX_value);

                                    pBoxLeftJoystickLeft.Image = imageList.Images[iLeftJoystickMaxLeft];

                                    if (iX_value >= mem_labelLeftJoystickMaxRight)
                                    {
                                        iLeftJoystickMaxRight = 1;

                                        pBoxLeftJoystickRight.Image = imageList.Images[iLeftJoystickMaxRight];
                                    }
                                    else
                                    {
                                        pBoxLeftJoystickRight.Image = imageList.Images[iLeftJoystickMaxRight];
                                    }
                                }

                                //  ----------------    Y coordinates    ------------------------
                                if ((byteArray[2] & 0x01) != 0)  // Neutral
                                {
                                    yFlag = 1;

                                    labelLeftPosY.Text = Convert.ToString(iY_value);

                                    pBoxLeftJoystickForward.Image = imageList.Images[iLeftJoystickMaxForward];
                                    pBoxLeftJoystickBack.Image = imageList.Images[iLeftJoystickMaxBackward];
                                }
                                else if ((byteArray[2] & 0x10) != 0)  // Back
                                {
                                    yFlag = 1;

                                    labelLeftPosY.Text = "B " + Convert.ToString(iY_value);

                                    pBoxLeftJoystickForward.Image = imageList.Images[iLeftJoystickMaxForward];

                                    if (iY_value >= mem_labelLeftJoystickMaxBackward)
                                    {
                                        iLeftJoystickMaxBackward = 1;

                                        pBoxLeftJoystickBack.Image = imageList.Images[iLeftJoystickMaxBackward];
                                    }
                                    else
                                    {
                                        pBoxLeftJoystickBack.Image = imageList.Images[iLeftJoystickMaxBackward];
                                    }
                                }
                                else if ((byteArray[2] & 0x04) != 0)  // Forward
                                {
                                    yFlag = -1;

                                    labelLeftPosY.Text = "F " + Convert.ToString(iY_value);

                                    pBoxLeftJoystickBack.Image = imageList.Images[iLeftJoystickMaxBackward];

                                    if (iY_value >= mem_labelLeftJoystickMaxForward)
                                    {
                                        iLeftJoystickMaxForward = 1;

                                        pBoxLeftJoystickForward.Image = imageList.Images[iLeftJoystickMaxForward];
                                    }
                                    else
                                    {
                                        pBoxLeftJoystickForward.Image = imageList.Images[iLeftJoystickMaxForward];
                                    }
                                }

                                // Display coordinate values on screen
                                Display_xy_coordinate(iX_value, iY_value, xFlag, yFlag, "L");

                            }
                            else if (sId == LEFT_ID && sCmd == LEFT_PGN_THUMBWHEEL) // (Left) Thumbwheel FDD9D0
                            {
                                int iX_value = GetX_value(byteArray);

                                if ((byteArray[0] & 0x01) != 0)  // Neutral
                                {
                                    labelLeftWheelPosX.Text = Convert.ToString(iX_value);
                                    pBoxLeftWheelLeft.Image = imageList.Images[iLeftWheelMaxLeft];
                                    pBoxLeftWheelRight.Image = imageList.Images[iLeftWheelMaxRight];
                                    trackBarLeft.Value = 1000;
                                }
                                else if ((byteArray[0] & 0x04) != 0)  // Left
                                {
                                    labelLeftWheelPosX.Text = "L " + Convert.ToString(iX_value);
                                    pBoxLeftWheelRight.Image = imageList.Images[iLeftWheelMaxRight];
                                    trackBarLeft.Value = 1000 - iX_value;

                                    if (iX_value >= mem_labelLeftWheelMaxLeft)
                                    {
                                        iLeftWheelMaxLeft = 1;

                                        pBoxLeftWheelLeft.Image = imageList.Images[iLeftWheelMaxLeft];
                                    }
                                    else
                                    {
                                        pBoxLeftWheelLeft.Image = imageList.Images[iLeftWheelMaxLeft];
                                    }

                                }
                                else if ((byteArray[0] & 0x10) != 0)  // Right
                                {
                                    labelLeftWheelPosX.Text = "R " + Convert.ToString(iX_value);
                                    pBoxLeftWheelLeft.Image = imageList.Images[iLeftWheelMaxLeft];
                                    trackBarLeft.Value = 1000 + iX_value;

                                    if (iX_value >= mem_labelLeftWheelMaxRight)
                                    {
                                        iLeftWheelMaxRight = 1;

                                        pBoxLeftWheelRight.Image = imageList.Images[iLeftWheelMaxRight];
                                    }
                                    else
                                    {
                                        pBoxLeftWheelRight.Image = imageList.Images[iLeftWheelMaxRight];
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logManager.Log("DataAnalyzeEachData: " + ex.ToString());
            }
        }


        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            timerClose.Enabled = true;
        }

        private int GetX_value(byte[] bytes)
        {
            // The upper 2 bits (bits 6 and 7) of bytes[0]
            byte lowerTwoBits = (byte)(bytes[0] >> 6); // 0b11

            // Lower 8 bits of bytes[1] (all bits used)
            byte upperEightBits = bytes[1]; // 0b10110101

            // Combine two bits to generate a 10-bit value.
            int result = (upperEightBits << 2) | lowerTwoBits; // 0b111011010110101

            return result;
        }

        private int GetY_value(byte[] bytes)
        {
            //The upper 2 bits (bits 6 and 7) of bytes[2]
            byte lowerTwoBits = (byte)(bytes[2] >> 6); // 0b11

            // Lower 8 bits of bytes[3] (all bits used)
            byte upperEightBits = bytes[3]; // 0b10110101

            // Combine two bits to generate a 10-bit value.
            int result = (upperEightBits << 2) | lowerTwoBits; // 0b111011010110101

            return result;
        }

        private void btnSetting_Click(object sender, EventArgs e)
        {
            FormSetting formSetting = new FormSetting();
            Point parentPoint = this.Location;
            formSetting.StartPosition = FormStartPosition.Manual;
            formSetting.Location = new Point(parentPoint.X + 50, parentPoint.Y + 50);

            if (DialogResult.OK == formSetting.ShowDialog())
            {
                LEFT_ID = formSetting.PassValue[0];
                txtLeftID.Text = LEFT_ID;

                RIGHT_ID = formSetting.PassValue[1];
                txtRightID.Text = RIGHT_ID;

                labelLeftJoystickMaxLeft.Text = formSetting.PassValue[2];
                labelLeftJoystickMaxRight.Text = formSetting.PassValue[3];
                labelRightJoystickMaxLeft.Text = formSetting.PassValue[4];
                labelRightJoystickMaxRight.Text = formSetting.PassValue[5];
                labelLeftJoystickMaxForward.Text = formSetting.PassValue[6];
                labelLeftJoystickMaxBackward.Text = formSetting.PassValue[7];
                labelRightJoystickMaxForward.Text = formSetting.PassValue[8];
                labelRightJoystickMaxBackward.Text = formSetting.PassValue[9];
                labelLeftWheelMaxLeft.Text = formSetting.PassValue[10];
                labelLeftWheelMaxRight.Text = formSetting.PassValue[11];
                labelRightWheelMaxLeft.Text = formSetting.PassValue[12];
                labelRightWheelMaxRight.Text = formSetting.PassValue[13];

                mem_labelLeftJoystickMaxLeft = Convert.ToInt32(formSetting.PassValue[2]);
                mem_labelLeftJoystickMaxRight = Convert.ToInt32(formSetting.PassValue[3]);
                mem_labelRightJoystickMaxLeft = Convert.ToInt32(formSetting.PassValue[4]);
                mem_labelRightJoystickMaxRight = Convert.ToInt32(formSetting.PassValue[5]);
                mem_labelLeftJoystickMaxForward = Convert.ToInt32(formSetting.PassValue[6]);
                mem_labelLeftJoystickMaxBackward = Convert.ToInt32(formSetting.PassValue[7]);
                mem_labelRightJoystickMaxForward = Convert.ToInt32(formSetting.PassValue[8]);
                mem_labelRightJoystickMaxBackward = Convert.ToInt32(formSetting.PassValue[9]);
                mem_labelLeftWheelMaxLeft = Convert.ToInt32(formSetting.PassValue[10]);
                mem_labelLeftWheelMaxRight = Convert.ToInt32(formSetting.PassValue[11]);
                mem_labelRightWheelMaxLeft = Convert.ToInt32(formSetting.PassValue[12]);
                mem_labelRightWheelMaxRight = Convert.ToInt32(formSetting.PassValue[13]);

                LEFT_PGN_BUTTON = formSetting.PassValue[14];
                LEFT_PGN_THUMBWHEEL = formSetting.PassValue[15];
                RIGHT_PGN_BUTTON = formSetting.PassValue[16];
                RIGHT_PGN_THUMBWHEEL = formSetting.PassValue[17];
            }
        }

        // Drawing joystick coordinates
        private void Display_xy_coordinate(int x, int y, int xFlag, int yFlag, string sDirection)
        {
            // Center of PictureBox (175, 175)
            int centerX = pictureBoxCircleRight.Width / 2;
            int centerY = pictureBoxCircleRight.Height / 2;
            int circleWidthHalf = 140;

            // Calculate relative coordinates based on the center (0-1000 -> -140 to +140)
            int relativeX = centerX + xFlag * (circleWidthHalf * x / 1000) - COORDINATE_DOT_SIZE / 2;
            int relativeY = centerY + yFlag * (circleWidthHalf * y / 1000) - COORDINATE_DOT_SIZE / 2;

            if (sDirection == "R")
            {
                pBoxRightDot.Location = new Point(relativeX, relativeY);
            }
            else
            {
                pBoxLeftDot.Location = new Point(relativeX, relativeY);
            }
        }

        private void timerClose_Tick(object sender, EventArgs e)
        {
            timerClose.Enabled = false;

            disconnect();
        }

        private void btnInspectionComplete_Click(object sender, EventArgs e)
        {
            {
                InitRecentJoystickData();

                //  Set max flag value to 0, change dot color to gray
                SetMaxFlagZero();

                // Change button color
                ChangeButtonColor();
            }
        }

        // Set the max flag value to 0.
        private void SetMaxFlagZero()
        {
            iRightJoystickMaxLeft = 0;
            iRightJoystickMaxRight = 0;
            iRightJoystickMaxForward = 0;
            iRightJoystickMaxBackward = 0;
            iRightWheelMaxLeft = 0;
            iRightWheelMaxRight = 0;
            iLeftJoystickMaxLeft = 0;
            iLeftJoystickMaxRight = 0;
            iLeftJoystickMaxForward = 0;
            iLeftJoystickMaxBackward = 0;
            iLeftWheelMaxLeft = 0;
            iLeftWheelMaxRight = 0;

            pBoxRightJoystickLeft.Image = imageList.Images[iRightJoystickMaxLeft];
            pBoxRightJoystickRight.Image = imageList.Images[iRightJoystickMaxRight];
            pBoxRightJoystickForward.Image = imageList.Images[iRightJoystickMaxForward];
            pBoxRightJoystickBack.Image = imageList.Images[iRightJoystickMaxBackward];
            pBoxRightWheelLeft.Image = imageList.Images[iRightWheelMaxLeft];
            pBoxRightWheelRight.Image = imageList.Images[iRightWheelMaxRight];

            pBoxRightButton1.Image = imageList.Images[0];
            pBoxRightButton2.Image = imageList.Images[0];
            pBoxRightButton3.Image = imageList.Images[0];
            pBoxRightButton4.Image = imageList.Images[0];

            pBoxLeftJoystickLeft.Image = imageList.Images[iLeftJoystickMaxLeft];
            pBoxLeftJoystickRight.Image = imageList.Images[iLeftJoystickMaxRight];
            pBoxLeftJoystickForward.Image = imageList.Images[iLeftJoystickMaxForward];
            pBoxLeftJoystickBack.Image = imageList.Images[iLeftJoystickMaxBackward];
            pBoxLeftWheelLeft.Image = imageList.Images[iLeftWheelMaxLeft];
            pBoxLeftWheelRight.Image = imageList.Images[iLeftWheelMaxRight];

            pBoxLeftButton1.Image = imageList.Images[0];
            pBoxLeftButton2.Image = imageList.Images[0];
            pBoxLeftButton3.Image = imageList.Images[0];
            pBoxLeftButton4.Image = imageList.Images[0];

        }

        // Change button color
        private void ChangeButtonColor()
        {
            btnRight1.BackColor = Color.FromArgb(253, 253, 253);
            btnRight2.BackColor = Color.FromArgb(253, 253, 253);
            btnRight3.BackColor = Color.FromArgb(253, 253, 253);
            btnRight4.BackColor = Color.FromArgb(253, 253, 253);

            btnLeft1.BackColor = Color.FromArgb(253, 253, 253);
            btnLeft2.BackColor = Color.FromArgb(253, 253, 253);
            btnLeft3.BackColor = Color.FromArgb(253, 253, 253);
            btnLeft4.BackColor = Color.FromArgb(253, 253, 253);
        }

        private void FormMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                e.Handled = true; // Preventing the space key from being passed to other controls (e.g., button 1)
                btnInspectionInit.PerformClick();
            }
        }

        // Implementing IMessageFilter
        public bool PreFilterMessage(ref Message m)
        {
            return false; // Keep sending messages

        }

        private void timerRecvCheck_Tick(object sender, EventArgs e)
        {
            timerRecvCheck.Enabled = false;

            // Check whether left/right joystick data is coming in.
            DateTime now = DateTime.Now;

            TimeSpan gapLeft = now - dtLeftLastReceivedTime;
            TimeSpan gapRight = now - dtRightLastReceivedTime;

            // left joystick
            if (gapLeft.TotalSeconds > 1)
            {
                labelLeftConnect.Text = "None";
                pBoxLeftConnect.Image = imageList.Images[0];
                pBoxLeftDot.Visible = false;
            }
            else
            {
                labelRightConnect.Text = "Connect";
                pBoxRightConnect.Image = imageList.Images[1];
                pBoxRightDot.Visible = true;
            }

            // Right joystick
            if (gapRight.TotalSeconds > 1)
            {
                labelRightConnect.Text = "None";
                pBoxRightConnect.Image = imageList.Images[0];
                pBoxRightDot.Visible = false;
            }
            else
            {
                labelLeftConnect.Text = "Connect";
                pBoxLeftConnect.Image = imageList.Images[1];
                pBoxLeftDot.Visible = true;
            }

            timerRecvCheck.Enabled = true;
        }
    }
}
