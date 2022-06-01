using System.Linq;
using UnityEngine;

public class TagDomain : MonoBehaviour, ITagDomain
{
    [Tooltip("If non-empty, only fatigue when these tags contact.")]
    public string[] Domain;

    public string[] TheDomain { get => Domain; set => Domain = value; }

    public bool IsInDomain(GameObject gameObject)
    {
        if (gameObject == null)
            return false;

        if (Domain == null)
            return true;

        for (int i = 0; i < Domain.Length; i++)
        {
            if (gameObject.CompareTag(TheDomain[i]))
                return true;
        }

        return false;
    }
}
