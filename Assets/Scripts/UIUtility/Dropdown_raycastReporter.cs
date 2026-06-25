using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Dropdown_raycastReporter : MonoBehaviour
{
    Transform dropDownList;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        dropDownList = transform.Find("Dropdown List");
        if (dropDownList != null)
        {
            PointerEventData ped = new PointerEventData(null);
            ped.position = Mouse.current.position.ReadValue();

            dropDownList.GetComponent<GraphicRaycaster>().Raycast(ped, ApplicationSetup.RaycastResults);
        }
    }
}
