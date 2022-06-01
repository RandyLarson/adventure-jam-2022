using UnityEngine;

public class ValueCache : MonoBehaviour
{
    public int? SortingLayer { get; set; }
}

public static class ValueCacheExtensions
{
    public static ValueCache EnsureValueCache(this GameObject go)
    {
        var itsValueCache = go.GetComponent<ValueCache>();
        if (itsValueCache == null)
            itsValueCache = go.gameObject.AddComponent<ValueCache>();

        return itsValueCache;
    }

    public static void SetSortLayer(this GameObject go, int newLayer)
    {
        if (go == null)
            return;

        var children = go.GetComponentsInChildren<SpriteRenderer>();
        if (children.Length > 0)
        {
            for (int i = 0; i < children.Length; i++)
            {
                var itsValueCache = children[i].gameObject.EnsureValueCache();
                if (itsValueCache != null && !itsValueCache.SortingLayer.HasValue)
                    itsValueCache.SortingLayer = children[i].sortingLayerID;
                children[i].sortingLayerID = newLayer;
            }
        }
    }

    public static void RestoreSortLayer(this GameObject go)
    {
        if (go == null)
            return;

        var children = go.GetComponentsInChildren<SpriteRenderer>();
        if (children.Length > 0)
        {
            for (int i = 0; i < children.Length; i++)
            {
                var itsValueCache = children[i].gameObject.EnsureValueCache();
                if (itsValueCache != null && itsValueCache.SortingLayer.HasValue)
                {
                    children[i].sortingLayerID = itsValueCache.SortingLayer.Value;
                }
            }
        }
    }

}