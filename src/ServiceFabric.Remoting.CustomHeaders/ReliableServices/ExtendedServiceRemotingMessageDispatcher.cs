﻿using System;
using System.Collections.Generic;
using System.Fabric;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Runtime;

namespace ServiceFabric.Remoting.CustomHeaders.ReliableServices
{
    /// <summary>
    /// <see cref="ServiceRemotingMessageDispatcher"/> that operates on the receiving side
    /// </summary>
    public class ExtendedServiceRemotingMessageDispatcher : ServiceRemotingMessageDispatcher
    {
        /// <inheritdoc/>
        public ExtendedServiceRemotingMessageDispatcher(ServiceContext serviceContext, IService service)
            : this(serviceContext, service, null)
        {
            
        }

        /// <inheritdoc/>
        public ExtendedServiceRemotingMessageDispatcher(ServiceContext serviceContext, IService service, IServiceRemotingMessageBodyFactory serviceRemotingMessageBodyFactory = null)
            : base(serviceContext, service, serviceRemotingMessageBodyFactory)
        {
       
        }

        /// <inheritdoc/>
        public ExtendedServiceRemotingMessageDispatcher(IEnumerable<Type> remotingTypes, ServiceContext serviceContext, object serviceImplementation, IServiceRemotingMessageBodyFactory serviceRemotingMessageBodyFactory = null)
            : base(remotingTypes, serviceContext, serviceImplementation, serviceRemotingMessageBodyFactory)
        {
        }

        /// <inheritdoc/>
        public override void HandleOneWayMessage(IServiceRemotingRequestMessage requestMessage)
        {
            RemotingContext.FromRemotingMessage(requestMessage);
            base.HandleOneWayMessage(requestMessage);
        }

        /// <inheritdoc/>
        public override async Task<IServiceRemotingResponseMessage> HandleRequestResponseAsync(IServiceRemotingRequestContext requestContext,
            IServiceRemotingRequestMessage requestMessage)
        {
            var header = requestMessage.GetHeader();
            string methodName = string.Empty;
            if (header.TryGetHeaderValue(CustomHeaders.MethodHeader, out byte[] headerValue))
            {
                methodName = Encoding.ASCII.GetString(headerValue);
            }

            RemotingContext.FromRemotingMessage(requestMessage);
            object state = null;
            if (BeforeHandleRequestResponseAsync != null)
                state = await BeforeHandleRequestResponseAsync.Invoke(requestMessage, methodName);
            var responseMessage = await base.HandleRequestResponseAsync(requestContext, requestMessage);
            if (AfterHandleRequestResponseAsync != null)
                await AfterHandleRequestResponseAsync.Invoke(responseMessage, methodName, state);

            return responseMessage;
        }

        /// <summary>
        /// Optional hook to provide code executed before the message is handled by the client
        /// IServiceRemotingRequestMessage: the message
        /// string: the method name
        /// </summary>
        /// <returns>object: state</returns>
        public Func<IServiceRemotingRequestMessage, string, Task<object>> BeforeHandleRequestResponseAsync { get; set; }

        /// <summary>
        /// Optional hook to provide code executed after the message is handled by the client
        /// IServiceRemotingResponseMessage: the message
        /// string: the method name
        /// object: state
        /// </summary>
        public Func<IServiceRemotingResponseMessage, string, object, Task> AfterHandleRequestResponseAsync { get; set; }
    }
}
