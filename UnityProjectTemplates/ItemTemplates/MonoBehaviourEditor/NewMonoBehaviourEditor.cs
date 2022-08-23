﻿using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace $rootnamespace$
{
    [CustomEditor(nameof($safeitemname$))]
    public class $safeitemname$Editor : Editor
    {
        $safeitemname$ self;

        public override VisualElement CreateInspectorGUI()
        {
            self = target as $safeitemname$;
            return base.CreateInspectorGUI();
        }

        public override void OnInspectorGUI()
        {
            // Draw the GUI unity generates normally
            DrawDefaultInspector();
        }
    }
}
