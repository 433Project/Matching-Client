using System;
using System.IO;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.ClientEngine;
//using SuperSocket.SocketBase.Protocol;
using System.Net;
using System.Threading;

namespace MatchingClient
{
    class Program
    {
        //***Global Variables***
        //Network Variables
        static EasyClient g_client = new EasyClient();
        static IPAddress ip;
        static int port;

        //Define Special Values
        enum GameState
        {
            UNINITIATED                 = 0,
            INITIATED                   = 1,
            MATCHING                    = 2,
            JOIN_ROOM                   = 3,
            GAME_IN_PROGRESS            = 4,
            GAME_FINISHED               = 5
        }

        static GameState gameState = GameState.UNINITIATED;

        static void Main(string[] args)
        {
            //Read config file. . .
            string setupFileName = "config.txt";
            var pathh = Path.Combine(Directory.GetCurrentDirectory(), setupFileName);    //Get the location of the config file

            //Be sure to catch any file reading errors
            try
            {
                args = File.ReadAllLines(pathh);                                         //Read the config file
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("\nFileNotFoundException during " + setupFileName + " reading attempt.\n");
                Console.WriteLine(e.Source + " : " + e.Message);
                Console.WriteLine("\nPlease ensure that " + setupFileName + " is contained within the application's directory.");
                Console.ReadLine();
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine("\nException during " + setupFileName + " reading attempt.\n");
                Console.WriteLine(e.Source + " : " + e.Message);
                Console.WriteLine("\nPlease ensure that " + setupFileName + " is contained within the application's directory.");
                Console.ReadLine();
                return;
            }

            //Determine config file parameter validity. . .
            if (args.Length >= 2)
            {
                if (!IPAddress.TryParse(args[0], out ip) || !Int32.TryParse(args[1], out port))
                {
                    Console.WriteLine("\nUnable to parse data in " + setupFileName + ".\n");
                    Console.WriteLine("\nPlease ensure that " + setupFileName + " contains valid entries.");
                    Console.ReadLine();
                    return;
                }
            } else
            {
                Console.WriteLine("\nVital setup parameters in " + setupFileName + " missing.\n");
                Console.WriteLine("\nPlease ensure that " + setupFileName + " contains all vital entries.");
                Console.ReadLine();
                return;
            }

            Console.Clear();
            //Start Application. . .
            Console.WriteLine("############################################################");
            Console.WriteLine("####################      MATCHING      ####################");
            Console.WriteLine("####################       CLIENT       ####################");
            Console.WriteLine("############################################################");

            Console.WriteLine("\nWelcome to the Matching Client!");
            Console.WriteLine("Please enter anything to connect. . .");
            Console.Read();
            ClearBottomOfScreen();

            //***Variables***
            //Control Variables
            ConsoleKeyInfo inputKey;

            // Initialize the client with the receive filter and request handler
            g_client.Initialize(new ClientReceiveFilter(6), (request) => {
                // handle the received request
                Console.WriteLine(request.Key);
            });

            if (!g_client.IsConnected)
            {
                ConnectToServer();
                Console.SetCursorPosition(0, 6);
                Console.WriteLine("          ########################################  ");
                Console.WriteLine("          ### Successfully Connected to Server ###  ");
                Console.WriteLine("          ########################################  ");
            }
            gameState = GameState.INITIATED;
            Thread.Sleep(800);

            //Initiate game window
            GameStateProcessor();

            //While the program is running. . . 
            while (true)
            {
                inputKey = Console.ReadKey();
                if (inputKey.KeyChar == 'q' || inputKey.KeyChar == 'Q')
                {
                    break;
                } else
                {
                    ChangeGameState();
                    GameStateProcessor();
                }
                //Confirm Connected Status
                if (!g_client.IsConnected)
                {
                    ConnectToServer();
                    Console.SetCursorPosition(0, 6);
                    Console.WriteLine("          ########################################  ");
                    Console.WriteLine("          ### Successfully Connected to Server ###  ");
                    Console.WriteLine("          ########################################  ");
                    Thread.Sleep(800);
                    GameStateProcessor();
                }
                //Initiate Game State Work
            }

            ClearBottomOfScreen();
            Console.SetCursorPosition(0, 5);
            Console.WriteLine("Thank you for playing! Press any key to quit!");
            Console.ReadKey();
        }

        /// <summary>
        /// This method changes the state of the game based on previous game state and input.
        /// </summary>
        static void ChangeGameState()
        {
            //Currently we simply change the game state to cycle
            if ((int)gameState >= 5)
            {
                gameState = GameState.INITIATED;
            } else
            {
                gameState++;
            }
        }

        /// <summary>
        /// This method determines which state the game is in and processes accordingly.
        /// </summary>
        static void GameStateProcessor ()
        {
            ClearBottomOfScreen();
            Console.SetCursorPosition(0, 5);
            switch (gameState) {
                case GameState.INITIATED:
                    Console.WriteLine("\t---Matching Server Lobby---");
                    Console.WriteLine(" Character ID: \t\tMatches Played: ");
                    Console.WriteLine(" Wins: \t\t\tLoses: ");
                    Console.WriteLine("\nPress 'q' to quit or any other key to play a match.");
                    break;
                case GameState.MATCHING:
                    Console.WriteLine("\t---SENDING MATCH REQUEST---");
                    Console.WriteLine(" Character ID: \t\tMatches Played: ");
                    Console.WriteLine(" Wins: \t\t\tLoses: ");
                    Console.WriteLine("\nPress 'q' to quit or any other key to play a match.");
                    break;
                case GameState.JOIN_ROOM:
                    Console.WriteLine("\t---JOINING ROOM---");
                    Console.WriteLine(" Character ID: \t\tMatches Played: ");
                    Console.WriteLine(" Wins: \t\t\tLoses: ");
                    Console.WriteLine("\nPress 'q' to quit or any other key to play a match.");
                    break;
                case GameState.GAME_IN_PROGRESS:
                    Console.WriteLine("\t---GAME IN PROGRESS---");
                    Console.WriteLine(" Character ID: \t\tMatches Played: ");
                    Console.WriteLine(" Wins: \t\t\tLoses: ");
                    Console.WriteLine("\nPress 'q' to quit or any other key to play a match.");
                    break;
                case GameState.GAME_FINISHED:
                    Console.WriteLine("\t---GAME FINISHED---");
                    Console.WriteLine(" Character ID: \t\tMatches Played: ");
                    Console.WriteLine(" Wins: \t\t\tLoses: ");
                    Console.WriteLine("\nPress 'q' to quit or any other key to play a match.");
                    break;
            }
        }

        /// <summary>
        /// This method loops until the connection to the server is established.
        /// </summary>
        static void ConnectToServer()
        {
            int loadingValue = 4;
            while (!g_client.IsConnected)
            {
                //connected = await client.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000));
                g_client.BeginConnect(new IPEndPoint(ip, port));
                //Print loading
                if (loadingValue > 3)
                {
                    loadingValue = 0;
                    Console.SetCursorPosition(0, 6);
                    Console.WriteLine("          ########################################  ");
                    Console.WriteLine("          ### Connecting to server.            ###  ");
                    Console.WriteLine("          ########################################  ");
                }
                else
                {
                    Console.SetCursorPosition(36 + (2 * loadingValue++), 7);
                    Console.Write(". ");
                }
                Thread.Sleep(500);
            }
        }

        /// <summary>
        /// This method clears the area of the screen below the title header.
        /// </summary>
        static void ClearBottomOfScreen()
        {
            Console.SetCursorPosition(0, 4);
            Console.WriteLine("                                                                  ");
            Console.WriteLine("                                                                  ");
            Console.WriteLine("                                                                  ");
            Console.WriteLine("                                                                  ");
            Console.WriteLine("                                                                  ");
            Console.WriteLine("                                                                  ");
            Console.WriteLine("                                                                  ");
            Console.WriteLine("                                                                  ");
            Console.WriteLine("                                                                  ");
        }
    }


    class ClientReceiveFilter : FixedHeaderReceiveFilter<PackageInfo<string, byte[]>>
    {
        public ClientReceiveFilter(int headerSize) : base(headerSize)
        {
        }

        protected override int GetBodyLengthFromHeader(IBufferStream bufferStream, int length)
        {
            if (bufferStream.Length <= 4)
                return 0;
            return (int)bufferStream.Length - 4;
        }

        public override PackageInfo<string, byte[]> ResolvePackage(IBufferStream bufferStream)
        {
            string key = bufferStream.ReadString(4, Encoding.UTF8);
            if (bufferStream.Length > 4)
            {
                byte[] body = new byte[bufferStream.Length - 4];
                bufferStream.Skip(4).Read(body, 4, body.Length);
                return new PackageInfo<string, byte[]>(key, body);
            }

            return new PackageInfo<string, byte[]>(key, null);
        }

    }

}
