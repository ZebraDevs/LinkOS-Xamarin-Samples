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
File:   ReceiptDemoView.cs

Descr:  A ZPL receipt printing demo. Shows how to put together a dynamic, variable length printout page 
        in ZPL.

Date: 03/8/16 
Updated:
**********************************************************************************************************/
using LinkOS.Plugin;
using LinkOS.Plugin.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Xamarin_LinkOS_Developer_Demo {

    public class ReceiptDemoView : BaseDemoView {

        Button printBtn;

        public ReceiptDemoView(TabbedDemoPage tabbedDemoPage) : base(tabbedDemoPage) {
            Label title = new Label { Text = "Receipt Demo" };
            printBtn = new Button { Text = "Print" };
            printBtn.Clicked += PrintBtn_Clicked;

            Children.Add(title);
            Children.Add(printBtn);
        }

        private void PrintBtn_Clicked(object sender, EventArgs e) {
            if (CheckPrinter()) {
                printBtn.IsEnabled = false;

                new Task(new Action(() => {
                    PrintLineMode();
                })).Start();
            }
        }

        private void PrintLineMode() {
            IConnection connection = null;
            try {
                connection = myPrinter.Connection;
                connection.Open();
                IZebraPrinter printer = ZebraPrinterFactory.Current.GetInstance(connection);
                if ((!CheckPrinterLanguage(connection)) || (!PreCheckPrinterStatus(printer))) {
                    ResetPage();
                    return;
                }

                SendZplReceipt(connection);

                if (PostPrintCheckStatus(printer)) {
                    //ShowAlert("Receipt printed.");
                }
            } catch (Exception ex) {
                // Connection Exceptions and issues are caught here
                ShowErrorAlert(ex.Message);
            } finally {
                if ((connection != null) && (connection.IsConnected))
                    connection.Close();
                ResetPage();
            }
        }

        private void ResetPage() {
            Device.BeginInvokeOnMainThread(() => {
                printBtn.IsEnabled = true;
            });
        }

        private void SendZplReceipt(IConnection printerConnection) {
            /*
             This routine is provided to you as an example of how to create a variable length label with user specified data.
             The basic flow of the example is as follows

                Header of the label with some variable data
                Body of the label
                    Loops thru user content and creates small line items of printed material
                Footer of the label

             As you can see, there are some variables that the user provides in the header, body and footer, and this routine uses that to build up a proper ZPL string for printing.
             Using this same concept, you can create one label for your receipt header, one for the body and one for the footer. The body receipt will be duplicated as many items as there are in your variable data

             */

            string tmpHeader =
                    /*
                     Some basics of ZPL. Find more information here : http://www.zebra.com

                     ^XA indicates the beginning of a label
                     ^PW sets the width of the label (in dots)
                     ^MNN sets the printer in continuous mode (variable length receipts only make sense with variably sized labels)
                     ^LL sets the length of the label (we calculate this value at the end of the routine)
                     ^LH sets the reference axis for printing. 
                        You will notice we change this positioning of the 'Y' axis (length) as we build up the label. Once the positioning is changed, all new fields drawn on the label are rendered as if '0' is the new home position
                     ^FO sets the origin of the field relative to Label Home ^LH
                     ^A sets font information 
                     ^FD is a field description
                     ^GB is graphic boxes (or lines)
                     ^B sets barcode information
                     ^XZ indicates the end of a label
                     */

                    "^XA" +
                    "^POI^PW400^MNN^LL325^LH0,0" + "\r\n" +
                    "^FO50,50" + "\r\n" + "^A0,N,70,70" + "\r\n" + "^FD Shipping^FS" + "\r\n" +
                    "^FO50,130" + "\r\n" + "^A0,N,35,35" + "\r\n" + "^FDPurchase Confirmation^FS" + "\r\n" +
                    "^FO50,180" + "\r\n" + "^A0,N,25,25" + "\r\n" + "^FDCustomer:^FS" + "\r\n" +
                    "^FO225,180" + "\r\n" + "^A0,N,25,25" + "\r\n" + "^FDAcme Industries^FS" + "\r\n" +
                    "^FO50,220" + "\r\n" + "^A0,N,25,25" + "\r\n" + "^FDDelivery Date:^FS" + "\r\n" +
                    "^FO225,220" + "\r\n" + "^A0,N,25,25" + "\r\n" + "^FD{0}^FS" + "\r\n" +
                    "^FO50,273" + "\r\n" + "^A0,N,30,30" + "\r\n" + "^FDItem^FS" + "\r\n" +
                    "^FO280,273" + "\r\n" + "^A0,N,25,25" + "\r\n" + "^FDPrice^FS" + "\r\n" +
                    "^FO50,300" + "\r\n" + "^GB350,5,5,B,0^FS" + "^XZ";

            int headerHeight = 325;

            string sdf = "yyyy/MM/dd";
            string dateString = new DateTime().ToString(sdf);
            string header = string.Format(tmpHeader, dateString);

            printerConnection.Write(GetBytes(header));

            int heightOfOneLine = 40;
            double totalPrice = 0;

            Dictionary<string, string> itemsToPrint = CreateListOfItems();
            foreach (string productName in itemsToPrint.Keys) {
                itemsToPrint.TryGetValue(productName, out string price);

                string lineItem = "^XA^POI^LL40" + "^FO50,10" + "\r\n" + "^A0,N,28,28" + "\r\n" + "^FD{0}^FS" + "\r\n" + "^FO280,10" + "\r\n" + "^A0,N,28,28" + "\r\n" + "^FD${1}^FS" + "^XZ";
                double.TryParse(price, out double tempPrice);
                totalPrice += tempPrice;
                string oneLineLabel = string.Format(lineItem, productName, price);

                printerConnection.Write(GetBytes(oneLineLabel));
            }

            long totalBodyHeight = (itemsToPrint.Count + 1) * heightOfOneLine;
            long footerStartPosition = headerHeight + totalBodyHeight;
            string tPrice = Convert.ToString(Math.Round((totalPrice), 2));

            string footer = string.Format("^XA^POI^LL600" + "\r\n" + 
                "^FO50,1" + "\r\n" + "^GB350,5,5,B,0^FS" + "\r\n" +
                "^FO50,15" + "\r\n" + "^A0,N,40,40" + "\r\n" + "^FDTotal^FS" + "\r\n" +
                "^FO175,15" + "\r\n" + "^A0,N,40,40" + "\r\n" + "^FD${0}^FS" + "\r\n" +
                "^FO50,130" + "\r\n" + "^A0,N,45,45" + "\r\n" + "^FDPlease Sign Below^FS" + "\r\n" +
                "^FO50,190" + "\r\n" + "^GB350,200,2,B^FS" + "\r\n" +
                "^FO50,400" + "\r\n" + "^GB350,5,5,B,0^FS" + "\r\n" +
                "^FO50,420" + "\r\n" + "^A0,N,30,30" + "\r\n" + "^FDThanks for choosing us!^FS" + "\r\n" +
                "^FO50,470" + "\r\n" + "^B3N,N,45,Y,N" + "\r\n" + "^FD0123456^FS" + "\r\n" + "^XZ", tPrice);

            printerConnection.Write(GetBytes(footer));
        }

        private Dictionary<string, string> CreateListOfItems() {
            string[] names = { "Microwave Oven", "Sneakers (Size 7)", "XL T-Shirt", "Socks (3-pack)", "Blender", "DVD Movie" };
            string[] prices = { "79.99", "69.99", "39.99", "12.99", "34.99", "16.99" };

            Dictionary<string, string> retVal = new Dictionary<string, string>();

            for (int ix = 0; ix < names.Length; ix++) {
                retVal.Add(names[ix], prices[ix]);
            }
            return retVal;
        }
    }
}