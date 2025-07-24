using UnityEngine;
using System.Globalization;
using System;

public class WordTree_TextIterator
{
    public int curIndex;
    public string text;

    public WordTree_TextIterator(string text)
    {
        this.curIndex = 0;
        this.text = text;
    }    
    
    public WordTree_TextIterator(string text, int startIndex)
    {
        this.curIndex = startIndex;
        this.text = text;
    }
}

public class WordTree_PossibleWord
{
    public int count;
    public int current;
    public int mark;
    public int offset;
    public int[] lengthArray = new int[8];

    public int AcceptMax(ref WordTree_TextIterator text)
    {
        text.curIndex = offset + lengthArray[lengthArray.Length - 1];
        return lengthArray[mark];
    }

    public int AcceptMarked(ref WordTree_TextIterator text)
    {
        text.curIndex = offset + lengthArray[mark];
        return lengthArray[mark];
    }

    public void SetCurrent(ref WordTree_TextIterator text)
    {
        text.curIndex = offset + lengthArray[mark];
    }

    public void MarkedCurrent()
    {
        mark = current;
    }

    public bool BackUp(ref WordTree_TextIterator text)
    {
        if (current > 0)
        {
            text.curIndex = offset + lengthArray[--current];
            return true;
        }
        return false;
    }
}

public class WordTree
{
    public WordTreeNode Root;

    private const int THAI_LOOKAHEAD = 3;
    private WordTree_PossibleWord[] possibleWords = new WordTree_PossibleWord[3];
    private int totalSearchTime;
    private int searchTreeTime;

    public WordTree()
    {

    }

    // 词库字符串建树
    public void GenTreeWithWordAndSeparate(string srcStr)
    {
        Root = new WordTreeNode(string.Empty, false);
        string[] strArr = srcStr.Split(new char[] { '\u000A', '\u000D'});
        //ADbg.LogF("==========================[wktest] GenTreeWithWordAndSeparate strArrLen = " + strArr.Length + " / srcStr.Length = " + srcStr.Length);
        foreach (var str in strArr)
        {
            AddWord(str);
        }

        for (int i = 0; i < THAI_LOOKAHEAD; i++)
        {
            possibleWords[i] = new WordTree_PossibleWord();
        }
    }

    // 向字典树添加单个词
    public void AddWord(string srcStr)
    {
        WordTreeNode curNode = Root;
        var strIter = StringInfo.GetTextElementEnumerator(srcStr);
        while (strIter.MoveNext())
        {
            string str = strIter.GetTextElement();
            curNode = curNode.AddChild(str);
        }

        if (curNode != Root)
        {
            curNode.SetIsWordEnd(true);
        }
    }

    public void RemoveWord(string word)
    {

    }

    public void DeInit()
    {

    }

    //字典树词匹配
    public int FindWord( string text, int startPos )
    {
        int endPos = startPos;
        int curPos = startPos;
        WordTreeNode curNode = Root;
        var iter = StringInfo.GetTextElementEnumerator(text, startPos);
        while (iter.MoveNext())
        {
            var str = iter.GetTextElement();
            curNode = curNode.GetChild(iter.GetTextElement());
            if (curNode == null)
            {
                break;
            }

            curPos += str.Length;
            if (curNode.IsWordEnd())
            {
                endPos = curPos;
            }
        }
        //endPos--;

        return endPos > startPos ? endPos - 1 : -1;
    }

    public int Candidates(WordTree_TextIterator m_ITex, ref WordTree_PossibleWord data)
    {
        searchTreeTime++;

        int startIndex = m_ITex.curIndex;

        if (startIndex < 0 || startIndex >= m_ITex.text.Length)
            return 0;

        var iter = StringInfo.GetTextElementEnumerator(m_ITex.text, startIndex);
        var curNode = Root;
        var count = 0;
        var curIndex = 0;

        string str;

        data.offset = startIndex;
        while (iter.MoveNext())
        {
            str = iter.GetTextElement();
            curNode = curNode.GetChild(str);
            curIndex += str.Length;

            if (curNode == null)
                break;

            if (curNode.IsWordEnd())
            {

                if (count >= data.lengthArray.Length)
                {
                    Array.Resize(ref data.lengthArray, data.lengthArray.Length * 2);
                }
                data.lengthArray[count] = curIndex;
                count++;
            }
        }
        data.count = count;
        data.current = count - 1;
        data.mark = data.current;

        return count;
    }

    //字典树匹配 From ICU
    public int FindWordWithCandidate(string text, int startPos)
    {
        int curWordLength = 0;
        int m_candidates = 0;
        int m_wordsFound = 0;
        int m_textEnd = text.Length;

        var m_IText = new WordTree_TextIterator(text, startPos);

        int lastIndex = -1;
        lastIndex = m_IText.curIndex;
        
        totalSearchTime++;

        m_candidates = Candidates(m_IText, ref possibleWords[m_wordsFound % THAI_LOOKAHEAD]);
        if (m_candidates == 1)
        {
            curWordLength = possibleWords[m_wordsFound % THAI_LOOKAHEAD].AcceptMarked(ref m_IText);
            m_wordsFound++;
        }
        else if (m_candidates > 1)
        {
            if (m_IText.curIndex >= text.Length)
            {
                goto FoundBest;
            }

            possibleWords[m_wordsFound % THAI_LOOKAHEAD].SetCurrent(ref m_IText);
            do
            {
                if (Candidates(m_IText, ref possibleWords[(m_wordsFound + 1) % THAI_LOOKAHEAD]) > 0)
                {
                    // Followed by another dictionary word; mark first word as a good candidate
                    possibleWords[m_wordsFound % THAI_LOOKAHEAD].MarkedCurrent();
                    if (m_IText.curIndex >= m_textEnd)
                    {
                        goto FoundBest;
                    }

                    possibleWords[(m_wordsFound + 1) % THAI_LOOKAHEAD].SetCurrent(ref m_IText);
                    do
                    {
                        // See if any of the possible second words is followed by a third word
                        if (Candidates(m_IText, ref possibleWords[(m_wordsFound + 2) % THAI_LOOKAHEAD]) > 0)
                        {
                            possibleWords[m_wordsFound % THAI_LOOKAHEAD].MarkedCurrent();
                            goto FoundBest;
                        }
                    }
                    while (possibleWords[(m_wordsFound + 1) % THAI_LOOKAHEAD].BackUp(ref m_IText));
                }
            }
            while (possibleWords[(m_wordsFound) % THAI_LOOKAHEAD].BackUp(ref m_IText));
        FoundBest:
            curWordLength = possibleWords[m_wordsFound % THAI_LOOKAHEAD].AcceptMarked(ref m_IText);
            m_wordsFound++;
        }

        if (curWordLength == 0 && m_candidates > 1)
        {
            curWordLength = possibleWords[m_wordsFound % THAI_LOOKAHEAD].AcceptMax(ref m_IText);
        }
        if (curWordLength > 0)
        {
            return m_IText.curIndex - 1;
        }
        else
        { 
            return -1;
        }
    }
}
