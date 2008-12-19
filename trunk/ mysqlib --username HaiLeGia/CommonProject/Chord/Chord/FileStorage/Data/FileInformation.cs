using System;

namespace Chord.FileStorage.Data
{
	[Serializable]
	public class FileInformation : Information
	{
		private long length;

		public FileInformation(string fileName, string ownerName, string parentDirectory)
		{
			this.ownerName = ownerName;
			name = fileName;
			this.parentDirectory = parentDirectory;
			security = new Security();
			type = FILE;
		}

		public long Length
		{
			get { return length; }
			set { length = value; }
		}
	}
}