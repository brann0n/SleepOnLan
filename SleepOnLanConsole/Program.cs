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
			try
			{
				Thread t = new Thread(
				
					() => new Program()
				);
				t.Start();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.ReadLine();
			}
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

#if !DEBUG
			ConsoleViewHandler.Hide();
			manager.SendNotification("Sleep On LAN is now hidden and monitoring WOL packages");
#endif
			while (true)
			{
				Console.WriteLine("Type 'exit' to exit, or use the contextmenu from the notification area");
				string exit = Console.ReadLine();
				if (exit == "exit")
				{
					break;
				}
			}
			Environment.Exit(0);
		}

        private void Sol_OnNoInternetConnectionAvailable()
        {
			Console.WriteLine("No network connection (IPv4 with a gateway) available at the moment.");
			Console.WriteLine("Please restart the program once you have a working network connection.");
			manager.SendNotification("Network unavailable, please refer to the console for more information.");
        }

        private void Sol_OnSOLMessageReceived(string source)
		{
			Console.WriteLine($"Received WOL message from device {source}");
			int idletime = IdleMonitor.IdleTime.Seconds;
			bool isHomeAssistant = source == Settings.Default.HomeAssistant;
			if (isHomeAssistant) Console.WriteLine("Source == HomeAssistant"); else Console.WriteLine("Source != HomeAssistant");
			if (idletime > Settings.Default.InitialIdleTime)
			{
				if(isHomeAssistant)
				{
					Console.WriteLine($"Request was sent from registered HomeAssistant device, bypassing {Settings.Default.FinalIdleTime}s timeout.");
					manager.SendNotification($"HomeAssistant put pc into sleep mode.");
					PowerstateManagement.Sleep();
					return;
				}

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
