using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SleepOnLanLibrary
{
	public class ConsoleViewHandler
	{
		[DllImport("kernel32.dll")]
		static extern IntPtr GetConsoleWindow();

		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		const int SW_HIDE = 0;
		const int SW_SHOW = 5;

		private static bool hidden = false;


		public static void Toggle()
		{
			if (hidden)
				Show();
			else
				Hide();
		}

		public static void Show()
		{
			var handle = GetConsoleWindow();

			// Show
			ShowWindow(handle, SW_SHOW);

			hidden = false;
		}

		public static void Hide()
		{
			var handle = GetConsoleWindow();

			// Hide
			ShowWindow(handle, SW_HIDE);

			hidden = true;
		}
	}
}
