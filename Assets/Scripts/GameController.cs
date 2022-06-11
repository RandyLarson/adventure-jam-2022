using Assets.Scripts.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public GameObject UiTitleScreen;
    public GameObject UiOptionsScreen;
    public GameObject PauseScreen;
    public GameObject UiAboutScreen;

    public GameDataHolder GameData;

    [ReadOnly]
    [Tooltip("The current scene/leel being played.")]
    public string CurrentGameScene;

    private static GameController InnerGameController { get; set; }

    public static GameDataHolder TheGameData => TheGameController.GameData;
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

    private void Awake()
    {
        GameConstants.Init();
    }

    //void OnPause(InputValue value)
    //{
    //    ShowPauseMenu();
    //}


    public void ShowPauseMenu()
    {
        PauseScreen.SafeSetActive(true);
    }

    private void Start()
    {

        // Present for game development ease. The controller scene is mandatory. In the production build, the
        // controller scene will be the initial scene and drive the whole thing. In development mode, though,
        // we want to have the level we are working on loaded into the editor. We also want to be able to 
        // press play and have this work. This bit here will locate the loaded level and remember this as the current
        // scene. It will also hide all the Ui scenes. This should allow us a more pleasant dev experience.
        if (SceneManager.sceneCount > 1)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene aScene = SceneManager.GetSceneAt(i);
                if (GameData.GamePrefs.LevelProgression.Contains(aScene.name))
                {
                    CurrentGameScene = aScene.name;
                    HideStartMenu();
                    break;
                }
            }
        }
        else
        {
            ShowStartMenu();
        }
    }

    private void Update()
    {
        GlobalSpawnQueue.SpawnQueueItems();
    }

    public void ShowStartMenu()
    {
        UiTitleScreen.SetActive(true);
    }

    public void HideStartMenu()
    { 
        UiTitleScreen.SetActive(false);
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

    public void LogGameAchievement(string achievment)
    {
        Debug.Log($"Game achievement: {achievment}.");
        TheGameData.CurentGameData.AddAchievement(achievment);
    }

}
