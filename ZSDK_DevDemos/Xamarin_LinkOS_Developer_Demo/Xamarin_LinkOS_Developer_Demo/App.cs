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
File:   App.cs

Descr:  Main application file. 

Date: 03/8/16 
Updated:
**********************************************************************************************************/
using Xamarin.Forms;

namespace Xamarin_LinkOS_Developer_Demo
{
    public class App : Application
    {
        public const string API_Version = "1.1.75";
        public const string APP_Version = "1.0.19";
        public const string GIT_API_HASH = "2e2d138ef76c5aa1c95195617734b97aa8e9a43e";
        public const string GIT_APP_HASH = "6a6eda468ca950a89e7b428f5042b13624156a93";

        public App()
		{
			MainPage = new MainNavigation ();
		}

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
