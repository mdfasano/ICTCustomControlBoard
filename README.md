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
  
### 2. Install the Board manager server to run locally
1. [Download]() from GitHub
2. extract the project files
3. open a Powershell window and navigate to the project folder
4. run the following commands in Powershell

```
dotnet restore
dotnet publish IctCustomControlBoard.csproj -c Release -r win-x86 --self-contained false -o .\publish
cd Publish
.\IctCustomControlBoard.exe
```
5. you should now have a server running and see text like 
`Board Manager Server Running...`

### 3. Include the IctCustomControlBoard library as a dependency in your project
- This allows you to send requests to the server via a custom `BoardRequest` object, detailed in the [usage](#usage) section 


## Usage
The Board Manager Server exposes an API via **Named Pipes** that allows clients to send commands to the connected boards. Communication is **JSON-based**, and requests are encapsulated in a **`BoardRequest` object**.

A `BoardRequest` object contains the following fields:

| Field        | Type     | Description |
|--------------|---------|-------------|
| `BoardNumber`| int     | Number of the board you want to communicate with. Corresponds with `Dev1` through `Dev4` |
| `Command`    | string  | The action to perform. Valid commands are: `GetBits`, `SetBits`, `GetVoltage`, `GetIOID`. |
| `PortNumber` | int     | Digital port number, corresponding to `port0`, `port1`, and `port2`. Required for digital I/O commands. |
| `Value`      | int     | Value to write to the specified port. Accepts `0–255`. Required only for `SetBits`. |
| `Channel`    | int     | Analog input channel number. Accepts `0-7` Required only for `GetVoltage`. |

<details>
	<summary>examples</summary>

**Example JSON request to set digital bits:**
```json
{
  "BoardNumber": 1,
  "Command": "BoardCommand.SetBits",
  "Port": 0,
  "Value": 255
}
```

**Example JSON request to read a voltage:**
```json
{
  "BoardIndex": 3,
  "Command": "GetVoltage",
  "Channel": 0
}
```

</details>

The server responds with a JSON object containing the results of the request:


| Field     | Type        | Description                                                              |
| --------- | ----------- | ------------------------------------------------------------------------ |
| `Success` | bool        | `true` if the operation succeeded, `false` otherwise.                    |
| `Bits`    | int/null    | Value read from a digital port (if `GetBits`), `null` otherwise.         |
| `Voltage` | double/null | Voltage read from an analog channel (if `GetVoltage`), `null` otherwise. |
| `Message` | string      | A human-readable message or error description.                           |

<details>
	<summary>examples</summary>

**Example JSON response after setting digital bits:**
```json
{
  "Success": true,
  "Bits": null,
  "Voltage": null,
  "Message": "Wrote 0xFF to port0"
}
```
**Example JSON response after setting a voltage:**
```json
{
  "Success": true,
  "Bits": null,
  "Voltage": 3.276,
  "Message": "Read voltage from channel 0"
}
```
</details>

## Usage Notes
- You cannot read from an output port, and you cannot write to an input port
- Invalid read/write requests will return a response object containing `Success: false` along with an explainatory message
