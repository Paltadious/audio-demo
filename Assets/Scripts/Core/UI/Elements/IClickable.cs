using System;
using UniRx;

namespace Core.UI.Elements
{
    public interface IClickable
    {
        IObservable<Unit> OnClick();
    }
}