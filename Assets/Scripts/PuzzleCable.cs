using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

public class PuzzleCable : MonoBehaviour
{
    [Header("Cable Setup")]
    [SerializeField] private int cableID;
    [SerializeField] private GameObject linkedSocket;
    [SerializeField] private Material matSocketConnected;
    [SerializeField] private Material matCableConnected;

    private MeshRenderer socketRenderer;
    private MeshRenderer cableRenderer;
    private Vector3 startPosition;
    private bool isConnected = false;
    private bool isHeld = false;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        cableRenderer = GetComponent<MeshRenderer>();
        startPosition = transform.localPosition;

        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    void Start()
    {
        if (linkedSocket != null)
            socketRenderer = linkedSocket.GetComponent<MeshRenderer>();
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        if (isConnected) return;
        isHeld = true;
    }

    void OnReleased(SelectExitEventArgs args)
    {
        if (isConnected) return;
        isHeld = false;

        transform.localPosition = startPosition;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isHeld) return;
        if (isConnected) return;

        if (other.gameObject == linkedSocket)
        {
            PuzzleManager.Instance.TryConnectCable(cableID, this);
        }
    }

    public void Connect()
    {
        isConnected = true;
        isHeld = false;

        transform.position = linkedSocket.transform.position;
        transform.SetParent(linkedSocket.transform);

        grabInteractable.enabled = false;

        if (socketRenderer != null && matSocketConnected != null)
            socketRenderer.material = matSocketConnected;

        if (cableRenderer != null && matCableConnected != null)
            cableRenderer.material = matCableConnected;

        GetComponent<AudioSource>()?.Play();

        Debug.Log($"Cable {cableID} connecté !");
    }

    public bool IsConnected => isConnected;
}