
namespace Kaizerwald.StateMachine
{
    public sealed class RegimentIdleState : RegimentStateBase
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public bool AutoFire { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private int AttackRange => BehaviourTree.RegimentType.Range;
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Setters ◇◇◇◇◇◇                                                                                     │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public void AutoFireOn()  => AutoFire = true;
        public void AutoFireOff() => AutoFire = false;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public RegimentIdleState(RegimentBehaviourTree behaviourTree) : base(behaviourTree, EStates.Idle)
        {
        }

        //Maybe "Stop" button like in Total war?
        public override void OnSetup(Order order) { return; }

        public override void OnEnter() { return; }

        public override void OnUpdate() { return; }

        public override void OnExit() { return; }

        public override bool ShouldExit(out EStates nextState)
        {
            nextState = StateIdentity;
            if (FireExit())
            {
                nextState = EStates.Fire;
            }
            return nextState != StateIdentity;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private bool FireExit()
        {
            bool enemyInRange = StateExtension.CheckEnemiesAtRange(LinkedRegiment, AttackRange, out int targetID, FOV_ANGLE);
            if (enemyInRange) EnemyRegimentTargetData.SetEnemyTarget(targetID);
            return enemyInRange;
        }
        
        private bool MoveExit()
        {
            return false;
        }
    }
}
