using UnityEngine;

namespace Core.Chains
{
    public abstract class Step
    {
        public bool IsEntered;

        protected Chain chain;

        bool IsStarted;
        bool IsCompleted;
        bool IsPaused;

        public void Enter(Chain chain)
        {
            if (!IsStarted) //first enter
            {
                Debug.Assert(!IsPaused, $"{this} not {nameof(IsStarted)} but {nameof(IsPaused)}");
                this.chain = chain;
                IsStarted = true;

                OnStart();
                Debug.Assert(!IsCompleted,
                    "It's not allowed to complete step in OnStart(). Please make it in OnEnter().");

                BaseOnEnter();
                return;
            }

            if (IsPaused) // re-enter after pause
            {
                IsPaused = false;

                OnResume();
                BaseOnEnter();
                return;
            }

            Debug.LogError($"{this}: an attempt to re-enter active (not paused) step.");
        }

        /// <summary>
        /// Sets step in appropriate condition, so it can be resumed in the future 
        /// </summary>
        public void Pause()
        {
            Exit();
            OnPause();
            IsPaused = true;
        }

        /// <summary>
        /// Completes successfully this step. Then chain will destroy this step and enter the following one. 
        /// To be called from implementation code of the step.
        /// </summary>
        public void Finish()
        {
            if (chain.IsFinished)
                return;

            Debug.Assert(!IsCompleted);
            if (IsCompleted) return;

            Exit();
            OnFinish();

            IsCompleted = true;
            chain.StepFinished(this);
        }

        /// <summary>
        /// Completes this step with interruption. Then chain will interrupt further work and finish itself. 
        /// To be called from implementation code.
        /// </summary>
        public void Interrupt(string message = null)
        {
            Debug.Assert(!IsCompleted);
            if (IsCompleted) return;

            Exit();
            OnInterrupt();

            IsCompleted = true;
            chain.StepInterrupted(this, ProcessArgs.Failed(message));
        }

        void BaseOnEnter()
        {
            Debug.Assert(!IsEntered);
            if (IsEntered) return;
            IsEntered = true;
            OnEnter();
        }

        void Exit()
        {
            Debug.Assert(IsEntered);
            if (!IsEntered) return;
            IsEntered = false;
            OnExit();
        }

        /// <summary> Is called on every enter </summary>
        protected virtual void OnEnter()
        {
        }

        /// <summary> Is called on every exit (Pause, Finish or Interrupt) </summary>
        protected virtual void OnExit()
        {
        }

        /// <summary> Is called on first enter. Do Not call Push/Replace here, call it in OnEnter() instead. </summary>
        protected virtual void OnStart()
        {
        }

        /// <summary> Is called on re-enter after pause </summary>
        protected virtual void OnResume()
        {
        }

        /// <summary> Is called on every pause </summary>
        protected virtual void OnPause()
        {
        }

        /// <summary> Is called on Success </summary>
        protected virtual void OnFinish()
        {
        }

        /// <summary> Is called on Fail </summary>
        protected virtual void OnInterrupt()
        {
        }

        public override string ToString() => GetType().Name;
    }
}