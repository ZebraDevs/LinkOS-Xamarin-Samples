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
using Android.Nfc;
using Android.Nfc.Tech;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using XamarinPrintStation.Droid;
using Zebra.Sdk.Printer.Discovery;

[assembly: Dependency(typeof(NfcManagerImplementation))]
namespace XamarinPrintStation.Droid {
    public class NfcManagerImplementation : INfcManager {

        public event EventHandler<string> TagScanned;

        public void OnNewIntent(object sender, Intent e) {
            IParcelable[] tags = e.GetParcelableArrayExtra(NfcAdapter.ExtraNdefMessages);
            if (tags?.Length > 0) {
                NdefMessage message = (NdefMessage)tags[0];
                string nfcData = Encoding.UTF8.GetString(message.GetRecords()[0].GetPayload());

                TagScanned?.Invoke(this, nfcData);
            }
        }

        public bool IsNfcAvailable() {
            try {
                return ((NfcManager)Android.App.Application.Context.GetSystemService(Context.NfcService)).DefaultAdapter.IsEnabled;
            } catch (Exception) {
                return false;
            }
        }
    }
}