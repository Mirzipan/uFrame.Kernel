using System;
using System.Collections;
using UniRx;
using UnityEngine.Networking;

namespace uFrame.Kernel
{
    public class uFrameNetworkComponent : NetworkBehaviour, IDisposableContainer
    {
        private CompositeDisposable _disposer;

        CompositeDisposable IDisposableContainer.Disposer
        {
            get { return _disposer ?? (_disposer = new CompositeDisposable()); }
            set { _disposer = value; }
        }
        
        protected IEventAggregator EventAggregator => uFrameKernel.EventAggregator;
        
        protected virtual void OnDestroy()
        {
            _disposer?.Dispose();
        }


        /// <summary>Wait for an Event to occur on the global event aggregator.</summary>
        /// <example>
        /// this.OnEvent&lt;MyEventClass&gt;().Subscribe(myEventClassInstance=&gt;{ DO_SOMETHING_HERE });
        /// </example>
        public IObservable<TEvent> Receive<TEvent>()
        {
            return EventAggregator.Receive<TEvent>();
        }

        /// <summary>Publishes a command to the event aggregator. Publish the class data you want, and let any "OnEvent" subscriptions handle them.</summary>
        /// <example>
        /// this.Publish(new MyEventClass() { Message = "Hello World" });
        /// </example>
        public void Publish<TEvent>(TEvent eventMessage)
        {
            EventAggregator.Publish(eventMessage);
        }

        protected virtual IEnumerator Start()
        {
            KernelLoading();
            while (!uFrameKernel.IsKernelLoaded) yield return null;
            KernelLoaded();
        }

        /// <summary>
        /// Before we wait for the kernel to load, even if the kernel is already loaded it will still invoke this before it attempts to wait.
        /// </summary>
        public virtual void KernelLoading()
        {

        }

        /// <summary>
        /// The first method to execute when we are sure the kernel has completed loading.
        /// </summary>
        public virtual void KernelLoaded()
        {

        }



    }
}