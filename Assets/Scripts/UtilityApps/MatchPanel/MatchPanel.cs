using UnityEngine;
using System.Collections.Generic;

public class MatchPanel : UtilityAppBase
{
    public List<MatchPanelTemplate> templates = new List<MatchPanelTemplate>();

    /// <summary>
    /// No hotkey bindings for MatchPanel.
    /// </summary>
    public override void InitializeInputs() { }

    private void Awake()
    {
        var matchMode = MatchDataManager.Instance.CurrentMatchMode;
        var template = templates.Find(x => x.MatchMode == matchMode);
        if (template == null)
            template = templates.Find(x => (x.MatchMode & matchMode) == matchMode);

        template.LoadTeamData();
    }

    /// <summary> Populates emblem sprites from FactionDataLoader. </summary>
    private void LoadPortraitSprites()
    {
    }
}
