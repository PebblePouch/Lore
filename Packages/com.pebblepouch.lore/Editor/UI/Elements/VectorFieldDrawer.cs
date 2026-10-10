using System;

using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    // Vector values as "X [1.5]  Y [2]  Z [0]": one number field per component, each with its own control name
    // (controlName + index), so focus handling and Tabular Editing's cell editing work as for Float values.
    // Unity's Vector3Field cannot name its inner fields.
    public static class VectorFieldDrawer
    {
        #region Public API
        // Returns the index of the changed component (-1: none) and its new value. delayed: applies on Enter or
        // focus loss. mixed(i): shows "—" for that component (null: keeps EditorGUI.showMixedValue).
        public static int Draw(
            Rect rect,
            int dimension,
            Vector3 value,
            string controlName,
            bool delayed,
            Func<int, bool> mixed,
            out float changedValue)
        {
            changedValue = 0f;

            int changed = -1;

            float width =
                Mathf.Max(
                    0f,
                    (rect.width - InspectorStyle.FieldValueVectorSpacing * (dimension - 1)) / dimension);

            bool previousMixed =
                EditorGUI.showMixedValue;

            for (int i = 0; i < dimension; i++)
            {
                Rect labelRect =
                    new(
                        rect.x + i * (width + InspectorStyle.FieldValueVectorSpacing),
                        rect.y,
                        Mathf.Min(InspectorStyle.FieldValueVectorLabelWidth, width),
                        rect.height);

                Rect fieldRect =
                    new(
                        labelRect.xMax,
                        rect.y,
                        Mathf.Max(0f, width - labelRect.width),
                        rect.height);

                GUI.Label(
                    labelRect,
                    LoreText.FieldTypes.VectorComponents[i],
                    InspectorStyle.FieldValueVectorLabel);

                string componentName =
                    controlName + i;

                EditorGUI.showMixedValue =
                    mixed != null
                        ? mixed(i)
                        : previousMixed;

                GUI.SetNextControlName(
                    componentName);

                EditorGUI.BeginChangeCheck();

                float number =
                    delayed
                        ? EditorGUI.DelayedFloatField(
                            fieldRect,
                            value[i],
                            InspectorStyle.FieldValueNumber)
                        : EditorGUI.FloatField(
                            fieldRect,
                            value[i],
                            InspectorStyle.FieldValueNumber);

                if (EditorGUI.EndChangeCheck())
                {
                    changed = i;
                    changedValue = number;
                }

                // While focused, the input reports its rect (see FieldValueElement.Draw).
                LoreEditService.ReportEditor(
                    componentName,
                    fieldRect,
                    () => GUI.GetNameOfFocusedControl() == componentName);
            }

            EditorGUI.showMixedValue = previousMixed;

            return changed;
        }
        #endregion
    }
}
