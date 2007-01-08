namespace MySql.Data.MySqlClient 
{
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    internal class Resources 
    {
        
        private static System.Resources.ResourceManager resourceMan;
        
        private static System.Globalization.CultureInfo resourceCulture;
        
        internal Resources() 
        {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    System.Resources.ResourceManager temp = new System.Resources.ResourceManager("MySql.Data.MySqlClient.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Improper MySqlCommandBuilder state: adapter is null.
        /// </summary>
        internal static string AdapterIsNull {
            get {
                return ResourceManager.GetString("AdapterIsNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Improper MySqlCommandBuilder state: adapter&apos;s SelectCommand is null.
        /// </summary>
        internal static string AdapterSelectIsNull {
            get {
                return ResourceManager.GetString("AdapterSelectIsNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Version string not in acceptable format.
        /// </summary>
        internal static string BadVersionFormat {
            get {
                return ResourceManager.GetString("BadVersionFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to  The buffer cannot be null.
        /// </summary>
        internal static string BufferCannotBeNull {
            get {
                return ResourceManager.GetString("BufferCannotBeNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to  Buffer is not large enough.
        /// </summary>
        internal static string BufferNotLargeEnough {
            get {
                return ResourceManager.GetString("BufferNotLargeEnough", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MySqlCommandBuilder does not support multi-table statements.
        /// </summary>
        internal static string CBMultiTableNotSupported {
            get {
                return ResourceManager.GetString("CBMultiTableNotSupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MySqlCommandBuilder cannot operate on tables with no unique or key columns.
        /// </summary>
        internal static string CBNoKeyColumn {
            get {
                return ResourceManager.GetString("CBNoKeyColumn", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Chaos isolation level is not supported.
        /// </summary>
        internal static string ChaosNotSupported {
            get {
                return ResourceManager.GetString("ChaosNotSupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The CommandText property has not been properly initialized..
        /// </summary>
        internal static string CommandTextNotInitialized {
            get {
                return ResourceManager.GetString("CommandTextNotInitialized", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The connection is already open..
        /// </summary>
        internal static string ConnectionAlreadyOpen {
            get {
                return ResourceManager.GetString("ConnectionAlreadyOpen", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Connection must be valid and open.
        /// </summary>
        internal static string ConnectionMustBeOpen {
            get {
                return ResourceManager.GetString("ConnectionMustBeOpen", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The connection is not open..
        /// </summary>
        internal static string ConnectionNotOpen {
            get {
                return ResourceManager.GetString("ConnectionNotOpen", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The connection property has not been set..
        /// </summary>
        internal static string ConnectionNotSet {
            get {
                return ResourceManager.GetString("ConnectionNotSet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to  Count cannot be negative.
        /// </summary>
        internal static string CountCannotBeNegative {
            get {
                return ResourceManager.GetString("CountCannotBeNegative", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SetLength is not a valid operation on CompressedStream.
        /// </summary>
        internal static string CSNoSetLength {
            get {
                return ResourceManager.GetString("CSNoSetLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There is already an open DataReader associated with this Connection which must be closed first..
        /// </summary>
        internal static string DataReaderOpen {
            get {
                return ResourceManager.GetString("DataReaderOpen", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error creating socket connection.
        /// </summary>
        internal static string ErrorCreatingSocket {
            get {
                return ResourceManager.GetString("ErrorCreatingSocket", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to From index and length use more bytes than from contains.
        /// </summary>
        internal static string FromAndLengthTooBig {
            get {
                return ResourceManager.GetString("FromAndLengthTooBig", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to From index must be a valid index inside the from buffer.
        /// </summary>
        internal static string FromIndexMustBeValid {
            get {
                return ResourceManager.GetString("FromIndexMustBeValid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Index and length use more bytes than to has room for.
        /// </summary>
        internal static string IndexAndLengthTooBig {
            get {
                return ResourceManager.GetString("IndexAndLengthTooBig", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Index must be a valid position in the buffer.
        /// </summary>
        internal static string IndexMustBeValid {
            get {
                return ResourceManager.GetString("IndexMustBeValid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Keyword not supported..
        /// </summary>
        internal static string KeywordNotSupported {
            get {
                return ResourceManager.GetString("KeywordNotSupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to NamedPipeStream does not support seeking.
        /// </summary>
        internal static string NamedPipeNoSeek {
            get {
                return ResourceManager.GetString("NamedPipeNoSeek", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to NamedPipeStream doesn&apos;t support SetLength.
        /// </summary>
        internal static string NamedPipeNoSetLength {
            get {
                return ResourceManager.GetString("NamedPipeNoSetLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Nested transactions are not supported..
        /// </summary>
        internal static string NoNestedTransactions {
            get {
                return ResourceManager.GetString("NoNestedTransactions", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to  Offset cannot be negative.
        /// </summary>
        internal static string OffsetCannotBeNegative {
            get {
                return ResourceManager.GetString("OffsetCannotBeNegative", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Offset must be a valid position in buffer.
        /// </summary>
        internal static string OffsetMustBeValid {
            get {
                return ResourceManager.GetString("OffsetMustBeValid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Parameter cannot have a negative value.
        /// </summary>
        internal static string ParameterCannotBeNegative {
            get {
                return ResourceManager.GetString("ParameterCannotBeNegative", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Parameter cannot be null.
        /// </summary>
        internal static string ParameterCannotBeNull {
            get {
                return ResourceManager.GetString("ParameterCannotBeNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Parameter is invalid..
        /// </summary>
        internal static string ParameterIsInvalid {
            get {
                return ResourceManager.GetString("ParameterIsInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Password must be valid and contain length characters.
        /// </summary>
        internal static string PasswordMustHaveLegalChars {
            get {
                return ResourceManager.GetString("PasswordMustHaveLegalChars", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Packets larger than max_allowed_packet are not allowed..
        /// </summary>
        internal static string QueryTooLarge {
            get {
                return ResourceManager.GetString("QueryTooLarge", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Reading from the stream has failed..
        /// </summary>
        internal static string ReadFromStreamFailed {
            get {
                return ResourceManager.GetString("ReadFromStreamFailed", resourceCulture);
            }
        }

        internal static string ReturnParameterExists
        {
            get
            {
                return ResourceManager.GetString("ReturnParameterExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Socket streams do not support seeking.
        /// </summary>
        internal static string SocketNoSeek {
            get {
                return ResourceManager.GetString("SocketNoSeek", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Stored procedures are not supported on this version of MySQL.
        /// </summary>
        internal static string SPNotSupported {
            get {
                return ResourceManager.GetString("SPNotSupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The stream has already been closed.
        /// </summary>
        internal static string StreamAlreadyClosed {
            get {
                return ResourceManager.GetString("StreamAlreadyClosed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to  The stream does not support reading.
        /// </summary>
        internal static string StreamNoRead {
            get {
                return ResourceManager.GetString("StreamNoRead", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The stream does not support writing.
        /// </summary>
        internal static string StreamNoWrite {
            get {
                return ResourceManager.GetString("StreamNoWrite", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to execute stored procedure &apos;{0}&apos;..
        /// </summary>
        internal static string UnableToExecuteSP {
            get {
                return ResourceManager.GetString("UnableToExecuteSP", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unix sockets are not supported on Windows.
        /// </summary>
        internal static string UnixSocketsNotSupported {
            get {
                return ResourceManager.GetString("UnixSocketsNotSupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Writing to the stream failed..
        /// </summary>
        internal static string WriteToStreamFailed {
            get {
                return ResourceManager.GetString("WriteToStreamFailed", resourceCulture);
            }
        }

		 internal static string WrongParameterName
		 {
			 get
			 {
				 return ResourceManager.GetString("WrongParameterName", resourceCulture);
			 }
		 }

    }
}
