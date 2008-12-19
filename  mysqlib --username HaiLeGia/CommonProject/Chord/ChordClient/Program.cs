using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chord.Common;
using Chord.FileStorage;

namespace ChordClient
{
	class Program
	{
		static void Main(string[] args)
		{
			ProxyCache cache = new ProxyCache();
			URL url = new URL("electron://127.0.0.1:13187/");
			FileNode node = cache.GetProxy(url);

			string path = node.CreateDirectory("electron", "hailg");
			path = node.CreateDirectory("electron", path, "tieuchau");
			string filePath = node.CreateFile("muon chut.xxx", path, true);
			node.WriteChunk(filePath, new byte[] {1, 2, 3, 4, 5, 6});

			byte[] arr1 = node.ReadChunk(filePath, 0, 3);
			Console.WriteLine(arr1.Length);
			arr1 = node.ReadAll(filePath);
			Console.WriteLine(arr1.Length);
			
			Console.ReadLine();
		}
	}
}
