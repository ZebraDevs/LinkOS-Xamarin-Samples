/********************************************************************************************************** 
 * CONFIDENTIAL AND PROPRIETARY 
 *
 * The source code and other information contained herein is the confidential and the exclusive property of
 * ZIH Corporation and is subject to the terms and conditions in your end user license agreement.
 * This source code, and any other information contained herein, shall not be copied, reproduced, published, 
 * displayed or distributed, in whole or in part, in any medium, by any means, for any purpose except as
 * expressly permitted under such license agreement.
 * 
 * Copyright ZIH Corporation 2016
 *
 * ALL RIGHTS RESERVED 
 *********************************************************************************************************/

/*********************************************************************************************************
File:   PrinterDiscoveryImplementation.cs

Descr:  Class to access OS specific methods to run USB and Bluetooth printer discovery. 

Date: 03/8/16 
Updated:
**********************************************************************************************************/
using Xamarin_LinkOS_Developer_Demo.Droid;
using LinkOS.Plugin;
using LinkOS.Plugin.Abstractions;
using Android.Bluetooth;
using System;
using Android;
using Android.Content.PM;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.Design.Widget;

[assembly: Xamarin.Forms.Dependency(typeof(PrinterDiscoveryImplementation))]
namespace Xamarin_LinkOS_Developer_Demo.Droid
{
    public class PrinterDiscoveryImplementation : IPrinterDiscovery
    {
        public PrinterDiscoveryImplementation() { }

        public void CancelDiscovery()
        {
            if (BluetoothAdapter.DefaultAdapter.IsDiscovering)
            {
                BluetoothAdapter.DefaultAdapter.CancelDiscovery();
                System.Diagnostics.Debug.WriteLine("Cancelling Discovery");
            }
        }

        public void FindBluetoothPrinters(IDiscoveryHandler handler)
        {
            const string permission = Manifest.Permission.AccessCoarseLocation;
            if (ContextCompat.CheckSelfPermission(Xamarin.Forms.Forms.Context, permission) == (int)Permission.Granted)
            {
                BluetoothDiscoverer.Current.FindPrinters(Xamarin.Forms.Forms.Context, handler);
                return;
            }
            TempHandler = handler;
            //Finally request permissions with the list of permissions and Id
            ActivityCompat.RequestPermissions(MainActivity.GetActivity(), PermissionsLocation, RequestLocationId);
        }
        public static IDiscoveryHandler TempHandler { get; set; }

        public readonly string[] PermissionsLocation =
        {
          Manifest.Permission.AccessCoarseLocation
        };
        public const int RequestLocationId = 0;

        

        public void FindUSBPrinters(IDiscoveryHandler handler)
        {
            UsbDiscoverer.Current.FindPrinters(Xamarin.Forms.Forms.Context, handler);
        }

        public void RequestUSBPermission(IDiscoveredPrinterUsb printer)
        {
            if (!printer.HasPermissionToCommunicate)
            {
                printer.RequestPermission(Xamarin.Forms.Forms.Context);
            }
        }
    }
}