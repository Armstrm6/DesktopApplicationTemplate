using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Csv;
using DesktopApplicationTemplate.Core.Services.Protocols.Ftp;
using DesktopApplicationTemplate.Core.Services.Protocols.Http;
using DesktopApplicationTemplate.Core.Services.Protocols.Tcp;
using DesktopApplicationTemplate.Core.Services.Protocols.FileObserver;
using DesktopApplicationTemplate.Core.Services.Protocols.Heartbeat;
using DesktopApplicationTemplate.Core.Services.Protocols.Hid;
using DesktopApplicationTemplate.Core.Services.Protocols.Mqtt;
using DesktopApplicationTemplate.Core.Services.Protocols.Scp;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.ViewModels;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using DesktopApplicationTemplate.UI;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.Persistence
{
    public static class ServicePersistence
    {
        public static string FilePath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "services.json");

        public static void Save(IEnumerable<ServiceListModel> services, ILoggingService? logger = null)
        {
            var data = new List<ServiceInfo>();
            var index = 0;
            foreach (var s in services)
            {
                TcpServiceOptions? tcp = null;
                CsvServiceOptions? csv = null;
                FtpServerOptions? ftp = null;
                HttpServiceOptions? http = null;
                HeartbeatServiceOptions? heartbeat = null;
                FileObserverServiceOptions? fileObserver = null;
                HidServiceOptions? hid = null;
                ScpServiceOptions? scp = null;
                var tcpSource = s.GetOptions<TcpServiceOptions>();
                if (s.Type == ServiceType.Tcp && tcpSource != null)
                {
                    tcp = new TcpServiceOptions
                    {
                        Host = tcpSource.Host,
                        Port = tcpSource.Port,
                        UseUdp = tcpSource.UseUdp,
                        SubnetMask = tcpSource.SubnetMask,
                        PrimaryDns = tcpSource.PrimaryDns,
                        AlternateDns = tcpSource.AlternateDns,
                        Mode = tcpSource.Mode,
                        ConnectionRole = tcpSource.ConnectionRole,
                        DestinationHost = tcpSource.DestinationHost,
                        DestinationPort = tcpSource.DestinationPort,
                        DestinationGateway = tcpSource.DestinationGateway,
                        DestinationSubnetMask = tcpSource.DestinationSubnetMask,
                        DestinationPrimaryDns = tcpSource.DestinationPrimaryDns,
                        DestinationAlternateDns = tcpSource.DestinationAlternateDns,
                        InputMessage = tcpSource.InputMessage,
                        Script = tcpSource.Script,
                        OutputMessage = tcpSource.OutputMessage,
                        LastTestMessage = tcpSource.LastTestMessage
                    };
                }

                var ftpSource = s.GetOptions<FtpServerOptions>();
                if (s.Type == ServiceType.Ftp && ftpSource != null)
                {
                    ftp = new FtpServerOptions
                    {
                        Port = ftpSource.Port,
                        RootPath = ftpSource.RootPath,
                        AllowAnonymous = ftpSource.AllowAnonymous,
                        Username = ftpSource.Username,
                        Password = ftpSource.Password
                    };
                }

                var httpSource = s.GetOptions<HttpServiceOptions>();
                if (s.Type == ServiceType.Http && httpSource != null)
                {
                    http = new HttpServiceOptions
                    {
                        BaseUrl = httpSource.BaseUrl,
                        Username = httpSource.Username,
                        Password = httpSource.Password,
                        ClientCertificatePath = httpSource.ClientCertificatePath
                    };
                }
                var csvSource = s.GetOptions<CsvServiceOptions>();
                if (s.Type == ServiceType.Csv && csvSource != null)
                {
                    csv = new CsvServiceOptions
                    {
                        OutputPath = csvSource.OutputPath ?? string.Empty,
                        Delimiter = csvSource.Delimiter ?? ",",
                        IncludeHeaders = csvSource.IncludeHeaders ?? true
                    };
                }

                var heartbeatSource = s.GetOptions<HeartbeatServiceOptions>();
                if (s.Type == ServiceType.Heartbeat && heartbeatSource != null)
                {
                    heartbeat = new HeartbeatServiceOptions
                    {
                        BaseMessage = heartbeatSource.BaseMessage,
                        IncludePing = heartbeatSource.IncludePing,
                        IncludeStatus = heartbeatSource.IncludeStatus
                    };
                }

                var fileObserverSource = s.GetOptions<FileObserverServiceOptions>();
                if (s.Type == ServiceType.FileObserver && fileObserverSource != null)
                {
                    fileObserver = new FileObserverServiceOptions
                    {
                        FilePath = fileObserverSource.FilePath,
                        ImageNames = fileObserverSource.ImageNames,
                        SendAllImages = fileObserverSource.SendAllImages,
                        SendFirstX = fileObserverSource.SendFirstX,
                        XCount = fileObserverSource.XCount,
                        SendTcpCommand = fileObserverSource.SendTcpCommand,
                        TcpCommand = fileObserverSource.TcpCommand
                    };
                }

                var hidSource = s.GetOptions<HidServiceOptions>();
                if (s.Type == ServiceType.Hid && hidSource != null)
                {
                    hid = new HidServiceOptions
                    {
                        MessageTemplate = hidSource.MessageTemplate,
                        UsbProtocol = hidSource.UsbProtocol,
                        AttachedService = hidSource.AttachedService,
                        DebounceTimeMs = hidSource.DebounceTimeMs,
                        KeyDownTimeMs = hidSource.KeyDownTimeMs
                    };
                }

                var scpSource = s.GetOptions<ScpServiceOptions>();
                if (s.Type == ServiceType.Scp && scpSource != null)
                {
                    scp = new ScpServiceOptions
                    {
                        Host = scpSource.Host,
                        Port = scpSource.Port,
                        Username = scpSource.Username,
                        Password = scpSource.Password,
                        LocalPath = scpSource.LocalPath,
                        RemotePath = scpSource.RemotePath
                    };
                }

                MqttServiceOptions? mqtt = null;
                var mqttSource = s.GetOptions<MqttServiceOptions>();
                if (s.Type == ServiceType.Mqtt && mqttSource != null)
                {
                    mqtt = new MqttServiceOptions
                    {
                        Host = mqttSource.Host,
                        Port = mqttSource.Port,
                        ClientId = mqttSource.ClientId,
                        Username = mqttSource.Username,
                        Password = mqttSource.Password,
                        ConnectionType = mqttSource.ConnectionType,
                        WebSocketPath = mqttSource.WebSocketPath,
                        ClientCertificate = mqttSource.ClientCertificate?.ToArray(),
                        WillTopic = mqttSource.WillTopic,
                        WillPayload = mqttSource.WillPayload,
                        WillQualityOfService = mqttSource.WillQualityOfService,
                        WillRetain = mqttSource.WillRetain,
                        KeepAliveSeconds = mqttSource.KeepAliveSeconds,
                        CleanSession = mqttSource.CleanSession,
                        ReconnectDelay = mqttSource.ReconnectDelay
                    };
                }

                data.Add(new ServiceInfo
                {
                    DisplayName = s.DisplayName,
                    DescriptorId = s.DescriptorId,
                    ServiceType = s.Type,
                    IsActive = s.IsActive,
                    Created = DateTime.Now,
                    Order = index++,
                    AssociatedServices = new List<string>(s.AssociatedServices),
                    TcpOptions = tcp,
                    FtpOptions = ftp,
                    HttpOptions = http,
                    CsvOptions = csv,
                    HeartbeatOptions = heartbeat,
                    FileObserverOptions = fileObserver,
                    HidOptions = hid,
                    ScpOptions = scp,
                    MqttOptions = mqtt,
                    TotalExecutionTimeMs = s.TotalExecutionTimeMs,
                    ExecutionCount = s.ExecutionCount,
                    IncomingMessageCount = s.IncomingMessageCount,
                    OutgoingMessageCount = s.OutgoingMessageCount,
                    Logs = s.Logs
                        .Take(ServiceListModel.MaxLogEntries)
                        .Select(l => new LogEntry
                        {
                            Level = l.Level,
                            Message = l.Message,
                            Color = l.Color,
                            ServiceType = l.ServiceType ?? s.Type,
                            ServiceName = string.IsNullOrWhiteSpace(l.ServiceName) ? s.DisplayName : l.ServiceName
                        })
                        .ToList(),
                    MessageHistory = s.GetMessageHistorySnapshot().ToList()
                });
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            try
            {
                var json = JsonSerializer.Serialize(data, options);
                logger?.Log($"Persisting services to {FilePath}", LogLevel.Debug);
                var directory = Path.GetDirectoryName(FilePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                using var fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                using var sw = new StreamWriter(fs);
                sw.Write(json);
                logger?.Log($"Saved {data.Count} services to {FilePath}", LogLevel.Debug);
            }
            catch (StackOverflowException)
            {
                var dumpOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    ReferenceHandler = ReferenceHandler.Preserve
                };
                var dump = JsonSerializer.Serialize(data, dumpOptions);
                var temp = Path.Combine(Path.GetTempPath(), "services_dump.json");
                File.WriteAllText(temp, dump);
                Environment.FailFast($"Stack overflow while saving services. Dump written to {temp}");
            }
        }

        public static List<ServiceInfo> Load(ILoggingService? logger = null)
        {
            if (!File.Exists(FilePath))
            {
                logger?.Log("Services file not found", LogLevel.Warning);
                return new List<ServiceInfo>();
            }

            string json;
            try
            {
                json = File.ReadAllText(FilePath);
            }
            catch (FileNotFoundException)
            {
                logger?.Log("Services file not found", LogLevel.Warning);
                return new List<ServiceInfo>();
            }

            try
            {
                var services = JsonSerializer.Deserialize<List<ServiceInfo>>(json) ?? new List<ServiceInfo>();

                foreach (var info in services)
                {
                    info.AssociatedServices ??= new List<string>();
                    info.Logs ??= new List<LogEntry>();
                    info.MessageHistory ??= new List<ServiceMessageHistoryEntry>();
                }

                foreach (var info in services)
                {
                    if (info.ServiceType == ServiceType.Mqtt && info.MqttOptions is null)
                    {
                        var opt = App.AppHost?.Services.GetService<IOptions<MqttServiceOptions>>();
                        if (opt != null)
                        {
                            var value = opt.Value;
                            info.MqttOptions = new MqttServiceOptions
                            {
                                Host = value.Host,
                                Port = value.Port,
                                ClientId = value.ClientId,
                                Username = value.Username,
                                Password = value.Password,
                                ConnectionType = value.ConnectionType,
                                WebSocketPath = value.WebSocketPath,
                                ClientCertificate = value.ClientCertificate?.ToArray(),
                                WillTopic = value.WillTopic,
                                WillPayload = value.WillPayload,
                                WillQualityOfService = value.WillQualityOfService,
                                WillRetain = value.WillRetain,
                                KeepAliveSeconds = value.KeepAliveSeconds,
                                CleanSession = value.CleanSession,
                                ReconnectDelay = value.ReconnectDelay
                            };
                        }
                        else
                        {
                            info.MqttOptions = new MqttServiceOptions();
                        }
                    }

                    if (info.ServiceType == ServiceType.Tcp && info.TcpOptions is null)
                    {
                        info.TcpOptions = new TcpServiceOptions();
                    }

                    if (info.ServiceType == ServiceType.Ftp && info.FtpOptions is null)
                    {
                        info.FtpOptions = new FtpServerOptions();
                    }

                    if (info.ServiceType == ServiceType.Http && info.HttpOptions is null)
                    {
                        info.HttpOptions = new HttpServiceOptions();
                    }

                    if (info.ServiceType == ServiceType.Heartbeat && info.HeartbeatOptions is null)
                    {
                        info.HeartbeatOptions = new HeartbeatServiceOptions();
                    }

                    if (info.ServiceType == ServiceType.FileObserver && info.FileObserverOptions is null)
                    {
                        info.FileObserverOptions = new FileObserverServiceOptions();
                    }

                    if (info.ServiceType == ServiceType.Hid && info.HidOptions is null)
                    {
                        info.HidOptions = new HidServiceOptions();
                    }

                    if (info.ServiceType == ServiceType.Scp && info.ScpOptions is null)
                    {
                        info.ScpOptions = new ScpServiceOptions();
                    }
                }

                logger?.Log($"Loaded {services.Count} services", LogLevel.Debug);
                return services;
            }
            catch (JsonException)
            {
                logger?.Log("Failed to parse services file", LogLevel.Error);
                return new List<ServiceInfo>();
            }
            catch
            {
                logger?.Log("Failed to parse services file", LogLevel.Error);
                return new List<ServiceInfo>();
            }
        }
    }

    public class ServiceInfo
    {
        public string DisplayName { get; set; } = string.Empty;
        public string? DescriptorId { get; set; }
        public ServiceType ServiceType { get; set; }
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
        public int Order { get; set; }
        public List<string> AssociatedServices { get; set; } = new();
        public TcpServiceOptions? TcpOptions { get; set; }
        public FtpServerOptions? FtpOptions { get; set; }
        public HttpServiceOptions? HttpOptions { get; set; }
        public CsvServiceOptions? CsvOptions { get; set; }
        public HeartbeatServiceOptions? HeartbeatOptions { get; set; }
        public FileObserverServiceOptions? FileObserverOptions { get; set; }
        public HidServiceOptions? HidOptions { get; set; }
        public ScpServiceOptions? ScpOptions { get; set; }
        public MqttServiceOptions? MqttOptions { get; set; }
        public double TotalExecutionTimeMs { get; set; }
        public int ExecutionCount { get; set; }
        public int IncomingMessageCount { get; set; }
        public int OutgoingMessageCount { get; set; }
        public List<LogEntry> Logs { get; set; } = new();
        public List<ServiceMessageHistoryEntry> MessageHistory { get; set; } = new();
    }

}
