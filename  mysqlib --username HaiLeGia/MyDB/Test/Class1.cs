﻿using System;
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
			
			var dbObjectContainer = new DbObjectContainer();
			dbObjectContainer.Register(typeof(UserData).Assembly);



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
