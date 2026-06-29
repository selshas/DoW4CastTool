using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices.WindowsRuntime;

[RequireComponent(typeof(ToggleGroup))]
public class FactionSelector : PickSelector<FactionData>
{
    protected override IList<FactionData> CollectData()
    {
        var factions = FactionDataLoader.Instance.Factions.OrderBy(x => x.Key).Select(x => x.Value).ToArray();
        return factions;
    }

    protected override void OnOptionLoaded(Transform child, FactionData data)
    {
        var toggle = child.GetComponent<Toggle>();
        toggle.group = toggleGroup;
        ((RawImage)toggle.targetGraphic).texture = data.SymbolTexture;

        var factionName = data.Name;
        toggle.onValueChanged.AddListener((isOn) =>
        {
            if (!isOn)
                return;

            MatchSetup.Instance.ChangeFaction(playerData.PlayerIndex, factionName);
            Close();
        });

        toggle.SetIsOnWithoutNotify(data.Name == playerData.FactionName);
    }
}
