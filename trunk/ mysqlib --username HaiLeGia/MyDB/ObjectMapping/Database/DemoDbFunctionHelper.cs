using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using Persistents;

namespace ObjectMapping.Database
{
	public class DemoDbFunctionHelper : IDbFunctionHelper
	{
		#region Implementation of IDbFunctionHelper

		private DbObjectContainer dbObjectContainer;
		public DbObjectContainer DbObjectContainer
		{
			get { return dbObjectContainer; }
			set { dbObjectContainer = value; }
		}

		public string GetUpdateString(object o)
		{
			var type = o.GetType();
			if (type == typeof(UserData))
			{
				var u = (UserData) o;
				var builder = new StringBuilder();
				byte[] strArrayBytes;
				using (var stream = new MemoryStream())
				{
					var dbSerializerHelper = new DbSerializerHelper(new BinaryWriter(stream));
					dbSerializerHelper.Write(u.StrArray);
					strArrayBytes = stream.ToArray();
				}
				builder.AppendFormat("UPDATE UserData SET Username = {0}, Password = {1}, StrArray = {2} WHERE Id = {3};", u.Username, u.Password, strArrayBytes, u.Id);
				if (u.Other != null)
				{
					builder.Append(GetUpdateString(u.Other));
				}
				return builder.ToString();
			}
			return null;
		}

		public string GetInsertString(object o)
		{
			throw new System.NotImplementedException();
		}

		public object ReadObject(string typeName, DbDataReader reader, string[] propertyNames)
		{
			if (typeName == "UserData")
			{
				var result = new UserData();
				result.Id = reader.GetInt64(reader.GetOrdinal("Id"));
				for (var i = 0; i < propertyNames.Length; i++)
				{
					string propertyName = propertyNames[i];
					switch (propertyName)
					{
						case "Username":
							result.Username = reader.GetString(reader.GetOrdinal("Username"));
							break;
						case "Password":
							result.Password = reader.GetString(reader.GetOrdinal("Password"));
							break;
						case "StrArray":
							using (var stream = new MemoryStream())
							{
								var buffer = new byte[1024];
								var read = reader.GetBytes(reader.GetOrdinal("StrArray"), 0, buffer, 0, buffer.Length);
								while (read > 0)
								{
									stream.Write(buffer, 0, (int)read);
									read = reader.GetBytes(reader.GetOrdinal("StrArray"), 0, buffer, 0, buffer.Length);
								}
								stream.Position = 0;
								var dbSerializeHelper = new DbSerializerHelper(new BinaryReader(stream));
								result.StrArray = dbSerializeHelper.ReadArrayString();
							}
							break;
						case "Other":
							var classMetadata = ClassMetaDataManager.Instace.GetClassMetaData(result.GetType());
							var relationInfo = classMetadata.RelationProperties["Other"];
							result.Other = dbObjectContainer.QueryExecutor.SelectByForeignKey<UserData>(relationInfo.PartnerKey, result.Id, null,
							                                                             SelectQuery.ALL_PROPS);
							break;
					}
				}
			}
			return null;
		}

		#endregion
	}
}
