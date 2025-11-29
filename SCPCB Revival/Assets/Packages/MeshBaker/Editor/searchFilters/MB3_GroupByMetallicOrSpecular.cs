using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using DigitalOpus.MB.Core;

namespace DigitalOpus.MB.MBEditor
{

    public class MB3_GroupByMetallicOrSpecular : IGroupByFilter
    {
        public string GetName()
        {
            return "MetallicOrSpecular";
        }

        public string GetDescription(GameObjectFilterInfo fi)
        {
            return "metallicOrSpecular=" + fi.metallicOrSpecularName;
        }

        public int Compare(GameObjectFilterInfo a, GameObjectFilterInfo b)
        {
            return a.metallicOrSpecularName.CompareTo(b.metallicOrSpecularName);
        }
    }

}



