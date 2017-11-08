using System;
using Nancy.Hosting.Self;
using Nancy;
using NDesk.Options;
using System.Collections.Generic;
using System.IO;

namespace NancySelfHost1
{
	public class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine(Directory.GetCurrentDirectory());
			Console.WriteLine(Clutterify.GetSourcesString());
			bool showHelp = false;
			int port = 80;
			string address = "localhost";
			string dbpath = IsLinux ? "/mnt/c/dev/web/database.db" : @"C:\dev\web\database.db";
			var p = new OptionSet()
			{
				{"p|port=", "port number to run server on", (ushort v) => port = (int)v},
				{"a|address=","address to run server on", v => address = v},
				{"d|database=", "location of database", v => dbpath = v},
				{"h|help",  "show this message and exit", v => showHelp = v != null },
			};

			List<string> extra;

			try
			{
				extra = p.Parse(args);
			}
			catch (OptionException e)
			{
				Console.WriteLine("Error, invalid arguments: " + e.Message);
				p.WriteOptionDescriptions(Console.Out);
				return;
			}
			if (extra.Count > 0)
			{
				Console.WriteLine("Error, too many arguments were given");
				p.WriteOptionDescriptions(Console.Out);
				return;
			}
			if (showHelp)
			{
				p.WriteOptionDescriptions(Console.Out);
				return;
			}

			if (!dbpath.EndsWith(".db"))
			{
				Console.WriteLine("Database must be a .db file");
				return;
			}
			if (!File.Exists(dbpath))
			{
				bool valid = false;
				do
				{
					Console.WriteLine("Database file does not exist, create? (y/n)");
					string yesno = Console.ReadLine();
					switch (yesno.ToLowerInvariant())
					{
						case "y":
						case "yes":
							valid = true;
							break;
						case "n":
						case "no":
							return;
					}
				} while (!valid);
			}


			var uri = new Uri("http://" + address + ":" + port);
			HostConfiguration hostConfigs = new HostConfiguration();
			hostConfigs.UrlReservations.CreateAutomatically = true;

			using(var host = new NancyHost(uri, new Bootstrappertje(), hostConfigs))
			{
				host.Start();

				Console.WriteLine("Your application is running on " + uri);
				Console.WriteLine("Press any [Enter] to close the host.");
				Console.ReadLine();
			}
		}

		public static bool IsLinux
		{
			get
			{
				int p = (int)Environment.OSVersion.Platform;
				return (p == 4) || (p == 6) || (p == 128);
			}
		}
	}
}
