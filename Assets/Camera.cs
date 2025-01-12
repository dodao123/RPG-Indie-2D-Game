using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform player; // Tham chiếu đến nhân vật
    public Vector3 offset; // Vị trí chênh lệch giữa camera và nhân vật
    public float smoothSpeed = 0.125f; // Tốc độ mượt mà khi di chuyển camera

    void LateUpdate()
    {
        if (player != null)
        {
            // Chỉ tính toán vị trí x và y, bỏ qua z (camera sẽ giữ nguyên z)
            Vector3 desiredPosition = new Vector3(player.position.x, player.position.y, transform.position.z) + offset;

            // Lerp là một hàm chuyển động mượt mà giữa hai giá trị (ở đây là vị trí)
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            // Cập nhật vị trí của camera
            transform.position = smoothedPosition;

            // (Tùy chọn) Đảm bảo camera luôn nhìn về nhân vật (nếu cần)
            // Không cần nhìn theo nhân vật trong game 2D, chỉ cần giữ camera ổn định
        }
    }
}
