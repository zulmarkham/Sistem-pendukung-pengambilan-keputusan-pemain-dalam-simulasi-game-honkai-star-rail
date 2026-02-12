using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class BattleStateMachine : MonoBehaviour
{
    public enum PerformAction
    {
        WAIT,
        PARTY_SELECTION,
        TAKEACTION,
        PERFORMACTION,
        CHECKALIVE,
        WIN,
        LOSE
    }
    public SkillPoints skillPoint = new SkillPoints();

    public PerformAction battleStates;

    public enum HeroGUI
    {
        ACTIVATE,
        WAITING,
        DONE
    }
    public HeroGUI HeroInput;

    public List<HandleTurn> PerformList = new List<HandleTurn>();
    public List<HandleTurn> QueueList = new List<HandleTurn>();
    public List<GameObject> HeroesInBattle = new List<GameObject>();
    public List<GameObject> EnemysInBattle = new List<GameObject>();
    public List<GameObject> AllUnits = new List<GameObject>();

    public float actionValueMax = 10000f; 

    public List<GameObject> HeroesToManage = new List<GameObject>();
    private HandleTurn HeroesChoice;
    public GameObject enemyButton;
    public GameObject heroButton; 
    public Transform Spacer;

    public GameObject AttackPanel;
    public GameObject EnemySelectPanel;
    public GameObject SkillPanel;
    public GameObject HeroSelectPanel; 

    public Transform ActionSpacer;
    public Transform SkillSpacer;
    public Transform BackButtonSpacer;
    public Transform DecisionButtonSpacer;
    public Transform HeroSpacer; 
    public GameObject actionButton;
    public GameObject skillButton;
    public GameObject ultimateButton;
    public GameObject backButton;
    public GameObject decisionButton;
    public FuzzyDecisionPanelUI fuzzyUI;
    public BattleResultUI resultUI;

    private List<GameObject> atkBtns = new List<GameObject>();
    private List<GameObject> enemyBtns = new List<GameObject>();
    private List<GameObject> heroBtns = new List<GameObject>(); 
    private List<GameObject> backBtns = new List<GameObject>();
    private List<GameObject> decisionBtns = new List<GameObject>();
    private int currentUnitIndex = 0;

    void Start()
    {
        EnemyDatabase.Instance.ResetBossAssignment();
        
        BattleRuntime.BattleActive = false;
        battleStates = PerformAction.PARTY_SELECTION;

        EnemysInBattle.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
        HeroesInBattle.AddRange(GameObject.FindGameObjectsWithTag("Hero"));
        HeroInput = HeroGUI.ACTIVATE;

        AttackPanel.SetActive(false);
        EnemySelectPanel.SetActive(false);
        SkillPanel.SetActive(false);
        HeroSelectPanel.SetActive(false);
    }
    public void StartBattleAfterSelection()
    {
        foreach (GameObject heroGO in HeroesInBattle)
        {
            HeroStateMachine hsm = heroGO.GetComponent<HeroStateMachine>();
            if (hsm.hero != null)
            {
                hsm.CreateCharaPanel();
                var floating = heroGO.GetComponentInChildren<UnitFloatingName>(true);
                if (floating != null)
                    floating.Show();

            }
        }
        int enemyCounter = 1;
        
        foreach (GameObject enemyGO in EnemysInBattle)
        {
            EnemyStateMachine esm = enemyGO.GetComponent<EnemyStateMachine>();
            if (esm != null && esm.enemyID == 0)
            {
                esm.enemyID = enemyCounter;
                enemyCounter++;
            }
            var floating = enemyGO.GetComponentInChildren<UnitFloatingName>(true);
            if (floating != null)
                floating.Show();
        }

        BattleRuntime.BattleActive = true;
        EnemyButton();
        HeroButton();

        AllUnits = GetSortedUnitByActionValue(HeroesInBattle, EnemysInBattle);
        FillQueueList(AllUnits);

        skillPoint.SkillPointInit(3);
        Debug.Log("Skill Point Awal = " + skillPoint.currentSkillPoints);

        currentUnitIndex = 0;
        HeroInput = HeroGUI.ACTIVATE;
        battleStates = PerformAction.TAKEACTION;
    }


    void Update()
    {
        switch (battleStates)
        {
            case (PerformAction.WAIT):
                break;

            case PerformAction.PARTY_SELECTION:
                break;

            case (PerformAction.TAKEACTION):
                if (QueueList.Count > 0 && currentUnitIndex < QueueList.Count)
                {
                    StartTurn();
                    battleStates = PerformAction.PERFORMACTION;
                }
                break;

            case (PerformAction.PERFORMACTION):
                break;

            case (PerformAction.CHECKALIVE):
                if (HeroesInBattle.Count < 1)
                {
                    battleStates = PerformAction.LOSE;
                }
                else if (EnemysInBattle.Count < 1)
                {
                    battleStates = PerformAction.WIN;
                }
                else
                {
                    ClearAttackPanel();
                    HeroInput = HeroGUI.ACTIVATE;
                }
                break;

            case (PerformAction.LOSE):
                {
                    Debug.Log("you lose the battle");
                    resultUI.ShowLose();
                }
                break;

            case (PerformAction.WIN):
                {
                    Debug.Log("you win the battle");
                    for (int i  = 0; i < HeroesInBattle.Count; i++)
                    {
                        HeroesInBattle[i].GetComponent<HeroStateMachine>().currentState = HeroStateMachine.TurnState.WAITING;
                    }

                    resultUI.ShowWin();
                }
                break;
        }

        switch (HeroInput)
        {
            case (HeroGUI.ACTIVATE):
                if (HeroesToManage.Count > 0)
                {
                    HeroesToManage[0].transform.Find("Selector").gameObject.SetActive(true);
                    HeroesChoice = new HandleTurn();

                    AttackPanel.SetActive(true);
                    CreateAttackButtons();
                    CreateBackButton();
                    CreateDecisionButton();
                    HeroInput = HeroGUI.WAITING;
                }
                break;

            case (HeroGUI.WAITING):
                break;

            case (HeroGUI.DONE):
                HeroInputDone();
                ExecuteHeroAction();             
                break;
        }
    }

    void StartTurn()
    {
        while (currentUnitIndex < QueueList.Count)
        {
            GameObject unit = QueueList[currentUnitIndex].AttackersGameObject;

            if (unit != null && unit.CompareTag("DeadEnemy") == false)
                break;

            currentUnitIndex++;
        }

        if (currentUnitIndex >= QueueList.Count)
        currentUnitIndex = 0;

        HandleTurn currentTurn = QueueList[currentUnitIndex];

        if (currentTurn.Type == "Hero")
        {
            HeroesToManage.Clear();
            HeroesToManage.Add(currentTurn.AttackersGameObject);
            HeroInput = HeroGUI.ACTIVATE;
        }
        else if (currentTurn.Type == "Enemy")
        {
            EnemyStateMachine ESM = currentTurn.AttackersGameObject.GetComponent<EnemyStateMachine>();
            if (ESM.currentState == EnemyStateMachine.TurnState.DEAD)
                return;

            ESM.currentState = EnemyStateMachine.TurnState.CHOOSEACTION;
        }
    }

    public IEnumerator NextTurnDelay()
    {
        yield return new WaitForSeconds(1f);
        NextTurn();
    }

    public void NextTurn()
    {
        currentUnitIndex++;
        if (currentUnitIndex >= QueueList.Count)
        {
            currentUnitIndex = 0;
        }

        battleStates = PerformAction.TAKEACTION;
    }
    void ExecuteHeroAction()
    {
        if (PerformList.Count == 0)
            return;

        HandleTurn action = PerformList[0];

        if (action.Type != "Hero")
            return;

        HeroStateMachine HSM = action.AttackersGameObject.GetComponent<HeroStateMachine>();
        HSM.EnemyToAttack = action.AttackersTarget;
        HSM.currentState = HeroStateMachine.TurnState.ACTION;

        battleStates = PerformAction.PERFORMACTION;
    }

    List<GameObject> GetSortedUnitByActionValue(List<GameObject> Heroes, List<GameObject> Enemy)
    {
        AllUnits.Clear();   
        AllUnits.AddRange(Heroes);
        AllUnits.AddRange(Enemy);

        List<GameObject> sorted = AllUnits.OrderBy(u => u.GetComponent<HeroStateMachine>() != null ? u.GetComponent<HeroStateMachine>().hero.baseActionValue : u.GetComponent<EnemyStateMachine>().enemy.baseActionValue)
        .ThenByDescending(u => u.GetComponent<HeroStateMachine>() != null ?
                               u.GetComponent<HeroStateMachine>().hero.baseSpeed :
                               u.GetComponent<EnemyStateMachine>().enemy.baseSpeed).ToList();

        return sorted;
    }
    void FillQueueList(List<GameObject> sortedUnits)
    {
        QueueList.Clear();

        foreach (GameObject unit in sortedUnits)
        {
            HandleTurn sortedTurn = new HandleTurn();

            if (unit.GetComponent<HeroStateMachine>() != null)
            {
                var hero = unit.GetComponent<HeroStateMachine>().hero;
                sortedTurn.Type = "Hero";
                sortedTurn.Attacker = hero.name;
                sortedTurn.AttackersGameObject = unit;
                sortedTurn.baseActionValue = hero.baseActionValue;
                sortedTurn.baseSpeed = hero.baseSpeed;
            }
            else if (unit.GetComponent<EnemyStateMachine>() != null)
            {
                var enemy = unit.GetComponent<EnemyStateMachine>().enemy;
                sortedTurn.Type = "Enemy";
                sortedTurn.Attacker = enemy.name;
                sortedTurn.AttackersGameObject = unit;
                sortedTurn.baseActionValue = enemy.baseActionValue;
                sortedTurn.baseSpeed = enemy.baseSpeed;
            }

            QueueList.Add(sortedTurn);
        }
    }

    public void CollectActions(HandleTurn input)
    {
        PerformList.Add(input);
    }

    #region Enemy/Hero button lists
    public void EnemyButton()
    {
        foreach (GameObject enemyBtn in enemyBtns)
        {
            Destroy(enemyBtn);
        }
        enemyBtns.Clear();

        for (int i = 0; i < EnemysInBattle.Count; i++)
        {
            GameObject enemy = EnemysInBattle[i];

            GameObject newButton = Instantiate(enemyButton) as GameObject;
            EnemySelectButton button = newButton.GetComponent<EnemySelectButton>();

            EnemyStateMachine esm = enemy.GetComponent<EnemyStateMachine>();

            button.EnemyPrefab = enemy;

            TextMeshProUGUI buttonText = newButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>();
            buttonText.text = "Enemy " + esm.enemyID;

            newButton.transform.SetParent(Spacer, false);
            enemyBtns.Add(newButton);
        }
    }

    public void HeroButton()
    {
        // create hero buttons for selection (but panel stays hidden)
        foreach (GameObject hb in heroBtns)
        {
            Destroy(hb);
        }
        heroBtns.Clear();

        for (int i = 0; i < HeroesInBattle.Count; i++)
        {
            GameObject heroGO = HeroesInBattle[i];

            GameObject newButton = Instantiate(heroButton) as GameObject;
            AllySelectButton button = newButton.GetComponent<AllySelectButton>();
            button.HeroPrefab = heroGO;

            TextMeshProUGUI buttonText = newButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>();
            buttonText.text = heroGO.GetComponent<HeroStateMachine>().hero.characterName;

            newButton.transform.SetParent(HeroSpacer, false);
            heroBtns.Add(newButton);
        }
    }
    #endregion

    // Basic attack
    public void Input1() //attackButton
    {
        HeroesChoice.Attacker = HeroesToManage[0].name;
        HeroesChoice.AttackersGameObject = HeroesToManage[0];
        HeroesChoice.Type = "Hero";
        HeroesChoice.choosenAttack = HeroesToManage[0].GetComponent<HeroStateMachine>().hero.Attacks[0];

        AttackPanel.SetActive(false);
        // basic attack always targets enemy
        EnemySelectPanel.SetActive(true);
    }

    // Enemy selected
    public void Input2(GameObject ChoosenEnemy) //enemySelection
    {
        HeroesChoice.AttackersTarget = ChoosenEnemy;
        HeroInput = HeroGUI.DONE;
    }

    // Hero selected (ally target)
    public void Input2Hero(GameObject chosenHero)
    {
        HeroesChoice.AttackersTarget = chosenHero;
        HeroInput = HeroGUI.DONE;
    }

    // skill button tapped
    public void Input3(BaseAttack choosenSkillAttack) //choosen skill attack (skill/ultimate)
    {

        if (choosenSkillAttack == null)
            return;

        if (choosenSkillAttack.category == BaseAttack.AttackCategory.Damage)
        {
            // check skill points
            if (skillPoint.currentSkillPoints <= 0)
            {
                Debug.Log("Skill Point tidak cukup!");
                return;
            }
        }
        else
        {
            if (skillPoint.currentSkillPoints <= 0)
            {
                Debug.Log("Skill Point tidak cukup!");
                return;
            }
        }

        HeroesChoice.Attacker = HeroesToManage[0].name;
        HeroesChoice.AttackersGameObject = HeroesToManage[0];
        HeroesChoice.Type = "Hero";
        HeroesChoice.choosenAttack = choosenSkillAttack;

        if (choosenSkillAttack.targetType == BaseAttack.AttackTarget.Enemy)
        {
            SkillPanel.SetActive(false);
            EnemySelectPanel.SetActive(true);
            EnemyButton();
        }
        else if (choosenSkillAttack.targetType == BaseAttack.AttackTarget.Ally ||
                 choosenSkillAttack.targetType == BaseAttack.AttackTarget.Self)
        {
            SkillPanel.SetActive(false);
            HeroSelectPanel.SetActive(true);
            HeroButton();
        }
    }

    public void Input4() // switch to skill
    {
        AttackPanel.SetActive(false);
        SkillPanel.SetActive(true);
    }

    public void Input5() //back Button
    {
        if (SkillPanel.activeSelf)
        {
            SkillPanel.SetActive(false);
            AttackPanel.SetActive(true);
        }
        else if (EnemySelectPanel.activeSelf)
        {
            EnemySelectPanel.SetActive(false);
            AttackPanel.SetActive(true);
        }
        else if (HeroSelectPanel.activeSelf)
        {
            HeroSelectPanel.SetActive(false);
            AttackPanel.SetActive(true);
        }
    }

    public void Input6() //ultimate
    {
        var hsm = HeroesToManage[0].GetComponent<HeroStateMachine>();
        var hero = hsm.hero;

        HeroesChoice.Attacker = hsm.name;
        HeroesChoice.AttackersGameObject = hsm.gameObject;
        HeroesChoice.Type = "Hero";

        BaseAttack ultimate = hero.UltimateAttack[0];
        ultimate.isUltimate = true;

        switch (hero.role)
            {
                case BaseHero.roleType.DPS:
                    ultimate.category = BaseAttack.AttackCategory.Damage;
                    ultimate.targetType = BaseAttack.AttackTarget.Enemy;
                    break;

                case BaseHero.roleType.SUPPORT:
                    ultimate.category = BaseAttack.AttackCategory.Buff;
                    ultimate.targetType = BaseAttack.AttackTarget.Ally;
                    break;

                case BaseHero.roleType.TANKHEALER:
                    ultimate.category = BaseAttack.AttackCategory.Heal;
                    ultimate.targetType = BaseAttack.AttackTarget.Ally;
                    break;
            }
            
        HeroesChoice.choosenAttack = ultimate;

        AttackPanel.SetActive(false);
        EnemySelectPanel.SetActive(false);
        HeroSelectPanel.SetActive(false);
        SkillPanel.SetActive(false);

        if (HeroesChoice.choosenAttack.targetType == BaseAttack.AttackTarget.Enemy)
        {
            EnemySelectPanel.SetActive(true);
            EnemyButton();
        }
        else // Ally atau Self
        {
            HeroSelectPanel.SetActive(true);
            HeroButton();
        }
    }

    public void Input7() // Fuzzy Recommendation
    {
        if (HeroesToManage.Count == 0) return;

        GameObject heroGO = HeroesToManage[0];
        HeroStateMachine HSM = heroGO.GetComponent<HeroStateMachine>();
        BaseHero hero = HSM.hero;

        int currentSP = skillPoint.currentSkillPoints;
        int maxSP = skillPoint.maxSkillPoints;
        float energy = hero.currentEnergy;

        float targetHP = 0f;
        float targetMaxHP = 0f;
        bool canUseSkill = hero.SkillAttack.Count > 0 && currentSP > 0;
        bool canUseUlt = hero.UltimateAttack.Count > 0 && hero.currentEnergy >= hero.energy;

        HeroAction recommendedAction = HeroAction.BasicAttack;

        switch (hero.role)
        {
            case BaseHero.roleType.DPS:
                TryGetLowestHpEnemy(out targetHP, out targetMaxHP);
                var dpsFuzzy = new FuzzyDPS();
                var dpsResult = dpsFuzzy.Decide(currentSP, maxSP, energy, targetHP, targetMaxHP, canUseSkill, canUseUlt);
                recommendedAction = dpsResult.Action;
                fuzzyUI.Show(
                    dpsResult.role,
                    currentSP,
                    maxSP,
                    energy,
                    dpsResult.HpPercentUI,

                    dpsResult.MuSPUI,
                    dpsResult.MuEnergyUI,
                    dpsResult.MuHPUI,

                    dpsResult.CanUseSkillUI,
                    dpsResult.CanUseUltUI,

                    dpsResult.RuleFiredText,

                    dpsResult.SumAlphaUI,
                    dpsResult.SumAlphaZUI,
                    dpsResult.CrispZ,

                    dpsResult.Action.ToString()
                );

                break;

            case BaseHero.roleType.SUPPORT:
                TryGetLowestHpEnemy(out targetHP, out targetMaxHP);
                var supportFuzzy = new FuzzySupport();
                var supportResult = supportFuzzy.Decide(currentSP, maxSP, energy, targetHP, targetMaxHP, canUseSkill, canUseUlt);
                recommendedAction = (HeroAction)supportResult.Action;
                fuzzyUI.Show(
                    supportResult.role,
                    currentSP,
                    maxSP,
                    energy,
                    supportResult.HpPercentUI,

                    supportResult.MuSPUI,
                    supportResult.MuEnergyUI,
                    supportResult.MuHPUI,

                    supportResult.CanUseSkillUI,
                    supportResult.CanUseUltUI,

                    supportResult.RuleFiredText,

                    supportResult.SumAlphaUI,
                    supportResult.SumAlphaZUI,
                    supportResult.CrispZ,

                    supportResult.Action.ToString()
                );

                break;

            case BaseHero.roleType.TANKHEALER:
                TryGetLowestHpAlly(out targetHP, out targetMaxHP);
                var tankFuzzy = new FuzzyTankHealer();
                var tankResult = tankFuzzy.Decide(currentSP, maxSP, energy, targetHP, targetMaxHP, canUseSkill, canUseUlt);
                recommendedAction = (HeroAction)tankResult.Action;
                fuzzyUI.Show(
                    tankResult.role,
                    currentSP,
                    maxSP,
                    energy,
                    tankResult.HpPercentUI,

                    tankResult.MuSPUI,
                    tankResult.MuEnergyUI,
                    tankResult.MuHPUI,

                    tankResult.CanUseSkillUI,
                    tankResult.CanUseUltUI,

                    tankResult.RuleFiredText,

                    tankResult.SumAlphaUI,
                    tankResult.SumAlphaZUI,
                    tankResult.CrispZ,

                    tankResult.Action.ToString()
                );

                break;
        }
    }
    private bool TryGetLowestHpEnemy(out float hp, out float maxHp)
    {   
        hp = 0f;
        maxHp = 0f;

        if (EnemysInBattle == null || EnemysInBattle.Count == 0)
            return false;

        GameObject enemyTarget = EnemysInBattle
        .OrderBy(e =>
        {
            var enemy = e.GetComponent<EnemyStateMachine>().enemy;
            return enemy.currentHP / enemy.baseHP; // persentase HP
        })
        .FirstOrDefault();

        if (enemyTarget == null)
            return false;

        var enemy = enemyTarget.GetComponent<EnemyStateMachine>().enemy;
        hp = enemy.currentHP;
        maxHp = enemy.baseHP;
        Debug.Log("Enemy dengan HP terendah (%): " + enemyTarget.name + " => " + (hp / maxHp * 100f) + "%");


        return true;
    }

    private bool TryGetLowestHpAlly(out float hp, out float maxHp)
    {
        hp = 0f;
        maxHp = 0f;

        if (HeroesInBattle == null || HeroesInBattle.Count == 0)
            return false;

        GameObject allyTarget = HeroesInBattle
        .OrderBy(h =>
        {
            var hero = h.GetComponent<HeroStateMachine>().hero;
            return (hero.currentHP / hero.baseHP); // gunakan persentase
        })
        .FirstOrDefault();

        if (allyTarget == null)
            return false;

        var hero = allyTarget.GetComponent<HeroStateMachine>().hero;
        hp = hero.currentHP;
        maxHp = hero.baseHP;
        Debug.Log("Enemy dengan HP terendah (%): " + allyTarget.name + " => " + (hp / maxHp * 100f) + "%");

        return true;
    }



    void HeroInputDone()
    {
        PerformList.Add(HeroesChoice);
        ClearAttackPanel();

        HeroesToManage[0].transform.Find("Selector").gameObject.SetActive(false);
        HeroesToManage.RemoveAt(0);
        HeroInput = HeroGUI.ACTIVATE;
    }

    void ClearAttackPanel()
    {
        EnemySelectPanel.SetActive(false);
        SkillPanel.SetActive(false);
        AttackPanel.SetActive(false);
        HeroSelectPanel.SetActive(false);

        foreach (GameObject atkBtn in atkBtns)
        {
            Destroy(atkBtn);
        }
        atkBtns.Clear();

        foreach (GameObject backBtn in backBtns)
        {
            Destroy(backBtn);
        }
        backBtns.Clear();
    }

    void CreateAttackButtons()
    {
        // clear previous buttons
        foreach (GameObject b in atkBtns) Destroy(b);
        atkBtns.Clear();

        HeroStateMachine HSM = HeroesToManage[0].GetComponent<HeroStateMachine>();
        BaseHero hero = HSM.hero;

        // Basic attack (always enemy target)
        GameObject AttackButton = Instantiate(actionButton) as GameObject;
        TextMeshProUGUI AttackButtonText = AttackButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>();
        AttackButtonText.text = "Basic Attack";
        AttackButton.GetComponent<Button>().onClick.AddListener(() => Input1());
        AttackButton.transform.SetParent(ActionSpacer, false);
        atkBtns.Add(AttackButton);

        // Skill button (open skill list)
        GameObject SkillAttackButton = Instantiate(actionButton) as GameObject;
        TextMeshProUGUI SkillAttackButtonText = SkillAttackButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>();
        SkillAttackButtonText.text = "Skill";
        SkillAttackButton.GetComponent<Button>().onClick.AddListener(() => Input4());
        SkillAttackButton.transform.SetParent(ActionSpacer, false);
        atkBtns.Add(SkillAttackButton);

        // Ultimate button
        GameObject UltimateAttackButton = Instantiate(actionButton) as GameObject;
        TextMeshProUGUI UltimateAttackButtonText = UltimateAttackButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>();
        UltimateAttackButtonText.text = "Ultimate";

        if (hero.currentEnergy >= hero.energy && hero.UltimateAttack.Count > 0)
        {
            UltimateAttackButton.GetComponent<Button>().interactable = true;
            UltimateAttackButton.GetComponent<Button>().onClick.AddListener(()=> Input6());
        }
        else
        {
            UltimateAttackButton.GetComponent<Button>().interactable = false;
        }
        UltimateAttackButton.transform.SetParent(ActionSpacer, false);
        atkBtns.Add(UltimateAttackButton);

        // Create Skill list according to role & attack target type
        // clear previous children in SkillSpacer (if any)
        foreach (Transform child in SkillSpacer)
        {
            Destroy(child.gameObject);   // 🧹 clear skill buttons sebelum diisi ulang
        }

        // Determine which skills to show based on hero.role
        List<BaseAttack> filteredSkills = new List<BaseAttack>();

        if (hero.SkillAttack != null && hero.SkillAttack.Count > 0)
        {
            foreach (BaseAttack s in hero.SkillAttack)
            {
                switch (hero.role)
                {
                    case BaseHero.roleType.DPS:
                        // DPS hanya damage ke enemy
                        if (s.category == BaseAttack.AttackCategory.Damage &&
                            s.targetType == BaseAttack.AttackTarget.Enemy)
                        {
                            filteredSkills.Add(s);
                        }
                        break;

                    case BaseHero.roleType.SUPPORT:
                        // Support hanya buff ke ally
                        if (s.category == BaseAttack.AttackCategory.Buff &&
                            (s.targetType == BaseAttack.AttackTarget.Ally ||
                            s.targetType == BaseAttack.AttackTarget.Self))
                        {
                            filteredSkills.Add(s);
                        }
                        break;

                    case BaseHero.roleType.TANKHEALER:
                        // TankHealer hanya heal & shield
                        if ((s.category == BaseAttack.AttackCategory.Heal ||
                            s.category == BaseAttack.AttackCategory.Shield) &&
                            (s.targetType == BaseAttack.AttackTarget.Ally ||
                            s.targetType == BaseAttack.AttackTarget.Self))
                        {
                            filteredSkills.Add(s);
                        }
                        break;
                }
            }
        }

        if (filteredSkills.Count > 0)
        {
            foreach (BaseAttack SkillAtk in filteredSkills)
            {
                GameObject SkillButton = Instantiate(skillButton) as GameObject;
                TextMeshProUGUI skillButtonText = SkillButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>();
                skillButtonText.text = SkillAtk.attackName;

                // local capture
                BaseAttack tempSkill = SkillAtk;
                Button btn = SkillButton.GetComponent<Button>();
                btn.onClick.AddListener(() => {
                    Input3(tempSkill);
                });

                SkillButton.transform.SetParent(SkillSpacer, false);
            }
        }
        else
        {
            SkillAttackButton.GetComponent<Button>().interactable = false;
        }
    }

    void CreateBackButton()
    {
        GameObject BackActButton = Instantiate(backButton) as GameObject;
        TextMeshProUGUI BackActButtonText = BackActButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>();
        BackActButtonText.text = "Back";
        BackActButton.GetComponent<Button>().onClick.AddListener(() => Input5());
        BackActButton.transform.SetParent(BackButtonSpacer, false);
        backBtns.Add(BackActButton);
    }

    void CreateDecisionButton() //Untuk FuzzyButton
    {

        // Hapus button lama dulu
        foreach (GameObject btn in decisionBtns)
            Destroy(btn);
        decisionBtns.Clear();

        GameObject DecisionButton = Instantiate(decisionButton) as GameObject;
        TextMeshProUGUI DecisionButtonText = DecisionButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>();
        DecisionButtonText.text = "Recommendation";
        DecisionButton.GetComponent<Button>().onClick.AddListener(() => Input7());
        DecisionButton.transform.SetParent(DecisionButtonSpacer, false);
        decisionBtns.Add(DecisionButton);        
    }
}
