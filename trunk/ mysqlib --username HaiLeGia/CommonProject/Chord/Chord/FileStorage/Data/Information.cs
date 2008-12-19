using System;

namespace Chord.FileStorage.Data
{
	[Serializable]
	public abstract class Information
	{
		public const int FILE = 1;
		public const int FOLDER = 0;

		protected string name;
		protected string ownerName;
		protected string parentDirectory;
		protected Security security;
		protected int type;

		public virtual string ParentDirectory
		{
			get { return parentDirectory; }
			set { parentDirectory = value; }
		}

		public virtual string Name
		{
			get { return name; }
			set { name = value; }
		}

		public virtual string OwnerName
		{
			get { return ownerName; }
			set { ownerName = value; }
		}

		public virtual Security Security
		{
			get { return security; }
		}

		public int Type
		{
			get { return type; }
		}
	}
}