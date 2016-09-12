#if WINDOWS
using System.Windows.Forms;
#endif

using System;
using LabNation.DeviceInterface.Devices;
using LabNation.Common;
using LabNation.DeviceInterface.DataSources;
using System.Threading;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using LabNation.DeviceInterface.Hardware;


namespace HackerSpecialTest
{
    class MainClass
    {
        /// <summary>
        /// The DeviceManager detects device connections
        /// </summary>
        static DeviceManager hsManager;
        static HackerSpecial device;
        static bool running = true;

        [STAThread]
        static void Main(string[] args)
        {
            //Open logger on console window
            FileLogger consoleLog = new FileLogger(new StreamWriter(Console.OpenStandardOutput()), LogLevel.INFO);

            Logger.Info("LabNation Hacker Special Demo");
            Logger.Info("---------------------------------");

            //Set up device manager with a device connection handler (see below)

            hsManager = new DeviceManager(null, connectHandler, new Dictionary<Type,Type>() { { typeof(ISmartScopeInterface), typeof(HackerSpecial) } });
            hsManager.Start();

            ConsoleKeyInfo cki = new ConsoleKeyInfo();
            byte offset = 0;

            while (running)
            {
#if WINDOWS
                Application.DoEvents();
#endif
                Thread.Sleep(100);

                if (Console.KeyAvailable)
                {
                    cki = Console.ReadKey(true);
                    HandleKey(cki);

                    if (device != null)
                    {
                        if (cki.Key == ConsoleKey.R)
                        {
                            Logger.Info("Reading ROM");
                            for (uint i = 0; i < device.FpgaRom.Registers.Count; i++)
                            {
                                byte val = device.FpgaRom[i].Read().GetByte();
                                Logger.Info("Reg {0} = 0x{1:X}", i, val);
                            }
                        }

                        if (cki.Key == ConsoleKey.U)
                        {
                            Logger.Info("testing user bank");
                            offset++;
                            for (uint i = 0; i < 30; i++)
                            {
                                byte val = device.FpgaUserMemory[i].Read().GetByte();
                                byte newval = (byte)(i + offset);
                                Logger.Info("Reading user mem {0} = 0x{1:X} - updating with 0x{2:X}", i, val, newval);
                                device.FpgaUserMemory[i].WriteImmediate(newval);
                            }
                        }

                        if (cki.Key == ConsoleKey.P)
                        {
                            Logger.Info("testing register bank");
                            offset++;
                            for (uint i = 0; i < 30; i++)
                            {
                                byte val = device.FpgaSettingsMemory[i].Read().GetByte();
                                byte newval = (byte)(255 - i + offset);
                                Logger.Info("Reading user mem {0} = 0x{1:X} - updating with 0x{2:X}", i, val, newval);
                                device.FpgaSettingsMemory[i].WriteImmediate(newval);
                            }
                        }

                        if (cki.Key == ConsoleKey.Enter)
                        {
                            Console.Write("Reg? >");
                            string regStr = Console.ReadLine();
                            try
                            {
                                uint reg = uint.Parse(regStr);
                                string valStr = "";
                                do
                                {
                                    Console.Write("Val? >");
                                    valStr = Console.ReadLine();
                                    byte val;
                                    try
                                    {
                                        val = byte.Parse(valStr);
                                        device.FpgaSettingsMemory[reg].WriteImmediate(val);
                                        Console.WriteLine(String.Format("Write to reg {0}: 0x{1:X}", reg, val));
                                    }
                                    catch (Exception e)
                                    {
                                        val = device.FpgaSettingsMemory[reg].Read().GetByte();
                                        Console.WriteLine(String.Format("Read reg {0}: 0x{1:X}", reg, val));
                                    }
                                } while (valStr != "");
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Failed to parse input");
                            }

                            
                        }

                        if (cki.Key == ConsoleKey.A)
                        {
                            if (cki.Modifiers.HasFlag(ConsoleModifiers.Shift))
                            {
                                Logger.Info("Resetting ADC regs");
                                device.WriteAdcReg(0xA, 0x5A);
                            }
                            else if (cki.Modifiers.HasFlag(ConsoleModifiers.Control))
                            {
                                Logger.Info("Configuring ADC");
                                device.ConfigureAdc();
                            }
                            else
                            {
                                Logger.Info("Reading ADC regs");
                                for (int i = 0; i < 9; i++)
                                {
                                    byte val = device.ReadAdcReg((uint)i);
                                    Logger.Info("Reading ADC mem {0} = 0x{1:X}", i, val);
                                }
                            }
                        }

                        if (cki.Key == ConsoleKey.D)
                        {
                            Logger.Info("Fetching scope data");
                            byte[] data = device.iface.GetData(64);
                            Logger.Info("Got {0:d} bytes", data.Length);
                            for (int i = 0; i < data.Length; i++)
                            {
                                Logger.LogC(LogLevel.INFO, String.Format("{0,2:X0} ", data[i]), ConsoleColor.Green);
                                if (i % 16 == 15)
                                    Logger.LogC(LogLevel.INFO, "\n");
                            }
                        }
                    }
                }
            }
            Logger.Info("Stopping device manager");
            hsManager.Stop();
            Logger.Info("Stopping Logger");
            consoleLog.Stop();
        }

        static void connectHandler(IDevice dev, bool connected)
        {
            //Only accept devices of the IScope type (i.e. not IWaveGenerator)
            //and block out the fallback device (dummy scope)
            if (connected && dev is HackerSpecial && !(dev is DummyScope))
            {
                Logger.Info("Device connected of type " + dev.GetType().Name + " with serial " + dev.Serial);
                device = (HackerSpecial)dev;
                device.ConfigureAdc();
                Usage();
            }
            else
            {
                device = null;
            }
            
        }

        private static void Usage()
        {
            Logger.Info("?          : Display this message");
            Logger.Info("Q/X/Esc    : Quit");
            Logger.Info("U          : Test user register bank");
            Logger.Info("R          : Test FPGA ROM");
            Logger.Info("P          : Test settings register bank");
            Logger.Info("D          : Tetch FPGA data");
            Logger.Info("------------------------------------------");
        }

        static void HandleKey(ConsoleKeyInfo k)
        {
            if (k.KeyChar == '?')
            {
                Usage();
                return;
            }
            switch (k.Key)
            {
                case ConsoleKey.Q:
                case ConsoleKey.X:
                case ConsoleKey.Escape:
                    running = false;
                    break;

            }
        }

    }
}