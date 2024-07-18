using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Serve as the controller in MVC
public class MovesManager : MonoBehaviour
{
    public DisplayManager displayManager;
    public UIManager uIManager;
    int selected = -1;

    // bool gameEnded = false;
   
    //List<GameObject> tiles;
    HashSet<GameObject> validTiles = new HashSet<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Check if the left mouse button is clicked
        {   
            GameObject objectClicked = DetectClickedObject(); // Will only return if a tile has been clicked

            if(objectClicked == null) { // If the player clicked somewhere else other than the board, reset
                ResetThisTurn();
            } else {
                // The player clicked on a tile for sure here
                // Now branch into 2 decisions
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
                int pos = CalculateTilePosition(objectClicked);
                if(selected == -1) { // No selected piece, we need to check if clicked is a selectable piece
                    
                    ChessPiece[] board = GameManager.instance.getBoard();
                    // Debug.Log("Clicked " + pos);

                    if(board[pos]!=ChessPiece.EMPTY){  // If have a piece, select it
                        ChessPiece turn = GameManager.instance.getTurn();
                        ChessPiece color = GameManager.instance.getColor(board[pos]);
                        if(turn == color){
                            selected = pos;
                            // Debug.Log("Selected " + selected);

                            // Highlight current piece
                            displayManager.HighlightTile(pos);

                            // Calculate valid moves
                            List<int> validPositions = CalculateValidMoves(selected);
                            List<int> filteredValidPositions = FilterValidMoves(selected, validPositions);
                            
                            // Highlight all valid tile
                            foreach (int validPos in filteredValidPositions){
                                validTiles.Add(displayManager.getTile(validPos));
                                displayManager.HighlightTile(validPos);
                            }
                        }
                        // If color doesn't match turn do nothing
                    } else {  // If clicked on an empty tile, reset turn
                        // Debug.Log("Selected empty tile" + selected);
                        ResetThisTurn();
                    }
                } else { // Already have piece selected, we need to check if the tile clicked is a valid move
                    if(CheckClickedValidTile(objectClicked)){
                        
                        MovePiece(selected, pos);

                        // Change Turn after move has been made
                        GameManager.instance.changeTurn();
                        
                        ResetThisTurn();
                    } else {
                        ResetThisTurn();
                    }
                }
            }

            int validMovesW = NumOfValidMoves(ChessPiece.WHITE);
            // Debug.Log("Valids for WHITE = " + validMovesW);
            int validMovesB = NumOfValidMoves(ChessPiece.BLACK);
            // Debug.Log("Valids for BLACK = " + validMovesB);
            
            if(validMovesW==0) {
                uIManager.displayResult(isInCheck(ChessPiece.WHITE),false);
                // gameEnded = true;
            }

            if(validMovesB==0) {
                uIManager.displayResult(isInCheck(ChessPiece.BLACK),true);
                // gameEnded = true;
            }

            uIManager.displayGameInfo();

            GameManager.instance._displayInfo();
        }

    }


    int NumOfValidMoves(ChessPiece colorToCheck){
        int count = 0;  

        ChessPiece[] board = GameManager.instance.getBoard();

        for(int i=0; i<64; i++){
            //ChessPiece piece = GameManager.instance.getPiece(board[i]);
            ChessPiece color = GameManager.instance.getColor(board[i]);
            if(color == colorToCheck){

                // Calculate valid moves
                List<int> validPositions = CalculateValidMoves(i);
                List<int> filteredValidPositions = FilterValidMoves(i, validPositions);
                
                count+=filteredValidPositions.Count;
            }
        }

        return count;
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

    bool CheckClickedValidTile(GameObject tile){
        return validTiles.Contains(tile);
    }

    int CalculateTilePosition(GameObject tile){
        List<GameObject> tiles = displayManager.getTiles();
        for(int i=0; i<64; i++){
            if(tiles[i]==tile) return i;
        }
        return -1;
    }

    List<int> CalculateValidMoves(int pos){

        ChessPiece[] board = GameManager.instance.getBoard();

        List<int> res = new List<int>();
        if(board[pos] == ChessPiece.EMPTY){
            return res;
        }

        ChessPiece piece = GameManager.instance.getPiece(board[pos]);
        ChessPiece color = GameManager.instance.getColor(board[pos]);

        int[,] directions = new int[,]{
            {-1,0},{1,0},{0,-1},{0,1},
            {-1,-1},{1,1},{-1,1},{1,-1}
        };
        
        int row = pos/8;
        int col = pos%8;
        
        // Debug.Log("Piece = " + piece);
        // Debug.Log("R C = " + row + ", " + col);

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
                    if(isRowColValid(row - 1, col - 1)&&((row - 1) * 8 + col-1)==GameManager.instance.getEnpassantTile()) AddMove(row - 1, col - 1, ref res, color);
                    if(isRowColValid(row - 1, col + 1)&&((row - 1) * 8 + col+1)==GameManager.instance.getEnpassantTile()) AddMove(row - 1, col + 1, ref res, color);

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
                    if(isRowColValid(row + 1, col - 1)&&((row + 1) * 8 + col-1)==GameManager.instance.getEnpassantTile()) AddMove(row + 1, col - 1, ref res, color);
                    if(isRowColValid(row + 1, col + 1)&&((row + 1) * 8 + col+1)==GameManager.instance.getEnpassantTile()) AddMove(row + 1, col + 1, ref res, color);
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
                
                // DEBUG 
                //GameManager.instance._displayInfo();
                
                break;
            default:
                break;
        }

        return res;
    }


    bool canKingCastle(CastlingRights side) {
        int kingPos, rookPos, start, finish;
        HashSet<int> underAttack = null;
        
        switch(side){
            case CastlingRights.WHITE_KING_SIDE:
                kingPos = 60; rookPos = 63; start = 60; finish = 62;
                underAttack = checkAllPosBeingAttackedBy(ChessPiece.BLACK);
                break;
            case CastlingRights.WHITE_QUEEN_SIDE: 
                kingPos = 60; rookPos = 56; start = 58; finish = 60;
                underAttack = checkAllPosBeingAttackedBy(ChessPiece.BLACK);
                break;
            case CastlingRights.BLACK_KING_SIDE: 
                kingPos = 4; rookPos = 7; start = 4; finish = 6; 
                underAttack = checkAllPosBeingAttackedBy(ChessPiece.WHITE);
                break;
            case CastlingRights.BLACK_QUEEN_SIDE: 
                kingPos = 4; rookPos = 0; start = 2; finish = 4; 
                underAttack = checkAllPosBeingAttackedBy(ChessPiece.WHITE);
                break;
            default:    
                return false;
        }
        
        if(pieceInPath(kingPos,rookPos)) return false;

        for(int i=start; i<=finish; i++){
            if(underAttack.Contains(i)) return false;
        }

        if (!GameManager.instance.getCanCastle(side)) return false;

        return true;
    }

    bool pieceInPath(int oriPos, int newPos) {
        ChessPiece[] board = GameManager.instance.getBoard();
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

    List<int> FilterValidMoves(int ori, List<int> valids){
        List<int> filtered = new List<int>();
        foreach(int i in valids){
            if(!isInCheckAfterMoving(ori,i)) filtered.Add(i);
        }
        return filtered;
    }

    bool isInCheckAfterMoving(int ori, int pos){
        ChessPiece[] board = GameManager.instance.getBoard();

        ChessPiece oriPiece, posPiece; 
        oriPiece = board[ori];
        posPiece = board[pos];
        
        ChessPiece color = GameManager.instance.getColor(board[ori]);
        ChessPiece piece = GameManager.instance.getPiece(board[ori]);

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

    bool isInCheck(ChessPiece color){
        ChessPiece enemyColor = ChessPiece.WHITE;
        if(color == ChessPiece.WHITE) enemyColor = ChessPiece.BLACK;

        HashSet<int> enemyAttacks = checkAllPosBeingAttackedBy(enemyColor);
        ChessPiece[] board = GameManager.instance.getBoard();
        foreach(int i in enemyAttacks){
            if(GameManager.instance.getColor(board[i])==color && GameManager.instance.getPiece(board[i])==ChessPiece.KING){
                return true;
            }
        }

        return false;
    }
    HashSet<int> checkAllPosBeingAttackedBy(ChessPiece color){
        
        List<int> res = new List<int>();

        ChessPiece[] board = GameManager.instance.getBoard();

        // Iterate through all squares on the board
        for (int i = 0; i < 64; ++i) {

            // If the square contains a piece of the specified color
            if (GameManager.instance.getPiece(board[i]) != ChessPiece.EMPTY && GameManager.instance.getColor(board[i]) == color) {
                ChessPiece piece = GameManager.instance.getPiece(board[i]);
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
                        // // Debug
                        // cout << "DEBUG - starting with " << i << endl;
                        // // Debug

                        List<int> moves = CalculateValidMoves(i);
                        foreach(int move in moves) res.Add(move);

                        // // Debug
                        // cout << "DEBUG - done with " << i << ", moves: ";
                        // for(int move:moves) cout << move << " ";
                        // cout << endl;
                        // // Debug

                        break;
                }
            }
        }

        HashSet<int> result = new HashSet<int>(res);
        return result;
    }

    bool AddMove(int r, int c, ref List<int> positions, ChessPiece color){
        if(isRowColValid(r,c)){
            int pos = r*8 + c;
            ChessPiece piece = GameManager.instance.getBoard()[pos];
            if(piece==ChessPiece.EMPTY || GameManager.instance.getColor(piece)!=color) {
                positions.Add(pos);
                return piece == ChessPiece.EMPTY;
            }
        }
        return false;
    }

    bool isRowColValid(int r, int c){
        return r>=0 && r<8 && c>=0 && c<8;
    }

    // Resets if Didn't click on the right piece/tile
    void ResetThisTurn(){
        // Debug.Log("Reset This Turn");

        if(selected!=-1){ 
            selected = -1;
            validTiles.Clear();
        } 
        
        displayManager.ResetAllTileColor();
    }

    void MovePiece(int oriPos, int newPos){
        ChessPiece oriPiece = GameManager.instance.getBoard()[oriPos];
        
        // Move to empty tile
        if(GameManager.instance.getBoard()[newPos]!=ChessPiece.EMPTY) {
            // GameManager.instance.CapturePiece(pos);
            displayManager.CapturePiece(newPos);
        }

        // Pawn Logics
        if(GameManager.instance.getPiece(oriPiece)==ChessPiece.PAWN){

            if(newPos == GameManager.instance.getEnpassantTile()){
                // Debug.Log("Pos = " + GameManager.instance.getEnpassantPawnPosition());
                displayManager.CapturePiece(GameManager.instance.getEnpassantPawnPosition());
            }

            // Pawn Promotion
            if((newPos>=0&&newPos<8)||(newPos>=56&&newPos<64)){
                displayManager.PawnPromotion(oriPos);
            }
        }

        // Castling
        if(GameManager.instance.getPiece(oriPiece)==ChessPiece.KING){
            if(Math.Abs(newPos-oriPos)==2){
                displayManager.Castling(oriPos, newPos);
            }
        }

        GameManager.instance.MovePiece(oriPos, newPos);
        displayManager.MovePiece(oriPos, newPos);

        //GameManager.instance._displayBoard();
    }

}
