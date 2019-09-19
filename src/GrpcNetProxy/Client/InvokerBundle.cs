using Grpc.Core;
using System;
using System.Threading;

namespace GrpcNetProxy.Client
{

    /// <summary>
    /// Invoker bundle
    /// </summary>
    internal class InvokerBundle
    {

        /// <summary>
        /// Error count
        /// </summary>
        private int _errorCount;

        /// <summary>
        /// Invoker is active 
        /// </summary>
        private int _active = 1;

        /// <summary>
        /// Invoker is aplicatively online (failed ping makes it inactive)
        /// </summary>
        private int _aplOnline = 1;

        /// <summary>
        /// Requests count
        /// </summary>
        private int _invokeCount = 0;

        /// <summary>
        /// Invoker id
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Add invoke count
        /// </summary>
        public void AddInvokeCount()
        {
            Interlocked.Increment(ref _invokeCount);
        }

        /// <summary>
        /// Reset invoke count 
        /// </summary>
        public void ResetInvokeCount()
        {
            Interlocked.Exchange(ref _invokeCount, 0);
        }

        /// <summary>
        /// Add error
        /// </summary>
        public void AddError()
        {
            Interlocked.Increment(ref _errorCount);
        }

        /// <summary>
        /// Reset error
        /// </summary>
        public void ResetError()
        {
            Interlocked.Exchange(ref _errorCount, 0);
        }

        /// <summary>
        /// Activate
        /// </summary>
        public void Activate()
        {
            Interlocked.Exchange(ref _active, 1);
        }

        /// <summary>
        /// Deactivate
        /// </summary>
        public void Deactivate()
        {
            Interlocked.Exchange(ref _active, 0);
        }

        /// <summary>
        /// Set applicatively online (ping success)
        /// </summary>
        public void SetAplOnline()
        {
            Interlocked.Exchange(ref _aplOnline, 1);
        }

        /// <summary>
        /// Set applicatively offline (ping failed)
        /// </summary>
        public void SetAplOffline()
        {
            Interlocked.Exchange(ref _aplOnline, 0);
        }

        /// <summary>
        /// error threshold under limit
        /// </summary>
        public bool ErrorsBelowThreshold => _errorCount < Host.ErrorThreshold;

        /// <summary>
        /// Error count
        /// </summary>
        public int ErrorCount => _errorCount;

        /// <summary>
        /// Is active status
        /// </summary>
        public bool IsActive => _active == 1;

        /// <summary>
        /// Is applicatively online
        /// </summary>
        public bool IsAplOnline => _aplOnline == 1;

        /// <summary>
        /// Invoke count
        /// </summary>
        public int InvokeCount => _invokeCount;

        /// <summary>
        /// Constructor with channel and invoker
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="invoker"></param>
        /// <param name="connectionData"></param>
        public InvokerBundle(Channel channel, CallInvoker invoker, GrpcChannelConnectionData connectionData)
        {
            Channel = channel;
            Invoker = invoker;
            Host = connectionData;
            Id = Guid.NewGuid().ToString("D");
        }

        /// <summary>
        /// Grpc channel
        /// </summary>
        public Channel Channel { get; private set; }

        /// <summary>
        /// Invoker
        /// </summary>
        public CallInvoker Invoker { get; private set; }

        /// <summary>
        /// Connection data
        /// </summary>
        public GrpcChannelConnectionData Host { get; private set; }

        /// <summary>
        /// Map invoker bundle to channel status
        /// </summary>
        /// <param name="ib"></param>
        /// <returns></returns>
        public GrpcChannelStatus GetChannelStatus() => new GrpcChannelStatus
        {
            ErrorCount = ErrorCount,
            ErrorsBelowThreshold = ErrorsBelowThreshold,
            Id = Id,
            InvokeCount = InvokeCount,
            IsActive = IsActive,
            IsAplOnline = IsAplOnline,
            State = Channel.State,
            Host = Host
        };

    }
}
