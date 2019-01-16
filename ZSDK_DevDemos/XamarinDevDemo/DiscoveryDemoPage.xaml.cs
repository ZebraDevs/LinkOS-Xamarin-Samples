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
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Zebra.Sdk.Printer.Discovery;

namespace XamarinDevDemo {

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DiscoveryDemoPage : ContentPage {

        private ObservableCollection<DiscoveredPrinter> discoveredPrinters = new ObservableCollection<DiscoveredPrinter>(); 

        public enum DiscoveryMethod {
            LocalBroadcast,
            DirectedBroadcast,
            MulticastBroadcast,
            SubnetSearch,
            ZebraUsbDrivers,
            UsbDirect,
            FindPrintersNearMe,
            FindAllBluetoothDevices
        }

        public DiscoveryDemoPage() {
            InitializeComponent();

            DiscoveredPrintersListView.ItemsSource = discoveredPrinters;
            DiscoveryMethodPicker.SelectedIndex = 0;
        }

        private void DiscoveryMethodPicker_SelectedIndexChanged(object sender, EventArgs e) {
            IpAddressEntry.IsVisible = false;
            NumberOfHopsEntry.IsVisible = false;
            SubnetRangeEntry.IsVisible = false;

            switch (GetSelectedDiscoveryMethod()) {
                case DiscoveryMethod.DirectedBroadcast:
                    IpAddressEntry.IsVisible = true;
                    break;

                case DiscoveryMethod.MulticastBroadcast:
                    NumberOfHopsEntry.IsVisible = true;
                    break;

                case DiscoveryMethod.SubnetSearch:
                    SubnetRangeEntry.IsVisible = true;
                    break;
            }

            ClearDiscoveredPrinters();
        }

        private async void DiscoverPrintersButton_Clicked(object sender, EventArgs eventArgs) {
            SetInputEnabled(false);

            try {
                ClearDiscoveredPrinters();

                DiscoveryHandlerImplementation discoveryHandler = new DiscoveryHandlerImplementation(this);

                await Task.Factory.StartNew(() => {
                    switch (GetSelectedDiscoveryMethod()) {
                        case DiscoveryMethod.LocalBroadcast:
                            NetworkDiscoverer.LocalBroadcast(discoveryHandler);
                            break;

                        case DiscoveryMethod.DirectedBroadcast:
                            NetworkDiscoverer.DirectedBroadcast(discoveryHandler, IpAddressEntry.Text);
                            break;

                        case DiscoveryMethod.MulticastBroadcast:
                            if (string.IsNullOrWhiteSpace(NumberOfHopsEntry.Text)) {
                                throw new ArgumentException("Number of hops must not be empty");
                            }

                            try {
                                NetworkDiscoverer.Multicast(discoveryHandler, int.Parse(NumberOfHopsEntry.Text));
                            } catch (FormatException) {
                                Device.BeginInvokeOnMainThread(async () => {
                                    await DisplayAlert("Format Error", "Number of hops must be an integer", "OK");
                                });
                            } finally {
                                SetInputEnabled(true);
                            }
                            break;

                        case DiscoveryMethod.SubnetSearch:
                            if (string.IsNullOrWhiteSpace(SubnetRangeEntry.Text)) {
                                throw new ArgumentException("Subnet range must not be empty");
                            }

                            NetworkDiscoverer.SubnetSearch(discoveryHandler, SubnetRangeEntry.Text);
                            break;

                        case DiscoveryMethod.ZebraUsbDrivers:
                            try {
                                foreach (DiscoveredPrinter printer in DependencyService.Get<IConnectionManager>().GetZebraUsbDriverPrinters()) {
                                    Device.BeginInvokeOnMainThread(() => {
                                        discoveredPrinters.Add(printer);
                                    });
                                }
                            } catch (NotImplementedException) {
                                Device.BeginInvokeOnMainThread(async () => {
                                    await DisplayAlert("Error", "USB driver discovery not supported on this platform", "OK");
                                });
                            } finally {
                                SetInputEnabled(true);
                            }
                            break;

                        case DiscoveryMethod.UsbDirect:
                            try {
                                DependencyService.Get<IConnectionManager>().GetZebraUsbDirectPrinters(discoveryHandler);
                            } catch (NotImplementedException) {
                                Device.BeginInvokeOnMainThread(async () => {
                                    await DisplayAlert("Error", "USB direct discovery not supported on this platform", "OK");
                                });
                            } finally {
                                SetInputEnabled(true);
                            }
                            break;

                        case DiscoveryMethod.FindPrintersNearMe:
                            NetworkDiscoverer.FindPrinters(discoveryHandler);
                            break;

                        case DiscoveryMethod.FindAllBluetoothDevices:
                            try {
                                DependencyService.Get<IConnectionManager>().FindBluetoothPrinters(discoveryHandler);
                            } catch (NotImplementedException) {
                                Device.BeginInvokeOnMainThread(async () => {
                                    await DisplayAlert("Error", "Bluetooth discovery not supported on this platform", "OK");
                                });
                            } finally {
                                SetInputEnabled(true);
                            }
                            break;
                    }
                });
            } catch (Exception e) {
                await DisplayAlert("Error", e.Message, "OK");
                SetInputEnabled(true);
            }
        }

        private void DiscoveredPrintersListView_ItemSelected(object sender, SelectedItemChangedEventArgs e) {
            DiscoveredPrinterEditor.Text = "";

            var selectedItem = DiscoveredPrintersListView.SelectedItem;
            if (selectedItem != null) {
                DiscoveredPrinter discoveredPrinter = (DiscoveredPrinter)selectedItem;
                Dictionary<string, string> settings = discoveredPrinter.DiscoveryDataMap;

                StringBuilder sb = new StringBuilder();
                foreach (string key in settings.Keys) {
                    sb.AppendLine($"{key}: {settings[key]}");
                }

                DiscoveredPrinterEditor.Text = sb.ToString();
            }
        }

        private DiscoveryMethod? GetSelectedDiscoveryMethod() {
            string discoveryMethod = (string)DiscoveryMethodPicker.SelectedItem;
            switch (discoveryMethod) {
                case "Local Broadcast":
                    return DiscoveryMethod.LocalBroadcast;

                case "Directed Broadcast":
                    return DiscoveryMethod.DirectedBroadcast;

                case "Multicast Broadcast":
                    return DiscoveryMethod.MulticastBroadcast;

                case "Subnet Search":
                    return DiscoveryMethod.SubnetSearch;

                case "Zebra USB Drivers":
                    return DiscoveryMethod.ZebraUsbDrivers;

                case "USB Direct":
                    return DiscoveryMethod.UsbDirect;

                case "Find Printers Near Me":
                    return DiscoveryMethod.FindPrintersNearMe;

                case "Find All Bluetooth Devices":
                    return DiscoveryMethod.FindAllBluetoothDevices;

                default:
                    return null;
            }
        }

        private void ClearDiscoveredPrinters() {
            try {
                discoveredPrinters.Clear();
            } catch (NotImplementedException) {
                discoveredPrinters.Clear(); // Workaround for Xamarin.Forms.Platform.WPF issue: https://github.com/xamarin/Xamarin.Forms/issues/3648
            }

            DiscoveredPrinterEditor.Text = "";
        }

        private void SetInputEnabled(bool enabled) {
            Device.BeginInvokeOnMainThread(() => {
                DiscoveryMethodPicker.IsEnabled = enabled;
                IpAddressEntry.IsEnabled = enabled;
                NumberOfHopsEntry.IsEnabled = enabled;
                SubnetRangeEntry.IsEnabled = enabled;
                DiscoverPrintersButton.IsEnabled = enabled;
                DiscoveredPrintersListView.IsEnabled = enabled;
                DiscoveredPrinterEditor.IsEnabled = enabled;
            });
        }

        private class DiscoveryHandlerImplementation : DiscoveryHandler {

            private DiscoveryDemoPage discoveryDemoPage;

            public DiscoveryHandlerImplementation(DiscoveryDemoPage discoveryDemoPage) {
                this.discoveryDemoPage = discoveryDemoPage;
            }

            public void DiscoveryError(string message) {
                Device.BeginInvokeOnMainThread(async () => {
                    await discoveryDemoPage.DisplayAlert("Discovery Error", message, "OK");
                });
            }

            public void DiscoveryFinished() {
                Device.BeginInvokeOnMainThread(() => {
                    discoveryDemoPage.SetInputEnabled(true);
                });
            }

            public void FoundPrinter(DiscoveredPrinter printer) {
                Device.BeginInvokeOnMainThread(() => {
                    discoveryDemoPage.discoveredPrinters.Add(printer);
                });
            }
        }
    }
}