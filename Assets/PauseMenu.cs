using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{

    public static bool GameIsPaused { set; get; }
    public static bool GameIsEnded { set; get; }
    public static bool isWhiteTurn { set; get; }//BoardManagerClassından alınacak iswhiteturn değerine eşit olacak

    public static PauseMenu Instance { set; get; }

    public GameObject PauseMenuUI;
    public GameObject GameOverUI;
    public GameObject PromotionUI;
  
    public Text GameStatue;//oyunun kazananını gösteren text
    public Text PauseStatue;//oyunun durduğunu gösteren text
    public Text TurnStatue;//Sıra kimde gösteren text

    public Button Pausebutton;
    public Button SpawnQueen;
    public Button SpawnRook;
    public Button SpawnKnight;
    public Button SpawnBishop;

    public int index { get; set; }

   
   

    private void Start()
    {
        Instance = this;
        
        GameIsEnded = false;
        GameIsPaused = true;

        GameStatue.text = "Welcome to the Game";

    }
    // Update is called once per frame
    void Update()
    {
        //Eğer oyun bitmiş durumda ise GameOver menü aktif olacak
       if(GameIsEnded==true)
        {
            GameOverUI.SetActive(true);
            setWinner();
        }
       
       //Eğer bu menü aktif ise Pause butonu ve sıranın kimde olduğunu gösteren text görünmeyecek
        if (GameOverUI.activeSelf)
        {
            Pausebutton.gameObject.SetActive(false);
           
            TurnStatue.gameObject.SetActive(false);
        }
        else
        {
            Pausebutton.gameObject.SetActive(true);
           
            TurnStatue.gameObject.SetActive(true);
        }
        
        setTurnText();
        

    }
   
    //Pause butonuna basıldığında Pausemenu açılacak. Oyun durumu değişecek
    public void PauseOnClick()
    {
        PauseMenuUI.SetActive(true);
       
        PauseStatue.text = "Paused";
  
        GameIsPaused = true;
    }

    //Resume ve Play tuşları aynı metodu kullanıyor. Çünkü BoardManager sınıfında oyun bittiğinde hemen ardından oyun yeniden başlıyor 
    //Tuşa basıldığında menüler açık ise kapanıyor ve oyun devam ediyor.
    public void ResumeOnClick()
    {
        PauseMenuUI.SetActive(false);
        GameOverUI.SetActive(false);
        GameIsEnded = false;
        GameIsPaused = false;
    }

    //Oyunu tekrardan başlatmak için BoardManager sınıfındaki EndGame fonksiyonu çağırılıyor. Oyun durumuda değişiyor.
    public void RestartOnClick()
    {
       // 
        PauseMenuUI.SetActive(false);
        GameIsPaused = false;
        GameIsEnded = false;
        BoardManager.Instance.EndGame();
       
    }
    //Uygulamadan çıkış
    public void ExitOnClick()
    {
        Application.Quit();
    }
    //Kazanan bilgilerini Boardmanager sınıfından alarak PauseMenu sınıfındaki değerlere atama yapar.
    public void setWinner()
    {

        if (isWhiteTurn)
        {
            GameStatue.text = "White Team Wins!";
        }
        else
        {
            GameStatue.text = "Black Team Wins!";
        }


    }

    //Sıranın kimde olduğunu ayarlayan bir metod. 
    //isWhiteTurn değeri her değiştiğinde text değişecek
    public void setTurnText()
    {
        
        if (isWhiteTurn)
        {
            TurnStatue.text = "White Turn!";
        }

        else
        {
            TurnStatue.text = "Black Turn!";
        }
    }
    public void SpawnBishopOnClick()
    {
        
        if (!isWhiteTurn)
        {

            BoardManager.Instance.promotionIndex = 3;
        }

        else
        {
            BoardManager.Instance.promotionIndex = 9;
        }

        PromotionUI.SetActive(false);
       
    }
    public void SpawnQueenOnClick()
    {
        
        if (!isWhiteTurn)
        {
           BoardManager.Instance.promotionIndex = 1;
        }

        else
        {
            BoardManager.Instance.promotionIndex = 7;
        }

        PromotionUI.SetActive(false);
       
    }
    public void SpawnKnightOnClick()
    {

        if (!isWhiteTurn)
        {
            BoardManager.Instance.promotionIndex = 4;
        }

        else
        {
            BoardManager.Instance.promotionIndex = 10;
        }

        PromotionUI.SetActive(false);
        
    }
    public void SpawnRookOnClick()
    {

        if (!isWhiteTurn)
        {
            BoardManager.Instance.promotionIndex = 2;
        }

        else
        {
            BoardManager.Instance.promotionIndex = 8;
        }

        PromotionUI.SetActive(false);
        
    }
}
