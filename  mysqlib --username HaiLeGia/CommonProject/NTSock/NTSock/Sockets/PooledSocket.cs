using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using Logger;
using NTSock.Controller;
using ObjectSerializer;

namespace NTSock.Sockets
{
	internal class PooledSocket
	{
		public const int BODY = 1;
		public const int HEADER = 0;
		private MemoryStream messageStream;
		private readonly SocketPool pool;
		private readonly Socket socket;
		private readonly object writeLock;
        private TotalFormatter formatter;

		private PooledSocket(Socket socket, SocketPool pool, TotalFormatter formatter)
		{
		    this.formatter = formatter;
			messageStream = new MemoryStream();
			this.socket = socket;
			writeLock = new object();
			this.pool = pool;
			socket.SendTimeout = 1000; //1 seconds :| is it okie?
			socket.NoDelay = true;
			socket.DontFragment = true;
			socket.LingerState.Enabled = false;
			ReadBuffer(4, HEADER);
		}

		public bool Connected
		{
			get { return socket.Connected && socket.RemoteEndPoint != null; }
		}

		public void Close()
		{
			socket.Close(1000);
			messageStream.Close();
		}

		public void Send(Mail mail)
		{
			byte[] data;
			data = formatter.Serialize(mail);
            using (var sendStream = new MemoryStream())
			{
				WriteInt(data.Length, sendStream);
				sendStream.Write(data, 0, data.Length);
				var datagram = sendStream.ToArray();
				Write(datagram);
			}
		}

		private static void WriteInt(int i, Stream stream)
		{
			for (int j = 3; j >= 0; j--)
			{
				stream.WriteByte((byte) (i >> j*8));
			}
		}

		private void Write(byte[] bytes)
		{
			try
			{
				lock (writeLock)
				{
					socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, Sent, null);
				}
			}
			catch (Exception e)
			{
				pool.RemoveSocket(this);
				pool.NetworkException(e);
			}
		}

		private void Sent(IAsyncResult ar)
		{
			try
			{
				socket.EndReceive(ar);
			}
			catch (Exception e)
			{
				pool.RemoveSocket(this);
				pool.NetworkException(e);
			}
		}

	    private void ReadBuffer(long remainLen, int type)
		{
			if (Connected)
			{
				var buffer = new byte[remainLen];
				AsyncCallback callBack = null;
				switch (type)
				{
					case HEADER:
						callBack = ProcessHeader;
						break;
					case BODY:
						callBack = ProcessBody;
						break;
					default:
						throw new ArgumentException("Cannot regconize type: " + type);
				}
				try
				{
					socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, callBack, buffer);
				}
				catch (Exception e)
				{
					pool.RemoveSocket(this);
					pool.NetworkException(e);
				}
			}
		}

        private int expectingBodyLen;
	    
	    private void ProcessHeader(IAsyncResult ar)
		{
			if (Connected)
			{
				try
				{
					int byteRead = socket.EndReceive(ar);
					if (byteRead == 0)
					{
						pool.RemoveSocket(this);
						pool.NetworkClose();
						return;
					}
					var readBytes = (byte[]) ar.AsyncState;
					messageStream.Write(readBytes, 0, byteRead);
					long remaining = 4 - messageStream.Length;
					if (remaining > 0)
					{
						ReadBuffer(remaining, HEADER);
					}
					else
					{
						messageStream.Position = 0;
						int mailSize = ReadInt(messageStream);
						ReadBuffer(mailSize, BODY);
					    expectingBodyLen = mailSize;
                        
					}
				}
				catch (Exception e)
				{
					pool.RemoveSocket(this);
					pool.NetworkException(e);
				}
			}
		}

		private void ProcessBody(IAsyncResult ar)
		{
			if (Connected)
			{
				try
				{
					int byteRead = socket.EndReceive(ar);
					if (byteRead == 0)
					{
						pool.RemoveSocket(this);
						pool.NetworkClose();
						return;
					}
					var readBytes = (byte[]) ar.AsyncState;
					messageStream.Write(readBytes, 0, byteRead);
                    messageStream.Flush();
					long remaining = readBytes.Length - byteRead;
					if (remaining > 0)
					{
						ReadBuffer(remaining, BODY);
					}
					else if (remaining == 0)
					{
                        messageStream.Position = 4;
						try
						{
							var mail = formatter.Deserialize<Mail>(messageStream);
							messageStream.Close();
							messageStream = new MemoryStream();
                            pool.ReceiveMail(mail);
						}
						catch (Exception e)
						{
                            pool.RemoveSocket(this);
							pool.MessageError(this, e);
						}
						ReadBuffer(4, HEADER);
					}
				}
				catch (Exception e)
				{
					pool.RemoveSocket(this);
					pool.NetworkException(e);
				}
			}
		}

		private static int ReadInt(Stream stream)
		{
			int i = 0;
			for (int j = 0; j < 4; j++)
			{
				i <<= 8;
				int readByte = stream.ReadByte();
				i |= readByte;
			}
			return i;
		}

		public static PooledSocket CreateNewSocket(EndPoint endpoint, SocketPool pool, TotalFormatter formatter)
		{
			try
			{
				var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				socket.Connect(endpoint);
				return new PooledSocket(socket, pool, formatter);
			}
			catch
			{
				return null;
			}
		}

		public static PooledSocket CreateNewSocket(string host, int port, SocketPool pool, TotalFormatter formatter)
		{
			try
			{
				var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				socket.Connect(host, port);
				return new PooledSocket(socket, pool, formatter);
			}
			catch
			{
				return null;
			}
		}

		public static PooledSocket CreateNewSocket(Socket socket, SocketPool pool, TotalFormatter formatter)
		{
			return new PooledSocket(socket, pool, formatter);
		}
	}
}