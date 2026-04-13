using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

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

    [Header("Phase 2 Panne FX")]
    [SerializeField] private Light[] panneLights;
    [SerializeField] private Color panneLightColor = new Color(0.75f, 0.08f, 0.08f, 1f);
    [SerializeField, Min(0f)] private float panneLightIntensity = 0.35f;
    [SerializeField, Min(0f)] private float panneBlinkDuration = 1.6f;
    [SerializeField, Min(0.05f)] private float panneBlinkInterval = 0.12f;

    [Header("Ventilation")]
    [SerializeField] private AudioSource ventilationSource;
    [SerializeField] private AudioClip ventilationClip;
    [SerializeField, Range(0f, 1f)] private float ventilationVolume = 0.35f;
    [SerializeField] private bool loopVentilation = true;
    [SerializeField, Range(0f, 1f)] private float ventilationSpatialBlend = 1f;
    [SerializeField, Min(0.1f)] private float ventilationMinDistance = 1f;
    [SerializeField, Min(0.1f)] private float ventilationMaxDistance = 8f;

    [Header("Panne Sound")]
    [SerializeField] private AudioSource panneSfxSource;
    [SerializeField] private AudioClip panneCreakClip;
    [SerializeField, Range(0f, 1f)] private float panneCreakVolume = 0.9f;
    [SerializeField] private bool usePanneHaptics = true;
    [SerializeField, Range(0f, 1f)] private float panneHapticAmplitude = 0.8f;
    [SerializeField, Min(0f)] private float panneHapticDuration = 0.2f;

    [Header("Elevator Floors")]
    [SerializeField, Min(1)] private int minFloor = 1;
    [SerializeField, Min(1)] private int maxFloor = 10;
    [SerializeField, Min(1)] private int currentFloor = 1;
    [SerializeField, Min(0.1f)] private float secondsPerFloor = 0.75f;
    [SerializeField] private bool randomPanneDuringTravel = true;

    [Header("Floor Display")]
    [SerializeField] private TextMesh floorDisplayText;

    private Coroutine panneBlinkCoroutine;
    private Coroutine travelCoroutine;
    private readonly Dictionary<Light, Color> baseLightColors = new Dictionary<Light, Color>();
    private readonly Dictionary<Light, float> baseLightIntensities = new Dictionary<Light, float>();
    private readonly Dictionary<Light, bool> baseLightEnabled = new Dictionary<Light, bool>();

    public int CurrentFloor => currentFloor;
    public bool IsTraveling => travelCoroutine != null;

    void Awake()
    {
        Instance = this;
        ClampFloorBounds();
        EnsurePhase1MusicSource();
        EnsurePanneSetup();
        EnsureVentilationSource();
        EnsureFloorDisplayReference();
    }

    void Start()
    {
        ApplyAudioForPhase(currentPhase);
        UpdateFloorDisplay();
    }

    void ClampFloorBounds()
    {
        if (maxFloor < minFloor)
            maxFloor = minFloor;

        currentFloor = Mathf.Clamp(currentFloor, minFloor, maxFloor);
    }

    void EnsureFloorDisplayReference()
    {
        if (floorDisplayText != null)
            return;

        GameObject floorDisplayObject = GameObject.Find("FloorDisplay");
        if (floorDisplayObject != null)
            floorDisplayText = floorDisplayObject.GetComponent<TextMesh>();

        if (floorDisplayText == null)
        {
            Debug.LogWarning("No FloorDisplay TextMesh assigned on GameManager. Create one in scene and assign it.", this);
        }
    }

    void UpdateFloorDisplay()
    {
        if (floorDisplayText == null)
            return;

        floorDisplayText.text = currentFloor.ToString("00");
    }

    public void RequestFloor(int requestedFloor)
    {
        if (currentPhase != Phase.Introduction)
            return;

        ClampFloorBounds();
        requestedFloor = Mathf.Clamp(requestedFloor, minFloor, maxFloor);

        if (requestedFloor == currentFloor)
            return;

        if (travelCoroutine != null)
            return;

        travelCoroutine = StartCoroutine(TravelToFloorRoutine(requestedFloor));
    }

    IEnumerator TravelToFloorRoutine(int targetFloor)
    {
        int direction = targetFloor > currentFloor ? 1 : -1;
        int panneTriggerFloor = -1;

        if (randomPanneDuringTravel)
        {
            int low = Mathf.Min(currentFloor, targetFloor) + 1;
            int high = Mathf.Max(currentFloor, targetFloor);

            low = Mathf.Max(low, 2);
            if (low <= high)
                panneTriggerFloor = Random.Range(low, high + 1);
        }

        while (currentFloor != targetFloor)
        {
            yield return new WaitForSeconds(secondsPerFloor);

            currentFloor += direction;
            UpdateFloorDisplay();

            if (currentPhase == Phase.Introduction && currentFloor == panneTriggerFloor)
            {
                SetPhase(Phase.Panne);
                break;
            }
        }

        travelCoroutine = null;
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

    void EnsurePanneSetup()
    {
        if (panneLights == null || panneLights.Length == 0)
        {
            GameObject elevator = GameObject.Find("Elevator");
            if (elevator != null)
                panneLights = elevator.GetComponentsInChildren<Light>(true);
        }

        CacheBaseLightsState();

        if (panneSfxSource == null)
            panneSfxSource = gameObject.AddComponent<AudioSource>();

        panneSfxSource.playOnAwake = false;
        panneSfxSource.loop = false;
        panneSfxSource.spatialBlend = 1f;
        panneSfxSource.minDistance = 1f;
        panneSfxSource.maxDistance = 8f;
    }

    void EnsureVentilationSource()
    {
        GameObject elevator = GameObject.Find("Elevator");
        Transform parent = elevator != null ? elevator.transform : transform;

        if (ventilationSource == null || ventilationSource.gameObject == gameObject)
        {
            Transform sourceTransform = parent.Find("Ventilation_Source");
            if (sourceTransform == null)
            {
                GameObject sourceObject = new GameObject("Ventilation_Source");
                sourceTransform = sourceObject.transform;
                sourceTransform.SetParent(parent, false);
                sourceTransform.localPosition = Vector3.zero;
            }

            ventilationSource = sourceTransform.GetComponent<AudioSource>();
            if (ventilationSource == null)
                ventilationSource = sourceTransform.gameObject.AddComponent<AudioSource>();
        }
        else if (ventilationSource.transform.parent != parent)
        {
            ventilationSource.transform.SetParent(parent, false);
            ventilationSource.transform.localPosition = Vector3.zero;
        }

        ventilationSource.playOnAwake = false;
        ventilationSource.loop = loopVentilation;
        ventilationSource.spatialBlend = ventilationSpatialBlend;
        ventilationSource.rolloffMode = AudioRolloffMode.Logarithmic;
        ventilationSource.minDistance = ventilationMinDistance;
        ventilationSource.maxDistance = ventilationMaxDistance;
        ventilationSource.volume = ventilationVolume;
    }

    void ApplyVentilationForPhase(Phase phase)
    {
        if (ventilationSource == null)
            return;

        if (phase == Phase.Introduction)
        {
            if (ventilationClip == null)
            {
                if (ventilationSource.isPlaying)
                    ventilationSource.Stop();

                Debug.LogWarning("Ventilation clip is not assigned on GameManager.", this);
                return;
            }

            if (ventilationSource.clip != ventilationClip)
                ventilationSource.clip = ventilationClip;

            ventilationSource.loop = loopVentilation;
            ventilationSource.volume = ventilationVolume;

            if (!ventilationSource.isPlaying)
                ventilationSource.Play();

            return;
        }

        if (ventilationSource.isPlaying)
            ventilationSource.Stop();
    }

    void CacheBaseLightsState()
    {
        baseLightColors.Clear();
        baseLightIntensities.Clear();
        baseLightEnabled.Clear();

        if (panneLights == null)
            return;

        foreach (Light lightRef in panneLights)
        {
            if (lightRef == null)
                continue;

            baseLightColors[lightRef] = lightRef.color;
            baseLightIntensities[lightRef] = lightRef.intensity;
            baseLightEnabled[lightRef] = lightRef.enabled;
        }
    }

    void StartPanneEffects()
    {
        EnsurePanneSetup();

        if (ventilationSource != null && ventilationSource.isPlaying)
            ventilationSource.Stop();

        if (panneCreakClip != null && panneSfxSource != null)
            panneSfxSource.PlayOneShot(panneCreakClip, panneCreakVolume);

        SendPanneHaptics();

        if (panneBlinkCoroutine != null)
            StopCoroutine(panneBlinkCoroutine);

        panneBlinkCoroutine = StartCoroutine(PanneLightsRoutine());
    }

    void ResetFromPanneEffects()
    {
        if (panneBlinkCoroutine != null)
        {
            StopCoroutine(panneBlinkCoroutine);
            panneBlinkCoroutine = null;
        }

        if (travelCoroutine != null)
        {
            StopCoroutine(travelCoroutine);
            travelCoroutine = null;
        }

        foreach (KeyValuePair<Light, Color> entry in baseLightColors)
        {
            Light lightRef = entry.Key;
            if (lightRef == null)
                continue;

            lightRef.color = entry.Value;

            if (baseLightIntensities.TryGetValue(lightRef, out float intensity))
                lightRef.intensity = intensity;

            if (baseLightEnabled.TryGetValue(lightRef, out bool enabledState))
                lightRef.enabled = enabledState;
        }
    }

    IEnumerator PanneLightsRoutine()
    {
        if (panneLights == null || panneLights.Length == 0)
            yield break;

        float elapsed = 0f;
        bool lightsOn = false;

        while (elapsed < panneBlinkDuration)
        {
            lightsOn = !lightsOn;

            foreach (Light lightRef in panneLights)
            {
                if (lightRef == null)
                    continue;

                lightRef.enabled = lightsOn;
            }

            yield return new WaitForSeconds(panneBlinkInterval);
            elapsed += panneBlinkInterval;
        }

        foreach (Light lightRef in panneLights)
        {
            if (lightRef == null)
                continue;

            lightRef.enabled = true;
            lightRef.color = panneLightColor;
            lightRef.intensity = panneLightIntensity;
        }

        panneBlinkCoroutine = null;
    }

    void SendPanneHaptics()
    {
        if (!usePanneHaptics)
            return;

        List<InputDevice> devices = new List<InputDevice>();
        InputDeviceCharacteristics characteristics =
            InputDeviceCharacteristics.HeldInHand |
            InputDeviceCharacteristics.Controller;

        InputDevices.GetDevicesWithCharacteristics(characteristics, devices);

        foreach (InputDevice device in devices)
        {
            if (!device.isValid)
                continue;

            if (!device.TryGetHapticCapabilities(out HapticCapabilities capabilities))
                continue;

            if (!capabilities.supportsImpulse)
                continue;

            device.SendHapticImpulse(0u, panneHapticAmplitude, panneHapticDuration);
        }
    }

    void ApplyAudioForPhase(Phase phase)
    {
        if (phase == Phase.Introduction)
        {
            if (phase1MusicSource != null)
            {
                if (phase1MusicClip == null)
                {
                    if (phase1MusicSource.isPlaying)
                        phase1MusicSource.Stop();

                    Debug.LogWarning("Phase 1 ambience clip is not assigned on GameManager.", this);
                }
                else
                {
                    if (phase1MusicSource.clip != phase1MusicClip)
                        phase1MusicSource.clip = phase1MusicClip;

                    phase1MusicSource.loop = loopPhase1Music;
                    phase1MusicSource.volume = phase1MusicVolume;

                    if (!phase1MusicSource.isPlaying)
                        phase1MusicSource.Play();
                }
            }
            else
            {
                Debug.LogWarning("Phase 1 ambience source is not assigned on GameManager.", this);
            }

            ApplyVentilationForPhase(phase);
            return;
        }

        if (phase1MusicSource != null && phase1MusicSource.isPlaying)
            phase1MusicSource.Stop();

        ApplyVentilationForPhase(phase);
    }

    public void SetPhase(Phase newPhase)
    {
        if (currentPhase == newPhase)
            return;

        currentPhase = newPhase;
        Debug.Log("Nouvelle phase : " + newPhase);
        ApplyAudioForPhase(newPhase);

        switch (newPhase)
        {
            case Phase.Introduction:
                ResetFromPanneEffects();
                break;
            case Phase.Panne:
                StartPanneEffects();
                break;
            case Phase.Tension:
                // TODO: démarrer le rétrécissement des murs
                break;
            case Phase.Puzzle:
                // TODO: ouvrir le panneau de maintenance
                break;
            case Phase.Resolution:
                ResetFromPanneEffects();
                break;
        }
    }
}