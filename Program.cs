using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Memory;
using System.Text.RegularExpressions;
using System.IO;
using System.Globalization;
using WindowsInput;
using System.Diagnostics;

namespace Intralism_Ehrenlos
{
    class Program
    {
        static void Main(string[] args)
        {
            InputSimulator sim = new InputSimulator();
            Mem m = new Mem();
            int ProcID = m.getProcIDFromName("Intralism.exe");

            float maximaleMapZeit = 0f;
            float aktuelleMapZeit = 0f;

            string foldername = "";
            string referenceworkshopid = "";
            string workshopid;
            string s = "";
            string WhatToPress;
            string TimingTime;

            bool firstrun = false;
            bool _playing = false;
            bool addingconfigfinished = false;
            bool informationstarted = false;

            bool mapgestartet = false;

            int min = 5;
            int max = 25;

            int mindelay = 10;
            int maxdelay = 20;

            int min2 = 0;
            int max2 = 5;

            int arcammount = 0;


            List<string> config = new List<string>();
            List<string> timepoints1 = new List<string>();
            List<string> timepoints2 = new List<string>();

            Thread _relaxThread = new Thread(RelaxThread);


            if (ProcID > 0)
            {
                m.OpenProcess(ProcID);

                Thread Background = new Thread(ReadMemory);
                Background.Start();
            }
            //string procName = Process.GetCurrentProcess().ProcessName;
            ProcessHelper.SetFocusToExternalApp("Intralism");
            void ReadMemory()
            {
                while (true)
                {
                    try
                    {
                        aktuelleMapZeit = m.readFloat("mono.dll+0x00269560,58,260,30,20,118");
                        maximaleMapZeit = m.readFloat("mono.dll+0x00269560,58,260,30,20,110");
                        workshopid = m.readString("mono.dll+0x0026AC30,A0,420,30,90,20,14", "", 40);
                    }
                    catch
                    {
                        Console.Write(string.Format(" {0:HH:mm:ss tt}", DateTime.Now));
                        Console.WriteLine("   -   Error while reading data! Trying again...");
                        Thread.Sleep(100);
                        continue;
                    }

                    workshopid = workshopid.Replace("workshop.", "");

                    if (referenceworkshopid != workshopid)
                    {
                        referenceworkshopid = workshopid;
                        OpenConfig();
                    }

                    if (mapgestartet == false)
                    {
                        if (aktuelleMapZeit > 0f)
                        {
                            mapgestartet = true;
                            OpenConfig();
                        }
                    }

                    if (mapgestartet == true)
                    {
                        if (aktuelleMapZeit >= maximaleMapZeit)
                        {
                            mapgestartet = false;
                        }
                    }


                        if (informationstarted == false)
                    {
                        informationstarted = true;
                        Thread Information = new Thread(InformationOutput);
                        Information.Start();
                    }
                }
            }

            void Prepare()
            {
                try
                {
                    if (!Directory.Exists(@"C:/test/" + foldername))
                    {
                        Directory.CreateDirectory(@"C:/test/" + foldername);

                        Console.Write("Enter the minimum Offset you want to use: ");
                        min = Int32.Parse(Console.ReadLine());
                        Console.Write("Enter the maximum Offset you want to use: ");
                        max = Int32.Parse(Console.ReadLine());

                        firstrun = false;
                    }

                    else
                    {
                        firstrun = false;
                    }
                }
                catch
                {
                    Console.Write(string.Format(" {0:HH:mm:ss tt}", DateTime.Now));
                    Console.WriteLine("   -   Couldn't prepare your firstrun!");
                }
            }

            void InformationOutput()
            {
                while (true)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Clear();
                    Console.WriteLine();
                    Console.WriteLine("        (%%%                                                                                                        ");
                    Console.WriteLine("     %%%%%    %%             %%  %%  #%  %%%%%% #%%%%*    %%%    %#    #%   %%%%  *%%,   %%.  %%%%%    %%%%  *%%%%%,");
                    Console.WriteLine("    %%%%    %%    %          %%  %%/ #%    %%   #%  (%    %%%,   %#    #%  %%  %( *%%%  #%%.  %%  %%  %%  %%    %*  ");
                    Console.WriteLine("   %%%    %%    %%%%         %%  %#% #%    %%   #%  (%   ,% %%   %#    #%  %%*    *%%%  %%%.  %%  %%  %%  %%    %*  ");
                    Console.WriteLine("  .%    %%    %%%%%%         %%  %*%##%    %%   #%%%%(   %% %%   %#    #%    %%(  *%*%(*%,%.  %%%%%%  %%  %%    %*  ");
                    Console.WriteLine("      %%    %%%%%%%%         %%  %* %%%    %%   #% .%(   %%%%%.  %#    #%      %% *%*#%%%,%.  %%  (%( %%  %%    %*  ");
                    Console.WriteLine("    %%    %%                 %%  %* (%%    %%   #%  %%  ,%,  %%  %#    #%  %%  %% *%* %% ,%.  %% .%%. #%, %%    %*  ");
                    Console.WriteLine("        %%    ((((           ((  (,  ((    ((   /(  .(( ((   ((  ((((( /(    (/   ,(, (( .(.  ((((.     *(      (,  ");
                    Console.WriteLine("       ,    %%%.                                                                                                    ");
                    Console.WriteLine("");

                    Prepare();

                    Console.WriteLine("\n EHRENLOS - INTRALISM BOT");
                    Console.WriteLine("\n ZEIT : {0} / {1}", aktuelleMapZeit, maximaleMapZeit);
                    Console.WriteLine(" MAPID : {0}", workshopid);
                    Console.WriteLine("\n\n\n LOGS:");
                    Thread.Sleep(1000);
                }
            }

            void OpenConfig()
            {
                try
                {
                    string configpath = @"C:\Program Files (x86)\Steam\steamapps\workshop\content\513510\" + workshopid + @"\config.txt";
                    //string configpath = @"C:\Program Files (x86)\Steam\steamapps\workshop\content\513510\1750124831\config.txt";



                    if (workshopid != null && workshopid != "UnityEngine.Events.U")
                    {
                        using (StreamReader sr = File.OpenText(configpath))
                        {
                            while ((s = sr.ReadLine()) != null)
                            {
                                config.Add(s);
                                Thread.Sleep(1);
                            }
                        }
                        addingconfigfinished = true;
                        AddTimepoints();
                    }
                }
                catch
                {
                    Console.Write(string.Format(" {0:HH:mm:ss tt}", DateTime.Now));
                    Console.WriteLine("   -   An error occured while opening the config. Retrying...");
                    Process.Start(System.AppDomain.CurrentDomain.FriendlyName);
                    Environment.Exit(0);

                }
            }

            void AddTimepoints()
            {
                if (addingconfigfinished == true)
                {
                    string pattern = @"{""time"":([0-9.]*),""data"":\[""SpawnObj"",""\[([A-z-]*)\],?[0-9]?""]}";                
                    string input = "";

                    foreach (string item3 in config)
                    {
                        input = input + item3;
                    }


                    RegexOptions options = RegexOptions.Multiline | RegexOptions.IgnoreCase;

                    foreach (Match x in Regex.Matches(input, pattern, options))
                    {
                        timepoints1.Add(x.Groups[1].ToString());
                        timepoints2.Add(x.Groups[2].ToString());
                    }

                    _relaxThread.Start();
                    _playing = true;
                }
            }

            void RelaxThread()
            {
                while (_playing)
                {
                    for (int i = 0; i < timepoints1.Count(); i++)
                    {
                        //int wait = new Random().Next(10,35);

                        // min = new Random().Next(0, 5);      
                        // max = new Random().Next(30, 35);     

                        TimingTime = timepoints1[i];

                        while (aktuelleMapZeit <= (float.Parse(TimingTime, CultureInfo.InvariantCulture)) - 0.0015)
                        {
                            Thread.Sleep(1);
                        }

                        WhatToPress = timepoints2[i];
                        Console.Write(string.Format(" {0:HH:mm:ss tt}", DateTime.Now));
                        Console.WriteLine("   -   {0}   -   {1} got clicked!", float.Parse(TimingTime, CultureInfo.InvariantCulture), WhatToPress);

                        arcammount = 0;

                        if (WhatToPress.Contains("Right"))
                        {
                            arcammount = arcammount + 1;
                        }
                        if (WhatToPress.Contains("Up"))
                        {
                            arcammount = arcammount + 1;
                        }
                        if (WhatToPress.Contains("Down"))
                        {
                            arcammount = arcammount + 1;
                        }
                        if (WhatToPress.Contains("Left"))
                        {
                            arcammount = arcammount + 1;
                        }

                        if (arcammount > 1)
                        {
                            if (WhatToPress.Contains("Right"))
                            {
                                sim.Keyboard.Sleep(new Random().Next(min2, max2));
                                sim.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.RIGHT);
                                sim.Keyboard.Sleep(new Random().Next(mindelay, maxdelay));
                                sim.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.RIGHT);
                                sim.Keyboard.Sleep(new Random().Next(min2, max2));
                            }
                            if (WhatToPress.Contains("Up"))
                            {
                                sim.Keyboard.Sleep(new Random().Next(min2, max2));
                                sim.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.UP);
                                sim.Keyboard.Sleep(new Random().Next(mindelay, maxdelay));
                                sim.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.UP);
                                sim.Keyboard.Sleep(new Random().Next(min2, max2));
                            }
                            if (WhatToPress.Contains("Down"))
                            {
                                sim.Keyboard.Sleep(new Random().Next(min2, max2));
                                sim.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.DOWN);
                                sim.Keyboard.Sleep(new Random().Next(mindelay, maxdelay));
                                sim.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.DOWN);
                                sim.Keyboard.Sleep(new Random().Next(min2, max2));
                            }
                            if (WhatToPress.Contains("Left"))
                            {
                                sim.Keyboard.Sleep(new Random().Next(min2, max2));
                                sim.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.LEFT);
                                sim.Keyboard.Sleep(new Random().Next(mindelay, maxdelay));
                                sim.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.LEFT);
                                sim.Keyboard.Sleep(new Random().Next(min2, max2));
                            }
                        }
                        else
                        {
                            if (WhatToPress.Contains("Right"))
                            {
                                sim.Keyboard.Sleep(new Random().Next(min, max));
                                sim.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.RIGHT);
                                sim.Keyboard.Sleep(new Random().Next(mindelay, maxdelay));
                                sim.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.RIGHT);
                                sim.Keyboard.Sleep(new Random().Next(min, max));
                            }
                            if (WhatToPress.Contains("Up"))
                            {
                                sim.Keyboard.Sleep(new Random().Next(min, max));
                                sim.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.UP);
                                sim.Keyboard.Sleep(new Random().Next(mindelay, maxdelay));
                                sim.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.UP);
                                sim.Keyboard.Sleep(new Random().Next(min, max));
                            }
                            if (WhatToPress.Contains("Down"))
                            {
                                sim.Keyboard.Sleep(new Random().Next(min, max));
                                sim.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.DOWN);
                                sim.Keyboard.Sleep(new Random().Next(mindelay, maxdelay));
                                sim.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.DOWN);
                                sim.Keyboard.Sleep(new Random().Next(min, max));
                            }
                            if (WhatToPress.Contains("Left"))
                            {
                                sim.Keyboard.Sleep(new Random().Next(min, max));
                                sim.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.LEFT);
                                sim.Keyboard.Sleep(new Random().Next(mindelay, maxdelay));
                                sim.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.LEFT);
                                sim.Keyboard.Sleep(new Random().Next(min, max));
                            }
                        }
                    }
                    break; 
                }
            }
        }
    }
}
