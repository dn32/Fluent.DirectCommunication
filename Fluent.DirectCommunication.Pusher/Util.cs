using System;

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
    }
}
