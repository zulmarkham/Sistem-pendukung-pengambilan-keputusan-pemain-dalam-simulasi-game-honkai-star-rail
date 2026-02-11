using UnityEngine;
using TMPro;

public class UnitFloatingName : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;

    private HeroStateMachine hero;
    private EnemyStateMachine enemy;

    void Awake()
    {
        hero = GetComponentInParent<HeroStateMachine>();
        enemy = GetComponentInParent<EnemyStateMachine>();
        gameObject.SetActive(false);
    }

    public void Show()
    {
        if (hero != null && hero.hero != null)
        {
            nameText.text = hero.hero.characterName;
        }
        else if (enemy != null && enemy.enemy != null)
        {
            nameText.text = enemy.enemy.name;
        }
        else
        {
            return;
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
