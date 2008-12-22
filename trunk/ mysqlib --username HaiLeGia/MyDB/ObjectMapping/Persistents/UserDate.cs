﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectMapping.Attributes;

namespace ObjectMapping.Persistents
{
    [PersistentAttribute]
    public class UserData : IDbObject
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

    	public long UpdateTime
    	{
    		get { throw new System.NotImplementedException(); }
    		set { throw new System.NotImplementedException(); }
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

        [OneToOneRelation(PartnerKey = "Id")]
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

    	#region Implementation of IDbObject

    	private bool isDirty;
    	public bool IsDirty
    	{
    		get { return isDirty; }
    		set { isDirty = value; }
    	}

    	#endregion
    }
}