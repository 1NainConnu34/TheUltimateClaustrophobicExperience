using UnityEngine;
using System.Collections;

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
            {
                creakingSource.Play();
            }
        }
    }

    public void StopClosing()
    {
        isClosing = false;

        if (creakingSource != null && creakingSource.isPlaying)
        {
            creakingSource.Stop();
        }
    }

    public void ResetWalls()
    {
        StopClosing();
        distanceMoved = 0f;
        puzzleTriggered = false;
        StartCoroutine(WallsOpenRoutine());
    }

    IEnumerator WallsOpenRoutine()
    {
        while (true)
        {
            bool leftDone = false;
            bool rightDone = false;

            if (wallLeft != null)
            {
                wallLeft.localPosition = Vector3.Lerp(
                    wallLeft.localPosition,
                    wallLeftStart,
                    Time.deltaTime * closingSpeed * 20f
                );
                leftDone = Vector3.Distance(wallLeft.localPosition, wallLeftStart) < 0.01f;
            }
            else leftDone = true;

            if (wallRight != null)
            {
                wallRight.localPosition = Vector3.Lerp(
                    wallRight.localPosition,
                    wallRightStart,
                    Time.deltaTime * closingSpeed * 20f
                );
                rightDone = Vector3.Distance(wallRight.localPosition, wallRightStart) < 0.01f;
            }
            else rightDone = true;

            if (leftDone && rightDone)
            {
                if (wallLeft != null) wallLeft.localPosition = wallLeftStart;
                if (wallRight != null) wallRight.localPosition = wallRightStart;
                yield break;
            }

            yield return null;
        }
    }

    void Update()
    {
        if (!isClosing) return;

        if (distanceMoved >= maxClosingDistance)
        {
            StopClosing();
            GameManager.Instance.TriggerBadEnding();
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