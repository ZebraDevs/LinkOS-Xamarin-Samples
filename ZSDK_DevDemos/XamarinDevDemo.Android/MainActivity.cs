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

using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace XamarinDevDemo.Droid {

    [Activity(Label = "XamarinDevDemo", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : FormsAppCompatActivity {

        public const int AccessCoarseLocationPermissionRequestCode = 0;

        protected override void OnCreate(Bundle bundle) {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            Forms.Init(this, bundle);
            LoadApplication(new App());

            GetAccessCoarseLocationPermission();
        }

        private void GetAccessCoarseLocationPermission() {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) == Permission.Granted) {
                return;
            }

            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.AccessCoarseLocation)) {
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.SetTitle("Permission Required")
                    .SetMessage("Zebra Developer Demos requires permission to access your location in order to perform Bluetooth discovery. Please accept this permission to allow Bluetooth discovery to function properly.")
                    .SetPositiveButton("OK", OnPermissionRequiredDialogOkClicked)
                    .SetCancelable(false)
                    .Show();

                return;
            }

            RequestAccessCoarseLocationPermission();
        }

        private void OnPermissionRequiredDialogOkClicked(object sender, DialogClickEventArgs e) {
            RequestAccessCoarseLocationPermission();
        }

        private void RequestAccessCoarseLocationPermission() {
            ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.AccessCoarseLocation }, AccessCoarseLocationPermissionRequestCode);
        }
    }
}

