using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    public InputActionAsset actions;
    private InputAction lookAction;
    public float sensitivity = 5f;
    public Transform playerBody;
    float xRotation = 0f;
    public GameObject player;
    private Animator animator;
    private Rigidbody rb;
    private void Awake()
    {
        lookAction = actions.FindActionMap("Player").FindAction("Look");
    }
    void OnEnable() => lookAction.Enable();
    void OnDisable() => lookAction.Disable();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // マウスカーソルを非表示にし、画面中央にロック
        Cursor.lockState = CursorLockMode.Locked;
        animator = player.GetComponent<Animator>();
        rb = player.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (rb.isKinematic)
        {
            return;
        }
        else 
        {
            // マウス入力を取得
            Vector2 lookInput = lookAction.ReadValue<Vector2>();
            float mouseX = lookInput.x * sensitivity * Time.deltaTime;
            float mouseY = lookInput.y * sensitivity * Time.deltaTime;
            if (mouseX != 0)
            {
                if (mouseX > 0.1)
                {
                    animator.SetBool("is_turn_right", true);
                }
                else if (mouseX < -0.1)
                {
                    animator.SetBool("is_turn_left", true);
                }
            }
            else
            {
                animator.SetBool("is_turn_right", false);
                animator.SetBool("is_turn_left", false);
            }
            // カメラを反転回転
            xRotation -= mouseY;
            // カメラの回転範囲を制限
            xRotation = Mathf.Clamp(xRotation, 10f, 30f);
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}
