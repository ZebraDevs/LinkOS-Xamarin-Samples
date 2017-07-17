using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Java.Lang;
using Java.Lang.Reflect;
using Java.Util;

namespace Xamarin_LinkOS_Developer_Demo.Droid
{
    [Activity(Label = "Xamarin_LinkOS_Developer_Demo", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        private static Activity myActivity;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            myActivity = this;
            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }

        public static Activity GetActivity()
        {
            return MainActivity.myActivity;
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            switch (requestCode)
            {
                case PrinterDiscoveryImplementation.RequestLocationId:
                    {
                        if (grantResults[0] == Permission.Granted)
                        {
                            //Permission granted
                            PrinterDiscoveryImplementation discoveryImp = new PrinterDiscoveryImplementation();
                            discoveryImp.FindBluetoothPrinters(PrinterDiscoveryImplementation.TempHandler);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Location Permission Denied.  Cannot do Bluetooth Discovery");
                        }
                    }
                    break;
            }
        }
    }
}

