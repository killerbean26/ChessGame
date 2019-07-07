using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 Her Taş bu sınıftan türetilecek. Bu sınıf her taş için ortak özellikler içeren BASE class olarak düşünülebilir.
 Olası hareket her taş için değişeceği içinde genel bir possiblemove fonksiyonu virtual tanımlandı. Daha sonra tüm taşların kendi özel hareketleri için bu method override edilmelidir.

     */

public abstract class Chessman : MonoBehaviour
{
    //Oluşturulan her Chessman veya Chessman den türetilen her sınıf için X koodinatının set get methodu.
    public int CurrentX {
        set;
        get; 
    }
    //Oluşturulan her Chessman veya Chessman den türetilen her sınıf için Y koodinatının set get methodu.
    public int CurrentY {
        set;
        get;
    }

    public bool isWhite;
    public void setPosition(int x, int y)
    {
        CurrentX = x;
        CurrentY = y;
    }
    
    public virtual bool[,] PossibleMove()
    {
        return new bool [8,8];
    }

}
