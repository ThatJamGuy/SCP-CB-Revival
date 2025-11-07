using System;

namespace SickDev.DevConsole {
    [Serializable]
    public class Filter {
        public string tag { get; private set; }
        public int hash { get; private set; }
        public bool isActive;

        public Filter(string tag, int hash) {
            this.tag = tag;
            this.hash = hash;
            isActive = true;
        }
    }
}