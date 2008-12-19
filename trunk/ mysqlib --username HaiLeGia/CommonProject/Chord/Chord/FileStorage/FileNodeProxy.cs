using System.Threading;

namespace Chord.FileStorage
{
	public interface FileNodeProxy
	{
		void CreateDirectory(string ownerName, string directoryName, WaitCallback callback, object excuteContext);
		void CreateDirectory(string ownerName, string pathName, string directoryName, WaitCallback callback, object excuteContext);
		void GetDirectoryInformation(string pathName, string directoryName, WaitCallback callback, object excuteContext);
		void GetFileInformation(string pathName, string directoryName, WaitCallback callback, object excuteContext);

		void CreateFile(string fileName, string path, bool isOverride, WaitCallback callback, object excuteContext);
		void WriteAll(string filePath, byte[] chunkData, WaitCallback callback, object excuteContext);
		void WriteChunk(string filePath, byte[] chunkData, int offset, int count, WaitCallback callback, object excuteContext);
		void WriteChunk(string filePath, byte[] chunkData, WaitCallback callback, object excuteContext);
		void ReadChunk(string filePath, int offset, int length, WaitCallback callback, object excuteContext);
		void ReadAll(string filePath, WaitCallback callback, object excuteContext);
		void DeleteFile(string filePath);
		void MoveFile(string oldPath, string newPath);
	}
}