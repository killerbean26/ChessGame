using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


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

    //private Material previousMat;
    //private Material selectedMat;

    //public GameObject PromotionUI;
    public int promotionIndex { set; get; }

    public int[] enPassantMove { set; get; }

    private Quaternion orientation = Quaternion.Euler(0,270,0);

   // private ChessAI chessAI;

    public bool isWhiteTurn = true;
    public bool isGameEnded = false;
    public bool isChecked = false;
    private void Start()
    {
        
        Instance = this;
        
        //chessAI = new ChessAI();

        Chessmans = new Chessman[8, 8];

        SpawnAllChessman();
    }
    private void Update()
    {
        
        UpdateSelection();
        DrawChessBoard();
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
                    
                    updateCamera();
                    PauseMenu.isWhiteTurn = isWhiteTurn;
                }
            }
        }
       
        //AI is black
        //if (!isWhiteTurn)
        //{
        //    Vector2 aiMove = new Vector2();
        //    Debug.Log(string.Format("Initial Values: x = {0}, y = {1}", aiMove.x, aiMove.y));

        //    do
        //    {
        //        selectedChessman = chessAI.SelectChessFigure();
        //        aiMove = chessAI.MakeMove(selectedChessman);
        //        Debug.Log(string.Format("{0} - {1}", aiMove.x, aiMove.y));
        //    } while (aiMove.x<0&&aiMove.y<0);
        //    MoveChessman((int)Mathf.Round(aiMove.x), (int)Mathf.Round(aiMove.y));

        //}

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
                //Beyaz Piyon Terfisi
                if (y == 7)
                {
                    activeChessman.Remove(selectedChessman.gameObject);
                    Destroy(selectedChessman.gameObject);

                    //Piyonun En son kareye gelmesi ile arayüz açılır
                    PauseMenu.Instance.PromotionUI.SetActive(true);

                    /*
                     StartCoroutine bir metodun bitmeden diğer metodun çalıştırılmasına izin verir.
                     promotionindex atamasını StartCoroutine olmadan ve WaitUntil kullanmadan yapsaydık
                     promotionİndexe arayüz tarafında atama yapılmadan varsayılan değer üzerinden fonksiyon gerçekleşecekti.
                     */
                    
                    StartCoroutine(WaitingUIAnswer(x,y));
                    



                }
                //Siyah Piyon Terfisi
                else if (y == 0)
                {
                    activeChessman.Remove(selectedChessman.gameObject);
                    Destroy(selectedChessman.gameObject);

                    PauseMenu.Instance.PromotionUI.SetActive(true);
                    StartCoroutine(WaitingUIAnswer(x, y));

                }
                //Geçerken Alma Kuralı
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
            
           
                    Check(x,y); 
                
           
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
        if (selectionX>=0 && selectionY>=0) //-1 e eşit değilse bir şey seçmişiz demektir.
        {
            Debug.DrawLine(Vector3.right*selectionX+Vector3.forward*selectionY,
                Vector3.right * (selectionX +1) + Vector3.forward * (selectionY+1));

            Debug.DrawLine(Vector3.right * selectionX + Vector3.forward * (selectionY +1),
                Vector3.right * (selectionX + 1) + Vector3.forward * selectionY);

        }

    }

    //KAMERA VE MOUSE POZİSYONUNA BAĞLI OLARAK SELECTION UPDATE EDİLECEK
    private void UpdateSelection()
    {
        if (!Camera.main) //Eğer ana kamera yoksa
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
        // GameObject Targetposition=new GameObject();
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
            go.transform.SetParent(transform); //Tahtaya bağlı hareket ediyor.
            Chessmans[x, y] = go.GetComponent<Chessman>();
            Chessmans[x, y].setPosition(x, y);
            activeChessman.Add(go);
        }
        else
        {
            GameObject go = Instantiate(chessmanPrefabs[index], GetTileCenter(x,y), orientation);
            go.transform.SetParent(transform); //Tahtaya bağlı hareket ediyor.
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

        //Sıranın kimde olduğunu arayüze aktarmak için yapılan bool ataması
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
        Debug.Log("test");
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
         * Parametre olarak taşların x,y koordinatlarını alır.
         * Wait Until ,belirlenen koşul doğru olana kadar bir sonraki işlemi bekletir.
         * terfi için gerekli indexs bilgisi varsayılan olarak 0 dır.
         * Bu durumda PauseMenu(UI işlemlerinin olduğu sınıf) kısmında terfi için yapılan index atamalarını beklemek durumunda kalır
         * Ardındanda promotion index ve konum bilgisi ile SpawnChessman metodu çağırılır ve istenen taş oyuna eklenmiş olur. 
         */
        yield return new WaitUntil(() => promotionIndex > 0);
        SpawnChessman(promotionIndex, x, y);
        selectedChessman = Chessmans[x, y];
        promotionIndex = 0;
    }
    /*
     * ŞAh çekme metodu şimdilik arkaplanda çalışıyor. ama düzgün değil. 
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

