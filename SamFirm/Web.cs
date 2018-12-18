namespace SamFirm
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using MySql.Data.MySqlClient;
    //using SamFirm;
    internal class Web
    {
        //public static Form1 form;
        public static string JSessionID = string.Empty;
        public static string Nonce = string.Empty;

        public static int DownloadBinary2(Command.Firmware f, string saveTo, bool GUI = true)
        {
            int num5;
            long num = 0L;
            HttpWebRequest wr = KiesRequest.Create("http://cloud-neofussvr.sslcs.cdngc.net/NF_DownloadBinaryForMass.do?file=" + f.Path + f.Filename);
            String link = "http://cloud-neofussvr.sslcs.cdngc.net/NF_DownloadBinaryForMass.do?file=" + f.Path + f.Filename;
            wr.Method = "GET";
            wr.Headers["Authorization"] = Imports.GetAuthorization(Nonce).Replace("Authorization: ", "").Replace("nonce=\"", "nonce=\"" + Nonce);
            String GetAuthorization = Imports.GetAuthorization(Nonce).Replace("Authorization: ", "").Replace("nonce=\"", "nonce=\"" + Nonce);

            //Console.WriteLine("\n" + link + "\nGetAuthorization: " + GetAuthorization);

            wr.Timeout = 0x61a8;
            wr.ReadWriteTimeout = 0x61a8;
            if (System.IO.File.Exists(saveTo))
            {
                long length = new FileInfo(saveTo).Length;
                if (long.Parse(f.Size) == length)
                {
                    Console.Write("File already downloaded.", false);
                    return 200;
                }
                Console.Write("File exists. Resuming download...", false);
                wr.AddRange((int)length);
                num = length;
            }
            using (HttpWebResponse response = (HttpWebResponse)wr.GetResponseFUS())
            {
                if (response == null)
                {
                    Console.Write("Error downloading: response is null", false);
                    return 0x385;
                }
                if ((response.StatusCode != HttpStatusCode.OK) && (response.StatusCode != HttpStatusCode.PartialContent))
                {
                    Console.Write("Error downloading: " + ((int)response.StatusCode), false);
                }
                else
                {
                    long total = long.Parse(response.GetResponseHeader("content-length")) + num;
                    string file = saveTo.Replace(".enc4", "");
                    if ((!System.IO.File.Exists(saveTo) && !System.IO.File.Exists(file)) /*|| (new FileInfo(saveTo).Length != total)*/)
                    {
                        //MySqlConnection conn = new MySqlConnection("datasource=localhost; port=3307; database=firmware; User Id=root; password=toor");
                        ConnectMysql connect = new ConnectMysql();
                        MySqlConnection conn = connect.Initialize();
                        conn.Open();
                        MySqlCommand cmd = new MySqlCommand();
                        cmd.Connection = conn;
                        cmd.CommandText = "INSERT INTO firmware.samsung(model, displayName, Version, OS, LastModified, fileName, path, size, crc, modelType, region, BinaryNature, LogicValueFactory, LogicValueHome, Announce, ConnectionError, FetchAttempts) " +
                        "VALUES(@model, @displayName, @Version, @OS, @LastModified, @fileName, @path, @size, @crc, @modelType, @region, @BinaryNature, @LogicValueFactory, @LogicValueHome, @Announce, @ConnectionError, @FetchAttempts)";
                        cmd.Parameters.AddWithValue("@model", f.Model);
                        cmd.Parameters.AddWithValue("@displayName", f.DisplayName);
                        cmd.Parameters.AddWithValue("@Version", f.Version);
                        cmd.Parameters.AddWithValue("@OS", f.OS);
                        cmd.Parameters.AddWithValue("@LastModified", f.LastModified);
                        cmd.Parameters.AddWithValue("@fileName", f.Filename);
                        cmd.Parameters.AddWithValue("@path", saveTo.Replace(".enc4", ""));
                        cmd.Parameters.AddWithValue("@size", f.Size);
                        cmd.Parameters.AddWithValue("@crc", f.CRC);
                        cmd.Parameters.AddWithValue("@modelType", f.Model_Type);
                        cmd.Parameters.AddWithValue("@region", f.Region);
                        cmd.Parameters.AddWithValue("@BinaryNature", f.BinaryNature);
                        cmd.Parameters.AddWithValue("@LogicValueFactory", f.LogicValueFactory);
                        cmd.Parameters.AddWithValue("@LogicValueHome", f.LogicValueHome);
                        cmd.Parameters.AddWithValue("@Announce", f.Announce);
                        cmd.Parameters.AddWithValue("@ConnectionError", f.ConnectionError);
                        cmd.Parameters.AddWithValue("@FetchAttempts", f.FetchAttempts);
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        byte[] buffer = new byte[0x2000];
                        Stopwatch sw = new Stopwatch();
                        Utility.ResetSpeed(num);
                        try
                        {
                            Utility.PreventDeepSleep(Utility.PDSMode.Start);
                            using (BinaryWriter writer = new BinaryWriter(new FileStream(saveTo, FileMode.Append)))
                            {
                                int count = 0;
                                do
                                {
                                    Utility.PreventDeepSleep(Utility.PDSMode.Continue);
                                    //if (GUI && form.PauseDownload)
                                    //{
                                    //    goto Label_02BB;
                                    //}
                                    num += count = response.GetResponseStream().Read(buffer, 0, buffer.Length);
                                    if (count > 0)
                                    {
                                        writer.Write(buffer, 0, count);
                                        if (GUI)
                                        {
                                            int dlspeed = Utility.DownloadSpeed(num, sw);
                                            //if (dlspeed != -1)
                                            //{
                                            //    form.lbl_speed.Invoke((Action)(delegate {
                                            //        form.lbl_speed.Text = dlspeed + "kB/s";
                                            //    }));
                                            //}
                                        }
                                    }
                                    if (GUI)
                                    {
                                        //form.SetProgressBar(Utility.GetProgress(num, total));
                                    }
                                    else
                                    {
                                        CmdLine.SetProgress(Utility.GetProgress(num, total));
                                    }
                                }
                                while (count > 0);
                            }
                        }
                        catch (IOException exception)
                        {
                            Console.Write("Error: Can't access output file " + saveTo, false);
                            if (GUI)
                            {
                                //form.PauseDownload = true;
                            }
                            Console.Write(exception.ToString(), false);
                            return -1;
                        }
                        catch (WebException)
                        {
                            Console.Write("Error: Connection interrupted", false);
                            SetReconnect();
                        }
                        finally
                        {
                            Utility.PreventDeepSleep(Utility.PDSMode.Stop);
                            if (GUI)
                            {
                                //form.lbl_speed.Invoke((Action)(delegate {
                                //    form.lbl_speed.Text = "0kB/s";
                                //}));
                            }
                        }
                    }
                }
                //Label_02BB:
                num5 = (int)response.StatusCode;
            }
            return num5;
        }
        public static int DownloadBinaryInform(string xml, out string xmlresponse) =>
            XMLFUSRequest("https://neofussvr.sslcs.cdngc.net/NF_DownloadBinaryInform.do", xml, out xmlresponse);

        public static int DownloadBinaryInit(string xml, out string xmlresponse) =>
            XMLFUSRequest("https://neofussvr.sslcs.cdngc.net/NF_DownloadBinaryInitForMass.do", xml, out xmlresponse);

        public static int GenerateNonce()
        {
            HttpWebRequest wr = KiesRequest.Create("https://neofussvr.sslcs.cdngc.net/NF_DownloadGenerateNonce.do");
            wr.Method = "POST";
            wr.ContentLength = 0L;
            using (HttpWebResponse response = (HttpWebResponse)wr.GetResponseFUS())
            {
                if (response == null)
                {
                    return 0x385;
                }
                HttpStatusCode statusCode = response.StatusCode;
                return (int)response.StatusCode;
            }
        }

        public static void SetReconnect()
        {
            if (!Utility.run_by_cmd)
            {
                //if (!form.PauseDownload)
                //{
                //    Utility.ReconnectDownload = true;
                //}
                //form.PauseDownload = true;
            }
            else
            {
                Utility.ReconnectDownload = true;
            }
        }
        public static int DownloadBinary(string path, string file, string saveTo, string size, bool GUI = true)
        {
            int num5;
            long num = 0L;
            HttpWebRequest wr = KiesRequest.Create("http://cloud-neofussvr.sslcs.cdngc.net/NF_DownloadBinaryForMass.do?file=" + path + file);
            String link = "http://cloud-neofussvr.sslcs.cdngc.net/NF_DownloadBinaryForMass.do?file=" + path + file;
            wr.Method = "GET";
            wr.Headers["Authorization"] = Imports.GetAuthorization(Nonce).Replace("Authorization: ", "").Replace("nonce=\"", "nonce=\"" + Nonce);
            String GetAuthorization = Imports.GetAuthorization(Nonce).Replace("Authorization: ", "").Replace("nonce=\"", "nonce=\"" + Nonce);

            //Console.WriteLine("\n" + link + "\nGetAuthorization: " + GetAuthorization);

            wr.Timeout = 0x61a8;
            wr.ReadWriteTimeout = 0x61a8;
            if (System.IO.File.Exists(saveTo))
            {
                long length = new FileInfo(saveTo).Length;
                if (long.Parse(size) == length)
                {
                    Console.Write("File already downloaded.", false);
                    return 200;
                }
                Console.Write("File exists. Resuming download...", false);
                wr.AddRange((int)length);
                num = length;
            }
            using (HttpWebResponse response = (HttpWebResponse)wr.GetResponseFUS())
            {
                if (response == null)
                {
                    Console.Write("Error downloading: response is null", false);
                    return 0x385;
                }
                if ((response.StatusCode != HttpStatusCode.OK) && (response.StatusCode != HttpStatusCode.PartialContent))
                {
                    Console.Write("Error downloading: " + ((int)response.StatusCode), false);
                }
                else
                {
                    long total = long.Parse(response.GetResponseHeader("content-length")) + num;
                    if (!System.IO.File.Exists(saveTo) || (new FileInfo(saveTo).Length != total))
                    {
                        byte[] buffer = new byte[0x2000];
                        Stopwatch sw = new Stopwatch();
                        Utility.ResetSpeed(num);
                        try
                        {
                            Utility.PreventDeepSleep(Utility.PDSMode.Start);
                            using (BinaryWriter writer = new BinaryWriter(new FileStream(saveTo, FileMode.Append)))
                            {
                                int count = 0;
                                do
                                {
                                    Utility.PreventDeepSleep(Utility.PDSMode.Continue);
                                    //if (GUI && form.PauseDownload)
                                    //{
                                    //    goto Label_02BB;
                                    //}
                                    num += count = response.GetResponseStream().Read(buffer, 0, buffer.Length);
                                    if (count > 0)
                                    {
                                        writer.Write(buffer, 0, count);
                                        if (GUI)
                                        {
                                            int dlspeed = Utility.DownloadSpeed(num, sw);
                                            if (dlspeed != -1)
                                            {
                                                //form.lbl_speed.Invoke((Action)(delegate {
                                                //    form.lbl_speed.Text = dlspeed + "kB/s";
                                                //}));
                                            }
                                        }
                                    }
                                    if (GUI)
                                    {
                                        //form.SetProgressBar(Utility.GetProgress(num, total));
                                    }
                                    else
                                    {
                                        CmdLine.SetProgress(Utility.GetProgress(num, total));
                                    }
                                }
                                while (count > 0);
                            }
                        }
                        catch (IOException exception)
                        {
                            Console.Write("Error: Can't access output file " + saveTo, false);
                            if (GUI)
                            {
                                //form.PauseDownload = true;
                            }
                            Console.Write(exception.ToString(), false);
                            return -1;
                        }
                        catch (WebException)
                        {
                            Console.Write("Error: Connection interrupted", false);
                            SetReconnect();
                        }
                        finally
                        {
                            Utility.PreventDeepSleep(Utility.PDSMode.Stop);
                            if (GUI)
                            {
                                //form.lbl_speed.Invoke((Action)(delegate {
                                //    form.lbl_speed.Text = "0kB/s";
                                //}));
                            }
                        }
                    }
                }
                //Label_02BB:
                num5 = (int)response.StatusCode;
            }
            return num5;
        }
        private static int XMLFUSRequest(string URL, string xml, out string xmlresponse)
        {
            xmlresponse = null;
            HttpWebRequest wr = KiesRequest.Create(URL);
            wr.Method = "POST";
            wr.Headers["Authorization"] = "FUS nonce=\"\", signature=\"" + Imports.GetAuthorization(Nonce) + "\", nc=\"\", type=\"\", realm=\"\"";
            byte[] bytes = Encoding.ASCII.GetBytes(Regex.Replace(xml, @"\r\n?|\n|\t", string.Empty));
            wr.ContentLength = bytes.Length;
            using (Stream stream = wr.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }
            using (HttpWebResponse response = (HttpWebResponse)wr.GetResponseFUS())
            {
                if (response == null)
                {
                    return 0x385;
                }
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    try
                    {
                        xmlresponse = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    }
                    catch (Exception)
                    {
                        return 900;
                    }
                }
                return (int)response.StatusCode;
            }
        }
    }
}

