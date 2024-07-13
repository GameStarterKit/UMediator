using UnityEditor;
using UnityEngine;

namespace Packages.UMediator.Editor
{
    internal static class MenuItems 
    {
        [MenuItem("Window/UniMediator/Documentation")]
        internal static void ViewDocumentation()
        {
            Application.OpenURL("https://github.com/tharinga/UniMediator");
        }
        
        [MenuItem("Window/UniMediator/Report Issue...")]
        internal static void ReportIssue()
        {
            Application.OpenURL("https://github.com/tharinga/UniMediator/issues");
        }
    }
}

