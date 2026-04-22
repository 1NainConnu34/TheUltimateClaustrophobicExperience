using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    [Header("Puzzle Setup")]
    [SerializeField] private PuzzleCable[] cables;
    [SerializeField] private GameObject maintenancePanel;

    [Header("Audio")]
    [SerializeField] private AudioSource successSource;
    [SerializeField] private AudioClip successClip;
    [SerializeField] private AudioSource errorSource;
    [SerializeField] private AudioClip errorClip;
    [SerializeField] private AudioSource elevatorAmbiance;
    [SerializeField] private AudioSource PuzzleStartSound;

    private bool puzzleSolved = false;
    private bool puzzleActive = false;
    private int connectedCount = 0;

    void Awake()
    {
        Instance = this;

        if (maintenancePanel != null)
            maintenancePanel.SetActive(false);
    }

    public void HidePuzzle()
    {
        if (maintenancePanel != null)
            maintenancePanel.SetActive(false);
    }

    public void ActivatePuzzle()
    {
        if (puzzleActive) return;

        puzzleActive = true;
        puzzleSolved = false;
        connectedCount = 0;

        if (maintenancePanel != null)
            maintenancePanel.SetActive(true);
        PuzzleStartSound.Play();
        elevatorAmbiance.Play();
        Debug.Log("Puzzle activé !");
    }

    public void TryConnectCable(int cableID, PuzzleCable cable)
    {
        if (!puzzleActive || puzzleSolved) return;

        cable.Connect();
        connectedCount++;

        successSource?.PlayOneShot(successClip);

        Debug.Log($"Câble {cableID} connecté ! {connectedCount}/{cables.Length}");

        if (connectedCount >= cables.Length)
            PuzzleSolved();
    }

    void PuzzleSolved()
    {
        puzzleSolved = true;
        puzzleActive = false;
        elevatorAmbiance.Stop();
        Debug.Log("Puzzle résolu !");

        Invoke(nameof(TriggerResolution), 1.5f);
    }

    void TriggerResolution()
    {
        GameManager.Instance.TriggerGoodEnding();
    }
}