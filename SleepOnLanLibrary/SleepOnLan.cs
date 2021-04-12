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

		public delegate void SOLEventHandler();

		public event SOLEventHandler OnSOLMessageReceived;
		public event SOLEventHandler OnNoInternetConnectionAvailable;

		private readonly byte[] receivePayload;

		private readonly string localMac;

		private DateTime lastMessageReceived = new DateTime();

		private readonly int port;

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
				OnNoInternetConnectionAvailable?.Invoke();
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
						var methodToInvoke = (SOLEventHandler)eventListeners[index];
						methodToInvoke.BeginInvoke(EndAsyncEvent, null);
					}
				}
				else
				{
					Console.WriteLine("skipped 1 message...");
				}			
			}
			else
			{
				Console.WriteLine("Received a faulty magic packet on port " + port);
			}

			InitAsyncListener(currentSocket);
		}

		private void EndAsyncEvent(IAsyncResult iar)
		{
			var ar = (AsyncResult)iar;
			var invokedMethod = (SOLEventHandler)ar.AsyncDelegate;
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
			NetworkInterface[] Interfaces = NetworkInterface.GetAllNetworkInterfaces();

			foreach (NetworkInterface Interface in Interfaces)
			{
				IPInterfaceProperties props = Interface.GetIPProperties();
				if (props.GatewayAddresses.Count > 0 && Interface.OperationalStatus == OperationalStatus.Up)
				{
					foreach(var ip in props.UnicastAddresses)
					{
						if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
						{
							return ip.Address;
						}
					}
				}
			}
			
			throw new Exception("No network adapters with an IPv4 address in the system!");
		}
	}
}
