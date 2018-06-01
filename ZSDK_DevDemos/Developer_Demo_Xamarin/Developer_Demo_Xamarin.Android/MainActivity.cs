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
using Android.Content.PM;
using Android.OS;

namespace Xamarin_LinkOS_Developer_Demo.Droid {

    [Activity(Label = "Xamarin_LinkOS_Developer_Demo", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity {

        private static Activity activity;

        protected override void OnCreate(Bundle bundle) {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            activity = this;
            Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }

        internal static Activity GetActivity() {
            return activity;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults) {
            switch (requestCode) {
                case PrinterDiscoveryImplementation.RequestLocationId:
                    if (grantResults[0] == Permission.Granted) {
                        //Permission granted
                        PrinterDiscoveryImplementation discoveryImp = new PrinterDiscoveryImplementation();
                        discoveryImp.FindBluetoothPrinters(PrinterDiscoveryImplementation.TempHandler);
                    } else {
                        System.Diagnostics.Debug.WriteLine("Location permission denied. Cannot do Bluetooth discovery.");
                    }
                    break;
            }
        }
    }
}

