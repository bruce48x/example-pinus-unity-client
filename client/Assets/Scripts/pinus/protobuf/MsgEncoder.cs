using System;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Pomelo.Protobuf
{
    public class MsgEncoder
    {
        private JObject protos { set; get; }//The message format(like .proto file)
        private Encoder encoder { set; get; }
        private Util util { set; get; }

        public MsgEncoder(JObject protos)
        {
            if (protos == null)
                this.protos = new JObject();
            else
                this.protos = protos;
            util = new Util();
        }

        /// <summary>
        /// Encode the message from server.
        /// </summary>
        /// <param name='route'>
        /// Route.
        /// </param>
        /// <param name='msg'>
        /// Message.
        /// </param>
        public byte[] encode(string route, JObject msg)
        {
            byte[] returnByte = null;
            JObject proto = util.GetProtoMessage(protos, route);
            if (!(proto is null))
            {
                int length = Encoder.byteLength(msg.ToString()) * 2;
                int offset = 0;
                byte[] buff = new byte[length];
                offset = encodeMsg(buff, offset, proto, msg);
                returnByte = new byte[offset];
                for (int i = 0; i < offset; i++)
                {
                    returnByte[i] = buff[i];
                }
            }
            return returnByte;
        }

        /// <summary>
        /// Encode the message.
        /// </summary>
        private int encodeMsg(byte[] buffer, int offset, JObject proto, JObject msg)
        {
            var msgDict = msg.ToObject<Dictionary<string, object>>();
            foreach (KeyValuePair<string, object> pair in msgDict)
            {
                var key = pair.Key;
                JObject protoField = util.GetField(proto, key);
                if (protoField is null)
                {
                    continue;
                }
                var fieldRule = protoField["rule"];
                if (fieldRule is null)
                {
                    var valueType = protoField["type"];
                    var valueId = protoField["id"];
                    if (!(valueType is null) && !(valueId is null))
                    {
                        offset = writeBytes(buffer, offset, encodeTag(valueType.ToString(), Convert.ToInt32(valueId)));
                        offset = encodeProp(msg[key], valueType.ToString(), offset, buffer, proto);
                    }
                }
                else
                {
                    if (fieldRule.ToString() == "repeated")
                    {
                        var arr = msg[key];
                        if (!(arr is null))
                        {
                            var ls = arr.ToObject<List<object>>();
                            if (ls.Count > 0)
                            {
                                offset = encodeArray(ls, protoField, offset, buffer, proto);
                            }
                        }
                    }
                }
            }
            return offset;
        }

        /// <summary>
        /// Encode the array type.
        /// </summary>
        private int encodeArray(List<object> msg, JObject value, int offset, byte[] buffer, JObject proto)
        {
            var valueType = value["type"];
            var valueId = value["id"];
            if (!(valueType is null) && !(valueId is null))
            {
                if (util.isSimpleType(valueType.ToString()))
                {
                    offset = writeBytes(buffer, offset, encodeTag(valueType.ToString(), Convert.ToInt32(valueId)));
                    offset = writeBytes(buffer, offset, Encoder.encodeUInt32((uint)msg.Count));
                    foreach (object item in msg)
                    {
                        offset = encodeProp(item, valueType.ToString(), offset, buffer, null);
                    }
                }
                else
                {
                    foreach (object item in msg)
                    {
                        offset = writeBytes(buffer, offset, encodeTag(valueType.ToString(), Convert.ToInt32(valueId)));
                        offset = encodeProp(item, valueType.ToString(), offset, buffer, proto);
                    }
                }
            }
            return offset;
        }

        /// <summary>
        /// Encode each item in message.
        /// </summary>
        private int encodeProp(object value, string type, int offset, byte[] buffer, JObject proto)
        {
            switch (type)
            {
                case "uint32":
                    writeUInt32(buffer, ref offset, value);
                    break;
                case "int32":
                case "sint32":
                    writeInt32(buffer, ref offset, value);
                    break;
                case "uint64":
                    writeUInt64(buffer, ref offset, value);
                    break;
                case "int64":
                case "sint64":
                    writeInt64(buffer, ref offset, value);
                    break;
                case "float":
                    writeFloat(buffer, ref offset, value);
                    break;
                case "double":
                    writeDouble(buffer, ref offset, value);
                    break;
                case "string":
                    writeString(buffer, ref offset, value);
                    break;
                case "bool":
                    writeBool(buffer, ref offset, value);
                    break;
                default:
                    JObject message = util.GetProtoMessage(protos, type);
                    if (!(message is null))
                    {
                        byte[] tembuff = new byte[Encoder.byteLength(value.ToString()) * 3];
                        int length = 0;
                        length = encodeMsg(tembuff, length, message, (JObject)value);
                        offset = writeBytes(buffer, offset, Encoder.encodeUInt32((uint)length));
                        for (int i = 0; i < length; i++)
                        {
                            buffer[offset] = tembuff[i];
                            offset++;
                        }
                    }
                    break;
            }
            return offset;
        }

        //Encode string.
        private void writeString(byte[] buffer, ref int offset, object value)
        {
            int le = Encoding.UTF8.GetByteCount(value.ToString());
            offset = writeBytes(buffer, offset, Encoder.encodeUInt32((uint)le));
            byte[] bytes = Encoding.UTF8.GetBytes(value.ToString());
            writeBytes(buffer, offset, bytes);
            offset += le;
        }

        //Encode double.
        private void writeDouble(byte[] buffer, ref int offset, object value)
        {
            WriteRawLittleEndian64(buffer, offset, (ulong)BitConverter.DoubleToInt64Bits(double.Parse(value.ToString())));
            offset += 8;
        }

        //Encode float.
        private void writeFloat(byte[] buffer, ref int offset, object value)
        {
            writeBytes(buffer, offset, Encoder.encodeFloat(float.Parse(value.ToString())));
            offset += 4;
        }

        private void writeBool(byte[] buffer, ref int offset, object value)
        {
            offset = writeBytes(buffer, offset, Encoder.encodeBool(value));
        }

        ////Encode UInt32.
        private void writeUInt32(byte[] buffer, ref int offset, object value)
        {
            offset = writeBytes(buffer, offset, Encoder.encodeUInt32(value.ToString()));
        }

        //Encode Int32
        private void writeInt32(byte[] buffer, ref int offset, object value)
        {
            offset = writeBytes(buffer, offset, Encoder.encodeSInt32(value.ToString()));
        }

        private void writeUInt64(byte[] buffer, ref int offset, object value)
        {
            offset = writeBytes(buffer, offset, Encoder.encodeUInt64(value.ToString()));
        }

        private void writeInt64(byte[] buffer, ref int offset, object value)
        {
            offset = writeBytes(buffer, offset, Encoder.encodeSInt64(value.ToString()));
        }

        //Write bytes to buffer.
        private int writeBytes(byte[] buffer, int offset, byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                buffer[offset] = bytes[i];
                offset++;
            }
            return offset;
        }

        //Encode tag.
        private byte[] encodeTag(string type, int tag)
        {
            int flag = util.containType(type);
            return Encoder.encodeUInt32((uint)(tag << 3 | flag));
        }


        private void WriteRawLittleEndian64(byte[] buffer, int offset, ulong value)
        {
            buffer[offset++] = ((byte)value);
            buffer[offset++] = ((byte)(value >> 8));
            buffer[offset++] = ((byte)(value >> 16));
            buffer[offset++] = ((byte)(value >> 24));
            buffer[offset++] = ((byte)(value >> 32));
            buffer[offset++] = ((byte)(value >> 40));
            buffer[offset++] = ((byte)(value >> 48));
            buffer[offset++] = ((byte)(value >> 56));
        }
    }
}