using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class BoardManager : MonoBehaviour
{
    public Vector2 blockSize;
    public float height;

    public GameObject piece;

    public PieceData[] pieceData;

    private PieceManager[,] board = new PieceManager[11, 11];

    // Start is called before the first frame update
    void Awake()
    {
        Initialize();
    }

    public Vector2Int ConvertPos(Vector2Int pos, bool isBlack)
    {
        if (isBlack)
        {
            return pos;
        }
        else
        {
            return new Vector2Int(10, 10) - pos;
        }
    }

    public Vector2Int GetCoord(Vector2Int pos)
    {
        return pos - new Vector2Int(5, 5);
    }

    public Vector3 GetVector(Vector2Int pos)
    {
        Vector2 position = pos * blockSize;
        return new Vector3(position.x, height, position.y);
    }

    void Initialize()
    {
        for (int i = 1; i <= 9; i++)
        {
            PlacePiece(new Vector2Int(i, 7), 0, true);
            PlacePiece(new Vector2Int(i, 7), 0, false);
        }
        PlacePiece(new Vector2Int(1, 9), 1, true);
        PlacePiece(new Vector2Int(9, 9), 1, true);
        PlacePiece(new Vector2Int(1, 9), 1, false);
        PlacePiece(new Vector2Int(9, 9), 1, false);

        PlacePiece(new Vector2Int(2, 9), 2, true);
        PlacePiece(new Vector2Int(8, 9), 2, true);
        PlacePiece(new Vector2Int(2, 9), 2, false);
        PlacePiece(new Vector2Int(8, 9), 2, false);

        PlacePiece(new Vector2Int(3, 9), 3, true);
        PlacePiece(new Vector2Int(7, 9), 3, true);
        PlacePiece(new Vector2Int(3, 9), 3, false);
        PlacePiece(new Vector2Int(7, 9), 3, false);

        PlacePiece(new Vector2Int(4, 9), 4, true);
        PlacePiece(new Vector2Int(6, 9), 4, true);
        PlacePiece(new Vector2Int(4, 9), 4, false);
        PlacePiece(new Vector2Int(6, 9), 4, false);

        PlacePiece(new Vector2Int(8, 8), 5, true);
        PlacePiece(new Vector2Int(8, 8), 5, false);

        PlacePiece(new Vector2Int(2, 8), 6, true);
        PlacePiece(new Vector2Int(2, 8), 6, false);

        PlacePiece(new Vector2Int(5, 9), 8, true);
        PlacePiece(new Vector2Int(5, 9), 7, false);
    }

    void PlacePiece(Vector2Int pos, int type, bool isBlack)
    {
        Vector2Int p = ConvertPos(pos, isBlack);
        board[p.x, p.y] = Instantiate(piece, transform).GetComponent<PieceManager>();
        board[p.x, p.y].board = this;
        board[p.x, p.y].pieceData = pieceData[type];
        board[p.x, p.y].pos = pos;
        board[p.x, p.y].isBlack = isBlack;
        board[p.x, p.y].promoted = false;
        board[p.x, p.y].Initialize();
    }

    public void Move(string notation)
    {
        Regex regex = new Regex(@"^([BW])(\+?[PLNSGBRK])(\d\d)?([\-x*`])(\d\d)(\+)?$");
        Match match;
        if ((match = regex.Match(notation)).Success)
        {
            bool isBlack = match.Groups[1].Value == "B";
            string code = match.Groups[2].Value;
            Vector2Int from = match.Groups[3].Success ? new Vector2Int(match.Groups[3].Value[0] - '0', match.Groups[3].Value[1] - '0') : new Vector2Int(-1, -1);
            int move = 0;
            switch (match.Groups[4].Value)
            {
                case "-":
                    move = 0;
                    break;
                case "x":
                    move = 1;
                    break;
                case "*":
                case "`":
                    move = 2;
                    break;
            }
            Vector2Int to = new Vector2Int(match.Groups[5].Value[0] - '0', match.Groups[5].Value[1] - '0');
            bool promote = match.Groups[6].Success;
            if (from.x < 0)
            {
                if (move == 0 || move == 1)
                {
                    List<Vector2Int> available = new List<Vector2Int>();
                    for (int file = 1; file <= 9; file++)
                    {
                        for (int rank = 1; rank <= 9; rank++)
                        {
                            Vector2Int pos = new Vector2Int(file, rank);
                            PieceManager piece = board[pos.x, pos.y];
                            if (piece != null && piece.pieceData.code[piece.promoted ? 1 : 0] == code && piece.isBlack == isBlack)
                            {
                                foreach (Vector2Int exact in piece.pieceData.moves[piece.promoted ? 1 : 0].exacts)
                                {
                                    if (pos + (isBlack ? 1 : -1) * exact == to)
                                    {
                                        available.Add(pos);
                                    }
                                }
                                foreach (Vector2Int range in piece.pieceData.moves[piece.promoted ? 1 : 0].ranges)
                                {
                                    for (int d = 1; d <= 9; d++)
                                    {
                                        Vector2Int a = pos + (isBlack ? d : -d) * range;
                                        if (a == to)
                                        {
                                            available.Add(pos);
                                        }
                                        if (a.x < 1 || a.x > 9 || a.y < 1 || a.y > 9 || board[a.x, a.y] != null)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (available.Count == 1)
                    {
                        from = available[0];
                    }
                    else
                    {
                        return;
                    }
                }
                else if (move == 2)
                {
                    bool available = false;
                    for (int file = 0; file <= 10; file++)
                    {
                        Vector2Int pos = new Vector2Int(file, isBlack ? 10 : 0);
                        PieceManager piece = board[pos.x, pos.y];
                        if (piece != null && piece.pieceData.code[piece.promoted ? 1 : 0] == code && piece.isBlack == isBlack)
                        {
                            from = pos;
                            available = true;
                            break;
                        }
                    }
                    if (!available)
                    {
                        return;
                    }
                }
            }
            PieceManager p = board[from.x, from.y];
            if (p == null || p.pieceData.code[p.promoted ? 1 : 0] != code || p.isBlack != isBlack)
            {
                return;
            }
            if ((p.promoted || !(isBlack ? (from.y >= 1 && from.y <= 3 || to.y >= 1 && to.y <= 3 && from.y != 10) : (from.y <= 9 && from.y >= 7 || to.y <= 9 && to.y >= 7 && from.y != 0)) || !(p.pieceData.code.Length > 1)) && promote)
            {
                return;
            }
            if ((move == 0 && board[to.x, to.y] != null) || (move == 1 && board[to.x, to.y] == null))
            {
                return;
            }

            if (move == 1)
            {
                for (int file = 0; file <= 10; file++)
                {
                    if (board[file, isBlack ? 10 : 0] == null)
                    {
                        board[to.x, to.y].MoveTo(new Vector2Int(file, 10), board[to.x, to.y].promoted);
                        board[file, isBlack ? 10 : 0] = board[to.x, to.y];
                        break;
                    }
                }
            }
            board[from.x, from.y].MoveTo(ConvertPos(to, isBlack), promote);
            board[to.x, to.y] = board[from.x, from.y];
            board[from.x, from.y] = null;
        }
    }
}
