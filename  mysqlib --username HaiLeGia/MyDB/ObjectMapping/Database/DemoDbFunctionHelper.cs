using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Text;
using MySql.Data.MySqlClient;

namespace ObjectMapping.Database
{
//	public class DemoDbFunctionHelper : IDbFunctionHelper
//	{
//		#region Implementation of IDbFunctionHelper
//
//		private DbObjectContainer dbObjectContainer;
//		public DbObjectContainer DbObjectContainer
//		{
//			get { return dbObjectContainer; }
//			set { dbObjectContainer = value; }
//		}
//
//		public int Update(IDbObject o, DbConnection connection, IDictionary<IDbObject, long> objectGraph)
//		{
//			int result = 0;
//			var command = connection.CreateCommand();
//			command.CommandText =
//				"UPDATE UserData SET Username = @username, Password = @password, StrArray = @strArray WHERE Id = @id";
//			var type = o.GetType();
//			if (type == typeof(UserData))
//			{
//				var u = (UserData) o;
//				byte[] strArrayBytes;
//                using (var stream = new MemoryStream())
//				{
//					var dbSerializerHelper = new DbSerializerHelper(new BinaryWriter(stream));
//					dbSerializerHelper.Write(u.StrArray);
//					strArrayBytes = stream.ToArray();
//				}
//				command.Parameters.Add(new MySqlParameter("@username", u.Username));
//				command.Parameters.Add(new MySqlParameter("@password", u.Password));
//				command.Parameters.Add(new MySqlParameter("@strArray", strArrayBytes));
//				command.Parameters.Add(new MySqlParameter("@id", u.Id));
//				result = command.ExecuteNonQuery();
//				if (u.Other != null)
//				{
//					result += Update(u.Other, connection, objectGraph);
//				}
//			}
//            return result;
//		}
//
//		public long Insert(IDbObject o, DbConnection connection, IDictionary<IDbObject, long> objectGraph)
//		{
//		    int result = 0;
//		    var command = connection.CreateCommand();
//		    command.CommandText = "INSERT INTO userdata (id, userName , password, strArray) VALUES (@id, @userName, @password, @strArray)";
//            
//            if (o.GetType() == typeof(UserData))
//            {
//                UserData u = (UserData) o;
//                byte[] strArrayBytes;
//                using(var stream = new MemoryStream())
//                {
//                    var dbSerializerHelper = new DbSerializerHelper(new BinaryWriter(stream));
//                    dbSerializerHelper.Write(u.StrArray);
//                    strArrayBytes = stream.ToArray();
//                }
//                command.Parameters.Add(new MySqlParameter("@id", u.Id));
//                command.Parameters.Add(new MySqlParameter("@userName", u.Username));
//                command.Parameters.Add(new MySqlParameter("@password", u.Password));
//                command.Parameters.Add(new MySqlParameter("@strArray", strArrayBytes));
//                result = command.ExecuteNonQuery();
//                if (u.Other != null)
//                {
//                    result += Update(u.Other, connection, objectGraph);
//                }
//            }
//            return result;
//
//		}
//
//		public object ReadObject(Type type, DbDataReader reader, IList<string> propertyNames, IDictionary<string, IDbObject> objectGraph)
//		{
//			if (type == typeof(UserData))
//			{
//				var result = new UserData();
//				result.Id = reader.GetInt64(reader.GetOrdinal("Id"));
//				for (var i = 0; i < propertyNames.Count; i++)
//				{
//					string propertyName = propertyNames[i];
//					switch (propertyName)
//					{
//						case "Username":
//							result.Username = reader.GetString(reader.GetOrdinal("Username"));
//							break;
//						case "Password":
//							result.Password = reader.GetString(reader.GetOrdinal("Password"));
//							break;
//						case "StrArray":
//							using (var stream = new MemoryStream())
//							{
//								var buffer = new byte[1024];
//								var read = reader.GetBytes(reader.GetOrdinal("StrArray"), 0, buffer, 0, buffer.Length);
//                                stream.Write(buffer, 0, (int)read);
//                                while (read == buffer.Length)
//								{
//                                    try
//                                    {
//                                        read = reader.GetBytes(reader.GetOrdinal("StrArray"), stream.Position, buffer, 0, buffer.Length);
//                                        stream.Write(buffer, 0, (int)read);    
//                                    }
//									catch(Exception e)
//									{
//									    break;
//									}
//                                }
//								stream.Position = 0;
//								var dbSerializeHelper = new DbSerializerHelper(new BinaryReader(stream));
//								result.StrArray = dbSerializeHelper.ReadArrayString();
//							}
//							break;
//						case "Other":
//							var classMetadata = ClassMetaDataManager.Instace.GetClassMetaData(result.GetType());
//							var relationInfo = classMetadata.RelationProperties["Other"];
//							result.Other = dbObjectContainer.QueryExecutor.SelectByForeignKey<UserData>(relationInfo.PartnerKey, result.Id, null,
//							                                                             SelectQuery.ALL_PROPS);
//							break;
//					}
//				}
//			    return result;
//			}
//			return null;
//		}
//
//		public int Insert(IDbObject @object, DbConnection connection, long referenceId, string referenceColumn)
//		{
//			return 0;
//		}
//
//		#endregion
//	}
}
