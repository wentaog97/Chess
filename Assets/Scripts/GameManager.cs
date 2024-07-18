using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This global class will save all of the game states, as well as commonly used methods
// Serve as the model in MVC
public class GameManager : MonoBehaviour
{   
    // Example starting positions
    const string 
        STARTING_POSITION = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", 
        POSITION2 = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 0",
        POSITION3 = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 0",
        POSITION4 = "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1"
    ;

    // Game States
    ChessPiece[] board = new ChessPiece[64];
    List<ChessPiece> capturedPieces = new List<ChessPiece>();
    bool isWhiteTurn;
    bool[] canCastle = new bool[4];
    int enPassantTile, enPassantPawnPosition, movesCounter;
    int halfMoveCount, lastCapturedTurn, lastPawnMovedTurn;
    string defaultFen = POSITION3;
    public bool isGameEnd = false;

    ChessNode currNode = new ChessNode();

    void Awake()
    {
        InitGame(defaultFen);
    }

    public void ResetGame(){
        for(int i=0; i<64; i++){
            board[i] = ChessPiece.EMPTY;
        }
        isGameEnd = false;
        InitGame(STARTING_POSITION);
    }

    public void InitGame(string fen)
    {  
        string[] gameInfo = fen.Split(' ');
        InitializeBoard(gameInfo[0]);
        InitializeGameStates(gameInfo[1], gameInfo[2], gameInfo[3], gameInfo[4], gameInfo[5]);

        _displayBoard();
        _displayInfo();
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

        halfMoveCount = 0;
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

    // States management methods
    public ChessPiece getTurn(){
        return isWhiteTurn ? ChessPiece.WHITE : ChessPiece.BLACK;
    }
    public void changeTurn(){
        isWhiteTurn = !isWhiteTurn;
    }
    public int getEnpassantTile(){
        return enPassantTile;
    }
    public int getEnpassantPawnPosition(){
        return enPassantPawnPosition;
    }
    public void setEnpassantTile(int pos){
        enPassantTile = pos;
    }
    public bool getCanCastle(CastlingRights side){
        return canCastle[(int)side];
    }
    public bool[] getCanCastle(){
        return canCastle;
    }
    public void setCanCastle(int side, bool state){
        canCastle[side] = state;
    }
    public int getHalfMoveCount(){
        return halfMoveCount;
    }
    public int getLastCapturedTurn(){
        return lastCapturedTurn;
    }
    public int getLastPawnMovedTurn(){
        return lastPawnMovedTurn;
    }

    // Chess piece movements
    public void MovePiece(int oriPos, int newPos){
        ChessPiece piece = getPiece(board[oriPos]);
        ChessPiece color = getColor(board[oriPos]);
        // If target position is empty, move to empty space

        
        if(board[newPos] == ChessPiece.EMPTY){
            // All pawn logics, including promotion, enpassant, move 2, move 1
            if(piece == ChessPiece.PAWN){
                // If it's pawn and reached last row, promote piece
                if((newPos >=0 && newPos <8) || (newPos >=56 && newPos <64)){
                    
                    board[newPos] = PawnPromotion(color); // PROMOTION - Need to separate this for it's own function
                    board[oriPos] = ChessPiece.EMPTY;
                    
                    enPassantTile = -1;
                    enPassantPawnPosition = -1;
                    halfMoveCount++;
                    
                    lastPawnMovedTurn = halfMoveCount;
                    return;
                }

                // If it's pawn and attack sqaure can be enpassant
                if(newPos == enPassantTile){
                    board[newPos] = board[oriPos];
                    board[oriPos] = 0;
                    CapturePiece(enPassantPawnPosition);
                    enPassantTile = -1;
                    enPassantPawnPosition = -1;
                    halfMoveCount++;

                    lastPawnMovedTurn = halfMoveCount;
                    return;
                }

                // If it's pawn and moved 2 squares mark enpassant info
                if(Math.Abs(newPos-oriPos)==16){
                    board[newPos] = board[oriPos];
                    board[oriPos] = ChessPiece.EMPTY;
                    enPassantTile = (oriPos+newPos)/2;
                    enPassantPawnPosition = newPos;
                    halfMoveCount++;

                    lastPawnMovedTurn = halfMoveCount;
                    return;
                }

                // If it's pawn and moved 1 square
                board[newPos] = board[oriPos];
                board[oriPos] = 0;
                enPassantTile = -1;
                enPassantPawnPosition = -1;
                halfMoveCount++;

                lastPawnMovedTurn = halfMoveCount;
                return;
            } 

            // Castle move logic
            if(piece == ChessPiece.KING && (Math.Abs(newPos-oriPos)==2)){
                if((newPos-oriPos)==-2){ // Queen Side Castle
                    board[oriPos-1]=board[oriPos-4];
                    board[oriPos-4]=0;
                } else { // King Side Castle
                    board[oriPos+1]=board[oriPos+3];
                    board[oriPos+3]=0;
                }
            }

            // If king moves, remove all castle availability
            if(piece == ChessPiece.KING){ //&& !testingMode
                if(color==ChessPiece.WHITE){
                    canCastle[(int)CastlingRights.WHITE_KING_SIDE] = false;
                    canCastle[(int)CastlingRights.WHITE_QUEEN_SIDE] = false;
                } else {
                    canCastle[(int)CastlingRights.BLACK_KING_SIDE] = false;
                    canCastle[(int)CastlingRights.BLACK_QUEEN_SIDE] = false;
                }
            }

            // If rook moves, remove the castle availability of that side
            if(piece == ChessPiece.ROOK){ //&& !testingMode
                if(color==ChessPiece.WHITE){
                    if(oriPos == 56) canCastle[(int)CastlingRights.WHITE_QUEEN_SIDE] = false;
                    if(oriPos == 63) canCastle[(int)CastlingRights.WHITE_KING_SIDE] = false;
                } else {
                    if(oriPos == 0) canCastle[(int)CastlingRights.BLACK_QUEEN_SIDE] = false;
                    if(oriPos == 7) canCastle[(int)CastlingRights.BLACK_KING_SIDE] = false;
                }
            }

            // All other piece when moving to empty square
            board[newPos] = board[oriPos];
            board[oriPos] = 0;
            enPassantTile = -1;
            enPassantPawnPosition = -1;
            halfMoveCount++;
            return;
        }  

        // Promotion and capture
        if((piece == ChessPiece.PAWN) && ((newPos >=0 && newPos <8) || (newPos >=56 && newPos <64))) {
            CapturePiece(newPos);
            board[newPos] = PawnPromotion(color); // PROMOTION - Need to separate this for it's own function
            board[oriPos] = ChessPiece.EMPTY;
            lastPawnMovedTurn = halfMoveCount+1;

        } else{
            // All other standard capture
            // If there is a piece on the target position, capture enemy piece
            CapturePiece(newPos); //capturedPieces.push_back(board[newPos]);
            board[newPos] = board[oriPos];
            board[oriPos] = 0;
        }

        enPassantTile = -1;
        enPassantPawnPosition = -1;

        halfMoveCount++;
    }
    public ChessPiece PawnPromotion(ChessPiece color){
        return color | ChessPiece.QUEEN;
    }
    public void CapturePiece(int newPos){
        if(getPiece(board[newPos]) == ChessPiece.ROOK){
            switch(newPos){
                case 0:
                    canCastle[(int)CastlingRights.BLACK_QUEEN_SIDE] = false;
                    break;
                case 7:
                    canCastle[(int)CastlingRights.BLACK_KING_SIDE] = false;
                    break;
                case 56:
                    canCastle[(int)CastlingRights.WHITE_QUEEN_SIDE] = false;
                    break;
                case 63:
                    canCastle[(int)CastlingRights.WHITE_KING_SIDE] = false;
                    break;
                default: break;
            }
        }
        capturedPieces.Add(board[newPos]);
        board[newPos] = ChessPiece.EMPTY;

        lastCapturedTurn = halfMoveCount;
    } 
    

    // Search related methods
    public ChessNode toNode(){
        ChessNode curr = new ChessNode();
        curr.Board = board;
        curr.Depth = 0;
        curr.Parent = null;
        curr.Children = null;
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
        gameInfo+="enPassantPawnPosition = " + enPassantPawnPosition + "\n";
        gameInfo+="isWhiteTurn = " + isWhiteTurn + "\n";
        Debug.Log(gameInfo);
    }
}



