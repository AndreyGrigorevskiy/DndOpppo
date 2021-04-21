using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections.Generic;

public class ClientObject
{

    private const int MASTER = 1;
    private const int PLAYER = 0;

    private NetworkStream stream;


    public TcpClient client;

    public static List<ClientObject> myConections = new List<ClientObject>();

    private int playerRole = PLAYER;
    private String name = "Noname";
    private bool isNamed = false;
    private bool gotTheRole = false;
    private int maxHp = 10;
    private int curentHp = 10;
    private String[] inventory = new String[5];
    private static bool master = false;



    public ClientObject(TcpClient tcpClient)
    {
        client = tcpClient;
        for(int i = 0; i< inventory.Length; i++)
        {
            inventory[i] = "";
        }
         stream = null;
    }

    public ClientObject(ClientObject clientObject)
    {
        client = clientObject.client;
    }

    public void heal(int count)
    {
        if(count+curentHp>maxHp)
        {
            curentHp = maxHp;
        }
        else
        {
            curentHp += count;
        }
    }

    public void takeGMG(int count)
    {
        curentHp -= count;
    }

    public void take(String item)
    {
        bool empty = true;
        for(int i = 0; i<inventory.Length;i++)
        {
            
            if(String.IsNullOrEmpty(inventory[i]))
            {
                inventory[i] = item;
                empty = false;
                break;
            }
        }
        if(empty)
        {
            inventory[0] = item;
        }
    }

    public void drop(int num)
    {
        inventory[num - 1] = "";
    }

    public void drop(String item)
    {
        for(int i = 0; i< inventory.Length; i++)
        {
            if(String.Equals(inventory[i],item))
            {
                inventory[i] = "";
                break;
            }
        }
    }

    public void setPlayerRole(int role)
    {
        playerRole = role;
        gotTheRole = true;
    }

    public int getPlayerRole()
    {
        return playerRole;
    }

    public void setName(String name)
    {
        this.name = name;
        isNamed = true;
    }

    public String getName()
    {
        return name;
    }
    
    private int dX(int count)
    {
        Random rnd = new Random();

        return rnd.Next(count)+1;
    }



    public void Process()
    {
        Console.OutputEncoding = Encoding.UTF8;
        
        try
        {
            stream = client.GetStream();
            byte[] data = new byte[64]; // буфер для получаемых данных
            while (true)
            {
                StringBuilder builder = new StringBuilder();
                if (!isNamed)
                {
                    data = Encoding.UTF8.GetBytes("Enter name:");
                    stream.Write(data, 0, data.Length);
                }
                else if (!gotTheRole)
                {
                    data = Encoding.UTF8.GetBytes("Choose youre desteny. Are you the master? (Y/N) \n");
                    stream.Write(data, 0, data.Length);

                }

                // получаем сообщение
                
                int bytes = 0;
                do
                {
                    bytes = stream.Read(data, 0, data.Length);
                    builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);


                String checkMess = builder.ToString();

                if (checkMess.IndexOf(":dst:")!=-1)
                {
                    Console.WriteLine(checkMess+name + "disconected");
                    
                    break;
                }

                if (checkMess.IndexOf(":msg:")!=-1)
                {
                   checkMess = checkMess.Remove(0, checkMess.IndexOf(":msg:") + 5);
                }
                else
                {
                    continue;
                }


                if (!isNamed)
                {
                    setName(checkMess);
                }
                else if(!gotTheRole)
                {
                    if (checkMess.Equals("Y")) 
                    {
                        setPlayerRole(MASTER);
                        setMaster(true);
                    }
                    else
                    {
                        setPlayerRole(PLAYER);
                    }
                }
                    

                string message = "";

                if (playerRole == MASTER)
                {
                     message = "[" + name + "] : [Master] =>" + checkMess;
                }
                else
                {
                    message = "[" + name + "] : [Player] =>" + checkMess;
                }

                

                if (string.Compare(checkMess, "!d20", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    message += " = " + dX(20);
                }

                if (string.Compare(checkMess, "!d16", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    message += " = " + dX(16);
                }

                if (string.Compare(checkMess, "!d12", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    message += " = " + dX(12);
                }

                if (string.Compare(checkMess, "!d10", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    message += " = " + dX(10);
                }

                if (string.Compare(checkMess, "!d8", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    message += " = " + dX(8);
                }

                if (string.Compare(checkMess, "!d6", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    message += " = " + dX(6);
                }

                if (string.Compare(checkMess, "!d4", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    message += " = " + dX(4);
                }

                if (checkMess.Contains("!heal"))
                {
                    checkMess = checkMess.Remove(0, 5);
                    try {
                        heal(Convert.ToInt32(checkMess));
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    
                    message += "current HP = " + curentHp;
                }

                if (checkMess.Contains("!dmg"))
                {
                    checkMess = checkMess.Remove(0, 4);
                    takeGMG(Convert.ToInt32(checkMess));
                    message += "current HP = " + curentHp;
                }

                if (checkMess.Contains("!take"))
                {
                    checkMess = checkMess.Remove(0, 5);
                    take(checkMess);
                }

                if (string.Compare(checkMess, "!drop", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    checkMess = checkMess.Remove(0, 5);
                    drop(checkMess);
                }

                if (string.Compare(checkMess, "!stat", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    message += "hp("+curentHp+"/"+maxHp+")";
                    message += "  inventory : ";
                    for(int i = 0;  i<inventory.Length; i++)
                    {
                        message += " -" + i + " " + inventory[i] + "- ";
                    }
                }



                Console.WriteLine(message);
                // отправляем обратно сообщение в верхнем регистре
                message += "\n";
                data = Encoding.UTF8.GetBytes(message);
                if (!isNamed)
                {
                    stream.Write(data, 0, data.Length);
                }
                else if (!gotTheRole)
                {
                    stream.Write(data, 0, data.Length);

                }
                else
                {
                    sendALL(data);
                }
                
                
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            if (stream != null)
                stream.Close();
            if (client != null)
                client.Close();
        }
    }

    public void sendALL(byte[] data)
    {
        foreach(ClientObject client in myConections)
        {
            client.stream.Write(data, 0, data.Length);
        }
    }

    public void sendByName(byte[] data,string name)
    {
        foreach (ClientObject client in myConections)
        {
            if(client.getName().Equals(name))
            {
                client.stream.Write(data, 0, data.Length);
            }
        }
    }

    public void setMaster(bool role)
    {
        master = role;
    }

    class Program
    {

        const int port = 9090;
        static String host = System.Net.Dns.GetHostName();
        static String ip = IPAddress.Any.ToString();
        static TcpListener listener;
        

        

        static void Main(string[] args)
        {            
            
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                Console.WriteLine(ip);
                Console.WriteLine("Ожидание подключений...");
                
                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    myConections.Add(new ClientObject(client));
                    //ClientObject clientObject = new ClientObject(client);
                    
                    if (master)
                    {
                        myConections[myConections.Count - 1].setPlayerRole(PLAYER);
                    }

                    // создаем новый поток для обслуживания нового клиента
                    
                    Thread clientThread = new Thread(new ThreadStart(myConections[myConections.Count-1].Process));
                    Console.WriteLine(myConections[myConections.Count-1].name);
                    clientThread.Start();
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (listener != null)
                    listener.Stop();
            }
        }
    }
}