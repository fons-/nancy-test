using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NancySelfHost1
{
	static class Clutterify
	{
		private static List<string> fileNames;
		private static List<string> sources;
		private static char sep = Path.DirectorySeparatorChar;
		private static string current;
		private static string sourcesPath;
		static Clutterify()
		{
			//current = Directory.GetCurrentDirectory();
			current = AppDomain.CurrentDomain.BaseDirectory;
			sourcesPath = Path.Combine(current, "sources");
			var order = File.ReadAllLines(Path.Combine(current, "order.txt")).Select(x => Path.Combine(sourcesPath, x)).Reverse();
			// TODO: try, empty order is fine

			fileNames = Directory.GetFiles(sourcesPath).ToList();

			foreach(string item in order)
			{
				if (fileNames.Contains(item))
				{
					int index = fileNames.IndexOf(item);

					string hold = fileNames[index];
					fileNames.RemoveAt(index);
					fileNames.Insert(0, hold);
				}
			}

			sources = fileNames.Select(x => 
			{
				string name = x.Split(sep).Last();
				// TODO: try
				everything.Add(name, File.ReadAllLines(x));
				return name;
			}).ToList();
		}

		public static Dictionary<string, string[]> everything = new Dictionary<string, string[]>();

		// TODO: precompute
		public static string GetSourcesString()
		{
			return '[' + sources.Select(x => '\"' + x + '\"').Aggregate((x, y) => x + ',' + y) + ']';
		}

		public static bool SourceExists(string name)
		{
			return sources.Contains(name);
		}
	}
}