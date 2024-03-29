// Copyright (C) 2004-2007 MySQL AB
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License version 2 as published by
// the Free Software Foundation
//
// There are special exceptions to the terms and conditions of the GPL 
// as it is applied to this software. View the full text of the 
// exception in file EXCEPTIONS in the directory of this software 
// distribution.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Data;
using System.IO;
using System.Globalization;
using System.Threading;
using NUnit.Framework;
using System.Text;

namespace MySql.Data.MySqlClient.Tests
{
	[TestFixture]
	public class CharacterSetTests : BaseTest
	{
		[TestFixtureSetUp]
		public override void FixtureSetup()
		{
            pooling = false;
            base.FixtureSetup();
		}

		[Test]
		public void UseFunctions()
		{
			execSQL("DROP TABLE IF EXISTS Test");
			execSQL("CREATE TABLE Test (valid char, UserCode varchar(100), password varchar(100)) CHARSET latin1");

			MySqlConnection c = new MySqlConnection(conn.ConnectionString + ";charset=latin1");
			c.Open();
			MySqlCommand cmd = new MySqlCommand("SELECT valid FROM Test WHERE Valid = 'Y' AND " +
				"UserCode = 'username' AND Password = AES_ENCRYPT('Password','abc')", c);
			cmd.ExecuteScalar();
			c.Close();
		}

        [Test]
        public void VarBinary()
        {
            if (Version < new Version(4, 1)) return;

            execSQL("DROP TABLE IF EXISTS test");
            createTable("CREATE TABLE test (id int, name varchar(200) collate utf8_bin) charset utf8", "InnoDB");
            execSQL("INSERT INTO test VALUES (1, 'Test1')");

            MySqlCommand cmd = new MySqlCommand("SELECT * FROM test", conn);
            MySqlDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();
                Assert.IsTrue(reader.Read());
                object o = reader.GetValue(1);
                Assert.IsTrue(o is string);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                if (reader != null) reader.Close();
            }
        }

		[Test]
		public void Latin1Connection() 
		{
            if (Version < new Version(4, 1)) return;

			execSQL("DROP TABLE IF EXISTS Test");
			execSQL("CREATE TABLE Test (id INT, name VARCHAR(200)) CHARSET latin1");
			execSQL("INSERT INTO Test VALUES( 1, _latin1 'Test')");

			MySqlConnection c = new MySqlConnection(conn.ConnectionString + ";charset=latin1");
			c.Open();

			MySqlCommand cmd = new MySqlCommand("SELECT id FROM Test WHERE name LIKE 'Test'", c);
			object id = cmd.ExecuteScalar();
			Assert.AreEqual(1, id);
			c.Close();
		}

        /// <summary>
        /// Bug #11621  	connector does not support charset cp1250
        /// </summary>
/*        [Test]
        public void CP1250Connection()
        {
            execSQL("DROP TABLE IF EXISTS Test");
            execSQL("CREATE TABLE Test (name VARCHAR(200)) CHARSET cp1250");

            MySqlConnection c = new MySqlConnection(conn.ConnectionString + ";charset=cp1250");
            c.Open();

            MySqlCommand cmd = new MySqlCommand("INSERT INTO Test VALUES('€ŤŽš')", c);
            cmd.ExecuteNonQuery();

            cmd.CommandText = "SELECT name FROM Test";
            object name = cmd.ExecuteScalar();
            Assert.AreEqual("€ŤŽš", name);
            c.Close();
        }
*/
        /// <summary>
        /// Bug #14592 Wrong column length returned for VARCHAR UTF8 columns 
        /// </summary>
        [Test]
        public void GetSchemaOnUTF8()
        {
            execSQL("DROP TABLE IF EXISTS test");
            execSQL("CREATE TABLE test(name VARCHAR(40) NOT NULL, name2 VARCHAR(20)) " +
                "CHARACTER SET utf8");
            execSQL("INSERT INTO test VALUES('Test', 'Test')");

            MySqlDataReader reader = null;

            try
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM test", conn);
                reader = cmd.ExecuteReader();
                DataTable dt = reader.GetSchemaTable();
                Assert.AreEqual(40, dt.Rows[0]["ColumnSize"]);
                Assert.AreEqual(20, dt.Rows[1]["ColumnSize"]);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                if (reader != null) reader.Close();
            }
        }

        [Test]
        public void UTF8BlogsTruncating()
        {
            execSQL("DROP TABLE IF EXISTS test");
            execSQL("CREATE TABLE test (name LONGTEXT) CHARSET utf8");

            string szParam = "test:éàçùêû";
            string szSQL = "INSERT INTO test Values (?monParametre)";

            try
            {
                string connStr = GetConnectionString(true) + ";charset=utf8";
                using (MySqlConnection c = new MySqlConnection(connStr))
                {
                    c.Open();
                    MySqlCommand cmd = new MySqlCommand(szSQL, c);
                    cmd.Parameters.Add(new MySqlParameter("?monParametre", MySqlDbType.VarChar));
                    cmd.Parameters[0].Value = szParam;
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "SELECT * FROM test";
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        string s = reader.GetString(0);
                        Assert.AreEqual(szParam, s);
                    }
                }                
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public void BlobAsUtf8()
        {
            execSQL("DROP TABLE IF EXISTS test");
            execSQL(@"CREATE TABLE test(include_blob BLOB, include_tinyblob TINYBLOB, 
                        include_longblob LONGBLOB, exclude_tinyblob TINYBLOB, exclude_blob BLOB, 
                        exclude_longblob LONGBLOB)");

            byte[] utf8_bytes = new byte[4] { 0xf0, 0x90, 0x80, 0x80 };
            Encoding utf8 = Encoding.GetEncoding("UTF-8");
            string utf8_string = utf8.GetString(utf8_bytes, 0, utf8_bytes.Length);

            // insert our UTF-8 bytes into the table
            MySqlCommand cmd = new MySqlCommand("INSERT INTO test VALUES (?p1, ?p2, ?p3, ?p4, ?p5, ?p5)", conn);
            cmd.Parameters.AddWithValue("?p1", utf8_bytes);
            cmd.Parameters.AddWithValue("?p2", utf8_bytes);
            cmd.Parameters.AddWithValue("?p3", utf8_bytes);
            cmd.Parameters.AddWithValue("?p4", utf8_bytes);
            cmd.Parameters.AddWithValue("?p5", utf8_bytes);
            cmd.Parameters.AddWithValue("?p6", utf8_bytes);
            cmd.ExecuteNonQuery();

            // now check that the on/off is working
            string connStr = GetConnectionString(true) + ";Treat Blobs As UTF8=yes;BlobAsUTF8IncludePattern=.*";
            using (MySqlConnection c = new MySqlConnection(connStr))
            {
                c.Open();
                MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM test", c);
                DataTable dt = new DataTable();
                da.Fill(dt);
                foreach (DataColumn col in dt.Columns)
                {
                    Assert.AreEqual(typeof(string), col.DataType);
                    Assert.AreEqual(utf8_string, dt.Rows[0][col.Ordinal].ToString());
                }
            }

            // now check that exclusion works
            connStr = GetConnectionString(true) + ";Treat Blobs As UTF8=yes;BlobAsUTF8ExcludePattern=exclude.*";
            using (MySqlConnection c = new MySqlConnection(connStr))
            {
                c.Open();
                MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM test", c);
                DataTable dt = new DataTable();
                da.Fill(dt);
                foreach (DataColumn col in dt.Columns)
                {
                    if (col.ColumnName.StartsWith("exclude"))
                        Assert.AreEqual(typeof(byte[]), col.DataType);
                    else
                    {
                        Assert.AreEqual(typeof(string), col.DataType);
                        Assert.AreEqual(utf8_string, dt.Rows[0][col.Ordinal].ToString());
                    }
                }
            }

            // now check that inclusion works
            connStr = GetConnectionString(true) + ";Treat Blobs As UTF8=yes;BlobAsUTF8IncludePattern=include.*";
            using (MySqlConnection c = new MySqlConnection(connStr))
            {
                c.Open();
                MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM test", c);
                DataTable dt = new DataTable();
                da.Fill(dt);
                foreach (DataColumn col in dt.Columns)
                {
                    if (col.ColumnName.StartsWith("include"))
                    {
                        Assert.AreEqual(typeof(string), col.DataType);
                        Assert.AreEqual(utf8_string, dt.Rows[0][col.Ordinal].ToString());
                    }
                    else
                        Assert.AreEqual(typeof(byte[]), col.DataType);
                }
            }
        }

		/// <summary>
		/// Bug #31185  	columns names are incorrect when using the 'AS' clause and name with accents
        /// Bug #38721  	GetOrdinal doesn't accept column names accepted by MySQL 5.0
		/// </summary>
		[Test]
		public void UTF8AsColumnNames()
		{
			string connStr = GetConnectionString(true) + ";charset=utf8;pooling=false";
			using (MySqlConnection c = new MySqlConnection(connStr))
			{
				c.Open();

				MySqlDataAdapter da = new MySqlDataAdapter("select now() as 'Numéro'", c);
				DataTable dt = new DataTable();
				da.Fill(dt);

				Assert.AreEqual("Numéro", dt.Columns[0].ColumnName);

                MySqlCommand cmd = new MySqlCommand("SELECT NOW() AS 'Numéro'", c);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    int ord = reader.GetOrdinal("Numéro");
                    Assert.AreEqual(0, ord);
                }
			}
		}

		/// <summary>
		/// Bug #31117  	Connector/Net exceptions do not support server charset
		/// </summary>
		[Test]
		public void NonLatin1Exception()
		{
			string connStr = GetConnectionString(true) + ";charset=utf8";

            execSQL("DROP TABLE IF EXISTS Test");
            execSQL("CREATE TABLE Test (id int)");

			using (MySqlConnection c = new MySqlConnection(connStr))
			{
				c.Open();

				try
				{
					MySqlCommand cmd = new MySqlCommand("select `Numéro` from Test", c);
					cmd.ExecuteScalar();
				}
				catch (Exception ex)
				{
					Assert.AreEqual("Unknown column 'Numéro' in 'field list'", ex.Message);
				}
			}
		}

        /// <summary>
        /// Bug #40076	"Functions Return String" option does not set the proper encoding for the string
        /// </summary>
        [Test]
        public void FunctionReturnsStringWithCharSet()
        {
            string connStr = GetConnectionString(true) + ";functions return string=true";
            using (MySqlConnection c = new MySqlConnection(connStr))
            {
                c.Open();

                MySqlCommand cmd = new MySqlCommand(
                    "SELECT CONCAT('Trädgårdsvägen', 1)", c);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    Assert.AreEqual("Trädgårdsvägen1", reader.GetString(0));
                }
            }
        }

        [Test]
        public void RespectBinaryFlags()
        {
            string connStr = GetConnectionString(true) + ";respect binary flags=false";
            using (MySqlConnection c = new MySqlConnection(connStr))
            {
                c.Open();

                MySqlDataAdapter da = new MySqlDataAdapter(
                    "SELECT CONCAT('Trädgårdsvägen', 1)", c);
                DataTable dt = new DataTable();
                da.Fill(dt);
                Assert.AreEqual("Trädgårdsvägen1", dt.Rows[0][0]);
            }
        }
    }
}
