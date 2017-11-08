using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using LiteDB;

namespace NancySelfHost1.Security
{
	public struct SaltHashCombo
	{
		public string salt { get; set; }
		public string hash { get; set; }

	}

	public static class Hasher
	{
		private static RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
		private static SHA256Managed crypter = new SHA256Managed();

		public static string GetRandomSalt(uint length)
		{
			var outputArray = new byte[length];
			rngCsp.GetBytes(outputArray);
			return ByteArrayToHexString(outputArray);
		}

		private static SaltHashCombo EncryptPassword(string salt, string password)
		{
			var output = new SaltHashCombo();
			output.salt = salt;
			var hashArray = crypter.ComputeHash(Encoding.UTF8.GetBytes(salt + password));
			output.hash = ByteArrayToHexString(hashArray);
			return output;
		}

		public static SaltHashCombo EncryptPassword(string password)
		{
			return EncryptPassword(GetRandomSalt(32), password);
		}

		public static bool VerifyPassword(string attempt, SaltHashCombo saltHashCombo)
		{
			var toTest = EncryptPassword(saltHashCombo.salt, attempt);
			return toTest.hash == saltHashCombo.hash;
		}

		private static string ByteArrayToHexString(byte[] array)
		{
			StringBuilder sb = new StringBuilder();
			foreach (byte b in array)
			{
				sb.Append(b.ToString("x2"));
			}
			return sb.ToString();
		}
	}

	public class UserEntry
	{
		[BsonId]
		public string username { get; set; }
		public SaltHashCombo saltHashCombo { get; set; }
		public DateTime lastLogin { get; set; }
	}

	public static class UserDatabase
	{
		private static LiteDatabase db;
		private static LiteCollection<UserEntry> users;
		
		static UserDatabase()
		{
			db = new LiteDatabase(@"users.db");
			users = db.GetCollection<UserEntry>("userEntries");
		}

		public static bool UserExists(string username)
		{
			return users.Exists(x => (x.username == username));
		}

		public static bool AttemptLogin(string username, string passwordAttempt)
		{
			UserEntry userEntry = FindUser(username);
			bool valid = Hasher.VerifyPassword(passwordAttempt, userEntry.saltHashCombo);
			if (valid)
			{
				userEntry.lastLogin = DateTime.Now;
				users.Update(userEntry);
			}
			return valid;
		}

		public static void AddUser(string username, string password)
		{
			if (UserExists(username))
			{
				throw new Exception("User already exists.");
			}
			UserEntry userEntry = new UserEntry
			{
				username = username,
				saltHashCombo = Hasher.EncryptPassword(password)
			};
			users.Insert(userEntry);
		}

		public static void ChangePassword(string username, string oldPass, string newPass)
		{
			if(AttemptLogin(username, oldPass))
			{
				UserEntry userEntry = users.FindOne(x => x.username == username);
				userEntry.saltHashCombo = Hasher.EncryptPassword(newPass);
				users.Update(userEntry);
			}
		}

		public static string ResetPassword(string username)
		{
			UserEntry userEntry = FindUser(username);
			string newPass = Hasher.GetRandomSalt(16);
			userEntry.saltHashCombo = Hasher.EncryptPassword(newPass);
			users.Update(userEntry);
			return newPass;
		}

		private static UserEntry FindUser(string username)
		{
			UserEntry userEntry = users.FindOne(x => x.username == username);
			if (userEntry == null)
			{
				throw new Exception("User does not exists.");
			}
			return userEntry;
		}
	}
}
