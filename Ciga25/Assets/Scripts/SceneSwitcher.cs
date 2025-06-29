using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [Header("Scene Settings")]
    public string loadSceneName; // 要加载的新场景名称

    private void OnEnable()
    {
        if (!string.IsNullOrEmpty(loadSceneName))
        {
            Debug.Log("SceneSwitcher enabled. Switching to scene: " + loadSceneName);
            SceneManager.LoadScene(loadSceneName);
        }
        else
        {
            Debug.LogWarning("No scene name specified for SceneSwitcher!");
        }
    }
}