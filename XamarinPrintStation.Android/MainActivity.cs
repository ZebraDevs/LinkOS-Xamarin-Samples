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

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Nfc;
using Android.Content;
using Android.Support.V4.Content;
using Android;
using Android.Support.V4.App;
using Android.Support.Design.Widget;

namespace XamarinPrintStation.Droid {
    [Activity(Label = "Print Station", Icon = "@mipmap/ic_launcher", RoundIcon = "@mipmap/ic_launcher_round", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait),
        IntentFilter(new[] { "android.nfc.action.NDEF_DISCOVERED" }, DataHost = "zebra.com", DataPath = "/apps/r/nfc", DataScheme = "http", Categories = new[] { "android.intent.category.DEFAULT" }),
        IntentFilter(new[] { "android.nfc.action.NDEF_DISCOVERED" }, DataHost = "www.zebra.com", DataPath = "/apps/r/nfc", DataScheme = "http", Categories = new[] { "android.intent.category.DEFAULT" })]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity {

        public const int AccessCoarseLocationPermissionRequestCode = 0;

        public NfcAdapter nfcAdapter = ((NfcManager)Application.Context.GetSystemService(NfcService)).DefaultAdapter;
        public NfcManagerImplementation nfcManagerImplementation;
        bool isNfcAvailable = false;

        protected override void OnCreate(Bundle bundle) {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            Xamarin.Forms.Forms.Init(this, bundle);
            Xamarin.Forms.DependencyService.Register<INfcManager, NfcManagerImplementation>();
            nfcManagerImplementation = Xamarin.Forms.DependencyService.Get<INfcManager>() as NfcManagerImplementation;
            isNfcAvailable = nfcManagerImplementation.IsNfcAvailable();

            LoadApplication(new App());

            GetAccessCoarseLocationPermission();
        }

        protected override void OnResume() {
            base.OnResume();

            if (isNfcAvailable) {
                if (NfcAdapter.ActionNdefDiscovered.Equals(Intent.Action)) {
                    nfcManagerImplementation.OnNewIntent(this, Intent);
                }

                if (nfcAdapter != null) {
                    Intent intent = new Intent(this, GetType()).AddFlags(ActivityFlags.SingleTop);
                    PendingIntent pendingIntent = PendingIntent.GetActivity(this, 0, intent, 0);

                    nfcAdapter.EnableForegroundDispatch(this, pendingIntent, null, null);
                }
            }
        }

        protected override void OnPause() {
            base.OnPause();

            if (isNfcAvailable) {
                nfcAdapter.DisableForegroundDispatch(this);
            }
        }

        protected override void OnNewIntent(Intent intent) {
            base.OnNewIntent(intent);

            if (isNfcAvailable) {
                nfcManagerImplementation.OnNewIntent(this, intent);
            }
        }

        private void GetAccessCoarseLocationPermission() {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) == Permission.Granted) {
                return;
            }

            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.AccessCoarseLocation)) {
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.SetTitle("Permission Required")
                    .SetMessage("Print Station requires permission to access your location in order to perform Bluetooth discovery. Please accept this permission to allow Bluetooth discovery to function properly.")
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

