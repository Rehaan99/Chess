using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting;
using System.Windows.Forms;

namespace GraphicGame
{
    public partial class ChessBoard : Form
    {
        #region Initialiser + Global Variables
        public ChessBoard()
        {
            InitializeComponent();
            DoubleBuffered = true;//allows for smooth animations.
            timer1.Start();
        }
        int[,][] BoardArray = new int[8, 8][]; // 0 = no piece, 1 = white piece, 2 = black piece
        object[,,] PieceIndex = new object[3, 7, 16];
        object[] AllPieces = new object[32];
        bool NotSelected = true;
        bool highlightKing = false;
        int QueenCreated = 0;
        int[] SelectedTile = new int[3];
        int[] MoveToTile = new int[3];
        int XCurPos = 0;
        int YCurPos = 0;
        int movesBeforeDraw = 0;
        int tileSizeW, tileSizeH, MouseTilePosX, MouseTilePosY, rightSpace;
        bool turn = true; // true means white turn
        #endregion
        #region Drawing the game
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            const double Ratio = 1.33;// 0.96 for game
            Width = (int)(Height * Ratio);
            int mapSize = 8;
            tileSizeW = (int)((Width - (Width / 5)) * 0.96 / mapSize);
            rightSpace = Width - ((int)((Width - (Width / 5)) * 0.96));
            tileSizeH = (Height - 32) / mapSize;
            int xPos, yPos;
            bool colourBlack = false;
            Graphics background = e.Graphics;
            Pen tilePen = new Pen(Color.Black, 1);
            for (yPos = 0; yPos < tileSizeH * mapSize; yPos += tileSizeH)
            {

                for (xPos = 0; xPos < tileSizeW * mapSize; xPos += tileSizeW)
                {
                    Rectangle backgroundSquare = new Rectangle(xPos, yPos, tileSizeW, tileSizeH);
                    if (colourBlack)
                    {
                        SolidBrush tileBrush = new SolidBrush(Color.SaddleBrown);
                        background.FillRectangle(tileBrush, backgroundSquare);
                        colourBlack = false;
                    }
                    else
                    {
                        SolidBrush tileBrush = new SolidBrush(Color.FromArgb(240, 217, 181));
                        background.FillRectangle(tileBrush, backgroundSquare);
                        colourBlack = true;
                    }
                    background.DrawRectangle(tilePen, backgroundSquare);
                }
                if (colourBlack)
                {
                    colourBlack = false;
                }
                else
                {
                    colourBlack = true;
                }
            }
            MouseTilePosX = (Cursor.Position.X - Location.X - 9) / tileSizeW;
            MouseTilePosY = (Cursor.Position.Y - Location.Y - 32) / tileSizeH;
            if (Cursor.Position.X - Location.X - 9 < 0 || Cursor.Position.Y - Location.Y - 32 < 0 ||
                Cursor.Position.Y - Location.Y - 32 > tileSizeH * mapSize || Cursor.Position.X - Location.X - 9 > tileSizeW * mapSize)
            {
                MouseTilePosY = -1;
                MouseTilePosX = -1;
            }
            dynamic Piece;
            for (int i = 0; i < 32; i++)
            {
                Piece = AllPieces[i];
                background.DrawImage(Piece.pieceImage, Piece.XLocation * tileSizeW, Piece.YLocation * tileSizeH, tileSizeW, tileSizeH);
            }
            Pen Selector = new Pen(Color.Yellow, 5);
            Pen Danger = new Pen(Color.Red, 6);
            if (highlightKing)
            {
                dynamic kingDanger;
                if (turn)
                {
                    kingDanger = AllPieces[0];
                }
                else
                {
                    kingDanger = AllPieces[1];
                }

                background.DrawRectangle(Danger, tileSizeW * kingDanger.XLocation, tileSizeH * kingDanger.YLocation, tileSizeW, tileSizeH);
            }
            if (NotSelected == false)
            {
                background.DrawRectangle(Selector, tileSizeW * XCurPos, tileSizeH * YCurPos, tileSizeW, tileSizeH);
            }

            if (showHints)
            {
                //display all the tiles that the clicked object can move to.               
                if (TraversablePoints.Count() > 0)
                {
                    SolidBrush Moveable = new SolidBrush(Color.FromArgb(120, Color.Blue));
                    for (int i = 0; i < TraversablePoints.Count(); i++)
                    {
                        Rectangle MoveableSquare = new Rectangle(tileSizeW * TraversablePoints[i][0], tileSizeH * TraversablePoints[i][1], tileSizeW, tileSizeH);
                        background.FillRectangle(Moveable, MoveableSquare);
                    }
                }
            }
            BackColor = Color.White;
            DebugOutput(background);

        }
        public void DebugOutput(Graphics background)
        {
            SolidBrush BlackText = new SolidBrush(Color.Black);
            Font TextFont = new Font("Times New Roman", rightSpace / 16);
            Font TurnFont = new Font("Times New Roman", rightSpace / 8);
            background.DrawString($"Mouse X Tile: {MouseTilePosX + 1}", TextFont, BlackText, Width - rightSpace, rightSpace / 7);
            background.DrawString($"Mouse Y Tile: {MouseTilePosY + 1}", TextFont, BlackText, Width - rightSpace, (rightSpace / 16) + rightSpace / 7);
            if (turn)
            {
                background.DrawString("White Turn", TurnFont, BlackText, Width - rightSpace, 0);
            }
            else
            {
                background.DrawString("Black Turn", TurnFont, BlackText, Width - rightSpace, 0);

            }
            int tipsXpos = (int)(Width - rightSpace * 0.95);
            int tipsYpos = (int)(Height - (Height / 10) * 1.75);
            int tipsWidth = (int)(rightSpace / 3 * 2.5);
            int tipsHeight = (Height / 10);
            tips.Location = new Point(tipsXpos, tipsYpos);
            tips.Size = new Size(tipsWidth, tipsHeight);
        }
        private void tips_Click(object sender, EventArgs e)
        {
            if (showHints)
            {
                tips.Text = "Display Hints";
                showHints = false;
            }
            else
            {
                tips.Text = "Hide Hints";
                showHints = true;
            }
        }
        Button tips = new Button();
        bool showHints = false;
        #endregion
        #region On Start-up (Load pieces)
        public void Form1_Load(object sender, EventArgs e) // could half the number of names by doubling the array sizes and using if statements in class.
        {

            Controls.Add(tips);
            tips.Text = "Display Hints";
            tips.Click += new EventHandler(tips_Click);
            King WhiteKing = new King();
            King BlackKing = new King();
            Queen WhiteQueen = new Queen();
            Queen BlackQueen = new Queen();
            Bishop[] WhiteBishop = new Bishop[2];
            Bishop[] BlackBishop = new Bishop[2];
            Knight[] WhiteKnight = new Knight[2];
            Knight[] BlackKnight = new Knight[2];
            Rook[] WhiteRook = new Rook[2];
            Rook[] BlackRook = new Rook[2];
            Pawn[] WhitePawns = new Pawn[8];
            Pawn[] BlackPawns = new Pawn[8];
            for (int i = 0; i < 8; i++)
            {
                for (int x = 0; x < 8; x++)
                {
                    BoardArray[i, x] = new int[] { 0, 0, 0 };
                }
            }
            //king (1)
            WhiteKing.Initialiser(true);
            BoardArray[WhiteKing.XLocation, WhiteKing.YLocation] = new int[] { 1, 1, 0 };//colour , piece
            PieceIndex[1, 1, 0] = WhiteKing;
            AllPieces[0] = WhiteKing;
            BlackKing.Initialiser(false);
            BoardArray[BlackKing.XLocation, BlackKing.YLocation] = new int[] { 2, 1, 0 };
            PieceIndex[2, 1, 0] = BlackKing;
            AllPieces[1] = BlackKing;
            //queen (2)
            WhiteQueen.Initialiser(true);
            BoardArray[WhiteQueen.XLocation, WhiteQueen.YLocation] = new int[] { 1, 2, 0 };
            PieceIndex[1, 2, 0] = WhiteQueen;
            AllPieces[2] = WhiteQueen;
            BlackQueen.Initialiser(false);
            BoardArray[BlackQueen.XLocation, BlackQueen.YLocation] = new int[] { 2, 2, 0 };
            PieceIndex[2, 2, 0] = BlackQueen;
            AllPieces[3] = BlackQueen;
            //bishop (3)
            for (int i = 0; i < 2; i++)
            {
                WhiteBishop[i] = new Bishop();
                WhiteBishop[i].Initialiser(true, i * 3 + 2);
                BoardArray[WhiteBishop[i].XLocation, WhiteBishop[i].YLocation] = new int[] { 1, 3, i };
                PieceIndex[1, 3, i] = WhiteBishop[i];
                AllPieces[i + 4] = WhiteBishop[i];
            }
            for (int i = 0; i < 2; i++)
            {
                BlackBishop[i] = new Bishop();
                BlackBishop[i].Initialiser(false, i * 3 + 2);
                BoardArray[BlackBishop[i].XLocation, BlackBishop[i].YLocation] = new int[] { 2, 3, i };
                PieceIndex[2, 3, i] = BlackBishop[i];
                AllPieces[i + 6] = BlackBishop[i];
            }
            //knight (4)
            for (int i = 0; i < 2; i++)
            {
                WhiteKnight[i] = new Knight();
                WhiteKnight[i].Initialiser(true, i * 5 + 1);
                BoardArray[WhiteKnight[i].XLocation, WhiteKnight[i].YLocation] = new int[] { 1, 4, i };
                PieceIndex[1, 4, i] = WhiteKnight[i];
                AllPieces[i + 8] = WhiteKnight[i];
            }
            for (int i = 0; i < 2; i++)
            {
                BlackKnight[i] = new Knight();
                BlackKnight[i].Initialiser(false, i * 5 + 1);
                BoardArray[BlackKnight[i].XLocation, BlackKnight[i].YLocation] = new int[] { 2, 4, i };
                PieceIndex[2, 4, i] = BlackKnight[i];
                AllPieces[i + 10] = BlackKnight[i];
            }
            //rook (5)
            for (int i = 0; i < 2; i++)
            {
                WhiteRook[i] = new Rook();
                WhiteRook[i].Initialiser(true, i * 7);
                BoardArray[WhiteRook[i].XLocation, WhiteRook[i].YLocation] = new int[] { 1, 5, i };
                PieceIndex[1, 5, i] = WhiteRook[i];
                AllPieces[i + 12] = WhiteRook[i];
            }
            for (int i = 0; i < 2; i++)
            {
                BlackRook[i] = new Rook();
                BlackRook[i].Initialiser(false, i * 7);
                BoardArray[BlackRook[i].XLocation, BlackRook[i].YLocation] = new int[] { 2, 5, i };
                PieceIndex[2, 5, i] = BlackRook[i];
                AllPieces[i + 14] = BlackRook[i];
            }
            //pawn (6)
            for (int i = 0; i < 8; i++)
            {
                WhitePawns[i] = new Pawn();
                WhitePawns[i].Initialiser(true, i);
                BoardArray[WhitePawns[i].XLocation, WhitePawns[i].YLocation] = new int[] { 1, 6, i };
                PieceIndex[1, 6, i] = WhitePawns[i];
                AllPieces[i + 16] = WhitePawns[i];
            }
            for (int i = 0; i < 8; i++)
            {
                BlackPawns[i] = new Pawn();
                BlackPawns[i].Initialiser(false, i);
                BoardArray[BlackPawns[i].XLocation, BlackPawns[i].YLocation] = new int[] { 2, 6, i };
                PieceIndex[2, 6, i] = BlackPawns[i];
                AllPieces[i + 24] = BlackPawns[i];
            }
        }
        #endregion
        #region game tick
        private void timer1_Tick(object sender, EventArgs e)
        {
            Invalidate();//used for double buffer
            for (int i = 0; i < 2; i++)
            {
                dynamic King = AllPieces[i];
                if (King.XLocation == -1 && i == 0)
                {
                    timer1.Stop();
                    MessageBox.Show("Black Wins");

                }
                else if (King.XLocation == -1 && i == 1)
                {
                    timer1.Stop();
                    MessageBox.Show("White Wins");

                }
            }
        }
        #endregion

        public bool GameOverChecker() // if there are no possible moves left to be made for one player, other player wins. if there are no moves left to be made for both players, draw.
        {
            if (turn)
            {
                Traversable(AllPieces[0]);
                
            }
            else
            {
                Traversable(AllPieces[1]);
            }
            if (KingSafeCheck() == false)
            {
                if (TraversablePoints.Count() == 0)
                { 
                return true;
                }
                
            }
            TraversablePoints.Clear();
            return false;
        }
        



        //MAKE CODE MORE EFFICIENT AND EASIER TO READ ASAP!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!



        // add 50 moves = draw
        // add counter to show when in less than 10?
        //add button to propose a draw
        //if only kings are left they draw.
        //add en passent?
        //add castling?
        //add AI using mini-max thing
        //incorporate LINQ
        // Check which piece is putting the king in check


        public void Traversable(object Piece)
        {
            dynamic movingPiece = Piece;
            int actualXPos, actualYPos;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (movingPiece.Movement(x, y, movingPiece.XLocation, movingPiece.YLocation, movingPiece.colour, BoardArray[x, y][0]))
                    {
                        if (movingPiece.GetType().ToString() == "GraphicGame.Knight")
                        {
                            if (BoardArray[x, y][0] != movingPiece.colour)
                            {
                                int[] ViableLoc = { x, y };
                                TraversablePoints.Add(ViableLoc);
                            }
                        }
                        else if (PathChecker(x, y, movingPiece, false))
                        {
                            if (BoardArray[x, y][0] != movingPiece.colour)
                            {
                                int[] ViableLoc = { x, y };
                                if (movingPiece.GetType().ToString() != "GraphicGame.King")
                                {
                                    TraversablePoints.Add(ViableLoc);
                                }
                                else
                                {                                   
                                    actualXPos = movingPiece.XLocation;
                                    actualYPos = movingPiece.YLocation;
                                    movingPiece.XLocation = x;
                                    movingPiece.YLocation = y;
                                    if (turn)
                                    {
                                        AllPieces[0] = movingPiece;
                                    }
                                    else
                                    {
                                        AllPieces[1] = movingPiece;
                                    }
                                    if (KingSafeCheck())
                                    {
                                        TraversablePoints.Add(ViableLoc);
                                    }
                                    else
                                    {
                                        highlightKing = false;
                                    }
                                    movingPiece.XLocation = actualXPos;
                                    movingPiece.YLocation = actualYPos;
                                    if (turn)
                                    {
                                        AllPieces[0] = movingPiece;
                                    }
                                    else
                                    {
                                        AllPieces[1] = movingPiece;
                                    }
                                }                               
                            }

                        }
                    }
                }
            }
        }
        List<int[]> TraversablePoints = new List<int[]>();


        public bool KingSafeCheck()
        {
            bool ValidMove;
            dynamic pChecker;
            dynamic kingChecker;
            for (int i = 0; i < AllPieces.Length; i++)
            {

                pChecker = AllPieces[i];
                if (pChecker.XLocation == -1)
                {
                    continue; // makes the program skip to the next iteration
                }
                if (turn)//white turn
                {
                    kingChecker = AllPieces[0];
                    if (pChecker.colour == 2)
                    {
                        ValidMove = pChecker.Movement(kingChecker.XLocation, kingChecker.YLocation, pChecker.XLocation, pChecker.YLocation, pChecker.colour, 1);
                    }
                    else
                    {
                        ValidMove = false;
                    }
                }
                else
                {
                    kingChecker = AllPieces[1];
                    if (pChecker.colour == 1)
                    {
                        ValidMove = pChecker.Movement(kingChecker.XLocation, kingChecker.YLocation, pChecker.XLocation, pChecker.YLocation, pChecker.colour, 2);
                    }
                    else
                    {
                        ValidMove = false;
                    }
                }
                if (ValidMove && PathChecker(kingChecker.XLocation, kingChecker.YLocation, pChecker, true))
                {
                    string type = pChecker.GetType().ToString();
                    if (type == "Pawn")
                    {
                        if (pChecker.IsCapturing == true)
                        {
                            highlightKing = true;
                            return false;
                        }
                    }
                    else
                    {
                        highlightKing = true;
                        return false;
                    }

                }
            }
            return true;
        }

        public bool PathChecker(int XNewPos, int YNewPos, dynamic Piece, bool kingCheck)
        {
            int checkXPos = XCurPos;
            int checkYPos = YCurPos;
            if (kingCheck)
            {
                checkXPos = Piece.XLocation;
                checkYPos = Piece.YLocation;
            }
            if (Piece.GetType().ToString() == "GraphicGame.Knight")
            {
                return true;
            }
            bool ValidMove;
            if (checkYPos - YNewPos > 0 && checkXPos - XNewPos > 0)//diagonal movement
            {

                for (int i = Math.Abs(checkYPos - YNewPos); i > 0; i--)
                {
                    checkXPos--;
                    checkYPos--;
                    if (checkXPos == -1 || checkYPos == -1)
                    {
                        return false;
                    }
                        ValidMove = Piece.TileChecker(BoardArray[checkXPos, checkYPos][0], i);
                        if (ValidMove == false)
                        {
                            return false;
                        }
                }
                return true;
            }
            else if (checkYPos - YNewPos < 0 && checkXPos - XNewPos < 0)//diagonal movement
            {
                for (int i = Math.Abs(checkYPos - YNewPos); i > 0; i--)
                {
                    checkXPos++;
                    checkYPos++;
                    if (checkXPos == 8 || checkYPos == 8)
                    {
                        return false;
                    }
                    ValidMove = Piece.TileChecker(BoardArray[checkXPos, checkYPos][0], i);
                        if (ValidMove == false)
                        {
                            return false;
                        }
                }
                return true;
            }
            else if (checkYPos - YNewPos > 0 && checkXPos - XNewPos < 0)//diagonal movement
            {
                for (int i = Math.Abs(checkYPos - YNewPos); i > 0; i--)
                {
                    checkXPos++;
                    checkYPos--;
                    if (checkXPos == 8 || checkYPos == -1)
                    {
                        return false;
                    }
                    ValidMove = Piece.TileChecker(BoardArray[checkXPos, checkYPos][0], i);
                        if (ValidMove == false)
                        {
                            return false;
                        }
                }
                return true;
            }
            else if (checkYPos - YNewPos < 0 && checkXPos - XNewPos > 0)//diagonal movement
            {
                for (int i = Math.Abs(checkYPos - YNewPos); i > 0; i--)
                {
                    
                    checkXPos--;
                        checkYPos++;
                    if (checkXPos == -1 || checkYPos == 8)
                    {
                        return false;
                    }
                        ValidMove = Piece.TileChecker(BoardArray[checkXPos, checkYPos][0], i);
                        if (ValidMove == false)
                        {
                            return false;
                        }
                }
                return true;
            }
            else if (checkYPos - YNewPos > 0)
            {
                for (int i = Math.Abs(checkYPos - YNewPos); i > 0; i--)
                {
                    checkYPos--;
                    ValidMove = Piece.TileChecker(BoardArray[XNewPos, checkYPos][0], i);
                    if (ValidMove == false)
                    {
                        return false;
                    }
                }
                return true;
            }
            else if (checkYPos - YNewPos < 0)
            {
                for (int i = Math.Abs(checkYPos - YNewPos); i > 0; i--)
                {
                    checkYPos++;
                    ValidMove = Piece.TileChecker(BoardArray[XNewPos, checkYPos][0], i);
                    if (ValidMove == false)
                    {
                        return false;
                    }
                }
                return true;
            }
            else if (checkXPos - XNewPos > 0)//white pawn
            {
                for (int i = Math.Abs(checkXPos - XNewPos); i > 0; i--)
                {
                    checkXPos--;
                    ValidMove = Piece.TileChecker(BoardArray[checkXPos, YNewPos][0], i);
                    if (ValidMove == false)
                    {
                        return false;
                    }
                }
                return true;
            }
            else if (checkXPos - XNewPos < 0)
            {
                for (int i = Math.Abs(checkXPos - XNewPos); i > 0; i--)
                {
                    checkXPos++;
                    ValidMove = Piece.TileChecker(BoardArray[checkXPos, YNewPos][0], i);
                    if (ValidMove == false)
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
        #region Click Events - playing the game
        // this is defintely VERY inefficeint - for now it works but change at some point
        private void Form1_Click(object sender, EventArgs e)
        {                                  
            highlightKing = false;
            bool ValidMove;
            bool isKingSafe;
            int XNewPos, YNewPos;
            int oldXPos1, oldYPos1;
            dynamic Piece;
            if (NotSelected == true)
            {
                if (MouseTilePosX != -1 || MouseTilePosY != -1)
                {
                    XCurPos = MouseTilePosX;
                    YCurPos = MouseTilePosY;
                }
                for (int i = 0; i < 3; i++)
                {
                    SelectedTile[i] = BoardArray[XCurPos, YCurPos][i];

                }
                if (turn == true && SelectedTile[0] == 1)//valid piece exists in that tile.
                {
                    NotSelected = false;
                    Traversable(PieceIndex[SelectedTile[0], SelectedTile[1], SelectedTile[2]]);
                }
                else if (turn == false && SelectedTile[0] == 2) // valid piece exists in that tile.
                {
                    NotSelected = false;
                    Traversable(PieceIndex[SelectedTile[0], SelectedTile[1], SelectedTile[2]]);
                }
            }
            else if (NotSelected == false)
            {
                XNewPos = MouseTilePosX;
                YNewPos = MouseTilePosY;
                if (XNewPos != -1 && YNewPos != -1)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        MoveToTile[i] = BoardArray[XNewPos, YNewPos][i];
                    }

                    Piece = PieceIndex[SelectedTile[0], SelectedTile[1], SelectedTile[2]];

                    oldXPos1 = Piece.XLocation;
                    oldYPos1 = Piece.YLocation;
                    ValidMove = Piece.Movement(XNewPos, YNewPos, XCurPos, YCurPos, SelectedTile[0], MoveToTile[0]);//defines the directions they can travel              
                    if (ValidMove && SelectedTile[1] != 4)//checks to see if path is blocked
                    {
                        ValidMove = PathChecker(XNewPos, YNewPos, Piece, false);
                    }
                    if (MoveToTile[0] != SelectedTile[0] && ValidMove == true)
                    {
                        BoardArray[Piece.XLocation, Piece.YLocation] = new int[] { 0, 0, 0 };
                        Piece.XLocation = XNewPos;
                        Piece.YLocation = YNewPos;
                        BoardArray[Piece.XLocation, Piece.YLocation] = new int[] { SelectedTile[0], SelectedTile[1], SelectedTile[2] };                       
                        isKingSafe = KingSafeCheck();
                        if (SelectedTile[1] == 6 && (YNewPos == 0 || YNewPos == 7))// pawn becomes queen
                        {
                            if (isKingSafe)
                            {
                                Queen NewQueen = new Queen
                                {
                                    XLocation = XNewPos,
                                    YLocation = YNewPos
                                };
                                dynamic queenPiece;
                                if (SelectedTile[0] == 1)
                                {
                                    queenPiece = AllPieces[2];
                                    NewQueen.colour = 1;
                                    NewQueen.pieceImage = queenPiece.pieceImage;
                                    BoardArray[NewQueen.XLocation, NewQueen.YLocation] = new int[] { 1, 2, QueenCreated + 1 };
                                    AllPieces[SelectedTile[2] + 16] = null;
                                    AllPieces[SelectedTile[2] + 16] = NewQueen;
                                    PieceIndex[1, 2, QueenCreated + 1] = NewQueen;
                                }
                                else
                                {
                                    queenPiece = AllPieces[3];
                                    NewQueen.colour = 2;
                                    NewQueen.pieceImage = queenPiece.pieceImage;
                                    BoardArray[NewQueen.XLocation, NewQueen.YLocation] = new int[] { 2, 2, QueenCreated + 1 };
                                    AllPieces[SelectedTile[2] + 24] = null;
                                    AllPieces[SelectedTile[2] + 24] = NewQueen;
                                    PieceIndex[2, 2, QueenCreated + 1] = NewQueen;
                                }
                                QueenCreated++;
                            }
                        }
                        bool pieceCaptured = false;
                        dynamic Piece2 = null;
                        //move to
                        if (MoveToTile[0] != SelectedTile[0]) // make sure its not the same colour piece being moved to.
                        {
                            if (MoveToTile[0] != 0)// if its moving over an occupied tile
                            {
                                pieceCaptured = true;
                                Piece2 = PieceIndex[MoveToTile[0], MoveToTile[1], MoveToTile[2]];
                                Piece2.XLocation = -1;
                                Piece2.YLocation = -1;
                            }
                            NotSelected = true;
                            isKingSafe = KingSafeCheck();                            
                            if (turn && isKingSafe)//switch turns
                            {
                                highlightKing = false;
                                turn = false;                               
                                if (GameOverChecker() == true)
                                {
                                    timer1.Stop();
                                    MessageBox.Show("White Wins");
                                }
                                TraversablePoints.Clear();
                                if (Piece.GetType().ToString() == "GraphicGame.Pawn")
                                {
                                    Piece.FirstMove = false;
                                }
                            }
                            else if (!turn && isKingSafe)
                            {
                                highlightKing = false;
                                turn = true;                              
                                
                                TraversablePoints.Clear();
                                if (Piece.GetType().ToString() == "GraphicGame.Pawn")
                                {
                                    Piece.FirstMove = false;
                                }
                            }
                            else // return to original positions
                            {
                                if (!turn)
                                {
                                    if (GameOverChecker() == true)
                                    {
                                        timer1.Stop();
                                        MessageBox.Show("White Wins");
                                    }
                                }
                                else
                                {
                                    if (GameOverChecker() == true)
                                    {
                                        timer1.Stop();
                                        MessageBox.Show("Black Wins");
                                    }

                                }
                                BoardArray[Piece.XLocation, Piece.YLocation] = new int[] { 0, 0, 0 };
                                Piece.XLocation = oldXPos1;
                                Piece.YLocation = oldYPos1;
                                BoardArray[Piece.XLocation, Piece.YLocation] = new int[] { SelectedTile[0], SelectedTile[1], SelectedTile[2] };
                                TraversablePoints.Clear();
                                if (pieceCaptured)
                                {
                                    Piece2.XLocation = XNewPos;
                                    Piece2.YLocation = YNewPos;
                                    BoardArray[Piece2.XLocation, Piece2.YLocation] = new int[] { MoveToTile[0], MoveToTile[1], MoveToTile[2] };
                                }
                            }
                        }
                    }
                    else
                    {
                        NotSelected = true;
                        for (int i = 0; i < 3; i++)
                        {
                            SelectedTile[i] = 0;
                        }
                        TraversablePoints.Clear();
                    }
                }
            }
        }
        #endregion
    }
}

