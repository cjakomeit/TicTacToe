using System;

namespace TicTacToe
{
    class Program
    {
        static void Main()
        {
            TicTacToeGame game = new();
            game.RunGame();
        }

        public class TicTacToeGame
        {
            public int _roundTracker = 0;
            public bool _gameOver = false;

            public void RunGame()
            {
                // Initializing all needed instances of all objects
                Player player1 = new('X');
                Player player2 = new('O');
                GameBoard board = new();

                board.DrawBoard();

                do
                {
                    // Increments the round number after player 2 completes their turn and game hasn't ended, or on game start
                    _roundTracker++;

                    // Player 1's turn
                    Console.WriteLine($"Turn: {_roundTracker}");
                    Console.WriteLine($"It's Player {player1.PlayerSymbol}'s turn.\n");
                    player1.SetTileChoice(board);
                    board.DrawBoard();

                    // Checks for game over after Player 1's turn and breaks immediately
                    if (CheckForGameOver(board, player1)) break;
                    

                    // Increments the round number after player 1 completes their turn and game doesn't end
                    _roundTracker++;

                    // Player 2's turn
                    Console.WriteLine($"Turn: {_roundTracker}");
                    Console.WriteLine($"It's Player {player2.PlayerSymbol}'s turn.");
                    player2.SetTileChoice(board);
                    board.DrawBoard();

                    CheckForGameOver(board, player2);

                } while (_gameOver != true);

            }

            // Checks for the variety of game over states and sets _gameOver accordingly, then reports back a string with the associated state 
            public bool CheckForGameOver(GameBoard board, Player currentPlayer)
            {
                if(HorizontalWin(board) || VerticalWin(board) || DiagonalWin(board))
                {
                    Console.WriteLine($"{currentPlayer.PlayerSymbol}s win!");
                    _gameOver = true;
                }

                else if(CatBoard(board))
                {
                    Console.WriteLine("The game ends in a draw.");
                    _gameOver = true;
                }

                return false;
            }

            public bool HorizontalWin(GameBoard board)
            {
                for(int i = 0; i < GameBoard._boardSize.y; i++)
                    if (board.TileMatrix[i, 0].XorO == board.TileMatrix[i, 1].XorO && board.TileMatrix[i, 0].XorO == board.TileMatrix[i, 2].XorO && !String.IsNullOrWhiteSpace(board.TileMatrix[i, 0].XorO)) return true;
                    
                return false;
            }

            public bool VerticalWin(GameBoard board)
            {
                for (int i = 0; i < GameBoard._boardSize.x; i++)
                    if (board.TileMatrix[0, i].XorO == board.TileMatrix[1, i].XorO && board.TileMatrix[0, i].XorO == board.TileMatrix[2, i].XorO && !String.IsNullOrWhiteSpace(board.TileMatrix[0, i].XorO)) return true;

                return false;
            }

            public bool DiagonalWin(GameBoard board)
            {
                if (board.TileMatrix[1, 1].XorO == board.TileMatrix[2, 0].XorO && board.TileMatrix[1, 1].XorO == board.TileMatrix[0, 2].XorO) return true;
                else if (board.TileMatrix[1, 1].XorO == board.TileMatrix[0, 0].XorO && board.TileMatrix[1, 1].XorO == board.TileMatrix[2, 2].XorO) return true;
                else return false;
            }

            public bool CatBoard(GameBoard board)
            {
                foreach (BoardTile tile in board.TileMatrix)
                    if (String.IsNullOrWhiteSpace(tile.XorO)) return false;

                return true;
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
                for (int i = 0; i < _boardSize.y; i++)
                {
                    Console.WriteLine($"  { TileMatrix[i, 0].XorO } | { TileMatrix[i, 1].XorO } | { TileMatrix[i, _boardSize.x - 1].XorO } ");

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
            public string XorO { get; set; } = " ";

            // Setting the coordinates outright, but leaving the contents to be determined
            public BoardTile(int x, int y) => Coordinates = (x, y);
        }

        public class Player 
        {
            public byte TileChoice { get; set; }
            public char PlayerSymbol { get; init; }

            public Player(char setSymbol) => PlayerSymbol = setSymbol;

            // Grabs the user choice, finds the associated tile in the matrix, then if that's a valid placement, it updates the tile's contents on the gameboard
            public void SetTileChoice(GameBoard board) 
            {
                do
                {
                    // Asks the user for their selected tile, then finds a matching tile in the matrix 
                    TileChoice = QueryUser();
                    BoardTile selectedTile = TileChoice switch
                    {
                        9 => board.TileMatrix[0, 2],
                        8 => board.TileMatrix[0, 1],
                        7 => board.TileMatrix[0, 0],
                        6 => board.TileMatrix[1, 2],
                        5 => board.TileMatrix[1, 1],
                        4 => board.TileMatrix[1, 0],
                        3 => board.TileMatrix[2, 2],
                        2 => board.TileMatrix[2, 1],
                        1 => board.TileMatrix[2, 0],
                        _ => board.TileMatrix[0, 2]
                    };

                    // If the tile is a valid selection, make the contents the player symbol, then hand tile to the board to update
                    if (ValidTileChoice(selectedTile))
                    {
                        selectedTile.XorO = Convert.ToString(PlayerSymbol);
                        board.UpdateTileContent(selectedTile);
                    }

                    // Instructs user to choose another tile if the one they selected isn't valid
                    else Console.WriteLine("That tile isn't available. Please choose another.");
                } while (TileChoice < 1 || TileChoice > 9);
            }

            public bool ValidTileChoice(BoardTile TileToCheck) => String.IsNullOrWhiteSpace(TileToCheck.XorO);

            public byte QueryUser()
            {
                Console.Write("Please choose a number corresponding to the tile (1-9): ");

                byte userResponse = Convert.ToByte(Console.ReadLine());
                return userResponse;
            }
        }

    }
}
