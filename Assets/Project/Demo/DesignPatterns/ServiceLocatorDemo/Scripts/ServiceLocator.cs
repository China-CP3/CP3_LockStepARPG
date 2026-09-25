using System.Collections.Generic;
using System;
using UnityEngine;

namespace CP3.Demo.ServiceLocator
{
    public interface IMarkService
    {
        //用于服务定位器中 各个服务需要继承的总接口
        //在注册服务时 没有继承该接口的服务不让注册 避免不相干的class or interface乱注册
    }
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> s_Services = new Dictionary<Type, object>();

        public static void RegisterService<T>(T service) where T:class, IMarkService
        {
            if(service == null)
            {
                //typeof(T).Name 每次调用会不会有开销？不会，typeof(T) 对同一个 T 全程返回同一个 Type 实例（CLR 缓存的），.Name 也是缓存字符串，零 GC。
                Debug.LogErrorFormat("RegisterService: service is null Type:{0}", typeof(T).Name);
                return;
            }

            Type type = typeof(T);

            if(!type.IsInterface)// 没有继承该接口的服务不让注册 避免不相干的class or interface乱注册 尽量做一个限制
            {
                Debug.LogErrorFormat("RegisterService: Type:{0} IsInterface:{1}", type.Name, type.IsInterface);
                return;
            }

            if (s_Services.ContainsKey(type))
            {
                Debug.LogErrorFormat("RegisterService: service has Contains:{0}", type.Name);
                return;
            }

            s_Services.Add(type, service);//两种方式 Add(key, value) 已存在会抛异常  [key] = value 已存在会静默覆盖

        }

        public static T GetService<T>() where T:class, IMarkService
        {
            Type type = typeof(T);
            if (s_Services.TryGetValue(type,out object service))
            {
                return service as T;
            }
            Debug.LogErrorFormat("GetService: not registered Type:{0}", type.Name);
            return null;
        }

    }
}
