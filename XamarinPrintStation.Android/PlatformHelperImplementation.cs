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

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using XamarinPrintStation.Droid;

[assembly: Dependency(typeof(PlatformHelperImplementation))]
namespace XamarinPrintStation.Droid {
    public class PlatformHelperImplementation : IPlatformHelper {

        public string GetIOSBundleIdentifier() {
            throw new NotImplementedException();
        }

        public bool IsWindows10() {
            return false;
        }
    }
}