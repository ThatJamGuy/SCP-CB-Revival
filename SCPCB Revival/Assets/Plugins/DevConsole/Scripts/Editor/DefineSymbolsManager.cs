using System.Linq;
using UnityEditor;

namespace SickDev.DevConsole { 
    public class DefineSymbolsManager {
        public static bool HasDefine(string define, BuildTargetGroup group) {
            return new DefineSymbolsAgent(group).Has(define);
        }

        public static void AddDefine(string define, BuildTargetGroup group) {
            new DefineSymbolsAgent(group).Add(define);
        }

        public static void RemoveDefine(string define, BuildTargetGroup group) {
            new DefineSymbolsAgent(group).Remove(define);
        }

        class DefineSymbolsAgent {
            BuildTargetGroup group;
            string defineSymbolsString;
            string[] defineSymbols;

            public DefineSymbolsAgent(BuildTargetGroup group) {
                this.group = group;
                LoadSymbols();
            }

            void LoadSymbols() {
                defineSymbolsString = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
                defineSymbols = defineSymbolsString.Split(';');
            }

            public bool Has(string define) {
                return defineSymbols.Contains(define);
            }

            public void Add(string define) {
                if(Has(define))
                    return;
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defineSymbolsString + ";" + define);
                LoadSymbols();
            }

            public void Remove(string define) {
                if(!Has(define))
                    return;

                int startIndex = defineSymbolsString.IndexOf(define);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defineSymbolsString.Remove(startIndex, define.Length));
                LoadSymbols();
            }
        }
    }
}