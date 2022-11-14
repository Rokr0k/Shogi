using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoardManager : MonoBehaviour
{
    public Vector2 blockSize;
    public float height;

    public GameObject piece;

    public PieceData[] pieceData;

    [HideInInspector] public PieceManager[,] board = new PieceManager[11, 11];

    void Awake()
    {
        Initialize();
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
            PlacePiece(new Vector2Int(i, 3), 0, false);
        }
        PlacePiece(new Vector2Int(1, 9), 1, true);
        PlacePiece(new Vector2Int(9, 9), 1, true);
        PlacePiece(new Vector2Int(1, 1), 1, false);
        PlacePiece(new Vector2Int(9, 1), 1, false);

        PlacePiece(new Vector2Int(2, 9), 2, true);
        PlacePiece(new Vector2Int(8, 9), 2, true);
        PlacePiece(new Vector2Int(2, 1), 2, false);
        PlacePiece(new Vector2Int(8, 1), 2, false);

        PlacePiece(new Vector2Int(3, 9), 3, true);
        PlacePiece(new Vector2Int(7, 9), 3, true);
        PlacePiece(new Vector2Int(3, 1), 3, false);
        PlacePiece(new Vector2Int(7, 1), 3, false);

        PlacePiece(new Vector2Int(4, 9), 4, true);
        PlacePiece(new Vector2Int(6, 9), 4, true);
        PlacePiece(new Vector2Int(4, 1), 4, false);
        PlacePiece(new Vector2Int(6, 1), 4, false);

        PlacePiece(new Vector2Int(8, 8), 5, true);
        PlacePiece(new Vector2Int(2, 2), 5, false);

        PlacePiece(new Vector2Int(2, 8), 6, true);
        PlacePiece(new Vector2Int(8, 2), 6, false);

        PlacePiece(new Vector2Int(5, 9), 8, true);
        PlacePiece(new Vector2Int(5, 1), 7, false);
    }

    void PlacePiece(Vector2Int pos, int type, bool isBlack)
    {
        board[pos.x, pos.y] = Instantiate(piece, transform).GetComponent<PieceManager>();
        board[pos.x, pos.y].board = this;
        board[pos.x, pos.y].pieceData = pieceData[type];
        board[pos.x, pos.y].pos = pos;
        board[pos.x, pos.y].isBlack = isBlack;
        board[pos.x, pos.y].promoted = false;
        board[pos.x, pos.y].Initialize();
    }

    public bool Move(string notation)
    {
        Regex regex = new Regex(@"^([BW])(\+?[PLNSGBRK])(\d\d)?([\-x*`])(\d\d)(\+)?$");
        Match match;
        if ((match = regex.Match(notation)).Success)
        {
            bool isBlack = match.Groups[1].Value == "B";
            string code = match.Groups[2].Value;
            PieceData pieceType;
            switch(code)
            {
                case "P":
                case "+P":
                    pieceType = pieceData[0];
                    break;
                case "L":
                case "+L":
                    pieceType = pieceData[1];
                    break;
                case "N":
                case "+N":
                    pieceType = pieceData[2];
                    break;
                case "S":
                case "+S":
                    pieceType = pieceData[3];
                    break;
                case "G":
                    pieceType = pieceData[4];
                    break;
                case "B":
                case "+B":
                    pieceType = pieceData[5];
                    break;
                case "R":
                case "+R":
                    pieceType = pieceData[6];
                    break;
                case "K":
                    pieceType = pieceData[7];
                    break;
                default:
                    return false;
            }
            PieceData.Move moveData = pieceType.moves[code.Length - 1];
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
                default:
                    return false;
            }
            Vector2Int to = new Vector2Int(match.Groups[5].Value[0] - '0', match.Groups[5].Value[1] - '0');
            bool promote = match.Groups[6].Success;
            if (from.x < 0)
            {
                if (move == 0 || move == 1)
                {
                    List<Vector2Int> available = new List<Vector2Int>();
                    foreach(Vector2Int exact in moveData.exacts)
                    {
                        Vector2Int pos = to - (isBlack ? 1 : -1) * exact;
                        if (pos.x >= 1 && pos.x <= 9 && pos.y >= 1 && pos.y <= 9 && board[pos.x, pos.y] != null && board[pos.x, pos.y].isBlack == isBlack && board[pos.x, pos.y].pieceData.code[board[pos.x, pos.y].promoted?1:0] == code)
                        {
                            available.Add(pos);
                        }
                    }
                    foreach (Vector2Int range in moveData.ranges)
                    {
                        for (int d = 1; d < 10; d++)
                        {
                            Vector2Int pos = to - (isBlack ? d : -d) * range;
                            if (pos.x < 1 || pos.x > 9 || pos.y < 1 || pos.y > 9)
                            {
                                break;
                            }
                            if (board[pos.x, pos.y] != null)
                            {
                                if (board[pos.x, pos.y].isBlack == isBlack && board[pos.x, pos.y].pieceData.code[board[pos.x, pos.y].promoted ? 1 : 0] == code)
                                {
                                    available.Add(pos);
                                }
                                break;
                            }
                        }
                    }
                    if (available.Count == 1)
                    {
                        from = available[0];
                    }
                    else
                    {
                        return false;
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
                        return false;
                    }
                }
            }
            PieceManager p = board[from.x, from.y];
            if ((p.promoted || !(p.pieceData.code.Length > 1) || !(isBlack ? (from.y >= 1 && from.y <= 3 || to.y >= 1 && to.y <= 3 && from.y != 10) : (from.y <= 9 && from.y >= 7 || to.y <= 9 && to.y >= 7 && from.y != 0))) && promote)
            {
                return false;
            }
            if ((move != 1 && board[to.x, to.y] != null) || (move == 1 && (board[to.x, to.y] == null || board[to.x, to.y].isBlack == isBlack)))
            {
                return false;
            }

            if (move == 1)
            {
                if (board[to.x, to.y].pieceData.code[0] == "K")
                {
                    SceneManager.LoadScene("GameScene");
                }
                for (int file = 0; file <= 10; file++)
                {
                    if (board[file, isBlack ? 10 : 0] == null)
                    {
                        board[to.x, to.y].MoveTo(new Vector2Int(file, (isBlack ? 10 : 0)), board[to.x, to.y].promoted);
                        board[file, isBlack ? 10 : 0] = board[to.x, to.y];
                        break;
                    }
                }
            }
            board[from.x, from.y].MoveTo(to, promote);
            board[to.x, to.y] = board[from.x, from.y];
            board[from.x, from.y] = null;

            return true;
        }
        return false;
    }

    public void Quit()
    {
        Application.Quit();
    }
}
