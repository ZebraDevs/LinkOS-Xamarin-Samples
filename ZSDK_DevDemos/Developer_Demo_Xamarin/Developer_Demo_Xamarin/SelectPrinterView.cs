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

/*********************************************************************************************************
File:   SelectPrinterView.cs

Descr:  Page that shows how to do printer discovery on all main ports (USB, Bluetooth, and Network).  
        Displays a list of found devices and the user can pick one to use with the demos.  
        
        Note (Android): Bluetooth discovery on Android will find all nearby Bluetooth devices, not just printers.
        Note (iOS): Due to Apple API restrictions, Bluetooth discovery will only find previously paired printers.
        
        Please see full API documentation for all notes on building apps for Bluetooth in Android and iOS.

Date: 03/8/16 
Updated:
**********************************************************************************************************/
using LinkOS.Plugin;
using LinkOS.Plugin.Abstractions;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Xamarin_LinkOS_Developer_Demo {

    public class SelectPrinterView : ContentPage {

        public delegate void PrinterSelectedHandler(IDiscoveredPrinter printer);
        public static event PrinterSelectedHandler OnPrinterSelected;

        private MainPage mainPage;
        ObservableCollection<IDiscoveredPrinter> printerList;
        ListView printerLv;
        Label statusLbl;

        public SelectPrinterView(MainPage mainPage) {
            this.mainPage = mainPage;

            Title = "Select a printer";

            printerList = new ObservableCollection<IDiscoveredPrinter>();

            printerLv = new ListView {
                ItemsSource = printerList,
                ItemTemplate = new DataTemplate(() => {
                    Label addressLbl = new Label();
                    addressLbl.SetBinding(Label.TextProperty, "Address");

                    Label friendlyLbl = new Label();
                    friendlyLbl.SetBinding(Label.TextProperty, "FriendlyName");

                    return new ViewCell {
                        View = new StackLayout {
                            Orientation = StackOrientation.Horizontal,
                            Children = { addressLbl, friendlyLbl }
                        }
                    };
                })
            };
            printerLv.ItemSelected += PrinterLv_ItemSelected;

            Button backBtn = new Button {
                Text = "Back",
                HorizontalOptions = LayoutOptions.Start
            };
            backBtn.Clicked += BackBtn_Clicked;

            statusLbl = new Label {
                Text = "Discovering Printers..."
            };

            StackLayout topSection = new StackLayout {
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Orientation = StackOrientation.Horizontal,

                Children = { backBtn, statusLbl }
            };

            Content = new StackLayout {
                Children = { topSection, printerLv }
            };
        }

        private void BackBtn_Clicked(object sender, EventArgs e) {
            DependencyService.Get<IPrinterDiscovery>().CancelDiscovery();
            mainPage.PopAsync();
        }

        private void PrinterLv_ItemSelected(object sender, SelectedItemChangedEventArgs e) {
            DependencyService.Get<IPrinterDiscovery>().CancelDiscovery();
            if (e.SelectedItem is IDiscoveredPrinterUsb) {
                if (!((IDiscoveredPrinterUsb)e.SelectedItem).HasPermissionToCommunicate) {
                    DependencyService.Get<IPrinterDiscovery>().RequestUsbPermission(((IDiscoveredPrinterUsb)e.SelectedItem));
                }
            }

            OnPrinterSelected?.Invoke((IDiscoveredPrinter)e.SelectedItem);
            mainPage.PopAsync();
        }

        ///// <summary>
        ///// Start discovery on all ports. USB, then Bluetooth, then Network.
        ///// </summary>
        public void StartPrinterDiscovery() {
            new Task(new Action(() => {
				if(printerList != null && printerList.Count > 0) {
					printerList.Clear();
				}
                StartDiscovery(ConnectionType.USB);
            })).Start();
        }

        private void StartDiscovery(ConnectionType connectionType) {
            UpdateStatus($"Discovering {connectionType.ToString()} printers...");

            try {
                System.Diagnostics.Debug.WriteLine($"Starting {connectionType.ToString()} discovery...");

                switch (connectionType) {
                    case ConnectionType.Bluetooth:
                        DependencyService.Get<IPrinterDiscovery>().FindBluetoothPrinters(new DiscoveryHandlerImplementation(this, ConnectionType.Bluetooth));
                        break;

                    case ConnectionType.Network:
                        NetworkDiscoverer.Current.LocalBroadcast(new DiscoveryHandlerImplementation(this, ConnectionType.Network));
                        break;

                    case ConnectionType.USB:
                        DependencyService.Get<IPrinterDiscovery>().FindUsbPrinters(new DiscoveryHandlerImplementation(this, ConnectionType.USB));
                        break;
                }
            } catch (Exception e) {
                if (e is NotImplementedException && connectionType == ConnectionType.USB) {
                    StartDiscovery(ConnectionType.Bluetooth);
                } else {
                    string errorMessage = $"Error discovering {nameof(connectionType)} printers: {e.Message}";
                    System.Diagnostics.Debug.WriteLine(errorMessage);
                    ShowErrorAlert(errorMessage);
                }
            }
        }

        private void ShowErrorAlert(string message) {
            Device.BeginInvokeOnMainThread(() => {
                DisplayAlert("Error", message, "OK");
            });
        }

        private void UpdateStatus(string message) {
            Device.BeginInvokeOnMainThread(() => {
                statusLbl.Text = message;
            });
        }

        private class DiscoveryHandlerImplementation : IDiscoveryHandler {

            private SelectPrinterView selectPrinterPage;
            private ConnectionType connectionType;

            public DiscoveryHandlerImplementation(SelectPrinterView selectPrinterPage, ConnectionType connectionType) {
                this.selectPrinterPage = selectPrinterPage;
                this.connectionType = connectionType;
            }

            public void DiscoveryError(string message) {
                System.Diagnostics.Debug.WriteLine($"Error discovering {Enum.GetName(typeof(ConnectionType), connectionType)} printers: {message}");
                selectPrinterPage.ShowErrorAlert(message);

                if (connectionType == ConnectionType.USB) {
                    selectPrinterPage.StartDiscovery(ConnectionType.Bluetooth);
                } else if (connectionType == ConnectionType.Bluetooth) {
                    selectPrinterPage.StartDiscovery(ConnectionType.Network);
                } else
                    selectPrinterPage.UpdateStatus("Discovery finished");
            }

            public void DiscoveryFinished() {
                System.Diagnostics.Debug.WriteLine($"Finished discovering {Enum.GetName(typeof(ConnectionType), connectionType)} printers");
                if (connectionType == ConnectionType.USB) {
                    selectPrinterPage.StartDiscovery(ConnectionType.Bluetooth);
                } else if (connectionType == ConnectionType.Bluetooth) {
                    selectPrinterPage.StartDiscovery(ConnectionType.Network);
                } else
                    selectPrinterPage.UpdateStatus("Discovery finished");
            }

            public void FoundPrinter(IDiscoveredPrinter discoveredPrinter) {
                System.Diagnostics.Debug.WriteLine($"Found printer: {discoveredPrinter.ToString()}");
                Device.BeginInvokeOnMainThread(() => {
                    selectPrinterPage.printerLv.BatchBegin();

                    if (!selectPrinterPage.printerList.Contains(discoveredPrinter)) {
                        selectPrinterPage.printerList.Add(discoveredPrinter);
                    }
                    selectPrinterPage.printerLv.BatchCommit();
                });
            }
        }
    }
}
