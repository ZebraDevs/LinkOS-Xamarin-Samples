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
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Zebra.Sdk.Comm;
using Zebra.Sdk.Printer;
using Zebra.Sdk.Settings;

namespace XamarinDevDemo {

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MultichannelDemoPage : ContentPage {

        private const string MacAddressSettingsKey = "MacAddress";
        private const string IpAddressSettingsKey = "IpAddress";

        private const string TestLabelBeginningZpl = "^XA";
        private const string TestLabelEndZpl = @"^FO17,16
                                                 ^GB379,371,8^FS
                                                 ^FT65,255
                                                 ^A0N,135,134
                                                 ^FDTEST^FS
                                                 ^XZ";
        
        private const string TestLabelBeginningCpcl = "! 0 200 200 406 1\r\n";
        private const string TestLabelEndCpcl = "ON-FEED IGNORE\r\n" +
                                                "BOX 20 20 380 380 8\r\n" +
                                                "T 0 6 137 177 TEST\r\n" +
                                                "PRINT\r\n";

        public enum ConnectionType {
            Network,
            Bluetooth
        }

        public MultichannelDemoPage() {
            InitializeComponent();

            ConnectionTypePicker.SelectedIndex = 0;

            IpAddressEntry.TextChanged += (object sender, TextChangedEventArgs e) => {
                Application.Current.Properties[IpAddressSettingsKey] = e.NewTextValue;
            };

            MacAddressEntry.TextChanged += (object sender, TextChangedEventArgs e) => {
                Application.Current.Properties[MacAddressSettingsKey] = e.NewTextValue;
            };
        }

        private void ConnectionTypePicker_SelectedIndexChanged(object sender, EventArgs e) {
            IpAddressEntry.IsVisible = false;
            MacAddressEntry.IsVisible = false;
            PrintingPortNumberEntry.IsVisible = false;
            StatusPortNumberEntry.IsVisible = false;

            switch (GetSelectedConnectionType()) {
                case ConnectionType.Network:
                    IpAddressEntry.IsVisible = true;
                    PrintingPortNumberEntry.IsVisible = true;
                    StatusPortNumberEntry.IsVisible = true;

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
            }
        }

        private async void TestButton_Clicked(object sender, EventArgs eventArgs) {
            SetInputEnabled(false);

            bool printJobFinished = false;
            MultichannelConnection multichannelConnection = null;

            try {
                multichannelConnection = CreateMultichannelConnection();

                if (multichannelConnection == null) {
                    return;
                }

                ZebraPrinter linkOsPrinter = null;

                await Task.Factory.StartNew(() => {
                    multichannelConnection.Open();

                    linkOsPrinter = ZebraPrinterFactory.GetLinkOsPrinter(multichannelConnection.StatusChannel);
                });

                Task statusTask = Task.Factory.StartNew(() => {
                    int queryCount = 0;
                    List<string> odometerSettings = new List<string> {
                        "odometer.total_label_count",
                        "odometer.total_print_length"
                    };

                    while (multichannelConnection.StatusChannel.Connected && !printJobFinished) {
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        LinkOsInformation linkOsInformation = new LinkOsInformation(SGD.GET("appl.link_os_version", multichannelConnection));
                        Dictionary<string, string> odometerSettingsMap = new SettingsValues().GetValues(odometerSettings, multichannelConnection.StatusChannel, linkOsPrinter.PrinterControlLanguage, linkOsInformation);
                        PrinterStatus printerStatus = linkOsPrinter.GetCurrentStatus();

                        queryCount++;
                        stopwatch.Stop();

                        Device.BeginInvokeOnMainThread(() => {
                            UpdateResult(queryCount, stopwatch.ElapsedMilliseconds, printerStatus, odometerSettingsMap);
                        });
                    }
                });

                Task printingTask = Task.Factory.StartNew(async () => {
                    try {
                        multichannelConnection.PrintingChannel.Write(GetTestLabelBeginningBytes(linkOsPrinter.PrinterControlLanguage));
                        await Task.Delay(500);

                        multichannelConnection.PrintingChannel.Write(GetTestLabelEndBytes(linkOsPrinter.PrinterControlLanguage));
                        await Task.Delay(3000);
                    } catch (Exception e) {
                        throw e;
                    } finally {
                        printJobFinished = true;
                    }
                });

                Task aggregateTask = Task.WhenAll(statusTask, printingTask);
                await aggregateTask;

                if (aggregateTask.Exception != null) {
                    throw aggregateTask.Exception;
                }
            } catch (Exception e) {
                ResultLabel.Text = $"Error: {e.Message}";
                await DisplayAlert("Error", e.Message, "OK");
            } finally {
                try {
                    multichannelConnection?.Close();
                } catch (ConnectionException) { }

                SetInputEnabled(true);
            }
        }

        private byte[] GetTestLabelBeginningBytes(PrinterLanguage printerLanguage) {
            if (printerLanguage == PrinterLanguage.ZPL) {
                return Encoding.UTF8.GetBytes(TestLabelBeginningZpl);
            } else if (printerLanguage == PrinterLanguage.CPCL || printerLanguage == PrinterLanguage.LINE_PRINT) {
                return Encoding.UTF8.GetBytes(TestLabelBeginningCpcl);
            } else {
                throw new ZebraPrinterLanguageUnknownException();
            }
        }

        private byte[] GetTestLabelEndBytes(PrinterLanguage printerLanguage) {
            if (printerLanguage == PrinterLanguage.ZPL) {
                return Encoding.UTF8.GetBytes(TestLabelEndZpl);
            } else if (printerLanguage == PrinterLanguage.CPCL || printerLanguage == PrinterLanguage.LINE_PRINT) {
                return Encoding.UTF8.GetBytes(TestLabelEndCpcl);
            } else {
                throw new ZebraPrinterLanguageUnknownException();
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

        private int GetPrintingPortNumber(string portNumberString) {
            if (!string.IsNullOrWhiteSpace(portNumberString)) {
                try {
                    return int.Parse(portNumberString);
                } catch (Exception) {
                    throw new ArgumentException("Printing port number must be an integer");
                }
            } else {
                return 9100;
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

        private MultichannelConnection CreateMultichannelConnection() {
            switch (GetSelectedConnectionType()) {
                case ConnectionType.Network:
                    int printingPortNumber = GetPrintingPortNumber(PrintingPortNumberEntry.Text);
                    int statusPortNumber = GetStatusPortNumber(StatusPortNumberEntry.Text);

                    if (printingPortNumber.Equals(statusPortNumber)) {
                        throw new ArgumentException("Printing port number cannot be the same as status port number");
                    }

                    return new MultichannelTcpConnection(IpAddressEntry.Text, printingPortNumber, statusPortNumber);
                case ConnectionType.Bluetooth:
                    try {
                        return DependencyService.Get<IConnectionManager>().GetMultichannelBluetoothConnection(MacAddressEntry.Text);
                    } catch (NotImplementedException) {
                        throw new NotImplementedException("Bluetooth multichannel connection not supported on this platform");
                    }
                default:
                    throw new ArgumentNullException("No connection type selected");
            }
        }

        private void UpdateResult(int queryCount, long queryTime, PrinterStatus printerStatus, Dictionary<string, string> odometerSettingsMap) {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Total number of queries: {queryCount}");
            sb.AppendLine($"Total time to query settings (ms): {queryTime}");

            if (odometerSettingsMap != null) {
                sb.AppendLine($"Total label count: {odometerSettingsMap["odometer.total_label_count"]}");
                sb.AppendLine($"Total print length: {odometerSettingsMap["odometer.total_print_length"]}");
            }

            if (printerStatus != null) {
                sb.AppendLine($"Printer ready: {printerStatus.isReadyToPrint}");
                sb.AppendLine($"Head open: {printerStatus.isHeadOpen}");
                sb.AppendLine($"Paper out: {printerStatus.isPaperOut}");
                sb.AppendLine($"Printer paused: {printerStatus.isPaused}");
                sb.AppendLine($"Labels remaining in batch: {printerStatus.labelsRemainingInBatch}");
            }

            ResultLabel.Text = sb.ToString();
        }

        private void SetInputEnabled(bool enabled) {
            Device.BeginInvokeOnMainThread(() => {
                ConnectionTypePicker.IsEnabled = enabled;
                IpAddressEntry.IsEnabled = enabled;
                MacAddressEntry.IsEnabled = enabled;
                TestButton.IsEnabled = enabled;
            });
        }
    }
}