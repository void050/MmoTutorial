using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"NetworkConfig.TickRate: {Shared.NetworkConfig.TickRate}" );
    }
}
