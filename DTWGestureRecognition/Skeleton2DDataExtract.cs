

namespace DTWGestureRecognition
{
    using System;
    using System.Windows;
    using Microsoft.Kinect;
    using System.Windows.Media.Media3D;

    /// <summary>
    /// This class is used to transform the data of the skeleton
    /// </summary>
    internal class Skeleton2DDataExtract
    {
        /// <summary>
        /// Skeleton2DdataCoordEventHandler delegate
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="a">Skeleton 2Ddata Coord Event Args</param>
        public delegate void Skeleton2DdataCoordEventHandler(object sender, Skeleton2DdataCoordEventArgs a);

        /// <summary>
        /// The Skeleton 2Ddata Coord Ready event
        /// </summary>
        public static event Skeleton2DdataCoordEventHandler Skeleton2DdataCoordReady;

        /// <summary>
        /// Crunches Kinect SDK's Skeleton Data and spits out a format more useful for DTW
        /// </summary>
        /// <param name="data">Kinect SDK's Skeleton Data</param>
        public static void ProcessData(Skeleton data)
        {
            // Extract the coordinates of the points.
            var p = new Point3D[6];
            Point3D shoulderRight = new Point3D(), shoulderLeft = new Point3D();
            if (data != null)
            {
                foreach (Joint j in data.Joints)
                {
                    switch (j.JointType)
                    {
                        case JointType.HandLeft:
                            p[0] = new Point3D(j.Position.X, j.Position.Y,j.Position.Z);
                            break;
                        case JointType.WristLeft:
                            p[1] = new Point3D(j.Position.X, j.Position.Y, j.Position.Z);
                            break;
                        case JointType.ElbowLeft:
                            p[2] = new Point3D(j.Position.X, j.Position.Y, j.Position.Z);
                            break;
                        case JointType.ElbowRight:
                            p[3] = new Point3D(j.Position.X, j.Position.Y, j.Position.Z);
                            break;
                        case JointType.WristRight:
                            p[4] = new Point3D(j.Position.X, j.Position.Y, j.Position.Z);
                            break;
                        case JointType.HandRight:
                            p[5] = new Point3D(j.Position.X, j.Position.Y, j.Position.Z);
                            break;
                        case JointType.ShoulderLeft:
                            shoulderLeft = new Point3D(j.Position.X, j.Position.Y, j.Position.Z);
                            break;
                        case JointType.ShoulderRight:
                            shoulderRight = new Point3D(j.Position.X, j.Position.Y, j.Position.Z);
                            break;
                    }
                }
                // Centre the data
                var center = new Point3D((shoulderLeft.X + shoulderRight.X) / 2, (shoulderLeft.Y + shoulderRight.Y) / 2, (shoulderLeft.Z + shoulderRight.Z) / 2);
                for (int i = 0; i < 6; i++)
                {
                    p[i].X -= center.X;
                    p[i].Y -= center.Y;
                    p[i].Z -= center.Z;
                }
                // Normalization of the coordinates
                double shoulderDist =
                    Math.Sqrt(Math.Pow((shoulderLeft.X - shoulderRight.X), 2) +
                              Math.Pow((shoulderLeft.Y - shoulderRight.Y), 2) +
                              Math.Pow((shoulderLeft.Z - shoulderRight.Z), 2));
                for (int i = 0; i < 6; i++)
                {
                    p[i].X /= shoulderDist;
                    p[i].Y /= shoulderDist;
                    p[i].Z /= shoulderDist;
                }
                // Launch the event!
                Skeleton2DdataCoordReady(null, new Skeleton2DdataCoordEventArgs(p));

            }
        }
    }
}