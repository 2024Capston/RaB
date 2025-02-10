using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public abstract class StageManager : NetworkSingletonBehaviour<StageManager>
{
    public abstract void StartGame();
    public abstract void RestartGame();
    public abstract void EndGame();
}
