#define DEBUG
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using BLToolkit.Reflection;
using BLToolkit.Reflection.Emit;
using NTSockv3.Messages;
using NTSockv3.Sockets;

namespace NTSockv3.Excutors
{
	public class ExecutorFactory
	{
		public static void CreateProxy(object service, AssemblyBuilderHelper assemblyBuilderHelper)
		{
			if (service == null)
			{
				throw new ArgumentNullException("Cannot create null proxy");
			}
			var serviceType = service.GetType();
			var proxyTypeName = serviceType.Namespace + "." + serviceType.Name + "Proxy";
			var proxyType = assemblyBuilderHelper.DefineType(proxyTypeName, typeof (IProxy));
			var serviceContainerField = typeof (IProxy).GetField("container", BindingFlags.Instance | BindingFlags.NonPublic);
			var poolField = typeof (IProxy).GetField("pool", BindingFlags.Instance | BindingFlags.NonPublic);
			var defaultConstructor = typeof (IProxy).GetConstructor(Type.EmptyTypes);
			var construtorEmit = proxyType.DefinePublicConstructor(typeof (ServiceContainer), typeof (SocketPool)).Emitter;
			construtorEmit
				.ldarg_0
				.call(defaultConstructor)
				.ldarg_0
				.ldarg_1
				.stfld(serviceContainerField)
				.ldarg_0
				.ldarg_2
				.stfld(poolField)
				.ret();
			var methods = serviceType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
			for (var i = 0; i < methods.Length; i++)
			{
				var method = methods[i];
				if (method.GetCustomAttributes(typeof(ServiceMethodAttribute), false).Length == 0)
				{
					continue;
				}
				var returnType = method.ReturnType;
				var requestTypeName = "Request." + serviceType.Name + "_" + method.Name + i + "Request";
				var requestType = assemblyBuilderHelper.AssemblyBuilder.GetType(requestTypeName);
				if (requestType == null)
				{
					throw new ApplicationException("Cannot find request type: " + requestTypeName);
				}
				var responseTypeName = "Response." + serviceType.Name + "_" + method.Name + i + "Response";
				var responseType = returnType != typeof(void) ? assemblyBuilderHelper.AssemblyBuilder.GetType(responseTypeName) : typeof(VoidResultResponse);
				if (responseType == null)
				{
					throw new ApplicationException("Cannot find response type: " + responseTypeName);
				}
				var paramInfos = method.GetParameters();
				var synParamTypes = new Type[paramInfos.Length];
				var asyncParamTypes = new Type[paramInfos.Length + 2];
				asyncParamTypes[0] = typeof(WaitCallback); //callback function
				asyncParamTypes[1] = typeof(object); //context param
				for (var j = 0; j < synParamTypes.Length; j++)
				{
					synParamTypes[j] = paramInfos[j].ParameterType;
					asyncParamTypes[j + 2] = paramInfos[j].ParameterType;
				}

				//Generate synchronous proxy method
				var synchronousMethodEmit = synParamTypes.Length == 0
												? proxyType.DefineMethod(method.Name, MethodAttributes.Public, returnType,
																		 Type.EmptyTypes).Emitter
												: proxyType.DefineMethod(method.Name, MethodAttributes.Public, returnType, synParamTypes).Emitter;
				var syncRequestLocal = synchronousMethodEmit.DeclareLocal(requestType);
				synchronousMethodEmit
					.newobj(requestType)
					.stloc(syncRequestLocal);
				synchronousMethodEmit = synchronousMethodEmit.ldarg_0;
				for (var j = 1; j <= paramInfos.Length; j++)
				{
					synchronousMethodEmit
						.ldloc(syncRequestLocal)
						.ldarg(j)
						.stfld(requestType.GetField(paramInfos[j - 1].Name));
				}

				if (returnType == typeof(void))
				{
					synchronousMethodEmit
						.ldloc(syncRequestLocal)
						.call(typeof (IProxy).GetMethod("DoRequest", BindingFlags.Instance | BindingFlags.NonPublic))
						.ret();
				}
				else
				{
					var doRequestMethod =
						typeof (IProxy).GetMethod("RequestSync", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(
							returnType);
					synchronousMethodEmit
						.ldloc(syncRequestLocal)
						.call(doRequestMethod)
						.ret();
				}

				//Generate asynchronous proxy method
				var asynchronousMethodEmit = proxyType.DefineMethod("Begin" + method.Name, MethodAttributes.Public, null, asyncParamTypes).Emitter;
				var asynRequestLocal = asynchronousMethodEmit.DeclareLocal(requestType);
                asynchronousMethodEmit
					.newobj(requestType)
					.stloc(asynRequestLocal);
				asynchronousMethodEmit = asynchronousMethodEmit.ldarg_0;
				for (var j = 1; j <= paramInfos.Length; j++)
				{
					asynchronousMethodEmit
						.ldloc(asynRequestLocal)
						.ldarg(j + 2)
						.stfld(requestType.GetField(paramInfos[j - 1].Name));
				}
				asynchronousMethodEmit
					.ldloc(asynRequestLocal)
					.ldarg_1
					.ldarg_2
					.call(typeof(IProxy).GetMethod("BeginRequest", BindingFlags.Instance | BindingFlags.NonPublic))
					.ret();
			}
			proxyType.Create();
		}

		public static IExecutor CreateExecutor(object service, MethodInfo method, int methodIndex, AssemblyBuilderHelper assemblyBuilderHelper)
		{
			if (service == null)
			{
				throw new ArgumentNullException("Cannot create null executor");
			}
			var type = service.GetType();
			var typeName = type.Name + "_" + method.Name + methodIndex;
			var typeBuilderHelper = assemblyBuilderHelper.DefineType("Executor." + typeName, typeof(object), typeof (IExecutor));
			var serviceFieldBuilder = typeBuilderHelper.DefineField(type.Name, type, FieldAttributes.Public);
			var emit = typeBuilderHelper.DefineMethod(typeof(IExecutor).GetMethod("Execute")).Emitter;
			var returnType = method.ReturnType;
			var paramInfos = method.GetParameters();
			var requestType = CreateRequestType(typeName, paramInfos, assemblyBuilderHelper);
			var responseType = CreateResponseType(typeName, returnType, assemblyBuilderHelper);
			var getExecutorKeyEmit = typeBuilderHelper.DefineMethod(typeof(IExecutor).GetMethod("get_ExecutorKey")).Emitter;
			getExecutorKeyEmit
				.ldstr(typeName)
				.ret();

			emit
				.ldarg_0
				.ldfld(serviceFieldBuilder);
			for (var j = 0; j < paramInfos.Length; j++)
			{
				var paramInfo = paramInfos[j];
				var fieldInfo = requestType.GetField(paramInfo.Name);
				emit = emit
					.ldarg_1
					.ldfld(fieldInfo);
			}
			if (returnType != typeof(void))
			{
				var resultLocal = emit.DeclareLocal(returnType);
				var responseLocal = emit.DeclareLocal(responseType);
				emit
					.call(method)
					.stloc(resultLocal)
					.newobj(responseType)
					.stloc(responseLocal)
					.ldloc(responseLocal)
					.ldloc(resultLocal)
					.stfld(responseType.GetField("result"))
					.ldloc(responseLocal)
					.ret();
			}
			else
			{
				emit
					.call(method)
					.newobj(typeof(VoidResultResponse))
					.ret();
			}
			var executorType = typeBuilderHelper.Create();
			var executor = (IExecutor) TypeAccessor.CreateInstance(executorType);
			var serviceField = executorType.GetField(type.Name);
			serviceField.SetValue(executor, service);
			return executor;
		}

		private static Type CreateResponseType(string typeName, Type returnType, AssemblyBuilderHelper assemblyBuilderHelper)
		{
			if (returnType == typeof(void))
			{
				return typeof(VoidResultResponse);
			}
			var responseType = typeName + "Response";
			var typeBuilderHelper = assemblyBuilderHelper.DefineType("Response." + responseType, typeof(Response));
			var resultField = typeBuilderHelper.DefineField("result", returnType, FieldAttributes.Public);
			var constructorEmit = typeBuilderHelper.DefinePublicConstructor(returnType).Emitter;
			var defaulConstructor = typeof(Response).GetConstructor(Type.EmptyTypes);
			constructorEmit
				.ldarg_0
				.call(defaulConstructor)
				.ldarg_0
				.ldarg_1
				.stfld(resultField)
				.ret();
			constructorEmit = typeBuilderHelper.DefinePublicConstructor(Type.EmptyTypes).Emitter;
			constructorEmit
				.ldarg_0
				.call(defaulConstructor)
				.ret();
			var resultProperty = typeBuilderHelper.TypeBuilder.DefineProperty("Result", PropertyAttributes.None, returnType,
			                                                                  new[] {returnType});
			var getResultPropertyMethod = typeBuilderHelper.DefineMethod("get_Result",
			                                                             MethodAttributes.Public | MethodAttributes.SpecialName |
			                                                             MethodAttributes.HideBySig, returnType, Type.EmptyTypes);
			var getResultPropertyMethodEmit = getResultPropertyMethod.Emitter;
			getResultPropertyMethodEmit
				.ldarg_0
				.ldfld(resultField)
				.ret();
			var setResultPropertyMethod = typeBuilderHelper.DefineMethod("set_Result",
			                                                             MethodAttributes.Public | MethodAttributes.SpecialName |
			                                                             MethodAttributes.HideBySig, null, returnType);
			var setResultPropertyMethodEmit = setResultPropertyMethod.Emitter;
			setResultPropertyMethodEmit
				.ldarg_0
				.ldarg_1
				.stfld(resultField)
				.ret();
			resultProperty.SetGetMethod(getResultPropertyMethod);
			resultProperty.SetSetMethod(setResultPropertyMethod);

			var getResultMethod = typeof (Response).GetMethod("GetResult");
			var getResultMethodEmit = typeBuilderHelper.DefineMethod(getResultMethod).Emitter;
			getResultMethodEmit
				.ldarg_0
				.ldfld(resultField)
				.boxIfValueType(returnType)
				.ret();
			return typeBuilderHelper.Create();
		}

		private static Type CreateRequestType(string typeName, ParameterInfo[] infos, AssemblyBuilderHelper assemblyBuilderHelper)
		{
			var requestTypeName = typeName + "Request";
			var typeBuilderHelper = assemblyBuilderHelper.DefineType("Request." + requestTypeName, typeof(Request));
			var requestDescriptionField = typeof (Request).GetField("requestDescription");
			var fieldInfos = new FieldInfo[infos.Length];
			var propertyInfos = new PropertyBuilder[infos.Length];

			var constructorEmit = typeBuilderHelper.DefinePublicConstructor(Type.EmptyTypes).Emitter;
			var defaultConstructor = typeof (Request).GetConstructor(Type.EmptyTypes);
			constructorEmit
				.ldarg_0
				.call(defaultConstructor)
				.ldarg_0
				.ldstr(typeName)
				.stfld(requestDescriptionField)
				.ret();
			for (var i = 0; i < infos.Length; i++)
			{
				fieldInfos[i] = typeBuilderHelper.DefineField(infos[i].Name, infos[i].ParameterType, FieldAttributes.Public);
				var propertyName = char.IsLower(infos[i].Name[0])
										? char.ToUpper(infos[i].Name[0]) + infos[i].Name.Substring(1)
										: infos[i].Name + "_";
				propertyInfos[i] = typeBuilderHelper.TypeBuilder.DefineProperty(propertyName, PropertyAttributes.None,
																				infos[i].ParameterType,
																				new[] { infos[i].ParameterType });
				var getPropertyMethod = typeBuilderHelper.DefineMethod("get_" + propertyName,
				                                                       MethodAttributes.Public | MethodAttributes.SpecialName |
				                                                       MethodAttributes.HideBySig, infos[i].ParameterType,
				                                                       Type.EmptyTypes);
				var getPropertyMethodEmit = getPropertyMethod.Emitter;
				getPropertyMethodEmit
					.ldarg_0
					.ldfld(fieldInfos[i])
					.ret();
				var setPropertyMethod = typeBuilderHelper.DefineMethod("set_" + propertyName,
				                                                       MethodAttributes.Public | MethodAttributes.SpecialName |
				                                                       MethodAttributes.HideBySig, null, infos[i].ParameterType);
				var setPropertyMethodEmit = setPropertyMethod.Emitter;
				setPropertyMethodEmit
					.ldarg_0
					.ldarg_1
					.stfld(fieldInfos[i])
					.ret();
				propertyInfos[i].SetGetMethod(getPropertyMethod);
				propertyInfos[i].SetSetMethod(setPropertyMethod);
			}
			return typeBuilderHelper.Create();
		}
	}
}