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
File:   FormatDemoView.cs

Descr:  First view for the Format Demo showing a list of availible ZPL print formats from the printer as 
        well as the device. 

Date: 03/8/16 
Updated:
**********************************************************************************************************/
using LinkOS.Plugin;
using LinkOS.Plugin.Abstractions;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Xamarin_LinkOS_Developer_Demo
{
    public class FormatDemoView : BaseDemoView
    {
        public delegate void FormatSelectedHandler(Format fileName);
        public event FormatSelectedHandler OnFormatSelected;

        ObservableCollection<Format> formatList;
        Label title;
        ActivityIndicator spinner;
        Button refreshBtn;
        ListView formatLv;

        public FormatDemoView() : base()
        {
            formatList = new ObservableCollection<Format>();
            title = new Label { Text = "Format Demo" };
            refreshBtn = new Button { Text = "Get Formats" };
            refreshBtn.Clicked += RefreshBtn_Clicked;
            spinner = new ActivityIndicator { IsRunning = false };

            formatLv = new ListView
            {
                ItemsSource = formatList
            };
            formatLv.ItemSelected += FormatLv_ItemSelected;

            Children.Add(title);
            Children.Add(refreshBtn);
            Children.Add(spinner);
            Children.Add(formatLv);
            //GetTemplates();
        }

        private void RefreshBtn_Clicked(object sender, EventArgs e)
        {
            if (CheckPrinter())
            {
                title.Text = "Format Demo";
                refreshBtn.IsEnabled = false;
                spinner.IsRunning = true;
                formatLv.IsEnabled = false;
                GetTemplates();
            }
        }

        private void GetTemplates()
        {
			formatList.Clear ();
            new Task(new Action(() => {
                StartGetTemplatesFromDevice();
                StartGetTemplatesFromPrinter();
            })).Start();
        }

        private void StartGetTemplatesFromPrinter()
        {
            //CheckPrinter();
            IConnection connection = myPrinter.Connection;
            try
            {
                connection.MaxTimeoutForRead = 5000;
                connection.TimeToWaitForMoreData = 1000;
                connection.Open();

                if (!CheckPrinterLanguage(connection))
                {
                    resetPage();
                    return;
                }
                IZebraPrinter printer = ZebraPrinterFactory.Current.GetInstance(connection);
                string[] fileNames = printer.RetrieveFileNames(new string[] { "ZPL" });
                addFormatsToList(fileNames, Format.Location.printer);
            }
            catch(Exception e)
            {
                ShowErrorAlert(e.Message + " - On printer");
            }
            finally
            {
                if ((connection != null) && (connection.IsConnected))
                    connection.Close();
                resetPage();
            }
        }
        private void addFormatsToList(string[] fileNames, Format.Location location)
        {
            if (fileNames == null)
                return;
            Device.BeginInvokeOnMainThread(() =>
            {
                foreach (string file in fileNames)
                {
                    Format fmt = new Format { Path = file, FileLocation = location };
                    formatList.Add(fmt);
                }
            });
        }

        private void resetPage()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                spinner.IsRunning = false;
                refreshBtn.IsEnabled = true;
                formatLv.IsEnabled = true;
                title.Text = "Format Demo - " + formatList.Count + " formats found";
                if (0 == formatList.Count)
                {
                    ShowAlert("No Formats Found.", "Notification");
                }
            });
        }

        private void StartGetTemplatesFromDevice()
        {
            string[] fileList = DependencyService.Get<IFileUtil>().GetFiles("ZPL");
            addFormatsToList(fileList, Format.Location.device);
        }

        private void FormatLv_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is Format)
            {
                if (OnFormatSelected != null)
                    OnFormatSelected((Format)e.SelectedItem);

                formatLv.SelectedItem = null;
            }
        }
    }

    public class Format
    {
        public string Path { get; set; }
        public enum Location
        {
            device,
            printer
        }
        public Location FileLocation { get; set; }
        public override string ToString()
        {
            return Path;
        }
        public string PrinterPath { get { return Path.Substring(Path.LastIndexOf('/')+1); } }
    }
}
