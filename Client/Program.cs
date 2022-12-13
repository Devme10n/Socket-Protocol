using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;


namespace AClient
{
    public class Client
    {
        private readonly static int BufferSize = 4096;

        public static void Main()
        {
            try
            {
                new Client().Init();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("Press any key to exit the program.");
            Console.ReadKey();
        }


        private Socket clientSocket;
        public Socket ClientSocket
        {
            get => clientSocket;
            set => clientSocket = value;
        }
        private readonly IPEndPoint EndPoint = new(IPAddress.Parse("127.0.0.1"), 5001);

        public Client()
        {
            ClientSocket = new(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp
            );
        }

        void Init()
        {
            ClientSocket.Connect(EndPoint);
            Console.WriteLine($"Server connected.");

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(Received);
            ClientSocket.ReceiveAsync(args);

            Send();
        }


        void Received(object? sender, SocketAsyncEventArgs e)
        {
            try
            {
                byte[] data = new byte[BufferSize];
                Socket server = (Socket)sender!;
                int n = server.Receive(data);

                string str = Encoding.Unicode.GetString(data);
                str = str.Replace("\0", "");
                Console.WriteLine("수신:" + str);


                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += new EventHandler<SocketAsyncEventArgs>(Received);
                ClientSocket.ReceiveAsync(args);
            }
            catch (Exception)
            {
                Console.WriteLine($"Server disconnected.");
                ClientSocket.Close();
            }
        }

        void Send()
        {
            byte[] dataID;
            Console.WriteLine("ID를 입력하세요");
            string nameID = Console.ReadLine()!;
            //
            string message = "ID:" + nameID + ":";
            dataID = Encoding.Unicode.GetBytes(message);
            clientSocket.Send(dataID);
            //

            Console.WriteLine("FP: 파티 등록, IP: 파티 참여, CP: 파티원 확인, DP: 파티 삭제,\n QP: 파티 탈퇴, Coupon: 쿠폰 전송, BR: 전체 메시지, TO: 개인 메시지");
            do
            {
                byte[] data;
                string msg = Console.ReadLine()!;
                string[] tokens = msg.Split(':');
                string m;
                if (tokens[0].Equals("BR"))
                {
                    m = "BR:" + nameID + ":" + tokens[1] + ":";
                    data = Encoding.Unicode.GetBytes(m);
                    Console.WriteLine("[전체전송]{0}", tokens[1]);
                    try { ClientSocket.Send(data); } catch { }
                }
                else if (tokens[0].Equals("Coupon"))
                {
                    SendFile(tokens[1]);
                }
                else if (tokens[0].Equals("TO"))
                {
                    m = "TO:" + nameID + ":" + tokens[1] + ":" + tokens[2] + ":";
                    data = Encoding.Unicode.GetBytes(m);
                    Console.WriteLine("[{0}에게 전송]:{1}", tokens[1], tokens[2]);
                    try { ClientSocket.Send(data); } catch { }
                }
                else if (tokens[0].Equals("FP"))
                {
                    m = "FP:" + nameID + ":" + tokens[1] + ":" + tokens[2] + ":";
                    data = Encoding.Unicode.GetBytes(m);
                    // 파티이름으로 배열 생성 + 입력받은 최대인원 수로 배열의 크기 설정
                    Console.WriteLine("[{0} 파티 생성]: {1}명", tokens[1], tokens[2]);
                    try { ClientSocket.Send(data); } catch { }
                }
                else if (tokens[0].Equals("IP"))
                {
                    m = "IP:" + nameID + ":" + tokens[1] + ":";
                    data = Encoding.Unicode.GetBytes(m);
                    Console.WriteLine("[{0}파티 참가 시도 중...]", tokens[1]);
                    try { ClientSocket.Send(data); } catch { }
                }
                else if (tokens[0].Equals("CP"))
                {
                    m = "CP:" + nameID + ":" + tokens[1] + ":";
                    data = Encoding.Unicode.GetBytes(m);
                    // 배열 내 사람 수 세기
                    Console.WriteLine("[{0}파티 참가 인원]:", tokens[1]);
                    try { ClientSocket.Send(data); } catch { }
                }
                else if (tokens[0].Equals("DP"))
                {
                    m = "DP:" + nameID + ":" + tokens[1] + ":";
                    data = Encoding.Unicode.GetBytes(m);
                    // 배열 초기화
                    Console.WriteLine("[{0}파티 삭제 시도 중...]", tokens[1]);
                    try { ClientSocket.Send(data); } catch { }
                }
                else if (tokens[0].Equals("QP"))
                {
                    m = "QP:" + nameID + ":" + tokens[1] + ":";
                    data = Encoding.Unicode.GetBytes(m);
                    // 배열에서 nameID 찾아서 삭제
                    Console.WriteLine("[{0}파티 탈퇴 성공]", tokens[1]);
                    try { ClientSocket.Send(data); } catch { }
                }
            } while (true);
        }

        void SendFile(string filename)
        {
            FileInfo fi = new FileInfo(filename);
            string fileLength = fi.Length.ToString();

            byte[] bDts = Encoding.Unicode.GetBytes
                ("Coupon:" + filename + ":" + fileLength + ":");
            clientSocket.Send(bDts);

            byte[] bDtsRx = new byte[4096];
            FileStream fs = new FileStream(filename,
                FileMode.Open, FileAccess.Read,
                FileShare.None);
            long received = 0;
            while (received < fi.Length)
            {
                received += fs.Read(bDtsRx, 0, 4096);
                clientSocket.Send(bDtsRx);
                Array.Clear(bDtsRx);
            }
            fs.Close();

            Console.WriteLine("파일 송신 종료");
        }

    }
}
