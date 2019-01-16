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

using Android.App;
using Android.Content;
using Android.Hardware.Usb;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using XamarinDevDemo.Droid;
using Zebra.Sdk.Comm;
using Zebra.Sdk.Printer.Discovery;

[assembly: Dependency(typeof(ConnectionManagerImplementation))]
namespace XamarinDevDemo.Droid {

    public class ConnectionManagerImplementation : IConnectionManager {

        private const string ActionUsbPermission = "com.zebra.XamarinDevDemo.USB_PERMISSION";
        private const int UsbPermissionTimeout = 30000;

        private static readonly object UsbConnectionLock = new object();

        private readonly IntentFilter filter = new IntentFilter("ACTION_USB_PERMISSION");

        public string BuildBluetoothConnectionChannelsString(string macAddress) {
            BluetoothConnection connection = new BluetoothConnection(macAddress);
            connection.Open(); // Check connection

            try {
                ServiceDiscoveryHandlerImplementation serviceDiscoveryHandler = new ServiceDiscoveryHandlerImplementation();
                BluetoothDiscoverer.FindServices(Android.App.Application.Context, macAddress, serviceDiscoveryHandler);

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
            BluetoothDiscoverer.FindPrinters(Android.App.Application.Context, discoveryHandler);
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

        private UsbDevice GetUsbDevice(UsbManager usbManager, string deviceAddress) {
            IDictionary<string, UsbDevice> deviceList = usbManager.DeviceList;
            return deviceList != null && deviceList.ContainsKey(deviceAddress) ? deviceList[deviceAddress] : null;
        }

        public Connection GetUsbConnection(string symbolicName) {
            lock (UsbConnectionLock) {
                try {
                    UsbManager usbManager = (UsbManager)Android.App.Application.Context.GetSystemService(Context.UsbService);
                    string deviceAddress = symbolicName.Substring(symbolicName.IndexOf(":") + 1);

                    UsbDevice usbDevice = GetUsbDevice(usbManager, deviceAddress);
                    if (usbDevice != null) {
                        if (!usbManager.HasPermission(usbDevice)) {
                            PendingIntent permissionIntent = PendingIntent.GetBroadcast(Android.App.Application.Context, 0, new Intent(ActionUsbPermission), 0);
                            usbManager.RequestPermission(usbDevice, permissionIntent);

                            Stopwatch stopwatch = new Stopwatch();
                            stopwatch.Start();

                            do {
                                Thread.Sleep(10);
                                if (stopwatch.ElapsedMilliseconds > UsbPermissionTimeout) {
                                    throw new ConnectionException("Timed out waiting for USB permission.");
                                }
                            } while (UsbReceiver.Result != Result.Ok);

                            if (!UsbReceiver.HasPermission) {
                                throw new ConnectionException("USB permission denied.");
                            }
                        }

                        return new UsbConnection(usbManager, usbDevice);
                    } else {
                        throw new ConnectionException($"USB device '{deviceAddress}' was not found.");
                    }
                } finally {
                    UsbReceiver.Reset();
                }
            }
        }

        public void GetZebraUsbDirectPrinters(DiscoveryHandler discoveryHandler) {
            UsbDiscoverer.FindPrinters(Android.App.Application.Context, discoveryHandler);
        }

        public List<DiscoveredPrinter> GetZebraUsbDriverPrinters() {
            throw new NotImplementedException();
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

        [BroadcastReceiver]
        [IntentFilter(new[] { ActionUsbPermission })]
        public class UsbReceiver : BroadcastReceiver {
            public UsbReceiver() {
                Reset();
            }

            public static bool HasPermission { get; private set; } = false;

            public static Result Result { get; private set; } = Result.Canceled;

            public static void Reset() {
                HasPermission = false;
                Result = Result.Canceled;
            }

            public override void OnReceive(Context context, Intent intent) {
                if (ActionUsbPermission.Equals(intent.Action)) {
                    UsbDevice device = (UsbDevice)intent.GetParcelableExtra(UsbManager.ExtraDevice);
                    if (intent.GetBooleanExtra(UsbManager.ExtraPermissionGranted, false)) {
                        if (device != null) {
                            HasPermission = true;
                        }
                    }
                    Result = Result.Ok;
                }
            }
        }
    }
}