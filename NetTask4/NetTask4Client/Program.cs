using System;
using System.Net.Sockets;

namespace NetTask8Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var sender = new UdpClient("127.0.0.1", 53);
            var request = "";
            while(request != "exit")
            {
                Console.WriteLine("Write Domain name (like: vk.com, dtf.ru):");
                Console.WriteLine("To exit write: exit");
                Console.WriteLine();
                request = Console.ReadLine();
                if (request == "exit") break;
                var dName = request.Split('.');
                if (request == "")
                {
                    Console.WriteLine("Donaim name couldn't be empty!");
                    continue;
                }
                var pL = 18 + request.Length;
                var data = Initialize(pL, request, dName);
                sender.Send(data, data.Length);
                Console.WriteLine("Packet Have been Sent");
            }

        }

        public static byte[] Initialize(int pL, string request, string[] dName)
        {
            var data = new byte[pL];
            data[0] = 170;data[1] = 170;
            data[2] = 1; data[5] = 1; data[pL - 3] = 1; data[pL - 1] = 1;
            data[12] = (byte)dName[0].Length;
            for(int i = 13; i < 13 + request.Length; i++)
            {
                var t = request[i - 13];
                if (t == '.') data[i] = (byte)dName[1].Length;
                else data[i] = (byte)(t);
            }
            return data;
        }
    }
}
