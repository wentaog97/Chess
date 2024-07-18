using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessNode
{
    // public string Move { get; set; } // The move that led to this node

    public ChessPiece[] Board = new ChessPiece[64]; // Bitboard representation of the chess board
    public int Depth;
    public ChessNode Parent { get; set; } // Parent node
    public List<ChessNode> Children { get; set; } // List of child nodes
    
    public List<ChessPiece> capturedPieces = new List<ChessPiece>();
    public bool isWhiteTurn;
    public bool[] canCastle = new bool[4];
    public int enPassantTile, enPassantPawnPosition, movesCounter;
    public int halfMoveCount, lastCapturedTurn, lastPawnMovedTurn;
    public bool isGameEnd = false;


    public ChessNode(){

    }

    // public ChessNode(
    //     ChessPiece[] board, int depth = 0, ChessNode parent = null, bool isWhiteTurn, bool[] canCastle, 
    //     int enPassantTile, int enPassantPawnPosition, int movesCounter, 
    //     int halfMoveCount, int lastCapturedTurn, int lastPawnMovedTurn, bool isGameEnd = false
    // ){
    //     Board = board;
    //     Move = move;
    //     Parent = parent;
    //     Depth = depth;
    //     Children = new List<ChessNode>();
    // }

    public void AddChild(ChessNode childNode)
    {
        Children.Add(childNode);
    }

    public bool IsTerminal()
    {
        // Determine if the node is a terminal node (e.g., checkmate, stalemate)
        // Implement your logic here
        return false;
    }

}
