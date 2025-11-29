using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using DigitalOpus.MB.Core;

namespace DigitalOpus.MB.MBEditor
{

    public class GroupByHasSameSetsOfMaps : IGroupByFilter
    {
        public string GetName() {
            return "HasSameSetsOfMaps";
        }
        public string GetDescription(GameObjectFilterInfo fi)
        {
            return "HasMaps=" + fi.setOfMaps;
        }
        public int Compare(GameObjectFilterInfo a, GameObjectFilterInfo b)
        {
            return a.setOfMaps.CompareTo(b.setOfMaps);
        }
    }
}
