using UnityEngine;

public interface IContentFiller
{
	GameObject GetListItem(int index, int itemType, GameObject obj);
	int GetItemCount();
	int GetItemType(int index);
}
