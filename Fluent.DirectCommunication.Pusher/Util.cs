using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluent.DirectCommunicationPusher
{
    public static class Util
    {
        public static void Message(this string msg)
        {
#if DEBUG
            Console.WriteLine(msg);
#endif
        }

        public static List<Type> GetAllIRequestControllerImplements()
        {
            var types = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(x => !x.IsDynamic)
                .SelectMany(x => x.ExportedTypes)
                .Where(x => typeof(IRequestController).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .ToList();

            return types;
        }

        public static T DynamicToObject<T>(dynamic data) where T: class, new()
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(data)) as T;
        }
    }
}
