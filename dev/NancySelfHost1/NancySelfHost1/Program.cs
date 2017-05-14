using System;
using Nancy.Hosting.Self;
using Nancy;

namespace NancySelfHost1
{
	class Program
	{
		static void Main(string[] args)
		{
			int port = 80;
			if(args.Length > 0)
			{
				int.TryParse(args[0], out port);
			}
			var uri =
				new Uri("http://localhost:" + port);
			HostConfiguration hostConfigs = new HostConfiguration();
			hostConfigs.UrlReservations.CreateAutomatically = true;

			using(var host = new NancyHost(uri, new DefaultNancyBootstrapper(), hostConfigs))
			{
				host.Start();

				Console.WriteLine("Your application is running on " + uri);
				Console.WriteLine("Press any [Enter] to close the host.");
				Console.ReadLine();
			}
		}
	}
}
