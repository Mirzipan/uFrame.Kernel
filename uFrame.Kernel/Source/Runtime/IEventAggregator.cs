using UniRx;
using System; // Required for WP8 and Store APPS

namespace uFrame.Kernel
{
    public interface IEventAggregator
    {
        IObservable<TEvent> Receive<TEvent>();
        void Publish<TEvent>(TEvent evt);
        bool DebugEnabled { get; set; }
    }
}