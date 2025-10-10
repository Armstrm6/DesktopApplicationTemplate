using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DesktopApplicationTemplate.UI.Helpers
{
    public class LimitedObservableCollection<T> : ObservableCollection<T>
    {
        public LimitedObservableCollection(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            Capacity = capacity;
        }

        public int Capacity { get; }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(Count, item);
            TrimExcess();
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (items is null)
            {
                return;
            }

            foreach (var item in items)
            {
                base.InsertItem(Count, item);
                TrimExcess();
            }
        }

        private void TrimExcess()
        {
            while (Count > Capacity)
            {
                RemoveAt(0);
            }
        }
    }
}

