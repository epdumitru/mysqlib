using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using Chord.Common;
using Chord.FileStorage.Data;
using Chord.FileStorage.LocalServices;
using Logger;
using NTSock;

namespace Chord.FileStorage
{
	[Serializable]
	public class FileNodeImpl : FileNode
	{
		[NonSerialized] protected IDirectoryService directoryService;
		[NonSerialized] protected IFileService fileService;

		[NonSerialized] private ServiceHost host;
		[NonSerialized] private References<FileNode> references;

		public FileNodeImpl(ID nodeId, URL nodeURL) : base(nodeId, nodeURL)
		{
			int successorCapacity = Int32.Parse(ConfigurationManager.AppSettings["FanOut"]);
			references = new References<FileNode>(nodeId, nodeURL, successorCapacity);
		}

		public FileNodeImpl(URL nodeURL)
			: base(nodeURL)
		{
			int successorCapacity = Int32.Parse(ConfigurationManager.AppSettings["FanOut"]);
			references = new References<FileNode>(nodeId, nodeURL, successorCapacity);
		}

		#region Node

		/// <summary>
		/// Returns the Chord node which is responsible for the given key.
		/// </summary>
		/// <param name="key">Key for which the successor is searched for</param>
		/// <returns>Responsible node</returns>
		public override FileNode FindSuccessor(ID key)
		{
			if (key == null)
			{
				throw new ArgumentNullException();
			}
			FileNode successor = references.Successor;
			if (successor == null) //this is the only node in out network
			{
				return this;
			}
			if (key.IsInInterval(nodeId, successor.NodeId) || key.Equals(successor.NodeId))
			{
				return successor;
			}
			else
			{
				FileNode closestPrecedingNode = references.GetClosestPrecedingNode(key);
				FileNode result = closestPrecedingNode.FindSuccessor(key);
				if (result == null)
				{
					throw new ApplicationException("Error when find node for key: " + key);
				}
				return closestPrecedingNode.FindSuccessor(key);
			}
		}

		/// <summary>
		/// Join to a chord network
		/// </summary>
		/// <param name="bootstrapURL"></param>
		public override void JoinNetwork(URL bootstrapURL)
		{
		}

		/// <summary>
		/// Create a chord network
		/// </summary>
		public override void Create()
		{
			int port = nodeURL.Port;
			host = new ServiceHost(port);
			fileService = new FileService();
			directoryService = new DirectoryService();
			host.Container.RegisterService("FileNodeService", this, false);
			host.Open();
		}

		/// <summary>
		/// Requests a sign of live. 
		/// </summary>
		public override void Ping()
		{
			// Do nothing
		}

		/// <summary>
		/// Inform a node that its predecessor leaves the network.
		/// </summary>
		/// <param name="predecessor"></param>
		public override void LeavesNetwork(FileNode predecessor)
		{
		}

		/// <summary>
		/// Closes the connection to the node.
		/// </summary>
		public override void Disconnect()
		{
			return;
		}

		#endregion

		#region FileSystemService

		public override string CreateDirectory(string ownerName, string directoryName)
		{
			ID id = HashFunction.GenerateID(directoryName);
			FileNode node = FindSuccessor(id);
			if (node == null)
			{
				throw new ApplicationException("Cannot find server for directory: " + directoryName);
			}
			if (node == this)
			{
				return directoryService.CreateDirectory(ownerName, directoryName);
			}
			else
			{
				return node.CreateDirectory(ownerName, directoryName);
			}
		}

		public override string CreateDirectory(string ownerName, string pathName, string directoryName)
		{
			FileNode node = GetNode(pathName);
			if (node == null)
			{
				throw new ApplicationException("Cannot find node for Path: " + pathName);
			}
			if (node == this)
			{
				return directoryService.CreateDirectory(ownerName, pathName, directoryName);
			}
			else
			{
				return node.CreateDirectory(ownerName, pathName, directoryName);
			}
		}

		public override DirectoryInformation GetDirectoryInformation(string pathName, string directoryName)
		{
			FileNode node = GetNode(pathName);
			if (node == null)
			{
				throw new ApplicationException("Cannot find node for Path: " + pathName);
			}
			if (node == this)
			{
				return directoryService.GetDirectoryInformation(pathName, directoryName);
			}
			else
			{
				return node.GetDirectoryInformation(pathName, directoryName);
			}
		}

		public override IList<Information> GetFileInformation(string pathName, string directoryName)
		{
			FileNode node = GetNode(pathName);
			if (node == null)
			{
				throw new ApplicationException("Cannot find node for Path: " + pathName);
			}
			if (node == this)
			{
				return directoryService.GetFileInformation(pathName, directoryName);
			}
			else
			{
				return node.GetFileInformation(pathName, directoryName);
			}
		}

		public override string CreateFile(string fileName, string path, bool isOverride)
		{
			FileNode node = GetNode(path);
			if (node == null)
			{
				throw new ApplicationException("Cannot find node for Path: " + path);
			}
			if (node == this)
			{
				return fileService.CreateFile(fileName, path, isOverride);
			}
			else
			{
				return node.CreateFile(fileName, path, isOverride);
			}
		}

		public override bool WriteChunk(string filePath, byte[] chunkData, int offset, int count)
		{
			FileNode node = GetNode(filePath);
			if (node == null)
			{
				throw new ApplicationException("Cannot find node for Path: " + filePath);
			}
			if (node == this)
			{
				return fileService.WriteChunk(filePath, chunkData, offset, count);
			}
			else
			{
				return node.WriteChunk(filePath, chunkData, offset, count);
			}
		}

		public override bool WriteChunk(string filePath, byte[] chunkData)
		{
			FileNode node = GetNode(filePath);
			if (node == null)
			{
				throw new ApplicationException("Cannot find node for Path: " + filePath);
			}
			if (node == this)
			{
				return fileService.WriteChunk(filePath, chunkData);
			}
			else
			{
				return node.WriteChunk(filePath, chunkData);
			}
		}

		public override byte[] ReadChunk(string filePath, int offset, int length)
		{
			FileNode node = GetNode(filePath);
			if (node == null)
			{
				throw new ApplicationException("Cannot find node for Path: " + filePath);
			}
			if (node == this)
			{
				return fileService.ReadChunk(filePath, offset, length);
			}
			else
			{
				return node.ReadChunk(filePath, offset, length);
			}
		}

		public override byte[] ReadAll(string filePath)
		{
			FileNode node = GetNode(filePath);
			if (node == null)
			{
				throw new ApplicationException("Cannot find node for Path: " + filePath);
			}
			if (node == this)
			{
				return fileService.ReadAll(filePath);
			}
			else
			{
				return node.ReadAll(filePath);
			}
		}

		public override void DeleteFile(string filePath)
		{
			FileNode node = GetNode(filePath);
			if (node == null)
			{
				throw new ApplicationException("Cannot find node for Path: " + filePath);
			}
			if (node == this)
			{
				fileService.DeleteFile(filePath);
			}
			else
			{
				node.DeleteFile(filePath);
			}
		}

		public override void MoveFile(string oldPath, string newPath)
		{
			FileNode node = GetNode(oldPath);
			if (node == null)
			{
				throw new ApplicationException("Cannot find node for Path: " + oldPath);
			}
			if (node == this)
			{
				fileService.MoveFile(oldPath, newPath);
			}
			else
			{
				node.MoveFile(oldPath, newPath);
			}
		}

		public override bool WriteAll(string filePath, byte[] chunkData)
		{
			FileNode node = GetNode(filePath);
			if (node == null)
			{
				Log.WriteLog(new ApplicationException("Cannot find node for Path: " + filePath));
				throw new ApplicationException("Cannot find node for Path: " + filePath);
			}
			if (node == this)
			{
				return fileService.WriteAll(filePath, chunkData);
			}
			else
			{
				return node.WriteAll(filePath, chunkData);
			}
		}

		private string GetParentDir(string path)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < path.Length; i++)
			{
				if (path[i] != Config.SLASH)
				{
					sb.Append(path[i]);
				}
				else
				{
					break;
				}
			}
			return sb.ToString();
		}

		private FileNode GetNode(string path)
		{
			string parentDir = GetParentDir(path);
			ID id = HashFunction.GenerateID(parentDir);
			return FindSuccessor(id);
		}

		#endregion
	}
}