using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using BLToolkit.Reflection.Emit;
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
//			var list = new List<int>();
//			list.Add(1);
//			list.Add(2);
//			list.Add(3);
//			IList list2 = list;
//			for (int i = 0; i < list2.Count; i++)
//			{
//				Console.WriteLine(list2[i]);
//			}
//			Console.ReadLine();
//			for (int i = 0; i < len; i++)
//			{
//				Console.WriteLine(i);
//			}
			var dbObjectContainer = new DbObjectContainer();
			dbObjectContainer.Register(typeof(UserData).Assembly);
            
//            A a = new A();
//			a.Id = 1;
//			B b = new B();
//			b.Name = "b";
//			b.Id = 1;
//			B b1 = new B();
//			b1.Name = "b1";
//			b1.Id = 2;
//			B b2 = new B();
//			b2.Name = "b2";
//			b2.Id = 3;
//			B b3 = new B();
//			b3.Name = "b3";
//			a.Bt = null;
//			b.At = null;
//			a.Str = "abc2";
//			a.IsDirty = true;
//			dbObjectContainer.QueryExecutor.Update(a, null);
//			dbObjectContainer.QueryExecutor.Update(b, null);





			Console.WriteLine("ok");
//			dbObjectContainer.QueryExecutor.Insert(b, null);
		    


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
			
//            Console.ReadKey();

//			var assemblyBuiler = new AssemblyBuilderHelper("Test.dll");
//			var typeBuilder = assemblyBuiler.DefineType("Test", typeof (object));
//			var testMethodEmit = typeBuilder.DefineMethod("Test", MethodAttributes.Public, typeof (void)).Emitter;
//			var memoryStreamLocal = testMethodEmit.DeclareLocal(typeof (MemoryStream));
//			testMethodEmit
//				.newobj(typeof (MemoryStream), Type.EmptyTypes)
//				.stloc(memoryStreamLocal);
//			var tryFinallyLabel = testMethodEmit.BeginExceptionBlock();
//			testMethodEmit
//				.ldstr("Hello")
//				.call(typeof (Console).GetMethod("WriteLine", new[] {typeof (string)}))
//				.BeginFinallyBlock()
//				.ldloc(memoryStreamLocal)
//				.call(typeof (IDisposable).GetMethod("Dispose"))
//				.EndExceptionBlock()
//				.ret();
//			typeBuilder.Create();
//			assemblyBuiler.Save();
//			Console.WriteLine("OK");
			Console.ReadKey();
				
		}
	}
}
