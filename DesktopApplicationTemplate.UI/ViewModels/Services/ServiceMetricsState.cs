using System;

namespace DesktopApplicationTemplate.UI.ViewModels.Services
{
    public sealed class ServiceMetricsState : ViewModelBase
    {
        private const string ExecutionTimePlaceholder = "Last: -- ms (Avg: -- ms)";

        private double _totalExecutionTimeMs;
        private int _executionCount;
        private TimeSpan _lastExecutionDuration;
        private int _incomingMessageCount;
        private int _outgoingMessageCount;

        public double TotalExecutionTimeMs
        {
            get => _totalExecutionTimeMs;
            set
            {
                if (Math.Abs(_totalExecutionTimeMs - value) < double.Epsilon)
                {
                    return;
                }

                _totalExecutionTimeMs = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AverageExecutionTimeMs));
                OnPropertyChanged(nameof(ExecutionTimeText));
            }
        }

        public int ExecutionCount
        {
            get => _executionCount;
            set
            {
                if (_executionCount == value)
                {
                    return;
                }

                _executionCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AverageExecutionTimeMs));
                OnPropertyChanged(nameof(ExecutionTimeText));
            }
        }

        public TimeSpan LastExecutionDuration
        {
            get => _lastExecutionDuration;
            set
            {
                if (_lastExecutionDuration == value)
                {
                    return;
                }

                _lastExecutionDuration = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ExecutionTimeText));
            }
        }

        public double? AverageExecutionTimeMs => _executionCount == 0
            ? null
            : _totalExecutionTimeMs / _executionCount;

        public string ExecutionTimeText => _executionCount == 0
            ? ExecutionTimePlaceholder
            : $"Last: {LastExecutionDuration.TotalMilliseconds:F0} ms (Avg: {AverageExecutionTimeMs.GetValueOrDefault():F0} ms)";

        public int IncomingMessageCount
        {
            get => _incomingMessageCount;
            private set
            {
                if (_incomingMessageCount == value)
                {
                    return;
                }

                _incomingMessageCount = value;
                OnPropertyChanged();
            }
        }

        public int OutgoingMessageCount
        {
            get => _outgoingMessageCount;
            private set
            {
                if (_outgoingMessageCount == value)
                {
                    return;
                }

                _outgoingMessageCount = value;
                OnPropertyChanged();
            }
        }

        public void RecordExecutionTime(TimeSpan duration)
        {
            if (duration < TimeSpan.Zero)
            {
                throw new ArgumentException("Duration must be non-negative", nameof(duration));
            }

            _totalExecutionTimeMs += duration.TotalMilliseconds;
            _executionCount++;
            LastExecutionDuration = duration;
            OnPropertyChanged(nameof(AverageExecutionTimeMs));
            OnPropertyChanged(nameof(ExecutionTimeText));
        }

        public void ResetMessageCounts()
        {
            SetMessageCounts(0, 0);
        }

        public void InitializeMessageCounts(int incoming, int outgoing)
        {
            SetMessageCounts(Math.Max(0, incoming), Math.Max(0, outgoing));
        }

        public void IncrementIncomingMessages()
        {
            IncomingMessageCount = checked(IncomingMessageCount + 1);
        }

        public void IncrementOutgoingMessages()
        {
            OutgoingMessageCount = checked(OutgoingMessageCount + 1);
        }

        public void SetMessageCounts(int incoming, int outgoing)
        {
            IncomingMessageCount = incoming;
            OutgoingMessageCount = outgoing;
        }
    }
}
