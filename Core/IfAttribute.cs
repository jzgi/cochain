﻿using System;

namespace Greatbone.Core
{

    /// <summary>
    /// Test a condition to be true so as conticue the processing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public abstract class IfAttribute : Attribute
    {

        public abstract bool Test(WebContext wc);

    }
}