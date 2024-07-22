using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// public enum ChessPieceValues {
//     PAWN = 10,
//     KNIGHT = 30,
//     BISHOP = 30, 
//     ROOK = 50, 
//     QUEEN = 90,
//     KING = 900
// }

public class ChessBot : MonoBehaviour
{
    public MovesManager movesManager;
    public GameManager gameManager;
    public UIManager uIManager;
    public ChessPiece botColor;
    public int botLevel = 1;

    //Piece values for eval function:
    // int pawn = 10, knight = 30, bishop = 30, rook = 50, queen = 90, king = 900;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BotMove(){
        ChessPiece[] board = gameManager.getBoard();

        List<int> movablePieces = movesManager.getAllMovablePieces(gameManager.currGame, botColor);
        
        int oriPos = -1, newPos = -1;

        switch(botLevel){
            case 1:
                ValuationBasedMove(ref oriPos, ref newPos, movablePieces);
                if(oriPos==-1) RandomMove(ref oriPos, ref newPos, movablePieces);
                break;
            // case 2:
            //     MinMaxMove();
            //     break;
            // case 3:
            //     HardBot();
            //     break;
            default:
                RandomMove(ref oriPos, ref newPos, movablePieces);
                break;
        }

        // Moving piece
        // Debug.Log("Moving " + oriPos + " to " + newPos);
        movesManager.MovePiece(gameManager.currGame, oriPos, newPos);

        movesManager.CheckForCheckMate(gameManager.currGame); 
        uIManager.displayGameInfo();
    }

    void RandomMove(ref int oriPos, ref int newPos, List<int> movablePieces){
        System.Random random = new System.Random();
        int randomIndex = random.Next(movablePieces.Count);
        int randomPiecePos = movablePieces[randomIndex];

        List<int> legalPos = movesManager.GetLegalMoves(gameManager.currGame, randomPiecePos);
        randomIndex = random.Next(legalPos.Count);
        int randomPieceNewPos = legalPos[randomIndex];

        oriPos = randomPiecePos;
        newPos = randomPieceNewPos;
    }
    
    void ValuationBasedMove(ref int oriPos, ref int newPos, List<int> movablePieces){
        int gameTrend = 0;

        ChessNode currNode = new ChessNode(gameManager.currGame);
        gameTrend = EvaluateNode(currNode);

        List<int> allPos = movesManager.getAllMovablePieces(currNode, currNode.getTurn());
        
        foreach(int pos in allPos){
            List<int> LegalMoves = movesManager.GetLegalMoves(currNode, pos);
            
            foreach(int move in LegalMoves){
                if((gameManager.getPiece(currNode.board[pos])==ChessPiece.PAWN)&&((move>=0&&move<8)||(move>=56&&move<64))){
                    for(int option=0; option<4; option++){
                        ChessNode newNode = new ChessNode(currNode);                        
                        movesManager.MoveNodePiece(newNode, pos, move, option);
                        int val = EvaluateNode(newNode);
                        if(botColor == ChessPiece.WHITE){
                            if(val > gameTrend) {
                                oriPos = pos;
                                newPos = move;
                                gameTrend = val;
                            }
                        } else {
                            if(val < gameTrend) {
                                oriPos = pos;
                                newPos = move;
                                gameTrend = val;
                            }
                        }
                    }
                } else{
                    ChessNode newNode = new ChessNode(currNode);
                    movesManager.MoveNodePiece(newNode, pos, move);
                    int val = EvaluateNode(newNode);
                    if(botColor == ChessPiece.WHITE){
                        if(val > gameTrend) {
                            oriPos = pos;
                            newPos = move;
                            gameTrend = val;
                        }
                    } else {
                        if(val < gameTrend) {
                            oriPos = pos;
                            newPos = move;
                            gameTrend = val;
                        }
                    }
                }
            }
        }
    }

    // void MinMaxMove(ref int oriPos, ref int newPos, List<int> movablePieces, int depth=4){
        
    // }

    // int MinMaxHelper(){

    // }


    void MediumBot(){}
    void HardBot(){}

    int EvaluateNode(ChessNode node){
        int res = 0;
        for(int i=0; i<64; i++){
            ChessPiece piece = node.getPiece(node.board[i]);
            ChessPiece color = node.getColor(node.board[i]);
            switch(piece){
                case ChessPiece.PAWN:
                    res += (color==ChessPiece.WHITE) ? 10 : -10;
                    break;
                case ChessPiece.ROOK:
                    res += (color==ChessPiece.WHITE) ? 50 : -50;
                    break;
                case ChessPiece.KNIGHT:
                    res += (color==ChessPiece.WHITE) ? 30 : -30;
                    break;
                case ChessPiece.BISHOP:
                    res += (color==ChessPiece.WHITE) ? 30 : -30;    
                    break;
                case ChessPiece.QUEEN:
                    res += (color==ChessPiece.WHITE) ? 90 : -90;
                    break;
                case ChessPiece.KING:
                    res += (color==ChessPiece.WHITE) ? 900 : -900;
                    break;
                default:
                    break;
            }
        }
        return res;
    }

}
