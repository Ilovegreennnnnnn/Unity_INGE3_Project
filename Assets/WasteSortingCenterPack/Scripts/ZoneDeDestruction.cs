using UnityEngine;

public class ZoneDeDestruction : MonoBehaviour
{
    // Cette fonction se déclenche quand un objet entre dans le Trigger
    void OnTriggerEnter(Collider other)
    {
        // 'other' est l'objet qui vient d'entrer (ton prefab)

        // On détruit le GameObject qui est entré
        Destroy(other.gameObject);
    }
}