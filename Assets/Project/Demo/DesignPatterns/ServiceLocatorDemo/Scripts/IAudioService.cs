using CP3.Demo.ServiceLocator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAudioService : IMarkService
{
    void PlayAudio();//接口里默认就是public abstract

}
