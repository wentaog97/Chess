using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Chess Piece Binary representation
// Use board[0][0] = BLACK | ROOK; to set and piece = board[5][3] & PIECE; to get
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
    WHITE_KING_SIDE = 0,
    WHITE_QUEEN_SIDE = 1,
    BLACK_KING_SIDE = 2,
    BLACK_QUEEN_SIDE = 3
}