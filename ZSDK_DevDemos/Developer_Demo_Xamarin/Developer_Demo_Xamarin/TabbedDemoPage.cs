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

/*********************************************************************************************************
File:   TabbedDemoPage.cs

Descr:  Main page that controls navigation between the 3 different demos.  

Date: 03/8/16 
Updated:
**********************************************************************************************************/
using System;
using Xamarin.Forms;

namespace Xamarin_LinkOS_Developer_Demo {

    public class TabbedDemoPage : TabbedPage {

        public MainPage MainPage {
            get; private set;
        }

        public TabbedDemoPage(MainPage mainPage) {
            MainPage = mainPage;

            Title = "Xamarin Developer Demos";

            Children.Add(new ContentPage {
                Title = "Receipt Demo",
                Content = new ReceiptDemoView(this)
            });

            Children.Add(new ContentPage {
                Title = "Status Demo",
                Content = new StatusDemoView(this)
            });

            Children.Add(new FormatDemoCarousel(this));
        }
    }
}
