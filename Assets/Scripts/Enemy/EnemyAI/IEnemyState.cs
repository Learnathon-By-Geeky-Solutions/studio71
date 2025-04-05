using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace patrolEnemy
{
    public interface IEnemyState
    {
        void Enter();
        void Execute();
        void Exit();
    }
}