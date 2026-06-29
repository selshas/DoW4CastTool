using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipManager : SingletonBehaviour<TooltipManager>
{
    public float DampenerIncrement = 1.0f;
    public float DampenerReleasing = 2.0f;
    public float DampeningTime = 0.5f;

    private float DampenerTimer = 0.0f;
    public float BufferTime = 0.5f;

    private Coroutine dampening;

    private TooltipTarget target;

    private GameObject frame;
    private TextMeshProUGUI text;

    public string Content 
    {
        get => text.text;
        set => SetContent(value);
    }

    protected override void OnInitialize()
    {
        text = GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
        frame = transform.GetChild(0).gameObject;
        frame.SetActive(false);
    }

    private void SetContent(string content)
    {
        text.text = content;
    }

    public void Trigger(TooltipTarget target)
    {
        gameObject.SetActive(true);

        if (dampening != null)
            StopCoroutine(dampening);

        dampening = StartCoroutine(DampeningOn());

        this.target = target;
        SetContent(target.Content);
    }

    public void Release()
    {
        frame.SetActive(false);

        if (dampening != null)
            StopCoroutine(dampening);

        dampening = StartCoroutine(DampeningOff());
    }

    private IEnumerator DampeningOn()
    {
        while (DampenerTimer <= DampeningTime)
        {
            yield return null;
            DampenerTimer += (Time.deltaTime * DampenerIncrement);
        }

        DampenerTimer = DampeningTime + BufferTime;

        dampening = null;

        frame.SetActive(true);
    }

    private IEnumerator DampeningOff()
    {
        while (DampenerTimer > 0)
        {
            yield return null;
            DampenerTimer -= (Time.deltaTime * DampenerReleasing);
        }

        DampenerTimer = 0;

        dampening = null;
    }

    public void PositionUpdate(PointerEventData eventData)
    {
        GetComponent<RectTransform>().anchoredPosition = eventData.position;
    }
}
