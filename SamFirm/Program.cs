namespace SamFirm
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;
    using MySql.Data.MySqlClient;
    internal static class Program
    {
        private static SamFirm.Command.Firmware FW;
        [STAThread]
        private static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Thread.Sleep(200);              
                ConnectMysql connect = new ConnectMysql();
                MySqlConnection conn = connect.Initialize();
                while (true)
                {
                    List<FirmwareInfo> firmwareInfos = getListFirmwareInfo(conn);
                    for (int i = 0; i < firmwareInfos.Count; i++)
                    {
                        Console.WriteLine(firmwareInfos[i].Model);
                        FW = Command.UpdateCheckAuto(firmwareInfos[i].Model, firmwareInfos[i].Region, false);
                        if (!string.IsNullOrEmpty(FW.Filename))
                        {
                            Command.Download2(FW, "C:\\Users\\chungnh\\Desktop\\FirmWare\\" + FW.Filename, true);
                            Console.WriteLine("Download finished", false);
                            decrypt_button_Click(FW, "C:\\Users\\chungnh\\Desktop\\FirmWare\\" + FW.Filename);
                        }
                    }
                    Thread.Sleep(1000);
                }
            }
            else
            {
                Utility.run_by_cmd = true;
                Environment.Exit(CmdLine.Main(args));
            }
            return 0;
        }

        private static void decrypt_button_Click(SamFirm.Command.Firmware FW, string des)
        {
            if (!System.IO.File.Exists(des))
            {
                Console.WriteLine("Error: File does not exist", false);
            }
            else
            {
                    Thread.Sleep(100);
                    Console.WriteLine("\nDecrypting firmware...", false);
                    if (des.EndsWith(".enc2"))
                    {
                        Crypto.SetDecryptKey(FW.Region, FW.Model, FW.Version);
                    }
                    else if (des.EndsWith(".enc4"))
                    {
                        if (FW.BinaryNature == 1)
                        {
                            Crypto.SetDecryptKey(FW.Version, FW.LogicValueFactory);
                        }
                        else
                        {
                            Crypto.SetDecryptKey(FW.Version, FW.LogicValueHome);
                        }
                    }
                    if (Crypto.Decrypt(des, Path.Combine(Path.GetDirectoryName(des), Path.GetFileNameWithoutExtension(des)), true) == 0)
                    {
                        System.IO.File.Delete(des);
                    }
                    Console.WriteLine("Decryption finished", false);         
                };            
            
        }

        private static List<FirmwareInfo> getListFirmwareInfo(MySqlConnection conn)
        {
            List<FirmwareInfo> mylist = new List<FirmwareInfo>();
            try
            {
                conn.Open();
                string stm = "select * from firmware.firmware_info;";
                MySqlCommand cmd = new MySqlCommand(stm, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    FirmwareInfo temp = new FirmwareInfo();
                    temp.Id = rdr.GetInt32(0);
                    temp.Model = rdr.GetString(1);
                    temp.Region = rdr.GetString(2);
                    mylist.Add(temp);
                }
                conn.Close();
                return mylist;
            }
            catch (Exception e)
            {
                Console.Write(e);
            }
            return mylist;
        }
        private static void SendEnterToParent()
        {
            Imports.EnumWindows(delegate (IntPtr wnd, IntPtr param) {
                uint lpdwProcessId = 0;
                Imports.GetWindowThreadProcessId(wnd, out lpdwProcessId);
                Process parentProcess = Imports.ParentProcessUtilities.GetParentProcess();
                if (lpdwProcessId == parentProcess.Id)
                {
                    Imports.SendMessage(wnd, 0x102, (IntPtr) 13, IntPtr.Zero);
                    return false;
                }
                return true;
            }, IntPtr.Zero);
        }
    }
}

