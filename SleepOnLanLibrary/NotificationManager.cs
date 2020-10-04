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
			manager = new NotifyIcon();
			
			manager.Icon = new Icon("appicon.ico");
			manager.Text = "Sleep On LAN";
			

			//build the context menu
			menu = new ContextMenu();

			//build the menu items
			MenuItem ShowHideItem = new MenuItem();
			ShowHideItem.Index = 0;
			ShowHideItem.Text = "Show/Hide Console";
			ShowHideItem.Click += (e, o) => ConsoleViewHandler.Toggle();

			MenuItem ExitApplication = new MenuItem();
			ExitApplication.Index = 1;
			ExitApplication.Text = "Exit App";
			ExitApplication.Click += (e, o) => Environment.Exit(0);

			menu.MenuItems.AddRange(new MenuItem[] {
				ShowHideItem,
				ExitApplication
			});

			manager.ContextMenu = menu;
			manager.Visible = true;
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
