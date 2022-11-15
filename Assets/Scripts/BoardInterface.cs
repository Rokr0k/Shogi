using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BoardInterface : MonoBehaviour
{
    public BoardManager board;
    public TMP_InputField input;
    public GameObject indicator;
    public CameraController cameraController;
    private bool isBlack;

    private bool willPromote;
    private Vector2Int selected;
    private GameObject[,] indicatorPool = new GameObject[9, 9];

    void Awake()
    {
        cameraController.isBlack = isBlack = true;
        if (input.placeholder is TMP_Text)
        {
            ((TMP_Text)input.placeholder).text = isBlack ? "Black" : "White";
        }
        input.onSubmit.AddListener(delegate
        {
            if (board.Move((isBlack ? "B" : "W") + input.text))
            {
                cameraController.isBlack = isBlack = !isBlack;
                if (input.placeholder is TMP_Text)
                {
                    ((TMP_Text)input.placeholder).text = isBlack ? "Black" : "White";
                }
            }
            input.text = "";
            input.ActivateInputField();
        });
        selected = new Vector2Int(-1, -1);
        for (int file = 0; file < 9; file++)
        {
            for (int rank = 0; rank < 9; rank++)
            {
                indicatorPool[file, rank] = Instantiate(indicator, transform);
                indicatorPool[file, rank].transform.position = board.GetVector(board.GetCoord(new Vector2Int(file + 1, rank + 1))) + Vector3.up * 0.25f;
                indicatorPool[file, rank].SetActive(false);
            }
        }
    }

    void Update()
    {
        if(Input.touchSupported && Input.touchCount>0)
        {
            Touch touch = Input.GetTouch(0);
            if(touch.phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Vector2Int pos = new Vector2Int(Mathf.RoundToInt(hit.point.x / board.blockSize.x), Mathf.RoundToInt(hit.point.z / board.blockSize.y)) + new Vector2Int(5, 5);
                    HitScreen(pos);
                }
                else
                {
                    selected = new Vector2Int(-1, -1);
                    Unselect();
                }
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector2Int pos = new Vector2Int(Mathf.RoundToInt(hit.point.x / board.blockSize.x), Mathf.RoundToInt(hit.point.z / board.blockSize.y)) + new Vector2Int(5, 5);
                HitScreen(pos);
            }
            else
            {
                selected = new Vector2Int(-1, -1);
                Unselect();
            }
        }
    }

    void HitScreen(Vector2Int pos)
    {
        if (selected.x < 0)
        {
            if (board.board[pos.x, pos.y] != null && board.board[pos.x, pos.y].isBlack == isBlack)
            {
                willPromote = false;
                selected = pos;
                if (pos.x >= 1 && pos.x <= 9 && pos.y >= 1 && pos.y <= 9)
                {
                    PieceData.Move moveData = board.board[pos.x, pos.y].pieceData.moves[board.board[pos.x, pos.y].promoted ? 1 : 0];
                    foreach (Vector2Int exact in moveData.exacts)
                    {
                        Vector2Int p = pos + (isBlack ? 1 : -1) * exact;
                        if (p.x >= 1 && p.x <= 9 && p.y >= 1 && p.y <= 9 && (board.board[p.x, p.y] == null || board.board[p.x, p.y].isBlack != isBlack))
                        {
                                indicatorPool[p.x - 1, p.y - 1].SetActive(true);
                        }
                    }
                    foreach (Vector2Int range in moveData.ranges)
                    {
                        for (int d = 1; d < 10; d++)
                        {
                            Vector2Int p = pos + (isBlack ? d : -d) * range;
                            if (p.x < 1 || p.x > 9 || p.y < 1 || p.y > 9)
                            {
                                break;
                            }
                            if (board.board[p.x, p.y] != null)
                            {
                                if (board.board[p.x, p.y].isBlack != isBlack)
                                {
                                    indicatorPool[p.x - 1, p.y - 1].SetActive(true);
                                }
                                break;
                            }
                            indicatorPool[p.x - 1, p.y - 1].SetActive(true);
                        }
                    }
                }
                else
                {
                    bool isPawn = board.board[pos.x, pos.y].pieceData.code[0] == "P";
                    bool isLance = board.board[pos.x, pos.y].pieceData.code[0] == "L";
                    bool isKnight = board.board[pos.x, pos.y].pieceData.code[0] == "N";

                    for (int file = 0; file < 9; file++)
                    {
                        if (isPawn)
                        {
                            bool rowPossible = true;
                            for (int r = 0; r < 9; r++)
                            {
                                if (board.board[file + 1, r + 1] != null && board.board[file + 1, r + 1].isBlack == isBlack && board.board[file + 1, r + 1].pieceData.code[board.board[file + 1, r + 1].promoted ? 1 : 0] == "P")
                                {
                                    rowPossible = false;
                                    break;
                                }
                            }
                            if (!rowPossible)
                            {
                                continue;
                            }
                        }
                        for (int rank = 0; rank < 9; rank++)
                        {
                            if ((isPawn || isLance) && (isBlack ? rank < 1 : rank > 7) || isKnight && (isBlack ? rank < 2 : rank > 6))
                            {
                                continue;
                            }
                            if (board.board[file + 1, rank + 1] == null)
                            {
                                indicatorPool[file, rank].SetActive(true);
                            }
                        }
                    }
                }
            }
        }
        else if (selected == pos)
        {
            willPromote = !willPromote;
            if (board.board[pos.x, pos.y].pieceData.code.Length < 2 || board.board[pos.x, pos.y].promoted || pos.x < 1 || pos.x > 9 || pos.y < 1 || pos.y > 9)
            {
                willPromote = false;
            }

            Unselect();

            PieceData.Move moveData = board.board[pos.x, pos.y].pieceData.moves[board.board[pos.x, pos.y].promoted ? 1 : 0];
            foreach (Vector2Int exact in moveData.exacts)
            {
                Vector2Int p = pos + (isBlack ? 1 : -1) * exact;
                if (p.x >= 1 && p.x <= 9 && p.y >= 1 && p.y <= 9 && (board.board[p.x, p.y] == null || board.board[p.x, p.y].isBlack != isBlack))
                {
                    if (!willPromote || (isBlack ? p.y <= 3 || pos.y <= 3 : p.y >= 7 || pos.y >= 7))
                    {
                        indicatorPool[p.x - 1, p.y - 1].SetActive(true);
                    }
                }
            }
            foreach (Vector2Int range in moveData.ranges)
            {
                for (int d = 1; d < 10; d++)
                {
                    Vector2Int p = pos + (isBlack ? d : -d) * range;
                    if (p.x < 1 || p.x > 9 || p.y < 1 || p.y > 9)
                    {
                        break;
                    }
                    if (board.board[p.x, p.y] != null)
                    {
                        if (board.board[p.x, p.y].isBlack != isBlack)
                        {
                            if (!willPromote || (isBlack ? p.y <= 3 || pos.y <= 3 : p.y >= 7 || pos.y >= 7))
                            {
                                indicatorPool[p.x - 1, p.y - 1].SetActive(true);
                            }
                        }
                        break;
                    }
                    if (!willPromote || (isBlack ? p.y <= 3 || pos.y <= 3 : p.y >= 7 || pos.y >= 7))
                    {
                        indicatorPool[p.x - 1, p.y - 1].SetActive(true);
                    }
                }
            }
        }
        else if (pos.x >= 1 && pos.x <= 9 && pos.y >= 1 && pos.y <= 9 && indicatorPool[pos.x - 1, pos.y - 1].activeSelf)
        {
            string team = isBlack ? "B" : "W";
            string type = board.board[selected.x, selected.y].pieceData.code[board.board[selected.x, selected.y].promoted ? 1 : 0];
            string from = (selected.x >= 1 && selected.x <= 9 && selected.y >= 1 && selected.y <= 9) ? selected.x.ToString() + selected.y.ToString() : "";
            string move = (selected.x >= 1 && selected.x <= 9 && selected.y >= 1 && selected.y <= 9) ? board.board[pos.x, pos.y] == null ? "-" : "x" : "*";
            string to = pos.x.ToString() + pos.y.ToString();
            string promote = willPromote ? "+" : "";

            if (board.Move(team + type + from + move + to + promote))
            {
                cameraController.isBlack = isBlack = !isBlack;
                if (input.placeholder is TMP_Text)
                {
                    ((TMP_Text)input.placeholder).text = isBlack ? "Black" : "White";
                }
            }

            selected = new Vector2Int(-1, -1);
            Unselect();
        }
        else
        {
            selected = new Vector2Int(-1, -1);
            Unselect();
        }
    }

    void Unselect()
    {
        for (int file = 0; file < 9; file++)
        {
            for (int rank = 0; rank < 9; rank++)
            {
                indicatorPool[file, rank].SetActive(false);
            }
        }
    }
}
