using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SpeedLever : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{
    [Header("Lever Settings")]
    [SerializeField] private Transform leverTransform;
    [SerializeField] private float maxRotationAngle = 45f; // Angle max de rotation
    [SerializeField] private Vector3 rotationAxis = Vector3.right; // Axe de rotation (X par d�faut)

    [Header("Treadmill Reference")]
    [SerializeField] private TreadmillsController treadmillController;

    [Header("Speed Range")]
    [SerializeField, Range(0, 1)] private float minSpeedPercent = 0f; // 0% = arr�t
    [SerializeField, Range(0, 1)] private float maxSpeedPercent = 1f; // 100% = vitesse max
    [SerializeField] private float defaultSpeedPercent = 0.5f; // Vitesse par d�faut (50%)

    [Header("Lever Behavior")]
    [SerializeField] private bool returnToCenter = true; // Revenir au centre quand on l�che
    [SerializeField] private float returnSpeed = 5f; // Vitesse de retour
    [SerializeField] private bool snapToPositions = false; // Snap vers des positions d�finies
    [SerializeField] private int snapPositionCount = 5; // Nombre de positions de snap

    private Quaternion initialRotation;
    private Quaternion centerRotation;
    private float currentAngle = 0f;
    private float targetAngle = 0f;

    protected override void Awake()
    {
        base.Awake();

        if (leverTransform == null)
            leverTransform = transform;

        initialRotation = leverTransform.localRotation;

        // Calculer la rotation centrale (position de d�part)
        float startAngle = Mathf.Lerp(-maxRotationAngle, maxRotationAngle, defaultSpeedPercent);
        centerRotation = initialRotation * Quaternion.AngleAxis(startAngle, rotationAxis);
        leverTransform.localRotation = centerRotation;

        // Initialiser la vitesse par d�faut
        if (treadmillController != null)
        {
            treadmillController.SetSpeed(defaultSpeedPercent);
        }
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        // Enregistrer l'angle actuel comme point de d�part
        currentAngle = GetCurrentAngle();
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
        {
            if (isSelected)
            {
                UpdateLeverFromController();
            }
            else if (returnToCenter)
            {
                ReturnToCenter();
            }

            // Toujours mettre � jour la vitesse du tapis
            UpdateTreadmillSpeed();
        }
    }

    private void UpdateLeverFromController()
    {
        // R�cup�rer la position du contr�leur
        if (interactorsSelecting.Count > 0)
        {
            var interactor = interactorsSelecting[0];

            // Calculer l'angle bas� sur le mouvement du contr�leur
            // (Version simplifi�e - tu peux am�liorer avec un syst�me de physique)
            Vector3 controllerPos = interactor.transform.position;
            Vector3 leverToController = controllerPos - leverTransform.position;

            // Projeter sur le plan de rotation
            Vector3 leverForward = leverTransform.forward;
            Vector3 leverUp = leverTransform.up;

            float dot = Vector3.Dot(leverToController.normalized, leverForward);
            targetAngle = Mathf.Asin(Mathf.Clamp(dot, -1f, 1f)) * Mathf.Rad2Deg;
            targetAngle = Mathf.Clamp(targetAngle, -maxRotationAngle, maxRotationAngle);

            if (snapToPositions)
            {
                targetAngle = SnapAngle(targetAngle);
            }

            currentAngle = targetAngle;
        }

        ApplyRotation(currentAngle);
    }

    private void ReturnToCenter()
    {
        // Calculer l'angle cible (position centrale)
        float centerAngle = Mathf.Lerp(-maxRotationAngle, maxRotationAngle, defaultSpeedPercent);

        // Interpoler vers le centre
        currentAngle = Mathf.Lerp(currentAngle, centerAngle, Time.deltaTime * returnSpeed);

        ApplyRotation(currentAngle);
    }

    private void ApplyRotation(float angle)
    {
        leverTransform.localRotation = initialRotation * Quaternion.AngleAxis(angle, rotationAxis);
    }

    private float GetCurrentAngle()
    {
        Quaternion deltaRotation = Quaternion.Inverse(initialRotation) * leverTransform.localRotation;
        float angle;
        Vector3 axis;
        deltaRotation.ToAngleAxis(out angle, out axis);

        if (angle > 180f) angle -= 360f;

        // V�rifier si l'axe est dans la bonne direction
        if (Vector3.Dot(axis, rotationAxis) < 0)
            angle = -angle;

        return angle;
    }

    private float SnapAngle(float angle)
    {
        if (snapPositionCount <= 1) return angle;

        float angleStep = (maxRotationAngle * 2f) / (snapPositionCount - 1);
        float normalizedAngle = angle + maxRotationAngle;
        float snappedAngle = Mathf.Round(normalizedAngle / angleStep) * angleStep;

        return snappedAngle - maxRotationAngle;
    }

    private void UpdateTreadmillSpeed()
    {
        if (treadmillController == null) return;

        // Convertir l'angle en pourcentage de vitesse (0 � 1)
        float speedPercent = (currentAngle + maxRotationAngle) / (maxRotationAngle * 2f);
        speedPercent = Mathf.Lerp(minSpeedPercent, maxSpeedPercent, speedPercent);
        speedPercent = Mathf.Clamp01(speedPercent);

        // Ne pas changer la vitesse si le tapis est en pause
        if (!treadmillController.isPaused)
        {
            treadmillController.SetSpeed(speedPercent);
        }
    }

    // M�thode publique pour d�finir la vitesse manuellement
    public void SetSpeedPercent(float percent)
    {
        percent = Mathf.Clamp01(percent);
        float angle = Mathf.Lerp(-maxRotationAngle, maxRotationAngle, percent);
        currentAngle = angle;
        targetAngle = angle;
        ApplyRotation(angle);
    }

    private void OnDrawGizmosSelected()
    {
        if (leverTransform == null) return;

        // Visualiser la plage de rotation
        Vector3 worldAxis = leverTransform.TransformDirection(rotationAxis);
        Vector3 baseDir = Vector3.Cross(worldAxis, Vector3.up);
        if (baseDir.magnitude < 0.1f)
            baseDir = Vector3.Cross(worldAxis, Vector3.forward);
        baseDir.Normalize();

        Gizmos.color = Color.green;
        Gizmos.DrawRay(leverTransform.position, worldAxis * 0.1f);

        // Position min
        Gizmos.color = Color.red;
        Vector3 minDir = Quaternion.AngleAxis(-maxRotationAngle, worldAxis) * baseDir;
        Gizmos.DrawRay(leverTransform.position, minDir * 0.15f);

        // Position max
        Gizmos.color = Color.blue;
        Vector3 maxDir = Quaternion.AngleAxis(maxRotationAngle, worldAxis) * baseDir;
        Gizmos.DrawRay(leverTransform.position, maxDir * 0.15f);

        // Arc de rotation
        Gizmos.color = Color.yellow;
        Vector3 prevPoint = leverTransform.position + minDir * 0.12f;
        for (int i = 1; i <= 20; i++)
        {
            float t = i / 20f;
            float angle = Mathf.Lerp(-maxRotationAngle, maxRotationAngle, t);
            Vector3 dir = Quaternion.AngleAxis(angle, worldAxis) * baseDir;
            Vector3 point = leverTransform.position + dir * 0.12f;
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }
}