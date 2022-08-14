using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 消息处理器基类 主要用于处理消息的逻辑的
/// </summary>
public abstract class BaseHandler 
{
    //处理者处理哪个消息
    public BaseMsg message;

    //真正处理消息的方法
    public abstract void MsgHandle();
}
