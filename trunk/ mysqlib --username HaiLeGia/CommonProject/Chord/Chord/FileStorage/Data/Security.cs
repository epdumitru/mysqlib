using System;

namespace Chord.FileStorage.Data
{
	[Serializable]
	public class Security
	{
		private bool[] accessType;

		public Security()
		{
			accessType = new bool[9]; //3 bytes for admin, 3 bytes for other, 3 bytes for owner
			for (int i = 0; i < 9; i++)
			{
				accessType[i] = true;
			}
		}

		public Security(bool[] accessType)
		{
			this.accessType = new bool[accessType.Length];
			Array.Copy(accessType, this.accessType, accessType.Length);
		}

		#region Admin Access

		public bool AdminRead
		{
			get { return accessType[0]; }
			set { accessType[0] = value; }
		}

		public bool AdminWrite
		{
			get { return accessType[1]; }
			set { accessType[1] = value; }
		}

		public bool AdminExecute
		{
			get { return accessType[2]; }
			set { accessType[2] = value; }
		}

		#endregion

		#region Other Access

		public bool OtherRead
		{
			get { return accessType[3]; }
			set { accessType[3] = value; }
		}

		public bool OtherWrite
		{
			get { return accessType[4]; }
			set { accessType[4] = value; }
		}

		public bool OtherExecute
		{
			get { return accessType[5]; }
			set { accessType[5] = value; }
		}

		#endregion

		#region Owner Access

		public bool OwnerRead
		{
			get { return accessType[6]; }
			set { accessType[6] = value; }
		}

		public bool OwnerWrite
		{
			get { return accessType[7]; }
			set { accessType[7] = value; }
		}

		public bool OwnerExecute
		{
			get { return accessType[8]; }
			set { accessType[8] = value; }
		}

		#endregion
	}
}