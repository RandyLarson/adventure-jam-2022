using UnityEngine;

public interface ITagDomain
{
    public string[] TheDomain { get; set; }

    public bool IsInDomain(GameObject gameObject);
}
