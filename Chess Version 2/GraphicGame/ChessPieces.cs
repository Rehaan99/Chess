﻿using GraphicGame.Properties;
using System;
using System.Drawing;

namespace GraphicGame
{
    #region ChessPiece Parent Class
    class ChessPieces
    {
        public int tileSize = 64;
        public Image pieceImage;
        public int XLocation;
        public int YLocation;
        public int colour;
        public static Image ChessImages(int pieceXLocation, int pieceYLocation)
        {
            Bitmap chessPieces = Resources.ChessPieces;
            Image pieceImage;
            int sizeWidth = Resources.ChessPieces.Width / 6;
            int sizeHeight = Resources.ChessPieces.Height / 2;
            int pieceX = pieceXLocation * sizeWidth;
            int pieceY = pieceYLocation * sizeWidth;
            var referencePoint = new Rectangle(pieceX, pieceY, sizeWidth, sizeHeight);
            pieceImage = chessPieces.Clone(referencePoint, chessPieces.PixelFormat);
            return pieceImage;
        }
    }
    #endregion
    #region Pawn
    class Pawn : ChessPieces
    {
        bool IsCapturing = false;
        public bool FirstMove = true;
        public void Initialiser(bool iswhite, int i)
        {
            if (iswhite)
            {
                colour = 1;
                YLocation = 6;
                pieceImage = ChessImages(5, 0);
            }
            else
            {
                colour = 2;
                YLocation = 1;
                pieceImage = ChessImages(5, 1);
            }
            XLocation = i;
        }
        public bool Movement(int DesiredMoveXPos, int DesiredMoveYPos, int CurrentXPos, int CurrentYPos, int colour, int OtherTileColour) // make sure it cant eat pieces in front of it.
        {
            if (colour == 1)
            {
                if (DesiredMoveYPos - CurrentYPos == -1 && DesiredMoveXPos == CurrentXPos)
                {

                    return true;

                }
                else if (DesiredMoveYPos - CurrentYPos == -2 && DesiredMoveXPos == CurrentXPos && FirstMove == true)
                {
       
                    return true;
                }
            }
            else if (colour == 2)
            {
                if (DesiredMoveYPos - CurrentYPos == 1 && DesiredMoveXPos == CurrentXPos)
                {
                   
                    return true;
                }
                else if (DesiredMoveYPos - CurrentYPos == 2 && DesiredMoveXPos == CurrentXPos && FirstMove == true)
                {
                   
                    return true;
                }
            }
            if (colour == 1 && OtherTileColour == 2)
            {                
                if (DesiredMoveYPos - CurrentYPos == -1 && (DesiredMoveXPos -CurrentXPos == -1 || DesiredMoveXPos - CurrentXPos == 1))
                {
                   
                    IsCapturing = true;
                    return true;
                }
            }
            else if (colour == 2 && OtherTileColour == 1)
            {               
                if (DesiredMoveYPos - CurrentYPos == 1 && (DesiredMoveXPos - CurrentXPos == -1 || DesiredMoveXPos - CurrentXPos == 1))
                {
                  
                    IsCapturing = true;
                    return true;
                }
            }       
            return false;
        }
        public bool TileChecker(int PieceExists, int TileIndex)
        {
            if (PieceExists != 0)
            {
                if (TileIndex == 1 && IsCapturing == true)
                {
                    IsCapturing = false;
                    return true;
                }
                return false;
            }
            return true;
        }
    }
    #endregion
    #region Rook
    class Rook : ChessPieces
    {
        public void Initialiser(bool iswhite, int i)
        {
            if (iswhite)
            {
                colour = 1;
                YLocation = 7;
                pieceImage = ChessImages(4, 0);
            }
            else
            {
                colour = 2;
                YLocation = 0;
                pieceImage = ChessImages(4, 1);
            }           
            XLocation = i;
        }
        public bool Movement(int DesiredMoveXPos, int DesiredMoveYPos, int CurrentXPos, int CurrentYPos, int colour, int OtherTileColour)
        {
            if (DesiredMoveXPos == CurrentXPos || DesiredMoveYPos == CurrentYPos)
            {
                return true;
            }
            return false;
        }
        public bool TileChecker(int PieceExists, int TileIndex)
        {
            if (PieceExists != 0)
            {
                if (TileIndex == 1)
                {
                    return true;
                }
                return false;
            }
            return true;
        }
    }
    #endregion
    #region Knight
    class Knight : ChessPieces
    {
        public void Initialiser(bool iswhite, int i)
        {
            if (iswhite)
            {
                colour = 1;
                YLocation = 7;
                pieceImage = ChessImages(3, 0);
            }
            else
            {
                colour = 2;
                YLocation = 0;
                pieceImage = ChessImages(3, 1);
            }
            XLocation = i;
        }
        public bool Movement(int DesiredMoveXPos, int DesiredMoveYPos, int CurrentXPos, int CurrentYPos, int colour, int OtherTileColour)
        {
            if (Math.Abs(DesiredMoveXPos - CurrentXPos) <= 2 && Math.Abs(DesiredMoveYPos - CurrentYPos) <= 2)
            {
                if (DesiredMoveXPos != CurrentXPos && DesiredMoveYPos != CurrentYPos)
                {
                    if (Math.Abs(DesiredMoveXPos - CurrentXPos) != Math.Abs(DesiredMoveYPos - CurrentYPos))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
    #endregion
    #region Bishop
    class Bishop : ChessPieces
    {
        public void Initialiser(bool iswhite, int i)
        {
            if (iswhite)
            {
                colour = 1;
                YLocation = 7;
                pieceImage = ChessImages(2, 0);
            }
            else
            {
                colour = 2;
                YLocation = 0;
                pieceImage = ChessImages(2, 1);
            }
            XLocation = i;
        }
        public bool Movement(int DesiredMoveXPos, int DesiredMoveYPos, int CurrentXPos, int CurrentYPos, int colour, int OtherTileColour)// make sure it cant jump over pieces.
        {
            if (Math.Abs(DesiredMoveXPos - CurrentXPos) == Math.Abs(DesiredMoveYPos - CurrentYPos))
            {
                return true;
            }
            return false;
        }
        public bool TileChecker(int PieceExists, int TileIndex)
        {
            if (PieceExists != 0)
            {
                if (TileIndex == 1)
                {
                    return true;
                }
                return false;
            }
            return true;
        }
    }
    #endregion
    #region Queen
    class Queen : ChessPieces
    {
        public void Initialiser(bool iswhite)
        {
            if (iswhite)
            {
                colour = 1;
                YLocation = 7;
                pieceImage = ChessImages(1, 0);
            }
            else
            {
                colour = 2;
                YLocation = 0;
                pieceImage = ChessImages(1, 1);
            }
            XLocation = 4;
        }
        public bool Movement(int DesiredMoveXPos, int DesiredMoveYPos, int CurrentXPos, int CurrentYPos, int colour, int OtherTileColour)
        {
            if (DesiredMoveXPos == CurrentXPos || DesiredMoveYPos == CurrentYPos || Math.Abs(DesiredMoveXPos - CurrentXPos) == Math.Abs(DesiredMoveYPos - CurrentYPos))
            {
                return true;
            }
            return false;
        }
        public bool TileChecker(int PieceExists, int TileIndex)
        {
            if (PieceExists != 0)
            {
                if (TileIndex == 1)
                {
                    return true;
                }
                return false;
            }
            return true;
        }
    }
    #endregion
    #region King
    class King : ChessPieces
    {
        public EventHandler Click { get; internal set; }

        public void Initialiser(bool iswhite)
        {
            if (iswhite)
            {
                colour = 1;
                YLocation = 7;
                pieceImage = ChessImages(0, 0);
            }
            else
            {
                colour = 2;
                YLocation = 0;
                pieceImage = ChessImages(0, 1);
            }
            XLocation = 3;
        }
        public bool Movement(int DesiredMoveXPos, int DesiredMoveYPos, int CurrentXPos, int CurrentYPos, int colour, int OtherTileColour)
        {
            if (Math.Abs(DesiredMoveXPos - CurrentXPos) <= 1 && Math.Abs(DesiredMoveYPos - CurrentYPos) <= 1)
            {
                return true;
            }
            return false;
        }
        public bool TileChecker(int PieceExists, int TileIndex)
        {
            if (PieceExists != 0)
            {
                if (TileIndex == 1)
                {
                    return true;
                }
                return false;
            }
            return true;
        }
    }
    #endregion
}
