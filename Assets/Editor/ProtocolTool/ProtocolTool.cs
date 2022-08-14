using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEngine;

public class ProtocolTool
{
    //配置文件所在路径
    private static string PROTO_INFO_PATH = Application.dataPath + "/Editor/ProtocolTool/ProtocolInfo.xml";

    private static GenerateCSharp generateCSharp = new GenerateCSharp();

    [MenuItem("ProtocolTool/生成C#脚本")]
    private static void GenerateCSharp()
    {
        //1.读取xml相关的信息
        //XmlNodeList list = GetNodes("enum");
        //2.根据这些信息 去拼接字符串 生成对应的脚本
        //生成对应的枚举脚本
        generateCSharp.GenerateEnum(GetNodes("enum"));
        //生成对应的数据结构类脚本
        generateCSharp.GenerateData(GetNodes("data"));
        //生成对应的消息类脚本
        generateCSharp.GenerateMsg(GetNodes("message"));
        //生成消息池
        generateCSharp.GenerateMsgPool(GetNodes("message"));

        //刷新编辑器界面 让我们可以看到生成的内容 不需要手动进行刷新了
        AssetDatabase.Refresh();
    }

    [MenuItem("ProtocolTool/生成C++脚本")]
    private static void GenerateC()
    {
        Debug.Log("生成C++代码");
    }

    [MenuItem("ProtocolTool/生成Java脚本")]
    private static void GenerateJava()
    {
        Debug.Log("生成Java代码");
    }


    /// <summary>
    /// 获取指定名字的所有子节点 的 List
    /// </summary>
    /// <param name="nodeName"></param>
    /// <returns></returns>
    private static XmlNodeList GetNodes(string nodeName)
    {
        XmlDocument xml = new XmlDocument();
        xml.Load(PROTO_INFO_PATH);
        XmlNode root = xml.SelectSingleNode("messages");
        return root.SelectNodes(nodeName);
    }
}
