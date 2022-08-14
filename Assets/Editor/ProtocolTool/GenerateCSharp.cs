using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

public class GenerateCSharp
{
    //协议保存路径
    private string SAVE_PATH = Application.dataPath + "/Scripts/Protocol/";

    //生成枚举
    public void GenerateEnum(XmlNodeList nodes)
    {
        //生成枚举脚本的逻辑
        string namespaceStr = "";
        string enumNameStr = "";
        string fieldStr = "";

        foreach (XmlNode enumNode in nodes)
        {
            //获取命名空间配置信息
            namespaceStr = enumNode.Attributes["namespace"].Value;
            //获取枚举名配置信息
            enumNameStr = enumNode.Attributes["name"].Value;
            //获取所有的字段节点 然后进行字符串拼接
            XmlNodeList enumFields = enumNode.SelectNodes("field");
            //一个新的枚举 需要清空一次上一次拼接的字段字符串
            fieldStr = "";
            foreach (XmlNode enumField in enumFields)
            {
                fieldStr += "\t\t" + enumField.Attributes["name"].Value;
                if (enumField.InnerText != "")
                    fieldStr += " = " + enumField.InnerText;
                fieldStr += ",\r\n";
            }
            //对所有可变的内容进行拼接
            string enumStr = $"namespace {namespaceStr}\r\n" +
                             "{\r\n" +
                                $"\tpublic enum {enumNameStr}\r\n" +
                                "\t{\r\n" +
                                    $"{fieldStr}" +
                                "\t}\r\n" +
                             "}";
            //保存文件的路径
            string path = SAVE_PATH + namespaceStr + "/Enum/";
            //如果不存在这个文件夹 则创建
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            //字符串保存 存储为枚举脚本文件
            File.WriteAllText(path + enumNameStr + ".cs", enumStr);
        }

        Debug.Log("枚举生成结束");
    }

    //生成数据结构类
    public void GenerateData(XmlNodeList nodes)
    {
        string namespaceStr = "";
        string classNameStr = "";
        string fieldStr = "";
        string getBytesNumStr = "";
        string writingStr = "";
        string readingStr = "";

        foreach (XmlNode dataNode in nodes)
        {
            //命名空间
            namespaceStr = dataNode.Attributes["namespace"].Value;
            //类名
            classNameStr = dataNode.Attributes["name"].Value;
            //读取所有字段节点
            XmlNodeList fields = dataNode.SelectNodes("field");
            //通过这个方法进行成员变量声明的拼接 返回拼接结果
            fieldStr = GetFieldStr(fields);
            //通过某个方法 对GetBytesNum函数中的字符串内容进行拼接 返回结果
            getBytesNumStr = GetGetBytesNumStr(fields);
            //通过某个方法 对Writing函数中的字符串内容进行拼接 返回结果
            writingStr = GetWritingStr(fields);
            //通过某个方法 对Reading函数中的字符串内容进行拼接 返回结果
            readingStr = GetReadingStr(fields);

            string dataStr = "using System;\r\n" +
                             "using System.Collections.Generic;\r\n" +
                             "using System.Text;\r\n" + 
                             $"namespace {namespaceStr}\r\n" +
                              "{\r\n" +
                              $"\tpublic class {classNameStr} : BaseData\r\n" +
                              "\t{\r\n" +
                                    $"{fieldStr}" +
                                    "\t\tpublic override int GetBytesNum()\r\n" +
                                    "\t\t{\r\n" +
                                        "\t\t\tint num = 0;\r\n" +
                                        $"{getBytesNumStr}" +
                                        "\t\t\treturn num;\r\n" +
                                    "\t\t}\r\n" +
                                    "\t\tpublic override byte[] Writing()\r\n" +
                                    "\t\t{\r\n" +
                                        "\t\t\tint index = 0;\r\n"+
                                        "\t\t\tbyte[] bytes = new byte[GetBytesNum()];\r\n" +
                                        $"{writingStr}" +
                                        "\t\t\treturn bytes;\r\n" +
                                    "\t\t}\r\n" +
                                    "\t\tpublic override int Reading(byte[] bytes, int beginIndex = 0)\r\n" +
                                    "\t\t{\r\n" +
                                        "\t\t\tint index = beginIndex;\r\n" +
                                        $"{readingStr}" +
                                        "\t\t\treturn index - beginIndex;\r\n" +
                                    "\t\t}\r\n" +
                              "\t}\r\n" +
                              "}";

            //保存为 脚本文件
            //保存文件的路径
            string path = SAVE_PATH + namespaceStr + "/Data/";
            //如果不存在这个文件夹 则创建
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            //字符串保存 存储为枚举脚本文件
            File.WriteAllText(path + classNameStr + ".cs", dataStr);

        }
        Debug.Log("数据结构类生成结束");
    }

    //生成消息类
    public void GenerateMsg(XmlNodeList nodes)
    {
        string idStr = "";
        string namespaceStr = "";
        string classNameStr = "";
        string fieldStr = "";
        string getBytesNumStr = "";
        string writingStr = "";
        string readingStr = "";

        foreach (XmlNode dataNode in nodes)
        {
            //消息ID
            idStr = dataNode.Attributes["id"].Value;
            //命名空间
            namespaceStr = dataNode.Attributes["namespace"].Value;
            //类名
            classNameStr = dataNode.Attributes["name"].Value;
            //读取所有字段节点
            XmlNodeList fields = dataNode.SelectNodes("field");
            //通过这个方法进行成员变量声明的拼接 返回拼接结果
            fieldStr = GetFieldStr(fields);
            //通过某个方法 对GetBytesNum函数中的字符串内容进行拼接 返回结果
            getBytesNumStr = GetGetBytesNumStr(fields);
            //通过某个方法 对Writing函数中的字符串内容进行拼接 返回结果
            writingStr = GetWritingStr(fields);
            //通过某个方法 对Reading函数中的字符串内容进行拼接 返回结果
            readingStr = GetReadingStr(fields);

            string dataStr = "using System;\r\n" +
                             "using System.Collections.Generic;\r\n" +
                             "using System.Text;\r\n" +
                             $"namespace {namespaceStr}\r\n" +
                              "{\r\n" +
                              $"\tpublic class {classNameStr} : BaseMsg\r\n" +
                              "\t{\r\n" +
                                    $"{fieldStr}" +
                                    "\t\tpublic override int GetBytesNum()\r\n" +
                                    "\t\t{\r\n" +
                                        "\t\t\tint num = 8;\r\n" +//这个8代表的是 消息ID的4个字节 + 消息体长度的4个字节
                                        $"{getBytesNumStr}" +
                                        "\t\t\treturn num;\r\n" +
                                    "\t\t}\r\n" +
                                    "\t\tpublic override byte[] Writing()\r\n" +
                                    "\t\t{\r\n" +
                                        "\t\t\tint index = 0;\r\n" +
                                        "\t\t\tbyte[] bytes = new byte[GetBytesNum()];\r\n" +
                                        "\t\t\tWriteInt(bytes, GetID(), ref index);\r\n" +
                                        "\t\t\tWriteInt(bytes, bytes.Length - 8, ref index);\r\n" +
                                        $"{writingStr}" +
                                        "\t\t\treturn bytes;\r\n" +
                                    "\t\t}\r\n" +
                                    "\t\tpublic override int Reading(byte[] bytes, int beginIndex = 0)\r\n" +
                                    "\t\t{\r\n" +
                                        "\t\t\tint index = beginIndex;\r\n" +
                                        $"{readingStr}" +
                                        "\t\t\treturn index - beginIndex;\r\n" +
                                    "\t\t}\r\n" +
                                    "\t\tpublic override int GetID()\r\n" +
                                    "\t\t{\r\n" +
                                        "\t\t\treturn " + idStr + ";\r\n" +
                                    "\t\t}\r\n" +
                              "\t}\r\n" +
                              "}";

            //保存为 脚本文件
            //保存文件的路径
            string path = SAVE_PATH + namespaceStr + "/Msg/";
            //如果不存在这个文件夹 则创建
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            //字符串保存 存储为枚举脚本文件
            File.WriteAllText(path + classNameStr + ".cs", dataStr);


            //生成处理器脚本
            //判断消息处理器脚本是否存在 如果存在 就不要覆盖了 避免把写过的逻辑处理代码覆盖了
            //如果想要改变 那就直接把没用的删了 它就会自动生成
            if (File.Exists(path + classNameStr + "Handler.cs"))
                continue;
            string handlerStr = $"namespace {namespaceStr}\r\n" +
                                "{\r\n" +
                                    $"\tpublic class {classNameStr}Handler : BaseHandler\r\n" +
                                    "\t{\r\n" +
                                        "\t\tpublic override void MsgHandle()\r\n" +
                                        "\t\t{\r\n" +
                                            $"\t\t\t{classNameStr} msg = message as {classNameStr};\r\n" +
                                        "\t\t}\r\n" +
                                    "\t}\r\n" +
                                "}\r\n";

            //把消息处理器类的内容保存到本地
            File.WriteAllText(path + classNameStr + "Handler.cs", handlerStr);
            Debug.Log("消息处理器类生成结束");

        }
        Debug.Log("消息生成结束");
    }

    //生成消息池 主要就是ID和消息类型以及消息处理器类型的对应关系
    public void GenerateMsgPool(XmlNodeList nodes)
    {
        List<string> ids = new List<string>();
        List<string> names = new List<string>();
        List<string> nameSpaces = new List<string>();

        foreach (XmlNode dataNode in nodes)
        {
            //记录所有消息的ID
            string id = dataNode.Attributes["id"].Value;
            if (!ids.Contains(id))
                ids.Add(id);
            else
                Debug.LogError("存在相同ID的消息" + id);
            //记录所有消息的名字
            string name = dataNode.Attributes["name"].Value;
            if (!names.Contains(name))
                names.Add(name);
            else
                Debug.LogError("存在同名的消息" + name + ",建议即使在不同命名空间中也不要有同名消息");
            //记录所有消息的命名空间
            string msgNamespace = dataNode.Attributes["namespace"].Value;
            if (!nameSpaces.Contains(msgNamespace))
                nameSpaces.Add(msgNamespace);
        }

        //获取所有需要引用的命名空间 拼接好
        string nameSpacesStr = "";
        for (int i = 0; i < nameSpaces.Count; i++)
            nameSpacesStr += $"using {nameSpaces[i]};\r\n";
        //获取所有消息注册相关的内容
        string registerStr = "";
        for (int i = 0; i < ids.Count; i++)
            registerStr += $"\t\tRegister({ids[i]}, typeof({names[i]}), typeof({names[i]}Handler));\r\n";

        //消息池对应的类的字符串信息
        string msgPoolStr = "using System;\r\n" +
                            "using System.Collections.Generic;\r\n" +
                            nameSpacesStr +
                            "public class MsgPool\r\n" +
                            "{\r\n" +
                                "\tprivate Dictionary<int, Type> messsages = new Dictionary<int, Type>();\r\n" +
                                "\tprivate Dictionary<int, Type> handlers = new Dictionary<int, Type>();\r\n" +
                                "\tpublic MsgPool()\r\n" +
                                "\t{\r\n" +
                                    registerStr +
                                "\t}\r\n" +
                                "\tprivate void Register(int id, Type messageType, Type handlerType)\r\n" +
                                "\t{\r\n" +
                                    "\t\tmesssages.Add(id, messageType);\r\n" +
                                    "\t\thandlers.Add(id, handlerType);\r\n" +
                                "\t}\r\n" +
                                "\tpublic BaseMsg GetMessage(int id)\r\n" +
                                "\t{\r\n" +
                                    "\t\tif (!messsages.ContainsKey(id))\r\n" +
                                    "\t\t\treturn null;\r\n" +
                                    "\t\treturn Activator.CreateInstance(messsages[id]) as BaseMsg;\r\n" +
                                "\t}\r\n" +
                                "\tpublic BaseHandler GetHandler(int id)\r\n" +
                                "\t{\r\n" +
                                    "\t\tif (!handlers.ContainsKey(id))\r\n" +
                                    "\t\t\treturn null;\r\n" +
                                    "\t\treturn Activator.CreateInstance(handlers[id]) as BaseHandler;\r\n" +
                                "\t}\r\n" +
                            "}\r\n";

        string path = SAVE_PATH + "/Pool/";
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        //保存到本地
        File.WriteAllText(path + "MsgPool.cs", msgPoolStr);

        Debug.Log("消息池生成结束");
    }

    /// <summary>
    /// 获取成员变量声明内容
    /// </summary>
    /// <param name="fields"></param>
    /// <returns></returns>
    private string GetFieldStr(XmlNodeList fields)
    {
        string fieldStr = "";
        foreach (XmlNode field in fields)
        {
            //变量类型
            string type = field.Attributes["type"].Value;
            //变量名
            string fieldName = field.Attributes["name"].Value;
            if(type == "list")
            {
                string T = field.Attributes["T"].Value;
                fieldStr += "\t\tpublic List<" + T + "> ";
            }
            else if(type == "array")
            {
                string data = field.Attributes["data"].Value;
                fieldStr += "\t\tpublic " + data + "[] ";
            }
            else if(type == "dic")
            {
                string Tkey = field.Attributes["Tkey"].Value;
                string Tvalue = field.Attributes["Tvalue"].Value;
                fieldStr += "\t\tpublic Dictionary<" + Tkey +  ", " + Tvalue + "> ";
            }
            else if(type == "enum")
            {
                string data = field.Attributes["data"].Value;
                fieldStr += "\t\tpublic " + data + " ";
            }
            else
            {
                fieldStr += "\t\tpublic " + type + " ";
            }

            fieldStr += fieldName + ";\r\n";
        }
        return fieldStr;
    }

    //拼接 GetBytesNum函数的方法
    private string GetGetBytesNumStr(XmlNodeList fields)
    {
        string bytesNumStr = "";

        string type = "";
        string name = "";
        foreach (XmlNode field in fields)
        {
            type = field.Attributes["type"].Value;
            name = field.Attributes["name"].Value;
            if (type == "list")
            {
                string T = field.Attributes["T"].Value;
                bytesNumStr += "\t\t\tnum += 2;\r\n";//+2 是为了节约字节数 用一个short去存储信息
                bytesNumStr += "\t\t\tfor (int i = 0; i < " + name + ".Count; ++i)\r\n";
                //这里使用的是 name + [i] 目的是获取 list当中的元素传入进行使用
                bytesNumStr += "\t\t\t\tnum += " + GetValueBytesNum(T, name + "[i]") + ";\r\n";
            }
            else if (type == "array")
            {
                string data = field.Attributes["data"].Value;
                bytesNumStr += "\t\t\tnum += 2;\r\n";//+2 是为了节约字节数 用一个short去存储信息
                bytesNumStr += "\t\t\tfor (int i = 0; i < " + name + ".Length; ++i)\r\n";
                //这里使用的是 name + [i] 目的是获取 list当中的元素传入进行使用
                bytesNumStr += "\t\t\t\tnum += " + GetValueBytesNum(data, name + "[i]") + ";\r\n";
            }
            else if (type == "dic")
            {
                string Tkey = field.Attributes["Tkey"].Value;
                string Tvalue = field.Attributes["Tvalue"].Value;
                bytesNumStr += "\t\t\tnum += 2;\r\n";//+2 是为了节约字节数 用一个short去存储信息
                bytesNumStr += "\t\t\tforeach (" + Tkey + " key in " + name + ".Keys)\r\n";
                bytesNumStr += "\t\t\t{\r\n";
                bytesNumStr += "\t\t\t\tnum += " + GetValueBytesNum(Tkey, "key") + ";\r\n";
                bytesNumStr += "\t\t\t\tnum += " + GetValueBytesNum(Tvalue, name + "[key]") + ";\r\n";
                bytesNumStr += "\t\t\t}\r\n";
            }
            else
                bytesNumStr += "\t\t\tnum += " + GetValueBytesNum(type, name) + ";\r\n";
        }

        return bytesNumStr;
    }
    //获取 指定类型的字节数
    private string GetValueBytesNum(string type, string name)
    {
        //这里我没有写全 所有的常用变量类型 你可以根据需求去添加
        switch (type)
        {
            case "int":
            case "float":
            case "enum":
                return "4";
            case "long":
                return "8";
            case "byte":
            case "bool":
                return "1";
            case "short":
                return "2";
            case "string":
                return "4 + Encoding.UTF8.GetByteCount(" + name + ")";
            default:
                return name + ".GetBytesNum()";
        }
    }

    //拼接 Writing函数的方法
    private string GetWritingStr(XmlNodeList fields)
    {
        string writingStr = "";

        string type = "";
        string name = "";
        foreach (XmlNode field in fields)
        {
            type = field.Attributes["type"].Value;
            name = field.Attributes["name"].Value;
            if(type == "list")
            {
                string T = field.Attributes["T"].Value;
                writingStr += "\t\t\tWriteShort(bytes, (short)" + name + ".Count, ref index);\r\n";
                writingStr += "\t\t\tfor (int i = 0; i < " + name + ".Count; ++i)\r\n";
                writingStr += "\t\t\t\t" + GetFieldWritingStr(T, name + "[i]") + "\r\n";
            }
            else if (type == "array")
            {
                string data = field.Attributes["data"].Value;
                writingStr += "\t\t\tWriteShort(bytes, (short)" + name + ".Length, ref index);\r\n";
                writingStr += "\t\t\tfor (int i = 0; i < " + name + ".Length; ++i)\r\n";
                writingStr += "\t\t\t\t" + GetFieldWritingStr(data, name + "[i]") + "\r\n";
            }
            else if (type == "dic")
            {
                string Tkey = field.Attributes["Tkey"].Value;
                string Tvalue = field.Attributes["Tvalue"].Value;
                writingStr += "\t\t\tWriteShort(bytes, (short)" + name + ".Count, ref index);\r\n";
                writingStr += "\t\t\tforeach (" + Tkey + " key in " + name + ".Keys)\r\n";
                writingStr += "\t\t\t{\r\n";
                writingStr += "\t\t\t\t" + GetFieldWritingStr(Tkey, "key") + "\r\n";
                writingStr += "\t\t\t\t" + GetFieldWritingStr(Tvalue, name + "[key]") + "\r\n";
                writingStr += "\t\t\t}\r\n";
            }
            else
            {
                writingStr += "\t\t\t" + GetFieldWritingStr(type, name) + "\r\n";
            }
        }
        return writingStr;
    }

    private string GetFieldWritingStr(string type, string name)
    {
        switch (type)
        {
            case "byte":
                return "WriteByte(bytes, " + name + ", ref index);";
            case "int":
                return "WriteInt(bytes, " + name + ", ref index);";
            case "short":
                return "WriteShort(bytes, " + name + ", ref index);";
            case "long":
                return "WriteLong(bytes, " + name + ", ref index);";
            case "float":
                return "WriteFloat(bytes, " + name + ", ref index);";
            case "bool":
                return "WriteBool(bytes, " + name + ", ref index);";
            case "string":
                return "WriteString(bytes, " + name + ", ref index);";
            case "enum":
                return "WriteInt(bytes, Convert.ToInt32(" + name + "), ref index);";
            default:
                return "WriteData(bytes, " + name + ", ref index);";
        }
    }

    private string GetReadingStr(XmlNodeList fields)
    {
        string readingStr = "";

        string type = "";
        string name = "";
        foreach (XmlNode field in fields)
        {
            type = field.Attributes["type"].Value;
            name = field.Attributes["name"].Value;
            if (type == "list")
            {
                string T = field.Attributes["T"].Value;
                readingStr += "\t\t\t" + name + " = new List<" + T + ">();\r\n";
                readingStr += "\t\t\tshort " + name + "Count = ReadShort(bytes, ref index);\r\n";
                readingStr += "\t\t\tfor (int i = 0; i < " + name + "Count; ++i)\r\n";
                readingStr += "\t\t\t\t" + name + ".Add(" + GetFieldReadingStr(T) + ");\r\n";
            }
            else if (type == "array")
            {
                string data = field.Attributes["data"].Value;
                readingStr += "\t\t\tshort " + name + "Length = ReadShort(bytes, ref index);\r\n";
                readingStr += "\t\t\t" + name + " = new " + data + "["+ name + "Length];\r\n";
                readingStr += "\t\t\tfor (int i = 0; i < " + name + "Length; ++i)\r\n";
                readingStr += "\t\t\t\t" + name + "[i] = " + GetFieldReadingStr(data) + ";\r\n";
            }
            else if (type == "dic")
            {
                string Tkey = field.Attributes["Tkey"].Value;
                string Tvalue = field.Attributes["Tvalue"].Value;
                readingStr += "\t\t\t" + name + " = new Dictionary<" + Tkey + ", " + Tvalue + ">();\r\n";
                readingStr += "\t\t\tshort " + name + "Count = ReadShort(bytes, ref index);\r\n";
                readingStr += "\t\t\tfor (int i = 0; i < " + name + "Count; ++i)\r\n";
                readingStr += "\t\t\t\t" + name + ".Add(" + GetFieldReadingStr(Tkey) + ", " +
                                                            GetFieldReadingStr(Tvalue) + ");\r\n";
            }
            else if (type == "enum")
            {
                string data = field.Attributes["data"].Value;
                readingStr += "\t\t\t" + name + " = (" + data + ")ReadInt(bytes, ref index);\r\n";
            }
            else
                readingStr += "\t\t\t" + name + " = " + GetFieldReadingStr(type) + ";\r\n";
        }

        return readingStr;
    }

    private string GetFieldReadingStr(string type)
    {
        switch (type)
        {
            case "byte":
                return "ReadByte(bytes, ref index)";
            case "int":
                return "ReadInt(bytes, ref index)";
            case "short":
                return "ReadShort(bytes, ref index)";
            case "long":
                return "ReadLong(bytes, ref index)";
            case "float":
                return "ReadFloat(bytes, ref index)";
            case "bool":
                return "ReadBool(bytes, ref index)";
            case "string":
                return "ReadString(bytes, ref index)";
            default:
                return "ReadData<" + type + ">(bytes, ref index)";
        }
    }
}
