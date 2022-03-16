// This library Handles the recording functions, thanks to Sskodje
using ScreenRecorderLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
// Here we need this library to register the hotkey
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Grabby2
{
    public partial class Grabby : Form
    {
        // Here we import user32.dll system library and declare the function we want to use
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        Recorder _rec;
        private bool _showClicks;
        private bool _recordAudioIn;
        private bool _recordAudioOut;
        private string grabbyFolder;
        public Dictionary<string, string> AudioInputsList { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> AudioOutputsList { get; set; } = new Dictionary<string, string>();
        public bool IsRecording { get; set; }
        public Grabby()
        {
            InitializeComponent();
            // Here we register an unique ID we use to check which hotkey was pressed
            int UniqueHotkeyId = 1;
            // This converts the hexadecimal value that identifies F9 to its int value
            int HotKeyCode = (int)Keys.F9;
            // We call our register function and save the result to this variable
            Boolean HotkeyRegister = RegisterHotKey(this.Handle, UniqueHotkeyId, 0x0000, HotKeyCode);

            // This are our recording variables
            IsRecording = false;
            _showClicks = Properties.Settings.Default.clickShow;
            _recordAudioIn = false;
            _recordAudioOut = false;
            var myDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            grabbyFolder = Path.Combine(myDocumentsFolder, "Grabby");
            if (!Directory.Exists(grabbyFolder))
            {
                Directory.CreateDirectory(grabbyFolder);
            }
            LoadUserPrefs();
            label9.Text = grabbyFolder;
        }

        private void button1_MouseEnter(object sender, EventArgs e)
        {
            button1.BackColor = Color.RoyalBlue;
        }

        private void button1_MouseLeave(object sender, EventArgs e)
        {
            if (button1.BackColor != Color.Transparent)
            {
                button1.BackColor = Color.Transparent;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // StopRecording();
            this.Close();
        }

        private void button1_MouseDown(object sender, MouseEventArgs e)
        {
            button1.BackColor = Color.DarkSlateBlue;
        }

        private void button1_MouseUp(object sender, MouseEventArgs e)
        {
            if (button1.BackColor != Color.RoyalBlue)
            {
                button1.BackColor = Color.RoyalBlue;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button2_MouseEnter(object sender, EventArgs e)
        {
            button2.BackColor = Color.RoyalBlue;
        }

        private void button2_MouseLeave(object sender, EventArgs e)
        {
            if (button2.BackColor != Color.Transparent)
            {
                button2.BackColor = Color.Transparent;
            }
        }

        private void button2_MouseDown(object sender, MouseEventArgs e)
        {
            button2.BackColor = Color.DarkSlateBlue;
        }

        private void button2_MouseUp(object sender, MouseEventArgs e)
        {
            if (button2.BackColor != Color.Transparent)
            {
                button2.BackColor = Color.Transparent;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            IsRecording = !IsRecording;
            if (IsRecording)
            {
                // Here we begin the recording process. First adjust the recording settings
                RecorderOptions options = new RecorderOptions
                {
                    MouseOptions = new MouseOptions
                    {
                        //Displays a colored dot under the mouse cursor when the left mouse button is pressed.
                        IsMouseClicksDetected = _showClicks,
                        MouseClickDetectionColor = "#FFFF00",
                        MouseRightClickDetectionColor = "#FFFF00",
                        MouseClickDetectionRadius = 20,
                        MouseClickDetectionDuration = 100,
                        IsMousePointerEnabled = true,
                        /* Polling checks every millisecond if a mouse button is pressed.
                           Hook works better with programmatically generated mouse clicks, but may affect
                           mouse performance and interferes with debugging.*/
                        MouseClickDetectionMode = MouseDetectionMode.Hook
                    },
                    VideoOptions = new VideoOptions
                    {
                        Quality = qualityTrackBar.Value * 10,
                    },
                    AudioOptions = new AudioOptions
                    {
                        IsAudioEnabled = checkBox1.Checked,
                        Bitrate = AudioBitrate.bitrate_128kbps,
                        Channels = AudioChannels.Stereo,
                        IsOutputDeviceEnabled = _recordAudioOut,
                        IsInputDeviceEnabled = _recordAudioIn,
                        AudioOutputDevice = comboBox2.SelectedValue as string,
                        AudioInputDevice = comboBox1.SelectedValue as string,
                    }
                };
                label1.Text = "STOP";
                ControlsEnabled(false);
                string _videopath = Path.Combine(grabbyFolder, "Recording_.mp4".AppendTimeStamp());
                _rec = Recorder.CreateRecorder(options);
                _rec.Record(_videopath);
                this.WindowState = FormWindowState.Minimized;
                notifyIcon1.ShowBalloonTip(3, "Recording...", "Grabby is recording your screen! to open the app just double click the grabby icon under this message", ToolTipIcon.Info);
            }
            else
            {
                // Here we stop the recording process
                _rec.Stop();
                label1.Text = "REC";
                ControlsEnabled(true);
            }
        }

        private void Grabby_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopRecording(_rec);
            SaveUserPrefs();
            notifyIcon1.Visible = false;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            recordPicBox.Image = Properties.Resources.rec_down;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (IsRecording == false)
            {
                recordPicBox.Image = Properties.Resources.rec_button;
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            _showClicks = !_showClicks;
            if (_showClicks)
            {
                btnShowClicks.ForeColor = Color.White;
                btnShowClicks.Text = "ON";
                btnShowClicks.BackColor = Color.Green;
            }
            else
            {
                btnShowClicks.ForeColor = Color.Black;
                btnShowClicks.Text = "OFF";
                btnShowClicks.BackColor = Color.DimGray;
            }
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            if (IsRecording == false)
            {
                recordPicBox.Image = Properties.Resources.rec_hover;
            }

        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            if (IsRecording == false)
            {
                recordPicBox.Image = Properties.Resources.rec_button;
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            _recordAudioIn = !_recordAudioIn;
            if (checkBox1.Checked)
            {
                activateInputbtn(_recordAudioIn);
            }
        }

        private void comboBox1_EnabledChanged(object sender, EventArgs e)
        {
            if (comboBox1.Enabled == true)
            {
                AudioInputsList = Recorder.GetSystemAudioDevices(AudioDeviceSource.InputDevices);
                foreach (var kvp in AudioInputsList)
                {
                    comboBox1.Items.Add(kvp.Value);
                }
            }
            else
            {
                comboBox1.Items.Clear();
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            _recordAudioOut = !_recordAudioOut;
            if (checkBox1.Checked)
            {
                activateOutputbtn(_recordAudioOut);
            }
        }

        private void comboBox2_EnabledChanged(object sender, EventArgs e)
        {
            if (comboBox2.Enabled == true)
            {
                AudioOutputsList = Recorder.GetSystemAudioDevices(AudioDeviceSource.OutputDevices);
                foreach (var kvp in AudioOutputsList)
                {
                    comboBox2.Items.Add(kvp.Value);
                }
            }
            else
            {
                comboBox2.Items.Clear();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                // We activate the audio input button
                _recordAudioIn = true;
                activateInputbtn(_recordAudioIn);
                button4.Enabled = true;

                // We activate the audio output button
                _recordAudioOut = true;
                activateOutputbtn(_recordAudioOut);
                button5.Enabled = true;
            }
            else
            {
                button4.Enabled = false;
                button5.Enabled = false;
            }
        }

        private void Grabby_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                if (IsRecording)
                {
                    toolStripMenuItem2.Visible = true;
                }
                notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = grabbyFolder;
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                grabbyFolder = folderBrowserDialog1.SelectedPath;
                label9.Text = grabbyFolder;
            }
            else
            {
                return;
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // This is also a close button, but we have to also hide the icon before we close
            StopRecording(_rec);
            notifyIcon1.Visible = false;
            toolStripMenuItem1.Visible = false;
            SaveUserPrefs();
            this.Close();
        }
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            StopRecording(_rec);
            ControlsEnabled(true);
        }

        private void StopRecording(Recorder recorder)
        {
            if (IsRecording == true)
            {
                recorder.Stop();
                label1.Text = "REC";
                recordPicBox.Image = Properties.Resources.rec_button;
                btnShowClicks.Enabled = true;
                toolStripMenuItem2.Visible = false;
                IsRecording = false;
            }
        }
        private void SaveUserPrefs()
        {
            // Save folder path selected by the user
            Properties.Settings.Default.qualityLevel = qualityTrackBar.Value;
            Properties.Settings.Default.recordingsPath = grabbyFolder;
            Properties.Settings.Default.clickShow = _showClicks;
            Properties.Settings.Default.Save();
        }

        private void LoadUserPrefs()
        {
            if (Properties.Settings.Default.recordingsPath.Length > 1)
            {
                grabbyFolder = Properties.Settings.Default.recordingsPath;
            }
            if (Properties.Settings.Default.qualityLevel != 0)
            {
                qualityTrackBar.Value = Properties.Settings.Default.qualityLevel;
            }
            if (Properties.Settings.Default.clickShow)
            {
                btnShowClicks.ForeColor = Color.White;
                btnShowClicks.Text = "ON";
                btnShowClicks.BackColor = Color.Green;
            }
        }

        // This function is for disabling all user controls when recording
        private void ControlsEnabled(bool enable)
        {
            if (enable)
            {
                checkBox1.Enabled = true;
                btnShowClicks.Enabled = true;
                button4.Enabled = true;
                button5.Enabled = true;
                button6.Enabled = true;
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                qualityTrackBar.Enabled = true;
            }
            else
            {
                checkBox1.Enabled = false;
                btnShowClicks.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
                button6.Enabled = false;
                comboBox1.Enabled = false;
                comboBox2.Enabled = false;
                qualityTrackBar.Enabled = false;
            }
        }

        // This part is for allowing the user to move the window by clicking and dragging anywhere in the form
        private const int WM_NCHITTEST = 0x0084;
        private const int HTCLIENT = 1;
        private const int HTCAPTION = 2;
        protected override void WndProc(ref Message m)
        {
            // Adding this code for catching if our hotkey is pressed
            if (m.Msg == 0x0312)
            {
                int id = m.WParam.ToInt32();

                if (id == 1)
                {
                    if (IsRecording == true)
                    {
                        toolStripMenuItem2_Click(this, null);
                    }
                    else
                    {
                        pictureBox1_Click(this, null);
                    }

                }
            }
            // This is for moving the window
            base.WndProc(ref m);
            switch (m.Msg)
            {
                case WM_NCHITTEST:
                    if (m.Result == (IntPtr)HTCLIENT)
                    {
                        m.Result = (IntPtr)HTCAPTION;
                    }
                    break;
            }

        }

        private void activateOutputbtn(bool active)
        {
            if (active)
            {
                button5.ForeColor = Color.White;
                button5.Text = "Output";
                comboBox2.Enabled = true;
                label7.ForeColor = Color.White;
                button5.BackColor = Color.RoyalBlue;
            }
            else
            {
                button5.ForeColor = Color.Black;
                button5.Text = "Output";
                comboBox2.Enabled = false;
                label7.ForeColor = Color.Gray;
                button5.BackColor = Color.DimGray;
            }

        }
        private void activateInputbtn(bool active)
        {
            if (active)
            {
                button4.ForeColor = Color.White;
                button4.Text = "Input";
                comboBox1.Enabled = true;
                label5.ForeColor = Color.White;
                button4.BackColor = Color.RoyalBlue;
            }
            else
            {
                button4.ForeColor = Color.Black;
                button4.Text = "Input";
                comboBox1.Enabled = false;
                label5.ForeColor = Color.Gray;
                button4.BackColor = Color.DimGray;
            }
        }

        private void comboBox1_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            AudioInputsList = Recorder.GetSystemAudioDevices(AudioDeviceSource.InputDevices);
            foreach (var kvp in AudioInputsList)
            {
                comboBox1.Items.Add(kvp.Value);
            }

        }

        private void comboBox2_Click(object sender, EventArgs e)
        {
            comboBox2.Items.Clear();
            AudioOutputsList = Recorder.GetSystemAudioDevices(AudioDeviceSource.OutputDevices);
            foreach (var kvp in AudioOutputsList)
            {
                comboBox2.Items.Add(kvp.Value);
            }
        }
    }
}
