using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kaizerwald.StateMachine
{
    public abstract class StateMachine<T> : MonoBehaviour 
    where T : struct, Enum
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                               ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        [field:SerializeField] public T State { get; protected set; }
        
        public Dictionary<T, State<T>> States { get; protected set; }
        
        
    }
}
