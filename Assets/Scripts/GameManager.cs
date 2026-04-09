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

    void Awake()
    {
        Instance = this;
    }

    public void SetPhase(Phase newPhase)
    {
        currentPhase = newPhase;
        Debug.Log("Nouvelle phase : " + newPhase);

        switch (newPhase)
        {
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