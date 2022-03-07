using System;

namespace Core.Chains
{
    public class ActionStep : Step
    {
        readonly Action actions;

        public ActionStep(Action actions)
        {
            this.actions = actions;
        }

        protected override void OnEnter()
        {
            actions.Invoke();
            Finish();
        }
    }
}