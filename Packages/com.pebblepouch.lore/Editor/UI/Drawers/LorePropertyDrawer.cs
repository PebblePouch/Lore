using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace PebblePouch.Lore.Editor
{
    // Entry, Category and Field fields in the user's own scripts (MonoBehaviours, ScriptableObjects, lists):
    //   Entry     "🐺 Wolf  (Characters)"   picker grouped by Category
    //   Category  "📁 Characters"
    //   Field     "❤ Health  (Integer)"
    // Searchable picker, drag & drop, click = ping, double-click = open in Lore. [LoreFilter] limits picker and
    // drag & drop. One drawer for all three: Unity uses a single drawer class for the LoreFilter attribute.
    [CustomPropertyDrawer(typeof(Entry))]
    [CustomPropertyDrawer(typeof(Category))]
    [CustomPropertyDrawer(typeof(Field))]
    [CustomPropertyDrawer(typeof(LoreFilterAttribute))]
    public sealed class LorePropertyDrawer : PropertyDrawer
    {
        #region Types
        private enum Kind
        {
            None,
            Entry,
            Category,
            Field
        }

        // The field's [LoreFilter], resolved against the current Categories.
        private readonly struct Filter
        {
            // null = any Category.
            public readonly HashSet<Category> Categories;

            // null = any type.
            public readonly FieldType? RequiredFieldType;

            public readonly IReadOnlyList<string> Hints;

            public Filter(
                HashSet<Category> categories,
                FieldType? requiredFieldType,
                IReadOnlyList<string> hints)
            {
                Categories = categories;
                RequiredFieldType = requiredFieldType;
                Hints = hints;
            }

            public bool Allows(Object value)
            {
                switch (value)
                {
                    case Entry entry:
                        return Categories == null ||
                               Categories.Contains(entry.Category);

                    case Category category:
                        return Categories == null ||
                               Categories.Contains(category);

                    case Field field:
                        return (RequiredFieldType == null || field.Type == RequiredFieldType.Value) &&
                               (Categories == null ||
                                Categories.Any(
                                    category =>
                                        category != null &&
                                        category.Fields.Contains(field)));

                    default:
                        return false;
                }
            }
        }
        #endregion

        #region PropertyDrawer
        public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label)
        {
            Kind kind = GetKind();

            if (kind == Kind.None ||
                property.propertyType != SerializedPropertyType.ObjectReference)
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            float height = EditorGUIUtility.singleLineHeight;

            string hint = GetHint(GetFilter(kind));

            if (hint != null)
            {
                height +=
                    DrawerStyle.HintSpacing +
                    DrawerStyle.Hint.CalcHeight(
                        new GUIContent(hint),
                        EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth);
            }

            return height;
        }

        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            Kind kind = GetKind();

            // [LoreFilter] on a field of another type: Unity's default field.
            if (kind == Kind.None ||
                property.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUI.PropertyField(position, property, label, true);

                return;
            }

            Filter filter = GetFilter(kind);

            Rect lineRect =
                new(
                    position.x,
                    position.y,
                    position.width,
                    EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginProperty(lineRect, label, property);

            Rect fieldRect =
                EditorGUI.PrefixLabel(
                    lineRect,
                    GUIUtility.GetControlID(FocusType.Passive),
                    label);

            DrawValue(fieldRect, property, kind, filter);

            EditorGUI.EndProperty();

            string hint = GetHint(filter);

            if (hint != null)
            {
                Rect hintRect =
                    new(
                        fieldRect.x,
                        lineRect.yMax + DrawerStyle.HintSpacing,
                        fieldRect.width,
                        position.yMax - lineRect.yMax - DrawerStyle.HintSpacing);

                GUI.Label(hintRect, hint, DrawerStyle.Hint);
            }
        }
        #endregion

        #region Draw
        private static void DrawValue(
            Rect rect,
            SerializedProperty property,
            Kind kind,
            Filter filter)
        {
            Rect buttonRect =
                new(
                    rect.xMax - DrawerStyle.PickerButtonWidth,
                    rect.y,
                    DrawerStyle.PickerButtonWidth,
                    rect.height);

            Object value = property.objectReferenceValue;

            bool missing =
                value == null &&
                SerializedReferenceUtility.IsMissingReference(property);

            HandleInput(rect, buttonRect, property, kind, filter, value);

            if (Event.current.type != EventType.Repaint)
                return;

            // GUI.Label (not style.Draw) so the tooltip shows.
            GUI.Label(
                rect,
                new GUIContent(
                    GetValueText(property, kind, value, missing),
                    value != null
                        ? LoreText.Drawers.ValueTooltip
                        : null),
                missing
                    ? DrawerStyle.MissingValue
                    : DrawerStyle.Value);

            GUIStyle buttonStyle =
                GUI.skin.FindStyle(DrawerStyle.PickerButtonStyleName) ??
                EditorStyles.miniButton;

            buttonStyle.Draw(buttonRect, GUIContent.none, false, false, false, false);
        }

        private static string GetValueText(
            SerializedProperty property,
            Kind kind,
            Object value,
            bool missing)
        {
            if (property.hasMultipleDifferentValues)
                return LoreText.Drawers.MixedValues;

            if (missing)
                return string.Format(LoreText.Drawers.Missing, GetKindName(kind));

            switch (value)
            {
                case Entry entry:
                    return string.Format(
                        LoreText.Drawers.ValueWithDetail,
                        entry.Glyph,
                        entry.Title,
                        GetCategoryTitle(entry),
                        DrawerStyle.DetailColor);

                case Category category:
                    return string.Format(
                        LoreText.Drawers.GlyphTitle,
                        category.Glyph,
                        category.Title);

                case Field field:
                    return string.Format(
                        LoreText.Drawers.ValueWithDetail,
                        field.Glyph,
                        field.Title,
                        ScriptableObjectTypeService.GetFieldTypeLabel(field),
                        DrawerStyle.DetailColor);

                default:
                    return string.Format(LoreText.Drawers.None, GetKindName(kind));
            }
        }
        #endregion

        #region Input
        private static void HandleInput(
            Rect rect,
            Rect buttonRect,
            SerializedProperty property,
            Kind kind,
            Filter filter,
            Object value)
        {
            Event current = Event.current;

            switch (current.type)
            {
                case EventType.MouseDown when current.button == 0 && buttonRect.Contains(current.mousePosition):
                    OpenPicker(buttonRect, property, kind, filter);
                    current.Use();
                    break;

                case EventType.MouseDown when current.button == 0 && rect.Contains(current.mousePosition):
                    if (value != null)
                    {
                        if (current.clickCount == 2)
                            OpenInLore(value);
                        else
                            EditorGUIUtility.PingObject(value);
                    }

                    current.Use();
                    break;

                case EventType.DragUpdated:
                case EventType.DragPerform:
                    HandleDrag(rect, property, kind, filter);
                    break;
            }
        }

        private static void HandleDrag(
            Rect rect,
            SerializedProperty property,
            Kind kind,
            Filter filter)
        {
            if (!rect.Contains(Event.current.mousePosition))
                return;

            Object dragged =
                DragAndDrop.objectReferences
                    .FirstOrDefault(
                        candidate =>
                            GetKind(candidate) == kind &&
                            filter.Allows(candidate));

            DragAndDrop.visualMode =
                dragged != null
                    ? DragAndDropVisualMode.Link
                    : DragAndDropVisualMode.Rejected;

            if (Event.current.type == EventType.DragPerform &&
                dragged != null)
            {
                DragAndDrop.AcceptDrag();

                property.objectReferenceValue = dragged;
                property.serializedObject.ApplyModifiedProperties();
            }

            Event.current.Use();
        }

        private static void OpenPicker(
            Rect buttonRect,
            SerializedProperty property,
            Kind kind,
            Filter filter)
        {
            // The property is only valid during this OnGUI call: apply the choice through its object and path.
            SerializedObject serializedObject = property.serializedObject;
            string propertyPath = property.propertyPath;

            new LorePickerDropdown(
                    GetPickerTitle(kind),
                    GetPickerGroups(kind, filter),
                    value =>
                    {
                        if (serializedObject.targetObject == null)
                            return;

                        serializedObject.Update();

                        SerializedProperty target =
                            serializedObject.FindProperty(
                                propertyPath);

                        if (target == null)
                            return;

                        target.objectReferenceValue = value;
                        serializedObject.ApplyModifiedProperties();
                    })
                .Show(buttonRect);
        }

        private static void OpenInLore(Object value)
        {
            LoreWindow.Open();

            switch (value)
            {
                case Entry entry:
                    LoreNavigation.NavigateToEntry(entry);
                    break;

                case Category category:
                    LoreNavigation.NavigateToCategory(category);
                    break;

                case Field field:
                    // After the window (and its Library host) is set up.
                    EditorApplication.delayCall += () => LibraryService.RevealField?.Invoke(field);
                    break;
            }
        }
        #endregion

        #region Picker
        private static IReadOnlyList<LorePickerDropdown.Group> GetPickerGroups(
            Kind kind,
            Filter filter)
        {
            switch (kind)
            {
                case Kind.Entry:
                    // Entries per Category, in Lore's order.
                    return LoreRepository.Instance.Groups
                        .Where(group => filter.Categories == null || filter.Categories.Contains(group.Category))
                        .Select(
                            group =>
                                new LorePickerDropdown.Group(
                                    string.Format(LoreText.Drawers.GlyphTitle, group.Glyph, group.Title),
                                    group.Entries
                                        .Where(entry => entry != null)
                                        .Select(
                                            entry =>
                                                new LorePickerDropdown.Item(
                                                    string.Format(LoreText.Drawers.GlyphTitle, entry.Glyph, entry.Title),
                                                    entry))
                                        .ToList()))
                        .ToList();

                case Kind.Category:
                    return Flat(
                        LoreRepository.Instance.Categories
                            .Where(category => category != null && filter.Allows(category))
                            .Select(
                                category =>
                                    new LorePickerDropdown.Item(
                                        string.Format(LoreText.Drawers.GlyphTitle, category.Glyph, category.Title),
                                        category)));

                case Kind.Field:
                    // All Fields, alphabetical like the Field Library.
                    return Flat(
                        new FieldLibrarySource()
                            .GetItems()
                            .Select(item => item.Field)
                            .Where(field => field != null && filter.Allows(field))
                            .OrderBy(field => field.Title, StringComparer.OrdinalIgnoreCase)
                            .Select(
                                field =>
                                    new LorePickerDropdown.Item(
                                        string.Format(
                                            LoreText.Drawers.ItemWithDetail,
                                            field.Glyph,
                                            field.Title,
                                            ScriptableObjectTypeService.GetFieldTypeLabel(field)),
                                        field)));

                default:
                    return Array.Empty<LorePickerDropdown.Group>();
            }
        }

        private static IReadOnlyList<LorePickerDropdown.Group> Flat(IEnumerable<LorePickerDropdown.Item> items)
        {
            return new[]
            {
                new LorePickerDropdown.Group(
                    null,
                    items.ToList())
            };
        }

        private static string GetPickerTitle(Kind kind)
        {
            return kind switch
            {
                Kind.Entry => LoreText.Drawers.EntryPickerTitle,
                Kind.Category => LoreText.Drawers.CategoryPickerTitle,
                Kind.Field => LoreText.Drawers.FieldPickerTitle,
                _ => string.Empty
            };
        }
        #endregion

        #region Filter
        private Kind GetKind()
        {
            Type type = fieldInfo?.FieldType;

            if (type == null)
                return Kind.None;

            // Lists and arrays: the drawer runs per element.
            Type elementType =
                type.IsArray
                    ? type.GetElementType()
                    : type.IsGenericType
                        ? type.GetGenericArguments().FirstOrDefault()
                        : type;

            return GetKind(elementType);
        }

        private static Kind GetKind(Object value) =>
            value != null
                ? GetKind(value.GetType())
                : Kind.None;

        private static Kind GetKind(Type type)
        {
            if (type == null)
                return Kind.None;

            if (typeof(Entry).IsAssignableFrom(type))
                return Kind.Entry;

            if (typeof(Category).IsAssignableFrom(type))
                return Kind.Category;

            if (typeof(Field).IsAssignableFrom(type))
                return Kind.Field;

            return Kind.None;
        }

        private Filter GetFilter(Kind kind)
        {
            var filterAttribute =
                fieldInfo?
                    .GetCustomAttributes(typeof(LoreFilterAttribute), true)
                    .FirstOrDefault() as LoreFilterAttribute;

            if (filterAttribute == null)
            {
                return new Filter(
                    null,
                    null,
                    Array.Empty<string>());
            }

            var hints = new List<string>();

            HashSet<Category> categories =
                ResolveCategories(
                    filterAttribute.GetCategories(),
                    hints);

            FieldType? fieldType = null;

            if (filterAttribute.HasFieldType)
            {
                if (kind == Kind.Field)
                {
                    fieldType = filterAttribute.FieldType;
                }
                else
                {
                    hints.Add(
                        string.Format(
                            LoreText.Drawers.CriterionNotApplicable,
                            LoreText.Drawers.FieldTypeCriterion,
                            GetKindName(kind)));
                }
            }

            return new Filter(
                categories,
                fieldType,
                hints);
        }

        // null when the filter has no Category criterion; unknown titles become hints.
        private static HashSet<Category> ResolveCategories(
            IReadOnlyList<string> titles,
            List<string> hints)
        {
            if (titles.Count == 0)
                return null;

            var categories = new HashSet<Category>();

            foreach (string title in titles)
            {
                Category category =
                    LoreRepository.Instance.Categories.FirstOrDefault(
                        candidate =>
                            candidate != null &&
                            string.Equals(
                                candidate.Title,
                                title,
                                StringComparison.OrdinalIgnoreCase));

                if (category != null)
                {
                    categories.Add(category);
                }
                else
                {
                    hints.Add(
                        string.Format(
                            LoreText.Drawers.UnknownCategory,
                            title));
                }
            }

            return categories;
        }

        private static string GetHint(Filter filter)
        {
            return filter.Hints == null || filter.Hints.Count == 0
                ? null
                : string.Join("\n", filter.Hints);
        }

        private static string GetKindName(Kind kind)
        {
            return kind switch
            {
                Kind.Entry => LoreText.Drawers.EntryKind,
                Kind.Category => LoreText.Drawers.CategoryKind,
                Kind.Field => LoreText.Drawers.FieldKind,
                _ => string.Empty
            };
        }

        private static string GetCategoryTitle(Entry entry)
        {
            return entry.Category != null
                ? entry.Category.Title
                : LoreText.Common.Uncategorized;
        }
        #endregion
    }
}
