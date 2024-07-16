using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text turn;
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
        
    }

    // Update is called once per frame
    void Update()
    {
        displayGameInfo();
    }

    void displayGameInfo(){
        turn.text = "Turn: " + (GameManager.instance.getTurn() ? "WHITE" : "BLACK");
    }
}
