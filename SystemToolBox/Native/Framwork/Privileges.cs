using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;

namespace SystemToolBox.Native.Framwork
{
    public static class Privileges
    {
        /// <summary>
        ///     Gets the specified permission
        /// </summary>
        /// <param name="securityEntity"></param>
        /// <returns></returns>
        internal static bool EnablePrivilege(NativeConstants.SecurityEntity securityEntity)
        {
            if (!Enum.IsDefined(typeof(NativeConstants.SecurityEntity), securityEntity))
                throw new InvalidEnumArgumentException("securityEntity", (int) securityEntity,
                    typeof(NativeConstants.SecurityEntity));

            var securityEntityValue = securityEntity.GetSecurityEntityValue();

            try
            {
                var locallyUniqueIdentifier = new NativeStructure.Luid();

                if (NativeMethods.LookupPrivilegeValue(null, securityEntityValue, ref locallyUniqueIdentifier))
                {
                    var tokenPrivileges = new NativeStructure.TokenPrivileges
                    {
                        PrivilegeCount = 1,
                        Attributes = NativeConstants.SE_PRIVILEGE_ENABLED,
                        Luid = locallyUniqueIdentifier
                    };

                    var hToken = IntPtr.Zero;
                    try
                    {
                        var hCurrentProcess = NativeMethods.GetCurrentProcess();
                        if (NativeMethods.OpenProcessToken(hCurrentProcess,
                            NativeConstants.TOKEN_ADJUST_PRIVILEGES | NativeConstants.TOKEN_QUERY,
                            out hToken))
                        {
                            if (NativeMethods.AdjustTokenPrivileges(hToken, false, ref tokenPrivileges,
                                1024, IntPtr.Zero, IntPtr.Zero))
                            {
                                var lastError = Marshal.GetLastWin32Error();
                                if (NativeConstants.ERROR_NOT_ALL_ASSIGNED == lastError)
                                {
                                    var win32Exception = new Win32Exception();
                                    throw new InvalidOperationException("AdjustTokenPrivileges failed.",
                                        win32Exception);
                                }

                                return true;
                            }
                            else
                            {
                                var win32Exception = new Win32Exception();
                                throw new InvalidOperationException("AdjustTokenPrivileges failed.", win32Exception);
                            }
                        }
                        else
                        {
                            var win32Exception = new Win32Exception();

                            var exceptionMessage = string.Format(CultureInfo.InvariantCulture,
                                "OpenProcessToken failed. CurrentProcess: {0}",
                                hCurrentProcess.ToInt32());

                            throw new InvalidOperationException(exceptionMessage, win32Exception);
                        }
                    }
                    finally
                    {
                        if (IntPtr.Zero != hToken)
                            NativeMethods.CloseHandle(hToken);
                    }
                }
                {
                    var win32Exception = new Win32Exception();

                    var exceptionMessage = string.Format(CultureInfo.InvariantCulture,
                        "LookupPrivilegeValue failed. SecurityEntityValue: {0}",
                        securityEntityValue);

                    throw new InvalidOperationException(exceptionMessage, win32Exception);
                }
            }
            catch (Exception e)
            {
                var exceptionMessage = string.Format(CultureInfo.InvariantCulture,
                    "GrandPrivilege failed. SecurityEntity: {0}",
                    securityEntity);

                throw new InvalidOperationException(exceptionMessage, e);
            }
        }

        /// <summary>
        ///     Gets the security entity value.
        /// </summary>
        /// <param name="securityEntity">The security entity.</param>
        private static string GetSecurityEntityValue(this NativeConstants.SecurityEntity securityEntity)
        {
            switch (securityEntity)
            {
                case NativeConstants.SecurityEntity.SE_ASSIGNPRIMARYTOKEN_NAME:
                    return "SeAssignPrimaryTokenPrivilege";
                case NativeConstants.SecurityEntity.SE_AUDIT_NAME:
                    return "SeAuditPrivilege";
                case NativeConstants.SecurityEntity.SE_BACKUP_NAME:
                    return "SeBackupPrivilege";
                case NativeConstants.SecurityEntity.SE_CHANGE_NOTIFY_NAME:
                    return "SeChangeNotifyPrivilege";
                case NativeConstants.SecurityEntity.SE_CREATE_GLOBAL_NAME:
                    return "SeCreateGlobalPrivilege";
                case NativeConstants.SecurityEntity.SE_CREATE_PAGEFILE_NAME:
                    return "SeCreatePagefilePrivilege";
                case NativeConstants.SecurityEntity.SE_CREATE_PERMANENT_NAME:
                    return "SeCreatePermanentPrivilege";
                case NativeConstants.SecurityEntity.SE_CREATE_SYMBOLIC_LINK_NAME:
                    return "SeCreateSymbolicLinkPrivilege";
                case NativeConstants.SecurityEntity.SE_CREATE_TOKEN_NAME:
                    return "SeCreateTokenPrivilege";
                case NativeConstants.SecurityEntity.SE_DEBUG_NAME:
                    return "SeDebugPrivilege";
                case NativeConstants.SecurityEntity.SE_ENABLE_DELEGATION_NAME:
                    return "SeEnableDelegationPrivilege";
                case NativeConstants.SecurityEntity.SE_IMPERSONATE_NAME:
                    return "SeImpersonatePrivilege";
                case NativeConstants.SecurityEntity.SE_INC_BASE_PRIORITY_NAME:
                    return "SeIncreaseBasePriorityPrivilege";
                case NativeConstants.SecurityEntity.SE_INCREASE_QUOTA_NAME:
                    return "SeIncreaseQuotaPrivilege";
                case NativeConstants.SecurityEntity.SE_INC_WORKING_SET_NAME:
                    return "SeIncreaseWorkingSetPrivilege";
                case NativeConstants.SecurityEntity.SE_LOAD_DRIVER_NAME:
                    return "SeLoadDriverPrivilege";
                case NativeConstants.SecurityEntity.SE_LOCK_MEMORY_NAME:
                    return "SeLockMemoryPrivilege";
                case NativeConstants.SecurityEntity.SE_MACHINE_ACCOUNT_NAME:
                    return "SeMachineAccountPrivilege";
                case NativeConstants.SecurityEntity.SE_MANAGE_VOLUME_NAME:
                    return "SeManageVolumePrivilege";
                case NativeConstants.SecurityEntity.SE_PROF_SINGLE_PROCESS_NAME:
                    return "SeProfileSingleProcessPrivilege";
                case NativeConstants.SecurityEntity.SE_RELABEL_NAME:
                    return "SeRelabelPrivilege";
                case NativeConstants.SecurityEntity.SE_REMOTE_SHUTDOWN_NAME:
                    return "SeRemoteShutdownPrivilege";
                case NativeConstants.SecurityEntity.SE_RESTORE_NAME:
                    return "SeRestorePrivilege";
                case NativeConstants.SecurityEntity.SE_SECURITY_NAME:
                    return "SeSecurityPrivilege";
                case NativeConstants.SecurityEntity.SE_SHUTDOWN_NAME:
                    return "SeShutdownPrivilege";
                case NativeConstants.SecurityEntity.SE_SYNC_AGENT_NAME:
                    return "SeSyncAgentPrivilege";
                case NativeConstants.SecurityEntity.SE_SYSTEM_ENVIRONMENT_NAME:
                    return "SeSystemEnvironmentPrivilege";
                case NativeConstants.SecurityEntity.SE_SYSTEM_PROFILE_NAME:
                    return "SeSystemProfilePrivilege";
                case NativeConstants.SecurityEntity.SE_SYSTEMTIME_NAME:
                    return "SeSystemtimePrivilege";
                case NativeConstants.SecurityEntity.SE_TAKE_OWNERSHIP_NAME:
                    return "SeTakeOwnershipPrivilege";
                case NativeConstants.SecurityEntity.SE_TCB_NAME:
                    return "SeTcbPrivilege";
                case NativeConstants.SecurityEntity.SE_TIME_ZONE_NAME:
                    return "SeTimeZonePrivilege";
                case NativeConstants.SecurityEntity.SE_TRUSTED_CREDMAN_ACCESS_NAME:
                    return "SeTrustedCredManAccessPrivilege";
                case NativeConstants.SecurityEntity.SE_UNDOCK_NAME:
                    return "SeUndockPrivilege";
                default:
                    throw new ArgumentOutOfRangeException(typeof(NativeConstants.SecurityEntity).Name);
            }
        }
    }
}