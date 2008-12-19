using System;

namespace Chord.Common
{
	[Serializable]
	public abstract class Node<T>
	{
		protected ID nodeId;
		protected URL nodeURL;

		public ID NodeId
		{
			get { return nodeId; }
			set { nodeId = value; }
		}

		public URL NodeURL
		{
			get { return nodeURL; }
			set { nodeURL = value; }
		}

		public override bool Equals(object obj)
		{
			if (obj == this)
			{
				return true;
			}
			Node<T> other = obj as Node<T>;
			if (other != null)
			{
				return nodeId.Equals(other.nodeId);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return nodeId.GetHashCode();
		}

		public override string ToString()
		{
			String id = null;
			if (nodeId != null)
			{
				id = nodeId.ToString();
			}
			String url = "null";
			if (nodeURL != null)
			{
				url = nodeURL.ToString();
			}
			return "Node[type=" + GetType().FullName + ", id="
			       + id + ", url=" + url + "]";
		}

		/// <summary>
		/// Returns the Chord node which is responsible for the given key.
		/// </summary>
		/// <param name="key">Key for which the successor is searched for</param>
		/// <returns>Responsible node</returns>
		public abstract T FindSuccessor(ID key);

		/// <summary>
		/// Join to a chord network
		/// </summary>
		/// <param name="bootstrapURL"></param>
		public abstract void JoinNetwork(URL bootstrapURL);

		/// <summary>
		/// Create a chord network
		/// </summary>
		public abstract void Create();

		/// <summary>
		/// Requests a sign of live. This method is invoked by another node which
		/// thinks it is this node's successor.
		/// </summary>
		public abstract void Ping();

		/// <summary>
		/// Inform a node that its predecessor leaves the network.
		/// </summary>
		/// <param name="predecessor"></param>
		public abstract void LeavesNetwork(T predecessor);

		/// <summary>
		/// Closes the connection to the node.
		/// </summary>
		public abstract void Disconnect();
	}
}