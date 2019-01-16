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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Zebra.Sdk.Comm;
using Zebra.Sdk.Printer;
using Zebra.Sdk.Printer.Discovery;

namespace XamarinPrintStation {
    public partial class MainPage : ContentPage {

        private const string IsAppInitializedPropertyName = "IsAppInitialized";

        public MainPageViewModel ViewModel { get; } = new MainPageViewModel();

        public MainPage() {
            InitializeComponent();

            BindingContext = ViewModel;

            if (Device.RuntimePlatform != Device.WPF || !DependencyService.Get<IPlatformHelper>().IsWindows10()) {
                ToolbarItems.Remove(BluetoothHelpButton); // Bluetooth help dialog is specific to Windows 10
            }

            try {
                if (DependencyService.Get<INfcManager>().IsNfcAvailable()) {
                    DependencyService.Get<INfcManager>().TagScanned += async (object sender, string nfcData) => {
                        Device.BeginInvokeOnMainThread(async () => {
                            await Navigation.PopToRootAsync(); // Must call Navigation.PopToRootAsync() on UI thread on Windows tablets
                        });

                        await Task.Factory.StartNew(() => {
                            ViewModel.IsSelectingNfcPrinter = true;
                            ViewModel.SelectedPrinter = null;

                            try {
                                ViewModel.StoredFormatList.Clear();
                            } catch (NotImplementedException) {
                                ViewModel.StoredFormatList.Clear(); // Workaround for Xamarin.Forms.Platform.WPF issue: https://github.com/xamarin/Xamarin.Forms/issues/3648
                            }

                            try {
                                ViewModel.PrinterFormatList.Clear();
                            } catch (NotImplementedException) {
                                ViewModel.PrinterFormatList.Clear(); // Workaround for Xamarin.Forms.Platform.WPF issue: https://github.com/xamarin/Xamarin.Forms/issues/3648
                            }
                        });

                        Connection connection = null;
                        DiscoveredPrinter printer = null;
                        bool success = false;

                        try {
                            await Task.Factory.StartNew(() => {
                                printer = NfcDiscoverer.DiscoverPrinter(nfcData);

                                connection = printer.GetConnection();
                                try {
                                    connection.Open(); // Test connection
                                } catch (Exception e) {
                                    throw new ConnectionException("Could not open connection. Please check your printer and device settings and try again.", e);
                                }

                                success = true;
                            });
                        } catch (Exception e) {
                            await AlertCreator.ShowErrorAsync(this, e.Message);
                        } finally {
                            await Task.Factory.StartNew(() => {
                                try {
                                    connection?.Close();
                                } catch (Exception) { }

                                if (success) {
                                    ViewModel.SelectedPrinter = printer;
                                    ViewModel.IsSelectingNfcPrinter = false;
                                    RefreshFormatLists();
                                } else {
                                    ViewModel.IsSelectingNfcPrinter = false;
                                }
                            });
                        }
                    };
                }
            } catch (TypeLoadException) { } // NFC is not supported on Windows 7

            InitializeStoredFormats();
        }

        private async Task AnimateStoredFormatsRefreshIconAsync() {
            await RefreshIconAnimator.AnimateOneRotationAsync(StoredFormatsRefreshIcon);

            if (ViewModel.IsStoredFormatListRefreshing) {
                await AnimateStoredFormatsRefreshIconAsync();
            }
        }

        private async Task AnimatePrinterFormatsRefreshIconAsync() {
            await RefreshIconAnimator.AnimateOneRotationAsync(PrinterFormatsRefreshIcon);

            if (ViewModel.IsPrinterFormatListRefreshing) {
                await AnimatePrinterFormatsRefreshIconAsync();
            }
        }

        private string GetStoredFormatDirectory() {
            string directoryName;

            try {
                directoryName = DependencyService.Get<IPlatformHelper>().GetIOSBundleIdentifier();
            } catch (NotImplementedException) {
                directoryName = "Print Station";
            }

            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), directoryName);
        }

        private void InitializeStoredFormats() {
            if (!Application.Current.Properties.ContainsKey(IsAppInitializedPropertyName)) {
                Task.Factory.StartNew(async () => {
                    await App.Database.SaveStoredFormatAsync(new PrintStationDatabase.StoredFormat {
                        DriveLetter = "E",
                        Name = "OILCHANGE",
                        Extension = "ZPL",
                        Content = "^XA" +
                            "^CI28" +
                            "^DFE:OILCHANGE.ZPL^FS" +
                            "^FT181,184^A0N,28,28^FH\\^FN1\"Date\"^FS" +
                            "^FT181,282^A0N,28,28^FH\\^FN2\"Mileage\"^FS" +
                            "^FT32,106^GFA,1024,1024,16::::::::::::::L0F8,L0HFI0IFC0,L0HFC00FHFC0K0380,K01FHFC0FHFC0J01FC0,K03E7FF00780K03FE0,K03E1FF00780J01FHF0,K03C03F00780J07FDF8,K07800700780I01FF8F0,K078007FKF807FF870,K07F007FKFC1FHF0,K07FF07FLF7FDF0,K03FFC7FNF3E0,L0JFL0HFC3E010,M0IFL07F07C010,M03FF0K03C078018,N03F0N0F8018,O0F0M01F0038,O070M01E003C,O070M03E003C,O070M07C003C,O070M0780030,O07FNF8,O07FNF0,:O07FMFE0,,:::::::::::::::::::::::^FT60,177^A0N,34,33^FH\\^FDDATE^FS" +
                            "^FT33,282^A0N,34,33^FH\\^FDMILEAGE^FS" +
                            "^FO172,239^GB287,64,8^FS" +
                            "^FO172,139^GB287,64,8^FS" +
                            "^FT153,77^A0N,56,55^FH\\^FDOIL CHANGE^FS" +
                            "^FO27,110^GB432,0,8^FS" +
                            "^XZ"
                    });

                    await App.Database.SaveStoredFormatAsync(new PrintStationDatabase.StoredFormat {
                        DriveLetter = "E",
                        Name = "ADDRESS",
                        Extension = "ZPL",
                        Content = "^XA" +
                            "^CI28" +
                            "^DFE:ADDRESS.ZPL^FS" +
                            "^FT80,80^A0N,28,28^FH\\^FN1\"Name\"^FS" +
                            "^FT80,110^A0N,28,28^FH\\^FN2\"Address1\"^FS" +
                            "^FT80,140^A0N,28,28^FH\\^FN3\"Address2\"^FS" +
                            "^FT80,170^A0N,28,28^FH\\^FN4\"CityStateZip\"^FS" +
                            "^XZ"
                    });
                });

                Application.Current.Properties.Add(IsAppInitializedPropertyName, true);
                Application.Current.SavePropertiesAsync();
            }
        }

        private async Task RefreshStoredFormatListAsync() {
            await Task.Factory.StartNew(() => {
                try {
                    ViewModel.StoredFormatList.Clear();
                } catch (NotImplementedException) {
                    ViewModel.StoredFormatList.Clear(); // Workaround for Xamarin.Forms.Platform.WPF issue: https://github.com/xamarin/Xamarin.Forms/issues/3648
                }
            });

            if (ViewModel.SelectedPrinter != null) {
                await Task.Factory.StartNew(() => {
                    ViewModel.IsStoredFormatListRefreshing = true;
                });

                Device.BeginInvokeOnMainThread(async () => {
                    await AnimateStoredFormatsRefreshIconAsync();
                });

                await Task.Factory.StartNew(() => {
                    Thread.Sleep(1000); // Give the user some time to read the progress message to confirm that refresh is occurring
                });

                await AddStoredFormatsToFormatListAsync();

                await Task.Factory.StartNew(() => {
                    ViewModel.IsStoredFormatListRefreshing = false;
                });

                ViewExtensions.CancelAnimations(StoredFormatsRefreshIcon);
            }
        }

        private async Task RefreshPrinterFormatListAsync() {
            await Task.Factory.StartNew(() => {
                try {
                    ViewModel.PrinterFormatList.Clear();
                } catch (NotImplementedException) {
                    ViewModel.PrinterFormatList.Clear(); // Workaround for Xamarin.Forms.Platform.WPF issue: https://github.com/xamarin/Xamarin.Forms/issues/3648
                }
            });

            if (ViewModel.SelectedPrinter != null) {
                await Task.Factory.StartNew(() => {
                    ViewModel.IsPrinterFormatListRefreshing = true;
                });

                Device.BeginInvokeOnMainThread(async () => {
                    await AnimatePrinterFormatsRefreshIconAsync();
                });

                await AddPrinterFormatsToFormatListAsync();
                
                await Task.Factory.StartNew(() => {
                    ViewModel.IsPrinterFormatListRefreshing = false;
                });

                ViewExtensions.CancelAnimations(PrinterFormatsRefreshIcon);
            }
        }

        public void RefreshFormatLists() {
            Device.BeginInvokeOnMainThread(async () => {
                await RefreshStoredFormatListAsync();
            });

            Device.BeginInvokeOnMainThread(async () => {
                await RefreshPrinterFormatListAsync();
            });
        }

        private async Task AddStoredFormatsToFormatListAsync() {
            List<FormatViewModel> formats = new List<FormatViewModel>();

            await Task.Factory.StartNew(async () => {
                List<PrintStationDatabase.StoredFormat> storedFormats = await App.Database.GetStoredFormatsAsync();
                foreach (PrintStationDatabase.StoredFormat storedFormat in storedFormats) {
                    if (string.Equals(storedFormat.Extension, "zpl", StringComparison.OrdinalIgnoreCase)) {
                        formats.Add(new FormatViewModel {
                            DatabaseId = storedFormat.Id,
                            DriveLetter = storedFormat.DriveLetter,
                            Name = storedFormat.Name,
                            Extension = storedFormat.Extension,
                            Content = storedFormat.Content,
                            Source = FormatSource.LocalStorage,
                            OnDeleteButtonClicked = new Command(async (object parameter) => {
                                if (!ViewModel.IsPrinterFormatListRefreshing && !ViewModel.IsSavingFormat) {
                                    FormatViewModel format = parameter as FormatViewModel;

                                    bool deleteFormat = await DisplayAlert("Delete Format", $"Are you sure you want to delete the stored format '{format.PrinterPath}'?", "Delete", "Cancel");
                                    if (deleteFormat) {
                                        await Task.Factory.StartNew(() => {
                                            ViewModel.IsDeletingFormat = true;
                                            format.IsDeleting = true;
                                        });

                                        await App.Database.DeleteStoredFormatByIdAsync(format.DatabaseId);

                                        await Task.Factory.StartNew(() => {
                                            format.IsDeleting = false;
                                            ViewModel.IsDeletingFormat = false;
                                        });

                                        await RefreshStoredFormatListAsync();
                                    }
                                }
                            }),
                            OnPrintButtonClicked = new Command(async (object parameter) => {
                                if (!ViewModel.IsPrinterFormatListRefreshing && !ViewModel.IsSavingFormat) {
                                    FormatViewModel format = parameter as FormatViewModel;
                                    await PushPageAsync(new PrintFormatPage(ViewModel.SelectedPrinter, format));
                                }
                            })
                        });
                    }
                }

                formats.ForEach(ViewModel.StoredFormatList.Add);
            });
        }

        private async Task<string> RetrieveFormatContentAsync(FormatViewModel format) {
            string formatContent = null;
            Connection connection = null;

            try {
                await Task.Factory.StartNew(() => {
                    connection = ConnectionCreator.Create(ViewModel.SelectedPrinter);
                    connection.Open();

                    ZebraPrinter printer = ZebraPrinterFactory.GetInstance(connection);
                    ZebraPrinterLinkOs linkOsPrinter = ZebraPrinterFactory.CreateLinkOsPrinter(printer);

                    formatContent = Encoding.UTF8.GetString(printer.RetrieveFormatFromPrinter(format.PrinterPath));
                });
            } catch (Exception e) {
                await AlertCreator.ShowErrorAsync(this, e.Message);
            } finally {
                await Task.Factory.StartNew(() => {
                    try {
                        connection?.Close();
                    } catch (ConnectionException) { }
                });
            }

            return formatContent;
        }

        private async Task SavePrinterFormatAsync(FormatViewModel format) {
            PrintStationDatabase.StoredFormat storedFormat = null;
            Connection connection = null;
            bool fileWriteAttempted = false;
            bool linePrintEnabled = false;

            try {
                await Task.Factory.StartNew(async () => {
                    connection = ConnectionCreator.Create(ViewModel.SelectedPrinter);
                    connection.Open();

                    string originalPrinterLanguage = SGD.GET(PrintFormatPage.DeviceLanguagesSgd, connection);
                    linePrintEnabled = "line_print".Equals(originalPrinterLanguage, StringComparison.OrdinalIgnoreCase);

                    if (linePrintEnabled) {
                        SGD.SET(PrintFormatPage.DeviceLanguagesSgd, "zpl", connection);
                    }

                    ZebraPrinter printer = ZebraPrinterFactory.GetInstance(connection);
                    ZebraPrinterLinkOs linkOsPrinter = ZebraPrinterFactory.CreateLinkOsPrinter(printer);

                    string formatContent = Encoding.UTF8.GetString(printer.RetrieveFormatFromPrinter(format.PrinterPath));

                    fileWriteAttempted = true;
                    storedFormat = new PrintStationDatabase.StoredFormat {
                        DriveLetter = format.DriveLetter,
                        Name = format.Name,
                        Extension = format.Extension,
                        Content = formatContent
                    };
                    await App.Database.SaveStoredFormatAsync(storedFormat);
                });
            } catch (Exception e) {
                await Task.Factory.StartNew(async () => {
                    if (fileWriteAttempted && storedFormat != null) {
                        await App.Database.DeleteStoredFormatByIdAsync(storedFormat.Id);
                    }
                });

                await AlertCreator.ShowErrorAsync(this, e.Message);
            } finally {
                if (linePrintEnabled) {
                    await Task.Factory.StartNew(() => {
                        try {
                            connection?.Open();
                            SGD.SET(PrintFormatPage.DeviceLanguagesSgd, "line_print", connection);
                        } catch (ConnectionException) { }
                    });
                }

                await Task.Factory.StartNew(() => {
                    try {
                        connection?.Close();
                    } catch (ConnectionException) { }
                });
            }
        }

        private async Task AddPrinterFormatsToFormatListAsync() {
            Connection connection = null;

            try {
                List<FormatViewModel> formats = new List<FormatViewModel>();

                await Task.Factory.StartNew(() => {
                    connection = ConnectionCreator.Create(ViewModel.SelectedPrinter);
                    connection.Open();

                    ZebraPrinter printer = ZebraPrinterFactory.GetInstance(connection);
                    ZebraPrinterLinkOs linkOsPrinter = ZebraPrinterFactory.CreateLinkOsPrinter(printer);

                    string[] formatPrinterPaths = printer.RetrieveFileNames(new string[] { "ZPL" });
                    foreach (string formatPrinterPath in formatPrinterPaths) {
                        int colonIndex = formatPrinterPath.IndexOf(":");
                        string driveLetter = formatPrinterPath.Substring(0, colonIndex);
                        string name = formatPrinterPath.Substring(colonIndex + 1, formatPrinterPath.LastIndexOf(".") - colonIndex - 1);
                        string extension = formatPrinterPath.Substring(formatPrinterPath.LastIndexOf(".") + 1);

                        if (!string.Equals(driveLetter, "Z", StringComparison.OrdinalIgnoreCase)) { // Ignore all formats stored on printer's Z drive
                            formats.Add(new FormatViewModel {
                                DriveLetter = driveLetter,
                                Name = name,
                                Extension = extension,
                                Content = null, // Populate this later, upon navigating to print format page
                                Source = FormatSource.Printer,
                                OnSaveButtonClicked = new Command(async (object parameter) => {
                                    if (!ViewModel.IsSavingFormat && !ViewModel.IsDeletingFormat && !ViewModel.IsStoredFormatListRefreshing) {
                                        FormatViewModel format = parameter as FormatViewModel;

                                        await Task.Factory.StartNew(() => {
                                            ViewModel.IsSavingFormat = true;
                                            format.IsSaving = true;
                                        });

                                        await SavePrinterFormatAsync(format);

                                        await Task.Factory.StartNew(() => {
                                            format.IsSaving = false;
                                            ViewModel.IsSavingFormat = false;
                                        });

                                        await RefreshStoredFormatListAsync();
                                    }
                                }),
                                OnPrintButtonClicked = new Command(async (object parameter) => {
                                    if (!ViewModel.IsSavingFormat) {
                                        FormatViewModel format = parameter as FormatViewModel;
                                        await PushPageAsync(new PrintFormatPage(ViewModel.SelectedPrinter, format));
                                    }
                                })
                            });
                        }
                    }

                    formats.ForEach(ViewModel.PrinterFormatList.Add);
                });
            } catch (Exception e) {
                await AlertCreator.ShowErrorAsync(this, e.Message);
            } finally {
                await Task.Factory.StartNew(() => {
                    try {
                        connection?.Close();
                    } catch (ConnectionException) { }
                });
            }
        }

        private async Task PushPageAsync(Page page) {
            if (Navigation.NavigationStack.Count == 0 || Navigation.NavigationStack.Last().GetType() == typeof(MainPage)) {
                await Navigation.PushAsync(page);
            }
        }

        private async Task PushModalAsync(Page page) {
            if (Navigation.ModalStack.Count == 0) {
                await Navigation.PushModalAsync(page);
            }
        }

        private async void StoredFormatsRefreshIcon_Tapped(object sender, EventArgs e) {
            if (!ViewModel.IsStoredFormatListRefreshing) {
                await RefreshStoredFormatListAsync();
            }
        }

        private async void PrinterFormatsRefreshIcon_Tapped(object sender, EventArgs e) {
            if (!ViewModel.IsPrinterFormatListRefreshing) {
                await RefreshPrinterFormatListAsync();
            }
        }

        private async void SelectPrinter_Clicked(object sender, EventArgs e) {
            await PushPageAsync(new SelectPrinterPage(this));
        }

        private async void AboutButton_Clicked(object sender, EventArgs e) {
            await PushModalAsync(new AboutPage());
        }

        private async void BluetoothHelpButton_Clicked(object sender, EventArgs e) {
            await PushModalAsync(new BluetoothHelpPage());
        }
    }
}
