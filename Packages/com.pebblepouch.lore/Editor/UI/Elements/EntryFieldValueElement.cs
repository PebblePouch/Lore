using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public sealed class EntryFieldValueElement
    {
        #region Fields
        private Entry _entry;
        private FieldValue _fieldValue;

        private readonly FieldValueTextElement _textElement = new();
        private readonly FieldValueIntegerElement _integerElement = new();
        private readonly FieldValueFloatElement _floatElement = new();
        private readonly FieldValueBooleanElement _booleanElement = new();
        private readonly FieldValueEntryElement _entryElement = new();
        private readonly FieldValueGameObjectElement _objectElement = new();
        private readonly FieldValueSpriteElement _spriteElement = new();
        private readonly FieldValueSceneElement _sceneElement = new();
        private readonly FieldValueFolderElement _folderElement = new();
        private readonly FieldValueUrlElement _urlElement = new();
        private readonly FieldValueScriptableObjectElement _scriptableObjectElement = new();
        private readonly FieldValueEnumElement _enumElement = new();
        private readonly FieldValueEnumsElement _enumsElement = new();
        private readonly FieldValueEntriesElement _entriesElement = new();
        private readonly FieldValueAudioElement _audioElement = new();
        private readonly FieldValueColorElement _colorElement = new();
        private readonly FieldValueVectorElement _vectorElement = new();
        #endregion

        #region Public API
        // Tabular Editing: Entry values use the compact card.
        public bool CompactEntryCard
        {
            set => _entryElement.Compact = value;
        }

        public void SetEntry(Entry entry)
        {
            _entry = entry;
        }

        public void SetFieldValue(FieldValue fieldValue)
        {
            _fieldValue = fieldValue;
        }

        public void Draw(
            Rect rect)
        {
            if (_fieldValue == null ||
                _fieldValue.Field == null)
            {
                return;
            }

            DrawContent(
                rect);
        }
        #endregion

        #region Draw
        private void DrawContent(
            Rect rect)
        {
            switch (_fieldValue.Field.Type)
            {
                case FieldType.Text:
                    _textElement.SetEntry(_entry);
                    _textElement.SetFieldValue(_fieldValue);
                    _textElement.Draw(rect);
                    break;

                case FieldType.Integer:
                    _integerElement.SetEntry(_entry);
                    _integerElement.SetFieldValue(_fieldValue);
                    _integerElement.Draw(rect);
                    break;

                case FieldType.Float:
                    _floatElement.SetEntry(_entry);
                    _floatElement.SetFieldValue(_fieldValue);
                    _floatElement.Draw(rect);
                    break;

                case FieldType.Boolean:
                    _booleanElement.SetEntry(_entry);
                    _booleanElement.SetFieldValue(_fieldValue);
                    _booleanElement.Draw(rect);
                    break;

                case FieldType.Entry:
                    _entryElement.SetEntry(_entry);
                    _entryElement.SetFieldValue(_fieldValue);
                    _entryElement.Draw(rect);
                    break;

                case FieldType.GameObject:
                    _objectElement.SetEntry(_entry);
                    _objectElement.SetFieldValue(_fieldValue);
                    _objectElement.Draw(rect);
                    break;

                case FieldType.Sprite:
                    _spriteElement.SetEntry(_entry);
                    _spriteElement.SetFieldValue(_fieldValue);
                    _spriteElement.Draw(rect);
                    break;

                case FieldType.Scene:
                    _sceneElement.SetEntry(_entry);
                    _sceneElement.SetFieldValue(_fieldValue);
                    _sceneElement.Draw(rect);
                    break;

                case FieldType.Folder:
                    _folderElement.SetEntry(_entry);
                    _folderElement.SetFieldValue(_fieldValue);
                    _folderElement.Draw(rect);
                    break;

                case FieldType.Url:
                    _urlElement.SetEntry(_entry);
                    _urlElement.SetFieldValue(_fieldValue);
                    _urlElement.Draw(rect);
                    break;

                case FieldType.ScriptableObject:
                case FieldType.Object:
                    _scriptableObjectElement.SetEntry(_entry);
                    _scriptableObjectElement.SetFieldValue(_fieldValue);
                    _scriptableObjectElement.Draw(rect);
                    break;

                case FieldType.Enum:
                    _enumElement.SetEntry(_entry);
                    _enumElement.SetFieldValue(_fieldValue);
                    _enumElement.Draw(rect);
                    break;

                case FieldType.Enums:
                    _enumsElement.SetEntry(_entry);
                    _enumsElement.SetFieldValue(_fieldValue);
                    _enumsElement.Draw(rect);
                    break;

                case FieldType.Entries:
                    _entriesElement.SetEntry(_entry);
                    _entriesElement.SetFieldValue(_fieldValue);
                    _entriesElement.Draw(rect);
                    break;

                case FieldType.Audio:
                    _audioElement.SetEntry(_entry);
                    _audioElement.SetFieldValue(_fieldValue);
                    _audioElement.Draw(rect);
                    break;

                case FieldType.Color:
                    _colorElement.SetEntry(_entry);
                    _colorElement.SetFieldValue(_fieldValue);
                    _colorElement.Draw(rect);
                    break;

                case FieldType.Vector:
                    _vectorElement.SetEntry(_entry);
                    _vectorElement.SetFieldValue(_fieldValue);
                    _vectorElement.Draw(rect);
                    break;
            }
        }
        #endregion
    }
}