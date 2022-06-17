using Assets.Scripts.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public GameObject UiTitleScreen;
    public GameObject UiOptionsScreen;
    public GameObject PauseScreen;
    public GameObject SettingsScreen;
    public GameObject UiAboutScreen;
    public DiagnosticController Diagnostics;

    public GameDataHolder GameData;

    public UnityEvent OnStartGame;
    public void StartGame()
    {
        OnStartGame?.Invoke();
    }

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

    public void RestartGame()
    {
        SceneManager.LoadScene(GameData.GamePrefs.LevelProgression[0]);
    }

    public void ShowPauseMenu()
    {
        PauseScreen.SafeSetActive(true);
    }

    public void ShowSettingsMenu()
    {
        SettingsScreen.SafeSetActive(true);
    }

    public GameObject PlayingLevelObject;
    public GameObject Intro;
    public GameObject Outro;

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

                if ( aScene.name == "Development")
                {
                    HideStartMenu();
                    //PlayingLevelObject.SetActive(true);
                    Intro.SetActive(false);
                }

                //if (GameData.GamePrefs.LevelProgression.Contains(aScene.name))
                //{
                //    CurrentGameScene = aScene.name;
                //    break;
                //}
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
        // set things to the starting mode:
        UiTitleScreen.SetActive(true);
        PlayingLevelObject.SetActive(false);
        Intro.SetActive(false);
        Outro.SetActive(true);
        PlayTheme();
    }

    public void PlayTheme()
    {
        GetComponent<AudioController>().PlayTheme();
    }

    public void HideStartMenu()
    { 
        UiTitleScreen.SetActive(false);
        GetComponent<AudioController>().StopTheme();
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
