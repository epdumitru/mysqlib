﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Persistents
{

	public class UserData
	{
		private long id;
		private string username;
		private string password;
		private UserData other;
		private string[] strArray;

		public long Id
		{
			get { return id; }
			set { id = value; }
		}

		public string Username
		{
			get { return username; }
			set { username = value; }
		}

		public string Password
		{
			get { return password; }
			set { password = value; }
		}

		public UserData Other
		{
			get { return other; }
			set { other = value; }
		}

		public string[] StrArray
		{
			get { return strArray; }
			set { strArray = value; }
		}
	}
}