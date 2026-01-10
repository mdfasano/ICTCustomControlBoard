# Board Manager Server

## Overview
This project provides a background service that exposes National Instruments USB DAQ devices (`Dev1`–`Dev4`) through a **Named Pipe JSON API**.  
It allows other applications to read/write digital ports and query analog input voltages on demand.

The companion **client** project (`BoardClient`) demonstrates how to communicate with the server.

---

## Features
- Supports multiple NI devices (USB-6501, USB-6002)
- Handles digital I/O (`GetBits`, `SetBits`)
- Handles analog input (`GetVoltage`)
- Uses Named Pipes for lightweight local IPC
- Includes JSON-based request/response model
- Compatible with .NET 8 (x86)

---

## Architecture
```
[Client App]
│
│ JSON requests via Named Pipe ("BoardPipe")
▼
[Board Manager Server]
├── BoardManagerServer.cs
├── CustomDIOBoard.cs
├── CustomAIBoard.cs
└── National Instruments DAQmx backend
```

Each connected NI device (Dev1 – Dev4) is wrapped by a `CustomDIOBoard` (for the USB-6501) or `CustomAIBoard` (for the USB-6002) instance.  
The server listens on a named pipe and executes commands requested by clients.

---

## Requirements
- Windows 7 or later
- .NET 8 SDK
- National Instruments **DAQmx** drivers installed
- Platform target: x86

---

## Installation & Setup

### 1. Install NI-DAQmx
- [Download](https://www.ni.com/en/support/downloads/drivers/download.ni-daq-mx.html#577117) and install from National Instruments’ website.

- Verify your devices appear in **NI MAX** under *Devices and Interfaces*.
  - The Devices should be named `Dev1`, `Dev2`, `Dev3`, and `Dev4`
  - Device numbers should map to the numbers used in the [project design spreadsheet](https://docs.google.com/spreadsheets/d/1UBV6FewfMPBUVl1tw0vITm_1WhsL8d6j/edit?gid=1023092147#gid=1023092147)
  
### 2. Build the Server
```bash
dotnet build -c Debug
