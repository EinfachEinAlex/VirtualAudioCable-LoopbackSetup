﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VirtualAudioCable_CS.Audio;
using VirtualAudioCable_CS.Audio.Device;
using VirtualAudioCable_CS.Audio.Enums;
using VirtualAudioCable_CS.Audio.Exception;
using VirtualAudioCable_CS.Audio.Property;
using VirtualAudioCable_CS.Audio.Struct;
using VirtualAudioCable_CS.Configuration;
using VirtualAudioCable_CS.Utils;
using VirtualAudioCable_CS.Utils.Output;

namespace VirtualAudioCable_CS
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args != null)
            {
                if (args.Length == 0)
                {
                    OutputWriter.Write("Please enter some arguments");
                    OutputWriter.Write("Available arguments are: set, reset");
                    return;
                }

                if (args[0] != null)
                {
                    SetupManager s = new SetupManager();
                    PropertyKeyStore pKS = new PropertyKeyStore();

                    switch (args[0])
                    {
                        case "set":
                            Set(s, pKS);
                            break;

                        case "reset":
                            Reset(s);
                            break;

                        default:
                            OutputWriter.Write("Please enter some arguments");
                            OutputWriter.Write("Available arguments are: set, reset");
                            break;
                    }
                }
            }
        }
        
        public static void Reset(SetupManager s)
        {
            if (Configurator.IsConfigurationPresent())
            {
                Configurator configurator = new Configurator();
                s.RevertToDefaultSettings(configurator.Read());

                if (configurator.RemoveConfiguration())
                {
                    OutputWriter.Write("Reverted everything back to normal", OutputType.Info);
                }
                else
                {
                    OutputWriter.Write("Reverted everything back to normal, but could not reset the Registry configuration", OutputType.Info);
                    OutputWriter.Write("It is located at \"HKEY_CURRENT_USER\\SOFTWARE\\VirtualAudioCable-CS\"", OutputType.Info);
                }
            }
            else
            {
                OutputWriter.Write("Could not find any configuration information", OutputType.Error);
            }
        }

        public static void Set(SetupManager s, PropertyKeyStore pKS)
        {
            if (Configurator.IsConfigurationPresent())
            {
                OutputWriter.Write("VirtualAudioCable is already configurated! Think about it!\n Do you want to continue? (Y/N)");

                string answer = Console.ReadLine();

                if (answer == null)
                    return;

                if (!(answer.Contains("Y") || answer.Contains("y")))
                {
                    return;
                }

                Console.Clear();
            }

            OutputWriter.Write("Setting VirtualAudioCable as default output devices", OutputType.Info);

            try
            {
                if (s.PrimaryDevice.PropertyStore.Contains(pKS.
                    DEVICE_INTERFACE_FRIENDLY_NAME))
                {
                    OutputWriter.Write(string.Format("Detected {0} as default audio output",
                            s.PrimaryDevice.PropertyStore[pKS.DEVICE_INTERFACE_FRIENDLY_NAME].Value.ToString()),
                        OutputType.Info);

                    if (s.VirtualAudioCableDevice.PropertyStore.Contains(pKS
                        .DEVICE_INTERFACE_FRIENDLY_NAME))
                    {
                        OutputWriter.Write(string.Format("Detected {0} as VirtualAudio input",
                                s.VirtualAudioCableDevice.PropertyStore[pKS.DEVICE_INTERFACE_FRIENDLY_NAME].Value.ToString()),
                            OutputType.Info);

                        OutputWriter.Write("Finalizing steps", OutputType.Info);

                        if (s.PrimaryDevice.PropertyStore.Contains(pKS
                            .SET_PRIMARY_AUDIO_LOOPBACK_DEVICES))
                        {
                            string primaryDevice = s.PrimaryDevice
                                .PropertyStore[pKS.SET_PRIMARY_AUDIO_LOOPBACK_DEVICES]
                                .Value.ToString();

                            bool primaryDeviceBool = Convert.ToBoolean(primaryDevice);

                            ConfigStore configStore = new ConfigStore(s.GetDefaultLoopBackDevice(), primaryDeviceBool);
                            Configurator configurator = new Configurator(configStore);
                            configurator.Write();

                            s.SetADAsLoopbackSource();
                            s.AssignVACAsLoopbackDevice();
                        }
                        else
                        {
                            throw new SetupErrorException();
                        }

                        OutputWriter.Write("Finish!", OutputType.Info);

                    }
                    else
                    {
                        throw new SetupErrorException();
                    }
                }
                else
                {
                    throw new SetupErrorException();
                }
            }
            catch (Exception e)
            {
                OutputWriter.Write(e.Message, OutputType.Error);
            }
        }
    }
}
