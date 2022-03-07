using System;

namespace Core.Chains
{
    public class ActionStep : InstantStep
    {
        readonly Action action;

        public ActionStep(Action action)
        {
            this.action = action;
        }
        
        public override void Enter(Chain chain)
        {
            action.Invoke();
            chain.StepFinished(this);
        }
    }
}