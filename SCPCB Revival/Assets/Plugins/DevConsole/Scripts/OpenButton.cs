using System;
using UnityEngine;

namespace SickDev.DevConsole {
    [Serializable]
    public class OpenButton {
        public float width;
        public float height;

        public void Draw(float positionY) {
            Rect rect = new Rect((Screen.width / 2)/DevConsole.settings.scale - width / 2, positionY, width, height);
            if (GUIUtils.DrawCenteredButton(rect, new GUIContent(DevConsole.singleton.isOpen?DevConsole.settings.closeConsoleIcon:DevConsole.settings.openConsoleIcon), DevConsole.settings.mainColor))
                DevConsole.singleton.ToggleOpen();
        }
    }
}
