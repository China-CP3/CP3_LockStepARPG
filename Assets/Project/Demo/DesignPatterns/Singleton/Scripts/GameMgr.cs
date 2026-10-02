using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMgr
{
    public static GameMgr Instance { get; }
    private GameMgr() { }

    public int Gold = 0;
    public void AddGold()
    {
        Gold++;
        Debug.LogFormat("Gold:{0}", Gold);
    }

}
