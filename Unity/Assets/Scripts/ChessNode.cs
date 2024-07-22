using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessNode
{
    // public ChessNode parent = null; // Parent node
    // public List<ChessNode> children = new List<ChessNode>(); // List of child nodes
    public ChessPiece[] board = new ChessPiece[64]; // Bitboard representation of the chess board
    public List<ChessPiece> capturedPieces = new List<ChessPiece>();
    public bool isWhiteTurn = false;
    public bool[] canCastle = new bool[4]{false,false,false,false};
    public int enPassantTile = -1, enPassantPawnPosition = -1, movesCounter = 0;
    public int halfMoveCount = 0, lastCapturedTurn = 0, lastPawnMovedTurn = 0;
    public bool isGameEnd = false;
    public ChessNode(){}
    // Deep Copy Constructor
    public ChessNode(ChessNode node){
        // For deep copy
        for(int i=0; i<64; i++){
            board[i] = node.board[i];
        }
        for(int i=0; i<4; i++){
            canCastle[i] = node.canCastle[i];
        }

        // Other fields can just assign
        this.capturedPieces = new List<ChessPiece>(node.capturedPieces);
        this.isWhiteTurn = node.isWhiteTurn;
        this.enPassantTile = node.enPassantTile;
        this.enPassantPawnPosition = node.enPassantPawnPosition;
        this.movesCounter = node.movesCounter;
        this.halfMoveCount = node.halfMoveCount;
        this.lastCapturedTurn = node.lastCapturedTurn;
        this.lastPawnMovedTurn = node.lastPawnMovedTurn;
        this.isGameEnd = node.isGameEnd;
        
        // // Establish parent child relation
        // parent = node;
        // node.children.Add(this);
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
    // Helpers
    public ChessPiece getPiece(ChessPiece n){
        return n & ChessPiece.PIECEMASK;
    }
    public ChessPiece getColor(ChessPiece n){
        return n & ChessPiece.BLACK;
    }
}
