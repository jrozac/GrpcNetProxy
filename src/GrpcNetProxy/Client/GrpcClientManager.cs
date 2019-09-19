using Grpc.Core;
using GrpcNetProxy.Shared;
using GrpcNetProxy.Status;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcNetProxy.Client
{

    /// <summary>
    /// Client manager
    /// </summary>
    public class GrpcClientManager
    {

        /// <summary>
        /// Status service
        /// </summary>
        private readonly IStatusService _statusService;

        /// <summary>
        /// Call invoker
        /// </summary>
        private readonly ManagedCallInvoker _callInvoker;

        /// <summary>
        /// Client configuration
        /// </summary>
        private readonly GrpcClientConfiguration _clientConfiguration;

        /// <summary>
        /// Monitor timer
        /// </summary>
        private Timer _monitorTimer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clientConfiguration"></param>
        /// <param name="serviceProvider"></param>
        internal GrpcClientManager(GrpcClientConfiguration clientConfiguration, IServiceProvider serviceProvider)
        {
            // save cofniguration
            _clientConfiguration = clientConfiguration;

            // create call invoker
            var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger("GrpcClientRequests");
            _callInvoker = new ManagedCallInvoker(logger, clientConfiguration,
                _clientConfiguration.Hosts, StatusServiceChannelCustomSelector);

            // create status service client if required
            if (_clientConfiguration.Options.StatusServiceEnabled)
            {
                _statusService = CreateStatusService(clientConfiguration, _callInvoker);
            }

            // reset channels
            ResetChannels();

            // run monitor
            RunMonitor();
        }

        /// <summary>
        /// Invoker getter
        /// </summary>
        /// <returns></returns>
        internal CallInvoker GetInvoker() => _callInvoker;

        /// <summary>
        /// Get name
        /// </summary>
        public string Name => _clientConfiguration?.Name;

        /// <summary>
        /// Gets channels ids for client
        /// </summary>
        /// <param name="clientName"></param>
        /// <returns></returns>
        public string[] GetChannelsIdsForClient()
        {
            return _callInvoker.Invokers.Select(s => s.Id).ToArray();
        }

        /// <summary>
        /// Get channel remote status
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        public GrpcChannelStatus GetChannelStatus(string channelId)
        {
            var status = GetChannelInvoker(channelId).GetChannelStatus();
            return status;
        }

        /// <summary>
        /// Get channels remote statsu
        /// </summary>
        /// <returns></returns>
        public List<GrpcChannelStatus> GetChannelsStatus()
        {
            return _callInvoker.Invokers.Select(c => c.GetChannelStatus()).ToList();
        }

        /// <summary>
        /// Reset channel
        /// </summary>
        /// <param name="channelId"></param>
        public void ResetChannel(string channelId)
        {
            var ch = GetChannelInvoker(channelId);

            // set aplication online depending of configuration
            if (_clientConfiguration.Options.StatusServiceEnabled)
            {
                ch.SetAplOffline();
            }
            else
            {
                ch.SetAplOnline();
            }

            // reset error and activate channel
            ch.ResetError();
            ch.ResetInvokeCount();
            ch.Activate();
        }

        /// <summary>
        /// Activate channel
        /// </summary>
        /// <param name="channelId"></param>
        public void ActivateChannel(string channelId)
        {
            GetChannelInvoker(channelId).Activate();
        }

        /// <summary>
        /// Deactivate channel
        /// </summary>
        /// <param name="channelId"></param>
        public void DeactivateChannel(string channelId)
        {
            GetChannelInvoker(channelId).Deactivate();
        }

        /// <summary>
        /// Get raw channel
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        public Channel GetRawChannel(string channelId)
        {
            return GetChannelInvoker(channelId).Channel;
        }

        /// <summary>
        /// Get client channel remote status
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        public async Task<bool?> GetChannelRemoteStatusValue(string channelId)
        {
            // status service not supported
            if(_statusService == null)
            {
                GetChannelInvoker(channelId).SetAplOnline();
                return await Task.FromResult<bool?>(null);
            }

            // make remote call
            bool remoteStatus = false;
            using (var store = new CancellationTokenSource())
            {
                try
                {
                    store.CancelAfter(5000);
                    var status = await _statusService.CheckStatus(new CheckStatusRequest { ChannelId = channelId }, store.Token);
                    remoteStatus = status?.Status ?? false;
                }
                catch (Exception)
                {
                }
            }

            // set online/offline status
            if(remoteStatus)
            {
                GetChannelInvoker(channelId).SetAplOnline();
            } else
            {
                GetChannelInvoker(channelId).SetAplOffline();
            }

            // return 
            return remoteStatus;
        }

        /// <summary>
        /// Reset all channels 
        /// </summary>
        public void ResetChannels()
        {
            var tasks = GetChannelsIdsForClient().Select(async chId => {
                ResetChannel(chId);
                await GetChannelRemoteStatusValue(chId);
            });

            // wait to complete
            Task.WaitAll(tasks.ToArray());
        }

        /// <summary>
        /// Get channel invoker
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal InvokerBundle GetChannelInvoker(string id)
        {
            var ch = _callInvoker.Invokers.FirstOrDefault(b => b.Id == id);
            if (ch == null)
            {
                throw new ArgumentException("Channel id is invalid");
            }
            return ch;
        }

        /// <summary>
        /// Create status service
        /// </summary>
        /// <param name="clientConfiguration"></param>
        /// <param name="invoker"></param>
        /// <returns></returns>
        private IStatusService CreateStatusService(GrpcClientConfiguration clientConfiguration, CallInvoker invoker)
        {
            // get build method
            var methodBuild = typeof(GrpcClientFactoryUtil).
                GetMethod(nameof(GrpcClientFactoryUtil.Create), BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeof(IStatusService));

            // create status service with custom invoker
            var statusService = (IStatusService) methodBuild.Invoke(null, new object[] { invoker, clientConfiguration });

            // return 
            return statusService;

        }

        /// <summary>
        /// Run client monitor
        /// </summary>
        private void RunMonitor()
        {
            // locker (to avoid multiple schedules)
            var locker = new object();

            // init timer
            _monitorTimer = new Timer((e) =>
            {
                // no lock by default
                var hasLock = false;

                try
                {
                    // try to lock and quit if already locked
                    Monitor.TryEnter(locker, ref hasLock);
                    if (!hasLock)
                    {
                        return;
                    }

                    // run action
                    MonitorAction();

                }
                finally
                {
                    if (hasLock)
                    {
                        Monitor.Exit(locker);
                    }
                }

            }, null, TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(_clientConfiguration.Options.MonitorIntervalMs));

        }

        /// <summary>
        /// Monitor action
        /// </summary>
        private void MonitorAction()
        {
            // process channels in separate tasks
            var tasks = _callInvoker.Invokers.Select(async ch =>
            {

                // do nothing if channel not active (manually disabled)
                if (!ch.IsActive)
                {
                    return;
                }

                // check online status and reset error if ok
                var aplOnline = await GetChannelRemoteStatusValue(ch.Id);
                if (aplOnline == true)
                {
                    ch.ResetError();
                }

            }).ToArray();

            // wait all tasks to complete
            Task.WaitAll(tasks);
        }

        /// <summary>
        /// Custom channel selector used for status service
        /// </summary>
        /// <param name="request"></param>
        /// <param name="serviceName"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        private string StatusServiceChannelCustomSelector(object request, string serviceName, string methodName)
        {
            if(_clientConfiguration.Options.StatusServiceEnabled && request is CheckStatusRequest)
            {
                var req = (CheckStatusRequest)request;
                return !string.IsNullOrWhiteSpace(req.ChannelId) ? req.ChannelId : null;
            }
            return null;
        }

    }
}
