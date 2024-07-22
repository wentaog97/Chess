using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This global class will save all of the game states, as well as commonly used methods
// Serve as the model in MVC
public class GameManager : MonoBehaviour
{   
    // Example starting positions
    // Perft result can be found at: https://www.chessprogramming.org/Perft_Results
    const string 
        STARTING_POSITION = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", 
        POSITION2 = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 0",
        POSITION3 = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 0",
        POSITION4 = "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1",
        POSITION5 = "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8",
        POSITION6 = "r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10"
    ;
    string defaultFen = STARTING_POSITION;
    // Curr game states all saved in ChessNode class
    public ChessNode currGame = new ChessNode();
    LinkedList<ChessNode> moveHistory = new LinkedList<ChessNode>();

    void Awake()
    {
        InitGame(defaultFen);
    }
    public void ResetGame(){
        for(int i=0; i<64; i++){
            currGame.board[i] = ChessPiece.EMPTY;
        }
        currGame.isGameEnd = false;
        InitGame(STARTING_POSITION);
    }
    void InitGame(string fen)
    {  
        string[] gameInfo = fen.Split(' ');
        InitializeBoard(gameInfo[0]);
        InitializeGameStates(gameInfo[1], gameInfo[2], gameInfo[3], gameInfo[4], gameInfo[5]);

        _displayBoard(currGame);
        _displayInfo(currGame);
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
                currGame.board[index++] = piece;
            }
        }
    }
    // Initialize the game states
    void InitializeGameStates(string turn, string castlingRights, string enPassantNotation, string halfMove, string fullMove){
        currGame.isWhiteTurn = (turn[0] == 'w');

        foreach (char c in castlingRights){
            if(c=='K') currGame.canCastle[0]=true;
            if(c=='Q') currGame.canCastle[1]=true;
            if(c=='k') currGame.canCastle[2]=true;
            if(c=='q') currGame.canCastle[3]=true;
        }
        
        if(enPassantNotation[0] == '-') currGame.enPassantTile = -1;
        else currGame.enPassantTile = NotationToPosition(enPassantNotation);

        currGame.halfMoveCount = 0;
    }

    // Helper Functions for conversions
    public ChessPiece getPiece(ChessPiece n){
        return n & ChessPiece.PIECEMASK;
    }
    public ChessPiece getColor(ChessPiece n){
        return n & ChessPiece.BLACK;
    }
    public ChessPiece[] getBoard(){
        return currGame.board;
    }

    // Chess Notations
    public string PositionToNotation(int pos){
        // Determine the column (0 to 7) and row (0 to 7) from the position
        int column = pos % 8;
        int row = pos / 8;

        // Convert column to corresponding letter ('a' to 'h')
        char columnLetter = (char)('a' + column);

        // Convert row to corresponding number ('1' to '8')
        char rowNumber = (char)('1' + row);

        // Combine the column letter and row number to form the chess notation
        return $"{columnLetter}{rowNumber}";
    }
    public int NotationToPosition(string str){

        // Extract the column letter and row number from the input string
        char columnLetter = str[0];
        char rowNumber = str[1];

        // Validate column letter ('a' to 'h')
        if (columnLetter < 'a' || columnLetter > 'h')
        {
            throw new ArgumentException("Column letter must be between 'a' and 'h'.");
        }

        // Validate row number ('1' to '8')
        if (rowNumber < '1' || rowNumber > '8')
        {
            throw new ArgumentException("Row number must be between '1' and '8'.");
        }

        // Convert column letter to column index (0 to 7)
        int column = columnLetter - 'a';

        // Convert row number to row index (0 to 7)
        int row = rowNumber - '1';

        // Calculate the position in the 64-size array
        return row * 8 + column;
    }
    
    // Move history related
    public void AddToMoveHistory(ChessNode oldNode){
        moveHistory.AddLast(oldNode);
    }
    public void UndoLastHalfMove(){
        currGame = moveHistory.Last.Value;
        moveHistory.RemoveLast();
    }
    public void UndoLastFullMove(){
        moveHistory.RemoveLast();
        currGame = moveHistory.Last.Value;
        moveHistory.RemoveLast();
    }

    // DEBUG methods    
    public void _displayBoard(ChessNode node){
        Debug.Log(_toStringBoard(node));
    }
    public void _displayBoard_bits(ChessNode node){
        string str = "";
        for(int r=0; r<8; r++){
            for(int c=0; c<8; c++){
                str+="[";

                int pos = r*8+c;
                if(node.board[pos]==0) str+="0";
                else str+= Convert.ToString((int)node.board[pos], 2).PadLeft(8, '0');
                

                str+="] ";
            }
            str += "\n";
        }

        Debug.Log(str);
    }
    public void _displayInfo(ChessNode node){
        Debug.Log(_toStringGameInfo(node));
    }
    public string _toStringBoard(ChessNode node){
        string str = "";
        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                str += "[";

                int pos = r * 8 + c;
                ChessPiece pieceValue = node.board[pos];

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
        return str;
    }
    public string _toStringGameInfo(ChessNode node){
        string gameInfo = "";
        gameInfo+="isWhiteTurn = " + node.isWhiteTurn + "\n";
        gameInfo+="canCastle = " + node.canCastle[0] + " " + node.canCastle[1] + " " + node.canCastle[2] + " " + node.canCastle[3] + " " + "\n";
        gameInfo+="enPassantTile = " + node.enPassantTile + ", in notation = " + PositionToNotation(node.enPassantTile) + "\n";
        gameInfo+="enPassantPawnPosition = " + node.enPassantPawnPosition + "\n";
        gameInfo+="isWhiteTurn = " + node.isWhiteTurn + "\n";
        return gameInfo;
    }
}




/*

// Search related methods
    public ChessNode toNode(){
        ChessNode curr = new ChessNode();
        curr.board = board;
        curr.depth = 0;
        curr.parent = null;
        curr.children = null;
        curr.capturedPieces = capturedPieces;
        curr.isWhiteTurn = isWhiteTurn;
        curr.canCastle = canCastle;
        curr.enPassantTile=enPassantTile;
        curr.enPassantPawnPosition=enPassantPawnPosition;
        curr.movesCounter=movesCounter;
        curr.halfMoveCount=halfMoveCount;
        curr.lastCapturedTurn=lastCapturedTurn;
        curr.lastPawnMovedTurn=lastPawnMovedTurn;
        curr.isGameEnd=isGameEnd;
        return curr;
    }



*/

