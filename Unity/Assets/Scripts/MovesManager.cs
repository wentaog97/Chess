using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Serve as the controller in MVC
public class MovesManager : MonoBehaviour
{
    public GameManager gameManager;
    public DisplayManager displayManager;
    public UIManager uIManager;
    int selected = -1, movingTo = -1;
    HashSet<GameObject> validTiles = new HashSet<GameObject>();
    public ChessPiece player1Color = ChessPiece.WHITE, player2Color = ChessPiece.BLACK;
    public ChessBot bot1, bot2;
    public bool isBot1 = false, isBot2 = false;
    bool isPromoting = false;

    // Update is called once per frame
    void Update()
    {   
        if(!gameManager.currGame.isGameEnd){
            if (gameManager.currGame.getTurn()==player1Color) {
                if(isBot1) bot1.BotMove();
                else if (Input.GetMouseButtonDown(0)) HandleClick(); 
            } else {
                if(isBot2) bot2.BotMove();
                else if (Input.GetMouseButtonDown(0)) HandleClick(); 
            }   
        }
    }

    // The player clicked on a tile or not, if not reset tile selected if there are any
    // After clicking a tile, now branch into 2 decisions
    // If something is not selected, the game has to:
    // - select the clicked piece that matches turn color
    // - calculate all the valid moves that this piece can do
    // - highlight all valid tiles on display
    // - highlight selected piece's tile
    // If something is selected, we can assume valid tiles and the piece's tile are selected
    // then the game needs to:
    // - determine the clicked tile is one of the valdi tile
    // - if yes move piece to tile and update
    // - if not reset everything
    void HandleClick(){
        GameObject objectClicked = DetectClickedObject(); // Will only return if a tile has been clicked
        
        if(objectClicked == null) { // If the player clicked somewhere else other than the board, reset
            if(isPromoting){
                displayManager.DoneWithPawnPromotion();
                isPromoting=false;
            }
            ResetThisTurn();
            return;
        } 

        if(isPromoting){
            GameObject options = displayManager.promotionOptions;
            switch(objectClicked.name){
                case ("Queen"):
                    // UnityEngine.Debug.Log("Promoting to queen");
                    MovePiece(gameManager.currGame, selected, movingTo,0);
                    break;
                case ("Rook"):
                    // UnityEngine.Debug.Log("Promoting to rook");
                    MovePiece(gameManager.currGame, selected, movingTo,1);
                    break;
                case ("Knight"):
                    // UnityEngine.Debug.Log("Promoting to knight");
                    MovePiece(gameManager.currGame, selected, movingTo,2);
                    break;
                case ("Bishop"):
                    // UnityEngine.Debug.Log("Promoting to bishop");
                    MovePiece(gameManager.currGame, selected, movingTo,3);
                    break;
                default:
                    break;
            }
            displayManager.DoneWithPawnPromotion();
            isPromoting=false;
            // Reset selected and wipe all highlights
            ResetThisTurn();
            // If reach end game, display it
            CheckForCheckMate(gameManager.currGame);
            // Updates the display
            uIManager.displayGameInfo();
            return;
        }
        // UnityEngine.Debug.Log("DEBUG2");

        movingTo = CalculateTilePosition(objectClicked);
        
        // No selected piece, we need to check if clicked is a selectable piece
        if(selected == -1) { 
            
            ChessPiece clickedPosition = gameManager.currGame.board[movingTo];

            if(clickedPosition!=ChessPiece.EMPTY){  // If have a piece, select it

                ChessPiece turn = gameManager.currGame.getTurn();
                ChessPiece color = gameManager.getColor(clickedPosition);

                if(turn == color){
                    selected = movingTo;

                    // Highlight selected piece
                    displayManager.HighlightTile(movingTo);

                    // Highlight all valid tile
                    foreach (int validPos in GetLegalMoves(gameManager.currGame,selected)){
                        validTiles.Add(displayManager.getTile(validPos));
                        displayManager.HighlightTile(validPos);
                    }
                    return; // After selecting, it would still be this turn, so no need to reset
                }
                // If color doesn't match turn do nothing
            } 
            // If clicked an empty tile do nothing 
            return;
        } 
        
        // In here we will have a piece selected, we need to check if the tile clicked is a valid move
        if(isClickedValidTile(objectClicked)){
            if(gameManager.getPiece(gameManager.currGame.board[selected])==ChessPiece.PAWN&&((movingTo>=0&&movingTo<8)||(movingTo>=56&&movingTo<64))){
                isPromoting = true;
                displayManager.PromotingPawn(selected);
                return;
            }

            MovePiece(gameManager.currGame, selected, movingTo);
            // Change Turn after move has been made
            // gameManager.currGame.changeTurn();

        } 

        // Reset selected and wipe all highlights
        ResetThisTurn();

        // If reach end game, display it
        CheckForCheckMate(gameManager.currGame);
        // UnityEngine.Debug.Log(selected+", " + movingTo + ", " + isPromoting);

        // Updates the display
        uIManager.displayGameInfo();
    }
    GameObject DetectClickedObject()
    {
        GameObject clickedObject = null;
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] hits = Physics2D.OverlapPointAll(mousePosition);
        
        foreach (Collider2D hit in hits)
        {   
            if(isPromoting&&hit.CompareTag("PromotionTile")){
                clickedObject = hit.gameObject;
                break;
            }

            if (hit.CompareTag("Tile"))
            {
                clickedObject = hit.gameObject;
                break;
            } 
            
        }

        //UnityEngine.Debug.Log(clickedObject);
        return clickedObject;
    }

    // Tile Clicked related
    bool isClickedValidTile(GameObject tile){
        return validTiles.Contains(tile);
    }
    int CalculateTilePosition(GameObject tile){
        List<GameObject> tiles = displayManager.getTiles();
        for(int i=0; i<64; i++){
            if(tiles[i]==tile) return i;
        }
        return -1;
    }

    // Castling related
    bool canKingCastle(ChessNode node, CastlingRights side) {
        int kingPos, rookPos, start, finish;
        HashSet<int> underAttack = null;
        
        switch(side){
            case CastlingRights.WHITE_KING_SIDE:
                kingPos = 60; rookPos = 63; start = 60; finish = 62;
                underAttack = CheckAllPosBeingAttackedBy(node, ChessPiece.BLACK);
                break;
            case CastlingRights.WHITE_QUEEN_SIDE: 
                kingPos = 60; rookPos = 56; start = 58; finish = 60;
                underAttack = CheckAllPosBeingAttackedBy(node, ChessPiece.BLACK);
                break;
            case CastlingRights.BLACK_KING_SIDE: 
                kingPos = 4; rookPos = 7; start = 4; finish = 6; 
                underAttack = CheckAllPosBeingAttackedBy(node, ChessPiece.WHITE);
                break;
            case CastlingRights.BLACK_QUEEN_SIDE: 
                kingPos = 4; rookPos = 0; start = 2; finish = 4; 
                underAttack = CheckAllPosBeingAttackedBy(node, ChessPiece.WHITE);
                break;
            default:    
                return false;
        }
        
        if(isPieceInPath(node, kingPos,rookPos)) return false;

        for(int i=start; i<=finish; i++){
            if(underAttack.Contains(i)) return false;
        }

        if (!node.getCanCastle(side)) return false;

        return true;
    }
    bool isPieceInPath(ChessNode node, int oriPos, int newPos) {
        ChessPiece[] board = node.board;
        // Board is an 8x8 chess board
        int oriRow = oriPos / 8;
        int oriCol = oriPos % 8;
        int newRow = newPos / 8;
        int newCol = newPos % 8;

        int rowStep = 0;
        int colStep = 0;

        // Determine the direction of movement
        if (oriRow == newRow) {
            // Horizontal movement
            colStep = (newCol > oriCol) ? 1 : -1;
        } else if (oriCol == newCol) {
            // Vertical movement
            rowStep = (newRow > oriRow) ? 1 : -1;
        } else if (Math.Abs(oriRow - newRow) == Math.Abs(oriCol - newCol)) {
            // Diagonal movement
            rowStep = (newRow > oriRow) ? 1 : -1;
            colStep = (newCol > oriCol) ? 1 : -1;
        } else {
            // Not a valid straight or diagonal move
            return false;
        }

        // Start from the next position in the path
        int currentRow = oriRow + rowStep;
        int currentCol = oriCol + colStep;
        
        // Iterate until we reach the target position
        while (currentRow != newRow || currentCol != newCol) {
            int currentPos = currentRow * 8 + currentCol;
            // cout << " BOARD [" << currentPos << "] = " << board[currentPos] << endl;
            if (board[currentPos] != ChessPiece.EMPTY) {
                return true; // Found a piece in the path
            }
            
            currentRow += rowStep;
            currentCol += colStep;
        }

        return false; // No piece found in the path
    }

    // Check valids moves related methods
    public List<int> getAllMovablePieces(ChessNode node, ChessPiece color){
        ChessPiece[] board = node.board;

        List<int> allPieces = new List<int>();
        for(int i=0; i<64; i++){
            if(node.getColor(board[i])==color) allPieces.Add(i);
        }

        List<int> res = new List<int>();
        foreach(int i in allPieces){            
            if(GetLegalMoves(node, i).Count>0) res.Add(i);
        }

        return res;
    } 
    List<int> CalculatePseudoValidMoves(ChessNode node, int pos){

        ChessPiece[] board = node.board;

        // Debug.Log("Calculating Pseudo Valid Moves");
        // Debug.Log("Pos = " + pos);

        List<int> res = new List<int>();
        if(board[pos] == ChessPiece.EMPTY){
            return res;
        }

        ChessPiece piece = gameManager.getPiece(board[pos]);
        ChessPiece color = gameManager.getColor(board[pos]);

        // Debug.Log("Piece = " + piece);
        // Debug.Log("Color = " + color);

        int[,] directions = new int[,]{
            {-1,0},{1,0},{0,-1},{0,1},
            {-1,-1},{1,1},{-1,1},{1,-1}
        };
        
        int row = pos/8;
        int col = pos%8;

        switch(piece){
            case ChessPiece.PAWN:
                if (color == ChessPiece.WHITE) {
                    // Forward moves
                    if(board[(row - 1) * 8 + col]==ChessPiece.EMPTY) {
                        AddMove(node, row - 1, col, ref res, color);
                        if (row == 6 && board[(row - 2) * 8 + col]==ChessPiece.EMPTY) {
                            AddMove(node, row - 2, col, ref res, color);
                        }
                    }
                    // Diagonal caputures
                    if(isRowColValid(row - 1, col - 1)&&board[(row - 1) * 8 + col-1]!=ChessPiece.EMPTY) AddMove(node, row - 1, col - 1, ref res, color);
                    if(isRowColValid(row - 1, col + 1)&&board[(row - 1) * 8 + col+1]!=ChessPiece.EMPTY) AddMove(node, row - 1, col + 1, ref res, color);
                    // If enpassant available
                    if(isRowColValid(row - 1, col - 1)&&((row - 1) * 8 + col-1)==node.getEnpassantTile()) AddMove(node, row - 1, col - 1, ref res, color);
                    if(isRowColValid(row - 1, col + 1)&&((row - 1) * 8 + col+1)==node.getEnpassantTile()) AddMove(node, row - 1, col + 1, ref res, color);

                } else {
                    // Forward moves
                    if(board[(row + 1) * 8 + col]==ChessPiece.EMPTY){
                        AddMove(node, row + 1, col, ref res, color);
                        if (row == 1 && board[(row + 2) * 8 + col]==ChessPiece.EMPTY) {
                            AddMove(node, row + 2, col, ref res, color);
                        }
                    }
                    // Diagonal caputures
                    if(isRowColValid(row + 1, col - 1)&&board[(row + 1) * 8 + col-1]!=ChessPiece.EMPTY) AddMove(node, row + 1, col - 1, ref res, color);
                    if(isRowColValid(row + 1, col + 1)&&board[(row + 1) * 8 + col+1]!=ChessPiece.EMPTY) AddMove(node, row + 1, col + 1, ref res, color);
                    // If enpassant available
                    if(isRowColValid(row + 1, col - 1)&&((row + 1) * 8 + col-1)==node.getEnpassantTile()) AddMove(node, row + 1, col - 1, ref res, color);
                    if(isRowColValid(row + 1, col + 1)&&((row + 1) * 8 + col+1)==node.getEnpassantTile()) AddMove(node, row + 1, col + 1, ref res, color);
                }
                
                break;
            case ChessPiece.ROOK:
                for (int i = 0; i < 4; ++i) {
                    for (int j = 1; j < 8; ++j) {
                        int newRow = row + directions[i,0] * j;
                        int newCol = col + directions[i,1] * j;
                        if (isRowColValid(newRow, newCol)) {
                            int index = newRow * 8 + newCol;
                            if(!AddMove(node, newRow, newCol, ref res, color)) break;
                        } else {
                            break;
                        }
                    }
                }
                break;
            case ChessPiece.KNIGHT:
                for(int i=-2; i<=2; i++){
                    for(int j=-2; j<=2; j++){
                        if(Math.Abs(i)!=Math.Abs(j) && i!=0 && j!=0){
                            int newRow = row + i;
                            int newCol = col + j;
                            AddMove(node, newRow, newCol, ref res, color);
                        }
                    }
                }
                break;
            case ChessPiece.BISHOP:

                // Debug.Log("Bishop found = " + pos);

                for (int i = 4; i < 8; ++i) {
                    for (int j = 1; j < 8; ++j) {
                        int newRow = row + directions[i,0] * j;
                        int newCol = col + directions[i,1] * j;
                        if (isRowColValid(newRow, newCol)) {
                            int index = newRow * 8 + newCol;
                            if(!AddMove(node, newRow, newCol, ref res, color)) break;
                        } else {
                            break;
                        }
                    }
                }

                // string r = "";
                // foreach (int i in res){
                //     r+= i + ", ";
                // }
                // Debug.Log("Bishop res = " + r);
                break;
            case ChessPiece.QUEEN:
                for (int i = 0; i < 8; ++i) {
                    for (int j = 1; j < 8; ++j) {
                        int newRow = row + directions[i,0] * j;
                        int newCol = col + directions[i,1] * j;
                        if (isRowColValid(newRow, newCol)) {
                            int index = newRow * 8 + newCol;
                            if(!AddMove(node, newRow, newCol,ref res, color)) break;
                        } else {
                            break;
                        }
                    }
                }
                break;
            case ChessPiece.KING:
                for (int i = 0; i < 8; ++i) {
                    int newRow = row + directions[i,0];
                    int newCol = col + directions[i,1];
                    AddMove(node, newRow, newCol, ref res, color);
                }

                if(!isInCheck(node, color)){
                    // Castling
                    if(color==ChessPiece.WHITE){
                        if(canKingCastle(node, CastlingRights.WHITE_KING_SIDE)) AddMove(node, row, col+2, ref res, color);
                        if(canKingCastle(node, CastlingRights.WHITE_QUEEN_SIDE)) AddMove(node, row, col-2, ref res, color);
                    } else {
                        if(canKingCastle(node, CastlingRights.BLACK_KING_SIDE)) AddMove(node, row, col+2, ref res, color);
                        if(canKingCastle(node, CastlingRights.BLACK_QUEEN_SIDE)) AddMove(node, row, col-2, ref res, color);
                    }
                    
                }                
                break;
            default:
                break;
        }
        return res;
    }

    HashSet<int> CheckAllPosBeingAttackedBy(ChessNode node, ChessPiece color){
        // Debug.Log("Checking " + color + " Attacks");
        List<int> res = new List<int>();

        ChessPiece[] board = node.board;

        // gameManager._displayBoard(node);

        // Iterate through all squares on the board
        for (int i = 0; i < 64; ++i) {

            // If the square contains a piece of the specified color
            if (node.getPiece(board[i]) != ChessPiece.EMPTY && node.getColor(board[i]) == color) {
                ChessPiece piece = node.getPiece(board[i]);
                int row = i / 8;
                int col = i % 8;
                switch (piece) {
                    // Pawn attacks
                    case ChessPiece.PAWN:
                        if (color == ChessPiece.WHITE) {
                            if (isRowColValid(row - 1, col - 1)) AddMove(node, row - 1, col - 1, ref res, color);
                            if (isRowColValid(row - 1, col + 1)) AddMove(node, row - 1, col + 1, ref res, color);
                        } else {
                            if (isRowColValid(row + 1, col - 1)) AddMove(node, row + 1, col - 1, ref res, color);
                            if (isRowColValid(row + 1, col + 1)) AddMove(node, row + 1, col + 1, ref res, color);
                        }
                        break;
                    // Other pieces use checkValidMoves
                    case ChessPiece.KING:
                        int[,] directions = new int[,]{
                            {-1,0},{1,0},{0,-1},{0,1},
                            {-1,-1},{1,1},{-1,1},{1,-1}
                        };

                        for (int direction = 0; direction < 8; direction++) {
                            int newRow = row + directions[direction,0];
                            int newCol = col + directions[direction,1];
                            AddMove(node, newRow, newCol, ref res, color);
                        }
                        break;
                    default:
                        List<int> moves = CalculatePseudoValidMoves(node, i);
                        foreach(int move in moves) res.Add(move);
                        break;
                }
            }
        }

        HashSet<int> result = new HashSet<int>(res);
        return result;
    }

    List<int> FilterValidMoves(ChessNode node, int pos, List<int> valids){
        List<int> filtered = new List<int>();
        foreach(int i in valids){
            if(!isInCheckAfterMoving(node, pos, i)) filtered.Add(i);
        }
        return filtered;
    }

    int NumOfValidMoves(ChessNode node, ChessPiece colorToCheck){
        int count = 0;  

        ChessPiece[] board = gameManager.getBoard();

        for(int i=0; i<64; i++){
            //ChessPiece piece = gameManager.getPiece(board[i]);
            ChessPiece color = gameManager.getColor(board[i]);
            if(color == colorToCheck){

                // Calculate valid moves
                List<int> validPositions = CalculatePseudoValidMoves(node, i);
                List<int> filteredValidPositions = FilterValidMoves(node, i, validPositions);
                
                count+=filteredValidPositions.Count;
            }
        }

        return count;
    }
    public List<int> GetLegalMoves(ChessNode node, int pos){
        // gameManager._toStringBoard(node);

        List<int> validPositions = CalculatePseudoValidMoves(node, pos);
        
        // string res = "POS = " + pos + "\nvalidPositions = ";
        // foreach(int i in validPositions){
        //     res+=(i + ", ");
        // }

        List<int> filteredValidPositions = FilterValidMoves(node, pos, validPositions);

        // res+="\nfilteredValidPositions = ";
        // foreach(int i in filteredValidPositions){
        //     res+=(i + ", ");
        // }

        // if(pos == 33) Debug.Log(res);

        return filteredValidPositions;
    }

    // Check/Checkmate related methods
    bool isInCheck(ChessNode node, ChessPiece color){
        
        // // Debug
        // Debug.Log("In is in check!");
        // gameManager._displayBoard(node);
        // // Debug

        ChessPiece enemyColor = ChessPiece.WHITE;
        if(color == ChessPiece.WHITE) enemyColor = ChessPiece.BLACK;

        // // Debug
        // Debug.Log("Color to check = " + enemyColor);
        // // Debug

        HashSet<int> enemyAttacks = CheckAllPosBeingAttackedBy(node, enemyColor);

        ChessPiece[] board = node.board;

        // // Debug
        // Debug.Log("Check the table:"); 
        // gameManager._displayBoard(node);
        // string enemyA = "Enemy attacks: ";
        // foreach(int i in enemyAttacks){
        //     enemyA += (i + ", ");
        // }
        // Debug.Log(enemyA); 
        // // Debug

        foreach(int i in enemyAttacks){
            if(node.getColor(board[i])==color && node.getPiece(board[i])==ChessPiece.KING){
                return true;
            }
        }

        return false;
    }
    bool isInCheckAfterMoving(ChessNode node, int ori, int pos){
        // bool inCheck = true;

        // if(ori == 33 && pos ==26 ){}
        // Debug.Log("Entering isInCheckAfterMoving");
        
        ChessPiece[] board = node.board;

        ChessPiece oriPiece, posPiece, tempPiece = ChessPiece.EMPTY; 
        
        oriPiece = board[ori];
        posPiece = board[pos];

        ChessPiece color = gameManager.getColor(board[ori]);
        ChessPiece piece = gameManager.getPiece(board[ori]);

        if(piece == ChessPiece.PAWN && pos == node.enPassantTile){
            
            board[pos] = board[ori];
            board[ori] = ChessPiece.EMPTY;
            tempPiece = board[node.enPassantPawnPosition];
            board[node.enPassantPawnPosition] = ChessPiece.EMPTY;
            
        } else {
            board[pos] = board[ori];
            board[ori] = ChessPiece.EMPTY;
        }
        
        bool inCheck = isInCheck(node, color);

        if(piece == ChessPiece.PAWN && pos == node.enPassantTile){
            board[pos] = posPiece;
            board[ori] = oriPiece;
            board[node.enPassantPawnPosition] = tempPiece;
        } else {
            board[pos] = posPiece;
            board[ori] = oriPiece;
        }

    
        // if(ori == 33 && pos ==26) {
        //     Debug.Log(inCheck);
        //     gameManager._displayBoard(node);
        // }

        // debugRes+= inCheck;
        // if(ori == 33 && pos == 26){
        //     Debug.Log(debugRes);
        // }

        // Debug.Log("Incheck = " + inCheck + " ori pos = " + ori + ", " + pos);
        

        if(inCheck){
            return true;
        }  
        return false;
    }
    public void CheckForCheckMate(ChessNode node){
        // Checks for checkmate/stalemate
        int validMovesW = NumOfValidMoves(node, ChessPiece.WHITE);
        int validMovesB = NumOfValidMoves(node, ChessPiece.BLACK);

        //Debug.Log("Turn = " + node.getTurn());

        if(validMovesW==0&&node.getTurn()==ChessPiece.WHITE) {
            uIManager.displayResult(isInCheck(node, ChessPiece.WHITE),false);
            node.isGameEnd = true;
        }

        if(validMovesB==0&&node.getTurn()==ChessPiece.BLACK) {
            uIManager.displayResult(isInCheck(node, ChessPiece.BLACK),true);
            node.isGameEnd = true;
        }

        if((node.getHalfMoveCount() - Math.Min(node.getLastCapturedTurn(), node.getLastPawnMovedTurn()) == 100)) {
            uIManager.displayResult(false,false);
            node.isGameEnd = true;
        }
    }

    public bool isCheckMate(ChessNode node, ChessPiece color){
        int validMovesW = NumOfValidMoves(node, ChessPiece.WHITE);
        int validMovesB = NumOfValidMoves(node, ChessPiece.BLACK);

        if(validMovesW==0&&node.getTurn()==ChessPiece.WHITE) return true;
        if(validMovesB==0&&node.getTurn()==ChessPiece.BLACK) return true;

        return false;
    }

    // Moving piece related methods
    public ChessNode MoveNodePiece(ChessNode node, int oriPos, int newPos, int pawnOption = 0){
        ChessNode oldNode = new ChessNode(node);
        ChessPiece[] board = node.board;
        ChessPiece piece = node.getPiece(board[oriPos]);
        ChessPiece color = node.getColor(board[oriPos]);
        // If target position is empty, move to empty space
        
        if(board[newPos] == ChessPiece.EMPTY){
            // All pawn logics, including promotion, enpassant, move 2, move 1
            if(piece == ChessPiece.PAWN){
                // If it's pawn and reached last row, promote piece
                if((newPos >=0 && newPos <8) || (newPos >=56 && newPos <64)){
                    
                    board[newPos] = PawnPromotion(node, color, pawnOption); // PROMOTION - Need to separate this for it's own function
                    board[oriPos] = ChessPiece.EMPTY;
                    
                    node.enPassantTile = -1;
                    node.enPassantPawnPosition = -1;
                    node.halfMoveCount++;
                    
                    node.lastPawnMovedTurn = node.halfMoveCount;
                    node.changeTurn();
                    return oldNode;
                }

                // If it's pawn and attack sqaure can be enpassant
                if(newPos == node.enPassantTile){
                    board[newPos] = board[oriPos];
                    board[oriPos] = 0;
                    CapturePiece(node, node.enPassantPawnPosition);
                    node.enPassantTile = -1;
                    node.enPassantPawnPosition = -1;
                    node.halfMoveCount++;

                    node.lastPawnMovedTurn = node.halfMoveCount;
                    node.changeTurn();
                    return oldNode;
                }

                // If it's pawn and moved 2 squares mark enpassant info
                if(Math.Abs(newPos-oriPos)==16){
                    board[newPos] = board[oriPos];
                    board[oriPos] = ChessPiece.EMPTY;
                    node.enPassantTile = (oriPos+newPos)/2;
                    node.enPassantPawnPosition = newPos;
                    node.halfMoveCount++;

                    node.lastPawnMovedTurn = node.halfMoveCount;
                    node.changeTurn();
                    return oldNode;
                }

                // If it's pawn and moved 1 square
                board[newPos] = board[oriPos];
                board[oriPos] = 0;
                node.enPassantTile = -1;
                node.enPassantPawnPosition = -1;
                node.halfMoveCount++;

                node.lastPawnMovedTurn = node.halfMoveCount;
                node.changeTurn();
                return oldNode;
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
                    node.canCastle[(int)CastlingRights.WHITE_KING_SIDE] = false;
                    node.canCastle[(int)CastlingRights.WHITE_QUEEN_SIDE] = false;
                } else {
                    node.canCastle[(int)CastlingRights.BLACK_KING_SIDE] = false;
                    node.canCastle[(int)CastlingRights.BLACK_QUEEN_SIDE] = false;
                }
            }

            // If rook moves, remove the castle availability of that side
            if(piece == ChessPiece.ROOK){ //&& !testingMode
                if(color==ChessPiece.WHITE){
                    if(oriPos == 56) node.canCastle[(int)CastlingRights.WHITE_QUEEN_SIDE] = false;
                    if(oriPos == 63) node.canCastle[(int)CastlingRights.WHITE_KING_SIDE] = false;
                } else {
                    if(oriPos == 0) node.canCastle[(int)CastlingRights.BLACK_QUEEN_SIDE] = false;
                    if(oriPos == 7) node.canCastle[(int)CastlingRights.BLACK_KING_SIDE] = false;
                }
            }

            // All other piece when moving to empty square
            board[newPos] = board[oriPos];
            board[oriPos] = 0;
            node.enPassantTile = -1;
            node.enPassantPawnPosition = -1;
            node.halfMoveCount++;
            node.changeTurn();
            return oldNode;
        }  

        // Promotion and capture
        if((piece == ChessPiece.PAWN) && ((newPos >=0 && newPos <8) || (newPos >=56 && newPos <64))) {
            CapturePiece(node, newPos);
            board[newPos] = PawnPromotion(node, color, pawnOption); // PROMOTION - Need to separate this for it's own function
            board[oriPos] = ChessPiece.EMPTY;
            node.lastPawnMovedTurn = node.halfMoveCount+1;

        } else{
            // All other standard capture
            // If there is a piece on the target position, capture enemy piece
            CapturePiece(node, newPos); //capturedPieces.push_back(board[newPos]);
            board[newPos] = board[oriPos];
            board[oriPos] = 0;
        }

        node.enPassantTile = -1;
        node.enPassantPawnPosition = -1;
        node.halfMoveCount++;
        node.changeTurn();
        return oldNode;
    }
    public ChessPiece PawnPromotion(ChessNode node, ChessPiece color, int option){
        ChessPiece res = new ChessPiece();
        switch(option){
            case 0: 
                res = color | ChessPiece.QUEEN;
                break;
            case 1: 
                res = color | ChessPiece.ROOK;
                break;
            case 2: 
                res = color | ChessPiece.KNIGHT;
                break;
            default: 
                res = color | ChessPiece.BISHOP;
                break;
        }
        return res;

    }
    public void CapturePiece(ChessNode node, int newPos){
        if(node.getPiece(node.board[newPos]) == ChessPiece.ROOK){
            switch(newPos){
                case 0:
                    node.canCastle[(int)CastlingRights.BLACK_QUEEN_SIDE] = false;
                    break;
                case 7:
                    node.canCastle[(int)CastlingRights.BLACK_KING_SIDE] = false;
                    break;
                case 56:
                    node.canCastle[(int)CastlingRights.WHITE_QUEEN_SIDE] = false;
                    break;
                case 63:
                    node.canCastle[(int)CastlingRights.WHITE_KING_SIDE] = false;
                    break;
                default: break;
            }
        }
        node.capturedPieces.Add(node.board[newPos]);
        node.board[newPos] = ChessPiece.EMPTY;

        node.lastCapturedTurn = node.halfMoveCount;
    } 
    bool AddMove(ChessNode node, int r, int c, ref List<int> positions, ChessPiece color){
        if(isRowColValid(r,c)){
            int pos = r*8 + c;
            ChessPiece piece = node.board[pos];
            if(piece==ChessPiece.EMPTY || gameManager.getColor(piece)!=color) {
                positions.Add(pos);
                return piece == ChessPiece.EMPTY;
            }
        }
        return false;
    }
    bool isRowColValid(int r, int c){
        return r>=0 && r<8 && c>=0 && c<8;
    }
    public void MovePiece(ChessNode node, int oriPos, int newPos, int pawnOption = -1){
        ChessPiece oriPiece = node.board[oriPos];
        
        if(pawnOption!=-1){
            gameManager.AddToMoveHistory(MoveNodePiece(node, oriPos, newPos, pawnOption));
        } else {
            gameManager.AddToMoveHistory(MoveNodePiece(node, oriPos, newPos));
        }

        uIManager.displayGameInfo();
        displayManager.UpdateDisplay();
    }
    public void UndoLastMove(){
        if(gameManager.currGame.halfMoveCount < 1) return;
        else if(gameManager.currGame.halfMoveCount == 1) {
            gameManager.UndoLastHalfMove();
        } else {
            gameManager.UndoLastFullMove();
        }
        
        uIManager.displayGameInfo();
        displayManager.UpdateDisplay();
    }

    // Resets if Didn't click on the right piece/tile
    void ResetThisTurn(){
        if(selected!=-1){ 
            selected = -1;
            movingTo = -1;
            validTiles.Clear();
        } 
        displayManager.ResetAllTileColor();
    }

    // Perft related
    public void Perft(ChessNode node, int depth){
        Queue<ChessNode> nodeQueue = new Queue<ChessNode>();
        ChessNode dupNode = new ChessNode(node);
        nodeQueue.Enqueue(dupNode);

        for(int i=0; i<depth; i++){
            // WriteDataToFile("Depth = " + i + "\n");

            // Time measurement
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // int count = 0;
            Queue<ChessNode> childrenNodeQueue = new Queue<ChessNode>();

            // BFS
            while(nodeQueue.Count!=0){
                ChessNode currNode = nodeQueue.Dequeue();
                List<int> allPos = getAllMovablePieces(currNode, currNode.getTurn());
                foreach(int pos in allPos){
                    // WriteDataToFile("NODE STEMMING FROM " + pos + "\n");
                    // WriteDataToFile(gameManager._toStringBoard(currNode));
                    // int nodeRes =  0;
                    List<int> LegalMoves = GetLegalMoves(currNode, pos);
                    // count+=LegalMoves.Count;
                    foreach(int move in LegalMoves){
                        //If dealing with pawn promotion
                        // WriteDataToFile("Ori = " + pos + " New = " + move);
                        
                        if((gameManager.getPiece(currNode.board[pos])==ChessPiece.PAWN)&&((move>=0&&move<8)||(move>=56&&move<64))){
                            for(int option=0; option<4; option++){
                                ChessNode newNode = new ChessNode(currNode);
                                MoveNodePiece(newNode, pos, move, option);
                                childrenNodeQueue.Enqueue(newNode);
                                // nodeRes++;
                                // WriteDataToFile(gameManager._toStringBoard(newNode));
                            }
                        } else{
                            ChessNode newNode = new ChessNode(currNode);
                            MoveNodePiece(newNode, pos, move);
                            childrenNodeQueue.Enqueue(newNode);
                            // nodeRes++;
                            // WriteDataToFile(gameManager._toStringBoard(newNode));
                        }
                    }

                    // WriteDataToFile("Node count for pos " + pos + " = " + nodeRes + "\n\n");
                    // Debug.Log("Node pos = " + pos + ": " + nodeRes);
                }
            }
            
            while(childrenNodeQueue.Count!=0){
                nodeQueue.Enqueue(childrenNodeQueue.Dequeue());
            }

            stopwatch.Stop();
            // UnityEngine.Debug.Log(count + " positions found! Depth = " + i);
            System.TimeSpan ts = stopwatch.Elapsed;
            // Format and display the elapsed time
            string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            UnityEngine.Debug.Log("RunTime " + elapsedTime);

            UnityEngine.Debug.Log(nodeQueue.Count + " nodes found! Depth = " + i);
            // WriteDataToFile((count + " positions found! Depth = " + i));
        } 
    }

    // Debug methods
    // public void WriteDataToFile(string data, string directory = "", string fileName = "perft_results.txt") {
    //     // Combine the directory and file name to get the full path
    //     string path = Path.Combine(directory, fileName);

    //     // Ensure the directory exists
    //     if (!Directory.Exists(directory)) {
    //         Directory.CreateDirectory(directory);
    //     }

    //     // Write data to the file
    //     //File.WriteAllText(path, data);
    //     File.AppendAllText(path, data + "\n");

    //     // Optionally, log the path to debug
    //     // Debug.Log($"Data written to: {path}");
    // }

}
