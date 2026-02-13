using ActivityMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ActivityMonitor.ViewModels
{
    public partial class MainViewModel
    {
        // Sorting state
        private string? _sortedColumn = "Process";
        private SortState _sortState = SortState.Ascending;

        public SortState GetCurrentSortState() => _sortState;
        public string? GetSortedColumn() => _sortedColumn;

        // Handles column sorting state
        public void ApplyColumnSort(string column)
        {
            if (_sortedColumn != column)
            {
                _sortedColumn = column;
                _sortState = SortState.Descending;
            }
            else
            {
                _sortState = _sortState == SortState.Descending
                    ? SortState.Ascending
                    : SortState.Descending;
            }

            ApplySorting();
        }

        // Applies filtering and sorting to the process list
        private void ApplyFilterAndSorting()
        {
            IEnumerable<GroupedProcessInfo> query = Processes;

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(p =>
                    p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // Apply sorting
            var selector = GetSortSelector();
            query = _sortState == SortState.Ascending
                ? query.OrderBy(selector)
                : query.OrderByDescending(selector);

            var list = query.ToList();

            // Update FilteredProcesses collection
            ReconcileFilteredList(list);
        }

        // Applies sorting to the main process list
        private void ApplySorting()
        {
            var selector = GetSortSelector();

            var ordered = _sortState == SortState.Ascending
                ? Processes.OrderBy(selector)
                : Processes.OrderByDescending(selector);

            var list = ordered.ToList();

            // Reorder collection in place
            for (int i = 0; i < list.Count; i++)
            {
                int oldIndex = Processes.IndexOf(list[i]);
                if (oldIndex != i)
                    Processes.Move(oldIndex, i);
            }
        }

        private Func<GroupedProcessInfo, object> GetSortSelector()
        {
            return _sortedColumn switch
            {
                "Process" => p => p.Name,
                "CPU (%)" => p => p.Cpu,
                "CPU Time" => p => p.CpuTime,
                "Threads" => p => p.ThreadCount,
                "Memory (MB)" => p => p.Memory,
                "Handles" => p => p.HandleCount,
                "Type" => p => p.ExecutionType,
                "Read (MB/s)" => p => p.DiskReadRate,
                "Write (MB/s)" => p => p.DiskWriteRate,
                "Total I/O" => p => p.TotalDiskIO,
                "GPU (%)" => p => p.GpuUsage,
                "GPU Engine" => p => p.GpuEngine,
                _ => p => p.Name
            };
        }

        private void ReconcileFilteredList(List<GroupedProcessInfo> targetList)
        {
            // Remove items no longer in the filtered list
            for (int i = FilteredProcesses.Count - 1; i >= 0; i--)
            {
                if (!targetList.Contains(FilteredProcesses[i]))
                    FilteredProcesses.RemoveAt(i);
            }

            // Add or reorder items
            for (int i = 0; i < targetList.Count; i++)
            {
                if (i >= FilteredProcesses.Count)
                {
                    FilteredProcesses.Add(targetList[i]);
                }
                else if (FilteredProcesses[i] != targetList[i])
                {
                    int oldIndex = FilteredProcesses.IndexOf(targetList[i]);

                    if (oldIndex >= 0)
                    {
                        FilteredProcesses.Move(oldIndex, i);
                    }
                    else
                    {
                        FilteredProcesses.Insert(i, targetList[i]);
                    }
                }
            }
        }
    }
}