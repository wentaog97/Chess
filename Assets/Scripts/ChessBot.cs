using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessBot : MonoBehaviour
{
    public MovesManager movesManager;
    public GameManager gameManager;
    public UIManager uIManager;
    public ChessPiece botColor;

    int botLevel = 0;

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
        List<int> botPieces = new List<int>();
        for(int i=0; i<64; i++){
            if(gameManager.getColor(board[i])==botColor) botPieces.Add(i);
        }
        
        List<int> moveAblePieces = new List<int>();
        foreach(int i in botPieces){            
            if(movesManager.GetLegalMoves(i).Count>0) moveAblePieces.Add(i);
        }
        
        int oriPos = -1, newPos = -1;

        switch(botLevel){
            case 1:
            case 2:
            case 3:
            default:
                RandomMove(ref oriPos, ref newPos, moveAblePieces);
                break;
        }

        // Moving piece
        Debug.Log("Moving " + oriPos + " to " + newPos);
        movesManager.MovePiece(oriPos, newPos);

        // Updates everything
        gameManager.changeTurn(); // Order matters here 
        movesManager.CheckForCheckMate(); // If turn hasn't changed, it will cause false check bug

        uIManager.displayGameInfo();
    }

    void RandomMove(ref int oriPos, ref int newPos, List<int> moveAblePieces){
        System.Random random = new System.Random();
        int randomIndex = random.Next(moveAblePieces.Count);
        int randomPiecePos = moveAblePieces[randomIndex];

        List<int> legalPos = movesManager.GetLegalMoves(randomPiecePos);
        randomIndex = random.Next(legalPos.Count);
        int randomPieceNewPos = legalPos[randomIndex];

        oriPos = randomPiecePos;
        newPos = randomPieceNewPos;
    }



}
