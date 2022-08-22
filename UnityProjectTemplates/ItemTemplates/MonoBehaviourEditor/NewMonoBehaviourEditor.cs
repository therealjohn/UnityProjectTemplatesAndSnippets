using UnityEngine;
using UnityEngine.Editor;
using System.Collections;

namespace $rootnamespace$
{
    [CustomEditor(nameof($safeitemname$))]
    public class $safeitemname$Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the GUI unity generates normally
            DrawDefaultInspector();
        }
    }
}