using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DTWGestureRecognition.HandDetection
{
    public class Hand
    {

        public PointFT palm { get; set; }
        public List<PointFT> fingertips { get; set; }

        public Vector3FT palm3D;
        public List<Vector3FT> fingertips3D { get; set; }

        public List<PointFT> contour { get; set; }
        public List<PointFT> inside { get; set; }

        public PointFT leftUpperCorner { get; set; }
        public PointFT rightDownCorner { get; set; }

        public Hand()
        {
            palm = new PointFT(-1, -1);

            fingertips = new List<PointFT>(5);
            fingertips3D = new List<Vector3FT>(5);

            contour = new List<PointFT>(2000);
            inside = new List<PointFT>(6000);

            leftUpperCorner = new PointFT(int.MaxValue, int.MaxValue);
            rightDownCorner = new PointFT(int.MinValue, int.MinValue);
        }

        // Check if a point is inside the hand countour box
        public bool isPointInsideContainerBox(PointFT p)
        {
            if (p.X < rightDownCorner.X && p.X > leftUpperCorner.X
                && p.Y > leftUpperCorner.Y && p.Y < rightDownCorner.Y)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool isCircleInsideContainerBox(PointFT p, float r)
        {
            if (leftUpperCorner.X > p.X - r)
            {
                return false;
            }
            if (rightDownCorner.X < p.X + r)
            {
                return false;
            }
            if (leftUpperCorner.Y > p.Y - r)
            {
                return false;
            }
            if (rightDownCorner.Y < p.Y + r)
            {
                return false;
            }

            return true;
        }

        // Calculate the contour box of the hand if it possible
        public bool calculateContainerBox(float reduction = 0)
        {
            if (contour != null && contour.Count > 0)
            {
                for (int j = 0; j < contour.Count; ++j)
                {
                    if (leftUpperCorner.X > contour[j].X)
                        leftUpperCorner.X = contour[j].X;

                    if (rightDownCorner.X < contour[j].X)
                        rightDownCorner.X = contour[j].X;

                    if (leftUpperCorner.Y > contour[j].Y)
                        leftUpperCorner.Y = contour[j].Y;

                    if (rightDownCorner.Y < contour[j].Y)
                        rightDownCorner.Y = contour[j].Y;
                }

                int incX = (int)((rightDownCorner.X - leftUpperCorner.X) * (reduction / 2));
                int incY = (int)((rightDownCorner.Y - leftUpperCorner.Y) * (reduction / 2));
                PointFT inc = new PointFT(incX, incY);
                leftUpperCorner = leftUpperCorner + inc;
                rightDownCorner = rightDownCorner + inc;

                return true;
            }
            else
            {
                return false;
            }
        }

        // Check if the hand is open
        public bool isOpen()
        {
            if (fingertips.Count == 5)
                return true;
            else
                return false;
        }

        // Check if the hand is close
        public bool isClose()
        {
            if (fingertips.Count == 0)
                return true;
            else
                return false;
        }

        // Obtain the 3D normalized point and add it to the list of fingertips
        public void calculate3DPoints(int width, int height, int[] distance)
        {
            if (palm.X != -1 && palm.Y != -1)
                palm3D = transformTo3DCoord(palm, width, height, distance[palm.X * width + palm.Y]);

            fingertips3D.Clear();
            for (int i = 0; i < fingertips.Count; ++i)
            {
                PointFT f = fingertips[i];
                fingertips3D.Add(transformTo3DCoord(f, width, height, distance[f.X * width + f.Y]));
            }
        }

        // Normalize in the interval [-1, 1] the given 3D point.
        // The Z value is in the inverval [-1, 0], being 0 the closest distance.
        private Vector3FT transformTo3DCoord(PointFT p, int width, int height, int distance)
        {
            Vector3FT v = new Vector3FT();
            v.X = p.Y / (width * 1.0f) * 2 - 1;
            v.Y = (1 - p.X / (height * 1.0f)) * 2 - 1;
            v.Z = (distance - 400) / -7600.0f;
            return v;
        }
    }
}
