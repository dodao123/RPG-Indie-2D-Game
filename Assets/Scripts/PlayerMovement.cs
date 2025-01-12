using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f; // Tốc độ di chuyển của nhân vật
    public float dashDistance = 5f; // Khoảng cách di chuyển khi nhấn Space
    private float dashSpeed = 20f; // Tốc độ khi dịch chuyển nhanh (Dash)

    private Rigidbody2D rb; // Rigidbody2D của nhân vật
    private Vector2 movement; // Vector dùng để lưu hướng di chuyển
    public Animator anim;

    private SpriteRenderer spriteRenderer; // SpriteRenderer của nhân vật

    [Header("Attack Settings")]
    public float attackDuration = 0.5f; // Thời gian animation chém (tính bằng giây)

    private bool isAttacking = false; // Trạng thái tấn công

    [Header("Attack Colliders")]
    public Collider2D defaultCollider; // Collider mặc định
    public Collider2D attackCollider;  // Collider dùng khi tấn công
    public GameObject attackColliderObject; // GameObject chứa attackCollider

    private CapsuleCollider2D capsuleCollider; // Tham chiếu đến Capsule Collider

    private bool isDashing = false; // Trạng thái dịch chuyển nhanh (Dash)

    void Start()
    {
        // Gán Rigidbody2D cho biến rb
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component is missing from the game object.");
        }

        // Kiểm tra Animator
        if (anim == null)
        {
            Debug.LogError("Animator component is missing from the game object.");
        }

        // Gán SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component is missing from the game object.");
        }

        // Đảm bảo collider không kích hoạt khi bắt đầu
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }
        // Gán Capsule Collider vào biến
        capsuleCollider = GetComponent<CapsuleCollider2D>();

        if (capsuleCollider == null)
        {
            Debug.LogError("CapsuleCollider2D component is missing from the game object.");
        }
    }

    void Update()
    {
        // Lấy input từ bàn phím (WASD hoặc phím mũi tên)
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Đảm bảo tốc độ di chuyển không bị tăng khi đi chéo
        if (movement.magnitude > 1)
        {
            movement = movement.normalized;
        }

        // Cập nhật animation trạng thái di chuyển
        anim.SetBool("isMove", movement.magnitude > 0);

        // Cập nhật hướng trong Animator
        anim.SetFloat("Horizontal", movement.x);
        anim.SetFloat("Vertical", movement.y);

        // Quay nhân vật theo hướng con trỏ chuột
        FlipTowardsMouse();

        // Kiểm tra trạng thái nhấn chuột trái
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            StartCoroutine(HandleAttackAnimation());
        }

        // Kiểm tra sự kiện nhấn phím Space để thực hiện Dash
        if (Input.GetKeyDown(KeyCode.Space) && !isDashing)
        {
            StartCoroutine(HandleDash()); // Kích hoạt chức năng Dash
        }
    }

    void FixedUpdate()
    {
        // Di chuyển nhân vật bằng Rigidbody2D khi không đang dash
        if (!isDashing)
        {
            rb.velocity = movement * moveSpeed;
        }
    }

    private void FlipTowardsMouse()
    {
        // Lấy vị trí của con trỏ chuột trong không gian thế giới
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f; // Đảm bảo tọa độ Z là 0

        // Xác định nhân vật có cần lật không
        if (mousePosition.x > transform.position.x)
        {
            FlipCharacter(false); // Mặt phải
        }
        else if (mousePosition.x < transform.position.x)
        {
            FlipCharacter(true); // Mặt trái
        }
    }

    private void FlipCharacter(bool flipX)
    {
        spriteRenderer.flipX = flipX;

        // Cập nhật Capsule Collider cho nhân vật
        if (capsuleCollider != null)
        {
            Vector2 center = capsuleCollider.offset;
            center.x = Mathf.Abs(center.x) * (flipX ? -1 : 1);
            capsuleCollider.offset = center;
        }

        // Cập nhật hướng của attackCollider
        if (attackColliderObject != null)
        {
            Vector3 localScale = attackColliderObject.transform.localScale;
            localScale.x = Mathf.Abs(localScale.x) * (flipX ? -1 : 1);
            attackColliderObject.transform.localScale = localScale;
        }
    }

    // Coroutine xử lý animation tấn công
    private IEnumerator HandleAttackAnimation()
    {
        isAttacking = true; // Vô hiệu hóa chuột trái
        anim.SetBool("isClickLeftMouse", true); // Bật trạng thái tấn công

        // Kích hoạt collider tấn công
        if (attackCollider != null)
        {
            attackCollider.enabled = true;
        }

        // Vô hiệu hóa collider mặc định
        if (defaultCollider != null)
        {
            defaultCollider.enabled = true;
        }

        yield return new WaitForSeconds(attackDuration); // Chờ thời gian animation chạy xong

        // Tắt trạng thái tấn công
        anim.SetBool("isClickLeftMouse", false);

        // Kích hoạt lại collider mặc định
        if (defaultCollider != null)
        {
            defaultCollider.enabled = true;
        }

        // Vô hiệu hóa collider tấn công
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }

        isAttacking = false; // Kích hoạt lại chuột trái
    }

    // Coroutine xử lý Dash
    private IEnumerator HandleDash()
    {
        isDashing = true;

        // Tăng tốc độ di chuyển khi Dash
        float initialSpeed = moveSpeed;
        moveSpeed = dashSpeed;

        // Lấy hướng di chuyển từ input WASD (horizontal và vertical)
        Vector2 dashDirection = movement.normalized; // Đảm bảo hướng di chuyển được chuẩn hóa

        // Thực hiện di chuyển ngắn theo hướng đã được xác định
        rb.velocity = dashDirection * dashDistance;

        // Chờ một thời gian ngắn (0.2 giây) để dừng lại
        yield return new WaitForSeconds(0.2f);

        // Quay lại tốc độ di chuyển ban đầu
        moveSpeed = initialSpeed;

        isDashing = false;
    }

}
