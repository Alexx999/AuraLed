using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraLedHelper.Core
{
    public class ServiceMessage
    {
        public ServiceCommand Command { get; set; }
        public int MessageId { get; set; }
    }

    public class ServiceMessage<T> : ServiceMessage
    {
        public T Payload { get; set; }
    }

    public enum ServiceCommand
    {
        Invalid, ResponseOk, ResponseFail, ReloadSettings, ApplySettings, ClearSettings
    }
}
