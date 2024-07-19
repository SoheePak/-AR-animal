using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackPanel : UIPanel
{
    private static BlackPanel _instance;

    protected override void Awake()
    {
        base.Awake();
        _instance = this;
    }

    public static BlackPanel Get()
    {
        return _instance;
    }
}
