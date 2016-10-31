using System;
using System.Text;
using Newtonsoft.Json.Linq;

namespace AuraLedHelper.Core
{
    public class JsonMessageProcessor : IMessageProcessor
    {
        private readonly Func<ServiceCommand, Type> _typeFunc;
        private readonly Action<ServiceMessage> _callback;

        public JsonMessageProcessor(Func<ServiceCommand, Type> typeFunc, Action<ServiceMessage> callback)
        {
            _typeFunc = typeFunc;
            _callback = callback;
        }

        public bool ProcessMessage(byte[] msg)
        {
            var str = Encoding.UTF8.GetString(msg, 0, msg.Length);
            var obj = JObject.Parse(str);

            JToken cmd;
            
            if (!obj.TryGetValue("Command", StringComparison.InvariantCultureIgnoreCase, out cmd))
            {
                return false;
            }

            Type type;

            try
            {
                var cmdValue = cmd.ToObject<ServiceCommand>();
                type = _typeFunc(cmdValue);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex);
                return false;
            }

            var value = (ServiceMessage) obj.ToObject(type);

            try
            {
                _callback(value);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex);
                return false;
            }

            return true;
        }
    }

    public interface IMessageProcessor
    {
        bool ProcessMessage(byte[] msg);
    }
}
