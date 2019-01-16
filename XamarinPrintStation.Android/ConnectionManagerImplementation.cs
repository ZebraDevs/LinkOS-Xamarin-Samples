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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Hardware.Usb;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using XamarinPrintStation.Droid;
using Zebra.Sdk.Comm;
using Zebra.Sdk.Printer.Discovery;

[assembly: Dependency(typeof(ConnectionManagerImplementation))]
namespace XamarinPrintStation.Droid {
    public class ConnectionManagerImplementation : IConnectionManager {

        private const string ActionUsbPermission = "com.zebra.XamarinPrintStation.USB_PERMISSION";
        private const int UsbPermissionTimeout = 30000;

        private static readonly object UsbConnectionLock = new object();

        private readonly IntentFilter filter = new IntentFilter("ACTION_USB_PERMISSION");

        public void FindBluetoothPrinters(DiscoveryHandler discoveryHandler) {
            BluetoothDiscoverer.FindPrinters(Android.App.Application.Context, discoveryHandler);
        }

        public void FindUrlPrinters(string nfcData, DiscoveryHandler discoveryHandler) {
            UrlPrinterDiscoverer.FindPrinters(nfcData, discoveryHandler, Android.App.Application.Context);
        }

        public Connection GetBluetoothConnection(string macAddress) {
            return new BluetoothConnection(macAddress);
        }

        private UsbDevice GetUsbDevice(UsbManager usbManager, string deviceAddress) {
            IDictionary<string, UsbDevice> deviceList = usbManager.DeviceList;
            return deviceList != null && deviceList.ContainsKey(deviceAddress) ? deviceList[deviceAddress] : null;
        }

        public void GetZebraUsbDirectPrinters(DiscoveryHandler discoveryHandler) {
            UsbDiscoverer.FindPrinters(Android.App.Application.Context, discoveryHandler);
        }

        public List<DiscoveredPrinter> GetZebraUsbDriverPrinters() {
            throw new NotImplementedException();
        }

        public bool IsBluetoothPrinter(DiscoveredPrinter printer) {
            return printer is DiscoveredPrinterBluetooth;
        }

        public Connection GetUsbDirectConnection(string symbolicName) {
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

        public Connection GetUsbDriverConnection(string printerName) {
            throw new NotImplementedException();
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