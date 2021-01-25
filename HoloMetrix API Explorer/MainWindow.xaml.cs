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

using HoloMetrix.Net.Remote;
using HoloMetrix.Net.Remote.SoundIntensity;
using HoloMetrix.Net.Utilities;
using HoloMetrix_API_Explorer.Dialogs;

namespace HoloMetrix_API_Explorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        StatusText statusText = new StatusText() { AppStatus = "", SIAppStatus = "",ConnectionStatus = "Not Connected" };

        BluetoothDevice device;
        RemoteSession remoteSession;
        SoundIntensityApp siApp;
        CuboidObject dut;
        CuboidSurface surface;
        Measurement measurement;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = statusText;
        }

        private void Connect_DevicePicker(object sender, RoutedEventArgs e)
        {
            remoteSession = Bluetooth.TryConnectToHoloMetrixHub();
            if(remoteSession != null)
            {
                remoteSession.RemoteChanged_AppStatus += RemoteSession_RemoteChanged_AppStatus;
            }
        }

        private void BluetoothConnection_ConnectionLost(object sender, EventArgs e)
        {
            statusText.ConnectionStatus = "Connection Lost";
        }

        private void BluetoothConnection_ConnectionEstablished(object sender, ConnectionEstablishedEventArgs e)
        {
            statusText.ConnectionStatus = "Connected";
        }

        private void GetDevices(object sender, RoutedEventArgs e)
        {
            GetDevices_Dialog devicesDialog = new GetDevices_Dialog();
            if (devicesDialog.ShowDialog() == true)
            {
                device = devicesDialog.SelectedDevice;
                Console.WriteLine("Selected device: " + device.DeviceInfo.DeviceName);
            }
        }

        private void ConnectToDevice(object sender, RoutedEventArgs e)
        {
            if (device != null)
            {
                remoteSession = device.TryConnectToService();
                if (remoteSession != null)
                {
                    remoteSession.RemoteChanged_AppStatus += RemoteSession_RemoteChanged_AppStatus;
                }
            }
        }

        private void RemoteSession_RemoteChanged_AppStatus(object sender, RemoteChangedEventArgs<RemoteAppStatus> e)
        {
            statusText.AppStatus = e.Value.ToString();
        }

        private void LaunchApp(object sender, RoutedEventArgs e)
        {
            if(remoteSession != null)
            {
                siApp = remoteSession.TryLaunchApp<SoundIntensityApp>();
                siApp.RemoteChanged_AppStatus += SiApp_RemoteChanged_AppStatus;
                siApp.RemoteChanged_ConnectionStatus += SiApp_RemoteChanged_ConnectionStatus;
            }
        }
        private void SiApp_RemoteChanged_ConnectionStatus(object sender, RemoteChangedEventArgs<bool> e)
        {
            statusText.ConnectionStatus = e.Value.ToString();
        }

        private void SiApp_RemoteChanged_AppStatus(object sender, RemoteChangedEventArgs<SoundIntensityApp.AppStatus> e)
        {
            statusText.SIAppStatus = e.Value.ToString();
        }

        private void Setup(object sender, RoutedEventArgs e)
        {
            if(siApp != null)
            {
                Setup_Dialog setupDialog = new Setup_Dialog(dut, surface);
                if(setupDialog.ShowDialog() == true)
                {
                    dut = setupDialog.Object;
                    surface = setupDialog.Surface;
                    measurement = siApp.Setup(dut, surface);
                    measurement.SelectSegmentRequested += Measurement_SelectSegmentRequested;
                    measurement.StartMeasurementRequested += Measurement_StartMeasurementRequested;
                }
            }
        }

        private void Measurement_StartMeasurementRequested(object sender, MeasurementEventArgs e)
        {
            Console.WriteLine("Start Measurement Requested");
        }

        private void Measurement_SelectSegmentRequested(object sender, SelectSegmentEventArgs e)
        {
            measurement.SelectSegment(e.SegmentGroupIndex, e.SegmentIndex);
        }

        private void SelectSegment(object sender, RoutedEventArgs e)
        {
            if (measurement != null)
            {
                SelectSegment_Dialog selectDialog = new SelectSegment_Dialog(measurement.CurrentSegment[0], measurement.CurrentSegment[1]);
                if (selectDialog.ShowDialog() == true)
                {
                    measurement.SelectSegment(selectDialog.SegmentGroupIndex, selectDialog.SegmentIndex);
                }
            }
        }

        private void SetGradient(object sender, RoutedEventArgs e)
        {
            if (measurement != null)
            {
                Gradient_Dialog gradientDialog = new Gradient_Dialog();
                if (gradientDialog.ShowDialog() == true)
                {
                    measurement.SetAnalysisGradient(gradientDialog.Gradient);
                    measurement.ClampGradient(gradientDialog.IsClamped, gradientDialog.Min, gradientDialog.Max);
                }
            }
        }

        private void StartMeasurement(object sender, RoutedEventArgs e)
        {
            if(measurement != null)
            {
                Measuring_Dialog measuringDialog = new Measuring_Dialog(measurement);
                measuringDialog.ShowDialog();
            }
        }

        private void SetResult(object sender, RoutedEventArgs e)
        {
            if(measurement != null)
            {
                Results_Dialog resultsDialog = new Results_Dialog(measurement);
                resultsDialog.ShowDialog();
            }
        }

        private void SendStatusMessages(object sender, RoutedEventArgs e)
        {
            if(measurement != null)
            {
                List<StatusMessage> messages = new List<StatusMessage>();
                messages.Add(new StatusMessage(StatusMessage.Severity.Green, "First Status"));
                messages.Add(new StatusMessage(StatusMessage.Severity.Yellow, "Second Status"));
                messages.Add(new StatusMessage(StatusMessage.Severity.Red, "Third Status"));
                measurement.SetStatusMessages(messages);
            }
        }

        private void GetAnchor(object sender, RoutedEventArgs e)
        {

        }

        private void ConnectLaunchSetup(object sender, RoutedEventArgs e)
        {
            remoteSession = Bluetooth.TryConnectToHoloMetrixHub();
            if (remoteSession == null)
                return;
            remoteSession.RemoteChanged_AppStatus += RemoteSession_RemoteChanged_AppStatus;
            siApp = remoteSession.TryLaunchApp<SoundIntensityApp>();
            if (siApp == null)
                return;
            dut = new CuboidObject(100f, 100f, 100f);
            surface = new CuboidSurface
                (
                100f, 100f, 100f,
                2, 2, 2,
                new SurfaceState[6]
                {
                    SurfaceState.Measure,
                    SurfaceState.Measure,
                    SurfaceState.Measure,
                    SurfaceState.Measure,
                    SurfaceState.Solid,
                    SurfaceState.Measure
                }
                );
            measurement = siApp.Setup(dut, surface);
            measurement.SelectSegmentRequested += Measurement_SelectSegmentRequested;
            measurement.StartMeasurementRequested += Measurement_StartMeasurementRequested;
        }
    }

    public class StatusText
    {
        public string AppStatus { get; set; } = "";
        public string SIAppStatus { get; set; } = "";
        public string ConnectionStatus { get; set; } = "";
    }
}
