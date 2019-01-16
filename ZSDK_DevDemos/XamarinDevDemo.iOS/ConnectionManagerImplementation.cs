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
using Xamarin.Forms;
using XamarinDevDemo.iOS;
using Zebra.Sdk.Comm;
using Zebra.Sdk.Printer.Discovery;

[assembly: Dependency(typeof(ConnectionManagerImplementation))]
namespace XamarinDevDemo.iOS {

    public class ConnectionManagerImplementation : IConnectionManager {

        public string BuildBluetoothConnectionChannelsString(string macAddress) {
            throw new NotImplementedException();
        }

        public void FindBluetoothPrinters(DiscoveryHandler discoveryHandler) {
            BluetoothDiscoverer.FindPrinters(discoveryHandler);
        }

        public Connection GetBluetoothConnection(string macAddress) {
            return new BluetoothConnection(macAddress);
        }

        public StatusConnection GetBluetoothStatusConnection(string macAddress) {
            throw new NotImplementedException();
        }

        public MultichannelConnection GetMultichannelBluetoothConnection(string macAddress) {
            throw new NotImplementedException();
        }

        public Connection GetUsbConnection(string symbolicName) {
            throw new NotImplementedException();
        }

        public void GetZebraUsbDirectPrinters(DiscoveryHandler discoveryHandler) {
            throw new NotImplementedException();
        }

        public List<DiscoveredPrinter> GetZebraUsbDriverPrinters() {
            throw new NotImplementedException();
        }
    }
}