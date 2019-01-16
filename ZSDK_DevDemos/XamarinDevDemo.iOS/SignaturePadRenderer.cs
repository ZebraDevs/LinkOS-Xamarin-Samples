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

using CoreGraphics;
using DeveloperDemo_Xamarin.iOS;
using Foundation;
using System.IO;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using XamarinDevDemo;

[assembly: ExportRenderer(typeof(SignaturePad), typeof(SignaturePadRenderer))]
namespace DeveloperDemo_Xamarin.iOS {

    public class SignaturePadRenderer : ViewRenderer<SignaturePad, UIView> {

        private CGPoint currentPoint;
        private CGPoint previousPoint;
        private CGPath path = new CGPath();

        protected override void OnElementChanged(ElementChangedEventArgs<SignaturePad> e) {
            base.OnElementChanged(e);

            if (e.OldElement != null) {
                e.OldElement.ImageStreamRequested -= OnImageStreamRequested;
            }

            if (e.NewElement != null) {
                e.NewElement.ImageStreamRequested += OnImageStreamRequested;
            }

            BackgroundColor = SignaturePad.DefaultBackgroundColor.ToUIColor();
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt) {
            base.TouchesBegan(touches, evt);

            UITouch touch = touches.AnyObject as UITouch;
            currentPoint = touch.LocationInView(this);
            previousPoint = touch.PreviousLocationInView(this);

            SetNeedsDisplay();
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt) {
            base.TouchesMoved(touches, evt);

            UITouch touch = touches.AnyObject as UITouch;
            currentPoint = touch.LocationInView(this);
            previousPoint = touch.PreviousLocationInView(this);

            SetNeedsDisplay();
        }

        public override void Draw(CGRect rect) {
            base.Draw(rect);

            if (!previousPoint.IsEmpty) {
                using (CGContext context = UIGraphics.GetCurrentContext()) {
                    context.SetStrokeColor(UIColor.Black.CGColor);
                    context.SetLineJoin(CGLineJoin.Round);
                    context.SetLineCap(CGLineCap.Round);
                    context.SetLineWidth(SignaturePad.DefaultStrokeWidth);

                    path.MoveToPoint(previousPoint);
                    path.AddLineToPoint(currentPoint);

                    context.AddPath(path);
                    context.DrawPath(CGPathDrawingMode.Stroke);
                }
            }
        }

        private void OnImageStreamRequested(object sender, SignaturePad.ImageStreamRequestedEventArgs e) {
            e.ImageStream = CreateImageStream(e.PrintWidth);
        }

        private Stream CreateImageStream(double printWidth) {
            UIImage image = CreateSignatureImage(printWidth);
            return image?.AsJPEG().AsStream();
        }

        public UIImage CreateSignatureImage(double printWidth) {
            UIImage image = null;
            CGSize size = Element.Bounds.Size.ToSizeF();
            float width = (float)Element.Bounds.Width;
            float height = (float)Element.Bounds.Height;
            float scale = (float)printWidth / width;

            UIGraphics.BeginImageContext(size);

            using (CGContext context = UIGraphics.GetCurrentContext()) {
                context.SetFillColor(Color.White.ToCGColor());
                context.SetStrokeColor(SignaturePad.DefaultStrokeColor.ToCGColor());
                context.SetLineWidth(SignaturePad.DefaultStrokeWidth);
                context.SetLineJoin(CGLineJoin.Round);
                context.SetLineCap(CGLineCap.Round);

                context.AddPath(path);
                context.DrawPath(CGPathDrawingMode.Stroke);

                image = UIGraphics.GetImageFromCurrentImageContext();
                image = image.Scale(new CGSize(printWidth, height * scale));
            }

            UIGraphics.EndImageContext();

            return image;
        }
    }
}