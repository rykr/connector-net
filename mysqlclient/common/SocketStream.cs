// Copyright (C) 2004-2005 MySQL AB
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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Runtime.CompilerServices;
using MySql.Data.MySqlClient;

namespace MySql.Data.Common
{
	/// <summary>
	/// Summary description for MySqlSocket.
	/// </summary>
	internal sealed class SocketStream : Stream, IDisposable
	{
		private Socket	socket;
		private bool	canRead;
		private bool	canWrite;

		public SocketStream(AddressFamily addressFamily, SocketType socketType, ProtocolType protocol)
			: base()
		{
			socket = new Socket(addressFamily, socketType, protocol);
			canRead = true;
			canWrite = true;
		}

		#region Properties

		public Socket Socket 
		{
			get { return socket; }
		}

		public override bool CanRead
		{
			get	{ return canRead;	}
		}

		public override bool CanSeek
		{
			get	{ return false;	}
		}

		public override bool CanWrite
		{
			get	{ return canWrite; }
		}

		public override long Length
		{
			get	{ return 0;	}
		}

		public override long Position
		{
			get	{ return 0;	}
			set	{ throw new NotSupportedException(Resources.GetString("SocketNoSeek")); }
		}

		#endregion

		#region Stream Implementation

		public override void Close()
		{
			Dispose();
		}


		public override void Flush()
		{
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			try 
			{
				return socket.Receive(buffer, offset, count, SocketFlags.None);
			}
			catch (Exception ex)
			{
				canRead = false;
				canWrite = false;
				socket.Shutdown(SocketShutdown.Both);
				socket.Close();
				socket = null;
				throw new MySqlException(ex.Message, true, ex);
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException(Resources.GetString("SocketNoSeek"));
		}

		public override void SetLength(long value)
		{
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			try 
			{
				if (canWrite && socket != null)
					socket.Send(buffer, offset, count, SocketFlags.None);
			}
			catch (Exception ex)
			{
				canRead = false;
				canWrite = false;
				socket.Shutdown(SocketShutdown.Both);
				socket.Close();
				socket = null;
				throw new MySqlException(ex.Message, true, ex);
			}
		}


		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (socket == null) return;

			canRead = false;
			canWrite = false;
			try 
			{
				socket.Shutdown(SocketShutdown.Both);				
			}
			catch (Exception)
			{
			}
			socket.Close();
			socket = null;
		}

		#endregion


		// This routine is internal to the Mono runtime so we can't change
		// the name
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Connect_internal(IntPtr sock,
			SocketAddress sa, out int error);

		private static void Connect_internal_NET(IntPtr sock,
			SocketAddress sa, out int error)
		{
			byte[] buff = new byte[sa.Size];
			for (int i=0; i<sa.Size; i++)
				buff[i] = sa[i];

			int result = NativeMethods.connect(sock, buff, sa.Size);
			if (result != 0)
				error = NativeMethods.WSAGetLastError();
			else
				error = 0;
		}

		public bool Connect(EndPoint remoteEP, int timeout)
		{
			int err;
			// set the socket to non blocking
			socket.Blocking = false;

			// then we star the connect
			SocketAddress addr = remoteEP.Serialize();

			if (Platform.IsWindows())
				Connect_internal_NET(socket.Handle, addr, out err);
			else
				Connect_internal(socket.Handle, addr, out err);

			if (err != 10035 && err != 10036)
				throw new MySqlException(Resources.GetString("ErrorCreatingSocket"));

			// next we wait for our connect timeout or until the socket is connected
			ArrayList write = new ArrayList();
			write.Add(socket);
			ArrayList error = new ArrayList();
			error.Add(socket);

			Socket.Select(null, write, error, timeout*1000*1000);

			if (write.Count == 0) 
				return false;

			// set socket back to blocking mode
			socket.Blocking = true;
			return true;
		}
	}
}
