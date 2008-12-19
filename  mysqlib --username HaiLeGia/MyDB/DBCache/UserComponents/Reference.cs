using System;
using System.Net;

namespace DBCache.UserComponents
{
	[Serializable]
	public class Reference : IComparable<Reference>
	{
		public static readonly Reference NULL = new Reference(new byte[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0});
		private byte[] pointer;

		public Reference()
		{
			pointer = new byte[12]; //4 byte for ip, 8 byte for id
		}

		public Reference(IPAddress address, ulong id)
			: this()
		{
			var addressBytes = address.GetAddressBytes();
			if (addressBytes.Length != 4)
			{
				throw new ArgumentException("Address is not supported");
			}
			Array.Copy(addressBytes, pointer, 4);
			var idByte = new byte[8];
			for (var i = 0; i < 8; i++)
			{
				idByte[i] = (byte)(id >> (i * 8));
			}
			Array.Copy(idByte, 0, pointer, 4, 8);
		}

		public Reference(byte[] pointer)
		{
			this.pointer = pointer;
		}

		public byte[] Pointer
		{
			get { return pointer; }
		}

		public IPAddress Address
		{
			get
			{
				var addressByte = new byte[4];
				Array.Copy(pointer, addressByte, 4);
				return new IPAddress(addressByte);
			}
		}

		public ulong LocalId
		{
			get
			{
				ulong localId = 0;
				for (var i = 0; i < 8; i++)
				{
					localId = (localId << 8) | pointer[11 - i];
				}
				return localId;
			}
		}
		public int CompareTo(Reference other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("Parameter other cannot be null");
			}
			if (other.pointer.Length != pointer.Length)
			{
				throw new ArgumentException("Reference is not the same type. ");
			}
			if (pointer == other.pointer)
			{
				return 0;
			}
			for (var i = 0; i < pointer.Length; i++)
			{
				if (pointer[i] < other.pointer[i])
				{
					return -1;
				}
				else if (pointer[i] > other.pointer[i])
				{
					return 1;
				}
			}
			return 0;
		}

		public Reference Decrease
		{
			get
			{
				var currentBytes = new byte[pointer.Length];
				Array.Copy(pointer, currentBytes, pointer.Length);
				var index = 4;
				while (index < currentBytes.Length)
				{
					if (currentBytes[index] > 0)
					{
						currentBytes[index]--;
						break;
					}
					currentBytes[index] = Byte.MaxValue;
					index++;
				}
				return new Reference(currentBytes);
			}
		}

		public Reference Increase
		{
			get
			{
				var currentBytes = new byte[pointer.Length];
				Array.Copy(pointer, currentBytes, pointer.Length);
				var index = 4;
				while (index < currentBytes.Length)
				{
					if (currentBytes[index] < Byte.MaxValue)
					{
						currentBytes[index] += 1;
						break;
					}
					currentBytes[index] = 0;
					index++;
				}
				return new Reference(currentBytes);
			}
		}
	}
}