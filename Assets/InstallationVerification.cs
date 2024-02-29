using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Needle.AutoInstaller
{
    internal static class InstallationVerification
    {
        internal const string TargetPath = "Assets/InstallationVerification.cs";
        [InitializeOnLoadMethod]
        private static void VerifyInstallation()
        {
            initialCheckTime = DateTime.Now;
            EditorApplication.update += WaitForPackageInstallation;
        }
        
        private static EditorWindow focussedWindow;
        private static DateTime initialCheckTime;
        private static void WaitForPackageInstallation()
        {
            // maximum wait time until verification
            if (DateTime.Now - initialCheckTime > TimeSpan.FromSeconds(15))
            {
                CheckAndLogInstallation();
                return;
            }
            
            if (EditorWindow.focusedWindow == focussedWindow) return;
            focussedWindow = EditorWindow.focusedWindow;
            
            // Triggered by Package Manager scoped registry verification. We're logging the message instead.
            var settingsWindows = Resources.FindObjectsOfTypeAll(typeof(EditorWindow)).Where(x => x.GetType().Name == "ProjectSettingsWindow").Cast<EditorWindow>().ToList();
            if (!settingsWindows.Contains(focussedWindow)) return;
            
            foreach (var wnd in settingsWindows)
                wnd.Close();
            
            CheckAndLogInstallation();
            
            void CheckAndLogInstallation()
            {
                EditorApplication.update -= WaitForPackageInstallation;            
                if (SceneView.lastActiveSceneView) SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Package installation complete ❤"));

                /*
                if (!Data || Data.packages == null) return;
                
                // check that the intended packages are now installed
                var packagesAreInstalled = Data.packages.All(x => File.Exists("Packages/" + x.name + "/package.json"));
                
                if (packagesAreInstalled)
                    Debug.Log(LogPrefix + "Added required package registries and packages to manifest. You're all set!");
                else
                    Debug.LogWarning(LogPrefix + "Can't verify installation. Please check the console for more information.");
                */
                
                if (File.Exists(TargetPath))
                    File.Delete(TargetPath);

                if (File.Exists(TargetPath + ".meta"))
                    File.Delete(TargetPath + ".meta");
                
                AssetDatabase.Refresh();
            }
        }

    }
}