using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DeployLib;
using System.Media;

namespace Deploy
{
    public partial class ConfigureControl : UserControl
    {
        public class ScanEventArgs : EventArgs
        {
            public int DeviceId { get; protected set; }
            public Configuration Configuration { get; protected set; }
            public ScanEventArgs(int deviceId)
            {
                DeviceId = deviceId;
            }
            public ScanEventArgs(Configuration configuration)
            {
                Configuration = configuration;
            }
        }

        public enum MessageType
        {
            MESSAGE_TYPE_ERROR = -1,
            MESSAGE_TYPE_INFO = 0,
            MESSAGE_TYPE_SUCCESS = 1,
        }

        public delegate void ScanEventHandler(object sender, ScanEventArgs e);
        public event ScanEventHandler ConfigurationScanned;
        public event ScanEventHandler DeviceScanned;


        //    20180217091500
        // 14 YYYYMMDDhhmmss
        // 12   YYMMDDhhmmss
        // 10   YYMMDDhhmm
        //  8   YYMMDDhh
        //  6     MMDDhh
        //  4     MMDD
        //  2       DD
        // r=rate (100Hz), g=range (+/-8g), d=duration (hours), b=begin (YYMMDDhh[mm]), s=session (9 digits)
        private long lastInput = 0;
        private bool inputFinished = true;
        public string lastInputString = null;
        public void ForgetLastInput() { lastInputString = null; }

        private DateTime? ParseDateTime(string value)
        {
            DateTime now = DateTime.Now;
            int year = -1;  // auto
            int month = -1; // auto
            int day = -1;   // auto
            int hour = 0;   // default midnight
            int minute = 0; // default o'clock
            int second = 0; // default zero

            if (value == null) { Console.WriteLine("ERROR: Date null"); return null; }
            value = value.Trim().ToLower();
            if (value.Length <= 0) { Console.WriteLine("ERROR: Date empty"); return null; }
            if (value.Length % 2 != 0) { Console.WriteLine("ERROR: Date non-even digits"); return null; } // must be even length
            if (value.Length < 2 || value.Length > 14) { Console.WriteLine("ERROR: Date invalid length"); return null; }

            // Seconds (suffix)
            if (value.Length >= 12)
            {
                second = int.Parse(value.Substring(value.Length - 2));
                value = value.Substring(0, value.Length - 2);
            }

            // Minutes (suffix)
            if (value.Length >= 10)
            {
                minute = int.Parse(value.Substring(value.Length - 2));
                value = value.Substring(0, value.Length - 2);
            }

            // Year (prefix)
            if (value.Length >= 8)
            {
                if (value.Length >= 10)
                {
                    year = int.Parse(value.Substring(0, 4));
                    value = value.Substring(4);
                }
                else
                {
                    year = int.Parse(value.Substring(0, 2)) + 2000;
                    value = value.Substring(2);
                }
            }

            // Hours (suffix)
            if (value.Length >= 6)
            {
                hour = int.Parse(value.Substring(value.Length - 2));
                value = value.Substring(0, value.Length - 2);
            }

            // Months (prefix)
            if (value.Length >= 4)
            {
                month = int.Parse(value.Substring(0, 2));
                value = value.Substring(2);
            }

            // Days (prefix)
            if (value.Length >= 2)
            {
                day = int.Parse(value.Substring(0, 2));
                value = value.Substring(2);
            }

            // Automatic day
            if (day < 0) { day = now.Day; }

            // Automatic month
            if (month < 0) { month = (now.Month + ((day < now.Day) ? 1 : 0) - 1) % 12 + 1; }

            // Automatic year
            if (year < 0) { year = now.Year + ((month < now.Month) ? 1 : 0); }

            try
            {
                return new DateTime(year, month, day, hour, minute, second);
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: Problem constructing date ({e.Message}) for {year}-{month}-{day} {hour}:{minute}:{second}");
                return null;
            }
        }

        public Configuration ParseConfig(string value)
        {
            try
            {
                Configuration configuration = new Configuration();
                if (value == null) { return null; }
                value = value.Trim().ToLower();
                char currentSetting = (char)0;
                string currentValue = "";
                for (int i = 0; i <= value.Length; i++)
                {
                    char c = (i < value.Length) ? value[i] : (char)0;
                    if (c >= '0' && c <= '9')
                    {
                        currentValue += c;
                    }
                    else
                    {
                        if (currentValue.Length > 0)
                        {
                            // Default setting for bare values
                            if (currentSetting == (char)0)
                            {
                                currentSetting = 's';
                            }

                            switch (currentSetting)
                            {
                                case 's':
                                    configuration.SessionId = uint.Parse(currentValue);
                                    break;
                                case 'b':
                                    DateTime? parsedBegin = ParseDateTime(currentValue);
                                    if (!parsedBegin.HasValue)
                                    {
                                        Console.WriteLine("ERROR: Cannot parse begin timestamp: " + currentValue);
                                        return null;
                                    }
                                    configuration.Start = parsedBegin.Value;
                                    break;
                                case 'd':
                                    // Hours to seconds
                                    configuration.Duration = int.Parse(currentValue) * 60 * 60;
                                    break;
                                case 'r':
                                    configuration.Rate = int.Parse(currentValue);
                                    break;
                                case 'g':
                                    configuration.Range = int.Parse(currentValue);
                                    break;
                                default:
                                    Console.WriteLine("ERROR: Unhandled setting: " + currentSetting);
                                    return null;
                            }
                        }
                        currentSetting = c;
                        currentValue = "";
                    }
                }
                return configuration;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Problem parsing configuration: " + e.Message);
                return null;
            }
        }

        public ConfigureControl()
        {
            InitializeComponent();
            SetMessage(null);
        }

        public void SetMessage(string notice, MessageType messageType = MessageType.MESSAGE_TYPE_INFO)
        {
            Notice = notice;
            switch (messageType)
            {
                case MessageType.MESSAGE_TYPE_SUCCESS:
                    labelNotice.BackColor = Color.DarkGreen;
                    labelNotice.ForeColor = Color.White;
                    SystemSounds.Asterisk.Play();
                    break;
                case MessageType.MESSAGE_TYPE_ERROR:
                    labelNotice.BackColor = Color.Red;
                    labelNotice.ForeColor = Color.White;
                    SystemSounds.Exclamation.Play();
                    break;
                default: // MessageType.MESSAGE_TYPE_INFO:
                    labelNotice.BackColor = SystemColors.Info;
                    labelNotice.ForeColor = SystemColors.InfoText;
                    break;
            }
        }

        protected String Notice
        {
            set
            {
                labelNotice.Text = value ?? "";
                labelNotice.Visible = (labelNotice.Text.Length != 0);
            }
            get
            {
                return labelNotice.Text;
            }
        }

        private void groupBoxOuter_Enter(object sender, EventArgs e)
        {

        }

        protected static bool IsNumeric(string s)
        {
            if (s == null || s.Length < 1) { return false; }
            for (var i = 0; i < s.Length; i++)
            {
                if (!char.IsDigit(s[i])) { return false; }
            }
            return true;
        }

        public void AddInput(char input)
        {
            lastInput = DateTime.Now.Ticks;

            // Clear on new input when previous input finished
            if (inputFinished)
            {
                textBoxConfigurationString.Text = "";
                inputFinished = false;
                textBoxConfigurationString.Enabled = true;
            }

            // Control
            if (input < 32)
            {
                if (input == 8 && textBoxConfigurationString.Text.Length > 0)
                {
                    textBoxConfigurationString.Text = textBoxConfigurationString.Text.Substring(0, textBoxConfigurationString.Text.Length - 1);
                }
                else if (input == 27)
                {
                    Expire();
                    SetMessage("", MessageType.MESSAGE_TYPE_INFO);
                }
                else if (input == 13)
                {
                    Submit();
                }
            }
            else // ASCII
            {
                textBoxConfigurationString.Text += input.ToString().ToLower();
            }

            textBoxConfigurationString.SelectionLength = 0;
            textBoxConfigurationString.SelectionStart = textBoxConfigurationString.Text.Length;
        }

        public void Submit(string newString = null)
        {
            if (newString != null)
            {
                textBoxConfigurationString.Text = newString;
            }

            if (textBoxConfigurationString.Text.Length > 0)
            {
                if (textBoxConfigurationString.Text.Length == 5 && IsNumeric(textBoxConfigurationString.Text))
                {
                    // A strictly numeric five digits is interpreted as a device barcode
                    int deviceId = int.Parse(textBoxConfigurationString.Text);
                    // Use device barcode
                    Console.WriteLine("DEVICE: #" + deviceId);
                    SetMessage("DEVICE: #" + deviceId);
                    // Raise event
                    DeviceScanned?.Invoke(this, new ScanEventArgs(deviceId));
                }
                else // ...anything else is parsed as a configuration string.
                {
                    Configuration configuration = ParseConfig(textBoxConfigurationString.Text);
                    if (configuration == null)
                    {
                        Console.WriteLine("ERROR: Configuration string could not be parsed.");
                        SetMessage("ERROR: Problem understanding configuration.", MessageType.MESSAGE_TYPE_ERROR);
                    }
                    else if (!configuration.Valid)
                    {
                        Console.WriteLine("ERROR: Invalid configuration.");
                        SetMessage("ERROR: Invalid configuration.", MessageType.MESSAGE_TYPE_ERROR);
                    }
                    else if (configuration.Within > 0)
                    {
                        Console.WriteLine("ERROR: Expired configuration.");
                        SetMessage("ERROR: Configuration ends in the past.", MessageType.MESSAGE_TYPE_ERROR);
                    }
                    else if ((configuration.Start - DateTime.Now).TotalDays > 180)
                    {
                        Console.WriteLine("ERROR: Configuration too far in the future.");
                        SetMessage("ERROR: Configuration too far in the future.", MessageType.MESSAGE_TYPE_ERROR);
                    }
                    else
                    {
                        if (lastInputString != null && lastInputString == textBoxConfigurationString.Text)
                        {
                            Console.WriteLine("ERROR: Duplicate successive input: " + textBoxConfigurationString.Text);
                            SetMessage("ERROR: Duplicated configuration detected and ignored (F8 to override).", MessageType.MESSAGE_TYPE_ERROR);
                        }
                        else
                        {
                            lastInputString = textBoxConfigurationString.Text;
                            // Use configuration
                            Console.WriteLine("CONFIGURATION: " + configuration.ToString());
                            // SetMessage("CONFIGURING: " + warning + configuration.ToString());
                            // Raise event
                            ConfigurationScanned?.Invoke(this, new ScanEventArgs(configuration));
                        }
                    }
                }
            }

            inputFinished = true;
            textBoxConfigurationString.SelectAll();
            textBoxConfigurationString.Enabled = false;
        }

        // Remove non-committed setting
        public void Expire()
        {
            inputFinished = true;
            textBoxConfigurationString.SelectAll();
            textBoxConfigurationString.Enabled = false;
            textBoxConfigurationString.Text = "";
        }

        private void timerTimeout_Tick(object sender, EventArgs e)
        {
            if (!inputFinished && (DateTime.Now.Ticks - lastInput) / TimeSpan.TicksPerMillisecond > 5000)
            {
                Expire();
                SetMessage("(input timed-out -- ignored)");
            }
        }

        private void labelNotice_Click(object sender, EventArgs e)
        {
            Expire();
            SetMessage("", MessageType.MESSAGE_TYPE_INFO);
        }
    }
}
