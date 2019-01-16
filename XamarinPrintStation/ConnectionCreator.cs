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
using Xamarin.Forms;
using Zebra.Sdk.Comm;
using Zebra.Sdk.Printer.Discovery;

namespace XamarinPrintStation {
    public static class ConnectionCreator {

        public static Connection Create(DiscoveredPrinter printer) {
            if (DependencyService.Get<IPrinterHelper>().IsUsbDirectPrinter(printer)) {
                return DependencyService.Get<IConnectionManager>().GetUsbDirectConnection(printer.Address);
            } else if (DependencyService.Get<IPrinterHelper>().IsUsbDriverPrinter(printer)) {
                return DependencyService.Get<IConnectionManager>().GetUsbDriverConnection(printer.Address);
            } else if (DependencyService.Get<IPrinterHelper>().IsBluetoothPrinter(printer)) {
                return DependencyService.Get<IConnectionManager>().GetBluetoothConnection(printer.Address);
            } else {
                return new TcpConnection(printer.Address, 9100);
            }
        }
    }
}
