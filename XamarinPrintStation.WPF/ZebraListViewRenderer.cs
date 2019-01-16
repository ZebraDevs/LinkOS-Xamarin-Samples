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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Xamarin.Forms.Platform.WPF;
using XamarinPrintStation.WPF;

[assembly: ExportRenderer(typeof(Xamarin.Forms.ListView), typeof(ZebraListViewRenderer))]
namespace XamarinPrintStation.WPF {
    public class ZebraListViewRenderer : ListViewRenderer {
        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.ListView> e) {
            base.OnElementChanged(e);

            if (Control != null) {
                Control.ItemContainerStyle = new Style(typeof(ListViewItem), Control.ItemContainerStyle) {
                    Setters = {
                        new Setter(UIElement.FocusableProperty, e.NewElement.SelectionMode != Xamarin.Forms.ListViewSelectionMode.None)
                    }
                };
            }
        }
    }
}
