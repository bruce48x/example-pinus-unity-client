using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pomelo.DotNetClient;
using System;
using Newtonsoft.Json.Linq;

public class main : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        string host = "127.0.0.1";//(www.xxx.com/127.0.0.1/::1/localhost etc.)
        int port = 3010;
        PomeloClient pclient = new PomeloClient();

        //listen on network state changed event
        pclient.NetWorkStateChangedEvent += (state) =>
        {
            Debug.Log("state changed "+state);
        };

        pclient.initClient(host, port, () =>
        {
            //The user data is the handshake user params
            JObject user = JObject.Parse("{}");
            pclient.connect(user, data =>
            {
                Debug.Log("发送消息");
                var msg = JObject.Parse(@"{
                    'name': 'bruce',
                    'custom': { 'field1': 7, 'field2': 0.9, 'field3': 9.0, 'field4': false, 'arr': [0,1,0,1] },
                    'customArr': [
                        { 'field1': 123, 'field2': 1.1, 'field3': 2.1, 'arr': [1,2,3], 'field4': true },
                        { 'field1': 321, 'field2': 1.2, 'field3': 2.2, 'arr': [4,5,6], 'field4': false },
                        { 'field1': 234, 'field2': 1.3, 'field3': 2.3, 'arr': [7,8,9], 'field4': true },
                    ],
                    'i64arr': [987,654,321]
                }");
                pclient.request("connector.entryHandler.entry", msg, (resp) =>
                {
                    //process the data
                    Debug.Log("收到服务端返回" + resp.ToString());
                });
            });
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
