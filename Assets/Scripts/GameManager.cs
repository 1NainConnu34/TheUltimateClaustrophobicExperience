using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum Phase
    {
        Introduction,   // Phase 1 
        Panne,          // Phase 2 
        Tension,        // Phase 3
        Puzzle,         // Phase 4 
        Resolution      // Phase 5 
    }

    public static GameManager Instance;
    public Phase currentPhase = Phase.Introduction;

    [Header("Phase 1 Ambience")]
    [SerializeField] private AudioSource phase1MusicSource;
    [SerializeField] private AudioClip phase1MusicClip;
    [SerializeField, Range(0f, 1f)] private float phase1MusicVolume = 0.35f;
    [SerializeField] private bool loopPhase1Music = true;

    void Awake()
    {
        Instance = this;
        EnsurePhase1MusicSource();
    }

    void Start()
    {
        ApplyAudioForPhase(currentPhase);
    }

    void EnsurePhase1MusicSource()
    {
        if (phase1MusicSource == null)
            phase1MusicSource = GetComponent<AudioSource>();

        if (phase1MusicSource == null)
            phase1MusicSource = gameObject.AddComponent<AudioSource>();

        phase1MusicSource.playOnAwake = false;
        phase1MusicSource.loop = loopPhase1Music;
        phase1MusicSource.spatialBlend = 0f;
        phase1MusicSource.volume = phase1MusicVolume;
    }

    void ApplyAudioForPhase(Phase phase)
    {
        if (phase1MusicSource == null)
            return;

        if (phase == Phase.Introduction)
        {
            if (phase1MusicClip == null)
            {
                if (phase1MusicSource.isPlaying)
                    phase1MusicSource.Stop();

                Debug.LogWarning("Phase 1 ambience clip is not assigned on GameManager.", this);
                return;
            }

            if (phase1MusicSource.clip != phase1MusicClip)
                phase1MusicSource.clip = phase1MusicClip;

            phase1MusicSource.loop = loopPhase1Music;
            phase1MusicSource.volume = phase1MusicVolume;

            if (!phase1MusicSource.isPlaying)
                phase1MusicSource.Play();

            return;
        }

        if (phase1MusicSource.isPlaying)
            phase1MusicSource.Stop();
    }

    public void SetPhase(Phase newPhase)
    {
        currentPhase = newPhase;
        Debug.Log("Nouvelle phase : " + newPhase);
        ApplyAudioForPhase(newPhase);

        switch (newPhase)
        {
            case Phase.Introduction:
                // TODO: déclencher effets d'introduction si besoin
                break;
            case Phase.Panne:
                // TODO: déclencher effets panne
                break;
            case Phase.Tension:
                // TODO: démarrer le rétrécissement des murs
                break;
            case Phase.Puzzle:
                // TODO: ouvrir le panneau de maintenance
                break;
            case Phase.Resolution:
                // TODO: ouvrir les portes ou game over
                break;
        }
    }
}