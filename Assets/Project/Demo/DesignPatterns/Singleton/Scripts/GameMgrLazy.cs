using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMgrLazy
{
    private GameMgrLazy() { }

    private static readonly Lazy<GameMgrLazy> lazy = new Lazy<GameMgrLazy>(() => { return new GameMgrLazy(); });

    public static GameMgrLazy Instance => lazy.Value;

    public int Gold = 0;
    public void AddGold() { Gold++; Debug.LogFormat("Gold:{0}", Gold); }
}
