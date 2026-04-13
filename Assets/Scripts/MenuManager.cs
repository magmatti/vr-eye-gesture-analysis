using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{

    private string fixationTestScene = "FixationTestScene";
    private string saccadeTestScene = "SaccadeTestScene";
    private string blinkTestScene = "BlinkTestScene";
    private string smoothPersuitScene = "SmoothPersuitTestScene";
    private string mainMenuScene = "MainMenuScene";
    
    public void LoadFixationScene()
    {
        SceneManager.LoadScene(fixationTestScene);
    }

    public void LoadSaccadeScene()
    {
        SceneManager.LoadScene(saccadeTestScene);
    }

    public void LoadBlinkScene()
    {
        SceneManager.LoadScene(blinkTestScene);
    }

    public void LoadSmoothPersuitScene()
    {
        SceneManager.LoadScene(smoothPersuitScene);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuScene);

    }

    public void ExitApplication()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
