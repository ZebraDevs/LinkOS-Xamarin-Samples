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

using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using XamarinDevDemo;
using XamarinDevDemo.Droid;

[assembly: ExportRenderer(typeof(SignaturePad), typeof(SignaturePadRenderer))]
namespace XamarinDevDemo.Droid {

    public class SignaturePadRenderer : ViewRenderer<SignaturePad, FrameLayout> {

        private Paint paint = new Paint();
        private Android.Graphics.Path path = new Android.Graphics.Path();
        private float previousX;
        private float previouxY;

        public SignaturePadRenderer(Context context) : base(context) {
            paint.AntiAlias = true;
            paint.SetStyle(Paint.Style.Stroke);
            paint.Color = SignaturePad.DefaultStrokeColor.ToAndroid();
            paint.StrokeJoin = Paint.Join.Round;
            paint.StrokeCap = Paint.Cap.Round;
            paint.StrokeWidth = SignaturePad.DefaultStrokeWidth * context.Resources.DisplayMetrics.Density;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<SignaturePad> e) {
            base.OnElementChanged(e);

            if (e.OldElement != null) {
                e.OldElement.ImageStreamRequested -= OnImageStreamRequested;
            }

            if (e.NewElement != null) {
                e.NewElement.ImageStreamRequested += OnImageStreamRequested;
            }

            Background = new ColorDrawable(SignaturePad.DefaultBackgroundColor.ToAndroid());
        }

        protected override void OnDraw(Canvas canvas) {
            canvas.DrawPath(path, paint);
        }

        public override bool OnTouchEvent(MotionEvent e) {
            float currentX = e.GetX();
            float currentY = e.GetY();

            switch (e.Action) {
                case MotionEventActions.Down:
                    path.MoveTo(currentX, currentY);

                    previousX = currentX;
                    previouxY = currentY;
                    return true;

                case MotionEventActions.Move:
                case MotionEventActions.Up:
                    for (int i = 0; i < e.HistorySize; i++) {
                        float historicalX = e.GetHistoricalX(i);
                        float historicalY = e.GetHistoricalY(i);
                        path.LineTo(historicalX, historicalY);
                    }
                    path.LineTo(currentX, currentY);

                    Invalidate();

                    previousX = currentX;
                    previouxY = currentY;
                    return true;

                default:
                    return false;
            }
        }

        private void OnImageStreamRequested(object sender, SignaturePad.ImageStreamRequestedEventArgs e) {
            e.ImageStream = CreateImageStream(e.PrintWidth);
        }

        private Stream CreateImageStream(double printWidth) {
            using (Bitmap image = CreateSignatureBitmap(printWidth)) {
                MemoryStream stream = new MemoryStream();
                bool success = image.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);

                image.Recycle();

                if (success) {
                    stream.Position = 0;
                    return stream;
                }
            }

            return null;
        }

        private Bitmap CreateSignatureBitmap(double printWidth) {
            float width = (float)Element.Bounds.Width * Resources.DisplayMetrics.Density;
            float height = (float)Element.Bounds.Height * Resources.DisplayMetrics.Density;
            float scale = (float)printWidth / width;
            float printHeight = height * scale;

            Bitmap bitmap = Bitmap.CreateBitmap((int)width, (int)height, Bitmap.Config.Argb8888);
            using (Canvas canvas = new Canvas(bitmap)) {
                canvas.DrawColor(Android.Graphics.Color.White);
                canvas.DrawPath(path, paint);
            }

            return Bitmap.CreateScaledBitmap(bitmap, (int)printWidth, (int)printHeight, false);
        }
    }
}