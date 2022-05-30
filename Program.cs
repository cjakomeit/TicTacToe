﻿using System;

namespace TicTacToe
{
    class Program
    {
        static void Main()
        {
            while (true)
            {
                TicTacToeGame game = new();
                game.RunGame();
            }
            
        }

        public class TicTacToeGame
        {
            public int _roundTracker = 0;
            public int _totalRounds;
            public int _roundsNeededToWin;

            public void RunGame()
            {
                // Initializing all needed instances of all objects at this phase
                Player player1 = new('X');
                Player player2 = new('O');

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

                    // Player 1's turn
                    Console.WriteLine($"Turn: {_turnCount}");
                    Console.WriteLine($"It's Player {player1.PlayerSymbol}'s turn.\n");
                    player1.SetTileChoice(board);
                    board.DrawBoard();

                    // Checks for game over after Player 1's turn and breaks immediately if true
                    if (CheckForRoundOver(board, player1, _turnCount)) break;
                    
                    // Increments the turn number after player 1 completes their turn and game doesn't end
                    _turnCount++;

                    // Player 2's turn
                    Console.WriteLine($"Turn: {_turnCount}");
                    Console.WriteLine($"It's Player {player2.PlayerSymbol}'s turn.\n");
                    player2.SetTileChoice(board);
                    board.DrawBoard();

                    if(CheckForRoundOver(board, player2, _turnCount)) break;
                    
                } while (_turnCount < 10);
            }
            
            // Checks for the variety of game over states and sets _gameOver accordingly, then writes the outcome
            public bool CheckForRoundOver(GameBoard board, Player currentPlayer, int turn)
            {
                if (HorizontalWin(board, turn) || VerticalWin(board, turn) || DiagonalWin(board, turn))
                {
                    currentPlayer.NumberWins++;
                    Console.WriteLine($"     **********\n" +
                                      $"     {{{currentPlayer.PlayerSymbol}'s win!}}\n" +
                                      $"     **********\n" +
                                      $"     Player {currentPlayer.PlayerSymbol} Wins: {currentPlayer.NumberWins}");
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
            public bool HorizontalWin(GameBoard board, int turn)
            {
                if (turn > 2)
                {
                    for (int i = 0; i < GameBoard._boardSize.x; i++)
                        if (board.TileMatrix[i, 0].XorO == board.TileMatrix[i, 1].XorO && board.TileMatrix[i, 1].XorO == board.TileMatrix[i, 2].XorO && !String.IsNullOrWhiteSpace(board.TileMatrix[i, 0].XorO)) return true;

                    return false;
                }

                else return false;
            }

            // There are three possible win conditions like with horizontal, and can only happen after turn 2
            public bool VerticalWin(GameBoard board, int turn)
            {
                if (turn > 2)
                {
                    for (int i = 0; i < GameBoard._boardSize.y; i++)
                        if (board.TileMatrix[0, i].XorO == board.TileMatrix[1, i].XorO && board.TileMatrix[1, i].XorO == board.TileMatrix[2, i].XorO && !String.IsNullOrWhiteSpace(board.TileMatrix[0, i].XorO)) return true;

                    return false;
                }

                else return false;
            }

            // There are only two diagonal win possibilities, and can only happen after round two
            public bool DiagonalWin(GameBoard board, int turn)
            {
                if (turn > 2)
                {
                    if (board.TileMatrix[1, 1].XorO == board.TileMatrix[2, 0].XorO && board.TileMatrix[1, 1].XorO == board.TileMatrix[0, 2].XorO && !String.IsNullOrWhiteSpace(board.TileMatrix[1, 1].XorO)) return true;
                    else if (board.TileMatrix[1, 1].XorO == board.TileMatrix[0, 0].XorO && board.TileMatrix[1, 1].XorO == board.TileMatrix[2, 2].XorO && !String.IsNullOrWhiteSpace(board.TileMatrix[1, 1].XorO)) return true;
                    else return false;
                }

                else return false;
            }

            // Currently only detects a full board
            public bool CatBoard(GameBoard board, int turn)
            {
                if (turn > 2)
                {
                    foreach (BoardTile tile in board.TileMatrix)

                        if (String.IsNullOrWhiteSpace(tile.XorO)) return false;


                    return true;
                }

                return false;
            }
        }

        public class Player 
        {
            public byte TileChoice { get; set; }
            public char PlayerSymbol { get; init; }
            public int NumberWins { get; set; }

            public Player(char setSymbol) => PlayerSymbol = setSymbol;

            // Grabs the user choice, finds the associated tile in the matrix, then if that's a valid placement, it updates the tile's contents on the gameboard
            public void SetTileChoice(GameBoard board) 
            {
                do
                {
                    // Asks the user for their selected tile, then finds a matching tile in the matrix 
                    TileChoice = QueryUser();

                    if (TileChoice > 0 && TileChoice < 10)
                    {
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
                            1 => board.TileMatrix[2, 0]
                        };

                        // If the tile is a valid selection, make the contents the player symbol, then hand tile to the board to update
                        if (ValidTileChoice(selectedTile))
                        {
                            selectedTile.XorO = Convert.ToString(PlayerSymbol);
                            board.UpdateTileContent(selectedTile);
                            break;
                        }

                        // Instructs user that a tile is taken and pushes them back to the top of the loop.
                        else
                        {
                            Console.WriteLine("\nThat tile is taken. Please choose another.\n");
                            continue;
                        }
                    }

                    // Instructs user to choose another tile if the one they selected isn't valid
                    else Console.WriteLine("\nThat tile isn't available. Please choose another.\n");
                } while (true);
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
