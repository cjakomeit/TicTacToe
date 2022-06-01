using System;

namespace TicTacToe
{
    class Program
    {
        static void Main()
        {
            // Initializes and sets the window title before entering into the main loop
            Menu menu = new();
            menu.SetWindowTitle();
            Options selectedOption;

            // Keeps player on title menu until they explicitly ask to leave
            do
            {
                menu.Display();
                selectedOption = menu.UserChoice();

                TicTacToeGame game = new();

                /*selectedOption switch
                {
                    Options.Human => game.RunGame(),
                    Options.AI => game.RunGame(),
                    Options.Stats => game.RunGame(),
                    Options.Settings => game.RunGame()
                    Options.Quit => break;
                }; Not sure yet how to proceed with the actual 
                   choice processing logic */

            } while (selectedOption != Options.Quit);


        }

        public class TicTacToeGame
        {
            public int _roundTracker = 0;
            public int _totalRounds;
            public int _roundsNeededToWin;

            public void RunGame()
            {
                // Initializing all needed instances of all objects at this phase
                Player player1 = new(Symbol.X);
                Player player2 = new(Symbol.O);

                Console.Write("Please choose a mode:\n" +
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
                    Round newRound = new ();
                    newRound.DoRound(player1, player2);


                } while (player1.NumberWins < _roundsNeededToWin && player2.NumberWins < _roundsNeededToWin);

                ReportTournamentWinner(player1, player2);
            }

            public void ReportTournamentWinner (Player player1, Player player2)
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
                for(int i = 0; i < _boardSize.y; i++)
                {
                    for(int j = 0; j < _boardSize.x; j++)
                        TileMatrix[i, j] = new BoardTile(i,j);
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
            public  (int x, int y) Coordinates { get; init; }
            public Symbol XorO { get; set; } = Symbol.Empty;

            // Setting the coordinates outright, but leaving the contents to be determined
            public BoardTile(int x, int y) => Coordinates = (x, y);

            public bool IsEmpty() => XorO == Symbol.Empty;

            public char ConverToChar() => XorO switch { Symbol.X => 'X', Symbol.O => 'O', Symbol.Empty => ' ' };
            
        }

        public class Round
        {
            public int _turnCount = 0;

            public void DoRound(Player player1, Player player2)
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
                    if (CheckForRoundOver(board, player1, _turnCount)) break;
                    
                    // Increments the turn number after player 1 completes their turn and game doesn't end
                    _turnCount++;

                    /* Player 2's turn */
                    Console.WriteLine($"Turn: {_turnCount}");
                    Console.WriteLine($"It's Player {player2.PlayerSymbol}'s turn.\n");
                    player2.SetTileChoice(board);
                    board.DrawBoard();

                    if(CheckForRoundOver(board, player2, _turnCount)) break;
                    
                } while (_turnCount < 10);
            }
            
            // Checks for the variety of game over states and sets _gameOver accordingly, then writes the outcome
            public static bool CheckForRoundOver(GameBoard board, Player currentPlayer, int turn)
            {
                if (HorizontalWin(board, turn) || VerticalWin(board, turn) || DiagonalWin(board, turn))
                {
                    currentPlayer.NumberWins++;
                    Console.WriteLine($" **********\n" +
                                      $" {{{currentPlayer.PlayerSymbol}'s win!}}\n" +
                                      $" **********");
                    return true;
                }

                else if (CatBoard(board, turn))
                {
                    Console.WriteLine("The round ends in a draw.");
                    return true;
                }

                return false;
            }

            // There are three possible win conditions, and can only happen after turn 2. Loops through the three possible conditions based on column
            public static bool HorizontalWin(GameBoard board, int turn)
            {
                if (turn > 2)
                {
                    for (int i = 0; i < GameBoard._boardSize.x; i++)
                        if (board.TileMatrix[i, 0].XorO == board.TileMatrix[i, 1].XorO && board.TileMatrix[i, 1].XorO == board.TileMatrix[i, 2].XorO && board.TileMatrix[i, 0].XorO != Symbol.Empty) return true;

                    return false;
                }

                else return false;
            }

            // There are three possible win conditions like with horizontal, and can only happen after turn 2
            public static bool VerticalWin(GameBoard board, int turn)
            {
                if (turn > 2)
                {
                    for (int i = 0; i < GameBoard._boardSize.y; i++)
                        if (board.TileMatrix[0, i].XorO == board.TileMatrix[1, i].XorO && board.TileMatrix[1, i].XorO == board.TileMatrix[2, i].XorO && board.TileMatrix[0, i].XorO != Symbol.Empty) return true;

                    return false;
                }

                else return false;
            }

            // There are only two diagonal win possibilities, and can only happen after round two
            public static bool DiagonalWin(GameBoard board, int turn)
            {
                if (turn > 2)
                {
                    if (board.TileMatrix[1, 1].XorO == board.TileMatrix[2, 0].XorO && board.TileMatrix[1, 1].XorO == board.TileMatrix[0, 2].XorO && board.TileMatrix[1, 1].XorO != Symbol.Empty) return true;
                    else if (board.TileMatrix[1, 1].XorO == board.TileMatrix[0, 0].XorO && board.TileMatrix[1, 1].XorO == board.TileMatrix[2, 2].XorO && board.TileMatrix[1, 1].XorO != Symbol.Empty) return true;
                    else return false;
                }

                else return false;
            }

            // Currently only detects a full board
            public static bool CatBoard(GameBoard board, int turn)
            {
                if (turn > 2)
                {
                    foreach (BoardTile tile in board.TileMatrix)
                        if (tile.XorO == Symbol.Empty) return false;
                    return true;
                }

                return false;
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

        public class Menu
        {
            private readonly string _windowTitle = "Tic Tac Toe";
            public Options[] _options;


            // Constructor initializes the list of options using the enum
            public Menu() =>_options = new[] { Options.Human, Options.AI, Options.Stats, Options.Settings, Options.Quit };
            

            public void Display()
            {
                Console.WriteLine("<<<<< OPTIONS >>>>>");

                for (int i = 0; i < _options.Length; i++)
                    Console.WriteLine($" {i + 1}: {ConvertOptionToString(_options[i])}");
            }

            public Options UserChoice()
            {
                Console.WriteLine("Select an option by entering the corresponding number: ");
                byte userInput = Convert.ToByte(Console.ReadLine());

                while (true)
                {
                    if (userInput >= 1 && userInput < _options.Length)
                        return userInput switch    // Doesn't need default option as user is forced to enter a correct value
                        {
                            1 => _options[1],
                            2 => _options[2],
                            3 => _options[3],
                            4 => _options[4],
                            5 => _options[5]    
                        };

                    else Console.WriteLine("Please enter a valid option");
                }
                
            }

            public void SetWindowTitle() => Console.Title = _windowTitle;

            // This takes an enum and gives it a special string to output, rather than the raw enum name, unless the enum name is good enough on its own (ie doesn't have a special case assigned)
            private static string ConvertOptionToString(Options optionToConvert) => optionToConvert switch { Options.Human => "2-Player", Options.AI => "Computer", _ => $"{optionToConvert}" };
        }

        public enum Symbol {Empty, X, O}
        public enum Options { Human, AI, Stats, Settings, Quit}
    }
}