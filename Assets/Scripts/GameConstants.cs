using UnityEngine;

public static class GameConstants
{
	public static string Default = "Default";

	public static string TagGameController = "GameController";
    public static string GamePointer = "GamePointer";

	public static int LayerMaskDefault;

	public static int SortingLayerDefaultId;

	public static void Init()
	{
		LayerMaskDefault = 1 << LayerMask.NameToLayer(Default);

		SortingLayerDefaultId = SortingLayer.NameToID(Default);
	}

}
