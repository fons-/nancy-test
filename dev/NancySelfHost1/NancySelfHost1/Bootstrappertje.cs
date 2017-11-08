using Nancy;
using Nancy.Bootstrapper;
using Nancy.Cryptography;
using Nancy.Session;
using Nancy.TinyIoc;
using System;

[assembly: IncludeInNancyAssemblyScanning]
namespace NancySelfHost1
{
	public class Bootstrappertje : DefaultNancyBootstrapper
	{
		// The bootstrapper enables you to reconfigure the composition of the framework,
		// by overriding the various methods and properties.
		// For more information https://github.com/NancyFx/Nancy/wiki/Bootstrapper


		public class CustomKeyGenerator:IKeyGenerator
		{
			public byte[] GetBytes(int count)
			{
				Console.WriteLine("Requesting {0} bytes.", count);
				var output = new byte[count];
				for(int i = 0; i < count; i++)
				{
					output[i] = 0;
				}
				return output;
			}
		}
		protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
		{
			Console.WriteLine("App startup");
			base.ApplicationStartup(container, pipelines);
			CryptographyConfiguration defaultCc = CryptographyConfiguration.Default;
			
			var customEncryptionProvider = new RijndaelEncryptionProvider(new CustomKeyGenerator());
			CryptographyConfiguration customCc = new CryptographyConfiguration(customEncryptionProvider, defaultCc.HmacProvider);
			CookieBasedSessions.Enable(pipelines, defaultCc);

			
		}

		protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
		{
			
			Console.WriteLine("Request startup");
			var now = DateTime.Now;
			Console.WriteLine(now.ToShortDateString() + " " + now.ToShortTimeString() + "   " + context.Request.UserHostAddress + "   " + context.Request.Url);
			base.RequestStartup(container, pipelines, context);
			//CookieBasedSessions.Enable(pipelines);
			
		}
	}
}