using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Rigidbody))]
public class PlanetCharacterController : MonoBehaviour
{
    [Header("References")]
    public Transform planetCenter;

    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 10f;
    // float jumpForce = 8f;
    public float gravityMultiplier = 25f;
    public float groundCheckOffset = 0.2f;

    [Header("Debug")]
    public bool enableLogs = true;

    private CharacterController controller;
    private PlayerControls input;

    private Vector2 moveInput;
    private Vector3 playerVelocity;
    private bool isGrounded;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Desactivamos la gravedad global

        controller = GetComponent<CharacterController>();

        // Crear la instancia del asset generado
        input = new PlayerControls();
        if (enableLogs) Debug.Log("[Input] PlayerControls instanciado: " + (input != null));
    }

    void OnEnable()
    {
        if (input == null) input = new PlayerControls();
        // Habilitar action map (asegúrate de que 'Player' existe en la clase generada)
        try
        {
            input.Player.Enable();
            if (enableLogs) Debug.Log("[Input] input.Player habilitado");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Input] Error habilitando input.Player. ¿El action map 'Player' existe? " + e.Message);
        }

        //// Suscribir solo al salto (opcional) — también podríamos leer Jump con ReadValue si falla el performed
        //input.Player.Jump.performed += ctx => {
        //    if (enableLogs) Debug.Log("[Input] Jump.performed");
        //    TryJump();
        //};
    }

    void OnDisable()
    {
        if (input != null)
        {
            try { input.Player.Disable(); }
            catch { }
        }
    }

    void Update()
    {
        // Lectura por frame — esto es la forma fiable
        if (input == null)
        {
            if (enableLogs) Debug.LogWarning("[Input] input es null en Update, intentando reinstanciar.");
            input = new PlayerControls();
            input.Player.Enable();
        }

        // Leer Move cada frame
        moveInput = Vector2.zero;
        try
        {
            moveInput = input.Player.Move.ReadValue<Vector2>();
        }
        catch (System.Exception e)
        {
            if (enableLogs) Debug.LogError("[Input] Error leyendo Move: " + e.Message);
        }

        if (enableLogs)
        {
            // Imprime cuando hay input distinto de cero (o imprime siempre si quieres)
            if (moveInput.sqrMagnitude > 0.0001f)
                Debug.Log($"[Input] Move value: {moveInput}");
        }

        HandleGravity();
        HandleMovement();
    }

    void HandleGravity()
    {
        if (planetCenter == null)
        {
            if (enableLogs) Debug.LogError("[PlanetController] planetCenter NO asignado en inspector.");
            return;
        }
        // Dirección hacia el punto
        Vector3 direction = (planetCenter.position - transform.position).normalized;

        float distance = Vector3.Distance(transform.position, planetCenter.position); //medir el radio
        float force = gravityMultiplier / (distance * distance); //dividir la fuerza por el radio a el cuadrado para simular la gravedad mas realista

        rb.AddForce(direction * force, ForceMode.Acceleration); // Aplicar fuerza

        Vector3 down = (planetCenter.position - transform.position).normalized;

        Vector3 up = -down;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.FromToRotation(transform.up, up) * transform.rotation,
            Time.deltaTime * 10f
        );
    }

    void HandleMovement()
    {
        if (planetCenter == null) return;

        Vector3 down = (planetCenter.position - transform.position).normalized;
        Vector3 up = -down;

        // Protección si Camera.main es null
        if (Camera.main == null)
        {
            if (enableLogs) Debug.LogError("[PlanetController] Camera.main es null. Asegúrate de que la cámara principal tenga tag 'MainCamera'.");
            return;
        }

        Vector3 camForward = Vector3.ProjectOnPlane(Camera.main.transform.forward, up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(Camera.main.transform.right, up).normalized;

        Vector3 moveDir = (camForward * moveInput.y + camRight * moveInput.x);

        // Evitar NaNs
        if (moveDir.sqrMagnitude > 1e-6f) moveDir = moveDir.normalized;
        else moveDir = Vector3.zero;

        controller.Move(moveDir * moveSpeed * Time.deltaTime);

        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        }

        controller.Move(playerVelocity * Time.deltaTime);
    }

    //void TryJump()
    //{
    //    if (!isGrounded) return;
    //    Vector3 up = (transform.position - planetCenter.position).normalized;
    //    playerVelocity += up * jumpForce;
    //}
}

