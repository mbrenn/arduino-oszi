using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;
namespace Arduino.Generic
{
    /// <summary>
    /// Represents a fixed length ring buffer to store a specified maximal count of items within.
    /// Class retrieved from: http://florianreischl.blogspot.de/2010/01/generic-c-ringbuffer.html . Thanks
    /// </summary>
    /// <typeparam name="T">The generic type of the items stored within the ring buffer.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    public class RingBuffer<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
    {
        /// <summary>
        /// the internal buffer
        /// </summary>
        private T[] buffer;

        /// <summary>
        /// The all-over position within the ring buffer. The position
        /// increases continously by adding new items to the buffer. This
        /// value is needed to calculate the current relative position within the
        /// buffer.
        /// </summary>
        private int position;

        /// <summary>
        /// The current version of the buffer, this is required for a correct
        /// exception handling while enumerating over the items of the buffer.
        /// </summary>
        private long version;

        /// <summary>
        /// Creates a new instance of a <see cref="RingBuffer&lt;T&gt;"/> with a
        /// specified cache size.
        /// </summary>
        /// <param name="capacity">The maximal count of items to be stored within
        /// the ring buffer.</param>
        public RingBuffer(int capacity)
        {
            // validate capacity
            if (capacity <= 0)
                throw new ArgumentException(
                    "Must be greater than zero",
                    "capacity"
                );

            // set capacity and init the cache
            this.Capacity = capacity;
            this.buffer = new T[capacity];
        }

        /// <summary>
        /// Gets or sets an item for a specified position within the ring buffer.
        /// </summary>
        /// <param name="index">The position to get or set an item.</param>
        /// <returns>The fond item at the specified position within the ring buffer.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public T this[int index]
        {
            get
            {
                // validate the index
                if (index < 0 || index >= LongCount)
                    throw new IndexOutOfRangeException();
                // calculate the relative position within the rolling base array
                var index2 = (position - LongCount + index) % Capacity;
                return this.buffer[index2];
            }
            set { Insert(index, value); }
        }

        /// <summary>
        /// Gets or sets an item for a specified position within the ring buffer.
        /// </summary>
        /// <param name="index">The position to get or set an item.</param>
        /// <returns>The fond item at the specified position within the ring buffer.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public T this[long index]
        {
            get
            {
                // validate the index
                if (index < 0 || index >= LongCount)
                    throw new IndexOutOfRangeException();
                // calculate the relative position within the rolling base array
                var index2 = (position - LongCount + index) % Capacity;
                return this.buffer[index2];
            }
            set { Insert(index, value); }
        }

        /// <summary>
        /// Gets the maximal count of items within the ring buffer.
        /// </summary>
        public int Capacity { get; private set; }

        public int Count { get { return (int)this.LongCount; } }

        /// <summary>
        /// Get the current count of items within the ring buffer.
        /// </summary>
        public long LongCount { get; private set; }

        /// <summary>
        /// Adds a new item to the buffer.
        /// </summary>
        /// <param name="item">The item to be added to the buffer.</param>
        public void Add(T item)
        {
            // add a new item to the current relative position within the
            // buffer and increase the position
            this.buffer[position++ % Capacity] = item;
            // increase the count if capacity is not yet reached
            if (LongCount < Capacity)
            {
                LongCount++;
            }

            // buffer changed; next version
            this.version++;
        }

        /// <summary>
        /// Clears the whole buffer and releases all referenced objects
        /// currently stored within the buffer.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < LongCount; i++)
            {
                this.buffer[i] = default(T);
            }

            position = 0;
            LongCount = 0;
            this.version++;
        }

        /// <summary>
        /// Determines if a specified item is currently present within
        /// the buffer.
        /// </summary>
        /// <param name="item">The item to search for within the current
        /// buffer.</param>
        /// <returns>True if the specified item is currently present within
        /// the buffer; otherwise false.</returns>
        public bool Contains(T item)
        {
            int index = IndexOf(item);
            return index != -1;
        }

        /// <summary>
        /// Copies the current items within the buffer to a specified array.
        /// </summary>
        /// <param name="array">The target array to copy the items of
        /// the buffer to.</param>
        /// <param name="arrayIndex">The start position witihn the target
        /// array to start copying.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            for (var i = 0L; i < LongCount; i++)
            {
                array[i + arrayIndex] = this.buffer[(position - LongCount + i) % Capacity];
            }
        }

        /// <summary>
        /// Gets an enumerator over the current items within the buffer.
        /// </summary>
        /// <returns>An enumerator over the current items within the buffer.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            long version = this.version;

            for (var i = 0L; i < LongCount; i++)
            {
                if (this.version != version)
                    throw new InvalidOperationException("Collection changed");
                yield return this[i];
            }
        }

        /// <summary>
        /// Gets the position of a specied item within the ring buffer.
        /// </summary>
        /// <param name="item">The item to get the current position for.</param>
        /// <returns>The zero based index of the found item within the
        /// buffer. If the item was not present within the buffer, this
        /// method returns -1.</returns>
        public int IndexOf(T item)
        {
            // loop over the current count of items
            for (var i = 0L; i < LongCount; i++)
            {
                // get the item at the relative position within the internal array
                T item2 = this.buffer[(position - LongCount + i) % Capacity];
                // if both items are null, return true
                if (null == item && null == item2)
                    return (int)i;
                // if equal return the position
                if (item != null && item.Equals(item2))
                    return (int)i;
            }
            // nothing found
            return -1;
        }

        /// <summary>
        /// Inserts an item at a specified position into the buffer.
        /// </summary>
        /// <param name="index">The position within the buffer to add
        /// the new item.</param>
        /// <param name="item">The new item to be added to the buffer.</param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        /// <remarks>
        /// If the specified index is equal to the current count of items
        /// within the buffer, the specified item will be added.
        ///
        /// <b>Warning</b>
        /// Frequent usage of this method might become a bad idea if you are
        /// working with a large buffer capacity. The insertion of an item
        /// at a specified position within the buffer causes causes all present
        /// items below the specified position to be moved one position.
        /// </remarks>
        public void Insert(int index, T item)
        {
            this.Insert((long) index, item);
        }

        public void Insert(long index, T item)
        {
            // validate index
            if (index < 0 || index > LongCount)
            {
                throw new IndexOutOfRangeException();
            }

            // add if index equals to count
            if (index == LongCount)
            {
                Add(item);
                return;
            }

            // get the maximal count of items to be moved
            var count = Math.Min(LongCount, Capacity - 1) - index;
            // get the relative position of the new item within the buffer
            var index2 = (position - LongCount + index) % Capacity;
            // move all items below the specified position
            for (var i = index2 + count; i > index2; i--)
            {
                var to = i % Capacity;
                var from = (i - 1) % Capacity;
                this.buffer[to] = this.buffer[from];
            }
            // set the new item
            this.buffer[index2] = item;
            // adjust storage information
            if (LongCount < Capacity)
            {
                LongCount++;
                position++;
            }

            // buffer changed; next version
            this.version++;
        }

        /// <summary>
        /// Removes a specified item from the current buffer.
        /// </summary>
        /// <param name="item">The item to be removed.</param>
        /// <returns>True if the specified item was successfully removed
        /// from the buffer; otherwise false.</returns>
        /// <remarks>
        /// <b>Warning</b>
        /// Frequent usage of this method might become a bad idea if you are
        /// working with a large buffer capacity. The removing of an item
        /// requires a scan of the buffer to get the position of the specified
        /// item. If the item was found, the deletion requires a move of all        
        /// items stored abouve the found position.
        /// </remarks>
        public bool Remove(T item)
        {
            // find the position of the specified item
            int index = IndexOf(item);
            // item was not found; return false
            if (index == -1)
            {
                return false;
            }

            // remove the item at the specified position
            RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Removes an item at a specified position within the buffer.
        /// </summary>
        /// <param name="index">The position of the item to be removed.</param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        /// <remarks>
        /// <b>Warning</b>
        /// Frequent usage of this method might become a bad idea if you are
        /// working with a large buffer capacity. The deletion requires a move
        /// of all items stored abouve the found position.
        /// </remarks>
        public void RemoveAt(int index)
        {
            this.RemoveAt((long)index);
        }

        public void RemoveAt(long index)
        {
            // validate the index
            if (index < 0 || index >= LongCount)
                throw new IndexOutOfRangeException();
            // move all items above the specified position one step
            // closer to zeri
            for (var i = index; i < LongCount - 1; i++)
            {
                // get the next relative target position of the item
                var to = (position - LongCount + i) % Capacity;
                // get the next relative source position of the item
                var from = (position - LongCount + i + 1) % Capacity;
                // move the item
                this.buffer[to] = this.buffer[from];
            }

            // get the relative position of the last item, which becomes empty
            // after deletion and set the item as empty
            int last = (position - 1) % Capacity;
            this.buffer[last] = default(T);

            // adjust storage information
            position--;
            LongCount--;

            // buffer changed; next version
            this.version++;
        }

        /// <summary>
        /// Gets if the buffer is read-only. This method always returns false.
        /// </summary>
        bool ICollection<T>.IsReadOnly { get { return false; } }

        /// <summary>
        /// See generic implementation of <see cref="GetEnumerator"/>.
        /// </summary>
        /// <returns>See generic implementation of <see cref="GetEnumerator"/>.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Gets the fill size of the buffer
        /// </summary>
        /// <returns>Number of bytes being allocated</returns>
        public int FillSize
        {
            get
            {
                return (int) Math.Min(this.LongCount, this.Capacity);
            }
        }
    }
}