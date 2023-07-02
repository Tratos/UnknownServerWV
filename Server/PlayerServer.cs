using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace Server
{
    public static class PlayerServer
    {
        public static readonly object _sync = new object();
        public static bool _exit;
        public static TcpListener lPlayers = (TcpListener)null;
        public static void Start()
        {
            SetExit(false);
            Log.Print("Starting PLAYERSERVER...");
            new Thread(new ParameterizedThreadStart(tHTTPMain)).Start();
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(10);
                Application.DoEvents();
            }
        }
        public static void Stop()
        {
            Log.Print("PLAYERSERVER stopping...");
            if (lPlayers != null) lPlayers.Stop();
            SetExit(true);
            Log.Print("PLAYERSERVER Stopped.");
        }
        public static void tHTTPMain(object obj)
        {
            try
            {
                Log.Print("PLAYERSERVER main loop running...");
                ushort port = Convert.ToUInt16(Config.settings["port_psrv"]);
                lPlayers = new TcpListener(IPAddress.Parse("0.0.0.0"), Convert.ToInt32(port));
                Log.Print("PLAYERSERVER Binding to 0.0.0.0:" + port + "...");
                lPlayers.Start();
                Log.Print("PLAYERSERVER Started listening.");
                TcpClient client;

                while (!GetExit())
                {
                    client = lPlayers.AcceptTcpClient();
                    NetworkStream ns = client.GetStream();
                    byte[] data = ReadContentTCP(ns);
                    try
                    {
                        ProcessPlayers(Encoding.ASCII.GetString(data), ns);
                    }
                    catch
                    {
                    }
                    client.Close();
                }

            }
            catch (Exception ex)
            {
              Log.Print("PLAYERSRV Error :" + ex);
            }
        }

        public static void ProcessPlayers(string data, Stream s)
        {
            string[] lines = data.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            Log.Print("PLAYERSRV Process:" + lines[0]);
            string cmd = lines[0].Split(' ')[0];
            string url = lines[0].Split(' ')[1].Split(':')[0];

            if (cmd == "GET")
            {
                switch (url)
                {
                        default:
                        {
                            Log.Print("PLAYERSRV: Function not implemented...");
                            break;
                        }
                }
            }

            if (cmd == "POST")
            {
                switch (url)
                {
                        default:
                        {
                            Log.Print("PLAYERSRV: Function not implemented...");
                            break;
                        }
                }
            }
        }
        public static byte[] ReadContentTCP(NetworkStream Stream)
        {
            MemoryStream res = new MemoryStream();
            byte[] buff = new byte[0x10000];
            Stream.ReadTimeout = 100;
            int bytesRead;
            try
            {
                while ((bytesRead = Stream.Read(buff, 0, 0x10000)) > 0)
                    res.Write(buff, 0, bytesRead);
            }
            catch { }
            Stream.Flush();
            return res.ToArray();
        }
        public static void ReplyWithXML(Stream s, string c)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("HTTP/1.1 200 OK");
            sb.AppendLine("Date: " + DateTime.Now.ToUniversalTime().ToString("r"));
            sb.AppendLine("Server: Warranty Voiders");
            sb.AppendLine("Content-Length: " + c.Length);
            sb.AppendLine("Keep-Alive: timeout=5, max=100");
            sb.AppendLine("Connection: Keep-Alive");
            sb.AppendLine();
            sb.Append(c);
            byte[] buf = Encoding.ASCII.GetBytes(sb.ToString());
            s.Write(buf, 0, buf.Length);
        }
        public static void ReplyWithJSON(Stream s, byte[] c)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("HTTP/1.1 200 OK");
            sb.AppendLine("Date: " + DateTime.Now.ToUniversalTime().ToString("r"));
            sb.AppendLine("Server: Warranty Voiders");
            sb.AppendLine("Content-Type: application/json; charset=UTF-8");
            sb.AppendLine("Content-Encoding: UTF-8");
            sb.AppendLine("Content-Length: " + c.Length);
            sb.AppendLine("Keep-Alive: timeout=5, max=100");
            sb.AppendLine("Connection: Keep-Alive");
            sb.AppendLine();
            byte[] buf = Encoding.ASCII.GetBytes(sb.ToString());
            s.Write(buf, 0, buf.Length);
            s.Write(c, 0, c.Length);
        }
        public static void ReplyWithText(Stream s, string c)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("HTTP/1.1 200 OK");
            sb.AppendLine("Date: " + DateTime.Now.ToUniversalTime().ToString("r"));
            sb.AppendLine("Server: Warranty Voiders");
            sb.AppendLine("Content-Type: text/html; charset=UTF-8");
            sb.AppendLine("Content-Encoding: UTF-8");
            sb.AppendLine("Content-Length: " + c.Length);
            sb.AppendLine("Keep-Alive: timeout=5, max=100");
            sb.AppendLine("Connection: close");
            sb.AppendLine();
            sb.Append(c);
            byte[] buf = Encoding.ASCII.GetBytes(sb.ToString());
            s.Write(buf, 0, buf.Length);
        }
        public static void ReplyWithBinary(Stream s, byte[] b)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("HTTP/1.1 200 OK");
            sb.AppendLine("Date: " + DateTime.Now.ToUniversalTime().ToString("r"));
            sb.AppendLine("Server: Warranty Voiders");
            sb.AppendLine("Content-Type: application/octet-stream");
            sb.AppendLine("Content-Length: " + b.Length);
            sb.AppendLine("Keep-Alive: timeout=5, max=100");
            sb.AppendLine("Connection: close");
            sb.AppendLine();
            byte[] buf = Encoding.ASCII.GetBytes(sb.ToString());
            s.Write(buf, 0, buf.Length);
            s.Write(b, 0, b.Length);
        }
        public static void SetExit(bool state)
        {
            lock (_sync)
            {
                _exit = state;
            }
        }
        public static bool GetExit()
        {
            bool result;
            lock (_sync)
            {
                result = _exit;
            }
            return result;
        }
    }
}
