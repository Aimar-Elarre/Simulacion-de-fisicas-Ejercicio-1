using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Gravity : MonoBehaviour
{
    public Transform gravityCenter; // Punto hacia donde cae
    public float gravityStrength = 9.81f; // Fuerza de la gravedad

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Desactivamos la gravedad global
    }

    void FixedUpdate()
    {
        if (gravityCenter == null) return;

        // Dirección hacia el punto
        Vector3 direction = (gravityCenter.position - transform.position).normalized;

        float distance = Vector3.Distance(transform.position, gravityCenter.position); //medir el radio
        float force = gravityStrength / (distance * distance); //dividir la fuerza por el radio a el cuadrado para simular la gravedad mas realista
        
        rb.AddForce(direction * force, ForceMode.Acceleration); // Aplicar fuerza
        
        
    }
}
