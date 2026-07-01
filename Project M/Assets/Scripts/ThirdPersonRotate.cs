using UnityEngine;

public class ThirdPersonRotate : MonoBehaviour
{
    [SerializeField] Movement movement;
    [SerializeField] float rotationSpeed = 12f;

    void Update()
    {
        RotateTowardsMovement();
    }

    void RotateTowardsMovement()
    {
        Vector3 move = movement.CurrentMoveDirection;

        if (move.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(move, Vector3.up);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}
