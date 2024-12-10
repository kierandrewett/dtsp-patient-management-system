using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace PMS.Controllers
{
    public enum LogCategory
    {
        DBORMReflection,
        DBORM,
        DB,

        ContextualForm,
        DataViewReflection,
        DataManager,

        PermissionController,
        SearchController,
        AccessibilityController,
        SynthesizerController,

        PasswordChangeWindow,
        SecurityQuestionsSSWindow,
        MainWindow,

    }

    public class LogController
    {
        public static string GetPrefixName(LogCategory prefix)
        {
            switch (prefix)
            {
                case LogCategory.DBORMReflection:
                    return "Database ORM Reflection";
                case LogCategory.DBORM:
                    return "Database ORM";
                case LogCategory.DB:
                    return "Database";
                case LogCategory.ContextualForm:
                    return "Contextual Form";
                case LogCategory.DataViewReflection:
                    return "Data View Reflection";
                case LogCategory.DataManager:
                    return "Data Manager";
                case LogCategory.PermissionController:
                    return "Permission Controller";
                case LogCategory.SearchController:
                    return "Search Controller";
                case LogCategory.AccessibilityController:
                    return "Accessibility Controller";
                case LogCategory.SynthesizerController:
                    return "Synthesizer Controller";
                case LogCategory.PasswordChangeWindow:
                    return "Password Change Window";
                case LogCategory.SecurityQuestionsSSWindow:
                    return "Security Questions Setup Window";
                case LogCategory.MainWindow:
                    return "Main Window";
                default:
                    return "";
            }
        }

        public static void WriteLine(string message, LogCategory? prefix = null)
        {
            string logMessage = "";

            if (prefix != null)
            {
                string prefixName = LogController.GetPrefixName((LogCategory)prefix!);

                if (!prefixName.IsNullOrEmpty())
                {
                    logMessage += $"({prefixName}): ";
                }
            }

            logMessage += message;

            Debug.WriteLine(logMessage);
        }
    }
}
