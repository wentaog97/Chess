using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessBot : MonoBehaviour
{
    public MovesManager movesManager;
    ChessPiece color;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void InitChessBot(ChessPiece color){
        this.color = color;
    }

    void OnTurnChange(){

    }
    
}
