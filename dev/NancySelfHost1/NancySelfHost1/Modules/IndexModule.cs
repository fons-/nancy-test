using Nancy;
using System;
using NancySelfHost1.Security;
using System.Dynamic;
using System.Linq;

namespace NancySelfHost1.Modules
{
	public class ClutterifyModule : NancyModule
	{
		private Random rnd = new Random();
		private string clutterifyUrl = "https://chrome.google.com/webstore/detail/clutterify/idadacnnahhpfpghpedoibefboehcncg";
		public ClutterifyModule()
		{
			Get["/clutterify"] = _ => Response.AsRedirect(clutterifyUrl);
			Get["/clutterify/{whatever*}"] = _ => Response.AsRedirect(clutterifyUrl);

			Get["/clutterify/sources"] = _ => Clutterify.GetSourcesString();

			Get["/clutterify/source/{name}"] = parameters =>
			{
				string[] listing;
				if (Clutterify.everything.TryGetValue(parameters.name, out listing))
				{
					int length = listing.Length;
					if (length == 1)
					{
						return Response.AsRedirect(listing[0]);
					}
					return Response.AsRedirect(listing[rnd.Next(length)]);
				}
				return null;
			};
		}
	}
	public class IndexModule : NancyModule
	{
		private Random rnd = new Random();
		public IndexModule()
		{
			Get["/"] = parameters =>
			{
				dynamic model = new ExpandoObject();
				model.loggedIn = Session["loggedIn"];
				model.username = Session["username"];
				return View["index", model];
			};

			Get["/enable"] = parameters =>
			{
				Session["para"] = "YES";
				return "set.";
			};
			Get["/disable"] = parameters =>
			{
				Request.Session["para"] = "NO";
				return "set.";
			};
			Get["/what"] = _ =>
			{
				string output = "KEY NOT FOUND";
				try
				{
					output = Request.Session["para"].ToString();
				} catch(Exception exc)
				{

				}
				return output;
				System.Security.Cryptography.SHA256Managed c = new System.Security.Cryptography.SHA256Managed();
				c.ComputeHash(System.Text.Encoding.UTF8.GetBytes("asdf"), 0, System.Text.Encoding.UTF8.GetByteCount("asdf"));
			};
			Get["/count"] = _ =>
			{
				try
				{
					Session["counter"] = (int)Session["counter"] + 1;
				} catch(Exception exc)
				{
					Session["counter"] = 1;
				}
				return Session["counter"].ToString();
			};
			Get["/test"] = parameters =>
			{
				string s = "";
				foreach(var x in Session)
				{
					s += x + "<br />";
				}
				return s;
				
				return Request.Session == Session ? "y" : "n";
			};
			//Get["/{whatever*}"] = _ => Response.AsRedirect("/");
			Get["/(?:(?i)cv(\\.pdf)?)"] = _ => Response.AsFile("Content/cv.pdf");
			Before.AddItemToEndOfPipeline(ctx =>
			{
				Console.WriteLine("endofpipeline");
				var sesh = ctx.Request.Session;
				if(sesh.Any(x => x.Key == "initialized"))
				{
					return null;
				}				
				Console.WriteLine("Setting session...");
				sesh["initialized"] = true;
				sesh["username"] = "NOT-LOGGED-IN";
				sesh["loggedIn"] = false;
				
				return null;
			});

			Get["/users/exists/{usr}"] = ctx => UserDatabase.UserExists(ctx.usr) ? "true" : "false";
			Post["/users/login"] = ctx =>
			{
				var username = Request.Form.username.ToString();
				var password = Request.Form.password.ToString();
				System.Console.WriteLine(username);
				System.Console.WriteLine(password);

				if (!UserDatabase.UserExists(username))
				{
					return "username not known.";
				}
				if(!UserDatabase.AttemptLogin(username, password))
				{
					return "wrong password.";
				}
				Session["username"] = username;
				Session["loggedIn"] = true;
				
				return Response.AsRedirect("/");
			};
			Post["/users/add"] = ctx =>
			{
				var username = Request.Form.username.ToString();
				var password = Request.Form.password.ToString();
				System.Console.WriteLine(username);
				System.Console.WriteLine(password);

				if (UserDatabase.UserExists(username))
				{
					return "username already exists.";
				}
				UserDatabase.AddUser(username, password);
				UserDatabase.AttemptLogin(username, password);
				Session["username"] = username;
				Session["loggedIn"] = true;

				return Response.AsRedirect("/");
			};
			Post["/users/logout"] = _ =>
			{
				Session["loggedIn"] = false;
				Session["username"] = "LOGGED-OUT";
				return Response.AsRedirect("/");
			};
		}
	}
}