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
using System.Threading.Tasks;
using System.Windows;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WPF;
using XamarinPrintStation.WPF;

[assembly: ExportRenderer(typeof(Button), typeof(ZebraButtonRenderer))]
namespace XamarinPrintStation.WPF {
    public class ZebraButtonRenderer : ButtonRenderer {
        protected override void OnElementChanged(ElementChangedEventArgs<Button> e) {
            base.OnElementChanged(e);

            if (Control != null) {
                SetBackgroundColor();

                Control.Foreground = Color.White.ToBrush();
                Control.BorderThickness = new System.Windows.Thickness(0);
                Control.Padding = new System.Windows.Thickness(15, 10, 15, 10);
                Control.FontWeight = FontWeights.Bold;
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e) {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == nameof(Button.IsEnabled) && Control != null) {
                SetBackgroundColor();
            }
        }

        private void SetBackgroundColor() {
            Control.Background = Element != null && Element.IsEnabled ? ZebraColor.ZebraBlue.ToBrush() : ZebraColor.ZebraBlueDisabled.ToBrush();
        }
    }
}
