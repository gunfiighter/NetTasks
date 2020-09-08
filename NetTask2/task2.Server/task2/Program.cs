using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace task2
{
	class Program
	{
		static void Main(string[] args)
		{
			byte[] data = new byte[1024];	
			var port = 123;
			IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);
			Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			server.Bind(ipep);
			Console.WriteLine("Waiting for client connection...");

			IPEndPoint sender = new IPEndPoint(IPAddress.Any, 123);
			EndPoint Remote = (EndPoint)(sender);
			int recv = server.ReceiveFrom(data, ref Remote);

			Console.Write("Сообщение получено от {0}:", Remote.ToString());
			Console.WriteLine(Encoding.UTF8.GetString(data, 0, recv));
			string welcome = "Succesful connection!";
			data = Encoding.UTF8.GetBytes(welcome);
			server.SendTo(data, data.Length, SocketFlags.None, Remote);

			while (true)
			{
				data = new byte[1024];
				recv = server.ReceiveFrom(data, ref Remote);
				string str = Encoding.UTF8.GetString(data, 0, recv);

				if (str == "exit") break;
				Console.WriteLine("Получили данные: " + str);
				//Записываем в переменную size новый размер файла (после процедуры prntofile)
				var timeCur = DateTime.Now;
				if (str == "time") {
					timeCur = timeCur.AddSeconds(1000000);
					data = Encoding.UTF8.GetBytes(timeCur.ToString());
				}
				else			//Перевод отсылаемой строки в байты
				data = Encoding.UTF8.GetBytes("");
				//Отсылаем серверу строку (переведенную в байты)
				server.SendTo(data, data.Length, SocketFlags.None, Remote);
			}

			Console.WriteLine("Exit...");

			server.Close();
		}
	}
}
