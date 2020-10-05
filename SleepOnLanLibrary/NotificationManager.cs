using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SleepOnLanLibrary
{
	public class NotificationManager
	{
		private NotifyIcon manager;
		private ContextMenu menu;
		public NotificationManager()
		{
			//build the context menu
			menu = new ContextMenu();

			//build the menu items
			MenuItem ShowHideItem = new MenuItem
			{
				Index = 0,
				Text = "Show/Hide Console",
				DefaultItem = true
			};

			MenuItem ExitApplication = new MenuItem
			{
				Index = 1,
				Text = "Exit App"
			};

			menu.MenuItems.AddRange(new MenuItem[] {
				ShowHideItem,
				ExitApplication
			});

			//create the icon
			manager = new NotifyIcon
			{
				Icon = new Icon("appicon.ico"),
				Text = "Sleep On LAN",
				ContextMenu = menu,
				Visible = true
			};

			//assign the events
			ShowHideItem.Click += (e, o) => ConsoleViewHandler.Toggle();
			ExitApplication.Click += (e, o) => Environment.Exit(0);
			manager.DoubleClick += Manager_DoubleClick;
		}

		private void Manager_DoubleClick(object sender, EventArgs e)
		{
			ConsoleViewHandler.Show();
		}

		public void SendNotification(string text)
		{
			manager.BalloonTipText = text;
			manager.ShowBalloonTip(2000, "Sleep On LAN", text, ToolTipIcon.Info);
		}
	}
}
