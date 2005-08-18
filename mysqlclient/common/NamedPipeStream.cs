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


namespace MySql.Data.Common
{
	/// <summary>
	/// Summary description for API.
	/// </summary>
	internal class NamedPipeStream : Stream
	{

		int			pipeHandle;
		FileAccess	_mode;

		public NamedPipeStream(string host, FileAccess mode)
		{
			Open(host, mode);
		}

		public void Open( string host, FileAccess mode )
		{
			_mode = mode;
			uint pipemode = 0;

			if ((mode & FileAccess.Read) > 0)
				pipemode |= NativeMethods.GENERIC_READ;
			if ((mode & FileAccess.Write) > 0)
				pipemode |= NativeMethods.GENERIC_WRITE;

			pipeHandle = NativeMethods.CreateFile( host, pipemode,
						0, null, NativeMethods.OPEN_EXISTING, 0, 0 );
//			try 
//			{
//				stream = new FileStream( (IntPtr)pipeHandle, FileAccess.ReadWrite );
//			}
//			catch (Exception ex) 
//			{
//				Console.WriteLine( ex.Message );
//			}

		}

/*		public bool DataAvailable
		{
			get 
			{
				uint bytesRead=0, avail=0, thismsg=0;

				bool result = Win32.PeekNamedPipe( pipeHandle, 
					null, 0, ref bytesRead, ref avail, ref thismsg );
				return (result == true && avail > 0);
			}
		}
*/
		public override bool CanRead
		{
			get { return (_mode & FileAccess.Read) > 0; }
		}

		public override bool CanWrite
		{
			get { return (_mode & FileAccess.Write) > 0; }
		}

		public override bool CanSeek
		{
			get { throw new NotSupportedException(Resources.GetString("NamedPipeNoSeek")); }
		}

		public override long Length
		{
			get { throw new NotSupportedException(Resources.GetString("NamedPipeNoSeek")); }
		}

		public override long Position 
		{
			get { throw new NotSupportedException(Resources.GetString("NamedPipeNoSeek")); }
			set { }
		}

		public override void Flush() 
		{
//			if (stream != null)
//				stream.Flush();
			if ( pipeHandle == 0 )
				throw new ObjectDisposedException("NamedPipeStream", 
					Resources.GetString("StreamAlreadyClosed"));
			NativeMethods.FlushFileBuffers((IntPtr)pipeHandle);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
/*			try 
			{
				uint bytesRead=0, avail=0, thismsg=0;

				bool result = Win32.PeekNamedPipe( pipeHandle, 
					null, 0, ref bytesRead, ref avail, ref thismsg );
				if (result)
					return stream.Read( buffer, offset, (int)avail );
				else
					return -1;
			}
			catch (Exception ex) 
			{
				Console.WriteLine(ex.Message);
			}
			return -1;*/
			if (buffer == null) 
				throw new ArgumentNullException("buffer", 
					Resources.GetString("BufferCannotBeNull"));
			if (buffer.Length < (offset + count))
				throw new ArgumentException(
					Resources.GetString("BufferNotLargeEnough"));
			if (offset < 0) 
				throw new ArgumentOutOfRangeException("offset", offset, 
					Resources.GetString("OffsetCannotBeNegative"));
			if (count < 0)
				throw new ArgumentOutOfRangeException("count", count, 
					Resources.GetString("CountCannotBeNegative"));
			if (! CanRead)
				throw new NotSupportedException(Resources.GetString("StreamNoRead"));
			if (pipeHandle == 0)
				throw new ObjectDisposedException("NamedPipeStream", 
					Resources.GetString("StreamAlreadyClosed"));

			// first read the data into an internal buffer since ReadFile cannot read into a buf at
			// a specified offset
			uint read=0;
			byte[] buf = new Byte[count];
			NativeMethods.ReadFile((IntPtr)pipeHandle, buf, (uint)count, out read, IntPtr.Zero); 
			
			for (int x=0; x < read; x++) 
			{
				buffer[offset+x] = buf[x];
			}
			return (int)read;
		}

		public override void Close()
		{
//			stream.Close();
			//stream = null;
			NativeMethods.CloseHandle((IntPtr)pipeHandle);
			pipeHandle = 0;
		}

		public override void SetLength(long length)
		{
			throw new NotSupportedException(Resources.GetString("NamedPipeNoSetLength"));
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
//			try 
//			{
//				stream.Write( buffer, offset, count );
//			}
//			catch (Exception ex) 
//			{
//				Console.WriteLine( ex.Message );
//			}
			if (buffer == null) 
				throw new ArgumentNullException("buffer", Resources.GetString("BufferCannotBeNull"));
			if (buffer.Length < (offset + count))
				throw new ArgumentException(Resources.GetString("BufferNotLargeEnough"), "buffer");
			if (offset < 0) 
				throw new ArgumentOutOfRangeException("offset", offset, 
					Resources.GetString("OffsetCannotBeNegative"));
			if (count < 0)
				throw new ArgumentOutOfRangeException("count", count, 
					Resources.GetString("CountCannotBeNegative"));
			if (! CanWrite)
				throw new NotSupportedException(Resources.GetString("StreamNoWrite"));
			if (pipeHandle == 0)
				throw new ObjectDisposedException("NamedPipeStream", Resources.GetString("StreamAlreadyClosed"));
			
			// copy data to internal buffer to allow writing from a specified offset
			uint bytesWritten = 0;
			bool result;

			if (offset == 0  && count <= 65535)
				result = NativeMethods.WriteFile((IntPtr)pipeHandle, buffer, (uint)count, out bytesWritten, IntPtr.Zero);
			else
			{
				byte[] localBuf = new byte[65535];

				result = true;
				uint thisWritten;
				while (count != 0 && result)
				{
					int cnt = Math.Min( count, 65535 );
					Array.Copy( buffer, offset, localBuf, 0, cnt );
					result = NativeMethods.WriteFile((IntPtr)pipeHandle, localBuf, (uint)cnt, out thisWritten, IntPtr.Zero);
					bytesWritten += thisWritten;
					count -= cnt;
					offset += cnt;
				}

//				byte[] tempBuf = new byte[count];
//				try 
//				{
//					Array.Copy( buffer, offset, tempBuf, 0, count );
//				}
//				catch (Exception ex) 
//				{
//					Console.Write(ex.Message);
//				}
//				localBuf = tempBuf;
			}

//			bool result = Win32.WriteFile( pipeHandle, localBuf, (uint)count, out bytesWritten, null );
//			byte[] buf = new Byte[count];
//			for (int x=0; x < count; x++) 
//			{
//				buf[x] = buffer[offset+x];
//			}
//			uint written=0;
//			GCHandle h = GCHandle.Alloc( buffer, GCHandleType.Pinned );
//			IntPtr addr = Marshal.UnsafeAddrOfPinnedArrayElement( buffer, offset );
//			bool result = WriteFile( pipeHandle, addr, (uint)count, ref written, IntPtr.Zero );
//			h.Free();

			if (! result)
			{
				throw new IOException("Writing to the stream failed");
			}
			if (bytesWritten < count)
				throw new IOException("Unable to write entire buffer to stream");
		}

		public override long Seek( long offset, SeekOrigin origin )
		{
			throw new NotSupportedException(Resources.GetString("NamedPipeNoSeek"));
		}
	}
}


