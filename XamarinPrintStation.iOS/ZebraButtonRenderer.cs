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
using System.ComponentModel;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using XamarinPrintStation.iOS;

[assembly: ExportRenderer(typeof(Button), typeof(ZebraButtonRenderer))]
namespace XamarinPrintStation.iOS {
    public class ZebraButtonRenderer : ButtonRenderer {
        protected override void OnElementChanged(ElementChangedEventArgs<Button> e) {
            base.OnElementChanged(e);

            if (Control != null) {
                SetBackgroundColor();

                Control.SetTitleColor(Color.White.ToUIColor(), UIControlState.Normal);
                Control.SetTitleColor(Color.White.ToUIColor(), UIControlState.Disabled);
                Control.Layer.CornerRadius = 0;
                Control.TitleLabel.Font = UIFont.BoldSystemFontOfSize(UIFont.SystemFontSize);
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e) {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == nameof(Button.IsEnabled) && Control != null) {
                SetBackgroundColor();
            }
        }

        private void SetBackgroundColor() {
            Control.BackgroundColor = Element != null && Element.IsEnabled ? ZebraColor.ZebraBlue.ToUIColor() : ZebraColor.ZebraBlueDisabled.ToUIColor(); ;
        }
    }
}