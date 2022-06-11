public static class GameData
{
    public static GameDataHolder Current => GameController.TheGameData;
}
public static class GamePrefs
{
    public static GamePreferences Current => GameController.TheGameData.GamePrefs;
}
