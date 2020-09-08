using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Xml;

namespace NetTask4
{
    class Program
    {
        public static Dictionary<string, int> ttl = new Dictionary<string, int>();
        public static Dictionary<string, string> ip = new Dictionary<string, string>();
        public static Dictionary<string, List<string>> dom = new Dictionary<string, List<string>>();
        public static List<string> info;
        public static void WriteIpToDomain()
        {
            FileStream fss = File.Open("inIp.txt", FileMode.Open, FileAccess.ReadWrite);
            fss.SetLength(0);
            fss.Close();
            using (var fs = new StreamWriter("inIp.txt"))
            {
                foreach (var en in ip)
                {

                    string s = (en.Key + "|" + en.Value +"|"+ ttl[en.Value]);
                    fs.WriteLine(s);
                }
            }
        }
        public static void WriteDomainToIp()
        {
            FileStream fss = File.Open("inDom.txt", FileMode.Open, FileAccess.ReadWrite);
            fss.SetLength(0);
            fss.Close();
            using (var fs = new StreamWriter("inDom.txt"))
            {
                foreach(var en in dom)
                {
                    StringBuilder s = new StringBuilder("");
                    s.Append(en.Key);
                    foreach(var ip in en.Value)
                    {
                        s.Append("|" + ip);
                    }
                    s.Append("|" +""+ ttl[en.Key]);
                    fs.WriteLine(s);
                    
                }
            }
        }
        

        public static void ReadIpToDomain()
        {
            if (System.IO.File.Exists("inIp.txt"))
            {
                using (StreamReader fs = new StreamReader("inIp.txt"))
                {
                    while (true)
                    {
                        var temp = fs.ReadLine();
                        if (temp == null) break;
                        var tempM = temp.Split('|');
                        ip[tempM[0]] = tempM[1];
                    }
                }
                FileStream fss = File.Open("inIp.txt", FileMode.Open, FileAccess.ReadWrite);
                fss.SetLength(0);
                fss.Close();
            }
            else
                using (StreamWriter fs = new StreamWriter("inIp.txt")) { }
        }
        public static void ReadDomainToIp()
        {
            if (System.IO.File.Exists("inDom.txt"))
            {
                using (StreamReader fs = new StreamReader("inDom.txt"))
                {
                    while (true)
                    {
                        // Читаем строку из файла во временную переменную.
                        var temp = fs.ReadLine();
                        if (temp == null) break;
                        var tempM = temp.Split('|');
                        dom[tempM[0]] = new List<string>();
                        for (int i = 1; i < tempM.GetLength(0) - 1; i++)
                        {
                            dom[tempM[0]].Add(tempM[i]);
                        }
                        ttl[tempM[0]] = int.Parse(tempM[tempM.GetLength(0) - 1]);
                    }              
                }
                FileStream fss = File.Open("inDom.txt", FileMode.Open, FileAccess.ReadWrite);
                fss.SetLength(0);
                fss.Close();
            }
            else
                using (StreamWriter fs = new StreamWriter("inDom.txt")) { }
        }

        public static List<string> ParseAnswer(byte[] data)
        {
            var info = new List<string>();
            if (data[0] != 170 || data[1] != 170)
            {
                info.Add("WRONGID");
                return info;
            }
            else info.Add("RIGHTID");

            if (data[2] < 128) info.Add("REQUEST");
            else info.Add("ANSWER");

            var length = data[12];
            var site = new StringBuilder("");
            var index = 13 + length;

            for (int i = 13; i < index; i++)
            {
                site.Append(((char)data[i]).ToString());
            }

            length = data[index++];
            while (length != 0)
            {
                site.Append(".");
                for (int i = index; i < index + length; i++)
                {
                    site.Append(((char)data[i]).ToString());
                }
                index = index + length;
                length = data[index++];
            }

            if (info[1] == "REQUEST")
            {
                info.Add(site.ToString());
            }
            else if (info[1] == "ANSWER")
            {
                if (data[3] % 16 != 0)
                {
                    info.Add("ERROR");
                    return info;
                }
                info.Add("NOERROR");              
                info.Add(site.ToString());

                index += 15;
                length = data[index++];

                var ipC = new StringBuilder("");
                while (length > 0)
                {
                    for (int i = index; i < index + length; i++)
                    {
                        ipC.Append(data[i].ToString());
                        if (i < index + length - 1) ipC.Append(".");
                    }
                    index += (12 + length - 1);
                    info.Add(ipC.ToString());
                    ipC = new StringBuilder("");
                    if (index >= data.GetLength(0) - 1) break;
                    //Console.WriteLine("length: {0}", length);
                    length = data[index++];
                }
            }
            return info;
        }
        static void Main(string[] args)
        {

            ReadIpToDomain();
            ReadDomainToIp();
            var localPort = 53;
            IPEndPoint remoteIp = null; // адрес входящего подключения
            UdpClient receiver = new UdpClient(localPort);
            var s = "";

            try
            {
                while (s != "exit")
                {
                    var del = new List<string>();
                    foreach (var domai in dom)
                    {
                        ttl[domai.Key]--;
                        if (ttl[domai.Key] < 1)
                        {
                            del.Add(domai.Key);
                        }
                    }
                    foreach (var domai in del)
                    {
                        var temp = dom[domai];
                        dom.Remove(domai);
                        foreach (var i in temp)
                        {
                            ip.Remove(i);
                        }
                    }

                    byte[] data = receiver.Receive(ref remoteIp); // получаем данные                    
                    info = ParseAnswer(data);
                    if (info[0] == "WRONGID") continue;

                    Console.WriteLine("Parsing the packet...");
                    if (info[1] == "REQUEST")
                    {
                        Ping pingSender = new Ping();
                        PingReply reply = pingSender.Send("www.ya.ru");                          
                        receiver.Send(data, data.GetLength(0), "8.8.8.8", 53);
                        Console.WriteLine("Request have been sent.");
                        continue;
                    }
                    else
                    {

                        if (info[2] == "ERROR")
                        {
                            Console.WriteLine("Error occured. Try Again...");
                            continue;
                        }
                        var domain = info[3];
                        Console.Write("Domain: {0}", domain.ToString());
                        Console.WriteLine();
                        Console.WriteLine("Uses Ip's below:");
                        for (int i = 4; i < info.Count; i++)
                        {
                            Console.WriteLine(info[i]);
                        }

                        ttl[domain] = 10;
                        if (!dom.ContainsKey(domain))
                        {
                            ttl[domain] = 10;
                            dom[domain] = new List<string>();
                            for (int i = 4; i < info.Count; i++)
                            {
                                dom[domain].Add(info[i]);
                            }
                        }
                        Console.WriteLine("Saving...");
                        WriteDomainToIp();
                        WriteIpToDomain();
                        Console.WriteLine("If you want to exit, write : exit , or press Enter to continue");
                        s = Console.ReadLine();
                    }
                }
            }

                catch (Exception ex)
                {
                    if ("An exception occurred during a Ping request." == ex.Message)
                    {
                        WriteDomainToIp();
                        WriteIpToDomain();
                        NoInternet(info);
                    }

                }
                finally
                {
                    receiver.Close();
                }

        }
        public static void NoInternet(List<string> info)
        {
            var domain = info[2];
            var isFoundDom = false;
            var isFoundIp = false;

            foreach (var domai in dom)
            {
                if (domain == domai.Key)
                {
                    isFoundDom = true;
                    break;
                }
            }
            foreach (var i in ip)
            {
                if (domain == i.Key)
                {
                    isFoundIp = true;
                    break;
                }
            }
            Console.WriteLine("No Internet Connection");
            Console.WriteLine("Trying to find in cash");
            if (isFoundDom)
            {
                Console.WriteLine("ip's of the {0}", domain);
                foreach (var domai in dom[domain])
                {
                    Console.WriteLine("{0}", domai);
                }
            }
            else
            if (isFoundIp)
            {

                Console.WriteLine("Domain name  of the {0} : {1}", domain, ip[domain]);
            }
            else Console.WriteLine("No Information in cache");
            Console.WriteLine("Press AnyKey to continue");
            Console.ReadKey();
        }
    /// <summary>
    /// USELESS WITH CLIENT
    /// </summary>
    }
}
