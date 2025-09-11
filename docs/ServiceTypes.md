# Service Types

The application uses the `ServiceType` enum to categorize built-in services. The table below lists each value and the service it represents.

| Enum value | Service description |
|------------|--------------------|
| `Mqtt` | Publishes and subscribes to topics using the MQTT protocol. |
| `Http` | Sends HTTP requests with configurable headers and body content. |
| `Ftp` | Transfers files using the File Transfer Protocol. |
| `Hid` | Handles Human Interface Devices and forwards input to other services. |
| `Csv` | Generates CSV files from values produced by other services. |
| `FileObserver` | Watches directories for new files and can trigger service actions. |
| `Scp` | Moves files over SSH using the SCP protocol. |
| `Tcp` | Hosts a TCP server for testing message processing scripts. |
| `Heartbeat` | Emits periodic heartbeat messages for health monitoring. |

