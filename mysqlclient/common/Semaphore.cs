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
using System.Threading;
using System.Runtime.InteropServices;

namespace MySql.Data.Common
{
    internal class Semaphore : WaitHandle
    {
        AutoResetEvent autoEvent = null;
        object countLock = new object();

        int availableCount = 0;
        int capacityCount = 0;

        /// <summary>
        /// Initializes a new Semaphore
        /// </summary>
        /// <param name="initialCount">Initial tickets in this Semaphore instance</param>
        /// <param name="maximumCount">Capacity of this Semaphore instance</param>
        public Semaphore(int initialCount, int maximumCount)
        {
            autoEvent = new AutoResetEvent(false);

            availableCount = initialCount;
            capacityCount = maximumCount;
        }

        /// <summary>
        /// Releases one Semaphore ticket
        /// </summary>
        /// <returns>The count on the Semaphore before the Release method was called.</returns>
        public int Release()
        {
            return Release(1);
        }

        /// <summary>
        /// Releases a given number of Semaphore tickets
        /// </summary>
        /// <param name="releaseCount">Amount of tickets to release</param>
        /// <returns>The count on the Semaphore before the Release method was called.</returns>
        public int Release(int releaseCount)
        {
            if (releaseCount < 0)
                throw new ArgumentException("Release count must be >= 0", "releaseCount");
            
            int previousCount = availableCount;
            
            if (releaseCount == 0) return previousCount;


            if ((previousCount + releaseCount > capacityCount))
                throw new InvalidOperationException("Unable to release Semaphore");

            // Pulse the amount of threads for tickets we have released
            for (int i = 0; i < releaseCount; i++)
            {
                Interlocked.Increment(ref availableCount);
                autoEvent.Set();
            }

            return previousCount;
        }

        /// <summary>
        /// Attempt to take a ticket from the counter.
        /// If we have a ticket available, take it.
        /// </summary>
        /// <returns>Whether a ticket was taken.</returns>
        bool TryTakeTicket()
        {
            int currentCount = Interlocked.Decrement(ref availableCount);

            if (currentCount >= 0) return true;

            Interlocked.Increment(ref availableCount);
            return false;
        }

        public override bool WaitOne(int millisecondsTimeout, bool exitContext)
        {
            if ((millisecondsTimeout < 0) && (millisecondsTimeout != Timeout.Infinite))
                throw new ArgumentOutOfRangeException("millisecondsTimeout");

            if (exitContext)
                throw new ArgumentException(null, "exitContext");

            DateTime start = DateTime.Now;
            int timeout = millisecondsTimeout;
            while (timeout > 0)
            {
                // Is there a ticket free? Take one if there is and return.
                if (TryTakeTicket())
                    return true;

                // We have no tickets right now, lets wait for one.
                if (!autoEvent.WaitOne(timeout, false))
                    return false;
                timeout = millisecondsTimeout - (int)DateTime.Now.Subtract(start).TotalMilliseconds;
            }

            return false;
        }
    }
}
