using System;
using System.Collections;
using System.Collections.Generic;

//public class PriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
//{
//    private readonly List<(TElement Element, TPriority Priority)> _heap;
//    private readonly IComparer<TPriority>? _comparer;

//    public PriorityQueue() : this(null, null) { }

//    public PriorityQueue(int initialCapacity) : this(initialCapacity, null) { }

//    public PriorityQueue(IComparer<TPriority>? comparer) : this(null, comparer) { }

//    public PriorityQueue(int initialCapacity, IComparer<TPriority>? comparer)
//    {
//        _heap = initialCapacity > 0
//            ? new List<(TElement, TPriority)>(initialCapacity)
//            : new List<(TElement, TPriority)>();
//        _comparer = comparer ?? Comparer<TPriority>.Default;
//    }

//    public PriorityQueue(IEnumerable<(TElement Element, TPriority Priority)> items)
//        : this(items, null) { }

//    public PriorityQueue(IEnumerable<(TElement Element, TPriority Priority)> items,
//        IComparer<TPriority>? comparer)
//    {
//        _comparer = comparer ?? Comparer<TPriority>.Default;
//        _heap = new List<(TElement, TPriority)>(items);

//        // Heapify process
//        for (int i = _heap.Count / 2 - 1; i >= 0; i--)
//        {
//            HeapifyDown(i);
//        }
//    }

//    public int Count => _heap.Count;

//    public void Enqueue(TElement element, TPriority priority)
//    {
//        _heap.Add((element, priority));
//        HeapifyUp(_heap.Count - 1);
//    }

//    public TElement Peek()
//    {
//        if (_heap.Count == 0)
//            throw new InvalidOperationException("Queue is empty.");

//        return _heap[0].Element;
//    }

//    public TElement Dequeue()
//    {
//        if (_heap.Count == 0)
//            throw new InvalidOperationException("Queue is empty.");

//        var result = _heap[0].Element;
//        MoveLastToTop();
//        return result;
//    }

//    public bool TryPeek(out TElement element, out TPriority priority)
//    {
//        if (_heap.Count == 0)
//        {
//            element = default!;
//            priority = default!;
//            return false;
//        }

//        (element, priority) = _heap[0];
//        return true;
//    }

//    public bool TryDequeue(out TElement element, out TPriority priority)
//    {
//        if (_heap.Count == 0)
//        {
//            element = default!;
//            priority = default!;
//            return false;
//        }

//        (element, priority) = _heap[0];
//        MoveLastToTop();
//        return true;
//    }

//    public void Clear()
//    {
//        _heap.Clear();
//    }

//    private void MoveLastToTop()
//    {
//        int lastIdx = _heap.Count - 1;
//        _heap[0] = _heap[lastIdx];
//        _heap.RemoveAt(lastIdx);

//        if (_heap.Count > 0)
//        {
//            HeapifyDown(0);
//        }
//    }

//    private void HeapifyUp(int index)
//    {
//        while (index > 0)
//        {
//            int parentIndex = (index - 1) / 2;
//            if (ComparePriorities(index, parentIndex) >= 0)
//                break;

//            Swap(index, parentIndex);
//            index = parentIndex;
//        }
//    }

//    private void HeapifyDown(int index)
//    {
//        while (true)
//        {
//            int leftChild = 2 * index + 1;
//            if (leftChild >= _heap.Count)
//                return;

//            int smallestChild = leftChild;
//            int rightChild = leftChild + 1;

//            if (rightChild < _heap.Count && ComparePriorities(rightChild, leftChild) < 0)
//            {
//                smallestChild = rightChild;
//            }

//            if (ComparePriorities(smallestChild, index) >= 0)
//                return;

//            Swap(index, smallestChild);
//            index = smallestChild;
//        }
//    }

//    private int ComparePriorities(int indexA, int indexB)
//    {
//        return _comparer!.Compare(_heap[indexA].Priority, _heap[indexB].Priority);
//    }

//    private void Swap(int indexA, int indexB)
//    {
//        (_heap[indexA], _heap[indexB]) = (_heap[indexB], _heap[indexA]);
//    }

//    // Optional: Enumerator implementation
//    public IEnumerator<TElement> GetEnumerator()
//    {
//        foreach (var item in _heap)
//        {
//            yield return item.Element;
//        }
//    }
//}

public class PriorityQueue<T> : IEnumerable<T> where T : IComparable<T>
{
    private readonly List<T> _heap;
    //private readonly IComparer<T>? _comparer;

    public PriorityQueue()
    {
        _heap = new List<T>();
        //_comparer = null;
    }

    public PriorityQueue(int initialCapacity)
    {
        _heap = new List<T>(initialCapacity);
        //_comparer = null;
    }

    //public PriorityQueue(IComparer<T> comparer)
    //{
    //    _heap = new List<T>();
    //    _comparer = comparer;
    //}

    //public PriorityQueue(IEnumerable<T> collection)
    //{
    //    _heap = new List<T>(collection);
    //    _comparer = null;

    //    // Heapify the collection
    //    for (int i = _heap.Count / 2 - 1; i >= 0; i--)
    //    {
    //        HeapifyDown(i);
    //    }
    //}

    public int Count => _heap.Count;

    public void Enqueue(T item)
    {
        _heap.Add(item);
        HeapifyUp(_heap.Count - 1);
    }

    public T Peek()
    {
        if (_heap.Count == 0)
            throw new InvalidOperationException("Queue is empty.");

        return _heap[0];
    }

    public T Dequeue()
    {
        if (_heap.Count == 0)
            throw new InvalidOperationException("Queue is empty.");

        T result = _heap[0];
        MoveLastToTop();
        return result;
    }

    public bool TryPeek(out T result)
    {
        if (_heap.Count == 0)
        {
            result = default!;
            return false;
        }

        result = _heap[0];
        return true;
    }

    public bool TryDequeue(out T result)
    {
        if (_heap.Count == 0)
        {
            result = default!;
            return false;
        }

        result = _heap[0];
        MoveLastToTop();
        return true;
    }

    public void Clear()
    {
        _heap.Clear();
    }

    private void MoveLastToTop()
    {
        int lastIdx = _heap.Count - 1;
        _heap[0] = _heap[lastIdx];
        _heap.RemoveAt(lastIdx);

        if (_heap.Count > 0)
        {
            HeapifyDown(0);
        }
    }

    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            int parentIndex = (index - 1) / 2;
            if (CompareItems(index, parentIndex) >= 0)
                break;

            Swap(index, parentIndex);
            index = parentIndex;
        }
    }

    private void HeapifyDown(int index)
    {
        while (true)
        {
            int leftChild = 2 * index + 1;
            if (leftChild >= _heap.Count)
                return;

            int smallestChild = leftChild;
            int rightChild = leftChild + 1;

            if (rightChild < _heap.Count && CompareItems(rightChild, leftChild) < 0)
            {
                smallestChild = rightChild;
            }

            if (CompareItems(smallestChild, index) >= 0)
                return;

            Swap(index, smallestChild);
            index = smallestChild;
        }
    }

    private int CompareItems(int indexA, int indexB)
    {
        //if (_comparer != null)
        //{
        //    return _comparer.Compare(_heap[indexA], _heap[indexB]);
        //}
        return _heap[indexA].CompareTo(_heap[indexB]);
    }

    private void Swap(int indexA, int indexB)
    {
        (_heap[indexA], _heap[indexB]) = (_heap[indexB], _heap[indexA]);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _heap.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}