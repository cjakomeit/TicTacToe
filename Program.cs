﻿using System;

namespace TicTacToe
{
    class Program
    {
        static void Main()
        {
            // Initializes screens/game flows, then sets the window title before entering into the main loop
            Stats gameStats = new();
            Options selectedOption;

            Menu.SetWindowTitle();

            // Keeps player on title menu until they explicitly ask to leave
            do
            {
                Menu.Display();
                selectedOption = Menu.UserChoice();

                if (selectedOption == Options.Human) TicTacToeGame.RunGame(gameStats, false);
                else if (selectedOption == Options.AI) TicTacToeGame.RunGame(gameStats, true);
                else if (selectedOption == Options.Stats) gameStats.StatsScreen();
                else if (selectedOption == Options.Settings) Settings.SettingsScreen();

            } while (selectedOption != Options.Quit);
        }

        // Runs the higher level game, with methods for running either an AI or Human game
        public static class TicTacToeGame
        {
            public static int _roundTracker = 0;
            public static int _totalRounds;
            public static int _roundsNeededToWin;

            /// <summary>
            /// Conducts the tournament for two players, using logic to determine if it should do an AI or human game
            /// </summary>
            /// <param name="gameStats">The instance of the stat tracking class for the game</param>
            /// <param name="isAIGame">Boolean indicating if the game should play with AI or not</param>
            public static void RunGame(Stats gameStats, bool isAIGame)
            {
                DetermineTournamentLength();

                if (isAIGame)
                    ReportTournamentWinner(GameFlow(new Player(Symbol.X), new AIPlayer(Symbol.O), gameStats, isAIGame));

                else
                    ReportTournamentWinner(GameFlow(new Player(Symbol.X), new Player(Symbol.O), gameStats, isAIGame));

                gameStats.UpdatePlayedStat(StatsProperties.HumanGames);
            }

            /// <summary>
            /// Conducts gameplay for both human and AI games
            /// </summary>
            /// <param name="player1"></param>
            /// <param name="player2"></param>
            /// <param name="gameStats">The instance of the stat tracking class for the game</param>
            /// <returns>Player instance</returns>
            private static Player GameFlow(Player player1, Player player2, Stats gameStats, bool isAIGame)
            {
                Player winner;

                do
                {
                    // Placed at the top to stop unnecessary overcounting
                    _roundTracker++;

                    // Block to output helpful info to the players
                    Console.Write($"\nRound wins needed to win the game: {_roundsNeededToWin}\n" +
                                  $"Player 1 wins: {player1.NumberWins}\n" +
                                  $"Player 2 wins: {player2.NumberWins}\n");

                    // Prints the round number and waits to proceed until interaction
                    Console.WriteLine($"\nRound {_roundTracker}" +
                                      $"\nPress any key to start");
                    Console.ReadKey(true);

                    // Triggers the next round
                    if (isAIGame) winner = Round.DoAIRound(player1, player2 as AIPlayer, gameStats);
                    else winner = Round.DoHumanRound(player1, player2, gameStats);

                } while (player1.NumberWins < _roundsNeededToWin && player2.NumberWins < _roundsNeededToWin);

                // Resets round count to 0 following completion of a tournament
                _roundTracker = 0;

                return winner;
            }

            // Takes a user input after outputting a list of options, then sets _totalRounds according to user choice, determining _roundsNeededToWin based on the new _totalRounds
            private static void DetermineTournamentLength()
            {
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

                _roundsNeededToWin = _totalRounds / 2 + 1;
            }

            // Recieves the winner from whatever game type is run and simply outputs that player's associated symbol to report the winner
            private static void ReportTournamentWinner(Player winner) => Console.WriteLine("\n +----------+" +
                                                                                          $"\n | Player {winner.PlayerSymbol} |" +
                                                                                           "\n |   Wins!  |" +
                                                                                           "\n +----------+ \n");
        }

        // Runs an individual game, to facilitate running a tournament in TicTacToeGame. Knows various board states, and loops checking for win states or draw after each player's turn
        // Also pulls in a Stats instance in order to increment stats correctly based on Player Symbol
        public sealed class Round
        {
            /* There must be a better way to avoid having all this logic essentially twice */
            public static Player DoHumanRound(Player player1, Player player2, Stats gameStats)
            {
                int turnCount = 1;

                GameBoard board = new();

                board.DrawBoard();

                Player currentPlayer = player1;

                while (turnCount < 10)
                {
                    // Executes current player's turn 
                    Console.WriteLine($"Turn: {turnCount}");
                    Console.WriteLine($"It's Player {currentPlayer.PlayerSymbol}'s turn.\n");

                    // Gets player's tile choice, including checking for validity
                    currentPlayer.SetTileChoice(board);

                    board.DrawBoard();

                    // Checks for game over and returns whichever player is the current one
                    if (CheckForRoundOver(board, currentPlayer, gameStats)) return currentPlayer;

                    // If the game isn't over, then switch players to proceed with the next turn (according to AI or human)
                    currentPlayer = currentPlayer == player1 ? player2 : player1;

                    // Increments the round number before loop completes and game hasn't ended
                    turnCount++;
                };

                // Verified through testing that this doesn't cause issues in the case of a draw like previously thought
                // Realistically should never be reached
                return currentPlayer;
            }

            public static Player DoAIRound(Player player1, AIPlayer player2, Stats gameStats)
            {
                int turnCount = 1;

                GameBoard board = new();

                board.DrawBoard();

                var currentPlayer = player1;

                while (turnCount < 10)
                {
                    // Executes current player's turn 
                    Console.WriteLine($"Turn: {turnCount}");
                    Console.WriteLine($"It's Player {currentPlayer.PlayerSymbol}'s turn.\n");

                    // Determines if the player is AI or human, then selects the appropriate method
                    if (currentPlayer is AIPlayer) player2.AIMove(board, turnCount);
                    else currentPlayer.SetTileChoice(board);

                    board.DrawBoard();

                    // Checks for game over and returns whichever player is the current one
                    if (CheckForRoundOver(board, currentPlayer, gameStats)) return currentPlayer;

                    // If the game isn't over, then switch players to proceed with the next turn (according to AI or human)
                    currentPlayer = currentPlayer == player1 ? player2 : player1;

                    // Increments the round number before loop completes and game hasn't ended
                    turnCount++;
                };

                // Verified through testing that this doesn't cause issues in the case of a draw like previously thought
                // Realistically should never be reached
                return currentPlayer;
            }

            // Checks for the variety of game over states and sets _gameOver accordingly, then writes the outcome
            private static bool CheckForRoundOver(GameBoard board, Player currentPlayer, Stats gameStats)
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

            // Overload specifically for determing scores for minimax search to facilitate AI moves
            public static bool CheckForRoundOver(GameBoard board)
            {
                if (HorizontalWin(board) || VerticalWin(board) || DiagonalWin(board))
                    return true;

                else if (CatBoard(board))
                    return true;

                else return false;
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

        // Manages the game board, with methods to draw and update tiles
        public class GameBoard
        {
            public static (int x, int y) _boardSize = (3, 3);
            public BoardTile[,] TileMatrix { get; init; } = new BoardTile[_boardSize.x, _boardSize.y];

            // Creates BoardTile instances based on the dimensions of the board
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

        // Holds a value connected to a player symbol enum, and has coordinates to be displayed. Also has methods to check if it's empty or conver the symbol it's holding to a char
        public class BoardTile
        {
            public (int x, int y) Coordinates { get; init; }
            public Symbol XorO { get; set; } = Symbol.Empty;

            // Setting the coordinates outright, but leaving the contents to be determined
            public BoardTile(int x, int y) => Coordinates = (x, y);

            public bool IsEmpty() => XorO == Symbol.Empty;

            public char ConverToChar() => XorO switch { Symbol.X => 'X', Symbol.O => 'O', Symbol.Empty => ' ' };

        }

        // Allows for creation of different players using different symbols from the Symbol enum. Contains methods for getting a tile choice from user and checking that tile is valid for play
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

        // Class that derives from Player, then adds some decision-making logic with a variability in success factor based on difficulty
        public class AIPlayer : Player
        {
            public int difficulty = 2;
            private Random _randValue = new Random();
            public AIPlayer(Symbol symbol) : base(symbol) { }

            // AI player picks random x and y coordinates, then checks to make sure choice is valid. If it's a valid choice, the gameboard will update the tile at cartesian coordinates
            public void AIMove(GameBoard board, int turn)
            {
                Console.WriteLine("Attempting to determine AI move");

                bool validChoice;

                do
                {
                    (int x, int y) randCoordinates = AIMoveSearch.FindBestMove(board, this);//(_randValue.Next(3), _randValue.Next(3));

                    Console.WriteLine($"AI chose: {randCoordinates.x}, {randCoordinates.y}");

                    validChoice = ValidTileChoice(randCoordinates switch
                    {
                        (0, 0) => board.TileMatrix[0, 0],
                        (0, 1) => board.TileMatrix[0, 1],
                        (0, 2) => board.TileMatrix[0, 2],
                        (1, 0) => board.TileMatrix[1, 0],
                        (1, 1) => board.TileMatrix[1, 1],
                        (1, 2) => board.TileMatrix[1, 2],
                        (2, 0) => board.TileMatrix[2, 0],
                        (2, 1) => board.TileMatrix[2, 1],
                        (2, 2) => board.TileMatrix[2, 2]
                    });

                    Console.WriteLine($"{validChoice}");

                    if (validChoice)
                    {
                        board.TileMatrix[randCoordinates.x, randCoordinates.y].XorO = PlayerSymbol;
                        board.UpdateTileContent(board.TileMatrix[randCoordinates.x, randCoordinates.y]);
                    }
                } while (validChoice != true && turn < 10);
            }

            public int FindMoveScore(GameBoard board)
            {
                // If the round is won by AI, returns +10 to the score of an AI move
                if (Round.CheckForRoundOver(board) && this.PlayerSymbol == Symbol.O)
                    return 10;

                // If the round is won by the player, returns -10 to score of an AI move
                else if (Round.CheckForRoundOver(board) && !(this.PlayerSymbol == Symbol.O))
                    return -10;

                // If none of the above are true (ie game isn't over), return 0;
                else
                    return 0;
            }

        }

        // Drives updating the settings for the game. Perhaps should be it's own class
        public class Settings : Menu
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
            private static string MakeFriendlyString(Enum enumName) => enumName switch { CustomizeOptions.ChangeFG => "Change text color", CustomizeOptions.ChangeBG => "Change background color", CustomizeOptions.Reset => "Reset colors", _ => Convert.ToString(enumName) };
        }

        // Creates an instance that tracks and outputs stats when requested. Eventually should be able to write to a save file
        public class Stats : Menu
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

        public static class AIMoveSearch
        {
            private static readonly int _MAX = 1000;
            private static readonly int _MIN = -1000;

            public static (int x, int y) FindBestMove(GameBoard board, AIPlayer player)
            {
                int bestScore = _MIN;
                (int x, int y) bestMove = (-1, -1);

                // Parses all cells, determing the move value for any empty cell, then returning the value of the cell with the best value
                for (int i = 0; i < GameBoard._boardSize.x; i++)
                {
                    for (int j = 0; j < GameBoard._boardSize.y; j++)
                    {
                        if (board.TileMatrix[i, j].IsEmpty())
                        {
                            // Simulate the move by setting the AI symbol
                            board.TileMatrix[i, j].XorO = Symbol.O;

                            // Find the move score for this move
                            int moveScore = Minimax(board, 0, false, player);

                            // Undoing the move
                            board.TileMatrix[i, j].XorO = Symbol.Empty;

                            // If the moveScore ends up better than the bestScore, then moveScore becomes bestScore, and the coordinates of bestMove are updated to match the move
                            if (moveScore > bestScore)
                            {
                                bestMove.x = i;
                                bestMove.y = j;
                                bestScore = moveScore;
                            }
                        }
                    }
                }

                return bestMove;
            }

            private static int Minimax(GameBoard board, int depth, bool maximizingPlayer, AIPlayer player)
            {
                int score = player.FindMoveScore(board);

                // If maximizing player has won, return the evaluated score
                if (score == 10) return score;

                // If minimizing player has won, return the evaluated score
                if (score == -10) return score;

                // If no moves are left, return draw score (0)
                if (Round.CatBoard(board)) return 0;

                if (maximizingPlayer)
                {
                    int best = _MIN;

                    // Searches all cells, row then column, to find an empty cell to simulate a move
                    for (int i = 0; i < GameBoard._boardSize.x; i++)
                    {
                        for (int j = 0; j < GameBoard._boardSize.y; j++)
                        {
                            //Console.WriteLine($"i: {i}, j: {j}");

                            if (board.TileMatrix[i, j].IsEmpty()) // Index out of bounds of array exception thrown 
                            {
                                // Sets the tile to the AI player's symbol to simulate the move, as long as it's empty
                                board.TileMatrix[i, j].XorO = Symbol.O;

                                // Recursively chooses the maximum value of the move. If value returned is greater than best (default -1000), then the best value is overwritten with the returned value
                                best = Math.Max(best, Minimax(board, depth + 1, !maximizingPlayer, player));

                                // Undoes the move to clean up the search
                                board.TileMatrix[i, j].XorO = Symbol.Empty;
                            }
                        }
                    }

                    // Once all cells have been evaluated, the best value (maximized) is returned
                    return best- depth;
                }

                else
                {
                    int best = _MAX;

                    // Searches all cells, row then column, to find an empty cell to simulate a move
                    for (int i = 0; i < GameBoard._boardSize.x; i++)
                    {
                        for (int j = 0; j < GameBoard._boardSize.y; j++)
                        {
                            if (board.TileMatrix[i, j].IsEmpty())
                            {
                                // Sets the tile to the AI player's symbol to simulate the move, as long as it's empty
                                board.TileMatrix[i, j].XorO = Symbol.X;

                                // Recursively chooses the minimum value of the move. If value returned is greater than best (default 1000), then the best value is overwritten with the returned value
                                best = Math.Min(best, Minimax(board, depth + 1, !maximizingPlayer, player));

                                // Undoes the move to clean up the search
                                board.TileMatrix[i, j].XorO = Symbol.Empty;
                            }
                        }
                    }

                    // Once all cells have been evaluated, the best value (minimized) is returned
                    return best + depth;
                }
            }
        }
            

        public enum Symbol { Empty, X, O }
        public enum Options { Human, AI, Stats, Settings, Quit }
        public enum AIOptions { Easy, Medium, Hard }
        public enum CustomizeOptions { ChangeFG, ChangeBG, Reset}
        public enum StatsProperties { XWins, XLosses, OWins, OLosses, HumanGames, AIGames}
    }
}