using UnityEngine;
using System;
using System.Collections.Generic;

namespace FalmeStreamless.Credits
{
    public class Pool : MonoBehaviour
    {
        public static event Action<float> onRemovedItem;

		public ListItemsScriptableObject itemList; 

		private Dictionary<string, Stack<CreditsItem>> freeItems = new Dictionary<string, Stack<CreditsItem>>();

		public CreditsItem GetItem(string id, Transform newParent)
		{
			if(!freeItems.ContainsKey(id))
				freeItems.Add(id, new Stack<CreditsItem>());

		    if (freeItems[id].Count <= 0) Add(id);

		    CreditsItem item = freeItems[id].Pop();
		    item.transform.SetParent(newParent);
		    return item;
		}

		private void Add(string id)
		{
		    CreditsItem item = Instantiate(GetPrefabById(id), transform).GetComponent<CreditsItem>();
		    item.SetPool(this);
			item.SetId(id);
		    freeItems[id].Push(item);
		}

        public void Release(string id, CreditsItem item)
        {
            onRemovedItem?.Invoke(item.GetHeight());
            item.transform.SetParent(transform);
			freeItems[id].Push(item);
        }

		private CreditsItem GetPrefabById(string id)
		{
			for(int a=0; a<itemList.items.Count; a++)
			{
				if(id.Equals(itemList.items[a].id))
					return itemList.items[a].prefab;
			}
			
			Debug.LogError("ID prefab not found in Pool list");
			return null; 
		}
    }
}
