using System;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Pomelo.Protobuf
{
    public class MsgDecoder
    {
        private JObject protos { set; get; }//The message format(like .proto file)
        private int offset { set; get; }
        private byte[] buffer { set; get; }//The binary message from server.
        private Util util { set; get; }

        public MsgDecoder(JObject protos)
        {
            if (protos == null)
                this.protos = new JObject();
            else
                this.protos = protos;
            util = new Util();
        }

        public JObject decode(string route, byte[] buf)
        {
            buffer = buf;
            offset = 0;
            JObject proto = util.GetProtoMessage(protos, route);
            if (!(proto is null))
            {
                JObject msg = new JObject();
                return decodeMsg(msg, proto, buffer.Length);
            }
            return null;
        }

        private JObject decodeMsg(JObject msg, JObject proto, int length)
        {
            while (offset < length)
            {
                Dictionary<string, int> head = getHead();
                int id;
                if (!head.TryGetValue("id", out id))
                    continue;


                var fields = proto["fields"];
                if (fields is null)
                    continue;

                var fieldsDict = fields.ToObject<Dictionary<string, JObject>>();

                foreach (KeyValuePair<string, JObject> pair in fieldsDict)
                {
                    var name = pair.Key;
                    var field = pair.Value;

                    var fieldId = field["id"];
                    if (fieldId is null)
                        continue;

                    if (Convert.ToInt32(fieldId) == id)
                    {
                        var type = field["type"];
                        if (type is null)
                            continue;

                        var rule = field["rule"];
                        if (rule is null)
                        {
                            msg.Add(name, JToken.FromObject(decodeProp(type.ToString(), proto)));
                        }
                        else
                        {
                            if (rule.ToString() == "repeated")
                            {
                                var msgVal = msg[name] as JArray;
                                if (msgVal is null)
                                {
                                    msgVal = new JArray();
                                    msg.Add(name, msgVal);
                                }
                                decodeArray(msgVal, type.ToString(), proto);
                            }
                        }
                    }
                }
            }
            return msg;
        }

        private void decodeArray(JArray list, string type, JObject proto)
        {
            uint length = Decoder.decodeUInt32(getBytes());
            int curOffset = offset;
            if (util.isSimpleType(type))
            {
                while (offset < curOffset + length)
                {
                    list.Add(decodeProp(type, null));
                }
            }
            else
            {
                while (offset < curOffset + length)
                {
                    list.Add(decodeProp(type, proto));
                }
            }
        }

        private object decodeProp(string type, JObject proto)
        {
            switch (type)
            {
                case "uint32":
                    return Decoder.decodeUInt32(getBytes());
                case "int32":
                case "sint32":
                    return Decoder.decodeSInt32(getBytes());
                case "uint64":
                    return Decoder.decodeUInt64(getBytes());
                case "int64":
                case "sint64":
                    return Decoder.decodeSInt64(getBytes());
                case "float":
                    return decodeFloat();
                case "double":
                    return decodeDouble();
                case "string":
                    return decodeString();
                case "bool":
                    return decodeBool();
                default:
                    return decodeObject(type, protos);
            }
        }

        //Decode the user-defined object type in message.
        private JObject decodeObject(string type, JObject proto)
        {
            if (proto != null)
            {
                JObject subProto = util.GetProtoMessage(proto, type);
                int l = (int)Decoder.decodeUInt32(getBytes());
                JObject msg = new JObject();
                return decodeMsg(msg, subProto, offset + l);
            }
            return new JObject();
        }

        //Decode string type.
        private string decodeString()
        {
            int length = (int)Decoder.decodeUInt32(getBytes());
            string msg_string = Encoding.UTF8.GetString(buffer, offset, length);
            offset += length;
            return msg_string;
        }

        //Decode double type.
        private double decodeDouble()
        {
            double msg_double = BitConverter.Int64BitsToDouble((long)ReadRawLittleEndian64());
            offset += 8;
            return msg_double;
        }

        //Decode float type
        private float decodeFloat()
        {
            float msg_float = BitConverter.ToSingle(buffer, offset);
            offset += 4;
            return msg_float;
        }

        private bool decodeBool()
        {
            bool res = Convert.ToBoolean(buffer[offset]);
            offset++;
            return res;
        }

        //Read long in littleEndian
        private ulong ReadRawLittleEndian64()
        {
            ulong b1 = buffer[offset];
            ulong b2 = buffer[offset + 1];
            ulong b3 = buffer[offset + 2];
            ulong b4 = buffer[offset + 3];
            ulong b5 = buffer[offset + 4];
            ulong b6 = buffer[offset + 5];
            ulong b7 = buffer[offset + 6];
            ulong b8 = buffer[offset + 7];
            return b1 | (b2 << 8) | (b3 << 16) | (b4 << 24)
                  | (b5 << 32) | (b6 << 40) | (b7 << 48) | (b8 << 56);
        }

        //Get the type and tag.
        private Dictionary<string, int> getHead()
        {
            int tag = (int)Decoder.decodeUInt32(getBytes());
            Dictionary<string, int> head = new Dictionary<string, int>();
            head.Add("type", tag & 0x7);
            head.Add("id", tag >> 3);
            return head;
        }

        //Get bytes.
        private byte[] getBytes()
        {
            List<byte> arrayList = new List<byte>();
            int pos = offset;
            byte b;
            do
            {
                b = buffer[pos];
                arrayList.Add(b);
                pos++;
            } while (b >= 128);
            offset = pos;
            int length = arrayList.Count;
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; i++)
            {
                bytes[i] = arrayList[i];
            }
            return bytes;
        }
    }
}