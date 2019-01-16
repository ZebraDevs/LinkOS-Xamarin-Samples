/***********************************************
 * CONFIDENTIAL AND PROPRIETARY 
 * 
 * The source code and other information contained herein is the confidential and exclusive property of
 * ZIH Corp. and is subject to the terms and conditions in your end user license agreement.
 * This source code, and any other information contained herein, shall not be copied, reproduced, published, 
 * displayed or distributed, in whole or in part, in any medium, by any means, for any purpose except as
 * expressly permitted under such license agreement.
 * 
 * Copyright ZIH Corp. 2018
 * 
 * ALL RIGHTS RESERVED
 ***********************************************/

using System;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Zebra.Sdk.Comm;
using Zebra.Sdk.Printer;
using Zebra.Sdk.Printer.Discovery;

namespace XamarinDevDemo {

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConnectivityDemoPage : ContentPage {

        private const string MacAddressSettingsKey = "MacAddress";
        private const string IpAddressSettingsKey = "IpAddress";
        private const string SymbolicNameSettingsKey = "SymbolicName";

        private const string TestLabelZpl = @"^XA
                                              ^FO17,16
                                              ^GB379,371,8^FS
                                              ^FT65,255
                                              ^A0N,135,134
                                              ^FDTEST^FS
                                              ^XZ";

        private const string TestLabelCpcl = "! 0 200 200 406 1\r\n" +
                                             "ON-FEED IGNORE\r\n" +
                                             "BOX 20 20 380 380 8\r\n" +
                                             "T 0 6 137 177 TEST\r\n" +
                                             "PRINT\r\n";

        public enum ConnectionType {
            Network,
            Bluetooth,
            UsbDirect,
            UsbDriver
        }

        public ConnectivityDemoPage() {
            InitializeComponent();

            ConnectionTypePicker.SelectedIndex = 0;

            IpAddressEntry.TextChanged += (object sender, TextChangedEventArgs e) => {
                Application.Current.Properties[IpAddressSettingsKey] = e.NewTextValue;
            };

            MacAddressEntry.TextChanged += (object sender, TextChangedEventArgs e) => {
                Application.Current.Properties[MacAddressSettingsKey] = e.NewTextValue;
            };

            SymbolicNameEntry.TextChanged += (object sender, TextChangedEventArgs e) => {
                Application.Current.Properties[SymbolicNameSettingsKey] = e.NewTextValue;
            };
        }

        private async void ConnectionTypePicker_SelectedIndexChanged(object sender, EventArgs e) {
            IpAddressEntry.IsVisible = false;
            MacAddressEntry.IsVisible = false;
            SymbolicNameEntry.IsVisible = false;
            UsbDriverPrinterPicker.IsVisible = false;
            PortNumberEntry.IsVisible = false;

            switch (GetSelectedConnectionType()) {
                case ConnectionType.Network:
                    IpAddressEntry.IsVisible = true;
                    PortNumberEntry.IsVisible = true;

                    if (Application.Current.Properties.ContainsKey(IpAddressSettingsKey)) {
                        IpAddressEntry.Text = Application.Current.Properties[IpAddressSettingsKey] as string;
                    } else {
                        IpAddressEntry.Text = null;
                    }
                    break;

                case ConnectionType.Bluetooth:
                    MacAddressEntry.IsVisible = true;

                    if (Application.Current.Properties.ContainsKey(MacAddressSettingsKey)) {
                        MacAddressEntry.Text = Application.Current.Properties[MacAddressSettingsKey] as string;
                    } else {
                        MacAddressEntry.Text = null;
                    }
                    break;

                case ConnectionType.UsbDirect:
                    SymbolicNameEntry.IsVisible = true;

                    if (Application.Current.Properties.ContainsKey(SymbolicNameSettingsKey)) {
                        SymbolicNameEntry.Text = Application.Current.Properties[SymbolicNameSettingsKey] as string;
                    } else {
                        SymbolicNameEntry.Text = null;
                    }
                    break;

                case ConnectionType.UsbDriver:
                    UsbDriverPrinterPicker.IsVisible = true;

                    try {
                        UsbDriverPrinterPicker.ItemsSource = DependencyService.Get<IConnectionManager>().GetZebraUsbDriverPrinters();
                    } catch (NotImplementedException) {
                        ConnectionTypePicker.SelectedIndex = 0;
                        await DisplayAlert("Error", "USB driver not supported on this platform", "OK");
                    }
                    break;
            }
        }

        private async void TestButton_Clicked(object sender, EventArgs eventArgs) {
            SetInputEnabled(false);

            Connection connection = null;
            try {
                connection = CreateConnection();
            } catch (Exception e) {
                UpdateConnectionStatus($"Error: {e.Message}", Color.Red);
            }

            if (connection == null) {
                SetInputEnabled(true);
                return;
            }

            await Task.Run(async () => {
                try {
                    await DisplayConnectionStatusAsync("Connecting...", Color.Goldenrod, 1500);

                    connection.Open();

                    await DisplayConnectionStatusAsync("Connected", Color.Green, 1500);
                    await DisplayConnectionStatusAsync("Determining printer language...", Color.Goldenrod, 1500);

                    PrinterLanguage printerLanguage = ZebraPrinterFactory.GetInstance(connection).PrinterControlLanguage;
                    await DisplayConnectionStatusAsync("Printer language: " + printerLanguage.ToString(), Color.Blue, 1500);

                    UpdateConnectionStatus("Sending data...", Color.Goldenrod);

                    connection.Write(GetTestLabelBytes(printerLanguage));

                    await Task.Delay(1000);
                } catch (Exception e) {
                    await DisplayConnectionStatusAsync($"Error: {e.Message}", Color.Red, 3000);
                } finally {
                    try {
                        connection?.Close();

                        await DisplayConnectionStatusAsync("Disconnecting...", Color.Goldenrod, 1000);
                        UpdateConnectionStatus("Not connected", Color.Red);
                    } catch (ConnectionException) { }
                }
            });

            SetInputEnabled(true);
        }

        private ConnectionType? GetSelectedConnectionType() {
            string connectionType = (string)ConnectionTypePicker.SelectedItem;
            switch (connectionType) {
                case "Network":
                    return ConnectionType.Network;

                case "Bluetooth":
                    return ConnectionType.Bluetooth;

                case "USB Direct":
                    return ConnectionType.UsbDirect;

                case "USB Driver":
                    return ConnectionType.UsbDriver;

                default:
                    return null;
            }
        }

        private int GetPortNumber(string portNumberString) {
            if (!string.IsNullOrWhiteSpace(portNumberString)) {
                try {
                    return int.Parse(portNumberString);
                } catch (Exception) {
                    throw new ArgumentException("Port number must be an integer");
                }
            } else {
                return 9100;
            }
        }

        private Connection CreateConnection() {
            switch (GetSelectedConnectionType()) {
                case ConnectionType.Network:
                    return new TcpConnection(IpAddressEntry.Text, GetPortNumber(PortNumberEntry.Text));

                case ConnectionType.Bluetooth:
                    try {
                        return DependencyService.Get<IConnectionManager>().GetBluetoothConnection(MacAddressEntry.Text);
                    } catch (NotImplementedException) {
                        throw new NotImplementedException("Bluetooth connection not supported on this platform");
                    }

                case ConnectionType.UsbDirect:
                    try {
                        return DependencyService.Get<IConnectionManager>().GetUsbConnection(SymbolicNameEntry.Text);
                    } catch (NotImplementedException) {
                        throw new NotImplementedException("USB connection not supported on this platform");
                    }

                case ConnectionType.UsbDriver:
                    return ((DiscoveredPrinter)UsbDriverPrinterPicker.SelectedItem)?.GetConnection();

                default:
                    throw new ArgumentNullException("No connection type selected");
            }
        }

        /*
		 * Returns the command for a test label depending on the printer control language.
		 * The test label is a box with the word "TEST" inside of it.
		 * 
		 * _________________________
		 * |                       |
		 * |                       |
		 * |        TEST           |
		 * |                       |
		 * |                       |
		 * |_______________________|
		 * 
		 */
        private byte[] GetTestLabelBytes(PrinterLanguage printerLanguage) {
            if (printerLanguage == PrinterLanguage.ZPL) {
                return Encoding.UTF8.GetBytes(TestLabelZpl);
            } else if (printerLanguage == PrinterLanguage.CPCL || printerLanguage == PrinterLanguage.LINE_PRINT) {
                return Encoding.UTF8.GetBytes(TestLabelCpcl);
            } else {
                throw new ZebraPrinterLanguageUnknownException();
            }
        }

        private void UpdateConnectionStatus(string statusMessage, Color color) {
            Device.BeginInvokeOnMainThread(() => {
                ConnectionStatusLabel.TextColor = color;
                ConnectionStatusLabel.Text = statusMessage;
            });
        }

        private async Task DisplayConnectionStatusAsync(string statusMessage, Color color, int displayTime) {
            UpdateConnectionStatus(statusMessage, color);
            await Task.Delay(displayTime);
        }

        private void SetInputEnabled(bool enabled) {
            Device.BeginInvokeOnMainThread(() => {
                ConnectionTypePicker.IsEnabled = enabled;
                IpAddressEntry.IsEnabled = enabled;
                MacAddressEntry.IsEnabled = enabled;
                SymbolicNameEntry.IsEnabled = enabled;
                UsbDriverPrinterPicker.IsEnabled = enabled;
                PortNumberEntry.IsEnabled = enabled;
                TestButton.IsEnabled = enabled;
            });
        }
    }
}