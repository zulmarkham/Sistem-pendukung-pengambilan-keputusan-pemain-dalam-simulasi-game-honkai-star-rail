using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AllySelectButton : MonoBehaviour
{
    public GameObject HeroPrefab;

    public void SelectThisHero()
    {
        BattleStateMachine BSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();
        if (BSM != null)
        {
            BSM.Input2Hero(HeroPrefab);
        }
    }
    public void HideSelector()
    {
        HeroPrefab.transform.Find("Selector").gameObject.SetActive(false);
    }
    public void ShowSelector()
    {
        HeroPrefab.transform.Find("Selector").gameObject.SetActive(true);
    }

    void Start()
    {
        var btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(SelectThisHero);
        }
    }
}
