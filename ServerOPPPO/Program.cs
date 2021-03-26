using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;






public class ClientObject
{

    private const int MASTER = 1;
    private const int PLAYER = 0;


    public TcpClient client;


    private int playerRole = PLAYER;
    private String name = "Noname";
    private bool isNamed = false;
    private bool gotTheRole = false;


    public ClientObject(TcpClient tcpClient)
    {
        client = tcpClient;
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
        NetworkStream stream = null;
        try
        {
            stream = client.GetStream();
            byte[] data = new byte[64]; // буфер для получаемых данных
            while (true)
            {
                // получаем сообщение
                StringBuilder builder = new StringBuilder();
                int bytes = 0;
                do
                {
                    bytes = stream.Read(data, 0, data.Length);
                    builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);

                if(!isNamed)
                {
                    data = Encoding.UTF8.GetBytes("Enter name:");
                    stream.Write(data, 0, data.Length);
                    setName(builder.ToString());
                }
                else if(!gotTheRole)
                {
                    //test
                    data = Encoding.UTF8.GetBytes("Choose youre desteny. Are you the master? (Y/N)");
                    stream.Write(data, 0, data.Length);

                    if (builder.ToString().Equals("Y"))
                    {
                        setPlayerRole(MASTER);
                    }
                    else
                    {
                        setPlayerRole(PLAYER);
                    }
                    
                }

                string message = "";

                if (this.playerRole == MASTER)
                {
                     message = "[" + name + "] : [Master] =>" + builder.ToString();
                }
                else
                {
                    message = "[" + name + "] : [Player] =>" + builder.ToString();
                }

                if (string.Compare(builder.ToString(), "!d20", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    message += " = " + dX(20);
                }

                if (string.Compare(builder.ToString(), "!d16", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    message += " = " + dX(16);
                }

                if (string.Compare(builder.ToString(), "!d12", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    message += " = " + dX(12);
                }

                if (string.Compare(builder.ToString(), "!d10", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    message += " = " + dX(10);
                }

                if (string.Compare(builder.ToString(), "!d8", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    message += " = " + dX(8);
                }

                if (string.Compare(builder.ToString(), "!d6", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    message += " = " + dX(6);
                }

                if (string.Compare(builder.ToString(), "!d4", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    message += " = " + dX(4);
                }

                Console.WriteLine(message);
                // отправляем обратно сообщение в верхнем регистре
                message += "\n";
                data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
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

    class Program
    {
        const int port = 9090;
        static TcpListener listener;
        static void Main(string[] args)
        {
            try
            {
                listener = new TcpListener(IPAddress.Parse("10.3.224.240"), port);
                listener.Start();
                Console.WriteLine("Ожидание подключений...");

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    ClientObject clientObject = new ClientObject(client);
                    // создаем новый поток для обслуживания нового клиента
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
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