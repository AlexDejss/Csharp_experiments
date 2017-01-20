using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Forms;
using NAudio;
using NAudio.Wave;
using NAudio.WindowsMediaFormat;
using System.Threading;

namespace swapScroll
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
           // nIcon.MouseDown += (TrayIconClick);
            loadDevice();
            readCurrentVolume();
        }

        /// <summary>
        /// Global Requer Values
        /// </summary>
        /// 
        bool off_on = true;

        NotifyIcon nIcon = new NotifyIcon();

        string[] Devices = new string[20];
        int[] idDevices = new int[20];
        bool fastScroll = true;

        /// <summary>
        /// DLL Import
        /// </summary>


        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        // Activate an application window.
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);



        private void TrayIconClick(object sender, EventArgs e)
        {
            this.WindowState = WindowState.Normal;
            nIcon.Visible = false;
            this.Show();
        }

        private void swapClick(object sender, RoutedEventArgs e)
        {
            if (off_on)
            {
                
                off_on = false;
                SwapOnOFF.Content = "Off";
                SwapOnOFF.Margin = new Thickness(58, 0, 0, 0);
                SwapOnOFF.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 195, 195, 195));
                Options.IsEnabled = false;
                MicVolume.Value =0;
            }
            else
            {
                off_on = true;
                SwapOnOFF.Content = "On";
                SwapOnOFF.Margin = new Thickness(0, 0, 58, 0);
                SwapOnOFF.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 134, 194, 255));
                Options.IsEnabled = true;
            }
        }

        public void loadDevice()
        {
            int waveInDevices = WaveIn.DeviceCount;
            for (int waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
            {
                WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveInDevice);
                EnableDevices.Items.Add(deviceInfo.ProductName + ")");
                Devices[waveInDevice] = deviceInfo.ProductName + ")";
                idDevices[waveInDevice] = waveInDevice;
            }
            EnableDevices.SelectedIndex = 0;
        }

        private void closeActionCust(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = System.Windows.MessageBox.Show("Do you realy want close program?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.No;
        }

        public void readCurrentVolume()
        {
            var waveIn = new WaveInEvent();
            waveIn.DataAvailable += WaveOnDataAvailable;
            waveIn.WaveFormat = new WaveFormat(8000, 1);
            waveIn.StartRecording();
        }

        public void WaveOnDataAvailable(object sender, WaveInEventArgs e)
        {
            int sendReady = 0;
            for (int index = 0; index < e.BytesRecorded; index += 2)
            {
                short sample = (short)((e.Buffer[index + 1] << 8) | e.Buffer[index + 0]);
                
                float amplitude = sample / 32768f;

                if (amplitude > 0.1f && off_on == true)
                {
                    float level = Math.Abs(amplitude); // from 0 to 1
                    MicVolume.Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        MicVolume.Value = level * 100;
                    }));
                    if (amplitude > 0.4f)
                    {
                        if (fastScroll == true)
                        {
                            amplitude = 0f;
                            SendKeys.SendWait("{PGDN}");
                        }
                        else
                        {
                            crutch.Dispatcher.BeginInvoke(new Action(delegate ()
                            {
                                sendReady = Convert.ToInt32(crutch.Text);
                            }));
                            if (sendReady ==0)
                            {
                                crutch.Dispatcher.BeginInvoke(new Action(delegate ()
                                {
                                    crutch.Text = "1";
                                }));
                                SendKeys.SendWait("{PGDN}");
                                Thread sleep = new Thread(SleepAdv);
                            }
                            
                        }
                    }
                    else
                    {
                        MicVolume.Dispatcher.BeginInvoke(new Action(delegate ()
                        {
                            return;
                        }));
                    }             
                }
            }
        }

        public void SleepAdv()
        {
            Thread.Sleep(3000);
            crutch.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                crutch.Text ="0";
            }));
        }

        private void swapMode(object sender, RoutedEventArgs e)
        {
            if (fastScroll)
            {

                fastScroll = false;
                OnOffFastScroll.Content = "Off"; ;
                OnOffFastScroll.Margin = new Thickness(0, 0, 58, 0);
                OnOffFastScroll.Margin = new Thickness(58, 0, 0, 0);
                OnOffFastScroll.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 195, 195, 195));
            }
            else
            {
                fastScroll = true;
                OnOffFastScroll.Content = "On";
                OnOffFastScroll.Margin = new Thickness(0, 0, 58, 0);
                OnOffFastScroll.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 134, 194, 255));

            }
        }
    }
}

