using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SortingComparison
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ArraySorter _sorter;
        private readonly SynchronizationContext _uiContext;
        private int[]? _originalArray;
        private int[]? _sharedArray;

        private SemaphoreSlim _semaphore = new SemaphoreSlim(Environment.ProcessorCount);
        private CancellationTokenSource _cts = new CancellationTokenSource();

       
        [ObservableProperty] private int _arraySize = 1000;
        [ObservableProperty] private string? _originalArrayString;
        [ObservableProperty] private string? _bubbleSortResult;
        [ObservableProperty] private string? _quickSortResult;
        [ObservableProperty] private string? _insertionSortResult;
        [ObservableProperty] private string? _shakerSortResult;
        [ObservableProperty] private string _totalComparisons = "Общее число сравнений: 0";
        [ObservableProperty] private bool _useSharedArray = false;
        [ObservableProperty] private int _maxThreads = Environment.ProcessorCount;

       
        [ObservableProperty] private double _bubbleProgress;
        [ObservableProperty] private double _quickProgress;
        [ObservableProperty] private double _insertionProgress;
        [ObservableProperty] private double _shakerProgress;

        public MainViewModel()
        {
            _sorter = new ArraySorter();
            _uiContext = SynchronizationContext.Current ?? new SynchronizationContext();

            
            _sorter.BubbleSortCompleted += OnBubbleSortCompleted;
            _sorter.QuickSortCompleted += OnQuickSortCompleted;
            _sorter.InsertionSortCompleted += OnInsertionSortCompleted;
            _sorter.ShakerSortCompleted += OnShakerSortCompleted;
        }

        

        [RelayCommand]
        private void GenerateArray()
        {
          
            _semaphore = new SemaphoreSlim(Math.Max(1, MaxThreads));

            _originalArray = _sorter.GenerateRandomArray(ArraySize);
            if (UseSharedArray) _sharedArray = _originalArray;
            else _sharedArray = null;

            OriginalArrayString = "Исходный массив: " + string.Join(", ", _originalArray, 0, Math.Min(20, _originalArray.Length)) + (ArraySize > 20 ? "..." : "");
            BubbleSortResult = QuickSortResult = InsertionSortResult = ShakerSortResult = null;
            BubbleProgress = QuickProgress = InsertionProgress = ShakerProgress = 0;
            TotalComparisons = "Общее число сравнений: 0";

           
            BubbleSortCommand.NotifyCanExecuteChanged();
            QuickSortCommand.NotifyCanExecuteChanged();
            InsertionSortCommand.NotifyCanExecuteChanged();
            ShakerSortCommand.NotifyCanExecuteChanged();
        }

        private bool CanGenerateArray() => true;

        // Пузырьковая
        private bool CanSortBubble() => _originalArray != null && BubbleSortResult != "Сортируется...";
        [RelayCommand(CanExecute = nameof(CanSortBubble))]
        private void BubbleSort()
        {
            BubbleSortResult = "Сортируется...";
            BubbleProgress = 0;

            
            var progress = new Progress<double>(p => _uiContext.Post(_ => BubbleProgress = p, null));
            var ct = _cts.Token;

           
            Task.Run(() =>
            {
                try
                {
                    _semaphore.Wait(ct); 
                }
                catch (OperationCanceledException)
                {
                    
                    _semaphore.Release();
                    _uiContext.Post(_ =>
                    {
                        BubbleSortResult = "Отменено";
                        BubbleSortCommand.NotifyCanExecuteChanged();
                    }, null);
                    return;
                }

                
                var thread = new Thread(() => _sorter.BubbleSort(_originalArray!, progress, ct, UseSharedArray, _sharedArray));
                thread.IsBackground = true;
                thread.Start();
            });
        }

        // Быстрая
        private bool CanSortQuick() => _originalArray != null && QuickSortResult != "Сортируется...";
        [RelayCommand(CanExecute = nameof(CanSortQuick))]
        private void QuickSort()
        {
            QuickSortResult = "Сортируется...";
            QuickProgress = 0;
            var progress = new Progress<double>(p => _uiContext.Post(_ => QuickProgress = p, null));
            var ct = _cts.Token;

            Task.Run(() =>
            {
                try { _semaphore.Wait(ct); }
                catch (OperationCanceledException)
                {
                    _semaphore.Release();
                    _uiContext.Post(_ =>
                    {
                        QuickSortResult = "Отменено";
                        QuickSortCommand.NotifyCanExecuteChanged();
                    }, null);
                    return;
                }

                var thread = new Thread(() => _sorter.QuickSort(_originalArray!, progress, ct, UseSharedArray, _sharedArray));
                thread.IsBackground = true;
                thread.Start();
            });
        }

        // Вставками
        private bool CanSortInsertion() => _originalArray != null && InsertionSortResult != "Сортируется...";
        [RelayCommand(CanExecute = nameof(CanSortInsertion))]
        private void InsertionSort()
        {
            InsertionSortResult = "Сортируется...";
            InsertionProgress = 0;
            var progress = new Progress<double>(p => _uiContext.Post(_ => InsertionProgress = p, null));
            var ct = _cts.Token;

            Task.Run(() =>
            {
                try { _semaphore.Wait(ct); }
                catch (OperationCanceledException)
                {
                    _semaphore.Release();
                    _uiContext.Post(_ =>
                    {
                        InsertionSortResult = "Отменено";
                        InsertionSortCommand.NotifyCanExecuteChanged();
                    }, null);
                    return;
                }

                var thread = new Thread(() => _sorter.InsertionSort(_originalArray!, progress, ct, UseSharedArray, _sharedArray));
                thread.IsBackground = true;
                thread.Start();
            });
        }

        // Шейкер 
        private bool CanSortShaker() => _originalArray != null && ShakerSortResult != "Сортируется...";
        [RelayCommand(CanExecute = nameof(CanSortShaker))]
        private void ShakerSort()
        {
            ShakerSortResult = "Сортируется...";
            ShakerProgress = 0;
            var progress = new Progress<double>(p => _uiContext.Post(_ => ShakerProgress = p, null));
            var ct = _cts.Token;

            Task.Run(() =>
            {
                try { _semaphore.Wait(ct); }
                catch (OperationCanceledException)
                {
                    _semaphore.Release();
                    _uiContext.Post(_ =>
                    {
                        ShakerSortResult = "Отменено";
                        ShakerSortCommand.NotifyCanExecuteChanged();
                    }, null);
                    return;
                }

                var thread = new Thread(() => _sorter.ShakerSort(_originalArray!, progress, ct, UseSharedArray, _sharedArray));
                thread.IsBackground = true;
                thread.Start();
            });
        }

        
        [RelayCommand]
        private void CancelAll()
        {
            _cts.Cancel();
           
            _cts = new CancellationTokenSource();
            
            _semaphore = new SemaphoreSlim(Math.Max(1, MaxThreads));
        }

       
        private void OnBubbleSortCompleted(int[] sortedArray, long comparisons, double elapsedMs)
        {
            
            _semaphore.Release();

            _uiContext.Post(_ =>
            {
                BubbleSortResult = $"Пузырьковая: {FormatArray(sortedArray)}, время: {elapsedMs:F2} мс, сравнений: {comparisons}";
                UpdateTotalComparisons();
                BubbleSortCommand.NotifyCanExecuteChanged();
            }, null);
        }

        private void OnQuickSortCompleted(int[] sortedArray, long comparisons, double elapsedMs)
        {
            _semaphore.Release();

            _uiContext.Post(_ =>
            {
                QuickSortResult = $"Быстрая: {FormatArray(sortedArray)}, время: {elapsedMs:F2} мс, сравнений: {comparisons}";
                UpdateTotalComparisons();
                QuickSortCommand.NotifyCanExecuteChanged();
            }, null);
        }

        private void OnInsertionSortCompleted(int[] sortedArray, long comparisons, double elapsedMs)
        {
            _semaphore.Release();

            _uiContext.Post(_ =>
            {
                InsertionSortResult = $"Вставками: {FormatArray(sortedArray)}, время: {elapsedMs:F2} мс, сравнений: {comparisons}";
                UpdateTotalComparisons();
                InsertionSortCommand.NotifyCanExecuteChanged();
            }, null);
        }

        private void OnShakerSortCompleted(int[] sortedArray, long comparisons, double elapsedMs)
        {
            _semaphore.Release();

            _uiContext.Post(_ =>
            {
                ShakerSortResult = $"Шейкер: {FormatArray(sortedArray)}, время: {elapsedMs:F2} мс, сравнений: {comparisons}";
                UpdateTotalComparisons();
                ShakerSortCommand.NotifyCanExecuteChanged();
            }, null);
        }

        private void UpdateTotalComparisons()
        {
            TotalComparisons = $"Общее число сравнений: {_sorter.TotalComparisons}";
        }

        private string FormatArray(int[] arr)
        {
            if (arr == null) return "";
            if (arr.Length <= 10) return string.Join(", ", arr);
            return string.Join(", ", arr, 0, 5) + "...";
        }
    }
}
