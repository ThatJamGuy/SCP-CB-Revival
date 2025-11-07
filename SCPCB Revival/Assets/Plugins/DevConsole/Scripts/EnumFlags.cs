using UnityEngine;

//This, together with its Drawer "EnumFlagsDrawer", allows selecting multiple elements from an enumeration possible
namespace SickDev.Utils {
    public class EnumFlagsAttribute : PropertyAttribute {
        public string[] displayOptions { get; set; }
    }
}