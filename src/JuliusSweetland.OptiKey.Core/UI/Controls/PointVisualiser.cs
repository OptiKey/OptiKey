// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using JuliusSweetland.OptiKey.UI.Utilities;
using log4net;

namespace JuliusSweetland.OptiKey.UI.Controls
{
    public class PointVisualiser : Canvas
    {
        #region Private Member Vars

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Captured Coordinates Dependency Property

        public static readonly DependencyProperty PointsToDisplayProperty =
            DependencyProperty.Register("PointsToDisplay", typeof(List<Point>), typeof(PointVisualiser),
                new FrameworkPropertyMetadata(default(List<Point>), FrameworkPropertyMetadataOptions.AffectsRender));

        public List<Point> PointsToDisplay
        {
            get { return (List<Point>)GetValue(PointsToDisplayProperty); }
            set { SetValue(PointsToDisplayProperty, value); }
        }

        #endregion

        #region On Render

        protected override void OnRender(DrawingContext dc)
        {
            if (PointsToDisplay != null)
            {
                Log.Debug("PointsToDisplay is not empty - rendering points");

                var canvasWidth = (int)ActualWidth;
                var canvasHeight = (int)ActualHeight;

                if (canvasWidth > 0 && canvasHeight > 0)
                {
                    //Create the bitModeScreenCoordinateToKeyMap
                    var wb = new WriteableBitmap(canvasWidth, canvasHeight, 96, 96, PixelFormats.Bgra32, null);

                    //Create a new image
                    var img = new Image
                    {
                        Source = wb,
                        Stretch = Stretch.None,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top
                    };

                    //Set scaling mode, edge mode and z index on canvas
                    RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);
                    RenderOptions.SetEdgeMode(img, EdgeMode.Aliased);
                    SetZIndex(img, -100);

                    //Each "dot" is 3x3 rectangle (centered on the coordinate detected)
                    var rect = new Int32Rect(0, 0, 3, 3);
                    int size = rect.Width * rect.Height * 4;
                    var pixels = new byte[size];

                    int screenCoordinatesIndex = 0;
                    int screenCoordinatesIndexUpperBound = PointsToDisplay.Count - 1;

                    foreach (Point capturedCoordinate in PointsToDisplay)
                    {
                        Point canvasPoint = PointFromScreen(capturedCoordinate); //Convert screen to canvas point

                        if (canvasPoint.X >= 0 && canvasPoint.X < canvasWidth
                            && canvasPoint.Y >= 0 && canvasPoint.Y < canvasHeight)
                        {
                            SetPixelValuesToRainbow(pixels, rect, screenCoordinatesIndex, screenCoordinatesIndexUpperBound); //Set up pixel colours (as RGB and Alpha array of bytes)

                            //We are drawing a 3x3 dot so try to start one pixel up and left (center pixel of rectangle will be the co-ordinate)
                            //If coord in against the top or left side (x=0 and/or y=0) this cannot be done, so just place as close as possible
                            //If coord in against the bottom or right side (x>=canvasWidth-1 and/or y>=canvasHeight-1) this cannot be done either, so just place as close as possible
                            rect.X = (int)canvasPoint.X == 0
                                ? (int)canvasPoint.X
                                : (int)canvasPoint.X > 0 && (int)canvasPoint.X < canvasWidth - 1
                                    ? (int)canvasPoint.X - 1
                                    : (int)canvasPoint.X - 2;

                            rect.Y = (int)canvasPoint.Y == 0
                                ? (int)canvasPoint.Y
                                : (int)canvasPoint.Y > 0 && (int)canvasPoint.Y < canvasHeight - 1
                                    ? (int)canvasPoint.Y - 1
                                    : (int)canvasPoint.Y - 2;

                            wb.WritePixels(rect, pixels, rect.Width * 4, 0);

                            screenCoordinatesIndex++;
                        }
                    }

                    dc.DrawImage(wb, new Rect(0, 0, canvasWidth, canvasHeight));
                }
            }
            else
            {
                Log.Debug("OnRender - PointsToDisplay is empty - nothing to render");
            }

            base.OnRender(dc);
        }

        #endregion

        #region Set Pixel Values To Rainbow

        private static void SetPixelValuesToRainbow(byte[] pixels, Int32Rect rect, int index, int indexUpperBound)
        {
            double h = ((double)index) / ((double)indexUpperBound);
            const double sl = 0.5;
            const double l = 0.5;

            var rgb = DrawingUtils.HSL2RGB(h, sl, l);

            for (int i = 0; i < rect.Height * rect.Width; ++i)
            {
                pixels[i * 4 + 0] = rgb.B; // Blue
                pixels[i * 4 + 1] = rgb.G; // Green
                pixels[i * 4 + 2] = rgb.R; // Red
                pixels[i * 4 + 3] = 255; // Alpha
            }
        }

        #endregion
    }
}
