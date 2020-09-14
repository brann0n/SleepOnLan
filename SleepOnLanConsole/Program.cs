using SleepOnLanConsole.Properties;
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
using System.Threading;
using System.Threading.Tasks;

namespace SleepOnLanConsole
{
	class Program
	{
		static void Main(string[] args)
		{
			SleepOnLan sol = new SleepOnLan(Settings.Default.Mac);
			sol.OnSOLMessageReceived += Sol_OnSOLMessageReceived;
			sol.Start();

			Console.WriteLine("program done");
			Console.ReadLine();
		}

		private static void Sol_OnSOLMessageReceived()
		{
			Console.WriteLine("Received WOL message...");
			int idletime = IdleMonitor.IdleTime.Seconds;
			if (idletime > 5)
			{
				Console.WriteLine("PC has been idle for 5 seconds, waiting 15 more seconds before putting pc into sleep mode...");
				Thread.Sleep(15000);
				if(IdleMonitor.IdleTime.Seconds > idletime + 15)
				{
					PowerstateManagement.Sleep();
				}
				else
				{
					Console.WriteLine("Pc was used during idle period...");
				}
			}
			else
			{
				Console.WriteLine("pc was not idle for 5 second :(");
			}
		}
	}
}
