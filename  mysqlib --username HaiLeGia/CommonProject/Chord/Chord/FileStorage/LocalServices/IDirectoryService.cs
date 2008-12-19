using System.Collections.Generic;
using Chord.FileStorage.Data;

namespace Chord.FileStorage.LocalServices
{
	public interface IDirectoryService
	{
		string CreateDirectory(string ownerName, string directoryName);
		string CreateDirectory(string ownerName, string pathName, string directoryName);
		DirectoryInformation GetDirectoryInformation(string pathName, string directoryName);
		IList<Information> GetFileInformation(string pathName, string directoryName);
	}
}