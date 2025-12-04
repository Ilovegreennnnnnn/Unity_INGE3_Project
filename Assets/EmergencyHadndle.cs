using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class EmergencyHandle : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{
    [Header("Handle Settings")]
    [SerializeField] private Transform handleTransform;
    [SerializeField] private float pullDistance = 0.3f; // Distance à tirer en mètres
    [SerializeField] private float activationThreshold = 0.8f; // 80% de la distance pour activer

    [Header("Treadmill Reference")]
    [SerializeField] private TreadmillsController treadmillController;

    [Header("Emergency Stop Settings")]
    [SerializeField] private float emergencyStopDuration = 3f; // Durée de l'arrêt
    [SerializeField] private float restartDelay = 1f; // Délai avant redémarrage

    [Header("Visual Feedback")]
    [SerializeField] private Renderer handleRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color activatedColor = Color.red;

    private Vector3 initialPosition;
    private Vector3 pullDirection;
    private bool isActivated = false;
    private bool isEmergencyActive = false;

    protected override void Awake()
    {
        base.Awake();

        if (handleTransform == null)
            handleTransform = transform;

        initialPosition = handleTransform.localPosition;
        pullDirection = -transform.up; // Tire vers le bas par défaut

        if (handleRenderer != null)
        {
            handleRenderer.material.color = normalColor;
        }
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        isActivated = false;
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
        {
            if (isSelected)
            {
                UpdateHandlePosition();
            }
            else if (!isEmergencyActive)
            {
                // Retour automatique à la position initiale
                ReturnToInitialPosition();
            }
        }
    }

    private void UpdateHandlePosition()
    {
        if (isEmergencyActive) return;

        // Calculer la distance tirée
        Vector3 currentOffset = handleTransform.localPosition - initialPosition;
        float currentPull = Vector3.Dot(currentOffset, pullDirection);

        // Limiter le mouvement
        currentPull = Mathf.Clamp(currentPull, 0f, pullDistance);

        // Appliquer la position
        handleTransform.localPosition = initialPosition + pullDirection * currentPull;

        // Vérifier si la poignée est suffisamment tirée
        float pullPercentage = currentPull / pullDistance;

        if (pullPercentage >= activationThreshold && !isActivated)
        {
            isActivated = true;
            TriggerEmergencyStop();
        }
    }

    private void ReturnToInitialPosition()
    {
        // Retour progressif à la position initiale
        handleTransform.localPosition = Vector3.Lerp(
            handleTransform.localPosition,
            initialPosition,
            Time.deltaTime * 5f
        );

        // Réinitialiser l'état si on est revenu à la position initiale
        if (Vector3.Distance(handleTransform.localPosition, initialPosition) < 0.01f)
        {
            isActivated = false;
        }
    }

    private void TriggerEmergencyStop()
    {
        if (treadmillController != null && !isEmergencyActive)
        {
            StartCoroutine(EmergencyStopRoutine());
        }
    }

    private IEnumerator EmergencyStopRoutine()
    {
        isEmergencyActive = true;

        // Feedback visuel
        if (handleRenderer != null)
        {
            handleRenderer.material.color = activatedColor;
        }

        // Arrêt du tapis
        treadmillController.SetPaused(true);
        Debug.Log("🚨 ARRÊT D'URGENCE ACTIVÉ!");

        // Attendre la durée d'arrêt
        yield return new WaitForSeconds(emergencyStopDuration);

        Debug.Log("⏳ Redémarrage dans " + restartDelay + " secondes...");

        // Faire clignoter la poignée pendant le compte à rebours
        float elapsedTime = 0f;
        while (elapsedTime < restartDelay)
        {
            if (handleRenderer != null)
            {
                float t = Mathf.PingPong(elapsedTime * 4f, 1f);
                handleRenderer.material.color = Color.Lerp(activatedColor, normalColor, t);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Redémarrage
        treadmillController.SetPaused(false);

        // Réinitialiser le feedback visuel
        if (handleRenderer != null)
        {
            handleRenderer.material.color = normalColor;
        }

        isEmergencyActive = false;
        Debug.Log("✅ Tapis redémarré");
    }

    private void OnDrawGizmosSelected()
    {
        if (handleTransform == null) return;

        // Visualiser la distance de tirage
        Gizmos.color = Color.yellow;
        Vector3 worldPullDir = transform.TransformDirection(pullDirection);
        Gizmos.DrawRay(handleTransform.position, worldPullDir * pullDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(handleTransform.position + worldPullDir * pullDistance * activationThreshold, 0.02f);
    }
}