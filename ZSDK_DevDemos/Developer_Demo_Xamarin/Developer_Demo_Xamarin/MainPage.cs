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

using Xamarin.Forms;

namespace Xamarin_LinkOS_Developer_Demo {

    public class MainPage : NavigationPage {

        public SelectPrinterView SelectPrinterPage {
            get; private set;
        }

        public MainPage() {
            SelectPrinterPage = new SelectPrinterView(this);
            PushAsync(new TabbedDemoPage(this));
        }

        public void NavigateToSelectPrinterPage() {
            PushAsync(SelectPrinterPage);
            SelectPrinterPage.StartPrinterDiscovery();
        }
    }
}
