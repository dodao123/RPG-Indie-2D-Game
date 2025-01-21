using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextLevel : MonoBehaviour
{
    // Biến chứa tham chiếu đến object Image
    public GameObject imageObject;

    // Start is called before the first frame update
    void Start()
    {
        // Ẩn object Image khi bắt đầu game
        if (imageObject != null)
        {
            imageObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Lấy tham chiếu đến EnemySpawner
        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();

        if (spawner != null) // Kiểm tra nếu spawner tồn tại
        {
            int wave = spawner.GetcurrentWave();
            int SoulCollected = spawner.GetplayerSoulPoints();

            // Kiểm tra nếu đối tượng va chạm có tag "Player"
            if (collision.CompareTag("Player"))
            {
                Debug.Log("Current Wave: " + wave);

                // Nếu điểm của người chơi nhỏ hơn 4, hiện object trong 3 giây
                if (SoulCollected < 4)
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

    // Coroutine để hiển thị object trong 3 giây
    private IEnumerator ShowImageFor3Seconds()
    {
        if (imageObject != null)
        {
            // Hiển thị object
            imageObject.SetActive(true);

            // Chờ 3 giây
            yield return new WaitForSeconds(3f);

            // Ẩn object
            imageObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Image object is not assigned in the Inspector!");
        }
    }
}
