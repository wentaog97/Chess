using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controller in MVC
public class MovesManager : MonoBehaviour
{
    public DisplayManager displayManager;
    int selected = -1;
    GameObject[] validTiles;
    List<int> validPositions;
    List<GameObject> tiles;

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
                // The player clicked on a tile for sure
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

                if(selected == -1) { // No selected piece, we need to check if clicked is a selectable piece
                    int pos = CalculateTilePosition(objectClicked);
                    
                    ChessPiece[] board = GameManager.instance.getBoard();
                    Debug.Log("Clicked " + pos);

                    if(board[pos]!=ChessPiece.EMPTY){  // If have a piece, select it
                        selected = pos;
                        Debug.Log("Selected " + selected);

                        // Highlight current piece
                        displayManager.HighlightTile(pos);

                        // Calculate valid moves
                        List<int> validPositions = CalculateValidMoves(selected);

                        // DEBUG SECTION
                        string res = "All Pos = ";
                        foreach(int i in validPositions){
                            res += i;
                            res += ' ';
                        }
                        Debug.Log(res);
                        // DEBUG SECTION

                        // Highlight all valid tile
                        foreach (int validPos in validPositions){
                            displayManager.HighlightTile(validPos);
                        }

                    } else {  // If clicked on an empty tile, reset turn
                        Debug.Log("Selected empty tile" + selected);
                        ResetThisTurn();
                    }
                } else { // Already have piece selected, we need to check if the tile clicked is a valid move
                    if(CheckClickedValidTile()){
                        
                    } else {
                        ResetThisTurn();
                    }
                }
            }

        }
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

    bool CheckClickedValidTile(){
        return false;
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
        
        Debug.Log("Piece = " + piece);


        switch(piece){
            case ChessPiece.PAWN:
                if (color == ChessPiece.WHITE) {
                    // Forward moves
                    if(board[(row - 1) * 8 + col]==ChessPiece.EMPTY) {
                        addMove(row - 1, col, ref res, color);
                        if (row == 6 && board[(row - 2) * 8 + col]==ChessPiece.EMPTY) {
                            addMove(row - 2, col, ref res, color);
                        }
                    }
                    // Diagonal caputures
                    if(isRowColValid(row - 1, col - 1)&&board[(row - 1) * 8 + col-1]!=ChessPiece.EMPTY) addMove(row - 1, col - 1, ref res, color);
                    if(isRowColValid(row - 1, col + 1)&&board[(row - 1) * 8 + col+1]!=ChessPiece.EMPTY) addMove(row - 1, col + 1, ref res, color);
                    // If enpassant available
                    if(isRowColValid(row - 1, col - 1)&&((row - 1) * 8 + col-1)==GameManager.instance.getEnpassantTile()) addMove(row - 1, col - 1, ref res, color);
                    if(isRowColValid(row - 1, col + 1)&&((row - 1) * 8 + col+1)==GameManager.instance.getEnpassantTile()) addMove(row - 1, col + 1, ref res, color);

                } else {
                    // Forward moves
                    if(board[(row + 1) * 8 + col]==ChessPiece.EMPTY){
                        addMove(row + 1, col, ref res, color);
                        if (row == 1 && board[(row + 2) * 8 + col]==ChessPiece.EMPTY) {
                            addMove(row + 2, col, ref res, color);
                        }
                    }
                    // Diagonal caputures
                    if(isRowColValid(row + 1, col - 1)&&board[(row + 1) * 8 + col-1]!=ChessPiece.EMPTY) addMove(row + 1, col - 1, ref res, color);
                    if(isRowColValid(row + 1, col + 1)&&board[(row + 1) * 8 + col+1]!=ChessPiece.EMPTY) addMove(row + 1, col + 1, ref res, color);
                    // If enpassant available
                    if(isRowColValid(row + 1, col - 1)&&((row + 1) * 8 + col-1)==GameManager.instance.getEnpassantTile()) addMove(row + 1, col - 1, ref res, color);
                    if(isRowColValid(row + 1, col + 1)&&((row + 1) * 8 + col+1)==GameManager.instance.getEnpassantTile()) addMove(row + 1, col + 1, ref res, color);
                }
                
                break;
            case ChessPiece.ROOK:
                for (int i = 0; i < 4; ++i) {
                    for (int j = 1; j < 8; ++j) {
                        int newRow = row + directions[i,0] * j;
                        int newCol = col + directions[i,1] * j;
                        if (isRowColValid(newRow, newCol)) {
                            int index = newRow * 8 + newCol;
                            if(!addMove(newRow, newCol, ref res, color)) break;
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
                            addMove(newRow, newCol, ref res, color);
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
                            if(!addMove(newRow, newCol, ref res, color)) break;
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
                            if(!addMove(newRow, newCol,ref res, color)) break;
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
                    addMove(newRow, newCol, ref res, color);
                }
                /*
                // Terminate for the purpose of checking attack positions
                ChessPiece turn = GameManager.instance.getTurn() ? ChessPiece.WHITE : ChessPiece.BLACK;
                if(turn!=color) return res;

                // Castling
                if(canKingCastle(CastlingRights.WHITE_KING_SIDE)) addMove(row, col+2, ref res, color);
                if(canKingCastle(CastlingRights.WHITE_QUEEN_SIDE)) addMove(row, col-2, ref res, color);
                if(canKingCastle(CastlingRights.BLACK_KING_SIDE)) addMove(row, col+2, ref res, color);
                if(canKingCastle(CastlingRights.BLACK_QUEEN_SIDE)) addMove(row, col-2, ref res, color);
                */
                break;
            default:
                break;
        }

        return res;
    }

/*
    bool canKingCastle(CastlingRights side) {
        int kingPos, rookPos;
        set<int> underAttack;
        if (side == WHITE_KING_SIDE) {
            kingPos = 60; rookPos = 63; 
            if(pieceInPath(kingPos,rookPos,board)) return false;
            underAttack = checkAllPosBeingAttackedBy(BLACK);
            for(int i=kingPos; i<=62; i++){
                if(underAttack.find(i)!=underAttack.end()) return false;
            }
            // // DEBUG
            // cout << "WK HAS NOT MOVED = " << kingHasNotMoved[WHITE_KING] << endl;
            // cout << "ROOK CAN CASTLE = " << rookCanCastle[WHITE_KING_SIDE] << endl;
            // // DEBUG

            if (!kingHasNotMoved[WHITE_KING] || !rookCanCastle[WHITE_KING_SIDE]) return false;
        } else if (side == WHITE_QUEEN_SIDE) {
            kingPos = 60; rookPos = 56; 
            if(pieceInPath(kingPos,rookPos,board)) return false;
            underAttack = checkAllPosBeingAttackedBy(BLACK);
            for(int i=58; i<=kingPos; i++){
                if(underAttack.find(i)!=underAttack.end()) return false;
            }
            if (!kingHasNotMoved[WHITE_KING] || !rookCanCastle[WHITE_QUEEN_SIDE]) return false;
        } else if (side == BLACK_KING_SIDE) {
            kingPos = 4; rookPos = 7; 
            if(pieceInPath(kingPos,rookPos,board)) return false;

            underAttack = checkAllPosBeingAttackedBy(WHITE);
            for(int i=kingPos; i<=6; i++){
                if(underAttack.find(i)!=underAttack.end()) return false;
            }
            if (!kingHasNotMoved[BLACK_KING] || !rookCanCastle[BLACK_KING_SIDE]) return false;
        } else if (side == BLACK_QUEEN_SIDE) {
            kingPos = 4; rookPos = 0; 
            if(pieceInPath(kingPos,rookPos,board)) return false;

            underAttack = checkAllPosBeingAttackedBy(WHITE);
            for(int i=2; i<=kingPos; i++){
                if(underAttack.find(i)!=underAttack.end()) return false;
            }
            if (!kingHasNotMoved[BLACK_KING] || !rookCanCastle[BLACK_QUEEN_SIDE]) return false;
        }

        return true;
    }
*/

/*
    HashSet<int> checkAllPosBeingAttackedBy(int color){
        HashSet<int> res = new HashSet<int>();

        ChessPiece board = GameManager.instance.getPiece();

        // Iterate through all squares on the board
        for (int i = 0; i < 64; ++i) {
            // If the square contains a piece of the specified color
            if (board[i] != ChessPiece.EMPTY && getColor(board[i]) == color) {
                int piece = getPiece(board[i]);
                int row = i / 8;
                int col = i % 8;
                switch (piece) {
                    // Pawn attacks
                    case PAWN:
                        if (color == WHITE) {
                            if (isRowColValid(row - 1, col - 1)) addMove(row - 1, col - 1, res, color);
                            if (isRowColValid(row - 1, col + 1)) addMove(row - 1, col + 1, res, color);
                        } else {
                            if (isRowColValid(row + 1, col - 1)) addMove(row + 1, col - 1, res, color);
                            if (isRowColValid(row + 1, col + 1)) addMove(row + 1, col + 1, res, color);
                        }
                        break;
                    // Other pieces use checkValidMoves
                    default:
                        // // Debug
                        // cout << "DEBUG - starting with " << i << endl;
                        // // Debug

                        vector<int> moves = checkValidMoves(i, board);
                        for(int move:moves) res.insert(move);

                        // // Debug
                        // cout << "DEBUG - done with " << i << ", moves: ";
                        // for(int move:moves) cout << move << " ";
                        // cout << endl;
                        // // Debug

                        break;
                }
            }
        }
        return res;
    }
*/

    bool addMove(int r, int c, ref List<int> positions, ChessPiece color){
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

/*
    void CalculateValidPositions(){

    }



    void highLightValidTiles(){

    }

    List<int> getValidPositions(int pos){
        List<int> result = new List<int>();
        int[] board = GameManager.instance.getBoard();

        int piece = GameManager.getPiece(board[pos]);
        int color = GameManager.getColor(board[pos]);

        Debug.Log("Pos: " + pos + ", Piece = " + piece + ", Color = " + color);

        result.Add(48);
        
        return result;
    }

*/


    // Resets if Didn't click on the right piece/tile
    void ResetThisTurn(){
        Debug.Log("Reset This Turn");

        if(selected!=-1){ 
            selected = -1;
        } 
        
        displayManager.ResetAllTileColor();
    }

    void MovePiece(int oriPos, int newPos){
        GameManager.instance.MovePiece(oriPos, newPos);
        displayManager.MovePiece(oriPos, newPos);
    }


}
