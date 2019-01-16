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

namespace XamarinDevDemo {

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StatusChannelDemoPage : ContentPage {

        private const string MacAddressSettingsKey = "MacAddress";
        private const string IpAddressSettingsKey = "IpAddress";

        public enum ConnectionType {
            Network,
            Bluetooth
        }

        public StatusChannelDemoPage() {
            InitializeComponent();

            ConnectionTypePicker.SelectedIndex = 0;

            AddressEntry.TextChanged += (object sender, TextChangedEventArgs e) => {
                switch (GetSelectedConnectionType()) {
                    case ConnectionType.Network:
                        Application.Current.Properties[IpAddressSettingsKey] = e.NewTextValue;
                        break;

                    case ConnectionType.Bluetooth:
                        Application.Current.Properties[MacAddressSettingsKey] = e.NewTextValue;
                        break;
                }
            };
        }

        private void ConnectionTypePicker_SelectedIndexChanged(object sender, EventArgs e) {
            switch (GetSelectedConnectionType()) {
                case ConnectionType.Network:
                    AddressEntry.Placeholder = "IP Address / DNS Name";
                    StatusPortNumberEntry.IsVisible = true;
                    FindAvailableChannelsButton.IsVisible = false;
                    AvailableChannelsHeaderLabel.IsVisible = false;
                    AvailableChannelsLabel.IsVisible = false;

                    if (Application.Current.Properties.ContainsKey(IpAddressSettingsKey)) {
                        AddressEntry.Text = Application.Current.Properties[IpAddressSettingsKey] as string;
                    } else {
                        AddressEntry.Text = null;
                    }
                    break;

                case ConnectionType.Bluetooth:
                    AddressEntry.Placeholder = "MAC Address";
                    StatusPortNumberEntry.IsVisible = false;
                    FindAvailableChannelsButton.IsVisible = true;
                    AvailableChannelsHeaderLabel.IsVisible = true;
                    AvailableChannelsLabel.IsVisible = true;

                    if (Application.Current.Properties.ContainsKey(MacAddressSettingsKey)) {
                        AddressEntry.Text = Application.Current.Properties[MacAddressSettingsKey] as string;
                    } else {
                        AddressEntry.Text = null;
                    }
                    break;
            }
        }

        private async void GetPrinterStatusButton_Clicked(object sender, EventArgs eventArgs) {
            AvailableChannelsLabel.Text = "";
            PrinterStatusLabel.Text = "Retrieving printer status...";
            SetInputEnabled(false);

            StatusConnection statusConnection = null;
            Connection rawConnection = null;

            try {
                statusConnection = CreateStatusConnection();

                if (statusConnection == null) {
                    return;
                }

                if (GetSelectedConnectionType() == ConnectionType.Bluetooth) {
                    try {
                        // Over Bluetooth, the printer only broadcasts the status connection if a valid raw connection is open
                        rawConnection = DependencyService.Get<IConnectionManager>().GetBluetoothConnection(AddressEntry.Text);
                    } catch (NotImplementedException) {
                        throw new NotImplementedException("Bluetooth connection not supported on this platform");
                    }

                    await Task.Factory.StartNew(() => {
                        rawConnection.Open();
                    });

                    await Task.Delay(3000); // Give the printer some time to start the status connection
                }

                await Task.Factory.StartNew(() => {
                    statusConnection.Open();

                    ZebraPrinter printer = ZebraPrinterFactory.GetLinkOsPrinter(statusConnection);
                    PrinterStatus printerStatus = printer.GetCurrentStatus();

                    Device.BeginInvokeOnMainThread(() => {
                        UpdateResult(printerStatus);
                    });
                });
            } catch (Exception e) {
                PrinterStatusLabel.Text = $"Error: {e.Message}";
                await DisplayAlert("Error", e.Message, "OK");
            } finally {
                try {
                    statusConnection?.Close();
                    rawConnection?.Close();
                } catch (ConnectionException) { }

                SetInputEnabled(true);
            }
        }

        private async void FindAvailableChannelsButton_Clicked(object sender, EventArgs eventArgs) {
            AvailableChannelsLabel.Text = "Finding available channels...";
            PrinterStatusLabel.Text = "";
            SetInputEnabled(false);

            try {
                await Task.Factory.StartNew(() => {
                    string connectionChannels;
                    try {
                        connectionChannels = DependencyService.Get<IConnectionManager>().BuildBluetoothConnectionChannelsString(AddressEntry.Text);
                    } catch (NotImplementedException) {
                        throw new NotImplementedException("Bluetooth connection channels not supported on this platform");
                    }

                    Device.BeginInvokeOnMainThread(() => {
                        AvailableChannelsLabel.Text = connectionChannels;
                    });
                });
            } catch (Exception e) {
                AvailableChannelsLabel.Text = $"Error: {e.Message}";
                await DisplayAlert("Error", e.Message, "OK");
            } finally {
                SetInputEnabled(true);
            }
        }

        private int GetStatusPortNumber(string portNumberString) {
            if (!string.IsNullOrWhiteSpace(portNumberString)) {
                try {
                    return int.Parse(portNumberString);
                } catch (Exception) {
                    throw new ArgumentException("Status port number must be an integer");
                }
            } else {
                return 9200;
            }
        }

        private ConnectionType? GetSelectedConnectionType() {
            string connectionType = (string)ConnectionTypePicker.SelectedItem;
            switch (connectionType) {
                case "Network":
                    return ConnectionType.Network;
                case "Bluetooth":
                    return ConnectionType.Bluetooth;
                default:
                    return null;
            }
        }

        private StatusConnection CreateStatusConnection() {
            switch (GetSelectedConnectionType()) {
                case ConnectionType.Network:
                    return new TcpStatusConnection(AddressEntry.Text, GetStatusPortNumber(StatusPortNumberEntry.Text));

                case ConnectionType.Bluetooth:
                    try {
                        return DependencyService.Get<IConnectionManager>().GetBluetoothStatusConnection(AddressEntry.Text);
                    } catch (NotImplementedException) {
                        throw new NotImplementedException("Bluetooth status connection not supported on this platform");
                    }

                default:
                    throw new ArgumentNullException("No connection type selected");
            }
        }

        private void UpdateResult(PrinterStatus printerStatus) {
            StringBuilder sb = new StringBuilder();

            if (printerStatus != null) {
                sb.AppendLine($"Printer ready: {printerStatus.isReadyToPrint}");
                sb.AppendLine($"Head open: {printerStatus.isHeadOpen}");
                sb.AppendLine($"Paper out: {printerStatus.isPaperOut}");
                sb.AppendLine($"Printer paused: {printerStatus.isPaused}");
                sb.AppendLine($"Labels remaining in batch: {printerStatus.labelsRemainingInBatch}");
            }

            PrinterStatusLabel.Text = sb.ToString();
        }

        private void SetInputEnabled(bool enabled) {
            Device.BeginInvokeOnMainThread(() => {
                ConnectionTypePicker.IsEnabled = enabled;
                StatusPortNumberEntry.IsEnabled = enabled;
                AddressEntry.IsEnabled = enabled;
                FindAvailableChannelsButton.IsEnabled = enabled;
                GetPrinterStatusButton.IsEnabled = enabled;
            });
        }
    }
}