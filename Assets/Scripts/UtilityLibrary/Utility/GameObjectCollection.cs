using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameObjectCollection<T>
{
    public List<KeyValuePair<GameObject, T>> Members { get; private set; } = new List<KeyValuePair<GameObject, T>>();

    public KeyValuePair<GameObject, T> Find(GameObject go)
    {
        return Members.FirstOrDefault(kvp => kvp.Key == go);
    }
    public bool Contains(GameObject go) => Find(go).Key != null;

    public void RememberObject(GameObject go, T sideData)
    {
        var existing = Find(go);
        if ( existing.Key == null )
            Members.Add(new KeyValuePair<GameObject, T>(go, sideData));
    }
    public void ForgetObject(GameObject go)
    {
        var existing = Find(go);
        if (existing.Key != null)
            Members.Remove(existing);
    }

    public void Clear()
    {
        Members.Clear();
    }

	public void PruneNullTargets()
	{
		Members.Where(t => t.Key == null)
			.ToList()
			.ForEach(t =>
			{
				Members.Remove(t);
			});
	}
}

