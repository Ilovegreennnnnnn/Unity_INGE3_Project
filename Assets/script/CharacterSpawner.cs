using UnityEngine;
using UnityEngine.InputSystem; // IMPORTANT : Nouveau système d'inputs !

public class CharacterSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject characterPrefab;
    public float maxRaycastDistance = 100f;

    private GameObject currentCharacter; // La dernière capsule créée
    private Camera mainCamera;
    private bool isRotating = false; // Est-ce qu'on pivote actuellement ?

    void Start()
    {
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("Pas de caméra principale trouvée !");
        }
    }

    void Update()
    {
        // Vérifie que le clavier est disponible
        if (Keyboard.current == null) return;

        // Détecte si G vient d'être pressé
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            SpawnCharacterAtCrosshair();
            isRotating = true; // Active la rotation
        }

        // Détecte si G est maintenu
        if (Keyboard.current.gKey.isPressed && isRotating && currentCharacter != null)
        {
            RotateCharacterTowardsCursor();
        }

        // Détecte si G est relâché
        if (Keyboard.current.gKey.wasReleasedThisFrame)
        {
            isRotating = false; // Arrête la rotation
        }
    }

    void SpawnCharacterAtCrosshair()
    {
        // Raycast depuis le centre de l'écran (curseur)
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // Lance le raycast
        if (Physics.Raycast(ray, out hit, maxRaycastDistance))
        {
            // Position où le raycast touche
            Vector3 spawnPosition = hit.point;
            spawnPosition.y += 1f; // Place la capsule au-dessus du sol

            // Crée la capsule
            currentCharacter = Instantiate(characterPrefab, spawnPosition, Quaternion.identity);

            Debug.Log("Capsule créée à : " + spawnPosition);
        }
        else
        {
            Debug.LogWarning("Aucune surface détectée ! Vise le sol.");
        }
    }

    void RotateCharacterTowardsCursor()
    {
        if (currentCharacter == null) return;

        // Raycast depuis le curseur pour trouver où il vise
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxRaycastDistance))
        {
            // Le point où le curseur vise sur le sol
            Vector3 targetPoint = hit.point;

            // Place ce point à la même hauteur que la capsule (rotation horizontale uniquement)
            targetPoint.y = currentCharacter.transform.position.y;

            // La capsule REGARDE ce point
            currentCharacter.transform.LookAt(targetPoint);
        }
    }
}