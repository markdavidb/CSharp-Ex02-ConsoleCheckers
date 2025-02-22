using System;
using Ex02.ConsoleUtils;
using System.Text;

namespace Ex02
{
    public class ConsoleUi
    {
        private CheckersLogic m_Game = null;
        private string m_Player1Name = string.Empty;
        private string m_Player2Name = string.Empty;
        private int m_BoardSize = 0;
        private bool m_IsVsComputer = false;

        public void StartGame()
        {
            bool playAgain = true;

            Screen.Clear();
            initGameParameters();

            while (playAgain)
            {
                startNewRound();
                runRound();
                handleRoundResult();
                playAgain = askPlayAgain();
            }

            Console.WriteLine("Thanks for playing!");
        }

        private void initGameParameters()
        {
            m_Player1Name = getPlayerName("1");
            m_IsVsComputer = askIfVsComputer();

            if (!m_IsVsComputer)
            {
                m_Player2Name = getPlayerName("2");
            }
            else
            {
                m_Player2Name = "Computer";
            }

            m_BoardSize = getBoardSize();
        }

        private void startNewRound()
        {
            if (m_Game == null)
            {
                m_Game = new CheckersLogic(m_BoardSize, m_Player1Name, m_Player2Name, m_IsVsComputer);
            }
            else
            {
                m_Game.Reset(m_BoardSize);
            }
        }

        private void runRound()
        {
            displayBoard();

            while (m_Game.GameStatus == eGameStatus.InProgress)
            {
                if (m_Game.CurrentPlayer.IsComputer)
                {
                    handleComputerMove();
                }
                else
                {
                    handleHumanMove();
                }
            }
        }

        private void handleComputerMove()
        {
            Console.WriteLine("COMPUTER'S TURN: Press Enter to see its move");
            Console.ReadLine();
            bool moveSucceeded = m_Game.MakeComputerMove(out string computerMove);

            if (moveSucceeded)
            {
                Console.WriteLine($"Computer's move: {computerMove}");
                displayBoard();
            }
            else
            {
                Console.WriteLine("Computer could not make a move.");
            }
        }

        private void handleHumanMove()
        {
            string moveInput = Console.ReadLine();

            if (moveInput == "Q" || moveInput == "q")
            {
                m_Game.QuitGame();
                Console.WriteLine($"{m_Game.CurrentPlayer.Name} has quit the game.");
            }
            else
            {
                bool parseSuccess = tryParseMove(moveInput, out Move playerMove, out string errorMsg);

                if (!parseSuccess)
                {
                    Console.WriteLine(errorMsg);

                }
                else
                {
                    bool successMove = m_Game.MakeMove(playerMove, out string errorMsgMove);

                    if (successMove)
                    {
                        displayBoard();
                    }
                    else
                    {
                        Console.WriteLine(errorMsgMove);
                    }
                }
            }
        }

        private void handleRoundResult()
        {
            eGameStatus gameStatus = m_Game.GameStatus;

            switch (gameStatus)
            {
                case eGameStatus.Player1Won:
                case eGameStatus.Player2Won:
                    handleWin();
                    break;

                case eGameStatus.Tie:
                    showTie();
                    break;

                case eGameStatus.Quit:
                    handleQuit();
                    break;

                default:
                    Console.WriteLine("Unexpected game status.");
                    break;
            }
        }

        private void handleWin()
        {
            Player winner = m_Game.Winner;

            if (winner != null)
            {
                Player loser = winner == m_Game.Player1 ? m_Game.Player2 : m_Game.Player1;
                int winnerPoints = m_Game.ComputePoints(winner);
                int loserPoints = m_Game.ComputePoints(loser);
                int scoreDifference = Math.Abs(winnerPoints - loserPoints);

                winner.Score += scoreDifference;
                Console.WriteLine($"The winner is {winner.Name} with {winner.Score} points!");
            }
        }

        private void handleQuit()
        {
            Player winner = m_Game.CurrentPlayer == m_Game.Player1 ? m_Game.Player2 : m_Game.Player1;

            Console.WriteLine($"The winner is {winner.Name} with {winner.Score} points!");
        }

        private string getPlayerName(string i_PlayerNumber)
        {
            string playerName = string.Empty;
            bool isValid = false;

            while (!isValid)
            {
                Console.Write($"Enter player {i_PlayerNumber} name (up to 20 characters, no spaces): ");
                string input = Console.ReadLine();

                isValid = CheckersLogic.ValidatePlayerName(input, out string errorMsg);

                if (isValid)
                {
                    playerName = input;
                }
                else
                {
                    Console.WriteLine($"Invalid name: {errorMsg}");
                }
            }

            return playerName;
        }

        private int getBoardSize()
        {
            int boardSize = 0;
            bool isValid = false;

            while (!isValid)
            {
                Console.Write("Choose board size (6, 8, 10): ");
                string sizeInput = Console.ReadLine();
                bool parseSuccess = int.TryParse(sizeInput, out boardSize);

                if (parseSuccess)
                {
                    isValid = CheckersLogic.ValidateBoardSize(boardSize, out string errorMsg);

                    if (!isValid)
                    {
                        Console.WriteLine($"Invalid board size: {errorMsg}");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a valid number (6, 8, or 10).");
                }
            }

            return boardSize;
        }

        private bool askIfVsComputer()
        {
            bool isVsComputer = false;
            bool validInput = false;

            Console.Write(@"Do you want to play against computer/another player ?
For a game against the computer - enter 1
For a game against another player - enter 2
");

            while (!validInput)
            {
                string gameModeChoice = Console.ReadLine();

                if (gameModeChoice == "1")
                {
                    isVsComputer = true;
                    validInput = true;
                }
                else if (gameModeChoice == "2")
                {
                    validInput = true;
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter 1 or 2.");
                }
            }

            return isVsComputer;
        }

        private void displayBoard()
        {
            int boardSize = m_Game.BoardSize;
            StringBuilder builder = new StringBuilder();

            Screen.Clear();
            builder.Append(buildColumnHeader(boardSize));

            for (int row = 0; row < boardSize; row++)
            {
                builder.Append(buildRowHeader(row, boardSize));
            }

            Move lastMove = m_Game.LastMove;

            if (lastMove != null && m_Game.LastPlayer != null)
            {
                ePieceType movedPieceType = m_Game.GetPieceTypeAtPosition(m_Game.LastPlayer, lastMove.To.Row, lastMove.To.Column);

                if (movedPieceType == ePieceType.None)
                {
                    movedPieceType = m_Game.LastPlayer.RegularPiece;
                }

                builder.AppendLine($"{m_Game.LastPlayer.Name}'s move was ({movedPieceType}): {lastMove.GetMoveAsString()}");
            }

            builder.AppendLine($"{m_Game.CurrentPlayer.Name}'s Turn ({m_Game.CurrentPlayer.RegularPiece}):");
            Console.Write(builder.ToString());
        }

        private static string buildColumnHeader(int i_Size)
        {
            StringBuilder headerBuilder = new StringBuilder();

            headerBuilder.Append("    ");

            for (int i = 0; i < i_Size; i++)
            {
                headerBuilder.AppendFormat("{0,-4}", (char)('a' + i));
            }

            headerBuilder.AppendLine();
            headerBuilder.AppendLine(new string('=', 4 + i_Size * 4));

            return headerBuilder.ToString();
        }

        private string buildRowHeader(int i_Row, int i_Size)
        {
            StringBuilder rowBuilder = new StringBuilder();

            rowBuilder.AppendFormat("{0} |", (char)('A' + i_Row));

            for (int col = 0; col < i_Size; col++)
            {
                ePieceType piece = m_Game.GetPieceAt(i_Row, col);

                rowBuilder.AppendFormat(" {0} |", getCharForPiece(piece));
            }

            rowBuilder.AppendLine();
            rowBuilder.AppendLine(new string('=', 4 + i_Size * 4));

            return rowBuilder.ToString();
        }

        private static char getCharForPiece(ePieceType i_Piece)
        {
            char pieceChar = ' ';

            switch (i_Piece)
            {
                case ePieceType.O:
                    pieceChar = 'O';
                    break;
                case ePieceType.U:
                    pieceChar = 'U';
                    break;
                case ePieceType.X:
                    pieceChar = 'X';
                    break;
                case ePieceType.K:
                    pieceChar = 'K';
                    break;
                default:
                    pieceChar = ' ';
                    break;
            }

            return pieceChar;
        }

        private bool tryParseMove(string i_Input, out Move io_Move, out string io_ErrorMsg)
        {
            bool isValid = CheckersLogic.TryParseMove(i_Input, m_Game.BoardSize, out io_Move, out io_ErrorMsg);

            return isValid;
        }

        private bool askPlayAgain()
        {
            bool playAgain = false;
            bool inputValid = false;

            while (!inputValid)
            {
                Console.Write("Would you like to play another round? (Y/N): ");
                string userChoice = Console.ReadLine()?.Trim().ToUpper();

                if (userChoice == "Y")
                {
                    playAgain = true;
                    inputValid = true;
                }
                else if (userChoice == "N")
                {
                    inputValid = true;
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter Y or N.");
                }
            }

            return playAgain;
        }

        private void showTie()
        {
            Console.WriteLine("The game ended in a tie!");
        }
    }
}