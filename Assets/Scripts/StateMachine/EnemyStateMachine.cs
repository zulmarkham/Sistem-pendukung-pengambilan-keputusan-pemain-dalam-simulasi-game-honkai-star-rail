using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyStateMachine : MonoBehaviour
{
    private BattleStateMachine BSM;
    public BaseEnemy enemy;
    public EnemyDatabase enemyDB;
    public int enemyID;

    public enum TurnState
    {
        PROCESSING,
        WAITING,
        CHOOSEACTION,
        ACTION,
        DEAD
    }

    public TurnState currentState;

    private Vector3 startPosition;
    public GameObject Selector;
    public HealthBar uiHealthBar;
    private EnemyPanelStats stats;
    public GameObject EnemyPanel;
    private Transform EnemyPanelSpacer;

    private bool actionStarted = false;
    public GameObject HeroToAttack;
    private float animSpeed = 10f;
    private bool alive = true;

    private HandleTurn currentAction;

    void Start()
    {
        enemyDB = GameObject.FindObjectOfType<EnemyDatabase>();
        BSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();
        
        EnemyPanelSpacer = GameObject.Find("BattlePanel").transform.Find("EnemyPanel").transform.Find("EnemyPanelSpacer");
        AssignEnemyData();
        ApplyBossVisual();
        CreateEnemyPanel();
        currentState = TurnState.WAITING;
        Selector.SetActive(false);

        startPosition = transform.position;
    }

    void Update()
    {
        if (!BattleRuntime.BattleActive)
        return;
        
        switch (currentState)
        {
            case TurnState.ACTION:
                if (!actionStarted)
                    StartCoroutine(TimeForAction());
                break;

            case TurnState.DEAD:
                if (!alive)
                    return;
                HandleDeath();
                break;

            case TurnState.CHOOSEACTION:
                ChooseAction();
                // currentState = TurnState.WAITING;
                break;

            case TurnState.WAITING:
                break;
        }
    }

    void ChooseAction()
    {
        if (BSM == null || BSM.HeroesInBattle == null || BSM.HeroesInBattle.Count == 0)
        {
            Debug.LogWarning("[Enemy] Tidak ada hero yang bisa di-target. Lewati giliran.");
            return;
        }

        if (enemy == null)
        {
            Debug.LogWarning("[Enemy] enemy data null, tidak bisa memilih aksi.");
            return;
        }

        if (enemy.Attacks == null || enemy.Attacks.Count == 0)
        {
            Debug.LogWarning($"{enemy.name} tidak memiliki attack. Lewati giliran.");
            return;
        }

        HandleTurn myAttack = new HandleTurn();
        myAttack.Attacker = enemy.name;
        myAttack.Type = "Enemy";
        myAttack.AttackersGameObject = this.gameObject;

        int heroCount = BSM.HeroesInBattle.Count;
        if (heroCount == 0)
        {
            Debug.LogWarning("[Enemy] HeroesInBattle kosong.");
            return;
        }
        myAttack.AttackersTarget = BSM.HeroesInBattle[Random.Range(0, heroCount)];

        int atkCount = enemy.Attacks.Count;
        if (atkCount == 0)
        {
            Debug.LogWarning($"{enemy.name} memiliki attackCount 0.");
            return;
        }
        int num = Random.Range(0, atkCount);
        myAttack.choosenAttack = enemy.Attacks[num];

        currentAction = myAttack;
        BSM.CollectActions(myAttack);
        currentState = TurnState.ACTION;

        Debug.Log($"{enemy.name} chooses {myAttack.choosenAttack.attackName} for {myAttack.choosenAttack.attackDamage} dmg");
    }


    private IEnumerator TimeForAction()
    {
        if (actionStarted)
            yield break;

        actionStarted = true;

        GameObject target = currentAction.AttackersTarget;
        Vector3 heroPosition = new Vector3(target.transform.position.x + 1.5f, target.transform.position.y, target.transform.position.z);

        while (MoveTowardsEnemy(heroPosition))
            yield return null;

        yield return new WaitForSeconds(0.5f);
        doDamage();

        Vector3 firstPosition = startPosition;
        while (MoveTowardsStart(firstPosition))
            yield return null;

        yield return new WaitForSeconds(0.2f);

        if (BSM.PerformList.Contains(currentAction))
            BSM.PerformList.Remove(currentAction);

        actionStarted = false;
        currentState = TurnState.WAITING;

        // lanjutkan giliran ke unit berikutnya
        if (BSM.battleStates != BattleStateMachine.PerformAction.WIN &&
            BSM.battleStates != BattleStateMachine.PerformAction.LOSE)
        {
            yield return new WaitForSeconds(0.3f);
            BSM.NextTurn();
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

    void doDamage()
    {
        if (currentAction == null)
            return;

        float damageMultiplier = 1f;

        // BasicAttack = 10% ATK
        if (currentAction.choosenAttack is BasicAttacks)
        {
            damageMultiplier = 0.15f;
        }
        // HeavyAttack = 20% ATK
        else if (currentAction.choosenAttack is HeavyAttack)
        {
            damageMultiplier = 0.2f;
        }

        float calc_damage = enemy.currentATK * damageMultiplier;
        Debug.Log($"{enemy.name} deals {calc_damage} damage with {currentAction.choosenAttack.attackName}");
        currentAction.AttackersTarget.GetComponent<HeroStateMachine>().takeDamage(calc_damage);
    }

    private void HandleDeath()
    {
        this.gameObject.tag = "DeadEnemy";
        alive = false;

        BSM.EnemysInBattle.Remove(this.gameObject);
        Selector.SetActive(false);

        this.gameObject.GetComponent<MeshRenderer>().material.color = new Color32(105, 105, 105, 255);
        BSM.EnemyButton();
        BSM.battleStates = BattleStateMachine.PerformAction.CHECKALIVE;
    }

    public void takeDamage(float getDamageAmount)
    {
        enemy.currentHP -= getDamageAmount;
        if (enemy.currentHP <= 0)
        {
            enemy.currentHP = 0;
            currentState = TurnState.DEAD;
        }

        UpdateEnemyPanel();
    }
    void CreateEnemyPanel()
    {
        if (EnemyPanel == null)
        {
            Debug.LogError("EnemyPanel Prefab belum diset di Inspector!");
            return;
        }

        EnemyPanel = Instantiate(EnemyPanel) as GameObject;
        stats = EnemyPanel.GetComponent<EnemyPanelStats>();

        stats.EnemyName.text = enemy.name;
        stats.EnemyHP.text = "HP: " + enemy.currentHP;

        uiHealthBar = stats.HealthBar;
        uiHealthBar.SetMaxHealth((int)enemy.baseHP);
        uiHealthBar.SetHealth((int)enemy.currentHP);


        EnemyPanel.transform.SetParent(EnemyPanelSpacer, false);

    }
    void UpdateEnemyPanel()
    {
        stats.EnemyHP.text = "HP: " + enemy.currentHP;
        uiHealthBar.SetHealth((int)enemy.currentHP);

    }
    void ApplyBossVisual()
    {
        if (enemy.type == BaseEnemy.enemyType.BOSS)
        {
            // Membesarkan ukuran boss
            transform.localScale *= 1.4f;

            // 
            // var renderer = GetComponent<Renderer>();
            // if (renderer != null)
            // {
            //     renderer.material.color = Color.red;
            // }
        }
    }

    private void AssignEnemyData()
    {
        if (enemyDB == null || enemyDB.enemyList == null || enemyDB.enemyList.Count == 0)
        {
            Debug.LogError("EnemyDatabase tidak ditemukan atau kosong!");
            return;
        }

        List<BaseEnemy> bossList = enemyDB.enemyList.FindAll(x => x.type == BaseEnemy.enemyType.BOSS);
        List<BaseEnemy> normalList = enemyDB.enemyList.FindAll(x => x.type != BaseEnemy.enemyType.BOSS);

        BaseEnemy chosen = null;

        // 30% peluang spawn boss
        bool spawnBossChance = Random.value < 0.3f;

        if (!enemyDB.bossAssigned && spawnBossChance && bossList.Count > 0)
        {
            chosen = bossList[Random.Range(0, bossList.Count)];
            enemyDB.bossAssigned = true;
        }
        else
        {
            // fallback: jika normalList kosong, ambil dari semua list
            if (normalList.Count > 0)
                chosen = normalList[Random.Range(0, normalList.Count)];
            else if (enemyDB.enemyList.Count > 0)
                chosen = enemyDB.enemyList[Random.Range(0, enemyDB.enemyList.Count)];
        }

        if (chosen == null)
        {
            Debug.LogError("AssignEnemyData: Tidak ada enemy yang bisa dipilih (chosen == null). Check enemyDB.enemyList");
            return;
        }

        // APPLY data dari 'chosen' ke instance 'enemy'
        enemy.name = chosen.name;
        enemy.type = chosen.type;
        enemy.weakness = chosen.weakness;

        enemy.baseATK = chosen.baseATK;
        enemy.baseHP = chosen.baseHP;
        enemy.baseSpeed = chosen.baseSpeed;

        enemy.currentATK = chosen.baseATK;
        enemy.currentHP = chosen.baseHP;
        enemy.currentSpeed = chosen.baseSpeed;

        enemy.baseAVunit();
        enemy.currentActionValue = enemy.baseActionValue;

        if (enemy.Attacks == null) enemy.Attacks = new System.Collections.Generic.List<BaseAttack>();

        Debug.Log($"Spawned Enemy: {enemy.name} ({enemy.type}) HP={enemy.currentHP}");
    }



#if UNITY_EDITOR
    void OnValidate()
    {
        if (enemy != null)
        {
            enemy.baseAVunit();
            enemy.currentActionValue = enemy.baseActionValue;
        }
    }
#endif
}
