using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
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

		private DateTime lastMessageReceived = new DateTime();

		private int port;

		public SleepOnLan(string localMac, int port)
		{
			receivePayload = new byte[1024];
			this.localMac = localMac;
			this.port = port;
		}


		public void Start()
		{
			Socket se = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

			IPEndPoint localEndPoint = new IPEndPoint(GetLocalIPAddress(), port);
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
				DateTime currentTime = DateTime.Now;
				TimeSpan span = currentTime - lastMessageReceived;
				if(span.TotalMilliseconds > 1500)
				{
					lastMessageReceived = currentTime;
					//OnSOLMessageReceived?.Invoke(); //prevent udp spam and only allow 1 per 1.5 seconds

					Delegate[] eventListeners = OnSOLMessageReceived.GetInvocationList();
					for (int index = 0; index < eventListeners.Count(); index++)
					{
						var methodToInvoke = (SOLReceivedHandler)eventListeners[index];
						methodToInvoke.BeginInvoke(EndAsyncEvent, null);
					}
				}
				else
				{
					Console.WriteLine("skipped 1 message...");
				}
				
				InitAsyncListener(currentSocket);
			}
			else
			{
				Console.WriteLine("Received a faulty message on port " + port);
			}
		}

		private void EndAsyncEvent(IAsyncResult iar)
		{
			var ar = (AsyncResult)iar;
			var invokedMethod = (SOLReceivedHandler)ar.AsyncDelegate;
			try
			{
				invokedMethod.EndInvoke(iar);
			}
			catch
			{
				// Handle any exceptions that were thrown by the invoked method
				Console.WriteLine("An event listener went kaboom!");
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
