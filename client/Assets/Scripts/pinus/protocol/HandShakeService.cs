using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Sockets;

namespace Pomelo.DotNetClient
{
    public class HandShakeService
    {
        private Protocol protocol;
        private Action<JObject> callback;

        public const string Version = "0.3.0";
        public const string Type = "unity-socket";


        public HandShakeService(Protocol protocol)
        {
            this.protocol = protocol;
        }

        public void request(object user, Action<JObject> callback)
        {
            byte[] body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(buildMsg(user)));

            protocol.send(PackageType.PKG_HANDSHAKE, body);

            this.callback = callback;
        }

        internal void invokeCallback(JObject data)
        {
            //Invoke the handshake callback
            if (callback != null) callback.Invoke(data);
        }

        public void ack()
        {
            protocol.send(PackageType.PKG_HANDSHAKE_ACK, new byte[0]);
        }

        private object buildMsg(object user)
        {
            if (user == null) user = new Object();

            //Build sys option
            var sys = new { version = Version, type = Type };
            //Build handshake message
            var msg = new { sys = sys, user = user };

            return msg;
        }
    }
}