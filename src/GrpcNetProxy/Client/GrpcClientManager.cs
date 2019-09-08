using Grpc.Core;
using GrpcNetProxy.Shared;
using Microsoft.Extensions.DependencyInjection;
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
        /// Channel manager
        /// </summary>
        private readonly GrpcChannelManager _channelManager;

        /// <summary>
        /// Status service
        /// </summary>
        private readonly IStatusService _statusService;

        /// <summary>
        /// Monitor timer
        /// </summary>
        private Timer _monitorTimer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channelManager"></param>
        internal GrpcClientManager(GrpcClientConfiguration clientConfiguration, IServiceProvider serviceProvider)
        {
            // resolve channel manager
            _channelManager = serviceProvider.GetServices<GrpcChannelManager>().First(m => m.Name == clientConfiguration.Name);

            // create status service client if required
            if (_channelManager.Configuration.StatusServiceEnabled)
            {
                _statusService = CreateStatusService(clientConfiguration, serviceProvider);
            }

            // reset channels
            ResetChannels();

            // run monitor
            RunMonitor();
        }

        /// <summary>
        /// Get name
        /// </summary>
        public string Name => _channelManager.Name;

        /// <summary>
        /// Gets channels ids for client
        /// </summary>
        /// <param name="clientName"></param>
        /// <returns></returns>
        public string[] GetChannelsIdsForClient()
        {
            return _channelManager.GetChannelsIds();
        }

        /// <summary>
        /// Get channel remote status
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        public GrpcChannelStatus GetChannelStatus(string channelId)
        {
            var status = _channelManager.GetChannel(channelId).GetChannelStatus();
            return status;
        }

        /// <summary>
        /// Get channels remote statsu
        /// </summary>
        /// <returns></returns>
        public List<GrpcChannelStatus> GetChannelsStatus()
        {
            return _channelManager.GetChannels().Select(c => c.GetChannelStatus()).ToList();
        }

        /// <summary>
        /// Reset channel
        /// </summary>
        /// <param name="channelId"></param>
        public void ResetChannel(string channelId)
        {
            _channelManager.ResetChannel(channelId);
        }

        /// <summary>
        /// Activate channel
        /// </summary>
        /// <param name="channelId"></param>
        public void ActivateChannel(string channelId)
        {
            _channelManager.ActivateChannel(channelId);
        }

        /// <summary>
        /// Deactivate channel
        /// </summary>
        /// <param name="channelId"></param>
        public void DeactivateChannel(string channelId)
        {
            _channelManager.DeactivateChannel(channelId);
        }

        /// <summary>
        /// Get raw channel
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        public Channel GetRawChannel(string channelId)
        {
            return _channelManager.GetChannel(channelId)?.Channel;
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
                _channelManager.GetChannel(channelId).SetAplOnline();
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
                _channelManager.GetChannel(channelId).SetAplOnline();
            } else
            {
                _channelManager.GetChannel(channelId).SetAplOffline();
            }

            // return 
            return remoteStatus;
        }

        /// <summary>
        /// Reset all channels 
        /// </summary>
        public void ResetChannels()
        {
            var tasks = _channelManager.GetChannelsIds().Select(async chId => {
                _channelManager.ResetChannel(chId);
                 await GetChannelRemoteStatusValue(chId);
            });

            // wait to complete
            Task.WaitAll(tasks.ToArray());
        }

        /// <summary>
        /// Get status service
        /// </summary>
        /// <param name="clientName"></param>
        /// <returns></returns>
        private IStatusService CreateStatusService(GrpcClientConfiguration clientConfiguration, IServiceProvider provider)
        {
            // get build method
            var methodBuild = typeof(GrpcClientBuilder).
                GetMethod(nameof(GrpcClientBuilder.Build), BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeof(IStatusService));

            // create status service with custom invoker
            var statusService = (IStatusService) methodBuild.Invoke(null, new object[] { provider, clientConfiguration });
            ((GrpcClientBase)statusService).GetCustomInvokerForRequestDelegate = (manager, request, serviceName, methodName) =>
            {
                var channelId = ((CheckStatusRequest)request)?.ChannelId;
                return channelId != null ? manager.GetChannel(channelId) : null;
            };

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

            }, null, TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(_channelManager.Configuration.MonitorInterval));

        }

        /// <summary>
        /// Monitor action
        /// </summary>
        private void MonitorAction()
        {
            // process channels in separate tasks
            var tasks = _channelManager.GetChannels().Select(async ch =>
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

    }
}
