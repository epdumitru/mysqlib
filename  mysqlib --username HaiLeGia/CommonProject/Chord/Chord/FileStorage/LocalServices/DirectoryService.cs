using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Chord.FileStorage.Data;

namespace Chord.FileStorage.LocalServices
{
	public class DirectoryService : IDirectoryService
	{
		private const string INFOR_FILE = "_dirinfo.chr";
		private readonly DirectoryInfo sharedFolder;

		public DirectoryService()
		{
			string sharedFolderPath = ConfigurationManager.AppSettings["SharedFolder"];
			sharedFolder = new DirectoryInfo(sharedFolderPath);
			if (!sharedFolder.Exists)
			{
				sharedFolder.Create();
			}
		}

		#region IDirectoryService Members

		public string CreateDirectory(string ownerName, string directoryName)
		{
			string pathName = sharedFolder.FullName + Config.SLASH + directoryName;
			DirectoryInfo info = new DirectoryInfo(pathName);
			if (!info.Exists)
			{
				info.Create();
				FileInfo dirInfor = new FileInfo(pathName + Config.SLASH + INFOR_FILE);
				FileStream stream = dirInfor.Open(FileMode.Create, FileAccess.Write);
				try
				{
					IFormatter formatter = new BinaryFormatter();
					DirectoryInformation infor = new DirectoryInformation(null, directoryName, ownerName);
					formatter.Serialize(stream, infor);
					stream.Close();
				}
				catch
				{
					if (stream != null)
					{
						try
						{
							stream.Close();
						}
						catch
						{
						}
					}
					info.Delete(true);
					return null;
				}
			}
			return directoryName;
		}

		public string CreateDirectory(string ownerName, string pathName, string directoryName)
		{
			if (pathName == null || pathName == String.Empty)
			{
				throw new ArgumentException("pathname cannot be null or empty.");
			}
			string viewPathName = pathName + Config.SLASH + directoryName;
			pathName = sharedFolder.FullName + Config.SLASH + viewPathName;
			DirectoryInfo info = new DirectoryInfo(pathName);
			if (!info.Exists)
			{
				info.Create();
				FileInfo dirInfor = new FileInfo(pathName + Config.SLASH + INFOR_FILE);
				FileStream stream = dirInfor.Open(FileMode.Create, FileAccess.Write);
				try
				{
					IFormatter formatter = new BinaryFormatter();
					DirectoryInformation infor = new DirectoryInformation(info.Parent.FullName, directoryName, ownerName);
					formatter.Serialize(stream, infor);
					stream.Close();
				}
				catch
				{
					if (stream != null)
					{
						try
						{
							stream.Close();
						}
						catch
						{
						}
					}
					info.Delete(true);
					return null;
				}
				return viewPathName;
			}
			return null;
		}

		public DirectoryInformation GetDirectoryInformation(string pathName, string directoryName)
		{
			if (pathName == null || pathName == "")
			{
				pathName = sharedFolder.FullName + Config.SLASH + directoryName;
			}
			else
			{
				pathName = sharedFolder.FullName + Config.SLASH + pathName + Config.SLASH + directoryName;
			}
			DirectoryInfo info = new DirectoryInfo(pathName);
			return GetDirectoryInfor(info);
		}

		public IList<Information> GetFileInformation(string pathName, string directoryName)
		{
			if (pathName == null || pathName == "")
			{
				pathName = sharedFolder.FullName + Config.SLASH + directoryName;
			}
			else
			{
				pathName = sharedFolder.FullName + Config.SLASH + pathName + Config.SLASH + directoryName;
			}
			DirectoryInfo info = new DirectoryInfo(pathName);
			if (info.Exists)
			{
				DirectoryInformation parentInfor = GetDirectoryInfor(info);
				IList<Information> result = new List<Information>();
				DirectoryInfo[] allDirectories = info.GetDirectories();
				for (int i = 0; i < allDirectories.Length; i++)
				{
					DirectoryInfo subDir = allDirectories[i];
					DirectoryInformation subDirInfo = GetDirectoryInfor(subDir);
					if (subDirInfo != null)
					{
						result.Add(subDirInfo);
					}
				}
				FileInfo[] fileInfos = info.GetFiles();
				for (int i = 0; i < fileInfos.Length; i++)
				{
					FileInfo subFileInfo = fileInfos[i];
					FileInformation fileInformation = new FileInformation(subFileInfo.Name, parentInfor.OwnerName, info.FullName);
					fileInformation.Length = subFileInfo.Length;
					result.Add(fileInformation);
				}
				return result;
			}
			return null;
		}

		#endregion

		private static DirectoryInformation GetDirectoryInfor(DirectoryInfo info)
		{
			if (info.Exists)
			{
				FileInfo dirInfor = new FileInfo(info.FullName + Config.SLASH + INFOR_FILE);
				FileStream stream = dirInfor.Open(FileMode.Open, FileAccess.Read);
				try
				{
					IFormatter formatter = new BinaryFormatter();
					DirectoryInformation infor = (DirectoryInformation) formatter.Deserialize(stream);
					return infor;
				}
				catch
				{
					return null;
				}
				finally
				{
					stream.Close();
				}
			}
			return null;
		}
	}
}