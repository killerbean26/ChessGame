using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : Chessman
{
    public override bool[,] PossibleMove()
    {
        bool[,] r = new bool[8, 8];

        //ref parametresi

        //UpRight -> dikeyde sağ yukarıya L hareketi
        KnightMove(CurrentX + 1, CurrentY + 2, ref r);

        //UpLeft -> dikeyde sol yukarıya L hareketi
        KnightMove(CurrentX - 1, CurrentY + 2, ref r);
        
        //RightUp  -> +X ekseninde yukarıya L hareketi
        KnightMove(CurrentX +2, CurrentY +1, ref r);
        
        //RightDown -> +X ekseninde aşağıya L hareketi
        KnightMove(CurrentX + 2, CurrentY - 1, ref r);

        //DownLeft  -> dikeyde sol aşağıya L hareketi
        KnightMove(CurrentX - 1, CurrentY - 2, ref r);

        //DownRight  -> dikeyde sağ aşağıya L hareketi
        KnightMove(CurrentX + 1, CurrentY - 2, ref r);
        
        //LeftUp  -> -X ekseninde yukarıya L hareketi
        KnightMove(CurrentX - 2, CurrentY + 1, ref r);

        //LeftDown -> -X ekseninde aşağıya L hareketi
        KnightMove(CurrentX - 2, CurrentY - 1, ref r);
        return r;
    }
    public void KnightMove(int x, int y, ref bool[,] r)
    {
        Chessman c;

        if (x>=0 && x<8 && y>=0 && y<8)
        {
            c = BoardManager.Instance.Chessmans[x, y];
            if (c == null)
            {
                r[x, y] = true;
            }
            else
            {
                if (c.isWhite != isWhite)
                {
                    r[x, y] = true;
                }

            }
        }
    }
}
