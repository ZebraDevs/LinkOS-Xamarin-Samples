/********************************************************************************************************** 
 * CONFIDENTIAL AND PROPRIETARY 
 *
 * The source code and other information contained herein is the confidential and the exclusive property of
 * ZIH Corporation and is subject to the terms and conditions in your end user license agreement.
 * This source code, and any other information contained herein, shall not be copied, reproduced, published, 
 * displayed or distributed, in whole or in part, in any medium, by any means, for any purpose except as
 * expressly permitted under such license agreement.
 * 
 * Copyright ZIH Corporation 2016
 *
 * ALL RIGHTS RESERVED 
 *********************************************************************************************************/

/*********************************************************************************************************
File:   StatusDemoView.cs

Descr:  Status demo view that shows how to get and show current printer status at any time.  

Date: 03/8/16 
Updated:
**********************************************************************************************************/
using LinkOS.Plugin;
using LinkOS.Plugin.Abstractions;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Xamarin_LinkOS_Developer_Demo
{
    public class StatusDemoView : BaseDemoView
    {
        Label printerStatusLbl;
        Label causesLbl;
        Button refreshBtn;

        public StatusDemoView() :base()
        {
            printerStatusLbl = new Label { Text = "Printer Status:" };
            causesLbl = new Label { Text = "" };
            refreshBtn = new Button { Text = "Check Status" };
            refreshBtn.Clicked += RefreshBtn_Clicked;
            refreshBtn.IsEnabled = true; 

            Children.Add(refreshBtn);
            Children.Add(printerStatusLbl);
            Children.Add(causesLbl);
        }

        private void CheckStatus()
        {
            new Task(new Action(() => {
                GetPrinterStatus();
            })).Start();
        }

        private void GetPrinterStatus()
        {
            //CheckPrinter();
            IConnection connection = null;
            try
            {
                connection = myPrinter.Connection;
                connection.Open();
                if (!CheckPrinterLanguage(connection))
                {
                    resetPage();
                    return;
                }
                IZebraPrinter printer = ZebraPrinterFactory.Current.GetInstance(connection);
                IPrinterStatus status = printer.CurrentStatus;
                ShowStatus(status);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception:" + e.Message);
                ShowErrorAlert(e.Message);
            }
            finally
            {
                if ((connection != null) && (connection.IsConnected))
                    connection.Close();
                resetPage();
            }
        }

        private void ShowStatus(IPrinterStatus status)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (status.IsReadyToPrint)
                {

                    printerStatusLbl.Text = "Printer Status: Printer Ready";
                    printerStatusLbl.TextColor = Color.Green;
                    causesLbl.Text = "";
                }
                else
                {
                    printerStatusLbl.Text = "Printer Status: Printer Error";
                    printerStatusLbl.TextColor = Color.Red;
                    causesLbl.Text = status.Status;
                }
            });
        }

        private void ClearStatus()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                printerStatusLbl.Text = "Printer Status: ";
                printerStatusLbl.TextColor = Color.Default;
                causesLbl.Text = "";
            });
        }

        private void RefreshBtn_Clicked(object sender, EventArgs e)
        {
            ClearStatus();
            if (CheckPrinter())
            {
                refreshBtn.IsEnabled = false;
                CheckStatus();
            }
        }
        private void resetPage()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                refreshBtn.IsEnabled = true;
            });
        }
    }
}
