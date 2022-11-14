using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Piece", menuName = "Shogi Objects/Create Piece Data", order = 1)]
public class PieceData : ScriptableObject
{
    public string[] code;
    public Texture letter;
    public float size;

    [System.Serializable]
    public struct Move
    {
        public Vector2Int[] exacts;
        public Vector2Int[] ranges;
    }

    public Move[] moves;
}
