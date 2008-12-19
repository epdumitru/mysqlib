using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace ObjectSerializer
{
    public class TotalFormatter
    {
        public TotalFormatter(int cacheSize)
        {
            
        }

        public byte[] Serialize(IDirtyObject serializeObject)
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, serializeObject);
                return stream.ToArray();
            }
        }

        public byte[] Serialize(object serializeObject)
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, serializeObject);
                return stream.ToArray();
            }
        }

        public T Deserialize<T>(Stream stream) where T : class
        {
            var fomatter = new BinaryFormatter();
            var returnObject = fomatter.Deserialize(stream);
            return returnObject as T;
        }
    }
}
