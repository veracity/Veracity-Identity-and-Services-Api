using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Primitives;
using Stardust.Interstellar.Rest.Extensions;

namespace Veracity.Services.Api.Extensions
{
    public class SupportCodeHandler : StatefullHeaderHandlerBase
    {
        private  ClientSuportCodeHandler _handler;

        public SupportCodeHandler(ClientSuportCodeHandler handler)
        {
            _handler = handler;
        }

        public ClientSuportCodeHandler GetHandler()
        {
            return _handler;
        }

        protected override void DoSetHeader(IStateContainer state, HttpRequestMessage req)
        {
            try
            {
                var item = GetHandler()?.GetSupportCode();
                if (string.IsNullOrWhiteSpace(item)) return;
                req.Headers.Add("x-supportCode", item);
            }
            catch (Exception)
            {
                //ignored
            }
        }

        protected override void DoGetHeader(IStateContainer state, HttpResponseMessage response)
        {

            var code = response.Headers.Contains("x-supportCode") ? response.Headers.GetValues("x-supportCode").FirstOrDefault():null;
            if (string.IsNullOrWhiteSpace(code)) return;
            GetHandler()?.SetSupportCode(code);
        }

        protected override void DoSetServiceHeaders(IStateContainer state, HttpResponseHeaders headers)
        {

        }

        protected override void DoGetServiceHeader(IStateContainer state, IDictionary<string, StringValues> headers)
        {
        }

        protected override void DoSetServiceHeaders(IStateContainer state, IDictionary<string, StringValues> headers)
        {
        }

        protected override void DoGetServiceHeader(IStateContainer state, HttpRequestHeaders headers)
        {

        }

        protected void DoGetServiceHeaders(IStateContainer state, IDictionary<string, StringValues> headers)
        {
        }

        public override int ProcessingOrder => 2;
    }
}