using System;

using BLToolkit.Reflection;
using BLToolkit.TypeBuilder.Builders;

namespace BLToolkit.Aspects
{
	/// <summary>
	/// http://www.bltoolkit.net/Doc/Aspects/index.htm
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple=true)]
	public class ClearCacheAttribute : AbstractTypeBuilderAttribute
	{
		#region Constructors

		public ClearCacheAttribute(string methodName)
		{
			_methodName = methodName;
		}

		public ClearCacheAttribute(string methodName, params Type[] parameterTypes)
		{
			_methodName     = methodName;
			_parameterTypes = parameterTypes;
		}

		public ClearCacheAttribute(Type declaringType, string methodName, params Type[] parameterTypes)
		{
			_declaringType  = declaringType;
			_methodName     = methodName;
			_parameterTypes = parameterTypes;
		}

		Type   _declaringType;
		string _methodName;
		Type[] _parameterTypes;

		#endregion

		public override IAbstractTypeBuilder TypeBuilder
		{
			get { return new Builders.ClearCacheAspectBuilder(_declaringType, _methodName, _parameterTypes); }
		}
	}
}
