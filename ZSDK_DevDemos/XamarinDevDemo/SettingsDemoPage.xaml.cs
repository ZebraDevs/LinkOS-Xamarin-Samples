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
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Zebra.Sdk.Comm;
using Zebra.Sdk.Printer;
using Zebra.Sdk.Printer.Discovery;
using Zebra.Sdk.Settings;

namespace XamarinDevDemo {

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsDemoPage : ContentPage {

        private const string MacAddressSettingsKey = "MacAddress";
        private const string IpAddressSettingsKey = "IpAddress";
        private const string SymbolicNameSettingsKey = "SymbolicName";

        private const string DeviceLanguagesSgd = "device.languages";

        private Dictionary<string, string> modifiedSettings = new Dictionary<string, string>();

        public enum ConnectionType {
            Network,
            Bluetooth,
            UsbDirect,
            UsbDriver
        }

        public SettingsDemoPage() {
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

        private async void GetSettingsButton_Clicked(object sender, EventArgs eventArgs) {
            ClearSettings();
            SetInputEnabled(false);

            Connection connection = null;
            bool linePrintEnabled = false;

            try {
                await Task.Factory.StartNew(() => {
                    connection = CreateConnection();
                    connection.Open();

                    string originalPrinterLanguage = SGD.GET(DeviceLanguagesSgd, connection);
                    linePrintEnabled = "line_print".Equals(originalPrinterLanguage, StringComparison.OrdinalIgnoreCase);

                    if (linePrintEnabled) {
                        SGD.SET(DeviceLanguagesSgd, "zpl", connection);
                    }

                    UpdateSettingsTable(connection, originalPrinterLanguage);
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

        private async void SaveSettingsButton_Clicked(object sender, EventArgs eventArgs) {
            SetInputEnabled(false);

            Connection connection = null;
            bool linePrintEnabled = false;
            bool deviceLanguageUpdated = false;

            try {
                await Task.Factory.StartNew(() => {
                    connection = CreateConnection();
                    connection.Open();

                    ZebraPrinter printer = ZebraPrinterFactory.GetInstance(connection);
                    ZebraPrinterLinkOs linkOsPrinter = ZebraPrinterFactory.CreateLinkOsPrinter(printer);

                    if (linkOsPrinter != null) {
                        string originalPrinterLanguage = SGD.GET(DeviceLanguagesSgd, connection);
                        linePrintEnabled = "line_print".Equals(originalPrinterLanguage, StringComparison.OrdinalIgnoreCase);

                        if (linePrintEnabled) {
                            SGD.SET(DeviceLanguagesSgd, "zpl", connection);
                            printer = ZebraPrinterFactory.GetInstance(connection);
                            linkOsPrinter = ZebraPrinterFactory.CreateLinkOsPrinter(printer);
                        }

                        foreach (string key in modifiedSettings.Keys) {
                            if (linkOsPrinter.IsSettingReadOnly(key) == false) {
                                linkOsPrinter.SetSetting(key, modifiedSettings[key]);
                            }
                        }

                        deviceLanguageUpdated = modifiedSettings.ContainsKey(DeviceLanguagesSgd);

                        modifiedSettings.Clear();
                        ClearSettings();
                        UpdateSettingsTable(connection, originalPrinterLanguage);
                    } else {
                        Device.BeginInvokeOnMainThread(async () => {
                            await DisplayAlert("Connection Error", "Connected printer does not support settings", "OK");
                        });
                    }
                });
            } catch (Exception e) {
                await DisplayAlert("Error", e.Message, "OK");
            } finally {
                if (linePrintEnabled && !deviceLanguageUpdated) {
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

        private void UpdateSettingsTable(Connection connection, string printerLanguage) {
            ZebraPrinter printer = ZebraPrinterFactory.GetInstance(connection);
            ZebraPrinterLinkOs linkOsPrinter = ZebraPrinterFactory.CreateLinkOsPrinter(printer);
            Dictionary<string, Setting> settings = linkOsPrinter.GetAllSettings();

            if (settings.ContainsKey(DeviceLanguagesSgd)) {
                settings[DeviceLanguagesSgd].Value = printerLanguage;
            }

            if (settings != null) {
                Device.BeginInvokeOnMainThread(() => {
                    foreach (string key in settings.Keys) {
                        Entry entry = new Entry {
                            Text = settings[key].Value,
                        };
                        entry.TextChanged += (object s, TextChangedEventArgs e) => {
                            if (key != null) {
                                if (modifiedSettings.ContainsKey(key)) {
                                    modifiedSettings[key] = e.NewTextValue;
                                } else {
                                    modifiedSettings.Add(key, e.NewTextValue);
                                }
                            }
                        };

                        SettingsTableSection.Add(new ViewCell {
                            View = new StackLayout {
                                Padding = new Thickness(15),
                                HorizontalOptions = LayoutOptions.FillAndExpand,
                                Children = {
                                    new Label {
                                        Text = key,
                                        FontSize = Device.GetNamedSize(NamedSize.Default, typeof(Label)),
                                        FontAttributes = FontAttributes.Bold
                                    },
                                    entry,
                                    new Label {
                                        Text = $"Type: {linkOsPrinter.GetSettingType(key)}, Range: {linkOsPrinter.GetSettingRange(key)}"
                                    }
                                }
                            }
                        });
                    }
                });
            } else {
                Device.BeginInvokeOnMainThread(async () => {
                    await DisplayAlert("Settings Error", "Error reading settings", "OK");
                });
            }
        }

        private void ClearSettings() {
            modifiedSettings.Clear();
            Device.BeginInvokeOnMainThread(() => {
                SettingsTableSection.Clear();
            });
        }

        private void SetInputEnabled(bool enabled) {
            Device.BeginInvokeOnMainThread(() => {
                ConnectionTypePicker.IsEnabled = enabled;
                IpAddressEntry.IsEnabled = enabled;
                MacAddressEntry.IsEnabled = enabled;
                SymbolicNameEntry.IsEnabled = enabled;
                UsbDriverPrinterPicker.IsEnabled = enabled;
                PortNumberEntry.IsEnabled = enabled;
                SaveSettingsButton.IsEnabled = enabled;
                GetSettingsButton.IsEnabled = enabled;
                SettingsTableView.IsEnabled = enabled;
            });
        }
    }
}