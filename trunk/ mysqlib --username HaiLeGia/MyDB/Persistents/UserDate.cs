using System;
using System.Collections.Generic;
using ObjectMapping;
using ObjectMapping.Attributes;

namespace Persistents
{
	[PersistentAttribute]
	public class UserData : IDbObject
	{
		private long id;
		private string username;
		private string password;
		private long updateTime;

		public long Id
		{
			get { return id; }
			set { id = value; }
		}

		public long UpdateTime
		{
			get { return updateTime;  }
			set { updateTime = value; }
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
		#region Implementation of IDbObject

		private bool isDirty;
		[IgnorePersistent]
		public bool IsDirty
		{
			get { return isDirty; }
			set { isDirty = value; }
		}

		#endregion
	}

    [PersistentAttribute]
    public class HuniBuddyGroup : IDbObject
    {
        private IDictionary<string, bool> buddies; // Accepted or not
        private long userDataId;
        private string groupName;
        private long id;

        public HuniBuddyGroup()
        {
            buddies = new Dictionary<string, bool>();
        }
        

        public string GroupName
        {
            get { return groupName; }
            set
            {
                groupName = value;
            }
        }

        public long UserDataId
        {
            get { return userDataId; }
            set { userDataId = value; }
        }

        public IDictionary<string, bool> Buddies
        {
            get { return buddies; }
            set { buddies = value; }
        }

        #region IPersistent Members

        public long Id
        {
            get { return id; }
            set { id = value; }
        }

        private long upateTime;
        public long UpdateTime
        {
            get { return upateTime; }
            set { upateTime = value; }
        }

        private bool isDirty;

        [IgnorePersistent]
        public bool IsDirty
        {
            get { return isDirty; }
            set { isDirty = value; }
        }

        #endregion
    }

    [PersistentAttribute]
    public class HuniBelongList :IDbObject
    {
        private long id;
        private string userName;
        private List<string> belong; //list userName
    	
    	public long UpateTime
    	{
    		get { return upateTime; }
    		set { upateTime = value; }
    	}

    	public HuniBelongList()
        {
            belong = new List<string>();
        }

        public string UserName
        {
            get { return userName; }
            set
            {
                userName = value;
            }
        }

        public List<string> Belong
        {
            get { return belong; }
            set { belong = value; }
        }

        public long Id
        {
            get { return id; }
            set { id = value; }
        }

        private long upateTime;
        public long UpdateTime
        {
            get { return upateTime; }
            set { upateTime = value; }
        }

        private bool isDirty;

        [IgnorePersistent]
        public bool IsDirty
        {
            get { return isDirty; }
            set { isDirty = value; }
        }

    }


}