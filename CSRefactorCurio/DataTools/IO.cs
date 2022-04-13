// ************************************************* ''
// DataTools C# Native Utility Library For Windows - Interop
//
// Module: FileApi
//         Native File Services.
// 
// Copyright (C) 2011-2020 Nathan Moschkin
// All Rights Reserved
//
// Licensed Under the MIT License   
// ************************************************* ''

using System;
using System.Runtime.InteropServices;

using DataTools.Win32.Memory;

namespace DataTools.Win32
{
    internal static class IO
    {

        public static readonly IntPtr INVALID_HANDLE_VALUE = (IntPtr)(-1);
        
        public const int METHOD_BUFFERED = 0;
        public const int METHOD_IN_DIRECT = 1;
        public const int METHOD_OUT_DIRECT = 2;
        public const int METHOD_NEITHER = 3;
        public const int FILE_ANY_ACCESS = 0;
        public const int FILE_SPECIAL_ACCESS = FILE_ANY_ACCESS;
        public const int FILE_READ_ACCESS = 1;    // file & pipe
        public const int FILE_WRITE_ACCESS = 2;    // file & pipe

        //
        // File creation flags must start at the high end since they
        // are combined with the attributes
        //

        //
        //  These are flags supported through CreateFile (W7) and CreateFile2 (W8 and beyond)
        //

        public const int FILE_FLAG_WRITE_THROUGH = unchecked((int)0x80000000);
        public const int FILE_FLAG_OVERLAPPED = 0x40000000;
        public const int FILE_FLAG_NO_BUFFERING = 0x20000000;
        public const int FILE_FLAG_RANDOM_ACCESS = 0x10000000;
        public const int FILE_FLAG_SEQUENTIAL_SCAN = 0x8000000;
        public const int FILE_FLAG_DELETE_ON_CLOSE = 0x4000000;
        public const int FILE_FLAG_BACKUP_SEMANTICS = 0x2000000;
        public const int FILE_FLAG_POSIX_SEMANTICS = 0x1000000;
        public const int FILE_FLAG_SESSION_AWARE = 0x800000;
        public const int FILE_FLAG_OPEN_REPARSE_POINT = 0x200000;
        public const int FILE_FLAG_OPEN_NO_RECALL = 0x100000;
        public const int FILE_FLAG_FIRST_PIPE_INSTANCE = 0x80000;

        // (_WIN32_WINNT >= _WIN32_WINNT_WIN8) Then

        //
        //  These are flags supported only through CreateFile2 (W8 and beyond)
        //
        //  Due to the multiplexing of file creation flags, file attribute flags and
        //  security QoS flags into a single DWORD (dwFlagsAndAttributes) parameter for
        //  CreateFile, there is no way to add any more flags to CreateFile. Additional
        //  flags for the create operation must be added to CreateFile2 only
        //

        public const int FILE_FLAG_OPEN_REQUIRING_OPLOCK = 0x40000;

        //
        // (_WIN32_WINNT >= &H0400)
        //
        // Define possible return codes from the CopyFileEx callback routine
        //

        public const int PROGRESS_CONTINUE = 0;
        public const int PROGRESS_CANCEL = 1;
        public const int PROGRESS_STOP = 2;
        public const int PROGRESS_QUIET = 3;

        //
        // Define CopyFileEx callback routine state change values
        //

        public const int CALLBACK_CHUNK_FINISHED = 0x0;
        public const int CALLBACK_STREAM_SWITCH = 0x1;

        //
        // Define CopyFileEx option flags
        //

        public const int COPY_FILE_FAIL_IF_EXISTS = 0x1;
        public const int COPY_FILE_RESTARTABLE = 0x2;
        public const int COPY_FILE_OPEN_SOURCE_FOR_WRITE = 0x4;
        public const int COPY_FILE_ALLOW_DECRYPTED_DESTINATION = 0x8;

        //
        //  Gap for private copyfile flags
        //

        //  (_WIN32_WINNT >= &H0600)
        public const int COPY_FILE_COPY_SYMLINK = 0x800;
        public const int COPY_FILE_NO_BUFFERING = 0x1000;
        //

        // (_WIN32_WINNT >= _WIN32_WINNT_WIN8) Then

        //
        //  CopyFile2 flags
        //

        public const int COPY_FILE_REQUEST_SECURITY_PRIVILEGES = 0x2000;
        public const int COPY_FILE_RESUME_FROM_PAUSE = 0x4000;
        public const int COPY_FILE_NO_OFFLOAD = 0x40000;

        //

        //  /* _WIN32_WINNT >= &H0400 */

        //  (_WIN32_WINNT >= &H0500)
        //
        // Define ReplaceFile option flags
        //

        public const int REPLACEFILE_WRITE_THROUGH = 0x1;
        public const int REPLACEFILE_IGNORE_MERGE_ERRORS = 0x2;

        //  (_WIN32_WINNT >= &H0600)
        public const int REPLACEFILE_IGNORE_ACL_ERRORS = 0x4;
        //

        //  '' ''  (_WIN32_WINNT >= &H0500)

        //
        // Define the NamedPipe definitions
        //

        
        
        //
        // Define the dwOpenMode values for CreateNamedPipe
        //

        public const int PIPE_ACCESS_INBOUND = 0x1;
        public const int PIPE_ACCESS_OUTBOUND = 0x2;
        public const int PIPE_ACCESS_DUPLEX = 0x3;

        //
        // Define the Named Pipe End flags for GetNamedPipeInfo
        //

        public const int PIPE_CLIENT_END = 0x0;
        public const int PIPE_SERVER_END = 0x1;

        //
        // Define the dwPipeMode values for CreateNamedPipe
        //

        public const int PIPE_WAIT = 0x0;
        public const int PIPE_NOWAIT = 0x1;
        public const int PIPE_READMODE_BYTE = 0x0;
        public const int PIPE_READMODE_MESSAGE = 0x2;
        public const int PIPE_TYPE_BYTE = 0x0;
        public const int PIPE_TYPE_MESSAGE = 0x4;
        public const int PIPE_ACCEPT_REMOTE_CLIENTS = 0x0;
        public const int PIPE_REJECT_REMOTE_CLIENTS = 0x8;

        //
        // Define the well known values for CreateNamedPipe nMaxInstances
        //

        public const int PIPE_UNLIMITED_INSTANCES = 255;

        //
        // Define the Security Quality of Service bits to be passed
        // into CreateFile
        //

        
        public const int FILE_BEGIN = 0;
        public const int FILE_CURRENT = 1;
        public const int FILE_END = 2;

        /// <summary>
        /// Move methods for SetFilePointer and SetFilePointerEx
        /// </summary>
        /// <remarks></remarks>
        public enum FilePointerMoveMethod : uint
        {
            /// <summary>
            /// Sets the position relative to the beginning of the file.
            /// If this method is selected, then offset must be a positive number.
            /// </summary>
            /// <remarks></remarks>
            Begin = FILE_BEGIN,

            /// <summary>
            /// Sets the position relative to the current position of the file.
            /// </summary>
            /// <remarks></remarks>
            Current = FILE_CURRENT,

            /// <summary>
            /// Sets the position relative to the end of the file.
            /// </summary>
            /// <remarks></remarks>
            End = FILE_END
        }

        //''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        //                                                                    ''
        //                             ACCESS TYPES                           ''
        //                                                                    ''
        //''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

        // begin_wdm
        //
        //  The following are masks for the predefined standard access types
        //

        public const int DELETE = 0x10000;
        public const int READ_CONTROL = 0x20000;
        public const int WRITE_DAC = 0x40000;
        public const int WRITE_OWNER = 0x80000;
        public const int SYNCHRONIZE = 0x100000;
        public const int STANDARD_RIGHTS_REQUIRED = 0xF0000;
        public const int STANDARD_RIGHTS_READ = READ_CONTROL;
        public const int STANDARD_RIGHTS_WRITE = READ_CONTROL;
        public const int STANDARD_RIGHTS_EXECUTE = READ_CONTROL;
        public const int STANDARD_RIGHTS_ALL = 0x1F0000;
        public const int SPECIFIC_RIGHTS_ALL = 0xFFFF;

        //
        // AccessSystemAcl access type
        //

        public const int ACCESS_SYSTEM_SECURITY = 0x1000000;

        //
        // MaximumAllowed access type
        //

        public const int MAXIMUM_ALLOWED = 0x2000000;

        //
        //  These are the generic rights.
        //

        public const int GENERIC_READ = unchecked((int) 0x80000000);
        public const int GENERIC_WRITE = 0x40000000;
        public const int GENERIC_EXECUTE = 0x20000000;
        public const int GENERIC_ALL = 0x10000000;


        public const int FILE_READ_DATA = 0x1;    // file & pipe
        public const int FILE_LIST_DIRECTORY = 0x1;    // directory
        public const int FILE_WRITE_DATA = 0x2;    // file & pipe
        public const int FILE_ADD_FILE = 0x2;    // directory
        public const int FILE_APPEND_DATA = 0x4;    // file
        public const int FILE_ADD_SUBDIRECTORY = 0x4;    // directory
        public const int FILE_CREATE_PIPE_INSTANCE = 0x4;    // named pipe
        public const int FILE_READ_EA = 0x8;    // file & directory
        public const int FILE_WRITE_EA = 0x10;    // file & directory
        public const int FILE_EXECUTE = 0x20;    // file
        public const int FILE_TRAVERSE = 0x20;    // directory
        public const int FILE_DELETE_CHILD = 0x40;    // directory
        public const int FILE_READ_ATTRIBUTES = 0x80;    // all
        public const int FILE_WRITE_ATTRIBUTES = 0x100;    // all
        public const int FILE_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0x1FF;
        public const int FILE_GENERIC_READ = STANDARD_RIGHTS_READ | FILE_READ_DATA | FILE_READ_ATTRIBUTES | FILE_READ_EA | SYNCHRONIZE;
        public const int FILE_GENERIC_WRITE = STANDARD_RIGHTS_WRITE | FILE_WRITE_DATA | FILE_WRITE_ATTRIBUTES | FILE_WRITE_EA | FILE_APPEND_DATA | SYNCHRONIZE;
        public const int FILE_GENERIC_EXECUTE = STANDARD_RIGHTS_EXECUTE | FILE_READ_ATTRIBUTES | FILE_EXECUTE | SYNCHRONIZE;
        public const int FILE_SHARE_READ = 0x1;
        public const int FILE_SHARE_WRITE = 0x2;
        public const int FILE_SHARE_DELETE = 0x4;

        // Public Enum FileAttributes
        // [ReadOnly] = &H1
        // Hidden = &H2
        // System = &H4
        // Directory = &H10
        // Archive = &H20
        // Device = &H40
        // Normal = &H80
        // Temporary = &H100
        // SparseFile = &H200
        // ReparsePoint = &H400
        // Compressed = &H800
        // Offline = &H1000
        // NotContentIndexed = &H2000
        // Encrypted = &H4000
        // IntegrityStream = &H8000
        // Virtual = &H10000
        // NoScrubData = &H20000
        // End Enum

        public const int FILE_ATTRIBUTE_READONLY = 0x1;
        public const int FILE_ATTRIBUTE_HIDDEN = 0x2;
        public const int FILE_ATTRIBUTE_SYSTEM = 0x4;
        public const int FILE_ATTRIBUTE_DIRECTORY = 0x10;
        public const int FILE_ATTRIBUTE_ARCHIVE = 0x20;
        public const int FILE_ATTRIBUTE_DEVICE = 0x40;
        public const int FILE_ATTRIBUTE_NORMAL = 0x80;
        public const int FILE_ATTRIBUTE_TEMPORARY = 0x100;
        public const int FILE_ATTRIBUTE_SPARSE_FILE = 0x200;
        public const int FILE_ATTRIBUTE_REPARSE_POINT = 0x400;
        public const int FILE_ATTRIBUTE_COMPRESSED = 0x800;
        public const int FILE_ATTRIBUTE_OFFLINE = 0x1000;
        public const int FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x2000;
        public const int FILE_ATTRIBUTE_ENCRYPTED = 0x4000;
        public const int FILE_ATTRIBUTE_INTEGRITY_STREAM = 0x8000;
        public const int FILE_ATTRIBUTE_VIRTUAL = 0x10000;
        public const int FILE_ATTRIBUTE_NO_SCRUB_DATA = 0x20000;
        public const int FILE_NOTIFY_CHANGE_FILE_NAME = 0x1;
        public const int FILE_NOTIFY_CHANGE_DIR_NAME = 0x2;
        public const int FILE_NOTIFY_CHANGE_ATTRIBUTES = 0x4;
        public const int FILE_NOTIFY_CHANGE_SIZE = 0x8;
        public const int FILE_NOTIFY_CHANGE_LAST_WRITE = 0x10;
        public const int FILE_NOTIFY_CHANGE_LAST_ACCESS = 0x20;
        public const int FILE_NOTIFY_CHANGE_CREATION = 0x40;
        public const int FILE_NOTIFY_CHANGE_SECURITY = 0x100;
        public const int FILE_ACTION_ADDED = 0x1;
        public const int FILE_ACTION_REMOVED = 0x2;
        public const int FILE_ACTION_MODIFIED = 0x3;
        public const int FILE_ACTION_RENAMED_OLD_NAME = 0x4;
        public const int FILE_ACTION_RENAMED_NEW_NAME = 0x5;
        public const int MAILSLOT_NO_MESSAGE = -1;
        public const int MAILSLOT_WAIT_FOREVER = -1;
        public const int FILE_CASE_SENSITIVE_SEARCH = 0x1;
        public const int FILE_CASE_PRESERVED_NAMES = 0x2;
        public const int FILE_UNICODE_ON_DISK = 0x4;
        public const int FILE_PERSISTENT_ACLS = 0x8;
        public const int FILE_FILE_COMPRESSION = 0x10;
        public const int FILE_VOLUME_QUOTAS = 0x20;
        public const int FILE_SUPPORTS_SPARSE_FILES = 0x40;
        public const int FILE_SUPPORTS_REPARSE_POINTS = 0x80;
        public const int FILE_SUPPORTS_REMOTE_STORAGE = 0x100;
        public const int FILE_VOLUME_IS_COMPRESSED = 0x8000;
        public const int FILE_SUPPORTS_OBJECT_IDS = 0x10000;
        public const int FILE_SUPPORTS_ENCRYPTION = 0x20000;
        public const int FILE_NAMED_STREAMS = 0x40000;
        public const int FILE_READ_ONLY_VOLUME = 0x80000;
        public const int FILE_SEQUENTIAL_WRITE_ONCE = 0x100000;
        public const int FILE_SUPPORTS_TRANSACTIONS = 0x200000;
        public const int FILE_SUPPORTS_HARD_LINKS = 0x400000;
        public const int FILE_SUPPORTS_EXTENDED_ATTRIBUTES = 0x800000;
        public const int FILE_SUPPORTS_OPEN_BY_FILE_ID = 0x1000000;
        public const int FILE_SUPPORTS_USN_JOURNAL = 0x2000000;
        public const int FILE_SUPPORTS_INTEGRITY_STREAMS = 0x4000000;
        public const long FILE_INVALID_FILE_ID = -1L;

        
        // begin_1_0
        // begin_2_0
        // begin_2_1
        // /********************************************************************************
        // *                                                                               *
        // * FileApi.h -- ApiSet Contract for api-ms-win-core-file-l1                      *
        // *                                                                               *
        // * Copyright (c) Microsoft Corporation. All rights reserved.                     *
        // *                                                                               *
        // ********************************************************************************/

        //
        // Constants
        //

        public const int MAX_PATH = 260;
        public const int CREATE_NEW = 1;
        public const int CREATE_ALWAYS = 2;
        public const int OPEN_EXISTING = 3;
        public const int OPEN_ALWAYS = 4;
        public const int TRUNCATE_EXISTING = 5;

        public enum CreateDisposition
        {
            CreateNew = 1,
            CreateAlways = 2,
            OpenExisting = 3,
            OpenAlways = 4,
            TruncateExisting = 5
        }

        public const int INVALID_FILE_SIZE = unchecked((int)0xFFFFFFFF);
        public const int INVALID_SET_FILE_POINTER = -1;
        public const int INVALID_FILE_ATTRIBUTES = -1;

        public enum FINDEX_INFO_LEVELS
        {
            FindExInfoStandard,
            FindExInfoMaxInfoLevel
        }

        public enum FINDEX_SEARCH_OPS
        {
            FindExSearchNameMatch,
            FindExSearchLimitToDirectories,
            FindExSearchLimitToDevices
        }

    }


}