using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace tracert
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Write an IpAdress or DomainName");
			var name = Console.ReadLine();
			Console.WriteLine("wait a second...");
			var counter = 1;
			IEnumerable<IPAddress> answer = GetTR(name);
			var wC = new WebClient();
			Console.WriteLine("№ I/O | {0, 16}  | {1, 16}  | {2, 46}  | AS", "IP","COUNTRY", "PROVIDER");
			foreach (var ip in answer)
			{
				var ipS = ip.ToString();
				//Console.WriteLine(ipS);
				dynamic reply = JObject.Parse(wC.DownloadString("http://ip-api.com/json/" + ipS));
				var st = reply.status;

				Console.WriteLine(" {0,4} | {1, 16}  | {2, 16}  | {3, 46}  |{4, 36}",
					counter++,ipS ,reply.country, reply.isp, reply["as"]);
			}
			Console.WriteLine("Press key to exit...");
			Console.ReadKey();
		}

		public static IEnumerable<IPAddress> GetTR(string hostname)
		{
			int timeout = 1000;
			int maxTTL = 30;
			int bufferSize = 32;

			byte[] buffer = new byte[bufferSize];
			new Random().NextBytes(buffer);

			Ping pinger = new Ping();

			for (int ttl = 1; ttl <= maxTTL; ttl++)
			{
				PingOptions options = new PingOptions(ttl, true);
				PingReply reply = pinger.Send(hostname, timeout, buffer, options);

				if (reply.Status == IPStatus.TtlExpired)
				{
					yield return reply.Address;
					continue;
				}
				if (reply.Status == IPStatus.TimedOut)
					continue;
				if (reply.Status == IPStatus.Success)
					yield return reply.Address;

				break;
			}
		}
	}
	
}
