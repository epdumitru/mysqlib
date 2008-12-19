using System;
using System.Text;

namespace Chord.Common
{
	public interface IKey
	{
		byte[] GetBytes();
	}

	[Serializable]
	public class Key : IKey
	{
		private string innerString;

		public Key(string s)
		{
			if (s == null)
			{
				string error = "s cannot be null.";
				throw new ArgumentNullException(error);
			}
			innerString = s;
		}

		#region IKey Members

		public byte[] GetBytes()
		{
			return UTF8Encoding.UTF8.GetBytes(innerString);
		}

		#endregion

		public override string ToString()
		{
			return "[Key: " + innerString + "]";
		}

		public override int GetHashCode()
		{
			return innerString.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			Key other = obj as Key;
			if (other != null)
			{
				return innerString.Equals(other.innerString);
			}
			return false;
		}
	}
}