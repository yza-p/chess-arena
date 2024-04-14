using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Knight : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        int[,] offsets =
        {
            {1, 2}, // starting from 1st quandrant
            {2, 1},
            {-1, 2},
            {-2, 1},
            {-1, -2},
            {-2, -1},
            {1, -2},
            {2, -1}
        };

        for (int i = 0; i < offsets.GetLength(0); i++)
        {
            int moveX = currentX + offsets[i, 0];
            int moveY = currentY + offsets[i, 1];
            
            if (moveX < tileCountX && moveY < tileCountY && moveX >= 0 && moveY >= 0)
                if (board[moveX, moveY] == null || board[moveX, moveY].team != team)
                    r.Add(new Vector2Int(moveX, moveY));

        }

        return r;
    }

}
