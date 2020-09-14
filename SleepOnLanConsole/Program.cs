using SleepOnLanLibrary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SleepOnLanConsole
{
	class Program
	{
		static void Main(string[] args)
		{
			SleepOnLan sol = new SleepOnLan("30-9C-23-43-48-10");
			sol.OnSOLMessageReceived += Sol_OnSOLMessageReceived;
			sol.Start();

			Console.WriteLine("program done");
			Console.ReadLine();
		}

		private static void Sol_OnSOLMessageReceived()
		{
			Console.WriteLine("Received message :)");
		}
	}
}
