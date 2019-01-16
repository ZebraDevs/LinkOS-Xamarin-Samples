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

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XamarinPrintStation {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BluetoothHelpPage : ContentPage {
        
        public BluetoothHelpPage() {
            InitializeComponent();
        }

        private async void CloseButton_Clicked(object sender, EventArgs e) {
            if (Navigation.ModalStack.Count > 0 && Navigation.ModalStack.Last().GetType() == typeof(BluetoothHelpPage)) {
                await Navigation.PopModalAsync();
            }
        }
    }
}