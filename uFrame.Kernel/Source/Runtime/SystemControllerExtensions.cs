using System;
using UniRx;

namespace uFrame.Kernel
{
    public static class SystemControllerExtensions
    {

        /// <summary>
        /// A wrapper for GetEvent on the EventAggregator GetEvent method.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <returns>An observable capable of subscriptions and filtering.</returns>
        public static IObservable<TEvent> OnReceive<TEvent>(this ISystemService systemController)
        {
            return systemController.EventAggregator.Receive<TEvent>();
        }

        /// <summary>
        /// A wrapper for the Event Aggregator.Publish method.
        /// </summary>
        /// <param name="systemController"></param>
        /// <param name="eventMessage"></param>
        public static void Publish(this ISystemService systemController, object eventMessage)
        {
            systemController.EventAggregator.Publish(eventMessage);
        }
    }
}