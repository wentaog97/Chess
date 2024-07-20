# Unity Chess Project

## Table of Contents
1. [Introduction](#introduction)
2. [Project Setup](#project-setup)
3. [Notes on Design](#notes-on-design)
4. [Testing and Debugging](#testing-and-debugging)
5. [Chess Bot Implementation](#chessbot-implementation)

## Introduction
This is Unity project is my implementation of Chess. 
The display will adjust to screen size dynamically and can be played on mobile devices. 
The game initializes by parsing the FEN String representation of the board, then initialize all the game states. 
Then player can click the piece to see possible moves. 
Once the player click a valid tile to move the selected piece, the piece will either move or capture enemy piece in the destination tile. 
Once player has no valid moves, the game will end and display the winner.
All rules have been implemented and tested, including enpassant, castling, pawn capture, pawn promotion, pinning, check, stalemate and checkmate.

## Project Setup

### Requirements
- Unity 2023.2.18f1
- .NET Framework 4.7.1

### Installation
1. Clone the repository:
    ```bash
    git clone https://github.com/yourusername/unity-chess.git
    ```
2. Open the project in Unity:
    ```plaintext
    Open Unity Hub, click on "Add", and select the cloned project folder.
    ```
3. Install necessary dependencies via Unity Package Manager.

## Notes on Design

### Board and Pieces
The chessboard is represented with a size 64 array and pieces are represented with bits. 
I used enum class for the pieces called ChessPiece with KING 6, QUEEN 5, BISHOP 4, KNIGHT 3, ROOK 2, PAWN 1.
There are also colors, BLACK 8 and WHITE 0, as well as a mask PIECE 7, and empty piece EMPTY 0
To set a piece to the bit board, you will need to use |. We will need a getPiece and getColor method to parse the bit representation

For example: 
BLACK | ROOK in bits would be 1000 | 0010, which is 1010. If you want to get piece type or color you can use &.
You can read more here: https://opensource.com/article/21/8/binary-bit-fields-masks

### Components
Follows roughly the MVC design pattern. 
The model is the game manager, which holds the game data and the bitboard.
The view is the rendered game play and UI, which is controlled display manager and UI manager components.
The controller is the moves manager component. 

The user would provide input, which is through left mouse button click.
The controller would parse the click and update the model. 
The display will be updated as the model changes.

The controller would check for all moves rules including:
- enpassant
- castling
- pawn capture
- pawn promotion
- pinning
- check
- stalemate
- checkmate
Once player has no valid moves, the game will display either stalemate or checkmate with winner.

## Testing and Debugging
Several bugs were caught during testing. Some notable ones are:
- Pawn can promote when going into empty space but cannot when capture
- King can still castle after rook has been captured

### Performance Testing Results
Example bug

NODE STEMMING FROM 37

[ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] 
[ ] [ ] [p] [ ] [ ] [ ] [ ] [ ] 
[ ] [ ] [ ] [p] [ ] [ ] [ ] [ ] 
[K] [P] [ ] [ ] [ ] [ ] [ ] [r] 
[ ] [R] [ ] [ ] [P] [p] [ ] [k] 
[ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] 
[ ] [ ] [ ] [ ] [ ] [ ] [P] [ ] 
[ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] 

Ori = 37 New = 45
Ori = 37 New = 44
Node count for pos 37 = 2

## ChessBot Implementation
Evaluation method calculating board value
DFS & alpha beta pruning


