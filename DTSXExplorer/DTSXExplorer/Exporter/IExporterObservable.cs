using System;

namespace DTSXExplorer
{
    internal interface IExporterObservable : IDisposable
    {
        /// <summary>
        /// Subscribe to notifications about exported DTSX files.
        /// </summary>
        /// <param name="observer">The observer of notifications.</param>
        void Subscribe(IExporterObserver observer);

        /// <summary>
        /// Unsubscribe to notification about exported DTSX files.
        /// </summary>
        /// <param name="observer">The observer of notifications.</param>
        void UnSubscribe(IExporterObserver observer);
    }
}
