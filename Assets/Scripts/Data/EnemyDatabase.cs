using System.Collections.Generic;
using UnityEngine;

public class EnemyDatabase : MonoBehaviour
{
    public static EnemyDatabase Instance;
    public bool bossAssigned = false;

    [Header("CSV File (TextAsset)")]
    public TextAsset enemyCSV;

    public List<BaseEnemy> enemyList = new List<BaseEnemy>();

    void Awake()
    {
        Instance = this;
        LoadEnemyCSV();
    }
    public void ResetBossAssignment()
    {
        bossAssigned = false;
    }

    public void LoadEnemyCSV()
    {
        enemyList.Clear();

        if (enemyCSV == null)
        {
            Debug.LogError("EnemyDatabase tidak ditemukan atau kosong!");
            return;
        }

        string[] rows = enemyCSV.text.Split('\n');

        for (int i = 1; i < rows.Length; i++)
        {
            string row = rows[i].Trim();
            if (string.IsNullOrEmpty(row)) continue;

            string[] col = row.Split(',');
            if (col.Length < 6) continue;

            BaseEnemy e = new BaseEnemy();
            e.name = col[0].Trim();

            // CSV: Tipe = enemyType Enum
            e.type = (BaseEnemy.enemyType)
                System.Enum.Parse(typeof(BaseEnemy.enemyType), col[1].Trim());

            // CSV: Weakness = enemyWeakness Enum
            e.weakness = (BaseEnemy.enemyWeakness)
                System.Enum.Parse(typeof(BaseEnemy.enemyWeakness), col[2].Trim());

            // Stats numbers
            float.TryParse(col[3], out e.baseATK);
            float.TryParse(col[4], out e.baseHP);
            float.TryParse(col[5], out e.baseSpeed);

            // Initialize runtime stats
            e.currentATK = e.baseATK;
            e.currentHP = e.baseHP;
            e.currentSpeed = e.baseSpeed;

            enemyList.Add(e);
        }

        Debug.Log($"Enemy Loaded: {enemyList.Count}");
    }

    public BaseEnemy GetRandomEnemy()
    {
        if (enemyList.Count == 0) return null;
        return enemyList[Random.Range(0, enemyList.Count)];
    }

    public BaseEnemy GetRandomBoss()
    {
        List<BaseEnemy> bosses = enemyList.FindAll(x => x.type == BaseEnemy.enemyType.BOSS);
        if (bosses.Count == 0) return null;
        return bosses[Random.Range(0, bosses.Count)];
    }
}
