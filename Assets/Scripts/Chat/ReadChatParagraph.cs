using System.Collections.Generic;
using UnityEngine;
public class ReadChatParagraph:MonoBehaviour
{
    #region 单例
    private static ReadChatParagraph instance;
    public static ReadChatParagraph Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ReadChatParagraph>();
                if (instance == null)
                {
                    GameObject managerObj = new GameObject("ReadChatParagraph");
                    instance = managerObj.AddComponent<ReadChatParagraph>();
                }
            }
            return instance;
        }
    }
    #endregion
    public List<GraphData> GraphDatas=new List<GraphData>();
    public GraphData CurGraphData;
    public GraphData.SerializedNode CurNode;

    public void Awake()
    {
        InitData();
    }

    public List<ParagraphData> FindAllParagraphData()
    {
        List<ParagraphData> ParagraphDatas = new List<ParagraphData>();
        foreach (var GraphData in GraphDatas)
        {
            ParagraphDatas.Add(GraphData.paragraphData);
        }
        return ParagraphDatas;
    }

    public ParagraphData FindParagraphDataByName(string name)
    {
        foreach (GraphData graphData in GraphDatas)
        {
            if (graphData.paragraphData.ParagraphName == name)
            {
                return graphData.paragraphData;
            }
        }
        Debug.LogError($"未找到段落: {name}");
        return null;
    }
    
    public void InitData()
    {
        //Debug.Log("初始化段落数据"+GameDataManager.Instance.GeneratedChatData.CurrentGraphData.name);
        CurGraphData = GameDataManager.Instance.GeneratedChatData.CurrentGraphData;
        CurNode = GameDataManager.Instance.GeneratedChatData.CurrentNodeData;
    }
    
    public GraphData.SerializedNode FindStartNodeOfParagraph(string paragraphName)
    {
        if (paragraphName == "")
        {
            Debug.Log("段落名为空字符串");
            return null;
        }
        GraphData graphData =
            GraphDatas.Find(x => x.paragraphData.ParagraphName==paragraphName);
        if(graphData==null) return null;
        CurGraphData=graphData;
        GraphData.SerializedNode nodeData = graphData.nodes.Find(x => x.typeName == "Start");
        
        CurNode = nodeData;
        if (nodeData != null)
        {
            return nodeData;
        } 
        Debug.LogError("段落" + paragraphName + "未找到初始节点");
        return null;
    }

    public GraphData.SerializedNode FindNextNode(string portName="")
    {
        GraphData.SerializedEdge edge;
        if (portName == "")
        {
            edge = CurGraphData.edges.Find(x => x.outputNodeGUID == CurNode.guid);
        }
        else
        {
            edge = CurGraphData.edges.Find(x => x.outputNodeGUID == CurNode.guid&&x.outputPortName==portName);
        }

        if (edge != null)
        {
            GraphData.SerializedNode nodeData = CurGraphData.nodes.Find(x => x.guid == edge.inputNodeGUID);
            if (nodeData != null)
            {
                CurNode = nodeData;
                return nodeData;
            }
            
        }
        Debug.LogError($"无法找到{CurNode.Title}的输出节点{portName}连接的下一节点");
        return null;

            
    }

}