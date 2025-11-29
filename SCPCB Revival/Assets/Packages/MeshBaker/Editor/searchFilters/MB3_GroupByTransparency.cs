using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using DigitalOpus.MB.Core;

namespace DigitalOpus.MB.MBEditor
{

    public class MB3_GroupByTransparency : IGroupByFilter
    {
        public string GetName()
        {
            return "Transparency";
        }

        public string GetDescription(GameObjectFilterInfo fi)
        {
            return "transparency=" + fi.standardShaderBlendModesName;
        }

        public int Compare(GameObjectFilterInfo a, GameObjectFilterInfo b)
        {
            return a.standardShaderBlendModesName.CompareTo(b.standardShaderBlendModesName);
        }
    }

}



