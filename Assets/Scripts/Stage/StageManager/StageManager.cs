using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class StageManager : NetworkSingletonBehaviour<StageManager>
{
    public StageLoader StageLoader { get; set; }
    
    protected override void Init()
    {
        _isDestroyOnLoad = true;
        base.Init();
    }
}
