using System;
using System.Text;

namespace Chord.Common
{
	[Serializable]
	public class ID : IComparable<ID>
	{
		private static readonly char[] hexAlphabet =
			new char[] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'};

		private byte[] idBytes;

		public ID(byte[] value)
		{
			idBytes = new byte[value.Length];
			Array.Copy(value, idBytes, idBytes.Length);
		}

		public int Length
		{
			get { return idBytes.Length*8; }
		}

		#region IComparable<ID> Members

		public int CompareTo(ID other)
		{
			if (other == null)
			{
				string s = "Cannot compare with null ID";
				throw new ArgumentNullException(s);
			}
			else if (other.idBytes.Length != idBytes.Length)
			{
				throw new ArgumentException("Cannot compare to difference length ID");
			}
			else
			{
				for (int i = 0; i < idBytes.Length; i++)
				{
					if (idBytes[i] > other.idBytes[i])
					{
						return 1;
					}
					else if (idBytes[i] < other.idBytes[i])
					{
						return -1;
					}
				}
				return 0;
			}
		}

		#endregion

		public override string ToString()
		{
			StringBuilder result = new StringBuilder(idBytes.Length*2);
			for (int i = 0; i < idBytes.Length; i++)
			{
				result.Append(hexAlphabet[idBytes[i] >> 4]);
				result.Append(hexAlphabet[idBytes[i] & 0x0F]);
			}
			return result.ToString();
		}

		public override bool Equals(object obj)
		{
			if (obj == this)
			{
				return true;
			}
			ID other = obj as ID;
			if (other != null)
			{
				return CompareTo(other) == 0;
			}
			return false;
		}

		public override int GetHashCode()
		{
			int result = 19;
			for (int i = 0; i < idBytes.Length; i++)
			{
				result = 13*result + idBytes[i];
			}
			return result;
		}

		public ID AddPowerOfTwo(int powerOfTwo)
		{
			if (powerOfTwo < 0 || powerOfTwo > idBytes.Length*8)
			{
				throw new IndexOutOfRangeException("Power of two must be in interval: [" + powerOfTwo + ", " + Length + "]");
			}
			byte[] copy = new byte[idBytes.Length];
			Array.Copy(idBytes, copy, idBytes.Length);
			int indexOfByte = idBytes.Length - 1 - (powerOfTwo/8);
			byte[] valueToAdd = {1, 2, 4, 8, 16, 32, 64, 128};
			byte addedValue = valueToAdd[powerOfTwo%8];
			byte oldValue;
			do
			{
				oldValue = copy[indexOfByte];
				copy[indexOfByte] += addedValue;
				addedValue = 1;
			} while (oldValue >= 128 && copy[indexOfByte] < 128 && indexOfByte-- > 0);
			return new ID(copy);
		}

		public bool IsInInterval(ID fromID, ID toID)
		{
			if (fromID == null || toID == null)
			{
				string s = "ID cannot be null.";
				throw new ArgumentNullException(s);
			}

			if (fromID.Equals(toID))
			{
				return !fromID.Equals(this);
			}

			if (fromID.CompareTo(toID) < 0)
			{
				return fromID.CompareTo(this) < 0 && CompareTo(toID) < 0;
			}

			// interval crosses zero -> split interval at zero
			// calculate min and max IDs
			byte[] minIDBytes = new byte[idBytes.Length];
			ID minID = new ID(minIDBytes);
			byte[] maxIDBytes = new byte[idBytes.Length];
			for (int i = 0; i < maxIDBytes.Length; i++)
			{
				maxIDBytes[i] = 255;
			}
			ID maxID = new ID(maxIDBytes);
			// check both splitted intervals
			// first interval: (fromID, maxID]
			return ((!fromID.Equals(maxID) && CompareTo(fromID) > 0 && CompareTo(maxID) <= 0) ||
			        // second interval: [minID, toID)
			        (!minID.Equals(toID) && CompareTo(minID) >= 0 && CompareTo(toID) < 0));
		}
	}
}