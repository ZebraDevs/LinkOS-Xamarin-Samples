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

using System.Collections.Generic;
using Zebra.Sdk.Comm;
using Zebra.Sdk.Printer.Discovery;

namespace XamarinDevDemo {

    public interface IConnectionManager {

        string BuildBluetoothConnectionChannelsString(string macAddress);

        void FindBluetoothPrinters(DiscoveryHandler discoveryHandler);

        Connection GetBluetoothConnection(string macAddress);

        StatusConnection GetBluetoothStatusConnection(string macAddress);

        MultichannelConnection GetMultichannelBluetoothConnection(string macAddress);

        Connection GetUsbConnection(string symbolicName);

        void GetZebraUsbDirectPrinters(DiscoveryHandler discoveryHandler);

        List<DiscoveredPrinter> GetZebraUsbDriverPrinters();
    }
}
