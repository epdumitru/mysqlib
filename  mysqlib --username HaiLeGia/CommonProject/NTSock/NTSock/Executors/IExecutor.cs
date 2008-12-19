using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NTSock.Executors
{
    public interface IExecutor
    {
		object Execute(object[] parameter);
//        object Execute(string methodDescription, object[] parameters);
    }
}
