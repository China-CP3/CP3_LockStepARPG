using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMgr
{
    private static GameMgr instance;
    public static GameMgr Instance
    {
        get { if (instance == null) instance = new GameMgr();return instance;}
    }
    private GameMgr() { }

    public int Gold = 0;
    public void AddGold()
    {
        Gold++;
        Debug.LogFormat("Gold:{0}", Gold);
    }

}
