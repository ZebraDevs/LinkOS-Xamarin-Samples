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
using XamarinPrintStation.WPF;
using Zebra.Sdk.Printer.Discovery;

[assembly: Dependency(typeof(PrinterHelperImplementation))]
namespace XamarinPrintStation.WPF {
    public class PrinterHelperImplementation : IPrinterHelper {
        public bool IsBluetoothPrinter(DiscoveredPrinter printer) {
            return printer is DiscoveredPrinterBluetooth;
        }

        public bool IsUsbDirectPrinter(DiscoveredPrinter printer) {
            return printer is DiscoveredUsbPrinter;
        }

        public bool IsUsbDriverPrinter(DiscoveredPrinter printer) {
            return printer is DiscoveredPrinterDriver;
        }
    }
}
