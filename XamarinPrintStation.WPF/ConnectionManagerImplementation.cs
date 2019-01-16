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
using Zebra.Sdk.Comm;
using Zebra.Sdk.Printer.Discovery;

[assembly: Dependency(typeof(ConnectionManagerImplementation))]
namespace XamarinPrintStation.WPF {
    public class ConnectionManagerImplementation : IConnectionManager {

        public void FindBluetoothPrinters(DiscoveryHandler discoveryHandler) {
            BluetoothDiscoverer.FindPrinters(discoveryHandler);
        }

        public void FindUrlPrinters(string nfcData, DiscoveryHandler discoveryHandler) {
            UrlPrinterDiscoverer.FindPrinters(nfcData, discoveryHandler);
        }

        public Connection GetBluetoothConnection(string macAddress) {
            return new BluetoothConnection(macAddress);
        }

        public Connection GetUsbDirectConnection(string symbolicName) {
            return new UsbConnection(symbolicName);
        }

        public Connection GetUsbDriverConnection(string printerName) {
            return new DriverPrinterConnection(printerName);
        }

        public void GetZebraUsbDirectPrinters(DiscoveryHandler discoveryHandler) {
            try {
                foreach (DiscoveredUsbPrinter printer in UsbDiscoverer.GetZebraUsbPrinters()) {
                    discoveryHandler.FoundPrinter(printer);
                }
                discoveryHandler.DiscoveryFinished();
            } catch (Exception e) {
                discoveryHandler.DiscoveryError(e.Message);
            }
        }

        public List<DiscoveredPrinter> GetZebraUsbDriverPrinters() {
            return UsbDiscoverer.GetZebraDriverPrinters().Cast<DiscoveredPrinter>().ToList();
        }

        public bool IsBluetoothPrinter(DiscoveredPrinter printer) {
            return printer is DiscoveredPrinterBluetooth;
        }
    }
}
