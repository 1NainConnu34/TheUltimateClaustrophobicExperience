using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ExteriorButton : MonoBehaviour
{
    public GameObject doorLeft;
    public GameObject doorRight;
    public float openDistance = 0.95f;
    public float openSpeed = 1.5f;

    private bool isOpen = false;
    private Vector3 doorLeftClosed;
    private Vector3 doorRightClosed;

    void Start()
    {
        doorLeftClosed = doorLeft.transform.localPosition;
        doorRightClosed = doorRight.transform.localPosition;

        var interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        interactable.selectEntered.AddListener(OnButtonPressed);
    }

    void OnButtonPressed(SelectEnterEventArgs args)
    {
        isOpen = !isOpen;
    }

    void Update()
    {
        if (isOpen)
        {
            doorLeft.transform.localPosition = Vector3.Lerp(
                doorLeft.transform.localPosition,
                doorLeftClosed + new Vector3(-openDistance, 0, 0),
                Time.deltaTime * openSpeed
            );
            doorRight.transform.localPosition = Vector3.Lerp(
                doorRight.transform.localPosition,
                doorRightClosed + new Vector3(openDistance, 0, 0),
                Time.deltaTime * openSpeed
            );
        }
        else
        {
            doorLeft.transform.localPosition = Vector3.Lerp(
                doorLeft.transform.localPosition,
                doorLeftClosed,
                Time.deltaTime * openSpeed
            );
            doorRight.transform.localPosition = Vector3.Lerp(
                doorRight.transform.localPosition,
                doorRightClosed,
                Time.deltaTime * openSpeed
            );
        }
    }
}