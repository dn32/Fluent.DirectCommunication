﻿using System;
using System.Diagnostics;

namespace Fluent.DirectCommunicationPusher
{
    public class TraceListenerDebug : TraceListener
    {
        public override void Write(string message)
        {
            Console.Write(message);
        }

        public override void WriteLine(string message)
        {
            Console.WriteLine(message);
        }
    }
}
