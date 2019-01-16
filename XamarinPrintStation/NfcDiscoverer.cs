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
using System.Text;
using System.Threading;
using Xamarin.Forms;
using Zebra.Sdk.Printer.Discovery;

namespace XamarinPrintStation {
    public static class NfcDiscoverer {
        public static DiscoveredPrinter DiscoverPrinter(string nfcData) {
            DiscoveredPrinter printer = null;

            try {
                NfcDiscoveryHandler nfcDiscoveryHandler = new NfcDiscoveryHandler();
                DependencyService.Get<IConnectionManager>().FindUrlPrinters(nfcData, nfcDiscoveryHandler);

                while (!nfcDiscoveryHandler.IsFinished) {
                    Thread.Sleep(100);
                }

                printer = nfcDiscoveryHandler.PreferredPrinter;
            } catch (Exception) { }

            return printer;
        }

        private class NfcDiscoveryHandler : DiscoveryHandler {

            public bool IsFinished { get; private set; } = false;

            public DiscoveredPrinter PreferredPrinter { get; private set; } = null;

            public void DiscoveryError(string message) {
                IsFinished = true;
            }

            public void DiscoveryFinished() {
                IsFinished = true;
            }

            public void FoundPrinter(DiscoveredPrinter printer) {
                if (PreferredPrinter == null || (DependencyService.Get<IConnectionManager>().IsBluetoothPrinter(PreferredPrinter) && printer is DiscoveredPrinterNetwork)) {
                    PreferredPrinter = printer;
                }
            }
        }
    }
}
