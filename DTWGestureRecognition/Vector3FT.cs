using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DTWGestureRecognition.HandDetection
{
    public class Vector3FT
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        /// <summary>
        /// Vector (0, 0, 0)
        /// </summary>
        public static Vector3FT Zero { get { return new Vector3FT(0, 0, 0); } }
        /// <summary>
        /// Vector (1, 1, 1)
        /// </summary>
        public static Vector3FT One { get { return new Vector3FT(1, 1, 1); } }

        /// <summary>
        /// Initializes a new instance of Vector3FT with every variable equals to 0.
        /// </summary>
        public Vector3FT()
        {
            this.X = 0;
            this.Y = 0;
            this.Z = 0;
        }

        /// <summary>
        /// Initializes a new instance of Vector3FT.
        /// </summary>
        /// <param name="X">Value of the x-param of the vector.</param>
        /// <param name="Y">Value of the y-param of the vector.</param>
        /// <param name="Z">Value of the z-param of the vector.</param>
        public Vector3FT(float X, float Y, float Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        /// <summary>
        /// Copy and create a new instance of Vector3FT.
        /// </summary>
        /// <param name="other">The Vector3FT to copy.</param>
        public Vector3FT(Vector3FT other)
        {
            this.X = other.X;
            this.Y = other.Y;
            this.Z = other.Z;
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="vector1">Source vector.</param>
        /// <param name="vector2">Source vector.</param>
        /// <returns>Returns the sum.</returns>
        public static Vector3FT add(Vector3FT vector1, Vector3FT vector2)
        {
            Vector3FT sol = new Vector3FT();
            sol.X = vector1.X + vector2.X;
            sol.Y = vector1.Y + vector2.Y;
            sol.Z = vector1.Z + vector2.Z;
            return sol;
        }

        /// <summary>
        /// Adds a scalar to all the coordinates of the source vector.
        /// </summary>
        /// <param name="vector1">Source vector.</param>
        /// <param name="value">The scalar to add.</param>
        /// <returns>Returs the sum.</returns>
        public static Vector3FT add(Vector3FT vector1, float value)
        {
            Vector3FT sol = new Vector3FT();
            sol.X = vector1.X + value;
            sol.Y = vector1.Y + value;
            sol.Z = vector1.Z + value;
            return sol;
        }

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="vector1">Minuend vector.</param>
        /// <param name="vector2">Subtrahend vector.</param>
        /// <returns>Returns the difference .</returns>
        public static Vector3FT subtract(Vector3FT vector1, Vector3FT vector2)
        {
            Vector3FT sol = new Vector3FT();
            sol.X = vector1.X - vector2.X;
            sol.Y = vector1.Y - vector2.Y;
            sol.Z = vector1.Z - vector2.Z;
            return sol;
        }

        /// <summary>
        /// Subtracts a scalar to all the coordinates of the source vector.
        /// </summary>
        /// <param name="vector1">Source vector.</param>
        /// <param name="value">The scalar to substract.</param>
        /// <returns>Returs the difference.</returns>
        public static Vector3FT subtract(Vector3FT vector1, float value)
        {
            Vector3FT sol = new Vector3FT();
            sol.X = vector1.X - value;
            sol.Y = vector1.Y - value;
            sol.Z = vector1.Z - value;
            return sol;
        }

        /// <summary>
        /// Multiplies two vectors.
        /// </summary>
        /// <param name="vector1">Source vector.</param>
        /// <param name="vector2">Source vector.</param>
        /// <returns>Returns the product.</returns>
        public static Vector3FT multiply(Vector3FT vector1, Vector3FT vector2)
        {
            Vector3FT sol = new Vector3FT();
            sol.X = vector1.X * vector2.X;
            sol.Y = vector1.Y * vector2.Y;
            sol.Z = vector1.Z * vector2.Z;
            return sol;
        }

        /// <summary>
        /// Multiplies the source vector by a scalar.
        /// </summary>
        /// <param name="vector1">Source vector.</param>
        /// <param name="value">The factor.</param>
        /// <returns>Returns the product.</returns>
        public static Vector3FT multiply(Vector3FT vector1, float value)
        {
            Vector3FT sol = new Vector3FT();
            sol.X = vector1.X * value;
            sol.Y = vector1.Y * value;
            sol.Z = vector1.Z * value;
            return sol;
        }

        /// <summary>
        /// Divides two vectors.
        /// </summary>
        /// <param name="vector1">The dividend.</param>
        /// <param name="vector2">The divisor.</param>
        /// <returns>Returns the division.</returns>
        public static Vector3FT divide(Vector3FT vector1, Vector3FT vector2)
        {
            Vector3FT sol = new Vector3FT();
            sol.X = vector1.X / vector2.X;
            sol.Y = vector1.Y / vector2.Y;
            sol.Z = vector1.Z / vector2.Z;
            return sol;
        }

        /// <summary>
        /// Divides the source vector by an scalar.
        /// </summary>
        /// <param name="vector1">Source vector.</param>
        /// <param name="value">The divisor.</param>
        /// <returns>Returns the division.</returns>
        public static Vector3FT divide(Vector3FT vector1, float value)
        {
            Vector3FT sol = new Vector3FT();
            sol.X = vector1.X / value;
            sol.Y = vector1.Y / value;
            sol.Z = vector1.Z / value;
            return sol;
        }

        /// <summary>
        /// Returns the length of the source vector.
        /// </summary>
        /// <returns>The length of the vector.</returns>
        public static float length(Vector3FT vector1)
        {
            return (float)Math.Sqrt(vector1.X * vector1.X + vector1.Y * vector1.Y + vector1.Z * vector1.Z);
        }

        /// <summary>
        /// Creates a unit vector from the specified vector. The result is a vector one unit in length pointing in the same direction as the original vector.
        /// </summary>
        /// <param name="vector1">Source vector.</param>
        /// <returns>The created unit vector.</returns>
        public static Vector3FT normalize(Vector3FT vector1)
        {
            float l = length(vector1);
            return divide(vector1, l);
        }

        /// <summary>
        /// Calculates the distance between two vectors.
        /// </summary>
        /// <param name="vector1">Source vector.</param>
        /// <param name="vector2">Source vector.</param>
        /// <returns>Returns the distance between the vectors.</returns>
        public static float distance(Vector3FT vector1, Vector3FT vector2)
        {
            return (float)Math.Sqrt((vector1.X - vector2.X) * (vector1.X - vector2.X) + (vector1.Y - vector2.Y) * (vector1.Y - vector2.Y) +
                (vector1.Z - vector2.Z) * (vector1.Z - vector2.Z));
        }

        /// <summary>
        /// Calculates the distance between two vectors.
        /// </summary>
        /// <param name="vector1">Source vector.</param>
        /// <param name="vector2">Source vector.</param>
        /// <returns>Returns the distance between the vectors squared.</returns>
        public static float distanceSquared(Vector3FT vector1, Vector3FT vector2)
        {
            return (vector1.X - vector2.X) * (vector1.X - vector2.X) + (vector1.Y - vector2.Y) * (vector1.Y - vector2.Y) +
                (vector1.Z - vector2.Z) * (vector1.Z - vector2.Z);
        }

        /// <summary>
        /// Calculates the dot product of two vectors. Returns a floating point value between -1 and 1.
        /// </summary>
        /// <param name="vector1">Source vector.</param>
        /// <param name="vector2">Source vector.</param>
        /// <returns>Returns the dot product of the source vectors.</returns>
        public static float dot(Vector3FT vector1, Vector3FT vector2)
        {
            return (vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z) / (length(vector1) * length(vector2));
        }

        /// <summary>
        /// Calculates the smaller angle between two vectors.
        /// </summary>
        /// <param name="vector1">Source vector.</param>
        /// <param name="vector2">Source vector.</param>
        /// <returns>Returns the angle in radians.</returns>
        public static float angle(Vector3FT vector1, Vector3FT vector2)
        {
            return (float)Math.Acos((vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z) / (length(vector1) * length(vector2)));
        }

        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="vector2">Source vector.</param>
        /// <returns>Returns the cross product of the source vectors.</returns>
        public static Vector3FT cross(Vector3FT vector1, Vector3FT vector2)
        {
            Vector3FT sol = new Vector3FT();
            sol.X = vector1.Y * vector2.Z - vector1.Z * vector2.Y;
            sol.Y = vector1.Z * vector2.X - vector1.X * vector2.Z;
            sol.Z = vector1.X * vector2.Y - vector1.Y * vector2.X;
            return sol;
        }

        /// <summary>
        /// Checks if two vectors are equals.
        /// </summary>
        /// <param name="obj">The vector to compare with.</param>
        /// <returns>Returns true if the vectors are equals, or false otherwise.</returns>
        public override bool Equals(object obj)
        {
            Vector3FT other = (Vector3FT)obj;
            if (this.X == other.X && this.Y == other.Y && this.Z == other.Z)
            {
                return true;
            }
            else
            {
                return false;
            }
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
        /// Retrieves a string representation of the current vector.
        /// </summary>
        /// <returns>String that represents the vector.</returns>
        public override string ToString()
        {
            return "(" + X + ", " + Y + ", " + Z + ")";
        }
    }
}
