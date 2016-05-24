using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DTWGestureRecognition.HandDetection
{
    public class PointFT
    {
        public int X { get; set; }
        public int Y { get; set; }

        /// <summary>
        /// Point (0, 0)
        /// </summary>
        public static PointFT Zero { get { return new PointFT(0, 0); } }
        /// <summary>
        /// Point (1, 1)
        /// </summary>
        public static PointFT One { get { return new PointFT(1, 1); } }

        /// <summary>
        /// Initializes a new instance of PointFT with every variable equals to 0.
        /// </summary>
        public PointFT()
        {
            this.X = 0;
            this.Y = 0;
        }

        /// <summary>
        /// Initializes a new instance of PointFT.
        /// </summary>
        /// <param name="X">Value of the x-param of the point.</param>
        /// <param name="Y">Value of the y-param of the point.</param>
        public PointFT(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }

        /// <summary>
        /// Copy and create a new instance of PointFT.
        /// </summary>
        /// <param name="other">The PointFT to copy.</param>
        public PointFT(PointFT other)
        {
            this.X = other.X;
            this.Y = other.Y;
        }

        /// <summary>
        /// Adds two points.
        /// </summary>
        /// <param name="point1">Source point.</param>
        /// <param name="point2">Source point.</param>
        /// <returns>Returns the sum.</returns>
        public static PointFT add(PointFT point1, PointFT point2)
        {
            PointFT sol = new PointFT();
            sol.X = point1.X + point2.X;
            sol.Y = point1.Y + point2.Y;
            return sol;
        }

        /// <summary>
        /// Adds a scalar to all the coordinates of the source point.
        /// </summary>
        /// <param name="point1">Source point.</param>
        /// <param name="value">The scalar to add.</param>
        /// <returns>Returs the sum.</returns>
        public static PointFT add(PointFT point1, int value)
        {
            PointFT sol = new PointFT();
            sol.X = point1.X + value;
            sol.Y = point1.Y + value;
            return sol;
        }

        /// <summary>
        /// Subtracts two points.
        /// </summary>
        /// <param name="point1">Minuend point.</param>
        /// <param name="point2">Subtrahend point.</param>
        /// <returns>Returns the difference .</returns>
        public static PointFT subtract(PointFT point1, PointFT point2)
        {
            PointFT sol = new PointFT();
            sol.X = point1.X - point2.X;
            sol.Y = point1.Y - point2.Y;
            return sol;
        }

        /// <summary>
        /// Subtracts a scalar to all the coordinates of the source point.
        /// </summary>
        /// <param name="point1">Source point.</param>
        /// <param name="value">The scalar to substract.</param>
        /// <returns>Returs the difference.</returns>
        public static PointFT subtract(PointFT point1, int value)
        {
            PointFT sol = new PointFT();
            sol.X = point1.X - value;
            sol.Y = point1.Y - value;
            return sol;
        }

        /// <summary>
        /// Multiplies two points.
        /// </summary>
        /// <param name="point1">Source point.</param>
        /// <param name="point2">Source point.</param>
        /// <returns>Returns the product.</returns>
        public static PointFT multiply(PointFT point1, PointFT point2)
        {
            PointFT sol = new PointFT();
            sol.X = point1.X * point2.X;
            sol.Y = point1.Y * point2.Y;
            return sol;
        }

        /// <summary>
        /// Multiplies the source point by a scalar.
        /// </summary>
        /// <param name="point1">Source point.</param>
        /// <param name="value">The factor.</param>
        /// <returns>Returns the product.</returns>
        public static PointFT multiply(PointFT point1, int value)
        {
            PointFT sol = new PointFT();
            sol.X = point1.X * value;
            sol.Y = point1.Y * value;
            return sol;
        }

        /// <summary>
        /// Divides two points.
        /// </summary>
        /// <param name="point1">The dividend.</param>
        /// <param name="point2">The divisor.</param>
        /// <returns>Returns the division.</returns>
        public static PointFT divide(PointFT point1, PointFT point2)
        {
            PointFT sol = new PointFT();
            sol.X = point1.X / point2.X;
            sol.Y = point1.Y / point2.Y;
            return sol;
        }

        /// <summary>
        /// Divides the source point by an scalar.
        /// </summary>
        /// <param name="point1">Source point.</param>
        /// <param name="value">The divisor.</param>
        /// <returns>Returns the division.</returns>
        public static PointFT divide(PointFT point1, int value)
        {
            PointFT sol = new PointFT();
            sol.X = point1.X / value;
            sol.Y = point1.Y / value;
            return sol;
        }

        /// <summary>
        /// Returns the length of the source point.
        /// </summary>
        /// <returns>The length of the point.</returns>
        public static float length(PointFT point1)
        {
            return (float)Math.Sqrt(point1.X * point1.X + point1.Y * point1.Y);
        }

        /// <summary>
        /// Calculate the number of points you have to cross to go from one point to another.
        /// </summary>
        /// <param name="point1">Source point.</param>
        /// <param name="point2">Source point.</param>
        /// <returns>Returns the distance between the points.</returns>
        public static int distance(PointFT point1, PointFT point2)
        {
            return Math.Abs(point1.X - point2.X) + Math.Abs(point1.Y - point2.Y);
        }

        /// <summary>
        /// Calculates the Euclidean distance between two points.
        /// </summary>
        /// <param name="point1">Source point.</param>
        /// <param name="point2">Source point.</param>
        /// <returns>Returns the distance between the points.</returns>
        public static float distanceEuclidean(PointFT point1, PointFT point2)
        {
            return (float)Math.Sqrt((point1.X - point2.X) * (point1.X - point2.X) + (point1.Y - point2.Y) * (point1.Y - point2.Y));
        }

        /// <summary>
        /// Calculates the Euclidean distance between two points.
        /// </summary>
        /// <param name="point1">Source point.</param>
        /// <param name="point2">Source point.</param>
        /// <returns>Returns the distance between the points squared.</returns>
        public static float distanceEuclideanSquared(PointFT point1, PointFT point2)
        {
            return (point1.X - point2.X) * (point1.X - point2.X) + (point1.Y - point2.Y) * (point1.Y - point2.Y);
        }

        /// <summary>
        /// Calculates the dot product of two points. Returns a floating point value between -1 and 1.
        /// </summary>
        /// <param name="point1">Source point.</param>
        /// <param name="point2">Source point.</param>
        /// <returns>Returns the dot product of the source points.</returns>
        public static float dot(PointFT point1, PointFT point2)
        {
            return (point1.X * point2.X + point1.Y * point2.Y) / (length(point1) * length(point2));
        }

        /// <summary>
        /// Calculates the smaller angle between two points.
        /// </summary>
        /// <param name="point1">Source point.</param>
        /// <param name="point2">Source point.</param>
        /// <returns>Returns the angle in radians.</returns>
        public static float angle(PointFT point1, PointFT point2)
        {
            return (float)Math.Acos((point1.X * point2.X + point1.Y * point2.Y) / (length(point1) * length(point2)));
        }

        /// <summary>
        /// Checks if two points are equals.
        /// </summary>
        /// <param name="obj">The point to compare with.</param>
        /// <returns>Returns true if the points are equals, or false otherwise.</returns>
        public override bool Equals(object obj)
        {
            PointFT other = (PointFT)obj;
            if (this.X == other.X && this.Y == other.Y)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /**** Operators *****/

        /// <summary>
        /// Adds two points.
        /// </summary>
        /// <param name="point1">Source point.</param>
        /// <param name="point2">Source point.</param>
        /// <returns>Returns the sum.</returns>
        public static PointFT operator +(PointFT point1, PointFT point2)
        {
            PointFT sol = new PointFT();
            sol.X = point1.X + point2.X;
            sol.Y = point1.Y + point2.Y;
            return sol;
        }

        /// <summary>
        /// Adds a scalar to all the coordinates of the source point.
        /// </summary>
        /// <param name="point1">Source point.</param>
        /// <param name="value">The scalar to add.</param>
        /// <returns>Returs the sum.</returns>
        public static PointFT operator +(PointFT point1, int value)
        {
            PointFT sol = new PointFT();
            sol.X = point1.X + value;
            sol.Y = point1.Y + value;
            return sol;
        }

        /// <summary>
        /// Subtracts two points.
        /// </summary>
        /// <param name="point1">Minuend point.</param>
        /// <param name="point2">Subtrahend point.</param>
        /// <returns>Returns the difference .</returns>
        public static PointFT operator -(PointFT point1, PointFT point2)
        {
            PointFT sol = new PointFT();
            sol.X = point1.X - point2.X;
            sol.Y = point1.Y - point2.Y;
            return sol;
        }

        /// <summary>
        /// Subtracts a scalar to all the coordinates of the source point.
        /// </summary>
        /// <param name="point1">Source point.</param>
        /// <param name="value">The scalar to substract.</param>
        /// <returns>Returs the difference.</returns>
        public static PointFT operator -(PointFT point1, int value)
        {
            PointFT sol = new PointFT();
            sol.X = point1.X - value;
            sol.Y = point1.Y - value;
            return sol;
        }

        /// <summary>
        /// Multiplies two points.
        /// </summary>
        /// <param name="point1">Source point.</param>
        /// <param name="point2">Source point.</param>
        /// <returns>Returns the product.</returns>
        public static PointFT operator *(PointFT point1, PointFT point2)
        {
            PointFT sol = new PointFT();
            sol.X = point1.X * point2.X;
            sol.Y = point1.Y * point2.Y;
            return sol;
        }

        /// <summary>
        /// Multiplies the source point by a scalar.
        /// </summary>
        /// <param name="point1">Source point.</param>
        /// <param name="value">The factor.</param>
        /// <returns>Returns the product.</returns>
        public static PointFT operator *(PointFT point1, int value)
        {
            PointFT sol = new PointFT();
            sol.X = point1.X * value;
            sol.Y = point1.Y * value;
            return sol;
        }

        /// <summary>
        /// Divides two points.
        /// </summary>
        /// <param name="point1">The dividend.</param>
        /// <param name="point2">The divisor.</param>
        /// <returns>Returns the division.</returns>
        public static PointFT operator /(PointFT point1, PointFT point2)
        {
            PointFT sol = new PointFT();
            sol.X = point1.X / point2.X;
            sol.Y = point1.Y / point2.Y;
            return sol;
        }

        /// <summary>
        /// Divides the source point by an scalar.
        /// </summary>
        /// <param name="point1">Source point.</param>
        /// <param name="value">The divisor.</param>
        /// <returns>Returns the division.</returns>
        public static PointFT operator /(PointFT point1, int value)
        {
            PointFT sol = new PointFT();
            sol.X = point1.X / value;
            sol.Y = point1.Y / value;
            return sol;
        }

        /**** Override ****/

        /// <summary>
        /// Compare two points.
        /// </summary>
        /// <param name="point1">Source point</param>
        /// <param name="point2">Source point</param>
        /// <returns>Return true if the source points are different, and false otherwise.</returns>
        public static bool operator !=(PointFT point1, PointFT point2)
        {
            return !point1.Equals(point2);
        }

        /// <summary>
        /// Compare two points.
        /// </summary>
        /// <param name="point1">Source point</param>
        /// <param name="point2">Source point</param>
        /// <returns>Return true if the source points are equal, and false otherwise.</returns>
        public static bool operator ==(PointFT point1, PointFT point2)
        {
            return point1.Equals(point2);
        }

        /// <summary>
        /// Calculate the has code for the current object.
        /// </summary>
        /// <returns>Returns the hash code.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Retrieves a string representation of the current point.
        /// </summary>
        /// <returns>String that represents the point.</returns>
        public override string ToString()
        {
            return "(" + X + ", " + Y + ")";
        }
    }
}
