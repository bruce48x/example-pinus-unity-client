using System;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            if (proto != null)
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
                    continue;

                var fieldRule = protoField["rule"];
                if (fieldRule is null)
                {
                    var valueType = protoField["type"];
                    var valueId = protoField["id"];
                    if (valueType != null && valueId != null)
                    {
                        offset = writeBytes(buffer, offset, encodeTag(valueType.ToString(), Convert.ToInt32(valueId)));
                        offset = encodeProp(pair.Value, valueType.ToString(), offset, buffer, false);
                    }
                }
                else
                {
                    if (fieldRule.ToString() == "repeated")
                    {
                        var arr = (JToken)pair.Value;
                        if (arr != null)
                        {
                            var ls = arr.ToObject<List<object>>();
                            if (ls.Count > 0)
                            {
                                offset = encodeArray(ls, protoField, offset, buffer);
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
        private int encodeArray(List<object> msg, JObject protoField, int offset, byte[] buffer)
        {
            var type = protoField["type"];
            var id = protoField["id"];
            if (type != null && id != null)
            {
                foreach (object item in msg)
                {
                    int length = Encoder.byteLength(msg.ToString()) * 2;
                    int subOffset = 0;
                    byte[] subBuff = new byte[length];
                    offset = writeBytes(buffer, offset, encodeTag("repeated", Convert.ToInt32(id)));
                    subOffset = encodeProp(item, type.ToString(), subOffset, subBuff, true);
                    offset = writeBytes(buffer, offset, Encoder.encodeUInt32((uint)subOffset));
                    for (var i = 0; i < subOffset; i++)
                    {
                        buffer[offset + i] = subBuff[i];
                    }
                    offset += subOffset;
                }
            }
            return offset;
        }

        /// <summary>
        /// Encode each item in message.
        /// </summary>
        private int encodeProp(object value, string type, int offset, byte[] buffer, bool inArray)
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
                    encodeObject(type, (JObject)value, ref offset, buffer, inArray);
                    break;
            }
            return offset;
        }

        private void encodeObject(string type, JObject msg, ref int offset, byte[] buffer, bool inArray)
        {
            JObject subProto = util.GetProtoMessage(protos, type);
            if (subProto != null)
            {
                byte[] subBuff = new byte[Encoder.byteLength(msg.ToString())];
                int subOffset = 0;
                subOffset = encodeMsg(subBuff, subOffset, subProto, msg);
                if (!inArray)
                {
                    // 不在数组里，则需要记录长度
                    offset = writeBytes(buffer, offset, Encoder.encodeUInt32((uint)subOffset));
                }
                for (int i = 0; i < subOffset; i++)
                {
                    buffer[offset + i] = subBuff[i];
                }
                offset += subOffset;
            }
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
