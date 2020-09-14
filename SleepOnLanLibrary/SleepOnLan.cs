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

namespace SleepOnLanLibrary
{
	public class SleepOnLan
	{

		public delegate void SOLReceivedHandler();

		public event SOLReceivedHandler OnSOLMessageReceived;

		private byte[] receivePayload;

		private string localMac;

		public SleepOnLan(string localMac)
		{
			receivePayload = new byte[1024];
			this.localMac = localMac;
		}


		public void Start()
		{
			Socket se = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

			IPEndPoint localEndPoint = new IPEndPoint(GetLocalIPAddress(), 9);
			se.Bind(localEndPoint);

			InitAsyncListener(se);
		}


		private void InitAsyncListener(Socket s)
		{
			//check for internet connection :)
			if (NetworkInterface.GetIsNetworkAvailable())
			{
				s.BeginReceive(receivePayload, 0, 1024, SocketFlags.None, new AsyncCallback(SOLReceivedCallback), s);
			}
			else
			{
				//TODO: throw the not connected event.
			}		
		}

		private void SOLReceivedCallback(IAsyncResult result)
		{
			Socket currentSocket = (Socket)result.AsyncState;
			int bytesReceived = currentSocket.EndReceive(result);

			if (CreateLocalPayload().SequenceEqual(receivePayload))
			{
				OnSOLMessageReceived?.Invoke();
				InitAsyncListener(currentSocket);
			}
			else
			{
				Console.WriteLine("Received a faulty message on port 9");
			}
		}

		private byte[] CreateLocalPayload()
		{
			var macAddress = localMac;
			macAddress = Regex.Replace(macAddress, "[-|:]", "");
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

			return payload;
		}

		private IPAddress GetLocalIPAddress()
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
