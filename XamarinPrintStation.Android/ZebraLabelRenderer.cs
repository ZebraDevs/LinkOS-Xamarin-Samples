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
using Xamarin.Forms.Platform.Android;
using XamarinPrintStation.Droid;
using static Android.Text.TextUtils;

[assembly: ExportRenderer(typeof(Label), typeof(ZebraLabelRenderer))]
namespace XamarinPrintStation.Droid {
    public class ZebraLabelRenderer : LabelRenderer {
        public ZebraLabelRenderer(Context context) : base(context) {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Label> e) {
            base.OnElementChanged(e);

            if (Control != null) {
                if (LineBreakMode.WordWrap.Equals(Element.GetValue(Label.LineBreakModeProperty))) {
                    Control.SetSingleLine(false); // Workaround for Xamarin.Forms Android bug in prerelease 3.3.0.840541-pre1 package: https://forums.xamarin.com/discussion/139984/why-are-my-labels-truncating-my-text-at-n
                }
            }
        }
    }
}