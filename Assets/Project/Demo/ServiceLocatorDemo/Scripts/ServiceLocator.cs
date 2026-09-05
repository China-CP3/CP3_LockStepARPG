using System.Collections.Generic;
using System;

namespace CP3.Demo.ServiceLocator
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> s_Services = new Dictionary<Type, object>();

    }
}
