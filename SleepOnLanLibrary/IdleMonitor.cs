using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SleepOnLanLibrary
{
	public class IdleMonitor
	{
		public static DateTime LastInput => SystemStartup.AddMilliseconds(LastInputTicks);
		public static TimeSpan IdleTime => DateTime.Now.Subtract(LastInput);

		[DllImport("user32.dll", SetLastError = false)]
		private static extern bool GetLastInputInfo(ref Lastinputinfo plii);

		private static readonly DateTime SystemStartup = DateTime.Now.AddMilliseconds(-Environment.TickCount);

		[StructLayout(LayoutKind.Sequential)]
		private struct Lastinputinfo
		{
			public uint cbSize;
			public readonly int dwTime;
		}

		public IdleMonitor()
		{
			for (var i = 0; i < 1000; i++)
			{
				Console.WriteLine($"Last Input: {LastInput.ToShortTimeString()}");
				Console.WriteLine($"Idle for: {IdleTime.Seconds} Seconds");
				Thread.Sleep(TimeSpan.FromSeconds(1));
			}
		}

		private static int LastInputTicks
		{
			get
			{
				var lii = new Lastinputinfo { cbSize = (uint)Marshal.SizeOf(typeof(Lastinputinfo)) };
				GetLastInputInfo(ref lii);
				return lii.dwTime;
			}
		}
	}
}
