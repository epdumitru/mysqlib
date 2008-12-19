using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBCache.Core;
using DBCache.UserComponents.Database;

namespace DBCache.UserComponents
{
	public interface IObjectContainer
	{
		Reference Add(object o);
		Reference Update(object o);
		void Delete(object o);
		void Delete(Reference reference);
		object Get(Reference reference);
		T Get<T>(Reference reference);
		IList<T> Get<T>(IQuery query);
		ExecuteResult Execute(IQuery query);
	}
}