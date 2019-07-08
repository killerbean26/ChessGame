using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/**
    There is no AI :(. Unfortunately I could not implement it.
    Also there are no some special moves.
    King does not have own special rules.

    Code is still buggy i think. it could be better :D

 */

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { set; get; }
  
    private bool[,] allowedMoves { set; get; }
    public Chessman[,] Chessmans { set; get; }
    private Chessman selectedChessman;

    private const float TILE_SIZE = 1.0f;
    private const float TILE_OFFSET = 0.5f;
        
    private int selectionX = -1;
    private int selectionY = -1;

    public List<GameObject> chessmanPrefabs;
    private List<GameObject> activeChessman;

    public int promotionIndex { set; get; }

    public int[] enPassantMove { set; get; }

    private Quaternion orientation = Quaternion.Euler(0,270,0);

    public bool isWhiteTurn = true;
    public bool isGameEnded = false;
    public bool isChecked = false;
    private void Start()
    {
        Instance = this;
        Chessmans = new Chessman[8, 8];
        SpawnAllChessman();
    }
    private void Update()
    {   
        UpdateSelection();
        DrawChessBoard();
        updateCamera();
        if (Input.GetMouseButtonDown(0))
        {
            if (selectionX>=0 && selectionY>=0)
            {
                if (selectedChessman== null)
                {
                    //Select the chessman
                    SelectChessman(selectionX,selectionY);
                    
                }
                else
                {
                    //Move the chessman
                    MoveChessman(selectionX,selectionY);
                    PauseMenu.isWhiteTurn = isWhiteTurn;
                }
            }
        }
    }

    private void SelectChessman(int x, int y)
    {
        if (Chessmans[x,y]==null)
        {
            return;
        }
        if (Chessmans[x, y].isWhite != isWhiteTurn)
        {
            return;
        }

        bool hasAtleastOneMove = false;
       
        allowedMoves = Chessmans[x, y].PossibleMove();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (allowedMoves[i,j])
                {
                    hasAtleastOneMove = true;
                    
                }
            }
        }
        if (!hasAtleastOneMove)
        {
            return;
        }
        selectedChessman = Chessmans[x, y];
        BoardHighlights.Instance.HighlightAllowedMoves(allowedMoves);
    }
    private void MoveChessman(int x, int y)
    {
        if (allowedMoves[x,y]==true)
        {
            Chessman c = Chessmans[x, y];
            if (c!=null&& c.isWhite!=isWhiteTurn)
            {
                //Capture a piece

                //If it is the king
                if (c.GetType()==typeof(King))
                {
                    isGameEnded = true;
                    EndGame();
                    return;
                }
                activeChessman.Remove(c.gameObject);
                Destroy(c.gameObject);
            }
            if (x==enPassantMove[0]&&y==enPassantMove[1])
            {
                if (isWhiteTurn)
                    c = Chessmans[x, y - 1];
                 
                else
                    c = Chessmans[x, y + 1];

                    activeChessman.Remove(c.gameObject);
                    Destroy(c.gameObject);          
            }
            enPassantMove[0] = -1;
            enPassantMove[1] = -1;
            if (selectedChessman.GetType()==typeof(Pawn))
            {
                //White Pawn Promotion
                if (y == 7)
                {
                    activeChessman.Remove(selectedChessman.gameObject);
                    Destroy(selectedChessman.gameObject);

                    //If pawn reaches top or bottom of board ,promotion menu opens
                    PauseMenu.Instance.PromotionUI.SetActive(true);

                    /*
                     StartCoroutine allows one method to be executed before the end of one method.
                    If we didn't assign promotionindex without StartCoroutine and without using WaitUntil,
                    the function would have been performed over the default value without any assignment on the promotionindex.*/
                    StartCoroutine(WaitingUIAnswer(x,y));
                }
                //Black Pawn Promotion
                else if (y == 0)
                {
                    activeChessman.Remove(selectedChessman.gameObject);
                    Destroy(selectedChessman.gameObject);

                    PauseMenu.Instance.PromotionUI.SetActive(true);
                    StartCoroutine(WaitingUIAnswer(x, y));
                }
                //EnPassant Rule
                if (selectedChessman.CurrentY==1&& y==3)
                {
                    enPassantMove[0] = x;
                    enPassantMove[1] = y-1;
                }
                else if (selectedChessman.CurrentY == 6 && y == 4)
                {
                    enPassantMove[0] = x;
                    enPassantMove[1] = y+1;
                }
            }
           
            Chessmans[selectedChessman.CurrentX, selectedChessman.CurrentY] = null;
            selectedChessman.transform.position = GetTileCenter(x, y);
            selectedChessman.setPosition(x, y);
            
            Chessmans[x, y] = selectedChessman;
            
 //---------------------------------------          
                    Check(x,y); 
                //still in development
//----------------------------------

            isWhiteTurn = !isWhiteTurn;
            
        }
        BoardHighlights.Instance.Hidehighlights();
       
        selectedChessman = null;
    }
    private void DrawChessBoard()
    {
        Vector3 widthLine = Vector3.right * 8;
        Vector3 heighthLine = Vector3.forward * 8;

        for (int i = 0; i <= 8; i++)
        {
            Vector3 start = Vector3.forward * i;
            Debug.DrawLine(start, start + widthLine);
            for (int j = 0; j <= 8; j++)
            {
                start = Vector3.right * i;
                Debug.DrawLine(start, start + heighthLine);
            }
        }
        //Draw the selection
        if (selectionX>=0 && selectionY>=0) //if not equal to -1 ,it means something selected.
        {
            Debug.DrawLine(Vector3.right*selectionX+Vector3.forward*selectionY,
                Vector3.right * (selectionX +1) + Vector3.forward * (selectionY+1));

            Debug.DrawLine(Vector3.right * selectionX + Vector3.forward * (selectionY +1),
                Vector3.right * (selectionX + 1) + Vector3.forward * selectionY);
        }
    }
    //Selection will be updated when camera and mouse position change.
    private void UpdateSelection()
    {
        if (!Camera.main) //If there is no main Camera
        {
            return;
        }
        RaycastHit hit; //Collision olduğunda info hit içinde
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); //Kameradan mouse pozisyonuna bir ışın oluşturduk
        if (Physics.Raycast(ray, out hit, 25.0f, LayerMask.GetMask("ChessPlane"))&&(!PauseMenu.Instance.GameOverUI.activeSelf||!PauseMenu.Instance.PauseMenuUI.activeSelf))
        {
            selectionX = (int)hit.point.x;
            selectionY = (int)hit.point.z;
        }
        else
        {
            selectionX = -1;
            selectionY = -1;
        }
    }


    private void updateCamera()
    {
        /**
            This function changes Camera position automatically.
           Position changing is turn based.
         */
         int posZ=-1;
        int rotY = 0;
       
        if (!isWhiteTurn)
        {
            posZ = 9;
            rotY = 180;
        }

        Camera.main.transform.position = new Vector3(4, 6, posZ);

        Camera.main.transform.eulerAngles = new Vector3(55, rotY, 0);
    }
    private void SpawnChessman(int index, int x, int y)
    {
        if (index < 6)
        {
            GameObject go = Instantiate(chessmanPrefabs[index], GetTileCenter(x,y), Quaternion.Euler(0,90,0));
            go.transform.SetParent(transform); //Moves with board.
            Chessmans[x, y] = go.GetComponent<Chessman>();
            Chessmans[x, y].setPosition(x, y);
            activeChessman.Add(go);
        }
        else
        {
            GameObject go = Instantiate(chessmanPrefabs[index], GetTileCenter(x,y), orientation);
            go.transform.SetParent(transform); //Moves with board.
            Chessmans[x, y] = go.GetComponent<Chessman>();
            Chessmans[x, y].setPosition(x, y);
            activeChessman.Add(go);
        }
    }

    public void SpawnAllChessman()
    {
        activeChessman = new List<GameObject>();
       
        enPassantMove = new int[2] { -1,-1};

        Debug.Log("Started");

        //Bool assignment for transferring the turn info to the UI 
        PauseMenu.isWhiteTurn = isWhiteTurn;

        //Spawn the White Team!

        //King
        SpawnChessman(0, 3, 0);

        //Queen
        SpawnChessman(1, 4, 0);

        //Rook
        SpawnChessman(2, 0, 0);
        SpawnChessman(2, 7, 0);

        //bishop
        SpawnChessman(3, 2, 0);
        SpawnChessman(3, 5, 0);

        //knight
        SpawnChessman(4, 1, 0);
        SpawnChessman(4, 6, 0);

        for (int i = 0; i < 8; i++)
        {
            SpawnChessman(5, i,1);
        }

        //Spawn the Black Team!

        //King
        SpawnChessman(6, 3, 7);

        //Queen
        SpawnChessman(7, 4, 7);

        //Rook
        SpawnChessman(8, 0, 7);
        SpawnChessman(8, 7, 7);

        //knight
        SpawnChessman(9, 2, 7);
        SpawnChessman(9, 5, 7);

        //Bishop
        SpawnChessman(10, 1, 7);
        SpawnChessman(10, 6, 7);

        for (int i = 0; i < 8; i++)
        {
            SpawnChessman(11, i, 6);
        }
    }
    private Vector3 GetTileCenter(int x, int y)
    {
        Vector3 origin;
        origin = Vector3.zero;
        origin.x = (TILE_SIZE * x) + TILE_OFFSET;
        origin.z = (TILE_SIZE * y) + TILE_OFFSET;
        return origin;
    }

    public void EndGame()
    {
       
        if (isGameEnded==true)
        {
            if (isWhiteTurn)
            {
                Debug.Log("White team wins");

                PauseMenu.GameIsEnded = true;
            }
            else //if (!isWhiteTurn)
            {
                Debug.Log("Black team wins");

                PauseMenu.GameIsEnded = true;
            }
        }
         
        else
        {
            Debug.Log("Restarted");
        }
       
        foreach (GameObject go in activeChessman)
        {
            Destroy(go);
        }
       
        isWhiteTurn = true;
        isGameEnded = false;
        BoardHighlights.Instance.Hidehighlights();
       
        SpawnAllChessman();
    }
    public List<GameObject> GetAllActiveFigures()
    {
        return activeChessman;
    }
    IEnumerator WaitingUIAnswer(int x,int y)
    {
        /*
         * Parameters: x and y coordinates of pawn which on the last square.
         * Wait Until , if statement true, next process starts.if statement false, next process wait until it is true.
         * promotionIndex default value is zero
         * In this case, PauseMenu (the class with the UI operations) will have to wait for index assignments for the promotionIndex.
         * Then the SpawnChessman method is called with the promotion index and position information and the desired stone is added to the game.
         */
        yield return new WaitUntil(() => promotionIndex > 0);
        SpawnChessman(promotionIndex, x, y);
        selectedChessman = Chessmans[x, y];
        promotionIndex = 0;
    }
    /*
        Check method stil in development.
     */
    public void Check(int x,int y)
    {
        allowedMoves = Chessmans[x,y].PossibleMove();
        
        Chessman c,c2;
        
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                c = Chessmans[i, j];
                if (c != null && c.GetType() == typeof(King) && allowedMoves[i, j])
                {
                    Debug.Log("check");
                   
                    isChecked = true;
                    c2 = Chessmans[i, j];
                                 
                    Debug.Log(isChecked);
                }
                else if (c != null && c.GetType() == typeof(King) && allowedMoves[i, j])
                {
                    Debug.Log("check2");
                }
                else
                    isChecked = false;
            }   
        }
    }
}