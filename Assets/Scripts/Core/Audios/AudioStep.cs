using Core.Audios;
using Core.Chains;

namespace Project.Audios
{
    public class AudioStep : InstantStep
    {
        SoundType soundType;

        public AudioStep(SoundType soundType)
        {
            this.soundType = soundType;
        }
        
        public override void Enter(Chain chain)
        {
            Audio.Instance.PlaySound(soundType);
            chain.StepFinished(this);
        }
    }
}