using System;
using System.Net;
using DBCache.UserComponents;

namespace DBCache.UserComponents
{
	public class ReferenceGenerator
	{
		private Reference reference;
		private object lck = new object();

		public ReferenceGenerator()
		{
			var addresses = Dns.GetHostAddresses(Dns.GetHostName());
			for (var i = 0; i < addresses.Length; i++)
			{
				var address = addresses[i];
				if (address.GetAddressBytes().Length == 4)
				{
					reference = new Reference(address, 0);
					break;
				}
			}
			if (reference == null)
			{
				throw new ArgumentException("Cannot create reference generator due to we cannot find a IP4");
			}
		}

		public Reference NewReference()
		{
			lock (lck)
			{
				reference = reference.Increase;
				return reference;
			}
		}
	}
}