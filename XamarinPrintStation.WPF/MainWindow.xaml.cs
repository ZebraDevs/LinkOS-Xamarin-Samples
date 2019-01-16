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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Networking.Proximity;
using Windows.Storage.Streams;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WPF;

namespace XamarinPrintStation.WPF {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FormsApplicationPage {

        public NfcManagerImplementation nfcManagerImplementation;
        private ProximityDevice proximityDevice;
        private long messageId = -1;

        public MainWindow() {
            InitializeComponent();

            Forms.Init();
            DependencyService.Register<INfcManager, NfcManagerImplementation>();
            nfcManagerImplementation = DependencyService.Get<INfcManager>() as NfcManagerImplementation;

            LoadApplication(new XamarinPrintStation.App());
        }

        protected override void Appearing() {
            base.Appearing();

            try {
                SubscribeForNfcMessage();
            } catch (TypeLoadException) { } // NFC is not supported on Windows 7
        }

        protected override void Disappearing() {
            base.Disappearing();

            try {
                UnsubscribeForNfcMessage();
            } catch (TypeLoadException) { } // NFC is not supported on Windows 7
        }

        private void SubscribeForNfcMessage() {
            proximityDevice = ProximityDevice.GetDefault();
            if (proximityDevice != null) {
                if (messageId == -1) {  // Only subscribe once
                    messageId = proximityDevice.SubscribeForMessage("WindowsUri", (device, message) => {
                        nfcManagerImplementation.OnNfcMessageReceived(device, message);
                    });
                }
            }
        }

        private void UnsubscribeForNfcMessage() {
            if (proximityDevice != null) {
                proximityDevice.StopSubscribingForMessage(messageId);
                messageId = -1;
            }
        }
    }
}
