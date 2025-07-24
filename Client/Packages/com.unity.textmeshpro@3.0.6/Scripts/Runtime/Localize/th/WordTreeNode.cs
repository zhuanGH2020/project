using System.Collections.Generic;

public class WordTreeNode
{
    public string value;
    public Dictionary<string, WordTreeNode> ChildNodes = new Dictionary<string, WordTreeNode>();
    private bool bIsWordEnd;

    public WordTreeNode()
    {
        bIsWordEnd = true;
    }

    public WordTreeNode(string str)
    {
        value = str;
        bIsWordEnd = false;
    }

    public WordTreeNode(string str, bool isEnd)
    {
        value = str;
        bIsWordEnd = isEnd;
    }

    public void DeInit()
    {
        var keys = ChildNodes.Keys;
        foreach (var key in keys)
        {
            var node = ChildNodes[key];
            node.DeInit();
        }
        ChildNodes.Clear();
    }

    public virtual bool IsWordEnd()
    {
        return bIsWordEnd;
    }

    public virtual string GetStr()
    {
        return value;
    }

    public virtual WordTreeNode GetChild(string value)
    {
        if (ChildNodes.ContainsKey(value))
        {
            return ChildNodes[value];
        }
        return null;
    }

    public virtual int GetChildCount()
    {
        return ChildNodes.Count;
    }

    public virtual void SetIsWordEnd(bool isWordEnd)
    {
        bIsWordEnd = isWordEnd;
    }

    public virtual WordTreeNode AddChild(string value)
    {
        if (!ChildNodes.ContainsKey(value))
        {
            WordTreeNode newNode = new WordTreeNode(value);
            return AddChild(newNode);
        }
        return ChildNodes[value];
    }

    public virtual WordTreeNode AddChild(WordTreeNode childNode)
    {
        string value = childNode.GetStr();
        if (!ChildNodes.ContainsKey(value))
        {
            ChildNodes.Add(value, childNode);
        }
        return ChildNodes[value];
    }

    public virtual void RemoveChild(string value)
    {
        if (ChildNodes.ContainsKey(value))
        {
            ChildNodes.Remove(value);
        }
    }

    public virtual void RemoveChild(WordTreeNode childNode)
    {
        string value = childNode.GetStr();
        if (ChildNodes.ContainsKey(value) && ChildNodes[value] == childNode)
        {
            ChildNodes.Remove(value);
        }
    }
}
