#if UNITY_EDITOR && SORTIFY
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sortify
{
    public static class SortifyHelper
    {
        public static string GetObjectID(GameObject obj)
        {
            var objID = obj.scene.name;
            var currentTransform = obj.transform;
            while (currentTransform != null)
            {
                objID += $"_{currentTransform.name}_{currentTransform.GetSiblingIndex()}";
                currentTransform = currentTransform.parent;
            }

            return objID;
        }


        public static GameObject FindObjectByID(string objID)
        {
            var activeScene = SceneManager.GetActiveScene();
            foreach (GameObject rootObj in activeScene.GetRootGameObjects())
            {
                GameObject foundObj = SearchInChildren(rootObj, objID);
                if (foundObj != null)
                    return foundObj;
            }

            return null;
        }

        private static GameObject SearchInChildren(GameObject parent, string objID)
        {
            if (GetObjectID(parent) == objID)
                return parent;

            foreach (Transform child in parent.transform)
            {
                GameObject foundObj = SearchInChildren(child.gameObject, objID);
                if (foundObj != null)
                    return foundObj;
            }

            return null;
        }

        public static List<GameObject> GetAllObjects()
        {
            var activeScene = SceneManager.GetActiveScene();
            var allObjects = new List<GameObject>();
            foreach (GameObject rootObject in activeScene.GetRootGameObjects())
            {
                allObjects.Add(rootObject);
                AddChildrenToList(rootObject, allObjects);
            }

            return allObjects;
        }

        private static void AddChildrenToList(GameObject parent, List<GameObject> allObjects)
        {
            foreach (Transform child in parent.transform)
            {
                allObjects.Add(child.gameObject);
                AddChildrenToList(child.gameObject, allObjects);
            }
        }

        public static Texture2D MakeTexture(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        public static Texture2D MakeRoundedTexture(int width, int height, Color col, int borderRadius)
        {
            Texture2D tex = new Texture2D(width, height);
            Color[] pix = new Color[width * height];
            int borderRadiusSquared = borderRadius * borderRadius;

            Vector2 centerTopLeft = new Vector2(borderRadius, borderRadius);
            Vector2 centerTopRight = new Vector2(width - borderRadius - 1, borderRadius);
            Vector2 centerBottomLeft = new Vector2(borderRadius, height - borderRadius - 1);
            Vector2 centerBottomRight = new Vector2(width - borderRadius - 1, height - borderRadius - 1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color c = col;
                    bool isOutside = false;

                    if (x < borderRadius && y < borderRadius)
                    {
                        Vector2 distance = new Vector2(x, y) - centerTopLeft;
                        if ((distance.x * distance.x + distance.y * distance.y) > borderRadiusSquared)
                            isOutside = true;
                    }
                    else if (x > width - borderRadius - 1 && y < borderRadius)
                    {
                        Vector2 distance = new Vector2(x, y) - centerTopRight;
                        if ((distance.x * distance.x + distance.y * distance.y) > borderRadiusSquared)
                            isOutside = true;
                    }
                    else if (x < borderRadius && y > height - borderRadius - 1)
                    {
                        Vector2 distance = new Vector2(x, y) - centerBottomLeft;
                        if ((distance.x * distance.x + distance.y * distance.y) > borderRadiusSquared)
                            isOutside = true;
                    }
                    else if (x > width - borderRadius - 1 && y > height - borderRadius - 1)
                    {
                        Vector2 distance = new Vector2(x, y) - centerBottomRight;
                        if ((distance.x * distance.x + distance.y * distance.y) > borderRadiusSquared)
                            isOutside = true;
                    }

                    if (isOutside)
                    {
                        c = new Color(0, 0, 0, 0);
                    }

                    pix[y * width + x] = c;
                }
            }

            tex.SetPixels(pix);
            tex.Apply();
            return tex;
        }

        public static string FormatName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(name[0]);

            for (int i = 1; i < name.Length; i++)
            {
                if (char.IsUpper(name[i]) && !char.IsUpper(name[i - 1]))
                    sb.Append(' ');

                sb.Append(name[i]);
            }

            return sb.ToString();
        }
    }
}
#endif
