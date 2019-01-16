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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Zebra.Sdk.Comm;
using Zebra.Sdk.Printer.Discovery;

namespace XamarinPrintStation {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SelectPrinterPage : ContentPage {

        private SelectPrinterPageViewModel viewModel = new SelectPrinterPageViewModel();
        private MainPage mainPage;

        public SelectPrinterPage(MainPage mainPage) {
            InitializeComponent();

            BindingContext = viewModel;
            this.mainPage = mainPage;

            Device.BeginInvokeOnMainThread(async () => {
                await DiscoverPrintersAsync();
            });
        }

        private async Task ClearDiscoveredPrinterListAsync() {
            await Task.Factory.StartNew(() => {
                viewModel.HighlightedPrinter = null;
            });

            Device.BeginInvokeOnMainThread(() => {
                try {
                    viewModel.DiscoveredPrinterList.Clear(); // ListView view model operations must be done on UI thread due to iOS issues when clearing list while item is selected: https://forums.xamarin.com/discussion/19114/invalid-number-of-rows-in-section
                } catch (NotImplementedException) {
                    viewModel.DiscoveredPrinterList.Clear(); // Workaround for Xamarin.Forms.Platform.WPF issue: https://github.com/xamarin/Xamarin.Forms/issues/3648
                }
            });
        }

        private async Task AnimateRefreshIconAsync() {
            await RefreshIconAnimator.AnimateOneRotationAsync(RefreshIcon);

            if (viewModel.IsPrinterListRefreshing) {
                await AnimateRefreshIconAsync();
            }
        }

        private async Task DiscoverPrintersAsync() {
            await ClearDiscoveredPrinterListAsync();

            await Task.Factory.StartNew(() => {
                viewModel.IsPrinterListRefreshing = true;
            });

            Device.BeginInvokeOnMainThread(async () => {
                await AnimateRefreshIconAsync();
            });

            await Task.Factory.StartNew(() => {
                try {
                    List<DiscoveredPrinter> usbDriverPrinters = DependencyService.Get<IConnectionManager>().GetZebraUsbDriverPrinters();
                    foreach (DiscoveredPrinter printer in usbDriverPrinters) {
                        Device.BeginInvokeOnMainThread(() => {
                            viewModel.DiscoveredPrinterList.Add(printer); // ListView view model operations must be done on UI thread due to iOS issues when clearing list while item is selected: https://forums.xamarin.com/discussion/19114/invalid-number-of-rows-in-section
                        });
                    }
                } catch (Exception) {
                    // Do nothing
                }

                try {
                    DiscoveryHandlerImplementation usbDiscoveryHandler = new DiscoveryHandlerImplementation(this);
                    DependencyService.Get<IConnectionManager>().GetZebraUsbDirectPrinters(usbDiscoveryHandler);

                    while (!usbDiscoveryHandler.IsFinished) {
                        Thread.Sleep(100);
                    }
                } catch (Exception) {
                    // Do nothing
                }

                DiscoveryHandlerImplementation networkDiscoveryHandler = new DiscoveryHandlerImplementation(this);
                NetworkDiscoverer.LocalBroadcast(networkDiscoveryHandler);

                while (!networkDiscoveryHandler.IsFinished) {
                    Thread.Sleep(100);
                }

                if (Device.RuntimePlatform != Device.WPF || DependencyService.Get<IPlatformHelper>().IsWindows10()) {
                    try {
                        DiscoveryHandlerImplementation bluetoothDiscoveryHandler = new DiscoveryHandlerImplementation(this);
                        DependencyService.Get<IConnectionManager>().FindBluetoothPrinters(bluetoothDiscoveryHandler);

                        while (!bluetoothDiscoveryHandler.IsFinished) {
                            Thread.Sleep(100);
                        }
                    } catch (Exception) {
                        // Do nothing
                    }
                }
            });

            await Task.Factory.StartNew(() => {
                viewModel.IsPrinterListRefreshing = false;
            });

            ViewExtensions.CancelAnimations(RefreshIcon);
        }

        private async void RefreshIcon_Tapped(object sender, EventArgs e) {
            if (!viewModel.IsPrinterListRefreshing) {
                await DiscoverPrintersAsync();
            }
        }

        private async void SelectButton_Clicked(object sender, EventArgs eventArgs) {
            if (!viewModel.IsSelectingPrinter && Navigation.NavigationStack.Count > 0 && Navigation.NavigationStack.Last().GetType() == typeof(SelectPrinterPage)) {
                await Task.Factory.StartNew(() => {
                    viewModel.IsSelectingPrinter = true;
                });

                try {
                    DiscoveredPrinter selectedPrinter = (DiscoveredPrinter)PrinterList.SelectedItem;
                    Connection connection = null;

                    try {
                        await Task.Factory.StartNew(() => {
                            connection = ConnectionCreator.Create(selectedPrinter);
                            connection.Open();
                        });
                    } catch (Exception e) {
                        await AlertCreator.ShowErrorAsync(this, e.Message);
                        return;
                    } finally {
                        await Task.Factory.StartNew(() => {
                            try {
                                connection?.Close();
                            } catch (ConnectionException) { }
                        });
                    }

                    await Task.Factory.StartNew(() => {
                        mainPage.ViewModel.SelectedPrinter = selectedPrinter;
                    });

                    mainPage.RefreshFormatLists();

                    await Navigation.PopAsync();
                } finally {
                    await Task.Factory.StartNew(() => {
                        viewModel.IsSelectingPrinter = false;
                    });
                }
            }
        }

        class DiscoveryHandlerImplementation : DiscoveryHandler {

            private SelectPrinterPage selectPrinterPage;

            public bool IsFinished { get; private set; } = false;

            public DiscoveryHandlerImplementation(SelectPrinterPage selectPrinterPage) {
                this.selectPrinterPage = selectPrinterPage;
            }

            public void DiscoveryError(string message) {
                IsFinished = true;
            }

            public void DiscoveryFinished() {
                IsFinished = true;
            }

            public void FoundPrinter(DiscoveredPrinter printer) {
                Device.BeginInvokeOnMainThread(() => {
                    selectPrinterPage.viewModel.DiscoveredPrinterList.Add(printer); // ListView view model operations must be done on UI thread due to iOS issues when clearing list while item is selected: https://forums.xamarin.com/discussion/19114/invalid-number-of-rows-in-section
                });
            }
        }
    }
}