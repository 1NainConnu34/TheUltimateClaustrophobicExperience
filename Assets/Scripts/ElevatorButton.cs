using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ElevatorButton : MonoBehaviour
{
    public int floorNumber;        
    public Material matNormal;        
    public Material matPressed;        

    private MeshRenderer meshRenderer;
    private bool isPressed = false;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        // Récupère le composant XR et écoute l'événement
        var interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        interactable.selectEntered.AddListener(OnButtonPressed);
    }

    void OnButtonPressed(SelectEnterEventArgs args)
    {
        if (isPressed) return;
        isPressed = true;

        // Allume le bouton visuellement
        meshRenderer.material = matPressed;

        // Son de clic
        GetComponent<AudioSource>()?.Play();

        // Décale légèrement le bouton vers l'intérieur (effet physique)
        transform.localPosition += new Vector3(0, 0, -0.01f);

        Debug.Log("Bouton étage " + floorNumber + " appuyé");

        // Déclenche la phase 2 après 2 secondes
        Invoke("TriggerPanne", 2f);
    }

    void TriggerPanne()
    {
        GameManager.Instance.SetPhase(GameManager.Phase.Panne);
    }
}