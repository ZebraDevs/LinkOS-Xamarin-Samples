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
using Zebra.Sdk.Comm;
using Zebra.Sdk.Printer.Discovery;

namespace XamarinPrintStation {
    public interface IConnectionManager {

        void FindBluetoothPrinters(DiscoveryHandler discoveryHandler);

        void FindUrlPrinters(string nfcData, DiscoveryHandler discoveryHandler);

        Connection GetBluetoothConnection(string macAddress);

        Connection GetUsbDirectConnection(string symbolicName);

        Connection GetUsbDriverConnection(string printerName);

        void GetZebraUsbDirectPrinters(DiscoveryHandler discoveryHandler);

        List<DiscoveredPrinter> GetZebraUsbDriverPrinters();

        bool IsBluetoothPrinter(DiscoveredPrinter printer);
    }
}
