using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        FixedPoint.Sqrt(FixedPoint.CreateByLong(63));
        FixedPoint.Sqrt(FixedPoint.CreateByLong(9000));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
