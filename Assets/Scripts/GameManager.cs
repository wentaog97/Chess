using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Chess Piece Binary representation
// Use board[0][0] = BLACK | ROOK; to set and piece = board[5][3] & PIECE; to get
[Flags]
public enum ChessPiece
{   
    // Pieces
    EMPTY = 0,
    PAWN = 1,
    ROOK = 2,
    KNIGHT = 3,
    BISHOP = 4,
    QUEEN = 5,
    KING = 6,
    // Colors
    BLACK = 8,
    WHITE = 0,
    // Mask
    PIECEMASK = 7,
}

public enum CastlingRights{
    WHITE_KING_SIDE,
    WHITE_QUEEN_SIDE,
    BLACK_KING_SIDE,
    BLACK_QUEEN_SIDE
}

// This global class will save all of the game states, as well as commonly used methods
// Model in MVC
internal class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    // Game States
    ChessPiece[] board = new ChessPiece[64];
    List<int> capturedPieces;
    bool isWhiteTurn;
    bool[] canCastle = new bool[4];
    int enPassantTile;
    int halfMove;
    int fullMove;
    int movesCounter;
    string defaultFen = "rnbqkbnr/pppppppp/8/8/3P4/8/PPP1PPPP/RNBQKBNR w KQkq - 0 1";  

    // Ensure Singleton class
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
        InitGame(defaultFen);
    }

    void InitGame(string fen)
    {  
        string[] gameInfo = fen.Split(' ');
        InitializeBoard(gameInfo[0]);
        InitializeGameStates(gameInfo[1], gameInfo[2], gameInfo[3], gameInfo[4], gameInfo[5]);

        _displayBoard();
        _displayInfo();
    }

    // Helper Functions for conversions
    public ChessPiece getPiece(ChessPiece n){
        return n & ChessPiece.PIECEMASK;
    }
    public ChessPiece getColor(ChessPiece n){
        return n & ChessPiece.BLACK;
    }
    public ChessPiece[] getBoard(){
        return board;
    }
    public string PositionToNotation(int pos){
        int c = pos%8;
        int r = pos/8;

        char col = (char)(c+'a');
        char row = (char)(7-r+'1');

        return ""+col+row;
    }
    public int NotationToPosition(string str){
        char col = str[0];
        char row = str[1];

        int c = col - 'a';
        int r = 7 - (row - '1');

        return r*8+c;
    }

    // Takes in the position part of the FEN and initialize the board
    void InitializeBoard(string fen){
        // Initialize the board position
        int index = 0;
        foreach(char c in fen){
            if (char.IsDigit(c))
            {
                index += c - '0'; // Skip empty squares
            }
            else
            {
                ChessPiece piece = ChessPiece.EMPTY;
                switch(c)
                {
                    case 'P': piece = ChessPiece.PAWN; break;
                    case 'R': piece = ChessPiece.ROOK; break;
                    case 'N': piece = ChessPiece.KNIGHT; break;
                    case 'B': piece = ChessPiece.BISHOP; break;
                    case 'Q': piece = ChessPiece.QUEEN; break;
                    case 'K': piece = ChessPiece.KING; break;
                    case 'p': piece = ChessPiece.PAWN | ChessPiece.BLACK; break;
                    case 'r': piece = ChessPiece.ROOK | ChessPiece.BLACK; break;
                    case 'n': piece = ChessPiece.KNIGHT | ChessPiece.BLACK; break;
                    case 'b': piece = ChessPiece.BISHOP | ChessPiece.BLACK; break;
                    case 'q': piece = ChessPiece.QUEEN | ChessPiece.BLACK; break;
                    case 'k': piece = ChessPiece.KING | ChessPiece.BLACK; break;
                    case '/': continue;
                    default: break;
                }
                board[index++] = piece;
            }
        }
    }

    // Initialize the game states
    void InitializeGameStates(string turn, string castlingRights, string enPassantNotation, string halfMove, string fullMove){
        isWhiteTurn = (turn[0] == 'w');

        foreach (char c in castlingRights){
            if(c=='K') canCastle[0]=true;
            if(c=='Q') canCastle[1]=true;
            if(c=='k') canCastle[2]=true;
            if(c=='q') canCastle[3]=true;
        }
        
        if(enPassantNotation[0] == '-') enPassantTile = -1;
        else enPassantTile = NotationToPosition(enPassantNotation);
    }

    // States management methods
    public bool getTurn(){
        return isWhiteTurn;
    }
    public void changeTurn(){
        isWhiteTurn = !isWhiteTurn;
    }
    public int getEnpassantTile(){
        return enPassantTile;
    }
    public void setEnpassantTile(int pos){
        enPassantTile = pos;
    }
    public bool getCanCastle(CastlingRights side){
        return canCastle[(int)side];
    }
    public void setCanCastle(int side, bool state){
        canCastle[side] = state;
    }
    public void addCapturedPieces(int piece){
        capturedPieces.Add(piece);
    }
    public void MovePiece(int oriPos, int newPos){
        ChessPiece piece = board[oriPos];
        board[newPos] = board[oriPos];
        board[oriPos] = ChessPiece.EMPTY;
    }


    // DEBUG methods    
    public void _displayBoard(){
        string str = "";
        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                str += "[";

                int pos = r * 8 + c;
                ChessPiece pieceValue = board[pos];

                if (pieceValue == ChessPiece.EMPTY)
                {
                    str += " ";
                }
                else
                {
                    bool isBlack = (pieceValue & ChessPiece.BLACK) != ChessPiece.EMPTY;
                    ChessPiece piece = pieceValue & ChessPiece.PIECEMASK;

                    switch (piece)
                    {
                        case ChessPiece.PAWN: str += isBlack ? "p" : "P"; break;
                        case ChessPiece.ROOK: str += isBlack ? "r" : "R"; break;
                        case ChessPiece.KNIGHT: str += isBlack ? "n" : "N"; break;
                        case ChessPiece.BISHOP: str += isBlack ? "b" : "B"; break;
                        case ChessPiece.QUEEN: str += isBlack ? "q" : "Q"; break;
                        case ChessPiece.KING: str += isBlack ? "k" : "K"; break;
                        case ChessPiece.EMPTY: str += " "; break;
                        default: str += "?"; break;
                    }
                }

                str += "] ";
            }
            str += "\n";
        }

        Debug.Log(str);
    }
    public void _displayBoard_bits(){
        string str = "";
        for(int r=0; r<8; r++){
            for(int c=0; c<8; c++){
                str+="[";

                int pos = r*8+c;
                if(board[pos]==0) str+="0";
                else str+= Convert.ToString((int)board[pos], 2).PadLeft(8, '0');
                

                str+="] ";
            }
            str += "\n";
        }

        Debug.Log(str);
    }
    public void _displayInfo(){
        string gameInfo = "";
        gameInfo+="isWhiteTurn = " + isWhiteTurn + "\n";
        gameInfo+="canCastle = " + canCastle[0] + " " + canCastle[1] + " " + canCastle[2] + " " + canCastle[3] + " " + "\n";
        gameInfo+="enPassantTile = " + enPassantTile + ", in notation = " + PositionToNotation(enPassantTile) + "\n";
        gameInfo+="isWhiteTurn = " + isWhiteTurn + "\n";
        Debug.Log(gameInfo);
    }
}



