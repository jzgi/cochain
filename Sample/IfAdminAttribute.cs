﻿using System;
using Greatbone.Core;

namespace Greatbone.Sample
{
    public class IfAdminAttribute : IfAttribute
    {
        public override bool Check(WebContext wc)
        {
            throw new NotImplementedException();
        }

        public override bool Check(WebContext wc, string var)
        {
            return false;
        }
    }
}