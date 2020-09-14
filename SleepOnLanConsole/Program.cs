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
			if (!NetworkInterface.GetIsNetworkAvailable())
			{
				//perform the next steps, if no network, check again after 5 minutes or something 
				//TODO: that ^
			}
			else
			{
				var macAddress = "30-9C-23-43-48-10";
				macAddress = Regex.Replace(macAddress, "[-|:]", "");
				Socket se = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				int payloadIndex = 0;
				byte[] payload = new byte[1024];

				// Add 6 bytes with value 255 (FF) in our payload
				for (int i = 0; i < 6; i++)
				{
					payload[payloadIndex] = 255;
					payloadIndex++;
				}

				// Repeat the device MAC address sixteen times
				for (int j = 0; j < 16; j++)
				{
					for (int k = 0; k < macAddress.Length; k += 2)
					{
						var s = macAddress.Substring(k, 2);
						payload[payloadIndex] = byte.Parse(s, NumberStyles.HexNumber);
						payloadIndex++;
					}
				}

				byte[] receivePayload = new byte[1024];
				IPEndPoint localEndPoint = new IPEndPoint(GetLocalIPAddress(), 9);
				se.Bind(localEndPoint);
				int receivedCount = se.Receive(receivePayload);

				if (payload.SequenceEqual(receivePayload))
				{
					Console.WriteLine("WOL Package received in the ON Mode");
				}
				else
				{
					Console.WriteLine("Received a faulty message on port 9, ");
				}

				Console.WriteLine(receivedCount);
				Console.WriteLine("program done");
				Console.ReadLine();
			}		
		}

		public static IPAddress GetLocalIPAddress()
		{
			var host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (var ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					return ip;
				}
			}
			throw new Exception("No network adapters with an IPv4 address in the system!");
		}
	}
}
