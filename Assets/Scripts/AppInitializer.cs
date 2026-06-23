using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AppInitializer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        // Wait Untile subsystems are loaded.
        while (
            !ApplicationSetup.Instance!.IsLoaded
            || !FactionDataLoader.Instance!.IsLoaded
        )
            yield return null;

        SceneManager.LoadScene(1);
    }
}
