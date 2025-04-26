using UnityEditor;
using UnityEngine;

namespace Array2DEditor
{
    [CustomPropertyDrawer(typeof(Array2DSprite))]
    public class Array2DSpriteDrawer : Array2DObjectDrawer<Sprite> { }
}
