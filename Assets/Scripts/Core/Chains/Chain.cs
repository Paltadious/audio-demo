using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Chains
{
    /// <summary>
    /// Easy manageble sequence of steps.
    /// </summary>
    /// <remarks>Chain could also be managed from inside of its steps.</remarks>
    public class Chain
    {
        public ProcessArgs ProcessArgs { get; private set; }

        public LinkedList<Step> Steps { get; }
        public Step CurrentStep => Steps.Count == 0 || !IsStarted ? null : Steps.First.Value;

        /// <summary>
        /// Is fired on every possible completion of steps like Finish(), Interrupt() or Reset().
        /// </summary>
        public event Action<ProcessArgs> OnStepsCompleted;
        /// <summary>
        /// Only for inner usege by steps to complete their stuff. Is fired before OnStepsCompleted.
        /// </summary>
        public event Action<ProcessArgs> OnBeforeStepsCompleted;

        public bool IsStarted { get; private set; }
        public bool IsFinished { get; private set; }
        public bool IsPaused { get; private set; }
        bool isNotFinalizeStep;

        public Chain(params Step[] steps)
        {
            Steps = new LinkedList<Step>(steps);
        }

        /// <summary>
        /// Launches sequance.
        /// </summary>
        public void Start(ProcessArgs processArgs = null)
        {
            Debug.Assert(!IsStarted, "An attempt to start already started chain.");
            if (IsStarted)
                return;

            ProcessArgs = processArgs ?? new ProcessArgs();

            IsStarted = true;

            if (TryFinish())
                return;

            CurrentStep.Enter(this);
        }
        
        /// <summary>
        /// Pause chain. it can be resumed later.
        /// </summary>
        public void Pause()
        {
            if (IsPaused)
                return;

            if (TryFinish())
                return;

            IsPaused = true;

            CurrentStep.Pause();
        }

        /// <summary>
        /// Resumes previously paused chain. 
        /// </summary>
        public void Resume()
        {
            if (!IsPaused)
                return;

            if (TryFinish())
                return;

            IsPaused = false;

            CurrentStep.Enter(this);
        }

        /// <summary>
        /// Immediatelly finishes the chain without completion of all of its steps.
        /// </summary>
        public void Interrupt()
        {
            if (TryFinish())
                return;

            CurrentStep.Interrupt();
        }
        
          /// <summary>
        /// Sets step in first position in execution order.
        /// If sequence was already started:
        ///     1) pauses current step;
        ///     2) adds new step;
        ///     3) enters first step.  
        /// </summary>
        public void Push(Step step)
        {
            if (IsStarted)
                CurrentStep.Pause();

            Steps.AddFirst(step);

            if (IsStarted)
                CurrentStep.Enter(this);
        }

        /// <summary>
        /// Sets steps in first position in execution order.
        /// If sequence was already started:
        ///     1) pauses current step;
        ///     2) adds new steps;
        ///     3) enters first step.  
        /// </summary>
        public void Push(params Step[] steps)
        {
            if (IsStarted)
                CurrentStep.Pause();

            for (int i = steps.Length - 1; i >= 0; i--)
            {
                Steps.AddFirst(steps[i]);
            }

            if (IsStarted)
                CurrentStep.Enter(this);
        }

        /// <summary>
        /// Finishes first step in chain and sets new one instead.
        /// </summary>
        public void Replace(Step step)
        {
            if (IsStarted)
            {
                Debug.Assert(!isNotFinalizeStep);
                isNotFinalizeStep = true;

                CurrentStep.Finish();

                isNotFinalizeStep = false;
            }

            Steps.RemoveFirst();

            Steps.AddFirst(step);

            if (IsStarted)
                CurrentStep.Enter(this);
        }

        /// <summary>
        /// Finishes first step in chain and sets new ones instead.
        /// </summary>
        public void Replace(Step[] steps)
        {
            if (IsStarted)
            {
                Debug.Assert(!isNotFinalizeStep);
                isNotFinalizeStep = true;

                CurrentStep.Finish();

                isNotFinalizeStep = false;
            }

            Steps.RemoveFirst();

            for (int i = steps.Length - 1; i >= 0; i--)
            {
                Steps.AddFirst(steps[i]);
            }

            if (IsStarted)
                CurrentStep.Enter(this);
        }

        /// <summary>
        /// Sets new step after first step.
        /// </summary>
        public void SetNext(Step step)
        {
            if (Steps.Count > 0)
                Steps.AddAfter(Steps.First, step);
            else
                Steps.AddFirst(step);
        }

        /// <summary>
        /// Sets new steps after first step.
        /// </summary>
        public void SetNext(Step[] steps)
        {
            if (Steps.Count > 0)
            {
                for (int i = steps.Length - 1; i >= 0; i--)
                {
                    Steps.AddAfter(Steps.First, steps[i]);
                }
            }
            else
            {
                for (int i = steps.Length - 1; i >= 0; i--)
                {
                    Steps.AddFirst(steps[i]);
                }
            }
        }

        // Step successfully finished itself and returns control to the chain 
        public void StepFinished(Step step)
        {
            CheckStepCompletionCorrectness(step);

            if (isNotFinalizeStep)
                return;

            Steps.RemoveFirst();

            if (TryFinish())
                return;

            if (IsStarted)
                CurrentStep.Enter(this);
        }

        // Step is interrupted and urgently finishes the chain
        public void StepInterrupted(Step step, ProcessArgs args = null)
        {
            CheckStepCompletionCorrectness(step);

            Steps.RemoveFirst();

            ProcessArgs.success = false;
            if (ProcessArgs != args && args != null)
            {
                // if there are args from any previous step, merge them with general args
                ProcessArgs = ProcessArgs.Merge(args);
            }

            if (!string.IsNullOrEmpty(ProcessArgs.Message))
            {
                Debug.LogError(ProcessArgs.Message);
            }

            Finish();
        }

        void CheckStepCompletionCorrectness(Step step)
        {
            if (step != CurrentStep)
            {
                Debug.LogError($"{step} is going to be completed, but current step is {CurrentStep}.");
            }
        }

        bool TryFinish()
        {
            if (IsFinished)
                return true;

            if (CurrentStep == null)
            {
                Finish();
                return true;
            }

            return false;
        }

        void Finish()
        {
            IsFinished = true;

#if UNITY_EDITOR
            foreach (Step step in Steps)
            {
                Debug.Assert(!step.IsEntered, $"{step} was not exited when chain is finishing.");
            }
#endif
            
            Steps.Clear();
            OnBeforeStepsCompleted?.Invoke(ProcessArgs);
            OnStepsCompleted?.Invoke(ProcessArgs);
        }
    }
}