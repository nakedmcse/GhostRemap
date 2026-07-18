# GhostRemap
Remap Ghost weapon keys for Ghost of Tsushima Legends to single press keys instead of hold R and number simultaneously.

## Configuration
Clone the repository and edit the following section to the keys you want to use for ghost weapon 1, 2 and 3.

```csharp
    // Virtual Key Codes
    // Data from https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
    private const int VK_HOME = 0x24;
    private const int VK_F = 0x46;
    private const int VK_B = 0x42;
    private const int VK_H = 0x48;
    
    // Mapping VK -> SC
    private static Dictionary<int, ushort> mapping = new Dictionary<int, ushort>()
    {
        { VK_F, SC_1 }, { VK_B, SC_2 }, { VK_H, SC_3 }
    };
```
The code defaults to F, B and H to ghost weapon 1,2 and 3.

> **IMPORTANT:** If you remap a key used in game for something else (EG V or G) then the key will no longer function in 
> game!

## Build
Open a terminal in the root of the project folder and issue the command:

```text
dotnet build -c Release
```

Then copy the contents of `bin\Release\net10.0-windows` to a folder on your drive.

It will probably be useful to drop a shortcut to `GhostRemap.exe` on the desktop.

> **NOTE:** This requires the .net 10 SDK installed to compile

## Usage
Before launching `Ghost of Tsushima Legends` run `GhostRemap.exe`.  It does not require elevated privileges, and will 
give no feedback it is running.  You can verify the process is running in task manager.

The `HOME` key can be used to disable and re-enable the mappings as required (the menus use F, so you will need to disable 
them when rolling item stats).

When you are done playing, use task manager to kill the process.