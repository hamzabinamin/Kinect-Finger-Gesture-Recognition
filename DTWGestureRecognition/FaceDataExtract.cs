using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.FaceTracking;
using System.Windows.Media.Media3D;

namespace DTWGestureRecognition
{
    class FaceDataExtract
    {
        private readonly double[] trainedPointsX = {-1,0,1,2,3,4,5,6,7,8};
        private readonly double[] trainedPointsY = {-1,0,1,2,3,4,5,6,7,8};
        private readonly double[] trainedPointsZ = {-1,0,1,2,3,4,5,6,7,8};
        private static readonly string[] labels = { "Face not detected", "Front Facing", "Leaning Left", "Leaning Right"," Down", "Up", "Left", "Right", "Left-up", "Right-up", "Left-Down", "Right-Down" };
        private readonly System.Windows.Point[] _points;
        private readonly System.Windows.Media.Media3D.Point3D[] _points3D;
        public static string getFaceLookingPosition(double x, double y, double z)
        {
            //if x head down
            //if y head right
            //if z head turned to right.
            string label = labels[0];
            if ((x < 20) && (x > -20))
            {
                if ((y < 20) && (y > -20))
                {
                    if ((z < 20) && (z > -20))
                    {
                        label = labels[1];
                    }
                    else
                        if (z > 20)
                        {
                            label = labels[2];
                        }
                    if (z < -20)
                    {
                        label = labels[3];
                    }
                }
                else
                    if (y > 20)
                    {
                        if ((z < 20) && (z > -20))
                        {
                            label = labels[6];
                        }
                        else
                            if (z > 20)
                            {
                                label = labels[6];
                            }
                        if (z < -20)
                        {
                            label = labels[6];
                        }
                    }
                    else
                        if (y < -20)
                        {
                            if ((z < 20) && (z > -20))
                            {
                                label = labels[7];
                            }
                            else
                                if (z > 20)
                                {
                                    label = labels[7];
                                }
                            if (z < -20)
                            {
                                label = labels[7];
                            }
                        }
            }
            else
                if (x > 20)
                {
                    if ((y < 20) && (y > -20))
                    {
                        if ((z < 20) && (z > -20))
                        {
                            label = labels[5];
                        }
                        else
                            if (z > 20)
                            {
                                label = labels[5];
                            }
                        if (z < -20)
                        {
                            label = labels[5];
                        }
                    }
                    else
                        if (y > 20)
                        {
                            if ((z < 20) && (z > -20))
                            {
                                label = labels[8];
                            }
                            else
                                if (z > 20)
                                {
                                    label = labels[8];
                                }
                            if (z < -20)
                            {
                                label = labels[8];
                            }
                        }
                        else
                            if (y < -20)
                            {
                                if ((z < 20) && (z > -20))
                                {
                                    label = labels[9];
                                }
                                else
                                    if (z > 20)
                                    {
                                        label = labels[9];
                                    }
                                if (z < -20)
                                {
                                    label = labels[9];
                                }
                            }
                }else
                    if (x < -20)
                    {
                        if ((y < 20) && (y > -20))
                        {
                            if ((z < 20) && (z > -20))
                            {
                                label = labels[4];
                            }
                            else
                                if (z > 20)
                                {
                                    label = labels[4];
                                }
                            if (z < -20)
                            {
                                label = labels[4];
                            }
                        }
                        else
                            if (y > 20)
                            {
                                if ((z < 20) && (z > -20))
                                {
                                    label = labels[10];
                                }
                                else
                                    if (z > 20)
                                    {
                                        label = labels[10];
                                    }
                                if (z < -20)
                                {
                                    label = labels[10];
                                }
                            }
                            else
                                if (y < -20)
                                {
                                    if ((z < 20) && (z > -20))
                                    {
                                        label = labels[11];
                                    }
                                    else
                                        if (z > 20)
                                        {
                                            label = labels[11];
                                        }
                                    if (z < -20)
                                    {
                                        label = labels[11];
                                    }
                                }
                    }


            return label;
        }
        public static double[] ProcessData(double x, double y, double z)
        {
            var tmp = new double[3];
            tmp[0] = x/23;
            tmp[1] = y/23;
            tmp[2] = z/23;
            return tmp;
        }
        public static double[] ProcessData(EnumIndexableCollection<AnimationUnit, float> faceAnimationsUnits)
        {
            // Extract the coordinates of the points.    
            var tmp = new double[6];
            if (faceAnimationsUnits != null)
            {
                tmp[0] = faceAnimationsUnits[AnimationUnit.BrowLower];
                tmp[1] = faceAnimationsUnits[AnimationUnit.BrowRaiser];
                tmp[2] = faceAnimationsUnits[AnimationUnit.JawLower];
                tmp[3] = faceAnimationsUnits[AnimationUnit.LipCornerDepressor];
                tmp[4] = faceAnimationsUnits[AnimationUnit.LipRaiser];
                tmp[5] = faceAnimationsUnits[AnimationUnit.LipStretcher];
            }
            return tmp;
        }
        public static double[] ProcessOrientationData()
        {
            var tmp = new double[3];
            tmp[0] = FaceTrackingViewer.x/23;
            tmp[1] = FaceTrackingViewer.y/23;
            tmp[2] = FaceTrackingViewer.z/23;
            return tmp;
        }
    }
}

