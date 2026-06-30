using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerNameAutoCompletionList : MonoBehaviour
{
    [SerializeField] private int candidateCount = 5;
    [SerializeField] private Color candidateNormalColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
    [SerializeField] private Color candidateHoverColor = new Color(0.25f, 0.25f, 0.25f, 0.9f);

    private List<string> candidates = new List<string>();
    private UIItemList<string> candidateLabels;
    private TMP_InputField attachedInputField;

    public int CandidateCount => candidateCount;

    /// <summary>
    /// Initializes the candidate label list from the prototype child.
    /// </summary>
    private void Awake()
    {
        candidateLabels = new UIItemList<string>(transform, candidates, BindCandidate);
    }

    /// <summary>
    /// Hides the list when the attached input field loses focus.
    /// </summary>
    private void Update()
    {
        if (attachedInputField != null && !attachedInputField.isFocused)
            Hide();
    }

    /// <summary>
    /// Sets the input field that receives the selected candidate value.
    /// </summary>
    public void AttachTo(TMP_InputField inputField)
    {
        attachedInputField = inputField;
    }

    /// <summary>
    /// Searches for candidates matching the keyword and updates the list.
    /// </summary>
    public void Refresh(string keyword)
    {
        PlayerDataLoader.Instance.FindCandidates(keyword, candidateCount, ref candidates);
        candidateLabels.UpdateItems(candidates);
    }

    /// <summary>
    /// Binds a candidate item, setting its label and wiring hover and click via EventTrigger.
    /// </summary>
    private void BindCandidate(Transform child, string name, int index)
    {
        var label = child.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
        label.text = name;

        var image = child.GetComponent<RawImage>();
        if (image == null)
            return;

        image.color = candidateNormalColor;

        if (child.GetComponent<EventTrigger>() != null)
            return;

        var trigger = child.gameObject.AddComponent<EventTrigger>();

        var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((_) => image.color = candidateHoverColor);
        trigger.triggers.Add(enterEntry);

        var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((_) => image.color = candidateNormalColor);
        trigger.triggers.Add(exitEntry);

        var clickEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        clickEntry.callback.AddListener((_) => SelectCandidate(label.text));
        trigger.triggers.Add(clickEntry);
    }

    /// <summary>
    /// Applies the selected candidate name to the attached input field and hides the list.
    /// </summary>
    private void SelectCandidate(string name)
    {
        if (attachedInputField != null)
            attachedInputField.text = name;

        Hide();
    }

    /// <summary>
    /// Hides the entire list.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
