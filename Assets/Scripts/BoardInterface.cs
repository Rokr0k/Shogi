using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BoardInterface : MonoBehaviour
{
    public BoardManager board;
    private TMP_InputField input;

    // Start is called before the first frame update
    void Awake()
    {
        input = GetComponent<TMP_InputField>();
        input.onSubmit.AddListener(delegate
        {
            board.Move(input.text);
            input.text = "";
            input.ActivateInputField();
        });
    }
}
