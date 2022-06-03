using System;

namespace TicTacToe
{
    class Program
    {
        static void Main()
        {
            // Initializes screens/game flows, then sets the window title before entering into the main loop
            TicTacToeGame game = new();
            Stats gameStats = new();
            Options selectedOption;

            Menu.SetWindowTitle();

            // Keeps player on title menu until they explicitly ask to leave
            do
            {
                Menu.Display();
                selectedOption = Menu.UserChoice();

                if (selectedOption == Options.Human) game.RunHumanGame(gameStats);
                else if (selectedOption == Options.AI) game.RunAIGame(gameStats);
                else if (selectedOption == Options.Stats) gameStats.StatsScreen();
                else if (selectedOption == Options.Settings) Settings.SettingsScreen();

            } while (selectedOption != Options.Quit);


        }

        public class TicTacToeGame
        {
            public int _roundTracker = 0;
            public int _totalRounds;
            public int _roundsNeededToWin;

            public void RunHumanGame(Stats gameStats)
            {
                // Initializing all needed instances of all objects at this phase
                Player player1 = new(Symbol.X);
                Player player2 = new(Symbol.O);

                Console.Write("\nPlease choose a mode:\n" +
                                  " 1. Single game\n" +
                                  " 2. Best of 3\n" +
                                  " 3. Best of 5\n");
                _totalRounds = Convert.ToInt32(Console.ReadLine()) switch
                {
                    1 => 1,
                    2 => 3,
                    3 => 5,
                    _ => 1
                };

                // Determine the rounds needed to win following player input
                _roundsNeededToWin = _totalRounds / 2 + 1;

                do
                {
                    // Placed at the top to stop unnecessary overcounting
                    _roundTracker++;

                    // Block to output helpful info to the players
                    Console.Write($"\nRound wins needed to win the game: {_totalRounds / 2 + 1}\n" +
                                  $"Player 1 wins: {player1.NumberWins}\n" +
                                  $"Player 2 wins: {player2.NumberWins}\n");

                    // Prints the round number and waits to proceed until interaction
                    Console.WriteLine($"\nRound {_roundTracker}" +
                                      $"\nPress any key to start");
                    Console.ReadKey(true);

                    // Triggers the next round
                    Round newRound = new();
                    newRound.DoRound(player1, player2, gameStats);


                } while (player1.NumberWins < _roundsNeededToWin && player2.NumberWins < _roundsNeededToWin);

                ReportTournamentWinner(player1, player2);
                gameStats.UpdatePlayedStat(StatsProperties.HumanGames);
            }

            public void RunAIGame(Stats gameStats)
            {
                gameStats.UpdatePlayedStat(StatsProperties.AIGames);
            }

            public void ReportTournamentWinner(Player player1, Player player2)
            {
                if (player1.NumberWins == _roundsNeededToWin)
                    Console.WriteLine("\n +----------+" +
                                      "\n | Player 1 |" +
                                      "\n |   Wins!  |" +
                                      "\n +----------+ \n");

                else if (player2.NumberWins == _roundsNeededToWin)
                    Console.WriteLine("\n +----------+" +
                                      "\n | Player 2 |" +
                                      "\n |   Wins!  |" +
                                      "\n +----------+ \n");
            }

        }

        public class GameBoard
        {
            public static (int x, int y) _boardSize = (3, 3);
            public BoardTile[,] TileMatrix { get; init; } = new BoardTile[_boardSize.x, _boardSize.y];

            public GameBoard()
            {
                for (int i = 0; i < _boardSize.y; i++)
                {
                    for (int j = 0; j < _boardSize.x; j++)
                        TileMatrix[i, j] = new BoardTile(i, j);
                }
            }

            public void DrawBoard()
            {
                // Drawing an empty line as a buffer
                Console.WriteLine();

                // Loops through the size of the board in 1 dimension to draw each row
                for (int i = 0; i < _boardSize.x; i++)
                {
                    Console.WriteLine($"  { TileMatrix[i, 0].ConverToChar() } | { TileMatrix[i, 1].ConverToChar()} | { TileMatrix[i, _boardSize.x - 1].ConverToChar() } ");

                    if (i < _boardSize.y - 1)
                        Console.WriteLine(" ---+---+---");
                    else continue;
                }

                Console.WriteLine();
            }

            // Takes the tile selected by a user, and then uses its coordinates to match it up with the correct tile in the Tile Matrix, then sets the correct contents
            public void UpdateTileContent(BoardTile tileToUpdate) => TileMatrix[tileToUpdate.Coordinates.x, tileToUpdate.Coordinates.y].XorO = tileToUpdate.XorO;

        }

        public class BoardTile
        {
            public (int x, int y) Coordinates { get; init; }
            public Symbol XorO { get; set; } = Symbol.Empty;

            // Setting the coordinates outright, but leaving the contents to be determined
            public BoardTile(int x, int y) => Coordinates = (x, y);

            public bool IsEmpty() => XorO == Symbol.Empty;

            public char ConverToChar() => XorO switch { Symbol.X => 'X', Symbol.O => 'O', Symbol.Empty => ' ' };

        }

        public class Round
        {
            public int _turnCount = 0;

            public void DoRound(Player player1, Player player2, Stats gameStats)
            {
                GameBoard board = new();

                board.DrawBoard();

                do
                {
                    // Increments the round number after player 2 completes their turn and game hasn't ended, or on game start
                    _turnCount++;

                    /* Player 1's turn */
                    Console.WriteLine($"Turn: {_turnCount}");
                    Console.WriteLine($"It's Player {player1.PlayerSymbol}'s turn.\n");
                    player1.SetTileChoice(board);
                    board.DrawBoard();

                    // Checks for game over after Player 1's turn and breaks immediately if true
                    if (CheckForRoundOver(board, player1, gameStats)) break;

                    // Increments the turn number after player 1 completes their turn and game doesn't end
                    _turnCount++;

                    /* Player 2's turn */
                    Console.WriteLine($"Turn: {_turnCount}");
                    Console.WriteLine($"It's Player {player2.PlayerSymbol}'s turn.\n");
                    player2.SetTileChoice(board);
                    board.DrawBoard();

                    if (CheckForRoundOver(board, player2, gameStats)) break;

                } while (_turnCount < 10);
            }

            // Checks for the variety of game over states and sets _gameOver accordingly, then writes the outcome
            public static bool CheckForRoundOver(GameBoard board, Player currentPlayer, Stats gameStats)
            {
                if (HorizontalWin(board) || VerticalWin(board) || DiagonalWin(board))
                {
                    currentPlayer.NumberWins++;
                    gameStats.UpdateResultStat(currentPlayer.PlayerSymbol);
                    Console.WriteLine($" **********\n" +
                                      $" {{{currentPlayer.PlayerSymbol}'s win!}}\n" +
                                      $" **********");
                    return true;
                }

                else if (CatBoard(board))
                {
                    Console.WriteLine("The round ends in a draw.");
                    return true;
                }

                return false;
            }

            // There are three possible win conditions, and can only happen after turn 2. Loops through the three possible conditions based on column
            public static bool HorizontalWin(GameBoard board)
            {
                for (int i = 0; i < GameBoard._boardSize.x; i++)
                    if (board.TileMatrix[i, 0].XorO == board.TileMatrix[i, 1].XorO && board.TileMatrix[i, 1].XorO == board.TileMatrix[i, 2].XorO && board.TileMatrix[i, 0].XorO != Symbol.Empty) return true;

                return false;
            }

            // There are three possible win conditions like with horizontal, and can only happen after turn 2
            public static bool VerticalWin(GameBoard board)
            {
                for (int i = 0; i < GameBoard._boardSize.y; i++)
                    if (board.TileMatrix[0, i].XorO == board.TileMatrix[1, i].XorO && board.TileMatrix[1, i].XorO == board.TileMatrix[2, i].XorO && board.TileMatrix[0, i].XorO != Symbol.Empty) return true;

                return false;
            }

            // There are only two diagonal win possibilities, and can only happen after round two
            public static bool DiagonalWin(GameBoard board)
            {
                if (board.TileMatrix[1, 1].XorO == board.TileMatrix[2, 0].XorO && board.TileMatrix[1, 1].XorO == board.TileMatrix[0, 2].XorO && board.TileMatrix[1, 1].XorO != Symbol.Empty) return true;
                else if (board.TileMatrix[1, 1].XorO == board.TileMatrix[0, 0].XorO && board.TileMatrix[1, 1].XorO == board.TileMatrix[2, 2].XorO && board.TileMatrix[1, 1].XorO != Symbol.Empty) return true;
                else return false;
            }

            // Currently only detects a full board
            public static bool CatBoard(GameBoard board)
            {
                foreach (BoardTile tile in board.TileMatrix)
                    if (tile.XorO == Symbol.Empty) return false;
                return true;
            }
        }

        public class Player
        {
            public ConsoleKey TileChoice { get; set; }
            public Symbol PlayerSymbol { get; init; }
            public int NumberWins { get; set; }

            public Player(Symbol symbol) => PlayerSymbol = symbol;

            // Grabs the user choice, finds the associated tile in the matrix, then if that's a valid placement, it updates the tile's contents on the gameboard
            public void SetTileChoice(GameBoard board)
            {
                do
                {
                    // Asks the user for their selected tile, then finds a matching tile in the matrix 
                    TileChoice = QueryUser();

                    BoardTile selectedTile = TileChoice switch
                    {
                        ConsoleKey.NumPad9 => board.TileMatrix[0, 2],
                        ConsoleKey.NumPad8 => board.TileMatrix[0, 1],
                        ConsoleKey.NumPad7 => board.TileMatrix[0, 0],
                        ConsoleKey.NumPad6 => board.TileMatrix[1, 2],
                        ConsoleKey.NumPad5 => board.TileMatrix[1, 1],
                        ConsoleKey.NumPad4 => board.TileMatrix[1, 0],
                        ConsoleKey.NumPad3 => board.TileMatrix[2, 2],
                        ConsoleKey.NumPad2 => board.TileMatrix[2, 1],
                        ConsoleKey.NumPad1 => board.TileMatrix[2, 0]
                    };

                    // If the tile is a valid selection, make the contents the player symbol, then hand tile to the board to update
                    if (ValidTileChoice(selectedTile))
                    {
                        selectedTile.XorO = PlayerSymbol;
                        board.UpdateTileContent(selectedTile);
                        break;
                    }

                    // Instructs user to choose another tile if the one they selected isn't valid
                    else Console.WriteLine("\nThat tile isn't available. Please choose another.\n");
                } while (true);
            }

            public static bool ValidTileChoice(BoardTile TileToCheck) => TileToCheck.XorO == Symbol.Empty;

            public static ConsoleKey QueryUser()
            {
                Console.Write("Please choose a number corresponding to the tile (1-9): ");

                ConsoleKey userInput = Console.ReadKey().Key;
                return userInput;
            }
        }

        // Need a way to make this just a standard Menu class so that it works for both the Title Menu and In-Game menu
        public class Menu
        {
            private static readonly string _windowTitle = "Tic Tac Toe";
            
            // Constructor initializes the list of options using the enum (would like to find a way to make this more universal)
            //public Menu() => _options = Enum.GetNames(typeof());

            public static void Display()
            {
                Console.WriteLine("\n+-------------+\n" +
                                  "| TIC-TAC-TOE |\n" +
                                  "+-------------+\n");

                for (int i = 0; i < Enum.GetNames(typeof(Options)).Length; i++)
                    Console.WriteLine($" {i + 1}: {ConvertOptionToString((Options)i)}");
            }

            public static Options UserChoice()
            {
                while (true)
                {
                    Console.Write("\nSelect an option by entering the corresponding number: ");
                    byte userInput = Convert.ToByte(Console.ReadLine());

                    if (userInput >= 1 && userInput < Enum.GetNames(typeof(Options)).Length + 1)
                        return userInput switch
                        {
                            (byte)Options.Human + 1 => Options.Human,
                            (byte)Options.AI + 1 => Options.AI,
                            (byte)Options.Stats + 1 => Options.Stats,
                            (byte)Options.Settings + 1 => Options.Settings,
                            (byte)Options.Quit + 1 => Options.Quit
                        };

                    /* Above is a complex solution that hopefully makes this more adaptable as a single menu class later on.
                     * Essentially I take each enum and cast it to it's integral value and add 1 to match user input. Then
                     * use that casted enum value to point directly to the enum value. Not totally sure about this solution
                     * but looking to the future essentially it would mean just adding whatever enum values and their
                     * associated byte + 1 values to the switch statement, rather than ensuring the index is correct in the array.
                    */

                    else Console.WriteLine("Please enter a valid option");
                }

            }

            public static void SetWindowTitle() => Console.Title = _windowTitle;

            // This takes an enum and gives it a special string to output, rather than the raw enum name, unless the enum name is good enough on its own (ie doesn't have a special case assigned)
            private static string ConvertOptionToString(Enum optionToConvert) => optionToConvert switch { Options.Human => "2-Player", Options.AI => "Computer", _ => Convert.ToString(optionToConvert) };
        }

        // Drives updating the settings for the game. Perhaps should be it's own class
        public static class Settings
        {
            public static void SettingsScreen(/*AIPlayer computer*/)
            {
                // Defining a variable to track user selection
                string selection;

                do
                {
                    DrawSettings();
                    selection = UserChoice();

                    if (selection == "1-1") Console.WriteLine($"AI Difficulty set to {AIOptions.Easy}"); //SetAILevel(AIOptions.Easy);
                    if (selection == "1-2") Console.WriteLine($"AI Difficulty set to {AIOptions.Medium}"); //SetAILevel(AIOptions.Medium);
                    if (selection == "1-3") Console.WriteLine($"AI Difficulty set to {AIOptions.Hard}"); //SetAILevel(AIOptions.Hard);
                    if (selection == "2-1") ChangeFGColor();
                    if (selection == "2-2") ChangeBGColor();
                    if (selection == "2-3") Console.ResetColor();
                } while (selection != "0");
            }

            private static void DrawSettings() 
            {
                // Output a header
                Console.WriteLine("\n<<<<< SETTINGS >>>>>");

                // Output the AI difficulty options
                Console.WriteLine("\n* AI Difficulty *");
                for (int i = 0; i < Enum.GetNames(typeof(AIOptions)).Length; i++)
                    Console.WriteLine($" 1-{i + 1} - {MakeFriendlyString((AIOptions)i)}");

                // Output the Customization options
                Console.WriteLine("\n* Customization *");
                for (int i = 0; i < Enum.GetNames(typeof(CustomizeOptions)).Length; i++)
                    Console.WriteLine($" 2-{i + 1} - {MakeFriendlyString((CustomizeOptions)i)}");

                Console.WriteLine("\n Enter 0 to exit");
            }

            private static void SetAILevel(AIOptions selectedOption)
            {
                // Will fill out when I have implemented the AI Player
            }

            private static void ChangeFGColor()
            {
                // Outputting the console color options with an associated index
                Console.WriteLine("\nConsole color options:");
                for (int i = 0; i < Enum.GetNames(typeof(ConsoleColor)).Length; i++)
                    Console.WriteLine($" {i + 1} - {MakeFriendlyString((ConsoleColor)i)} ");
                
                // Getting the enum's byte value and connecting that to the user input which is converted to a byte
                Console.ForegroundColor = Convert.ToByte(UserChoice()) - 1 switch 
                {
                    (byte)ConsoleColor.Black => ConsoleColor.Black,
                    (byte)ConsoleColor.DarkBlue => ConsoleColor.DarkBlue,
                    (byte)ConsoleColor.DarkGreen => ConsoleColor.DarkGreen,
                    (byte)ConsoleColor.DarkCyan => ConsoleColor.DarkCyan,
                    (byte)ConsoleColor.DarkRed => ConsoleColor.DarkRed,
                    (byte)ConsoleColor.DarkMagenta => ConsoleColor.DarkMagenta,
                    (byte)ConsoleColor.DarkYellow => ConsoleColor.DarkYellow,
                    (byte)ConsoleColor.Gray => ConsoleColor.Gray,
                    (byte)ConsoleColor.DarkGray => ConsoleColor.DarkGray,
                    (byte)ConsoleColor.Blue => ConsoleColor.Blue,
                    (byte)ConsoleColor.Green => ConsoleColor.Green,
                    (byte)ConsoleColor.Cyan => ConsoleColor.Cyan,
                    (byte)ConsoleColor.Red => ConsoleColor.Red,
                    (byte)ConsoleColor.Magenta => ConsoleColor.Magenta,
                    (byte)ConsoleColor.Yellow => ConsoleColor.Yellow,
                    (byte)ConsoleColor.White => ConsoleColor.White,
                    _ => ConsoleColor.Green
                };
            }

            private static void ChangeBGColor()
            {
                // Outputting the console color options with an associated index
                Console.WriteLine("\nConsole color options:");
                for (int i = 0; i < Enum.GetNames(typeof(ConsoleColor)).Length; i++)
                    Console.WriteLine($" {i + 1} - {MakeFriendlyString((ConsoleColor)i)} ");

                // Getting the enum's byte value and connecting that to the user input which is converted to a byte
                Console.BackgroundColor = Convert.ToByte(UserChoice()) - 1 switch
                {
                    (byte)ConsoleColor.Black => ConsoleColor.Black,
                    (byte)ConsoleColor.DarkBlue => ConsoleColor.DarkBlue,
                    (byte)ConsoleColor.DarkGreen => ConsoleColor.DarkGreen,
                    (byte)ConsoleColor.DarkCyan => ConsoleColor.DarkCyan,
                    (byte)ConsoleColor.DarkRed => ConsoleColor.DarkRed,
                    (byte)ConsoleColor.DarkMagenta => ConsoleColor.DarkMagenta,
                    (byte)ConsoleColor.DarkYellow => ConsoleColor.DarkYellow,
                    (byte)ConsoleColor.Gray => ConsoleColor.Gray,
                    (byte)ConsoleColor.DarkGray => ConsoleColor.DarkGray,
                    (byte)ConsoleColor.Blue => ConsoleColor.Blue,
                    (byte)ConsoleColor.Green => ConsoleColor.Green,
                    (byte)ConsoleColor.Cyan => ConsoleColor.Cyan,
                    (byte)ConsoleColor.Red => ConsoleColor.Red,
                    (byte)ConsoleColor.Magenta => ConsoleColor.Magenta,
                    (byte)ConsoleColor.Yellow => ConsoleColor.Yellow,
                    (byte)ConsoleColor.White => ConsoleColor.White,
                    _ => ConsoleColor.Green
                };
            }

            private static string UserChoice()
            {
                Console.Write("\nPlease enter the number of the option you'd like to select: ");
                return Console.ReadLine();
            }

            // This takes an enum and gives it a special string to output, rather than the raw enum name, unless the enum name is good enough on its own (ie doesn't have a special case assigned)
            private static string MakeFriendlyString(Enum enumName) => enumName switch { CustomizeOptions.ChangeFG => "Change text color", CustomizeOptions.ChangeBG => "Change background color", CustomizeOptions.Reset => "Reset colors", _ => Convert.ToString(enumName)};
        }

        public class Stats
        {
            // Leaving fields formatted as enum for readability
            private int XWins;
            private int XLosses;
            private int OWins;
            private int OLosses;
            private int HumanGames;
            private int AIGames;

            public Stats()
            {
                // Reserving this constructor for when I know how to write to files 
                // at which point the constructor will access the file on startup to
                // sync the Stats properties with what's in the file. Effectively a 
                // save system for stats specifically.
            }

            public void StatsScreen()
            {
                do
                { 
                    DrawStatsScreen();
                    Console.WriteLine("\n Enter 0 to exit \n");

                } while (Console.ReadKey(true).Key != ConsoleKey.NumPad0 && Console.ReadKey(true).Key != ConsoleKey.D0);
            }

            private void DrawStatsScreen()
            {
                Console.WriteLine("\n<<<<< STATS >>>>>");
                for (int i = 0; i < Enum.GetNames(typeof(StatsProperties)).Length; i++)
                    Console.WriteLine($" {MakeFriendlyString((StatsProperties)i)}: {DetermineStatToShow(i)}");
            }

            // Figure this will eventually both increment the associated property and write to a save file, before eventually just writing to a save file
            public void UpdateResultStat(Symbol playerSymbol)
            {
                if (playerSymbol == Symbol.X)
                { 
                    XWins++; 
                    OLosses++; 
                }
                
                if (playerSymbol == Symbol.O)
                {
                    OWins++;
                    XLosses++;
                }
            }

            public void UpdatePlayedStat(StatsProperties gameType)
            {
                if (gameType == StatsProperties.HumanGames) HumanGames++;
                if (gameType == StatsProperties.AIGames) AIGames++;
            }

            private int DetermineStatToShow(int statIndex) => statIndex switch { (int)StatsProperties.XWins => XWins, (int)StatsProperties.XLosses => XLosses, (int)StatsProperties.OWins => OWins, (int)StatsProperties.OLosses => OLosses, (int)StatsProperties.HumanGames => HumanGames, (int)StatsProperties.AIGames => AIGames };
            private static string MakeFriendlyString(StatsProperties statToConvert) => statToConvert switch { StatsProperties.XWins => "X Wins", StatsProperties.XLosses => "X Losses", StatsProperties.OWins => "O Wins", StatsProperties.OLosses => "O Losses", StatsProperties.HumanGames => "2-Player Games", StatsProperties.AIGames => "AI Games" };
        }
        public enum Symbol { Empty, X, O }
        public enum Options { Human, AI, Stats, Settings, Quit }
        public enum AIOptions { Easy, Medium, Hard }
        public enum CustomizeOptions { ChangeFG, ChangeBG, Reset}
        public enum StatsProperties { XWins, XLosses, OWins, OLosses, HumanGames, AIGames}
    }
}