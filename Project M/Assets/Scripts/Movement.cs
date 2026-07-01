using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;

public class Movement : MonoBehaviour
{
    private PlayerInputActions input;

    public Vector2 Move { get; private set; }
    [SerializeField] float moveSpeed = 5f;

    public Vector2 Look { get; private set; }
    [SerializeField] Transform cameraTransform;
    [SerializeField] float sensitivity = 2f;
    float xRotation;

    void Awake()
    {
        input = new PlayerInputActions();
    }

    void OnEnable() => input.Enable();
    void OnDisable() => input.Disable();

    void Update()
    {
        Move = input.Player.Move.ReadValue<Vector2>();
        Look = input.Player.Look.ReadValue<Vector2>();
        Rotate();
        MovePlayer();
    }
    void MovePlayer()
    {
        Vector3 move = transform.right * Move.x + transform.forward * Move.y;

        transform.position += move * moveSpeed * Time.deltaTime;
    }
    void Rotate()
    {

        float mouseX = Look.x * sensitivity;
        float mouseY = Look.y * sensitivity;


        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
  
