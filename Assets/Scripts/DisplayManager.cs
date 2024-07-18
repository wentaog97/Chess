using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Serve as the View in MVC
public class DisplayManager : MonoBehaviour
{   
    public GameManager gameManager;
    // Prefabs
    public GameObject tilePrefab, piecePrefab;
    public Sprite pawnWhite, rookWhite, knightWhite, bishopWhite, queenWhite, kingWhite;
    public Sprite pawnBlack, rookBlack, knightBlack, bishopBlack, queenBlack, kingBlack;

    // Board related
    List<GameObject> tiles = new List<GameObject>();
    List<GameObject> pieces = new List<GameObject>();
    //List<GameObject> capturedPieces = new List<GameObject>();

    // Camera/display related
    public Transform mainCameraTransform;
    int screenWidth = Screen.width;
    int screenHeight = Screen.height;
    int padding = 50;
    float pieceSizeScaler = 0.85f;
    float tileSize;

    // Board tile colors
    // Recommended colors: DARK = 131,147,132,255, LIGHT = 211,212,211,255
    public Color darkTileColor;
    public Color lightTileColor;
    List<int> highlightedTiles = new List<int>();

    // Start is called before the first frame update
    void Start()
    {   
        InitializeBoard();
    }

    // Update is called once per frame
    void Update()
    {
        // If display dimension change, readjust everything
        if (screenWidth != Screen.width || screenHeight != Screen.height) DisplayDimensionChange();
    }

    public void ResetGame(){
        foreach(GameObject piece in pieces){
            Destroy(piece);
        }
        pieces.Clear();

        Debug.Log(pieces.Count);
        
        InitializePieces();
    }
    void InitializeBoard(){
        ChessPiece[] board = gameManager.getBoard();
        
        // Calculate Screen Size for Tile/Piece size adjustment
        tileSize = CalculateTileSize();
        InitializeTiles();
        InitializePieces();
    }

    // Board Display
    float CalculateTileSize()
    {
        float screenWidth = Screen.width-padding;
        float screenHeight = Screen.height-padding;

        float tileSizeByWidth = screenWidth / 8f;
        float tileSizeByHeight = screenHeight / 8f;

        // Calculate world space size of the tiles
        float aspectRatio = (float)screenWidth / screenHeight;
        float orthoSize = Camera.main.orthographicSize;
        float screenHeightInWorldUnits = 2 * orthoSize;
        float screenWidthInWorldUnits = screenHeightInWorldUnits * aspectRatio;

        float tileSizeInWorldUnitsByWidth = screenWidthInWorldUnits / 8f;
        float tileSizeInWorldUnitsByHeight = screenHeightInWorldUnits / 8f;

        return Mathf.Min(tileSizeInWorldUnitsByWidth, tileSizeInWorldUnitsByHeight);
    }
    void InitializeTiles()
    {
        float boardSize = tileSize * 8;

        float startX = -boardSize / 2 + tileSize / 2;
        float startY = boardSize / 2 - tileSize / 2;

        for(int r=0; r<8; r++){
            for(int c=0; c<8; c++){
                Vector3 position = new Vector3(startX + c * tileSize, startY - r * tileSize, 0);
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);

                tile.transform.localScale = new Vector3(tileSize, tileSize, 1);

                SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
                if(r%2==0){
                    if(c%2==0){
                        sr.color = lightTileColor;
                    } else {
                        sr.color = darkTileColor;
                    }
                } else {
                    if(c%2==0){
                        sr.color = darkTileColor;
                    } else {
                        sr.color = lightTileColor;
                    }
                }
                tiles.Add(tile);
            }
        }

        mainCameraTransform.position = new Vector3(0, 0, -10); // Ensure camera is centered on the board
    }
    void InitializePieces(){
        ChessPiece[] board = gameManager.getBoard();
        Vector3 offset = new Vector3(0,0,-1);
        float pieceSize = CalculateTileSize()*pieceSizeScaler;

        for(int r=0; r<8; r++){
            for(int c=0; c<8; c++){
                if(board[r*8+c] == ChessPiece.EMPTY) {
                    pieces.Add(null);
                    continue;
                }

                Vector3 tilePosition = tiles[r*8+c].transform.position;
                GameObject pieceGameObject = Instantiate(piecePrefab, tilePosition + offset, Quaternion.identity);
                
                pieces.Add(pieceGameObject);

                pieceGameObject.transform.localScale = new Vector3(pieceSize, pieceSize, 1);

                SpriteRenderer sr = pieceGameObject.GetComponent<SpriteRenderer>();
                
                ChessPiece piece = board[r*8+c];

                // Set the sprite based on the piece type
                switch (piece & ChessPiece.PIECEMASK)
                {
                    case ChessPiece.PAWN:
                        sr.sprite = (piece & ChessPiece.BLACK) != 0 ? pawnBlack : pawnWhite;
                        break;
                    case ChessPiece.ROOK:
                        sr.sprite = (piece & ChessPiece.BLACK) != 0 ? rookBlack : rookWhite;
                        break;
                    case ChessPiece.KNIGHT:
                        sr.sprite = (piece & ChessPiece.BLACK) != 0 ? knightBlack : knightWhite;
                        break;
                    case ChessPiece.BISHOP:
                        sr.sprite = (piece & ChessPiece.BLACK) != 0 ? bishopBlack : bishopWhite;
                        break;
                    case ChessPiece.QUEEN:
                        sr.sprite = (piece & ChessPiece.BLACK) != 0 ? queenBlack : queenWhite;
                        break;
                    case ChessPiece.KING:
                        sr.sprite = (piece & ChessPiece.BLACK) != 0 ? kingBlack : kingWhite;
                        break;
                    default:
                        Destroy(pieceGameObject); // Destroy if it's an invalid piece
                        break;
                }
            }   
        }
    }
    void AdjustTilesAndPiecesToScreenRatio(float size){
        // Adjust the positions and scales of the tiles
        float boardSize = size * 8;

        float startX = -boardSize / 2 + size / 2;
        float startY = boardSize / 2 - size / 2;

        for (int r = 0; r < 8; r++) {
            for (int c = 0; c < 8; c++) {
                int index = r * 8 + c;
                Vector3 position = new Vector3(startX + c * size, startY - r * size, 0);
                GameObject tile = tiles[index];
                tile.transform.position = position;
                tile.transform.localScale = new Vector3(size, size, 1);
            }
        }
        
        
        // Adjust the positions and scales of the pieces
        Vector3 offset = new Vector3(0, 0, -1);
        float pieceSize = size * pieceSizeScaler;

        for (int i = 0; i < pieces.Count; i++) {
            GameObject piece = pieces[i];
            if(piece==null) continue;
            piece.transform.localScale = new Vector3(pieceSize, pieceSize, 1);

            // Update the piece position based on its corresponding tile
            piece.transform.position = tiles[i].transform.position + offset;
        }
        
        
    }
    void DisplayDimensionChange(){
        screenWidth = Screen.width;
        screenHeight = Screen.height;
        tileSize = CalculateTileSize();
        AdjustTilesAndPiecesToScreenRatio(tileSize);
    }

    // For Moves Manager
    public List<GameObject> getPieces(){
        return pieces;
    }
    public List<GameObject> getTiles(){
        return tiles;
    }
    public GameObject getTile(int pos){
        return tiles[pos];
    }

    // Only moves the front end!
    public void MovePiece(int oriPos, int newPos){
        pieces[newPos] = pieces[oriPos];
        pieces[oriPos] = null;
        pieces[newPos].transform.position = new Vector3(tiles[newPos].transform.position.x, tiles[newPos].transform.position.y, pieces[newPos].transform.position.z);
    }

    public void HighlightTile(int pos){
        GameObject tile = tiles[pos];
        SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
        sr.color = new Color(sr.color.r+40, sr.color.g, sr.color.b, sr.color.a);

        highlightedTiles.Add(pos);
    }

    public void ResetTileColor(int pos){
        GameObject tile = tiles[pos];
        int r = pos/8;
        int c = pos%8;
        SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
        if(r%2==0){
            if(c%2==0){
                sr.color = lightTileColor;
            } else {
                sr.color = darkTileColor;
            }
        } else {
            if(c%2==0){
                sr.color = darkTileColor;
            } else {
                sr.color = lightTileColor;
            }
        }
        highlightedTiles.Remove(pos);
    }
    public void ResetAllTileColor(){
        List<int> allHighLightedPos = new List<int>(highlightedTiles);
        foreach(int pos in allHighLightedPos){
            ResetTileColor(pos);
        }
        highlightedTiles.Clear();
    }

    public void CapturePiece(int newPos){
        Destroy(pieces[newPos]);
    }
    public void PawnPromotion(int pos){
        SpriteRenderer sr = pieces[pos].GetComponent<SpriteRenderer>();
        sr.sprite = (gameManager.getBoard()[pos] & ChessPiece.BLACK) != 0 ? queenBlack : queenWhite;
    }
    public void Castling(int oriPos, int newPos){
        int rookPos, targetPos;
        // Debug.Log(oriPos + ", " + newPos);

        if(oriPos==4){ // Black
            if(newPos==2){
                targetPos = 3; rookPos = 0;
            } else {
                targetPos = 5; rookPos = 7;
            }
        } else { // White
            if(newPos==58){
                targetPos = 59; rookPos = 56;
            } else {
                targetPos = 61; rookPos = 63;     
            }
        }
        MovePiece(rookPos, targetPos);


    }

}


