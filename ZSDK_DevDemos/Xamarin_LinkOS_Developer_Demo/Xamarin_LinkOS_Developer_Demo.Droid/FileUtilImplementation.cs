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
File:   IFileUtil.cs

Descr:  Class to access OS specific methods to get and read files from a device. 

Date: 03/8/16 
Updated:
**********************************************************************************************************/
using Xamarin_LinkOS_Developer_Demo.Droid;
using System.IO;
using System;
using Java.Lang;

[assembly: Xamarin.Forms.Dependency(typeof(FileUtilImplementation))]
namespace Xamarin_LinkOS_Developer_Demo.Droid
{
    public class FileUtilImplementation : IFileUtil
    {
        public string[] GetFiles(string extension)
        {
            string[] fileList = null;
            string path = "/storage/sdcard0/Download/";
            if (!Directory.Exists(path))
            {
                path = Android.OS.Environment.ExternalStorageDirectory + "/" + Android.OS.Environment.DirectoryDownloads + "/";
                if (!Directory.Exists(path))
                {
                    path = "";
                }
            }
            if (path.Length > 0)
            {
                fileList = Directory.GetFiles(path, "*."+extension);
            }
            return fileList;
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }
    }
}