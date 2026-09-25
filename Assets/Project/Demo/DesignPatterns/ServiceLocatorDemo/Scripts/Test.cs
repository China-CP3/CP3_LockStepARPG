using CP3.Demo.ServiceLocator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ServiceLocator.RegisterService<IAudioService>(new AudioService());
        ServiceLocator.GetService<IAudioService>().PlayAudio();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
