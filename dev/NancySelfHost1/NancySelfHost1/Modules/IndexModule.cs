using Nancy;

namespace NancySelfHost1.Modules
{
	public class IndexModule : NancyModule
	{
		public IndexModule()
		{
			Get["/"] = parameters =>
			{
				return View["index"];
			};
			Get["/{whatever*}"] = _ => Response.AsRedirect("/");
		}
	}
}