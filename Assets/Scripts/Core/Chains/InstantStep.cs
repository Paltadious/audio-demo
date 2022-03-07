namespace Core.Chains
{
    public abstract class InstantStep : IStep
    {
        public abstract void Enter(Chain chain);
    }
}