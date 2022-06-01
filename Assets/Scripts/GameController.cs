using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public GameObject UiTitleScreen;
    public GameObject UiOptionsScreen;
    public GameObject UiAboutScreen;

    public GameDataHolder GameData;

    [ReadOnly]
    [Tooltip("The current scene/leel being played.")]
    public string CurrentGameScene;

    private static GameController InnerGameController { get; set; }
    public static GameController TheGameController
    {
        get
        {
            if (null == InnerGameController)
            {
                var goController = GameObject.FindGameObjectWithTag(GameConstants.TagGameController);
                if (goController.TryGetComponent<GameController>(out GameController controller))
                    InnerGameController = controller;

            }
            return InnerGameController;
        }
        set
        {
            InnerGameController = value;
        }
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (CurrentGameScene?.Any() == true)
            SceneManager.UnloadSceneAsync(CurrentGameScene);
        CurrentGameScene = sceneName;
    }

    public void LoadNextScene()
    {
        int nextSceneIndex = 0;

        if (CurrentGameScene?.Any() == true)
        {
            for (int i = 0; i < GameData.GamePrefs.LevelProgression.Length; i++)
            {
                if (GameData.GamePrefs.LevelProgression[i] == CurrentGameScene)
                {
                    nextSceneIndex = i + 1;
                    break;
                }
            }
        }

        nextSceneIndex %= GameData.GamePrefs.LevelProgression.Length;
        LoadScene(GameData.GamePrefs.LevelProgression[nextSceneIndex]);
    }

}
