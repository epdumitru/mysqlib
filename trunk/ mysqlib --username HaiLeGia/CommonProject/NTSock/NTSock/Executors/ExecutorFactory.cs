#define DEBUG
using System;
using System.Reflection;
using System.Reflection.Emit;
using BLToolkit.Reflection;
using BLToolkit.Reflection.Emit;

namespace NTSock.Executors
{
	public interface IHello
	{
		void SayHello(string name);
	}

	public class Class2
	{
		public object DoIt()
		{
			Console.WriteLine("Do it");
			return null;
		}

		public int DoIt2(string a, int b, DateTime time, Class2 class2, IHello helloParam)
		{
			return a.Length + b;
		}

		public object Test(string description, object[] parammeters, DateTime time)
		{
			switch (description)
			{
				case "0":
					DoIt();
					break;
				case "1":
					return DoIt2((string)parammeters[0], (int)parammeters[1], DateTime.Now, this, (IHello)parammeters[4]);
			}
			return null;
		}
	}

    public class ExecutorFactory
    {
        public static IExecutor CreateExecutor(object service, MethodInfo method)
        {
            if (service == null)
            {
                throw new ArgumentNullException("Cannot create a null executor");
            }
            var type = service.GetType();
			var typeName = type.Name + method.Name;
			var assemblyBuilderHelper = new AssemblyBuilderHelper(typeName + "_Executor" + ".dll");
            var typeBuilderHelper = assemblyBuilderHelper.DefineType(typeName, typeof(object),
                                                                                typeof (IExecutor));
            var serviceFieldBuilder = typeBuilderHelper.DefineField(type.Name, type, FieldAttributes.Public);
			var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            var emit = typeBuilderHelper.DefineMethod(typeof(IExecutor).GetMethod("Execute")).Emitter;
			var returnType = method.ReturnType;
			var paramInfos = method.GetParameters();
			emit
				.ldarg_0
				.ldfld(serviceFieldBuilder);
			for (var j = 0; j < paramInfos.Length; j++)
			{
				var paramInfo = paramInfos[j];
				var paramType = paramInfo.ParameterType;
				emit = emit
							.ldarg_1
							.ldc_i4(j)
							.ldelem_ref;
				if (paramType.IsValueType)
				{
					emit
						.unbox_any(paramType);
				}
				else if (paramType.IsClass || paramType.IsInterface)
				{
					emit.castclass(paramType);
				}
			}
			emit.call(method);
			if (returnType != typeof(void))
			{
				emit
					.boxIfValueType(returnType)
					.ret();
			}
			else
			{
				emit
					.ldnull
					.ret();
			}
			var executorType = typeBuilderHelper.Create();
            var executor = (IExecutor) TypeAccessor.CreateInstance(executorType);
            var serviceField = executorType.GetField(type.Name);
            serviceField.SetValue(executor, service);
#if DEBUG
            assemblyBuilderHelper.Save();
#endif
            return executor;
        }
    }
}
