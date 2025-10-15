using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services.Protocols.Csv;
using DesktopApplicationTemplate.Core.Services.Protocols.FileObserver;
using DesktopApplicationTemplate.Core.Services.Protocols.Ftp;
using DesktopApplicationTemplate.Core.Services.Protocols.Heartbeat;
using DesktopApplicationTemplate.Core.Services.Protocols.Http;
using DesktopApplicationTemplate.Core.Services.Protocols.Hid;
using DesktopApplicationTemplate.Core.Services.Protocols.Mqtt;
using DesktopApplicationTemplate.Core.Services.Protocols.Scp;
using DesktopApplicationTemplate.Core.Services.Protocols.Tcp;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels.Services;
using WpfBrush = System.Windows.Media.Brush;
using WpfBrushes = System.Windows.Media.Brushes;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public enum ServiceRuntimeState
    {
        Inactive,
        Activating,
        Active,
        Error
    }

    public class ServiceListModel : ViewModelBase
    {
        internal const int MaxLogEntries = 200;

        public const string InputMessageAttributeName = "InputMessage";
        public const string OutputMessageAttributeName = "OutputMessage";
        public const string IncomingMessageCountAttributeName = "IncomingMessageCount";
        public const string OutgoingMessageCountAttributeName = "OutgoingMessageCount";
        public const string RuntimeStateAttributeName = "RuntimeState";

        private readonly IMessageRoutingService _routingService;
        private readonly Dictionary<string, RoutingAttributeValue> _routingAttributes = new(StringComparer.OrdinalIgnoreCase);

        private readonly record struct RoutingAttributeValue(string Value, MessageRoutingDirection? Direction);

        private string _displayName = string.Empty;
        public string DisplayName
        {
            get => _displayName;
            set
            {
                if (string.Equals(_displayName, value, StringComparison.Ordinal))
                {
                    return;
                }

                var previousName = _displayName;
                _displayName = value ?? string.Empty;
                OnPropertyChanged();
                LogState.UpdateIdentity(_type, _displayName);
                OnDisplayNameChanged(previousName);
            }
        }

        private void OnDisplayNameChanged(string previousName)
        {
            if (string.Equals(previousName, _displayName, StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(_displayName))
                {
                    RepublishRoutingAttributes();
                }

                return;
            }

            if (!string.IsNullOrWhiteSpace(previousName))
            {
                _routingService.ClearService(Type, previousName);
                _routingService.ClearService(previousName);
            }

            if (!string.IsNullOrWhiteSpace(_displayName))
            {
                RepublishRoutingAttributes();
            }
        }

        private ServiceType _type;
        public ServiceType Type
        {
            get => _type;
            set
            {
                if (_type == value)
                {
                    return;
                }

                var previousType = _type;
                _type = value;
                OptionsState.ServiceType = _type;
                LogState.UpdateIdentity(_type, _displayName);
                OnPropertyChanged();
                OnServiceTypeChanged(previousType);
            }
        }

        private void OnServiceTypeChanged(ServiceType previousType)
        {
            if (previousType == _type)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_displayName))
            {
                return;
            }

            _routingService.ClearService(previousType, _displayName);
            _routingService.ClearService(_displayName);
            RepublishRoutingAttributes();
        }

        private string? _descriptorId;
        public string? DescriptorId
        {
            get => _descriptorId;
            set
            {
                if (string.Equals(_descriptorId, value, StringComparison.Ordinal))
                {
                    return;
                }

                _descriptorId = value;
                OptionsState.DescriptorId = _descriptorId;
                OnPropertyChanged();
            }
        }
        [JsonIgnore] public Page? Page { get; set; }
        public int Order { get; set; }

        private bool _isMarkedForRemoval;
        public bool IsMarkedForRemoval
        {
            get => _isMarkedForRemoval;
            set
            {
                if (_isMarkedForRemoval == value)
                {
                    return;
                }

                _isMarkedForRemoval = value;
                OnPropertyChanged();
            }
        }

        private WpfBrush _backgroundColor = WpfBrushes.LightGray;
        public WpfBrush BackgroundColor
        {
            get => _backgroundColor;
            set { _backgroundColor = value; OnPropertyChanged(); }
        }

        private WpfBrush _borderColor = WpfBrushes.Gray;
        public WpfBrush BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; OnPropertyChanged(); }
        }

        private string? _iconGlyph;
        public string? IconGlyph
        {
            get => _iconGlyph;
            private set { _iconGlyph = value; OnPropertyChanged(); }
        }

        private string? _descriptorLabel;
        public string? DescriptorLabel
        {
            get => _descriptorLabel;
            private set { _descriptorLabel = value; OnPropertyChanged(); }
        }

        private bool _needsAttention;
        public bool NeedsAttention => _needsAttention;

        private string? _attentionReason;
        public string? AttentionReason => _attentionReason;

        internal void SetAttentionState(bool needsAttention, string? reason)
        {
            if (!needsAttention)
            {
                reason = null;
            }

            if (_needsAttention != needsAttention)
            {
                _needsAttention = needsAttention;
                OnPropertyChanged(nameof(NeedsAttention));
            }

            if (!string.Equals(_attentionReason, reason, StringComparison.Ordinal))
            {
                _attentionReason = reason;
                OnPropertyChanged(nameof(AttentionReason));
            }
        }
        [JsonIgnore] public Page? ServicePage { get; set; }

        [JsonIgnore]
        internal IServiceLookup? ServiceLookup { get; set; }

        public ObservableCollection<string> AssociatedServices { get; } = new();

        public ServiceOptionsState OptionsState { get; }
        public ServiceLogState LogState { get; }
        public ServiceMetricsState MetricsState { get; }

        internal static Func<string?, IServiceOptionsSerializer?>? OptionsSerializerResolver
        {
            get => ServiceStateCoordinator.OptionsSerializerResolver;
            set => ServiceStateCoordinator.OptionsSerializerResolver = value;
        }

        internal static event Action? CrossServiceAssociationsClearing
        {
            add => ServiceStateCoordinator.CrossServiceAssociationsClearing += value;
            remove => ServiceStateCoordinator.CrossServiceAssociationsClearing -= value;
        }

        public static bool EnableCrossServiceLogForwarding
        {
            get => ServiceStateCoordinator.EnableCrossServiceLogForwarding;
            set => ServiceStateCoordinator.EnableCrossServiceLogForwarding = value;
        }

        /// <summary>
        /// Gets or sets the serialized representation of protocol options for persistence.
        /// </summary>
        [JsonInclude]
        public Dictionary<string, JsonElement> SerializedOptions
        {
            get => OptionsState.SerializedOptions;
            set => OptionsState.SerializedOptions = value;
        }

        public ServiceListModel(IMessageRoutingService routingService)
        {
            _routingService = routingService ?? throw new ArgumentNullException(nameof(routingService));

            MetricsState = new ServiceMetricsState();
            OptionsState = new ServiceOptionsState();
            LogState = new ServiceLogState(MetricsState, MaxLogEntries);

            MetricsState.PropertyChanged += OnMetricsStatePropertyChanged;
            LogState.PropertyChanged += OnLogStatePropertyChanged;
            LogState.LogAdded += OnLogStateLogAdded;

            MetricsState.ResetMessageCounts();
            LogState.UpdateIdentity(_type, _displayName);
            OptionsState.ServiceType = _type;
            OptionsState.DescriptorId = _descriptorId;

            SetRoutingAttribute(InputMessageAttributeName, LogState.InputMessage, MessageRoutingDirection.Input, publish: false);
            SetRoutingAttribute(OutputMessageAttributeName, LogState.OutputMessage, MessageRoutingDirection.Output, publish: false);
            SetRoutingAttribute(RuntimeStateAttributeName, _runtimeState.ToString(), publish: false);
            SetRoutingAttribute(IncomingMessageCountAttributeName, MetricsState.IncomingMessageCount.ToString(CultureInfo.InvariantCulture), publish: false);
            SetRoutingAttribute(OutgoingMessageCountAttributeName, MetricsState.OutgoingMessageCount.ToString(CultureInfo.InvariantCulture), publish: false);
        }

        public double? AverageExecutionTimeMs => MetricsState.AverageExecutionTimeMs;

        public TimeSpan LastExecutionDuration => MetricsState.LastExecutionDuration;

        public string ExecutionTimeText => MetricsState.ExecutionTimeText;

        public int IncomingMessageCount => MetricsState.IncomingMessageCount;

        public int OutgoingMessageCount => MetricsState.OutgoingMessageCount;

        public string InputMessage => LogState.InputMessage;

        public string OutputMessage => LogState.OutputMessage;

        public WpfBrush LastInputBrush => LogState.LastInputBrush;

        public string LastLogMessage => LogState.LastLogMessage;

        public WpfBrush LastLogBrush => LogState.LastLogBrush;

        public event Action<bool>? ActiveChanged;

        public event Action<ServiceListModel, LogEntry>? LogAdded;

        public double TotalExecutionTimeMs
        {
            get => MetricsState.TotalExecutionTimeMs;
            set => MetricsState.TotalExecutionTimeMs = value;
        }

        public int ExecutionCount
        {
            get => MetricsState.ExecutionCount;
            set => MetricsState.ExecutionCount = value;
        }

        public void InitializeMessageCounts(int incoming, int outgoing)
        {
            if (!LogState.HasMessageHistory)
            {
                MetricsState.ResetMessageCounts();
                return;
            }

            MetricsState.InitializeMessageCounts(incoming, outgoing);
        }

        public void ResetMessageCounts()
        {
            MetricsState.ResetMessageCounts();
        }

        public string UpdateInputMessage(string? message, WpfBrush? brush = null)
        {
            return LogState.UpdateInputMessage(message, brush);
        }

        public string UpdateOutputMessage(string? message)
        {
            return LogState.UpdateOutputMessage(message);
        }

        public void RecordMessageHistory(string? incomingMessage, string? outgoingMessage, string? destination, DateTime timestamp)
        {
            LogState.RecordMessageHistory(incomingMessage, outgoingMessage, destination, timestamp);
        }

        public IReadOnlyList<ServiceMessageHistoryEntry> GetMessageHistorySnapshot()
        {
            return LogState.GetMessageHistorySnapshot();
        }

        public void LoadMessageHistory(IEnumerable<ServiceMessageHistoryEntry> entries)
        {
            LogState.LoadMessageHistory(entries);
        }

        public void LoadPersistedLogs(IEnumerable<LogEntry> entries)
        {
            LogState.LoadPersistedLogs(entries);
        }

        public void RecordExecutionTime(TimeSpan duration)
        {
            MetricsState.RecordExecutionTime(duration);
        }

        public void SetOptions<TOptions>(TOptions? options, string? key = null)
            where TOptions : class
        {
            OptionsState.SetOptions(options, key);
        }

        /// <summary>
        /// Retrieves strongly typed options previously registered for the service.
        /// </summary>
        /// <typeparam name="TOptions">The expected options type.</typeparam>
        /// <param name="key">Optional override for the storage key.</param>
        /// <returns>The stored options instance, or <c>null</c> when unavailable.</returns>
        public TOptions? GetOptions<TOptions>(string? key = null)
            where TOptions : class
        {
            return OptionsState.GetOptions<TOptions>(key);
        }

        internal bool TryApplySerializedOptions(IServiceOptionsSerializer serializer, JsonElement payload, string? key = null)
        {
            return OptionsState.TryApplySerializedOptions(serializer, payload, key);
        }

        /// <summary>
        /// Retrieves existing options or creates a new instance via the provided factory.
        /// </summary>
        /// <typeparam name="TOptions">The expected options type.</typeparam>
        /// <param name="factory">Factory used when no stored options exist.</param>
        /// <param name="key">Optional override for the storage key.</param>
        /// <returns>The resolved options instance.</returns>
        public TOptions GetOrCreateOptions<TOptions>(Func<TOptions> factory, string? key = null)
            where TOptions : class
        {
            return OptionsState.GetOrCreateOptions(factory, key);
        }

        #region Legacy option bindings

        [JsonPropertyName("TcpOptions")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TcpServiceOptions? LegacyTcpOptions
        {
            get => null;
            set => SetOptions(value);
        }

        [JsonPropertyName("FtpOptions")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public FtpServerOptions? LegacyFtpOptions
        {
            get => null;
            set => SetOptions(value);
        }

        [JsonPropertyName("HttpOptions")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public HttpServiceOptions? LegacyHttpOptions
        {
            get => null;
            set => SetOptions(value);
        }

        [JsonPropertyName("HidOptions")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public HidServiceOptions? LegacyHidOptions
        {
            get => null;
            set => SetOptions(value);
        }

        [JsonPropertyName("HeartbeatOptions")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public HeartbeatServiceOptions? LegacyHeartbeatOptions
        {
            get => null;
            set => SetOptions(value);
        }

        [JsonPropertyName("FileObserverOptions")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public FileObserverServiceOptions? LegacyFileObserverOptions
        {
            get => null;
            set => SetOptions(value);
        }

        [JsonPropertyName("ScpOptions")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ScpServiceOptions? LegacyScpOptions
        {
            get => null;
            set => SetOptions(value);
        }

        [JsonPropertyName("CsvOptions")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CsvServiceOptions? LegacyCsvOptions
        {
            get => null;
            set => SetOptions(value);
        }

        [JsonPropertyName("MqttOptions")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public MqttServiceOptions? LegacyMqttOptions
        {
            get => null;
            set => SetOptions(value);
        }

        #endregion

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged();
                    if (_isActive)
                    {
                        AddLog("[Service Activated]", WpfBrushes.Green);
                    }
                    else
                    {
                        if (RuntimeState != ServiceRuntimeState.Error)
                        {
                            RuntimeState = ServiceRuntimeState.Inactive;
                        }
                        AddLog("[Service Deactivated]", WpfBrushes.Red);
                    }
                    ActiveChanged?.Invoke(_isActive);
                }
            }
        }

        internal void InitializeActivationState(bool isActive, bool notify = false)
        {
            var stateChanged = _isActive != isActive;
            _isActive = isActive;

            if (isActive)
            {
                SetRuntimeState(ServiceRuntimeState.Active);
            }
            else if (RuntimeState != ServiceRuntimeState.Error)
            {
                SetRuntimeState(ServiceRuntimeState.Inactive);
            }

            if (stateChanged && notify)
            {
                OnPropertyChanged(nameof(IsActive));
                ActiveChanged?.Invoke(_isActive);
            }
        }

        public void SetRuntimeState(ServiceRuntimeState state)
        {
            RuntimeState = state;
        }

        private ServiceRuntimeState _runtimeState = ServiceRuntimeState.Inactive;
        public ServiceRuntimeState RuntimeState
        {
            get => _runtimeState;
            private set
            {
                var changed = _runtimeState != value;
                _runtimeState = value;
                if (changed)
                {
                    OnPropertyChanged();
                }

                SetRoutingAttribute(RuntimeStateAttributeName, _runtimeState.ToString());
            }
        }

        public ObservableCollection<LogEntry> Logs => LogState.Logs;

        public void AddLog(string message, WpfBrush? color = null, LogLevel level = LogLevel.Debug, bool checkReference = true)
        {
            var brush = color ?? WpfBrushes.Black;
            LogState.AddLog(message, brush, level);
            if (checkReference)
            {
                ServiceStateCoordinator.TryForwardLog(this, message ?? string.Empty, brush, level);
            }
        }

        internal void AddForwardedLog(string message, WpfBrush brush, LogLevel level)
        {
            LogState.AddLog(message, brush, level);
        }

        private void OnLogStateLogAdded(LogEntry entry)
        {
            LogAdded?.Invoke(this, entry);
        }

        private void OnLogStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ServiceLogState.InputMessage):
                    OnPropertyChanged(nameof(InputMessage));
                    SetRoutingAttribute(InputMessageAttributeName, LogState.InputMessage, MessageRoutingDirection.Input);
                    break;
                case nameof(ServiceLogState.OutputMessage):
                    OnPropertyChanged(nameof(OutputMessage));
                    SetRoutingAttribute(OutputMessageAttributeName, LogState.OutputMessage, MessageRoutingDirection.Output);
                    break;
                case nameof(ServiceLogState.LastInputBrush):
                    OnPropertyChanged(nameof(LastInputBrush));
                    break;
                case nameof(ServiceLogState.LastLogMessage):
                    OnPropertyChanged(nameof(LastLogMessage));
                    break;
                case nameof(ServiceLogState.LastLogBrush):
                    OnPropertyChanged(nameof(LastLogBrush));
                    break;
                case nameof(ServiceLogState.Logs):
                    OnPropertyChanged(nameof(Logs));
                    break;
            }
        }

        private void OnMetricsStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ServiceMetricsState.TotalExecutionTimeMs):
                    OnPropertyChanged(nameof(TotalExecutionTimeMs));
                    OnPropertyChanged(nameof(AverageExecutionTimeMs));
                    OnPropertyChanged(nameof(ExecutionTimeText));
                    break;
                case nameof(ServiceMetricsState.ExecutionCount):
                    OnPropertyChanged(nameof(ExecutionCount));
                    OnPropertyChanged(nameof(AverageExecutionTimeMs));
                    OnPropertyChanged(nameof(ExecutionTimeText));
                    break;
                case nameof(ServiceMetricsState.LastExecutionDuration):
                    OnPropertyChanged(nameof(LastExecutionDuration));
                    OnPropertyChanged(nameof(ExecutionTimeText));
                    break;
                case nameof(ServiceMetricsState.IncomingMessageCount):
                    OnPropertyChanged(nameof(IncomingMessageCount));
                    SetRoutingAttribute(IncomingMessageCountAttributeName, MetricsState.IncomingMessageCount.ToString(CultureInfo.InvariantCulture));
                    break;
                case nameof(ServiceMetricsState.OutgoingMessageCount):
                    OnPropertyChanged(nameof(OutgoingMessageCount));
                    SetRoutingAttribute(OutgoingMessageCountAttributeName, MetricsState.OutgoingMessageCount.ToString(CultureInfo.InvariantCulture));
                    break;
            }
        }

        internal void EnsureAssociation(ServiceListModel target)
        {
            if (!AssociatedServices.Contains(target.DisplayName))
            {
                AssociatedServices.Add(target.DisplayName);
            }

            if (!target.AssociatedServices.Contains(DisplayName))
            {
                target.AssociatedServices.Add(DisplayName);
            }
        }

        public void PublishRoutingAttribute(string attributeName, string? value, MessageRoutingDirection? direction = null)
        {
            SetRoutingAttribute(attributeName, value, direction, publish: true);
        }

        public void ClearRoutingAttribute(string attributeName)
        {
            SetRoutingAttribute(attributeName, null, direction: null, publish: true);
        }

        internal void RestoreRoutingAttributes(IDictionary<string, string>? attributes)
        {
            _routingAttributes.Clear();

            if (attributes is null)
            {
                return;
            }

            foreach (var pair in attributes)
            {
                if (string.IsNullOrWhiteSpace(pair.Key))
                {
                    continue;
                }

                SetRoutingAttribute(pair.Key, pair.Value ?? string.Empty, direction: null, publish: false);
            }
        }

        internal Dictionary<string, string> GetRoutingAttributesSnapshot()
        {
            return _routingAttributes.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.Value,
                StringComparer.OrdinalIgnoreCase);
        }

        internal void RepublishRoutingAttributes()
        {
            if (string.IsNullOrWhiteSpace(_displayName))
            {
                return;
            }

            foreach (var pair in _routingAttributes)
            {
                PublishNormalizedAttribute(pair.Key, pair.Value.Value, pair.Value.Direction);
            }
        }

        internal void ClearRoutingAttributes()
        {
            if (!string.IsNullOrWhiteSpace(_displayName))
            {
                _routingService.ClearService(Type, _displayName);
                _routingService.ClearService(_displayName);
            }

            _routingAttributes.Clear();
        }

        private void SetRoutingAttribute(string attributeName, string? value, MessageRoutingDirection? direction = null, bool publish = true)
        {
            if (string.IsNullOrWhiteSpace(attributeName))
            {
                throw new ArgumentException("Attribute name cannot be null or whitespace.", nameof(attributeName));
            }

            var normalized = attributeName.Trim();

            if (value is null)
            {
                if (_routingAttributes.Remove(normalized, out var existing) && publish)
                {
                    ClearNormalizedAttribute(normalized, existing.Direction);
                }

                return;
            }

            var resolvedDirection = direction ?? InferDirection(normalized);
            if (!resolvedDirection.HasValue &&
                _routingAttributes.TryGetValue(normalized, out var current) &&
                current.Direction.HasValue)
            {
                resolvedDirection = current.Direction;
            }

            var resolvedValue = value;
            _routingAttributes[normalized] = new RoutingAttributeValue(resolvedValue, resolvedDirection);

            if (publish)
            {
                PublishNormalizedAttribute(normalized, resolvedValue, resolvedDirection);
            }
        }

        private void PublishNormalizedAttribute(string attributeName, string value, MessageRoutingDirection? direction)
        {
            if (string.IsNullOrWhiteSpace(_displayName))
            {
                return;
            }

            if (direction.HasValue)
            {
                _routingService.UpdateMessage(Type, _displayName, value, direction.Value);
            }
            else
            {
                _routingService.PublishAttribute(Type, _displayName, attributeName, value);
            }
        }

        private void ClearNormalizedAttribute(string attributeName, MessageRoutingDirection? direction)
        {
            if (string.IsNullOrWhiteSpace(_displayName))
            {
                return;
            }

            _routingService.ClearAttribute(Type, _displayName, attributeName);
        }

        private static MessageRoutingDirection? InferDirection(string attributeName)
        {
            if (string.Equals(attributeName, InputMessageAttributeName, StringComparison.OrdinalIgnoreCase))
            {
                return MessageRoutingDirection.Input;
            }

            if (string.Equals(attributeName, OutputMessageAttributeName, StringComparison.OrdinalIgnoreCase))
            {
                return MessageRoutingDirection.Output;
            }

            return null;
        }

        public void ApplyPresentation(ServicePresentationMetadata metadata)
        {
            var normalized = ServicePresentationMetadata.Normalize(metadata);
            BackgroundColor = ServiceLogState.ParseBrush(normalized.PrimaryAccentColor, WpfBrushes.LightGray);
            BorderColor = ServiceLogState.ParseBrush(normalized.SecondaryAccentColor, WpfBrushes.Gray);
            IconGlyph = normalized.IconGlyph;
            DescriptorLabel = normalized.DisplayLabel;
            OnPropertyChanged(nameof(BackgroundColor));
            OnPropertyChanged(nameof(BorderColor));
        }
    }
}

