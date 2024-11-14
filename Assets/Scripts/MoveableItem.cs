using UnityEngine;

public class MoveableItem : MonoBehaviour
{
    Player player;
    [SerializeField] GameObject playerHand;
    new Rigidbody rigidbody;
    float timeLeftThrowing;
    bool disabled;

    public float TimeLeftThrowing { get => timeLeftThrowing; set => timeLeftThrowing = value; }
    public bool Disabled { get => disabled; set => disabled = value; }

    private void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
        rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (timeLeftThrowing > 0)
        {
            timeLeftThrowing -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (!Disabled && player.ObjectInHand==null && collider.GetComponent<Player>()!=null && timeLeftThrowing<=0)
        {
            player.PutObjectInHand(gameObject);
            rigidbody.isKinematic = true;
            transform.parent = playerHand.transform;
            transform.localPosition = new Vector3(0, 0, 0);
            transform.localRotation = Quaternion.identity;
        }
    }
}
