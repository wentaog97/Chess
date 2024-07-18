using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Serve as the controller in MVC
public class MovesManager : MonoBehaviour
{
    public GameManager gameManager;
    public DisplayManager displayManager;
    public UIManager uIManager;
    int selected = -1;
    HashSet<GameObject> validTiles = new HashSet<GameObject>();

    public ChessPiece player1Color = ChessPiece.WHITE, player2Color = ChessPiece.BLACK;
    
    public ChessBot bot1, bot2;

    public bool isBot1 = false, isBot2 = false;

    // Update is called once per frame
    void Update()
    {   
        if(!gameManager.isGameEnd){
            if (gameManager.getTurn()==player1Color) {
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
            ResetThisTurn();
            return;
        } 

        int pos = CalculateTilePosition(objectClicked);
        
        // No selected piece, we need to check if clicked is a selectable piece
        if(selected == -1) { 
            
            ChessPiece clickedPosition = gameManager.getBoard()[pos];

            if(clickedPosition!=ChessPiece.EMPTY){  // If have a piece, select it

                ChessPiece turn = gameManager.getTurn();
                ChessPiece color = gameManager.getColor(clickedPosition);

                if(turn == color){
                    selected = pos;

                    // Highlight selected piece
                    displayManager.HighlightTile(pos);

                    // Highlight all valid tile
                    foreach (int validPos in GetLegalMoves(selected)){
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
        
        // In here we will have piece selected, we need to check if the tile clicked is a valid move
        if(isClickedValidTile(objectClicked)){
            MovePiece(selected, pos);
            // Change Turn after move has been made
            gameManager.changeTurn();
        } 

        // Reset selected and wipe all highlights
        ResetThisTurn();

        // If reach end game, display it
        CheckForCheckMate();

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
            if (hit.CompareTag("Tile"))
            {
                clickedObject = hit.gameObject;
                break;
            }
        }
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
    bool canKingCastle(CastlingRights side) {
        int kingPos, rookPos, start, finish;
        HashSet<int> underAttack = null;
        
        switch(side){
            case CastlingRights.WHITE_KING_SIDE:
                kingPos = 60; rookPos = 63; start = 60; finish = 62;
                underAttack = CheckAllPosBeingAttackedBy(ChessPiece.BLACK);
                break;
            case CastlingRights.WHITE_QUEEN_SIDE: 
                kingPos = 60; rookPos = 56; start = 58; finish = 60;
                underAttack = CheckAllPosBeingAttackedBy(ChessPiece.BLACK);
                break;
            case CastlingRights.BLACK_KING_SIDE: 
                kingPos = 4; rookPos = 7; start = 4; finish = 6; 
                underAttack = CheckAllPosBeingAttackedBy(ChessPiece.WHITE);
                break;
            case CastlingRights.BLACK_QUEEN_SIDE: 
                kingPos = 4; rookPos = 0; start = 2; finish = 4; 
                underAttack = CheckAllPosBeingAttackedBy(ChessPiece.WHITE);
                break;
            default:    
                return false;
        }
        
        if(isPieceInPath(kingPos,rookPos)) return false;

        for(int i=start; i<=finish; i++){
            if(underAttack.Contains(i)) return false;
        }

        if (!gameManager.getCanCastle(side)) return false;

        return true;
    }
    bool isPieceInPath(int oriPos, int newPos) {
        ChessPiece[] board = gameManager.getBoard();
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
        
        // // DEBUG
        // cout << "rs = " << rowStep << " cs = " << colStep << endl;
        // // DEBUG


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
    List<int> CalculatePseudoValidMoves(int pos){

        ChessPiece[] board = gameManager.getBoard();

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
                        AddMove(row - 1, col, ref res, color);
                        if (row == 6 && board[(row - 2) * 8 + col]==ChessPiece.EMPTY) {
                            AddMove(row - 2, col, ref res, color);
                        }
                    }
                    // Diagonal caputures
                    if(isRowColValid(row - 1, col - 1)&&board[(row - 1) * 8 + col-1]!=ChessPiece.EMPTY) AddMove(row - 1, col - 1, ref res, color);
                    if(isRowColValid(row - 1, col + 1)&&board[(row - 1) * 8 + col+1]!=ChessPiece.EMPTY) AddMove(row - 1, col + 1, ref res, color);
                    // If enpassant available
                    if(isRowColValid(row - 1, col - 1)&&((row - 1) * 8 + col-1)==gameManager.getEnpassantTile()) AddMove(row - 1, col - 1, ref res, color);
                    if(isRowColValid(row - 1, col + 1)&&((row - 1) * 8 + col+1)==gameManager.getEnpassantTile()) AddMove(row - 1, col + 1, ref res, color);

                } else {
                    // Forward moves
                    if(board[(row + 1) * 8 + col]==ChessPiece.EMPTY){
                        AddMove(row + 1, col, ref res, color);
                        if (row == 1 && board[(row + 2) * 8 + col]==ChessPiece.EMPTY) {
                            AddMove(row + 2, col, ref res, color);
                        }
                    }
                    // Diagonal caputures
                    if(isRowColValid(row + 1, col - 1)&&board[(row + 1) * 8 + col-1]!=ChessPiece.EMPTY) AddMove(row + 1, col - 1, ref res, color);
                    if(isRowColValid(row + 1, col + 1)&&board[(row + 1) * 8 + col+1]!=ChessPiece.EMPTY) AddMove(row + 1, col + 1, ref res, color);
                    // If enpassant available
                    if(isRowColValid(row + 1, col - 1)&&((row + 1) * 8 + col-1)==gameManager.getEnpassantTile()) AddMove(row + 1, col - 1, ref res, color);
                    if(isRowColValid(row + 1, col + 1)&&((row + 1) * 8 + col+1)==gameManager.getEnpassantTile()) AddMove(row + 1, col + 1, ref res, color);
                }
                
                break;
            case ChessPiece.ROOK:
                for (int i = 0; i < 4; ++i) {
                    for (int j = 1; j < 8; ++j) {
                        int newRow = row + directions[i,0] * j;
                        int newCol = col + directions[i,1] * j;
                        if (isRowColValid(newRow, newCol)) {
                            int index = newRow * 8 + newCol;
                            if(!AddMove(newRow, newCol, ref res, color)) break;
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
                            AddMove(newRow, newCol, ref res, color);
                        }
                    }
                }
                break;
            case ChessPiece.BISHOP:
                for (int i = 4; i < 8; ++i) {
                    for (int j = 1; j < 8; ++j) {
                        int newRow = row + directions[i,0] * j;
                        int newCol = col + directions[i,1] * j;
                        if (isRowColValid(newRow, newCol)) {
                            int index = newRow * 8 + newCol;
                            if(!AddMove(newRow, newCol, ref res, color)) break;
                        } else {
                            break;
                        }
                    }
                }
                break;
            case ChessPiece.QUEEN:
                for (int i = 0; i < 8; ++i) {
                    for (int j = 1; j < 8; ++j) {
                        int newRow = row + directions[i,0] * j;
                        int newCol = col + directions[i,1] * j;
                        if (isRowColValid(newRow, newCol)) {
                            int index = newRow * 8 + newCol;
                            if(!AddMove(newRow, newCol,ref res, color)) break;
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
                    AddMove(newRow, newCol, ref res, color);
                }

                if(!isInCheck(color)){
                    // Castling
                    if(color==ChessPiece.WHITE){
                        if(canKingCastle(CastlingRights.WHITE_KING_SIDE)) AddMove(row, col+2, ref res, color);
                        if(canKingCastle(CastlingRights.WHITE_QUEEN_SIDE)) AddMove(row, col-2, ref res, color);
                    } else {
                        if(canKingCastle(CastlingRights.BLACK_KING_SIDE)) AddMove(row, col+2, ref res, color);
                        if(canKingCastle(CastlingRights.BLACK_QUEEN_SIDE)) AddMove(row, col-2, ref res, color);
                    }
                    
                }                
                break;
            default:
                break;
        }
        return res;
    }
    HashSet<int> CheckAllPosBeingAttackedBy(ChessPiece color){
        
        List<int> res = new List<int>();

        ChessPiece[] board = gameManager.getBoard();

        // Iterate through all squares on the board
        for (int i = 0; i < 64; ++i) {

            // If the square contains a piece of the specified color
            if (gameManager.getPiece(board[i]) != ChessPiece.EMPTY && gameManager.getColor(board[i]) == color) {
                ChessPiece piece = gameManager.getPiece(board[i]);
                int row = i / 8;
                int col = i % 8;
                switch (piece) {
                    // Pawn attacks
                    case ChessPiece.PAWN:
                        if (color == ChessPiece.WHITE) {
                            if (isRowColValid(row - 1, col - 1)) AddMove(row - 1, col - 1, ref res, color);
                            if (isRowColValid(row - 1, col + 1)) AddMove(row - 1, col + 1, ref res, color);
                        } else {
                            if (isRowColValid(row + 1, col - 1)) AddMove(row + 1, col - 1, ref res, color);
                            if (isRowColValid(row + 1, col + 1)) AddMove(row + 1, col + 1, ref res, color);
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
                            AddMove(newRow, newCol, ref res, color);
                        }
                        break;
                    default:
                        List<int> moves = CalculatePseudoValidMoves(i);
                        foreach(int move in moves) res.Add(move);
                        break;
                }
            }
        }

        HashSet<int> result = new HashSet<int>(res);
        return result;
    }
    List<int> FilterValidMoves(int pos, List<int> valids){
        List<int> filtered = new List<int>();
        foreach(int i in valids){
            if(!isInCheckAfterMoving(pos,i)) filtered.Add(i);
        }
        return filtered;
    }
    int NumOfValidMoves(ChessPiece colorToCheck){
        int count = 0;  

        ChessPiece[] board = gameManager.getBoard();

        for(int i=0; i<64; i++){
            //ChessPiece piece = gameManager.getPiece(board[i]);
            ChessPiece color = gameManager.getColor(board[i]);
            if(color == colorToCheck){

                // Calculate valid moves
                List<int> validPositions = CalculatePseudoValidMoves(i);
                List<int> filteredValidPositions = FilterValidMoves(i, validPositions);
                
                count+=filteredValidPositions.Count;
            }
        }

        return count;
    }
    public List<int> GetLegalMoves(int pos){
        List<int> validPositions = CalculatePseudoValidMoves(pos);
        List<int> filteredValidPositions = FilterValidMoves(pos, validPositions);
        return filteredValidPositions;
    }

    // Check/Checkmate related methods
    bool isInCheck(ChessPiece color){
        ChessPiece enemyColor = ChessPiece.WHITE;
        if(color == ChessPiece.WHITE) enemyColor = ChessPiece.BLACK;

        HashSet<int> enemyAttacks = CheckAllPosBeingAttackedBy(enemyColor);
        ChessPiece[] board = gameManager.getBoard();
        foreach(int i in enemyAttacks){
            if(gameManager.getColor(board[i])==color && gameManager.getPiece(board[i])==ChessPiece.KING){
                return true;
            }
        }

        return false;
    }
    bool isInCheckAfterMoving(int ori, int pos){
        ChessPiece[] board = gameManager.getBoard();

        ChessPiece oriPiece, posPiece; 
        oriPiece = board[ori];
        posPiece = board[pos];
        
        ChessPiece color = gameManager.getColor(board[ori]);
        ChessPiece piece = gameManager.getPiece(board[ori]);

        board[pos] = board[ori];
        board[ori] = ChessPiece.EMPTY;
        
        if(isInCheck(color)){
            board[pos] = posPiece;
            board[ori] = oriPiece;
            return true;
        }

        board[pos] = posPiece;
        board[ori] = oriPiece;  
        return false;
    }
    public void CheckForCheckMate(){
        // Checks for checkmate/stalemate
        int validMovesW = NumOfValidMoves(ChessPiece.WHITE);
        int validMovesB = NumOfValidMoves(ChessPiece.BLACK);

        Debug.Log("Turn = " + gameManager.getTurn());

        if(validMovesW==0&&gameManager.getTurn()==ChessPiece.WHITE) {
            uIManager.displayResult(isInCheck(ChessPiece.WHITE),false);
            gameManager.isGameEnd = true;
        }

        if(validMovesB==0&&gameManager.getTurn()==ChessPiece.BLACK) {
            uIManager.displayResult(isInCheck(ChessPiece.BLACK),true);
            gameManager.isGameEnd = true;
        }

        if((gameManager.getHalfMoveCount() - Math.Min(gameManager.getLastCapturedTurn(), gameManager.getLastPawnMovedTurn()) == 100)) {
            uIManager.displayResult(false,false);
            gameManager.isGameEnd = true;
        }
    }

    // Moving piece related methods
    bool AddMove(int r, int c, ref List<int> positions, ChessPiece color){
        if(isRowColValid(r,c)){
            int pos = r*8 + c;
            ChessPiece piece = gameManager.getBoard()[pos];
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
    public void MovePiece(int oriPos, int newPos){
        ChessPiece oriPiece = gameManager.getBoard()[oriPos];
        
        // Move to empty tile
        if(gameManager.getBoard()[newPos]!=ChessPiece.EMPTY) {
            // gameManager.CapturePiece(pos);
            displayManager.CapturePiece(newPos);
        }

        // Pawn Logics
        if(gameManager.getPiece(oriPiece)==ChessPiece.PAWN){

            if(newPos == gameManager.getEnpassantTile()){
                // Debug.Log("Pos = " + gameManager.getEnpassantPawnPosition());
                displayManager.CapturePiece(gameManager.getEnpassantPawnPosition());
            }

            // Pawn Promotion
            if((newPos>=0&&newPos<8)||(newPos>=56&&newPos<64)){
                displayManager.PawnPromotion(oriPos);
            }
        }

        // Castling
        if(gameManager.getPiece(oriPiece)==ChessPiece.KING){
            if(Math.Abs(newPos-oriPos)==2){
                displayManager.Castling(oriPos, newPos);
            }
        }

        gameManager.MovePiece(oriPos, newPos);
        displayManager.MovePiece(oriPos, newPos);

        //gameManager._displayBoard();
    }

    // Resets if Didn't click on the right piece/tile
    void ResetThisTurn(){
        if(selected!=-1){ 
            selected = -1;
            validTiles.Clear();
        } 
        displayManager.ResetAllTileColor();
    }


    // Perft related
    void Perft(int depth){
        int count = 0;

        ChessNode curr = gameManager.toNode();

        Debug.Log(count + " nodes found! Depth = " + depth);
    }
}
