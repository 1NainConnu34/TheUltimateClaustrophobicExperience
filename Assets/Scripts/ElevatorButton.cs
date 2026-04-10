using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ElevatorButton : MonoBehaviour
{
    [Header("Button Setup")]
    [SerializeField] private int floorNumber;
    [SerializeField] private Material matNormal;
    [SerializeField] private Material matPressed;
    [SerializeField, Min(0f)] private float pressDepth = 0.01f;
    [SerializeField, Min(0f)] private float panneDelay = 2f;
    [SerializeField] private bool triggerOnDirectTouch = true;

    private MeshRenderer meshRenderer;
    private Vector3 startLocalPosition;
    private bool isPressed;
    private readonly HashSet<XRDirectInteractor> touchingInteractors = new();

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = GetComponentInChildren<MeshRenderer>();

        startLocalPosition = transform.localPosition;
    }

    void OnEnable()
    {
        touchingInteractors.Clear();
        ReleaseButton();
    }

    void OnDisable()
    {
        touchingInteractors.Clear();
        ReleaseButton();
    }

    void Start()
    {
        if (meshRenderer == null)
            Debug.LogWarning($"[{nameof(ElevatorButton)}] No MeshRenderer found on {name}.", this);

        if (meshRenderer != null && matNormal != null)
            meshRenderer.material = matNormal;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!triggerOnDirectTouch)
            return;

        if (!TryGetDirectInteractor(other, out XRDirectInteractor directInteractor))
            return;

        if (!touchingInteractors.Add(directInteractor))
            return;

        if (touchingInteractors.Count == 1)
            PressButton("directTouch");
    }

    void OnTriggerExit(Collider other)
    {
        if (!triggerOnDirectTouch)
            return;

        if (!TryGetDirectInteractor(other, out XRDirectInteractor directInteractor))
            return;

        if (!touchingInteractors.Remove(directInteractor))
            return;

        if (touchingInteractors.Count == 0)
            ReleaseButton();
    }

    [ContextMenu("Test Press Button")]
    public void DebugPress()
    {
        PressButton("debug");
    }

    [ContextMenu("Test Release Button")]
    public void DebugRelease()
    {
        ReleaseButton();
    }

    bool TryGetDirectInteractor(Collider other, out XRDirectInteractor directInteractor)
    {
        directInteractor = other.GetComponentInParent<XRDirectInteractor>();
        return directInteractor != null;
    }

    void PressButton(string source)
    {
        if (isPressed)
            return;

        isPressed = true;

        if (meshRenderer != null && matPressed != null)
            meshRenderer.material = matPressed;

        GetComponent<AudioSource>()?.Play();

        transform.localPosition = startLocalPosition + Vector3.back * pressDepth;

        Debug.Log($"Floor button {floorNumber} pressed via {source}.", this);

        CancelInvoke(nameof(TriggerPanne));
        Invoke(nameof(TriggerPanne), panneDelay);
    }

    void ReleaseButton()
    {
        if (!isPressed)
            return;

        isPressed = false;
        CancelInvoke(nameof(TriggerPanne));

        if (meshRenderer != null && matNormal != null)
            meshRenderer.material = matNormal;

        transform.localPosition = startLocalPosition;
    }

    void TriggerPanne()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError($"[{nameof(ElevatorButton)}] GameManager.Instance is null.", this);
            return;
        }

        GameManager.Instance.SetPhase(GameManager.Phase.Panne);
    }
}