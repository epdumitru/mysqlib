using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;

namespace ObjectMapping
{
	internal class DbSerializerHelper
	{
		private BinaryWriter writer;
		private BinaryReader reader;

		public DbSerializerHelper(BinaryWriter writer)
		{
			this.writer = writer;
		}

		public DbSerializerHelper(BinaryReader reader)
		{
			this.reader = reader;
		}

		#region Write primitive
		public void Write(string str)
		{
			if (str == null)
			{
				writer.Write(-1);
			}
			else
			{
				writer.Write(str.Length);
				writer.Write(str);
			}
		}

		public void Write(float f)
		{
			writer.Write(f);
		}

		public void Write(ulong ul)
		{
			writer.Write(ul);
		}

		public void Write(long l)
		{
			writer.Write(l);
		}

		public void Write(uint ui)
		{
			writer.Write(ui);
		}

		public void Write(int i)
		{
			writer.Write(i);
		}

		public void Write(ushort us)
		{
			writer.Write(us);
		}

		public void Write(short s)
		{
			writer.Write(s);
		}

		public void Write(decimal de)
		{
			writer.Write(de);
		}

		public void Write(double d)
		{
			writer.Write(d);
		}

		public void Write(char ch)
		{
			writer.Write(ch);
		}

		public void Write(sbyte sb)
		{
			writer.Write(sb);
		}

		public void Write(byte b)
		{
			writer.Write(b);
		}

		public void Write(bool b)
		{
			writer.Write(b);
		}

		public void Write(DateTime dt)
		{
			writer.Write(dt.Ticks);
		}
		#endregion
		
		#region Read primitive
		public string ReadString()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			return reader.ReadString();
		}

		public float ReadSingle()
		{
			return reader.ReadSingle();
		}

		public ulong ReadUInt64()
		{
			return reader.ReadUInt64();
		}

		public long ReadInt64()
		{
			return reader.ReadInt64();
		}

		public uint ReadUInt32()
		{
			return reader.ReadUInt32();
		}

		public int ReadInt32()
		{
			return reader.ReadInt32();
		}

		public ushort ReadUInt16()
		{
			return reader.ReadUInt16();
		}

		public short ReadInt16()
		{
			return reader.ReadInt16();
		}

		public decimal ReadDecimal()
		{
			return reader.ReadDecimal();
		}

		public double ReadDouble()
		{
			return reader.ReadDouble();
		}

		public char ReadChar()
		{
			return reader.ReadChar();
		}

		public sbyte ReadSByte()
		{
			return reader.ReadSByte();
		}

		public byte ReadByte()
		{
			return reader.ReadByte();
		}

		public bool ReadBoolean()
		{
			return reader.ReadBoolean();
		}

		public DateTime ReadDateTime()
		{
			return new DateTime(reader.ReadInt64());
		}
		#endregion

		#region Write Primitive Array
		public void Write(string[] array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				var length = array.Length;
				writer.Write(length);
				for (var i = 0; i < length; i++)
				{
					writer.Write(array[i]);	
				}
			}
		}

		public void Write(float[] array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				var length = array.Length;
				writer.Write(length);
				for (var i = 0; i < length; i++)
				{
					writer.Write(array[i]);
				}
			}
		}

		public void Write(ulong[] array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				var length = array.Length;
				writer.Write(length);
				for (var i = 0; i < length; i++)
				{
					writer.Write(array[i]);
				}
			}
		}

		public void Write(long[] array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				var length = array.Length;
				writer.Write(length);
				for (var i = 0; i < length; i++)
				{
					writer.Write(array[i]);
				}
			}
		}

		public void Write(uint[] array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				var length = array.Length;
				writer.Write(length);
				for (var i = 0; i < length; i++)
				{
					writer.Write(array[i]);
				}
			}
		}

		public void Write(int[] array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				var length = array.Length;
				writer.Write(length);
				for (var i = 0; i < length; i++)
				{
					writer.Write(array[i]);
				}
			}
		}

		public void Write(ushort[] array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				var length = array.Length;
				writer.Write(length);
				for (var i = 0; i < length; i++)
				{
					writer.Write(array[i]);
				}
			}
		}

		public void Write(short[] array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				var length = array.Length;
				writer.Write(length);
				for (var i = 0; i < length; i++)
				{
					writer.Write(array[i]);
				}
			}
		}

		public void Write(decimal[] array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				var length = array.Length;
				writer.Write(length);
				for (var i = 0; i < length; i++)
				{
					writer.Write(array[i]);
				}
			}
		}

		public void Write(double[] array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				var length = array.Length;
				writer.Write(length);
				for (var i = 0; i < length; i++)
				{
					writer.Write(array[i]);
				}
			}
		}

		public void Write(char[] array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				writer.Write(array);
			}
		}

		public void Write(sbyte[] array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				var length = array.Length;
				writer.Write(length);
				for (var i = 0; i < length; i++)
				{
					writer.Write(array[i]);
				}
			}
		}

		public void Write(byte[] array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				writer.Write(array);
			}
		}

		public void Write(bool[] array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				var length = array.Length;
				writer.Write(length);
				for (var i = 0; i < length; i++)
				{
					writer.Write(array[i]);
				}
			}
		}

		public void Write(DateTime[] array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				var length = array.Length;
				writer.Write(length);
				for (var i = 0; i < length; i++)
				{
					writer.Write(array[i].Ticks);
				}
			}
		}
		#endregion

		#region Read Primitive Array
		public string[] ReadArrayString()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			var result = new string[len];
			for (int i = 0; i < len; i++)
			{
				result[i] = reader.ReadString();
			}
			return result;
		}

		public float[] ReadArraySingle()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			var result = new float[len];
			for (int i = 0; i < len; i++)
			{
				result[i] = reader.ReadSingle();
			}
			return result;
		}

		public ulong[] ReadArrayUInt64()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			var result = new ulong[len];
			for (int i = 0; i < len; i++)
			{
				result[i] = reader.ReadUInt64();
			}
			return result;
		}

		public long[] ReadArrayInt64()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			var result = new long[len];
			for (int i = 0; i < len; i++)
			{
				result[i] = reader.ReadInt64();
			}
			return result;
		}

		public uint[] ReadArrayUInt32()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			var result = new uint[len];
			for (int i = 0; i < len; i++)
			{
				result[i] = reader.ReadUInt32();
			}
			return result;
		}

		public int[] ReadArrayInt32()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			var result = new int[len];
			for (int i = 0; i < len; i++)
			{
				result[i] = reader.ReadInt32();
			}
			return result;
		}

		public ushort[] ReadArrayUInt16()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			var result = new ushort[len];
			for (int i = 0; i < len; i++)
			{
				result[i] = reader.ReadUInt16();
			}
			return result;
		}

		public short[] ReadArrayInt16()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			var result = new short[len];
			for (int i = 0; i < len; i++)
			{
				result[i] = reader.ReadInt16();
			}
			return result;
		}

		public decimal[] ReadArrayDecimal()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			var result = new decimal[len];
			for (int i = 0; i < len; i++)
			{
				result[i] = reader.ReadDecimal();
			}
			return result;
		}

		public double[] ReadArrayDouble()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			var result = new double[len];
			for (int i = 0; i < len; i++)
			{
				result[i] = reader.ReadDouble();
			}
			return result;
		}

		public char[] ReadArrayChar()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			return reader.ReadChars(len);
		}

		public sbyte[] ReadArraySByte()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			var result = new sbyte[len];
			for (int i = 0; i < len; i++)
			{
				result[i] = reader.ReadSByte();
			}
			return result;
		}

		public byte[] ReadArrayByte()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			return reader.ReadBytes(len);
		}

		public bool[] ReadArrayBoolean()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			var result = new bool[len];
			for (int i = 0; i < len; i++)
			{
				result[i] = reader.ReadBoolean();
			}
			return result;
		}

		public DateTime[] ReadArrayDateTime()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			var result = new DateTime[len];
			for (int i = 0; i < len; i++)
			{
				result[i] = new DateTime(reader.ReadInt64());
			}
			return result;
		}
		#endregion

		#region Write Primitive List
		public void Write(IList<string> array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				var length = array.Count;
				writer.Write(length);
				for (var i = 0; i < length; i++)
				{
					Write(array[i]);
				}
			}
		}

		public void Write(IList<float> array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				var length = array.Count;
				writer.Write(length);
				for (var i = 0; i < length; i++)
				{
					writer.Write(array[i]);
				}
			}
		}

		public void Write(IList<ulong> array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				var length = array.Count;
				writer.Write(length);
				for (var i = 0; i < length; i++)
				{
					writer.Write(array[i]);
				}
			}
		}

		public void Write(IList<long> array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				var length = array.Count;
				writer.Write(length);
				for (var i = 0; i < length; i++)
				{
					writer.Write(array[i]);
				}
			}
		}

		public void Write(IList<uint> array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				var length = array.Count;
				writer.Write(length);
				for (var i = 0; i < length; i++)
				{
					writer.Write(array[i]);
				}
			}
		}

		public void Write(IList<int> array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				var length = array.Count;
				writer.Write(length);
				for (var i = 0; i < length; i++)
				{
					writer.Write(array[i]);
				}
			}
		}

		public void Write(IList<ushort> array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				var length = array.Count;
				writer.Write(length);
				for (var i = 0; i < length; i++)
				{
					writer.Write(array[i]);
				}
			}
		}

		public void Write(IList<short> array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				var length = array.Count;
				writer.Write(length);
				for (var i = 0; i < length; i++)
				{
					writer.Write(array[i]);
				}
			}
		}

		public void Write(IList<decimal> array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				var length = array.Count;
				writer.Write(length);
				for (var i = 0; i < length; i++)
				{
					writer.Write(array[i]);
				}
			}
		}

		public void Write(IList<double> array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				var length = array.Count;
				writer.Write(length);
				for (var i = 0; i < length; i++)
				{
					writer.Write(array[i]);
				}
			}
		}

		public void Write(IList<char> array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				var length = array.Count;
				writer.Write(length);
				for (var i = 0; i < length; i++)
				{
					writer.Write(array[i]);
				}
			}
		}

		public void Write(IList<sbyte> array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				var length = array.Count;
				writer.Write(length);
				for (var i = 0; i < length; i++)
				{
					writer.Write(array[i]);
				}
			}
		}

		public void Write(IList<byte> array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				var length = array.Count;
				writer.Write(length);
				for (var i = 0; i < length; i++)
				{
					writer.Write(array[i]);
				}
			}
		}

		public void Write(IList<bool> array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				var length = array.Count;
				writer.Write(length);
				for (var i = 0; i < length; i++)
				{
					writer.Write(array[i]);
				}
			}
		}

		public void Write(IList<DateTime> array)
		{
			if (array == null)
			{
				writer.Write(-1);
			}
			else
			{
				var length = array.Count;
				writer.Write(length);
				for (var i = 0; i < length; i++)
				{
					writer.Write(array[i].Ticks);
				}
			}
		}
		#endregion

		#region Read Primitive Array
		public IList<string> ReadListString()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			var result = new List<string>(len);
			for (int i = 0; i < len; i++)
			{
				result.Add(ReadString());
			}
			return result;
		}

		public IList<float> ReadListSingle()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			var result = new List<float>(len);
			for (int i = 0; i < len; i++)
			{
				result.Add(reader.ReadSingle());
			}
			return result;
		}

		public IList<ulong> ReadListUInt64()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			var result = new List<ulong>(len);
			for (int i = 0; i < len; i++)
			{
				result.Add(reader.ReadUInt64());
			}
			return result;
		}

		public IList<long> ReadListInt64()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			var result = new List<long>(len);
			for (int i = 0; i < len; i++)
			{
				result.Add(reader.ReadInt64());
			}
			return result;
		}

		public IList<uint> ReadListUInt32()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			var result = new List<uint>(len);
			for (int i = 0; i < len; i++)
			{
				result.Add(reader.ReadUInt32());
			}
			return result;
		}

		public IList<int> ReadListInt32()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			var result = new List<int>(len);
			for (int i = 0; i < len; i++)
			{
				result.Add(reader.ReadInt32());
			}
			return result;
		}

		public IList<ushort> ReadListUInt16()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			var result = new List<ushort>(len);
			for (int i = 0; i < len; i++)
			{
				result.Add(reader.ReadUInt16());
			}
			return result;
		}

		public IList<short> ReadListInt16()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			var result = new List<short>(len);
			for (int i = 0; i < len; i++)
			{
				result.Add(reader.ReadInt16());
			}
			return result;
		}

		public IList<decimal> ReadListDecimal()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			var result = new List<decimal>(len);
			for (int i = 0; i < len; i++)
			{
				result.Add(reader.ReadDecimal());
			}
			return result;
		}

		public IList<double> ReadListDouble()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			var result = new List<double>(len);
			for (int i = 0; i < len; i++)
			{
				result.Add(reader.ReadDouble());
			}
			return result;
		}

		public IList<char> ReadListChar()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			var result = new List<char>(len);
			for (int i = 0; i < len; i++)
			{
				result.Add(reader.ReadChar());
			}
			return result;
		}

		public IList<sbyte> ReadListSByte()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			var result = new List<sbyte>(len);
			for (int i = 0; i < len; i++)
			{
				result.Add(reader.ReadSByte());
			}
			return result;
		}

		public IList<byte> ReadListByte()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			var result = new List<byte>(len);
			for (int i = 0; i < len; i++)
			{
				result.Add(reader.ReadByte());
			}
			return result;
		}

		public IList<bool> ReadListBoolean()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			var result = new List<bool>(len);
			for (int i = 0; i < len; i++)
			{
				result.Add(reader.ReadBoolean());
			}
			return result;
		}

		public IList<DateTime> ReadListDateTime()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			var result = new List<DateTime>(len);
			for (int i = 0; i < len; i++)
			{
				result.Add(new DateTime(reader.ReadInt64()));
			}
			return result;
		}
		#endregion

		#region Dictionary
		public void WriteDictionary<K, V>(IDictionary<K, V> dictionary)
		{
			if (dictionary == null)
			{
				writer.Write(-1);
				return;
			}
			foreach (var pair in dictionary)
			{
				var key = pair.Key;
				var value = pair.Value;
				WritePrimitiveObject(key);
				WritePrimitiveObject(value);
			}
		}

		public IDictionary<K, V> ReadDictionary<K, V>()
		{
			var len = reader.ReadInt32();
			if (len == -1)
			{
				return null;
			}
			var result = new Dictionary<K, V>(len);
			for (var i = 0; i < len; i++)
			{
				result.Add(ReadPrimitiveObject<K>(), ReadPrimitiveObject<V>());
			}
			return result;
		}

		private T ReadPrimitiveObject<T>()
		{
			var type = typeof (T);
			if (type == typeof(bool))
			{
				return (T) (object) reader.ReadBoolean();
			}
			if (type == typeof(byte))
			{
				return (T)(object)reader.ReadByte();	
			}
			if (type == typeof(sbyte))
			{
				return (T)(object)reader.ReadSByte();
			}
			if (type == typeof(char))
			{
				return (T)(object)reader.ReadChar();
			}
			if (type == typeof(short))
			{
				return (T)(object)reader.ReadInt16();
			}
			if (type == typeof(ushort))
			{
				return (T)(object)reader.ReadUInt16();
			}
			if (type == typeof(int))
			{
				return (T)(object)reader.ReadInt32();
			}
			if (type == typeof(uint))
			{
				return (T)(object)reader.ReadUInt32();
			}
			if (type == typeof(float))
			{
				return (T)(object)reader.ReadSingle();
			}
			if (type == typeof(long))
			{
				return (T)(object)reader.ReadInt64();
			}
			if (type == typeof(ulong))
			{
				return (T)(object)reader.ReadUInt64();
			}
			if (type == typeof(double))
			{
				return (T)(object)reader.ReadDouble();
			}
			if (type == typeof(decimal))
			{
				return (T)(object)reader.ReadDecimal();
			}
			if (type == typeof(string))
			{
				return (T) (object) ReadString();
			}
			if (type == typeof(DateTime))
			{
				return (T)(object) ReadDateTime();
			}
			return default(T);
		}

		private void WritePrimitiveObject(object o)
		{
			var type = o.GetType();
			if (type == typeof(bool))
			{
				writer.Write((bool) o);
				return;
			}
			if (type == typeof(byte))
			{
				writer.Write((byte)o);
				return;
			}
			if (type == typeof(sbyte))
			{
				writer.Write((sbyte)o);
				return;
			}
			if (type == typeof(char))
			{
				writer.Write((char)o);
				return;
			}
			if (type == typeof(short))
			{
				writer.Write((short)o);
				return;
			}
			if (type == typeof(ushort))
			{
				writer.Write((ushort)o);
				return;
			}
			if (type == typeof(int))
			{
				writer.Write((int)o);
				return;
			}
			if (type == typeof(uint))
			{
				writer.Write((uint)o);
				return;
			}
			if (type == typeof(float))
			{
				writer.Write((float)o);
				return;
			}
			if (type == typeof(long))
			{
				writer.Write((long)o);
				return;
			}
			if (type == typeof(ulong))
			{
				writer.Write((ulong)o);
				return;
			}
			if (type == typeof(double))
			{
				writer.Write((double)o);
				return;
			}
			if (type == typeof(decimal))
			{
				writer.Write((decimal)o);
				return;
			}
			if (type == typeof(string))
			{
				Write((string)o);
				return;
			}
			if (type == typeof(DateTime))
			{
				Write((DateTime) o);
				return;
			}
			throw new ArgumentException("Type not supported: " + type);
		}

		#endregion

		public static byte[] ReadBlob(string columnName, DbDataReader reader)
		{
			using (var stream = new MemoryStream())
			{
				var buffer = new byte[1024];
				var read = reader.GetBytes(reader.GetOrdinal(columnName), 0, buffer, 0, buffer.Length);
				stream.Write(buffer, 0, (int) read);
				while (read == buffer.Length)
				{
					try
					{
						read = reader.GetBytes(reader.GetOrdinal(columnName), stream.Position, buffer, 0, buffer.Length);
						stream.Write(buffer, 0, (int)read);
					}
					catch(Exception e)
					{
						Logger.Log.WriteLog("Exception when read blob data: " + e);
					}
				}
				return stream.ToArray();
			}
		}

		public static TimeRelationInfo ReadRelationTimeInfo(string columnName, DbDataReader reader)
		{
			using (var stream = new MemoryStream())
			{
				var buffer = new byte[1024];
				var read = reader.GetBytes(reader.GetOrdinal(columnName), 0, buffer, 0, buffer.Length);
				stream.Write(buffer, 0, (int)read);
				while (read == buffer.Length)
				{
					try
					{
						read = reader.GetBytes(reader.GetOrdinal(columnName), stream.Position, buffer, 0, buffer.Length);
						stream.Write(buffer, 0, (int)read);
					}
					catch (Exception e)
					{
						Logger.Log.WriteLog("Exception when read blob data: " + e);
					}
				}
				if (stream.Length > 0)
				{
					stream.Position = 0;
					var dbSerilizerHelper = new DbSerializerHelper(new BinaryReader(stream));
					var result = new TimeRelationInfo { updateTime = dbSerilizerHelper.ReadInt64(), idList = dbSerilizerHelper.ReadListInt64() };
					return result;
				}
			}
			return null;
		}
	}
}
