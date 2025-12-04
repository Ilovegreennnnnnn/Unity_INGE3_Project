using UnityEngine;

public class TirageTest : MonoBehaviour
{
    [Header("Réglages du tirage")]
    [SerializeField] private Vector3 pullDirection = Vector3.down;
    [SerializeField] private float pullDistance = 0.2f;

    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.localPosition;
    }

    // Cette fonction sera appelée quand tu tires la poignée
    public void PullHandle(float amount)
    {
        Vector3 offset = pullDirection.normalized * pullDistance * amount;
        transform.localPosition = initialPosition + offset;
    }
}
