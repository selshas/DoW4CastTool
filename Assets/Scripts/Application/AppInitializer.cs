using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AppInitializer : MonoBehaviour
{
    /// <summary>
    /// Waits for subsystems to load, then transitions to the outgame scene.
    /// </summary>
    IEnumerator Start()
    {
        while (
            !ApplicationSetup.Instance!.IsLoaded
            || !FactionDataLoader.Instance!.IsLoaded
        )
            yield return null;

        SceneManager.LoadScene(SceneNames.OutgameOverlay);
    }
}
