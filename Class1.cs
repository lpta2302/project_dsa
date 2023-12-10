using System.Collections.Generic;
using System.Linq;
using System;
using System.Text.RegularExpressions;
//Class để lưu trữ các lỗi dưới dạng tên tag và chỉ số dòng
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
//Queue cơ bản dùng để lưu trữ kết quả
public class MyQueue<T>
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
        //Nếu queue rỗng thì thêm node đầu vào
        if (head == null)
        {
            head = newNode;
            rear = newNode;
        }
        else
        {
        // Chèn vào sau rear (cuối hàng)
            newNode.Before = rear;
            rear.After = newNode;
            rear = newNode;
        }
    }
    public T Dequeue()
    {
        //Nếu queue rỗng, báo lỗi
        if (head == null)
        {
            throw new Exception("Queue is empty");
        }
        //Lưu lại giá trị Data vào data để trả về
        T data = head.Data;
        head = head.After;

        //Nếu queue chỉ có một phần tử thì dời rear vào null để giải phóng phần tử đó
        if (head == null)
        {
            rear = null;
        }
        //Không thì giải phóng phần tử đầu tiên của queue
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
    public List<T> ToList(){
        List<T> result = new List<T>();
        Node curNode = head;
        while(curNode!=null){
            result.Add(curNode.Data);
            curNode = curNode.After;
        }
        return result;
    }
}
// Hàng đợi ưu tiên kế thừa queue cơ bản dùng để lưu trữ lỗi
public class MyPriorityQueue : MyQueue<Data>{
    //Duyệt từ cuối lên trên, tìm kiếm phần tử đứng sau phần từ đầu tiên nhỏ hơn phần tử được thêm vào
    private Node Find(Data data)
    {
        Node curNode = rear;

        while (curNode.Before != null && curNode.Before.Data.Line >= data.Line){
            curNode = curNode.Before;
        }

        return curNode;
    }
    public void Insert(Data data)
    {
        Node newNode = new Node(data);
    
        if (head == null)
            head = rear = newNode;
        else
        {
            //Nếu phần tử cuối cùng có giá trị nhỏ hơn phần tử thêm vào 
            //thì chèn vào sau phần tử cuối cùng để đảm bảo không bỏ sót phần tử cuối khi duyệt
            if (data.Line > rear.Data.Line)
            {
                newNode.Before = rear;
                rear.After = newNode;
                rear = newNode;
            }
            else
            {
                //tìm phần từ đứng sau phần tử đầu tiên nhỏ hơn phần tử thêm
                Node afterNode = Find(data);

                newNode.Before = afterNode.Before;
                newNode.After = afterNode;
                //Nếu afterNode là phần tử đầu (head - tức là ở trước không có phần tử nào)
                //thì cập nhật head
                if(afterNode.Before == null)
                    head = newNode;
                else
                //Nếu không thì nối phần tử đứng trước vòa newNode
                    afterNode.Before.After = newNode;

                afterNode.Before = newNode;
            }
        }
    }
}
// Hàng đợi hai đầu ra kết thừa queue cơ bản dùng để lưu trữ thẻ mở
public class MyInputRestrictedQueue : MyQueue<Data>{
    // Duyệt từ cuối tìm kiếm phần tử = key
    public int Search(string key){
        Node curNode = rear;
        int i=0;
        while(curNode != null){
            if(curNode.Data.Content == key){
                //trả về i để biết phần tử này nằm ở thứ tự thứ mấy từ dưới đếm lên(bắt đầu là 0)
                return i;
            }
            i++;
            curNode = curNode.Before;
        }
        //trả về -1 nếu tìm không thấy
        return -1;
    }
    //Tương tự như Dequeue
    public Data DequeueEnd()
    {
        if (head == null)
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
    //Những tag ngoại lệ không cần tag đóng
    private string[] voidTags = {
        "!DOCTYPE","!--","br", "hr", "img", "input", "link", "base", "meta", "param", "area", "embed", "col", "track", "source"
    };
    //đường dẫn file
    private string filePath;
    //List lỗi và kết quả để xuất ra
    public List<Data> Errors;
    public List<string> Results;
    public HTMLChecker(string filePath)
    {
        this.filePath = filePath;
    }
    //bắt đầu thực hiện check
    public void Start()
    {
        Errors = new List<Data>();
        Results = new List<string>();
        checkHTML();
    }
    //hàm đọc file
    private string getDataFromFile()
    {
        try{
            return File.ReadAllText(filePath);
        }
        catch{
            throw new Exception("Lỗi nhận file");
        }
    }
    //hàm lấy tag name
    private string getTagName(string tag, bool openTag=false){
        if(openTag)
            return tag.Substring(0,tag.Length).Trim().Split(' ')[0];
        else
            return tag.Substring(0,tag.Length).Replace(" ","");
    }
    //hàm báo lỗi thiếu thẻ
    private void noteError(Data tag,MyPriorityQueue errors,bool isOpenTag = false){
        if(isOpenTag)
            errors.Insert(
            new Data($"Open tag <{tag.Content}> has not been closed",tag.Line));
        else
            errors.Insert(
            new Data($"Close tag </{tag.Content}> has found but not found open tag",tag.Line));
    }
    //hàm bái lỗi có dấu cách
    private void noteError(string tagContent,int line,MyPriorityQueue errors,bool isOpenTag = false){
        if(isOpenTag)
            errors.Insert(new Data($"Found <{tagContent}> has 'SPACE' after '<'",line));
        else
            errors.Insert(
                new Data($"Found </{tagContent}> is End Tag but has 'SPACE'", line));
    }
    // hàm để lấy thẻ. Nếu thả này không có đống thì i = n
    private string getTag(char[] cs, ref int i, int length){
        string res="";
        while(i<length && cs[i]!='>'){
            res+=cs[i];
            i++;
        }
        return res;
    }
    private void checkHTML(){
        string html = getDataFromFile();
        //bỏ comment
        html = Regex.Replace(html,@"<!--([\s\S]*?)-->","");
        //tách từng dòng
        string[] lines = html.Split("\n");
        MyQueue<string> results = new MyQueue<string>();
        MyInputRestrictedQueue openTags = new MyInputRestrictedQueue();
        MyPriorityQueue errors = new MyPriorityQueue();
        //duyệt qua từng dòng
        for(int i =0;i<lines.Length;i++){
            char[] cs = lines[i].ToCharArray();
            int j=0,n=cs.Length;

            while(j<n){
                // Nếu bắt gặp mở thẻ
                if(cs[j]=='<'){
                    //j++ này là để bỏ qua 1 ký tự
                    j++;
                    //nếu là thẻ đóng
                    if(cs[j]=='/'){
                        j++;
                        //lấy thẻ
                        string tagContent = getTag(cs, ref j, n);
                        // nếu không đóng thì đưa vào kết quả
                        if(j == n)
                            results.Enqueue("</"+tagContent);
                        else {
                            //thẻ đóng không được chứ dấu cách
                            if (tagContent.Contains(' '))
                            {
                                tagContent = getTagName(tagContent);
                                noteError(tagContent,i+1,errors);
                            }
                            else
                                tagContent = getTagName(tagContent);
                            //kiểm tra xem có thẻ mở tương ứng không và nằm ở vị trí nào trong queue
                            int thrower = openTags.Search(tagContent);
                            if (thrower != -1)
                            {
                                //lần lượt lấy các thẻ mở ra theo thứ tự từ dưới lên
                                while(thrower >0 )
                                {
                                    noteError(openTags.DequeueEnd(), errors,true);
                                    thrower--;
                                }
                                openTags.DequeueEnd();
                            }
                            else
                                {noteError(new Data(tagContent,i+1),errors);}
                        }
                    }
                    else
                    {
                        string tagContent = getTag(cs, ref j, n);
                        if(j == n)
                            results.Enqueue("<"+tagContent);
                        else{
                            //thẻ mở không được bắt đầu bằng dấu cách
                            if (tagContent.StartsWith(' '))
                            {
                                tagContent = getTagName(tagContent, true);
                                noteError(tagContent,i+1,errors,true);
                            }
                            else
                                tagContent = getTagName(tagContent, true);

                            //đưa vào hàng đợi nếu không phải là trường hợp đặc biệt
                            if (!voidTags.Contains(tagContent))
                                openTags.Enqueue(new Data(tagContent, i + 1));
                        }
                    }
                    j++;
                }
                string content = "";
                //lấy các chữ không phải thẻ cho đến khi gặp dấu mở thẻ
                while(j<n && cs[j]!='<'){
                    content+=cs[j];
                    j++;
                }

                if(!string.IsNullOrWhiteSpace(content))
                    results.Enqueue(content.Trim());
            }
        }
        //sau khi duyệt hết nếu còn thẻ chưa đóng thì báo lỗi
        while(!openTags.IsEmpty()){
            noteError(openTags.DequeueEnd(),errors,true);
        }
        // đưa thành kết quả
        Errors = errors.ToList();
        Results = results.ToList();
    }
}
