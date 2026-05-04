using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{

    [Header("Scene names")]
    [SerializeField] private string fixationTestScene = "FixationTestScene";
    [SerializeField] private string saccadeTestScene = "SaccadeTestScene";
    [SerializeField] private string blinkTestScene = "BlinkTestScene";
    [SerializeField] private string combinedGestureScene = "CombinedGestureTestScene";
    [SerializeField] private string mainMenuScene = "MainMenuScene";
    
    public void LoadFixationScene() => SceneManager.LoadScene(fixationTestScene);

    public void LoadSaccadeScene() => SceneManager.LoadScene(saccadeTestScene);

    public void LoadBlinkScene() => SceneManager.LoadScene(blinkTestScene);

    public void LoadCombinedGestureScene() => SceneManager.LoadScene(combinedGestureScene);

    public void LoadMainMenu() => SceneManager.LoadScene(mainMenuScene);

    public void ExitApplication()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
