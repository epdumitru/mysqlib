using System;
using System.IO;
using BLToolkit.Reflection;
using BLToolkit.Reflection.Emit;

namespace DynamicObject
{
	public class ArrayClass
	{
		private int[] intArray;
		private string[] strArray;
		private ArrayClass[] objArray;

		public void Serialize(Stream stream)
		{
			int i;
			int len;
			BinaryWriter writer = new BinaryWriter(stream);
			len = intArray.Length;
			writer.Write(len);
			for (i = 0; i < len; i++)
			{
				writer.Write(intArray[i]);
			}
			len = strArray.Length;
			writer.Write(len);
			for (i = 0; i < len; i++)
			{
				writer.Write(strArray[i]);
			}
			len = objArray.Length;
			writer.Write(len);
			for (i = 0; i < len; i++)
			{
				objArray[i].Serialize(stream);
			}
		}
	}
    public interface IHello
    {
        void SayHello(string name);
    }

    public class Class1
    {
        public static void Main(string[] args)
        {
			Console.WriteLine(DateTime.MinValue);
        	Console.ReadKey();
//        	var typeBuilderHelper = new AssemblyBuilderHelper("HelloWorld.dll").DefineType("Hello",
//        	                                                                                         typeof (object),
//        	                                                                                         typeof (IHello));
//            var emit = typeBuilderHelper.DefineMethod(typeof(IHello).GetMethod("SayHello")).Emitter;
//            for (var i = 0; i < 10; i++)
//            {
//				emit
//                    .ldstr("Hello, {0} World")
//                    .ldarg_1
//                    .call(typeof(string), "Format", typeof(string), typeof(object))
//                    .call(typeof(Console), "WriteLine", typeof(string))
//                    ;
//            }
//            emit.ret();
//            var type = emit.Method.Type.Create();
//            var hello = (IHello) TypeAccessor.CreateInstance(type);
//            hello.SayHello("Hailg");
//            Console.ReadLine();
        }
    }
}
