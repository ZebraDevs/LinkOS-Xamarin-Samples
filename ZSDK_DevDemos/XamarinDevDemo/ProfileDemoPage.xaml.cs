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
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Zebra.Sdk.Comm;
using Zebra.Sdk.Device;
using Zebra.Sdk.Printer;
using Zebra.Sdk.Printer.Discovery;

namespace XamarinDevDemo {

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProfileDemoPage : ContentPage {

        private const string MacAddressSettingsKey = "MacAddress";
        private const string IpAddressSettingsKey = "IpAddress";
        private const string SymbolicNameSettingsKey = "SymbolicName";

        private const string DeviceLanguagesSgd = "device.languages";

        private string LocalApplicationDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        private ObservableCollection<string> filenames = new ObservableCollection<string>();

        public enum ConnectionType {
            Network,
            Bluetooth,
            UsbDirect,
            UsbDriver
        }

        public ProfileDemoPage() {
            InitializeComponent();

            ConnectionTypePicker.SelectedIndex = 0;
            FileListView.ItemsSource = filenames;

            IpAddressEntry.TextChanged += (object sender, TextChangedEventArgs e) => {
                Application.Current.Properties[IpAddressSettingsKey] = e.NewTextValue;
            };

            MacAddressEntry.TextChanged += (object sender, TextChangedEventArgs e) => {
                Application.Current.Properties[MacAddressSettingsKey] = e.NewTextValue;
            };

            SymbolicNameEntry.TextChanged += (object sender, TextChangedEventArgs e) => {
                Application.Current.Properties[SymbolicNameSettingsKey] = e.NewTextValue;
            };

            RefreshLocalApplicationDataFiles();
        }

        private async void ConnectionTypePicker_SelectedIndexChanged(object sender, EventArgs e) {
            IpAddressEntry.IsVisible = false;
            MacAddressEntry.IsVisible = false;
            SymbolicNameEntry.IsVisible = false;
            UsbDriverPrinterPicker.IsVisible = false;
            PrintingPortNumberEntry.IsVisible = false;

            switch (GetSelectedConnectionType()) {
                case ConnectionType.Network:
                    IpAddressEntry.IsVisible = true;
                    PrintingPortNumberEntry.IsVisible = true;

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

        private async void CreateProfileButton_Clicked(object sender, EventArgs eventArgs) {
            SetInputEnabled(false);

            string filename = FilenameEntry.Text;
            Connection connection = null;
            bool linePrintEnabled = false;

            try {
                if (!string.IsNullOrWhiteSpace(filename)) {
                    await Task.Factory.StartNew(() => {
                        connection = CreateConnection();
                        connection.Open();

                        ZebraPrinter printer = ZebraPrinterFactory.GetInstance(connection);
                        ZebraPrinterLinkOs linkOsPrinter = ZebraPrinterFactory.CreateLinkOsPrinter(printer);

                        string originalPrinterLanguage = SGD.GET(DeviceLanguagesSgd, connection);
                        linePrintEnabled = "line_print".Equals(originalPrinterLanguage, StringComparison.OrdinalIgnoreCase);

                        if (linePrintEnabled) {
                            SGD.SET(DeviceLanguagesSgd, "zpl", connection);
                            printer = ZebraPrinterFactory.GetInstance(connection);
                            linkOsPrinter = ZebraPrinterFactory.CreateLinkOsPrinter(printer);
                        }

                        if (linkOsPrinter != null) {
                            if (!filename.EndsWith(".zprofile")) {
                                filename += ".zprofile";
                            }
                            string path = Path.Combine(LocalApplicationDataFolderPath, filename);
                            linkOsPrinter.CreateProfile(path);

                            Xamarin.Forms.Device.BeginInvokeOnMainThread(async () => {
                                await DisplayAlert("Profile Created Successfully", $"Profile created successfully with filename {filename}", "OK");
                            });

                            RefreshLocalApplicationDataFiles();
                        } else {
                            Xamarin.Forms.Device.BeginInvokeOnMainThread(async () => {
                                await DisplayAlert("Error", "Profile creation is only available on Link-OS\u2122 printers", "OK");
                            });
                        }
                    });
                } else {
                    await DisplayAlert("Invalid Filename", "Please enter a valid filename", "OK");
                    SetInputEnabled(true);
                }
            } catch (Exception e) {
                await DisplayAlert("Error", e.Message, "OK");
            } finally {
                if (linePrintEnabled) {
                    await Task.Factory.StartNew(() => {
                        try {
                            connection?.Open();
                            SGD.SET(DeviceLanguagesSgd, "line_print", connection);
                        } catch (ConnectionException) { }
                    });
                }

                try {
                    connection?.Close();
                } catch (ConnectionException) { }

                SetInputEnabled(true);
            }
        }

        private async void UploadProfileButton_Clicked(object sender, EventArgs eventArgs) {
            SetInputEnabled(false);

            Connection connection = null;
            bool linePrintEnabled = false;

            try {
                await Task.Factory.StartNew(() => {
                    connection = CreateConnection();
                    connection.Open();

                    ZebraPrinter printer = ZebraPrinterFactory.GetInstance(connection);
                    ZebraPrinterLinkOs linkOsPrinter = ZebraPrinterFactory.CreateLinkOsPrinter(printer);

                    string originalPrinterLanguage = SGD.GET(DeviceLanguagesSgd, connection);
                    linePrintEnabled = "line_print".Equals(originalPrinterLanguage, StringComparison.OrdinalIgnoreCase);

                    if (linePrintEnabled) {
                        SGD.SET(DeviceLanguagesSgd, "zpl", connection);
                        printer = ZebraPrinterFactory.GetInstance(connection);
                        linkOsPrinter = ZebraPrinterFactory.CreateLinkOsPrinter(printer);
                    }

                    string selectedFilename = (string)FileListView.SelectedItem;
                    if (selectedFilename != null) {
                        if (linkOsPrinter != null) {
                            string path = Path.Combine(LocalApplicationDataFolderPath, selectedFilename);
                            linkOsPrinter.LoadProfile(path, FileDeletionOption.NONE, false);

                            Xamarin.Forms.Device.BeginInvokeOnMainThread(async () => {
                                await DisplayAlert("Profile Uploaded Successfully", $"Profile loaded successfully to printer", "OK");
                            });
                        } else {
                            Xamarin.Forms.Device.BeginInvokeOnMainThread(async () => {
                                await DisplayAlert("Error", "Profile loading is only available on Link-OS\u2122 printers", "OK");
                            });
                        }
                    } else {
                        Xamarin.Forms.Device.BeginInvokeOnMainThread(async () => {
                            await DisplayAlert("No Profile Selected", "Please select a profile to upload", "OK");
                        });
                    }
                });
            } catch (Exception e) {
                await DisplayAlert("Error", e.Message, "OK");
            } finally {
                if (linePrintEnabled) {
                    await Task.Factory.StartNew(() => {
                        try {
                            connection?.Open();
                            SGD.SET(DeviceLanguagesSgd, "line_print", connection);
                        } catch (ConnectionException) { }
                    });
                }

                try {
                    connection?.Close();
                } catch (ConnectionException) { }

                SetInputEnabled(true);
            }
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
                    return new TcpConnection(IpAddressEntry.Text, GetPortNumber(PrintingPortNumberEntry.Text));

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

        private void RefreshLocalApplicationDataFiles() {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(async () => {
                try {
                    try {
                        filenames.Clear();
                    } catch (NotImplementedException) {
                        filenames.Clear(); // Workaround for Xamarin.Forms.Platform.WPF issue: https://github.com/xamarin/Xamarin.Forms/issues/3648
                    }

                    string[] paths = Directory.GetFiles(LocalApplicationDataFolderPath);
                    foreach (string path in paths) {
                        filenames.Add(Path.GetFileName(path));
                    }
                } catch (Exception e) {
                    await DisplayAlert("Error", e.Message, "OK");
                }
            });
        }

        private void SetInputEnabled(bool enabled) {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {
                ConnectionTypePicker.IsEnabled = enabled;
                IpAddressEntry.IsEnabled = enabled;
                MacAddressEntry.IsEnabled = enabled;
                SymbolicNameEntry.IsEnabled = enabled;
                UsbDriverPrinterPicker.IsEnabled = enabled;
                PrintingPortNumberEntry.IsEnabled = enabled;
                FilenameEntry.IsEnabled = enabled;
                CreateProfileButton.IsEnabled = enabled;
                FileListView.IsEnabled = enabled;
                UploadProfileButton.IsEnabled = enabled;
            });
        }
    }
}