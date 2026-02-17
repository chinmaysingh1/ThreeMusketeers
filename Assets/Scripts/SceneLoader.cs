using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneLoader : MonoBehaviour
{
    public void LoadSceneAsync(string sceneName)
    {
        StartCoroutine(LoadSceneAsyncRoutine(sceneName));
    }

    // Coroutine does the yielding
    private IEnumerator LoadSceneAsyncRoutine(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // Optional: prevent auto-activation until you're ready
        // asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {

            Debug.Log($"Loading scene '{sceneName}': {asyncLoad.progress * 100f:0}%");
            yield return null;

            // If using allowSceneActivation=false, you'd activate when ready:
            // if (asyncLoad.progress >= 0.9f) asyncLoad.allowSceneActivation = true;
        }
    }
}
