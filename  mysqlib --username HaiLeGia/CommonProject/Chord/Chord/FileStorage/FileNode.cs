using System;
using System.Collections.Generic;
using Chord.Common;
using Chord.FileStorage.Data;
using Chord.FileStorage.LocalServices;

namespace Chord.FileStorage
{
	[Serializable]
	public abstract class FileNode : Node<FileNode>, IDirectoryService, IFileService
	{
		public FileNode(ID nodeId, URL nodeURL)
		{
			this.nodeId = nodeId;
			this.nodeURL = nodeURL;
		}

		public FileNode(URL nodeURL)
		{
			nodeId = HashFunction.GenerateID(nodeURL);
			this.nodeURL = nodeURL;
		}

		#region IDirectoryService Members

		public abstract string CreateDirectory(string ownerName, string directoryName);
		public abstract string CreateDirectory(string ownerName, string pathName, string directoryName);
		public abstract DirectoryInformation GetDirectoryInformation(string pathName, string directoryName);
		public abstract IList<Information> GetFileInformation(string pathName, string directoryName);

		#endregion

		#region IFileService Members

		public abstract string CreateFile(string fileName, string path, bool isOverride);
		public abstract bool WriteAll(string filePath, byte[] chunkData);
		public abstract bool WriteChunk(string filePath, byte[] chunkData, int offset, int count);
		public abstract bool WriteChunk(string filePath, byte[] chunkData);
		public abstract byte[] ReadChunk(string filePath, int offset, int length);
		public abstract byte[] ReadAll(string filePath);
		public abstract void DeleteFile(string filePath);
		public abstract void MoveFile(string oldPath, string newPath);

		#endregion
	}
}