using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceManager : MonoBehaviour
{
    public PieceData pieceData;
    public Vector2Int pos;
    public bool promoted;
    public bool isBlack;
    [SerializeField] private Quaternion[] rotation;
    public BoardManager board;

    private Vector3 originPosition;
    private Quaternion originRotation;
    private Vector3 destPosition;
    private Quaternion destRotation;
    private float timer;

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            float x = 1 - timer;
            transform.position = Vector3.Lerp(originPosition, destPosition, Mathf.Lerp(Mathf.Pow(x, 2), 1 - Mathf.Pow(1 - x, 2), x)) + Vector3.up * timer * x * 4;
            transform.rotation = Quaternion.Euler((isBlack ? 1 : -1) * Vector3.right * Mathf.Sin(timer * 2 * Mathf.PI) * 30) * Quaternion.Lerp(originRotation, destRotation, x);
        }
    }

    public void Initialize()
    {
        GetComponent<Renderer>().material.SetTexture("_SubTex", pieceData.letter);
        transform.localScale = Vector3.one * pieceData.size;
        transform.rotation = rotation[isBlack ? promoted ? 1 : 0 : promoted ? 3 : 2];
        transform.position = board.GetVector(board.GetCoord(board.ConvertPos(pos, isBlack)));
    }

    public void MoveTo(Vector2Int target, bool flip)
    {
        originPosition = board.GetVector(board.GetCoord(board.ConvertPos(pos, isBlack)));
        originRotation = rotation[isBlack ? pieceData.code.Length > 1 && promoted ? 1 : 0 : pieceData.code.Length > 1 && promoted ? 3 : 2];
        if(flip)
        {
            promoted = !promoted;
        }
        if (target.y == 10)
        {
            isBlack = !isBlack;
        }
        destPosition = board.GetVector(board.GetCoord(board.ConvertPos(target, isBlack)));
        destRotation = rotation[isBlack ? pieceData.code.Length > 1 && promoted ? 1 : 0 : pieceData.code.Length > 1 && promoted ? 3 : 2];
        pos = target;
        timer = 1;
    }
}
