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
    [SerializeField] private bool triggerOnDirectTouch = true;

    [Header("Exterior Settings")]
    [SerializeField] private bool isExteriorButton = false;

    [Header("Door Button Settings")]
    [SerializeField] private bool isDoorButton = false;
    [SerializeField] private GameObject doorLeft;
    [SerializeField] private GameObject doorRight;
    [SerializeField] private float openDistance = 0.95f;
    [SerializeField] private float openSpeed = 1.5f;

    private MeshRenderer meshRenderer;
    private Vector3 startLocalPosition;
    private Vector3 doorLeftClosed;
    private Vector3 doorRightClosed;
    private bool isPressed;
    private bool isOpen = false;
    private readonly HashSet<XRDirectInteractor> touchingInteractors = new();

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = GetComponentInChildren<MeshRenderer>();

        startLocalPosition = transform.localPosition;

        if ((isExteriorButton || isDoorButton) && doorLeft != null)
            doorLeftClosed = doorLeft.transform.localPosition;
        if ((isExteriorButton || isDoorButton) && doorRight != null)
            doorRightClosed = doorRight.transform.localPosition;
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

    void Update()
    {
        if (!isExteriorButton && !isDoorButton) return;
        if (doorLeft == null || doorRight == null) return;

        Vector3 targetLeft = isOpen
            ? doorLeftClosed + new Vector3(-openDistance, 0, 0)
            : doorLeftClosed;

        Vector3 targetRight = isOpen
            ? doorRightClosed + new Vector3(openDistance, 0, 0)
            : doorRightClosed;

        doorLeft.transform.localPosition = Vector3.Lerp(
            doorLeft.transform.localPosition, targetLeft, Time.deltaTime * openSpeed);

        doorRight.transform.localPosition = Vector3.Lerp(
            doorRight.transform.localPosition, targetRight, Time.deltaTime * openSpeed);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!triggerOnDirectTouch) return;
        if (!TryGetDirectInteractor(other, out XRDirectInteractor directInteractor)) return;
        if (!touchingInteractors.Add(directInteractor)) return;
        if (touchingInteractors.Count == 1)
            PressButton("directTouch");
    }

    void OnTriggerExit(Collider other)
    {
        if (!triggerOnDirectTouch) return;
        if (!TryGetDirectInteractor(other, out XRDirectInteractor directInteractor)) return;
        if (!touchingInteractors.Remove(directInteractor)) return;
        if (touchingInteractors.Count == 0)
            ReleaseButton();
    }

    [ContextMenu("Test Press Button")]
    public void DebugPress() => PressButton("debug");

    [ContextMenu("Test Release Button")]
    public void DebugRelease() => ReleaseButton();

    bool TryGetDirectInteractor(Collider other, out XRDirectInteractor directInteractor)
    {
        directInteractor = other.GetComponentInParent<XRDirectInteractor>();
        return directInteractor != null;
    }

    void PressButton(string source)
    {
        if (isPressed) return;

        isPressed = true;

        if (meshRenderer != null && matPressed != null)
            meshRenderer.material = matPressed;

        GetComponent<AudioSource>()?.Play();

        transform.localPosition = startLocalPosition + Vector3.back * pressDepth;

        Debug.Log($"Button {floorNumber} pressed via {source}.", this);

        if (isExteriorButton)
        {
            isOpen = true;
            Invoke(nameof(CloseDoors), 10f);
            Invoke(nameof(ReleaseButton), 0.2f);
        }
        else if (isDoorButton)
        {
            // Bloqué en phase 3, 4
            if (GameManager.Instance != null &&
                (GameManager.Instance.currentPhase == GameManager.Phase.Tension ||
                 GameManager.Instance.currentPhase == GameManager.Phase.Puzzle))
            {
                Debug.Log("Porte bloquée — phase active !");
                Invoke(nameof(ReleaseButton), 0.2f);
                return;
            }

            isOpen = !isOpen;
            Invoke(nameof(ReleaseButton), 0.2f);
        }
        else
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError($"[{nameof(ElevatorButton)}] GameManager.Instance is null.", this);
                return;
            }

            GameManager.Instance.RequestFloor(floorNumber);
            Invoke(nameof(ReleaseButton), 0.3f);
        }
    }

    void CloseDoors()
    {
        isOpen = false;
    }

    void ReleaseButton()
    {
        if (!isPressed) return;

        isPressed = false;

        if (meshRenderer != null && matNormal != null)
            meshRenderer.material = matNormal;

        transform.localPosition = startLocalPosition;
    }
}