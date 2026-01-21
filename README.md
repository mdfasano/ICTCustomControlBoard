# IctCustomControlBoard Library

## Overview
**IctCustomControlBoard** is a .NET library for interfacing with **National Instruments USB DAQ devices**, specifically `USB-6501` and `USB-6002`.  
It provides programmatic access to digital I/O ports and analog input channels.
The library wraps NI DAQ devices in convenient classes:

---

## Features
- Supports multiple NI devices (USB-6501, USB-6002)  
- Handles digital I/O (`GetBits`, `SetBits`)  
- Handles analog input (`GetVoltage`)  

---

## Architecture
```
Your Application
|
IctCustomControlBoard Library
├── BoardManager.cs
├── CustomBoard.cs
└── National Instruments DAQmx backend
```

## BoardManager API

The `BoardManager` class provides the main interface to interact with connected boards. The following methods are available:

| Method | Description |
|--------|-------------|
| `SetBits(ulong bits)` | Accepts a pattern of 8-bit chunks as a `ulong`. The bits are parsed and written to the digital output ports of the board. |
| `GetBits()` | Returns all digital port values packaged as a single `ulong`. Each chunk of 8 bits represents the state of a port. |
| `GetVoltages()` | Returns a tuple `(v1, v2)` containing the voltages from the two analog input channels. |
| `GetBoardInfo()` | Returns an array of four `BoardInfo` structs, each containing metadata about the connected boards. |

