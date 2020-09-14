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
			Console.WriteLine("Sleep On LAN software by github.com/brann0n");
			SleepOnLan sol = new SleepOnLan(Settings.Default.Mac, Settings.Default.Port);
			sol.OnSOLMessageReceived += Sol_OnSOLMessageReceived;
			sol.Start();
			Console.WriteLine("Async event listener has started, awaiting events...");
			
			while (true)
			{
				Console.WriteLine("Type 'exit' to exit");
				string exit = Console.ReadLine();
				if (exit == "exit")
				{
					break;
				}
			}
		}

		private static void Sol_OnSOLMessageReceived()
		{
			Console.WriteLine("Received WOL message...");
			int idletime = IdleMonitor.IdleTime.Seconds;
			if (idletime > Settings.Default.InitialIdleTime)
			{
				Console.WriteLine("PC has been idle for more than {0}s, waiting {1}s more before putting pc into sleep mode...", Settings.Default.InitialIdleTime, Settings.Default.FinalIdleTime);
				Thread.Sleep(Settings.Default.FinalIdleTime * 1000);
				if(IdleMonitor.IdleTime.Seconds > idletime + Settings.Default.FinalIdleTime)
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
				Console.WriteLine("pc was not idle for {0}s :(", Settings.Default.InitialIdleTime);
			}
		}
	}
}
