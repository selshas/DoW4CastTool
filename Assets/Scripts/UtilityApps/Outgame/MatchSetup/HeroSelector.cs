using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ToggleGroup))]
public class HeroSelector : PickSelector<HeroData>
{
    protected override IList<HeroData> CollectData()
    {
        var factionData = FactionDataLoader.Instance.Factions[playerData.FactionName];
        var heroes = factionData.Heroes.OrderBy(x => x.Key).Select(x => x.Value).ToArray();
        return heroes;
    }

    protected override void OnOptionLoaded(Transform child, HeroData data)
    {
        var toggle = child.GetComponent<Toggle>();
        toggle.group = toggleGroup;
        ((RawImage)toggle.targetGraphic).texture = data.PortraitTexture;

        var heroName = data.Name;
        toggle.onValueChanged.AddListener((isOn) =>
        {
            if (!isOn)
                return;

            MatchSetup.Instance.ChangeHero(playerData.PlayerIndex, heroName);
            Close();
        });

        toggle.SetIsOnWithoutNotify(data.Name == playerData.HeroName);
    }
}