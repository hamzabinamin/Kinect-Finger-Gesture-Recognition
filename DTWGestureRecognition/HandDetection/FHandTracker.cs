using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Microsoft.Kinect;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;


namespace DTWGestureRecognition.HandDetection
{
    class FHandTracker
    {
   KinectSensor sensor;
        private bool connected = false;

        public Bitmap depthImage;
        public Bitmap colorImage;
        
        private IntPtr depthPtr;
        private IntPtr colorPtr;
        private static int frameNb = 0;
        private void skip(){}
        public delegate void afterReady();
        private afterReady afterColorReady;
        private afterReady afterDepthReady;
        private bool doWork = true;

        public FHandTrackerSettings settings{get; set;}

        public List<Hand> hands { get; set; }

        public FHandTracker()
        {

            afterColorReady = skip;
            afterDepthReady = skip;

            settings = new FHandTrackerSettings();

            hands = new List<Hand>();

            // Check if there is any Kinect device connected
            if (KinectSensor.KinectSensors.Count > 0)
            {
                connected = true;
                sensor = KinectSensor.KinectSensors.ElementAt(0);

                //sensor.DepthStream.Range = DepthRange.Near;

                sensor.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(depthFrameReady);
                sensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(colorFrameReady);
               
            }
            else // No device connected
            {
                connected = false;
            }

        }

        public void start()
        {
            sensor.DepthStream.Enable(settings.depthFormat);
            sensor.ColorStream.Enable();
            sensor.Start();
        }

        public void stop()
        {
            sensor.DepthStream.Disable();
            sensor.ColorStream.Disable();
            sensor.Stop();
        }

        public void setEventColorReady(afterReady del)
        {
            afterColorReady = del;
        }

        public void clearEventColorReady()
        {
            afterColorReady = skip;
        }

        public void setEventDepthReady(afterReady del)
        {
            afterDepthReady = del;
        }

        public void clearEventDepthReady()
        {
            afterDepthReady = skip;
        }

        public void colorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame frame = e.OpenColorImageFrame())
            {

                if (frame == null)
                    return;

                byte[] pixels = new byte[frame.PixelDataLength];
                frame.CopyPixelDataTo(pixels);

                Marshal.FreeHGlobal(colorPtr);
                colorPtr = Marshal.AllocHGlobal(pixels.Length);
                Marshal.Copy(pixels, 0, colorPtr, pixels.Length);

                int stride = frame.Width * 4;

                colorImage = new Bitmap(
                    frame.Width,
                    frame.Height,
                    stride,
                    PixelFormat.Format32bppRgb,
                    colorPtr);

                afterColorReady();

            }
        }

        public  void depthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            // Get the depth frame from Kinect
            if (frameNb < 4)
            {
                frameNb++;
                return;
            }
            else
            {
                frameNb = 0;
            }
            if (doWork)
            {
                using (DepthImageFrame frame = e.OpenDepthImageFrame())
                {


                    // Check that the frame is not null
                    if (frame == null)
                        return;

                    // Calculate the real distance for every pixel in the depth image
                    int[] distances = generateDistances(frame);

                    // Return a 0 or 1 matrix, which contains wich pixels are near enough
                    bool[][] near = generateValidMatrix(frame, distances);


                    // Return the tracked hands based on the near pixels
                    hands = localizeHands(near);

                    byte[] pixels = new byte[frame.PixelDataLength * 4];

                    // Free last depth Matrix
                    Marshal.FreeHGlobal(depthPtr);
                    depthPtr = Marshal.AllocHGlobal(pixels.Length);
                    Marshal.Copy(pixels, 0, depthPtr, pixels.Length);

                    // Create the bitmap
                    int height = near.Length;
                    int width = 0;
                    if (near.Length > 0)
                    {
                        width = near[0].Length;
                    }
                    int stride = width * 4;

                    depthImage = new Bitmap(
                        width,
                        height,
                        stride,
                        PixelFormat.Format32bppRgb,
                        depthPtr);

                    // Calculate 3D points for the hands

                    for (int i = 0; i < hands.Count; ++i)
                    {
                        hands[i].calculate3DPoints(settings.screenWidth, settings.screenHeight, distances);
                    }

                    // Call the rest of the functions
                    afterDepthReady();



                    // Draw fingertips and palm center
                    /*
                    Graphics gBmp = Graphics.FromImage(depthImage);
                    Brush blueBrush = new SolidBrush(Color.Blue);
                    Brush redBrush = new SolidBrush(Color.Red);

                    for (int i = 0; i < hands.Count; ++i)
                    {
                        // Red point which is the center of the palm
                        gBmp.FillEllipse(redBrush, hands[i].palm.Y - 5, hands[i].palm.X - 5, 10, 10);

                        // Draw inside points
                        //for (int j = 0; j < hands[i].inside.Length; ++j)
                        //{
                        //    Point p = hands[i].inside.Get(j);
                        //    depthImage.SetPixel(p.Y, p.X, Color.Blue);
                        //}

                        // Draw the contour of the hands
                        for (int j = 0; j < hands[i].contour.Count; ++j)
                        {
                            PointFT p = hands[i].contour[j];
                            depthImage.SetPixel(p.Y, p.X, Color.Red);
                        }

                        // Blue points which represent the fingertips
                        for (int j = 0; j < hands[i].fingertips.Count; ++j)
                        {
                            if (hands[i].fingertips[j].X != -1)
                            {
                                gBmp.FillEllipse(blueBrush, hands[i].fingertips[j].Y - 5, 
                                    hands[i].fingertips[j].X - 5, 10, 10);
                            }
                        }
                    }

                    blueBrush.Dispose();
                    redBrush.Dispose();
                    gBmp.Dispose();
                     */
                    /*
                    if (this.hands.Count == 2)
                    {
                        Debug.WriteLine(this.hands[1].palm.X + " " + this.hands[0].palm.X);
                    }
                    if (this.hands.Count == 1)
                    {
                        Debug.WriteLine(this.hands[0].palm.X);
                    }
                     * */
                }
            }
        }

        private int[] generateDistances(DepthImageFrame frame)
        {
            // Raw depth data form the Kinect
            short[] depth = new short[frame.PixelDataLength];
            frame.CopyPixelDataTo(depth);

            // Calculate the real distance
            int[] distance = new int[frame.PixelDataLength];
            for (int i = 0; i < distance.Length; ++i)
            {
                distance[i] = depth[i] >> DepthImageFrame.PlayerIndexBitmaskWidth;
            }

            return distance;
        }

        private bool[][] generateValidMatrix(DepthImageFrame frame, int[] distance)
        {
            // Create the matrix. The size depends on the margins
            int x1 = (int) (frame.Width * settings.marginLeftPerc / 100.0f);
            int x2 = (int) (frame.Width * (1 - (settings.marginRightPerc / 100.0f)));
            int y1 = (int) (frame.Height * settings.marginTopPerc / 100.0f);
            int y2 = (int) (frame.Height * (1 - (settings.marginBotPerc / 100.0f)));
            bool[][] near = new bool[y2 - y1][];
            for (int i = 0; i < near.Length; ++i)
            {
                near[i] = new bool[x2 - x1];
            }

            // Calculate max and min distance
            int max = int.MinValue, min = int.MaxValue;

            for (int k = 0; k < distance.Length; ++k)
            {
                if (distance[k] > max) max = distance[k];
                if (distance[k] < min && distance[k] != -1) min = distance[k];
            }

            // Decide if it is near or not
            int margin = (int)(min + settings.nearSpacePerc * (max - min));
            int index = 0;
            if (settings.absoluteSpace != -1) margin = min + settings.absoluteSpace;
            for (int i = 0; i < near.Length; ++i)
            {
                for (int j = 0; j < near[i].Length; ++j)
                {
                    index = frame.Width * (i + y1) + (j + x1);
                    if (distance[index] <= margin && distance[index] != -1)
                    {
                        near[i][j] = true;
                    }
                    else
                    {
                        near[i][j] = false;
                    }
                }
            }

            // Dilate and erode the image to get smoother figures
            if (settings.smoothingIterations > 0)
            {
                near = dilate(near, settings.smoothingIterations);
                near = erode(near, settings.smoothingIterations);
            }

            // Mark as not valid the borders of the matrix to improve the efficiency in some methods
            int m;
            // First row
            for (int j = 0; j < near[0].Length; ++j)
                near[0][j] = false;

            // Last row
            m = near.Length - 1;
            for (int j = 0; j < near[0].Length; ++j)
                near[m][j] = false;

            // First column
            for (int i = 0; i < near.Length; ++i)
                near[i][0] = false;

            // Last column
            m = near[0].Length - 1;
            for (int i = 0; i < near.Length; ++i)
                near[i][m] = false;

            return near;
        }

        private List<Hand> localizeHands(bool[][] valid)
        {
            int i, j, k;

            List<Hand> hands = new List<Hand>();

            List<PointFT> insidePoints = new List<PointFT>();
            List<PointFT> contourPoints = new List<PointFT>();


            bool[][] contour = new bool[valid.Length][];
            for (i = 0; i < valid.Length; ++i)
            {
                contour[i] = new bool[valid[0].Length];
            }

            // Divide points in contour and inside points
            int count = 0;
            for (i = 1; i < valid.Length - 1; ++i)
            {
                for (j = 1; j < valid[i].Length - 1; ++j)
                {

                    if (valid[i][j])
                    {
                        // Count the number of valid adjacent points
                        count = this.numValidPixelAdjacent(ref i, ref j, ref valid);

                        if (count == 4) // Inside
                        {
                            insidePoints.Add(new PointFT(i, j));
                        }
                        else // Contour
                        {
                            contour[i][j] = true;
                            contourPoints.Add(new PointFT(i, j));
                        }

                    }
                }
            }

            // Create the sorted contour list, using the turtle algorithm
            for (i = 0; i < contourPoints.Count; ++i)
            {
                Hand hand = new Hand();

                // If it is a possible start point
                if(contour[contourPoints[i].X][contourPoints[i].Y]){
                    
                    // Calculate the contour
                    hand.contour = CalculateFrontier(ref valid, contourPoints[i], ref contour);

                    // Check if the contour is big enough to be a hand
                    if (hand.contour.Count / (contourPoints.Count * 1.0f) > 0.20f 
                        && hand.contour.Count > settings.k)
                    {
                        // Calculate the container box
                        hand.calculateContainerBox(settings.containerBoxReduction);

                        // Add the hand to the list
                        hands.Add(hand);
                    }

                    // Don't look for more hands, if we reach the limit
                    if (hands.Count >= settings.maxTrackedHands)
                    {
                        break;
                    }
                }

            }

            // Allocate the inside points to the correct hand using its container box

            //List<int> belongingHands = new List<int>();
            for (i = 0; i < insidePoints.Count; ++i)
            {
                for (j = 0; j < hands.Count; ++j)
                {
                    if (hands[j].isPointInsideContainerBox(insidePoints[i]))
                    {
                        hands[j].inside.Add(insidePoints[i]);
                        //belongingHands.Add(j);
                    }
                }

                // A point can only belong to one hand, if not we don't take that point into account
                /*if (belongingHands.Count == 1)
                {
                    hands[belongingHands.ElementAt(0)].inside.Add(insidePoints[i]);
                }
                belongingHands.Clear();*/
            }

            // Find the center of the palm
            float min, max, distance = 0;

            for (i = 0; i < hands.Count; ++i)
            {
                max = float.MinValue;
                for (j = 0; j < hands[i].inside.Count; j += settings.findCenterInsideJump)
                {
                    min = float.MaxValue;
                    for (k = 0; k < hands[i].contour.Count; k += settings.findCenterInsideJump)
                    {
                        distance = PointFT.distanceEuclidean(hands[i].inside[j], hands[i].contour[k]);
                        if (!hands[i].isCircleInsideContainerBox(hands[i].inside[j], distance)) continue;
                        if (distance < min) min = distance;
                        if (min < max) break;
                    }

                    if (max < min && min != float.MaxValue)
                    {
                        max = min;
                        hands[i].palm = hands[i].inside[j];
                    }
                }
            }

            // Find the fingertips
            PointFT p1, p2, p3, pAux, r1, r2;
            int size;
            double angle;
            int jump;

            for (i = 0; i < hands.Count; ++i)
            {
                // Check if there is a point at the beginning to avoid checking the last ones of the list
                max = hands[i].contour.Count;
                size = hands[i].contour.Count;
                jump = (int) (size * settings.fingertipFindJumpPerc);
                for (j = 0; j < settings.k; j += 1)
                {
                    p1 = hands[i].contour[(j - settings.k + size) % size];
                    p2 = hands[i].contour[j];
                    p3 = hands[i].contour[(j + settings.k) % size];
                    r1 = p1 - p2;
                    r2 = p3 - p2;

                    angle = PointFT.angle(r1, r2);

                    if (angle > 0 && angle < settings.theta)
                    {
                        pAux = p3 + ((p1 - p3) / 2);
                        if (PointFT.distanceEuclideanSquared(pAux, hands[i].palm) >
                            PointFT.distanceEuclideanSquared(hands[i].contour[j], hands[i].palm)) 
                            continue;

                        hands[i].fingertips.Add(hands[i].contour[j]);
                        max = hands[i].contour.Count + j - jump;
                        max = Math.Min(max, hands[i].contour.Count);
                        j += jump;
                        break;
                    }
                }

                // Continue with the rest of the points
                for ( ; j < max; j += settings.findFingertipsJump)
                {
                    p1 = hands[i].contour[(j - settings.k + size) % size];
                    p2 = hands[i].contour[j];
                    p3 = hands[i].contour[(j + settings.k) % size];
                    r1 = p1 - p2;
                    r2 = p3 - p2;

                    angle = PointFT.angle(r1, r2);

                    if (angle > 0 && angle < settings.theta)
                    {
                        pAux = p3 + ((p1 - p3) / 2);
                        if (PointFT.distanceEuclideanSquared(pAux, hands[i].palm) >
                            PointFT.distanceEuclideanSquared(hands[i].contour[j], hands[i].palm))
                            continue;

                        hands[i].fingertips.Add(hands[i].contour[j]);
                        j += jump;
                    }
                }
            }

            return hands;
        }

        /*
         * This function calcute the border of a closed figure starting in one of the contour points.
         * The turtle algorithm is used.
         */
        private List<PointFT> CalculateFrontier(ref bool[][] valid, PointFT start, ref bool[][] contour)
        {
            List<PointFT> list = new List<PointFT>();
            PointFT last = new PointFT(-1, -1);
            PointFT current = new PointFT(start);
            int dir = 0;

            do
            {
                if (valid[current.X][current.Y])
                {
                    dir = (dir + 1) % 4;
                    if (current != last)
                    {
                        list.Add(new PointFT(current.X, current.Y));
                        last = new PointFT(current);
                        contour[current.X][current.Y] = false;
                    }
                }
                else
                {
                    dir = (dir + 4 - 1) % 4;
                }

                switch (dir)
                {
                    case 0: current.X += 1; break; // Down
                    case 1: current.Y += 1; break; // Right
                    case 2: current.X -= 1; break; // Up
                    case 3: current.Y -= 1; break; // Left
                }
            } while (current != start);

            return list;
        }
        public void disable()
        {
            this.doWork = false;
        }
        public void enable()
        {
            this.doWork = true;
        }
        private bool[][] dilate(bool[][] image, int it)
        {
            // Matrix to store the dilated image
            bool[][] dilateImage = new bool[image.Length][];
            for (int i = 0; i < image.Length; ++i)
            {
                dilateImage[i] = new bool[image[i].Length];
            }

            // Distances matrix
            int[][] distance = manhattanDistanceMatrix(image, true);

            // Dilate the image
            for (int i = 0; i < image.Length; i++)
            {
                for (int j = 0; j < image[i].Length; j++)
                {
                    dilateImage[i][j] = ((distance[i][j] <= it) ? true : false);
                }
            }

            return dilateImage;
        }

        private bool[][] erode(bool[][] image, int it)
        {
            // Matrix to store the dilated image
            bool[][] erodeImage = new bool[image.Length][];
            for (int i = 0; i < image.Length; ++i)
            {
                erodeImage[i] = new bool[image[i].Length];
            }

            // Distances matrix
            int[][] distance = manhattanDistanceMatrix(image, false);

            // Dilate the image
            for (int i = 0; i < image.Length; i++)
            {
                for (int j = 0; j < image[i].Length; j++)
                {
                    erodeImage[i][j] = ((distance[i][j] > it) ? true : false);
                }
            }

            return erodeImage;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="zeroDistanceValue"></param>
        /// <returns></returns>
        private int[][] manhattanDistanceMatrix(bool[][] image, bool zeroDistanceValue)
        {
            int[][] distanceMatrix = new int[image.Length][];
            for (int i = 0; i < distanceMatrix.Length; ++i)
            {
                distanceMatrix[i] = new int[image[i].Length];
            }

            // traverse from top left to bottom right
            for (int i = 0; i < distanceMatrix.Length; i++)
            {
                for (int j = 0; j < distanceMatrix[i].Length; j++)
                {
                    if ((image[i][j] && zeroDistanceValue) || (!image[i][j] && !zeroDistanceValue))
                    {
                        // first pass and pixel was on, it gets a zero
                        distanceMatrix[i][j] = 0;
                    }
                    else
                    {
                        // pixel was off
                        // It is at most the sum of the lengths of the array
                        // away from a pixel that is on
                        distanceMatrix[i][j] = image.Length + image[i].Length;
                        // or one more than the pixel to the north
                        if (i > 0) distanceMatrix[i][j] = Math.Min(distanceMatrix[i][j], distanceMatrix[i - 1][j] + 1);
                        // or one more than the pixel to the west
                        if (j > 0) distanceMatrix[i][j] = Math.Min(distanceMatrix[i][j], distanceMatrix[i][j - 1] + 1);
                    }
                }
            }
            // traverse from bottom right to top left
            for (int i = distanceMatrix.Length - 1; i >= 0; i--)
            {
                for (int j = distanceMatrix[i].Length - 1; j >= 0; j--)
                {
                    // either what we had on the first pass
                    // or one more than the pixel to the south
                    if (i + 1 < distanceMatrix.Length)
                        distanceMatrix[i][j] = Math.Min(distanceMatrix[i][j], distanceMatrix[i + 1][j] + 1);
                    // or one more than the pixel to the east
                    if (j + 1 < distanceMatrix[i].Length)
                        distanceMatrix[i][j] = Math.Min(distanceMatrix[i][j], distanceMatrix[i][j + 1] + 1);
                }
            }

            return distanceMatrix;
        }

        /*
         * Counts the number of adjacent valid points without taking into account the diagonals
         */
        private int numValidPixelAdjacent(ref int i, ref int j, ref bool[][] valid)
        {
            int count = 0;

            if (valid[i + 1][j]) ++count;
            if (valid[i - 1][j]) ++count;
            if (valid[i][j + 1]) ++count;
            if (valid[i][j - 1]) ++count;
            //if (valid[i + 1][j + 1]) ++count;
            //if (valid[i + 1][j - 1]) ++count;
            //if (valid[i - 1][j + 1]) ++count;
            //if (valid[i - 1][j - 1]) ++count;

            return count;
        }

        // Generate a representable image of the valid matrix
        private byte[] generateDepthImage(bool[][] near)
        {
            // Image pixels
            byte[] pixels = new byte[near.Length * near[0].Length * 4];
            int width = near[0].Length;

            for (int i = 1; i < near.Length - 1; ++i)
            {
                for (int j = 1; j < near[i].Length - 1; ++j)
                {
                    if (near[i][j]){

                        if (!near[i + 1][j] || !near[i - 1][j]
                        || !near[i][j + 1] || !near[i][j - 1]) // Is border
                        {
                            pixels[(i * width + j) * 4 + 0] = 255;
                            pixels[(i * width + j) * 4 + 1] = 0;
                            pixels[(i * width + j) * 4 + 2] = 0;
                            pixels[(i * width + j) * 4 + 3] = 0;
                        }
                        else
                        {
                            pixels[(i * width + j) * 4 + 0] = 255;
                            pixels[(i * width + j) * 4 + 1] = 255;
                            pixels[(i * width + j) * 4 + 2] = 255;
                            pixels[(i * width + j) * 4 + 3] = 0;
                        }
                    }
                }
            }

            return pixels;
        }

        public bool isConnected()
        {
            return connected;
        }

        public Bitmap getDepthImage()
        {
            return depthImage;
        }

        public Bitmap getColorImage()
        {
            return colorImage;
        }
    }
}
