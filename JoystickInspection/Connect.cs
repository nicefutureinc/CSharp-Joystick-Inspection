using Peak.Can.Basic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using TPCANHandle = System.UInt16;

namespace JoystickInspection
{
    public partial class Connect: Form
    {
        TPCANHandle pcanHandle = PCANBasic.PCAN_USBBUS1;
        private TPCANBaudrate baudrate = TPCANBaudrate.PCAN_BAUD_500K;

        /// <summary>
        /// Handles of non plug and play PCAN-Hardware
        /// </summary>
        private TPCANHandle[] m_NonPnPHandles;

        /// <summary>
        /// Saves the handle of a PCAN hardware
        /// </summary>
        private TPCANHandle m_PcanHandle;

        private string[] sendValue = new string[6];

        public Connect()
        {
            InitializeComponent();

            InitializeBasicComponents();
        }

        /// <summary>
        /// Initialization of PCAN-Basic components
        /// </summary>
        private void InitializeBasicComponents()
        {
            //// Creates the list for received messages
            ////
            //m_LastMsgsList = new System.Collections.ArrayList();
            //// Creates the delegate used for message reading
            ////
            //m_ReadDelegate = new ReadDelegateHandler(ReadMessages);
            //// Creates the event used for signalize incomming messages 
            ////
            //m_ReceiveEvent = new System.Threading.AutoResetEvent(false);
            //// Creates an array with all possible non plug-and-play PCAN-Channels
            ////
            //m_NonPnPHandles = new TPCANHandle[]
            //{
            //    PCANBasic.PCAN_ISABUS1,
            //    PCANBasic.PCAN_ISABUS2,
            //    PCANBasic.PCAN_ISABUS3,
            //    PCANBasic.PCAN_ISABUS4,
            //    PCANBasic.PCAN_ISABUS5,
            //    PCANBasic.PCAN_ISABUS6,
            //    PCANBasic.PCAN_ISABUS7,
            //    PCANBasic.PCAN_ISABUS8,
            //    PCANBasic.PCAN_DNGBUS1
            //};

            m_NonPnPHandles = new TPCANHandle[]
            {
            };

            // Fills and configures the Data of several comboBox components
            //
            FillComboBoxData();

            //// Prepares the PCAN-Basic's debug-Log file
            ////
            //ConfigureLogFile();
        }

        /// <summary>
        /// Configures the data of all ComboBox components of the main-form
        /// </summary>
        private void FillComboBoxData()
        {
            // Channels will be check
            //
            btnHwRefresh_Click(this, new EventArgs());
        }

        private void Connect_Load(object sender, EventArgs e)
        {
            //comPortCBox.DataSource = SerialPort.GetPortNames();
            //if (comPortCBox.Items.Count > 0)
            //    comPortCBox.SelectedIndex = 0;

            //String[] baudRateText = { "300", "600", "1200", "2400", "4800", "9600", "14400", "19200", "28800", "38400", "57600", "115200", "460800", "921600" };
            //baudRateCBox.Items.AddRange(baudRateText);
            //baudRateCBox.SelectedIndex = 12;

            //String[] dataBitsText = { "1", "2", "3", "4", "5", "6", "7", "8" };
            //dataBitsCBox.Items.AddRange(dataBitsText);
            //dataBitsCBox.SelectedIndex = 7;

            //String[] parityText = { "None", "odd", "Even", "Mark", "Space" };
            //parityCBox.Items.AddRange(parityText);
            //parityCBox.SelectedIndex = 0;

            //String[] stopBitsText = { "None", "One", "Two", "OnePointFive" };
            //stopBitsCBox.Items.AddRange(stopBitsText);
            //stopBitsCBox.SelectedIndex = 1;

            //String[] flowControlText = { "None", "SW", "HW", "SW/HW" };
            //flowControlCBox.Items.AddRange(flowControlText);
            //flowControlCBox.SelectedIndex = 0;
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        public string[] PassValue
        {
            get
            {
                //sendValue[0] = cbbChannel.Text;
                //sendValue[1] = baudRateCBox.Text;
                //sendValue[2] = dataBitsCBox.Text;
                //sendValue[3] = parityCBox.SelectedIndex + "";
                //sendValue[4] = stopBitsCBox.Text;
                //sendValue[5] = flowControlCBox.SelectedIndex + "";

                //MessageBox.Show("dialog = " + Convert.ToString(m_PcanHandle));

                sendValue[0] = Convert.ToString(m_PcanHandle);
                sendValue[1] = cbbChannel.Text;
                sendValue[2] = "";
                sendValue[3] = "";
                sendValue[4] = "";
                sendValue[5] = "";

                return sendValue;
            }
        }

        private void btnHwRefresh_Click(object sender, EventArgs e)
        {
            TPCANStatus stsResult;
            uint iChannelsCount;
            bool bIsFD;

            // Clears the Channel comboBox and fill it again with 
            // the PCAN-Basic handles for no-Plug&Play hardware and
            // the detected Plug&Play hardware
            //
            cbbChannel.Items.Clear();
            try
            {
                // Includes all no-Plug&Play Handles
                for (int i = 0; i < m_NonPnPHandles.Length; i++)
                    cbbChannel.Items.Add(FormatChannelName(m_NonPnPHandles[i]));

                // Checks for available Plug&Play channels
                //
                stsResult = PCANBasic.GetValue(PCANBasic.PCAN_NONEBUS, TPCANParameter.PCAN_ATTACHED_CHANNELS_COUNT, out iChannelsCount, sizeof(uint));
                if (stsResult == TPCANStatus.PCAN_ERROR_OK)
                {
                    TPCANChannelInformation[] info = new TPCANChannelInformation[iChannelsCount];

                    stsResult = PCANBasic.GetValue(PCANBasic.PCAN_NONEBUS, TPCANParameter.PCAN_ATTACHED_CHANNELS, info);
                    if (stsResult == TPCANStatus.PCAN_ERROR_OK)
                        // Include only connectable channels
                        //
                        foreach (TPCANChannelInformation channel in info)
                            if ((channel.channel_condition & PCANBasic.PCAN_CHANNEL_AVAILABLE) == PCANBasic.PCAN_CHANNEL_AVAILABLE)
                            {
                                bIsFD = (channel.device_features & PCANBasic.FEATURE_FD_CAPABLE) == PCANBasic.FEATURE_FD_CAPABLE;
                                cbbChannel.Items.Add(FormatChannelName(channel.channel_handle, bIsFD));
                            }
                }

                cbbChannel.SelectedIndex = cbbChannel.Items.Count - 1;
                connectButton.Enabled = cbbChannel.Items.Count > 0;

                if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                    MessageBox.Show(GetFormatedError(stsResult));
            }
            catch (DllNotFoundException)
            {
                MessageBox.Show("Unable to find the library: PCANBasic.dll !", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
        }

        /// <summary>
        /// Gets the formated text for a PCAN-Basic channel handle
        /// </summary>
        /// <param name="handle">PCAN-Basic Handle to format</param>
        /// <param name="isFD">If the channel is FD capable</param>
        /// <returns>The formatted text for a channel</returns>
        private string FormatChannelName(TPCANHandle handle, bool isFD)
        {
            TPCANDevice devDevice;
            byte byChannel;

            // Gets the owner device and channel for a 
            // PCAN-Basic handle
            //
            if (handle < 0x100)
            {
                devDevice = (TPCANDevice)(handle >> 4);
                byChannel = (byte)(handle & 0xF);
            }
            else
            {
                devDevice = (TPCANDevice)(handle >> 8);
                byChannel = (byte)(handle & 0xFF);
            }

            // Constructs the PCAN-Basic Channel name and return it
            //
            if (isFD)
                return string.Format("{0}:FD {1} ({2:X2}h)", devDevice, byChannel, handle);
            else
                return string.Format("{0} {1} ({2:X2}h)", devDevice, byChannel, handle);
        }

        /// <summary>
        /// Gets the formated text for a PCAN-Basic channel handle
        /// </summary>
        /// <param name="handle">PCAN-Basic Handle to format</param>
        /// <returns>The formatted text for a channel</returns>
        private string FormatChannelName(TPCANHandle handle)
        {
            return FormatChannelName(handle, false);
        }

        /// <summary>
        /// Help Function used to get an error as text
        /// </summary>
        /// <param name="error">Error code to be translated</param>
        /// <returns>A text with the translated error</returns>
        private string GetFormatedError(TPCANStatus error)
        {
            StringBuilder strTemp;

            // Creates a buffer big enough for a error-text
            //
            strTemp = new StringBuilder(256);
            // Gets the text using the GetErrorText API function
            // If the function success, the translated error is returned. If it fails,
            // a text describing the current error is returned.
            //
            if (PCANBasic.GetErrorText(error, 0, strTemp) != TPCANStatus.PCAN_ERROR_OK)
                return string.Format("An error occurred. Error-code's text (0x{0:X}) couldn't be retrieved", error);
            else
                return strTemp.ToString();
        }

        private void cbbChannel_SelectedIndexChanged(object sender, EventArgs e)
        {
            //bool bNonPnP;
            string strTemp;

            // Get the handle fromt he text being shown
            //
            strTemp = cbbChannel.Text;
            strTemp = strTemp.Substring(strTemp.IndexOf('(') + 1, 3);

            strTemp = strTemp.Replace('h', ' ').Trim(' ');

            // Determines if the handle belong to a No Plug&Play hardware 
            //
            m_PcanHandle = Convert.ToUInt16(strTemp, 16);
        }
    }
}
