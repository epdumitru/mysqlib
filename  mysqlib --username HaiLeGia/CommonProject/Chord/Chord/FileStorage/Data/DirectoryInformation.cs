using System;
using Chord.Common;

namespace Chord.FileStorage.Data
{
	[Serializable]
	public class DirectoryInformation : Information
	{
		private ID id;

		public DirectoryInformation(string parentDirectory, string directoryName, string ownerName)
		{
			this.parentDirectory = parentDirectory;
			name = directoryName;
			this.ownerName = ownerName;
			security = new Security();
			if (parentDirectory == null)
			{
				id = HashFunction.GenerateID(directoryName);
			}
			type = FOLDER;
		}

		internal ID Id
		{
			get { return id; }
			set { id = value; }
		}

		public override string Name
		{
			set
			{
				name = value;
				if (parentDirectory == null)
				{
					id = HashFunction.GenerateID(name);
				}
			}
		}
	}
}