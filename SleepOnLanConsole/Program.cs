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
		private NotificationManager manager;
		static void Main(string[] args)
		{
			new Thread(delegate () {
				new Program();
			}).Start();
		}

		private Program()
		{
			Console.WriteLine("Sleep On LAN software by github.com/brann0n");
			SleepOnLan sol = new SleepOnLan(Settings.Default.Mac, Settings.Default.Port);
			new Thread(delegate () {
				manager = new NotificationManager();
				System.Windows.Forms.Application.Run();
			}).Start();
			
			sol.OnSOLMessageReceived += Sol_OnSOLMessageReceived;
            sol.OnNoInternetConnectionAvailable += Sol_OnNoInternetConnectionAvailable;
			sol.Start();
			Console.WriteLine("Async event listener has started, awaiting events...");
			ConsoleViewHandler.Hide();
			manager.SendNotification("Sleep On LAN is now hidden and monitoring WOL packages");
			while (true)
			{
				Console.WriteLine("Type 'exit' to exit, or use the contextmenu from the notification area");
				string exit = Console.ReadLine();
				if (exit == "exit")
				{
					break;
				}
			}
		}

        private void Sol_OnNoInternetConnectionAvailable()
        {
			Console.WriteLine("No network connection (IPv4 with a gateway) available at the moment.");
			Console.WriteLine("Please restart the program once you have a working network connection.");
			manager.SendNotification("Network unavailable, please refer to the console for more information.");
        }

        private void Sol_OnSOLMessageReceived()
		{
			Console.WriteLine("Received WOL message...");
			int idletime = IdleMonitor.IdleTime.Seconds;
			if (idletime > Settings.Default.InitialIdleTime)
			{
				Console.WriteLine("PC has been idle for more than {0}s, waiting {1}s more before putting pc into sleep mode...", Settings.Default.InitialIdleTime, Settings.Default.FinalIdleTime);
				manager.SendNotification($"Putting PC into sleep mode in {Settings.Default.FinalIdleTime} seconds");
				Thread.Sleep(Settings.Default.FinalIdleTime * 1000);
				int finalIdleTime = IdleMonitor.IdleTime.Seconds;
				if (finalIdleTime >= idletime + Settings.Default.FinalIdleTime)
				{
					PowerstateManagement.Sleep();
				}
				else
				{
					manager.SendNotification("Request was cancelled by user.");
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
