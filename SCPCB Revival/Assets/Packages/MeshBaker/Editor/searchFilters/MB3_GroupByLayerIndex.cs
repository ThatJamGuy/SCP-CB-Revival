using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using DigitalOpus.MB.Core;

namespace DigitalOpus.MB.MBEditor
{

    public class MB3_GroupByLayerIndex : IGroupByFilter
    {
        public string GetName()
        {
            return "Layer Index";
        }

        public string GetDescription(GameObjectFilterInfo fi)
        {
            return "layerIndex=" + fi.layerIndex;
        }

        public int Compare(GameObjectFilterInfo a, GameObjectFilterInfo b)
        {
            return b.layerIndex - a.layerIndex;
        }
    }
}