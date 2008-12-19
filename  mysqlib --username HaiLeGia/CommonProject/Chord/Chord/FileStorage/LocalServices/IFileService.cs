namespace Chord.FileStorage.LocalServices
{
	public interface IFileService
	{
		string CreateFile(string fileName, string path, bool isOverride);
		bool WriteAll(string filePath, byte[] chunkData); //Create a new file with that chunk data
		bool WriteChunk(string filePath, byte[] chunkData, int offset, int count);
		bool WriteChunk(string filePath, byte[] chunkData);
		byte[] ReadChunk(string filePath, int offset, int length);
		byte[] ReadAll(string filePath);
		void DeleteFile(string filePath);
		void MoveFile(string oldPath, string newPath);
	}
}