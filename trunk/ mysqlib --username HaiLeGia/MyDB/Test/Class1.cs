using System;
using ObjectMapping;
using Persistents;


namespace Test
{
	public class Class1
	{
		public Type Test()
		{
			return typeof (Class1);
		}

		public static void Main(string[] args)
		{
			var dbObjectContainer = new DbObjectContainer();
			dbObjectContainer.Register(typeof(UserData).Assembly);


//			dbObjectContainer.QueryExecutor.Insert();


//			var formatter1 = new BinaryFormatter();
//			TestObject2 testObject = new TestObject2();
//			Stopwatch watch = new Stopwatch();
//			watch.Start();
//			byte[] arr = formatter1.Serialize(testObject); ;
//			Console.WriteLine(arr.Length);
//			using (var stream = new MemoryStream(arr))
//			{
//				var object2 = formatter1.Deserialize<TestObject2>(stream);
//				Console.WriteLine(object2.Name);
//			}
			//			var originalAsm = typeof(Persistents.TestObjectHolder).Assembly;
			//			SerializableTypeFactory.CreateSerializableAssembly(originalAsm);
			//			Create object
			//			var object1 = new Persistents.TestObjectHolder();


			//			Stopwatch watch = new Stopwatch();
			//			watch.Start();
			//			for (var i = 0; i < 100000; i++)
			//			{
			//			}
			//			watch.Stop();
			//			Console.WriteLine("Execution time: " + watch.ElapsedMilliseconds);
//						var formatter2 = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
//						watch.Start();
//						for (var i = 0; i < 100000; i++)
//						{
//							using (var stream = new MemoryStream())
//							{
//								formatter2.Serialize(stream, testObject);
//								arr = stream.ToArray();
//								Console.WriteLine(arr.Length);
//							}
//							using (var stream = new MemoryStream(arr))
//							{
//								formatter2.Deserialize(stream);
//							}
//						}
//						watch.Stop();
//						Console.WriteLine("Execution time: " + watch.ElapsedMilliseconds);
			Console.ReadKey();
		}
	}
}
