using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    
    public Text turn;
    public Text win;
    public Button reset;
    public Button undo;
    public DisplayManager displayManager;
    /*
    public Text blackCaptured;
    public Text WhiteCaptured;
    public Text moveRecord;

    public Button undo;
    public Button save;
    public Button load;
    */

    // Start is called before the first frame update
    void Start()
    {
        displayGameInfo();

        reset.onClick.AddListener(OnResetButtonPress);
        //undo.onClick.AddListener(OnUndoButtonPress);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnResetButtonPress(){
        GameManager.instance.ResetGame();
        displayManager.ResetGame();
        displayGameInfo();
    }

    // void OnUndoButtonPress()
    // {
    //     GameManager.instance.UndoMove();
    //     displayManager.UndoMove();
    //     displayGameInfo();
    // }

    public void displayGameInfo(){
        turn.text = "Turn: " + ((GameManager.instance.getTurn()==ChessPiece.WHITE) ? "WHITE" : "BLACK");
    }

    public void displayResult(bool isInCheck, bool isWhiteWin){
        if(!isInCheck)  win.text = ("STALEMATE!");
        else if(isWhiteWin) win.text = ("WHITE WINS!");
        else win.text = ("BLACK WINS!");
    }


}
