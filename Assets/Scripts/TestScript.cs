using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log(GameDesignData.Instance.RakugoList[0].Content);
    }
}
