using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{

    private string fixationTestScene = "FixationTestScene";
    private string saccadeTestScene = "SaccadeTestScene";
    private string blinkTestScene = "BlinkTestScene";
    private string smoothPersuitScene = "SmoothPersuitScene";
    
    public void LoadFixationScene()
    {
        SceneManager.LoadScene(fixationTestScene);
    }

    public void LoadSaccadeScene()
    {
        SceneManager.LoadScene(saccadeTestScene);
    }

    public void LoadBlinkTest()
    {
        SceneManager.LoadScene(blinkTestScene);
    }

    public void LoadSmoothPersuitTest()
    {
        SceneManager.LoadScene(smoothPersuitScene);
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
