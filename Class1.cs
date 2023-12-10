using System.Collections.Generic;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using System.IO;

public class Data
{
    public string Content;
    public int Line;
    public Data(string content, int line)
    {
        Content = content;
        Line = line;
    }
}
class MyQueue<T>
{
    public class Node
    {
        public T Data;
        public Node After, Before;
        public Node(T data)
        {
            Data = data;
            After = Before = null;
        }
    }
    public Node head;
    public Node rear;
    public MyQueue()
    {
        head = rear = null;
    }
    public void Enqueue(T item)
    {
        Node newNode = new Node(item);

        if (head == null)
        {
            head = newNode;
            rear = newNode;
        }
        else
        {
            newNode.Before = rear;
            rear.After = newNode;
            rear = newNode;
        }
    }
    public T Dequeue()
    {
        if (head == null)
        {
            throw new InvalidOperationException("Queue is empty");
        }

        T data = head.Data;
        head = head.After;

        if (head == null)
        {
            rear = null;
        }
        else
        {
            head.Before = null;
        }

        return data;
    }
    public bool IsEmpty()
    {
        return head == null;
    }
    public void Clear()
    {
        head = rear = null;
    }
    public List<T> ToList()
    {
        List<T> result = new List<T>();
        while (head != null)
        {
            result.Add(head.Data);
            head = head.After;
        }
        return result;
    }
}
class MyPriorityQueue : MyQueue<Data>
{
    private Node Find(Data data)
    {
        Node curNode = head;

        while (curNode.After != null && curNode.After.Data.Line < data.Line)
        {
            curNode = curNode.After;
        }

        return curNode;
    }
    public void Insert(Data data)
    {
        Node newNode = new Node(data);
        if (head == null)
            head = newNode;
        else
        {
            if (head == null || data.Line < head.Data.Line)
            {
                newNode.After = head;
                if (head != null)
                    head.Before = newNode;
                head = newNode;
            }
            else
            {
                Node prevNode = Find(data);

                newNode.After = prevNode.After;
                newNode.Before = prevNode;
                prevNode.After = newNode;
            }
        }
    }
}
class MyInputRestrictedQueue : MyQueue<Data>
{
    public int Search(string key)
    {
        Node curNode = rear;
        int i = 0;
        while (curNode != null)
        {
            if (curNode.Data.Content == key)
            {
                return i;
            }
            i++;
            curNode = curNode.Before;
        }
        return -1;
    }
    public Data DequeueEnd()
    {
        if (IsEmpty())
            throw new Exception("Queue is empty");
        else
        {
            Data res = rear.Data;
            rear = rear.Before;
            if (rear == null)
                head = null;
            else
                rear.After = null;
            return res;
        }
    }
}
public class HTMLChecker
{
    private string[] voidTags = {
        "!DOCTYPE","!--","br", "hr", "img", "input", "link", "base", "meta", "param", "area", "embed", "col", "track", "source"
    };
    private string filePath;
    public List<Data> Errors;
    public List<string> Results;
    public HTMLChecker(string filePath)
    {
        this.filePath = filePath;
    }
    public void Start()
    {
        Errors = new List<Data>();
        Results = new List<string>();
        checkHTML();
    }
    private string getDataFromFile()
    {
        // return File.ReadAllText(filePath);
        try
        {
            string s = File.ReadAllText(filePath);
            return s;
        }
        catch (Exception ex)
        {
            System.Console.WriteLine(("không nhận được file"));
        }
        return null;
    }
    private string getTagName(string tag, bool openTag = false)
    {
        if (openTag)
            return tag.Substring(0, tag.Length).Trim().Split(' ')[0];
        else
            return tag.Substring(0, tag.Length).Replace(" ", "");
    }
    private void noteOpenTag(MyInputRestrictedQueue openTags, MyPriorityQueue errors)
    {
        Data tag = openTags.DequeueEnd();
        errors.Insert(
            new Data($"Line {tag.Line}: Open tag <{tag.Content}> has not been closed", tag.Line));
    }
    private void noteCloseTag(Data tag, MyPriorityQueue errors)
    {
        errors.Insert(
            new Data($"Line {tag.Line}: Close tag </{tag.Content}> has found but not found open tag", tag.Line));
    }
    private string getTag(char[] cs, ref int i, int length)
    {
        string res = "";
        while (i < length && cs[i] != '>')
        {
            res += cs[i];
            i++;
        }
        return res;
    }
    private void checkHTML()
    {
        string html = getDataFromFile();
        html = Regex.Replace(html, @"<!--([\s\S]*?)-->", "");
        string[] lines = html.Split('\n');
        MyQueue<string> results = new MyQueue<string>();
        MyInputRestrictedQueue openTags = new MyInputRestrictedQueue();
        MyPriorityQueue errors = new MyPriorityQueue();

        for (int i = 0; i < lines.Length; i++)
        {
            char[] cs = lines[i].ToCharArray();
            int j = 0, n = cs.Length;
            while (j < n)
            {
                if (cs[j] == '<')
                {
                    j++;
                    if (cs[j] == '/')
                    {
                        j++;
                        string tagContent = getTag(cs, ref j, n);
                        if (j == n)
                            results.Enqueue("</" + tagContent);
                        else
                        {
                            if (tagContent.Contains(' '))
                            {
                                tagContent = getTagName(tagContent);
                                errors.Insert(
                                    new Data($"Line {i + 1}: Found </{tagContent}> 'SPACE' in End Tag", i + 1));
                            }
                            else
                                tagContent = getTagName(tagContent);
                            int thrower = openTags.Search(tagContent);
                            if (thrower != -1)
                            {
                                while (thrower > 0)
                                {
                                    noteOpenTag(openTags, errors);
                                    thrower--;
                                }
                                openTags.DequeueEnd();
                            }
                            else
                            { noteCloseTag(new Data(tagContent, i + 1), errors); }
                        }
                    }
                    else
                    {
                        string tagContent = getTag(cs, ref j, n);
                        if (j == n)
                            results.Enqueue("<" + tagContent);
                        else
                        {
                            if (tagContent.StartsWith(" " ))
                            {
                                tagContent = getTagName(tagContent, true);
                                errors.Insert(new Data($"Line {i + 1}: Found <{tagContent}> has 'SPACE' after '<'", i + 1));
                            }
                            else
                                tagContent = getTagName(tagContent, true);
                            if (!voidTags.Contains(tagContent))
                                openTags.Enqueue(new Data(tagContent, i + 1));
                        }
                    }
                    j++;
                }
                string content = "";
                while (j < n && cs[j] != '<')
                {
                    content += cs[j];
                    j++;
                }
                if (!string.IsNullOrWhiteSpace(content))
                    results.Enqueue(content.Trim());
            }
        }
        while (!openTags.IsEmpty())
        {
            noteOpenTag(openTags, errors);
        }
        Errors = errors.ToList();
        Results = results.ToList();
    }
}