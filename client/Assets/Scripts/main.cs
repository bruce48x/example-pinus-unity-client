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
        Debug.Log("unity start!");

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
            Debug.Log("初始化 pomelo client 成功");
            JObject user = JObject.Parse("{}");
            pclient.connect(user, data =>
            {
                Debug.Log("连接服务端成功" + data.ToString());
                //process handshake call back data
                var msg = new JObject { { "name", "bruce" } };
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
