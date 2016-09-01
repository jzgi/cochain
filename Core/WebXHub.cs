﻿using System;
using System.Net;
using System.Reflection;

namespace Greatbone.Core
{
    /// <summary>A multiplexing hub controller that is attached to a realm controller. </summary>
    ///
    public abstract class WebXHub : WebSub
    {
        // the added sub controllers
        private Set<WebSub> subs;

        protected WebXHub(WebServiceContext wsc) : base(wsc)
        {
        }

        public TSub AddSub<TSub>(string key, bool auth) where TSub : WebSub
        {
            Service.Context.Enter(this);

            if (subs == null)
            {
                subs = new Set<WebSub>(16);
            }
            // create instance by reflection
            Type type = typeof(TSub);
            ConstructorInfo ci = type.GetConstructor(new[] {typeof(WebServiceContext)});
            if (ci == null)
            {
                throw new WebException(type + ": the constructor with WebServiceContext not defined");
            }
            TSub sub = (TSub) ci.Invoke(new object[] {Service.Context});
            // call the initialization and add
            subs.Add(sub);

            Service.Context.Exit();
            return sub;
        }

        public override void Handle(string relative, WebContext wc)
        {
            int slash = relative.IndexOf('/');
            if (slash == -1) // without a slash then handle it locally
            {
                WebAction a = GetAction(relative);
//				TX x = wc.X as TX;
//				a?.Do(wc, wc.X);
            }
            else // not local then sub
            {
                string rsc = relative.Substring(0, slash);
                WebSub sub;
                if (subs.TryGet(rsc, out sub))
                {
                    sub.Handle(rsc.Substring(slash), wc);
                }
            }
        }
    }
}