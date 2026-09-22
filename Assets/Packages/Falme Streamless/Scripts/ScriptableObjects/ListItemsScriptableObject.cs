using UnityEngine;
using System.Collections.Generic;

namespace FalmeStreamless.Credits
{
	[CreateAssetMenu(fileName = "ListItemsScriptableObject", menuName = "Scriptable Objects/ListItems")]
	public class ListItemsScriptableObject : ScriptableObject
	{
		public List<PoolItem> items = new List<PoolItem>();
	}
}
