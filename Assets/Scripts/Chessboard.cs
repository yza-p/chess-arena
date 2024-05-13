using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Chessboard : MonoBehaviour
{
    [Header("Board")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    // [SerializeField] private float zOffset = 1;
    [SerializeField] private Vector3 boardCenter = new Vector3(-1, 0);

    [Header("Prefabs and Sprites")]
    [SerializeField] private GameObject[] prefabs;
    // default prefab is black, sprite is for white only as alternative
    [SerializeField] private Sprite[] whiteSprite;
    [SerializeField] private GameObject victoryScreen;
    [SerializeField] private GameObject connectScreen;


    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;

    private ChessPiece[,] chessPieces;
    private ChessPiece pieceToMove;
    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private List<ChessPiece> deadWhites = new List<ChessPiece>();
    private List<ChessPiece> deadBlacks = new List<ChessPiece>();

    private Vector2Int currentHover;
    private Vector3 bounds;
    private bool isDragging;
    private bool isWhiteTurn;

    // Multiplayer
    private int playerCount = -1;
    private int currentTeam = -1;
    private bool inProgress = false;

    private void Start()
    {
        isWhiteTurn = true;

        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);

        RegisterEvents();
    }

    private void Update()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }

        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (inProgress && Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
        {
            // Get the board tile that is being hovered
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

            // TODO: Refactor hover effect
            // HOVER UI EFFECT: if the previous hover is not a chess tile.
            // Just change the UI of current hovering tile
            if (currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            // HOVER UI EFFECT: if the previous hover is also a chess tile,
            // Change UI of both previous and current hovering tile
            if (currentHover != hitPosition)
            {
                tiles[currentHover.x, currentHover.y].layer =
                    (ContainsValidMove(ref availableMoves, currentHover))
                    ? LayerMask.NameToLayer("Highlight")
                    : LayerMask.NameToLayer("Tile");
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer =
                    (ContainsValidMove(ref availableMoves, currentHover))
                    ? LayerMask.NameToLayer("Highlight")
                    : LayerMask.NameToLayer("Hover");
            }


            // If we press down the mouse
            if (Input.GetMouseButtonDown(0))
            {
                // ... check if there's a piece in the current position
                if (chessPieces[hitPosition.x, hitPosition.y] != null)
                {
                    // is it the piece's team turn?
                    if ((isWhiteTurn && chessPieces[hitPosition.x, hitPosition.y].team == 0 && currentTeam == 0) ||
                        (!isWhiteTurn && chessPieces[hitPosition.x, hitPosition.y].team == 1 && currentTeam == 1))
                    {
                        pieceToMove = chessPieces[hitPosition.x, hitPosition.y];
                        isDragging = true;
                        // Render the dragged object in front of other pieces
                        pieceToMove.GetComponent<Renderer>().sortingLayerName = "BeingDragged";

                        availableMoves = pieceToMove.GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
                        HighlightTiles();
                    }
                }
            }

            // If we are releasing the mouse button
            if (pieceToMove != null && Input.GetMouseButtonUp(0)) 
            {
                isDragging = false;
                pieceToMove.GetComponent<Renderer>().sortingLayerName = "Default";
                Vector2Int previousPosition = new Vector2Int(pieceToMove.currentX, pieceToMove.currentY);

                if (!ContainsValidMove(ref availableMoves, new Vector2(hitPosition.x, hitPosition.y)))
                {
                    MoveTo(previousPosition.x, previousPosition.y, hitPosition.x, hitPosition.y);
                    
                    NetMakeMove nm = new NetMakeMove();
                    nm.originalX = previousPosition.x;
                    nm.originalY = previousPosition.y;
                    nm.destX = hitPosition.x;
                    nm.destY = hitPosition.y;
                    nm.teamId = currentTeam;
                    Client.Instance.SendToServer(nm);
                }
                else
                {
                    pieceToMove.transform.position = GetTileCenter(previousPosition.x, previousPosition.y);
                    pieceToMove = null;
                    RemoveHighlightTiles();
                }
                
            }
        }
        else
        {
            // if the hover is not the chessboard, hover is effect is disabled
            if (currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }

            // if mouse is moving a piece and it went out of the board, cancel the move
            if (pieceToMove && Input.GetMouseButtonUp(0))
            {
                pieceToMove.transform.position = GetTileCenter(pieceToMove.currentX, pieceToMove.currentY);
                pieceToMove = null;
                RemoveHighlightTiles();
            }
        }


        // If a piece is being dragged, the sprite follows the cursor
        if (isDragging && pieceToMove)
        {
            Vector3 mousePos = currentCamera.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
            pieceToMove.transform.position = mousePos;
        }
    }


    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        bounds = new Vector3((tileCountX / 2) * tileSize, (tileCountY / 2) * tileSize, 0) + boardCenter;

        tiles = new GameObject[tileCountX, tileCountY];

        for (int x = 0; x < tileCountX; x++)
            for (int y = 0; y < tileCountY; y++)
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
    }
    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;
        //tileObject.transform.position = new Vector2(-4, -4);

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, y * tileSize, -1) - bounds;
        vertices[1] = new Vector3(x * tileSize, (y + 1) * tileSize, -1) - bounds;
        vertices[2] = new Vector3((x + 1) * tileSize, y * tileSize, -1) - bounds;
        vertices[3] = new Vector3((x + 1) * tileSize, (y + 1) * tileSize, -1) - bounds;

        int[] tris = new int[] { 0, 2, 1, 1, 2, 3 };

        mesh.vertices = vertices;
        mesh.triangles = tris;

        // mesh.RecalculateNormals();
        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }


    // Spawning pieces
    private void SpawnAllPieces()
    {
        chessPieces = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];

        int otherTeam = (currentTeam == 0) ? 1 : 0;

        // White Team
        chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, currentTeam);
        chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, currentTeam);
        chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, currentTeam);
        chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, currentTeam);
        chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.King, currentTeam);
        chessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, currentTeam);
        chessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, currentTeam);
        chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, currentTeam);
        for (int i = 0; i < TILE_COUNT_X; i++)
            chessPieces[i, 1] = SpawnSinglePiece(ChessPieceType.Pawn, currentTeam);

        // Black Team
        chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, otherTeam);
        chessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, otherTeam);
        chessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, otherTeam);
        chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, otherTeam);
        chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.King, otherTeam);
        chessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, otherTeam);
        chessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, otherTeam);
        chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, otherTeam);
        for (int i = 0; i < TILE_COUNT_X; i++)
            chessPieces[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, otherTeam);

    }
    private ChessPiece SpawnSinglePiece(ChessPieceType type, int team)
    {
        ChessPiece cp = Instantiate(prefabs[(int) type - 1], transform).GetComponent<ChessPiece>();

        cp.type = type;
        cp.team = team;

        // if white team, change the sprite
        if (team == 0)
            cp.GetComponent<SpriteRenderer>().sprite = whiteSprite[(int)type - 1];
        

        return cp;
    }

    
    // Positioning pieces
    private void PositionAllPieces()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (chessPieces[x, y] != null)
                    PositionSinglePiece(x, y);
    }
    private void PositionSinglePiece(int x, int y)
    {
        chessPieces[x, y].currentX = x;
        chessPieces[x, y].currentY = y;
        chessPieces[x, y].transform.position = GetTileCenter(x, y) ;
    }
    private Vector3 GetTileCenter(int x, int y)
    {
        // The z-value -5 for the z-offset of the pieces (so that they are closer to camera)
        return new Vector3(x * tileSize, y * tileSize) - bounds
            + new Vector3(tileSize / 2, tileSize / 2) + new Vector3(0, 0, -5);
    }


    // Highlight Tiles
    private void HighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
    }
    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");

        availableMoves.Clear();
    }


    // Checkmate
    private void Checkmate (int team)
    {
        DisplayVictory(team);
    }
    private void DisplayVictory(int team)
    {
        victoryScreen.SetActive(true);
        victoryScreen.transform.GetChild(team).gameObject.SetActive(true);
    }
    public void OnResetButton()
    {
        victoryScreen.transform.GetChild(0).gameObject.SetActive(false);
        victoryScreen.transform.GetChild(1).gameObject.SetActive(false);
        victoryScreen.SetActive(false);

        pieceToMove = null;
        availableMoves = new List<Vector2Int>();

        for(int x=0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                    Destroy(chessPieces[x, y].gameObject);

                chessPieces[x, y] = null;
            }
        }

        deadWhites.Clear();
        deadBlacks.Clear();

        SpawnAllPieces();
        PositionAllPieces();
        isWhiteTurn = true;
    }
    public void OnExitButton()
    {
        SceneManager.LoadScene(0);
    }


    // Operations
    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2 position)
    {
        for (int i = 0; i < moves.Count; i++)
            if (moves[i].x == position.x && moves[i].y == position.y)
                return true;

        return false;
    }
    private void MoveTo(int originalX, int originalY, int x, int y)
    {

        ChessPiece cp = chessPieces[originalX, originalY];
        Vector2Int previousPosition = new Vector2Int(originalX, originalY);

        // Is there another piece on the target position?
        if (chessPieces[x, y] != null)
        {
            ChessPiece otherPiece = chessPieces[x, y];
            
            // Cancel move if the other piece is from same team
            if (cp.team == otherPiece.team)
                return;

            // Capture the otherpiece if it's from other team
            if (otherPiece.team == 0)
                deadWhites.Add(otherPiece);
            else
                deadBlacks.Add(otherPiece);

            if (otherPiece.type == ChessPieceType.King)
                Checkmate(otherPiece.team);

            // IDEA: Add a resurrection boon?
            // This would only hide the piece, but the object is still there
            otherPiece.GetComponent<Renderer>().enabled = false;
        }

        chessPieces[x, y] = cp;
        chessPieces[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece(x, y);

        isWhiteTurn = !isWhiteTurn;

        return;
    }
    private Vector2Int LookupTileIndex(GameObject tileHit)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (tiles[x, y] == tileHit)
                    return new Vector2Int(x, y);

        return -Vector2Int.one;
    }


    // Registers
    private void RegisterEvents()
    {
        NetUtility.S_WELCOME += OnWelcomeServer;
        NetUtility.C_WELCOME += OnWelcomeClient;
        NetUtility.C_START_GAME += OnStartGameClient;
    }

    private void UnRegisterEvents()
    {

    }

    private void OnWelcomeServer(NetMessage msg, NetworkConnection cnn)
    {
        // Server side; Client connected, send assigned team
        NetWelcome nw = msg as NetWelcome;
        nw.AssignedTeam = ++playerCount;
        Server.Instance.SendToClient(cnn, nw);

        if (playerCount == 1)
            Server.Instance.Broadcast(new NetStartGame());
    }
    private void OnWelcomeClient(NetMessage msg)
    {
        // Client side; unpack message
        NetWelcome nw = msg as NetWelcome;
        currentTeam = nw.AssignedTeam;
        
        Debug.Log($"My assigned team is {nw.AssignedTeam}");
    }
    private void OnStartGameClient(NetMessage msg)
    {
        Debug.Log("Starting game...");
        SpawnAllPieces();
        PositionAllPieces();
        connectScreen.transform.localScale = new Vector3(0, 0, 0);
        inProgress = true;
    }

}
