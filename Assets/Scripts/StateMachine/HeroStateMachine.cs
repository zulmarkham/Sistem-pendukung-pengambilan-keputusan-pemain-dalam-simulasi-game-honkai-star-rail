using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HeroStateMachine : MonoBehaviour
{
    

    private BattleStateMachine BSM;
    public BaseHero hero;

    public enum TurnState
    {
        PROCESSING,
        ADDTODOLIST,
        WAITING,
        SELECTING,
        ACTION,
        DEAD
    }

    public TurnState currentState;

    //for the UI
    public HealthBar uiHealthBar;
    public GameObject Selector;
    public GameObject EnemyToAttack;
    public GameObject AllyToTarget;
    
    private bool actionStarted = false;
    private Vector3 startPosition;
    private float animSpeed = 10f;
    private bool alive = true;
    private HeroPanelStats stats;
    public GameObject CharaPanel;
    private Transform HeroPanelSpacer;


    void Start()
    {
        HeroPanelSpacer = GameObject.Find("BattlePanel").transform.Find("CharaPanel").transform.Find("HeroPanelSpacer");

        startPosition = transform.position;
        Selector.SetActive(false);

        BSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();
        currentState = TurnState.WAITING;

    }
    public void ApplySelectedHero(BaseHero newHero)
    {
        hero = newHero;
        CreateCharaPanel();
    }

    #if UNITY_EDITOR
    // void OnValidate()
    // {

    //     if (Application.isPlaying) return;

    //     if (hero == null || string.IsNullOrEmpty(hero.characterName))
    //         return;

    //     CharacterData data = null;

    //     // ambil dari database jika ada
    //     if (CharacterDatabase.Instance != null)
    //     {
    //         data = CharacterDatabase.Instance.GetCharacter(hero.characterName);
    //     }
    //     else
    //     {
    //         // fallback CSV (EDITOR ONLY)
    //         TextAsset csv = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(
    //             "Assets/Resources/Data/data_characters.csv");

    //         if (csv != null)
    //         {
    //             string[] lines = csv.text.Split(
    //                 new[] { '\n', '\r' },
    //                 System.StringSplitOptions.RemoveEmptyEntries);

    //             for (int i = 1; i < lines.Length; i++)
    //             {
    //                 string[] cols = lines[i].Split(',');
    //                 if (cols.Length < 7) continue;

    //                 if (cols[0].Trim() == hero.characterName)
    //                 {
    //                     data = new CharacterData();
    //                     data.name = cols[0].Trim();
    //                     System.Enum.TryParse(cols[1].Trim(), out data.role);
    //                     System.Enum.TryParse(cols[2].Trim(), out data.element);
    //                     float.TryParse(cols[3].Trim(), out data.baseATK);
    //                     float.TryParse(cols[4].Trim(), out data.baseHP);
    //                     float.TryParse(cols[5].Trim(), out data.baseSpeed);
    //                     float.TryParse(cols[6].Trim(), out data.energy);
    //                     break;
    //                 }
    //             }
    //         }
    //     }

    //     if (data != null)
    //     {
    //         hero.ApplyCharacterData(data);

    //         // editor preview saja
    //         hero.baseAVunit();
    //         hero.currentActionValue = hero.baseActionValue;
    //     }
    // }
    #endif



    void Update()
    {
        if (!BattleRuntime.BattleActive)
        return; 
        switch (currentState)
        {
            case (TurnState.ACTION):
            if (!actionStarted)
                StartCoroutine(TimeForAction());
                break;



            case (TurnState.DEAD):
                if (!alive)
                {
                    return;
                }
                else
                {
                    this.gameObject.tag = "DeadHero";
                    BSM.HeroesInBattle.Remove(this.gameObject);
                    BSM.HeroesToManage.Remove(this.gameObject); 
                    Selector.SetActive(false);
                    BSM.AttackPanel.SetActive(false);
                    BSM.EnemySelectPanel.SetActive(false);
                    for (int i = 0; i < BSM.QueueList.Count; i++)
                    {
                        if (BSM.QueueList[i].AttackersGameObject == this.gameObject)
                        {
                            BSM.QueueList.Remove(BSM.QueueList[i]);
                        }
                    }
                    this.gameObject.GetComponent<MeshRenderer>().material.color = new Color32(105, 105, 105, 255);

                    BSM.battleStates = BattleStateMachine.PerformAction.CHECKALIVE;

                    alive = false;
                }
                break;

            case (TurnState.WAITING):
                break;

        }
    }

    private IEnumerator TimeForAction()
    {
        if (actionStarted)
        {
            yield break;
        }

        actionStarted = true;

        // Vector3 enemyPosition = new Vector3(EnemyToAttack.transform.position.x - 1.5f, EnemyToAttack.transform.position.y, EnemyToAttack.transform.position.z);

        // while (MoveTowardsEnemy(enemyPosition))
        // {
        //     yield return null;
        // }
        if (EnemyToAttack != null)
        {
            // gerak ke musuh (attack)
            Vector3 targetPos = EnemyToAttack.transform.position + new Vector3(-1.5f, 0, 0);

            while (MoveTowardsEnemy(targetPos))
                yield return null;
        }
        else if (AllyToTarget != null)
        {
            // gerak ke ally (buff / heal / shield)
            Vector3 targetPos = AllyToTarget.transform.position + new Vector3(-1.0f, 0, 0);

            while (MoveTowardsEnemy(targetPos))
                yield return null;
        }


        yield return new WaitForSeconds(0.5f);
        doDamage();

        Vector3 firstPosition = startPosition;
        while (MoveTowardsStart(firstPosition))
        {
            yield return null;
        }

        if (BSM.PerformList.Count > 0)
        {
            BSM.PerformList.RemoveAt(0);
        }


        yield return new WaitForSeconds(0.5f);

        // Reset & lanjut ke giliran berikutnya
        actionStarted = false;
        currentState = TurnState.WAITING;

        if (BSM.battleStates != BattleStateMachine.PerformAction.WIN && BSM.battleStates != BattleStateMachine.PerformAction.LOSE)
        {
            StartCoroutine(BSM.NextTurnDelay());
        }
            

    }
    private bool MoveTowardsEnemy(Vector3 target)
    {
        return target != (transform.position = Vector3.MoveTowards(transform.position, target, animSpeed * Time.deltaTime));

    }
    private bool MoveTowardsStart(Vector3 target)
    {
        return target != (transform.position = Vector3.MoveTowards(transform.position, target, animSpeed * Time.deltaTime));
    }

    public void takeDamage(float getDamageAmount)
    {
        int energyGain = Random.Range(3, 6); // Nilai kecil
        hero.currentEnergy = Mathf.Min(hero.currentEnergy + energyGain, hero.energy);
        float remainingDamage = getDamageAmount;

        // 1️⃣ Shield menyerap damage dulu
        if (hero.currentShield > 0)
        {
            if (hero.currentShield >= remainingDamage)
            {
                // Shield cukup, HP aman
                hero.currentShield -= remainingDamage;
                remainingDamage = 0f;
            }
            else
            {
                // Shield habis, sisa damage ke HP
                remainingDamage -= hero.currentShield;
                hero.currentShield = 0f;
            }
        }

        // 2️⃣ Sisa damage ke HP
        if (remainingDamage > 0)
        {
            hero.currentHP -= remainingDamage;
        }

        // 3️⃣ Clamp HP
        if (hero.currentHP <= 0)
        {
            hero.currentHP = 0;
            currentState = TurnState.DEAD;
        }

        // 4️⃣ Update UI
        UpdateHeroPanel();
    }

    void doDamage()
    {
        var chosenAttack = BSM.PerformList[0].choosenAttack;
        float calc_value = 0f;
        string debugInfo = "";
        float damageMultiplier = 1f;
        bool isUltimate = chosenAttack.isUltimate && hero.currentEnergy >= hero.energy;

        EnemyStateMachine enemySM = null;
        HeroStateMachine allyHSM = null;

        // ambil enemy hanya jika ada
        if (EnemyToAttack != null)
        {
            enemySM = EnemyToAttack.GetComponent<EnemyStateMachine>();
        }

        // ambil ally dari PerformList
        if (BSM.PerformList[0].AttackersTarget != null)
        {
            allyHSM = BSM.PerformList[0].AttackersTarget.GetComponent<HeroStateMachine>();
        }

        // ================= BASIC ATTACK =================
        if (chosenAttack == hero.Attacks[0])
        {
            switch (hero.role)
            {
                case BaseHero.roleType.DPS:
                    damageMultiplier = 1.5f;
                    break;
                case BaseHero.roleType.SUPPORT:
                    damageMultiplier = 1.3f;
                    break;
                case BaseHero.roleType.TANKHEALER:
                    damageMultiplier = 1.2f;
                    break;
            }

            calc_value = hero.currentATK * damageMultiplier;
            debugInfo = $"{hero.characterName} BASIC ({hero.role}) ATK({hero.currentATK}) x {damageMultiplier * 100f}%";

            if (enemySM != null &&
                hero.element.ToString() == enemySM.enemy.weakness.ToString())
            {
                calc_value *= 2f;
                debugInfo += " | WEAKNESS HIT! x2 DAMAGE";
            }

            Debug.Log($"{debugInfo} => TOTAL DMG: {calc_value}");
            enemySM.takeDamage(calc_value);
             ConsumeAtkBuffAfterAttack();
        }
        else
        {
            // ================= ULTIMATE =================
            if (isUltimate)
            {
                // 🔧 FIX INTI BUG:
                // jika ultimate bukan DPS, pastikan enemy TIDAK dipakai
                if (hero.role != BaseHero.roleType.DPS)
                {
                    enemySM = null;
                }

                switch (hero.role)
                {
                    case BaseHero.roleType.DPS:
                    {
                        damageMultiplier = 5.0f;
                        calc_value = hero.currentATK * damageMultiplier;

                        debugInfo = $"{hero.characterName} ULTIMATE DPS ATK({hero.currentATK}) x {damageMultiplier * 100f}%";

                        if (enemySM != null &&
                            hero.element.ToString() == enemySM.enemy.weakness.ToString())
                        {
                            calc_value *= 2f;
                            debugInfo += " | WEAKNESS HIT! x2";
                        }

                        Debug.Log($"{debugInfo} => TOTAL DMG: {calc_value}");
                        enemySM.takeDamage(calc_value);
                        ConsumeAtkBuffAfterAttack();
                        break;
                    }

                    case BaseHero.roleType.SUPPORT:
                    {
                        GameObject allyObj = BSM.PerformList[0].AttackersTarget;
                        if (allyObj == null) break;

                        HeroStateMachine targetHSM = allyObj.GetComponent<HeroStateMachine>();
                        if (targetHSM == null) break;

                        allyHSM.hero.atkBuffMultiplier += 1.0f; //   +100%
                        allyHSM.hero.RecalculateATK();
                        targetHSM.hero.RecalculateATK();

                        Debug.Log(
                            $"{hero.characterName} ULTIMATE BUFF " +
                            $"{targetHSM.hero.characterName} +100% ATK (1 Turn)"
                        );
                        break;
                    }

                    case BaseHero.roleType.TANKHEALER:
                    {
                        GameObject allyObj = BSM.PerformList[0].AttackersTarget;
                        if (allyObj == null) break;

                        HeroStateMachine targetHSM = allyObj.GetComponent<HeroStateMachine>();
                        if (targetHSM == null) break;

                        float healAmount = hero.baseHP * 0.15f;
                        float shieldAmount = hero.baseHP * 0.10f;

                        targetHSM.hero.currentHP += healAmount;
                        if (targetHSM.hero.currentHP > targetHSM.hero.baseHP)
                            targetHSM.hero.currentHP = targetHSM.hero.baseHP;

                        targetHSM.hero.currentShield += shieldAmount;
                        targetHSM.UpdateHeroPanel();

                        Debug.Log(
                            $"{hero.characterName} ULTIMATE TANK " +
                            $"HEAL {healAmount} + SHIELD {shieldAmount}"
                        );
                        break;
                    }
                }

                hero.currentEnergy = 0;
                UpdateHeroPanel();
                return;
            }

            // ================= SKILL =================
            switch (chosenAttack.category)
            {
                case BaseAttack.AttackCategory.Damage:
                {
                    damageMultiplier = 2.5f;
                    calc_value = hero.currentATK * damageMultiplier;

                    debugInfo = $"{hero.characterName} SKILL ({chosenAttack.attackName}) ATK({hero.currentATK}) x {damageMultiplier * 100f}%";

                    if (enemySM != null &&
                        hero.element.ToString() == enemySM.enemy.weakness.ToString())
                    {
                        calc_value *= 2f;
                        debugInfo += " | WEAKNESS HIT! x2 DAMAGE";
                    }

                    Debug.Log($"{debugInfo} => TOTAL DMG: {calc_value}");
                    enemySM.takeDamage(calc_value);
                     ConsumeAtkBuffAfterAttack();
                    break;
                }

                case BaseAttack.AttackCategory.Heal:
                {
                    GameObject allyObj = BSM.PerformList[0].AttackersTarget;
                    if (allyObj == null) break;

                    HeroStateMachine targetHSM = allyObj.GetComponent<HeroStateMachine>();
                    if (targetHSM == null) break;

                    float healAmount = hero.baseHP * 0.10f;

                    targetHSM.hero.currentHP += healAmount;
                    if (targetHSM.hero.currentHP > targetHSM.hero.baseHP)
                        targetHSM.hero.currentHP = targetHSM.hero.baseHP;

                    targetHSM.UpdateHeroPanel();
                    Debug.Log($"{hero.characterName} HEAL {targetHSM.hero.characterName} for {healAmount}");
                    break;
                }

                case BaseAttack.AttackCategory.Shield:
                {
                    GameObject allyObj = BSM.PerformList[0].AttackersTarget;
                    if (allyObj == null) break;

                    HeroStateMachine targetHSM = allyObj.GetComponent<HeroStateMachine>();
                    if (targetHSM == null) break;

                    float shieldAmount = hero.baseHP * 0.05f;
                    targetHSM.hero.currentShield += shieldAmount;

                    targetHSM.UpdateHeroPanel();
                    Debug.Log( $"{hero.characterName} SHIELD {targetHSM.hero.characterName} +{shieldAmount}");
                    break;
                }

                case BaseAttack.AttackCategory.Buff:
                {
                    if (hero.role != BaseHero.roleType.SUPPORT)
                    {
                        Debug.LogWarning($"{hero.characterName} ({hero.role}) tidak boleh menggunakan BUFF");
                        break;
                    }

                    GameObject allyObj = BSM.PerformList[0].AttackersTarget;
                    if (allyObj == null) break;

                    HeroStateMachine targetHSM = allyObj.GetComponent<HeroStateMachine>();
                    if (targetHSM == null) break;

                    allyHSM.hero.atkBuffMultiplier += 0.5f; // +50%
                    allyHSM.hero.RecalculateATK();

                    Debug.Log(
                        $"{hero.characterName} BUFF ATK {targetHSM.hero.characterName} " + $"{allyHSM.hero.characterName} +50% ATK");
                    break;
                }
            }
        }

        updateSkillPoint();
        updateEnergy();
    }
    void ConsumeAtkBuffAfterAttack()
    {
        if (hero.atkBuffMultiplier > 1f)
        {
            hero.atkBuffMultiplier = 1f;
            hero.RecalculateATK();

            Debug.Log($"{hero.characterName} ATK Buff consumed after attack");
        }
    }


    void updateSkillPoint()
    {
        var chosenAttack = BSM.PerformList[0].choosenAttack;


        if (chosenAttack == hero.Attacks[0])
        {
            BSM.skillPoint.gainSkillPoint(1);
            Debug.Log("SP +1 (Basic Attack). Current SP = " + BSM.skillPoint.currentSkillPoints);
            return;
        }

        if (hero.SkillAttack.Contains(chosenAttack))
        {
            BSM.skillPoint.useSkillPoint(1);
            Debug.Log("SP -1 (Skill Attack). Current SP = " + BSM.skillPoint.currentSkillPoints);
            return;
        }
    }

    void updateEnergy()
    {
        var chosenAttack = BSM.PerformList[0].choosenAttack;

        int energyGain;

        if (chosenAttack == hero.Attacks[0])
        {
            energyGain = Random.Range(10, 20); // basic attack
            Debug.Log("Energy from BASIC: +" + energyGain);
        }
        else
        {
            energyGain = Random.Range(30, 40); // skill attack
            Debug.Log("Energy from SKILL: +" + energyGain);
        }

        hero.currentEnergy = Mathf.Min(hero.currentEnergy + energyGain, hero.energy);
        if (hero.currentEnergy == hero.energy)
        {
            Debug.Log(hero.name + " READY FOR ULTIMATE!");
        }

        stats.HeroEnergy.text = "Energy: " + hero.currentEnergy + " / " + hero.energy;

    }

    public void CreateCharaPanel()
    {
        CharaPanel = Instantiate(CharaPanel) as GameObject;
        stats = CharaPanel.GetComponent<HeroPanelStats>();
        stats.HeroName.text = hero.characterName + "(" + hero.role.ToString() + ")";
        stats.HeroHP.text = "HP: " + hero.currentHP;
        stats.HeroEnergy.text = "Energy: " + hero.currentEnergy + " / " + hero.energy;
        stats.HeroShield.text = "Shield: " + hero.currentShield;

        uiHealthBar = stats.HealthBar;
        uiHealthBar.SetMaxHealth((int)hero.baseHP);
        uiHealthBar.SetHealth((int)hero.currentHP);


        CharaPanel.transform.SetParent(HeroPanelSpacer, false);

    }

    void UpdateHeroPanel()
    {
        if (stats == null || uiHealthBar == null || hero == null)
        {
            Debug.LogWarning($"{gameObject.name} UpdateHeroPanel dipanggil tapi UI belum siap.");
            return;
        }
        stats.HeroHP.text = "HP: " + hero.currentHP;
        stats.HeroEnergy.text = "Energy: " + hero.currentEnergy + " / " + hero.energy;
        stats.HeroShield.text = "Shield: " + hero.currentShield;

        uiHealthBar.SetHealth((int)hero.currentHP);

    }

    void ActionValueUpdate(float heroBaseActionValue)

    {

    }

    public void LoadHeroStatsFromDatabase()
    {
        if (hero == null || string.IsNullOrEmpty(hero.name))
            return;

        if (CharacterDatabase.Instance == null) return;

        var data = CharacterDatabase.Instance.GetCharacter(hero.name);

        if (data != null)
        {
            hero.ApplyCharacterData(data);
            Debug.Log($"[Editor] Loaded stats: {hero.name}");
        }
        else
        {
            Debug.LogWarning($"[Editor] Character not found in database: {hero.name}");
        }
    }

// helper kecil untuk index-safe access
    private string GetSafe(string[] arr, int idx)
    {
        if (arr == null || idx < 0 || idx >= arr.Length) return string.Empty;
        return arr[idx].Trim();
    }
    public void LoadHeroStatsEditor()
    {
        if (CharacterDatabase.Instance == null) return;
        var data = CharacterDatabase.Instance.GetCharacter(hero.name);
        if (data != null)
        {
            hero.ApplyCharacterData(data);
            Debug.Log("[Editor] Character updated: " + hero.name);
        }
    }

}
