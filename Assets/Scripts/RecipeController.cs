using Assets.Scripts.Extensions;
using UnityEngine;
using UnityEngine.Events;

public class RecipeController : MonoBehaviour
{
    public int OnIngredientNum = -1;
    public GameObject[] Progression;
    public UnityEvent OnComplete;

    public void IngredientAdded()
    {
        if (OnIngredientNum + 1 < Progression.Length)
        {
            if (OnIngredientNum > 0)
                Progression[OnIngredientNum - 1].SafeSetActive(false);

            OnIngredientNum++;
            Progression[OnIngredientNum].SafeSetActive(true);

            if (OnIngredientNum == Progression.Length-1)
                OnComplete?.Invoke();
        }
    }
}
