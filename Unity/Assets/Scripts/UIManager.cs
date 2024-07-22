using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public int perftLevel = 1;
    public Text turn;
    public Text win;
    public Button reset;
    public Button undo;
    public Button perft;
    public DisplayManager displayManager;
    public GameManager gameManager;
    public MovesManager movesManager;
    public Toggle isBot1, isBot2;

    // Start is called before the first frame update
    void Start()
    {
        displayGameInfo();
        movesManager.isBot1 = isBot1.isOn;
        movesManager.isBot2 = isBot2.isOn;

        isBot1.onValueChanged.AddListener(OnToggle1ValueChanged);
        isBot2.onValueChanged.AddListener(OnToggle2ValueChanged);

        reset.onClick.AddListener(OnResetButtonPress);
        undo.onClick.AddListener(OnUndoButtonPress);
        perft.onClick.AddListener(OnPerftButtonPress);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void OnToggle1ValueChanged(bool isOn)
    {
        movesManager.isBot1 = isOn;
    }

    void OnToggle2ValueChanged(bool isOn)
    {
        movesManager.isBot2 = isOn;
    }

    void OnResetButtonPress(){
        gameManager.ResetGame();
        displayManager.UpdateDisplay();
        win.text = "";
        displayGameInfo();
    }

    void OnUndoButtonPress()
    {
        // Debug.Log("Undo Clicked!");
        movesManager.UndoLastMove();
        win.text = "";
    }
    void OnPerftButtonPress()
    {
        movesManager.Perft(gameManager.currGame, perftLevel);
    }

    public void displayGameInfo(){
        turn.text = 
        "Turn: " + ((gameManager.currGame.getTurn()==ChessPiece.WHITE) ? "WHITE" : "BLACK")
        + "\n" +
        "Half move count = " + gameManager.currGame.getHalfMoveCount()
        ;
    }

    public void displayResult(bool isInCheck, bool isWhiteWin){
        if(!isInCheck)  win.text = ("STALEMATE!");
        else if(isWhiteWin) win.text = ("WHITE WINS!");
        else win.text = ("BLACK WINS!");
    }


}
