using System.Collections.Generic;
using UnityEngine;

public class Rook : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();


        //// TODO: refactor for cleaner code

        // DOWN
        for (int i = currentY - 1; i >= 0; i--)
        {
            if (board[currentX, i] != null)
            {
                if (board[currentX, i].team != team)
                    r.Add(new Vector2Int(currentX, i));
                break;
            }

            r.Add(new Vector2Int(currentX, i));
        }

        // UP
        for (int i = currentY + 1; i < tileCountY; i++)
        {
            if (board[currentX, i] != null)
            {
                if (board[currentX, i].team != team)
                    r.Add(new Vector2Int(currentX, i));
                break;
            }

            r.Add(new Vector2Int(currentX, i));
        }

        // RIGHT
        for (int i = currentX + 1; i < tileCountX; i++)
        {
            if (board[i, currentY] != null)
            {
                if (board[i, currentY].team != team)
                    r.Add(new Vector2Int(i, currentY));
                break;
            }

            r.Add(new Vector2Int(i, currentY));
        }

        // LEFT
        for (int i = currentX - 1; i >= 0; i--)
        {
            if (board[i, currentY] != null)
            {
                if (board[i, currentY].team != team)
                    r.Add(new Vector2Int(i, currentY));
                break;
            }

            r.Add(new Vector2Int(i, currentY));
        }

        return r;
    }
}
