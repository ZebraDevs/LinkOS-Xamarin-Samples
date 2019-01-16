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
using System.Collections.Specialized;
using System.Text;
using Xamarin.Forms;
using Zebra.Sdk.Printer.Discovery;

namespace XamarinPrintStation {
    public class MainPageViewModel : ViewModelBase {

        private DiscoveredPrinter selectedPrinter;
        private bool isStoredFormatListRefreshing = false;
        private bool isPrinterFormatListRefreshing = false;
        private bool isSelectingNfcPrinter = false;
        private bool isSavingFormat = false;
        private bool isDeletingFormat = false;

        public DiscoveredPrinter SelectedPrinter {
            get => selectedPrinter;
            set {
                selectedPrinter = value;
                OnPropertyChanged();
            }
        }

        public bool IsStoredFormatListRefreshing {
            get => isStoredFormatListRefreshing;
            set {
                isStoredFormatListRefreshing = value;
                OnPropertyChanged();
            }
        }

        public bool IsPrinterFormatListRefreshing {
            get => isPrinterFormatListRefreshing;
            set {
                isPrinterFormatListRefreshing = value;
                OnPropertyChanged();
            }
        }

        public bool IsSelectingNfcPrinter {
            get => isSelectingNfcPrinter;
            set {
                isSelectingNfcPrinter = value;
                OnPropertyChanged();
            }
        }

        public bool IsSavingFormat {
            get => isSavingFormat;
            set {
                isSavingFormat = value;
                OnPropertyChanged();
            }
        }

        public bool IsDeletingFormat {
            get => isDeletingFormat;
            set {
                isDeletingFormat = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FormatViewModel> StoredFormatList { get; } = new ObservableCollection<FormatViewModel>();

        public ObservableCollection<FormatViewModel> PrinterFormatList { get; } = new ObservableCollection<FormatViewModel>();

        public MainPageViewModel() {
            StoredFormatList.CollectionChanged += StoredFormatList_CollectionChanged;
            PrinterFormatList.CollectionChanged += PrinterFormatList_CollectionChanged;
        }

        private void StoredFormatList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(StoredFormatList));
        }

        private void PrinterFormatList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(PrinterFormatList));
        }
    }
}
