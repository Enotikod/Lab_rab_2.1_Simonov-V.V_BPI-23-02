using System;
using System.Threading;

namespace SortingComparison
{
    public delegate void SortCompletedHandler(int[] sortedArray, long comparisons, double elapsedMilliseconds);

    public class ArraySorter
    {
        private long _totalComparisons;
        private readonly object _locker = new object();
        private readonly object _sharedArrayLock = new object();

        public event SortCompletedHandler? BubbleSortCompleted;
        public event SortCompletedHandler? QuickSortCompleted;
        public event SortCompletedHandler? InsertionSortCompleted;
        public event SortCompletedHandler? ShakerSortCompleted;

        public long TotalComparisons => Interlocked.Read(ref _totalComparisons);

        public int[] GenerateRandomArray(int size)
        {
            var rand = new Random();
            var arr = new int[size];
            for (int i = 0; i < size; i++)
                arr[i] = rand.Next(1000); return arr;
            
        }

        private int[] CopyArray(int[] source)
        {
            var copy = new int[source.Length];
            Array.Copy(source, copy, source.Length); return copy;
           
        }

     
        public void BubbleSort(int[] originalArray, IProgress<double>? progress = null,
            CancellationToken token = default, bool useSharedArray = false, int[]? sharedArray = null)
        {
            int[] array = useSharedArray ? sharedArray! : CopyArray(originalArray);
            long comparisons = 0;
            var watch = System.Diagnostics.Stopwatch.StartNew();

            int n = array.Length;
            for (int i = 0; i < n - 1; i++)
            {
                if (token.IsCancellationRequested) break;

                for (int j = 0; j < n - 1 - i; j++)
                {
                    if (token.IsCancellationRequested) break;

                    if (useSharedArray)
                    {
                        lock (_sharedArrayLock)
                        {
                            comparisons++;
                            if (array[j] > array[j + 1])
                                (array[j], array[j + 1]) = (array[j + 1], array[j]);
                        }
                    }
                    else
                    {
                        comparisons++;
                        if (array[j] > array[j + 1])
                            (array[j], array[j + 1]) = (array[j + 1], array[j]);
                    }
                }

                progress?.Report((i + 1) * 100.0 / (n - 1));
            }

            watch.Stop();
            lock (_locker) _totalComparisons += comparisons;
            BubbleSortCompleted?.Invoke(array, comparisons, watch.Elapsed.TotalMilliseconds);
        }

        
        public void QuickSort(int[] originalArray, IProgress<double>? progress = null,
            CancellationToken token = default, bool useSharedArray = false, int[]? sharedArray = null)
        {
            int[] array = useSharedArray ? sharedArray! : CopyArray(originalArray);
            long comparisons = 0;
            var watch = System.Diagnostics.Stopwatch.StartNew();

            int n = array.Length;
            long processed = 0;

            void Sort(int l, int r)
            {
                if (l >= r || token.IsCancellationRequested) return;
                int p = Partition(l, r);
                Sort(l, p - 1);
                Sort(p + 1, r);
            }

            int Partition(int l, int r)
            {
                int pivot = array[r];
                int i = l - 1;

                for (int j = l; j < r; j++)
                {
                    if (token.IsCancellationRequested) break;

                    comparisons++;
                    if (array[j] < pivot)
                    {
                        i++;
                        (array[i], array[j]) = (array[j], array[i]);
                    }

                    long done = Interlocked.Increment(ref processed);
                    if (done % 100 == 0)
                        progress?.Report(done * 100.0 / n);
                }

                (array[i + 1], array[r]) = (array[r], array[i + 1]); return i + 1;
            }

            Sort(0, n - 1);
            progress?.Report(100);

            watch.Stop();
            lock (_locker) _totalComparisons += comparisons;
            QuickSortCompleted?.Invoke(array, comparisons, watch.Elapsed.TotalMilliseconds);
        }

      
        public void InsertionSort(int[] originalArray, IProgress<double>? progress = null,
            CancellationToken token = default, bool useSharedArray = false, int[]? sharedArray = null)
        {
            int[] array = useSharedArray ? sharedArray! : CopyArray(originalArray);
            long comparisons = 0;
            var watch = System.Diagnostics.Stopwatch.StartNew();

            int n = array.Length;

            for (int i = 1; i < n; i++)
            {
                if (token.IsCancellationRequested) break;

                int key = array[i];
                int j = i - 1;

                while (j >= 0 && array[j] > key)
                {
                    comparisons++;
                    array[j + 1] = array[j];
                    j--;
                }

                comparisons++;
                array[j + 1] = key;

                progress?.Report(i * 100.0 / (n - 1));
            }

            watch.Stop();
            lock (_locker) _totalComparisons += comparisons;
            InsertionSortCompleted?.Invoke(array, comparisons, watch.Elapsed.TotalMilliseconds);
        }

        
        public void ShakerSort(int[] originalArray, IProgress<double>? progress = null,
            CancellationToken token = default, bool useSharedArray = false, int[]? sharedArray = null)
        {
            int[] array = useSharedArray ? sharedArray! : CopyArray(originalArray);
            long comparisons = 0;
            var watch = System.Diagnostics.Stopwatch.StartNew();

            int n = array.Length;
            int left = 0, right = n - 1;

            while (left < right)
            {
                if (token.IsCancellationRequested) break;

                for (int i = left; i < right; i++)
                {
                    comparisons++;
                    if (array[i] > array[i + 1])
                        (array[i], array[i + 1]) = (array[i + 1], array[i]);
                }
                right--;

                for (int i = right; i > left; i--)
                {
                    comparisons++;
                    if (array[i - 1] > array[i])
                        (array[i - 1], array[i]) = (array[i], array[i - 1]);
                }
                left++;

              
                int sorted = left + (n - 1 - right);
                progress?.Report(sorted * 100.0 / n);
            }

            progress?.Report(100);

            watch.Stop();
            lock (_locker) _totalComparisons += comparisons;
            ShakerSortCompleted?.Invoke(array, comparisons, watch.Elapsed.TotalMilliseconds);
        }
    }
}
