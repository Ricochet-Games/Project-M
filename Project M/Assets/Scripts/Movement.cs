using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;

public class Movement : MonoBehaviour
{
    private PlayerInputActions input;
    [SerializeField] Transform cameraTransform;

    public Vector3 CurrentMoveDirection { get; private set; }

    public Vector2 Move { get; private set; }
    [SerializeField] float moveSpeed = 5f;

    void Awake()
    {
        input = new PlayerInputActions();
    }

    void OnEnable() => input.Enable();
    void OnDisable() => input.Disable();

    void Update()
    {
        Move = input.Player.Move.ReadValue<Vector2>();
        MovePlayer();
    }
    void MovePlayer()
    {
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 move = right * Move.x + forward * Move.y;

        CurrentMoveDirection = move; 

        transform.position += move * moveSpeed * Time.deltaTime;
    }
}
  
