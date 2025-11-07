using System;
using UnityEngine;

namespace SickDev.DevConsole{
    [Serializable]
    public struct EntryOptions {
        public FontStyle style;
        public int size;

        bool hasColorBeenAssigned;

        Color _color;
        public Color color {
            get {
                if(!hasColorBeenAssigned)
                    color = DevConsole.settings.entryDefaultColor;
                return _color;
            }
            set {
                _color = value;
                hasColorBeenAssigned = true;
            }
        }
    }
}
