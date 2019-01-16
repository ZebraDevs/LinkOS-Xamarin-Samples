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
using Zebra.Sdk.Printer;

namespace XamarinPrintStation {
    public class PrintFormatPageViewModel : ViewModelBase {

        private bool isVariableFieldListRefreshing = false;
        private bool isSendingPrintJob = false;

        public bool IsVariableFieldListRefreshing {
            get => isVariableFieldListRefreshing;
            set {
                isVariableFieldListRefreshing = value;
                OnPropertyChanged();
            }
        }

        public bool IsSendingPrintJob {
            get => isSendingPrintJob;
            set {
                isSendingPrintJob = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FormatVariable> FormatVariableList { get; } = new ObservableCollection<FormatVariable>();

        public PrintFormatPageViewModel() {
            FormatVariableList.CollectionChanged += FormatVariableList_CollectionChanged;
        }

        private void FormatVariableList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(FormatVariableList));
        }
    }
}
