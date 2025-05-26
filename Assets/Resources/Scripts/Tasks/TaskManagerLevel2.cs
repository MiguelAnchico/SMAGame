using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TaskManagerLevel2 : MonoBehaviour
{
    public DialogueUI dialogueUI;

    public DialogueEntry[] dialogoInicial;
    public DialogueEntry[] dialogoVictoria;
    public DialogueEntry[] dialogoDerrota;

    public DialogueEntry[] dialogoInicialMision2;
    public DialogueEntry[] dialogoVictoriaMision2;
    public DialogueEntry[] dialogoDerrotaMision2;

    public GameObject enemyPrefab;
    public Transform[] spawnPoints;

    public TMP_Text taskText;
    public TMP_Text timerText;
    public GameObject taskPanel;
    public GameObject taskIcon;

    private int enemiesToSpawnMission1 = 4;
    private int enemiesToSpawnMission2 = 8;

    private int enemiesSpawned;
    private int enemiesDefeated;

    private float taskDuration = 30f;
    private float spawnInterval = 2f;
    private float timeRemaining;

    private bool taskActive = false;
    private int currentMission = 0;

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    public static TaskManagerLevel2 Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (taskText != null) taskText.gameObject.SetActive(false);
        if (taskPanel != null) taskPanel.SetActive(false);
        if (taskIcon != null) taskIcon.SetActive(false);
        if (timerText != null) timerText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!taskActive) return;

        timeRemaining -= Time.deltaTime;
        if (timerText != null)
            timerText.text = $"Tiempo: {Mathf.Ceil(timeRemaining)}s";

        if (timeRemaining <= 0)
        {
            taskActive = false;
            Debug.Log($"[TaskManager] Tiempo acabado en misión {currentMission}");
            EndTask(false);
        }
    }

    public void StartMission(int missionNumber)
    {
        Debug.Log($"[TaskManager] Iniciando misión {missionNumber}");

        currentMission = missionNumber;
        enemiesSpawned = 0;
        enemiesDefeated = 0;

        taskDuration = missionNumber == 2 ? 60f : 30f;

        timeRemaining = taskDuration;
        taskActive = true;

        int enemiesToSpawn = missionNumber == 1 ? enemiesToSpawnMission1 : enemiesToSpawnMission2;

        if (taskText != null)
        {
            taskText.gameObject.SetActive(true);
            taskPanel.SetActive(true);
            taskIcon.SetActive(true);
            taskText.text = $"Elimina enemigos: {enemiesDefeated}/{enemiesToSpawn}";
        }

        if (timerText != null)
            timerText.gameObject.SetActive(true);

        StartCoroutine(SpawnEnemies(enemiesToSpawn));
    }

    private IEnumerator SpawnEnemies(int enemiesToSpawn)
    {
        while (enemiesSpawned < enemiesToSpawn && taskActive)
        {
            SpawnEnemy();
            enemiesSpawned++;
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemy()
    {
        if (spawnPoints.Length == 0) return;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        spawnedEnemies.Add(enemy);
        Debug.Log($"[TaskManager] Enemigo spawnado en {spawnPoint.position}");
    }

    public void EnemyDefeated()
    {
        if (!taskActive) return;

        enemiesDefeated++;
        int enemiesToSpawn = currentMission == 1 ? enemiesToSpawnMission1 : enemiesToSpawnMission2;

        if (taskText != null)
            taskText.text = $"Elimina enemigos: {enemiesDefeated}/{enemiesToSpawn}";

        Debug.Log($"[TaskManager] Enemigo derrotado: {enemiesDefeated}/{enemiesToSpawn}");

        if (enemiesDefeated >= enemiesToSpawn)
        {
            taskActive = false;
            EndTask(true);
        }
    }

    private void EndTask(bool success)
    {
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }
        spawnedEnemies.Clear();

        if (taskText != null) taskText.gameObject.SetActive(false);
        if (taskPanel != null) taskPanel.SetActive(false);
        if (taskIcon != null) taskIcon.SetActive(false);
        if (timerText != null) timerText.text = "";

        if (currentMission == 1)
        {
            if (success)
            {
                PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
                if (playerHealth != null) playerHealth.RestoreFullHealth();
                dialogueUI.StartDialogue(dialogoVictoria);
            }
            else
            {
                dialogueUI.StartDialogue(dialogoDerrota);
            }

            StartCoroutine(StartMission2Sequence());
        }
        else if (currentMission == 2)
        {
            PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth != null) playerHealth.RestoreFullHealth();

            StartCoroutine(ShowFinalScreenAfterDialogue(success ? dialogoVictoriaMision2 : dialogoDerrotaMision2));
        }
    }

    private IEnumerator StartMission2Sequence()
    {
        Debug.Log("[TaskManager] Esperando 5 segundos para iniciar misión 2...");
        yield return new WaitForSeconds(5f);

        dialogueUI.StartDialogue(dialogoInicialMision2);
        while (dialogueUI.IsDialogueActive())
        {
            yield return null;
        }

        StartMission(2);
    }

    private IEnumerator ShowFinalScreenAfterDialogue(DialogueEntry[] dialogue)
    {
        dialogueUI.StartDialogue(dialogue);
        while (dialogueUI.IsDialogueActive())
        {
            yield return null;
        }

        GameEndManager endManager = FindObjectOfType<GameEndManager>();
        if (endManager != null)
        {
            endManager.ShowGameEndScreen();
            Time.timeScale = 0f;
        }
    }
}

