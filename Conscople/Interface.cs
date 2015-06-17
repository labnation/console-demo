using System;
using Mono.Terminal;
using LabNation.DeviceInterface.Devices;

namespace Conscople
{
	public class ScopeConsoleInterface
	{
		DeviceManager deviceManager;
		IScope scope;
		Dialog d;
		Label l;

		public ScopeConsoleInterface()
		{
			deviceManager = new DeviceManager (connectHandler);
			deviceManager.Start ();
			Application.Init (false);

			d = new Dialog (40, 8, "Hello");
			l = new Label (0, 0, "Tadaam");
			d.Add (l);

			Application.Iteration += updateUI;
			Application.Run(d);
			deviceManager.Stop ();
		}

		void updateUI(object sender, EventArgs args)
		{
			if (this.scope != null)
				l.Text = "Scope connected with serial " + scope.Serial;
			else
				l.Text = "No scope connected";
		}

		void connectHandler(IDevice dev, bool connected)
		{
			if(connected && dev is IScope && dev != deviceManager.fallbackDevice)
			{
				this.scope = (IScope)dev;
			}
			else
			{
				this.scope = null;
			}
			Application.Refresh ();
		}
	}
}

