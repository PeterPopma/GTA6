using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    [SerializeField] float delay = 2;
    float timeLeft;

    public void DestroyAfterDelay()
    {
        timeLeft = delay;
    }

    // Update is called once per frame
    void Update()
    {
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
