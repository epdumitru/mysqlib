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
using System.Threading;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace MySql.Data.MySqlClient.Tests
{
	/// <summary>
	/// Summary description for PoolingTests.
	/// </summary>
	[TestFixture]
	public class PoolingTests : BaseTest
	{
		[TestFixtureSetUp]
		public override void FixtureSetup()
		{
			csAdditions = ";pooling=true; connection reset=true;";
			base.FixtureSetup();
		}

		[Test]
		public void Connection()
		{
			string connStr = conn.ConnectionString + ";pooling=true";

			MySqlConnection c = new MySqlConnection(connStr);
			c.Open();
			int serverThread = c.ServerThread;
			c.Close();

			// first test that only a single connection get's used
			for (int i = 0; i < 10; i++)
			{
				c = new MySqlConnection(connStr);
				c.Open();
				Assert.AreEqual(serverThread, c.ServerThread);
				c.Close();
			}

			c.Open();
			KillConnection(c);
			c.Close();

			connStr += ";Min Pool Size=10";
			MySqlConnection[] connArray = new MySqlConnection[10];
			for (int i = 0; i < connArray.Length; i++)
			{
				connArray[i] = new MySqlConnection(connStr);
				connArray[i].Open();
			}

			// now make sure all the server ids are different
			for (int i = 0; i < connArray.Length; i++)
			{
				for (int j = 0; j < connArray.Length; j++)
				{
					if (i != j)
						Assert.IsTrue(connArray[i].ServerThread != connArray[j].ServerThread);
				}
			}

			for (int i = 0; i < connArray.Length; i++)
			{
				KillConnection(connArray[i]);
				connArray[i].Close();
			}
		}

		[Test]
		public void OpenKilled()
		{
			try
			{
				string connStr = conn.ConnectionString + ";pooling=true;min pool size=1; max pool size=1";
				MySqlConnection c = new MySqlConnection(connStr);
				c.Open();
				int threadId = c.ServerThread;
				// thread gets killed right here
				KillConnection(c);
				c.Close();

				c.Dispose();

				c = new MySqlConnection(connStr);
				c.Open();
				int secondThreadId = c.ServerThread;
				KillConnection(c);
				c.Close();
				Assert.IsFalse(threadId == secondThreadId);
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

		[Test]
		public void ReclaimBrokenConnection()
		{
			// now create a new connection string only allowing 1 connection in the pool
			string connStr = conn.ConnectionString + ";max pool size=1";

			// now use up that connection
			MySqlConnection c = new MySqlConnection(connStr);
			c.Open();

			// now attempting to open a connection should fail
			try
			{
				MySqlConnection c2 = new MySqlConnection(connStr);
				c2.Open();
				Assert.Fail("Open after using up pool should fail");
			}
			catch (Exception) { }

			// we now kill the first connection to simulate a server stoppage
			base.KillConnection(c);

			// now we do something on the first connection
			try
			{
				c.ChangeDatabase("mysql");
				Assert.Fail("This change database should not work");
			}
			catch (Exception) { }

			// Opening a connection now should work
			try
			{
				MySqlConnection c2 = new MySqlConnection(connStr);
				c2.Open();
				c2.Close();
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

		[Test]
		public void TestUserReset()
		{
			execSQL("SET @testvar='5'");
			MySqlCommand cmd = new MySqlCommand("SELECT @testvar", conn);
			object var = cmd.ExecuteScalar();
			Assert.AreEqual("5", var);
			conn.Close();

			conn.Open();
			object var2 = cmd.ExecuteScalar();
			Assert.AreEqual(DBNull.Value, var2);
		}

		/// <summary>
		/// Bug #25614 After connection is closed, and opened again UTF-8 characters are not read well 
		/// </summary>
		[Test]
		public void UTF8AfterClosing()
		{
			string originalValue = "šđčćžŠĐČĆŽ";

			execSQL("DROP TABLE IF EXISTS test");
			execSQL("CREATE TABLE test (id int(11) NOT NULL, " +
				"value varchar(100) NOT NULL, PRIMARY KEY  (`id`) " +
				") ENGINE=MyISAM DEFAULT CHARSET=utf8");

			string connStr = GetConnectionString(true) + ";charset=utf8";
			MySqlConnection con = new MySqlConnection(connStr);
			con.Open();

			try
			{
				MySqlCommand cmd = new MySqlCommand("INSERT INTO test VALUES (1, 'šđčćžŠĐČĆŽ')", con);
				cmd.ExecuteNonQuery();

				cmd = new MySqlCommand("SELECT value FROM test WHERE id = 1", con);
				string firstS = cmd.ExecuteScalar().ToString();
				Assert.AreEqual(originalValue, firstS);

				con.Close();
				con.Open();

				//Does not work:
				cmd = new MySqlCommand("SELECT value FROM test WHERE id = 1", con);
				string secondS = cmd.ExecuteScalar().ToString();

				con.Close();
				Assert.AreEqual(firstS, secondS);
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

#if !CF

		private void PoolingWorker(object cn)
		{
			MySqlConnection conn = (cn as MySqlConnection);

			Thread.Sleep(5000);
			conn.Close();
		}

		/// <summary>
		/// Bug #24373 High CPU utilization when no idle connection 
		/// </summary>
		[Test]
		public void MultipleThreads()
		{
			string connStr = GetConnectionString(true) + ";max pool size=1";
			MySqlConnection c = new MySqlConnection(connStr);
			c.Open();

			ParameterizedThreadStart ts = new ParameterizedThreadStart(PoolingWorker);
			Thread t = new Thread(ts);
			t.Start(c);

			try
			{
				MySqlConnection c2 = new MySqlConnection(connStr);
				c2.Open();
				c2.Close();
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

#endif


		[Test]
		public void NewTest()
		{
			if (version < new Version(5, 0)) return;

			execSQL("DROP TABLE IF EXISTS Test");
			execSQL("DROP PROCEDURE IF EXISTS spTest");
			execSQL("CREATE TABLE Test (id INT, name VARCHAR(50))");
			execSQL("CREATE PROCEDURE spTest(theid INT) BEGIN SELECT * FROM test WHERE id=theid; END");
			execSQL("INSERT INTO test VALUES (1, 'First')");
			execSQL("INSERT INTO test VALUES (2, 'Second')");
			execSQL("INSERT INTO test VALUES (3, 'Third')");
			execSQL("INSERT INTO test VALUES (4, 'Fourth')");

            string connStr = GetConnectionString(true) + ";logging=true";

			for (int i = 1; i < 5; i++)
			{
				using (MySqlConnection con = new MySqlConnection(connStr))
				{
					try
					{
						con.Open();
						MySqlCommand reccmd = new MySqlCommand("spTest", con);
                        reccmd.CommandTimeout = 0;
						reccmd.CommandType = CommandType.StoredProcedure;
						MySqlParameter par = new MySqlParameter("@theid", MySqlDbType.String);
						par.Value = i;
						reccmd.Parameters.Add(par);
						using (MySqlDataReader recdr = reccmd.ExecuteReader())
						{
							if (recdr.Read())
							{
								int x = recdr.GetOrdinal("name");
								Assert.AreEqual(1, x);
							}
						}
					}
					catch (Exception ex)
					{
						Assert.Fail(ex.Message);
					}
				}
			}
		}

		/// <summary>
		/// Bug #29409  	Bug on Open Connection with pooling=true to a MYSQL Server that is shutdown
		/// </summary>
		[Test]
		public void ConnectAfterMaxPoolSizeTimeouts()
		{
			//TODO: refactor test suite to support starting/stopping services
			/*            string connStr = "server=localhost;uid=root;database=test;pooling=true;connect timeout=6; max pool size = 6";
						MySqlConnection c = new MySqlConnection(connStr);
						for (int i = 0; i < 6; i++)
						{
							try
							{
								c.Open();
							}
							catch (Exception ex)
							{
							}
						}
						c.Open();
						c.Close();*/
		}

		[Test]
		public void ClearPool()
		{
			string connStr = GetConnectionString(true) + "; min pool size=10";
			MySqlConnection[] connections = new MySqlConnection[10];
			connections[0] = new MySqlConnection(connStr);
			connections[0].Open();

            string assemblyName = typeof(MySqlConnection).Assembly.FullName;
            string pmName = String.Format("MySql.Data.MySqlClient.MySqlPoolManager, {0}", assemblyName);

            Type poolManager = Type.GetType(pmName, false);
            FieldInfo poolManagerHashTable = poolManager.GetField("pools",
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            Hashtable poolHash = (Hashtable)poolManagerHashTable.GetValue(null);
            FieldInfo clearingPoolsFI = poolManager.GetField("clearingPools",
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            ICollection clearingPools = (ICollection)clearingPoolsFI.GetValue(null);

            // now we need to investigate
            string poolName = String.Format("MySql.Data.MySqlClient.MySqlPool, {0}", assemblyName);
            Type poolType = Type.GetType(poolName, false);
            
            FieldInfo inUsePool = poolType.GetField("inUsePool", BindingFlags.NonPublic | BindingFlags.Instance);
            ICollection inUseList = (ICollection)inUsePool.GetValue(poolHash[connStr]);
            Assert.AreEqual(1, inUseList.Count);

            FieldInfo idlePool = poolType.GetField("idlePool", BindingFlags.NonPublic | BindingFlags.Instance);
            ICollection idleList = (ICollection)idlePool.GetValue(poolHash[connStr]);
            Assert.AreEqual(9, idleList.Count);

			// now open 4 more of these.  Now we shoudl have 5 open and five
			// still in the pool
			for (int i = 1; i < 5; i++)
			{
				connections[i] = new MySqlConnection(connStr);
				connections[i].Open();
			}

            Assert.AreEqual(5, inUseList.Count);
            Assert.AreEqual(5, idleList.Count);

            Assert.AreEqual(0, clearingPools.Count);
			// now tell this connection to clear its pool
			MySqlConnection.ClearPool(connections[0]);
            Assert.AreEqual(1, clearingPools.Count);
			Assert.AreEqual(0, idleList.Count);

			for (int i = 0; i < 5; i++)
				connections[i].Close();
            Assert.AreEqual(0, clearingPools.Count);
		}

		[Test]
		public void TestBadConnections()
		{
			MySqlConnectionStringBuilder builder = new
			MySqlConnectionStringBuilder();

			builder.Pooling = true;
			builder.Server = "xxxxxxxx"; // one that definitely does not exist.
			builder.UserID = "whoever";
			builder.Password = "whatever";

			int numberOfConnections = 1;

			for (int i = 0; i < numberOfConnections; ++i)
			{
				using (MySqlConnection connection = new MySqlConnection(builder.ConnectionString))
				{
					try
					{
						connection.Open();
						Assert.Fail("Connection should throw an exception.");
					}
					catch (Exception)
					{
					}
				}
				Thread.Sleep(50);
			}
			MySqlConnection.ClearAllPools();
		}
	}
}
