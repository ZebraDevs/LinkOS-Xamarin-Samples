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

using DeveloperDemo_Xamarin.WPF;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WPF;
using XamarinDevDemo;

[assembly: ExportRenderer(typeof(SignaturePad), typeof(SignaturePadRenderer))]
namespace DeveloperDemo_Xamarin.WPF {

    public class SignaturePadRenderer : ViewRenderer<SignaturePad, InkCanvas> {

        private InkCanvas inkCanvas;

        protected override void OnElementChanged(ElementChangedEventArgs<SignaturePad> e) {
            base.OnElementChanged(e);

            if (Control == null) {
                inkCanvas = new InkCanvas();
                inkCanvas.DefaultDrawingAttributes.Color = SignaturePad.DefaultStrokeColor.ToMediaColor();
                inkCanvas.DefaultDrawingAttributes.StylusTip = StylusTip.Ellipse;
                inkCanvas.DefaultDrawingAttributes.Width = SignaturePad.DefaultStrokeWidth;
                inkCanvas.DefaultDrawingAttributes.FitToCurve = true;
                SetNativeControl(inkCanvas);
            }

            if (e.OldElement != null) {
                e.OldElement.ImageStreamRequested -= OnImageStreamRequested;
            }

            if (e.NewElement != null) {
                e.NewElement.ImageStreamRequested += OnImageStreamRequested;
            }

            Control.Background = SignaturePad.DefaultBackgroundColor.ToBrush();
        }

        private void OnImageStreamRequested(object sender, SignaturePad.ImageStreamRequestedEventArgs e) {
            e.ImageStream = CreateImageStream(e.PrintWidth);
        }

        private Stream CreateImageStream(double printWidth) {
            var tcs = new TaskCompletionSource<Stream>();
            Device.BeginInvokeOnMainThread(() => {
                try {
                    double width = Element.Bounds.Width;
                    double height = Element.Bounds.Height;
                    double scale = printWidth / width;
                    double printHeight = height * scale;

                    InkCanvas ink = new InkCanvas { Strokes = new StrokeCollection() };
                    ink.Measure(new System.Windows.Size(printWidth, printHeight));

                    ScaleTransform scaleTransform = new ScaleTransform(scale, scale);

                    foreach (Stroke stroke in inkCanvas.Strokes) {
                        Stroke scaledStroke = stroke.Clone();
                        scaledStroke.Transform(scaleTransform.Value, true);
                        ink.Strokes.Add(scaledStroke);
                    }

                    ink.Arrange(new System.Windows.Rect(0, 0, printWidth, printHeight));

                    RenderTargetBitmap rtb = new RenderTargetBitmap((int)printWidth, (int)printHeight, 96, 96, PixelFormats.Default);
                    rtb.Render(ink);

                    MemoryStream stream = new MemoryStream();
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(rtb));
                    encoder.Save(stream);

                    tcs.SetResult(stream);
                } catch (Exception e) {
                    tcs.SetException(e);
                }
            });
            return tcs.Task.Result;
        }
    }
}
