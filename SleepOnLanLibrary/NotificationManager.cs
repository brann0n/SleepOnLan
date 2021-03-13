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
		private readonly NotifyIcon Manager;
		private readonly ContextMenu Menu;
		private readonly MenuItem ShowHideItem;
		private readonly MenuItem AutoStartItem;

		public NotificationManager()
		{
			//build the context menu
			Menu = new ContextMenu();

			//build the menu items
			ShowHideItem = new MenuItem
			{
				Index = 0,
				Text = "Console Visible",
				DefaultItem = true
			};

			AutoStartItem = new MenuItem
			{
				Index = 1,
				Text = "Autostart"
			};

			MenuItem ExitApplication = new MenuItem
			{
				Index = 2,
				Text = "Exit App"
			};			

			Menu.MenuItems.AddRange(new MenuItem[] {
				ShowHideItem,
				AutoStartItem,
				new MenuItem("-"), //seperator bar
				ExitApplication
			});

			//create the icon
			Manager = new NotifyIcon
			{
				Icon = new Icon("appicon.ico"),
				Text = "Sleep On LAN",
				ContextMenu = Menu,
				Visible = true
			};
			
			//event that takes care of renaming titles in the menu before showing
            Menu.Popup += Menu_Popup;		

			//assign the events
			ShowHideItem.Click += (e, o) => ConsoleViewHandler.Toggle();
			ExitApplication.Click += (e, o) => Environment.Exit(0);
			AutoStartItem.Click += (e, o) => AutoStartManager.SetAutoStart(!AutoStartManager.IsAutoStartEnabled());

			Manager.DoubleClick += Manager_DoubleClick;
		}

        private void Menu_Popup(object sender, EventArgs e)
        {
			ShowHideItem.Checked = !ConsoleViewHandler.IsHidden();
			AutoStartItem.Checked = AutoStartManager.IsAutoStartEnabled();
        }

        private void Manager_DoubleClick(object sender, EventArgs e)
		{
			ConsoleViewHandler.Show();
		}

		public void SendNotification(string text)
		{
			Manager.BalloonTipText = text;
			Manager.ShowBalloonTip(1500, "Sleep On LAN", text, ToolTipIcon.Info);
		}
	}
}
