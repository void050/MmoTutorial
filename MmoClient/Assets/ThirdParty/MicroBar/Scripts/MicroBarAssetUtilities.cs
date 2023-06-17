#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// ****************************************************************************************************
// Utility functions for managing assets (search assets...)
// ****************************************************************************************************
namespace Microlight.MicroBar {
    public static class MicroBarAssetUtilities {
        /// <summary>
        /// Returns GameObject from Assets folder with specified name
        /// Can return more than one if there is prefab with same name 
        /// or has same name in it (Script and ScriptTwo) will return both
        /// </summary>
        /// <param name="prefabName">Name of the prefab</param>
        public static GameObject GetPrefab(string prefabName) {
            // Find prefab
            string[] guids = AssetDatabase.FindAssets($"t:prefab " + prefabName);
            if(guids == null || guids.Length == 0) {
                Debug.LogWarning("Microlight.AssetUtility: Can't find prefab.");
                return null;
            }
            if(guids.Length > 1) {
                Debug.LogWarning("Microlight.AssetUtility: There are multiple " + prefabName + " prefabs. Found: " + guids.Length);
                return null;
            }
            return AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[0]));   // Return it as GameObject
        }
    }
}
#endif