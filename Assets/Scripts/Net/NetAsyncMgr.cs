using GamePlayer;
using GameSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class NetAsyncMgr : MonoBehaviour
{
    private static NetAsyncMgr instance;

    public static NetAsyncMgr Instance => instance;

    //和服务器进行连接的 Socket
    private Socket socket;

    //接受消息用的 缓存容器
    private byte[] cacheBytes = new byte[1024 * 1024];
    private int cacheNum = 0;

    private Queue<BaseHandler> receiveQueue = new Queue<BaseHandler>();

    //发送心跳消息的间隔时间
    private int SEND_HEART_MSG_TIME = 2;
    private HeartMsg hearMsg = new HeartMsg();

    //消息池对象 用于快速获取消息和消息处理类对象
    private MsgPool msgPool = new MsgPool();

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        //过场景不移除
        DontDestroyOnLoad(this.gameObject);
        //客户端循环定时给服务端发送心跳消息
        InvokeRepeating("SendHeartMsg", 0, SEND_HEART_MSG_TIME);
    }

    private void SendHeartMsg()
    {
        if (socket != null && socket.Connected)
            Send(hearMsg);
    }

    // Update is called once per frame
    void Update()
    {
        if (receiveQueue.Count > 0)
        {
            //目标二：不要每次新加了消息 就在这里去处理对应消息的逻辑
            //更加自动化的去处理他们 并且不要在网络层这来处理
            //通过消息处理者基类对象 调用处理方法 以后不管添加多少个消息
            //这都不用修改了
            receiveQueue.Dequeue().MsgHandle();
        }
    }

    //连接服务器的代码
    public void Connect(string ip, int port)
    {
        if (socket != null && socket.Connected)
            return;

        IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.RemoteEndPoint = ipPoint;
        args.Completed += (socket, args) =>
        {
            if(args.SocketError == SocketError.Success)
            {
                print("连接成功");
                //收消息
                SocketAsyncEventArgs receiveArgs = new SocketAsyncEventArgs();
                receiveArgs.SetBuffer(cacheBytes, 0, cacheBytes.Length);
                receiveArgs.Completed += ReceiveCallBack;
                this.socket.ReceiveAsync(receiveArgs);
            }
            else
            {
                print("连接失败" + args.SocketError);
                //服务器没有开启 或者说我们本地的网络没有连接
                //弹出面板 进行提示就行
            }
        };
        socket.ConnectAsync(args);
    }

    //收消息完成的回调函数
    private void ReceiveCallBack(object obj, SocketAsyncEventArgs args)
    {
        if(args.SocketError == SocketError.Success)
        {
            HandleReceiveMsg(args.BytesTransferred);
            //继续去收消息
            args.SetBuffer(cacheNum, args.Buffer.Length - cacheNum);
            //继续异步收消息
            if (this.socket != null && this.socket.Connected)
                socket.ReceiveAsync(args);
            else
                Close();
        }
        else
        {
            print("接受消息出错" + args.SocketError);
            //关闭客户端连接
            Close();
        }
    }

    public void Close(bool isSelf = false)
    {
        if(socket != null)
        {
            QuitMsg msg = new QuitMsg();
            socket.Send(msg.Writing());
            socket.Shutdown(SocketShutdown.Both);
            socket.Disconnect(false);
            socket.Close();
            socket = null;
        }

        //不是自己主动断开连接的
        if(!isSelf)
        {
            //断线重连 弹出一个面板
        }
    }

    public void SendTest(byte[] bytes)
    {
        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.SetBuffer(bytes, 0, bytes.Length);
        args.Completed += (socket, args) =>
        {
            if (args.SocketError != SocketError.Success)
            {
                print("发送消息失败" + args.SocketError);
                Close();
            }

        };
        this.socket.SendAsync(args);
    }

    public void Send(BaseMsg msg)
    {
        if(this.socket != null && this.socket.Connected)
        {
            byte[] bytes = msg.Writing();
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.SetBuffer(bytes, 0, bytes.Length);
            args.Completed += (socket, args) =>
            {
                if (args.SocketError != SocketError.Success)
                {
                    print("发送消息失败" + args.SocketError);
                    Close();
                }
                    
            };
            this.socket.SendAsync(args);
        }
        else
        {
            Close();
        }
    }

    //处理接受消息 分包、黏包问题的方法
    private void HandleReceiveMsg(int receiveNum)
    {
        int msgID = 0;
        int msgLength = 0;
        int nowIndex = 0;

        cacheNum += receiveNum;

        while (true)
        {
            //每次将长度设置为-1 是避免上一次解析的数据 影响这一次的判断
            msgLength = -1;
            //处理解析一条消息
            if (cacheNum - nowIndex >= 8)
            {
                //解析ID
                msgID = BitConverter.ToInt32(cacheBytes, nowIndex);
                nowIndex += 4;
                //解析长度
                msgLength = BitConverter.ToInt32(cacheBytes, nowIndex);
                nowIndex += 4;
            }

            if (cacheNum - nowIndex >= msgLength && msgLength != -1)
            {
                //解析消息体
                //BaseMsg baseMsg = null;
                //BaseHandler handler = null;
                ////目标一：不需要每次手动的去添加代码
                ////添加了消息后 根据这个ID 就能够自动去根据ID得到对应的消息类 来进行反序列化
                ////要更加的自动化
                //switch (msgID)
                //{
                //    case 1001:
                //        baseMsg = new PlayerMsg();
                //        baseMsg.Reading(cacheBytes, nowIndex);

                //        handler = new PlayerMsgHandler();
                //        handler.message = baseMsg;
                //        break;

                //}
                //if (handler != null)
                //    receiveQueue.Enqueue(handler);

                //得到一个指定ID的消息类对象 只不过是用父类装子类
                BaseMsg baseMsg = msgPool.GetMessage(msgID);
                if (baseMsg != null)
                {
                    //反序列化
                    baseMsg.Reading(cacheBytes, nowIndex);
                    //得到一个消息处理器对象
                    BaseHandler baseHandler = msgPool.GetHandler(msgID);
                    baseHandler.message = baseMsg;
                    //把消息处理器对象 放入队列中 稍后在Update中进行处理
                    receiveQueue.Enqueue(baseHandler);
                }

                nowIndex += msgLength;
                if (nowIndex == cacheNum)
                {
                    cacheNum = 0;
                    break;
                }
            }
            else
            {
                if (msgLength != -1)
                    nowIndex -= 8;
                //就是把剩余没有解析的字节数组内容 移到前面来 用于缓存下次继续解析
                Array.Copy(cacheBytes, nowIndex, cacheBytes, 0, cacheNum - nowIndex);
                cacheNum = cacheNum - nowIndex;
                break;
            }
        }

    }

    private void OnDestroy()
    {
        Close(true);
    }
}
