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
File:   FormatDemoCarousel.cs

Descr:  Page showing the Format Demo as a two page sliding carousel. 

Date: 03/8/16 
Updated:
**********************************************************************************************************/
using Xamarin.Forms;

namespace Xamarin_LinkOS_Developer_Demo
{
    public class FormatDemoCarousel : CarouselPage
    {
        Format CurrentFormat { get; set; }
        FormatDemoView formatDemoView;
        FormatView formatView;

        public FormatDemoCarousel()
        {
            this.Title = "Format Demo";
            formatDemoView = new FormatDemoView();
            formatView = new FormatView();

            this.Children.Add(new ContentPage
            {
                Title = "Select a Format",
                Content = formatDemoView
            }
            );
            this.Children.Add(new ContentPage
            {
                Title = "Enter Information",
                Content = formatView
            });
            formatDemoView.OnFormatSelected += FormatDemoView_OnFormatSelected;
        }

        private void FormatDemoView_OnFormatSelected(Format fileName)
        {
			Device.BeginInvokeOnMainThread (() => 
			{
					formatView.SetFormat(fileName);
					this.CurrentPage = this.Children[1];
			});
        }
    }
}
