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
using System;
using Xamarin_LinkOS_Developer_Demo.iOS;
using LinkOS.Plugin;
using LinkOS.Plugin.Abstractions;

[assembly: Xamarin.Forms.Dependency(typeof(PrinterDiscoveryImplementation))]
namespace Xamarin_LinkOS_Developer_Demo.iOS
{
    public class PrinterDiscoveryImplementation : IPrinterDiscovery
    {
        public PrinterDiscoveryImplementation() { }

        public void CancelDiscovery()
        {
        }

        public void FindBluetoothPrinters(IDiscoveryHandler handler)
        {
            BluetoothDiscoverer.Current.FindPrinters(null, handler);
        }

        public void FindUSBPrinters(IDiscoveryHandler handler)
        {
            throw new NotImplementedException();
        }

        public void RequestUSBPermission(IDiscoveredPrinterUsb printer)
        {
            throw new NotImplementedException();
        }
    }
}
