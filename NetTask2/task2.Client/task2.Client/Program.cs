using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace task2.Client
{
	class Program
	{
		static void Main(string[] args)
		{
			byte[] data = new byte[1024];
			string input, stringData;

			Console.Write("Укажите IP-адрес сервера: ");
			string addr = Console.ReadLine();

			if (addr == "") addr = "127.0.0.1";

			UdpClient server = new UdpClient(addr, 123);
			IPEndPoint sender = new IPEndPoint(IPAddress.Any, 123);

			string welcome = "Клиент успешно подключился!";
			data = Encoding.UTF8.GetBytes(welcome);
			server.Send(data, data.Length);
			data = server.Receive(ref sender);
			Console.Write("Сообщение принято от {0}:", sender.ToString());
			stringData = Encoding.UTF8.GetString(data, 0, data.Length);
			Console.WriteLine(stringData);

			while (true)
			{
				data = new byte[1024];
				input = Console.ReadLine();
				data = Encoding.UTF8.GetBytes(input);

				server.Send(data, data.Length);

				if (input == "exit") break;

				//Получение данных...
				data = server.Receive(ref sender);
				//Перевод принятых байтов в строку
				stringData = Encoding.UTF8.GetString(data, 0, data.Length);
				Console.Write("<");
				//Отображение на экране принятой строки (размер файла)
				Console.WriteLine(stringData);
			}

			Console.WriteLine("Exit...");

			server.Close();
		}
	}
}
