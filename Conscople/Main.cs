using System;
using Mono.Terminal;

namespace ConsoleTest
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			Application.Init (false);
			Dialog d = new Dialog (40, 8, "Hello");

			d.Add (new Label (0, 0, "Hello World"));
			Application.Run (d);
		}
	}
}