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

/*********************************************************************************************************
File:   PrinterDiscoveryImplementation.cs

Descr:  Class to access OS specific methods to run USB and Bluetooth printer discovery. 

Date: 03/8/16 
Updated:
**********************************************************************************************************/
using Android;
using Android.App;
using Android.Bluetooth;
using Android.Content.PM;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using LinkOS.Plugin;
using LinkOS.Plugin.Abstractions;
using Xamarin_LinkOS_Developer_Demo.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(PrinterDiscoveryImplementation))]
namespace Xamarin_LinkOS_Developer_Demo.Droid {

    public class PrinterDiscoveryImplementation : IPrinterDiscovery {

        public readonly string[] PermissionsLocation = { Manifest.Permission.AccessCoarseLocation };
        public const int RequestLocationId = 0;

        public static IDiscoveryHandler TempHandler { get; set; }

        public PrinterDiscoveryImplementation() { }

        public void CancelDiscovery() {
            if (BluetoothAdapter.DefaultAdapter.IsDiscovering) {
                BluetoothAdapter.DefaultAdapter.CancelDiscovery();
                System.Diagnostics.Debug.WriteLine("Cancelling discovery...");
            }
        }

        public void FindBluetoothPrinters(IDiscoveryHandler handler) {
            const string permission = Manifest.Permission.AccessCoarseLocation;
            if (ContextCompat.CheckSelfPermission(Application.Context, permission) == (int)Permission.Granted) {
                BluetoothDiscoverer.Current.FindPrinters(Application.Context, handler);
                return;
            }

            TempHandler = handler;
            // Finally request permissions with the list of permissions and ID
            ActivityCompat.RequestPermissions(MainActivity.GetActivity(), PermissionsLocation, RequestLocationId);
        }

        public void FindUsbPrinters(IDiscoveryHandler handler) {
            UsbDiscoverer.Current.FindPrinters(Application.Context, handler);
        }

        public void RequestUsbPermission(IDiscoveredPrinterUsb printer) {
            if (!printer.HasPermissionToCommunicate) {
                printer.RequestPermission(Application.Context);
            }
        }
    }
}