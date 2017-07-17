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
File:   MainNavigation.cs

Descr:  Top level navigation page to handle switching between the demos and the "Select Printer" page.

Date: 03/8/16 
Updated:
**********************************************************************************************************/
using Xamarin.Forms;

namespace Xamarin_LinkOS_Developer_Demo
{
	public class MainNavigation : NavigationPage
	{
		Page tabbedDemoPage;

		public MainNavigation ()
		{
			tabbedDemoPage = new TabbedDemoPage();
			BaseDemoView.OnChoosePrinterChosen += BaseDemoView_OnChoosePrinterChosen;
			PushAsync (tabbedDemoPage);
		}
		private void BaseDemoView_OnChoosePrinterChosen()
		{
			SelectPrinterView selectPrinterView = new SelectPrinterView();
			((SelectPrinterView)selectPrinterView).OnBackToMainPage += App_OnBackToMainPage;
			SelectPrinterView.OnPrinterSelected += SelectPrinterView_OnPrinterSelected;
			PushAsync (selectPrinterView);
		}

		private void SelectPrinterView_OnPrinterSelected(LinkOS.Plugin.Abstractions.IDiscoveredPrinter printer)
		{
            SelectPrinterView.OnPrinterSelected -= SelectPrinterView_OnPrinterSelected;
            PopAsync();
		}

		private void App_OnBackToMainPage()
		{
            SelectPrinterView.OnPrinterSelected -= SelectPrinterView_OnPrinterSelected;
            PopAsync ();
		}
	}
}


