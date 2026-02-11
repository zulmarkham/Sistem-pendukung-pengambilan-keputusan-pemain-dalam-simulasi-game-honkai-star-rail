using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class PartySelectionManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Dropdown[] heroDropdowns; // 4 dropdown

    [Header("Hero Slots in Battle")]
    public HeroStateMachine[] heroSlots; // 4 hero di scene

    private int[] selectedIndex;
    
    public BattleStateMachine battleStateMachine;
    public GameObject startBattleButton;
    public GameObject choosePartyPanel;


    void Start()
    {
        selectedIndex = new int[heroDropdowns.Length];
        for (int i = 0; i < selectedIndex.Length; i++)
            selectedIndex[i] = -1;


        InitDropdownTitles();
        RefreshAllDropdowns();
    }
    public void OnStartBattleButton()
    {
        if (!IsPartyValid())
        {
            Debug.Log("Party belum lengkap!");
            return;
        }

        // 🔥 MATIKAN UI SELECTION
        choosePartyPanel.SetActive(false);

        // 🔥 BARU MULAI BATTLE
        battleStateMachine.StartBattleAfterSelection();
    }
    void InitDropdownTitles()
    {
        for (int i = 0; i < heroDropdowns.Length; i++)
        {
            TMP_Dropdown dropdown = heroDropdowns[i];
            dropdown.captionText.text = $"Hero {i + 1}";
        }
    }




    void RefreshAllDropdowns()
    {
        for (int i = 0; i < heroDropdowns.Length; i++)
        {
            RefreshDropdown(i);
        }
    }

    void RefreshDropdown(int slot)
    {
        TMP_Dropdown dropdown = heroDropdowns[slot];
        dropdown.onValueChanged.RemoveAllListeners();
        dropdown.ClearOptions();

        List<TMP_Dropdown.OptionData> options = new();
        List<int> optionToCharacterIndex = new();

        options.Add(new TMP_Dropdown.OptionData("Pilih Hero"));
        optionToCharacterIndex.Add(-1);

        for (int i = 0; i < CharacterDatabase.Instance.characters.Count; i++)
        {
            if (IsUsedByOtherSlot(i, slot)) continue;

            CharacterData c = CharacterDatabase.Instance.characters[i];
            string label = $"{c.name} ({c.role})";

            options.Add(new TMP_Dropdown.OptionData(label));
            optionToCharacterIndex.Add(i);
        }

        dropdown.AddOptions(options);

        int current = selectedIndex[slot];
        int dropdownValue = optionToCharacterIndex.IndexOf(current);
        dropdown.value = dropdownValue >= 0 ? dropdownValue : 0;
        dropdown.RefreshShownValue();

        dropdown.onValueChanged.AddListener(v =>
        {
            if (optionToCharacterIndex[v] == -1) return;
            OnHeroSelected(slot, optionToCharacterIndex[v]);
        });
    }



    bool IsUsedByOtherSlot(int characterIndex, int currentSlot)
    {
        for (int i = 0; i < selectedIndex.Length; i++)
        {
            if (i == currentSlot) continue;
            if (selectedIndex[i] == characterIndex) return true;
        }
        return false;
    }

    public void OnHeroSelected(int slotIndex, int characterIndex)
    {
        selectedIndex[slotIndex] = characterIndex;

        HeroStateMachine hsm = heroSlots[slotIndex];
        if (hsm == null)
        {
            Debug.LogError($"Hero slot {slotIndex} belum di-assign!");
            return;
        }

        CharacterData data = CharacterDatabase.Instance.characters[characterIndex];
        if (data == null)
        {
            Debug.LogError("CharacterData null!");
            return;
        }

        // 🔥 AMBIL BaseHero YANG SUDAH ADA DI PREFAB
        BaseHero hero = hsm.hero; // <- pastikan ini reference ke BaseHero prefab

        // 🔥 UPDATE STAT SAJA
        hero.ApplyCharacterData(data);
        hero.InitBattleState();
        // OPTIONAL: update UI hero


        RefreshAllDropdowns();
    }



    public bool IsPartyValid()
    {
        for (int i = 0; i < selectedIndex.Length; i++)
        {
            if (selectedIndex[i] < 0)
                return false;
        }
        return true;
    }

}
