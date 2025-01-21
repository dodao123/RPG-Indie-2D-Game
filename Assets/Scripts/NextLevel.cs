using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextLevel : MonoBehaviour
{
    // Tạo 4 biến để chứa các object
    [SerializeField] private GameObject object1;
    [SerializeField] private GameObject object2;
    [SerializeField] private GameObject object3;
    [SerializeField] private GameObject object4;

    // Biến chứa tham chiếu đến object Image
    public GameObject imageObject;

    void Start()
    {
        // Ẩn tất cả object khi bắt đầu
        if (imageObject != null) imageObject.SetActive(false);
        DisableAllObjects();
    }

    void Update()
    {
        // Lấy tham chiếu đến EnemySpawner
        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null)
        {
            int soulPoints = spawner.GetplayerSoulPoints();
            UpdateObjects(soulPoints);
        }
    }

    // Hàm cập nhật hiển thị các object dựa trên số điểm
    private void UpdateObjects(int soulPoints)
    {
        if (object1 != null) object1.SetActive(soulPoints >= 1);
        if (object2 != null) object2.SetActive(soulPoints >= 2);
        if (object3 != null) object3.SetActive(soulPoints >= 3);
        if (object4 != null) object4.SetActive(soulPoints >= 4);
    }

    // Hàm tắt tất cả các object
    private void DisableAllObjects()
    {
        if (object1 != null) object1.SetActive(false);
        if (object2 != null) object2.SetActive(false);
        if (object3 != null) object3.SetActive(false);
        if (object4 != null) object4.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null)
        {
            int wave = spawner.GetcurrentWave();
            int soulCollected = spawner.GetplayerSoulPoints();

            if (collision.CompareTag("Player"))
            {
                Debug.Log("Current Wave: " + wave);
                if (soulCollected < 4)
                {
                    Debug.Log("Player soul points are less than 4. Showing image...");
                    StartCoroutine(ShowImageFor3Seconds());
                }
            }
        }
        else
        {
            Debug.LogError("EnemySpawner not found in the scene!");
        }
    }

    private IEnumerator ShowImageFor3Seconds()
    {
        if (imageObject != null)
        {
            imageObject.SetActive(true);
            yield return new WaitForSeconds(3f);
            imageObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Image object is not assigned in the Inspector!");
        }
    }
}