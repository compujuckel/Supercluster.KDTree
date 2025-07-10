// <copyright file="BoundedPriorityList.cs" company="Eric Regina">
// Copyright (c) Eric Regina. All rights reserved.
// </copyright>

namespace Supercluster.KDTree
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// A list of limited length that remains sorted by <typeparamref name="TPriority"/>.
    /// Useful for keeping track of items in nearest neighbor searches. Insert is O(log n). Retrieval is O(1)
    /// </summary>
    /// <typeparam name="TElement">The type of element the list maintains.</typeparam>
    /// <typeparam name="TPriority">The type the elements are prioritized by.</typeparam>
    public ref struct BoundedPriorityList<TElement, TPriority>
        where TPriority : IComparable<TPriority>
    {
        /// <summary>
        /// The list holding the actual elements
        /// </summary>
        private readonly Span<TElement> elementList;

        /// <summary>
        /// The list of priorities for each element.
        /// There is a one-to-one correspondence between the
        /// priority list and the element list.
        /// </summary>
        private readonly Span<TPriority> priorityList;

        /// <summary>
        /// Gets the element with the largest priority.
        /// </summary>
        public TElement MaxElement => this.elementList[this.Count - 1];

        /// <summary>
        /// Gets the largest priority.
        /// </summary>
        public TPriority MaxPriority => this.priorityList[this.Count - 1];

        /// <summary>
        /// Gets the element with the lowest priority.
        /// </summary>
        public TElement MinElement => this.elementList[0];

        /// <summary>
        /// Gets the smallest priority.
        /// </summary>
        public TPriority MinPriority => this.priorityList[0];

        /// <summary>
        /// Gets the maximum allowed capacity for the <see cref="BoundedPriorityList{TElement,TPriority}"/>
        /// </summary>
        public int Capacity { get; }

        /// <summary>
        /// Returns true if the list is at maximum capacity.
        /// </summary>
        public bool IsFull => this.Count == this.Capacity;

        /// <summary>
        /// Returns the count of items currently in the list.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Indexer for the internal element array.
        /// </summary>
        /// <param name="index">The index in the array.</param>
        /// <returns>The element at the specified index.</returns>
        public TElement this[int index] => this.elementList[index];

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundedPriorityList{TElement, TPriority}"/> struct.
        /// </summary>
        /// <param name="elements">Span of elements</param>
        /// <param name="priorities">Span of priorities</param>
        public BoundedPriorityList(Span<TElement> elements, Span<TPriority> priorities)
        {
            if (priorities.Length != elements.Length)
            {
                throw new ArgumentException("The priorities and elements must be the same length.");
            }

            this.Capacity = priorities.Length;
            this.priorityList = priorities;
            this.elementList = elements;
        }

        /// <summary>
        /// Attempts to add the provided  <paramref name="item"/>. If the list
        /// is currently at maximum capacity and the elements priority is greater
        /// than or equal to the highest priority, the <paramref name = "item"/> is not inserted. If the
        /// <paramref name = "item"/> is eligible for insertion, the upon insertion the <paramref name = "item"/> that previously
        /// had the largest priority is removed from the list.
        /// This is an O(log n) operation.
        /// </summary>
        /// <param name="item">The item to be inserted</param>
        /// <param name="priority">The priority of th given item.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TElement item, TPriority priority)
        {
            if (this.Count >= this.Capacity)
            {
                if (this.priorityList[this.Count - 1].CompareTo(priority) < 0)
                {
                    return;
                }

                var index = this.priorityList.Slice(0, this.Count).BinarySearch(priority);
                index = index >= 0 ? index : ~index;

                this.Insert(index, priority, item);
            }
            else
            {
                var index = this.priorityList.Slice(0, this.Count).BinarySearch(priority);
                index = index >= 0 ? index : ~index;

                this.Insert(index, priority, item);
            }
        }

        private void Insert(int index, TPriority priority, TElement item)
        {
            // Note that insertions at the end are legal.
            if ((uint)index > (uint)this.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var removeLast = this.IsFull ? 1 : 0;

            if (index < this.Count)
            {
                this.priorityList.Slice(index, this.Count - index - removeLast).CopyTo(this.priorityList.Slice(index + 1));
                this.elementList.Slice(index, this.Count - index - removeLast).CopyTo(this.elementList.Slice(index + 1));
            }

            this.priorityList[index] = priority;
            this.elementList[index] = item;
            this.Count += 1 - removeLast;
        }
    }
}
