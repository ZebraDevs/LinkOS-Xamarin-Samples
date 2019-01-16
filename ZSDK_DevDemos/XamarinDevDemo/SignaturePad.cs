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
using System.IO;
using Xamarin.Forms;

namespace XamarinDevDemo {

    public class SignaturePad : BoxView {

        public static readonly Color DefaultBackgroundColor = Color.LightGray;
        public static readonly Color DefaultStrokeColor = Color.Black;
        public static readonly float DefaultStrokeWidth = 3.0f;

        public event EventHandler<ImageStreamRequestedEventArgs> ImageStreamRequested;

        public class ImageStreamRequestedEventArgs : EventArgs {
            public double PrintWidth { get; set; }
            public Stream ImageStream { get; set; }
        }

        public Stream GetImageStream(double printWidth) {
            var args = new ImageStreamRequestedEventArgs {
                PrintWidth = printWidth
            };

            ImageStreamRequested?.Invoke(this, args);
            return args.ImageStream;
        }
    }
}
