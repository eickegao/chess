using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using Gtk;
using Cairo;

namespace GUI
{
    public enum PieceSelectionState { None, Selected }

    public partial class MainWindow: Gtk.Window
    {
        ImageSurface boardBackground;
        ImageSurface selectionBorder;
        PieceSelectionState currentSelectionState = PieceSelectionState.None;
        Regex engineOutputRegex = new Regex (@"^(?=.*(depth \d*))(?=.*(nps \d*))(?=.*(score (?:cp|mate) [+\-0-9]*))(?=.*(pv [a-h12345678 ]*)).*$");
        byte selectedPiece;
        int materialDifference = 0;
        Cairo.Context boardContext;
        byte[] pieceValues = { 1, 3, 3, 5, 8 }; // Pawn, Knight, Bishop, Rook, Queen
        byte[] whiteSquares = { 1, 3, 5, 7, 8, 10, 12, 14, 17, 19, 21, 23, 24, 26, 28, 30, 33,
                                35, 37, 39, 40, 42, 44, 46, 49, 51, 53, 55, 56, 58, 60, 62 };
        DateTime engineThinkCooldown = DateTime.Now;

        Task engineThinkTask;

        public MainWindow () : base (Gtk.WindowType.Toplevel)
        {
            boardBackground = new ImageSurface ("img/board.png");
            selectionBorder = new ImageSurface ("img/border.png");
            PieceDisplay.Init ();
            Build();
            SetPosition (WindowPosition.CenterAlways);
        }

        public void InitWidgets()
        {
            BoardArea.AddEvents ((int)Gdk.EventMask.ButtonPressMask);
            MaterialDifferenceLabel.Text = "Material is equal.";
        }

        protected void OnDeleteEvent (object sender, DeleteEventArgs a)
        {
            Application.Quit ();
            a.RetVal = true;
        }

        protected void OnQuit (object sender, EventArgs e)
        {
            Application.Quit ();
        }

        protected void OnAbout (object sender, EventArgs e)
        {
            AboutDialog about = new AboutDialog ();
            about.ProgramName = "Chess Training";
            about.Authors = new string[] { "Jiajun Gao" };
            about.Version = "1.0";
            about.Copyright = "Copyright 2017 Jiajun Gao";
            about.Run ();
            about.Destroy ();
        }


        protected void OnBoardExpose (object o, ExposeEventArgs args)
        {
            RedrawBoard ();
        }

        public void ShowGameOverDialog(GameStatus status)
        {
            string message = "";
            switch (status) {
                case GameStatus.Stalemate:
                    message = "Draw by stalemate.";
                    MainClass.CurrentGameStatus = GameStatus.Stalemate;
                    break;
                case GameStatus.WhiteCheckmate:
                    message = "Black wins by checkmate!";
                    MainClass.CurrentGameStatus = GameStatus.WhiteCheckmate;
                    break;
                case GameStatus.BlackCheckmate:
                    message = "White wins by checkmate!";
                    MainClass.CurrentGameStatus = GameStatus.BlackCheckmate;
                    break;
                case GameStatus.WhiteAdjudicate:
                    message = "Illegal move by white. Gandalf adjudication: Black wins.";
                    MainClass.CurrentGameStatus = GameStatus.BlackAdjudicate;
                    break;
                case GameStatus.BlackAdjudicate:
                    message = "Illegal move by black. Gandalf adjudication: White wins.";
                    MainClass.CurrentGameStatus = GameStatus.WhiteAdjudicate;
                    break;
                case GameStatus.WhiteTime:
                    message = "Time expired: White. Black wins.";
                    MainClass.CurrentGameStatus = GameStatus.WhiteTime;
                    break;
                case GameStatus.BlackTime:
                    message = "Time expired: Black. White wins.";
                    MainClass.CurrentGameStatus = GameStatus.BlackTime;
                    break;
                case GameStatus.DrawInsuffientMaterial:
                    message = "Game is a draw due to insufficient material.";
                    MainClass.CurrentGameStatus = GameStatus.DrawInsuffientMaterial;
                    break;
                case GameStatus.DrawFifty:
                    message = "Game is a draw according to the 50-move rule.";
                    MainClass.CurrentGameStatus = GameStatus.DrawFifty;
                    break;
                case GameStatus.DrawRepetition:
                    message = "Game is a draw by threefold repetition.";
                    MainClass.CurrentGameStatus = GameStatus.DrawRepetition;
                    break;
                default:
                    break;
            }
            MessageDialog gameOverDialog = new MessageDialog (
                                               this,
                                               DialogFlags.DestroyWithParent,
                                               MessageType.Info,
                                               ButtonsType.Ok,
                                               message);
            gameOverDialog.Run ();
            gameOverDialog.Destroy ();
        }

        void RedrawBoard()
        {
            Debug.Log ("Redrawing board.");
            boardContext = Gdk.CairoHelper.Create (BoardArea.GdkWindow);
            double transx = Math.Abs((BoardArea.Allocation.Width - (boardBackground.Width * 0.75))) / 2;
            boardContext.Translate (transx, 0);
            boardContext.Scale (0.75, 0.75);
            boardBackground.Show (boardContext, 0, 0);
            PieceDisplay.DrawPieces (boardContext);
            boardContext.Dispose ();
        }

        byte NotationToBoardSquare(string notation)
        {
            byte col = (byte)(Convert.ToInt32 (notation [0]) - 97);  // 97 is ASCII value for 'a'
            byte row = (byte)Math.Abs(Char.GetNumericValue (notation [1]) - 8);
            byte square = (byte)(col + (row * 8));

            return square;
        }

        protected void OnResetBoard (object sender, EventArgs e)
        {
            MainClass.CurrentBoard = new Board ();
            MainClass.CurrentGameStatus = GameStatus.Inactive;
            MainClass.ResetClock ();
            PiecePseudoLegalMoves.GeneratePseudoLegalMoves (MainClass.CurrentBoard);
            PieceLegalMoves.GenerateLegalMoves (MainClass.CurrentBoard);
            MainClass.CurrentGameHistory = new GameHistory ();
            ClearGameHistoryView ();
            RedrawBoard ();
        }


        public void UpdateClock(ChessClock clock)
        {
            if (clock.Colour == PieceColour.White) {
                WhiteClockLabel.Text = clock.TimeLeft.ToString ("g");
            } else {
                BlackClockLabel.Text = clock.TimeLeft.ToString ("g");
            }
        }

        protected void OnFlipBoard (object sender, EventArgs e)
        {
            MainClass.BoardOrientation = MainClass.BoardOrientation == PieceColour.White ?
                PieceColour.Black : PieceColour.White;
            RedrawBoard ();
        }

        protected void OnPieceClick (object o, ButtonPressEventArgs args)
        {
            Debug.Log (String.Format("BoardArea press at ({0}, {1})", args.Event.X, args.Event.Y));

            double transx = Math.Abs ((BoardArea.Allocation.Width - (boardBackground.Width * 0.75))) / 2;

            PointD clickLocation = new PointD (args.Event.X - transx, args.Event.Y - transx);
            if (clickLocation.X < 30 || clickLocation.Y < 30
                || clickLocation.X > 522 || clickLocation.Y > 522)
                return;

            PointD pieceLocation = PieceDisplay.pieceCoordinates [0];
            int pieceIndex = 0;

            for (int i = 0; i < PieceDisplay.pieceCoordinates.Length; i++) {
                PointD p = PieceDisplay.pieceCoordinates[i];
                double x1 = p.X * 0.75;
                double y1 = p.Y * 0.75;
                double x2 = x1 + 61.5;
                double y2 = y1 + 61.5;
                if (x1 <= clickLocation.X && clickLocation.X <= x2) {
                    if (y1 <= clickLocation.Y && clickLocation.Y <= y2) {
                        pieceLocation = p;
                        pieceIndex = MainClass.BoardOrientation == PieceColour.White ? i : Math.Abs (i - 63);
                        break;
                    }
                }
            }

            if (currentSelectionState == PieceSelectionState.None) {
                if (MainClass.CurrentBoard.Squares [pieceIndex].Piece == null)
                    return;
                selectedPiece = (byte)pieceIndex;

                boardContext = Gdk.CairoHelper.Create (BoardArea.GdkWindow);
                boardContext.Translate (transx, 0);
                boardContext.Scale (0.75, 0.75);
                selectionBorder.Show (boardContext, pieceLocation.X + 1, pieceLocation.Y + 1);
                boardContext.Dispose ();
                currentSelectionState = PieceSelectionState.Selected;
            } else {
                if (!MainClass.CurrentBoard.IsMoveValid (selectedPiece, (byte)pieceIndex)) {
                    currentSelectionState = PieceSelectionState.None;
                    Gtk.Application.Invoke (delegate {
                        RedrawBoard ();
                    });
                    return;
                }

                if (MainClass.CurrentGameStatus != GameStatus.Active &&
                    MainClass.CurrentGameStatus != GameStatus.Inactive) {
                    Console.Error.WriteLine ("(EE) Attempted move during finished game.");
                    MessageDialog errorDialog = new MessageDialog (
                                                    this,
                                                    DialogFlags.DestroyWithParent,
                                                    MessageType.Error,
                                                    ButtonsType.Ok,
                                                    "The game is over!");
                    errorDialog.Run ();
                    errorDialog.Destroy ();
                    return;
                }

                // Handle pawn promotion
                PieceType? promoteTo = null;
                if (MainClass.CurrentBoard.Squares [selectedPiece].Piece.Type == PieceType.Pawn &&
                   MainClass.CurrentBoard.IsMoveValid (selectedPiece, (byte)pieceIndex) &&
                   Array.IndexOf (MainClass.CurrentBoard.pawnPromotionDestinations, (byte)pieceIndex) != -1) {
                    PawnPromotionDialog dialog = new PawnPromotionDialog ();
                    if (dialog.Run () == (int)Gtk.ResponseType.Ok) {
                        promoteTo = dialog.PromoteTo;
                    } else {
                        dialog.Destroy ();
                        return;
                    }
                    dialog.Destroy ();
                }

                try {
                    SpecifierType specifierRequired = GameHistory.checkDisabiguationNeeded(MainClass.CurrentBoard, selectedPiece, (byte)pieceIndex);
                    MoveResult result = MainClass.CurrentBoard.MakeMove (selectedPiece, (byte)pieceIndex, promoteTo);

                    Piece movingPiece = null;
                    if(promoteTo == null) {
                        movingPiece = MainClass.CurrentBoard.Squares[(byte)pieceIndex].Piece;
                    } else {
                        movingPiece = new Piece(MainClass.CurrentBoard.Squares [(byte)pieceIndex].Piece.Colour, PieceType.Pawn);
                    }

                    if(result == MoveResult.Capture && movingPiece.Type == PieceType.Pawn) {
                        specifierRequired = SpecifierType.File;
                    }

                    int checkOrCheckmate = 0;
                    GameStatus mateState = MainClass.CurrentBoard.CheckForMate ();
                    if (mateState == GameStatus.WhiteCheckmate || mateState == GameStatus.BlackCheckmate) {
                        checkOrCheckmate = 2;
                    } else if (MainClass.CurrentBoard.WhiteCheck || MainClass.CurrentBoard.BlackCheck) {
                        checkOrCheckmate = 1;
                    }

                    string fenPosition = MainClass.CurrentBoard.ToFEN().Split(' ')[0];
                    MainClass.CurrentGameHistory.AddMove(new Move(selectedPiece, (byte)pieceIndex,
                        MainClass.CurrentBoard.Squares [(byte)pieceIndex].Piece.Colour,
                        movingPiece,
                        result,
                        MainClass.CurrentBoard.ToFEN(),
                        checkOrCheckmate,
                        specifierRequired,
                        promoteTo), fenPosition);
                    UpdateGameHistoryView();

                    if (MainClass.CurrentGameHistory.UpdateFiftyMoveCount (result) == GameStatus.DrawFifty) {
                        MainClass.CurrentGameStatus = GameStatus.DrawFifty;
                    } else if (MainClass.CurrentGameHistory.CheckThreefoldRepetition() == GameStatus.DrawRepetition) {
                        MainClass.CurrentGameStatus = GameStatus.DrawRepetition;
                    }
                } catch(InvalidOperationException) {
                    Debug.Log ("Invalid move entered.");
                }
                Gtk.Application.Invoke(delegate {
                    RedrawBoard();
                });
                GameStatus isMate = MainClass.CurrentBoard.CheckForMate ();
                if (isMate != GameStatus.Active) {
                    MainClass.CurrentGameStatus = isMate;
                }
                if (MainClass.CurrentGameStatus != GameStatus.Active && MainClass.CurrentGameStatus != GameStatus.Inactive) {
                    ShowGameOverDialog (MainClass.CurrentGameStatus);
                }

                if (MainClass.CurrentGameStatus == GameStatus.Inactive) {
                    MainClass.CurrentGameStatus = GameStatus.Active;
                }

                Gtk.Application.Invoke (delegate {
                    MainClass.UpdateClock ();
                    UpdatePlayerToMove();
                });

                currentSelectionState = PieceSelectionState.None;
            }
        }

        /**
         * @fn UpdateMaterialDifference
         * @brief Updates the material difference counter in the sidebar.
         */
        public void UpdateMaterialDifference(Board board)
        {
            int totalWhite = 0;
            int totalBlack = 0;

            int materialDifference = 0;
            foreach (Square sq in board.Squares) {
                if (sq.Piece != null) {
                    if (sq.Piece.Type != PieceType.King && sq.Piece.Colour == PieceColour.White) {
                        totalWhite += pieceValues [(int)(sq.Piece.Type)];
                        materialDifference += pieceValues [(int)(sq.Piece.Type)];
                    } else if (sq.Piece.Type != PieceType.King && sq.Piece.Colour == PieceColour.Black) {
                        totalBlack += pieceValues [(int)(sq.Piece.Type)];
                        materialDifference -= pieceValues [(int)(sq.Piece.Type)];
                    }
                }
            }

            if (materialDifference < 0) {
                MaterialDifferenceLabel.Text = String.Format ("Black is {0} pawn{1} up.",
                    Math.Abs (materialDifference), materialDifference < -1 ? "s" : "");
            } else if (materialDifference > 0) {
                MaterialDifferenceLabel.Text = String.Format ("White is {0} pawn{1} up.",
                    Math.Abs (materialDifference), materialDifference > 1 ? "s" : "");
            } else {
                MaterialDifferenceLabel.Text = "Material is equal.";
            }

            int maxMaterial = Math.Max (totalWhite, totalBlack);
            if (maxMaterial <= 3) { // We compare to 3 because kings are not counted.
                int minMaterial = Math.Min (totalWhite, totalBlack);
                if (minMaterial == 0) {
                    MainClass.CurrentGameStatus = GameStatus.DrawInsuffientMaterial;
                } else if (maxMaterial == minMaterial && maxMaterial == 3) {   // Now we check for opposite-coloured bishops
                    int whiteBishopSquare = -1;
                    int blackBishopSquare = -1;
                    for(int i = 0; i < board.Squares.Length; i++) {
                        Square sq = board.Squares [i];
                        if (sq.Piece != null && sq.Piece.Type == PieceType.Bishop) {
                            if (sq.Piece.Colour == PieceColour.White)
                                whiteBishopSquare = i;
                            else
                                blackBishopSquare = i;
                        }
                    }
                    // Only continue if both pieces are actually bishops
                    if (whiteBishopSquare != -1 && blackBishopSquare != -1) {
                        // Now we check to see if they are on opposite colours, using XOR
                        bool whiteBishopOnWhiteSquare = Array.IndexOf (whiteSquares, (byte)whiteBishopSquare) > -1;
                        bool blackBishopOnWhiteSquare = Array.IndexOf (whiteSquares, (byte)blackBishopSquare) > -1;
                        if (whiteBishopOnWhiteSquare ^ blackBishopOnWhiteSquare) {
                            MainClass.CurrentGameStatus = GameStatus.DrawInsuffientMaterial;
                        }
                    }
                }
            }
        }

        protected void OnOnePlayerSet (object sender, EventArgs e)
        {
            MainClass.CurrentMode = GameMode.OnePlayer;
        }

        protected void OnTwoPlayerSet (object sender, EventArgs e)
        {
            MainClass.CurrentMode = GameMode.TwoPlayer;
        }



        public void UpdatePlayerToMove()
        {
            if (MainClass.CurrentGameStatus == GameStatus.Active) {
                if (MainClass.CurrentBoard.PlayerToMove == PieceColour.White) {
                    PlayerToMoveLabel.Text = "Player to move: White";
                } else {
                    PlayerToMoveLabel.Text = "Player to move: Black";
                }
            } else {
                PlayerToMoveLabel.Text = "The game is not active.";
            }
        }

        protected void OnExportPGN (object sender, EventArgs e)
        {
            var fc = new FileChooserDialog ("Choose where to save the PGN",
                         this,
                         FileChooserAction.Save,
                         "Cancel", ResponseType.Cancel,
                         "Save", ResponseType.Accept);

            if (fc.Run () == (int)ResponseType.Accept) {
                MainClass.CurrentGameHistory.SavePGN (fc.Filename);
            }
            fc.Destroy ();
        }

        private void UpdateGameHistoryView()
        {
            Move move = MainClass.CurrentGameHistory.GetLastMove ();
            string moveOutput = "";
            if (move.Colour == PieceColour.White) {
                moveOutput += " " + MainClass.CurrentGameHistory.MoveCount + ". ";
            }
            moveOutput += GameHistory.MoveToNotation (move) + " ";
            GameHistoryView.Buffer.Text += moveOutput;
        }

        private void ClearGameHistoryView()
        {
            GameHistoryView.Buffer.Text = "";
        }

        protected void OnImportPGN (object sender, EventArgs e)
        {
            var fc = new FileChooserDialog ("Choose a PGN file to open.",
                                            this,
                                            FileChooserAction.Open,
                                            "Cancel", ResponseType.Cancel,
                                            "Open", ResponseType.Accept);
            if (fc.Run () == (int)ResponseType.Accept) {
                MainClass.CurrentGameHistory = GameHistory.importPGN (File.ReadAllText (fc.Filename));
                string pgn = MainClass.CurrentGameHistory.ToPGNString ();
                int indexOfMovesStart = pgn.IndexOf ("1.");
                if (indexOfMovesStart > 0) {
                    GameHistoryView.Buffer.Text = pgn.Substring (indexOfMovesStart);
                }

                // Load the FEN from the last move
                FENParser parser = new FENParser(MainClass.CurrentGameHistory.GetLastMove().FEN);
                MainClass.CurrentBoard = parser.GetBoard();
                MainClass.CurrentGameStatus = GameStatus.Inactive;
                PiecePseudoLegalMoves.GeneratePseudoLegalMoves(MainClass.CurrentBoard);
                PieceLegalMoves.GenerateLegalMoves(MainClass.CurrentBoard);
                Gtk.Application.Invoke (delegate {
                    RedrawBoard ();
                });

                MainClass.CurrentGameStatus = GameStatus.Inactive;
                GameStatus currentStatus = MainClass.CurrentBoard.CheckForMate ();
                if (currentStatus != GameStatus.Inactive && currentStatus != GameStatus.Active) {
                    Gtk.Application.Invoke (delegate {
                        ShowGameOverDialog (currentStatus);
                    });
                }
                MainClass.ResetClock ();
                UpdateMaterialDifference (MainClass.CurrentBoard);
            }
            fc.Destroy ();
        }
    }
}
