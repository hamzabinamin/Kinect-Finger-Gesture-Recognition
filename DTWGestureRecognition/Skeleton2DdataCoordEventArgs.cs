

namespace DTWGestureRecognition
{
    using System.Windows;
    using System.Windows.Media.Media3D;

    /// <summary>
    /// Takes Kinect SDK Skeletal Frame coordinates and converts them intoo a format useful to th DTW
    /// </summary>
    internal class Skeleton2DdataCoordEventArgs
    {
        /// <summary>
        /// Positions of the elbows, the wrists and the hands (placed from left to right)
        /// </summary>
        private readonly Point3D[] _points;
        public static Point3D[] _points2;

        /// <summary>
        /// Initializes a new instance of the Skeleton2DdataCoordEventArgs class
        /// </summary>
        /// <param name="points">The points we need to handle in this class</param>
        public Skeleton2DdataCoordEventArgs(Point3D[] points)
        {
            _points = (Point3D[])points.Clone();
            _points2 = (Point3D[])points.Clone();
        }


        /// <summary>
        /// Gets the point at a certain index
        /// </summary>
        /// <param name="index">The index we wish to retrieve</param>
        /// <returns>The point at the sent index</returns>
        public Point3D GetPoint(int index)
        {
            return _points[index];

            
        }

        public static Point3D GetPoint2(int index)
        {
            return _points2[index];


        }

        /// <summary>
        /// Gets the coordinates of our _points
        /// </summary>
        /// <returns>The coordinates of our _points</returns>
        internal double[] GetCoords()
        {
            var tmp = new double[_points.Length * 3];
            for (int i = 0; i < _points.Length; i++)
            {
                tmp[3 * i] = _points[i].X;
                tmp[(3 * i) + 1] = _points[i].Y;
                tmp[(3 * i) + 2] = _points[i].Z;
            }

            return tmp;
        }

        public static double[] GetCoords2()
        {
            var tmp = new double[_points2.Length * 3];
            for (int i = 0; i < _points2.Length; i++)
            {
                tmp[3 * i] = _points2[i].X;
                tmp[(3 * i) + 1] = _points2[i].Y;
                tmp[(3 * i) + 2] = _points2[i].Z;
            }

            return tmp;
        }
    }
}