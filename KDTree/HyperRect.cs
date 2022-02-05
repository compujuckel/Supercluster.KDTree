// <copyright file="HyperRect.cs" company="Eric Regina">
// Copyright (c) Eric Regina. All rights reserved.
// </copyright>

namespace Supercluster.KDTree
{
    using System.Numerics;

    /// <summary>
    /// Represents a hyper-rectangle. An N-Dimensional rectangle.
    /// </summary>
    public struct HyperRect
    {
        /// <summary>
        /// Backing field for the <see cref="MinPoint"/> property.
        /// </summary>
        public Vector3 MinPoint;

        /// <summary>
        /// Backing field for the <see cref="MaxPoint"/> property.
        /// </summary>
        public Vector3 MaxPoint;

        /// <summary>
        /// Get a hyper rectangle which spans the entire implicit metric space.
        /// </summary>
        /// <returns>The hyper-rectangle which spans the entire metric space.</returns>
        public static HyperRect Infinite()
        {
            var rect = default(HyperRect);

            rect.MinPoint = new Vector3(float.MinValue);
            rect.MaxPoint = new Vector3(float.MaxValue);

            return rect;
        }

        /// <summary>
        /// Gets the point on the rectangle that is closest to the given point.
        /// If the point is within the rectangle, then the input point is the same as the
        /// output point.f the point is outside the rectangle then the point on the rectangle
        /// that is closest to the given point is returned.
        /// </summary>
        /// <param name="toPoint">We try to find a point in or on the rectangle closest to this point.</param>
        /// <returns>The point on or in the rectangle that is closest to the given point.</returns>
        public Vector3 GetClosestPoint(Vector3 toPoint)
        {
            var closest = Vector3.Zero;

            if (this.MinPoint.X.CompareTo(toPoint.X) > 0)
            {
                closest.X = this.MinPoint.X;
            }
            else if (this.MaxPoint.X.CompareTo(toPoint.X) < 0)
            {
                closest.X = this.MaxPoint.X;
            }
            else
            {
                closest.X = toPoint.X;
            }

            if (this.MinPoint.Y.CompareTo(toPoint.Y) > 0)
            {
                closest.Y = this.MinPoint.Y;
            }
            else if (this.MaxPoint.Y.CompareTo(toPoint.Y) < 0)
            {
                closest.Y = this.MaxPoint.Y;
            }
            else
            {
                closest.Y = toPoint.Y;
            }

            if (this.MinPoint.Z.CompareTo(toPoint.Z) > 0)
            {
                closest.Z = this.MinPoint.Z;
            }
            else if (this.MaxPoint.Z.CompareTo(toPoint.Z) < 0)
            {
                closest.Z = this.MaxPoint.Z;
            }
            else
            {
                closest.Z = toPoint.Z;
            }

            return closest;
        }

        /// <summary>
        /// Clones the <see cref="HyperRect"/>.
        /// </summary>
        /// <returns>A clone of the <see cref="HyperRect"/></returns>
        public HyperRect Clone()
        {
            // For a discussion of why we don't implement ICloneable
            // see http://stackoverflow.com/questions/536349/why-no-icloneablet
            var rect = default(HyperRect);
            rect.MinPoint = this.MinPoint;
            rect.MaxPoint = this.MaxPoint;
            return rect;
        }
    }
}
