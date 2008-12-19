using System;
using System.Collections;
using System.Collections.Generic;

namespace Chord.Common
{
	internal class FingerTable<T> where T : Node<T>
	{
		private ID localId;
		private References<T> references;
		private T[] remoteNodes;

		public FingerTable(ID localId, References<T> references)
		{
			this.localId = localId;
			this.references = references;
			remoteNodes = new T[localId.Length];
		}

		#region Private

		private void SetEntry(int index, T node)
		{
			CheckIndex(index);
			if (node != null)
			{
				remoteNodes[index] = node;
			}
		}

		private T GetEntry(int index)
		{
			CheckIndex(index);
			return remoteNodes[index];
		}

		private void UnsetEntry(int index)
		{
			CheckIndex(index);
			T overridedNode = remoteNodes[index];
			remoteNodes[index] = null;
			if (overridedNode != null)
			{
				references.DisconnectIfUnreferenced(overridedNode);
			}
		}

		private void CheckIndex(int index)
		{
			if (index < 0 || index >= remoteNodes.Length)
			{
				throw new ArgumentOutOfRangeException("Index is out of range");
			}
		}

		#endregion

		public void AddReference(T node)
		{
			if (node != null)
			{
				for (int i = 0; i < remoteNodes.Length; i++)
				{
					ID startIndex = localId.AddPowerOfTwo(i);
					if (!startIndex.IsInInterval(localId, node.NodeId))
					{
						break;
					}
					if (remoteNodes[i] == null)
					{
						remoteNodes[i] = node;
					}
					else if (node.NodeId.IsInInterval(localId, remoteNodes[i].NodeId))
					{
						T overrideNode = remoteNodes[i];
						remoteNodes[i] = node;
						if (overrideNode != null)
						{
							references.DisconnectIfUnreferenced(overrideNode);
						}
					}
				}
			}
		}

		public T[] GetCopyOfReferences()
		{
			T[] copy = new T[remoteNodes.Length];
			Array.Copy(remoteNodes, copy, remoteNodes.Length);
			return copy;
		}

		public void RemoveReferences(T node)
		{
			if (node != null)
			{
				// determine node reference with next larger ID than ID of node
				// reference to remove
				T referenceForReplacement = null;
				for (int i = remoteNodes.Length - 1; i >= 0; i--)
				{
					T n = remoteNodes[i];
					if (node.Equals(n))
					{
						break;
					}
					if (n != null)
					{
						referenceForReplacement = n;
					}
				}

				// remove reference(s)
				for (int i = 0; i < remoteNodes.Length; i++)
				{
					if (node.Equals(remoteNodes[i]))
					{
						if (referenceForReplacement == null)
						{
							UnsetEntry(i);
						}
						else
						{
							remoteNodes[i] = referenceForReplacement;
						}
					}
				}

				// try to add references of successor list to fill 'holes' in finger table
				T[] referencesOfSuccessorList = references.Successors;
				foreach (T referenceToAdd in referencesOfSuccessorList)
				{
					if (!node.Equals(referenceToAdd))
					{
						AddReference(referenceToAdd);
					}
				}
			}
		}

		public bool Contains(T node)
		{
			if (node == null)
			{
				throw new ArgumentNullException("Node cannot be null");
			}
			for (int i = 0; i < remoteNodes.Length; i++)
			{
				if (node.Equals(remoteNodes[i]))
				{
					return true;
				}
			}
			return false;
		}

		public T[] GetFingerTablenEntries(int count)
		{
			HashSet<T> result = new HashSet<T>();
			for (int i = 0; i < remoteNodes.Length; i++)
			{
				result.Add(remoteNodes[i]);
				if (result.Count >= count)
				{
					break;
				}
			}
			T[] res = new T[result.Count];
			result.CopyTo(res, 0);
			return res;
		}

		public T GetClosestPrecedingNode(ID lookupId)
		{
			for (int i = remoteNodes.Length - 1; i >= 0; i--)
			{
				T node = remoteNodes[i];
				if (node != null && node.NodeId.IsInInterval(localId, lookupId))
				{
					return node;
				}
			}
			return null;
		}
	}

	internal class SuccessorList<T> where T : Node<T>
	{
		private int capacity;
		private ID localId;
		private References<T> references;
		private IList<T> successors;

		public SuccessorList(ID localId, int capacity, References<T> references)
		{
			this.localId = localId;
			this.capacity = capacity;
			this.references = references;
			successors = new List<T>();
		}

		public int Capacity
		{
			get { return capacity; }
		}

		public int Count
		{
			get { return successors.Count; }
		}

		public void AddSuccessor(T node)
		{
			if (successors.Contains(node) ||
			    (successors.Count >= capacity && node.NodeId.IsInInterval(successors[successors.Count - 1].NodeId, localId)))
			{
				return;
			}
			bool inserted = false;
			for (int i = 0; i < successors.Count && !inserted; i++)
			{
				if (node.NodeId.IsInInterval(localId, successors[i].NodeId))
				{
					successors.Insert(i, node);
					inserted = true;
				}
			}
			if (!inserted && successors.Count < capacity)
			{
				successors.Add(node);
			}
		}

		public void RemoveReferences(T node)
		{
			if (node != null)
			{
				successors.Remove(node);
				T[] fingerTableNodes = references.GetFirstFingerTableEntries(capacity);
				foreach (T tmpNode in fingerTableNodes)
				{
					if (!node.Equals(tmpNode) && successors.Count < capacity)
					{
						AddSuccessor(tmpNode);
					}
				}
			}
		}

		public T[] GetReferences()
		{
			T[] result = new T[successors.Count];
			successors.CopyTo(result, 0);
			return result;
		}

		public T GetClosestPrecedingNode(ID lookupId)
		{
			for (int i = successors.Count - 1; i >= 0; i--)
			{
				T node = successors[i];
				if (node.NodeId.IsInInterval(localId, lookupId))
				{
					return node;
				}
			}
			return null;
		}

		public bool Contains(T node)
		{
			if (node == null)
			{
				throw new ArgumentNullException();
			}
			return successors.Contains(node);
		}

		public T GetDirectSuccessor()
		{
			if (successors.Count > 0)
			{
				return successors[0];
			}
			return null;
		}
	}

	public class References<T> where T : Node<T>
	{
		private FingerTable<T> fingerTable;
		private ID localId;
		private URL localURL;
		private T predecessor;
		private SuccessorList<T> successorList;

		public References(ID localId, URL localURL, int capacity)
		{
			this.localURL = localURL;
			this.localId = localId;
			fingerTable = new FingerTable<T>(localId, this);
			successorList = new SuccessorList<T>(localId, capacity, this);
		}

		public T Successor
		{
			get { return successorList.GetDirectSuccessor(); }
		}

		public T[] Successors
		{
			get { return successorList.GetReferences(); }
		}

		public T Predecessor
		{
			get { return predecessor; }
			set
			{
				if (value != null && !value.Equals(predecessor))
				{
					T formerPredecessor = predecessor;
					predecessor = value;
					DisconnectIfUnreferenced(formerPredecessor);
				}
			}
		}

		public T GetClosestPrecedingNode(ID lookupId)
		{
			if (lookupId == null)
			{
				throw new ArgumentNullException();
			}
			Dictionary<ID, T> suitableNodes = new Dictionary<ID, T>();
			T node1 = fingerTable.GetClosestPrecedingNode(lookupId);
			if (node1 != null)
			{
				suitableNodes.Add(node1.NodeId, node1);
			}
			T node2 = successorList.GetClosestPrecedingNode(lookupId);
			if (node2 != null && !suitableNodes.ContainsKey(node2.NodeId))
			{
				suitableNodes.Add(node2.NodeId, node2);
			}
			if (predecessor != null && !suitableNodes.ContainsKey(predecessor.NodeId) &&
			    lookupId.IsInInterval(predecessor.NodeId, localId))
			{
				suitableNodes.Add(predecessor.NodeId, predecessor);
			}
			ArrayList ids = new ArrayList(suitableNodes.Keys);
			ids.Add(lookupId);
			ids.Sort();
			int lookupIndex = ids.IndexOf(lookupId);
			int resultIndex = (ids.Count + lookupIndex - 1)%ids.Count;
			ID closestId = (ID) ids[resultIndex];
			if (suitableNodes.ContainsKey(closestId))
			{
				return suitableNodes[closestId];
			}
			else
			{
				throw new NullReferenceException("Closest node is null.");
			}
		}

		public void AddReference(T node)
		{
			if (node != null && node.NodeId != localId)
			{
				fingerTable.AddReference(node);
				successorList.AddSuccessor(node);
			}
		}

		public void RemoveReference(T node)
		{
			if (node != null)
			{
				fingerTable.RemoveReferences(node);
				successorList.RemoveReferences(node);
				if (node.Equals(predecessor))
				{
					predecessor = null;
				}
				DisconnectIfUnreferenced(node);
			}
		}

		public void DisconnectIfUnreferenced(T node)
		{
			if (node != null && !Contains(node))
			{
				node.Disconnect();
			}
		}

		private bool Contains(T node)
		{
			return node.Equals(predecessor) || fingerTable.Contains(node) || successorList.Contains(node);
		}

		public void AddReferenceAsPredecessor(T node)
		{
			if (node == null)
			{
				throw new ArgumentNullException();
			}
			if (predecessor == null || node.NodeId.IsInInterval(predecessor.NodeId, localId))
			{
				Predecessor = node;
			}
			AddReference(node);
		}

		public T[] GetFirstFingerTableEntries(int capacity)
		{
			return fingerTable.GetFingerTablenEntries(capacity);
		}
	}
}