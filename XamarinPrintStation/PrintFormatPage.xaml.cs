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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Zebra.Sdk.Comm;
using Zebra.Sdk.Printer;
using Zebra.Sdk.Printer.Discovery;

namespace XamarinPrintStation {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PrintFormatPage : ContentPage {

        public const string DeviceLanguagesSgd = "device.languages";

        private DiscoveredPrinter selectedPrinter;
        private FormatViewModel format;
        private PrintFormatPageViewModel viewModel = new PrintFormatPageViewModel();

        public PrintFormatPage(DiscoveredPrinter selectedPrinter, FormatViewModel format) {
            InitializeComponent();

            this.selectedPrinter = selectedPrinter;
            this.format = format;
            BindingContext = viewModel;

            Device.BeginInvokeOnMainThread(async () => {
                await PopulateVariableFieldListAsync();
            });
        }

        private async Task PopulateVariableFieldListAsync() {
            await Task.Factory.StartNew(() => {
                viewModel.IsVariableFieldListRefreshing = true;

                try {
                    viewModel.FormatVariableList.Clear();
                } catch (NotImplementedException) {
                    viewModel.FormatVariableList.Clear(); // Workaround for Xamarin.Forms.Platform.WPF issue: https://github.com/xamarin/Xamarin.Forms/issues/3648
                }
            });

            Connection connection = null;
            bool linePrintEnabled = false;

            try {
                await Task.Factory.StartNew(() => {
                    connection = ConnectionCreator.Create(selectedPrinter);
                    connection.Open();

                    string originalPrinterLanguage = SGD.GET(DeviceLanguagesSgd, connection);
                    linePrintEnabled = "line_print".Equals(originalPrinterLanguage, StringComparison.OrdinalIgnoreCase);

                    if (linePrintEnabled) {
                        SGD.SET(DeviceLanguagesSgd, "zpl", connection);
                    }

                    ZebraPrinter printer = ZebraPrinterFactory.GetInstance(connection);
                    ZebraPrinterLinkOs linkOsPrinter = ZebraPrinterFactory.CreateLinkOsPrinter(printer);

                    if (format.Source == FormatSource.Printer) {
                        format.Content = Encoding.UTF8.GetString(printer.RetrieveFormatFromPrinter(format.PrinterPath));
                    }

                    FieldDescriptionData[] variableFields = printer.GetVariableFields(format.Content);
                    foreach (FieldDescriptionData variableField in variableFields) {
                        viewModel.FormatVariableList.Add(new FormatVariable {
                            Name = variableField.FieldName ?? $"Field {variableField.FieldNumber}",
                            Index = variableField.FieldNumber,
                        });
                    }
                });
            } catch (Exception e) {
                await AlertCreator.ShowErrorAsync(this, e.Message);
            } finally {
                if (linePrintEnabled) {
                    await ResetPrinterLanguageToLinePrintAsync(connection);
                }

                await Task.Factory.StartNew(() => {
                    try {
                        connection?.Close();
                    } catch (ConnectionException) { }

                    viewModel.IsVariableFieldListRefreshing = false;
                });
            }
        }

        private string GetPrinterStatusErrorMessage(PrinterStatus printerStatus) {
            if (printerStatus.isReadyToPrint) {
                if (printerStatus.isHeadCold) {
                    return "Printhead too cold";
                } else if (printerStatus.isPartialFormatInProgress) {
                    return "Partial format in progress";
                }
            } else {
                if (printerStatus.isHeadTooHot) {
                    return "Printhead too hot";
                } else if (printerStatus.isHeadOpen) {
                    return "Printhead open";
                } else if (printerStatus.isPaperOut) {
                    return "Media out";
                } else if (printerStatus.isReceiveBufferFull) {
                    return "Receive buffer full";
                } else if (printerStatus.isRibbonOut) {
                    return "Ribbon error";
                } else if (printerStatus.isPaused) {
                    return "Printer paused";
                } else {
                    return "Unknown error";
                }
            }
            return null;
        }

        private Dictionary<int, string> BuildFormatVariableDictionary() {
            Dictionary<int, string> dictionary = new Dictionary<int, string>();
            foreach (FormatVariable formatVariable in viewModel.FormatVariableList) {
                dictionary.Add(formatVariable.Index, formatVariable.Value);
            }
            return dictionary;
        }

        private async Task ResetPrinterLanguageToLinePrintAsync(Connection connection) {
            await Task.Factory.StartNew(() => {
                try {
                    connection?.Open();
                    SGD.SET(DeviceLanguagesSgd, "line_print", connection);
                } catch (ConnectionException) { }
            });
        }

        private async void PrintButton_Clicked(object sender, EventArgs eventArgs) {
            await Task.Factory.StartNew(() => {
                viewModel.IsSendingPrintJob = true;
            });

            Connection connection = null;
            bool linePrintEnabled = false;

            try {
                await Task.Factory.StartNew(() => {
                    connection = ConnectionCreator.Create(selectedPrinter);
                    connection.Open();

                    string originalPrinterLanguage = SGD.GET(DeviceLanguagesSgd, connection);
                    linePrintEnabled = "line_print".Equals(originalPrinterLanguage, StringComparison.OrdinalIgnoreCase);

                    if (linePrintEnabled) {
                        SGD.SET(DeviceLanguagesSgd, "zpl", connection);
                    }

                    ZebraPrinter printer = ZebraPrinterFactory.GetInstance(connection);
                    ZebraPrinterLinkOs linkOsPrinter = ZebraPrinterFactory.CreateLinkOsPrinter(printer);

                    string errorMessage = GetPrinterStatusErrorMessage(printer.GetCurrentStatus());
                    if (errorMessage != null) {
                        throw new PrinterNotReadyException($"{errorMessage}. Please check your printer and try again.");
                    } else {
                        if (format.Source != FormatSource.Printer) {
                            connection.Write(Encoding.UTF8.GetBytes(format.Content));
                        }

                        linkOsPrinter.PrintStoredFormatWithVarGraphics(format.PrinterPath, BuildFormatVariableDictionary(), "UTF-8");
                    }
                });

                if (linePrintEnabled) {
                    await ResetPrinterLanguageToLinePrintAsync(connection);
                }

                await Task.Factory.StartNew(() => {
                    viewModel.IsSendingPrintJob = false;
                });
            } catch (PrinterNotReadyException e) {
                if (linePrintEnabled) {
                    await ResetPrinterLanguageToLinePrintAsync(connection);
                }

                await Task.Factory.StartNew(() => {
                    viewModel.IsSendingPrintJob = false;
                });

                await AlertCreator.ShowAsync(this, "Printer Error", e.Message);
            } catch (Exception e) {
                if (linePrintEnabled) {
                    await ResetPrinterLanguageToLinePrintAsync(connection);
                }

                await Task.Factory.StartNew(() => {
                    viewModel.IsSendingPrintJob = false;
                });

                await AlertCreator.ShowErrorAsync(this, e.Message);
            } finally {
                await Task.Factory.StartNew(() => {
                    try {
                        connection?.Close();
                    } catch (ConnectionException) { }
                });
            }
        }
    }
}