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
File:   IPrinterDiscovery.cs

Descr:  Interface to access OS specific methods to run USB and Bluetooth printer discovery. 

Date: 03/8/16 
Updated:
**********************************************************************************************************/
using LinkOS.Plugin.Abstractions;

namespace Xamarin_LinkOS_Developer_Demo
{
    public interface IPrinterDiscovery
    {
        void FindBluetoothPrinters(IDiscoveryHandler handler);
        void FindUSBPrinters(IDiscoveryHandler handler);
        void RequestUSBPermission(IDiscoveredPrinterUsb printer);
        void CancelDiscovery();
    }
}
