using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LatencyManager : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TextMeshProUGUI txtmshpro;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float latency = (NetworkManager.Singleton.LocalTime.TimeAsFloat - NetworkManager.Singleton.ServerTime.TimeAsFloat);
        txtmshpro.text = latency.ToString();
        
    }
}
