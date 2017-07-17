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
File:   TabbedDemoPage.cs

Descr:  Main page that controls navigation between the 3 different demos.  

Date: 03/8/16 
Updated:
**********************************************************************************************************/
using System;
using Xamarin.Forms;

namespace Xamarin_LinkOS_Developer_Demo
{
    public class TabbedDemoPage : TabbedPage
    {
        public TabbedDemoPage()
        {
            this.Title = "Xamarin Developer Demos";
            this.Children.Add(new ContentPage
            {
                Title = "Receipt Demo",
                Content = new ReceiptDemoView()
            });
            this.Children.Add(new ContentPage
            {
                Title = "Status Demo",
                Content = new StatusDemoView()
            });
            this.Children.Add(new FormatDemoCarousel());
            BaseDemoView.OnErrorAlert += BaseDemoView_OnErrorAlert;
            BaseDemoView.OnAboutChosen += BaseDemoView_OnAboutChosen;
            BaseDemoView.OnAlert += BaseDemoView_OnAlert;
        }

        private void BaseDemoView_OnAboutChosen()
        {
            string message = "Developer Demo "  + App.APP_Version + " {" + App.GIT_APP_HASH + "}" + Environment.NewLine 
                + "Using SDK " + App.API_Version +" {" + App.GIT_API_HASH +"}" + Environment.NewLine + "Copyright Zebra Technologies 2016";
            Device.BeginInvokeOnMainThread(() =>
            {
                DisplayAlert("About", message, "OK");
            });
        }

        private void BaseDemoView_OnErrorAlert(string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DisplayAlert("Error", message, "OK");
            });
        }
        private void BaseDemoView_OnAlert(string message, string title)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DisplayAlert(title, message, "OK");
            });
        }
    }
}
