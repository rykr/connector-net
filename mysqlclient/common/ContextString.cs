// Copyright (C) 2004-2006 MySQL AB
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
using System.Collections;
using System.Text;

namespace MySql.Data.Common
{
	internal class ContextString
	{
        string contextMarkers;
        bool escapeBackslash;

		// Create a private ctor so the compiler doesn't give us a default one
		public ContextString(string contextMarkers, bool escapeBackslash)
		{
            this.contextMarkers = contextMarkers;
            this.escapeBackslash = escapeBackslash;
		}

		public string ContextMarkers
		{
			get { return contextMarkers; }
			set { contextMarkers = value; }
		}

        public int IndexOf(string src, char target)
        {
            char contextMarker = Char.MinValue;
            bool escaped = false;
            int pos = 0;

            foreach (char c in src)
            {
                int contextIndex = contextMarkers.IndexOf(c);

                // if we have found the closing marker for our open marker, then close the context
                if (contextIndex > -1 && contextMarker == contextMarkers[contextIndex] && !escaped)
                    contextMarker = Char.MinValue;

                // if we have found a context marker and we are not in a context yet, then start one
                else if (contextMarker == Char.MinValue && contextIndex > -1 && !escaped)
                    contextMarker = c;

                else if (contextMarker == Char.MinValue && c == target)
                    return pos;
                else if (c == '\\' && escapeBackslash)
                    escaped = !escaped;
                pos++;
            }
            return -1;
        }

		public string[] Split(string src, string delimiters)
		{
			ArrayList parts = new ArrayList();
			StringBuilder sb = new StringBuilder();
			bool escaped = false;

			char contextMarker = Char.MinValue;

			foreach (char c in src)
			{
				if (delimiters.IndexOf(c) != -1 && !escaped)
				{
					if (contextMarker != Char.MinValue) 
						sb.Append(c);
					else
					{
						if (sb.Length > 0) 
						{
							parts.Add( sb.ToString() );
							sb.Remove( 0, sb.Length );
						}
					}
				}
				else if (c == '\\' && escapeBackslash)
				  escaped = !escaped;
				else 
				{
					int contextIndex = contextMarkers.IndexOf(c);

					// if we already have a context marker but we match with the first of a pair
					// then both markers must be the same.
					if ((contextIndex % 2) == 0 && contextMarker != Char.MinValue)
						contextIndex++;

					// if we have found the closing marker for our open marker, then close the context
					if ((contextIndex % 2) == 1 && contextMarker == contextMarkers[contextIndex-1] && !escaped)
						contextMarker = Char.MinValue;

					// if we have found a context marker and we are not in a context yet, then start one
					else if (contextMarker == Char.MinValue && (contextMarkers.IndexOf(c) % 2 == 0) && !escaped)
						contextMarker = c;

					sb.Append( c );
				}
			}
			if (sb.Length > 0)
				parts.Add( sb.ToString() );
			return (string[])parts.ToArray( typeof(string) );
		}
	}
}
