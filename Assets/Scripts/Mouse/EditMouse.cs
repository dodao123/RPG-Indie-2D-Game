using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    // Biến để kéo hình ảnh UI của con trỏ vào từ Inspector
    public RectTransform cursorTransform;

    void Start()
    {
        if (cursorTransform == null)
        {
            Debug.LogError("Cursor Transform is not assigned in the Inspector!");
        }
    }

    void Update()
    {
        // Kiểm tra nếu biến cursorTransform đã được kéo vào
        if (cursorTransform != null)
        {
            // Cập nhật vị trí con trỏ theo vị trí chuột
            Vector3 cursorPosition = Input.mousePosition;
            cursorTransform.position = cursorPosition;
        }
    }
}
