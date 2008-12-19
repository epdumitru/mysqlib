using System;
using System.Configuration;
using System.IO;
using Logger;

namespace Chord.FileStorage.LocalServices
{
	public class FileService : IFileService
	{
		private readonly DirectoryInfo sharedFolder;

		public FileService()
		{
			string sharedFolderPath = ConfigurationManager.AppSettings["SharedFolder"];
			sharedFolder = new DirectoryInfo(sharedFolderPath);
			if (!sharedFolder.Exists)
			{
				sharedFolder.Create();
			}
		}

		#region IFileService Members

		public string CreateFile(string fileName, string path, bool isOverride)
		{
			if (path == null || path == String.Empty)
			{
				throw new ArgumentException("Path cannot be null or empty");
			}
			string viewPath = path + Config.SLASH + fileName;
			path = sharedFolder.FullName + Config.SLASH + viewPath;
			FileInfo infor = new FileInfo(path);
			lock (path)
			{
				if (infor.Exists && !isOverride)
				{
					throw new ArgumentException("File name already exists.");
				}
				try
				{
					FileStream stream = infor.Create();
					if (stream != null)
					{
						try
						{
							stream.Close();
						}
						catch
						{
						}
						return viewPath;
					}
					else
					{
						return null;
					}
				}
				catch
				{
				}
				return null;
			}
		}

		public bool WriteAll(string filePath, byte[] chunkData)
		{
			if (filePath == null || filePath == String.Empty)
			{
				Log.WriteLog(new ArgumentException("File path cannot be null or empty"));
				throw new ArgumentException("File path cannot be null or empty");
			}
			filePath = sharedFolder.FullName + Config.SLASH + filePath;
			FileInfo infor = new FileInfo(filePath);
			lock (filePath)
			{
				FileStream fStream = infor.Open(FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
				try
				{
					fStream.Write(chunkData, 0, chunkData.Length);
					return true;
				}
				catch
				{
					return false;
				}
				finally
				{
					if (fStream != null)
					{
						fStream.Close();
					}
				}
			}
		}

		public bool WriteChunk(string filePath, byte[] chunkData, int offset, int count)
		{
			if (filePath == null || filePath == String.Empty)
			{
				throw new ArgumentException("File path cannot be null or empty");
			}
			filePath = sharedFolder.FullName + Config.SLASH + filePath;
			FileInfo infor = new FileInfo(filePath);
			lock (filePath)
			{
				if (!infor.Exists)
				{
					throw new ArgumentException("Cannot find the file specified");
				}
				FileStream fStream = infor.Open(FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
				try
				{
					fStream.Write(chunkData, offset, count);
					return true;
				}
				catch
				{
					return false;
				}
				finally
				{
					if (fStream != null)
					{
						fStream.Close();
					}
				}
			}
		}

		public bool WriteChunk(string filePath, byte[] chunkData)
		{
			return WriteChunk(filePath, chunkData, 0, chunkData.Length);
		}

		public byte[] ReadChunk(string filePath, int offset, int length)
		{
			if (filePath == null || filePath == String.Empty)
			{
				throw new ArgumentException("File path cannot be null or empty");
			}
			filePath = sharedFolder.FullName + Config.SLASH + filePath;
			FileInfo infor = new FileInfo(filePath);
			lock (filePath)
			{
				if (!infor.Exists)
				{
					throw new ArgumentException("Cannot find the file specified");
				}
				FileStream fStream = infor.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				try
				{
					fStream.Seek(offset, SeekOrigin.Begin);
					byte[] result = new byte[length];
					int len = fStream.Read(result, 0, result.Length);
					if (len < result.Length)
					{
						byte[] result2 = new byte[len];
						Array.Copy(result, result2, len);
						return result2;
					}
					else
					{
						return result;
					}
				}
				catch
				{
					return null;
				}
				finally
				{
					if (fStream != null)
					{
						fStream.Close();
					}
				}
			}
		}

		private byte[] emptyBytes = new byte[0];
		public byte[] ReadAll(string filePath)
		{
			if (filePath == null || filePath == String.Empty)
			{
				throw new ArgumentException("File path cannot be null or empty");
			}
			filePath = sharedFolder.FullName + Config.SLASH + filePath;
			FileInfo infor = new FileInfo(filePath);
			lock (filePath)
			{
				if (!infor.Exists)
				{
					return emptyBytes;
				}
				FileStream fStream = infor.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				try
				{
					fStream.Seek(0, SeekOrigin.Begin);
					byte[] result = new byte[infor.Length];
					fStream.Read(result, 0, result.Length);
					return result;
				}
				catch
				{
					return null;
				}
				finally
				{
					if (fStream != null)
					{
						fStream.Close();
					}
				}
			}
		}

		public void DeleteFile(string filePath)
		{
			if (filePath == null || filePath == String.Empty)
			{
				throw new ArgumentException("File path cannot be null or empty");
			}
			filePath = sharedFolder.FullName + Config.SLASH + filePath;
			FileInfo infor = new FileInfo(filePath);
			lock (filePath)
			{
				if (infor.Exists)
				{
					infor.Delete();
				}
			}
		}

		public void MoveFile(string oldPath, string newPath)
		{
			if (oldPath == null || oldPath == String.Empty || newPath == null || newPath == String.Empty)
			{
				throw new ArgumentException("File path cannot be null or empty");
			}
			oldPath = sharedFolder.FullName + Config.SLASH + oldPath;
			newPath = sharedFolder.FullName + Config.SLASH + newPath;
			FileInfo newFileInfor = new FileInfo(newPath);
			if (newFileInfor.Exists)
			{
				try
				{
					newFileInfor.Delete();
				}
				catch
				{
				}
			}
			FileInfo infor = new FileInfo(oldPath);
			lock (oldPath)
			{
				if (infor.Exists)
				{
					infor.MoveTo(newPath);
				}
			}
		}

		#endregion
	}
}