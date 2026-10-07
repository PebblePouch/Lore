using System;
using System.Collections.Generic;

using UnityEngine;

namespace PebblePouch.Lore.Samples.OzDemo
{
    // A short conversation. Characters reference it through their "Dialogue" Field (a ScriptableObject Field whose
    // type is OzDialogue), so Lore only stores the reference and the game reads the lines itself.
    [CreateAssetMenu(menuName = "Lore Samples/Oz Dialogue", fileName = "Dialogue")]
    public sealed class OzDialogue : ScriptableObject
    {
        #region Types
        [Serializable]
        public sealed class Line
        {
            [SerializeField]
            private string _speaker;

            [SerializeField, TextArea]
            private string _text;

            public string Speaker => _speaker;

            public string Text => _text;
        }
        #endregion

        #region Fields
        [SerializeField]
        private List<Line> _lines = new();
        #endregion

        #region Properties
        public IReadOnlyList<Line> Lines => _lines;
        #endregion
    }
}
