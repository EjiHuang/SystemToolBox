using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SystemToolBox.Native
{
    /// <summary>
    /// 常量定义
    /// </summary>
    public partial class NativeConstants
    {
        #region 零碎的常量

        internal const int SE_PRIVILEGE_ENABLED = 0x00000002;

        internal const int ERROR_NOT_ALL_ASSIGNED = 1300;

        internal const UInt32 STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        internal const UInt32 STANDARD_RIGHTS_READ = 0x00020000;
        internal const UInt32 TOKEN_ASSIGN_PRIMARY = 0x0001;
        internal const UInt32 TOKEN_DUPLICATE = 0x0002;
        internal const UInt32 TOKEN_IMPERSONATE = 0x0004;
        internal const UInt32 TOKEN_QUERY = 0x0008;
        internal const UInt32 TOKEN_QUERY_SOURCE = 0x0010;
        internal const UInt32 TOKEN_ADJUST_PRIVILEGES = 0x0020;
        internal const UInt32 TOKEN_ADJUST_GROUPS = 0x0040;
        internal const UInt32 TOKEN_ADJUST_DEFAULT = 0x0080;
        internal const UInt32 TOKEN_ADJUST_SESSIONID = 0x0100;
        internal const UInt32 TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);
        internal const UInt32 TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED |
                            TOKEN_ASSIGN_PRIMARY |
                            TOKEN_DUPLICATE |
                            TOKEN_IMPERSONATE |
                            TOKEN_QUERY |
                            TOKEN_QUERY_SOURCE |
                            TOKEN_ADJUST_PRIVILEGES |
                            TOKEN_ADJUST_GROUPS |
                            TOKEN_ADJUST_DEFAULT |
                            TOKEN_ADJUST_SESSIONID);

        #endregion


        // http://msdn.microsoft.com/en-us/library/bb530716(VS.85).aspx
        public enum SecurityEntity
        {
            SE_CREATE_TOKEN_NAME,
            SE_ASSIGNPRIMARYTOKEN_NAME,
            SE_LOCK_MEMORY_NAME,
            SE_INCREASE_QUOTA_NAME,
            SE_UNSOLICITED_INPUT_NAME,
            SE_MACHINE_ACCOUNT_NAME,
            SE_TCB_NAME,
            SE_SECURITY_NAME,
            SE_TAKE_OWNERSHIP_NAME,
            SE_LOAD_DRIVER_NAME,
            SE_SYSTEM_PROFILE_NAME,
            SE_SYSTEMTIME_NAME,
            SE_PROF_SINGLE_PROCESS_NAME,
            SE_INC_BASE_PRIORITY_NAME,
            SE_CREATE_PAGEFILE_NAME,
            SE_CREATE_PERMANENT_NAME,
            SE_BACKUP_NAME,
            SE_RESTORE_NAME,
            SE_SHUTDOWN_NAME,
            SE_DEBUG_NAME,
            SE_AUDIT_NAME,
            SE_SYSTEM_ENVIRONMENT_NAME,
            SE_CHANGE_NOTIFY_NAME,
            SE_REMOTE_SHUTDOWN_NAME,
            SE_UNDOCK_NAME,
            SE_SYNC_AGENT_NAME,
            SE_ENABLE_DELEGATION_NAME,
            SE_MANAGE_VOLUME_NAME,
            SE_IMPERSONATE_NAME,
            SE_CREATE_GLOBAL_NAME,
            SE_CREATE_SYMBOLIC_LINK_NAME,
            SE_INC_WORKING_SET_NAME,
            SE_RELABEL_NAME,
            SE_TIME_ZONE_NAME,
            SE_TRUSTED_CREDMAN_ACCESS_NAME
        }

        public enum ExitWindowsAction : uint
        {
            EWX_LOGOFF = 0,
            EWX_SHUTDOWN = 1,
            EWX_REBOOT = 2,
            EWX_FORCE = 4,
            EWX_POWEROFF = 8
        }
    }
}
