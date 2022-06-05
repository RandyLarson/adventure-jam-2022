using UnityEngine;

public static class GameConstants
{
	public static string Default = "Default";
	public static string IsWalking = "Walking";
	public static string IsJumping = "Jumping";

	public static string TagGameController = "GameController";
    public static string GamePointer = "GamePointer";

	public static int LayerMaskDefault;

	public static int SortingLayerDefaultId;

	public static int AnimatorHashIsWalking = Animator.StringToHash(IsWalking);
	public static int AnimatorHashIsJumping = Animator.StringToHash(IsJumping);

	public static void Init()
	{
		LayerMaskDefault = 1 << LayerMask.NameToLayer(Default);

		SortingLayerDefaultId = SortingLayer.NameToID(Default);
	}

}
