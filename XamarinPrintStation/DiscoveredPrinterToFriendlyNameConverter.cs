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
using System.Globalization;
using System.Text;
using Xamarin.Forms;
using Zebra.Sdk.Printer.Discovery;

namespace XamarinPrintStation {
    public class DiscoveredPrinterToFriendlyNameConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is DiscoveredPrinter printer) {
                if (printer is DiscoveredPrinterNetwork && printer.DiscoveryDataMap.ContainsKey("SYSTEM_NAME")) {
                    return printer.DiscoveryDataMap["SYSTEM_NAME"];
                } else if (DependencyService.Get<IConnectionManager>().IsBluetoothPrinter(printer) && printer.DiscoveryDataMap.ContainsKey("FRIENDLY_NAME")) {
                    return printer.DiscoveryDataMap["FRIENDLY_NAME"];
                } else if (printer.DiscoveryDataMap.ContainsKey("SERIAL_NUMBER")) {
                    return printer.DiscoveryDataMap["SERIAL_NUMBER"];
                } else {
                    return printer.Address;
                }
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
