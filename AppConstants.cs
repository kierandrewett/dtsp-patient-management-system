using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PMS
{
    class AppConstants
    {
        #if DEBUG
            public static bool IsDebug = true;
        #else
            public static bool IsDebug = false;
        #endif

        public static string AppFullName = (string)Application.Current.Resources["AppFullName"];
        public static string AppName = (string)Application.Current.Resources["AppName"];
        public static string AppBuildEnv
        {
            get => AppConstants.IsDebug ? "Debug" : "Release";
        }

        public static string AppVersion
        {
            get
            {
                Version? ver = Assembly.GetExecutingAssembly().GetName().Version;
                return ver != null ? ver.ToString() : "0.0.0";
            }
        }
        public static string AppComputedTitle
        {
            get => $"{AppFullName} ({AppBuildEnv}) - Version: {AppVersion}";
        }

        public static int MaximumFailedLoginAttempts = (int)Application.Current.Resources["MaximumFailedLoginAttempts"];
        public static int FailedLoginsTimeoutMins = (int)Application.Current.Resources["FailedLoginsTimeoutMins"];

        public static string UnauthorisedMessage = (string)Application.Current.Resources["UnauthorisedMessage"];

        public static int PasswordMinLength = (int)Application.Current.Resources["PasswordMinLength"];
    }
}
