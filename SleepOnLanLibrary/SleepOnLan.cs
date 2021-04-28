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

		public delegate void SOLEventHandler(string source);
		public delegate void SOLErrorEventHandler();

		public event SOLEventHandler OnSOLMessageReceived;
		public event SOLErrorEventHandler OnNoInternetConnectionAvailable;

		

		private readonly string localMac;

		private DateTime lastMessageReceived = new DateTime();

		private readonly int port;

		public SleepOnLan(string localMac, int port)
		{
			this.localMac = localMac;
			this.port = port;
		}


		public void Start()
		{
			UdpClient uc = new UdpClient(port, AddressFamily.InterNetwork)
			{
				EnableBroadcast = true
			};

			InitAsyncListener(uc);
		}

		private void InitAsyncListener(UdpClient s)
		{
			//check for internet connection :)
			if (NetworkInterface.GetIsNetworkAvailable())
			{
				s.BeginReceive(new AsyncCallback(SOLReceivedCallback), s);
			}
			else
			{
				OnNoInternetConnectionAvailable?.Invoke();
			}		
		}

		private void SOLReceivedCallback(IAsyncResult result)
		{
			IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, port);
			UdpClient currentSocket = (UdpClient)result.AsyncState;
			byte[] receivePayload = currentSocket.EndReceive(result, ref remoteEndpoint);
			byte[] expectedPayload = CreateLocalPayload();
			if (expectedPayload.SequenceEqual(receivePayload))
			{
				DateTime currentTime = DateTime.Now;
				TimeSpan span = currentTime - lastMessageReceived;
				if(span.TotalMilliseconds > 1500)
				{
					//prevent udp spam and only allow 1 per 1.5 seconds
					lastMessageReceived = currentTime;

					Delegate[] eventListeners = OnSOLMessageReceived.GetInvocationList();
					for (int index = 0; index < eventListeners.Count(); index++)
					{
						var methodToInvoke = (SOLEventHandler)eventListeners[index];
						methodToInvoke.BeginInvoke(remoteEndpoint.Address.ToString(), EndAsyncEvent, null);
					}
				}
				else
				{
					Console.WriteLine("skipped 1 message...");
				}			
			}
			else
			{
				Console.WriteLine("Received a dirty packet on port " + port);
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
			byte[] payload = new byte[102];

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
