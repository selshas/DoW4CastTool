using UnityEngine;
using UnityEngine.EventSystems;

public class UIUnfocusor : MonoBehaviour
{
    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
