using System.Net.Sockets;
using System.Net;
using System.Text;

namespace AServer
{
    public class Server
    {
        private readonly static int BufferSize = 4096;

        public static void Main()
        {
            try
            {
                new Server().Init();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        // 전역변수
        private string[] party1;
        public string[] Party1
        {
            get => party1;
            set => party1 = value;
        }

        private Dictionary<string, Socket> connectedClients = new();
        public Dictionary<string, Socket> ConnectedClients
        {
            get => connectedClients;
            set => connectedClients = value;
        }

        private Socket ServerSocket;

        private readonly IPEndPoint EndPoint = new(IPAddress.Parse("127.0.0.1"), 5001);

        int clientNum;
        Server()
        {
            ServerSocket = new(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp
            );
            clientNum = 0;
        }

        void Init()
        {
            ServerSocket.Bind(EndPoint);
            ServerSocket.Listen(100);
            Console.WriteLine("Waiting connection request.");

            Accept();

        }


        void Accept()
        {
            do
            {
                Socket client = ServerSocket.Accept();


                Console.WriteLine($"Client accepted: {client.RemoteEndPoint}.");

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += new EventHandler<SocketAsyncEventArgs>(Received);
                client.ReceiveAsync(args);

            } while (true);
        }

        void Disconnected(Socket client)
        {
            Console.WriteLine($"Client disconnected: {client.RemoteEndPoint}.");
            foreach (KeyValuePair<string, Socket> clients in connectedClients)
            {
                if (clients.Value == client)
                {
                    ConnectedClients.Remove(clients.Key);
                    clientNum--;
                }
            }
            client.Disconnect(false);
            client.Close();
        }

        void Received(object? sender, SocketAsyncEventArgs e)
        {
            Socket client = (Socket)sender!;
            byte[] data = new byte[BufferSize];
            try
            {
                int n = client.Receive(data);
                if (n > 0)
                {

                    //
                    MessageProc(client, data);

                    SocketAsyncEventArgs argsR = new SocketAsyncEventArgs();
                    argsR.Completed += new EventHandler<SocketAsyncEventArgs>(Received);
                    client.ReceiveAsync(argsR);
                }
                else { throw new Exception(); }
            }
            catch (Exception)
            {
                Disconnected(client);
            }
        }

        void MessageProc(Socket s, byte[] bytes)
        {
            string m = Encoding.Unicode.GetString(bytes);
            //
            string[] tokens = m.Split(':');
            string fromID;
            string toID;
            string code = tokens[0];
            string partyName;
            int maxMember;

            // ID: 사용자 id
            if (code.Equals("ID"))
            {
                clientNum++;
                fromID = tokens[1].Trim();
                Console.WriteLine("[접속{0}]ID:{1},{2}",
                    clientNum, fromID, s.RemoteEndPoint);
                //
                connectedClients.Add(fromID, s);
                s.Send(Encoding.Unicode.GetBytes("ID_REG_Success:"));
                Broadcast(s, m);
            }
            else if (tokens[0].Equals("BR"))
            {
                fromID = tokens[1].Trim();
                string msg = tokens[2].Trim();
                Console.WriteLine("[전체]{0}:{1}", fromID, msg);
                //
                Broadcast(s, m);
                s.Send(Encoding.Unicode.GetBytes("BR_Success:"));
            }
            else if (code.Equals("TO"))
            {
                fromID = tokens[1].Trim();
                toID = tokens[2].Trim();
                string msg = tokens[3];
                string rMsg = "[From:" + fromID + "][TO:" + toID + "]" + msg;
                Console.WriteLine(rMsg);

                //
                SendTo(toID, m);
                s.Send(Encoding.Unicode.GetBytes("To_Success:"));
            }

            //////////////////////////////////////////////////////////////////////////////

            else if (code.Equals("Coupon"))
            {
                ReceiveFile(s, m);

                // 파티원들에게 쿠폰 알림
                string rMsg = "쿠폰이 발급되었습니다.";

                s.Send(Encoding.Unicode.GetBytes("Coupon_Success:"));
                Broadcast(s, rMsg);

            }
            else if (code.Equals("FP"))
            {
                fromID = tokens[1].Trim();
                partyName = tokens[2].Trim();
                maxMember = int.Parse(tokens[3].Trim());
                party1 = new string[maxMember + 1];
                party1[0] = partyName;
                party1[1] = fromID;

                int partyMaxMemeber = party1.Length - 1;
                string rMsg = "[User:" + party1[1] + "][Make Party:" + party1[0] + "]" + "[max member:" + partyMaxMemeber + "]";

                s.Send(Encoding.Unicode.GetBytes("FP_Success:"));
                Broadcast(s, rMsg);
            }
            else if (code.Equals("IP"))
            {
                fromID = tokens[1].Trim();
                partyName = tokens[2].Trim();

                for (int i = 1; i < party1.Length; i++)
                {
                    if (party1[i] == null)
                    {
                        party1[i] = fromID;
                        string rMsg = "[User:" + party1[i] + "][IP:" + party1[0] + "]";
                        s.Send(Encoding.Unicode.GetBytes("IP_Success:"));
                        break;
                    }
                    else
                    {
                        string rMsg = "IP_Fail:";
                        s.Send(Encoding.Unicode.GetBytes(rMsg));
                    }
                }
                // 파티원에게만 결과 SendTo
                if (party1[0] == partyName)
                {
                    var memberList = new List<string>();
                    for (int i = 1; i < party1.Length; i++)
                    {
                        if (party1[i] != null)
                        {
                            memberList.Add(party1[i]);
                        }
                    }
                    var partyMember = memberList.ToArray();
                    for (int j = 0; j < partyMember.Length; j++)
                    {
                        SendTo(partyMember[j], m);
                    }
                }
            }
            else if (code.Equals("CP"))
            {
                fromID = tokens[1].Trim();
                partyName = tokens[2].Trim();
                try
                {
                    if (party1[0] == partyName)
                    {
                        var memberList = new List<string>();
                        for (int i = 1; i < party1.Length; i++)
                        {
                            if (party1[i] != null)
                            {
                                memberList.Add(party1[i]);
                            }
                        }
                        var partyMember = memberList.ToArray();
                        string rMsg = string.Join(",", partyMember) + "\n[총 " + partyMember.Length + "명]:";
                        Console.WriteLine(rMsg);
                        s.Send(Encoding.Unicode.GetBytes(rMsg));
                    }
                }
                catch { Console.WriteLine(partyName + "은 없는 파티입니다."); }
            }
            else if (code.Equals("DP"))
            {
                fromID = tokens[1].Trim();
                partyName = tokens[2].Trim();
                string rMsg = "[User:" + fromID + "][Delete Party:" + partyName + "]";
                var delMemberIndex = Array.IndexOf(party1, fromID);
                Array.Clear(party1, 0, party1.Length); // 배열의 내용을 지웁니다. -> 파티 삭제
                Console.WriteLine(rMsg);

                s.Send(Encoding.Unicode.GetBytes("DP_Success:"));
                Broadcast(s, m);
            }
            else if (code.Equals("QP"))
            {
                fromID = tokens[1].Trim();
                partyName = tokens[2].Trim();
                string rMsg = "[User:" + fromID + "][Quit Party:" + partyName + "]";
                var delMemberIndex = Array.IndexOf(party1, fromID);
                party1[delMemberIndex] = null;
                Console.WriteLine(rMsg);

                s.Send(Encoding.Unicode.GetBytes("QP_Success:"));
                Broadcast(s, m);
            }
            else
            {
                Broadcast(s, m);
            }
        }

        void ReceiveFile(Socket s, string m)
        {
            // 다운로드 디렉토리
            string output_path = @"FileDown\";
            if (!Directory.Exists(output_path))
            {
                Directory.CreateDirectory(output_path);
            }
            string[] tokens = m.Split(':');
            string fileName = tokens[1].Trim();
            long fileLength = Convert.ToInt64(tokens[2].Trim());
            string FileDest = output_path + fileName;

            long flen = 0;
            FileStream fs = new FileStream(FileDest,
                                FileMode.OpenOrCreate,
                            FileAccess.Write, FileShare.None);
            while (flen < fileLength)
            {
                byte[] fdata = new byte[4096];
                int r = s.Receive(fdata, 0, 4096,
                    SocketFlags.None);
                fs.Write(fdata, 0, r);
                flen += r;
            }
            fs.Close();

        }
        void SendTo(string id, string msg)
        {
            Socket socket;
            byte[] bytes = Encoding.Unicode.GetBytes(msg);
            if (connectedClients.ContainsKey(id))
            {
                connectedClients.TryGetValue(id, out socket!);
                try { socket.Send(bytes); } catch { }
            }
        }
        void Broadcast(Socket s, string msg)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(msg);
            foreach (KeyValuePair<string, Socket> client in connectedClients.ToArray())
            {
                try
                {
                    if (s != client.Value)
                        client.Value.Send(bytes);
                }
                catch (Exception)
                {
                    Disconnected(client.Value);
                }
            }
        }

    }
}