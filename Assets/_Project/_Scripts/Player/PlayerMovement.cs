using FishNet.Object;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private PlayerInput playerInput;
    private Rigidbody2D rb;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        playerInput.OnJump += HandleJump;
    }

    private void Update()
    {
        if (!IsOwner) return; // Only allow the local player to control movement

        Vector2 inputVector = playerInput.GetMoveVector();

        // Only move horizontally
        Vector2 moveVector = Vector2.zero;
        if (inputVector.x != 0)
        {
            moveVector.x = inputVector.x;
        }

        rb.linearVelocity = new Vector2(moveVector.x * moveSpeed, rb.linearVelocity.y);

    }
    private void HandleJump()
    {
        SendJumpToServer();
    }

    [ServerRpc]
    private void SendMoveInputToServer(Vector2 moveVector)
    {
        // Clamp to prevent excessive speed by client
        moveVector = new Vector2(Mathf.Clamp(moveVector.x, -1f, 1f), 0f);
        rb.linearVelocity = new Vector2(moveVector.x * moveSpeed, rb.linearVelocity.y);
    }

    [ServerRpc]
    private void SendJumpToServer()
    {
        Debug.Log("Jump action triggered on server!");
    }
}
