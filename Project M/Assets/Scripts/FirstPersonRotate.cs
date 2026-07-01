using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;

public class FirstPersonRotate: MonoBehaviour
{
    private PlayerInputActions input;
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
        Look = input.Player.Look.ReadValue<Vector2>();
        Rotate();
    }
    void Rotate()
    {

        float mouseX = Look.x * sensitivity;
        float mouseY = Look.y * sensitivity;


        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}

