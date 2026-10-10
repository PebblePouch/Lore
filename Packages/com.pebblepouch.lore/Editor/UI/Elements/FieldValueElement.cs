using UnityEditor;
using UnityEngine;

namespace PebblePouch.Lore.Editor
{
    public abstract class FieldValueElement
    {
        #region Fields
        // Control names of text inputs: prefix + Entry id + Field id (Tabular Editing detects cell edits by it).
        public const string ControlNamePrefix = "FieldValue_";

        protected Entry _entry;
        protected FieldValue _fieldValue;
        #endregion

        #region Public API
        public virtual void SetEntry(Entry entry)
        {
            _entry = entry;
        }

        public virtual void SetFieldValue(FieldValue fieldValue)
        {
            _fieldValue = fieldValue;
        }

        public void Draw(
            Rect rect)
        {
            if (_fieldValue == null)
                return;

            if (!HasTextInput)
            {
                DrawContent(
                    rect);

                return;
            }

            // Text inputs get a unique control name per Entry and Field. While focused, they report their rect
            // so the window keeps keyboard focus for clicks inside them (caret placement instead of select-all).
            // One element instance draws all rows, so the control name (not this) identifies the input.
            string controlName = GetControlName();

            GUI.SetNextControlName(
                controlName);

            DrawContent(
                rect);

            LoreEditService.ReportEditor(
                controlName,
                rect,
                () => GUI.GetNameOfFocusedControl() == controlName);
        }
        #endregion

        #region Draw
        // True for elements whose DrawContent draws a single EditorGUI text-based field (text, numbers)
        // as its first control.
        protected virtual bool HasTextInput => false;

        protected abstract void DrawContent(
            Rect rect);
        #endregion

        #region Helpers
        protected string GetControlName()
        {
            string entryId =
                _entry != null
                    ? _entry.Id
                    : string.Empty;

            string fieldId =
                _fieldValue.Field != null
                    ? _fieldValue.Field.Id
                    : string.Empty;

            return $"{ControlNamePrefix}{entryId}_{fieldId}";
        }

        // Call before changing the value (undo step). Text inputs change on every keystroke: typing in one input
        // becomes one step.
        protected void RecordChange()
        {
            if (_entry == null)
                return;

            if (HasTextInput)
            {
                LoreUndo.RecordTyping(
                    _entry,
                    LoreText.Undo.EditFieldValue,
                    GetControlName());

                return;
            }

            LoreUndo.Record(
                _entry,
                LoreText.Undo.EditFieldValue);
        }

        // Call before changing the value of a control that changes continuously (color picker, dragging):
        // consecutive changes of this value become one undo step.
        protected void RecordContinuousChange()
        {
            if (_entry == null)
                return;

            LoreUndo.RecordTyping(
                _entry,
                LoreText.Undo.EditFieldValue,
                GetControlName());
        }

        protected void Save()
        {
            if (_entry == null)
                return;

            LoreChangeTracker.MarkModified(
                _entry);

            LoreRepaintService.Repaint();
        }
        #endregion
    }
}