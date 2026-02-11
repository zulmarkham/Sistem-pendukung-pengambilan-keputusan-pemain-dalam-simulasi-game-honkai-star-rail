using UnityEngine;
using TMPro;

public class FuzzyDecisionPanelUI : MonoBehaviour
{
    [Header("Panel Root")]
    public GameObject panel;

    [Header("Texts")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI inputText;
    public TextMeshProUGUI membershipText;
    public TextMeshProUGUI ruleFiredText;
    public TextMeshProUGUI defuzzText;
    public TextMeshProUGUI decisionText;

    void Start()
    {
        panel.SetActive(false);
    }

    public void Show(
        string role,
        int currentSP, int maxSP,
        float energy,
        float hpPercent,

        float[] muSP,
        float[] muEnergy,
        float[] muHP,
        bool canUseSkill,
        bool canUseUlt,
        string ruleFiredTextValue,
        float sumAlpha,
        float sumAlphaZ,
        float crispZ,

        string finalDecision
    )
    {
        panel.SetActive(true);

        titleText.text = $"FUZZY DECISION ({role})";

        inputText.text =
            "INPUT\n" +
            $"SP        : {currentSP}/{maxSP}\n" +
            $"Energy    : {energy:F1}\n" +
            $"Target HP : {hpPercent:F1}%\n" +
            $"Skill     : {(canUseSkill ? "AVAILABLE" : "NOT READY")}\n" +
            $"Ultimate  : {(canUseUlt ? "AVAILABLE" : "NOT READY")}";

        membershipText.text =
            "Membership Input\n" +
            $"SP      → L:{muSP[0]:F2} M:{muSP[1]:F2} H:{muSP[2]:F2}\n" +
            $"Energy  → L:{muEnergy[0]:F2} M:{muEnergy[1]:F2} H:{muEnergy[2]:F2}\n" +
            $"HP      → L:{muHP[0]:F2} M:{muHP[1]:F2} H:{muHP[2]:F2}";

        ruleFiredText.text =
            "RULE FIRED\n" +
            (string.IsNullOrEmpty(ruleFiredTextValue)
                ? "No rule fired"
                : ruleFiredTextValue);


        defuzzText.text =
            "Defuzzifikasi\n" +
            $"Σα  : {sumAlpha:F2}\n" +
            $"ΣαZ : {sumAlphaZ:F2}\n" +
            $"Crisp Z   : {crispZ:F2}";

        decisionText.text =
            $"FINAL DECISION\n{finalDecision}";
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
    }
}
