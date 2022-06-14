using UnityEngine;

public class TreeGrowingController : MonoBehaviour
{
    public LevelGoal TreeGrowingGoal;

    public void StartTreeGrowth()
    {
        if ( gameObject.TryGetComponent<Animator>(out var treeAnim))
        {
            treeAnim.SetBool("IsGrowing", true);
        }
    }

    public void TreeHasFinishedGrowing()
    {
        if ( TreeGrowingGoal != null)
        {
            TreeGrowingGoal.IsComplete = true;
        }
    }
}
