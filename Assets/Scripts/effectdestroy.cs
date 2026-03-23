using UnityEngine;

public class effectdestroy : MonoBehaviour
{
    public float lifetime; // Thời gian tồn tại của hiệu ứng
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, lifetime); // Hủy đối tượng sau một khoảng thời gian nhất định để tránh rác
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
