using System;
using System.Runtime.InteropServices;

namespace MySql.Data.Common
{
	internal class WinSock
	{
		// SOcket routines
		[DllImport("ws2_32.dll", SetLastError=true)]
		static extern public IntPtr socket(int af, int type, int protocol);

		[DllImport("ws2_32.dll", SetLastError=true)]
		static extern public int ioctlsocket(IntPtr socket, uint cmd, ref UInt32 arg);

		[DllImport("ws2_32.dll", SetLastError=true)]
		public static extern int WSAIoctl(IntPtr s, uint dwIoControlCode, byte[] inBuffer, uint cbInBuffer,
			byte[] outBuffer, uint cbOutBuffer, IntPtr lpcbBytesReturned, IntPtr lpOverlapped,
			IntPtr lpCompletionRoutine);

		[DllImport("ws2_32.dll", SetLastError=true)]
		static extern public int WSAGetLastError();

		[DllImport("ws2_32.dll", SetLastError=true)]
		static extern public int connect(IntPtr socket, byte[] addr, int addrlen);

		[DllImport("ws2_32.dll", SetLastError=true)]
		static extern public int recv(IntPtr socket, byte[] buff, int len, int flags);

		[DllImport("ws2_32.Dll", SetLastError=true)]
		static extern public int send(IntPtr socket, byte[] buff, int len, int flags);
	}
}
