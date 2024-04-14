using System.Collections.Generic;
using UnityEngine;

public class Bishop : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        //// TODO: refactor for cleaner code

        // 1st Quadrant
        for (int x = currentX + 1, y = currentY + 1; x < tileCountX && y < tileCountY; x++, y++)
        {
            if (board[x, y] != null)
            {
                if (board[x, y].team != team)
                    r.Add(new Vector2Int(x, y));
                break;
            }

            r.Add(new Vector2Int(x, y));
        }

        // 2nd Quadrant
        for (int x = currentX - 1, y = currentY + 1; x >= 0 && y < tileCountY; x--, y++)
        {
            if (board[x, y] != null)
            {
                if (board[x, y].team != team)
                    r.Add(new Vector2Int(x, y));
                break;
            }

            r.Add(new Vector2Int(x, y));
        }

        // 3rd Quadrant
        for (int x = currentX - 1, y = currentY - 1; x >= 0 && y >= 0; x--, y--)
        {
            if (board[x, y] != null)
            {
                if (board[x, y].team != team)
                    r.Add(new Vector2Int(x, y));
                break;
            }

            r.Add(new Vector2Int(x, y));
        }

        // 4th Quadrant
        for (int x = currentX + 1, y = currentY - 1; x < tileCountX && y >= 0; x++, y--)
        {
            if (board[x, y] != null)
            {
                if (board[x, y].team != team)
                    r.Add(new Vector2Int(x, y));
                break;
            }

            r.Add(new Vector2Int(x, y));
        }

        return r;
    }
}
