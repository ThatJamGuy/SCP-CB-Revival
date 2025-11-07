using System;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using SickDev.CommandSystem;

namespace SickDev.DevConsole {
    [Serializable]
    public class AutoComplete:Window<int, Command> {
        string lastText = null;
        Command[] allCommands;
        Matcher matcher;
        GUIContent tempContent = new GUIContent();
        List<JobInfo> jobs = new List<JobInfo>();
        object lockObject = new object();
        bool needsCommandReload;

        protected override GUIContent title { get {return new GUIContent("AUTO COMPLETE");} }
        public override Command current { get { return allCommands[entries[currentIndex]]; } }
        public bool hasMatches { get { return entries.Count > 0; } }

        protected override void OnInitialized() {
            DevConsole.singleton.commandsManager.onCommandAdded += OnCommandsModified;
            DevConsole.singleton.commandsManager.onCommandRemoved += OnCommandsModified;
            lastText = null;
            needsCommandReload = true;
        }

        void OnCommandsModified(Command commandAddedOrRemoved) {
            needsCommandReload = true;
        }

        protected override void OnBeginDraw() {
            if(needsCommandReload)
                ReloadCommands();
            if(lastText != DevConsole.singleton.input.text) {
                RebuildCommandsToShow();
                lastText = DevConsole.singleton.input.text;
            }
        }

        void ReloadCommands() {
            allCommands = DevConsole.singleton.GetCommands();
            Clear();
            needsCommandReload = false;
            matcher = new Matcher(allCommands);
            RebuildCommandsToShow();
        }
        
        void RebuildCommandsToShow() {
            JobInfo job;
            lock(lockObject) {
                if(jobs.Count > 0)
                    jobs[jobs.Count - 1].needed = false;
                job = new JobInfo();
                jobs.Add(job);
            }

            bool allowThreading = Application.platform != RuntimePlatform.WebGLPlayer;
            if(allowThreading) {
                Thread thread = new Thread(RebuildCommandsToShowThreaded);
                thread.Start(job);
            }
            else
                RebuildCommandsToShowThreaded(job);
        }

        void RebuildCommandsToShowThreaded(object jobObject) {
            JobInfo job = (JobInfo)jobObject;
            int[] matches = matcher.GetMatches(DevConsole.singleton.input.text);
            lock(lockObject) {
                if(job.needed) {
                    Clear();
                    AddRange(matches);
                    NavigateToFirst();
                }
                jobs.Remove(job);
            }
        }

        protected override void DrawEntry(Rect rect, int index) {
            GUI.Label(rect, allCommands[index].signature.raw, GUIUtils.textStyle);
        }

        protected override void OnEntryClicked(int index) {
            Select(index);
        }

        public void SelectCurrent() {
            Select(entries[currentIndex]);
        }

        void Select(int index) {
            DevConsole.singleton.input.text = allCommands[index].name + " ";
            DevConsole.singleton.input.MoveCursorToEnd();
            Close();
        }

        protected override float GetEntryWidth(int index) {
            if(allCommands == null || index >= allCommands.Length)
                return 0;
            tempContent.text = allCommands[index].signature.raw;
            return GUIUtils.textStyle.CalcSize(tempContent).x;
        }

        protected override void OnOpen() {
            if (DevConsole.settings.autoCompleteBehaviour == Settings.AutoCompleteBehaviour.Disabled)
                Close();
        }

        class Matcher {
            const string regex = @"((\.|_)?.+?)(?=[A-Z]|\.|_|\s|$)";

            Command[] commands;
            string[] titledCaseCommands;
            int exactMatch = -1;
            List<int> startsWith = new List<int>();
            List<int> matchesRegex = new List<int>();
            List<int> matchesRegexTitledCase = new List<int>();

            static Regex _inputRegx;
            static Regex inputRegex {
                get {
                    if(_inputRegx == null)
                        _inputRegx = new Regex(regex);
                    return _inputRegx;
                }
            }

            public Matcher(Command[] commands) {
                this.commands = commands;
                BuildTitledCaseCommands();
            }

            void BuildTitledCaseCommands() {
                titledCaseCommands = new string[commands.Length];
                for (int i = 0; i < commands.Length; i++)
                    titledCaseCommands[i] = StringToFirstUpperCase(commands[i].name);
            }

            public int[] GetMatches(string text) {
                exactMatch = -1;
                startsWith.Clear();
                matchesRegex.Clear();
                matchesRegexTitledCase.Clear();

                try {
                    Regex commandsRegex = BuildCommandsRegex(text);
                    CategorizeMatches(text, commandsRegex);
                    int[] sortedMatches = SortMatches();
                    return sortedMatches;
                }
                catch {
                    return new int[0];
                }
            }

            Regex BuildCommandsRegex(string text) {
                Match match = inputRegex.Match(text);
                StringBuilder commandsRegexString = new StringBuilder(".*");

                while(match.Success) {
                    commandsRegexString.Append(string.Format(@"({0}|((\.|_){1})).*", match.Value, StringToFirstLowerCase(match.Value)));
                    match = match.NextMatch();
                }
                return new Regex(commandsRegexString.ToString());
            }

            void CategorizeMatches(string text, Regex regex) {
                Match match;
                for(int i = 0; i < commands.Length; i++) {
                    if (exactMatch == -1 && commands[i].IsOverloadOf(text))
                        exactMatch = i;
                    else if(commands[i].name.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                        startsWith.Add(i);
                    else {
                        match = regex.Match(commands[i].name);
                        if(match.Success)
                            matchesRegex.Add(i);
                        else {
                            match = regex.Match(titledCaseCommands[i]);
                            if(match.Success)
                                matchesRegexTitledCase.Add(i);
                        }
                    }
                }
            }

            int[] SortMatches() {
                int size = (exactMatch!=-1?1:0) + startsWith.Count + matchesRegex.Count + matchesRegexTitledCase.Count;
                int[] sortedMatches = new int[size];
                int lastIndex = 0;

                startsWith.Sort(CompareEntries);
                matchesRegex.Sort(CompareEntries);
                matchesRegexTitledCase.Sort(CompareEntries);

                if(exactMatch != -1)
                    sortedMatches[lastIndex++] = exactMatch;
                for(int i = 0; i < startsWith.Count; i++)
                    sortedMatches[lastIndex++] = startsWith[i];
                for(int i = 0; i < matchesRegex.Count; i++)
                    sortedMatches[lastIndex++] = matchesRegex[i];
                for(int i = 0; i < matchesRegexTitledCase.Count; i++)
                    sortedMatches[lastIndex++] = matchesRegexTitledCase[i];

                return sortedMatches;
            }

            int CompareEntries(int index1, int index2) {
                return string.Compare(commands[index1].name, commands[index2].name, true);
            }

            static string StringToFirstUpperCase(string s) {
                char[] chars = s.ToCharArray();
                chars[0] = char.ToUpper(chars[0]);
                return new string(chars);
            }

            static string StringToFirstLowerCase(string s) {
                char[] chars = s.ToCharArray();
                chars[0] = char.ToLower(chars[0]);
                return new string(chars);
            }
        }

        class JobInfo {
            public bool needed = true;
        }
    }
}
