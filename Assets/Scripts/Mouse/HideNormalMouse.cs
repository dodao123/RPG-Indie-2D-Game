using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        // Ẩn con trỏ mặc định của hệ thống
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined; // Giữ con trỏ trong cửa sổ game
    }
}

