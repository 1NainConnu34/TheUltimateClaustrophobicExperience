using UnityEngine;

public class WallClosing : MonoBehaviour
{
    [Header("Walls")]
    [SerializeField] private Transform wallLeft;
    [SerializeField] private Transform wallRight;

    [Header("Closing Settings")]
    [SerializeField, Min(0f)] private float closingSpeed = 0.05f;
    [SerializeField, Min(0f)] private float maxClosingDistance = 0.8f;
    [SerializeField, Min(0f)] private float puzzleTriggerDistance = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioSource creakingSource;
    [SerializeField] private AudioClip creakingClip;
    [SerializeField, Range(0f, 1f)] private float creakingVolume = 0.6f;

    private Vector3 wallLeftStart;
    private Vector3 wallRightStart;
    private bool isClosing = false;
    private float distanceMoved = 0f;
    private bool puzzleTriggered = false;

    void Awake()
    {
        if (wallLeft != null) wallLeftStart = wallLeft.localPosition;
        if (wallRight != null) wallRightStart = wallRight.localPosition;
        EnsureCreakingSource();
    }

    void EnsureCreakingSource()
    {
        if (creakingSource != null) return;

        creakingSource = gameObject.AddComponent<AudioSource>();
        creakingSource.playOnAwake = false;
        creakingSource.loop = true;
        creakingSource.spatialBlend = 1f;
        creakingSource.volume = creakingVolume;
    }

    public void StartClosing()
    {
        isClosing = true;
        puzzleTriggered = false;

        if (creakingClip != null && creakingSource != null)
        {
            creakingSource.clip = creakingClip;
            creakingSource.loop = true;
            creakingSource.volume = creakingVolume;
            if (!creakingSource.isPlaying)
                creakingSource.Play();
        }
    }

    public void StopClosing()
    {
        isClosing = false;

        if (creakingSource != null && creakingSource.isPlaying)
            creakingSource.Stop();
    }

    public void ResetWalls()
    {
        StopClosing();
        distanceMoved = 0f;
        puzzleTriggered = false;

        if (wallLeft != null) wallLeft.localPosition = wallLeftStart;
        if (wallRight != null) wallRight.localPosition = wallRightStart;
    }

    void Update()
    {
        if (!isClosing) return;

        if (distanceMoved >= maxClosingDistance)
        {
            StopClosing();
            GameManager.Instance.SetPhase(GameManager.Phase.Resolution);
            return;
        }

        float step = closingSpeed * Time.deltaTime;
        distanceMoved += step;

        if (wallLeft != null)
            wallLeft.localPosition += new Vector3(step, 0, 0);
        if (wallRight != null)
            wallRight.localPosition += new Vector3(-step, 0, 0);

        if (!puzzleTriggered && distanceMoved >= puzzleTriggerDistance)
        {
            puzzleTriggered = true;
            GameManager.Instance.SetPhase(GameManager.Phase.Puzzle);
        }
    }
}