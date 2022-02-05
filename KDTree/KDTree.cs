// <copyright file="KDTree.cs" company="Eric Regina">
// Copyright (c) Eric Regina. All rights reserved.
// </copyright>

namespace Supercluster.KDTree
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    using Supercluster.KDTree.Utilities;
    using static Utilities.BinaryTreeNavigation;

    /// <summary>
    /// Represents a KD-Tree. KD-Trees are used for fast spatial searches. Searching in a
    /// balanced KD-Tree is O(log n) where linear search is O(n). Points in the KD-Tree are
    /// equi-length arrays of type <typeparamref name="TDimension"/>. The node objects associated
    /// with the points is an array of type <typeparamref name="TNode"/>.
    /// </summary>
    /// <remarks>
    /// KDTrees can be fairly difficult to understand at first. The following references helped me
    /// understand what exactly a KDTree is doing and the contain the best descriptions of searches in a KDTree.
    /// Samet's book is the best reference of multidimensional data structures I have ever seen. Wikipedia is also a good starting place.
    /// References:
    /// <ul style="list-style-type:none">
    /// <li> <a href="http://store.elsevier.com/product.jsp?isbn=9780123694461">Foundations of Multidimensional and Metric Data Structures, 1st Edition, by Hanan Samet. ISBN: 9780123694461</a> </li>
    /// <li> <a href="https://en.wikipedia.org/wiki/K-d_tree"> https://en.wikipedia.org/wiki/K-d_tree</a> </li>
    /// </ul>
    /// </remarks>
    /// <typeparam name="TNode">The type representing the actual node objects.</typeparam>
    [Serializable]
    public class KDTree<TNode>
    {
        /// <summary>
        /// The number of points in the KDTree
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// The numbers of dimensions that the tree has.
        /// </summary>
        public const int Dimensions = 3;

        /// <summary>
        /// The array in which the binary tree is stored. Enumerating this array is a level-order traversal of the tree.
        /// </summary>
        public Vector3[] InternalPointArray { get; }

        /// <summary>
        /// The array in which the node objects are stored. There is a one-to-one correspondence with this array and the <see cref="InternalPointArray"/>.
        /// </summary>
        public TNode[] InternalNodeArray { get; }

        /// <summary>
        /// Gets a <see cref="BinaryTreeNavigator{TPoint,TNode}"/> that allows for manual tree navigation,
        /// </summary>
        public BinaryTreeNavigator<Vector3, TNode> Navigator
            => new BinaryTreeNavigator<Vector3, TNode>(this.InternalPointArray, this.InternalNodeArray);

        /// <summary>
        /// Initializes a new instance of the <see cref="KDTree{TDimension,TNode}"/> class.
        /// </summary>
        /// <param name="points">The points to be constructed into a <see cref="KDTree{TDimension,TNode}"/></param>
        /// <param name="nodes">The nodes associated with each point.</param>
        public KDTree(
            Vector3[] points,
            TNode[] nodes)
        {

            // Calculate the number of nodes needed to contain the binary tree.
            // This is equivalent to finding the power of 2 greater than the number of points
            var elementCount = (int)Math.Pow(2, (int)(Math.Log(points.Length) / Math.Log(2)) + 1);
            this.InternalPointArray = new Vector3[elementCount];
            this.InternalNodeArray = new TNode[elementCount];
            this.Count = points.Length;
            this.GenerateTree(0, 0, points, nodes);
        }

        /// <summary>
        /// Finds the nearest neighbors in the <see cref="KDTree{TNode}"/> of the given <paramref name="point"/>.
        /// </summary>
        /// <param name="point">The point whose neighbors we search for.</param>
        /// <param name="neighbors">The number of neighbors to look for.</param>
        /// <returns>The</returns>
        public Tuple<Vector3, TNode>[] NearestNeighbors(Vector3 point, int neighbors)
        {
            var nearestNeighborList = new BoundedPriorityList<int, float>(neighbors, true);
            var rect = HyperRect.Infinite();
            this.SearchForNearestNeighbors(0, point, rect, 0, nearestNeighborList, float.MaxValue);

            return nearestNeighborList.ToResultSet(this);
        }

        /// <summary>
        /// Searches for the closest points in a hyper-sphere around the given center.
        /// </summary>
        /// <param name="center">The center of the hyper-sphere</param>
        /// <param name="radius">The radius of the hyper-sphere</param>
        /// <param name="neighboors">The number of neighbors to return.</param>
        /// <returns>The specified number of closest points in the hyper-sphere</returns>
        public Tuple<Vector3, TNode>[] RadialSearch(Vector3 center, float radius, int neighboors = -1)
        {
            var nearestNeighbors = new BoundedPriorityList<int, float>(this.Count);
            if (neighboors == -1)
            {
                this.SearchForNearestNeighbors(
                    0,
                    center,
                    HyperRect.Infinite(),
                    0,
                    nearestNeighbors,
                    radius);
            }
            else
            {
                this.SearchForNearestNeighbors(
                    0,
                    center,
                    HyperRect.Infinite(),
                    0,
                    nearestNeighbors,
                    radius);
            }

            return nearestNeighbors.ToResultSet(this);
        }

        /// <summary>
        /// Grows a KD tree recursively via median splitting. We find the median by doing a full sort.
        /// </summary>
        /// <param name="index">The array index for the current node.</param>
        /// <param name="dim">The current splitting dimension.</param>
        /// <param name="points">The set of points remaining to be added to the kd-tree</param>
        /// <param name="nodes">The set of nodes RE</param>
        private void GenerateTree(
            int index,
            int dim,
            IReadOnlyCollection<Vector3> points,
            IEnumerable<TNode> nodes)
        {
            // See wikipedia for a good explanation kd-tree construction.
            // https://en.wikipedia.org/wiki/K-d_tree

            // zip both lists so we can sort nodes according to points
            var zippedList = points.Zip(nodes, (p, n) => new { Point = p, Node = n });

            // sort the points along the current dimension
            var sortedPoints = zippedList.OrderBy(z => z.Point.Dim(dim)).ToArray();

            // get the point which has the median value of the current dimension.
            var medianPoint = sortedPoints[points.Count / 2];
            var medianPointIdx = sortedPoints.Length / 2;

            // The point with the median value all the current dimension now becomes the value of the current tree node
            // The previous node becomes the parents of the current node.
            this.InternalPointArray[index] = medianPoint.Point;
            this.InternalNodeArray[index] = medianPoint.Node;

            // We now split the sorted points into 2 groups
            // 1st group: points before the median
            var leftPoints = new Vector3[medianPointIdx];
            var leftNodes = new TNode[medianPointIdx];

            for (int i = 0; i < leftPoints.Length; i++)
            {
                leftPoints[i] = sortedPoints[i].Point;
            }

            for (int i = 0; i < leftNodes.Length; i++)
            {
                leftNodes[i] = sortedPoints[i].Node;
            }

            // 2nd group: Points after the median
            var rightPoints = new Vector3[sortedPoints.Length - (medianPointIdx + 1)];
            var rightNodes = new TNode[sortedPoints.Length - (medianPointIdx + 1)];

            for (int i = 0; i < rightPoints.Length; i++)
            {
                rightPoints[i] = sortedPoints[i + medianPointIdx + 1].Point;
            }

            for (int i = 0; i < rightNodes.Length; i++)
            {
                rightNodes[i] = sortedPoints[i + medianPointIdx + 1].Node;
            }

            // We new recurse, passing the left and right arrays for arguments.
            // The current node's left and right values become the "roots" for
            // each recursion call. We also forward cycle to the next dimension.
            var nextDim = (dim + 1) % Dimensions; // select next dimension

            // We only need to recurse if the point array contains more than one point
            // If the array has no points then the node stay a null value
            if (leftPoints.Length <= 1)
            {
                if (leftPoints.Length == 1)
                {
                    this.InternalPointArray[LeftChildIndex(index)] = leftPoints[0];
                    this.InternalNodeArray[LeftChildIndex(index)] = leftNodes[0];
                }
            }
            else
            {
                this.GenerateTree(LeftChildIndex(index), nextDim, leftPoints, leftNodes);
            }

            // Do the same for the right points
            if (rightPoints.Length <= 1)
            {
                if (rightPoints.Length == 1)
                {
                    this.InternalPointArray[RightChildIndex(index)] = rightPoints[0];
                    this.InternalNodeArray[RightChildIndex(index)] = rightNodes[0];
                }
            }
            else
            {
                this.GenerateTree(RightChildIndex(index), nextDim, rightPoints, rightNodes);
            }
        }

        /// <summary>
        /// A top-down recursive method to find the nearest neighbors of a given point.
        /// </summary>
        /// <param name="nodeIndex">The index of the node for the current recursion branch.</param>
        /// <param name="target">The point whose neighbors we are trying to find.</param>
        /// <param name="rect">The <see cref="HyperRect"/> containing the possible nearest neighbors.</param>
        /// <param name="dimension">The current splitting dimension for this recursion branch.</param>
        /// <param name="nearestNeighbors">The <see cref="BoundedPriorityList{TElement,TPriority}"/> containing the nearest neighbors already discovered.</param>
        /// <param name="maxSearchRadiusSquared">The squared radius of the current largest distance to search from the <paramref name="target"/></param>
        private void SearchForNearestNeighbors(
            int nodeIndex,
            Vector3 target,
            HyperRect rect,
            int dimension,
            BoundedPriorityList<int, float> nearestNeighbors,
            float maxSearchRadiusSquared)
        {
            if (this.InternalPointArray.Length <= nodeIndex || nodeIndex < 0)
            {
                return;
            }

            // Work out the current dimension
            var dim = dimension % Dimensions;

            // Split our hyper-rectangle into 2 sub rectangles along the current
            // node's point on the current dimension
            var leftRect = rect.Clone();
            leftRect.MaxPoint.DimRef(dim) = this.InternalPointArray[nodeIndex].DimRef(dim);

            var rightRect = rect.Clone();
            rightRect.MinPoint.DimRef(dim) = this.InternalPointArray[nodeIndex].DimRef(dim);

            // Determine which side the target resides in
            var compare = target.DimRef(dim).CompareTo(this.InternalPointArray[nodeIndex].DimRef(dim));

            var nearerRect = compare <= 0 ? leftRect : rightRect;
            var furtherRect = compare <= 0 ? rightRect : leftRect;

            var nearerNode = compare <= 0 ? LeftChildIndex(nodeIndex) : RightChildIndex(nodeIndex);
            var furtherNode = compare <= 0 ? RightChildIndex(nodeIndex) : LeftChildIndex(nodeIndex);

            // Move down into the nearer branch
            this.SearchForNearestNeighbors(
                nearerNode,
                target,
                nearerRect,
                dimension + 1,
                nearestNeighbors,
                maxSearchRadiusSquared);

            // Walk down into the further branch but only if our capacity hasn't been reached
            // OR if there's a region in the further rectangle that's closer to the target than our
            // current furtherest nearest neighbor
            var closestPointInFurtherRect = furtherRect.GetClosestPoint(target);
            float distanceSquaredToTarget = Vector3.DistanceSquared(closestPointInFurtherRect, target);

            if (distanceSquaredToTarget.CompareTo(maxSearchRadiusSquared) <= 0)
            {
                if (nearestNeighbors.IsFull)
                {
                    if (distanceSquaredToTarget.CompareTo(nearestNeighbors.MaxPriority) < 0)
                    {
                        this.SearchForNearestNeighbors(
                            furtherNode,
                            target,
                            furtherRect,
                            dimension + 1,
                            nearestNeighbors,
                            maxSearchRadiusSquared);
                    }
                }
                else
                {
                    this.SearchForNearestNeighbors(
                        furtherNode,
                        target,
                        furtherRect,
                        dimension + 1,
                        nearestNeighbors,
                        maxSearchRadiusSquared);
                }
            }

            // Try to add the current node to our nearest neighbors list
            distanceSquaredToTarget = Vector3.DistanceSquared(this.InternalPointArray[nodeIndex], target);
            if (distanceSquaredToTarget.CompareTo(maxSearchRadiusSquared) <= 0)
            {
                nearestNeighbors.Add(nodeIndex, distanceSquaredToTarget);
            }
        }
    }
}
