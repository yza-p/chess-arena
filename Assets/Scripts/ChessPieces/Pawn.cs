using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        int direction = (team == 0) ? 1 : -1;

        
        // As long as there are tiles in front
        if (!(currentY + direction >= tileCountY || currentY + direction < 0))
        {
            // Allow move one space in front
            if (board[currentX, currentY + direction] == null)
            {
                r.Add(new Vector2Int(currentX, currentY + direction));

                // Also allow move 2 spaces in front if at the start
                if (board[currentX, currentY + (direction * 2)] == null)
                {
                    if (team == 0 && currentY == 1)
                        r.Add(new Vector2Int(currentX, currentY + (direction * 2)));
                    if (team == 1 && currentY == 6)
                        r.Add(new Vector2Int(currentX, currentY + (direction * 2)));
                }
            }

            // Allow diagonal move when capturing
            if (currentX != tileCountX - 1) // all tiles except the rightmost tile
                if (board[currentX + 1, currentY + direction] != null && board[currentX + 1, currentY + direction].team != team)
                    r.Add(new Vector2Int(currentX + 1, currentY + direction));
            if (currentX != 0) // all tiles except the leftmost tile
                if (board[currentX - 1, currentY + direction] != null && board[currentX - 1, currentY + direction].team != team)
                    r.Add(new Vector2Int(currentX - 1, currentY + direction));
        }
        


        return r;
    }
}
