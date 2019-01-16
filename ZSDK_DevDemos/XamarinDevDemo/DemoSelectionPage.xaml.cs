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
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XamarinDevDemo {

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DemoSelectionPage : ContentPage {

        public DemoSelectionPage() {
            InitializeComponent();
        }

        private async Task PushPageAsync(Page page) {
            if (Navigation.NavigationStack.Count == 0 || Navigation.NavigationStack.Last().GetType() == typeof(DemoSelectionPage)) {
                await Navigation.PushAsync(page);
            }
        }

        private async void ConnectivityDemoButton_Clicked(object sender, EventArgs e) {
            await PushPageAsync(new ConnectivityDemoPage());
        }

        private async void DiscoveryDemoButton_Clicked(object sender, EventArgs e) {
            await PushPageAsync(new DiscoveryDemoPage());
        }

        private async void MultichannelDemoButton_Clicked(object sender, EventArgs e) {
            await PushPageAsync(new MultichannelDemoPage());
        }

        private async void PrinterStatusDemoButton_Clicked(object sender, EventArgs e) {
            await PushPageAsync(new PrinterStatusDemoPage());
        }

        private async void ProfileDemoButton_Clicked(object sender, EventArgs e) {
            await PushPageAsync(new ProfileDemoPage());
        }

        private async void SendFileDemoButton_Clicked(object sender, EventArgs e) {
            await PushPageAsync(new SendFileDemoPage());
        }

        private async void SettingsDemoButton_Clicked(object sender, EventArgs e) {
            await PushPageAsync(new SettingsDemoPage());
        }

        private async void SignatureCaptureDemoButton_Clicked(object sender, EventArgs e) {
            await PushPageAsync(new SignatureCaptureDemoPage());
        }

        private async void StatusChannelDemoButton_Clicked(object sender, EventArgs e) {
            await PushPageAsync(new StatusChannelDemoPage());
        }
    }
}