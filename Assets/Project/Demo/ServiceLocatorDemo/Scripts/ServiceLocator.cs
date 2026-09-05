using System.Collections.Generic;
using System;
using UnityEngine;

namespace CP3.Demo.ServiceLocator
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> s_Services = new Dictionary<Type, object>();

        public static void RegisterService<T>(T service) where T:class
        {
            if(service == null)
            {
                Debug.LogErrorFormat("RegisterService: service is null Type:{0}", typeof(T).Name);
                return;
            }
            
        }
    }
}
