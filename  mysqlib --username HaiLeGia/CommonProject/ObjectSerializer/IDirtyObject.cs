using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectSerializer
{
    public interface IDirtyObject
    {
        bool Change { get; set; }
    }
}
