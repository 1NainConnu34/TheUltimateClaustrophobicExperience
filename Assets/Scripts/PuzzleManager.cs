using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    [Header("Puzzle Setup")]
    [SerializeField] private PuzzleStage[] stages;

    [Header("Audio")]
    [SerializeField] private AudioSource successSource;
    [SerializeField] private AudioClip successClip;
    [SerializeField] private AudioSource elevatorAmbiance;
    [SerializeField] private AudioSource puzzleStartSound;

    private bool puzzleSolved = false;
    private bool puzzleActive = false;
    private int currentStage = 0;
    private int connectedCount = 0;

    void Awake()
    {
        Instance = this;

        foreach (PuzzleStage stage in stages)
        {
            if (stage.maintenancePanel != null)
                stage.maintenancePanel.SetActive(false);
        }
    }

    public void HidePuzzle()
    {
        foreach (PuzzleStage stage in stages)
        {
            if (stage.maintenancePanel != null)
                stage.maintenancePanel.SetActive(false);
        }
    }

    public void StopAmbiance()
    {
        if (elevatorAmbiance != null && elevatorAmbiance.isPlaying)
            elevatorAmbiance.Stop();
        if (puzzleStartSound != null && puzzleStartSound.isPlaying)
            puzzleStartSound.Stop();
    }

    public void ActivatePuzzle()
    {
        if (puzzleActive) return;

        puzzleActive = true;
        puzzleSolved = false;
        currentStage = 0;
        connectedCount = 0;

        if (puzzleStartSound != null) puzzleStartSound.Play();
        if (elevatorAmbiance != null) elevatorAmbiance.Play();

        ActivateStage(0);

        Debug.Log("Puzzle activé — Stage 1 !");
    }

    void ActivateStage(int stageIndex)
    {
        foreach (PuzzleStage stage in stages)
        {
            if (stage.maintenancePanel != null)
                stage.maintenancePanel.SetActive(false);
        }

        if (stages[stageIndex].maintenancePanel != null)
            stages[stageIndex].maintenancePanel.SetActive(true);

        connectedCount = 0;

        Debug.Log($"Stage {stageIndex + 1} activé !");
    }

    public void TryConnectCable(int cableID, PuzzleCable cable)
    {
        if (!puzzleActive || puzzleSolved) return;

        cable.Connect();
        connectedCount++;

        successSource?.PlayOneShot(successClip);

        Debug.Log($"Câble {cableID} connecté ! {connectedCount}/{stages[currentStage].cables.Length}");

        if (connectedCount >= stages[currentStage].cables.Length)
            StageCompleted();
    }

    void StageCompleted()
    {
        // Complete puzzle after first stage (no retry loop)
        AllStagesCompleted();
    }

    void NextStage()
    {
        ActivateStage(currentStage);
    }

    void AllStagesCompleted()
    {
        puzzleSolved = true;
        puzzleActive = false;

        if (elevatorAmbiance != null) elevatorAmbiance.Stop();

        Debug.Log("Tous les puzzles résolus !");

        Invoke(nameof(TriggerResolution), 1.5f);
    }

    void TriggerResolution()
    {
        GameManager.Instance.TriggerGoodEnding();
    }
}

[System.Serializable]
public class PuzzleStage
{
    public GameObject maintenancePanel;
    public PuzzleCable[] cables;
}