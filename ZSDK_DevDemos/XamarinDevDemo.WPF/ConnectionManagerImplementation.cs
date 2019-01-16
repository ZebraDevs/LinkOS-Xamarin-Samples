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
using XamarinDevDemo.WPF;
using Zebra.Sdk.Comm;
using Zebra.Sdk.Printer.Discovery;

[assembly: Dependency(typeof(ConnectionManagerImplementation))]
namespace XamarinDevDemo.WPF {

    public class ConnectionManagerImplementation : IConnectionManager {

        public string BuildBluetoothConnectionChannelsString(string macAddress) {
            BluetoothConnection connection = new BluetoothConnection(macAddress);
            connection.Open(); // Check connection

            try {
                ServiceDiscoveryHandlerImplementation serviceDiscoveryHandler = new ServiceDiscoveryHandlerImplementation();
                BluetoothDiscoverer.FindServices(macAddress, serviceDiscoveryHandler);

                while (!serviceDiscoveryHandler.Finished) {
                    Task.Delay(100);
                }

                StringBuilder sb = new StringBuilder();
                foreach (ConnectionChannel connectionChannel in serviceDiscoveryHandler.ConnectionChannels) {
                    sb.AppendLine(connectionChannel.ToString());
                }
                return sb.ToString();
            } finally {
                try {
                    connection?.Close();
                } catch (ConnectionException) { }
            }
        }

        public void FindBluetoothPrinters(DiscoveryHandler discoveryHandler) {
            BluetoothDiscoverer.FindPrinters(discoveryHandler);
        }

        public Connection GetBluetoothConnection(string macAddress) {
            return new BluetoothConnection(macAddress);
        }

        public StatusConnection GetBluetoothStatusConnection(string macAddress) {
            return new BluetoothStatusConnection(macAddress);
        }

        public MultichannelConnection GetMultichannelBluetoothConnection(string macAddress) {
            return new MultichannelBluetoothConnection(macAddress);
        }

        public Connection GetUsbConnection(string symbolicName) {
            return new UsbConnection(symbolicName);
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

        private class ServiceDiscoveryHandlerImplementation : ServiceDiscoveryHandler {

            public List<ConnectionChannel> ConnectionChannels { get; private set; }

            public bool Finished { get; private set; }

            public ServiceDiscoveryHandlerImplementation() {
                ConnectionChannels = new List<ConnectionChannel>();
            }

            public void DiscoveryFinished() {
                Finished = true;
            }

            public void FoundService(ConnectionChannel channel) {
                ConnectionChannels.Add(channel);
            }
        }
    }
}
