using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace AuraLedHelper.Service
{
    internal static class WtsHelper
    {
        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern bool WTSQueryUserToken(UInt32 sessionId, out IntPtr Token);

        [DllImport("kernel32.dll")]
        private static extern UInt32 WTSGetActiveConsoleSessionId();

        enum TOKEN_INFORMATION_CLASS
        {
            TokenUser = 1,
            TokenGroups,
            TokenPrivileges,
            TokenOwner,
            TokenPrimaryGroup,
            TokenDefaultDacl,
            TokenSource,
            TokenType,
            TokenImpersonationLevel,
            TokenStatistics,
            TokenRestrictedSids,
            TokenSessionId,
            TokenGroupsAndPrivileges,
            TokenSessionReference,
            TokenSandBoxInert,
            TokenAuditPolicy,
            TokenOrigin
        }

        struct TOKEN_USER
        {
#pragma warning disable 649
            public SID_AND_ATTRIBUTES User;
#pragma warning restore 649
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SID_AND_ATTRIBUTES
        {
            public IntPtr Sid;
            public int Attributes;
        }

        // Using IntPtr for pSID insted of Byte[]
        [DllImport("advapi32", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool ConvertSidToStringSid(
            IntPtr pSID,
            out IntPtr ptrSid);

        [DllImport("kernel32.dll")]
        static extern IntPtr LocalFree(IntPtr hMem);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool GetTokenInformation(
            IntPtr TokenHandle,
            TOKEN_INFORMATION_CLASS TokenInformationClass,
            IntPtr TokenInformation,
            int TokenInformationLength,
            out int ReturnLength);

        private static string GetSID(IntPtr token)
        {
            bool Result;

            int TokenInfLength = 0;
            string sidAsString = String.Empty;

            // first call gets lenght of TokenInformation
            Result = GetTokenInformation(token, TOKEN_INFORMATION_CLASS.TokenUser, IntPtr.Zero, TokenInfLength, out TokenInfLength);

            IntPtr TokenInformation = Marshal.AllocHGlobal(TokenInfLength);
            Result = GetTokenInformation(token, TOKEN_INFORMATION_CLASS.TokenUser, TokenInformation, TokenInfLength, out TokenInfLength);

            if (Result)
            {
                TOKEN_USER TokenUser = (TOKEN_USER)Marshal.PtrToStructure(TokenInformation, typeof(TOKEN_USER));

                IntPtr pstr = IntPtr.Zero;
                Boolean ok = ConvertSidToStringSid(TokenUser.User.Sid, out pstr);

                sidAsString = Marshal.PtrToStringAuto(pstr);
                LocalFree(pstr);
            }

            Marshal.FreeHGlobal(TokenInformation);

            return sidAsString;
        }

        public static string GetCurrentAccountName()
        {
            var sid = GetCurrentAccountSID();
            if (sid == null) return null;
            return new SecurityIdentifier(sid).Translate(typeof(NTAccount)).ToString();
        }

        public static string GetCurrentAccountSID()
        {
            IntPtr token = IntPtr.Zero;

            var id = WTSGetActiveConsoleSessionId();
            if (id == 0)
            {
                return null;
            }
            if (WTSQueryUserToken(id, out token))
            {
                return GetSID(token);
            }
            else
            {
                int err = Marshal.GetLastWin32Error();
                return null;
            }
        }
    }
}
