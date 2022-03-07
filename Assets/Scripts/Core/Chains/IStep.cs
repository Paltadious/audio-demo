namespace Core.Chains
{
    public interface IStep
    {
        void Enter(Chain chain);
    }

    public interface IProcessStep : IStep
    {
        void Pause();
        void Finish();
        void Interrupt(string message = null);
    }
}