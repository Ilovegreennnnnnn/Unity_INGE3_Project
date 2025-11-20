using UnityEngine;
using System.Collections;

public class Distributeur : MonoBehaviour
{
    [Header("Paramètres de Spawn")]
    public GameObject[] objetsADistribuer; // Glisse tes 3 prefabs ici
    public float intervalle = 2.0f;        // Temps entre chaque spawn

    [Header("Paramètres de Physique")]
    public float forceDePoussee = 5f;      // La puissance du "jet"

    void Start()
    {
        StartCoroutine(BoucleDeDistribution());
    }

    IEnumerator BoucleDeDistribution()
    {
        while (true)
        {
            SpawnObjet();
            yield return new WaitForSeconds(intervalle);
        }
    }

    void SpawnObjet()
    {
        if (objetsADistribuer.Length == 0) return;

        // 1. Choisir un objet au hasard
        int index = Random.Range(0, objetsADistribuer.Length);

        // 2. Créer l'objet et le stocker dans une variable 'nouvelObjet'
        GameObject nouvelObjet = Instantiate(objetsADistribuer[index], transform.position, transform.rotation);

        // 3. Récupérer le Rigidbody pour appliquer la force
        Rigidbody rb = nouvelObjet.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // Pousse l'objet sur l'axe X (Vector3.right = X positif / flèche rouge)
            // ForceMode.Impulse donne une poussée immédiate (comme un coup de canon)
            rb.AddForce(Vector3.right * forceDePoussee, ForceMode.Impulse);
        }
        else
        {
            Debug.LogWarning("Attention : Le prefab n'a pas de Rigidbody, il ne peut pas être poussé !");
        }
    }
}