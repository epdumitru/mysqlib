using System;
using System.Security.Cryptography;
using System.Text;

namespace Chord.Common
{
	public class HashFunction
	{
		public static ID GenerateID(IKey key)
		{
			byte[] keyBytes = key.GetBytes();
			ID id;
			using (SHA1 sha1 = new SHA1CryptoServiceProvider())
			{
				id = new ID(sha1.ComputeHash(keyBytes));
				sha1.Clear();
			}
			return id;
		}

		public static ID GenerateID(byte[] bytes)
		{
			ID id;
			using (SHA1 sha1 = new SHA1CryptoServiceProvider())
			{
				id = new ID(sha1.ComputeHash(bytes));
				sha1.Clear();
			}
			return id;
		}

		public static ID GenerateID(URL url)
		{
			if (url == null)
			{
				throw new ArgumentNullException("url cannot be null.");
			}
			byte[] data = UTF8Encoding.UTF8.GetBytes(url.UrlString);
			ID id;
			using (SHA1 sha1 = new SHA1CryptoServiceProvider())
			{
				id = new ID(sha1.ComputeHash(data));
				sha1.Clear();
			}
			return id;
		}

		public static ID GenerateID(string s)
		{
			byte[] data = UTF8Encoding.UTF8.GetBytes(s);
			ID id;
			using (SHA1 sha1 = new SHA1CryptoServiceProvider())
			{
				id = new ID(sha1.ComputeHash(data));
				sha1.Clear();
			}
			return id;
		}
	}
}